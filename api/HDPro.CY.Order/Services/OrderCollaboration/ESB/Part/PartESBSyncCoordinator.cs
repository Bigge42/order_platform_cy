using HDPro.Core.Utilities;
using HDPro.Entity.SystemModels;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.Part
{
    /// <summary>
    /// 部件业务ESB同步协调器
    /// 统一管理所有部件相关的ESB数据同步
    /// </summary>
    public class PartESBSyncCoordinator
    {
        private readonly PartUnFinishTrackESBSyncService _partUnFinishTrackService;
        private readonly PartPrdMOESBSyncService _partPrdMOSyncService;
        private readonly PartPrdMODetailESBSyncService _partPrdMODetailSyncService;
        private readonly ILogger<PartESBSyncCoordinator> _logger;

        public PartESBSyncCoordinator(
            PartUnFinishTrackESBSyncService partUnFinishTrackService,
            PartPrdMOESBSyncService partPrdMOSyncService,
            PartPrdMODetailESBSyncService partPrdMODetailSyncService,
            ILogger<PartESBSyncCoordinator> logger)
        {
            _partUnFinishTrackService = partUnFinishTrackService;
            _partPrdMOSyncService = partPrdMOSyncService;
            _partPrdMODetailSyncService = partPrdMODetailSyncService;
            _logger = logger;
        }

        /// <summary>
        /// 同步所有部件相关数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncAllPartData(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var operationStartTime = DateTime.Now;

            try
            {
                _logger.LogInformation("===== 开始部件业务全量ESB数据同步 =====");
                _logger.LogInformation($"同步时间范围：{startDate ?? "默认开始时间"} 到 {endDate ?? "默认结束时间"}");

                var totalSyncCount = 0;
                var syncResults = new System.Text.StringBuilder();

                // 同步部件生产订单头
                _logger.LogInformation("第1步：开始同步部件生产订单头数据...");
                var partPrdMOResult = await _partPrdMOSyncService.SyncDataFromESB(startDate, endDate);

                if (partPrdMOResult.Status)
                {
                    _logger.LogInformation($"第1步完成：部件生产订单头同步成功 - {partPrdMOResult.Message}");
                    syncResults.AppendLine($"✓ 部件生产订单头：{partPrdMOResult.Message}");
                    totalSyncCount++;
                }
                else
                {
                    _logger.LogError($"第1步失败：部件生产订单头同步失败 - {partPrdMOResult.Message}");
                    syncResults.AppendLine($"✗ 部件生产订单头：{partPrdMOResult.Message}");
                }

                // 同步部件生产订单明细
                _logger.LogInformation("第2步：开始同步部件生产订单明细数据...");
                var partPrdMODetailResult = await _partPrdMODetailSyncService.SyncDataFromESB(startDate, endDate);

                if (partPrdMODetailResult.Status)
                {
                    _logger.LogInformation($"第2步完成：部件生产订单明细同步成功 - {partPrdMODetailResult.Message}");
                    syncResults.AppendLine($"✓ 部件生产订单明细：{partPrdMODetailResult.Message}");
                    totalSyncCount++;
                }
                else
                {
                    _logger.LogError($"第2步失败：部件生产订单明细同步失败 - {partPrdMODetailResult.Message}");
                    syncResults.AppendLine($"✗ 部件生产订单明细：{partPrdMODetailResult.Message}");
                }

                // 同步部件未完跟踪
                _logger.LogInformation("第3步：开始同步部件未完跟踪数据...");
                var partUnFinishResult = await _partUnFinishTrackService.SyncDataFromESB(startDate, endDate);

                if (partUnFinishResult.Status)
                {
                    _logger.LogInformation($"第3步完成：部件未完跟踪同步成功 - {partUnFinishResult.Message}");
                    syncResults.AppendLine($"✓ 部件未完跟踪：{partUnFinishResult.Message}");
                    totalSyncCount++;
                }
                else
                {
                    _logger.LogError($"第3步失败：部件未完跟踪同步失败 - {partUnFinishResult.Message}");
                    syncResults.AppendLine($"✗ 部件未完跟踪：{partUnFinishResult.Message}");
                }

                //4. 表体数据汇总到表头
                var summaryService = await OCP_PartPrdMOService.Instance.SummaryDetails2Head();
                if (summaryService.Status)
                {
                    _logger.LogInformation($"第4步完成：部件生产订单明细汇总成功 - {summaryService.Message}");
                    syncResults.AppendLine($"✓ 部件生产订单明细汇总成功：{summaryService.Message}");
                }
                else
                {
                    _logger.LogError($"第4步失败：部件生产订单明细汇总失败 - {partUnFinishResult.Message}");
                    syncResults.AppendLine($"✗ 部件生产订单明细汇总失败：{partUnFinishResult.Message}");
                }

                var totalTime = (DateTime.Now - operationStartTime).TotalSeconds;
                var successMessage = $"部件业务ESB数据同步完成！总耗时：{totalTime:F2}秒，成功同步 {totalSyncCount}/3 个数据类型\n\n详细结果：\n{syncResults}";

                _logger.LogInformation("===== 部件业务全量ESB数据同步完成 =====");
                _logger.LogInformation(successMessage);

                if (totalSyncCount > 0)
                {
                    return response.OK(successMessage);
                }
                else
                {
                    return response.Error($"部件业务ESB数据同步失败，所有同步操作都未成功\n\n详细结果：\n{syncResults}");
                }
            }
            catch (Exception ex)
            {
                var totalTime = (DateTime.Now - operationStartTime).TotalSeconds;
                var errorMessage = $"部件业务ESB数据同步发生异常，总耗时：{totalTime:F2}秒，异常信息：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 仅同步部件生产订单数据（包括头和明细）
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncPartPrdMOOnly(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var operationStartTime = DateTime.Now;

            try
            {
                _logger.LogInformation("===== 开始部件生产订单单独同步操作 =====");

                var totalSyncCount = 0;
                var syncResults = new System.Text.StringBuilder();

                // 同步部件生产订单头
                _logger.LogInformation("第1步：开始同步部件生产订单头数据...");
                var partPrdMOResult = await _partPrdMOSyncService.SyncDataFromESB(startDate, endDate);

                if (partPrdMOResult.Status)
                {
                    _logger.LogInformation($"第1步完成：部件生产订单头同步成功 - {partPrdMOResult.Message}");
                    syncResults.AppendLine($"✓ 部件生产订单头：{partPrdMOResult.Message}");
                    totalSyncCount++;
                }
                else
                {
                    _logger.LogError($"第1步失败：部件生产订单头同步失败 - {partPrdMOResult.Message}");
                    syncResults.AppendLine($"✗ 部件生产订单头：{partPrdMOResult.Message}");
                }

                // 同步部件生产订单明细
                _logger.LogInformation("第2步：开始同步部件生产订单明细数据...");
                var partPrdMODetailResult = await _partPrdMODetailSyncService.SyncDataFromESB(startDate, endDate);

                if (partPrdMODetailResult.Status)
                {
                    _logger.LogInformation($"第2步完成：部件生产订单明细同步成功 - {partPrdMODetailResult.Message}");
                    syncResults.AppendLine($"✓ 部件生产订单明细：{partPrdMODetailResult.Message}");
                    totalSyncCount++;
                }
                else
                {
                    _logger.LogError($"第2步失败：部件生产订单明细同步失败 - {partPrdMODetailResult.Message}");
                    syncResults.AppendLine($"✗ 部件生产订单明细：{partPrdMODetailResult.Message}");
                }

                var totalTime = (DateTime.Now - operationStartTime).TotalSeconds;
                var successMessage = $"部件生产订单同步完成！总耗时：{totalTime:F2}秒，成功同步 {totalSyncCount}/2 个数据类型\n\n详细结果：\n{syncResults}";

                _logger.LogInformation("===== 部件生产订单单独同步完成 =====");
                _logger.LogInformation(successMessage);

                if (totalSyncCount > 0)
                {
                    return response.OK(successMessage);
                }
                else
                {
                    return response.Error($"部件生产订单同步失败，所有同步操作都未成功\n\n详细结果：\n{syncResults}");
                }
            }
            catch (Exception ex)
            {
                var totalTime = (DateTime.Now - operationStartTime).TotalSeconds;
                var errorMessage = $"部件生产订单同步发生异常，总耗时：{totalTime:F2}秒，异常信息：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 仅同步部件未完跟踪数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncPartUnFinishTrackOnly(string startDate = null, string endDate = null)
        {
            _logger.LogInformation("执行部件未完跟踪单独同步操作");
            return await _partUnFinishTrackService.SyncDataFromESB(startDate, endDate);
        }

        /// <summary>
        /// 获取同步状态信息
        /// </summary>
        /// <returns>状态信息</returns>
        public WebResponseContent GetSyncStatus()
        {
            var response = new WebResponseContent();

            var statusInfo = new
            {
                CoordinatorName = "部件业务ESB同步协调器",
                Description = "管理部件相关的ESB数据同步",
                SupportedOperations = new[]
                {
                    "SyncAllPartData - 同步所有部件数据（生产订单头、明细、未完跟踪）",
                    "SyncPartPrdMOOnly - 仅同步部件生产订单（头和明细）",
                    "SyncPartUnFinishTrackOnly - 仅同步部件未完跟踪"
                },
                SyncServices = new[]
                {
                    new { Name = "PartPrdMOESBSyncService", Description = "部件生产订单头ESB同步", ApiPath = "/SearchPrdMO" },
                    new { Name = "PartPrdMODetailESBSyncService", Description = "部件生产订单明细ESB同步", ApiPath = "/SearchPrdMOEntry" },
                    new { Name = "PartUnFinishTrackESBSyncService", Description = "部件未完跟踪ESB同步", ApiPath = "/SearchPartProgress" }
                },
                LastUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Version = "2.0.0"
            };

            return response.OK("部件业务ESB同步协调器状态正常", statusInfo);
        }
    }
}