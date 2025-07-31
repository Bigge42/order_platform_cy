/*
 * K3Cloud登录请求模型
 */
using System.ComponentModel.DataAnnotations;

namespace HDPro.CY.Order.Services.K3Cloud.Models
{
    /// <summary>
    /// K3Cloud登录请求模型
    /// </summary>
    public class K3CloudLoginRequest
    {
        /// <summary>
        /// 账套ID
        /// </summary>
        [Required]
        public string acctid { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        public string username { get; set; }

        /// <summary>
        /// 应用ID
        /// </summary>
        [Required]
        public string appId { get; set; }

        /// <summary>
        /// 应用密钥
        /// </summary>
        [Required]
        public string appSecret { get; set; }

        /// <summary>
        /// 语言ID，默认2052（中文）
        /// </summary>
        public int lcid { get; set; } = 2052;
    }
} 