using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using HDPro.Entity.SystemModels;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.ESB;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.Purchase
{
    /// <summary>
    /// 采购未完跟踪ESB同步服务
    /// 该服务专门处理采购订单未完成跟踪数据的ESB同步，支持多种匹配策略确保数据准确性
    /// 根据最新接口字段更新 - 2024年12月
    ///
    /// 数量字段映射说明：
    /// - POQty: 直接从ESB数据的FQTY映射（采购数量）
    /// - TotalDeliveryQty: 直接从ESB数据的qty_send映射（累计送货数量）
    /// - TotalArrivalQty: 直接从ESB数据的qty_rcvd映射（累计到货数量）
    /// - TotalQualifiedQty: 直接从ESB数据的qty_ok映射（累计合格数量）
    /// - InboundQty: 直接从ESB数据的FREALQTY映射（累计入库数量）
    ///
    /// 物料信息映射：
    /// - MaterialID: 直接从ESB数据的FMATERIALID映射
    /// - MaterialCode: 从物料表OCP_Material.MaterialCode获取
    /// - MaterialName: 从物料表OCP_Material.MaterialName获取
    /// - Specification: 从物料表OCP_Material.SpecModel获取（规格型号）
    /// - MaterialCategory: 根据物料编码前缀规则确定分类（阀体部件02开头、阀体零件08/04/6.开头等）
    ///
    /// 日期字段映射：
    /// - 使用基类的统一日期解析方法ParseDate
    /// - 支持多种日期格式的解析和转换
    /// - 对日期解析异常进行统一处理
    ///
    /// 供应商信息映射：
    /// - SupplierCode: 从供应商表OCP_Supplier.SupplierNumber获取
    /// - SupplierName: 从供应商表OCP_Supplier.SupplierName获取
    /// - 支持供应商字典缓存优化性能，未找到供应商时设置为空字符串
    ///
    /// 新增字段映射：
    /// - Seq: 直接从ESB数据的FSEQ映射（行号）
    /// - DeliveryNo: 直接从ESB数据的F_BLN_DELIVERYNO映射（送货单号）
    /// - BillStatus: 直接从ESB数据的FCGSTATUS映射（订单状态）
    /// - LatestPreprocessDate: 从ESB数据的stock_dt映射（最近前处理日期）
    ///
    /// 状态字段计算：
    /// - DeliveryStatus: 根据累计送货数量与采购数量比较计算（已完成/部分送货/未送货）
    /// - InspectionStatus: 根据累计合格数量与到货数量比较计算（已完成/部分质检/待质检/未到货）
    /// - PreprocessStatus: 根据前处理日期和合格数量计算（已处理/待处理/未开始）
    /// - InstockStatus: 根据入库数量与采购数量比较计算（已完成/部分入库/待入库/未开始）
    /// - WMSInboundQty: 与InboundQty保持一致（累计入库数量）
    /// </summary>
    public class PurchaseOrderUnFinishTrackESBSyncService : ESBSyncServiceBase<OCP_POUnFinishTrack, ESBPurchaseOrderProgressData, IOCP_POUnFinishTrackRepository>
    {
        private readonly ESBLogger _esbLogger;

        public PurchaseOrderUnFinishTrackESBSyncService(
            IOCP_POUnFinishTrackRepository repository,
            ESBBaseService esbBaseService,
            ILogger<PurchaseOrderUnFinishTrackESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbBaseService, logger)
        {
            _esbLogger = ESBLoggerFactory.CreatePurchaseOrderLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供采购订单专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        /// <summary>
        /// 获取ESB接口配置名称
        /// </summary>
        protected override string GetESBApiConfigName()
        {
            return nameof(PurchaseOrderUnFinishTrackESBSyncService);
        }

        /// <summary>
        /// 获取操作类型名称
        /// </summary>
        protected override string GetOperationType()
        {
            return "采购未完跟踪";
        }

        /// <summary>
        /// 验证ESB数据
        /// </summary>
        protected override bool ValidateESBData(ESBPurchaseOrderProgressData esbData)
        {
            // 基本字段验证
            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("采购未完跟踪", "FENTRYID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取用于查询现有记录的键值
        /// </summary>
        protected override object GetEntityKey(ESBPurchaseOrderProgressData esbData)
        {
            // 使用FENTRYID作为主键
            return esbData.FENTRYID;
        }

        /// <summary>
        /// 查询现有记录
        /// </summary>
        protected override async Task<List<OCP_POUnFinishTrack>> QueryExistingRecords(List<object> keys)
        {
            var entryIds = keys.Cast<int>().Select(x => (long)x).ToList();
            return await Task.Run(() =>
                _repository.FindAsIQueryable(x => x.FENTRYID.HasValue && entryIds.Contains(x.FENTRYID.Value))
                .ToList());
        }

        /// <summary>
        /// 判断现有记录是否匹配ESB数据
        /// </summary>
        protected override bool IsEntityMatch(OCP_POUnFinishTrack entity, ESBPurchaseOrderProgressData esbData)
        {
            return entity.FENTRYID == esbData.FENTRYID;
        }

        /// <summary>
        /// 将ESB数据映射到实体
        /// </summary>
        protected override void MapESBDataToEntity(ESBPurchaseOrderProgressData esbData, OCP_POUnFinishTrack entity)
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
        protected override void MapESBDataToEntityWithCache(ESBPurchaseOrderProgressData esbData, OCP_POUnFinishTrack entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // 使用缓存优化的映射
            MapESBDataToEntityCore(esbData, entity, materialRecordsCache, supplierRecordsCache);
        }

        /// <summary>
        /// 重写提取物料ID方法，从采购进度数据中提取物料ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBPurchaseOrderProgressData> esbDataList)
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
        /// 映射ESB数据到实体的核心方法
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <param name="supplierDict">供应商信息字典（可选，用于性能优化）</param>
        private void MapESBDataToEntityCore(ESBPurchaseOrderProgressData esbData, OCP_POUnFinishTrack entity, Dictionary<long, OCP_Material> materialDict = null, Dictionary<long, OCP_Supplier> supplierDict = null)
        {
            // 基本信息映射
            entity.FENTRYID = esbData.FENTRYID;
            entity.FID = esbData.FID;
            entity.PlanTraceNo = esbData.FMTONO;
            entity.POBillNo = esbData.FBILLNO;
            entity.Seq = esbData.FSEQ;                      // 行号
           
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
                entity.MaterialCode = material.MaterialCode ?? string.Empty;
                entity.MaterialName = material.MaterialName ?? string.Empty;
                entity.Specification = material.SpecModel ?? string.Empty;        // 规格型号
                entity.MaterialCategory = GetMaterialCategoryByCode(material.MaterialCode, material.MaterialName);  // 根据编码规则确定物料分类
            }
            else
            {
                // 物料信息不存在时的默认处理
                entity.MaterialCode = string.Empty;
                entity.MaterialName = string.Empty;
                entity.Specification = string.Empty;
                entity.MaterialCategory = string.Empty;

                // 记录警告日志
                if (materialId.HasValue && materialId.Value > 0)
                {
                    ESBLogger.LogWarning($"采购未完跟踪ESB同步：未找到物料信息，物料ID={materialId.Value}，订单明细ID={esbData.FENTRYID}");
                }
            }

            // 日期信息映射
            entity.CGSQCreateDate = ParseDate(esbData.FCGSQCREATEDATE);
            entity.CGSQAuditDate = ParseDate(esbData.FCGSQAPPROVEDATE);
            entity.POCreateDate = ParseDate(esbData.FCGDDCREATEDATE);
            entity.POAuditDate = ParseDate(esbData.FCGDDAPPROVEDATE);  // 修正：应该是POAuditDate
            entity.RequiredDeliveryDate = ParseDate(esbData.FDELIVERYDATE);

            // 跟踪信息映射
            entity.LatestReplyDeliveryDate = ParseDate(esbData.F_BLN_SPDATE);
            entity.LatestDeliveryDate = ParseDate(esbData.prt_dt);
            entity.LatestArrivalDate = ParseDate(esbData.rcv_dt);
            entity.LatestQualityCheckDate = ParseDate(esbData.chk_dt);
            entity.LatestPreprocessDate = ParseDate(esbData.stock_dt);  // 新增：最近前处理日期
            entity.LatestInboundDate = ParseDate(esbData.FRKAPPROVEDATE);

            // 数量信息映射
            entity.POQty = esbData.FQTY;                    // 采购数量
            entity.TotalDeliveryQty = esbData.qty_send;     // 新增：累计送货数量
            entity.TotalArrivalQty = esbData.qty_rcvd;      // 新增：累计到货数量
            entity.TotalQualifiedQty = esbData.qty_ok;      // 新增：累计合格数量
            entity.InboundQty = esbData.FREALQTY;           // 累计入库数量

            // 供应商信息映射
            if (esbData.FSUPPLIERID.HasValue)
            {
                entity.SupplierCode = GetSupplierCodeById(esbData.FSUPPLIERID, supplierDict);
                entity.SupplierName = GetSupplierNameById(esbData.FSUPPLIERID, supplierDict);
            }
            else
            {
                entity.SupplierCode = string.Empty;
                entity.SupplierName = string.Empty;
            }

            // 新增字段映射
            entity.DeliveryNo = esbData.F_BLN_DELIVERYNO;   // 送货单号
            entity.BillStatus = esbData.FCGSTATUS;          // 订单状态

            // 备注字段映射
            entity.Remark = esbData.F_BLN_BZ ?? string.Empty;     // 备注
            entity.OrderRemark = esbData.F_BLN_BZ ?? string.Empty;

            // 状态字段计算（根据业务逻辑设置状态）
            CalculateStatusFields(entity);
        }

        /// <summary>
        /// 计算状态字段
        /// </summary>
        /// <param name="entity">采购未完跟踪实体</param>
        private void CalculateStatusFields(OCP_POUnFinishTrack entity)
        {
            var poQty = entity.POQty ?? 0;
            var totalDeliveryQty = entity.TotalDeliveryQty ?? 0;
            var totalArrivalQty = entity.TotalArrivalQty ?? 0;
            var totalQualifiedQty = entity.TotalQualifiedQty ?? 0;
            var inboundQty = entity.InboundQty ?? 0;

            // 送货状态计算
            if (totalDeliveryQty >= poQty && poQty > 0)
            {
                entity.DeliveryStatus = "已完成";
            }
            else if (totalDeliveryQty > 0)
            {
                entity.DeliveryStatus = "部分送货";
            }
            else
            {
                entity.DeliveryStatus = "未送货";
            }

            // 质检状态计算
            if (totalQualifiedQty >= totalArrivalQty && totalArrivalQty > 0)
            {
                entity.InspectionStatus = "已完成";
            }
            else if (totalQualifiedQty > 0)
            {
                entity.InspectionStatus = "部分质检";
            }
            else if (totalArrivalQty > 0)
            {
                entity.InspectionStatus = "待质检";
            }
            else
            {
                entity.InspectionStatus = "未到货";
            }

            // 前处理状态计算（基于前处理日期）
            if (entity.LatestPreprocessDate.HasValue)
            {
                entity.PreprocessStatus = "已处理";
            }
            else if (totalQualifiedQty > 0)
            {
                entity.PreprocessStatus = "待处理";
            }
            else
            {
                entity.PreprocessStatus = "未开始";
            }

            // 入库状态计算
            if (inboundQty >= poQty && poQty > 0)
            {
                entity.InstockStatus = "已完成";
            }
            else if (inboundQty > 0)
            {
                entity.InstockStatus = "部分入库";
            }
            else if (totalQualifiedQty > 0)
            {
                entity.InstockStatus = "待入库";
            }
            else
            {
                entity.InstockStatus = "未开始";
            }

            // 设置WMSInboundQty字段（与InboundQty保持一致）
            entity.WMSInboundQty = entity.InboundQty;
        }

        /// <summary>
        /// 重写提取供应商ID方法，从采购未完跟踪数据中提取供应商ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>供应商ID列表</returns>
        protected override List<int?> ExtractSupplierIds(List<ESBPurchaseOrderProgressData> esbDataList)
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
        /// 执行批量数据库操作
        /// </summary>
        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_POUnFinishTrack> toUpdate, List<OCP_POUnFinishTrack> toInsert)
        {
            var response = new WebResponseContent();

            var totalOperations = toUpdate.Count + toInsert.Count;
            if (totalOperations == 0)
            {
                return response.OK("没有需要处理的采购未完跟踪数据");
            }

            // 使用Repository基类的事务管理方法，与其他ESB服务保持一致
            return await Task.Run(() => _repository.DbContextBeginTransaction(() =>
            {
                var webResponse = new WebResponseContent();

                try
                {
                    // 使用UpdateRange批量更新
                    if (toUpdate.Any())
                    {
                        _repository.UpdateRange(toUpdate, false); // 不立即保存
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条采购未完跟踪记录");
                    }

                    // 使用AddRange批量插入
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false); // 不立即保存
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条采购未完跟踪记录");
                    }

                    // 一次性保存所有更改
                    _repository.SaveChanges();

                    var message = $"采购未完跟踪数据批量操作成功：更新 {toUpdate.Count} 条，插入 {toInsert.Count} 条";
                    ESBLogger.LogInfo(message);
                    return webResponse.OK(message);
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, "采购未完跟踪数据批量操作失败");
                    return webResponse.Error($"采购未完跟踪数据批量操作失败：{ex.Message}");
                }
            }));
        }

        #endregion
    }
} 