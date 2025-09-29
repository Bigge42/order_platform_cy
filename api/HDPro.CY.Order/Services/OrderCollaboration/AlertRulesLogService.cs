/*
 * 预警规则日志服务
 * 负责记录预警任务执行情况到Sys_QuartzLog表
 */
using HDPro.Core.DBManager;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OrderCollaboration
{
    /// <summary>
    /// 预警规则日志服务
    /// </summary>
    public class AlertRulesLogService : IDependency
    {
        private readonly ILogger<AlertRulesLogService> _logger;

        public AlertRulesLogService(ILogger<AlertRulesLogService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 记录任务开始执行
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <param name="ruleId">规则ID</param>
        /// <returns>日志ID</returns>
        public async Task<Guid> LogTaskStartAsync(string taskName, long? ruleId = null)
        {
            try
            {
                var logId = Guid.NewGuid();
                var taskId = ruleId.HasValue ? Guid.Parse($"00000000-0000-0000-0000-{ruleId.Value:D12}") : Guid.NewGuid();
                
                var log = new Sys_QuartzLog
                {
                    LogId = logId,
                    Id = taskId,
                    TaskName = taskName,
                    StratDate = DateTime.Now,
                    Result = null, // 执行中
                    CreateDate = DateTime.Now,
                    Creator = "AlertRulesSystem"
                };

                // 使用系统数据库上下文保存日志
                var sqlDapper = DBServerProvider.GetSqlDapper("SysDbContext");
                await sqlDapper.ExcuteNonQueryAsync(@"
                    INSERT INTO Sys_QuartzLog (
                        LogId, Id, TaskName, StratDate, Result, CreateDate, Creator
                    ) VALUES (
                        @LogId, @Id, @TaskName, @StratDate, @Result, @CreateDate, @Creator
                    )", log);

                _logger.LogInformation("预警任务开始执行记录已保存 - 任务: {TaskName}, 日志ID: {LogId}", taskName, logId);
                return logId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录预警任务开始执行时发生异常 - 任务: {TaskName}", taskName);
                return Guid.Empty;
            }
        }

        /// <summary>
        /// 记录任务执行完成
        /// </summary>
        /// <param name="logId">日志ID</param>
        /// <param name="success">是否成功</param>
        /// <param name="responseContent">返回内容</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public async Task LogTaskCompleteAsync(Guid logId, bool success, string responseContent = null, string errorMsg = null)
        {
            try
            {
                if (logId == Guid.Empty)
                {
                    return;
                }

                var endDate = DateTime.Now;
                
                // 先获取开始时间计算耗时
                var sqlDapper = DBServerProvider.GetSqlDapper("SysDbContext");
                var startDateResult = await sqlDapper.QueryFirstAsync<object>(
                    "SELECT ISNULL(StratDate, GETDATE()) FROM Sys_QuartzLog WHERE LogId = @LogId",
                    new { LogId = logId });
                var startDate = Convert.ToDateTime(startDateResult);

                var elapsedTime = (int)(endDate - startDate).TotalSeconds;

                // 更新日志记录
                await sqlDapper.ExcuteNonQueryAsync(@"
                    UPDATE Sys_QuartzLog SET
                        EndDate = @EndDate,
                        ElapsedTime = @ElapsedTime,
                        Result = @Result,
                        ResponseContent = @ResponseContent,
                        ErrorMsg = @ErrorMsg,
                        ModifyDate = @ModifyDate,
                        Modifier = @Modifier
                    WHERE LogId = @LogId", new
                {
                    LogId = logId,
                    EndDate = endDate,
                    ElapsedTime = elapsedTime,
                    Result = success ? 1 : 0,
                    ResponseContent = responseContent?.Length > 4000 ? responseContent.Substring(0, 4000) : responseContent,
                    ErrorMsg = errorMsg?.Length > 4000 ? errorMsg.Substring(0, 4000) : errorMsg,
                    ModifyDate = endDate,
                    Modifier = "AlertRulesSystem"
                });

                _logger.LogInformation("预警任务执行完成记录已更新 - 日志ID: {LogId}, 成功: {Success}, 耗时: {ElapsedTime}秒", 
                    logId, success, elapsedTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录预警任务执行完成时发生异常 - 日志ID: {LogId}", logId);
            }
        }

        /// <summary>
        /// 记录任务执行异常
        /// </summary>
        /// <param name="logId">日志ID</param>
        /// <param name="exception">异常信息</param>
        /// <returns></returns>
        public async Task LogTaskExceptionAsync(Guid logId, Exception exception)
        {
            try
            {
                var errorMsg = $"{exception.Message}\n{exception.StackTrace}";
                await LogTaskCompleteAsync(logId, false, null, errorMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录预警任务异常时发生异常 - 日志ID: {LogId}", logId);
            }
        }

        /// <summary>
        /// 清理过期日志
        /// </summary>
        /// <param name="daysToKeep">保留天数，默认30天</param>
        /// <returns></returns>
        public async Task CleanupOldLogsAsync(int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var sqlDapper = DBServerProvider.GetSqlDapper("SysDbContext");
                
                var deletedCount = await sqlDapper.ExcuteNonQueryAsync(@"
                    DELETE FROM Sys_QuartzLog
                    WHERE TaskName LIKE '%预警%'
                    AND CreateDate < @CutoffDate",
                    new { CutoffDate = cutoffDate });

                _logger.LogInformation("清理预警任务过期日志完成，删除 {DeletedCount} 条记录", deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理预警任务过期日志时发生异常");
            }
        }
    }
}
