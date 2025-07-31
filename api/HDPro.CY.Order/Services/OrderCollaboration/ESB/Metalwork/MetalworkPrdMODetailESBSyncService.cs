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

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.Metalwork
{
    /// <summary>
    /// 金工生产订单明细ESB同步服务
    /// </summary>
    public class MetalworkPrdMODetailESBSyncService : ESBSyncServiceBase<OCP_JGPrdMODetail, ESBJGPrdMODetailData, IOCP_JGPrdMODetailRepository>
    {
        private readonly IOCP_JGPrdMORepository _jgPrdMORepository;
        private readonly ESBLogger _esbLogger;

        public MetalworkPrdMODetailESBSyncService(
            IOCP_JGPrdMODetailRepository repository,
            IOCP_JGPrdMORepository jgPrdMORepository,
            ESBBaseService esbService,
            ILogger<MetalworkPrdMODetailESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _jgPrdMORepository = jgPrdMORepository;
            _esbLogger = ESBLoggerFactory.CreateMetalworkTrackingLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供金工跟踪专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        /// <summary>
        /// 获取ESB接口配置名称
        /// </summary>
        protected override string GetESBApiConfigName()
        {
            return nameof(MetalworkPrdMODetailESBSyncService);
        }

        /// <summary>
        /// 获取操作类型描述
        /// </summary>
        protected override string GetOperationType()
        {
            return "金工生产订单明细";
        }

        /// <summary>
        /// 验证ESB数据有效性
        /// </summary>
        protected override bool ValidateESBData(ESBJGPrdMODetailData esbData)
        {
            // 基本字段验证
            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("金工生产订单明细", "FENTRYID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            if (!esbData.FID.HasValue || esbData.FID <= 0)
            {
                ESBLogger.LogValidationError("金工生产订单明细", "FID无效", $"FENTRYID={esbData.FENTRYID}，FID={esbData.FID}");
                return false;
            }

            // 物料ID验证
            if (esbData.FMATERIALID.HasValue && esbData.FMATERIALID.Value <= 0)
            {
                ESBLogger.LogValidationError("金工生产订单明细", "物料ID无效", $"FENTRYID={esbData.FENTRYID}，MaterialID={esbData.FMATERIALID}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取实体主键
        /// </summary>
        protected override object GetEntityKey(ESBJGPrdMODetailData esbData)
        {
            return (long)esbData.FENTRYID;
        }

        /// <summary>
        /// 查询现有记录
        /// </summary>
        protected override async Task<List<OCP_JGPrdMODetail>> QueryExistingRecords(List<object> keys)
        {
            var entryIdList = keys.Select(k => Convert.ToInt64(k)).ToList();
            return await Task.Run(() =>
                _repository.FindAsIQueryable(x => x.FENTRYID.HasValue && entryIdList.Contains(x.FENTRYID.Value))
                .ToList());
        }

        /// <summary>
        /// 判断现有记录是否匹配ESB数据
        /// </summary>
        protected override bool IsEntityMatch(OCP_JGPrdMODetail entity, ESBJGPrdMODetailData esbData)
        {
            return entity.FENTRYID == esbData.FENTRYID;
        }

        /// <summary>
        /// 将ESB数据映射到实体
        /// </summary>
        protected override void MapESBDataToEntity(ESBJGPrdMODetailData esbData, OCP_JGPrdMODetail entity)
        {
            MapESBDataToEntityCore(esbData, entity, null);
        }

        /// <summary>
        /// 重写带缓存的映射方法，优化物料查询性能
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="masterRecordsCache">主表记录缓存</param>
        /// <param name="materialRecordsCache">物料记录缓存</param>
        /// <param name="supplierRecordsCache">供应商记录缓存</param>
        /// <param name="customerRecordsCache">客户记录缓存</param>
        protected override void MapESBDataToEntityWithCache(ESBJGPrdMODetailData esbData, OCP_JGPrdMODetail entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // 使用缓存优化的映射
            MapESBDataToEntityCore(esbData, entity, materialRecordsCache, masterRecordsCache);
        }

        /// <summary>
        /// 重写提取物料ID方法，从金工生产订单明细数据中提取物料ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBJGPrdMODetailData> esbDataList)
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
        /// 映射ESB数据到实体的核心方法
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <param name="masterRecordsCache">主表记录缓存（可选，用于性能优化）</param>
        private void MapESBDataToEntityCore(ESBJGPrdMODetailData esbData, OCP_JGPrdMODetail entity, Dictionary<long, OCP_Material> materialDict = null, Dictionary<long, object> masterRecordsCache = null)
        {
            // 主键与基础信息
            entity.FENTRYID = esbData.FENTRYID;
            entity.FID = esbData.FID.HasValue ? (long?)esbData.FID.Value : null;
            entity.LineNumber = esbData.FSEQ;
            entity.ProductionOrderStatus = esbData.FSTATUSNAME;
            entity.PlanTraceNo = esbData.FMTONO;
            entity.MaterialID = esbData.FMATERIALID.HasValue ? (long?)esbData.FMATERIALID.Value : null;

            // 物料信息映射（从物料表获取）
            var materialId = esbData.FMATERIALID.HasValue ? (long?)esbData.FMATERIALID.Value : null;
            entity.MaterialNumber = GetMaterialCodeById(materialId, materialDict);
            entity.MaterialName = GetMaterialNameById(materialId, materialDict);

            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;
            
            // 日期字段映射
            entity.PlanCompleteDate = ParseDate(esbData.FPLANFINISHDATE);
            entity.ActualCompleteDate = ParseDate(esbData.FFINISHDATE);
            entity.PickMtrlDate = ParseDate(esbData.F_BLN_LLRQ);
            
            // 数量字段映射
            entity.ProductionQty = esbData.FQTY;
            entity.InboundQty = esbData.FSTOCKINQUAAUXQTY;
            entity.UnInboundQty = esbData.FNOTINSTOCKQTY;

            // 关联主表ID
            SetMasterTableRelation(esbData, entity, masterRecordsCache);

            // 系统字段
            var now = DateTime.Now;
            if (entity.DetailID <= 0) // 新增
            {
                entity.CreateDate = now;
                entity.CreateID = 1; // 系统用户
                entity.Creator = "ESB";
            }
            entity.ModifyDate = now;
            entity.ModifyID = 1;
            entity.Modifier = "ESB";
        }

        /// <summary>
        /// 设置与主表的关联关系
        /// </summary>
        private void SetMasterTableRelation(ESBJGPrdMODetailData esbData, OCP_JGPrdMODetail entity, Dictionary<long, object> masterRecordsCache = null)
        {
            if (esbData.FID.HasValue && esbData.FID > 0)
            {
                long fidValue = esbData.FID.Value;
                OCP_JGPrdMO masterRecord = null;

                // 优先从缓存中查找
                if (masterRecordsCache != null && masterRecordsCache.ContainsKey(fidValue))
                {
                    masterRecord = masterRecordsCache[fidValue] as OCP_JGPrdMO;
                }
                else
                {
                    // 从数据库查找
                    masterRecord = _jgPrdMORepository.FindFirst(x => x.FID == fidValue);
                }

                if (masterRecord != null)
                {
                    entity.ID = masterRecord.ID; // 设置关联的主表ID
                    _logger.LogDebug($"金工明细记录FENTRYID={esbData.FENTRYID}关联到主表ID={masterRecord.ID}，主表FID={fidValue}");
                }
                else
                {
                    _logger.LogWarning($"未找到FID={fidValue}对应的金工生产订单主表记录，明细记录FENTRYID={esbData.FENTRYID}的ID字段将为空");
                    entity.ID = null;
                }
            }
        }

        /// <summary>
        /// 批量预查询主表记录以优化性能
        /// </summary>
        protected override async Task<Dictionary<long, object>> PreQueryMasterRecords(List<ESBJGPrdMODetailData> esbDataList)
        {
            var fidList = esbDataList.Where(x => x.FID.HasValue && x.FID > 0)
                                   .Select(x => (long)x.FID.Value)
                                   .Distinct()
                                   .ToList();

            if (!fidList.Any())
                return new Dictionary<long, object>();

            var masterRecords = await Task.Run(() =>
                _jgPrdMORepository.FindAsIQueryable(x => x.FID.HasValue && fidList.Contains(x.FID.Value))
                .ToList());

            var result = new Dictionary<long, object>();
            foreach (var record in masterRecords)
            {
                if (record.FID.HasValue)
                {
                    result[record.FID.Value] = record;
                }
            }

            _logger.LogInformation($"预查询到 {result.Count} 条金工生产订单主表记录，用于关联 {esbDataList.Count} 条明细记录");
            return result;
        }

        /// <summary>
        /// 执行批量操作
        /// </summary>
        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_JGPrdMODetail> toUpdate, List<OCP_JGPrdMODetail> toInsert)
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
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条金工生产订单明细记录");
                    }

                    // 使用AddRange批量插入
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条金工生产订单明细记录");
                    }

                    _repository.SaveChanges();
                    
                    var totalProcessed = toUpdate.Count + toInsert.Count;
                    ESBLogger.LogInfo($"金工生产订单明细批量操作成功完成，总计处理 {totalProcessed} 条记录");
                    
                    return webResponse.OK($"批量操作成功，更新 {toUpdate.Count} 条，新增 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, "执行金工生产订单明细批量操作失败");
                    return webResponse.Error($"批量操作失败：{ex.Message}");
                }
            }));
        }

        #endregion
    }
} 