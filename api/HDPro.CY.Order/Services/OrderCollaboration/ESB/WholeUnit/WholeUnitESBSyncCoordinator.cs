using HDPro.Core.Utilities;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.WholeUnit;
using HDPro.Entity.DomainModels;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.WholeUnit
{
    /// <summary>
    /// 整机业务ESB同步协调器
    /// 负责统一管理整机相关的所有ESB同步服务，包括：
    /// 1. 整机生产订单头同步
    /// 2. 整机生产订单明细同步 
    /// 3. 整机跟踪同步
    /// 版本：1.0.0
    /// </summary>
    public class WholeUnitESBSyncCoordinator
    {
        private readonly WholeUnitPrdMOESBSyncService _prdMOSyncService;
        private readonly WholeUnitPrdMODetailESBSyncService _prdMODetailSyncService;
        private readonly WholeUnitTrackingESBSyncService _trackingSyncService;
        private readonly ILogger<WholeUnitESBSyncCoordinator> _logger;

        public WholeUnitESBSyncCoordinator(
            WholeUnitPrdMOESBSyncService prdMOSyncService,
            WholeUnitPrdMODetailESBSyncService prdMODetailSyncService,
            WholeUnitTrackingESBSyncService trackingSyncService,
            ILogger<WholeUnitESBSyncCoordinator> logger)
        {
            _prdMOSyncService = prdMOSyncService;
            _prdMODetailSyncService = prdMODetailSyncService;
            _trackingSyncService = trackingSyncService;
            _logger = logger;
        }

        /// <summary>
        /// 全量同步整机数据
        /// 按顺序执行：生产订单头 -> 生产订单明细 -> 整机跟踪
        /// </summary>
        /// <param name="startDate">开始时间（可选）</param>
        /// <param name="endDate">结束时间（可选）</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncAllWholeUnitData(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var operationStartTime = DateTime.Now;

            try
            {
                _logger.LogInformation("==== 开始整机业务全量ESB数据同步 ====");
                _logger.LogInformation($"同步时间范围：{startDate ?? "默认"} 到 {endDate ?? "默认"}");

                var results = new System.Collections.Generic.List<string>();

                // 第一步：同步整机生产订单头
                _logger.LogInformation("第一步：开始同步整机生产订单头...");
                var step1Result = await _prdMOSyncService.SyncDataFromESB(startDate, endDate);
                results.Add($"生产订单头：{(step1Result.Status ? "成功" : "失败")} - {step1Result.Message}");

                if (!step1Result.Status)
                {
                    _logger.LogError($"整机生产订单头同步失败：{step1Result.Message}");
                    // 继续执行后续步骤，不因单个步骤失败而中断
                }

                // 第二步：同步整机生产订单明细
                _logger.LogInformation("第二步：开始同步整机生产订单明细...");
                var step2Result = await _prdMODetailSyncService.SyncDataFromESB(startDate, endDate);
                results.Add($"生产订单明细：{(step2Result.Status ? "成功" : "失败")} - {step2Result.Message}");

                if (!step2Result.Status)
                {
                    _logger.LogError($"整机生产订单明细同步失败：{step2Result.Message}");
                }

                // 第三步：同步整机跟踪
                _logger.LogInformation("第三步：开始同步整机跟踪...");
                var step3Result = await _trackingSyncService.SyncDataFromESB(startDate, endDate);
                results.Add($"整机跟踪：{(step3Result.Status ? "成功" : "失败")} - {step3Result.Message}");

                if (!step3Result.Status)
                {
                    _logger.LogError($"整机跟踪同步失败：{step3Result.Message}");
                }

                //4. 表体数据汇总到表头
                var summaryService = await OCP_PrdMOService.Instance.SummaryDetails2Head();
                if (summaryService.Status)
                {
                    _logger.LogInformation($"第4步完成：整机生产订单明细汇总成功 - {summaryService.Message}");
                }
                else
                {
                    _logger.LogError($"第4步失败：整机生产订单明细汇总失败 - {summaryService.Message}");
                }


                // 计算总执行时间
                var totalElapsed = DateTime.Now - operationStartTime;

                // 统计结果
                var successCount = 0;
                if (step1Result.Status) successCount++;
                if (step2Result.Status) successCount++;
                if (step3Result.Status) successCount++;

                var summaryMessage = $"整机业务全量同步完成，总耗时：{totalElapsed.TotalSeconds:F2}秒，" +
                                   $"成功：{successCount}/3，详情：{string.Join("；", results)}";

                _logger.LogInformation($"==== 整机业务全量ESB数据同步完成 ====");
                _logger.LogInformation(summaryMessage);

                response.Status = successCount > 0; // 至少有一个成功就算成功
                response.Message = summaryMessage;

                return response;
            }
            catch (Exception ex)
            {
                var errorMessage = $"整机业务全量同步过程中发生异常：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 仅同步整机生产订单数据（头+明细）
        /// </summary>
        /// <param name="startDate">开始时间（可选）</param>
        /// <param name="endDate">结束时间（可选）</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncWholeUnitPrdMOOnly(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var operationStartTime = DateTime.Now;

            try
            {
                _logger.LogInformation("==== 开始整机生产订单ESB数据同步 ====");

                var results = new System.Collections.Generic.List<string>();

                // 同步生产订单头
                var step1Result = await _prdMOSyncService.SyncDataFromESB(startDate, endDate);
                results.Add($"生产订单头：{(step1Result.Status ? "成功" : "失败")} - {step1Result.Message}");

                // 同步生产订单明细
                var step2Result = await _prdMODetailSyncService.SyncDataFromESB(startDate, endDate);
                results.Add($"生产订单明细：{(step2Result.Status ? "成功" : "失败")} - {step2Result.Message}");

                var totalElapsed = DateTime.Now - operationStartTime;
                var successCount = (step1Result.Status ? 1 : 0) + (step2Result.Status ? 1 : 0);

                var summaryMessage = $"整机生产订单同步完成，总耗时：{totalElapsed.TotalSeconds:F2}秒，" +
                                   $"成功：{successCount}/2，详情：{string.Join("；", results)}";

                _logger.LogInformation("==== 整机生产订单ESB数据同步完成 ====");

                response.Status = successCount > 0;
                response.Message = summaryMessage;
                return response;
            }
            catch (Exception ex)
            {
                var errorMessage = $"整机生产订单同步过程中发生异常：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 仅同步整机跟踪数据
        /// </summary>
        /// <param name="startDate">开始时间（可选）</param>
        /// <param name="endDate">结束时间（可选）</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncWholeUnitTrackingOnly(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var operationStartTime = DateTime.Now;

            try
            {
                _logger.LogInformation("==== 开始整机跟踪ESB数据同步 ====");

                var result = await _trackingSyncService.SyncDataFromESB(startDate, endDate);
                var totalElapsed = DateTime.Now - operationStartTime;

                var summaryMessage = $"整机跟踪同步完成，总耗时：{totalElapsed.TotalSeconds:F2}秒，" +
                                   $"结果：{(result.Status ? "成功" : "失败")} - {result.Message}";

                _logger.LogInformation("==== 整机跟踪ESB数据同步完成 ====");

                response.Status = result.Status;
                response.Message = summaryMessage;
                return response;
            }
            catch (Exception ex)
            {
                var errorMessage = $"整机跟踪同步过程中发生异常：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 获取整机业务同步状态信息
        /// </summary>
        /// <returns>状态信息</returns>
        public async Task<WebResponseContent> GetWholeUnitSyncStatus()
        {
            var response = new WebResponseContent();

            try
            {
                var statusInfo = new
                {
                    BusinessName = "整机业务ESB同步",
                    Version = "1.0.0",
                    Description = "整机生产订单和跟踪数据的ESB接口同步服务",
                    Services = new[]
                    {
                        new { Name = "整机生产订单头同步", ApiPath = "/SearchPrdMO", Status = "就绪" },
                        new { Name = "整机生产订单明细同步", ApiPath = "/SearchPrdMOEntry", Status = "就绪" },
                        new { Name = "整机跟踪同步", ApiPath = "/SearchWholeUnitTracking", Status = "就绪" }
                    },
                    AvailableOperations = new[]
                    {
                        "SyncAllWholeUnitData - 全量同步整机数据",
                        "SyncWholeUnitPrdMOOnly - 仅同步生产订单数据",
                        "SyncWholeUnitTrackingOnly - 仅同步跟踪数据"
                    },
                    LastCheckTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                response.Status = true;
                response.Data = statusInfo;
                response.Message = "整机业务ESB同步服务状态正常";

                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取整机业务同步状态时发生错误");
                return response.Error($"获取状态失败：{ex.Message}");
            }
        }
    }
}