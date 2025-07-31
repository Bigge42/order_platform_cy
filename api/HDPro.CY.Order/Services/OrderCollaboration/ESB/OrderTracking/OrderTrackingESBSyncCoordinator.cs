/*
 * 订单跟踪ESB同步协调器
 * 负责协调和管理订单跟踪相关的ESB同步操作
 */
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.OrderTracking
{
    /// <summary>
    /// 订单跟踪ESB同步协调器
    /// </summary>
    public class OrderTrackingESBSyncCoordinator
    {
        private readonly OrderTrackingESBSyncService _orderTrackingESBSyncService;
        private readonly ILogger<OrderTrackingESBSyncCoordinator> _logger;
        private readonly ESBLogger _esbLogger;

        public OrderTrackingESBSyncCoordinator(
            OrderTrackingESBSyncService orderTrackingESBSyncService,
            ILogger<OrderTrackingESBSyncCoordinator> logger,
            ILoggerFactory loggerFactory)
        {
            _orderTrackingESBSyncService = orderTrackingESBSyncService;
            _logger = logger;
            _esbLogger = ESBLoggerFactory.CreateOrderTrackingLogger(loggerFactory);
        }

        /// <summary>
        /// 执行完整的订单跟踪ESB同步流程
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ExecuteFullSync(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var syncStartTime = DateTime.Now;

            try
            {
                // 使用专用日志记录同步开始
                _esbLogger.LogInfo("开始执行订单跟踪ESB完整同步流程...");
                _esbLogger.LogSyncStart("订单跟踪", "系统自动", startDate ?? "无限制", endDate ?? "无限制");

                // 执行订单跟踪数据同步
                var orderTrackingResult = await _orderTrackingESBSyncService.SyncDataFromESB(startDate, endDate);

                if (!orderTrackingResult.Status)
                {
                    var errorMsg = $"订单跟踪数据同步失败：{orderTrackingResult.Message}";
                    _esbLogger.LogError(errorMsg);
                    return response.Error(errorMsg);
                }

                var syncEndTime = DateTime.Now;
                var totalTime = (syncEndTime - syncStartTime).TotalSeconds;

                var successMessage = $"订单跟踪ESB完整同步流程执行完成！" +
                    $"订单跟踪同步：{orderTrackingResult.Message}，" +
                    $"总耗时：{totalTime:F2} 秒";

                _esbLogger.LogInfo(successMessage);
                return response.OK(successMessage);
            }
            catch (Exception ex)
            {
                var syncEndTime = DateTime.Now;
                var totalTime = (syncEndTime - syncStartTime).TotalSeconds;
                var errorMessage = $"订单跟踪ESB完整同步流程执行失败，耗时：{totalTime:F2} 秒";

                _esbLogger.LogError(ex, errorMessage);
                return response.Error($"{errorMessage}，错误：{ex.Message}");
            }
        }

        /// <summary>
        /// 手动触发订单跟踪ESB同步
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSync(string startDate, string endDate)
        {
            _esbLogger.LogInfo("手动触发订单跟踪ESB同步，时间范围：{StartDate} 到 {EndDate}", startDate, endDate);
            return await ExecuteFullSync(startDate, endDate);
        }

        /// <summary>
        /// 获取订单跟踪ESB同步状态信息
        /// </summary>
        /// <returns>状态信息</returns>
        public Task<WebResponseContent> GetSyncStatus()
        {
            try
            {
                var statusInfo = new
                {
                    CoordinatorName = "订单跟踪ESB同步协调器",
                    Services = new[]
                    {
                        new { Name = "订单跟踪同步服务", Type = "OrderTrackingESBSyncService", Status = "就绪" }
                    },
                    LastCheckTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                _esbLogger.LogDebug("获取订单跟踪ESB同步状态成功");
                return Task.FromResult(new WebResponseContent().OK("获取状态成功", statusInfo));
            }
            catch (Exception ex)
            {
                _esbLogger.LogError(ex, "获取订单跟踪ESB同步状态失败");
                return Task.FromResult(new WebResponseContent().Error($"获取状态失败：{ex.Message}"));
            }
        }
    }
}