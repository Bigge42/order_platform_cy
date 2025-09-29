/*
 * 预警规则初始化器
 * 用于系统启动时初始化预警规则的定时任务
 */
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OrderCollaboration
{
    /// <summary>
    /// 预警规则初始化后台服务
    /// </summary>
    public class AlertRulesInitializer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AlertRulesInitializer> _logger;

        public AlertRulesInitializer(
            IServiceProvider serviceProvider,
            ILogger<AlertRulesInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// 执行后台服务
        /// </summary>
        /// <param name="stoppingToken">取消令牌</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 等待系统完全启动
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            try
            {
                _logger.LogInformation("开始初始化预警规则定时任务");

                using var scope = _serviceProvider.CreateScope();
                var schedulerService = scope.ServiceProvider.GetService<AlertRulesSchedulerService>();
                
                if (schedulerService != null)
                {
                    var result = await schedulerService.InitializeAllAlertRuleJobsAsync();
                    
                    if (result.Status)
                    {
                        _logger.LogInformation("预警规则定时任务初始化成功: {Message}", result.Message);
                    }
                    else
                    {
                        _logger.LogError("预警规则定时任务初始化失败: {Message}", result.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("未找到AlertRulesSchedulerService服务");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化预警规则定时任务时发生异常");
            }
        }
    }
}
