/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("ERP_OrderTracking",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;
using Microsoft.Extensions.Logging;
using HDPro.Core.Filters;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.Controllers
{
    public partial class ERP_OrderTrackingController
    {
        private readonly IERP_OrderTrackingService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ERP_OrderTrackingController> _logger;

        [ActivatorUtilitiesConstructor]
        public ERP_OrderTrackingController(
            IERP_OrderTrackingService service,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ERP_OrderTrackingController> logger
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// 定时任务：ERP订单跟踪明细同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("ERPOrderTrackingSync")]
        public async Task<IActionResult> ERPOrderTrackingSyncTask()
        {
            try
            {
                _logger.LogInformation("定时任务：开始ERP订单跟踪明细同步");

                var result = await _service.SyncERPOrderTrackingAsync();

                _logger.LogInformation($"定时任务：ERP订单跟踪明细同步完成，结果：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：ERP订单跟踪明细同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }
    }
}
