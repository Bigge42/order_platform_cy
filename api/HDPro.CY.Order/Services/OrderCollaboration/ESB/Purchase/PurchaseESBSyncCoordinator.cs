using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Entity.SystemModels;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.Purchase
{
    /// <summary>
    /// 采购ESB同步协调器，统一管理采购相关的ESB同步操作
    /// </summary>
    public class PurchaseESBSyncCoordinator
    {
        private readonly PurchaseOrderESBSyncService _purchaseOrderSync;
        private readonly PurchaseOrderDetailESBSyncService _purchaseOrderDetailSync;
        private readonly PurchaseOrderUnFinishTrackESBSyncService _purchaseUnFinishTrackSync;
        private readonly ILogger<PurchaseESBSyncCoordinator> _logger;
        private readonly ESBLogger _esbLogger;

        public PurchaseESBSyncCoordinator(
            PurchaseOrderESBSyncService purchaseOrderSync,
            PurchaseOrderDetailESBSyncService purchaseOrderDetailSync,
            PurchaseOrderUnFinishTrackESBSyncService purchaseUnFinishTrackSync,
            ILogger<PurchaseESBSyncCoordinator> logger,
            ILoggerFactory loggerFactory)
        {
            _purchaseOrderSync = purchaseOrderSync;
            _purchaseOrderDetailSync = purchaseOrderDetailSync;
            _purchaseUnFinishTrackSync = purchaseUnFinishTrackSync;
            _logger = logger;
            _esbLogger = ESBLoggerFactory.CreatePurchaseOrderLogger(loggerFactory);
        }

        /// <summary>
        /// 同步所有采购订单相关数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncAllPurchaseOrderData(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var results = new List<string>();
            var errors = new List<string>();

            try
            {
                _esbLogger.LogInfo("开始采购订单相关数据的完整同步");

                // 1. 同步采购订单头
                var purchaseOrderResult = await _purchaseOrderSync.SyncDataFromESB(startDate, endDate);
                if (purchaseOrderResult.Status)
                {
                    results.Add($"采购订单头：{purchaseOrderResult.Message}");
                }
                else
                {
                    errors.Add($"采购订单头同步失败：{purchaseOrderResult.Message}");
                }

                // 2. 同步采购订单明细
                var purchaseOrderDetailResult = await _purchaseOrderDetailSync.SyncDataFromESB(startDate, endDate);
                if (purchaseOrderDetailResult.Status)
                {
                    results.Add($"采购订单明细：{purchaseOrderDetailResult.Message}");
                }
                else
                {
                    errors.Add($"采购订单明细同步失败：{purchaseOrderDetailResult.Message}");
                }

                // 3. 同步采购未完跟踪
                var purchaseUnFinishTrackResult = await _purchaseUnFinishTrackSync.SyncDataFromESB(startDate, endDate);
                if (purchaseUnFinishTrackResult.Status)
                {
                    results.Add($"采购未完跟踪：{purchaseUnFinishTrackResult.Message}");
                }
                else
                {
                    errors.Add($"采购未完跟踪同步失败：{purchaseUnFinishTrackResult.Message}");
                }

                //4. 表体数据汇总到表头
                var summaryService = await OCP_PurchaseOrderService.Instance.SummaryDetails2Head();
                if (summaryService.Status)
                {
                    results.Add($"采购订单明细汇总：{summaryService.Message}");
                }
                else
                {
                    errors.Add($"采购订单明细汇总失败：{summaryService.Message}");
                }


                // 汇总结果
                if (errors.Count == 0)
                {
                    var successMessage = $"采购订单相关数据同步全部成功！详情：{string.Join("；", results)}";
                    _esbLogger.LogInfo(successMessage);
                    return response.OK(successMessage);
                }
                else if (results.Count > 0)
                {
                    var partialMessage = $"采购订单数据部分同步成功。成功：{string.Join("；", results)}。失败：{string.Join("；", errors)}";
                    _esbLogger.LogWarning(partialMessage);
                    return response.Error(partialMessage);
                }
                else
                {
                    var failMessage = $"采购订单数据同步全部失败。失败：{string.Join("；", errors)}";
                    _esbLogger.LogError(failMessage);
                    return response.Error(failMessage);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"采购订单数据同步过程中发生异常：{ex.Message}";
                _esbLogger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 手动触发采购订单数据同步
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncPurchaseOrderData(string startDate, string endDate)
        {
            _logger.LogInformation($"手动触发采购订单数据同步，操作用户：{HDPro.Core.ManageUser.UserContext.Current?.UserName ?? "未知用户"}，时间范围：{startDate} 到 {endDate}");
            return await SyncAllPurchaseOrderData(startDate, endDate);
        }

        /// <summary>
        /// 仅同步采购订单头
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncPurchaseOrderOnly(string startDate = null, string endDate = null)
        {
            return await _purchaseOrderSync.SyncDataFromESB(startDate, endDate);
        }

        /// <summary>
        /// 仅同步采购订单明细
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncPurchaseOrderDetailOnly(string startDate = null, string endDate = null)
        {
            return await _purchaseOrderDetailSync.SyncDataFromESB(startDate, endDate);
        }

        /// <summary>
        /// 仅同步采购未完跟踪
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncPurchaseUnFinishTrackOnly(string startDate = null, string endDate = null)
        {
            return await _purchaseUnFinishTrackSync.SyncDataFromESB(startDate, endDate);
        }

        /// <summary>
        /// 获取采购数据同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        public async Task<WebResponseContent> GetSyncStatus()
        {
            var response = new WebResponseContent();

            try
            {
                var statusInfo = new
                {
                    LastSyncTime = DateTime.Now, // 应该从数据库或缓存获取实际的最后同步时间
                    AvailableOperations = new[]
                    {
                        "SyncAllPurchaseOrderData - 同步所有采购订单数据",
                        "SyncPurchaseOrderOnly - 仅同步采购订单头",
                        "SyncPurchaseOrderDetailOnly - 仅同步采购订单明细",
                        "SyncPurchaseUnFinishTrackOnly - 仅同步采购未完跟踪"
                    }
                };

                return response.OK("采购ESB同步状态获取成功", statusInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取采购ESB同步状态失败");
                return response.Error($"获取同步状态失败：{ex.Message}");
            }
        }
    }
}