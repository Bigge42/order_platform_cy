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

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.TechManagement
{
    /// <summary>
    /// 技术管理ESB同步服务
    /// 
    /// 物料相关字段映射说明：
    /// 1. MaterialID: 直接从ESB数据的FMATERIALID映射
    /// 2. MaterialNumber: 从物料表OCP_Material.MaterialCode获取
    /// 3. MaterialName: 从物料表OCP_Material.MaterialName获取
    /// 4. ProductModel: 从物料表OCP_Material.ProductModel获取（产品型号）
    /// 5. NominalDiameter: 从物料表OCP_Material.NominalDiameter获取（公称通径）
    /// 6. NominalPressure: 从物料表OCP_Material.NominalPressure获取（公称压力）
    /// 7. FlowCharacteristic: 从物料表OCP_Material.FlowCharacteristic获取（流量特性）
    /// 8. PackingForm: 从物料表OCP_Material.PackingForm获取（填料形式）
    /// 9. FlangeConnection: 从物料表OCP_Material.FlangeConnection获取（法兰连接方式）
    /// 10. ActuatorModel: 从物料表OCP_Material.ActuatorModel获取（执行机构型号）
    /// 11. ActuatorStroke: 从物料表OCP_Material.ActuatorStroke获取（执行机构行程）
    /// 12. ErpClsid: 从物料表OCP_Material.ErpClsid获取（物料属性）
    ///
    /// 新增字段映射说明：
    /// 13. StandardDays: 从ESB数据的FBZTS映射（标准天数，decimal转int）
    /// 14. DeliveryDate: 从ESB数据的F_ORA_DATETIME映射（客户要货日期）
    /// 15. ReplyDeliveryDate: 从ESB数据的F_BLN_HFJHRQ映射（销售订单回复交货日期）
    /// 16. Remarks: 从ESB数据的F_BLN_BZ映射（备注）
    /// 17. FDDSTATUS: 从ESB数据的FDDSTATUS映射（订单状态，优先级高于其他状态字段）
    /// 
    /// 关联总任务字段映射说明：
    /// 1. JoinTaskBillNo: 直接从ESB数据的FZRWBILLNO映射（关联总任务单据号）
    /// 2. IsJoinTask: 基于FZRWBILLNO判断，有值为1，无值为0（是否关联总任务）
    /// 
    /// 数量相关字段映射说明：
    /// 1. SalesQty: 直接从ESB数据的FQTY映射（销售数量）
    /// 注：技术管理表中没有直接的采购数量字段，采购相关数量在采购订单表和委外订单表中维护
    /// 
    /// 性能优化：
    /// - 支持批量查询物料信息，避免N+1查询问题
    /// - 使用物料信息字典缓存，提高查询效率
    /// - 对缺失的物料信息进行日志记录和默认值处理
    /// </summary>
    public class TechManagementESBSyncService : ESBSyncServiceBase<OCP_TechManagement, ESBTechManagementData, IOCP_TechManagementRepository>
    {
        private readonly ESBLogger _esbLogger;

        public TechManagementESBSyncService(
            IOCP_TechManagementRepository repository,
            ESBBaseService esbService,
            ILogger<TechManagementESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _esbLogger = ESBLoggerFactory.CreateTechManagementLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供技术管理专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        /// <summary>
        /// 获取ESB接口配置名称
        /// </summary>
        protected override string GetESBApiConfigName()
        {
            return nameof(TechManagementESBSyncService);
        }

        /// <summary>
        /// 获取操作类型描述
        /// </summary>
        protected override string GetOperationType()
        {
            return "技术管理";
        }

        /// <summary>
        /// 验证ESB数据有效性
        /// </summary>
        protected override bool ValidateESBData(ESBTechManagementData esbData)
        {
            // 基本字段验证
            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("技术管理", "FENTRYID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            // 销售订单号验证
            if (string.IsNullOrWhiteSpace(esbData.FBILLNO))
            {
                ESBLogger.LogValidationError("技术管理", "销售订单号为空", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取实体主键
        /// </summary>
        protected override object GetEntityKey(ESBTechManagementData esbData)
        {
            return (long)esbData.FENTRYID;
        }

        /// <summary>
        /// 查询现有记录
        /// </summary>
        protected override async Task<List<OCP_TechManagement>> QueryExistingRecords(List<object> keys)
        {
            var entryIdList = keys.Cast<long>().ToList();
            return await Task.Run(() =>
                _repository.FindAsIQueryable(x => x.SOEntryID != null && entryIdList.Contains(x.SOEntryID.Value))
                .ToList());
        }

        /// <summary>
        /// 判断现有记录是否匹配ESB数据
        /// </summary>
        protected override bool IsEntityMatch(OCP_TechManagement entity, ESBTechManagementData esbData)
        {
            return entity.SOEntryID == esbData.FENTRYID;
        }

        /// <summary>
        /// 将ESB数据映射到实体
        /// </summary>
        protected override void MapESBDataToEntity(ESBTechManagementData esbData, OCP_TechManagement entity)
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
        protected override void MapESBDataToEntityWithCache(ESBTechManagementData esbData, OCP_TechManagement entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // 使用缓存优化的映射
            MapESBDataToEntityCore(esbData, entity, materialRecordsCache);
        }

        /// <summary>
        /// 重写提取物料ID方法，从技术管理数据中提取物料ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBTechManagementData> esbDataList)
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
        private void MapESBDataToEntityCore(ESBTechManagementData esbData, OCP_TechManagement entity, Dictionary<long, OCP_Material> materialDict = null)
        {
            // 基本信息映射
            entity.SOEntryID = esbData.FENTRYID;
            entity.SOBillNo = esbData.FBILLNO;
            entity.SalesContractNo = esbData.F_BLN_CONTACTNONAME;
            entity.PlanTraceNo = esbData.FMTONO;
            entity.MaterialID = esbData.FMATERIALID;

            // 物料信息映射（从物料表获取）
            var materialId = esbData.FMATERIALID.HasValue ? (long?)esbData.FMATERIALID.Value : null;
            var material = GetMaterialByIdWithDict(materialId, materialDict);
            
            if (material != null)
            {
                entity.MaterialNumber = material.MaterialCode ?? string.Empty;
                entity.MaterialName = material.MaterialName ?? string.Empty;
                entity.ProductModel = material.ProductModel ?? string.Empty;          // 产品型号
                entity.NominalDiameter = material.NominalDiameter ?? string.Empty;    // 公称通径
                entity.NominalPressure = material.NominalPressure ?? string.Empty;    // 公称压力
                entity.FlowCharacteristic = material.FlowCharacteristic ?? string.Empty; // 流量特性
                entity.PackingForm = material.PackingForm ?? string.Empty;            // 填料形式
                entity.FlangeConnection = material.FlangeConnection ?? string.Empty;  // 法兰连接方式
                entity.ActuatorModel = material.ActuatorModel ?? string.Empty;        // 执行机构型号
                entity.ActuatorStroke = material.ActuatorStroke ?? string.Empty;      // 执行机构行程
                entity.ErpClsid = material.ErpClsid ?? string.Empty;                  // 物料属性
            }
            else
            {
                // 物料信息不存在时的默认处理
                entity.MaterialNumber = string.Empty;
                entity.MaterialName = string.Empty;
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
                if (materialId.HasValue && materialId.Value > 0)
                {
                    ESBLogger.LogWarning($"技术管理ESB同步：未找到物料信息，物料ID={materialId.Value}，订单明细ID={esbData.FENTRYID}");
                }
            }
            
            // 订单状态映射 - 优先使用FDDSTATUS，如果为空则综合三个状态字段
            if (!string.IsNullOrWhiteSpace(esbData.FDDSTATUS))
            {
                entity.OrderStatus = esbData.FDDSTATUS;
            }
            else
            {
                // 备用方案：综合三个状态字段
                var statusParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(esbData.FMRPFREEZESTATUSNAME))
                    statusParts.Add($"冻结:{esbData.FMRPFREEZESTATUSNAME}");
                if (!string.IsNullOrWhiteSpace(esbData.FMRPTERMINATESTATUSNAME))
                    statusParts.Add($"终止:{esbData.FMRPTERMINATESTATUSNAME}");
                if (!string.IsNullOrWhiteSpace(esbData.FMRPCLOSESTATUSNAME))
                    statusParts.Add($"关闭:{esbData.FMRPCLOSESTATUSNAME}");
                
                entity.OrderStatus = statusParts.Any() ? string.Join(";", statusParts) : null;
            }

            // 数量信息
            entity.SalesQty = esbData.FQTY;

            // 任务时间信息
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;

            // 关联总任务字段映射
            entity.ProScheduleYearMonth=esbData.FZRWBILLNO;
            entity.JoinTaskBillNo = esbData.FZRWBILLNO;
            entity.IsJoinTask = string.IsNullOrWhiteSpace(esbData.FZRWBILLNO) ? 0 : 1;

            // BOM信息
            entity.BOMCreateDate = ParseDate(esbData.BOMCREATEDATE);
            entity.HasBOM = esbData.FISBOM == "是" ? 1 : 0;

            // 日期字段映射
            entity.RequiredCompletionDate = ParseDate(esbData.FPLANFINISHDATE);
            entity.RequiredFinishTime = ParseDate(esbData.F_BLN_HFJHRQ);
            entity.ReplyDeliveryDate = ParseDate(esbData.F_BLN_HFJHRQ);  // 销售订单回复交货日期
            entity.DeliveryDate = ParseDate(esbData.F_ORA_DATETIME);     // 客户要货日期
            entity.ESBModifyDate = ParseDate(esbData.FMODIFYDATE);       // ESB修改日期

            // 标准天数映射
            entity.StandardDays = esbData.FBZTS.HasValue ? (int?)Math.Round(esbData.FBZTS.Value) : null;

            // 备注字段映射
            entity.Remarks = esbData.F_BLN_BZ;
            entity.FPSYJ = esbData.FPSYJ;//评审意见

            // 系统字段
            var now = DateTime.Now;
            if (entity.TechID <= 0) // 新增
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
        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_TechManagement> toUpdate, List<OCP_TechManagement> toInsert)
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
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条技术管理记录");
                    }

                    // 使用AddRange批量插入
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条技术管理记录");
                    }

                    _repository.SaveChanges();
                    
                    var totalProcessed = toUpdate.Count + toInsert.Count;
                    ESBLogger.LogInfo($"技术管理批量操作成功完成，总计处理 {totalProcessed} 条记录");
                    
                    return webResponse.OK($"批量操作成功，更新 {toUpdate.Count} 条，新增 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, "执行技术管理批量操作失败");
                    return webResponse.Error($"批量操作失败：{ex.Message}");
                }
            }));
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 转换字符串为布尔值
        /// </summary>
        /// <param name="value">字符串值</param>
        /// <returns>布尔值</returns>
        private bool? ConvertToBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var normalizedValue = value.Trim().ToLower();
            
            // 支持多种格式的布尔值转换
            return normalizedValue switch
            {
                "是" => true,
                "否" => false,
                "有" => true,
                "无" => false,
                "true" => true,
                "false" => false,
                "1" => true,
                "0" => false,
                "yes" => true,
                "no" => false,
                _ => null
            };
        }

        #endregion
    }
} 