/*
*所有关于OCP_UrgentOrder类的业务代码接口应在此处编写
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
    public partial interface IOCP_UrgentOrderService
    {
        /// <summary>
        /// 发送订单相关的OA消息（通用方法）
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="orderType">订单类型</param>
        /// <param name="messageType">消息类型</param>
        /// <param name="customContent">自定义内容</param>
        /// <returns></returns>
        Task<WebResponseContent> SendOrderOAMessageAsync(
            string senderLoginName,
            List<string> receiverLoginNames,
            string orderNo,
            string orderType,
            string messageType,
            string customContent = null);

        /// <summary>
        /// 获取催单消息统计数据
        /// </summary>
        /// <returns>催单统计数据</returns>
        Task<MessageStatisticsDto> GetUrgentOrderStatisticsAsync();

        /// <summary>
        /// 更新催单状态
        /// </summary>
        /// <param name="urgentOrderId">催单ID</param>
        /// <param name="status">新状态</param>
        /// <returns>操作结果</returns>
        Task<WebResponseContent> UpdateUrgentOrderStatusAsync(long urgentOrderId, string status);

        /// <summary>
        /// 批量更新催单状态
        /// </summary>
        /// <param name="urgentOrderIds">催单ID列表</param>
        /// <param name="status">新状态</param>
        /// <returns>操作结果</returns>
        Task<WebResponseContent> BatchUpdateUrgentOrderStatusAsync(List<long> urgentOrderIds, string status);

        /// <summary>
        /// 获取催单状态选项
        /// </summary>
        /// <returns>状态选项列表</returns>
        List<object> GetUrgentOrderStatusOptions();

        /// <summary>
        /// 获取消息状态过滤选项
        /// </summary>
        /// <returns>消息状态过滤选项列表</returns>
        List<object> GetMessageStatusFilterOptions();

        /// <summary>
        /// 获取按业务类型统计的催单消息数据
        /// </summary>
        /// <param name="messageStatus">消息状态过滤参数（可选）：sent-已发送消息, pending-待回复消息, overdue-已超期消息, replied-已回复消息</param>
        /// <returns>按业务类型统计的催单消息数据</returns>
        Task<BusinessTypeMessageStatisticsDto> GetUrgentOrderStatisticsByBusinessTypeAsync(string messageStatus = null);

        /// <summary>
        /// 根据业务类型和业务主键查询催单和协商信息
        /// </summary>
        /// <param name="businessType">业务类型</param>
        /// <param name="businessKey">业务主键</param>
        /// <returns>业务关联的催单和协商信息</returns>
        Task<WebResponseContent> GetBusinessOrderCollaborationAsync(string businessType, string businessKey);

        /// <summary>
        /// 批量添加催单
        /// </summary>
        /// <param name="urgentOrders">催单列表</param>
        /// <returns>操作结果</returns>
        Task<WebResponseContent> BatchAddAsync(List<OCP_UrgentOrder> urgentOrders);

        /// <summary>
        /// 批量获取默认负责人
        /// </summary>
        /// <param name="request">批量获取请求</param>
        /// <returns>默认负责人信息列表</returns>
        Task<WebResponseContent> BatchGetDefaultResponsibleAsync(BatchGetDefaultResponsibleRequest request);

        /// <summary>
        /// 更新催单SRM推送状态
        /// </summary>
        /// <param name="urgentOrderId">催单ID</param>
        /// <param name="isSendSRM">是否已推送SRM（0-否，1-是）</param>
        /// <returns>操作结果</returns>
        Task<WebResponseContent> UpdateUrgentOrderSRMStatusAsync(long urgentOrderId, int isSendSRM);

        /// <summary>
        /// 获取当前用户催单情况统计（管理员可以看到所有人的统计）
        /// </summary>
        /// <returns>当前用户催单情况统计数据</returns>
        Task<List<object>> GetUserUrgentOrderStatisticsAsync();

        /// <summary>
        /// 物资供应中心用户对催单向SRM发起催货
        /// </summary>
        /// <param name="urgentOrderId">催单ID</param>
        /// <returns>操作结果</returns>
        Task<WebResponseContent> SendUrgentOrderToSRMAsync(long urgentOrderId);

        /// <summary>
        /// 批量向SRM发起催货
        /// </summary>
        /// <param name="urgentOrderIds">催单ID列表</param>
        /// <returns>操作结果</returns>
        Task<WebResponseContent> BatchSendUrgentOrderToSRMAsync(List<long> urgentOrderIds);

    }
}
