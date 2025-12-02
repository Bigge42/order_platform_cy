/*
 *所有关于OCP_AlertRules类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_AlertRulesService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.Dapper;
using HDPro.Core.DBManager;
using Microsoft.Extensions.Logging;
using HDPro.CY.Order.IServices.OA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Reflection;
using Quartz;
using HDPro.CY.Order.Services.OrderCollaboration;
using HDPro.Core.Configuration;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_AlertRulesService
    {
        private readonly IOCP_AlertRulesRepository _repository;//访问数据库
        private readonly ILogger<OCP_AlertRulesService> _logger;
        private readonly IOAIntegrationService _oaIntegrationService;

        [ActivatorUtilitiesConstructor]
        public OCP_AlertRulesService(
            IOCP_AlertRulesRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OCP_AlertRulesService> logger,
            IOAIntegrationService oaIntegrationService
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _logger = logger;
            _oaIntegrationService = oaIntegrationService;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_AlertRules特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_AlertRules特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_AlertRules特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_AlertRules entity)
        {
            var response = base.ValidateCYOrderEntity(entity);

            // 在此处添加OCP_AlertRules特有的数据验证逻辑

            return response;
        }

        /// <summary>
        /// 执行所有预警规则检查
        /// </summary>
        /// <param name="logToQuartz">是否记录到Sys_QuartzLog表(默认true)</param>
        /// <returns>执行结果</returns>
        public async Task<WebResponseContent> ExecuteAllAlertRulesAsync(bool logToQuartz = true)
        {
            var response = new WebResponseContent();
            Guid logId = Guid.Empty;
            AlertRulesLogService logService = null;

            try
            {
                // 如果需要记录日志,初始化日志服务
                if (logToQuartz)
                {
                    logService = AutofacContainerModule.GetService<AlertRulesLogService>();
                    if (logService != null)
                    {
                        logId = await logService.LogTaskStartAsync("手动执行所有预警规则");
                    }
                }

                _logger.LogInformation("开始执行预警规则检查");

                // 获取所有启用的预警规则
                var alertRules = await GetActiveAlertRulesAsync();

                if (!alertRules.Any())
                {
                    _logger.LogInformation("没有找到启用的预警规则");
                    var msg = "没有启用的预警规则";

                    if (logService != null && logId != Guid.Empty)
                    {
                        await logService.LogTaskCompleteAsync(logId, true, msg);
                    }

                    return response.OK(msg);
                }

                var totalProcessed = 0;
                var totalAlerts = 0;

                foreach (var rule in alertRules)
                {
                    try
                    {
                        var alertCount = await ExecuteSingleAlertRuleAsync(rule);
                        totalAlerts += alertCount;
                        totalProcessed++;

                        _logger.LogInformation("预警规则 [{RuleName}] 执行完成，触发预警 {AlertCount} 条",
                            rule.RuleName, alertCount);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "执行预警规则 [{RuleName}] 时发生异常", rule.RuleName);
                    }
                }

                _logger.LogInformation("预警规则检查完成，处理规则 {ProcessedCount} 个，触发预警 {AlertCount} 条",
                    totalProcessed, totalAlerts);

                var resultMsg = $"预警检查完成，处理规则 {totalProcessed} 个，触发预警 {totalAlerts} 条";

                if (logService != null && logId != Guid.Empty)
                {
                    await logService.LogTaskCompleteAsync(logId, true, resultMsg);
                }

                return response.OK(resultMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行预警规则检查时发生异常");

                if (logService != null && logId != Guid.Empty)
                {
                    await logService.LogTaskExceptionAsync(logId, ex);
                }

                return response.Error($"预警检查异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有启用的预警规则
        /// </summary>
        /// <returns>预警规则列表</returns>
        private async Task<List<OCP_AlertRules>> GetActiveAlertRulesAsync()
        {
            return await _repository.FindAsIQueryable(x => true)
                .Where(x => !string.IsNullOrEmpty(x.RuleName))
                .ToListAsync();
        }

        /// <summary>
        /// 执行单个预警规则
        /// </summary>
        /// <param name="rule">预警规则</param>
        /// <returns>触发的预警数量</returns>
        private async Task<int> ExecuteSingleAlertRuleAsync(OCP_AlertRules rule)
        {
            try
            {
                _logger.LogInformation("开始执行预警规则: {RuleName}, 表名: {TableName}, 字段: {FieldName}",
                    rule.RuleName, rule.AlertPage, rule.FieldName);

                // 构建查询SQL
                var sql = BuildAlertQuerySql(rule);

                if (string.IsNullOrEmpty(sql))
                {
                    _logger.LogWarning("预警规则 [{RuleName}] 构建SQL失败", rule.RuleName);
                    return 0;
                }

                // 执行查询获取超期数据
                var overdueRecords = await ExecuteAlertQueryAsync(sql);

                if (!overdueRecords.Any())
                {
                    _logger.LogInformation("预警规则 [{RuleName}] 没有找到超期数据", rule.RuleName);
                    return 0;
                }

                _logger.LogInformation("预警规则 [{RuleName}] 找到超期数据 {Count} 条",
                    rule.RuleName, overdueRecords.Count);

                // 发送预警通知
                var alertCount = await SendAlertNotificationsAsync(rule, overdueRecords);

                return alertCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行预警规则 [{RuleName}] 时发生异常", rule.RuleName);
                throw;
            }
        }

        /// <summary>
        /// 构建预警查询SQL
        /// </summary>
        /// <param name="rule">预警规则</param>
        /// <returns>查询SQL</returns>
        private string BuildAlertQuerySql(OCP_AlertRules rule)
        {
            try
            {
                var sql = new StringBuilder();
                var currentDate = DateTime.Now.Date;

                // 基础查询
                sql.AppendLine($"SELECT * FROM [{rule.AlertPage}]");
                sql.AppendLine("WHERE 1=1");

                // 确保预警字段不为空且是有效日期
                sql.AppendLine($"AND [{rule.FieldName}] IS NOT NULL");
                sql.AppendLine($"AND ISDATE([{rule.FieldName}]) = 1");

                // 日期条件：预警字段 + 阈值天数 < 当前日期（表示超期）
                // 使用TRY_CAST确保安全的日期转换
                sql.AppendLine($"AND DATEADD(DAY, {rule.DayCount}, TRY_CAST([{rule.FieldName}] AS DATE)) < CAST(GETDATE() AS DATE)");

                // 完成状态过滤
                if (!string.IsNullOrEmpty(rule.FinishStatusField))
                {
                    if (rule.ConditionType =="1")// 1 "是否有值判断")
                    {
                        // 如果是"是否有值判断"，则过滤掉已有值的记录（即未完成的）
                        sql.AppendLine($"AND ([{rule.FinishStatusField}] IS NULL OR [{rule.FinishStatusField}] = '')");
                    }
                    else if (rule.ConditionType =="2" && !string.IsNullOrEmpty(rule.ConditionValue))// 2 "根据完成判定值来判断"
                    {
                        // 如果是"根据完成判定值来判断"，则过滤掉等于完成判定值的记录
                        sql.AppendLine($"AND [{rule.FinishStatusField}] != '{rule.ConditionValue.Replace("'", "''")}'");
                    }
                }

                var finalSql = sql.ToString();
                _logger.LogInformation("构建的预警查询SQL: {Sql}", finalSql);

                return finalSql;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "构建预警查询SQL时发生异常，规则: {RuleName}", rule.RuleName);
                return null;
            }
        }

        /// <summary>
        /// 执行预警查询
        /// </summary>
        /// <param name="sql">查询SQL</param>
        /// <returns>查询结果</returns>
        private async Task<List<Dictionary<string, object>>> ExecuteAlertQueryAsync(string sql)
        {
            try
            {
                var sqlDapper = DBServerProvider.GetSqlDapper<OCP_AlertRules>();
                var result = await sqlDapper.QueryListAsync<dynamic>(sql, null);

                // 转换为字典列表以便后续处理
                var records = new List<Dictionary<string, object>>();

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        // 跳过null项
                        if (item == null)
                        {
                            continue;
                        }

                        var dict = new Dictionary<string, object>();

                        // DapperRow实现了IDictionary<string, object>接口,可以直接转换
                        if (item is System.Collections.Generic.IDictionary<string, object> dapperDict)
                        {
                            // 直接从DapperRow字典中复制数据,这是最高效的方式
                            foreach (var kvp in dapperDict)
                            {
                                dict[kvp.Key] = kvp.Value;
                            }
                        }
                        else
                        {
                            // 如果不是IDictionary,使用反射获取属性(备用方案)
                            var itemType = item.GetType();
                            var properties = itemType.GetProperties();

                            foreach (var prop in properties)
                            {
                                try
                                {
                                    var value = prop.GetValue(item);
                                    dict[prop.Name] = value;
                                }
                                catch
                                {
                                    dict[prop.Name] = null;
                                }
                            }
                        }

                        records.Add(dict);
                    }
                }

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行预警查询时发生异常，SQL: {Sql}", sql);
                throw;
            }
        }

        /// <summary>
        /// 发送预警通知
        /// </summary>
        /// <param name="rule">预警规则</param>
        /// <param name="overdueRecords">超期记录</param>
        /// <returns>发送的通知数量</returns>
        private async Task<int> SendAlertNotificationsAsync(OCP_AlertRules rule, List<Dictionary<string, object>> overdueRecords)
        {
            try
            {
                if (string.IsNullOrEmpty(rule.ResponsiblePersonLoginName))
                {
                    _logger.LogWarning("预警规则 [{RuleName}] 未配置责任人登录名，跳过通知发送", rule.RuleName);
                    return 0;
                }

                // 构建预警消息内容
                var content = BuildAlertMessage(rule, overdueRecords);

                // 根据配置决定是否触发OA流程
                if (rule.TriggerOA == 1)
                {
                    await SendOANotificationAsync(rule, content);
                }
                else
                {
                    _logger.LogInformation("预警规则 [{RuleName}] 未启用OA流程触发", rule.RuleName);
                }

                return overdueRecords.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送预警通知时发生异常，规则: {RuleName}", rule.RuleName);
                throw;
            }
        }

        /// <summary>
        /// 构建预警消息内容
        /// </summary>
        /// <param name="rule">预警规则</param>
        /// <param name="overdueRecords">超期记录</param>
        /// <returns>消息内容</returns>
        private string BuildAlertMessage(OCP_AlertRules rule, List<Dictionary<string, object>> overdueRecords)
        {
            var message = new StringBuilder();
            message.AppendLine($"【{rule.RuleName}】预警通知");
            message.AppendLine();
            message.AppendLine($"预警时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            message.AppendLine($"预警页面：{rule.AlertPage}");
            message.AppendLine($"超期数量：{overdueRecords.Count} 条");
            message.AppendLine($"阈值天数：{rule.DayCount} 天");
            message.AppendLine();

            if (overdueRecords.Count <= 10)
            {
                message.AppendLine("超期明细：");
                for (int i = 0; i < overdueRecords.Count; i++)
                {
                    var record = overdueRecords[i];
                    message.AppendLine($"{i + 1}. ");

                    // 显示关键字段信息
                    foreach (var kvp in record.Take(5)) // 只显示前5个字段
                    {
                        message.AppendLine($"   {kvp.Key}: {kvp.Value}");
                    }
                    message.AppendLine();
                }
            }
            else
            {
                message.AppendLine($"超期记录较多（{overdueRecords.Count} 条），请登录系统查看详细信息。");
            }

            message.AppendLine("请及时处理相关事项。");

            return message.ToString();
        }

        /// <summary>
        /// 发送OA通知
        /// </summary>
        /// <param name="rule">预警规则</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        private async Task SendOANotificationAsync(OCP_AlertRules rule, string content)
        {
            try
            {
                var receiverLoginNames = new List<string>();

                // 解析责任人登录名（支持多个，用逗号分隔）
                if (!string.IsNullOrEmpty(rule.ResponsiblePersonLoginName))
                {
                    receiverLoginNames = rule.ResponsiblePersonLoginName
                        .Split(',', ';')
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();
                }

                if (!receiverLoginNames.Any())
                {
                    _logger.LogWarning("预警规则 [{RuleName}] 责任人登录名为空，无法发送OA通知", rule.RuleName);
                    return;
                }

                _logger.LogInformation("开始发送预警OA通知，规则: {RuleName}, 接收者: {Receivers}",
                    rule.RuleName, string.Join(",", receiverLoginNames));

                // 从配置文件获取系统发送者账号
                var senderLoginName = AppSetting.GetSettingString("ShareholderOA:SenderLoginName") ?? "system";

                var result = await _oaIntegrationService.GetShareholderTokenAndSendMessageAsync(
                    senderLoginName,
                    receiverLoginNames,
                    content);

                if (result.Status)
                {
                    _logger.LogInformation("预警OA通知发送成功，规则: {RuleName}, 接收者: {Receivers}",
                        rule.RuleName, string.Join(",", receiverLoginNames));
                }
                else
                {
                    _logger.LogError("预警OA通知发送失败，规则: {RuleName}, 错误: {Error}",
                        rule.RuleName, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送预警OA通知时发生异常，规则: {RuleName}", rule.RuleName);
            }
        }

        /// <summary>
        /// 根据Cron表达式检查是否应该执行预警
        /// </summary>
        /// <param name="cronExpression">Cron表达式</param>
        /// <returns>是否应该执行</returns>
        public bool ShouldExecuteAlert(string cronExpression)
        {
            try
            {
                if (string.IsNullOrEmpty(cronExpression))
                {
                    return false;
                }

                // 使用Quartz的Cron表达式解析
                var cron = new CronExpression(cronExpression);
                var now = DateTime.Now;

                // 检查当前时间是否匹配Cron表达式
                // 这里简化处理，实际应该由定时任务框架来调度
                var nextFireTime = cron.GetNextValidTimeAfter(now.AddMinutes(-1));
                var prevFireTime = cron.GetNextValidTimeAfter(now.AddMinutes(-2));

                // 如果下次执行时间在当前时间的1分钟内，认为应该执行
                return nextFireTime.HasValue &&
                       nextFireTime.Value.DateTime <= now.AddMinutes(1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析Cron表达式时发生异常: {CronExpression}", cronExpression);
                return false;
            }
        }

        /// <summary>
        /// 执行指定预警规则
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <param name="logToQuartz">是否记录到Sys_QuartzLog表(默认true)</param>
        /// <returns>执行结果</returns>
        public async Task<WebResponseContent> ExecuteAlertRuleByIdAsync(long ruleId, bool logToQuartz = true)
        {
            var response = new WebResponseContent();
            Guid logId = Guid.Empty;
            AlertRulesLogService logService = null;

            try
            {
                var rule = await _repository.FindAsyncFirst(x => x.ID == ruleId);
                if (rule == null)
                {
                    return response.Error($"未找到ID为 {ruleId} 的预警规则");
                }

                // 如果需要记录日志,初始化日志服务
                if (logToQuartz)
                {
                    logService = AutofacContainerModule.GetService<AlertRulesLogService>();
                    if (logService != null)
                    {
                        logId = await logService.LogTaskStartAsync($"手动执行预警规则-{ruleId}", ruleId);
                    }
                }

                _logger.LogInformation("开始执行指定预警规则: {RuleName} (ID: {RuleId})", rule.RuleName, ruleId);

                var alertCount = await ExecuteSingleAlertRuleAsync(rule);

                var resultMsg = $"预警规则 [{rule.RuleName}] 执行完成，触发预警 {alertCount} 条";

                if (logService != null && logId != Guid.Empty)
                {
                    await logService.LogTaskCompleteAsync(logId, true, resultMsg);
                }

                return response.OK(resultMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行指定预警规则时发生异常，规则ID: {RuleId}", ruleId);

                if (logService != null && logId != Guid.Empty)
                {
                    await logService.LogTaskExceptionAsync(logId, ex);
                }

                return response.Error($"执行预警规则异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试预警规则（不发送通知，只返回查询结果）
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <returns>测试结果</returns>
        public async Task<WebResponseContent> TestAlertRuleAsync(long ruleId)
        {
            var response = new WebResponseContent();
            try
            {
                var rule = await _repository.FindAsyncFirst(x => x.ID == ruleId);
                if (rule == null)
                {
                    return response.Error($"未找到ID为 {ruleId} 的预警规则");
                }

                _logger.LogInformation("开始测试预警规则: {RuleName} (ID: {RuleId})", rule.RuleName, ruleId);

                // 构建查询SQL
                var sql = BuildAlertQuerySql(rule);
                if (string.IsNullOrEmpty(sql))
                {
                    return response.Error("构建查询SQL失败");
                }

                // 执行查询
                var overdueRecords = await ExecuteAlertQueryAsync(sql);

                var result = new
                {
                    RuleName = rule.RuleName,
                    AlertPage = rule.AlertPage,
                    FieldName = rule.FieldName,
                    DayCount = rule.DayCount,
                    OverdueCount = overdueRecords.Count,
                    QuerySql = sql,
                    SampleRecords = overdueRecords.Take(5).ToList() // 返回前5条记录作为示例
                };

                return response.OK("测试完成", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试预警规则时发生异常，规则ID: {RuleId}", ruleId);
                return response.Error($"测试预警规则异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新预警规则任务状态
        /// </summary>
        /// <param name="ruleId">规则ID</param>
        /// <param name="taskStatus">任务状态</param>
        /// <returns>更新结果</returns>
        public async Task<WebResponseContent> UpdateTaskStatusAsync(long ruleId, int taskStatus)
        {
            var response = new WebResponseContent();
            try
            {
                var rule = await _repository.FindAsyncFirst(x => x.ID == ruleId);
                if (rule == null)
                {
                    return response.Error("预警规则不存在");
                }

                rule.TaskStatus = taskStatus;
                var updateResult = _repository.Update(rule, new string[] { "TaskStatus" }, true);

                if (updateResult <= 0)
                {
                    return response.Error("更新任务状态失败");
                }

                _logger.LogInformation("预警规则任务状态更新成功，规则ID: {RuleId}, 新状态: {TaskStatus}", ruleId, taskStatus);
                return response.OK("任务状态更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新预警规则任务状态时发生异常，规则ID: {RuleId}", ruleId);
                return response.Error($"更新任务状态异常: {ex.Message}");
            }
        }
  }
}