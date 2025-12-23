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

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.SubOrder
{
    /// <summary>
    /// 委外未完跟踪ESB同步服务
    /// 对接ESB接口：/gateway/DataCenter/SearchPoorProgress
    ///
    /// 字段映射说明（根据2024年12月最新接口更新）：
    ///
    /// 基本信息映射：
    /// - FENTRYID → FENTRYID (采购订单明细主键)
    /// - FID → FID (采购订单主键)
    /// - FMTONO → MtoNo (计划跟踪号)
    /// - FBILLNO → POBillNo (采购单据号)
    /// - FMATERIALID → MaterialID (物料ID)
    /// - FSEQ → Seq (行号)
    /// - FCGSTATUS → BillStatus (订单状态)
    ///
    /// 委外订单信息映射：
    /// - FWWBILLNO → SubOrderNo (委外订单号)
    /// - FWWCREATEDATE → SubOrderCreateDate (委外订单创建时间)
    /// - FWWAPPROVEDATE → SubOrderAuditDate (委外订单审核时间)
    /// - F_BLN_LLRQ → PickDate (委外领料日期)
    ///
    /// 采购订单信息映射：
    /// - FCGDDCREATEDATE → POCreateDate (采购订单创建时间)
    /// - FCGDDAPPROVEDATE → POApproveDate (采购订单审核时间)
    /// - FDELIVERYDATE → DeliveryDate (要求交货日期)
    /// - F_BLN_SPDATE → LastReplyDeliveryDate (回复交货日期)
    ///
    /// 进度跟踪信息映射：
    /// - prt_dt → LastDeliveryDate (最近送货日期)
    /// - qty_send → TotalDeliveryQty (累计送货数量)
    /// - rcv_dt → LastArrivalDate (最近到货日期)
    /// - qty_rcvd → TotalArrivalQty (累计到货数量)
    /// - chk_dt → LastInspectionDate (最近质检日期)
    /// - qty_ok → TotalQualifiedQty (累计合格数量)
    /// - stock_dt → LatestPreprocessDate (最近前处理日期)
    /// - FREALQTY → InstockQty (累计入库数量)
    /// - FRKAPPROVEDATE → LastInstockDate (最近入库日期)
    /// - F_BLN_DELIVERYNO → DeliveryNo (送货单号)
    ///
    /// 数量字段映射：
    /// - FQTY → POQty (采购数量)
    /// - FREALQTY → InstockQty (入库数量)
    ///
    /// 计划信息映射：
    /// - FCUSTUNMONTH → PlanTaskMonth (催货单位任务时间月份)
    /// - FCUSTUNWEEK → PlanTaskWeek (催货单位任务时间周)
    /// - FCUSTUNEMER → Urgency (催货单位紧急等级)
    ///
    /// 物料信息映射：
    /// - MaterialID: 直接从ESB数据的FMATERIALID映射
    /// - MaterialNumber: 从物料表OCP_Material.MaterialCode获取
    /// - MaterialName: 从物料表OCP_Material.MaterialName获取
    /// - Specification: 从物料表OCP_Material.Specification获取（规格型号）
    /// - MaterialCategory: 根据物料编码前缀规则确定分类（阀体部件02开头、阀体零件08/04/6.开头等）
    /// - 支持物料字典缓存优化性能，未找到物料时记录警告日志
    ///
    /// 供应商信息映射：
    /// - SupplierCode: 从供应商表OCP_Supplier.SupplierNumber获取
    /// - SupplierName: 从供应商表OCP_Supplier.SupplierName获取
    /// - 支持供应商字典缓存优化性能，未找到供应商时设置为空字符串
    ///
    /// 备注信息映射：
    /// - F_BLN_BZ → Remark (备注字段)
    ///
    /// 日期字段映射：
    /// - 支持多种日期格式的解析和转换
    /// - 对无效日期进行异常处理和日志记录
    /// </summary>
    public class SubOrderUnFinishTrackESBSyncService : ESBSyncServiceBase<OCP_SubOrderUnFinishTrack, ESBSubOrderProgressData, IOCP_SubOrderUnFinishTrackRepository>
    {
        private readonly ESBLogger _esbLogger;

        public SubOrderUnFinishTrackESBSyncService(
            IOCP_SubOrderUnFinishTrackRepository repository,
            ESBBaseService esbBaseService,
            ILogger<SubOrderUnFinishTrackESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbBaseService, logger)
        {
            _esbLogger = ESBLoggerFactory.CreateSubOrderLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供委外订单专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 抽象方法实现

        /// <summary>
        /// 获取操作类型名称
        /// </summary>
        protected override string GetOperationType()
        {
            return "委外未完跟踪";
        }

        /// <summary>
        /// 验证ESB数据
        /// </summary>
        protected override bool ValidateESBData(ESBSubOrderProgressData esbData)
        {
            // 仅验证必填字段FENTRYID
            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("委外未完跟踪", "FENTRYID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取用于查询现有记录的键值
        /// </summary>
        protected override object GetEntityKey(ESBSubOrderProgressData esbData)
        {
            return esbData.FENTRYID;
        }

        /// <summary>
        /// 查询现有记录
        /// </summary>
        protected override async Task<List<OCP_SubOrderUnFinishTrack>> QueryExistingRecords(List<object> keys)
        {
            var entryIds = keys.Cast<int>().Select(x => (long)x).Distinct().ToList();
            return await Task.Run(() =>
                _repository.FindAsIQueryable(x => x.FENTRYID.HasValue && entryIds.Contains(x.FENTRYID.Value))
                .AsNoTracking()  // 🔧 关键修复：使用 AsNoTracking 避免实体跟踪冲突
                .ToList());
        }

        /// <summary>
        /// 判断现有记录是否匹配ESB数据
        /// </summary>
        protected override bool IsEntityMatch(OCP_SubOrderUnFinishTrack entity, ESBSubOrderProgressData esbData)
        {
            return entity.FENTRYID == esbData.FENTRYID;
        }

        /// <summary>
        /// 将ESB数据映射到实体
        /// </summary>
        protected override void MapESBDataToEntity(ESBSubOrderProgressData esbData, OCP_SubOrderUnFinishTrack entity)
        {
            MapESBDataToEntityCore(esbData, entity, null, null);
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
        protected override void MapESBDataToEntityWithCache(ESBSubOrderProgressData esbData, OCP_SubOrderUnFinishTrack entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // 使用缓存优化的映射
            MapESBDataToEntityCore(esbData, entity, materialRecordsCache, supplierRecordsCache);
        }

        /// <summary>
        /// 重写提取物料ID方法，从委外进度数据中提取物料ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBSubOrderProgressData> esbDataList)
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
        /// 重写提取供应商ID方法，从委外未完跟踪数据中提取供应商ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>供应商ID列表</returns>
        protected override List<int?> ExtractSupplierIds(List<ESBSubOrderProgressData> esbDataList)
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
        /// 根据2024年12月最新接口字段更新
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <param name="supplierDict">供应商信息字典（可选，用于性能优化）</param>
        private void MapESBDataToEntityCore(ESBSubOrderProgressData esbData, OCP_SubOrderUnFinishTrack entity, Dictionary<long, OCP_Material>? materialDict = null, Dictionary<long, OCP_Supplier>? supplierDict = null)
        {
            // 基本信息映射
            entity.MtoNo = esbData.FMTONO;
            entity.FID = esbData.FID;
            entity.FENTRYID = esbData.FENTRYID;
            entity.POBillNo = esbData.FBILLNO;
            entity.Seq = esbData.FSEQ;
            entity.BillStatus = esbData.FCGSTATUS;

            // 计划信息映射
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;

            // 物料信息映射
            entity.MaterialID = esbData.FMATERIALID;

            // 物料信息映射（从物料表获取）
            var materialId = esbData.FMATERIALID.HasValue ? (long?)esbData.FMATERIALID.Value : null;
            var material = GetMaterialByIdWithDict(materialId, materialDict);

            if (material != null)
            {
                entity.MaterialNumber = material.MaterialCode ?? string.Empty;
                entity.MaterialName = material.MaterialName ?? string.Empty;
                entity.Specification = material.SpecModel ?? string.Empty;        // 规格型号
                entity.MaterialCategory = GetMaterialCategoryByCode(material.MaterialCode, material.MaterialName);  // 根据编码规则确定物料分类
            }
            else
            {
                // 物料信息不存在时的默认处理
                entity.MaterialNumber = string.Empty;
                entity.MaterialName = string.Empty;
                entity.Specification = string.Empty;
                entity.MaterialCategory = string.Empty;

                // 记录警告日志
                if (materialId.HasValue && materialId.Value > 0)
                {
                    ESBLogger.LogWarning($"委外未完跟踪ESB同步：未找到物料信息，物料ID={materialId.Value}，订单明细ID={esbData.FENTRYID}");
                }
            }

            // 委外订单信息映射
            entity.SubOrderNo = esbData.FWWBILLNO;
            entity.SubOrderCreateDate = ParseDate(esbData.FWWCREATEDATE);
            entity.SubOrderAuditDate = ParseDate(esbData.FWWAPPROVEDATE);
            entity.PickDate = ParseDate(esbData.F_BLN_LLRQ);
            entity.SubOrderReleaseDate = ParseDate(esbData.FCONVEYDATE);

            // 采购订单信息映射
            entity.POCreateDate = ParseDate(esbData.FCGDDCREATEDATE);
            entity.POApproveDate = ParseDate(esbData.FCGDDAPPROVEDATE);
            entity.DeliveryDate = ParseDate(esbData.FDELIVERYDATE);
            entity.LastReplyDeliveryDate = ParseDate(esbData.F_BLN_SPDATE);

            // 进度跟踪信息映射（新增字段）
            entity.LastDeliveryDate = ParseDate(esbData.prt_dt);
            entity.TotalDeliveryQty = esbData.qty_send;
            entity.LastArrivalDate = ParseDate(esbData.rcv_dt);
            entity.TotalArrivalQty = esbData.qty_rcvd;
            entity.LastInspectionDate = ParseDate(esbData.chk_dt);
            entity.TotalQualifiedQty = esbData.qty_ok;
            entity.LatestPreprocessDate = ParseDate(esbData.stock_dt);
            entity.LastInstockDate = ParseDate(esbData.FRKAPPROVEDATE);
            entity.DeliveryNo = esbData.F_BLN_DELIVERYNO;

            // 供应商信息映射
            if (esbData.FSUPPLIERID.HasValue && supplierDict != null)
            {
                entity.SupplierCode = GetSupplierCodeById(esbData.FSUPPLIERID, supplierDict);
                entity.SupplierName = GetSupplierNameById(esbData.FSUPPLIERID, supplierDict);
            }
            else
            {
                entity.SupplierCode = string.Empty;
                entity.SupplierName = string.Empty;
            }

            // 数量信息映射
            entity.POQty = esbData.FQTY;        // 采购数量
            entity.InstockQty = esbData.FREALQTY;  // 累计入库数量

            // 备注信息映射
            entity.Remark = esbData.F_BLN_BZ ?? string.Empty;  // 备注字段映射
            entity.OrderRemark=esbData.F_BLN_BZ ?? string.Empty;
        }



        /// <summary>
        /// 执行批量数据库操作
        /// </summary>
        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_SubOrderUnFinishTrack> toUpdate, List<OCP_SubOrderUnFinishTrack> toInsert)
        {
            var response = new WebResponseContent();

            try
            {
                var totalOperations = toUpdate.Count + toInsert.Count;
                
                if (totalOperations == 0)
                {
                    return response.OK("没有需要处理的委外未完跟踪数据");
                }

                using (var transaction = await _repository.DbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 🔧 关键修复：在批量操作前清理 ChangeTracker，避免实体跟踪冲突
                        _repository.DbContext.ChangeTracker.Clear();

                        // 🔧 去重处理：确保 toUpdate 和 toInsert 中没有重复的 FENTRYID
                        var distinctToUpdate = toUpdate.GroupBy(x => x.FENTRYID).Select(g => g.First()).ToList();
                        var distinctToInsert = toInsert.GroupBy(x => x.FENTRYID).Select(g => g.First()).ToList();

                        if (distinctToUpdate.Count < toUpdate.Count)
                        {
                            ESBLogger.LogWarning($"检测到 {toUpdate.Count - distinctToUpdate.Count} 条重复的更新记录已被去重");
                        }
                        if (distinctToInsert.Count < toInsert.Count)
                        {
                            ESBLogger.LogWarning($"检测到 {toInsert.Count - distinctToInsert.Count} 条重复的插入记录已被去重");
                        }

                        // 批量更新
                        if (distinctToUpdate.Any())
                        {
                            _repository.DbContext.Set<OCP_SubOrderUnFinishTrack>().UpdateRange(distinctToUpdate);
                            ESBLogger.LogInfo($"准备更新 {distinctToUpdate.Count} 条委外未完跟踪记录");
                        }

                        // 批量插入
                        if (distinctToInsert.Any())
                        {
                            await _repository.DbContext.Set<OCP_SubOrderUnFinishTrack>().AddRangeAsync(distinctToInsert);
                            ESBLogger.LogInfo($"准备插入 {distinctToInsert.Count} 条委外未完跟踪记录");
                        }

                        var affectedRows = await _repository.DbContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        var message = $"委外未完跟踪数据批量操作成功：更新 {distinctToUpdate.Count} 条，插入 {distinctToInsert.Count} 条，影响行数 {affectedRows}";
                        ESBLogger.LogInfo(message);
                        return response.OK(message);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ESBLogger.LogError(ex, "委外未完跟踪数据批量操作失败，已回滚");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"委外未完跟踪数据批量操作发生异常：{ex.Message}";
                ESBLogger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        #endregion



        #region 实现抽象方法

        protected override string GetESBApiConfigName()
        {
            return nameof(SubOrderUnFinishTrackESBSyncService);
        }

        #endregion
    }
}