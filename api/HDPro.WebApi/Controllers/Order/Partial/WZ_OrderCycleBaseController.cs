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

namespace HDPro.CY.Order.Controllers
{
    public partial class WZ_OrderCycleBaseController
    {
        private readonly IWZ_OrderCycleBaseService _service;//访问业务代码

        [ActivatorUtilitiesConstructor]
        public WZ_OrderCycleBaseController(IWZ_OrderCycleBaseService service)
            : base(service)
        {
            _service = service;
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
                return JsonNormal(new WebResponseContent().Error($"同步失败：{ex.Message}"));
            }
        }
    }
}
