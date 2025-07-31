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
    /// 整机跟踪ESB同步服务实现
    /// 对接ESB接口：/SearchWholeUnitTracking
    /// 将ESB数据同步到OCP_PrdMOTracking表
    /// </summary>
    public class WholeUnitTrackingESBSyncService : ESBSyncServiceBase<OCP_PrdMOTracking, ESBWholeUnitTrackingData, IOCP_PrdMOTrackingRepository>
    {
        private readonly ESBLogger _esbLogger;

        public WholeUnitTrackingESBSyncService(
            IOCP_PrdMOTrackingRepository repository,
            ESBBaseService esbService,
            ILogger<WholeUnitTrackingESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
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
            return "整机跟踪";
        }

        /// <summary>
        /// 验证ESB数据有效性
        /// </summary>
        protected override bool ValidateESBData(ESBWholeUnitTrackingData esbData)
        {
            // 基本字段验证 - 根据新接口文档更新
            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("整机跟踪", "FENTRYID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(esbData.ID))
            {
                ESBLogger.LogValidationError("整机跟踪", "主键ID为空", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            // 可选：验证物料ID（如果提供的话）
            if (esbData.FMATERIALID.HasValue && esbData.FMATERIALID <= 0)
            {
                ESBLogger.LogValidationError("整机跟踪", "物料ID无效", $"FENTRYID={esbData.FENTRYID}，FMATERIALID={esbData.FMATERIALID}");
                return false;
            }

            // 可选：验证数量字段（如果提供的话）
            if (esbData.FQTY.HasValue && esbData.FQTY < 0)
            {
                ESBLogger.LogValidationError("整机跟踪", "生产数量不能为负数", $"FENTRYID={esbData.FENTRYID}，FQTY={esbData.FQTY}");
                return false;
            }

            if (esbData.FSTOCKINQUAAUXQTY.HasValue && esbData.FSTOCKINQUAAUXQTY < 0)
            {
                ESBLogger.LogValidationError("整机跟踪", "入库数量不能为负数", $"FENTRYID={esbData.FENTRYID}，FSTOCKINQUAAUXQTY={esbData.FSTOCKINQUAAUXQTY}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取实体主键
        /// </summary>
        protected override object GetEntityKey(ESBWholeUnitTrackingData esbData)
        {
            // 根据新接口文档，使用复合主键ID（生产订单明细主键+MES工单ID组成，中间由'+'连接）
            return esbData.ID;
        }

        /// <summary>
        /// 查询现有记录
        /// </summary>
        protected override async Task<List<OCP_PrdMOTracking>> QueryExistingRecords(List<object> keys)
        {
            var idList = keys.Cast<string>().ToList();
            return await Task.Run(() =>
                _repository.FindAsIQueryable(x => !string.IsNullOrEmpty(x.ESBID) && idList.Contains(x.ESBID))
                .ToList());
        }

        /// <summary>
        /// 判断现有记录是否匹配ESB数据
        /// </summary>
        protected override bool IsEntityMatch(OCP_PrdMOTracking entity, ESBWholeUnitTrackingData esbData)
        {
            return entity.ESBID == esbData.ID;
        }

        /// <summary>
        /// 将ESB数据映射到实体
        /// </summary>
        protected override void MapESBDataToEntity(ESBWholeUnitTrackingData esbData, OCP_PrdMOTracking entity)
        {
            // ESB主键赋值 - 这是关键的ESBID字段（由生产订单明细主键+MES工单ID组成，中间由'+'连接）
            entity.ESBID = esbData.ID;
            
            // 基本信息映射 - 根据接口文档更新
            // 注意：FENTRYID字段在实体中暂时没有对应字段，可以通过ESBID解析获取
            entity.PlanTraceNo = esbData.FMTONO;
            entity.ProductionOrderNo = esbData.FBILLNO;
            entity.MOBillNo = esbData.WorkOrder_Code; // MES工单号

            // 物料信息映射
            entity.MaterialID = esbData.FMATERIALID;

            // 计划信息映射
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;

            // 日期字段映射，使用基类的统一日期解析方法
            entity.MaterialPickDate = ParseDate(esbData.LLTIME); // MES领料时间
            entity.PreCompleteDate = ParseDate(esbData.YZTIME); // MES预装时间
            entity.ValvePartCompleteDate = ParseDate(esbData.FTBJZPTIME); // MES阀体部件装配时间
            entity.PressureCompleteDate = ParseDate(esbData.QYXLTIME); // MES强压泄漏试验时间
            entity.FinalInspectionDate = ParseDate(esbData.JYTIME); // MES检验时间
            entity.PlanCompleteDate = ParseDate(esbData.FSCPLANFINISHDATE); // 计划完工日期
            entity.CompleteDate = ParseDate(esbData.FSCFINISHDATE); // 实际完工日期

            // 状态信息映射
            entity.BillStatus = esbData.FSTATUSNAME ?? string.Empty; // 生产订单状态
            entity.ExecuteStatus = esbData.ExecuteStateNAME ?? string.Empty; // 执行状态
            entity.ProScheduleYearMonth = esbData.FZRWBILLNO ?? string.Empty; // 排产年月

            // MES批次号映射
            entity.MESBatchNo = esbData.Retrospect_Code ?? string.Empty;

            // 日志记录无法映射的字段
            if (esbData.FENTRYID > 0)
            {
                ESBLogger.LogInfo($"ESB数据包含生产订单明细主键但实体缺少对应字段：FENTRYID={esbData.FENTRYID}，将通过ESBID存储");
            }
            if (!string.IsNullOrEmpty(esbData.FSTARTDATE))
            {
                ESBLogger.LogInfo($"ESB数据包含实际开工时间但实体缺少对应字段：FSTARTDATE={esbData.FSTARTDATE}，TrackID={entity.TrackID}");
            }
            if (!string.IsNullOrEmpty(esbData.FMESCurrentProcedure))
            {
                ESBLogger.LogInfo($"ESB数据包含MES当前工序但实体缺少对应字段：FMESCurrentProcedure={esbData.FMESCurrentProcedure}，TrackID={entity.TrackID}");
            }

            // 数量信息映射 - 根据新接口文档更新
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
        /// 执行批量操作
        /// </summary>
        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_PrdMOTracking> toUpdate, List<OCP_PrdMOTracking> toInsert)
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
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条整机跟踪记录");
                    }

                    // 使用AddRange批量插入
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条整机跟踪记录");
                    }

                    _repository.SaveChanges();
                    
                    var totalProcessed = toUpdate.Count + toInsert.Count;
                    ESBLogger.LogInfo($"整机跟踪批量操作成功完成，总计处理 {totalProcessed} 条记录");
                    
                    return webResponse.OK($"批量操作成功，更新 {toUpdate.Count} 条，新增 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, "执行整机跟踪批量操作失败");
                    return webResponse.Error($"批量操作失败：{ex.Message}");
                }
            }));
        }

        protected override string GetESBApiConfigName()
        {
            return nameof(WholeUnitTrackingESBSyncService);
        }

        #endregion

        #region 批量预查询优化

        /// <summary>
        /// 从ESB数据列表中提取物料ID进行批量预查询
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBWholeUnitTrackingData> esbDataList)
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
        protected override void MapESBDataToEntityWithCache(ESBWholeUnitTrackingData esbData, OCP_PrdMOTracking entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // ESB主键赋值 - 这是关键的ESBID字段（由生产订单明细主键+MES工单ID组成，中间由'+'连接）
            entity.ESBID = esbData.ID;
            
            // 基本信息映射 - 根据接口文档更新
            entity.PlanTraceNo = esbData.FMTONO;
            entity.ProductionOrderNo = esbData.FBILLNO;
            entity.MOBillNo = esbData.WorkOrder_Code; // MES工单号

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
                        ESBLogger.LogWarning($"整机跟踪ESB同步：未找到物料信息，物料ID={esbData.FMATERIALID.Value}，跟踪ID={esbData.ID}");
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

            // 日期字段映射，使用基类的统一日期解析方法
            entity.MaterialPickDate = ParseDate(esbData.LLTIME); // MES领料时间
            entity.PreCompleteDate = ParseDate(esbData.YZTIME); // MES预装时间
            entity.ValvePartCompleteDate = ParseDate(esbData.FTBJZPTIME); // MES阀体部件装配时间
            entity.PressureCompleteDate = ParseDate(esbData.QYXLTIME); // MES强压泄漏试验时间
            entity.FinalInspectionDate = ParseDate(esbData.JYTIME); // MES检验时间
            entity.PlanCompleteDate = ParseDate(esbData.FSCPLANFINISHDATE); // 计划完工日期
            entity.CompleteDate = ParseDate(esbData.FSCFINISHDATE); // 实际完工日期

            // 状态信息映射
            entity.BillStatus = esbData.FSTATUSNAME ?? string.Empty; // 生产订单状态
            entity.ExecuteStatus = esbData.ExecuteStateNAME ?? string.Empty; // 执行状态
            entity.ProScheduleYearMonth = esbData.FZRWBILLNO ?? string.Empty; // 排产年月

            // MES批次号映射
            entity.MESBatchNo = esbData.Retrospect_Code ?? string.Empty;

            // 数量信息映射 - 根据新接口文档更新
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

        #endregion
    }
} 