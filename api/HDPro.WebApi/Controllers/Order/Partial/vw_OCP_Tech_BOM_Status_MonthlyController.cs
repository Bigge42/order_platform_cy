/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("vw_OCP_Tech_BOM_Status_Monthly",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;

namespace HDPro.CY.Order.Controllers
{
    public partial class vw_OCP_Tech_BOM_Status_MonthlyController
    {
        private readonly Ivw_OCP_Tech_BOM_Status_MonthlyService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public vw_OCP_Tech_BOM_Status_MonthlyController(
            Ivw_OCP_Tech_BOM_Status_MonthlyService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 同步技术日期
        /// </summary>
        [HttpPost]
        [Route("sync-order-dates")]
        public async Task<IActionResult> SyncOrderDatesAsync()
        {
            var result = await _service.SyncOrderDatesAsync();
            return Json(result);
        }
    }
}
