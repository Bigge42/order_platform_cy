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
using Microsoft.AspNetCore.Authorization;

namespace HDPro.CY.Order.Controllers
{
    public partial class WZ_OrderCycleBaseController
    {
        public sealed class DateRangeDto
        {
            public DateTime Start { get; set; }

            public DateTime End { get; set; }
        }

        /// <summary>
        /// 从订单跟踪表同步数据到订单周期基础表
        /// </summary>
        /// <returns>同步的记录数量</returns>
        [HttpPost("sync-from-order-tracking")]
        [AllowAnonymous]
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
        /// <returns>回填批量结果摘要</returns>
        [HttpPost("batch-call-valve-rule-service")]
        [AllowAnonymous]
        public async Task<IActionResult> BatchCallValveRuleService()
        {
            try
            {
                var result = await Service.BatchCallValveRuleServiceAsync();
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"规则服务调用失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 预排产统计：按排产日期汇总订单数量（阀体×产线）
        /// </summary>
        [HttpGet("pre-schedule-output")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPreScheduleOutput(
            [FromQuery] string valveCategory,
            [FromQuery] string productionLine,
            [FromQuery(Name = "start")] DateTime startDate,
            [FromQuery(Name = "end")] DateTime endDate)
        {
            try
            {
                var list = await Service.GetPreScheduleOutputAsync(
                    valveCategory,
                    productionLine,
                    startDate,
                    endDate);
                return JsonNormal(list);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"预排产统计失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 预排产刷新：将 WZ_OrderCycleBase 汇总写入 WZ_ProductionOutput
        /// </summary>
        [HttpPost("pre-schedule-output/refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshPreScheduleOutput([FromBody] DateRangeDto dto)
        {
            try
            {
                var count = await Service.RefreshPreScheduleOutputAsync(dto.Start, dto.End);
                return JsonNormal(new { inserted = count, range = $"{dto.Start:yyyy-MM-dd}~{dto.End:yyyy-MM-dd}" });
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"预排产刷新失败：{ex.Message}"));
            }
        }
    }
}
