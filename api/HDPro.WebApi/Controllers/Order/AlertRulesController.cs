/*
 * 预警规则控制器
 * 提供预警功能的API接口
 */
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Services.OrderCollaboration;
using HDPro.Core.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Entity.DomainModels.OrderCollaboration.Enums;

namespace HDPro.WebApi.Controllers.Order
{
    /// <summary>
    /// 预警规则控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlertRulesController : ControllerBase
    {
        private readonly IOCP_AlertRulesService _alertRulesService;
        private readonly AlertRulesSchedulerService _schedulerService;
        private readonly ILogger<AlertRulesController> _logger;

        public AlertRulesController(
            IOCP_AlertRulesService alertRulesService,
            AlertRulesSchedulerService schedulerService,
            ILogger<AlertRulesController> logger)
        {
            _alertRulesService = alertRulesService;
            _schedulerService = schedulerService;
            _logger = logger;
        }

        /// <summary>
        /// 执行所有预警规则检查
        /// </summary>
        /// <returns>执行结果</returns>
        [HttpPost("execute-all")]
        public async Task<WebResponseContent> ExecuteAllAlertRules()
        {
            _logger.LogInformation("接收到执行所有预警规则的请求");
            
            try
            {
                return await _alertRulesService.ExecuteAllAlertRulesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行所有预警规则时发生异常");
                return WebResponseContent.Instance.Error($"执行预警规则异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行指定预警规则
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns>执行结果</returns>
        [HttpPost("execute/{ruleId}")]
        public async Task<WebResponseContent> ExecuteAlertRule(long ruleId)
        {
            _logger.LogInformation("接收到执行指定预警规则的请求，规则ID: {RuleId}", ruleId);
            
            if (ruleId <= 0)
            {
                return WebResponseContent.Instance.Error("规则ID无效");
            }

            try
            {
                return await _alertRulesService.ExecuteAlertRuleByIdAsync(ruleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行指定预警规则时发生异常，规则ID: {RuleId}", ruleId);
                return WebResponseContent.Instance.Error($"执行预警规则异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试预警规则
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns>测试结果</returns>
        [HttpPost("test/{ruleId}")]
        public async Task<WebResponseContent> TestAlertRule(long ruleId)
        {
            _logger.LogInformation("接收到测试预警规则的请求，规则ID: {RuleId}", ruleId);
            
            if (ruleId <= 0)
            {
                return WebResponseContent.Instance.Error("规则ID无效");
            }

            try
            {
                return await _alertRulesService.TestAlertRuleAsync(ruleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试预警规则时发生异常，规则ID: {RuleId}", ruleId);
                return WebResponseContent.Instance.Error($"测试预警规则异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查Cron表达式是否应该执行
        /// </summary>
        /// <param name="cronExpression">Cron表达式</param>
        /// <returns>检查结果</returns>
        [HttpPost("check-cron")]
        public WebResponseContent CheckCronExpression([FromBody] string cronExpression)
        {
            _logger.LogInformation("接收到检查Cron表达式的请求: {CronExpression}", cronExpression);
            
            if (string.IsNullOrEmpty(cronExpression))
            {
                return WebResponseContent.Instance.Error("Cron表达式不能为空");
            }

            try
            {
                // 简单的Cron表达式验证
                var shouldExecute = !string.IsNullOrWhiteSpace(cronExpression) && cronExpression.Split(' ').Length >= 5;

                return WebResponseContent.Instance.OK("检查完成", new
                {
                    CronExpression = cronExpression,
                    ShouldExecute = shouldExecute,
                    CheckTime = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查Cron表达式时发生异常: {CronExpression}", cronExpression);
                return WebResponseContent.Instance.Error($"检查Cron表达式异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化所有预警规则的定时任务
        /// </summary>
        /// <returns>初始化结果</returns>
        [HttpPost("scheduler/initialize")]
        public async Task<WebResponseContent> InitializeScheduler()
        {
            _logger.LogInformation("接收到初始化预警规则定时任务的请求");

            try
            {
                return await _schedulerService.InitializeAllAlertRuleJobsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化预警规则定时任务时发生异常");
                return WebResponseContent.Instance.Error($"初始化定时任务异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建或更新预警规则的定时任务
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("scheduler/create/{ruleId}")]
        public async Task<WebResponseContent> CreateSchedulerJob(long ruleId)
        {
            _logger.LogInformation("接收到创建预警规则定时任务的请求，规则ID: {RuleId}", ruleId);

            if (ruleId <= 0)
            {
                return WebResponseContent.Instance.Error("规则ID无效");
            }

            try
            {
                // 首先获取规则信息
                var pageData = new PageDataOptions();
                pageData.Filter = new List<SearchParameters>
                {
                    new SearchParameters { Name = "ID", Value = ruleId.ToString() }
                };
                var result = _alertRulesService.GetPageData(pageData);

                if (result.rows == null || !result.rows.Any())
                {
                    return WebResponseContent.Instance.Error("预警规则不存在");
                }

                var rule = result.rows.First();
                return await _schedulerService.CreateOrUpdateAlertRuleJobAsync(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建预警规则定时任务时发生异常，规则ID: {RuleId}", ruleId);
                return WebResponseContent.Instance.Error($"创建定时任务异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除预警规则的定时任务
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("scheduler/delete/{ruleId}")]
        public async Task<WebResponseContent> DeleteSchedulerJob(long ruleId)
        {
            _logger.LogInformation("接收到删除预警规则定时任务的请求，规则ID: {RuleId}", ruleId);

            if (ruleId <= 0)
            {
                return WebResponseContent.Instance.Error("规则ID无效");
            }

            try
            {
                return await _schedulerService.DeleteAlertRuleJobAsync(ruleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除预警规则定时任务时发生异常，规则ID: {RuleId}", ruleId);
                return WebResponseContent.Instance.Error($"删除定时任务异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 暂停预警规则的定时任务
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("scheduler/pause/{ruleId}")]
        public async Task<WebResponseContent> PauseSchedulerJob(long ruleId)
        {
            _logger.LogInformation("接收到暂停预警规则定时任务的请求，规则ID: {RuleId}", ruleId);

            if (ruleId <= 0)
            {
                return WebResponseContent.Instance.Error("规则ID无效");
            }

            try
            {
                return await _schedulerService.PauseAlertRuleJobAsync(ruleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "暂停预警规则定时任务时发生异常，规则ID: {RuleId}", ruleId);
                return WebResponseContent.Instance.Error($"暂停定时任务异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 恢复预警规则的定时任务
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("scheduler/resume/{ruleId}")]
        public async Task<WebResponseContent> ResumeSchedulerJob(long ruleId)
        {
            _logger.LogInformation("接收到恢复预警规则定时任务的请求，规则ID: {RuleId}", ruleId);

            if (ruleId <= 0)
            {
                return WebResponseContent.Instance.Error("规则ID无效");
            }

            try
            {
                return await _schedulerService.ResumeAlertRuleJobAsync(ruleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "恢复预警规则定时任务时发生异常，规则ID: {RuleId}", ruleId);
                return WebResponseContent.Instance.Error($"恢复定时任务异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有预警规则任务的状态
        /// </summary>
        /// <returns>任务状态列表</returns>
        [HttpGet("scheduler/status")]
        public async Task<WebResponseContent> GetSchedulerStatus()
        {
            _logger.LogInformation("接收到获取预警规则任务状态的请求");

            try
            {
                return await _schedulerService.GetAllAlertRuleJobStatusAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取预警规则任务状态时发生异常");
                return WebResponseContent.Instance.Error($"获取任务状态异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新预警规则任务状态
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <param name="taskStatus">任务状态</param>
        /// <returns>操作结果</returns>
        [HttpPost("task-status/{ruleId}/{taskStatus}")]
        public async Task<WebResponseContent> UpdateTaskStatus(long ruleId, int taskStatus)
        {
            _logger.LogInformation("接收到更新预警规则任务状态的请求，规则ID: {RuleId}, 状态: {TaskStatus}", ruleId, taskStatus);

            if (ruleId <= 0)
            {
                return WebResponseContent.Instance.Error("规则ID无效");
            }

            if (!Enum.IsDefined(typeof(AlertRuleTaskStatus), taskStatus))
            {
                return WebResponseContent.Instance.Error("任务状态值无效");
            }

            try
            {
                // 直接操作DbContext更新任务状态
                var updateResult = await _alertRulesService.UpdateTaskStatusAsync(ruleId, taskStatus);
                if (!updateResult.Status)
                {
                    return WebResponseContent.Instance.Error($"更新任务状态失败: {updateResult.Message}");
                }

                var status = (AlertRuleTaskStatus)taskStatus;

                // 根据状态管理定时任务
                switch (status)
                {
                    case AlertRuleTaskStatus.Stopped:
                        await _schedulerService.DeleteAlertRuleJobAsync(ruleId);
                        break;
                    case AlertRuleTaskStatus.Enabled:
                    case AlertRuleTaskStatus.Paused:
                        // 对于启用和暂停状态，需要获取完整的规则信息
                        var pageData = new PageDataOptions();
                        pageData.Filter = new List<SearchParameters>
                        {
                            new SearchParameters { Name = "ID", Value = ruleId.ToString() }
                        };
                        var result = _alertRulesService.GetPageData(pageData);

                        if (result.rows == null || !result.rows.Any())
                        {
                            return WebResponseContent.Instance.Error("预警规则不存在，无法创建定时任务");
                        }

                        // 将动态对象转换为OCP_AlertRules实体
                        var ruleData = result.rows.First();
                        var alertRuleEntity = new OCP_AlertRules
                        {
                            ID = Convert.ToInt64(ruleData.ID),
                            RuleName = ruleData.RuleName?.ToString(),
                            AlertPage = ruleData.AlertPage?.ToString(),
                            FieldName = ruleData.FieldName?.ToString(),
                            DayCount = ruleData.DayCount != null ? Convert.ToInt32(ruleData.DayCount) : 0,
                            FinishStatusField = ruleData.FinishStatusField?.ToString(),
                            ConditionType = ruleData.ConditionType?.ToString(),
                            ConditionValue = ruleData.ConditionValue?.ToString(),
                            ResponsiblePersonLoginName = ruleData.ResponsiblePersonLoginName?.ToString(),
                            PushInterval = ruleData.PushInterval?.ToString(),
                            TriggerOA = ruleData.TriggerOA != null ? Convert.ToInt32(ruleData.TriggerOA) : 0,
                            TaskStatus = taskStatus
                        };

                        await _schedulerService.CreateOrUpdateAlertRuleJobAsync(alertRuleEntity);
                        break;
                }

                return WebResponseContent.Instance.OK($"预警规则任务状态已更新为: {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新预警规则任务状态时发生异常，规则ID: {RuleId}", ruleId);
                return WebResponseContent.Instance.Error($"更新任务状态异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 批量更新预警规则任务状态
        /// </summary>
        /// <param name="request">批量更新请求</param>
        /// <returns>操作结果</returns>
        [HttpPost("batch-task-status")]
        public async Task<WebResponseContent> BatchUpdateTaskStatus([FromBody] BatchUpdateTaskStatusRequest request)
        {
            _logger.LogInformation("接收到批量更新预警规则任务状态的请求，规则数量: {Count}", request.RuleIds?.Count ?? 0);

            if (request?.RuleIds == null || request.RuleIds.Count == 0)
            {
                return WebResponseContent.Instance.Error("规则ID列表不能为空");
            }

            if (!Enum.IsDefined(typeof(AlertRuleTaskStatus), request.TaskStatus))
            {
                return WebResponseContent.Instance.Error("任务状态值无效");
            }

            try
            {
                var successCount = 0;
                var failCount = 0;

                foreach (var ruleId in request.RuleIds)
                {
                    try
                    {
                        var result = await UpdateTaskStatus(ruleId, request.TaskStatus);
                        if (result.Status)
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                        }
                    }
                    catch
                    {
                        failCount++;
                    }
                }

                return WebResponseContent.Instance.OK($"批量更新完成，成功: {successCount}, 失败: {failCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新预警规则任务状态时发生异常");
                return WebResponseContent.Instance.Error($"批量更新异常: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 预警规则请求模型
    /// </summary>
    public class AlertRuleRequest
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        public long RuleId { get; set; }
    }

    /// <summary>
    /// Cron表达式检查请求模型
    /// </summary>
    public class CronExpressionRequest
    {
        /// <summary>
        /// Cron表达式
        /// </summary>
        public string CronExpression { get; set; }
    }

    /// <summary>
    /// 批量更新任务状态请求模型
    /// </summary>
    public class BatchUpdateTaskStatusRequest
    {
        /// <summary>
        /// 规则ID列表
        /// </summary>
        public List<long> RuleIds { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public int TaskStatus { get; set; }
    }
}
