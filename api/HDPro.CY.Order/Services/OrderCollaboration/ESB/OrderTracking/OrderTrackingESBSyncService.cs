/*
 * 订单跟踪ESB同步服务
 * 对接ESB接口：SearchSalOder
 * 负责同步销售订单数据到OCP_OrderTracking表
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.ESB;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.Utilities;
using HDPro.Entity.SystemModels;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.BaseProvider;
using HDPro.Core.ManageUser;
using Microsoft.EntityFrameworkCore;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.OrderTracking
{
    /// <summary>
    /// 订单跟踪ESB同步服务
    /// </summary>
    public class OrderTrackingESBSyncService : ESBSyncServiceBase<OCP_OrderTracking, ESBOrderData, IOCP_OrderTrackingRepository>
    {
        private readonly ESBLogger _esbLogger;

        public OrderTrackingESBSyncService(
            IOCP_OrderTrackingRepository repository,
            ESBBaseService esbService,
            ILogger<OrderTrackingESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _esbLogger = ESBLoggerFactory.CreateOrderTrackingLogger(loggerFactory);
            // 初始化ESB服务的日志记录器，确保ESBBaseService也使用订单跟踪的专用日志
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供订单跟踪专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        protected override string GetESBApiConfigName()
        {
            return nameof(OrderTrackingESBSyncService);
        }

        protected override string GetOperationType()
        {
            return "订单跟踪";
        }

        protected override bool ValidateESBData(ESBOrderData esbData)
        {
            if (esbData == null)
            {
                ESBLogger.LogValidationError("订单跟踪", "ESB数据为空");
                return false;
            }

            // 基本字段验证 - SOEntryID是唯一标识
            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("订单跟踪", "明细ID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(esbData.FBILLNO))
            {
                ESBLogger.LogValidationError("订单跟踪", "订单号为空", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            return true;
        }

        protected override object GetEntityKey(ESBOrderData esbData)
        {
            return esbData.FENTRYID;
        }

        protected override async Task<List<OCP_OrderTracking>> QueryExistingRecords(List<object> keys)
        {
            var repository = _repository as IRepository<OCP_OrderTracking>;
            var longKeys = keys.Cast<long>().Distinct().ToList();

            return await Task.FromResult(
                repository.FindAsIQueryable(x => longKeys.Contains(x.SOEntryID.Value))
                    .AsNoTracking()  // 🔧 关键修复：使用 AsNoTracking 避免实体跟踪冲突
                    .ToList()
            );
        }

        protected override bool IsEntityMatch(OCP_OrderTracking entity, ESBOrderData esbData)
        {
            return entity.SOEntryID == esbData.FENTRYID;
        }

        /// <summary>
        /// 重写提取物料ID方法，从订单数据中提取物料ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBOrderData> esbDataList)
        {
            if (esbDataList == null || !esbDataList.Any())
                return new List<long?>();

            return esbDataList
                .Where(x => x.FMATERIALID.HasValue && x.FMATERIALID.Value > 0)
                .Select(x => x.FMATERIALID)
                .Distinct()
                .ToList();
        }

        protected override void MapESBDataToEntity(ESBOrderData esbData, OCP_OrderTracking entity)
        {
            // 使用缓存优化的映射
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
        protected override void MapESBDataToEntityWithCache(ESBOrderData esbData, OCP_OrderTracking entity,
            Dictionary<long, object>? masterRecordsCache = null,
            Dictionary<long, OCP_Material>? materialRecordsCache = null,
            Dictionary<long, OCP_Supplier>? supplierRecordsCache = null,
            Dictionary<long, OCP_Customer>? customerRecordsCache = null)
        {
            // 使用缓存优化的映射
            MapESBDataToEntityCore(esbData, entity, materialRecordsCache);
        }

        /// <summary>
        /// 映射ESB数据到实体的核心方法
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        private void MapESBDataToEntityCore(ESBOrderData esbData, OCP_OrderTracking entity, Dictionary<long, OCP_Material>? materialDict = null)
        {
            // 基本信息映射
            entity.SOBillID = esbData.FID;
            entity.SOEntryID = esbData.FENTRYID ?? 0;
            entity.SOBillNo = esbData.FBILLNO;
            entity.ContractNo = esbData.F_BLN_CONTACTNONAME;
            entity.MtoNo = esbData.FMTONO;
          

            // 物料信息映射
            entity.MaterialID = esbData.FMATERIALID;

            // 物料信息映射（从物料表获取）
            var materialId = esbData.FMATERIALID.HasValue ? (long?)esbData.FMATERIALID.Value : null;
            var material = GetMaterialByIdWithDict(materialId, materialDict);

            if (material != null)
            {
                entity.MaterialNumber = material.MaterialCode ?? string.Empty;
                entity.MaterialName = material.MaterialName ?? string.Empty;
                entity.ProductionModel = material.ProductModel ?? string.Empty;  // 产品型号从物料表获取
                entity.TopSpecification = material.SpecModel;
            }
            else
            {
                // 物料信息不存在时，使用ESB数据中的基本信息作为备用
                entity.MaterialNumber = esbData.FNUMBER ?? string.Empty;
                entity.MaterialName = esbData.FNAME ?? string.Empty;
                entity.ProductionModel = string.Empty;
                entity.TopSpecification = string.Empty;

                // 记录警告日志
                if (esbData.FMATERIALID.HasValue && esbData.FMATERIALID.Value > 0)
                {
                    ESBLogger.LogWarning($"物料ID {esbData.FMATERIALID.Value} 在物料表中未找到，订单明细ID：{esbData.FENTRYID}");
                }
            }
            
            // 新的催货单位字段映射
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;
            
            entity.CustName = esbData.FCUSTName;
            entity.UseUnit = esbData.F_BLN_USEUNIT1NAME;
            entity.ContractType = esbData.F_ORA_ASSISTANTNAME;
            entity.SalesPerson = esbData.XSYNAME;
            entity.ProjectName = esbData.F_BLN_PROJECTNAME;
            entity.MRPTERMINATESTATUS = esbData.FMRPTERMINATESTATUS;
            entity.MRPFREEZESTATUS = esbData.FMRPFREEZESTATUS;
            entity.MtoNoStatus = esbData.FMTONOSTATUS;
            entity.CancelStatus = esbData.FCANCELSTATUS;
            entity.JoinTaskBillNo = esbData.FZRWBILLNO;
            entity.IsJoinTask = string.IsNullOrWhiteSpace(entity.JoinTaskBillNo) ? 0 : 1;

            // 排产月份映射（关联总任务单据编号）
            entity.ProScheduleYearMonth = esbData.FZRWBILLNO;

            // 金额和数量
            if (decimal.TryParse(esbData.FAMOUNT?.ToString(), out decimal amount))
                entity.Amount = amount;
            
            if (decimal.TryParse(esbData.FQTY?.ToString(), out decimal orderQty))
                entity.OrderQty = orderQty;
            
            if (decimal.TryParse(esbData.FINSTOCKFQTY?.ToString(), out decimal instockQty))
                entity.InstockQty = instockQty;

            // 新增字段映射
            if (decimal.TryParse(esbData.XSCKQTY?.ToString(), out decimal outQty))
                entity.OutStockQty = outQty;

            if (decimal.TryParse(esbData.FWWQTY?.ToString(), out decimal unfinishQty))
                entity.UnInstockQty = unfinishQty;
            else
                // 计算未完数量（如果没有直接提供）
                entity.UnInstockQty = (entity.OrderQty ?? 0) - (entity.InstockQty ?? 0);

            // 状态字段映射 - 映射到实际存在的字段（避免重复映射）
            entity.BillStatus = esbData.FDDSTATUS;

            // 日期字段映射 - 映射到实际存在的字段
            entity.DeliveryDate = ParseDate(esbData.F_ORA_DATETIME);
            entity.ReplyDeliveryDate = ParseDate(esbData.F_BLN_HFJHRQ);
            entity.BidDate = ParseDate(esbData.open_bid_date);
            entity.BomCreateDate = ParseDate(esbData.BOMCREATEDATE);
            entity.OrderAuditDate = ParseDate(esbData.XSDDAPPROVEDATE);
            entity.PrdScheduleDate = ParseDate(esbData.F_ORA_DATE1);
            entity.PlanConfirmDate = ParseDate(esbData.FPLANCONFIRMDATE);
            entity.PlanStartDate = ParseDate(esbData.FPLANSTARTDATE);
            entity.StartDate = ParseDate(esbData.FSTARTDATE);
            entity.OrderCreateDate = ParseDate(esbData.FCREATEDATE);
            entity.ModifyDate = ParseDate(esbData.FMODIFYDATE);
            entity.LastInStockDate = ParseDate(esbData.FRKDATE);
            entity.LastOutStockDate = ParseDate(esbData.XSCKFDATE);
            entity.ComputedDate = ParseDate(esbData.FDeliveryDate);

            // 设置完成状态
            if (entity.InstockQty >= entity.OrderQty && entity.OrderQty > 0)
            {
                entity.FinishStatus = "已完成";
            }
            else if (entity.InstockQty > 0)
            {
                entity.FinishStatus = "部分完成";
            }
            else
            {
                entity.FinishStatus = "未开始";
            }
        }

        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_OrderTracking> toUpdate, List<OCP_OrderTracking> toInsert)
        {
            if (!toUpdate.Any() && !toInsert.Any())
                return new WebResponseContent().OK("无数据需要处理");

            var response = await Task.Run(() => _repository.DbContextBeginTransaction(() =>
            {
                var webResponse = new WebResponseContent();

                try
                {
                    // 🔧 关键修复：在批量操作前清理 ChangeTracker，避免实体跟踪冲突
                    _repository.DbContext.ChangeTracker.Clear();

                    // 🔧 去重处理：确保 toUpdate 和 toInsert 中没有重复的 SOEntryID
                    var distinctToUpdate = toUpdate.GroupBy(x => x.SOEntryID).Select(g => g.First()).ToList();
                    var distinctToInsert = toInsert.GroupBy(x => x.SOEntryID).Select(g => g.First()).ToList();

                    if (distinctToUpdate.Count < toUpdate.Count)
                    {
                        ESBLogger.LogWarning("检测到 {DuplicateCount} 条重复的更新记录已被去重", toUpdate.Count - distinctToUpdate.Count);
                    }
                    if (distinctToInsert.Count < toInsert.Count)
                    {
                        ESBLogger.LogWarning("检测到 {DuplicateCount} 条重复的插入记录已被去重", toInsert.Count - distinctToInsert.Count);
                    }

                    // 使用UpdateRange批量更新
                    if (distinctToUpdate.Any())
                    {
                        _repository.UpdateRange(distinctToUpdate, false); // 不立即保存
                        ESBLogger.LogInfo("准备批量更新 {UpdateCount} 条记录", distinctToUpdate.Count);
                    }

                    // 使用AddRange批量插入
                    if (distinctToInsert.Any())
                    {
                        _repository.AddRange(distinctToInsert, false); // 不立即保存
                        ESBLogger.LogInfo("准备批量插入 {InsertCount} 条记录", distinctToInsert.Count);
                    }

                    // 一次性保存所有更改
                    _repository.SaveChanges();

                    return webResponse.OK($"批量操作完成，更新 {distinctToUpdate.Count} 条，插入 {distinctToInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogProcessingError("订单跟踪", "批量数据库操作失败", string.Empty, ex);
                    return webResponse.Error($"批量操作失败：{ex.Message}");
                }
            }));

            return response;
        }

        #endregion

        #region 公共接口方法

        /// <summary>
        /// 手动触发同步（用于测试或手动执行）
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncOrderTrackingData(string startDate, string endDate)
        {
            var userName = UserContext.Current?.UserName ?? "未知用户";
            ESBLogger.LogSyncStart("订单跟踪", userName, startDate, endDate);
            return await ManualSyncData(startDate, endDate);
        }

        #endregion
    }
}