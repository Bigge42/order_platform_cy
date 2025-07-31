/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_PurchaseSupplierMapping",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Models;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_PurchaseSupplierMappingController
    {
        private readonly IOCP_PurchaseSupplierMappingService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_PurchaseSupplierMappingController(
            IOCP_PurchaseSupplierMappingService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 根据业务类型和业务主键获取供应商负责人信息
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>供应商负责人信息</returns>
        [HttpPost("GetSupplierResponsible")]
        public async Task<IActionResult> GetSupplierResponsible([FromBody] GetSupplierResponsibleRequest request)
        {
            var result = await _service.GetSupplierResponsibleAsync(request);
            return Json(result);
        }
    }
}
