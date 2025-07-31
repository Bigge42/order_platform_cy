/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_ThirdPartyApp",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.Sys.IServices;

namespace HDPro.Sys.Controllers
{
    public partial class Sys_ThirdPartyAppController
    {
        private readonly ISys_ThirdPartyAppService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_ThirdPartyAppController(
            ISys_ThirdPartyAppService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 创建第三方应用
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateApp([FromBody] CreateThirdPartyAppRequest request)
        {
            var result = await _service.CreateThirdPartyAppAsync(
                request.AppName,
                request.Description,
                request.AllowedIPs,
                request.ExpireTime,
                request.Prefix
            );
            
            return Ok(result);
        }

        /// <summary>
        /// 重新生成AppSecret
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <returns></returns>
        [HttpPost("regenerate-secret/{appId}")]
        public async Task<IActionResult> RegenerateSecret(string appId)
        {
            var result = await _service.RegenerateAppSecretAsync(appId);
            return Ok(result);
        }

        /// <summary>
        /// 启用/禁用应用
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="enabled">是否启用</param>
        /// <returns></returns>
        [HttpPost("set-enabled/{appId}")]
        public async Task<IActionResult> SetEnabled(string appId, [FromBody] bool enabled)
        {
            var result = await _service.SetAppEnabledAsync(appId, enabled);
            return Ok(result);
        }
    }

    /// <summary>
    /// 创建第三方应用请求
    /// </summary>
    public class CreateThirdPartyAppRequest
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 应用描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 允许的IP地址，多个用逗号分隔
        /// </summary>
        public string AllowedIPs { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpireTime { get; set; }

        /// <summary>
        /// AppId前缀
        /// </summary>
        public string Prefix { get; set; } = "CY1jOrd";
    }
}
