/*
 * OA消息集成使用示例
 * 展示如何在各种业务场景中使用OA集成服务发送消息
 */
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IServices.OA;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OA
{
    /// <summary>
    /// OA消息集成使用示例
    /// 展示如何在不同业务场景中发送OA消息
    /// </summary>
    public class OAMessageUsageExample : IDependency
    {
        private readonly IOAIntegrationService _oaIntegrationService;
        private readonly ILogger<OAMessageUsageExample> _logger;

        public OAMessageUsageExample(
            IOAIntegrationService oaIntegrationService,
            ILogger<OAMessageUsageExample> logger)
        {
            _oaIntegrationService = oaIntegrationService;
            _logger = logger;
        }

        /// <summary>
        /// 示例1：催单通知消息
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="supplierName">供应商名称</param>
        /// <param name="urgencyLevel">紧急程度</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SendUrgentOrderNotificationAsync(
            string senderLoginName,
            string orderNo,
            string supplierName,
            string urgencyLevel,
            List<string> receiverLoginNames)
        {
            try
            {
                var content = $"【催单通知】\n\n" +
                             $"订单号：{orderNo}\n" +
                             $"供应商：{supplierName}\n" +
                             $"紧急程度：{urgencyLevel}\n" +
                             $"请尽快处理此催单，确保按时交付。\n\n" +
                             $"发送时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                _logger.LogInformation($"发送催单股份OA通知 - 订单号: {orderNo}, 接收者: {string.Join(",", receiverLoginNames)}");

                return await _oaIntegrationService.GetShareholderTokenAndSendMessageAsync(
                    senderLoginName,
                    receiverLoginNames,
                    content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送催单通知异常 - 订单号: {orderNo}");
                return new WebResponseContent().Error($"发送催单通知异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例2：订单状态变更通知
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="oldStatus">原状态</param>
        /// <param name="newStatus">新状态</param>
        /// <param name="changeReason">变更原因</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SendOrderStatusChangeNotificationAsync(
            string senderLoginName,
            string orderNo,
            string oldStatus,
            string newStatus,
            string changeReason,
            List<string> receiverLoginNames)
        {
            try
            {
                var content = $"【订单状态变更通知】\n\n" +
                             $"订单号：{orderNo}\n" +
                             $"状态变更：{oldStatus} → {newStatus}\n" +
                             $"变更原因：{changeReason}\n" +
                             $"请及时关注订单状态变化。\n\n" +
                             $"变更时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                _logger.LogInformation($"发送订单状态变更通知 - 订单号: {orderNo}, 状态: {oldStatus} → {newStatus}");

                return await _oaIntegrationService.GetTokenAndSendMessageAsync(
                    senderLoginName,
                    receiverLoginNames,
                    content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送订单状态变更通知异常 - 订单号: {orderNo}");
                return new WebResponseContent().Error($"发送状态变更通知异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例3：交期协商通知
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="originalDate">原交期</param>
        /// <param name="proposedDate">建议交期</param>
        /// <param name="negotiationReason">协商原因</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SendDeliveryNegotiationNotificationAsync(
            string senderLoginName,
            string orderNo,
            DateTime originalDate,
            DateTime proposedDate,
            string negotiationReason,
            List<string> receiverLoginNames)
        {
            try
            {
                var content = $"【交期协商通知】\n\n" +
                             $"订单号：{orderNo}\n" +
                             $"原交期：{originalDate:yyyy-MM-dd}\n" +
                             $"建议交期：{proposedDate:yyyy-MM-dd}\n" +
                             $"协商原因：{negotiationReason}\n" +
                             $"请及时回复协商结果。\n\n" +
                             $"发起时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                _logger.LogInformation($"发送交期协商股份OA通知 - 订单号: {orderNo}, 原交期: {originalDate:yyyy-MM-dd}, 建议交期: {proposedDate:yyyy-MM-dd}");

                return await _oaIntegrationService.GetShareholderTokenAndSendMessageAsync(
                    senderLoginName,
                    receiverLoginNames,
                    content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送交期协商通知异常 - 订单号: {orderNo}");
                return new WebResponseContent().Error($"发送交期协商通知异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例4：异常处理通知
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="exceptionType">异常类型</param>
        /// <param name="exceptionDescription">异常描述</param>
        /// <param name="suggestedAction">建议处理措施</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SendExceptionHandlingNotificationAsync(
            string senderLoginName,
            string orderNo,
            string exceptionType,
            string exceptionDescription,
            string suggestedAction,
            List<string> receiverLoginNames)
        {
            try
            {
                var content = $"【异常处理通知】\n\n" +
                             $"订单号：{orderNo}\n" +
                             $"异常类型：{exceptionType}\n" +
                             $"异常描述：{exceptionDescription}\n" +
                             $"建议措施：{suggestedAction}\n" +
                             $"请紧急处理此异常。\n\n" +
                             $"发现时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                _logger.LogInformation($"发送异常处理通知 - 订单号: {orderNo}, 异常类型: {exceptionType}");

                return await _oaIntegrationService.GetTokenAndSendMessageAsync(
                    senderLoginName,
                    receiverLoginNames,
                    content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送异常处理通知异常 - 订单号: {orderNo}");
                return new WebResponseContent().Error($"发送异常处理通知异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例5：进度更新通知
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="currentProgress">当前进度</param>
        /// <param name="progressDescription">进度描述</param>
        /// <param name="nextMilestone">下一里程碑</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SendProgressUpdateNotificationAsync(
            string senderLoginName,
            string orderNo,
            string currentProgress,
            string progressDescription,
            string nextMilestone,
            List<string> receiverLoginNames)
        {
            try
            {
                var content = $"【进度更新通知】\n\n" +
                             $"订单号：{orderNo}\n" +
                             $"当前进度：{currentProgress}\n" +
                             $"进度说明：{progressDescription}\n" +
                             $"下一里程碑：{nextMilestone}\n" +
                             $"请关注订单进度变化。\n\n" +
                             $"更新时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                _logger.LogInformation($"发送进度更新通知 - 订单号: {orderNo}, 当前进度: {currentProgress}");

                return await _oaIntegrationService.GetTokenAndSendMessageAsync(
                    senderLoginName,
                    receiverLoginNames,
                    content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送进度更新通知异常 - 订单号: {orderNo}");
                return new WebResponseContent().Error($"发送进度更新通知异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例6：使用OA服务的便捷订单消息方法
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="orderType">订单类型</param>
        /// <param name="messageType">消息类型</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="customContent">自定义内容</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SendOrderMessageAsync(
            string senderLoginName,
            string orderNo,
            string orderType,
            string messageType,
            List<string> receiverLoginNames,
            string customContent = null)
        {
            try
            {
                _logger.LogInformation($"使用便捷方法发送订单消息 - 订单号: {orderNo}, 消息类型: {messageType}");

                return await _oaIntegrationService.SendOrderMessageAsync(
                    senderLoginName,
                    orderNo,
                    orderType,
                    messageType,
                    receiverLoginNames,
                    customContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送订单消息异常 - 订单号: {orderNo}");
                return new WebResponseContent().Error($"发送订单消息异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例7：批量发送通知
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="notifications">通知列表</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SendBatchNotificationsAsync(
            string senderLoginName,
            List<NotificationInfo> notifications)
        {
            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            try
            {
                foreach (var notification in notifications)
                {
                    try
                    {
                        var result = await _oaIntegrationService.GetShareholderTokenAndSendMessageAsync(
                            senderLoginName,
                            notification.ReceiverLoginNames,
                            notification.Content);

                        if (result.Status)
                        {
                            successCount++;
                            _logger.LogInformation($"批量通知发送成功 - 标题: {notification.Title}");
                        }
                        else
                        {
                            failureCount++;
                            _logger.LogError($"批量通知发送失败 - 标题: {notification.Title}, 错误: {result.Message}");
                        }

                        results.Add(new 
                        { 
                            Title = notification.Title, 
                            Status = result.Status, 
                            Message = result.Message 
                        });
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex, $"批量通知发送异常 - 标题: {notification.Title}");
                        results.Add(new 
                        { 
                            Title = notification.Title, 
                            Status = false, 
                            Message = ex.Message 
                        });
                    }
                }

                _logger.LogInformation($"批量通知发送完成 - 总数: {notifications.Count}, 成功: {successCount}, 失败: {failureCount}");

                return new WebResponseContent().OK("批量发送完成", new
                {
                    TotalCount = notifications.Count,
                    SuccessCount = successCount,
                    FailureCount = failureCount,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量发送通知异常");
                return new WebResponseContent().Error($"批量发送异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例6：管理员账号处理演示
        /// 展示如何处理管理员账号（admin、cyadmin）自动映射为OA登录名
        /// </summary>
        /// <param name="orderNo">订单号</param>
        /// <param name="urgencyLevel">紧急程度</param>
        /// <returns></returns>
        public async Task<WebResponseContent> DemoAdminAccountHandlingAsync(
            string orderNo,
            string urgencyLevel)
        {
            try
            {
                // 演示管理员账号作为发送者的情况
                _logger.LogInformation("演示管理员账号处理 - 发送者为管理员账号");

                var content = $"【管理员催单通知】\n\n" +
                             $"订单号：{orderNo}\n" +
                             $"紧急程度：{urgencyLevel}\n" +
                             $"此消息由系统管理员发送，请优先处理。\n\n" +
                             $"发送时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                // 接收者列表，包含普通用户和管理员账号
                var receiverLoginNames = new List<string>
                {
                    "user001",      // 普通用户
                    "admin",        // 管理员账号，会自动映射为 015206
                    "cyadmin",      // CY管理员账号，会自动映射为 015206
                    "supplier01"    // 供应商用户
                };

                _logger.LogInformation("发送催单通知（管理员演示） - 订单号: {OrderNo}, 发送者: admin（会映射为015206）, 接收者: {Receivers}",
                    orderNo, string.Join(",", receiverLoginNames));

                // 使用 admin 作为发送者（会自动映射为015206）
                var result = await _oaIntegrationService.GetTokenAndSendMessageAsync(
                    "admin",                // 发送者：管理员账号，会自动映射为 015206
                    receiverLoginNames,     // 接收者：列表中的 admin 和 cyadmin 也会自动映射
                    content);

                if (result.Status)
                {
                    _logger.LogInformation("管理员账号演示成功 - 管理员账号已自动映射为OA登录名");
                }
                else
                {
                    _logger.LogError("管理员账号演示失败: {ErrorMessage}", result.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"管理员账号处理演示异常 - 订单号: {orderNo}");
                return new WebResponseContent().Error($"管理员账号处理演示异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例7：批量发送管理员通知
        /// 展示如何批量发送来自管理员的通知消息
        /// </summary>
        /// <param name="notifications">通知信息列表</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SendBatchAdminNotificationsAsync(
            List<AdminNotificationInfo> notifications)
        {
            try
            {
                if (notifications == null || !notifications.Any())
                {
                    return new WebResponseContent().Error("通知列表不能为空");
                }

                var results = new List<object>();
                var successCount = 0;
                var failedCount = 0;

                _logger.LogInformation("开始批量发送管理员通知，共 {Count} 条", notifications.Count);

                foreach (var notification in notifications)
                {
                    try
                    {
                        var content = $"【管理员通知】\n\n" +
                                     $"标题：{notification.Title}\n" +
                                     $"内容：{notification.Content}\n" +
                                     $"相关订单：{notification.OrderNo ?? "无"}\n\n" +
                                     $"此消息由系统管理员发送，请及时处理。\n" +
                                     $"发送时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                        // 使用管理员账号发送（会自动映射）
                        var result = await _oaIntegrationService.GetTokenAndSendMessageAsync(
                            notification.SenderAdminType ?? "admin",  // 支持 admin 或 cyadmin
                            notification.ReceiverLoginNames,
                            content);

                        if (result.Status)
                        {
                            successCount++;
                            results.Add(new
                            {
                                Title = notification.Title,
                                Status = "成功",
                                Message = "发送成功"
                            });
                        }
                        else
                        {
                            failedCount++;
                            results.Add(new
                            {
                                Title = notification.Title,
                                Status = "失败",
                                Message = result.Message
                            });
                        }

                        _logger.LogInformation("管理员通知发送完成 - 标题: {Title}, 状态: {Status}",
                            notification.Title, result.Status ? "成功" : "失败");
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        var error = $"发送通知异常: {ex.Message}";
                        results.Add(new
                        {
                            Title = notification.Title,
                            Status = "异常",
                            Message = error
                        });

                        _logger.LogError(ex, "发送管理员通知异常 - 标题: {Title}", notification.Title);
                    }
                }

                _logger.LogInformation("批量管理员通知发送完成 - 成功: {SuccessCount}, 失败: {FailedCount}",
                    successCount, failedCount);

                return new WebResponseContent(true)
                {
                    Message = $"批量发送完成，成功 {successCount} 条，失败 {failedCount} 条",
                    Data = new
                    {
                        TotalCount = notifications.Count,
                        SuccessCount = successCount,
                        FailedCount = failedCount,
                        Results = results
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量发送管理员通知异常");
                return new WebResponseContent().Error($"批量发送管理员通知异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例8：快速发送管理员催单
        /// 提供快捷方式发送管理员催单消息
        /// </summary>
        /// <param name="adminType">管理员类型（admin 或 cyadmin）</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="supplierName">供应商名称</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="customMessage">自定义消息内容（可选）</param>
        /// <returns></returns>
        public async Task<WebResponseContent> QuickSendAdminUrgentOrderAsync(
            string adminType,
            string orderNo,
            string supplierName,
            List<string> receiverLoginNames,
            string customMessage = null)
        {
            try
            {
                // 验证管理员类型
                if (string.IsNullOrEmpty(adminType))
                {
                    adminType = "admin"; // 默认使用 admin
                }

                if (!adminType.Equals("admin", StringComparison.OrdinalIgnoreCase) &&
                    !adminType.Equals("cyadmin", StringComparison.OrdinalIgnoreCase))
                {
                    return new WebResponseContent().Error("管理员类型只能是 admin 或 cyadmin");
                }

                var content = customMessage ?? 
                             $"【管理员催单通知】\n\n" +
                             $"订单号：{orderNo}\n" +
                             $"供应商：{supplierName}\n" +
                             $"紧急程度：高\n" +
                             $"此订单已被系统管理员标记为催单，请立即处理并及时反馈处理进度。\n\n" +
                             $"如有疑问，请联系系统管理员。\n" +
                             $"发送时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                _logger.LogInformation("快速发送管理员催单股份OA - 管理员类型: {AdminType}, 订单号: {OrderNo}, 供应商: {SupplierName}",
                    adminType, orderNo, supplierName);

                return await _oaIntegrationService.GetShareholderTokenAndSendMessageAsync(
                    adminType,              // 会自动映射为 015206
                    receiverLoginNames,
                    content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "快速发送管理员催单异常 - 订单号: {OrderNo}", orderNo);
                return new WebResponseContent().Error($"快速发送管理员催单异常: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 通知信息模型
    /// </summary>
    public class NotificationInfo
    {
        /// <summary>
        /// 通知标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 通知内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 接收者登录名列表
        /// </summary>
        public List<string> ReceiverLoginNames { get; set; }

        /// <summary>
        /// 通知类型
        /// </summary>
        public string NotificationType { get; set; }

        /// <summary>
        /// 相关订单号
        /// </summary>
        public string OrderNo { get; set; }
    }

    /// <summary>
    /// 管理员通知信息模型
    /// </summary>
    public class AdminNotificationInfo
    {
        /// <summary>
        /// 通知标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 通知内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 接收者登录名列表
        /// </summary>
        public List<string> ReceiverLoginNames { get; set; }

        /// <summary>
        /// 发送者管理员类型（admin 或 cyadmin）
        /// </summary>
        public string SenderAdminType { get; set; }

        /// <summary>
        /// 相关订单号（可选）
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 通知类型
        /// </summary>
        public string NotificationType { get; set; }

        /// <summary>
        /// 优先级（高、中、低）
        /// </summary>
        public string Priority { get; set; }
    }
} 