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
    /// 采购订单明细ESB同步服务实现
    /// </summary>
    public class PurchaseOrderDetailESBSyncService : ESBSyncServiceBase<OCP_PurchaseOrderDetail, ESBPurchaseOrderDetailData, IOCP_PurchaseOrderDetailRepository>
    {
        private readonly IOCP_PurchaseOrderRepository _purchaseOrderRepository;
        private readonly ESBLogger _esbLogger;

        public PurchaseOrderDetailESBSyncService(
            IOCP_PurchaseOrderDetailRepository repository,
            IOCP_PurchaseOrderRepository purchaseOrderRepository,
            ESBBaseService esbService,
            ILogger<PurchaseOrderDetailESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
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
            return nameof(PurchaseOrderDetailESBSyncService);
        }

        protected override string GetOperationType()
        {
            return "采购订单明细";
        }

        protected override bool ValidateESBData(ESBPurchaseOrderDetailData esbData)
        {
            // 基本字段验证
            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("采购订单明细", "FENTRYID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            return true;
        }

        protected override object GetEntityKey(ESBPurchaseOrderDetailData esbData)
        {
            return esbData.FENTRYID;
        }

        protected override async Task<List<OCP_PurchaseOrderDetail>> QueryExistingRecords(List<object> keys)
        {
            var entryIds = keys.Cast<int>().Select(x => (long)x).ToList();
            return await Task.Run(() =>
                _repository.FindAsIQueryable(x => x.FENTRYID.HasValue && entryIds.Contains(x.FENTRYID.Value))
                .ToList());
        }

        protected override bool IsEntityMatch(OCP_PurchaseOrderDetail entity, ESBPurchaseOrderDetailData esbData)
        {
            return entity.FENTRYID == esbData.FENTRYID;
        }

        protected override void MapESBDataToEntity(ESBPurchaseOrderDetailData esbData, OCP_PurchaseOrderDetail entity)
        {
            MapESBDataToEntityCore(esbData, entity, null, null);
        }

        /// <summary>
        /// 重写带缓存的映射方法，优化物料、供应商查询性能
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="masterRecordsCache">主表记录缓存</param>
        /// <param name="materialRecordsCache">物料记录缓存</param>
        /// <param name="supplierRecordsCache">供应商记录缓存</param>
        /// <param name="customerRecordsCache">客户记录缓存</param>
        protected override void MapESBDataToEntityWithCache(ESBPurchaseOrderDetailData esbData, OCP_PurchaseOrderDetail entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // 使用缓存优化的映射
            MapESBDataToEntityCore(esbData, entity, materialRecordsCache, supplierRecordsCache);
        }

        /// <summary>
        /// 映射ESB数据到实体的核心方法
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <param name="supplierDict">供应商信息字典（可选，用于性能优化）</param>
        private void MapESBDataToEntityCore(ESBPurchaseOrderDetailData esbData, OCP_PurchaseOrderDetail entity, 
            Dictionary<long, OCP_Material> materialDict = null, Dictionary<long, OCP_Supplier> supplierDict = null)
        {
            // 基本信息映射
            entity.FENTRYID = esbData.FENTRYID;
            entity.FID = esbData.FID;
            entity.LineNumber = esbData.FSEQ;
            entity.PlanTraceNo = esbData.FMTONO;
            entity.MaterialID = esbData.FMATERIALID;

            // 物料信息映射（优先使用缓存）
            entity.MaterialNumber = GetMaterialCodeById(esbData.FMATERIALID, materialDict);
            entity.MaterialName = GetMaterialNameById(esbData.FMATERIALID, materialDict);

            // 计划信息映射
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;

            // 供应商信息映射（优先使用缓存）
            entity.SupplierID = esbData.FSUPPLIERID;
            entity.SupplierCode = GetSupplierCodeById(esbData.FSUPPLIERID, supplierDict);
            entity.DeliveryNo = esbData.F_BLN_DELIVERYNO;

            // 数量信息映射
            entity.PurchaseQty = esbData.FQTY;
            entity.InstockQty = esbData.FREALQTY;
            entity.UnfinishedQty = esbData.FWWQTY;

            // 日期字段转换
            entity.RequiredDeliveryDate = _esbService.ParseDateTime(esbData.FDELIVERYDATE);
            entity.ReplyDeliveryDate = _esbService.ParseDateTime(esbData.F_BLN_SPDATE);
        }

        /// <summary>
        /// 批量预查询主表记录，优化性能避免N+1查询问题
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>主表记录字典，Key为FID，Value为主表记录</returns>
        protected override async Task<Dictionary<long, object>> PreQueryMasterRecords(List<ESBPurchaseOrderDetailData> esbDataList)
        {
            var result = new Dictionary<long, object>();
            
            if (esbDataList == null || !esbDataList.Any())
                return result;

            try
            {
                // 🚀 性能优化：批量查询主表记录
                // 收集所有唯一的FID
                var fids = esbDataList
                    .Where(x => x.FID.HasValue && x.FID.Value > 0)
                    .Select(x => (long)x.FID.Value)
                    .Distinct()
                    .ToList();

                if (!fids.Any())
                {
                    ESBLogger.LogDebug("没有有效的FID，跳过主表批量查询");
                    return result;
                }

                // 一次性批量查询所有需要的主表记录
                var masterRecords = await Task.Run(() =>
                    _purchaseOrderRepository.FindAsIQueryable(x => x.FID.HasValue && fids.Contains(x.FID.Value))
                    .ToList());

                // 构建FID到主表记录的映射字典
                foreach (var record in masterRecords)
                {
                    if (record.FID.HasValue)
                    {
                        result[record.FID.Value] = record;
                    }
                }

                ESBLogger.LogInfo($"批量查询采购主表记录完成：查询 {fids.Count} 个FID，找到 {result.Count} 条主表记录");
                
                return result;
            }
            catch (Exception ex)
            {
                ESBLogger.LogError(ex, $"批量预查询采购订单主表记录失败：{ex.Message}");
                return result; // 发生异常时返回空字典，降级到单次查询模式
            }
        }

        /// <summary>
        /// 重写提取物料ID方法，从采购订单明细数据中提取物料ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBPurchaseOrderDetailData> esbDataList)
        {
            if (esbDataList == null || !esbDataList.Any())
                return new List<long?>();

            return esbDataList
                .Where(x => x.FMATERIALID.HasValue && x.FMATERIALID.Value > 0)
                .Select(x => (long?)x.FMATERIALID)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 重写提取供应商ID方法，从采购订单明细数据中提取供应商ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>供应商ID列表</returns>
        protected override List<int?> ExtractSupplierIds(List<ESBPurchaseOrderDetailData> esbDataList)
        {
            if (esbDataList == null || !esbDataList.Any())
                return new List<int?>();

            return esbDataList
                .Where(x => x.FSUPPLIERID.HasValue && x.FSUPPLIERID.Value > 0)
                .Select(x => x.FSUPPLIERID)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 重写设置主表ID方法，使用缓存优化性能
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">实体对象</param>
        /// <param name="masterRecordsCache">主表记录缓存</param>
        /// <returns>是否成功设置主表ID</returns>
        protected override bool SetMasterTableId(ESBPurchaseOrderDetailData esbData, OCP_PurchaseOrderDetail entity, Dictionary<long, object> masterRecordsCache = null)
        {
            if (!esbData.FID.HasValue || esbData.FID.Value <= 0)
            {
                ESBLogger.LogWarning($"ESB数据FID无效，无法设置主表ID，明细记录FENTRYID={esbData.FENTRYID}");
                entity.OrderID = null;
                return false;
            }

            OCP_PurchaseOrder masterRecord = null;

            // 🚀 优先使用缓存查询，提升性能
            if (masterRecordsCache != null && masterRecordsCache.ContainsKey(esbData.FID.Value))
            {
                masterRecord = masterRecordsCache[esbData.FID.Value] as OCP_PurchaseOrder;
                ESBLogger.LogDebug($"从缓存获取采购主表记录，FID={esbData.FID.Value}");
            }
            else
            {
                // 缓存未命中，降级到单次查询（用于异常情况）
                masterRecord = _purchaseOrderRepository.FindAsIQueryable(x => x.FID == esbData.FID).FirstOrDefault();
                ESBLogger.LogDebug($"缓存未命中，执行单次查询采购主表记录，FID={esbData.FID.Value}");
            }

            if (masterRecord != null)
            {
                entity.OrderID = masterRecord.OrderID; // 设置关联的主表ID（采购订单主表主键是OrderID）
                ESBLogger.LogDebug($"明细记录FENTRYID={esbData.FENTRYID}关联到主表OrderID={masterRecord.OrderID}，主表FID={esbData.FID}");
                return true;
            }
            else
            {
                ESBLogger.LogWarning($"未找到FID={esbData.FID}对应的主表记录，明细记录FENTRYID={esbData.FENTRYID}的ID字段将为空");
                entity.OrderID = null;
                return false;
            }
        }



        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_PurchaseOrderDetail> toUpdate, List<OCP_PurchaseOrderDetail> toInsert)
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
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条采购订单明细记录");
                    }

                    // 使用AddRange批量插入
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条采购订单明细记录");
                    }

                    // 一次性保存所有更改
                    _repository.SaveChanges();
                    
                    return webResponse.OK($"采购订单明细批量操作完成，更新 {toUpdate.Count} 条，插入 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, $"采购订单明细批量数据库操作失败：{ex.Message}");
                    return webResponse.Error($"采购订单明细批量操作失败：{ex.Message}");
                }
            }));
        }

        #endregion
    }
} 