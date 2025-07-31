/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_PurchaseOrder",Enums.ActionPermissionOptions.Search)]
 */
using HDPro.CY.Order.IServices;
using HDPro.Entity.DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_PurchaseOrderController
    {
        private readonly IOCP_PurchaseOrderService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_PurchaseOrderController(
            IOCP_PurchaseOrderService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }


        [HttpPost, Route("SummaryDetails2Head"), AllowAnonymous]
        public async Task<JsonResult> SummaryDetails2Head()
        {
            var result = await _service.SummaryDetails2Head();
            return JsonNormal(result);
        }
    }
}
