using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using HDPro.CY.Order.IServices.WZ;
using HDPro.Entity.DomainModels.OrderCollaboration;
// 如果你们项目使用权限标记/基类控制器，请按需引入：
// using HDPro.Core.Filters;
// using HDPro.Core.Controllers; 等

namespace HDPro.CY.Order.Controllers.WZ
{
    [ApiController]
    [Route("api/WZ/ProductionOutput")]
    // [PermissionTable(Name = "WZ_ProductionOutput")] // 如有权限控制，按需开启
    public class WZProductionOutputController : ControllerBase
    {
        private readonly IWZProductionOutputService _service;

        public WZProductionOutputController(IWZProductionOutputService service)
        {
            _service = service;
        }

        /// <summary>
        public sealed class ThresholdItemDto
        {
            public string ValveCategory { get; set; }
            public string ProductionLine { get; set; }
            public decimal Threshold { get; set; }
        }


        /// <summary>
        /// 批量写入阈值：按阀体+产线更新 CurrentThreshold
        /// POST /api/WZ/ProductionOutput/thresholds
        /// body: [{ "valveCategory":"阀体A", "productionLine":"产线1", "threshold":20 }]
        /// </summary>
        [HttpPost("thresholds")]
        public async Task<ActionResult<object>> SaveThresholds([FromBody] List<ThresholdItemDto> items, CancellationToken ct = default)
        {
            var thresholds = items?.ConvertAll(i =>
                (i.ValveCategory ?? string.Empty, i.ProductionLine ?? string.Empty, i.Threshold)) ?? new List<(string, string, decimal)>();
            var affected = await _service.UpdateThresholdsAsync(thresholds, ct);
            return Ok(new { updated = affected });
        }
        /// 查询：按阀体、产线、时间范围获取每日产量
        /// GET /api/WZ/ProductionOutput?valveCategory=直通阀&productionLine=产线1&start=2025-01-01&end=2025-12-31
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<WZ_ProductionOutput>>> Get(
            [FromQuery] string valveCategory,
            [FromQuery] string productionLine,
            [FromQuery(Name = "start")] DateTime startDate,
            [FromQuery(Name = "end")] DateTime endDate,
            CancellationToken ct = default)
        {
            var list = await _service.GetAsync(valveCategory, productionLine, startDate, endDate, ct);
            return Ok(list);
        }

        public sealed class DateRangeDto
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        /// <summary>
        /// 手动刷新：清空并重建缓存（仅管理员调用）
        /// POST /api/WZ/ProductionOutput/refresh
        /// body: { "start":"2025-08-11", "end":"2025-08-12" }
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<object>> Refresh([FromBody] DateRangeDto dto, CancellationToken ct = default)
        {
            var count = await _service.RefreshAsync(dto.Start, dto.End, ct);
            return Ok(new { inserted = count, range = $"{dto.Start:yyyy-MM-dd}~{dto.End:yyyy-MM-dd}" });
        }
    }
}
