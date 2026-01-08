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
        private const string EsbUrl = "http://10.11.0.101:8003/gateway/DataCenter/CXCNSJ";
        private const int ChunkDays = 7;  // 修改日窗口切片长度（可按 ESB 性能调整）
        private const int InsertBatchSize = 2000; // 大批量入库时的分批大小
        private const int AssignBatchDefaultSize = 1000;

        private const string AssignedProductionLineSql = @"
UPDATE dbo.WZ_ProductionOutput
SET AssignedProductionLine =
    CASE
        WHEN ProductionLine IS NOT NULL AND LTRIM(RTRIM(ProductionLine)) <> N'' THEN
            CASE
                WHEN ProductionLine LIKE N'旋转%' AND ValveCategory = N'蝶阀' THEN
                    CASE
                        WHEN LTRIM(RTRIM(NominalDiameter)) IN (N'DN50', N'DN65', N'DN80', N'DN100', N'DN125', N'DN150') THEN N'蝶阀1'
                        WHEN LTRIM(RTRIM(NominalDiameter)) IN (N'DN200', N'DN250', N'DN300', N'DN100') THEN N'蝶阀2'
                        WHEN LTRIM(RTRIM(NominalDiameter)) IN (N'DN350', N'DN400', N'DN450', N'DN500', N'DN600') THEN N'蝶阀3'
                        ELSE N'蝶阀4'
                    END
                WHEN ProductionLine IN (N'旋转1', N'旋转2', N'旋转3', N'旋转4', N'旋转5') AND ValveCategory = N'软密封球阀'
                    THEN ProductionLine + N'A'
                ELSE NULL
            END
        ELSE NULL
    END
WHERE ProductionLine IS NOT NULL AND LTRIM(RTRIM(ProductionLine)) <> N'';
";

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
        /// 规则直判：计算 AssignedProductionLine
        /// </summary>
        public static string CalcAssignedProductionLine(string productionLine, string valveCategory, string nominalDiameter)
        {
            var line = string.IsNullOrWhiteSpace(productionLine) ? null : productionLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                return null;

            var category = string.IsNullOrWhiteSpace(valveCategory) ? null : valveCategory.Trim();
            var dn = string.IsNullOrWhiteSpace(nominalDiameter) ? string.Empty : nominalDiameter.Trim();

            if (line.StartsWith("旋转", StringComparison.Ordinal) &&
                string.Equals(category, "蝶阀", StringComparison.Ordinal))
            {
                if (dn == "DN50" || dn == "DN65" || dn == "DN80" || dn == "DN100" || dn == "DN125" || dn == "DN150")
                    return "蝶阀1";

                if (dn == "DN200" || dn == "DN250" || dn == "DN300" || dn == "DN100")
                    return "蝶阀2";

                if (dn == "DN350" || dn == "DN400" || dn == "DN450" || dn == "DN500" || dn == "DN600")
                    return "蝶阀3";

                return "蝶阀4";
            }

            if ((line == "旋转1" || line == "旋转2" || line == "旋转3" || line == "旋转4" || line == "旋转5") &&
                string.Equals(category, "软密封球阀", StringComparison.Ordinal))
            {
                return $"{line}A";
            }

            return null;
        }

        /// <summary>
        /// 获取规则直判 SQL 更新语句
        /// </summary>
        public string GetAssignedProductionLineSql() => AssignedProductionLineSql;

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
        /// 规则直判：批量计算并更新 AssignedProductionLine
        /// </summary>
        public async Task<WZProductionOutputAssignResult> AssignProductionLineAsync(int batchSize, CancellationToken ct = default)
        {
            var effectiveBatchSize = batchSize > 0 ? batchSize : AssignBatchDefaultSize;
            var result = new WZProductionOutputAssignResult { BatchSize = effectiveBatchSize };

            int lastId = 0;
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var batch = await _db.Set<WZ_ProductionOutput>()
                    .Where(x => x.Id > lastId && !string.IsNullOrEmpty(x.ProductionLine))
                    .OrderBy(x => x.Id)
                    .Take(effectiveBatchSize)
                    .ToListAsync(ct);

                if (batch.Count == 0)
                    break;

                result.TotalCount += batch.Count;

                using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    var updatedInBatch = 0;
                    foreach (var item in batch)
                    {
                        try
                        {
                            var assigned = CalcAssignedProductionLine(item.ProductionLine, item.ValveCategory, item.NominalDiameter);
                            var normalizedAssigned = string.IsNullOrWhiteSpace(assigned) ? null : assigned;
                            var currentAssigned = string.IsNullOrWhiteSpace(item.AssignedProductionLine) ? null : item.AssignedProductionLine;

                            if (!string.Equals(currentAssigned, normalizedAssigned, StringComparison.Ordinal))
                            {
                                item.AssignedProductionLine = normalizedAssigned;
                                updatedInBatch++;
                                result.UpdatedCount++;
                            }
                            else
                            {
                                result.SkippedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.ErrorCount++;
                            _logger.LogError(ex, "AssignedProductionLine 计算异常，Id={Id}", item.Id);
                        }
                    }

                    if (updatedInBatch > 0)
                        await _db.SaveChangesAsync(ct);

                    await tx.CommitAsync(ct);
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync(ct);
                    _logger.LogError(ex, "AssignedProductionLine 批量更新失败，批次起始Id={StartId}", batch.First().Id);
                    throw;
                }
                finally
                {
                    _db.ChangeTracker.Clear();
                }

                lastId = batch[^1].Id;
            }

            _logger.LogInformation("AssignedProductionLine 批量更新完成，总数 {Total}，更新 {Updated}，跳过 {Skipped}，异常 {Error}",
                result.TotalCount, result.UpdatedCount, result.SkippedCount, result.ErrorCount);

            return result;
        }
    }
}
