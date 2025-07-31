/*
 * 销售订单明细ESB同步服务
 * 对接ESB接口：SearchERPSalOrderEntry
 * 负责同步销售订单明细数据到OCP_SOProgressDetail表
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

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.SalesManagement
{
    /// <summary>
    /// 销售订单明细ESB同步服务
    /// </summary>
    public class SalesOrderDetailESBSyncService : ESBSyncServiceBase<OCP_SOProgressDetail, ESBSalesOrderDetailData, IOCP_SOProgressDetailRepository>
    {
        private readonly ESBLogger _esbLogger;

        public SalesOrderDetailESBSyncService(
            IOCP_SOProgressDetailRepository repository,
            ESBBaseService esbService,
            ILogger<SalesOrderDetailESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _esbLogger = ESBLoggerFactory.CreateSalesManagementLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供销售管理专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        protected override string GetESBApiConfigName()
        {
            return nameof(SalesOrderDetailESBSyncService);
        }

        protected override string GetOperationType()
        {
            return "销售订单明细";
        }

        protected override bool ValidateESBData(ESBSalesOrderDetailData esbData)
        {
            if (esbData == null)
            {
                ESBLogger.LogValidationError("销售订单明细", "ESB数据为空", "");
                return false;
            }

            // 仅验证必填字段FENTRYID
            if (esbData.FENTRYID <= 0)
            {
                ESBLogger.LogValidationError("销售订单明细", "FENTRYID无效", $"FENTRYID={esbData.FENTRYID}");
                return false;
            }

            return true;
        }

        protected override object GetEntityKey(ESBSalesOrderDetailData esbData)
        {
            return esbData.FENTRYID;
        }

        protected override async Task<List<OCP_SOProgressDetail>> QueryExistingRecords(List<object> keys)
        {
            var repository = _repository as IRepository<OCP_SOProgressDetail>;
            var longKeys = keys.Cast<int>().Select(k => (long)k).ToList();
            
            return await Task.FromResult(
                repository.FindAsIQueryable(x => longKeys.Contains(x.SOEntryID.Value))
                    .ToList()
            );
        }

        protected override bool IsEntityMatch(OCP_SOProgressDetail entity, ESBSalesOrderDetailData esbData)
        {
            return entity.SOEntryID == esbData.FENTRYID;
        }

        protected override void MapESBDataToEntity(ESBSalesOrderDetailData esbData, OCP_SOProgressDetail entity)
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
        protected override void MapESBDataToEntityWithCache(ESBSalesOrderDetailData esbData, OCP_SOProgressDetail entity,
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // 使用缓存优化的映射
            MapESBDataToEntityCore(esbData, entity, materialRecordsCache);
        }

        /// <summary>
        /// 重写提取物料ID方法，从销售订单明细数据中提取物料ID
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected override List<long?> ExtractMaterialIds(List<ESBSalesOrderDetailData> esbDataList)
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
        /// <param name="orderIdDict">OrderID字典（可选，用于性能优化）</param>
        private void MapESBDataToEntityCore(ESBSalesOrderDetailData esbData, OCP_SOProgressDetail entity, Dictionary<long, OCP_Material> materialDict = null, Dictionary<string, long> orderIdDict = null)
        {
            // 设置主键
            entity.SOEntryID= esbData.FENTRYID;

            // 基本信息 - 修正字段映射
            entity.SOBillNo = esbData.FSALBILLNO?.Trim();
            entity.MtoNo = esbData.FMTONO?.Trim();
            entity.MOBillNo = esbData.FMOBILLNO?.Trim();
            entity.BusinessStatus = esbData.FWORKSTATE?.Trim();

            // 从主表获取OrderID - 优先使用缓存
            if (orderIdDict != null && orderIdDict.TryGetValue(entity.SOBillNo ?? "", out var cachedOrderId))
            {
                entity.OrderID = cachedOrderId;
                _logger.LogDebug($"使用缓存获取OrderID，订单号：{entity.SOBillNo}，OrderID：{cachedOrderId}");
            }
            else
            {
                entity.OrderID = GetOrderIdByBillNo(entity.SOBillNo);
            }

            // 数量信息 - 根据实体字段修正
            entity.Qty = esbData.FQTY;
            entity.InstockQty = esbData.FRKREALQTY;
            entity.UnInstockQty = esbData.FSQTY;
            entity.OutStockQty = esbData.FCKREALQTY;

            entity.PositionNo = esbData.F_BLN_WH;

            // 物料信息映射（从物料表获取）
            var materialId = esbData.FMATERIALID.HasValue ? (long?)esbData.FMATERIALID.Value : null;
            entity.MaterialNumber = GetMaterialCodeById(materialId, materialDict);
            entity.ProductionModel = GetMaterialProductModelById(materialId, materialDict);
            entity.Specification=GetMaterialSpecById(materialId, materialDict);
            entity.MaterialID = materialId;

            _logger.LogDebug($"映射销售订单明细数据完成，订单号：{entity.SOBillNo}，计划跟踪号：{entity.MtoNo}，OrderID：{entity.OrderID}");
        }

        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_SOProgressDetail> toUpdate, List<OCP_SOProgressDetail> toInsert)
        {
            if (!toUpdate.Any() && !toInsert.Any())
                return new WebResponseContent().OK("无数据需要处理");

            return await Task.Run(() => _repository.DbContextBeginTransaction(() =>
            {
                var webResponse = new WebResponseContent();
                try
                {
                    if (toUpdate.Any())
                    {
                        _repository.UpdateRange(toUpdate, false);
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条销售订单明细记录");
                    }
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条销售订单明细记录");
                    }
                    _repository.SaveChanges();
                    var totalProcessed = toUpdate.Count + toInsert.Count;
                    ESBLogger.LogInfo($"销售订单明细批量操作成功完成，总计处理 {totalProcessed} 条记录");
                    return webResponse.OK($"批量操作成功，更新 {toUpdate.Count} 条，新增 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, "执行销售订单明细批量操作失败");
                    return webResponse.Error($"批量操作失败：{ex.Message}");
                }
            }));
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 根据销售订单号从主表获取OrderID
        /// </summary>
        /// <param name="billNo">销售订单号</param>
        /// <returns>OrderID，如果未找到则返回0</returns>
        private long GetOrderIdByBillNo(string billNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(billNo))
                {
                    _logger.LogWarning("销售订单号为空，无法获取OrderID");
                    return 0;
                }

                var progressRepository = AutofacContainerModule.GetService<IOCP_SOProgressRepository>();
                var progressRecord = progressRepository.FindAsIQueryable(x => x.BillNo == billNo)
                                                      .FirstOrDefault();

                if (progressRecord != null)
                {
                    _logger.LogDebug($"找到销售订单 {billNo} 对应的OrderID：{progressRecord.OrderID}");
                    return progressRecord.OrderID;
                }
                else
                {
                    _logger.LogWarning($"未找到销售订单号 {billNo} 对应的主表记录，OrderID设置为0");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根据销售订单号 {billNo} 获取OrderID时发生异常");
                return 0;
            }
        }

        /// <summary>
        /// 批量获取OrderID字典，用于性能优化
        /// </summary>
        /// <param name="billNos">销售订单号列表</param>
        /// <returns>销售订单号到OrderID的映射字典</returns>
        private Dictionary<string, long> GetOrderIdDict(List<string> billNos)
        {
            try
            {
                if (billNos == null || !billNos.Any())
                {
                    return new Dictionary<string, long>();
                }

                var validBillNos = billNos.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                if (!validBillNos.Any())
                {
                    return new Dictionary<string, long>();
                }

                var progressRepository = AutofacContainerModule.GetService<IOCP_SOProgressRepository>();
                var progressRecords = progressRepository.FindAsIQueryable(x => validBillNos.Contains(x.BillNo))
                                                       .Select(x => new { x.BillNo, x.OrderID })
                                                       .ToList();

                // 处理重复的销售订单号，取第一个OrderID
                var groupedRecords = progressRecords.GroupBy(x => x.BillNo).ToList();
                var orderIdDict = groupedRecords.ToDictionary(g => g.Key, g => g.First().OrderID);

                // 记录重复的订单号
                var duplicateRecords = groupedRecords.Where(g => g.Count() > 1).ToList();
                if (duplicateRecords.Any())
                {
                    foreach (var duplicate in duplicateRecords)
                    {
                        _logger.LogWarning($"发现重复的销售订单号：{duplicate.Key}，共有 {duplicate.Count()} 条记录，已取第一个OrderID：{duplicate.First().OrderID}");
                    }
                }

                _logger.LogDebug($"批量获取OrderID完成，查询 {validBillNos.Count} 个订单号，找到 {orderIdDict.Count} 个匹配记录，其中 {duplicateRecords.Count} 个订单号有重复记录");
                
                return orderIdDict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量获取OrderID字典时发生异常");
                return new Dictionary<string, long>();
            }
        }

        #endregion

        #region 特殊方法 - 按销售订单号查询

        /// <summary>
        /// 根据销售订单号同步明细数据
        /// 注意：此接口需要销售订单号作为参数，不是时间范围
        /// </summary>
        /// <param name="salesOrderNo">销售订单号</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncByOrderNumber(string salesOrderNo)
        {
            var response = new WebResponseContent();
            var (syncStartTime, executorInfo, currentUserId, currentUserName) = _esbService.GetSyncOperationInfo();

            try
            {
                if (string.IsNullOrWhiteSpace(salesOrderNo))
                {
                    return response.Error("销售订单号不能为空");
                }

                _logger.LogInformation($"开始根据销售订单号同步明细数据，订单号：{salesOrderNo}，执行人：{executorInfo}");

                // 调用ESB接口
                var requestData = new ESBSalesOrderDetailRequest { FSALBILLNO = salesOrderNo };
                var esbData = await _esbService.CallESBApiWithRequestData<ESBSalesOrderDetailData>(
                    GetFinalESBApiPath(), 
                    requestData, 
                    $"销售订单明细-{salesOrderNo}"
                );

                if (esbData == null || !esbData.Any())
                {
                    var message = $"未获取到销售订单号 {salesOrderNo} 的明细数据";
                    _logger.LogWarning(message);
                    return response.OK(message);
                }

                _logger.LogInformation($"获取到 {esbData.Count} 条明细数据");

                // 验证和处理数据
                var validData = esbData.Where(ValidateESBData).ToList();
                if (!validData.Any())
                {
                    return response.Error("所有数据验证失败，无有效数据可处理");
                }

                // 查询现有记录
                var keys = validData.Select(GetEntityKey).ToList();
                var existingRecords = await QueryExistingRecords(keys);

                // 预先批量获取OrderID字典，提高性能
                var billNos = validData.Select(x => x.FSALBILLNO?.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                var orderIdDict = GetOrderIdDict(billNos);
                _logger.LogInformation($"预先获取 {orderIdDict.Count} 个订单的OrderID映射，用于批量处理");

                var toUpdate = new List<OCP_SOProgressDetail>();
                var toInsert = new List<OCP_SOProgressDetail>();
                var currentTime = DateTime.Now;

                foreach (var data in validData)
                {
                    var existing = existingRecords.FirstOrDefault(x => IsEntityMatch(x, data));
                    
                    if (existing != null)
                    {
                        // 更新现有记录 - 使用带缓存的映射方法
                        MapESBDataToEntityCore(data, existing, null, orderIdDict);
                        SetAuditFields(existing, currentTime, currentUserId, currentUserName, false);
                        toUpdate.Add(existing);
                    }
                    else
                    {
                        // 创建新记录 - 使用带缓存的映射方法
                        var newEntity = new OCP_SOProgressDetail();
                        MapESBDataToEntityCore(data, newEntity, null, orderIdDict);
                        SetAuditFields(newEntity, currentTime, currentUserId, currentUserName, true);
                        toInsert.Add(newEntity);
                    }
                }

                // 执行数据库操作
                return await ExecuteBatchOperations(toUpdate, toInsert);
            }
            catch (Exception ex)
            {
                var errorMsg = $"根据销售订单号同步明细数据时发生异常：{ex.Message}";
                _logger.LogError(ex, errorMsg);
                return response.Error(errorMsg);
            }
        }

        #endregion

        #region 公共接口方法

        /// <summary>
        /// 根据时间范围同步销售订单明细数据
        /// 注意：此方法调用的是按订单号查询的接口，需要先获取订单列表
        /// </summary>
        /// <param name="startDate">开始日期 (yyyy-MM-dd)</param>
        /// <param name="endDate">结束日期 (yyyy-MM-dd)</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncSalesOrderDetailData(string startDate = null, string endDate = null)
        {
            // 此接口需要特殊处理，因为ESB接口需要销售订单号作为参数
            // 通常需要先从销售订单列表中获取订单号，然后逐个查询明细
            return await Task.FromResult(new WebResponseContent().Error("此接口需要销售订单号，请使用SyncByOrderNumber方法"));
        }

        /// <summary>
        /// 手动同步销售订单明细数据
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncSalesOrderDetailData(string startDate, string endDate)
        {
            return await ManualSyncData(startDate, endDate);
        }

        #endregion

        #region 静态实例

        public static SalesOrderDetailESBSyncService Instance
        {
            get { return AutofacContainerModule.GetService<SalesOrderDetailESBSyncService>(); }
        }

        #endregion
    }
} 