/*
 * OA集成服务接口
 * 定义与OA系统集成的公共服务接口
 */
using HDPro.Core.Utilities;
using HDPro.CY.Order.Services.OA;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices.OA
{
    /// <summary>
    /// OA集成服务接口
    /// 定义与OA系统的集成功能接口
    /// </summary>
    public interface IOAIntegrationService
    {
        /// <summary>
        /// 获取OA系统Token
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <returns>Token字符串</returns>
        Task<WebResponseContent> GetTokenAsync(string loginName);

        /// <summary>
        /// 推送消息到OA系统
        /// </summary>
        /// <param name="token">认证Token</param>
        /// <param name="sendUserId">发送者用户ID</param>
        /// <param name="loginNames">接收者登录名列表</param>
        /// <param name="content">消息内容</param>
        /// <param name="urls">附件或扩展链接数组（可为空）</param>
        /// <returns>推送结果</returns>
        Task<WebResponseContent> SendMessageAsync(
            string token, 
            string sendUserId, 
            List<string> loginNames, 
            string content, 
            List<string> urls = null);

        /// <summary>
        /// 获取Token并推送消息（便捷方法）
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="content">消息内容</param>
        /// <param name="urls">附件或扩展链接数组（可为空）</param>
        /// <returns>推送结果</returns>
        Task<WebResponseContent> GetTokenAndSendMessageAsync(
            string senderLoginName,
            List<string> receiverLoginNames, 
            string content, 
            List<string> urls = null);

        /// <summary>
        /// 推送订单相关消息的便捷方法
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="orderType">订单类型</param>
        /// <param name="messageType">消息类型（如：催单、协商等）</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="customContent">自定义消息内容（可选）</param>
        /// <returns>推送结果</returns>
        Task<WebResponseContent> SendOrderMessageAsync(
            string senderLoginName,
            string orderNo,
            string orderType,
            string messageType,
            List<string> receiverLoginNames,
            string customContent = null);

        /// <summary>
        /// 发起OA流程
        /// </summary>
        /// <param name="token">认证Token</param>
        /// <param name="formData">表单数据</param>
        /// <returns>流程发起结果</returns>
        Task<WebResponseContent> StartProcessAsync(string token, OAFormData formData);

        /// <summary>
        /// 获取Token并发起流程（便捷方法）
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="formData">表单数据</param>
        /// <returns>流程发起结果</returns>
        Task<WebResponseContent> GetTokenAndStartProcessAsync(string loginName, OAFormData formData);

        #region 股份OA流程接口

        /// <summary>
        /// 获取股份OA系统Token
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <returns>Token字符串</returns>
        Task<WebResponseContent> GetShareholderTokenAsync(string loginName);

        /// <summary>
        /// 发起股份OA流程
        /// </summary>
        /// <param name="token">认证Token</param>
        /// <param name="formData">股份表单数据</param>
        /// <returns>流程发起结果</returns>
        Task<WebResponseContent> StartShareholderProcessAsync(string token, ShareholderOAFormData formData);

        /// <summary>
        /// 获取Token并发起股份流程（便捷方法）
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="formData">股份表单数据</param>
        /// <returns>流程发起结果</returns>
        Task<WebResponseContent> GetShareholderTokenAndStartProcessAsync(string loginName, ShareholderOAFormData formData);

        /// <summary>
        /// 智能发起流程（根据配置自动选择使用原有OA还是股份OA）
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="processData">流程数据</param>
        /// <returns>流程发起结果</returns>
        Task<WebResponseContent> SmartStartProcessAsync(string loginName, SmartOAProcessData processData);

        /// <summary>
        /// 发起OA合同变更评审流程
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="contractChangeData">合同变更数据</param>
        /// <returns>流程发起结果</returns>
        Task<WebResponseContent> StartContractChangeReviewProcessAsync(string loginName, ContractChangeReviewData contractChangeData);

        /// <summary>
        /// 推送消息到股份OA系统
        /// </summary>
        /// <param name="token">认证Token</param>
        /// <param name="sendUserId">发送者用户ID</param>
        /// <param name="loginNames">接收者登录名列表</param>
        /// <param name="content">消息内容</param>
        /// <param name="urls">附件或扩展链接数组（可为空）</param>
        /// <returns>推送结果</returns>
        Task<WebResponseContent> SendShareholderMessageAsync(
            string token,
            string sendUserId,
            List<string> loginNames,
            string content,
            List<string>? urls = null);

        /// <summary>
        /// 获取Token并推送消息到股份OA系统（便捷方法）
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="content">消息内容</param>
        /// <param name="urls">附件或扩展链接数组（可为空）</param>
        /// <returns>推送结果</returns>
        Task<WebResponseContent> GetShareholderTokenAndSendMessageAsync(
            string senderLoginName,
            List<string> receiverLoginNames,
            string content,
            List<string>? urls = null);

        /// <summary>
        /// 同时发送消息到原OA和股份OA系统
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="content">消息内容</param>
        /// <param name="urls">附件或扩展链接数组（可为空）</param>
        /// <returns>推送结果（包含两个OA系统的发送结果）</returns>
        Task<WebResponseContent> SendToBothOASystemsAsync(
            string senderLoginName,
            List<string> receiverLoginNames,
            string content,
            List<string>? urls = null);

        /// <summary>
        /// 根据人员工号获取股份OA人员ID
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <param name="token">认证Token（可选，如果为空则动态获取Token）</param>
        /// <returns>人员信息</returns>
        Task<WebResponseContent> GetShareholderOAUserByCodeAsync(string employeeCode, string token = null);

        /// <summary>
        /// 获取Token并查询股份OA人员信息（参照GetShareholderTokenAndStartProcessAsync模式）
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <returns>人员信息查询结果</returns>
        Task<WebResponseContent> GetShareholderTokenAndQueryUserAsync(string employeeCode);

        #endregion
    }
} 