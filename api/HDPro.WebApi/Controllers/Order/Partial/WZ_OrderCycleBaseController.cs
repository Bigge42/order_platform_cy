/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("WZ_OrderCycleBase",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices;
using HDPro.Core.Utilities;
using System.Collections.Generic;
using HDPro.CY.Order.Models.OrderCycleBaseDtos;

namespace HDPro.CY.Order.Controllers
{
    public partial class WZ_OrderCycleBaseController
    {
        /// <summary>
        /// 从订单跟踪表同步数据到订单周期基础表
        /// </summary>
        /// <returns>同步的记录数量</returns>
        [HttpPost("sync-from-order-tracking")]
        public async Task<IActionResult> SyncFromOrderTracking()
        {
            try
            {
                var result = await Service.SyncFromOrderTrackingAsync();
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"同步失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 批量调用阀门规则服务，回填订单周期信息
        /// </summary>
        /// <param name="items">业务数据集合</param>
        /// <returns>成功回填的数量</returns>
        [HttpPost("batch-call-valve-rule-service")]
        public async Task<IActionResult> BatchCallValveRuleService([FromBody] List<OrderRuleDto> items)
        {
            try
            {
                var result = await Service.BatchCallValveRuleServiceAsync(items);
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"规则服务调用失败：{ex.Message}"));
            }
        }
    }
}
