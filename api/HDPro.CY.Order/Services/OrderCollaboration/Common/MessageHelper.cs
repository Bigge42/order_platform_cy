using HDPro.Entity.DomainModels;
using HDPro.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HDPro.CY.Order.Services.OrderCollaboration.Common
{
    /// <summary>
    /// 消息帮助类
    /// </summary>
    public static class MessageHelper
    {
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns>用户信息元组 (LoginName, DisplayName)</returns>
        public static (string LoginName, string DisplayName) GetCurrentUserInfo()
        {
            var currentUser = HDPro.Core.ManageUser.UserContext.Current;
            var loginName = currentUser?.UserInfo?.UserName ?? BusinessConstants.DefaultConfig.SystemUser;
            var displayName = currentUser?.UserInfo?.UserTrueName ?? BusinessConstants.DefaultConfig.SystemUserName;
            return (loginName, displayName);
        }

        /// <summary>
        /// 确定消息接收者列表
        /// </summary>
        /// <param name="assignedPerson">指定负责人</param>
        /// <param name="defaultPerson">默认负责人</param>
        /// <param name="entityId">实体ID（用于日志）</param>
        /// <param name="entityType">实体类型（用于日志）</param>
        /// <returns>接收者登录名列表</returns>
        public static List<string> DetermineReceivers(string assignedPerson, string defaultPerson, object entityId, string entityType)
        {
            var receivers = new List<string>();
            
            // 如果指定负责人存在，只推送给指定负责人
            if (!string.IsNullOrEmpty(assignedPerson))
            {
                receivers.Add(assignedPerson);
                HDLogHelper.Log($"{entityType}_AssignedReceiver", 
                    $"{entityType}通知发送给指定负责人 - {entityType}ID: {entityId}, 指定负责人: {assignedPerson}", 
                    BusinessConstants.LogCategory.OrderCollaboration);
                return receivers;
            }
            
            // 如果没有指定负责人，则推送给默认负责人
            if (!string.IsNullOrEmpty(defaultPerson))
            {
                receivers.Add(defaultPerson);
                HDLogHelper.Log($"{entityType}_DefaultReceiver", 
                    $"{entityType}通知发送给默认负责人 - {entityType}ID: {entityId}, 默认负责人: {defaultPerson}", 
                    BusinessConstants.LogCategory.OrderCollaboration);
                return receivers;
            }

            // 如果没有找到任何负责人，使用默认的管理员账号
            receivers.Add(BusinessConstants.DefaultConfig.DefaultAdminUser);
            HDLogHelper.Log($"{entityType}_AdminReceiver", 
                $"{entityType}通知未找到任何负责人，使用默认管理员账号 - {entityType}ID: {entityId}", 
                BusinessConstants.LogCategory.OrderCollaboration);

            return receivers;
        }

        /// <summary>
        /// 构建催单OA消息内容
        /// </summary>
        /// <param name="urgentOrder">催单实体</param>
        /// <param name="senderName">发送者名称</param>
        /// <returns>消息内容</returns>
        public static string BuildUrgentOrderMessage(OCP_UrgentOrder urgentOrder, string senderName)
        {
            var timeUnit = GetTimeUnitText(urgentOrder.TimeUnit ?? 1);
            var urgencyText = GetUrgencyLevelText(urgentOrder.UrgencyLevel);
            
            return $"【{BusinessConstants.MessageType.UrgentOrder}】\n\n" +
                   $"催单ID：{urgentOrder.UrgentOrderID}\n" +
                   $"单据编号：{urgentOrder.BillNo ?? "未设置"}\n" +
                   $"业务类型：{GetBusinessTypeText(urgentOrder.BusinessType)}\n" +
                   $"紧急等级：{urgencyText}\n" +
                   $"催单内容：{urgentOrder.UrgentContent ?? "无"}\n" +
                   $"要求回复时间：{urgentOrder.AssignedReplyTime ?? 0}{timeUnit}\n" +
                   $"创建时间：{urgentOrder.CreateDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未知"}\n" +
                   $"催单人：{senderName}\n\n" +
                   $"请及时处理并回复相关信息。";
        }

        /// <summary>
        /// 构建催单SignalR消息标题
        /// </summary>
        /// <param name="urgentOrder">催单实体</param>
        /// <returns>SignalR消息标题</returns>
        public static string BuildUrgentOrderSignalRTitle(OCP_UrgentOrder urgentOrder)
        {
            var businessTypeText = GetBusinessTypeText(urgentOrder.BusinessType);
            var urgencyText = GetUrgencyLevelText(urgentOrder.UrgencyLevel);
            
            return $"【催单通知】{businessTypeText}订单催单 - {urgencyText}";
        }

        /// <summary>
        /// 构建催单SignalR消息简短内容
        /// </summary>
        /// <param name="urgentOrder">催单实体</param>
        /// <param name="senderName">发送者名称</param>
        /// <returns>SignalR消息简短内容</returns>
        public static string BuildUrgentOrderSignalRContent(OCP_UrgentOrder urgentOrder, string senderName)
        {
            var businessTypeText = GetBusinessTypeText(urgentOrder.BusinessType);
            var urgencyText = GetUrgencyLevelText(urgentOrder.UrgencyLevel);
            var timeUnit = GetTimeUnitText(urgentOrder.TimeUnit ?? 1);
            
            return $"单据编号：{urgentOrder.BillNo ?? "未设置"} | " +
                   $"业务类型：{businessTypeText} | " +
                   $"紧急等级：{urgencyText} | " +
                   $"要求回复：{urgentOrder.AssignedReplyTime ?? 0}{timeUnit} | " +
                   $"催单人：{senderName}";
        }

        /// <summary>
        /// 构建协商OA消息内容
        /// </summary>
        /// <param name="negotiation">协商实体</param>
        /// <param name="senderName">发送者名称</param>
        /// <returns>消息内容</returns>
        public static string BuildNegotiationMessage(OCP_Negotiation negotiation, string senderName)
        {
            return $"【{BusinessConstants.MessageType.Negotiation}】\n\n" +
                   $"协商ID：{negotiation.NegotiationID}\n" +
                   $"单据编号：{negotiation.BillNo ?? "未设置"}\n" +
                   $"协商类型：{negotiation.NegotiationType ?? "未设置"}\n" +
                   $"协商内容：{negotiation.NegotiationContent ?? "无"}\n" +
                   $"协商原因：{negotiation.NegotiationReason ?? "无"}\n" +
                   $"交货日期：{negotiation.DeliveryDate?.ToString("yyyy-MM-dd") ?? "未设置"}\n" +
                   $"协商日期：{negotiation.NegotiationDate?.ToString("yyyy-MM-dd") ?? "未设置"}\n" +
                   $"事项说明：{negotiation.MatterDescription ?? "无"}\n" +
                   $"创建时间：{negotiation.CreateDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未知"}\n" +
                   $"协商人：{senderName}\n\n" +
                   $"请及时处理并回复相关信息。";
        }

        /// <summary>
        /// 构建协商SignalR消息标题
        /// </summary>
        /// <param name="negotiation">协商实体</param>
        /// <returns>SignalR消息标题</returns>
        public static string BuildNegotiationSignalRTitle(OCP_Negotiation negotiation)
        {
            var businessTypeText = GetBusinessTypeText(negotiation.BusinessType);
            var negotiationTypeText = negotiation.NegotiationType ?? "协商";
            
            return $"【协商通知】{businessTypeText}订单{negotiationTypeText}";
        }

        /// <summary>
        /// 构建协商SignalR消息简短内容
        /// </summary>
        /// <param name="negotiation">协商实体</param>
        /// <param name="senderName">发送者名称</param>
        /// <returns>SignalR消息简短内容</returns>
        public static string BuildNegotiationSignalRContent(OCP_Negotiation negotiation, string senderName)
        {
            var businessTypeText = GetBusinessTypeText(negotiation.BusinessType);
            var negotiationTypeText = negotiation.NegotiationType ?? "协商";
            
            return $"单据编号：{negotiation.BillNo ?? "未设置"} | " +
                   $"业务类型：{businessTypeText} | " +
                   $"协商类型：{negotiationTypeText} | " +
                   $"交货日期：{negotiation.DeliveryDate?.ToString("yyyy-MM-dd") ?? "未设置"} | " +
                   $"协商人：{senderName}";
        }

        /// <summary>
        /// 获取业务类型文本
        /// </summary>
        /// <param name="businessType">业务类型代码</param>
        /// <returns>业务类型文本</returns>
        public static string GetBusinessTypeText(string businessType)
        {
            return businessType switch
            {
                BusinessConstants.BusinessType.OutSourcing => "委外",
                BusinessConstants.BusinessType.Purchase => "采购",
                BusinessConstants.BusinessType.Sales => "销售",
                BusinessConstants.BusinessType.Technology => "技术",
                BusinessConstants.BusinessType.Component => "部件",
                BusinessConstants.BusinessType.Metalwork => "金工",
                BusinessConstants.BusinessType.Assembly => "装配",
                BusinessConstants.BusinessType.Planning => "计划",
                _ => businessType ?? "未知"
            };
        }

        /// <summary>
        /// 获取紧急等级文本
        /// </summary>
        /// <param name="urgencyLevel">紧急等级</param>
        /// <returns>紧急等级文本</returns>
        public static string GetUrgencyLevelText(string urgencyLevel)
        {
            return urgencyLevel switch
            {
                "特急" => "特急",
                "A" => "A",
                "B" => "B",
                "C" => "C",
                _ => urgencyLevel ?? "一般"
            };
        }

        /// <summary>
        /// 获取时间单位文本
        /// </summary>
        /// <param name="timeUnit">时间单位</param>
        /// <returns>时间单位文本</returns>
        public static string GetTimeUnitText(int timeUnit)
        {
            return timeUnit switch
            {
                1 => "分钟",
                2 => "小时",
                3 => "天",
                _ => "小时"
            };
        }

        /// <summary>
        /// 获取催单类型文本（用于SRM接口stype参数）
        /// </summary>
        /// <param name="urgentType">催单类型</param>
        /// <returns>催单类型文本</returns>
        public static string GetUrgentTypeText(int? urgentType)
        {
            return urgentType switch
            {
                0 => "确认交期",
                1 => "确认进度",
                _ => "确认交期" // 默认值
            };
        }

        /// <summary>
        /// 记录集成操作开始日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="entityType">实体类型</param>
        /// <param name="entityId">实体ID</param>
        /// <param name="businessType">业务类型</param>
        /// <param name="operator">操作人</param>
        /// <param name="receivers">接收者列表</param>
        public static void LogIntegrationStart(string operationType, string entityType, object entityId, 
            string businessType, string @operator, List<string> receivers = null)
        {
            var receiversText = receivers != null ? $", 接收者: {string.Join(",", receivers)}" : "";
            HDLogHelper.Log($"{entityType}_{operationType}_Start", 
                $"开始{operationType} - {entityType}ID: {entityId}, 业务类型: {businessType}, 操作人: {@operator}{receiversText}", 
                BusinessConstants.LogCategory.OrderCollaboration);
        }

        /// <summary>
        /// 记录集成操作成功日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="entityType">实体类型</param>
        /// <param name="entityId">实体ID</param>
        /// <param name="billNo">单据号</param>
        /// <param name="businessType">业务类型</param>
        /// <param name="receivers">接收者列表</param>
        public static void LogIntegrationSuccess(string operationType, string entityType, object entityId, 
            string billNo, string businessType, List<string> receivers = null)
        {
            var receiversText = receivers != null ? $", 接收者: {string.Join(",", receivers)}" : "";
            HDLogHelper.Log($"{entityType}_{operationType}_Success", 
                $"{operationType}成功 - {entityType}ID: {entityId}, 单据号: {billNo}, 业务类型: {businessType}{receiversText}", 
                BusinessConstants.LogCategory.OrderCollaboration);
        }

        /// <summary>
        /// 记录集成操作失败日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="entityType">实体类型</param>
        /// <param name="entityId">实体ID</param>
        /// <param name="billNo">单据号</param>
        /// <param name="businessType">业务类型</param>
        /// <param name="errorMessage">错误信息</param>
        public static void LogIntegrationError(string operationType, string entityType, object entityId, 
            string billNo, string businessType, string errorMessage)
        {
            HDLogHelper.Log($"{entityType}_{operationType}_Error", 
                $"{operationType}失败 - {entityType}ID: {entityId}, 单据号: {billNo}, 业务类型: {businessType}, 错误信息: {errorMessage}", 
                BusinessConstants.LogCategory.OrderCollaboration);
        }

        /// <summary>
        /// 记录集成操作异常日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="entityType">实体类型</param>
        /// <param name="entityId">实体ID</param>
        /// <param name="businessType">业务类型</param>
        /// <param name="exception">异常信息</param>
        public static void LogIntegrationException(string operationType, string entityType, object entityId, 
            string businessType, Exception exception)
        {
            HDLogHelper.Log($"{entityType}_{operationType}_Exception", 
                $"{operationType}异常 - {entityType}ID: {entityId}, 业务类型: {businessType}, 错误详情: {exception.Message}", 
                BusinessConstants.LogCategory.OrderCollaboration);
        }
    }
} 