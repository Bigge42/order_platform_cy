using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Entity.SystemModels;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.ESB;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.Purchase
{
    /// <summary>
    /// 采购订单头ESB同步服务实现
    /// </summary>
    public class PurchaseOrderESBSyncService : ESBSyncServiceBase<OCP_PurchaseOrder, ESBPurchaseOrderData, IOCP_PurchaseOrderRepository>
    {
        private readonly ESBLogger _esbLogger;

        public PurchaseOrderESBSyncService(
            IOCP_PurchaseOrderRepository repository,
            ESBBaseService esbService,
            ILogger<PurchaseOrderESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _esbLogger = ESBLoggerFactory.CreatePurchaseOrderLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供采购订单专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        protected override string GetESBApiConfigName()
        {
            return nameof(PurchaseOrderESBSyncService);
        }

        protected override string GetOperationType()
        {
            return "采购订单头";
        }

        protected override bool ValidateESBData(ESBPurchaseOrderData esbData)
        {
            // 基本字段验证
            if (esbData.FID <= 0)
            {
                ESBLogger.LogValidationError("采购订单头", "FID无效", $"FID={esbData.FID}");
                return false;
            }

            return true;
        }

        protected override object GetEntityKey(ESBPurchaseOrderData esbData)
        {
            return (long)esbData.FID;
        }

        protected override async Task<List<OCP_PurchaseOrder>> QueryExistingRecords(List<object> keys)
        {
            var billIds = keys.Cast<long>().ToList();
            return await Task.Run(() =>
                _repository.FindAsIQueryable(x => x.FID.HasValue && billIds.Contains(x.FID.Value))
                .ToList());
        }

        protected override bool IsEntityMatch(OCP_PurchaseOrder entity, ESBPurchaseOrderData esbData)
        {
            return entity.FID == esbData.FID;
        }

        protected override void MapESBDataToEntity(ESBPurchaseOrderData esbData, OCP_PurchaseOrder entity)
        {
            MapESBDataToEntityCore(esbData, entity, null);
        }

        /// <summary>
        /// 重写带缓存的映射方法，优化供应商查询性能
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="masterRecordsCache">主表记录缓存</param>
        /// <param name="materialRecordsCache">物料记录缓存</param>
        /// <param name="supplierRecordsCache">供应商记录缓存</param>
        /// <param name="customerRecordsCache">客户记录缓存</param>
        protected override void MapESBDataToEntityWithCache(ESBPurchaseOrderData esbData, OCP_PurchaseOrder entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // 使用缓存优化的映射
            MapESBDataToEntityCore(esbData, entity, supplierRecordsCache);
        }

        /// <summary>
        /// 映射ESB数据到实体的核心方法
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="supplierDict">供应商信息字典（可选，用于性能优化）</param>
        private void MapESBDataToEntityCore(ESBPurchaseOrderData esbData, OCP_PurchaseOrder entity, 
            Dictionary<long, OCP_Supplier> supplierDict = null)
        {
            // 基本信息映射
            entity.FID = esbData.FID;
            entity.BillNo = esbData.FBILLNO;
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;
            entity.BusinessType = esbData.FBILLTYPENAME;
            entity.PurchasePerson = esbData.FCREATENAME;

            // 供应商信息映射（优先使用缓存）
            entity.SupplierID = esbData.FSUPPLIERID;
            entity.SupplierCode = GetSupplierCodeById(esbData.FSUPPLIERID, supplierDict);
            entity.SupplierName = GetSupplierNameById(esbData.FSUPPLIERID, supplierDict);
        }

        /// <summary>
        /// 重写提取供应商ID方法，从采购订单头数据中提取供应商ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>供应商ID列表</returns>
        protected override List<int?> ExtractSupplierIds(List<ESBPurchaseOrderData> esbDataList)
        {
            if (esbDataList == null || !esbDataList.Any())
                return new List<int?>();

            return esbDataList
                .Where(x => x.FSUPPLIERID.HasValue && x.FSUPPLIERID.Value > 0)
                .Select(x => x.FSUPPLIERID)
                .Distinct()
                .ToList();
        }

        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_PurchaseOrder> toUpdate, List<OCP_PurchaseOrder> toInsert)
        {
            if (!toUpdate.Any() && !toInsert.Any())
                return new WebResponseContent().OK("无数据需要处理");

            return await Task.Run(() => _repository.DbContextBeginTransaction(() =>
            {
                var webResponse = new WebResponseContent();
                
                try
                {
                    // 使用UpdateRange批量更新
                    if (toUpdate.Any())
                    {
                        _repository.UpdateRange(toUpdate, false);
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条采购订单头记录");
                    }

                    // 使用AddRange批量插入
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条采购订单头记录");
                    }

                    // 一次性保存所有更改
                    _repository.SaveChanges();
                    
                    return webResponse.OK($"采购订单头批量操作完成，更新 {toUpdate.Count} 条，插入 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, $"采购订单头批量数据库操作失败：{ex.Message}");
                    return webResponse.Error($"采购订单头批量操作失败：{ex.Message}");
                }
            }));
        }

        #endregion
    }
} 