/*
 * K3Cloud登录响应模型
 */
namespace HDPro.CY.Order.Services.K3Cloud.Models
{
    /// <summary>
    /// K3Cloud登录响应模型
    /// </summary>
    public class K3CloudLoginResponse
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 消息代码
        /// </summary>
        public string MessageCode { get; set; }

        /// <summary>
        /// 登录结果类型
        /// </summary>
        public int LoginResultType { get; set; }

        /// <summary>
        /// 会话ID
        /// </summary>
        public string KDSVCSessionId { get; set; }

        /// <summary>
        /// 语言ID
        /// </summary>
        public int Lcid { get; set; }

        /// <summary>
        /// 是否登录成功
        /// </summary>
        public bool IsSuccess => LoginResultType == 1 && !string.IsNullOrEmpty(KDSVCSessionId);
    }
} 