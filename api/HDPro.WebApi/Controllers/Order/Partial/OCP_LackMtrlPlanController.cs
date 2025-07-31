/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_LackMtrlPlan",Enums.ActionPermissionOptions.Search)]
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
    public partial class OCP_LackMtrlPlanController
    {
        private readonly IOCP_LackMtrlPlanService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_LackMtrlPlanController(
            IOCP_LackMtrlPlanService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 设置默认方案
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>操作结果</returns>
        [HttpPost, Route("SetDefaultPlan")]
        public IActionResult SetDefaultPlan([FromBody] SetDefaultPlanRequest request)
        {
            if (request?.ComputeId == null || request.ComputeId <= 0)
            {
                return Json(new { status = false, message = "方案ID不能为空" });
            }

            var result = _service.SetDefaultPlan(request.ComputeId);
            return Json(result);
        }

        /// <summary>
        /// 取消默认方案
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>操作结果</returns>
        [HttpPost, Route("CancelDefaultPlan")]
        public IActionResult CancelDefaultPlan([FromBody] CancelDefaultPlanRequest request)
        {
            if (request?.ComputeId == null || request.ComputeId <= 0)
            {
                return Json(new { status = false, message = "方案ID不能为空" });
            }

            var result = _service.CancelDefaultPlan(request.ComputeId);
            return Json(result);
        }
    }

    /// <summary>
    /// 设置默认方案请求参数
    /// </summary>
    public class SetDefaultPlanRequest
    {
        /// <summary>
        /// 方案ID
        /// </summary>
        public long ComputeId { get; set; }
    }

    /// <summary>
    /// 取消默认方案请求参数
    /// </summary>
    public class CancelDefaultPlanRequest
    {
        /// <summary>
        /// 方案ID
        /// </summary>
        public long ComputeId { get; set; }
    }
}
