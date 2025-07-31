using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.Metalwork
{
    /// <summary>
    /// 金工车间业务ESB同步协调器
    /// 负责协调金工生产订单、明细和未完工跟踪的同步操作
    /// </summary>
    public class MetalworkESBSyncCoordinator
    {
        private readonly MetalworkPrdMOESBSyncService _prdMOSyncService;
        private readonly MetalworkPrdMODetailESBSyncService _prdMODetailSyncService;
        private readonly MetalworkUnFinishTrackESBSyncService _unFinishTrackSyncService;
        private readonly ILogger<MetalworkESBSyncCoordinator> _logger;

        public MetalworkESBSyncCoordinator(
            MetalworkPrdMOESBSyncService prdMOSyncService,
            MetalworkPrdMODetailESBSyncService prdMODetailSyncService,
            MetalworkUnFinishTrackESBSyncService unFinishTrackSyncService,
            ILogger<MetalworkESBSyncCoordinator> logger)
        {
            _prdMOSyncService = prdMOSyncService;
            _prdMODetailSyncService = prdMODetailSyncService;
            _unFinishTrackSyncService = unFinishTrackSyncService;
            _logger = logger;
        }

        /// <summary>
        /// 同步所有金工车间业务数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncAllMetalworkData(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var syncResults = new List<(string Service, bool Success, string Message)>();

            try
            {
                _logger.LogInformation("=== 开始金工车间业务ESB数据同步 ===");
                _logger.LogInformation($"同步时间范围：{startDate ?? "默认开始时间"} 到 {endDate ?? "默认结束时间"}");

                // 1. 同步金工生产订单头
                _logger.LogInformation("1. 开始同步金工生产订单头数据...");
                var prdMOResult = await _prdMOSyncService.SyncDataFromESB(startDate, endDate);
                syncResults.Add(("金工生产订单头", prdMOResult.Status, prdMOResult.Message));
                _logger.LogInformation($"金工生产订单头同步完成：{prdMOResult.Message}");

                // 2. 同步金工生产订单明细
                _logger.LogInformation("2. 开始同步金工生产订单明细数据...");
                var prdMODetailResult = await _prdMODetailSyncService.SyncDataFromESB(startDate, endDate);
                syncResults.Add(("金工生产订单明细", prdMODetailResult.Status, prdMODetailResult.Message));
                _logger.LogInformation($"金工生产订单明细同步完成：{prdMODetailResult.Message}");

                // 3. 同步金工未完工跟踪
                _logger.LogInformation("3. 开始同步金工未完工跟踪数据...");
                var unFinishTrackResult = await _unFinishTrackSyncService.SyncDataFromESB(startDate, endDate);
                syncResults.Add(("金工未完工跟踪", unFinishTrackResult.Status, unFinishTrackResult.Message));
                _logger.LogInformation($"金工未完工跟踪同步完成：{unFinishTrackResult.Message}");

                // 汇总结果
                var successCount = 0;
                var totalCount = syncResults.Count;
                var resultMessages = new List<string>();

                foreach (var (service, success, message) in syncResults)
                {
                    if (success)
                    {
                        successCount++;
                        resultMessages.Add($"✓ {service}: {message}");
                    }
                    else
                    {
                        resultMessages.Add($"✗ {service}: {message}");
                    }
                }

                var summaryMessage = $"金工车间业务ESB同步完成，成功 {successCount}/{totalCount} 个服务";
                _logger.LogInformation($"=== {summaryMessage} ===");

                // 如果全部成功，返回成功结果
                if (successCount == totalCount)
                {
                    return response.OK(summaryMessage, new
                    {
                        Summary = summaryMessage,
                        Details = resultMessages,
                        SuccessCount = successCount,
                        TotalCount = totalCount
                    });
                }
                else
                {
                    return response.Error(summaryMessage);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"金工车间业务ESB同步过程中发生异常：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 手动同步金工生产订单数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncPrdMOData(string startDate, string endDate)
        {
            try
            {
                _logger.LogInformation($"手动同步金工生产订单数据，时间范围：{startDate} 到 {endDate}");
                
                var result = await _prdMOSyncService.ManualSyncData(startDate, endDate);
                
                _logger.LogInformation($"手动同步金工生产订单完成：{result.Message}");
                return result;
            }
            catch (Exception ex)
            {
                var errorMessage = $"手动同步金工生产订单失败：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return new WebResponseContent().Error(errorMessage);
            }
        }

        /// <summary>
        /// 手动同步金工生产订单明细数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncPrdMODetailData(string startDate, string endDate)
        {
            try
            {
                _logger.LogInformation($"手动同步金工生产订单明细数据，时间范围：{startDate} 到 {endDate}");
                
                var result = await _prdMODetailSyncService.ManualSyncData(startDate, endDate);
                
                _logger.LogInformation($"手动同步金工生产订单明细完成：{result.Message}");
                return result;
            }
            catch (Exception ex)
            {
                var errorMessage = $"手动同步金工生产订单明细失败：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return new WebResponseContent().Error(errorMessage);
            }
        }

        /// <summary>
        /// 手动同步金工未完工跟踪数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncUnFinishTrackData(string startDate, string endDate)
        {
            try
            {
                _logger.LogInformation($"手动同步金工未完工跟踪数据，时间范围：{startDate} 到 {endDate}");

                var result = await _unFinishTrackSyncService.ManualSyncData(startDate, endDate);

                _logger.LogInformation($"手动同步金工未完工跟踪完成：{result.Message}");
                return result;
            }
            catch (Exception ex)
            {
                var errorMessage = $"手动同步金工未完工跟踪失败：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return new WebResponseContent().Error(errorMessage);
            }
        }

        /// <summary>
        /// 仅同步金工未完工跟踪数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncMetalworkUnFinishTrackOnly(string startDate = null, string endDate = null)
        {
            try
            {
                _logger.LogInformation("执行金工未完工跟踪单独同步操作");
                return await _unFinishTrackSyncService.SyncDataFromESB(startDate, endDate);
            }
            catch (Exception ex)
            {
                var errorMessage = $"金工未完工跟踪单独同步失败：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return new WebResponseContent().Error(errorMessage);
            }
        }

        /// <summary>
        /// 获取金工车间业务同步状态概览
        /// </summary>
        /// <returns>状态信息</returns>
        public async Task<WebResponseContent> GetMetalworkSyncStatus()
        {
            try
            {
                var statusInfo = new
                {
                    LastSyncTime = DateTime.Now,
                    SupportedOperations = new[]
                    {
                        "金工生产订单头同步",
                        "金工生产订单明细同步", 
                        "金工未完工跟踪同步"
                    },
                    ESBEndpoints = new[]
                    {
                        "/SearchJGPrdMO",
                        "/SearchJGPrdMODetail",
                        "/SearchJGUnFinishTrack"
                    },
                    Description = "金工车间业务ESB数据同步服务，支持生产订单、明细和未完工跟踪数据的双向同步"
                };

                return new WebResponseContent().OK("获取金工车间同步状态成功", statusInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取金工车间同步状态失败");
                return new WebResponseContent().Error($"获取状态失败：{ex.Message}");
            }
        }
    }
} 