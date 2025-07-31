/*
*所有关于OCP_Negotiation类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.Models;
namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_NegotiationService
    {
        /// <summary>
        /// 发送协商相关的OA消息（通用方法）
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="negotiationType">协商类型</param>
        /// <param name="negotiationContent">协商内容</param>
        /// <param name="customContent">自定义内容</param>
        /// <returns></returns>
        Task<WebResponseContent> SendNegotiationOAMessageAsync(
            string senderLoginName,
            List<string> receiverLoginNames,
            string orderNo,
            string negotiationType,
            string negotiationContent,
            string customContent = null);

        /// <summary>
        /// 发送交期协商OA通知（便捷方法）
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="originalDate">原交期</param>
        /// <param name="proposedDate">建议交期</param>
        /// <param name="negotiationReason">协商原因</param>
        /// <returns></returns>
        Task<WebResponseContent> SendDeliveryNegotiationOAMessageAsync(
            string senderLoginName,
            List<string> receiverLoginNames,
            string orderNo,
            System.DateTime originalDate,
            System.DateTime proposedDate,
            string negotiationReason);

        /// <summary>
        /// 获取协商消息统计数据
        /// </summary>
        /// <returns>协商统计数据</returns>
        Task<MessageStatisticsDto> GetNegotiationStatisticsAsync();

        /// <summary>
        /// 更新协商状态
        /// </summary>
        /// <param name="negotiationID">协商ID</param>
        /// <param name="newStatus">新状态</param>
        /// <returns>更新结果</returns>
        Task<WebResponseContent> UpdateNegotiationStatusAsync(long negotiationID, string newStatus);

        /// <summary>
        /// 批量更新协商状态
        /// </summary>
        /// <param name="negotiationIDs">协商ID列表</param>
        /// <param name="newStatus">新状态</param>
        /// <returns>更新结果</returns>
        Task<WebResponseContent> BatchUpdateNegotiationStatusAsync(List<long> negotiationIDs, string newStatus);

        /// <summary>
        /// 获取协商状态选项
        /// </summary>
        /// <returns>协商状态选项</returns>
        List<object> GetNegotiationStatusOptions();

        /// <summary>
        /// 获取消息状态过滤选项
        /// </summary>
        /// <returns>消息状态过滤选项列表</returns>
        List<object> GetMessageStatusFilterOptions();

        /// <summary>
        /// 获取按业务类型统计的协商消息数据
        /// </summary>
        /// <param name="messageStatus">消息状态过滤参数（可选）：sent-已发送消息, pending-待回复消息, overdue-已超期消息, replied-已回复消息</param>
        /// <returns>按业务类型统计的协商消息数据</returns>
        Task<BusinessTypeMessageStatisticsDto> GetNegotiationStatisticsByBusinessTypeAsync(string messageStatus = null);

        /// <summary>
        /// 根据协商ID发起OA流程
        /// 从委外跟踪表或采购跟踪表读取相关信息并发起流程
        /// </summary>
        /// <param name="negotiationId">协商ID</param>
        /// <param name="userId">发起用户ID</param>
        /// <returns>OA流程发起结果</returns>
        Task<WebResponseContent> StartOAProcessByNegotiationIdAsync(long negotiationId, string userId);

        /// <summary>
        /// 获取当前用户协商情况统计（管理员可以看到所有人的统计）
        /// </summary>
        /// <returns>当前用户协商情况统计数据</returns>
        Task<List<object>> GetUserNegotiationStatisticsAsync();
    }
}
