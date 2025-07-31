using System;

namespace HDPro.Core.Configuration
{
    /// <summary>
    /// SSO单点登录配置
    /// </summary>
    public class SSOConfig
    {
        /// <summary>
        /// SSO服务器地址
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// 客户端密钥
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// 回调地址
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// 授权端点
        /// </summary>
        public string AuthorizeEndpoint { get; set; } = "/oauth/authorize";

        /// <summary>
        /// 令牌端点
        /// </summary>
        public string TokenEndpoint { get; set; } = "/oauth/token";

        /// <summary>
        /// 用户信息端点
        /// </summary>
        public string UserInfoEndpoint { get; set; } = "/oauth/userinfo";

        /// <summary>
        /// 是否启用SSO
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }
} 