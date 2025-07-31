using System;

namespace HDPro.Core.Configuration
{
    /// <summary>
    /// TC系统配置选项
    /// </summary>
    public class TCSystemOptions
    {
        /// <summary>
        /// 配置节名称
        /// </summary>
        public const string SectionName = "TC";

        /// <summary>
        /// TC系统基础URL
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// WebService端点路径
        /// </summary>
        public string WebServiceEndpoint { get; set; } = "/webservice/service/teamcenter";

        /// <summary>
        /// 请求超时时间（秒）
        /// </summary>
        public int Timeout { get; set; } = 30;

        /// <summary>
        /// 配置描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 获取完整的WebService URL
        /// </summary>
        public string WebServiceUrl => $"{BaseUrl?.TrimEnd('/')}{WebServiceEndpoint}";

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        /// <returns>配置是否有效</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(BaseUrl) && 
                   !string.IsNullOrWhiteSpace(WebServiceEndpoint) &&
                   Timeout > 0;
        }

        /// <summary>
        /// 获取超时时间（TimeSpan格式）
        /// </summary>
        public TimeSpan TimeoutSpan => TimeSpan.FromSeconds(Timeout);
    }
}
