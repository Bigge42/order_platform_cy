using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HDPro.Core.EFDbContext;
using HDPro.Core.Filters;
using HDPro.Core.ManageUser;
using HDPro.CY.Order.IServices.WZ;
using HDPro.Entity.DomainModels;
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

        /// <summary>
        /// 查询：按当前产能明细归属汇总销售跟踪订单明细金额。
        /// GET /api/WZ/ProductionOutput/sales-amount?start=2026-01-01&end=2026-03-31
        /// </summary>
        [HttpGet("sales-amount")]
        public async Task<ActionResult<List<WZProductionOutputSalesAmountDto>>> GetSalesAmount(
            [FromQuery] string valveCategory,
            [FromQuery] string productionLine,
            [FromQuery(Name = "start")] DateTime startDate,
            [FromQuery(Name = "end")] DateTime endDate,
            CancellationToken ct = default)
        {
            var list = await _service.GetSalesAmountAsync(valveCategory, productionLine, startDate, endDate, ct);
            return Ok(list);
        }

        public sealed class DateRangeDto
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        public sealed class SyncLogDto
        {
            public Guid LogId { get; set; }
            public string TaskName { get; set; } = string.Empty;
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public int? ElapsedSeconds { get; set; }
            public bool Success { get; set; }
            public string ResponseContent { get; set; } = string.Empty;
            public string ErrorMsg { get; set; } = string.Empty;
        }

        public sealed class PreProductionMergeDto
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public string ValveCategory { get; set; }
            public string ProductionLine { get; set; }
        }

        /// <summary>
        /// 查询热力图单元格订单明细。
        /// GET /api/WZ/ProductionOutput/details?date=2026-07-01&valveCategory=直通阀&productionLine=直通1
        /// </summary>
        [HttpGet("details")]
        public async Task<ActionResult<List<WZProductionOutputCellDetailDto>>> GetCellDetails(
            [FromQuery(Name = "date")] DateTime productionDate,
            [FromQuery] string valveCategory,
            [FromQuery] string productionLine,
            [FromQuery] int take = 10000,
            CancellationToken ct = default)
        {
            take = Math.Clamp(take, 1, 50000);
            var list = await _service.GetCellDetailsAsync(productionDate, valveCategory, productionLine, take, ct);
            return Ok(list);
        }

        /// <summary>
        /// 分页查询热力图单元格订单明细。
        /// GET /api/WZ/ProductionOutput/details-page?date=2026-07-01&valveCategory=直通阀&productionLine=直通1&page=1&pageSize=200
        /// </summary>
        [HttpGet("details-page")]
        public async Task<ActionResult<WZProductionOutputCellDetailPageDto>> GetCellDetailsPage(
            [FromQuery(Name = "date")] DateTime productionDate,
            [FromQuery] string valveCategory,
            [FromQuery] string productionLine,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 200,
            CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 1000);
            var result = await _service.GetCellDetailsPageAsync(productionDate, valveCategory, productionLine, page, pageSize, ct);
            return Ok(result);
        }

        /// <summary>
        /// 导出未知产线/冲突明细，供人工补充规则。
        /// GET /api/WZ/ProductionOutput/unknown-details?start=2026-07-01&end=2026-07-31
        /// </summary>
        [HttpGet("unknown-details")]
        public async Task<ActionResult<List<WZProductionOutputUnknownDetailDto>>> GetUnknownDetails(
            [FromQuery(Name = "start")] DateTime startDate,
            [FromQuery(Name = "end")] DateTime endDate,
            [FromQuery] int take = 100000,
            CancellationToken ct = default)
        {
            take = Math.Clamp(take, 1, 200000);
            var list = await _service.GetUnknownDetailsAsync(startDate, endDate, take, ct);
            return Ok(list);
        }

        /// <summary>
        /// 保存人工产线映射规则，后续同步/重新归类会优先使用。
        /// POST /api/WZ/ProductionOutput/manual-line-rules
        /// </summary>
        [HttpPost("manual-line-rules")]
        public async Task<ActionResult<object>> SaveManualLineRules(
            [FromBody] List<WZProductionOutputManualLineRuleDto> rules,
            CancellationToken ct = default)
        {
            var userName = UserContext.Current?.UserName;
            if (!string.Equals(userName, "cyadmin", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { message = "只有 cyadmin 可以导入人工映射规则", status = false, code = 403 });
            }

            var saved = await _service.SaveManualLineRulesAsync(rules ?? new List<WZProductionOutputManualLineRuleDto>(), ct);
            return Ok(new { saved, requested = rules?.Count ?? 0 });
        }

        private static bool IsSyncLogSuccessful(int? result, string responseContent, string errorMsg)
        {
            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                return false;
            }

            var response = responseContent ?? string.Empty;
            if (response.Contains("\"IsSuccessStatusCode\":false", StringComparison.OrdinalIgnoreCase)
                || response.Contains("\"StatusCode\":500", StringComparison.OrdinalIgnoreCase)
                || response.Contains("\"StatusCode\":400", StringComparison.OrdinalIgnoreCase)
                || response.Contains("Internal Server Error", StringComparison.OrdinalIgnoreCase)
                || response.Contains("UNHANDLED_EXCEPTION", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return result == 1;
        }

        /// <summary>
        /// 同步健康检查：查看最近同步时间、源表新鲜度、未归属明细和样例。
        /// GET /api/WZ/ProductionOutput/sync-health?start=2026-07-01&end=2026-07-31
        /// </summary>
        [HttpGet("sync-health")]
        public async Task<ActionResult<WZProductionOutputSyncHealthDto>> GetSyncHealth(
            [FromQuery(Name = "start")] DateTime? startDate,
            [FromQuery(Name = "end")] DateTime? endDate,
            CancellationToken ct = default)
        {
            var health = await _service.GetSyncHealthAsync(startDate, endDate, ct);
            return Ok(health);
        }

        /// <summary>
        /// 查询最近 WZ 产能定时增量同步记录。
        /// GET /api/WZ/ProductionOutput/sync-history?take=10
        /// </summary>
        [HttpGet("sync-history")]
        public async Task<ActionResult<List<SyncLogDto>>> GetSyncHistory(
            [FromQuery] int take = 10,
            CancellationToken ct = default)
        {
            take = Math.Clamp(take, 1, 50);
            using var sysDb = new SysDbContext();

            var rawLogs = await sysDb.Set<Sys_QuartzLog>()
                .AsNoTracking()
                .Where(x => x.TaskName != null
                    && (x.TaskName == "WZ产能每日增量同步"
                        || EF.Functions.Like(x.TaskName, "%WZ%产能%增量%")))
                .OrderByDescending(x => x.StratDate)
                .ThenByDescending(x => x.CreateDate)
                .Take(take)
                .Select(x => new
                {
                    x.LogId,
                    TaskName = x.TaskName ?? string.Empty,
                    StartTime = x.StratDate,
                    EndTime = x.EndDate,
                    ElapsedSeconds = x.ElapsedTime,
                    x.Result,
                    ResponseContent = x.ResponseContent ?? string.Empty,
                    ErrorMsg = x.ErrorMsg ?? string.Empty
                })
                .ToListAsync(ct);

            var logs = rawLogs
                .Select(x => new SyncLogDto
                {
                    LogId = x.LogId,
                    TaskName = x.TaskName,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    ElapsedSeconds = x.ElapsedSeconds,
                    Success = IsSyncLogSuccessful(x.Result, x.ResponseContent, x.ErrorMsg),
                    ResponseContent = x.ResponseContent,
                    ErrorMsg = x.ErrorMsg
                })
                .ToList();

            return Ok(logs);
        }

        /// <summary>
        /// OCP订单跟踪口径预览：按排产日期窗口生成WZ明细统计，不写入数据库。
        /// POST /api/WZ/ProductionOutput/order-tracking/preview
        /// body: { "start":"2026-07-01", "end":"2026-07-31" }
        /// </summary>
        [HttpPost("order-tracking/preview")]
        public async Task<ActionResult<WZProductionOutputRefreshResultDto>> PreviewFromOrderTracking(
            [FromBody] DateRangeDto dto,
            CancellationToken ct = default)
        {
            var result = await _service.PreviewFromOrderTrackingAsync(dto.Start, dto.End, ct);
            return Ok(result);
        }

        /// <summary>
        /// OCP订单跟踪口径刷新：按排产日期窗口重建WZ明细与汇总。
        /// POST /api/WZ/ProductionOutput/refresh/order-tracking
        /// body: { "start":"2026-07-01", "end":"2026-07-31" }
        /// </summary>
        [HttpPost("refresh/order-tracking")]
        public async Task<ActionResult<WZProductionOutputRefreshResultDto>> RefreshFromOrderTracking(
            [FromBody] DateRangeDto dto,
            CancellationToken ct = default)
        {
            var userName = UserContext.Current?.UserName;
            if (!string.Equals(userName, "cyadmin", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { message = "只有 cyadmin 可以同步数据", status = false, code = 403 });
            }

            var result = await _service.RefreshFromOrderTrackingAsync(dto.Start, dto.End, ct);
            return Ok(result);
        }

        /// <summary>
        /// 手动刷新：按同步接口入参时间窗口清空并重建缓存（仅管理员调用）。
        /// POST /api/WZ/ProductionOutput/refresh
        /// body: { "start":"2025-08-11", "end":"2026-05-16" }
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<object>> Refresh([FromBody] DateRangeDto dto, CancellationToken ct = default)
        {
            var userName = UserContext.Current?.UserName;
            if (!string.Equals(userName, "cyadmin", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { message = "只有 cyadmin 可以同步数据", status = false, code = 403 });
            }

            var count = await _service.RefreshAsync(dto.Start, dto.End, ct);
            return Ok(new { inserted = count, range = $"{dto.Start:yyyy-MM-dd}~{dto.End:yyyy-MM-dd}" });
        }

        /// <summary>
        /// 重新计算现有未知/冲突明细的产线：不重拉源数据，不清空明细表。
        /// POST /api/WZ/ProductionOutput/reclassify
        /// body: { "start":"2026-07-01", "end":"2026-07-31" }
        /// </summary>
        [HttpPost("reclassify")]
        public async Task<ActionResult<WZProductionOutputRefreshResultDto>> ReclassifyExistingDetails(
            [FromBody] DateRangeDto dto,
            CancellationToken ct = default)
        {
            var userName = UserContext.Current?.UserName;
            if (!string.Equals(userName, "cyadmin", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { message = "只有 cyadmin 可以同步数据", status = false, code = 403 });
            }

            var result = await _service.ReclassifyExistingDetailsAsync(dto.Start, dto.End, ct);
            return Ok(result);
        }

        /// <summary>
        /// 回填现有明细的物料编码/规格型号：不重拉源数据、不清空明细表。
        /// POST /api/WZ/ProductionOutput/material-models/backfill
        /// body: { "start":"2026-07-01", "end":"2026-07-31" }
        /// </summary>
        [HttpPost("material-models/backfill")]
        public async Task<ActionResult<WZProductionOutputMaterialBackfillResultDto>> BackfillMaterialModels(
            [FromBody] DateRangeDto dto,
            [FromQuery] int batchSize = 5000,
            CancellationToken ct = default)
        {
            var userName = UserContext.Current?.UserName;
            if (!string.Equals(userName, "cyadmin", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { message = "只有 cyadmin 可以回填物料规格型号", status = false, code = 403 });
            }

            var result = await _service.BackfillMaterialModelsAsync(dto.Start, dto.End, batchSize, ct);
            return Ok(result);
        }

        /// <summary>
        /// Quartz/local task: 回填现有明细的物料编码/规格型号。
        /// POST /api/WZ/ProductionOutput/material-models/backfill-task
        /// </summary>
        [ApiTask]
        [HttpPost("material-models/backfill-task")]
        public async Task<ActionResult<WZProductionOutputMaterialBackfillResultDto>> BackfillMaterialModelsTask(
            [FromBody] DateRangeDto dto,
            [FromQuery] int batchSize = 5000,
            CancellationToken ct = default)
        {
            var result = await _service.BackfillMaterialModelsAsync(dto.Start, dto.End, batchSize, ct);
            return Ok(result);
        }

        /// <summary>
        /// Quartz task: append previous day's incremental production output.
        /// POST /api/WZ/ProductionOutput/refresh/daily-increment-task
        /// </summary>
        [ApiTask]
        [HttpPost("refresh/daily-increment-task")]
        public async Task<ActionResult<object>> RefreshDailyIncrementTask(CancellationToken ct = default)
        {
            var startDate = DateTime.Today.AddDays(-1);
            var endDate = DateTime.Today;
            return await RefreshIncrementalWindowAsync(startDate, endDate, "previous-day-with-next-day-end-idempotent", ct);
        }

        /// <summary>
        /// External task: refresh production output by explicit source-date window.
        /// POST /api/WZ/ProductionOutput/refresh/incremental-task
        /// body: { "start":"2026-05-18", "end":"2026-05-19" }
        /// </summary>
        [ApiTask]
        [HttpPost("refresh/incremental-task")]
        public async Task<ActionResult<object>> RefreshIncrementalTask([FromBody] DateRangeDto dto, CancellationToken ct = default)
        {
            var startDate = dto?.Start == default ? DateTime.Today.AddDays(-1) : dto.Start.Date;
            var endDate = dto?.End == default ? startDate.AddDays(1) : dto.End.Date;
            return await RefreshIncrementalWindowAsync(startDate, endDate, "explicit-source-date-window-idempotent", ct);
        }

        private async Task<ActionResult<object>> RefreshIncrementalWindowAsync(
            DateTime startDate,
            DateTime endDate,
            string mode,
            CancellationToken ct)
        {
            if (endDate < startDate)
            {
                return BadRequest(new { message = "end 不能早于 start", range = $"{startDate:yyyy-MM-dd}~{endDate:yyyy-MM-dd}" });
            }

            var count = await _service.RefreshIncrementalAsync(startDate.Date, endDate.Date, ct);
            return Ok(new { updated = count, range = $"{startDate:yyyy-MM-dd}~{endDate:yyyy-MM-dd}", mode });
        }

        /// <summary>
        /// 预排产展示：汇总预排产输出并合并实际产量
        /// POST /api/WZ/ProductionOutput/preproduction/merge
        /// body: { "start":"2025-08-11", "end":"2025-08-12", "valveCategory":"", "productionLine":"" }
        /// </summary>
        [HttpPost("preproduction/merge")]
        public async Task<ActionResult<List<WZ_ProductionOutput>>> MergePreProduction(
            [FromBody] PreProductionMergeDto dto,
            CancellationToken ct = default)
        {
            var list = await _service.GetWithPreProductionAsync(
                dto.ValveCategory,
                dto.ProductionLine,
                dto.Start,
                dto.End,
                ct);
            return Ok(list);
        }

        /// <summary>
        /// 排产优化展示：用产能排产日期覆盖生产日期后汇总并合并实际产量
        /// POST /api/WZ/ProductionOutput/preproduction/optimize
        /// body: { "start":"2025-08-11", "end":"2025-08-12", "valveCategory":"", "productionLine":"" }
        /// </summary>
        [HttpPost("preproduction/optimize")]
        public async Task<ActionResult<List<WZ_ProductionOutput>>> OptimizePreProduction(
            [FromBody] PreProductionMergeDto dto,
            CancellationToken ct = default)
        {
            var list = await _service.GetWithOptimizedPreProductionAsync(
                dto.ValveCategory,
                dto.ProductionLine,
                dto.Start,
                dto.End,
                ct);
            return Ok(list);
        }
    }
}
