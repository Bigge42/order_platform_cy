/*
 * 预警规则调度服务
 * 负责管理预警规则的定时任务
 */
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.OrderCollaboration.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OrderCollaboration
{
    /// <summary>
    /// 预警规则调度服务
    /// </summary>
    public class AlertRulesSchedulerService : IDependency
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IOCP_AlertRulesService _alertRulesService;
        private readonly ILogger<AlertRulesSchedulerService> _logger;

        public AlertRulesSchedulerService(
            ISchedulerFactory schedulerFactory,
            IOCP_AlertRulesService alertRulesService,
            ILogger<AlertRulesSchedulerService> logger)
        {
            _schedulerFactory = schedulerFactory;
            _alertRulesService = alertRulesService;
            _logger = logger;
        }

        /// <summary>
        /// 初始化所有预警规则的定时任务
        /// </summary>
        /// <returns></returns>
        public async Task<WebResponseContent> InitializeAllAlertRuleJobsAsync()
        {
            var response = new WebResponseContent();
            try
            {
                _logger.LogInformation("开始初始化所有预警规则的定时任务");

                // 获取所有启用的预警规则
                var alertRules = await GetActiveAlertRulesWithScheduleAsync();
                
                if (!alertRules.Any())
                {
                    _logger.LogInformation("没有找到需要调度的预警规则");
                    return response.OK("没有需要调度的预警规则");
                }

                var scheduler = await _schedulerFactory.GetScheduler();
                var successCount = 0;
                var failCount = 0;

                foreach (var rule in alertRules)
                {
                    try
                    {
                        await CreateOrUpdateAlertRuleJobAsync(rule);
                        successCount++;
                        _logger.LogInformation("预警规则 [{RuleName}] 定时任务创建成功", rule.RuleName);
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        _logger.LogError(ex, "创建预警规则 [{RuleName}] 定时任务时发生异常", rule.RuleName);
                    }
                }

                _logger.LogInformation("预警规则定时任务初始化完成，成功: {SuccessCount}, 失败: {FailCount}", 
                    successCount, failCount);

                return response.OK($"定时任务初始化完成，成功: {successCount}, 失败: {failCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化预警规则定时任务时发生异常");
                return response.Error($"初始化定时任务异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建或更新单个预警规则的定时任务
        /// </summary>
        /// <param name="rule">预警规则</param>
        /// <returns></returns>
        public async Task<WebResponseContent> CreateOrUpdateAlertRuleJobAsync(OCP_AlertRules rule)
        {
            var response = new WebResponseContent();
            try
            {
                if (string.IsNullOrEmpty(rule.PushInterval))
                {
                    return response.Error("推送周期不能为空");
                }

                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey($"AlertRule_{rule.ID}", "AlertRules");
                var triggerKey = new TriggerKey($"AlertRule_{rule.ID}_Trigger", "AlertRules");

                // 删除已存在的任务
                if (await scheduler.CheckExists(jobKey))
                {
                    await scheduler.DeleteJob(jobKey);
                    _logger.LogInformation("删除已存在的预警规则任务: {JobKey}", jobKey);
                }

                // 根据TaskStatus决定是否创建任务
                var taskStatus = (AlertRuleTaskStatus)rule.TaskStatus;

                if (taskStatus == AlertRuleTaskStatus.Stopped)
                {
                    _logger.LogInformation("预警规则 [{RuleName}] 状态为停止，不创建定时任务", rule.RuleName);
                    return response.OK($"预警规则 [{rule.RuleName}] 状态为停止，不创建定时任务");
                }

                // 创建新的任务
                var job = JobBuilder.Create<SingleAlertRuleJob>()
                    .WithIdentity(jobKey)
                    .WithDescription($"预警规则: {rule.RuleName}")
                    .UsingJobData("RuleId", rule.ID)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .WithDescription($"预警规则触发器: {rule.RuleName}")
                    .WithCronSchedule(rule.PushInterval)
                    .Build();

                await scheduler.ScheduleJob(job, trigger);

                // 根据状态决定是否暂停任务
                if (taskStatus == AlertRuleTaskStatus.Paused)
                {
                    await scheduler.PauseJob(jobKey);
                    _logger.LogInformation("预警规则定时任务创建并暂停 - 规则: {RuleName}, Cron: {CronExpression}",
                        rule.RuleName, rule.PushInterval);
                    return response.OK($"预警规则 [{rule.RuleName}] 定时任务创建成功并已暂停");
                }
                else
                {
                    _logger.LogInformation("预警规则定时任务创建并启用 - 规则: {RuleName}, Cron: {CronExpression}",
                        rule.RuleName, rule.PushInterval);
                    return response.OK($"预警规则 [{rule.RuleName}] 定时任务创建成功并已启用");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建预警规则定时任务时发生异常，规则: {RuleName}", rule.RuleName);
                return response.Error($"创建定时任务异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除预警规则的定时任务
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns></returns>
        public async Task<WebResponseContent> DeleteAlertRuleJobAsync(long ruleId)
        {
            var response = new WebResponseContent();
            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey($"AlertRule_{ruleId}", "AlertRules");

                if (await scheduler.CheckExists(jobKey))
                {
                    await scheduler.DeleteJob(jobKey);
                    _logger.LogInformation("预警规则定时任务删除成功，规则ID: {RuleId}", ruleId);
                    return response.OK($"预警规则定时任务删除成功");
                }
                else
                {
                    _logger.LogWarning("预警规则定时任务不存在，规则ID: {RuleId}", ruleId);
                    return response.OK("定时任务不存在");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除预警规则定时任务时发生异常，规则ID: {RuleId}", ruleId);
                return response.Error($"删除定时任务异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 暂停预警规则的定时任务
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns></returns>
        public async Task<WebResponseContent> PauseAlertRuleJobAsync(long ruleId)
        {
            var response = new WebResponseContent();
            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey($"AlertRule_{ruleId}", "AlertRules");

                if (await scheduler.CheckExists(jobKey))
                {
                    await scheduler.PauseJob(jobKey);
                    _logger.LogInformation("预警规则定时任务暂停成功，规则ID: {RuleId}", ruleId);
                    return response.OK("预警规则定时任务暂停成功");
                }
                else
                {
                    return response.Error("定时任务不存在");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "暂停预警规则定时任务时发生异常，规则ID: {RuleId}", ruleId);
                return response.Error($"暂停定时任务异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 恢复预警规则的定时任务
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns></returns>
        public async Task<WebResponseContent> ResumeAlertRuleJobAsync(long ruleId)
        {
            var response = new WebResponseContent();
            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey($"AlertRule_{ruleId}", "AlertRules");

                if (await scheduler.CheckExists(jobKey))
                {
                    await scheduler.ResumeJob(jobKey);
                    _logger.LogInformation("预警规则定时任务恢复成功，规则ID: {RuleId}", ruleId);
                    return response.OK("预警规则定时任务恢复成功");
                }
                else
                {
                    return response.Error("定时任务不存在");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "恢复预警规则定时任务时发生异常，规则ID: {RuleId}", ruleId);
                return response.Error($"恢复定时任务异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有需要调度的预警规则
        /// </summary>
        /// <returns></returns>
        private async Task<List<OCP_AlertRules>> GetActiveAlertRulesWithScheduleAsync()
        {
            // 这里应该调用AlertRulesService的方法，但为了避免循环依赖，直接使用Repository
            var repository = AutofacContainerModule.GetService<IOCP_AlertRulesRepository>();

            return await repository.FindAsIQueryable(x =>
                !string.IsNullOrEmpty(x.RuleName) &&
                !string.IsNullOrEmpty(x.PushInterval) &&
                (x.TaskStatus == (int)AlertRuleTaskStatus.Enabled || x.TaskStatus == (int)AlertRuleTaskStatus.Paused))
                .ToListAsync();
        }

        /// <summary>
        /// 获取所有预警规则任务的状态
        /// </summary>
        /// <returns></returns>
        public async Task<WebResponseContent> GetAllAlertRuleJobStatusAsync()
        {
            var response = new WebResponseContent();
            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("AlertRules"));
                
                var jobStatuses = new List<object>();
                
                foreach (var jobKey in jobKeys)
                {
                    var jobDetail = await scheduler.GetJobDetail(jobKey);
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    
                    var ruleId = jobDetail.JobDataMap.GetLongValue("RuleId");
                    var triggerState = triggers.Any() ? 
                        await scheduler.GetTriggerState(triggers.First().Key) : 
                        TriggerState.None;
                    
                    jobStatuses.Add(new
                    {
                        RuleId = ruleId,
                        JobKey = jobKey.ToString(),
                        Description = jobDetail.Description,
                        TriggerState = triggerState.ToString(),
                        NextFireTime = triggers.Any() ? triggers.First().GetNextFireTimeUtc()?.ToString("yyyy-MM-dd HH:mm:ss") : null,
                        PreviousFireTime = triggers.Any() ? triggers.First().GetPreviousFireTimeUtc()?.ToString("yyyy-MM-dd HH:mm:ss") : null
                    });
                }
                
                return response.OK("获取任务状态成功", jobStatuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取预警规则任务状态时发生异常");
                return response.Error($"获取任务状态异常: {ex.Message}");
            }
        }
    }
}
