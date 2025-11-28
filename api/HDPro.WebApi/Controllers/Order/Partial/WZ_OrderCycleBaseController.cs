/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("WZ_OrderCycleBase",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using HDPro.CY.Order.IServices;
using HDPro.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace HDPro.CY.Order.Controllers
{
    public partial class WZ_OrderCycleBaseController
    {
        private readonly IWZ_OrderCycleBaseService _service;//访问业务代码
        private readonly ILogger<WZ_OrderCycleBaseController> _logger;

        [ActivatorUtilitiesConstructor]
        public WZ_OrderCycleBaseController(
            IWZ_OrderCycleBaseService service,
            ILogger<WZ_OrderCycleBaseController> logger)
            : base(service)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// 从订单跟踪表同步数据到订单周期基础表
        /// </summary>
        /// <returns>同步的记录数量</returns>
        [HttpPost("sync-from-order-tracking")]
        public async Task<IActionResult> SyncFromOrderTracking()
        {
            try
            {
                var result = await _service.SyncFromOrderTrackingAsync();
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【订单周期同步】接口调用失败：{Message}", ex.Message);
                return JsonNormal(new WebResponseContent().Error($"同步失败：{ex.Message}"));
            }
        }
    }
}
