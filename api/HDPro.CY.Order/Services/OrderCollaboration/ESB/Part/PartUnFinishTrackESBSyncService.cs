using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.ESB;
using HDPro.Entity.SystemModels;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IRepositories;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using HDPro.Core.BaseProvider;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.Part
{
    /// <summary>
    /// 部件未完跟踪ESB同步服务
    /// 对接ESB接口：/gateway/DataCenter/SearchPartProgress
    /// </summary>
    public class PartUnFinishTrackESBSyncService : ESBSyncServiceBase<OCP_PartUnFinishTracking, ESBPartProgressData, IOCP_PartUnFinishTrackingRepository>
    {
        private readonly ESBLogger _esbLogger;

        public PartUnFinishTrackESBSyncService(
            IOCP_PartUnFinishTrackingRepository repository,
            ESBBaseService esbBaseService,
            ILogger<PartUnFinishTrackESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbBaseService, logger)
        {
            _esbLogger = ESBLoggerFactory.CreatePartTrackingLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供部件跟踪专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 抽象方法实现

        /// <summary>
        /// 获取ESB接口配置名称
        /// </summary>
        protected override string GetESBApiConfigName()
        {
            return nameof(PartUnFinishTrackESBSyncService);
        }

        /// <summary>
        /// 获取操作类型名称
        /// </summary>
        protected override string GetOperationType()
        {
            return "部件未完跟踪";
        }

        /// <summary>
        /// 验证ESB数据有效性
        /// </summary>
        protected override bool ValidateESBData(ESBPartProgressData esbData)
        {
            // 基本字段验证
            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("部件未完跟踪", "FENTRYID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            // 复合主键ID验证
            if (string.IsNullOrWhiteSpace(esbData.ID))
            {
                ESBLogger.LogValidationError("部件未完跟踪", "复合主键ID为空", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            // 物料ID验证
            if (esbData.FMATERIALID.HasValue && esbData.FMATERIALID.Value <= 0)
            {
                ESBLogger.LogValidationError("部件未完跟踪", "物料ID无效", $"FENTRYID={esbData.FENTRYID}，MaterialID={esbData.FMATERIALID}");
                return false;
            }

            // 数量字段验证
            if (esbData.FQTY.HasValue && esbData.FQTY.Value < 0)
            {
                ESBLogger.LogValidationError("部件未完跟踪", "生产数量不能为负数", $"FENTRYID={esbData.FENTRYID}，Qty={esbData.FQTY}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取实体唯一标识
        /// </summary>
        protected override object GetEntityKey(ESBPartProgressData esbData)
        {
            // 使用复合主键ID作为唯一标识
            return esbData.ID ?? $"{esbData.FENTRYID}+{esbData.WorkOrder_Code}";
        }

        /// <summary>
        /// 查询现有记录
        /// </summary>
        protected override async Task<List<OCP_PartUnFinishTracking>> QueryExistingRecords(List<object> keys)
        {
            try
            {
                var idList = keys.Select(k => k.ToString()).ToList();
                
                // 优先使用ESBID进行查询
                var entities = await ((IRepository<OCP_PartUnFinishTracking>)_repository).DbContext.Set<OCP_PartUnFinishTracking>()
                    .Where(x => !string.IsNullOrEmpty(x.ESBID) && idList.Contains(x.ESBID))
                    .ToListAsync();

                // 如果通过ESBID找不到记录，再尝试通过FENTRYID查找（兼容旧数据）
                if (!entities.Any())
                {
                    var entryIdKeys = keys.Where(k => k != null).ToList();
                    if (entryIdKeys.Any())
                    {
                        // 尝试解析复合ID中的FENTRYID部分
                        var entryIds = new List<long>();
                        foreach (var key in entryIdKeys)
                        {
                            var keyStr = key.ToString();
                            var parts = keyStr.Split('+');
                            if (parts.Length > 0 && long.TryParse(parts[0], out long entryId))
                            {
                                entryIds.Add(entryId);
                            }
                        }

                        if (entryIds.Any())
                        {
                            entities = await ((IRepository<OCP_PartUnFinishTracking>)_repository).DbContext.Set<OCP_PartUnFinishTracking>()
                                .Where(x => entryIds.Contains(x.FENTRYID.Value))
                                .ToListAsync();
                        }
                    }
                }

                return entities.GroupBy(e => e.TrackID).Select(g => g.First()).ToList();
            }
            catch (Exception ex)
            {
                ESBLogger.LogError(ex, "查询现有部件未完跟踪记录时发生异常");
                return new List<OCP_PartUnFinishTracking>();
            }
        }

        /// <summary>
        /// 判断现有记录是否匹配ESB数据
        /// </summary>
        protected override bool IsEntityMatch(OCP_PartUnFinishTracking entity, ESBPartProgressData esbData)
        {
            // 优先使用ESBID匹配
            if (!string.IsNullOrWhiteSpace(entity.ESBID) && !string.IsNullOrWhiteSpace(esbData.ID))
            {
                return entity.ESBID == esbData.ID;
            }

            // 备用匹配：FENTRYID匹配（用于兼容旧数据）
            if (esbData.FENTRYID > 0 && entity.FENTRYID.HasValue)
            {
                return entity.FENTRYID == esbData.FENTRYID;
            }

            return false;
        }

        /// <summary>
        /// 将ESB数据映射到实体
        /// </summary>
        protected override void MapESBDataToEntity(ESBPartProgressData esbData, OCP_PartUnFinishTracking entity)
        {
            // ESB主键赋值 - 这是关键的ESBID字段
            entity.ESBID = esbData.ID ?? $"{esbData.FENTRYID}+{esbData.WorkOrder_Code}";
            
            // 基本信息映射
            entity.FENTRYID = esbData.FENTRYID;
            entity.PlanTraceNo = esbData.FMTONO;
            entity.ProductionOrderNo = esbData.FBILLNO;
            entity.MOBillNo = esbData.WorkOrder_Code;

            // 物料信息映射
            entity.MaterialID = esbData.FMATERIALID;

            // 计划信息映射
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;

            // 时间字段映射
            entity.MaterialPickDate = ParseDate(esbData.LLTIME);
            entity.PreCompleteDate = ParseDate(esbData.YZTIME);
            entity.ValvePartCompleteDate = ParseDate(esbData.FTBJZPTIME);
            entity.PressureCompleteDate = ParseDate(esbData.QYXLTIME);
            entity.InspectionDate = ParseDate(esbData.JYTIME);

            // 数量信息映射
            entity.ProductionQty = esbData.FQTY;
            entity.InboundQty = esbData.FSTOCKINQUAAUXQTY;

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
        /// 执行批量数据库操作
        /// </summary>
        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_PartUnFinishTracking> toUpdate, List<OCP_PartUnFinishTracking> toInsert)
        {
            var response = new WebResponseContent();

            try
            {
                var totalOperations = toUpdate.Count + toInsert.Count;
                
                if (totalOperations == 0)
                {
                    return response.OK("没有需要处理的部件未完跟踪数据");
                }

                using (var transaction = await ((IRepository<OCP_PartUnFinishTracking>)_repository).DbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 批量更新
                        if (toUpdate.Any())
                        {
                            ((IRepository<OCP_PartUnFinishTracking>)_repository).DbContext.Set<OCP_PartUnFinishTracking>().UpdateRange(toUpdate);
                            ESBLogger.LogInfo($"准备更新 {toUpdate.Count} 条部件未完跟踪记录");
                        }

                        // 批量插入
                        if (toInsert.Any())
                        {
                            await ((IRepository<OCP_PartUnFinishTracking>)_repository).DbContext.Set<OCP_PartUnFinishTracking>().AddRangeAsync(toInsert);
                            ESBLogger.LogInfo($"准备插入 {toInsert.Count} 条部件未完跟踪记录");
                        }

                        var affectedRows = await ((IRepository<OCP_PartUnFinishTracking>)_repository).DbContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        var message = $"部件未完跟踪数据批量操作成功：更新 {toUpdate.Count} 条，插入 {toInsert.Count} 条，影响行数 {affectedRows}";
                        ESBLogger.LogInfo(message);
                        return response.OK(message);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ESBLogger.LogError(ex, "部件未完跟踪数据批量操作失败，已回滚");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"部件未完跟踪数据批量操作发生异常：{ex.Message}";
                ESBLogger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        #endregion

        #region 批量预查询优化

        /// <summary>
        /// 从ESB数据列表中提取物料ID进行批量预查询
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBPartProgressData> esbDataList)
        {
            return esbDataList
                .Where(x => x.FMATERIALID.HasValue && x.FMATERIALID.Value > 0)
                .Select(x => x.FMATERIALID)
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
        protected override void MapESBDataToEntityWithCache(ESBPartProgressData esbData, OCP_PartUnFinishTracking entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // ESB主键赋值 - 这是关键的ESBID字段
            entity.ESBID = esbData.ID ?? $"{esbData.FENTRYID}+{esbData.WorkOrder_Code}";
            
            // 基本信息映射
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
                    // 基础物料字段
                    entity.MaterialCode = material.MaterialCode ?? string.Empty;
                    entity.MaterialName = material.MaterialName ?? string.Empty;
                    entity.Specification = material.SpecModel ?? string.Empty;        // 规格型号
                    entity.ProductModel = material.ProductModel ?? string.Empty;      // 产品型号
                    entity.NominalDiameter = material.NominalDiameter ?? string.Empty; // 公称通径
                    entity.NominalPressure = material.NominalPressure ?? string.Empty; // 公称压力
                    entity.FlowCharacteristic = material.FlowCharacteristic ?? string.Empty; // 流量特性
                    entity.PackingForm = material.PackingForm ?? string.Empty;        // 填料形式
                    entity.FlangeConnection = material.FlangeConnection ?? string.Empty; // 法兰连接方式
                    entity.ActuatorModel = material.ActuatorModel ?? string.Empty;    // 执行机构型号
                    entity.ActuatorStroke = material.ActuatorStroke ?? string.Empty;  // 执行机构行程
                    entity.ErpClsid = material.ErpClsid ?? string.Empty;              // 物料属性
                }
                else
                {
                    // 物料信息不存在时的默认处理
                    entity.MaterialCode = string.Empty;
                    entity.MaterialName = string.Empty;
                    entity.Specification = string.Empty;
                    entity.ProductModel = string.Empty;
                    entity.NominalDiameter = string.Empty;
                    entity.NominalPressure = string.Empty;
                    entity.FlowCharacteristic = string.Empty;
                    entity.PackingForm = string.Empty;
                    entity.FlangeConnection = string.Empty;
                    entity.ActuatorModel = string.Empty;
                    entity.ActuatorStroke = string.Empty;
                    entity.ErpClsid = string.Empty;

                    // 记录警告日志
                    if (esbData.FMATERIALID.HasValue && esbData.FMATERIALID.Value > 0)
                    {
                        ESBLogger.LogWarning($"部件跟踪ESB同步：未找到物料信息，物料ID={esbData.FMATERIALID.Value}，跟踪ID={esbData.ID}");
                    }
                }
            }
            else
            {
                // 无物料ID或缓存为空时的默认处理
                entity.MaterialCode = string.Empty;
                entity.MaterialName = string.Empty;
                entity.Specification = string.Empty;
                entity.ProductModel = string.Empty;
                entity.NominalDiameter = string.Empty;
                entity.NominalPressure = string.Empty;
                entity.FlowCharacteristic = string.Empty;
                entity.PackingForm = string.Empty;
                entity.FlangeConnection = string.Empty;
                entity.ActuatorModel = string.Empty;
                entity.ActuatorStroke = string.Empty;
                entity.ErpClsid = string.Empty;
            }

            // 计划信息映射
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;

            // 时间字段映射
            entity.MaterialPickDate = ParseDate(esbData.LLTIME);
            entity.PreCompleteDate = ParseDate(esbData.YZTIME);
            entity.ValvePartCompleteDate = ParseDate(esbData.FTBJZPTIME);
            entity.PressureCompleteDate = ParseDate(esbData.QYXLTIME);
            entity.InspectionDate = ParseDate(esbData.JYTIME);
            entity.PlanCompleteDate = ParseDate(esbData.FSCPLANFINISHDATE);
            entity.CompleteDate = ParseDate(esbData.FSCFINISHDATE);

            // 状态信息映射
            entity.BillStatus = esbData.FSTATUSNAME ?? string.Empty;
            entity.ExecuteStatus = esbData.ExecuteStateNAME ?? string.Empty;
            entity.ProScheduleYearMonth = esbData.FZRWBILLNO ?? string.Empty;

            // 数量信息映射
            entity.ProductionQty = esbData.FQTY;
            entity.InboundQty = esbData.FSTOCKINQUAAUXQTY;

            // MES批次号映射
            entity.MESBatchNo = esbData.Retrospect_Code ?? string.Empty;

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