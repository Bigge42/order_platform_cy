/*
*所有关于Sys_ThirdPartyApp类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System;
using System.Threading.Tasks;

namespace HDPro.Sys.IServices
{
    public partial interface ISys_ThirdPartyAppService
    {
        /// <summary>
        /// 创建第三方应用
        /// </summary>
        /// <param name="appName">应用名称</param>
        /// <param name="description">应用描述</param>
        /// <param name="allowedIPs">允许的IP地址，多个用逗号分隔</param>
        /// <param name="expireTime">过期时间</param>
        /// <param name="prefix">AppId前缀</param>
        /// <returns></returns>
        Task<WebResponseContent> CreateThirdPartyAppAsync(
            string appName, 
            string description = "", 
            string allowedIPs = "", 
            DateTime? expireTime = null,
            string prefix = "CY1jOrd");

        /// <summary>
        /// 重新生成AppSecret
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <returns></returns>
        Task<WebResponseContent> RegenerateAppSecretAsync(string appId);

        /// <summary>
        /// 启用/禁用第三方应用
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="enabled">是否启用</param>
        /// <returns></returns>
        Task<WebResponseContent> SetAppEnabledAsync(string appId, bool enabled);
    }
 }
