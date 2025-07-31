/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_FilterPlan",Enums.ActionPermissionOptions.Search)]
 */
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.System.dto;
using HDPro.Sys.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDPro.Sys.Controllers
{
    public partial class Sys_FilterPlanController
    {
        private readonly ISys_FilterPlanService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_FilterPlanController(
            ISys_FilterPlanService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost, Route("AddOrUpdatePlan")]
        public async Task<IActionResult> AddOrUpdatePlan([FromBody] System_FilterPlanInputDto plan)
        {
            return Json(await Service.AddOrUpdatePlan(plan));
        }

        [HttpGet, Route("GetPlan")]
        public async Task<IActionResult> GetPlan(string BillName)
        {
            return Json(await Service.GetPlan(BillName));
        }


        [HttpPost, Route("DeletePlan")]
        public async Task<IActionResult> DeletePlan(long id)
        {
            return Json(await Service.DeletePlan(id));
        }

    }
}
