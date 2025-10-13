using System;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;     // IDependency
using HDPro.CY.Order.IServices.WZ;
using HDPro.Entity.DomainModels.OrderCollaboration;

namespace HDPro.CY.Order.Services.WZ
{
    /// <summary>
    /// 产线产量（热力图数据）服务实现
    /// 需求要点：
    /// 1) 手动刷新：清空缓存表 -> 调 ESB -> 聚合(日×阀体×产线) -> 回写
    /// 2) 查询：按阀体、产线、日期范围返回每日产量
    /// </summary>
    public partial class WZProductionOutputService : IWZProductionOutputService, IDependency
    {
        private readonly ServiceDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WZProductionOutputService> _logger;

        // ESB 接口地址（产线产能设计接口）
        private const string EsbUrl = "http://10.11.0.101:8003/gateway/DataCenter/CXCNSJ";

        public WZProductionOutputService(
            ServiceDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            ILogger<WZProductionOutputService> logger)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// ESB 返回行（仅取看板需要的关键字段）
        /// </summary>
        private sealed class EsbRow
        {
            [JsonProperty("F_ORA_FMLB")] public string ValveCategory { get; set; }   // 阀门类别
            [JsonProperty("F_ORA_SCX")] public string ProductionLine { get; set; }  // 生产线
            [JsonProperty("FQTY")] public decimal? Qty { get; set; }           // 订单数量
            // 日期字段可能有多个，优先使用 排产日期 F_ORA_DATE1
            [JsonProperty("F_ORA_DATE1")] public string SchDate { get; set; }
            [JsonProperty("FDATE")] public string OrderDate { get; set; }
            [JsonProperty("F_ORA_DATETIME")] public string CustReqDate { get; set; }
        }

        public async Task<int> RefreshAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate 不能早于 startDate");

            _logger.LogInformation("WZ 热力图数据刷新开始：{Start} ~ {End}", startDate, endDate);

            // 1) 调 ESB
            var client = _httpClientFactory.CreateClient();
            var payload = new
            {
                FSTARTDATE = startDate.ToString("yyyy-MM-dd"),
                FENDDATE = endDate.ToString("yyyy-MM-dd")
            };
            var reqContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, EsbUrl) { Content = reqContent };
            using var resp = await client.SendAsync(request, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);
            var jarr = JArray.Parse(json);
            var rows = jarr.ToObject<List<EsbRow>>() ?? new List<EsbRow>();

            _logger.LogInformation("ESB 返回记录数：{Count}", rows.Count);

            // 2) 聚合：按（日 × 阀体 × 产线）Sum(FQTY)
            static DateTime? PickDate(EsbRow r)
            {
                // 优先排产日期，其次下单/要货日期
                if (DateTime.TryParse(r.SchDate, out var d1)) return d1.Date;
                if (DateTime.TryParse(r.OrderDate, out var d2)) return d2.Date;
                if (DateTime.TryParse(r.CustReqDate, out var d3)) return d3.Date;
                return null;
            }

            var aggregates = rows
                .Select(r => new
                {
                    Date = PickDate(r),
                    Cat = (r.ValveCategory ?? string.Empty).Trim(),
                    Line = (r.ProductionLine ?? string.Empty).Trim(),
                    Qty = r.Qty ?? 0m
                })
                .Where(x => x.Date.HasValue && !string.IsNullOrEmpty(x.Cat) && !string.IsNullOrEmpty(x.Line))
                .GroupBy(x => new { x.Date, x.Cat, x.Line })
                .Select(g => new WZ_ProductionOutput
                {
                    ProductionDate = g.Key.Date!.Value,
                    ValveCategory = g.Key.Cat,
                    ProductionLine = g.Key.Line,
                    Quantity = g.Sum(z => z.Qty)
                })
                .ToList();

            _logger.LogInformation("聚合后记录数：{Count}", aggregates.Count);

            // 3) 事务：清空表 -> 批量写入
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // TRUNCATE 优先，失败回退 DELETE
                try
                {
                    await _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [WZ_ProductionOutput];", ct);
                }
                catch
                {
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM [WZ_ProductionOutput];", ct);
                    // 如需重置自增可开启（SQL Server）：
                    // await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('WZ_ProductionOutput', RESEED, 0);", ct);
                }

                if (aggregates.Count > 0)
                {
                    await _db.Set<WZ_ProductionOutput>().AddRangeAsync(aggregates, ct);
                    await _db.SaveChangesAsync(ct);
                }

                await tx.CommitAsync(ct);
                _logger.LogInformation("WZ 热力图数据刷新完成，入库：{Inserted} 条。", aggregates.Count);
                return aggregates.Count;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "WZ 热力图数据刷新失败。");
                throw;
            }
        }

        public Task<List<WZ_ProductionOutput>> GetAsync(
            string valveCategory,
            string productionLine,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate 不能早于 startDate");

            var query = _db.Set<WZ_ProductionOutput>()
                           .AsNoTracking()
                           .Where(x => x.ProductionDate >= startDate && x.ProductionDate <= endDate);

            if (!string.IsNullOrWhiteSpace(valveCategory))
                query = query.Where(x => x.ValveCategory == valveCategory);

            if (!string.IsNullOrWhiteSpace(productionLine))
                query = query.Where(x => x.ProductionLine == productionLine);

            // 如需稳定排序（日期->产线）
            query = query.OrderBy(x => x.ProductionDate)
                         .ThenBy(x => x.ValveCategory)
                         .ThenBy(x => x.ProductionLine);

            return query.ToListAsync(ct);
        }
    }
}
