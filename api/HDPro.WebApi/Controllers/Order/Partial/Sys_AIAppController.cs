using HDPro.CY.Order.IServices;
using HDPro.Core.Enums;
using HDPro.Core.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Controllers
{
    public partial class Sys_AIAppController
    {
        private readonly ISys_AIAppService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_AIAppController(
            ISys_AIAppService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet, Route("GetPlatformApps")]
        [ApiActionPermission(ActionPermissionOptions.Search)]
        public async Task<IActionResult> GetPlatformApps(string keyword = null)
        {
            return Json(await _service.GetPlatformAppsAsync(keyword));
        }

        [HttpPost, Route("SyncFromPlatform")]
        [ApiActionPermission(ActionPermissionOptions.Update)]
        public async Task<IActionResult> SyncFromPlatform([FromBody] List<string> platformAppIds)
        {
            return Json(await _service.SyncFromPlatformAsync(platformAppIds));
        }

        [HttpPost, Route("CreatePlatformAppKey")]
        [ApiActionPermission(ActionPermissionOptions.Update)]
        public async Task<IActionResult> CreatePlatformAppKey([FromBody] CreatePlatformAppKeyRequest request)
        {
            return Json(await _service.CreatePlatformAppKeyAsync(request?.PlatformAppId));
        }
    }

    public class CreatePlatformAppKeyRequest
    {
        public string PlatformAppId { get; set; }
    }
}
