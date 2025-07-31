/*
 * 缺料运算结果ESB同步协调器
 * 负责协调和管理缺料运算结果相关的ESB同步操作
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IRepositories;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.LackMaterial
{
    /// <summary>
    /// 缺料运算结果ESB同步协调器
    /// </summary>
    public class LackMtrlResultESBSyncCoordinator
    {
        private readonly LackMtrlResultESBSyncService _lackMtrlResultESBSyncService;
        private readonly IOCP_LackMtrlPlanRepository _planRepository;
        private readonly IOCP_LackMtrlPoolRepository _poolRepository;
        private readonly ILogger<LackMtrlResultESBSyncCoordinator> _logger;
        private readonly ESBLogger _esbLogger;

        public LackMtrlResultESBSyncCoordinator(
            LackMtrlResultESBSyncService lackMtrlResultESBSyncService,
            IOCP_LackMtrlPlanRepository planRepository,
            IOCP_LackMtrlPoolRepository poolRepository,
            ILogger<LackMtrlResultESBSyncCoordinator> logger,
            ILoggerFactory loggerFactory)
        {
            _lackMtrlResultESBSyncService = lackMtrlResultESBSyncService;
            _planRepository = planRepository;
            _poolRepository = poolRepository;
            _logger = logger;
            _esbLogger = ESBLoggerFactory.CreateLackMaterialLogger(loggerFactory);
        }

        /// <summary>
        /// 批量同步所有可用运算方案的缺料数据
        /// 自动获取运算方案列表，检查运算队列状态，循环执行同步
        /// </summary>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncAllAvailableComputeSchemes()
        {
            var response = new WebResponseContent();
            var syncStartTime = DateTime.Now;

            try
            {
                _esbLogger.LogInfo("开始批量同步所有可用运算方案的缺料数据");

                // 1. 获取所有运算方案
                var availableSchemes = await GetAvailableComputeSchemes();
                
                if (!availableSchemes.Any())
                {
                    var message = "未找到可用的运算方案";
                    _logger.LogInformation(message);
                    return response.OK(message);
                }

                _logger.LogInformation($"找到 {availableSchemes.Count} 个可用运算方案");

                // 2. 循环处理每个运算方案
                var results = new List<string>();
                var errors = new List<string>();
                var processedCount = 0;
                var skippedCount = 0;

                foreach (var scheme in availableSchemes)
                {
                    try
                    {
                        // 检查运算队列状态，如果已完成则跳过
                        var isCompleted = await IsComputeSchemeCompleted(scheme.ComputeID);
                        if (isCompleted)
                        {
                            _logger.LogInformation($"运算方案 {scheme.ComputeID}({scheme.PlanName}) 已完成同步，跳过处理");
                            skippedCount++;
                            continue;
                        }

                        _logger.LogInformation($"开始同步运算方案：{scheme.ComputeID}({scheme.PlanName})");

                        // 执行单个运算方案的同步
                        var syncResult = await _lackMtrlResultESBSyncService.SyncDataByComputeId(scheme.ComputeID.ToString());
                        
                        if (syncResult.Status)
                        {
                            results.Add($"运算方案 {scheme.ComputeID}({scheme.PlanName})：{syncResult.Message}");
                            processedCount++;
                        }
                        else
                        {
                            errors.Add($"运算方案 {scheme.ComputeID}({scheme.PlanName}) 同步失败：{syncResult.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        var error = $"运算方案 {scheme.ComputeID}({scheme.PlanName}) 同步异常：{ex.Message}";
                        errors.Add(error);
                        _logger.LogError(ex, error);
                    }
                }

                // 3. 汇总结果
                var syncEndTime = DateTime.Now;
                var totalTime = (syncEndTime - syncStartTime).TotalSeconds;

                var summary = $"批量缺料数据同步完成！" +
                    $"总运算方案：{availableSchemes.Count}，" +
                    $"成功同步：{processedCount}，" +
                    $"跳过已完成：{skippedCount}，" +
                    $"失败：{errors.Count}，" +
                    $"总耗时：{totalTime:F2} 秒";

                if (errors.Any())
                {
                    var detailMessage = $"{summary}\n成功：\n{string.Join("\n", results)}\n失败：\n{string.Join("\n", errors)}";
                    _logger.LogWarning(detailMessage);
                    return response.Error(detailMessage);
                }
                else
                {
                    var detailMessage = $"{summary}\n详情：\n{string.Join("\n", results)}";
                    _logger.LogInformation(detailMessage);
                    return response.OK(detailMessage);
                }
            }
            catch (Exception ex)
            {
                var syncEndTime = DateTime.Now;
                var totalTime = (syncEndTime - syncStartTime).TotalSeconds;
                var errorMessage = $"批量缺料数据同步异常，总耗时：{totalTime:F2} 秒，错误：{ex.Message}";
                
                _logger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 根据运算ID执行缺料数据同步
        /// </summary>
        /// <param name="computeId">运算ID</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ExecuteSyncByComputeId(string computeId)
        {
            var response = new WebResponseContent();
            var syncStartTime = DateTime.Now;

            try
            {
                _logger.LogInformation($"开始执行缺料运算结果ESB同步流程，运算ID：{computeId}");

                // 执行缺料数据同步
                var lackMtrlResult = await _lackMtrlResultESBSyncService.SyncDataByComputeId(computeId);
                
                if (!lackMtrlResult.Status)
                {
                    _logger.LogError($"缺料数据同步失败：{lackMtrlResult.Message}");
                    return response.Error($"缺料数据同步失败：{lackMtrlResult.Message}");
                }

                var syncEndTime = DateTime.Now;
                var totalTime = (syncEndTime - syncStartTime).TotalSeconds;

                var successMessage = $"缺料运算结果ESB同步流程执行完成！" +
                    $"缺料数据同步：{lackMtrlResult.Message}，" +
                    $"总耗时：{totalTime:F2} 秒";

                _logger.LogInformation(successMessage);
                return response.OK(successMessage);
            }
            catch (Exception ex)
            {
                var syncEndTime = DateTime.Now;
                var totalTime = (syncEndTime - syncStartTime).TotalSeconds;
                var errorMessage = $"缺料运算结果ESB同步流程执行失败，耗时：{totalTime:F2} 秒，错误：{ex.Message}";
                
                _logger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 手动触发缺料数据同步
        /// </summary>
        /// <param name="computeId">运算ID</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSync(string computeId)
        {
            _logger.LogInformation($"手动触发缺料运算结果ESB同步，运算ID：{computeId}");
            return await ExecuteSyncByComputeId(computeId);
        }

        /// <summary>
        /// 获取同步状态信息
        /// </summary>
        /// <param name="computeId">运算ID</param>
        /// <returns>状态信息</returns>
        public async Task<WebResponseContent> GetSyncStatus(string computeId)
        {
            try
            {
                _logger.LogInformation($"获取缺料运算结果同步状态，运算ID：{computeId}");
                
                if (long.TryParse(computeId, out long computeIdLong))
                {
                    // 检查运算方案是否存在
                    var scheme = await Task.Run(() =>
                        _planRepository.FindFirst(x => x.ComputeID == computeIdLong));
                    
                    if (scheme == null)
                    {
                        return new WebResponseContent().Error($"未找到运算ID为 {computeId} 的运算方案");
                    }

                    // 检查运算队列状态
                    var isCompleted = await IsComputeSchemeCompleted(computeIdLong);
                    
                    var statusInfo = new
                    {
                        ComputeId = computeId,
                        PlanName = scheme.PlanName,
                        IsCompleted = isCompleted,
                        CreateDate = scheme.CreateDate,
                        Creator = scheme.Creator,
                        LastCheckTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    return new WebResponseContent().OK($"缺料运算结果同步状态查询成功", statusInfo);
                }
                else
                {
                    return new WebResponseContent().Error("运算ID格式无效");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取缺料运算结果同步状态失败，运算ID：{computeId}");
                return new WebResponseContent().Error($"获取状态失败：{ex.Message}");
            }
        }

        #region 私有辅助方法

        /// <summary>
        /// 获取所有可用的运算方案
        /// </summary>
        /// <returns>运算方案列表</returns>
        private async Task<List<OCP_LackMtrlPlan>> GetAvailableComputeSchemes()
        {
            try
            {
                // 获取所有运算方案，按创建时间降序排列
                var schemes = await Task.Run(() =>
                    _planRepository.FindAsIQueryable(x => true)
                    .OrderByDescending(x => x.CreateDate)
                    .ToList());

                _logger.LogInformation($"从数据库获取到 {schemes.Count} 个运算方案");
                return schemes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取运算方案列表异常");
                return new List<OCP_LackMtrlPlan>();
            }
        }

        /// <summary>
        /// 检查运算方案是否已完成同步
        /// </summary>
        /// <param name="computeId">运算ID</param>
        /// <returns>是否已完成</returns>
        private async Task<bool> IsComputeSchemeCompleted(long computeId)
        {
            try
            {
                // 检查运算队列中是否存在状态为2(运算完成)的记录
                var completedCount = await Task.Run(() =>
                    _poolRepository.FindAsIQueryable(x => x.ComputeID == computeId && x.Status == 2)
                    .Count());

                return completedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"检查运算方案完成状态异常，运算ID：{computeId}");
                return false; // 异常情况下认为未完成，允许同步
            }
        }

        #endregion
    }
} 