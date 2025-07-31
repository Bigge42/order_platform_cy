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

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.WholeUnit
{
    /// <summary>
    /// 整机生产订单明细ESB同步服务实现
    /// 对接ESB接口：/SearchPrdMOEntry (整机版本)
    /// 将ESB数据同步到OCP_PrdMODetail表
    /// </summary>
    public class WholeUnitPrdMODetailESBSyncService : ESBSyncServiceBase<OCP_PrdMODetail, ESBWholeUnitPrdMODetailData, IOCP_PrdMODetailRepository>
    {
        private readonly IOCP_PrdMORepository _prdMORepository;
        private readonly ESBLogger _esbLogger;

        public WholeUnitPrdMODetailESBSyncService(
            IOCP_PrdMODetailRepository repository,
            IOCP_PrdMORepository prdMORepository,
            ESBBaseService esbService,
            ILogger<WholeUnitPrdMODetailESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _prdMORepository = prdMORepository;
            _esbLogger = ESBLoggerFactory.CreateWholeUnitTrackingLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供整机跟踪专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        /// <summary>
        /// 获取操作类型描述
        /// </summary>
        protected override string GetOperationType()
        {
            return "整机生产订单明细";
        }

        /// <summary>
        /// 验证ESB数据有效性
        /// </summary>
        protected override bool ValidateESBData(ESBWholeUnitPrdMODetailData esbData)
        {
            // 基本字段验证
            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("整机生产订单明细", "FENTRYID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            // 可选：验证物料ID（如果提供的话）
            if (esbData.FMATERIALID.HasValue && esbData.FMATERIALID <= 0)
            {
                ESBLogger.LogValidationError("整机生产订单明细", "物料ID无效", $"FENTRYID={esbData.FENTRYID}，FMATERIALID={esbData.FMATERIALID}");
                return false;
            }

            // 可选：验证数量字段（如果提供的话）
            if (esbData.FQTY.HasValue && esbData.FQTY < 0)
            {
                ESBLogger.LogValidationError("整机生产订单明细", "生产数量不能为负数", $"FENTRYID={esbData.FENTRYID}，FQTY={esbData.FQTY}");
                return false;
            }

            if (esbData.FSTOCKINQUAAUXQTY.HasValue && esbData.FSTOCKINQUAAUXQTY < 0)
            {
                ESBLogger.LogValidationError("整机生产订单明细", "入库数量不能为负数", $"FENTRYID={esbData.FENTRYID}，FSTOCKINQUAAUXQTY={esbData.FSTOCKINQUAAUXQTY}");
                return false;
            }

            if (esbData.FNOTINSTOCKQTY.HasValue && esbData.FNOTINSTOCKQTY < 0)
            {
                ESBLogger.LogValidationError("整机生产订单明细", "未入库数量不能为负数", $"FENTRYID={esbData.FENTRYID}，FNOTINSTOCKQTY={esbData.FNOTINSTOCKQTY}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取实体主键
        /// </summary>
        protected override object GetEntityKey(ESBWholeUnitPrdMODetailData esbData)
        {
            return esbData.FENTRYID;
        }

        /// <summary>
        /// 查询现有记录
        /// </summary>
        protected override async Task<List<OCP_PrdMODetail>> QueryExistingRecords(List<object> keys)
        {
            var fentryIdList = keys.Cast<long>().ToList();
            return await Task.Run(() =>
                _repository.FindAsIQueryable(x => x.FENTRYID.HasValue && fentryIdList.Contains(x.FENTRYID.Value))
                .ToList());
        }

        /// <summary>
        /// 判断现有记录是否匹配ESB数据
        /// </summary>
        protected override bool IsEntityMatch(OCP_PrdMODetail entity, ESBWholeUnitPrdMODetailData esbData)
        {
            return entity.FENTRYID == esbData.FENTRYID;
        }

        /// <summary>
        /// 将ESB数据映射到实体
        /// </summary>
        protected override void MapESBDataToEntity(ESBWholeUnitPrdMODetailData esbData, OCP_PrdMODetail entity)
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
        protected override void MapESBDataToEntityWithCache(ESBWholeUnitPrdMODetailData esbData, OCP_PrdMODetail entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // 使用缓存优化的映射
            MapESBDataToEntityCore(esbData, entity, materialRecordsCache);
        }

        /// <summary>
        /// 重写提取物料ID方法，从整机生产订单明细数据中提取物料ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBWholeUnitPrdMODetailData> esbDataList)
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
        private void MapESBDataToEntityCore(ESBWholeUnitPrdMODetailData esbData, OCP_PrdMODetail entity, Dictionary<long, OCP_Material> materialDict = null)
        {
            // 基本信息映射
            entity.FENTRYID = esbData.FENTRYID;
            entity.FID = esbData.FID;
            entity.Seq = esbData.FSEQ;
            
            // 根据新接口文档更新字段映射
            entity.ProductionOrderStatus = esbData.FSTATUSNAME;
            entity.PlanTraceNo = esbData.FMTONO;
            entity.SalesDocumentNo = esbData.FSALBILLNO;
            entity.SalesContractNo = esbData.F_BLN_CONTACTNONAME;

            // 物料信息映射
            entity.MaterialID = esbData.FMATERIALID;
            
            // 物料信息映射（从物料表获取）
            var materialId = esbData.FMATERIALID.HasValue ? (long?)esbData.FMATERIALID.Value : null;
            entity.MaterialNumber = GetMaterialCodeById(materialId, materialDict);
            entity.MaterialName = GetMaterialNameById(materialId, materialDict);

            // 计划信息映射
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;

            // 数量信息映射 - 根据新接口文档更新
            entity.PlanQty = esbData.FQTY;                          // 生产数量
            entity.InboundQty = esbData.FSTOCKINQUAAUXQTY;          // 入库数量
            entity.UnInboundQty = esbData.FNOTINSTOCKQTY;           // 未入库数量

            // 日期字段映射，使用基类的统一日期解析方法
            entity.PlanCompleteDate = ParseDate(esbData.FPLANFINISHDATE);   // 计划完工时间
            entity.ActualCompleteDate = ParseDate(esbData.FFINISHDATE);     // 实际完工时间
            entity.PickMtrlDate = ParseDate(esbData.F_BLN_LLRQ);            // 领料日期

            // 设置主表关联
            SetMasterTableRelation(esbData, entity);

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
        /// 设置主表关联关系
        /// </summary>
        private void SetMasterTableRelation(ESBWholeUnitPrdMODetailData esbData, OCP_PrdMODetail entity)
        {
            try
            {
                // 根据FID查找主表记录
                if (esbData.FID.HasValue && esbData.FID > 0)
                {
                    var masterRecord = _prdMORepository.FindFirst(x => x.FID == esbData.FID);
                    if (masterRecord != null)
                    {
                        entity.ID = masterRecord.ID; // 设置主表ID关联
                        _logger.LogDebug($"成功关联主表记录，FID={esbData.FID}，主表ID={masterRecord.ID}");
                    }
                    else
                    {
                        _logger.LogWarning($"未找到FID={esbData.FID}的主表记录，明细记录FENTRYID={esbData.FENTRYID}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"设置主表关联失败，FID={esbData.FID}，FENTRYID={esbData.FENTRYID}");
            }
        }

        /// <summary>
        /// 执行批量操作
        /// </summary>
        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_PrdMODetail> toUpdate, List<OCP_PrdMODetail> toInsert)
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
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条整机生产订单明细记录");
                    }

                    // 使用AddRange批量插入
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条整机生产订单明细记录");
                    }

                    _repository.SaveChanges();
                    
                    var totalProcessed = toUpdate.Count + toInsert.Count;
                    ESBLogger.LogInfo($"整机生产订单明细批量操作成功完成，总计处理 {totalProcessed} 条记录");
                    
                    return webResponse.OK($"批量操作成功，更新 {toUpdate.Count} 条，新增 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, "执行整机生产订单明细批量操作失败");
                    return webResponse.Error($"批量操作失败：{ex.Message}");
                }
            }));
        }

        /// <summary>
        /// 批量预查询主表记录（性能优化）
        /// </summary>
        protected override async Task<Dictionary<long, object>> PreQueryMasterRecords(List<ESBWholeUnitPrdMODetailData> esbDataList)
        {
            var fidList = esbDataList.Where(x => x.FID.HasValue && x.FID > 0)
                                   .Select(x => x.FID.Value)
                                   .Distinct()
                                   .ToList();

            if (!fidList.Any())
                return new Dictionary<long, object>();

            var masterRecords = await Task.Run(() =>
                _prdMORepository.FindAsIQueryable(x => x.FID.HasValue && fidList.Contains(x.FID.Value))
                .ToDictionary(x => x.FID.Value, x => (object)x.ID));

            _logger.LogInformation($"预查询到 {masterRecords.Count} 条主表记录用于关联");
            return masterRecords;
        }

        /// <summary>
        /// 设置主表ID关联（使用缓存）
        /// </summary>
        protected override bool SetMasterTableId(ESBWholeUnitPrdMODetailData esbData, OCP_PrdMODetail entity, Dictionary<long, object> masterRecordsCache = null)
        {
            if (!esbData.FID.HasValue || esbData.FID <= 0)
                return false;

            if (masterRecordsCache != null && masterRecordsCache.TryGetValue(esbData.FID.Value, out var cachedId))
            {
                entity.ID = (long)cachedId;
                return true;
            }

            // 缓存未命中，执行单独查询
            return SetMasterTableId(esbData, entity);
        }

        /// <summary>
        /// 设置主表ID关联（直接查询）
        /// </summary>
        protected override bool SetMasterTableId(ESBWholeUnitPrdMODetailData esbData, OCP_PrdMODetail entity)
        {
            try
            {
                if (esbData.FID.HasValue && esbData.FID > 0)
                {
                    var masterRecord = _prdMORepository.FindFirst(x => x.FID == esbData.FID);
                    if (masterRecord != null)
                    {
                        entity.ID = masterRecord.ID;
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询主表记录失败，FID={esbData.FID}");
                return false;
            }
        }

        protected override string GetESBApiConfigName()
        {
            return nameof(WholeUnitPrdMODetailESBSyncService);
        }

        #endregion
    }
} 