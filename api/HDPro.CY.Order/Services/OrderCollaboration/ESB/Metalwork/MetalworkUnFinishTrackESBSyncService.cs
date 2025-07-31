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
    /// 金工未完工跟踪ESB同步服务
    ///
    /// ESB字段映射说明：
    /// 基本信息字段：
    /// - ID → ESBID: ESB主键，由生产订单明细主键+MES工单ID组成
    /// - FENTRYID → FENTRYID: 生产订单明细主键
    /// - FMTONO → PlanTraceNo: 计划跟踪号
    /// - FBILLNO → ProductionOrderNo: 生产订单号
    /// - WorkOrder_Code → MOBillNo: MES工单号
    /// - FMATERIALID → MaterialID: 物料ID
    ///
    /// 状态信息字段：
    /// - FSTATUSNAME → BillStatus: 生产订单状态
    /// - ExecuteStateNAME → ExecuteStatus: 执行状态（异常，准备中，执行中，已完成）
    /// - ProcessStateName → CurrentProcessStatus: 当前工序状态
    ///
    /// 计划信息字段：
    /// - FCUSTUNMONTH → PlanTaskMonth: 催货单位任务时间月份
    /// - FCUSTUNWEEK → PlanTaskWeek: 催货单位任务时间周
    /// - FCUSTUNEMER → Urgency: 催货单位紧急等级
    ///
    /// 日期字段：
    /// - FSTARTDATE → StartDate: 生产订单实际开工时间
    /// - FMESJLRQ → MaterialRequestDate: MES叫料日期
    /// - GXWWFCDATE → ProcessSubOutDate: 工序委外发出日期
    /// - GXWWRKDATE → ProcessSubInstockDate: 工序入库日期
    /// - FSCPLANFINISHDATE → PlanCompleteDate: 计划完工日期
    /// - FSCFINISHDATE → CompleteDate: 实际完工日期
    ///
    /// 工序信息字段：
    /// - FMESCurrentProcedure → CurrentProcess: MES当前工序
    ///
    /// 数量字段：
    /// - FQTY → ProductionQty: 生产数量
    /// - FSTOCKINQUAAUXQTY → InboundQty: 入库数量
    ///
    /// 其他字段：
    /// - Retrospect_Code → MESBatchNo: MES批次号
    /// - FZRWBILLNO → ProScheduleYearMonth: 排产年月/关联总任务单据编号
    ///
    /// 物料信息字段（从物料表获取）：
    /// - MaterialNumber: 从物料表MaterialCode获取
    /// - MaterialName: 从物料表MaterialName获取
    /// - Specification: 从物料表SpecModel获取（规格型号）
    /// - MaterialCategory: 根据编码规则确定物料分类
    /// </summary>
    public class MetalworkUnFinishTrackESBSyncService : ESBSyncServiceBase<OCP_JGUnFinishTrack, ESBJGUnFinishTrackData, IOCP_JGUnFinishTrackRepository>
    {
        private readonly ESBLogger _esbLogger;

        public MetalworkUnFinishTrackESBSyncService(
            IOCP_JGUnFinishTrackRepository repository,
            ESBBaseService esbService,
            ILogger<MetalworkUnFinishTrackESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _esbLogger = ESBLoggerFactory.CreateMetalworkTrackingLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供金工跟踪专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        /// <summary>
        /// 获取操作类型描述
        /// </summary>
        protected override string GetOperationType()
        {
            return "金工未完工跟踪";
        }

        /// <summary>
        /// 验证ESB数据有效性
        /// </summary>
        protected override bool ValidateESBData(ESBJGUnFinishTrackData esbData)
        {
            // 基本字段验证
            if (string.IsNullOrWhiteSpace(esbData.ID))
            {
                ESBLogger.LogValidationError("金工未完工跟踪", "ID无效", $"ID={esbData.ID}");
                return false;
            }

            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("金工未完工跟踪", "FENTRYID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取实体主键
        /// </summary>
        protected override object GetEntityKey(ESBJGUnFinishTrackData esbData)
        {
            return esbData.ID;
        }

        /// <summary>
        /// 查询现有记录
        /// </summary>
        protected override async Task<List<OCP_JGUnFinishTrack>> QueryExistingRecords(List<object> keys)
        {
            var idList = keys.Select(k => k.ToString()).ToList();
            return await Task.Run(() =>
                _repository.FindAsIQueryable(x => !string.IsNullOrEmpty(x.ESBID) && idList.Contains(x.ESBID))
                .ToList());
        }

        /// <summary>
        /// 判断现有记录是否匹配ESB数据
        /// </summary>
        protected override bool IsEntityMatch(OCP_JGUnFinishTrack entity, ESBJGUnFinishTrackData esbData)
        {
            // 优先使用ESBID进行匹配
            return entity.ESBID == esbData.ID;
        }

        /// <summary>
        /// 将ESB数据映射到实体
        /// </summary>
        protected override void MapESBDataToEntity(ESBJGUnFinishTrackData esbData, OCP_JGUnFinishTrack entity)
        {
            // ESB主键赋值 - 这是关键的ESBID字段
            entity.ESBID = esbData.ID;
            
            entity.FENTRYID = esbData.FENTRYID;
            entity.PlanTraceNo = esbData.FMTONO;
            entity.ProductionOrderNo = esbData.FBILLNO;
            entity.MOBillNo = esbData.WorkOrder_Code;
            entity.MaterialID = esbData.FMATERIALID;
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;

            // 日期与工序
            entity.StartDate = ParseDate(esbData.FSTARTDATE);
            entity.CurrentProcess = esbData.FMESCurrentProcedure;
            entity.MaterialRequestDate = ParseDate(esbData.FMESJLRQ);

            // 数量与批次
            entity.ProductionQty = esbData.FQTY;
            entity.InboundQty = esbData.FSTOCKINQUAAUXQTY;
            entity.MESBatchNo = esbData.Retrospect_Code;

            // 系统字段
            var now = DateTime.Now;
            if (entity.TrackID <= 0) // 新增
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
        /// 执行批量操作
        /// </summary>
        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_JGUnFinishTrack> toUpdate, List<OCP_JGUnFinishTrack> toInsert)
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
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条金工未完工跟踪记录");
                    }

                    // 使用AddRange批量插入
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条金工未完工跟踪记录");
                    }

                    _repository.SaveChanges();
                    
                    var totalProcessed = toUpdate.Count + toInsert.Count;
                    ESBLogger.LogInfo($"金工未完工跟踪批量操作成功完成，总计处理 {totalProcessed} 条记录");
                    
                    return webResponse.OK($"批量操作成功，更新 {toUpdate.Count} 条，新增 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, "执行金工未完工跟踪批量操作失败");
                    return webResponse.Error($"批量操作失败：{ex.Message}");
                }
            }));
        }

        protected override string GetESBApiConfigName()
        {
            return nameof(MetalworkUnFinishTrackESBSyncService);
        }

        #endregion

        #region 批量预查询优化

        /// <summary>
        /// 从ESB数据列表中提取物料ID进行批量预查询
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBJGUnFinishTrackData> esbDataList)
        {
            return esbDataList
                .Where(x => x.FMATERIALID.HasValue && x.FMATERIALID.Value > 0)
                .Select(x => (long?)x.FMATERIALID.Value)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 使用缓存优化的ESB数据映射方法
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="masterRecordsCache">主表记录缓存</param>
        /// <param name="materialRecordsCache">物料记录缓存</param>
        /// <param name="supplierRecordsCache">供应商记录缓存</param>
        /// <param name="customerRecordsCache">客户记录缓存</param>
        protected override void MapESBDataToEntityWithCache(ESBJGUnFinishTrackData esbData, OCP_JGUnFinishTrack entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // ESB主键赋值 - 这是关键的ESBID字段
            entity.ESBID = esbData.ID;
            
            entity.FENTRYID = esbData.FENTRYID;
            entity.PlanTraceNo = esbData.FMTONO;
            entity.ProductionOrderNo = esbData.FBILLNO;
            entity.MOBillNo = esbData.WorkOrder_Code;

            // 🚀 使用缓存优化的物料信息映射
            entity.MaterialID = esbData.FMATERIALID;
            if (esbData.FMATERIALID.HasValue && materialRecordsCache != null)
            {
                var material = GetMaterialByIdWithDict(esbData.FMATERIALID.Value, materialRecordsCache);
                if (material != null)
                {
                    entity.MaterialNumber = material.MaterialCode ?? string.Empty;
                    entity.MaterialName = material.MaterialName ?? string.Empty;
                    entity.Specification = material.SpecModel ?? string.Empty;        // 规格型号
                    entity.MaterialCategory = GetMaterialCategoryByCode(material.MaterialCode, material.MaterialName); // 根据编码规则确定物料分类
                }
                else
                {
                    // 物料信息不存在时的默认处理
                    entity.MaterialNumber = string.Empty;
                    entity.MaterialName = string.Empty;
                    entity.Specification = string.Empty;
                    entity.MaterialCategory = string.Empty;

                    // 记录警告日志
                    if (esbData.FMATERIALID.HasValue && esbData.FMATERIALID.Value > 0)
                    {
                        ESBLogger.LogWarning($"金工跟踪ESB同步：未找到物料信息，物料ID={esbData.FMATERIALID.Value}，跟踪ID={esbData.ID}");
                    }
                }
            }
            else
            {
                // 无物料ID或缓存为空时的默认处理
                entity.MaterialNumber = string.Empty;
                entity.MaterialName = string.Empty;
                entity.Specification = string.Empty;
                entity.MaterialCategory = string.Empty;
            }

            // 计划信息映射
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;

            // 基本信息映射
            entity.ESBID = esbData.ID;
            entity.FENTRYID = esbData.FENTRYID;
            entity.PlanTraceNo = esbData.FMTONO;
            entity.ProductionOrderNo = esbData.FBILLNO;
            entity.MOBillNo = esbData.WorkOrder_Code;

            // 状态信息映射
            entity.BillStatus = esbData.FSTATUSNAME ?? string.Empty;
            entity.ExecuteStatus = esbData.ExecuteStateNAME ?? string.Empty;
            entity.CurrentProcessStatus = esbData.ProcessStateName ?? string.Empty;

            // 日期字段映射
            entity.StartDate = ParseDate(esbData.FSTARTDATE);
            entity.MaterialRequestDate = ParseDate(esbData.FMESJLRQ);
            entity.ProcessSubOutDate = ParseDate(esbData.GXWWFCDATE);
            entity.ProcessSubInstockDate = ParseDate(esbData.GXWWRKDATE);
            entity.PlanCompleteDate = ParseDate(esbData.FSCPLANFINISHDATE);
            entity.CompleteDate = ParseDate(esbData.FSCFINISHDATE);

            // 工序信息映射
            entity.CurrentProcess = esbData.FMESCurrentProcedure ?? string.Empty;

            // 数量信息映射
            entity.ProductionQty = esbData.FQTY;
            entity.InboundQty = esbData.FSTOCKINQUAAUXQTY;

            // MES批次号映射
            entity.MESBatchNo = esbData.Retrospect_Code ?? string.Empty;

            // 排产年月映射（使用FZRWBILLNO字段）
            entity.ProScheduleYearMonth = esbData.FZRWBILLNO ?? string.Empty;

            // 系统字段
            var now = DateTime.Now;
            if (entity.TrackID <= 0) // 新增
            {
                entity.CreateDate = now;
                entity.CreateID = 1; // 系统用户
                entity.Creator = "ESB";
            }
            entity.ModifyDate = now;
            entity.ModifyID = 1;
            entity.Modifier = "ESB";
        }

        #endregion
    }
} 