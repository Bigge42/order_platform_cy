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
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager; // IDependency
using HDPro.CY.Order.IServices.WZ;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.OrderCollaboration;

namespace HDPro.CY.Order.Services.WZ
{
    /// <summary>
    /// 产线产量（热力图数据）服务实现
    /// 刷新（全量重建）语义说明：
    /// - 输入参数 startDate/endDate 是“订单修改日期窗口”，用于筛 ESB 接口数据；
    /// - 数据入库以“排产日期（ProductionDate）× 阀体类别（ValveCategory）× 产线（ProductionLine）”为键进行聚合求和；
    /// - 不对 ProductionDate 做按段过滤；跨段（按修改日分段）重复返回的键进行“累加汇总”，最后一次性入库；
    /// - 当前实现为“全量重建”：先清空缓存表，再插入本次窗口汇总结果。
    /// 
    /// 命名客户端：
    /// - 请在 Program.cs/Startup.cs 中注册： services.AddHttpClient("WZ", c => { c.Timeout = TimeSpan.FromMinutes(2); });
    /// </summary>
    public partial class WZProductionOutputService : IWZProductionOutputService, IDependency
    {
        private static readonly SemaphoreSlim _refreshGate = new(1, 1); // 防并发刷新
        private static readonly SemaphoreSlim _preProductionGate = new(1, 1); // 防并发汇总预排产
        private const string EsbUrl = "http://10.11.0.101:8003/gateway/DataCenter/CXCNSJ";
        private const int ChunkDays = 7;  // 修改日窗口切片长度（可按 ESB 性能调整）
        private const int InsertBatchSize = 2000; // 大批量入库时的分批大小

        private readonly ServiceDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WZProductionOutputService> _logger;

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
        /// 注意：ESB 筛选依据是“修改日窗口”，但这里不包含修改日字段；ProductionDate 由下方 PickDate 选择。
        /// </summary>
        private sealed class EsbRow
        {
            [JsonProperty("F_ORA_FMLB")] public string ValveCategory { get; set; }   // 阀门类别
            [JsonProperty("F_ORA_SCX")] public string ProductionLine { get; set; }  // 生产线
            [JsonProperty("FQTY")] public decimal? Qty { get; set; }           // 订单数量

            // 日期字段：优先 排产日期 F_ORA_DATE1，其次订单日期 & 要货日期
            [JsonProperty("F_ORA_DATE1")] public string SchDate { get; set; }         // 排产日期（推荐映射 ProductionDate）
            [JsonProperty("FDATE")] public string OrderDate { get; set; }       // 订单日期
            [JsonProperty("F_ORA_DATETIME")] public string CustReqDate { get; set; }     // 客户要货日期
        }

        /// <summary>把日期区间按固定天数切段（闭区间）</summary>
        private static IEnumerable<(DateTime S, DateTime E)> ChunkDates(DateTime start, DateTime end, int stepDays = ChunkDays)
        {
            var s = start.Date; var e = end.Date;
            while (s <= e)
            {
                var chunkEnd = s.AddDays(stepDays - 1);
                if (chunkEnd > e) chunkEnd = e;
                yield return (s, chunkEnd);
                s = chunkEnd.AddDays(1);
            }
        }

        /// <summary>字符串规整：Trim + 全/半角等标准化，避免“看起来一样但字符串不同”</summary>
        private static string NormalizeStr(string s)
            => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim().Normalize(NormalizationForm.FormKC);

        /// <summary>
        /// 选择用于 ProductionDate 的日期（优先排产日 F_ORA_DATE1 ；否则回落到订单日/要货日）
        /// </summary>
        private static DateTime? PickDate(EsbRow r)
        {
            if (!string.IsNullOrWhiteSpace(r.SchDate) && DateTime.TryParse(r.SchDate, out var d1)) return d1.Date;  // 排产日
            if (!string.IsNullOrWhiteSpace(r.OrderDate) && DateTime.TryParse(r.OrderDate, out var d2)) return d2.Date; // 订单日
            if (!string.IsNullOrWhiteSpace(r.CustReqDate) && DateTime.TryParse(r.CustReqDate, out var d3)) return d3.Date; // 要货日
            return null;
        }

        /// <summary>
        /// 刷新（全量重建）：按“修改日期窗口”从 ESB 分段拉取数据，按“排产日×阀体×产线”聚合，最后一次性清表并入库。
        /// 返回值：最终写入表中的“键行数”（即不同的 ProductionDate×ValveCategory×ProductionLine 的条目数）。
        /// </summary>
        public async Task<int> RefreshAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
        {
            await _refreshGate.WaitAsync(ct); // 防并发
            try
            {
                if (endDate < startDate)
                    throw new ArgumentException("endDate 不能早于 startDate");

                var client = _httpClientFactory.CreateClient("WZ");
                _logger.LogInformation("【WZ 刷新-全量重建】修改日窗口：{S} ~ {E}", startDate, endDate);

                // —— 全局聚合桶：跨分段累加（键 = 排产日×阀体×产线）
                var buckets = new Dictionary<(DateTime Date, string Cat, string Line), decimal>();

                // —— 分段按“修改日窗口”拉取
                foreach (var (S, E) in ChunkDates(startDate, endDate, ChunkDays))
                {
                    ct.ThrowIfCancellationRequested();

                    var payload = new
                    {
                        FSTARTDATE = S.ToString("yyyy-MM-dd"),
                        FENDDATE = E.ToString("yyyy-MM-dd")
                    };

                    using var req = new HttpRequestMessage(HttpMethod.Post, EsbUrl)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
                    };

                    using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
                    resp.EnsureSuccessStatusCode();

                    var json = await resp.Content.ReadAsStringAsync(ct);

                    // ESB 直接返回数组；若为包裹结构（如 {data:[...]})，这里需加一层模型解析
                    var rows = JsonConvert.DeserializeObject<List<EsbRow>>(json) ?? new List<EsbRow>();

                    // —— 本段内先做一次分组（按键求和），再汇入全局桶
                    var aggregates = rows
                        .Select(r => new
                        {
                            Date = PickDate(r), // 排产日（或回落日期）
                            Cat = NormalizeStr(r.ValveCategory),
                            Line = NormalizeStr(r.ProductionLine),
                            Qty = r.Qty ?? 0m
                        })
                        .Where(x => x.Date.HasValue && x.Cat.Length > 0 && x.Line.Length > 0)
                        .GroupBy(x => new { x.Date, x.Cat, x.Line })
                        .Select(g => new
                        {
                            Key = (Date: g.Key.Date!.Value, Cat: g.Key.Cat, Line: g.Key.Line),
                            Sum = g.Sum(z => z.Qty)
                        })
                        .ToList();

                    foreach (var a in aggregates)
                    {
                        if (buckets.TryGetValue(a.Key, out var cur))
                            buckets[a.Key] = cur + a.Sum;
                        else
                            buckets[a.Key] = a.Sum;
                    }

                    _logger.LogInformation("  ├─ 段 {S}~{E}：ESB行 {Raw}，聚合键 {Keys}，累计键 {Total}",
                        S.ToString("yyyy-MM-dd"), E.ToString("yyyy-MM-dd"),
                        rows.Count, aggregates.Count, buckets.Count);
                }

                // —— 开启事务：清空 + 分批插入
                using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    // ① 清空缓存表（TRUNCATE 优先；失败则 DELETE 兜底）
                    try
                    {
                        await _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [WZ_ProductionOutput];", ct);
                    }
                    catch
                    {
                        await _db.Database.ExecuteSqlRawAsync("DELETE FROM [WZ_ProductionOutput];", ct);
                    }

                    // ② 分批插入
                    var total = buckets.Count;
                    if (total > 0)
                    {
                        var batch = new List<WZ_ProductionOutput>(InsertBatchSize);
                        int written = 0;

                        foreach (var kv in buckets)
                        {
                            batch.Add(new WZ_ProductionOutput
                            {
                                ProductionDate = kv.Key.Date,
                                ValveCategory = kv.Key.Cat,
                                ProductionLine = kv.Key.Line,
                                Quantity = kv.Value
                            });

                            if (batch.Count >= InsertBatchSize)
                            {
                                await _db.Set<WZ_ProductionOutput>().AddRangeAsync(batch, ct);
                                await _db.SaveChangesAsync(ct);
                                written += batch.Count;
                                batch.Clear();
                                _logger.LogInformation("  ├─ 已入库 {Written}/{Total} 行 ...", written, total);
                            }
                        }

                        if (batch.Count > 0)
                        {
                            await _db.Set<WZ_ProductionOutput>().AddRangeAsync(batch, ct);
                            await _db.SaveChangesAsync(ct);
                            written += batch.Count;
                            _logger.LogInformation("  ├─ 已入库 {Written}/{Total} 行（收尾批）", written, total);
                        }
                    }

                    await tx.CommitAsync(ct);
                    _logger.LogInformation("【WZ 刷新完成】最终入库键数：{N}", total);
                    return total;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            }
            finally
            {
                _refreshGate.Release();
            }
        }

        /// <summary>
        /// 查询：按阀体、产线、日期范围返回每日产量（从本地缓存表直接读取）
        /// 说明：这里的日期范围是针对 ProductionDate（排产日）的业务查询窗口。
        /// </summary>
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

            query = query.OrderBy(x => x.ProductionDate)
                         .ThenBy(x => x.ValveCategory)
                         .ThenBy(x => x.ProductionLine);

            return query.ToListAsync(ct);
        }

        /// <summary>
        /// 批量写入阈值：按阀体+产线更新 CurrentThreshold
        /// </summary>
        public async Task<int> UpdateThresholdsAsync(
            IReadOnlyCollection<(string ValveCategory, string ProductionLine, decimal Threshold)> thresholds,
            CancellationToken ct = default)
        {
            if (thresholds == null || thresholds.Count == 0) return 0;

            var affected = 0;
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                foreach (var item in thresholds)
                {
                    var valve = NormalizeStr(item.ValveCategory);
                    var line = NormalizeStr(item.ProductionLine);
                    if (valve.Length == 0 || line.Length == 0) continue;

                    affected += await _db.Database.ExecuteSqlRawAsync(
                        "UPDATE [WZ_ProductionOutput] SET [CurrentThreshold] = {0} WHERE [ValveCategory] = {1} AND [ProductionLine] = {2};",
                        item.Threshold, valve, line);
                }

                await tx.CommitAsync(ct);
                return affected;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// 汇总预排产输出表：按 ProductionDate × 阀体 × 产线聚合，写入 WZ_PreProductionOutput_1
        /// </summary>
        private async Task<int> RefreshPreProductionOutputAsync(CancellationToken ct = default)
        {
            await _preProductionGate.WaitAsync(ct);
            try
            {
                var rows = await _db.Set<WZ_PreProductionOutput>()
                    .AsNoTracking()
                    .Select(p => new
                    {
                        p.ProductionDate,
                        p.ValveCategory,
                        p.ProductionLine,
                        p.Quantity
                    })
                    .ToListAsync(ct);

                var buckets = rows
                    .Where(r => r.ProductionDate.HasValue)
                    .Select(r => new
                    {
                        Date = r.ProductionDate!.Value.Date,
                        Cat = NormalizeStr(r.ValveCategory),
                        Line = NormalizeStr(r.ProductionLine),
                        Qty = r.Quantity
                    })
                    .Where(x => x.Cat.Length > 0 && x.Line.Length > 0)
                    .GroupBy(x => new { x.Date, x.Cat, x.Line })
                    .Select(g => new
                    {
                        g.Key.Date,
                        g.Key.Cat,
                        g.Key.Line,
                        Sum = g.Sum(z => z.Qty)
                    })
                    .ToList();

                using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    try
                    {
                        await _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [WZ_PreProductionOutput_1];", ct);
                    }
                    catch
                    {
                        await _db.Database.ExecuteSqlRawAsync("DELETE FROM [WZ_PreProductionOutput_1];", ct);
                    }

                    if (buckets.Count > 0)
                    {
                        var batch = new List<WZ_PreProductionOutput_1>(InsertBatchSize);
                        foreach (var item in buckets)
                        {
                            batch.Add(new WZ_PreProductionOutput_1
                            {
                                ProductionDate = item.Date,
                                ValveCategory = item.Cat,
                                ProductionLine = item.Line,
                                Quantity = item.Sum
                            });

                            if (batch.Count >= InsertBatchSize)
                            {
                                await _db.Set<WZ_PreProductionOutput_1>().AddRangeAsync(batch, ct);
                                await _db.SaveChangesAsync(ct);
                                batch.Clear();
                            }
                        }

                        if (batch.Count > 0)
                        {
                            await _db.Set<WZ_PreProductionOutput_1>().AddRangeAsync(batch, ct);
                            await _db.SaveChangesAsync(ct);
                        }
                    }

                    await tx.CommitAsync(ct);
                    return buckets.Count;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            }
            finally
            {
                _preProductionGate.Release();
            }
        }

        /// <summary>
        /// 查询：合并实际产量与预排产汇总数据
        /// </summary>
        public async Task<List<WZ_ProductionOutput>> GetWithPreProductionAsync(
            string valveCategory,
            string productionLine,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate 不能早于 startDate");

            await RefreshPreProductionOutputAsync(ct);

            var actualQuery = _db.Set<WZ_ProductionOutput>()
                .AsNoTracking()
                .Where(x => x.ProductionDate >= startDate && x.ProductionDate <= endDate);

            var preQuery = _db.Set<WZ_PreProductionOutput_1>()
                .AsNoTracking()
                .Where(x => x.ProductionDate >= startDate && x.ProductionDate <= endDate);

            if (!string.IsNullOrWhiteSpace(valveCategory))
            {
                actualQuery = actualQuery.Where(x => x.ValveCategory == valveCategory);
                preQuery = preQuery.Where(x => x.ValveCategory == valveCategory);
            }

            if (!string.IsNullOrWhiteSpace(productionLine))
            {
                actualQuery = actualQuery.Where(x => x.ProductionLine == productionLine);
                preQuery = preQuery.Where(x => x.ProductionLine == productionLine);
            }

            var actualRows = await actualQuery.ToListAsync(ct);
            var preRows = await preQuery.ToListAsync(ct);

            var merged = new Dictionary<(DateTime Date, string Cat, string Line), WZ_ProductionOutput>();

            foreach (var row in actualRows)
            {
                var key = (row.ProductionDate.Date, NormalizeStr(row.ValveCategory), NormalizeStr(row.ProductionLine));
                merged[key] = new WZ_ProductionOutput
                {
                    ProductionDate = row.ProductionDate.Date,
                    ValveCategory = key.Item2,
                    ProductionLine = key.Item3,
                    Quantity = row.Quantity,
                    CurrentThreshold = row.CurrentThreshold
                };
            }

            foreach (var row in preRows)
            {
                var key = (row.ProductionDate.Date, NormalizeStr(row.ValveCategory), NormalizeStr(row.ProductionLine));
                if (merged.TryGetValue(key, out var existing))
                {
                    existing.Quantity += row.Quantity;
                }
                else
                {
                    merged[key] = new WZ_ProductionOutput
                    {
                        ProductionDate = row.ProductionDate.Date,
                        ValveCategory = key.Item2,
                        ProductionLine = key.Item3,
                        Quantity = row.Quantity
                    };
                }
            }

            return merged.Values
                .OrderBy(x => x.ProductionDate)
                .ThenBy(x => x.ValveCategory)
                .ThenBy(x => x.ProductionLine)
                .ToList();
        }
    }
}
