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

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.SubOrder
{
    /// <summary>
    /// 委外订单明细ESB同步服务实现
    /// </summary>
    public class SubOrderDetailESBSyncService : ESBSyncServiceBase<OCP_SubOrderDetail, ESBSubOrderDetailData, IOCP_SubOrderDetailRepository>
    {
        private readonly IOCP_SubOrderRepository _subOrderRepository;
        private readonly ESBLogger _esbLogger;

        public SubOrderDetailESBSyncService(
            IOCP_SubOrderDetailRepository repository,
            IOCP_SubOrderRepository subOrderRepository,
            ESBBaseService esbService,
            ILogger<SubOrderDetailESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _subOrderRepository = subOrderRepository;
            _esbLogger = ESBLoggerFactory.CreateSubOrderLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供委外订单专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        protected override string GetESBApiConfigName()
        {
            return nameof(SubOrderDetailESBSyncService);
        }
        /// <summary>
        /// 获取操作类型描述
        /// </summary>
        protected override string GetOperationType()
        {
            return "委外订单明细";
        }
        protected override bool ValidateESBData(ESBSubOrderDetailData esbData)
        {
            // 仅验证必填字段FENTRYID
            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("委外订单明细", "FENTRYID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            return true;
        }

        protected override object GetEntityKey(ESBSubOrderDetailData esbData)
        {
            return esbData.FENTRYID;
        }

        protected override async Task<List<OCP_SubOrderDetail>> QueryExistingRecords(List<object> keys)
        {
            var entryIds = keys.Cast<int>().Select(x => (long)x).ToList();
            return await Task.Run(() =>
                _repository.FindAsIQueryable(x => x.FENTRYID.HasValue && entryIds.Contains(x.FENTRYID.Value))
                .ToList());
        }

        protected override bool IsEntityMatch(OCP_SubOrderDetail entity, ESBSubOrderDetailData esbData)
        {
            return entity.FENTRYID == esbData.FENTRYID;
        }

        protected override void MapESBDataToEntity(ESBSubOrderDetailData esbData, OCP_SubOrderDetail entity)
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
        protected override void MapESBDataToEntityWithCache(ESBSubOrderDetailData esbData, OCP_SubOrderDetail entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // 使用缓存优化的映射
            MapESBDataToEntityCore(esbData, entity, materialRecordsCache, supplierRecordsCache);
        }

        /// <summary>
        /// 重写提取物料ID方法，从委外订单明细数据中提取物料ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBSubOrderDetailData> esbDataList)
        {
            if (esbDataList == null || !esbDataList.Any())
                return new List<long?>();

            return esbDataList
                .Where(x => x.FMATERIALID.HasValue && x.FMATERIALID.Value > 0)
                .Select(x => (long?)x.FMATERIALID.Value)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 重写提取供应商ID方法，从委外订单明细数据中提取供应商ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>供应商ID列表</returns>
        protected override List<int?> ExtractSupplierIds(List<ESBSubOrderDetailData> esbDataList)
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
        /// 映射ESB数据到实体的核心方法
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <param name="supplierDict">供应商信息字典（可选，用于性能优化）</param>
        private void MapESBDataToEntityCore(ESBSubOrderDetailData esbData, OCP_SubOrderDetail entity, 
            Dictionary<long, OCP_Material> materialDict = null, Dictionary<long, OCP_Supplier> supplierDict = null)
        {
            // 基本信息映射
            entity.FENTRYID = esbData.FENTRYID;
            entity.FID = esbData.FID;
            entity.Seq = esbData.FSEQ;
            entity.MtoNo = esbData.FMTONO;
            entity.MaterialID = esbData.FMATERIALID;

            // 物料信息映射（优先使用缓存）
            entity.MaterialNumber = GetMaterialCodeById(esbData.FMATERIALID, materialDict);
            entity.MaterialName = GetMaterialNameById(esbData.FMATERIALID, materialDict);
            
            // 供应商信息映射（优先使用缓存）
            entity.SupplierID = esbData.FSUPPLIERID;
            entity.SupplierCode = GetSupplierCodeById(esbData.FSUPPLIERID, supplierDict);
            entity.DeliveryNo = esbData.F_BLN_DELIVERYNO;
            
            // 数量信息
            entity.PurchaseQty = esbData.FQTY;
            entity.InstockQty = esbData.FREALQTY;
            entity.UnfinishedQty = esbData.FWWQTY;
            
            
            // 日期信息
            entity.PickDate = _esbService.ParseDateTime(esbData.F_BLN_LLRQ);
        }

        /// <summary>
        /// 批量预查询主表记录，优化性能避免N+1查询问题
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>主表记录字典，Key为FID，Value为主表记录</returns>
        protected override async Task<Dictionary<long, object>> PreQueryMasterRecords(List<ESBSubOrderDetailData> esbDataList)
        {
            var result = new Dictionary<long, object>();
            
            if (esbDataList == null || !esbDataList.Any())
                return result;

            try
            {
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
                    _subOrderRepository.FindAsIQueryable(x => x.FID.HasValue && fids.Contains(x.FID.Value))
                    .ToList());

                // 构建FID到主表记录的映射字典
                foreach (var record in masterRecords)
                {
                    if (record.FID.HasValue)
                    {
                        result[record.FID.Value] = record;
                    }
                }

                ESBLogger.LogInfo($"批量查询主表记录完成：查询 {fids.Count} 个FID，找到 {result.Count} 条主表记录");
                
                return result;
            }
            catch (Exception ex)
            {
                ESBLogger.LogError(ex, $"批量预查询委外订单主表记录失败：{ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// 重写设置主表ID方法，使用缓存优化性能
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">实体对象</param>
        /// <param name="masterRecordsCache">主表记录缓存</param>
        /// <returns>是否成功设置主表ID</returns>
        protected override bool SetMasterTableId(ESBSubOrderDetailData esbData, OCP_SubOrderDetail entity, Dictionary<long, object> masterRecordsCache = null)
        {
            if (!esbData.FID.HasValue || esbData.FID.Value <= 0)
            {
                ESBLogger.LogWarning($"ESB数据FID无效，无法设置主表ID，明细记录FENTRYID={esbData.FENTRYID}");
                entity.ID = null;
                return false;
            }

            OCP_SubOrder masterRecord = null;
            var fid = (long)esbData.FID.Value;

            // 优先使用缓存查询，提升性能
            if (masterRecordsCache != null && masterRecordsCache.ContainsKey(fid))
            {
                masterRecord = masterRecordsCache[fid] as OCP_SubOrder;
                ESBLogger.LogDebug($"从缓存获取主表记录，FID={fid}");
            }
            else
            {
                // 缓存未命中，降级到单次查询
                masterRecord = _subOrderRepository.FindAsIQueryable(x => x.FID == fid).FirstOrDefault();
                ESBLogger.LogDebug($"缓存未命中，执行单次查询主表记录，FID={fid}");
            }

            if (masterRecord != null)
            {
                entity.ID = masterRecord.ID;
                ESBLogger.LogDebug($"明细记录FENTRYID={esbData.FENTRYID}关联到主表ID={masterRecord.ID}，主表FID={fid}");
                return true;
            }
            else
            {
                ESBLogger.LogWarning($"未找到FID={fid}对应的主表记录，明细记录FENTRYID={esbData.FENTRYID}的ID字段将为空");
                entity.ID = null;
                return false;
            }
        }

        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_SubOrderDetail> toUpdate, List<OCP_SubOrderDetail> toInsert)
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
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条委外订单明细记录");
                    }

                    // 使用AddRange批量插入
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条委外订单明细记录");
                    }

                    // 一次性保存所有更改
                    _repository.SaveChanges();
                    
                    return webResponse.OK($"委外订单明细批量操作完成，更新 {toUpdate.Count} 条，插入 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, $"委外订单明细批量数据库操作失败：{ex.Message}");
                    return webResponse.Error($"委外订单明细批量操作失败：{ex.Message}");
                }
            }));
        }

        #endregion
    }
} 