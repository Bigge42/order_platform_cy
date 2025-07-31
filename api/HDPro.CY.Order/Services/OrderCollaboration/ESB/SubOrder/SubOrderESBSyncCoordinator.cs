using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Entity.SystemModels;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.SubOrder
{
    /// <summary>
    /// 委外ESB同步协调器，统一管理委外相关的ESB同步操作
    /// </summary>
    public class SubOrderESBSyncCoordinator
    {
        private readonly SubOrderESBSyncService _subOrderSync;
        private readonly SubOrderDetailESBSyncService _subOrderDetailSync;
        private readonly SubOrderUnFinishTrackESBSyncService _subOrderUnFinishTrackSync;
        private readonly ILogger<SubOrderESBSyncCoordinator> _logger;

        // TODO: 后续添加委外未完跟踪服务
        // private readonly SubOrderUnFinishTrackESBSyncService _subOrderUnFinishTrackSync;

        public SubOrderESBSyncCoordinator(
            SubOrderESBSyncService subOrderSync,
            SubOrderDetailESBSyncService subOrderDetailSync,
            SubOrderUnFinishTrackESBSyncService subOrderUnFinishTrackSync,
            ILogger<SubOrderESBSyncCoordinator> logger)
        {
            _subOrderSync = subOrderSync;
            _subOrderDetailSync = subOrderDetailSync;
            _subOrderUnFinishTrackSync = subOrderUnFinishTrackSync;
            _logger = logger;
        }

        /// <summary>
        /// 同步所有委外订单相关数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncAllSubOrderData(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var results = new List<string>();
            var errors = new List<string>();

            try
            {
                _logger.LogInformation("开始委外订单相关数据的完整同步");

                // 1. 同步委外订单头
                var subOrderResult = await _subOrderSync.SyncDataFromESB(startDate, endDate);
                if (subOrderResult.Status)
                {
                    results.Add($"委外订单头：{subOrderResult.Message}");
                }
                else
                {
                    errors.Add($"委外订单头同步失败：{subOrderResult.Message}");
                }

                // 2. 同步委外订单明细
                var subOrderDetailResult = await _subOrderDetailSync.SyncDataFromESB(startDate, endDate);
                if (subOrderDetailResult.Status)
                {
                    results.Add($"委外订单明细：{subOrderDetailResult.Message}");
                }
                else
                {
                    errors.Add($"委外订单明细同步失败：{subOrderDetailResult.Message}");
                }

                // 3. 同步委外未完跟踪
                var subOrderUnFinishTrackResult = await _subOrderUnFinishTrackSync.SyncDataFromESB(startDate, endDate);
                if (subOrderUnFinishTrackResult.Status)
                {
                    results.Add($"委外未完跟踪：{subOrderUnFinishTrackResult.Message}");
                }
                else
                {
                    errors.Add($"委外未完跟踪同步失败：{subOrderUnFinishTrackResult.Message}");
                }

                // TODO: 3. 同步委外未完跟踪
                // var subOrderUnFinishTrackResult = await _subOrderUnFinishTrackSync.SyncDataFromESB(startDate, endDate);
                // if (subOrderUnFinishTrackResult.Status)
                // {
                //     results.Add($"委外未完跟踪：{subOrderUnFinishTrackResult.Message}");
                // }
                // else
                // {
                //     errors.Add($"委外未完跟踪同步失败：{subOrderUnFinishTrackResult.Message}");
                // }

                //4. 表体数据汇总到表头
                var summaryService = await OCP_SubOrderService.Instance.SummaryDetails2Head();
                if (summaryService.Status)
                {
                    results.Add($"委外订单明细汇总：{summaryService.Message}");
                }
                else
                {
                    errors.Add($"委外订单明细汇总失败：{summaryService.Message}");
                }

                // 汇总结果
                if (errors.Count == 0)
                {
                    var successMessage = $"委外订单相关数据同步全部成功！详情：{string.Join("；", results)}";
                    _logger.LogInformation(successMessage);
                    return response.OK(successMessage);
                }
                else if (results.Count > 0)
                {
                    var partialMessage = $"委外订单数据部分同步成功。成功：{string.Join("；", results)}。失败：{string.Join("；", errors)}";
                    _logger.LogWarning(partialMessage);
                    return response.Error(partialMessage);
                }
                else
                {
                    var failMessage = $"委外订单数据同步全部失败。失败：{string.Join("；", errors)}";
                    _logger.LogError(failMessage);
                    return response.Error(failMessage);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"委外订单数据同步过程中发生异常：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 手动触发委外订单数据同步
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncSubOrderData(string startDate, string endDate)
        {
            _logger.LogInformation($"手动触发委外订单数据同步，操作用户：{HDPro.Core.ManageUser.UserContext.Current?.UserName ?? "未知用户"}，时间范围：{startDate} 到 {endDate}");
            return await SyncAllSubOrderData(startDate, endDate);
        }

        /// <summary>
        /// 仅同步委外订单头
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncSubOrderOnly(string startDate = null, string endDate = null)
        {
            return await _subOrderSync.SyncDataFromESB(startDate, endDate);
        }

        /// <summary>
        /// 仅同步委外订单明细
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncSubOrderDetailOnly(string startDate = null, string endDate = null)
        {
            return await _subOrderDetailSync.SyncDataFromESB(startDate, endDate);
        }

        /// <summary>
        /// 仅同步委外未完跟踪
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncSubOrderUnFinishTrackOnly(string startDate = null, string endDate = null)
        {
            return await _subOrderUnFinishTrackSync.SyncDataFromESB(startDate, endDate);
        }

        /// <summary>
        /// 获取委外数据同步状态
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
                        "SyncAllSubOrderData - 同步所有委外订单数据",
                        "SyncSubOrderOnly - 仅同步委外订单头",
                        "SyncSubOrderDetailOnly - 仅同步委外订单明细",
                        "SyncSubOrderUnFinishTrackOnly - 仅同步委外未完跟踪"
                    },
                    SupportedDataTypes = new[]
                    {
                        new { Type = "SubOrder", Name = "委外订单头", Status = "已实现", ApiPath = "/SearchReqorder" },
                        new { Type = "SubOrderDetail", Name = "委外订单明细", Status = "已实现", ApiPath = "/SearchPoorEntry" },
                        new { Type = "SubOrderUnFinishTrack", Name = "委外未完跟踪", Status = "已实现", ApiPath = "/SearchPoorProgress" }
                    }
                };

                return response.OK("委外ESB同步状态获取成功", statusInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取委外ESB同步状态失败");
                return response.Error($"获取同步状态失败：{ex.Message}");
            }
        }
    }
} 