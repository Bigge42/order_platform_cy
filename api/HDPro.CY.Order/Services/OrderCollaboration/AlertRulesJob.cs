/*
 * 预警规则定时任务
 * 用于定期执行预警规则检查
 */
using HDPro.CY.Order.IServices;
using HDPro.Core.Extensions.AutofacManager;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OrderCollaboration
{
    /// <summary>
    /// 预警规则定时任务
    /// </summary>
    [DisallowConcurrentExecution] // 防止并发执行
    public class AlertRulesJob : IJob
    {
        private readonly ILogger<AlertRulesJob> _logger;
        private readonly IOCP_AlertRulesService _alertRulesService;
        private readonly AlertRulesLogService _logService;

        public AlertRulesJob(
            ILogger<AlertRulesJob> logger,
            IOCP_AlertRulesService alertRulesService,
            AlertRulesLogService logService)
        {
            _logger = logger;
            _alertRulesService = alertRulesService;
            _logService = logService;
        }

        /// <summary>
        /// 执行定时任务
        /// </summary>
        /// <param name="context">任务执行上下文</param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key;
            var triggerKey = context.Trigger.Key;
            var taskName = "预警规则全量检查任务";

            _logger.LogInformation("预警规则定时任务开始执行 - Job: {JobKey}, Trigger: {TriggerKey}",
                jobKey, triggerKey);

            // 记录任务开始
            var logId = await _logService.LogTaskStartAsync(taskName);

            try
            {
                // 执行所有预警规则检查
                var result = await _alertRulesService.ExecuteAllAlertRulesAsync();

                if (result.Status)
                {
                    _logger.LogInformation("预警规则定时任务执行成功 - {Message}", result.Message);
                    await _logService.LogTaskCompleteAsync(logId, true, result.Message);
                }
                else
                {
                    _logger.LogError("预警规则定时任务执行失败 - {Message}", result.Message);
                    await _logService.LogTaskCompleteAsync(logId, false, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "预警规则定时任务执行时发生异常");
                await _logService.LogTaskExceptionAsync(logId, ex);

                // 可以在这里添加异常通知逻辑
                // 比如发送邮件或OA消息给管理员
            }
            finally
            {
                _logger.LogInformation("预警规则定时任务执行完成 - Job: {JobKey}", jobKey);
            }
        }
    }

    /// <summary>
    /// 单个预警规则定时任务
    /// 用于执行特定的预警规则
    /// </summary>
    [DisallowConcurrentExecution]
    public class SingleAlertRuleJob : IJob
    {
        private readonly ILogger<SingleAlertRuleJob> _logger;
        private readonly IOCP_AlertRulesService _alertRulesService;
        private readonly AlertRulesLogService _logService;

        public SingleAlertRuleJob(
            ILogger<SingleAlertRuleJob> logger,
            IOCP_AlertRulesService alertRulesService,
            AlertRulesLogService logService)
        {
            _logger = logger;
            _alertRulesService = alertRulesService;
            _logService = logService;
        }

        /// <summary>
        /// 执行单个预警规则任务
        /// </summary>
        /// <param name="context">任务执行上下文</param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key;
            var triggerKey = context.Trigger.Key;

            // 从JobDataMap中获取规则ID
            var ruleId = context.JobDetail.JobDataMap.GetLongValue("RuleId");
            var taskName = $"预警规则单个检查任务(规则ID:{ruleId})";

            _logger.LogInformation("单个预警规则定时任务开始执行 - Job: {JobKey}, Trigger: {TriggerKey}, RuleId: {RuleId}",
                jobKey, triggerKey, ruleId);

            // 记录任务开始
            var logId = await _logService.LogTaskStartAsync(taskName, ruleId);

            try
            {
                if (ruleId <= 0)
                {
                    _logger.LogError("预警规则ID无效: {RuleId}", ruleId);
                    await _logService.LogTaskCompleteAsync(logId, false, null, $"预警规则ID无效: {ruleId}");
                    return;
                }

                // 执行指定预警规则
                var result = await _alertRulesService.ExecuteAlertRuleByIdAsync(ruleId);

                if (result.Status)
                {
                    _logger.LogInformation("单个预警规则定时任务执行成功 - RuleId: {RuleId}, {Message}",
                        ruleId, result.Message);
                    await _logService.LogTaskCompleteAsync(logId, true, result.Message);
                }
                else
                {
                    _logger.LogError("单个预警规则定时任务执行失败 - RuleId: {RuleId}, {Message}",
                        ruleId, result.Message);
                    await _logService.LogTaskCompleteAsync(logId, false, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "单个预警规则定时任务执行时发生异常 - RuleId: {RuleId}", ruleId);
                await _logService.LogTaskExceptionAsync(logId, ex);
            }
            finally
            {
                _logger.LogInformation("单个预警规则定时任务执行完成 - Job: {JobKey}, RuleId: {RuleId}",
                    jobKey, ruleId);
            }
        }
    }
}
