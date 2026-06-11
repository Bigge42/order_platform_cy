using System;
using System.Threading;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices.WZ;
using Microsoft.AspNetCore.Mvc;

namespace HDPro.CY.Order.Controllers.WZ
{
    /// <summary>
    /// 异常排产调整工作台接口。
    /// </summary>
    [ApiController]
    [Route("api/WZ/CapacityScheduleAdjustment")]
    public class WZCapacityScheduleAdjustmentController : ControllerBase
    {
        private readonly IWZCapacityScheduleAdjustmentService _service;

        public WZCapacityScheduleAdjustmentController(IWZCapacityScheduleAdjustmentService service)
        {
            _service = service;
        }

        /// <summary>
        /// 查询标红超载和标黄法定节假日的异常排产订单。
        /// </summary>
        [HttpPost("list")]
        public async Task<IActionResult> QueryAbnormalOrders(
            [FromBody] CapacityScheduleAdjustmentQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.QueryAbnormalOrdersAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// 查询指定异常订单附近 15 天的当前产线产能窗口。
        /// </summary>
        [HttpGet("window/{id:int}")]
        public async Task<IActionResult> GetWindow(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetAdjustmentWindowAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// 保存手动选择的排产优化日期，并同步排产优化汇总。
        /// </summary>
        [HttpPost("save")]
        public async Task<IActionResult> Save(
            [FromBody] CapacityScheduleAdjustmentSaveDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.SaveAdjustmentAsync(dto, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
