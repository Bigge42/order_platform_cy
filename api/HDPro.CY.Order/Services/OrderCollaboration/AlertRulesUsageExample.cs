/*
 * 预警功能使用示例
 * 演示如何使用预警规则功能
 */
using HDPro.CY.Order.IServices;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OrderCollaboration
{
    /// <summary>
    /// 预警功能使用示例
    /// </summary>
    public class AlertRulesUsageExample
    {
        private readonly IOCP_AlertRulesService _alertRulesService;
        private readonly AlertRulesSchedulerService _schedulerService;
        private readonly ILogger<AlertRulesUsageExample> _logger;

        public AlertRulesUsageExample(
            IOCP_AlertRulesService alertRulesService,
            AlertRulesSchedulerService schedulerService,
            ILogger<AlertRulesUsageExample> logger)
        {
            _alertRulesService = alertRulesService;
            _schedulerService = schedulerService;
            _logger = logger;
        }

        /// <summary>
        /// 示例1：创建销售订单交期预警规则
        /// </summary>
        /// <returns></returns>
        public async Task<WebResponseContent> CreateSalesOrderDeliveryAlertExample()
        {
            try
            {
                var alertRule = new OCP_AlertRules
                {
                    RuleName = "销售订单交期预警",
                    AlertPage = "OCP_SalesOrder", // 表名
                    FieldName = "DeliveryDate", // 交期字段
                    DayCount = 3, // 提前3天预警
                    FinishStatusField = "OrderStatus", // 完成状态字段
                    ConditionType = "根据完成判定值来判断", // 完成判定方式
                    ConditionValue = "已完成", // 完成判定值
                    ResponsiblePersonName = "销售经理",
                    ResponsiblePersonLoginName = "sales_manager",
                    PushInterval = "0 0 9 * * ?", // 每天上午9点执行
                    TriggerOA = 1, // 触发OA流程
                    CreateDate = DateTime.Now,
                    Creator = "系统管理员"
                };

                // 这里应该调用Service的Add方法保存规则
                _logger.LogInformation("创建销售订单交期预警规则示例");
                
                return new WebResponseContent().OK("销售订单交期预警规则创建成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建销售订单交期预警规则示例时发生异常");
                return new WebResponseContent().Error($"创建预警规则异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例2：创建采购订单到货预警规则
        /// </summary>
        /// <returns></returns>
        public async Task<WebResponseContent> CreatePurchaseOrderArrivalAlertExample()
        {
            try
            {
                var alertRule = new OCP_AlertRules
                {
                    RuleName = "采购订单到货预警",
                    AlertPage = "OCP_PurchaseOrder", // 表名
                    FieldName = "ExpectedArrivalDate", // 预计到货日期字段
                    DayCount = 2, // 提前2天预警
                    FinishStatusField = "ArrivalStatus", // 到货状态字段
                    ConditionType = "是否有值判断", // 完成判定方式：有值表示已到货
                    ConditionValue = "", // 空值表示未到货
                    ResponsiblePersonName = "采购经理",
                    ResponsiblePersonLoginName = "purchase_manager",
                    PushInterval = "0 0 8,14 * * ?", // 每天上午8点和下午2点执行
                    TriggerOA = 1, // 触发OA流程
                    CreateDate = DateTime.Now,
                    Creator = "系统管理员"
                };

                _logger.LogInformation("创建采购订单到货预警规则示例");
                
                return new WebResponseContent().OK("采购订单到货预警规则创建成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建采购订单到货预警规则示例时发生异常");
                return new WebResponseContent().Error($"创建预警规则异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例3：手动执行预警检查
        /// </summary>
        /// <returns></returns>
        public async Task<WebResponseContent> ManualExecuteAlertExample()
        {
            try
            {
                _logger.LogInformation("开始手动执行预警检查示例");

                // 执行所有预警规则
                var result = await _alertRulesService.ExecuteAllAlertRulesAsync();
                
                _logger.LogInformation("手动执行预警检查完成，结果: {Result}", result.Message);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手动执行预警检查示例时发生异常");
                return new WebResponseContent().Error($"执行预警检查异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例4：测试预警规则
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns></returns>
        public async Task<WebResponseContent> TestAlertRuleExample(long ruleId)
        {
            try
            {
                _logger.LogInformation("开始测试预警规则示例，规则ID: {RuleId}", ruleId);

                // 测试预警规则（不发送通知）
                var result = await _alertRulesService.TestAlertRuleAsync(ruleId);
                
                _logger.LogInformation("测试预警规则完成，规则ID: {RuleId}, 结果: {Result}", 
                    ruleId, result.Message);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试预警规则示例时发生异常，规则ID: {RuleId}", ruleId);
                return new WebResponseContent().Error($"测试预警规则异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例5：管理定时任务
        /// </summary>
        /// <returns></returns>
        public async Task<WebResponseContent> ManageSchedulerExample()
        {
            try
            {
                _logger.LogInformation("开始管理定时任务示例");

                // 初始化所有预警规则的定时任务
                var initResult = await _schedulerService.InitializeAllAlertRuleJobsAsync();
                _logger.LogInformation("初始化定时任务结果: {Result}", initResult.Message);

                // 获取所有任务状态
                var statusResult = await _schedulerService.GetAllAlertRuleJobStatusAsync();
                _logger.LogInformation("获取任务状态结果: {Result}", statusResult.Message);

                return new WebResponseContent().OK("定时任务管理示例完成", new
                {
                    InitResult = initResult,
                    StatusResult = statusResult
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "管理定时任务示例时发生异常");
                return new WebResponseContent().Error($"管理定时任务异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例6：暂停和恢复定时任务
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns></returns>
        public async Task<WebResponseContent> PauseAndResumeJobExample(long ruleId)
        {
            try
            {
                _logger.LogInformation("开始暂停和恢复定时任务示例，规则ID: {RuleId}", ruleId);

                // 暂停任务
                var pauseResult = await _schedulerService.PauseAlertRuleJobAsync(ruleId);
                _logger.LogInformation("暂停任务结果: {Result}", pauseResult.Message);

                // 等待一段时间
                await Task.Delay(5000);

                // 恢复任务
                var resumeResult = await _schedulerService.ResumeAlertRuleJobAsync(ruleId);
                _logger.LogInformation("恢复任务结果: {Result}", resumeResult.Message);

                return new WebResponseContent().OK("暂停和恢复任务示例完成", new
                {
                    PauseResult = pauseResult,
                    ResumeResult = resumeResult
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "暂停和恢复定时任务示例时发生异常，规则ID: {RuleId}", ruleId);
                return new WebResponseContent().Error($"暂停和恢复任务异常: {ex.Message}");
            }
        }
    }
}

/*
预警功能使用说明：

1. 预警规则配置说明：
   - RuleName: 规则名称，用于标识预警规则
   - AlertPage: 预警页面，实际是数据库表名
   - FieldName: 预警字段，必须是日期类型字段
   - DayCount: 阈值天数，预警字段+阈值天数与当前日期比较
   - FinishStatusField: 完成状态字段，用于判断记录是否已完成
   - ConditionType: 完成判定方式，支持"是否有值判断"和"根据完成判定值来判断"
   - ConditionValue: 完成判定值，当ConditionType为"根据完成判定值来判断"时使用
   - ResponsiblePersonLoginName: 责任人登录名，支持多个用逗号分隔
   - PushInterval: 推送周期，使用Cron表达式
   - TriggerOA: 是否触发OA流程，1=是，0=否

2. Cron表达式示例：
   - "0 0 9 * * ?": 每天上午9点执行
   - "0 0 8,14 * * ?": 每天上午8点和下午2点执行
   - "0 0 9 * * MON-FRI": 工作日上午9点执行
   - "0 0/30 * * * ?": 每30分钟执行一次

3. 完成判定方式说明：
   - "是否有值判断": 如果完成状态字段有值（不为空），则认为已完成
   - "根据完成判定值来判断": 如果完成状态字段等于完成判定值，则认为已完成

4. API接口使用：
   - POST /api/AlertRules/execute-all: 执行所有预警规则
   - POST /api/AlertRules/execute/{ruleId}: 执行指定预警规则
   - POST /api/AlertRules/test/{ruleId}: 测试预警规则（不发送通知）
   - POST /api/AlertRules/check-cron: 检查Cron表达式

5. 注意事项：
   - 预警字段必须是日期类型
   - 推送周期使用标准Cron表达式
   - 责任人登录名必须是有效的OA用户登录名
   - 定时任务会自动创建，也可以手动管理
*/
