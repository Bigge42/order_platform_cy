using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HDPro.Core.Utilities;
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
    /// - 先按订单明细唯一键写入 WZ_ProductionOutputDetail，避免 ESB 重复行放大产能；
    /// - 再从明细表按 ProductionDate × 阀体类别 × 产线重算 WZ_ProductionOutput 缓存；
    /// - 增量同步也保持幂等：更新明细后重算汇总，不做 Quantity 累加。
    /// 
    /// 命名客户端：
    /// - 请在 Program.cs/Startup.cs 中注册： services.AddHttpClient("WZ", c => { c.Timeout = TimeSpan.FromMinutes(2); });
    /// </summary>
    public partial class WZProductionOutputService : IWZProductionOutputService, IDependency
    {
        private static readonly SemaphoreSlim _refreshGate = new(1, 1); // 防并发刷新
        private static readonly SemaphoreSlim _preProductionGate = new(1, 1); // 防并发汇总预排产
        private static readonly SemaphoreSlim _preProductionOptimizeGate = new(1, 1); // 防并发汇总排产优化
        private const string EsbUrl = "http://10.11.0.101:8003/gateway/DataCenter/CXCNSJ";
        private const int ChunkDays = 7;  // 修改日窗口切片长度（可按 ESB 性能调整）
        private const int MaxEsbConcurrentRequests = 4; // 分片请求并发上限，避免全年同步串行等待
        private const int InsertBatchSize = 2000; // 大批量入库时的分批大小
        private const int MaxEsbRetryCount = 3; // 单个时间片最大重试次数
        private const int EsbRetryDelayMilliseconds = 1500; // 失败重试基础等待时长
        private const string ValveRuleServiceUrl = "http://10.11.10.101:8000/batch_infer?debug_trace=false";
        private const int DefaultValveRuleBatchSize = 200;
        private const int DefaultValveRuleRequestTimeoutSeconds = 120;
        private const int DefaultValveRuleTotalTimeoutSeconds = 1800;
        private static readonly int ValveRuleBatchSize = GetPositiveIntEnvironmentVariable("HDPRO_WZ_RULE_BATCH_SIZE", DefaultValveRuleBatchSize);
        private static readonly int ValveRuleRequestTimeoutSeconds = GetPositiveIntEnvironmentVariable("HDPRO_WZ_RULE_REQUEST_TIMEOUT_SECONDS", DefaultValveRuleRequestTimeoutSeconds);
        private static readonly int ValveRuleTotalTimeoutSeconds = GetPositiveIntEnvironmentVariable("HDPRO_WZ_RULE_TOTAL_TIMEOUT_SECONDS", DefaultValveRuleTotalTimeoutSeconds);
        private const string DetailStatusMatched = "matched";
        private const string DetailStatusMatchedByOrderCycle = "matched_order_cycle";
        private const string DetailStatusMatchedBySyncLine = "matched_sync_line";
        private const string DetailStatusMatchedByRule = "matched_rule";
        private const string DetailStatusMatchedByManual = "matched_manual";
        private const string DetailStatusMissingLine = "missing_line";
        private const string DetailStatusConflict = "conflict";
        private const string UnknownValveCategory = "未知阀类";
        private const string UnknownProductionLine = "未知产线";
        private const string BillPlanKeySeparator = "\u001F";

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
            [JsonProperty("FENTRYID")] public long? EntryId { get; set; } // 销售订单明细 ID
            [JsonProperty("FBILLNO")] public string BillNo { get; set; } = string.Empty; // 单据编号
            [JsonProperty("FMTONO")] public string PlanTrackingNo { get; set; } = string.Empty; // 计划跟踪号
            [JsonProperty("FSEQ")] public int? Seq { get; set; } // 行号
            [JsonProperty("FMATERIALID")] public string MaterialId { get; set; } = string.Empty; // 物料 ID
            [JsonProperty("FNUMBER")] public string MaterialCode { get; set; } = string.Empty; // 物料编码（部分接口返回）
            [JsonProperty("F_ORA_FMLB")] public string ValveCategory { get; set; } = string.Empty;   // 阀门类别
            [JsonProperty("F_ORA_SCX")] public string ProductionLine { get; set; } = string.Empty;  // 生产线
            [JsonProperty("FQTY")] public decimal? Qty { get; set; }           // 订单数量

            // 日期字段：优先 排产日期 F_ORA_DATE1，其次订单日期 & 要货日期
            [JsonProperty("F_ORA_DATE1")] public string SchDate { get; set; } = string.Empty;         // 排产日期（推荐映射 ProductionDate）
            [JsonProperty("FDATE")] public string OrderDate { get; set; } = string.Empty;       // 订单日期
            [JsonProperty("F_ORA_DATETIME")] public string CustReqDate { get; set; } = string.Empty;     // 客户要货日期
        }

        private sealed class ParsedProductionOutputRow
        {
            public string BusinessKey { get; set; } = string.Empty;
            public long? EntryId { get; set; }
            public string BillNo { get; set; } = string.Empty;
            public string PlanTrackingNo { get; set; } = string.Empty;
            public int? Seq { get; set; }
            public string MaterialKey { get; set; } = string.Empty;
            public string MaterialCode { get; set; } = string.Empty;
            public string MaterialId { get; set; } = string.Empty;
            public string SpecModel { get; set; } = string.Empty;
            public string ProductModel { get; set; } = string.Empty;
            public DateTime? ProductionDate { get; set; }
            public string ValveCategory { get; set; } = string.Empty;
            public string ProductionLine { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
        }

        private sealed class ProductionOutputDetailRow
        {
            public string BusinessKey { get; set; } = string.Empty;
            public long? EntryId { get; set; }
            public string BillNo { get; set; } = string.Empty;
            public string PlanTrackingNo { get; set; } = string.Empty;
            public int? Seq { get; set; }
            public string MaterialKey { get; set; } = string.Empty;
            public string MaterialCode { get; set; } = string.Empty;
            public string MaterialId { get; set; } = string.Empty;
            public string SpecModel { get; set; } = string.Empty;
            public string ProductModel { get; set; } = string.Empty;
            public DateTime ProductionDate { get; set; }
            public string ValveCategory { get; set; } = string.Empty;
            public string ProductionLine { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public string ClassifyStatus { get; set; } = string.Empty;
            public int RawRowCount { get; set; }
            public int LineCandidateCount { get; set; }
            public DateTime SourceStartDate { get; set; }
            public DateTime SourceEndDate { get; set; }
        }

        private sealed class OrderCycleLineCandidate
        {
            public long? EntryId { get; set; }
            public string SalesOrderNo { get; set; } = string.Empty;
            public string PlanTrackingNo { get; set; } = string.Empty;
            public string MaterialKey { get; set; } = string.Empty;
            public List<string> MaterialKeys { get; set; } = new();
            public string ValveCategory { get; set; } = string.Empty;
            public string ProductionLine { get; set; } = string.Empty;
            public string AssignedProductionLine { get; set; } = string.Empty;
            public DateTime? ProductionDate { get; set; }
            public string NominalDiameter { get; set; } = string.Empty;
            public string NominalPressure { get; set; } = string.Empty;
            public string SpecModel { get; set; } = string.Empty;
            public string ProductName { get; set; } = string.Empty;
            public bool IsRuleServiceCandidate { get; set; }
            public bool IsSyncProductionLineCandidate { get; set; }
            public bool ResolvedByRuleService { get; set; }
            public string MatchKey { get; set; } = string.Empty;
            public DateTime? OrderApprovedDate { get; set; }
            public DateTime? ReplyDeliveryDate { get; set; }
            public DateTime? RequestedDeliveryDate { get; set; }
            public string BodyMaterial { get; set; } = string.Empty;
            public string InnerMaterial { get; set; } = string.Empty;
            public string FlangeConnection { get; set; } = string.Empty;
            public string BonnetForm { get; set; } = string.Empty;
            public string FlowCharacteristic { get; set; } = string.Empty;
            public string Actuator { get; set; } = string.Empty;
            public string AccessoryConfig { get; set; } = string.Empty;
            public string OutsourcedValveBody { get; set; } = string.Empty;
            public string ValveCategory1 { get; set; } = string.Empty;
            public string SealFaceForm { get; set; } = string.Empty;
            public string SpecialProduct { get; set; } = string.Empty;
            public string PurchaseFlag { get; set; } = string.Empty;
        }

        private sealed class OrderTrackingMaterialRow
        {
            public long? TrackingId { get; set; }
            public long? EntryId { get; set; }
            public string SalesOrderNo { get; set; } = string.Empty;
            public string PlanTrackingNo { get; set; } = string.Empty;
            public long? MaterialId { get; set; }
            public string MaterialCode { get; set; } = string.Empty;
            public string MaterialName { get; set; } = string.Empty;
            public string SpecModel { get; set; } = string.Empty;
            public string ProductModel { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public DateTime? ProductionDate { get; set; }
            public DateTime? OrderApprovedDate { get; set; }
            public DateTime? ReplyDeliveryDate { get; set; }
            public DateTime? RequestedDeliveryDate { get; set; }
        }

        private sealed class ResolvedLineAssignment
        {
            public long? EntryId { get; set; }
            public string BillPlanKey { get; set; } = string.Empty;
            public List<string> MaterialKeys { get; set; } = new();
            public bool AllowBillPlanFallback { get; set; } = true;
            public string ValveCategory { get; set; } = string.Empty;
            public string ProductionLine { get; set; } = string.Empty;
            public DateTime? ProductionDate { get; set; }
            public string SpecModel { get; set; } = string.Empty;
            public string ProductModel { get; set; } = string.Empty;
            public string ClassifyStatus { get; set; } = string.Empty;
            public int Score { get; set; }
        }

        private sealed class DetailBackfillSummary
        {
            public int Candidates { get; set; }
            public int FilledByManual { get; set; }
            public int FilledByOrderCycle { get; set; }
            public int FilledBySyncLine { get; set; }
            public int FilledByRule { get; set; }
            public int RemainingMissingLine { get; set; }
            public int RemainingConflict { get; set; }
        }

        private sealed class ValveLineRuleRequest
        {
            [JsonProperty("id")] public string Id { get; set; } = string.Empty;
            [JsonProperty("OrderApprovedDate")] public DateTime? OrderApprovedDate { get; set; }
            [JsonProperty("ReplyDeliveryDate")] public DateTime? ReplyDeliveryDate { get; set; }
            [JsonProperty("RequestedDeliveryDate")] public DateTime? RequestedDeliveryDate { get; set; }
            [JsonProperty("fa_ti_cai_zhi")] public string BodyMaterial { get; set; } = string.Empty;
            [JsonProperty("nei_jian_cai_zhi")] public string InnerMaterial { get; set; } = string.Empty;
            [JsonProperty("fa_lan_lian_jie")] public string FlangeConnection { get; set; } = string.Empty;
            [JsonProperty("shang_gai_xing_shi")] public string BonnetForm { get; set; } = string.Empty;
            [JsonProperty("liu_liang_te_xing")] public string FlowCharacteristic { get; set; } = string.Empty;
            [JsonProperty("zhi_xing_ji_gou")] public string Actuator { get; set; } = string.Empty;
            [JsonProperty("fu_jian_pei_zhi")] public string AccessoryConfig { get; set; } = string.Empty;
            [JsonProperty("wai_gou_fa_ti")] public string OutsourcedValveBody { get; set; } = string.Empty;
            [JsonProperty("fa_men_da_lei")] public string ValveCategory1 { get; set; } = string.Empty;
            [JsonProperty("fa_men_lei_bie")] public string ValveCategory { get; set; } = string.Empty;
            [JsonProperty("mi_feng_mian_xing_shi")] public string SealFaceForm { get; set; } = string.Empty;
            [JsonProperty("te_pin")] public string SpecialProduct { get; set; } = string.Empty;
            [JsonProperty("wai_gou_biao_zhi")] public string PurchaseFlag { get; set; } = string.Empty;
            [JsonProperty("chan_pin_ming_cheng")] public string ProductName { get; set; } = string.Empty;
            [JsonProperty("gong_cheng_tong_jing")] public string NominalDiameter { get; set; } = string.Empty;
            [JsonProperty("gong_cheng_ya_li")] public string NominalPressure { get; set; } = string.Empty;
        }

        private sealed class ValveLineRuleResult
        {
            [JsonProperty("id")] public string Id { get; set; } = string.Empty;
            [JsonProperty("sheng_chan_xian")] public string ProductionLine { get; set; } = string.Empty;
        }

        private sealed class ValveLineRuleResponseItem
        {
            [JsonProperty("id")] public string Id { get; set; } = string.Empty;
            [JsonProperty("success")] public bool Success { get; set; }
            [JsonProperty("result")] public ValveLineRuleResult Result { get; set; }
        }

        private sealed class ValveLineRuleBatchResponse
        {
            [JsonProperty("results")] public List<ValveLineRuleResponseItem> Results { get; set; } = new List<ValveLineRuleResponseItem>();
            [JsonProperty("log_file")] public string LogFile { get; set; } = string.Empty;
        }

        private sealed class RuleLineCacheEntry
        {
            public bool Success { get; set; }
            public string ProductionLine { get; set; } = string.Empty;
        }

        private enum RuleProductTextMode
        {
            SpecModelFirst,
            ProductNameFallback
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

        private static int GetPositiveIntEnvironmentVariable(string name, int defaultValue)
        {
            var raw = Environment.GetEnvironmentVariable(name);
            return int.TryParse(raw, out var value) && value > 0 ? value : defaultValue;
        }

        private static string NormalizeSyncProductionLineCandidate(string value)
        {
            var line = NormalizeStr(value);
            if (line.Length == 0 || line.Contains("车间", StringComparison.Ordinal))
            {
                return string.Empty;
            }

            return line.StartsWith("旋转", StringComparison.Ordinal)
                || line.StartsWith("直通", StringComparison.Ordinal)
                || line.StartsWith("蝶阀", StringComparison.Ordinal)
                ? line
                : string.Empty;
        }

        private static string BuildBillPlanKey(string billNo, string planTrackingNo)
            => $"{NormalizeStr(billNo)}{BillPlanKeySeparator}{NormalizeStr(planTrackingNo)}";

        private static string BuildBillPlanMaterialKey(string billNo, string planTrackingNo, string materialKey)
            => $"{BuildBillPlanKey(billNo, planTrackingNo)}{BillPlanKeySeparator}{NormalizeStr(materialKey)}";

        private static string BuildBillPlanMaterialKeyFromBillPlanKey(string billPlanKey, string materialKey)
            => $"{NormalizeStr(billPlanKey)}{BillPlanKeySeparator}{NormalizeStr(materialKey)}";

        private static string MaterialIdToKey(long? materialId)
            => materialId.HasValue && materialId.Value > 0 ? materialId.Value.ToString() : string.Empty;

        private static string BuildMaterialKey(string materialCode, string materialId)
        {
            var material = NormalizeStr(materialCode);
            if (material.Length == 0)
            {
                material = NormalizeStr(materialId);
            }

            return material;
        }

        private static string ExtractMaterialKeyFromBusinessKey(string businessKey)
        {
            var key = NormalizeStr(businessKey);
            if (key.Length == 0)
            {
                return string.Empty;
            }

            const string materialMarker = "|M:";
            const string entryMarker = "|E:";
            var materialStart = key.IndexOf(materialMarker, StringComparison.Ordinal);
            if (materialStart < 0)
            {
                return string.Empty;
            }

            materialStart += materialMarker.Length;
            var materialEnd = key.IndexOf(entryMarker, materialStart, StringComparison.Ordinal);
            if (materialEnd < 0)
            {
                materialEnd = key.Length;
            }

            return materialEnd > materialStart
                ? NormalizeStr(key.Substring(materialStart, materialEnd - materialStart))
                : string.Empty;
        }

        private static string NormalizeMaterialKey(string value)
            => NormalizeStr(value);

        private static List<string> BuildMaterialKeys(params string[] values)
        {
            var result = new List<string>();
            if (values == null)
            {
                return result;
            }

            foreach (var value in values)
            {
                var key = NormalizeMaterialKey(value);
                if (key.Length > 0 && !result.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    result.Add(key);
                }
            }

            return result;
        }

        private static List<string> BuildMaterialKeys(OCP_Material material, params string[] values)
        {
            var result = BuildMaterialKeys(values);
            if (material != null)
            {
                foreach (var key in BuildMaterialKeys(material.MaterialCode, material.MaterialID > 0 ? material.MaterialID.ToString() : string.Empty))
                {
                    if (!result.Contains(key, StringComparer.OrdinalIgnoreCase))
                    {
                        result.Add(key);
                    }
                }
            }

            return result;
        }

        private static List<string> GetDetailMaterialKeys(ProductionOutputDetailRow detail)
        {
            if (detail == null)
            {
                return new List<string>();
            }

            return BuildMaterialKeys(
                detail.MaterialKey,
                detail.MaterialCode,
                detail.MaterialId,
                ExtractMaterialKeyFromBusinessKey(detail.BusinessKey));
        }

        private static List<string> GetOrderTrackingMaterialKeys(OrderTrackingMaterialRow row)
        {
            if (row == null)
            {
                return new List<string>();
            }

            return BuildMaterialKeys(row.MaterialCode, MaterialIdToKey(row.MaterialId));
        }

        private static int ScoreOrderTrackingMaterialRow(OrderTrackingMaterialRow row)
        {
            if (row == null)
            {
                return 0;
            }

            var score = 0;
            if (row.MaterialId.HasValue && row.MaterialId.Value > 0) score += 8;
            if (NormalizeStr(row.MaterialCode).Length > 0) score += 6;
            if (NormalizeStr(row.SpecModel).Length > 0) score += 4;
            if (NormalizeStr(row.ProductModel).Length > 0) score += 3;
            if (NormalizeStr(row.MaterialName).Length > 0) score += 1;
            if (row.ProductionDate.HasValue) score += 2;
            if (row.EntryId.HasValue && row.EntryId.Value > 0) score += 2;
            return score;
        }

        private static OrderTrackingMaterialRow PickBestOrderTrackingMaterialRow(IEnumerable<OrderTrackingMaterialRow> rows)
            => rows?
                .Where(x => x != null)
                .OrderByDescending(ScoreOrderTrackingMaterialRow)
                .ThenByDescending(x => x.TrackingId ?? 0)
                .FirstOrDefault();

        private static bool IsSummarizableStatus(string status)
            => string.Equals(status, DetailStatusMatched, StringComparison.Ordinal)
                || string.Equals(status, DetailStatusMatchedByOrderCycle, StringComparison.Ordinal)
                || string.Equals(status, DetailStatusMatchedBySyncLine, StringComparison.Ordinal)
                || string.Equals(status, DetailStatusMatchedByRule, StringComparison.Ordinal)
                || string.Equals(status, DetailStatusMatchedByManual, StringComparison.Ordinal);

        private static string BuildBusinessKey(EsbRow row)
        {
            if (row == null)
            {
                return string.Empty;
            }

            var billNo = NormalizeStr(row.BillNo);
            var planTrackingNo = NormalizeStr(row.PlanTrackingNo);
            var seq = row.Seq.HasValue ? row.Seq.Value.ToString() : string.Empty;
            var materialKey = BuildMaterialKey(row.MaterialCode, row.MaterialId);
            var entryKey = row.EntryId.HasValue && row.EntryId.Value > 0
                ? row.EntryId.Value.ToString()
                : string.Empty;

            if (billNo.Length > 0 || planTrackingNo.Length > 0 || seq.Length > 0 || materialKey.Length > 0)
            {
                return $"B:{billNo}|P:{planTrackingNo}|S:{seq}|M:{materialKey}|E:{entryKey}";
            }

            return entryKey.Length > 0 ? $"E:{entryKey}" : string.Empty;
        }

        private static string BuildOrderTrackingBusinessKey(OrderTrackingMaterialRow row)
        {
            if (row == null)
            {
                return string.Empty;
            }

            var billNo = NormalizeStr(row.SalesOrderNo);
            var planTrackingNo = NormalizeStr(row.PlanTrackingNo);
            var materialKey = BuildMaterialKey(row.MaterialCode, MaterialIdToKey(row.MaterialId));
            var entryKey = row.EntryId.HasValue && row.EntryId.Value > 0
                ? row.EntryId.Value.ToString()
                : string.Empty;
            var trackingKey = row.TrackingId.HasValue && row.TrackingId.Value > 0
                ? row.TrackingId.Value.ToString()
                : string.Empty;

            if (billNo.Length > 0 || planTrackingNo.Length > 0 || materialKey.Length > 0)
            {
                return $"OCP:B:{billNo}|P:{planTrackingNo}|M:{materialKey}|E:{entryKey}|ID:{trackingKey}";
            }

            if (entryKey.Length > 0)
            {
                return $"OCP:E:{entryKey}";
            }

            return trackingKey.Length > 0 ? $"OCP:ID:{trackingKey}" : string.Empty;
        }

        private static ParsedProductionOutputRow ParseProductionOutputRow(EsbRow row)
        {
            return new ParsedProductionOutputRow
            {
                BusinessKey = BuildBusinessKey(row),
                EntryId = row.EntryId,
                BillNo = NormalizeStr(row.BillNo),
                PlanTrackingNo = NormalizeStr(row.PlanTrackingNo),
                Seq = row.Seq,
                MaterialKey = BuildMaterialKey(row.MaterialCode, row.MaterialId),
                MaterialCode = NormalizeStr(row.MaterialCode),
                MaterialId = NormalizeStr(row.MaterialId),
                SpecModel = string.Empty,
                ProductModel = string.Empty,
                ProductionDate = PickDate(row),
                ValveCategory = NormalizeStr(row.ValveCategory),
                ProductionLine = NormalizeStr(row.ProductionLine),
                Quantity = row.Qty ?? 0m
            };
        }

        private static ProductionOutputDetailRow BuildDetailRow(
            IGrouping<string, ParsedProductionOutputRow> group,
            DateTime sourceStartDate,
            DateTime sourceEndDate)
        {
            var rows = group.ToList();
            var first = rows.First();
            var lineGroups = rows
                .Where(x => x.ProductionDate.HasValue
                    && x.ValveCategory.Length > 0
                    && x.ProductionLine.Length > 0)
                .GroupBy(x => new
                {
                    Date = x.ProductionDate!.Value.Date,
                    x.ValveCategory,
                    x.ProductionLine,
                    x.Quantity
                })
                .Select(g => new
                {
                    g.Key.Date,
                    g.Key.ValveCategory,
                    g.Key.ProductionLine,
                    g.Key.Quantity,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Date)
                .ThenBy(x => x.ValveCategory)
                .ThenBy(x => x.ProductionLine)
                .ToList();

            var fallback = rows
                .Where(x => x.ProductionDate.HasValue)
                .OrderBy(x => x.ProductionDate!.Value)
                .FirstOrDefault() ?? first;

            var chosen = lineGroups.FirstOrDefault();
            var status = lineGroups.Count == 0
                ? DetailStatusMissingLine
                : lineGroups.Count == 1
                    ? DetailStatusMatched
                    : DetailStatusConflict;

            return new ProductionOutputDetailRow
            {
                BusinessKey = group.Key,
                EntryId = first.EntryId,
                BillNo = first.BillNo,
                PlanTrackingNo = first.PlanTrackingNo,
                Seq = first.Seq,
                MaterialKey = first.MaterialKey,
                MaterialCode = first.MaterialCode,
                MaterialId = first.MaterialId,
                SpecModel = first.SpecModel,
                ProductModel = first.ProductModel,
                ProductionDate = chosen?.Date ?? fallback.ProductionDate!.Value.Date,
                ValveCategory = chosen?.ValveCategory ?? string.Empty,
                ProductionLine = chosen?.ProductionLine ?? string.Empty,
                Quantity = chosen?.Quantity ?? fallback.Quantity,
                ClassifyStatus = status,
                RawRowCount = rows.Count,
                LineCandidateCount = lineGroups.Count,
                SourceStartDate = sourceStartDate.Date,
                SourceEndDate = sourceEndDate.Date
            };
        }

        private static (List<ProductionOutputDetailRow> Details, int SkippedNoDate, int SkippedNoKey, int Matched, int MissingLine, int Conflict)
            BuildDetailRows(IEnumerable<EsbRow> rows, DateTime sourceStartDate, DateTime sourceEndDate)
        {
            var parsedRows = rows
                .Select(ParseProductionOutputRow)
                .ToList();

            var skippedNoDate = parsedRows.Count(x => !x.ProductionDate.HasValue);
            var skippedNoKey = parsedRows.Count(x => x.ProductionDate.HasValue && x.BusinessKey.Length == 0);

            var details = parsedRows
                .Where(x => x.ProductionDate.HasValue && x.BusinessKey.Length > 0)
                .GroupBy(x => x.BusinessKey)
                .Select(g => BuildDetailRow(g, sourceStartDate, sourceEndDate))
                .ToList();

            return (
                details,
                skippedNoDate,
                skippedNoKey,
                details.Count(x => IsSummarizableStatus(x.ClassifyStatus)),
                details.Count(x => x.ClassifyStatus == DetailStatusMissingLine),
                details.Count(x => x.ClassifyStatus == DetailStatusConflict));
        }

        private static IEnumerable<List<T>> ChunkList<T>(IReadOnlyList<T> values, int chunkSize)
        {
            if (values == null || values.Count == 0)
            {
                yield break;
            }

            if (chunkSize <= 0)
            {
                chunkSize = 1000;
            }

            for (var index = 0; index < values.Count; index += chunkSize)
            {
                var take = Math.Min(chunkSize, values.Count - index);
                var chunk = new List<T>(take);
                for (var i = 0; i < take; i++)
                {
                    chunk.Add(values[index + i]);
                }

                yield return chunk;
            }
        }

        private static ResolvedLineAssignment? ResolveOrderCycleLineCandidate(OrderCycleLineCandidate candidate)
        {
            if (candidate == null)
            {
                return null;
            }

            var valveCategory = NormalizeStr(candidate.ValveCategory);
            var usedRule = false;

            if (valveCategory.Length == 0)
            {
                var categoryRule = ValveCategoryRuleJudge.TryJudgeBySpecOrProduct(candidate.SpecModel, candidate.ProductName);
                if (categoryRule.HasValue)
                {
                    valveCategory = NormalizeStr(categoryRule.Value.Category);
                    usedRule = valveCategory.Length > 0;
                }
            }

            var assignedLine = NormalizeStr(candidate.AssignedProductionLine);
            var rawLine = NormalizeStr(candidate.ProductionLine);
            if (assignedLine.Length == 0 && rawLine.Length > 0)
            {
                var ruleLine = global::HDPro.CY.Order.Services.WZ_OrderCycleBaseService.CalcAssignedProductionLine(
                    rawLine,
                    valveCategory,
                    candidate.NominalDiameter);

                assignedLine = NormalizeStr(ruleLine);
                usedRule = usedRule || assignedLine.Length > 0;
            }

            if (assignedLine.Length == 0)
            {
                assignedLine = rawLine;
            }

            if (valveCategory.Length == 0 || assignedLine.Length == 0)
            {
                return null;
            }

            var score = 0;
            if (NormalizeStr(candidate.ValveCategory).Length > 0) score += 8;
            if (NormalizeStr(candidate.AssignedProductionLine).Length > 0) score += 8;
            if (candidate.IsSyncProductionLineCandidate) score += 6;
            if (rawLine.Length > 0) score += 4;
            if (candidate.ProductionDate.HasValue) score += 3;
            if (NormalizeStr(candidate.NominalDiameter).Length > 0) score += 2;
            if (NormalizeStr(candidate.SpecModel).Length > 0 || NormalizeStr(candidate.ProductName).Length > 0) score += 1;

            var status = candidate.ResolvedByRuleService
                ? DetailStatusMatchedByRule
                : candidate.IsSyncProductionLineCandidate
                    ? DetailStatusMatchedBySyncLine
                    : (usedRule || candidate.IsRuleServiceCandidate)
                        ? DetailStatusMatchedByRule
                        : DetailStatusMatchedByOrderCycle;

            return new ResolvedLineAssignment
            {
                EntryId = candidate.EntryId,
                BillPlanKey = BuildBillPlanKey(candidate.SalesOrderNo, candidate.PlanTrackingNo),
                MaterialKeys = candidate.MaterialKeys?.Count > 0
                    ? BuildMaterialKeys(candidate.MaterialKeys.ToArray())
                    : BuildMaterialKeys(candidate.MaterialKey),
                ValveCategory = valveCategory,
                ProductionLine = assignedLine,
                ProductionDate = candidate.ProductionDate?.Date,
                SpecModel = NormalizeStr(candidate.SpecModel),
                ProductModel = NormalizeStr(candidate.ProductName),
                ClassifyStatus = status,
                Score = score
            };
        }

        private static ResolvedLineAssignment PickBestResolvedLine(IEnumerable<ResolvedLineAssignment> assignments)
        {
            return assignments
                .Where(x => x != null
                    && NormalizeStr(x.ValveCategory).Length > 0
                    && NormalizeStr(x.ProductionLine).Length > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.ClassifyStatus)
                .ThenBy(x => x.ValveCategory)
                .ThenBy(x => x.ProductionLine)
                .FirstOrDefault();
        }

        private static (
            Dictionary<long, ResolvedLineAssignment> ByEntryId,
            Dictionary<string, ResolvedLineAssignment> ByBillPlanMaterial,
            Dictionary<string, ResolvedLineAssignment> ByBillPlan)
            BuildResolvedLineLookups(IEnumerable<ResolvedLineAssignment> resolvedAssignments)
        {
            var byEntryId = new Dictionary<long, ResolvedLineAssignment>();
            var byBillPlanMaterial = new Dictionary<string, ResolvedLineAssignment>(StringComparer.OrdinalIgnoreCase);
            var byBillPlan = new Dictionary<string, ResolvedLineAssignment>(StringComparer.OrdinalIgnoreCase);
            if (resolvedAssignments == null)
            {
                return (byEntryId, byBillPlanMaterial, byBillPlan);
            }

            var assignments = resolvedAssignments
                .Where(x => x != null
                    && NormalizeStr(x.ValveCategory).Length > 0
                    && NormalizeStr(x.ProductionLine).Length > 0)
                .ToList();

            foreach (var group in assignments
                .Where(x => x != null && x.EntryId.HasValue && x.EntryId.Value > 0)
                .GroupBy(x => x.EntryId!.Value))
            {
                var best = PickBestResolvedLine(group);
                if (best != null)
                {
                    byEntryId[group.Key] = best;
                }
            }

            foreach (var group in assignments
                .SelectMany(x => (x.MaterialKeys?.Count > 0 ? x.MaterialKeys : BuildMaterialKeys())
                    .Select(materialKey => new
                    {
                        Key = BuildBillPlanMaterialKeyFromBillPlanKey(x.BillPlanKey, materialKey),
                        Assignment = x
                    }))
                .Where(x => NormalizeStr(x.Key).Length > 0)
                .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                var best = PickBestResolvedLine(group.Select(x => x.Assignment));
                if (best != null)
                {
                    byBillPlanMaterial[group.Key] = best;
                }
            }

            foreach (var group in assignments
                .Where(x => x != null
                    && x.AllowBillPlanFallback
                    && NormalizeStr(x.BillPlanKey).Length > 0)
                .GroupBy(x => x.BillPlanKey, StringComparer.OrdinalIgnoreCase))
            {
                var best = PickBestResolvedLine(group);
                if (best != null)
                {
                    byBillPlan[group.Key] = best;
                }
            }

            return (byEntryId, byBillPlanMaterial, byBillPlan);
        }

        private static (int FilledByManual, int FilledByOrderCycle, int FilledBySyncLine, int FilledByRule) ApplyResolvedLineAssignments(
            List<ProductionOutputDetailRow> unresolvedDetails,
            IReadOnlyDictionary<long, ResolvedLineAssignment> byEntryId,
            IReadOnlyDictionary<string, ResolvedLineAssignment> byBillPlanMaterial,
            IReadOnlyDictionary<string, ResolvedLineAssignment> byBillPlan)
        {
            var filledByManual = 0;
            var filledByOrderCycle = 0;
            var filledBySyncLine = 0;
            var filledByRule = 0;
            if (unresolvedDetails == null || unresolvedDetails.Count == 0)
            {
                return (filledByManual, filledByOrderCycle, filledBySyncLine, filledByRule);
            }

            foreach (var detail in unresolvedDetails)
            {
                ResolvedLineAssignment resolved = null;
                if (detail.EntryId.HasValue && detail.EntryId.Value > 0)
                {
                    byEntryId?.TryGetValue(detail.EntryId.Value, out resolved);
                }

                if (resolved == null)
                {
                    var billPlanKey = BuildBillPlanKey(detail.BillNo, detail.PlanTrackingNo);
                    foreach (var materialKey in GetDetailMaterialKeys(detail))
                    {
                        byBillPlanMaterial?.TryGetValue(BuildBillPlanMaterialKeyFromBillPlanKey(billPlanKey, materialKey), out resolved);
                        if (resolved != null)
                        {
                            break;
                        }
                    }
                }

                if (resolved == null)
                {
                    byBillPlan?.TryGetValue(BuildBillPlanKey(detail.BillNo, detail.PlanTrackingNo), out resolved);
                }

                if (resolved == null)
                {
                    continue;
                }

                detail.ValveCategory = resolved.ValveCategory;
                detail.ProductionLine = resolved.ProductionLine;
                if (NormalizeStr(detail.SpecModel).Length == 0)
                {
                    detail.SpecModel = NormalizeStr(resolved.SpecModel);
                }
                if (NormalizeStr(detail.ProductModel).Length == 0)
                {
                    detail.ProductModel = NormalizeStr(resolved.ProductModel);
                }
                if (resolved.ProductionDate.HasValue)
                {
                    detail.ProductionDate = resolved.ProductionDate.Value.Date;
                }
                detail.ClassifyStatus = resolved.ClassifyStatus;

                if (string.Equals(resolved.ClassifyStatus, DetailStatusMatchedByManual, StringComparison.Ordinal))
                {
                    filledByManual++;
                }
                else if (string.Equals(resolved.ClassifyStatus, DetailStatusMatchedByOrderCycle, StringComparison.Ordinal))
                {
                    filledByOrderCycle++;
                }
                else if (string.Equals(resolved.ClassifyStatus, DetailStatusMatchedBySyncLine, StringComparison.Ordinal))
                {
                    filledBySyncLine++;
                }
                else if (string.Equals(resolved.ClassifyStatus, DetailStatusMatchedByRule, StringComparison.Ordinal))
                {
                    filledByRule++;
                }
            }

            return (filledByManual, filledByOrderCycle, filledBySyncLine, filledByRule);
        }

        private static string ResolveRuleProductText(OrderCycleLineCandidate candidate, RuleProductTextMode mode)
        {
            if (candidate == null)
            {
                return string.Empty;
            }

            var specModel = NormalizeStr(candidate.SpecModel);
            var productName = NormalizeStr(candidate.ProductName);
            return mode == RuleProductTextMode.SpecModelFirst
                ? (specModel.Length > 0 ? specModel : productName)
                : (productName.Length > 0 ? productName : specModel);
        }

        private static bool HasRuleServiceInputFields(OrderCycleLineCandidate candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            return NormalizeStr(candidate.ValveCategory1).Length > 0
                || NormalizeStr(candidate.ValveCategory).Length > 0
                || NormalizeStr(candidate.ProductName).Length > 0
                || NormalizeStr(candidate.SpecModel).Length > 0
                || NormalizeStr(candidate.NominalDiameter).Length > 0
                || NormalizeStr(candidate.NominalPressure).Length > 0
                || NormalizeStr(candidate.BodyMaterial).Length > 0
                || NormalizeStr(candidate.InnerMaterial).Length > 0
                || NormalizeStr(candidate.SealFaceForm).Length > 0
                || NormalizeStr(candidate.FlangeConnection).Length > 0
                || NormalizeStr(candidate.BonnetForm).Length > 0
                || NormalizeStr(candidate.FlowCharacteristic).Length > 0
                || NormalizeStr(candidate.Actuator).Length > 0
                || NormalizeStr(candidate.AccessoryConfig).Length > 0
                || NormalizeStr(candidate.OutsourcedValveBody).Length > 0
                || NormalizeStr(candidate.PurchaseFlag).Length > 0
                || NormalizeStr(candidate.SpecialProduct).Length > 0;
        }

        private async Task<Dictionary<string, OCP_Material>> LoadMaterialMapAsync(
            IEnumerable<string> materialKeys,
            CancellationToken ct)
        {
            var keys = materialKeys?
                .Select(NormalizeMaterialKey)
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            var materialMap = new Dictionary<string, OCP_Material>(StringComparer.OrdinalIgnoreCase);
            if (keys.Count == 0)
            {
                return materialMap;
            }

            foreach (var chunk in ChunkList(keys, 500))
            {
                var idChunk = chunk
                    .Select(x => long.TryParse(x, out var value) ? (long?)value : null)
                    .Where(x => x.HasValue && x.Value > 0)
                    .Select(x => x!.Value)
                    .Distinct()
                    .ToList();

                var materials = await _db.Set<OCP_Material>()
                    .AsNoTracking()
                    .Where(x => (x.MaterialCode != null && chunk.Contains(x.MaterialCode))
                        || idChunk.Contains(x.MaterialID))
                    .ToListAsync(ct);

                foreach (var material in materials)
                {
                    foreach (var key in BuildMaterialKeys(material))
                    {
                        if (!materialMap.ContainsKey(key))
                        {
                            materialMap[key] = material;
                        }
                    }
                }
            }

            return materialMap;
        }

        private async Task<int> EnrichDetailMaterialModelsAsync(
            IReadOnlyList<ProductionOutputDetailRow> details,
            CancellationToken ct)
        {
            if (details == null || details.Count == 0)
            {
                return 0;
            }

            var materialKeys = details
                .SelectMany(GetDetailMaterialKeys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var materialMap = await LoadMaterialMapAsync(materialKeys, ct);
            if (materialMap.Count == 0)
            {
                return 0;
            }

            var enriched = 0;
            foreach (var detail in details)
            {
                OCP_Material material = null;
                foreach (var key in GetDetailMaterialKeys(detail))
                {
                    if (materialMap.TryGetValue(key, out material))
                    {
                        break;
                    }
                }

                if (material == null)
                {
                    continue;
                }

                var changed = false;
                var materialId = material.MaterialID > 0 ? material.MaterialID.ToString() : string.Empty;
                if (materialId.Length > 0 && NormalizeStr(detail.MaterialId).Length == 0)
                {
                    detail.MaterialId = materialId;
                    changed = true;
                }

                var materialCode = NormalizeStr(material.MaterialCode);
                if (materialCode.Length > 0 && !string.Equals(NormalizeStr(detail.MaterialCode), materialCode, StringComparison.OrdinalIgnoreCase))
                {
                    detail.MaterialCode = materialCode;
                    changed = true;
                }

                var materialKey = BuildMaterialKey(detail.MaterialCode, detail.MaterialId);
                if (materialKey.Length > 0 && NormalizeStr(detail.MaterialKey).Length == 0)
                {
                    detail.MaterialKey = materialKey;
                    changed = true;
                }

                var specModel = NormalizeStr(material.SpecModel);
                if (specModel.Length > 0 && !string.Equals(NormalizeStr(detail.SpecModel), specModel, StringComparison.Ordinal))
                {
                    detail.SpecModel = specModel;
                    changed = true;
                }

                var productModel = NormalizeStr(material.ProductModel);
                if (productModel.Length > 0 && !string.Equals(NormalizeStr(detail.ProductModel), productModel, StringComparison.Ordinal))
                {
                    detail.ProductModel = productModel;
                    changed = true;
                }

                if (changed)
                {
                    enriched++;
                }
            }

            return enriched;
        }

        private async Task<int> EnrichDetailMaterialsFromOcpOrderTrackingAsync(
            IReadOnlyList<ProductionOutputDetailRow> details,
            CancellationToken ct)
        {
            if (details == null || details.Count == 0)
            {
                return 0;
            }

            static bool IsUnknownValue(string value, string unknown)
            {
                var normalized = NormalizeStr(value);
                return normalized.Length == 0 || string.Equals(normalized, unknown, StringComparison.OrdinalIgnoreCase);
            }

            var targets = details
                .Where(x => x != null
                    && (NormalizeStr(x.MaterialCode).Length == 0
                        || NormalizeStr(x.MaterialId).Length == 0
                        || NormalizeStr(x.MaterialKey).Length == 0
                        || NormalizeStr(x.SpecModel).Length == 0
                        || NormalizeStr(x.ProductModel).Length == 0
                        || IsUnknownValue(x.ValveCategory, UnknownValveCategory)))
                .ToList();
            if (targets.Count == 0)
            {
                return 0;
            }

            var entryIds = targets
                .Where(x => x.EntryId.HasValue && x.EntryId.Value > 0)
                .Select(x => x.EntryId!.Value)
                .Distinct()
                .ToList();

            var wantedBillPlanKeys = targets
                .Where(x => NormalizeStr(x.BillNo).Length > 0 && NormalizeStr(x.PlanTrackingNo).Length > 0)
                .Select(x => BuildBillPlanKey(x.BillNo, x.PlanTrackingNo))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var billNos = targets
                .Select(x => NormalizeStr(x.BillNo))
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var orderRows = new List<OrderTrackingMaterialRow>();
            foreach (var chunk in ChunkList(entryIds, 1000))
            {
                var rows = await _db.Set<OCP_OrderTracking>()
                    .AsNoTracking()
                    .Where(x => x.SOEntryID.HasValue && chunk.Contains(x.SOEntryID.Value))
                    .Select(x => new OrderTrackingMaterialRow
                    {
                        TrackingId = x.Id,
                        EntryId = x.SOEntryID,
                        SalesOrderNo = x.SOBillNo,
                        PlanTrackingNo = x.MtoNo,
                        MaterialId = x.MaterialID,
                        MaterialCode = x.MaterialNumber,
                        MaterialName = x.MaterialName,
                        SpecModel = x.TopSpecification,
                        ProductModel = x.ProductionModel,
                        Quantity = x.OrderQty ?? 0m,
                        ProductionDate = x.PrdScheduleDate,
                        OrderApprovedDate = x.OrderAuditDate,
                        ReplyDeliveryDate = x.ReplyDeliveryDate,
                        RequestedDeliveryDate = x.DeliveryDate
                    })
                    .ToListAsync(ct);

                orderRows.AddRange(rows);
            }

            if (wantedBillPlanKeys.Count > 0)
            {
                foreach (var chunk in ChunkList(billNos, 500))
                {
                    var rows = await _db.Set<OCP_OrderTracking>()
                        .AsNoTracking()
                        .Where(x => x.SOBillNo != null && chunk.Contains(x.SOBillNo))
                        .Select(x => new OrderTrackingMaterialRow
                        {
                            TrackingId = x.Id,
                            EntryId = x.SOEntryID,
                            SalesOrderNo = x.SOBillNo,
                            PlanTrackingNo = x.MtoNo,
                            MaterialId = x.MaterialID,
                            MaterialCode = x.MaterialNumber,
                            MaterialName = x.MaterialName,
                            SpecModel = x.TopSpecification,
                            ProductModel = x.ProductionModel,
                            Quantity = x.OrderQty ?? 0m,
                            ProductionDate = x.PrdScheduleDate,
                            OrderApprovedDate = x.OrderAuditDate,
                            ReplyDeliveryDate = x.ReplyDeliveryDate,
                            RequestedDeliveryDate = x.DeliveryDate
                        })
                        .ToListAsync(ct);

                    orderRows.AddRange(rows.Where(x => wantedBillPlanKeys.Contains(BuildBillPlanKey(x.SalesOrderNo, x.PlanTrackingNo))));
                }
            }

            orderRows = orderRows
                .Where(x => x != null && GetOrderTrackingMaterialKeys(x).Count > 0)
                .GroupBy(x => x.TrackingId.HasValue
                    ? $"T:{x.TrackingId.Value}"
                    : $"{BuildBillPlanKey(x.SalesOrderNo, x.PlanTrackingNo)}{BillPlanKeySeparator}{BuildMaterialKey(x.MaterialCode, MaterialIdToKey(x.MaterialId))}",
                    StringComparer.OrdinalIgnoreCase)
                .Select(PickBestOrderTrackingMaterialRow)
                .Where(x => x != null)
                .ToList();

            if (orderRows.Count == 0)
            {
                return 0;
            }

            var materialKeys = orderRows
                .SelectMany(GetOrderTrackingMaterialKeys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var materialMap = await LoadMaterialMapAsync(materialKeys, ct);

            var byEntryId = orderRows
                .Where(x => x.EntryId.HasValue && x.EntryId.Value > 0)
                .GroupBy(x => x.EntryId!.Value)
                .ToDictionary(x => x.Key, x => PickBestOrderTrackingMaterialRow(x));

            var byBillPlanMaterial = new Dictionary<string, OrderTrackingMaterialRow>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in orderRows)
            {
                foreach (var materialKey in GetOrderTrackingMaterialKeys(row))
                {
                    var key = BuildBillPlanMaterialKey(row.SalesOrderNo, row.PlanTrackingNo, materialKey);
                    if (!byBillPlanMaterial.TryGetValue(key, out var existing)
                        || ScoreOrderTrackingMaterialRow(row) > ScoreOrderTrackingMaterialRow(existing))
                    {
                        byBillPlanMaterial[key] = row;
                    }
                }
            }

            var byUniqueBillPlan = orderRows
                .GroupBy(x => BuildBillPlanKey(x.SalesOrderNo, x.PlanTrackingNo), StringComparer.OrdinalIgnoreCase)
                .Where(g =>
                {
                    var distinctMaterialKeys = g
                        .SelectMany(GetOrderTrackingMaterialKeys)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(2)
                        .Count();
                    return distinctMaterialKeys == 1;
                })
                .ToDictionary(x => x.Key, x => PickBestOrderTrackingMaterialRow(x), StringComparer.OrdinalIgnoreCase);

            var enriched = 0;
            foreach (var detail in targets)
            {
                OrderTrackingMaterialRow order = null;
                if (detail.EntryId.HasValue && detail.EntryId.Value > 0)
                {
                    byEntryId.TryGetValue(detail.EntryId.Value, out order);
                }

                var billPlanKey = BuildBillPlanKey(detail.BillNo, detail.PlanTrackingNo);
                if (order == null)
                {
                    foreach (var materialKey in GetDetailMaterialKeys(detail))
                    {
                        if (byBillPlanMaterial.TryGetValue(BuildBillPlanMaterialKeyFromBillPlanKey(billPlanKey, materialKey), out order))
                        {
                            break;
                        }
                    }
                }

                if (order == null)
                {
                    byUniqueBillPlan.TryGetValue(billPlanKey, out order);
                }

                if (order == null)
                {
                    continue;
                }

                OCP_Material material = null;
                foreach (var materialKey in GetOrderTrackingMaterialKeys(order))
                {
                    if (materialMap.TryGetValue(materialKey, out material))
                    {
                        break;
                    }
                }

                var changed = false;
                var materialId = material != null && material.MaterialID > 0
                    ? material.MaterialID.ToString()
                    : MaterialIdToKey(order.MaterialId);
                var materialCode = NormalizeStr(material?.MaterialCode);
                if (materialCode.Length == 0)
                {
                    materialCode = NormalizeStr(order.MaterialCode);
                }

                if (NormalizeStr(detail.MaterialId).Length == 0 && materialId.Length > 0)
                {
                    detail.MaterialId = materialId;
                    changed = true;
                }

                if (NormalizeStr(detail.MaterialCode).Length == 0 && materialCode.Length > 0)
                {
                    detail.MaterialCode = materialCode;
                    changed = true;
                }

                var detailMaterialKey = BuildMaterialKey(detail.MaterialCode, detail.MaterialId);
                if (NormalizeStr(detail.MaterialKey).Length == 0 && detailMaterialKey.Length > 0)
                {
                    detail.MaterialKey = detailMaterialKey;
                    changed = true;
                }

                var specModel = NormalizeStr(material?.SpecModel);
                if (specModel.Length == 0) specModel = NormalizeStr(order.SpecModel);
                if (specModel.Length == 0) specModel = NormalizeStr(material?.ProductModel);

                var productModel = NormalizeStr(material?.ProductModel);
                if (productModel.Length == 0) productModel = NormalizeStr(order.ProductModel);
                if (productModel.Length == 0) productModel = NormalizeStr(order.MaterialName);

                if (NormalizeStr(detail.SpecModel).Length == 0 && specModel.Length > 0)
                {
                    detail.SpecModel = specModel;
                    changed = true;
                }

                if (NormalizeStr(detail.ProductModel).Length == 0 && productModel.Length > 0)
                {
                    detail.ProductModel = productModel;
                    changed = true;
                }

                var valveCategory = NormalizeStr(material?.ValveCategory);
                if (valveCategory.Length == 0)
                {
                    var categoryRule = ValveCategoryRuleJudge.TryJudgeBySpecOrProduct(specModel, productModel);
                    if (categoryRule.HasValue)
                    {
                        valveCategory = NormalizeStr(categoryRule.Value.Category);
                    }
                }

                if (IsUnknownValue(detail.ValveCategory, UnknownValveCategory) && valveCategory.Length > 0)
                {
                    detail.ValveCategory = valveCategory;
                    changed = true;
                }

                var productionLine = NormalizeSyncProductionLineCandidate(material?.Workshop);
                if (IsUnknownValue(detail.ProductionLine, UnknownProductionLine) && productionLine.Length > 0)
                {
                    detail.ProductionLine = productionLine;
                    changed = true;
                }

                var finalValve = NormalizeStr(detail.ValveCategory);
                var finalLine = NormalizeStr(detail.ProductionLine);
                if (finalValve.Length > 0
                    && finalLine.Length > 0
                    && !string.Equals(finalValve, UnknownValveCategory, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(finalLine, UnknownProductionLine, StringComparison.OrdinalIgnoreCase)
                    && !IsSummarizableStatus(detail.ClassifyStatus))
                {
                    detail.ClassifyStatus = DetailStatusMatchedBySyncLine;
                    changed = true;
                }

                if (changed)
                {
                    enriched++;
                }
            }

            return enriched;
        }

        private static List<WZProductionOutputManualLineRuleDto> NormalizeManualLineRules(
            IReadOnlyCollection<WZProductionOutputManualLineRuleDto> rules)
        {
            var result = new Dictionary<string, WZProductionOutputManualLineRuleDto>(StringComparer.OrdinalIgnoreCase);
            if (rules == null || rules.Count == 0)
            {
                return new List<WZProductionOutputManualLineRuleDto>();
            }

            foreach (var rule in rules)
            {
                if (rule == null)
                {
                    continue;
                }

                var billNo = NormalizeStr(rule.BillNo);
                var planTrackingNo = NormalizeStr(rule.PlanTrackingNo);
                var materialCode = NormalizeStr(rule.MaterialCode);
                var materialId = NormalizeStr(rule.MaterialId);
                var materialKey = NormalizeStr(rule.MaterialKey);
                var valveCategory = NormalizeStr(rule.ValveCategory);
                var productionLine = NormalizeStr(rule.ProductionLine);
                if (billNo.Length == 0
                    || planTrackingNo.Length == 0
                    || valveCategory.Length == 0
                    || productionLine.Length == 0
                    || string.Equals(valveCategory, UnknownValveCategory, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(productionLine, UnknownProductionLine, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var key = string.Join(
                    BillPlanKeySeparator,
                    billNo,
                    planTrackingNo,
                    materialCode,
                    materialId,
                    materialKey);

                result[key] = new WZProductionOutputManualLineRuleDto
                {
                    BillNo = billNo,
                    PlanTrackingNo = planTrackingNo,
                    MaterialCode = materialCode,
                    MaterialId = materialId,
                    MaterialKey = materialKey,
                    ValveCategory = valveCategory,
                    ProductionLine = productionLine,
                    Remark = NormalizeStr(rule.Remark),
                    Enable = rule.Enable
                };
            }

            return result.Values.ToList();
        }

        private static DataTable BuildManualLineRuleDataTable(IReadOnlyList<WZProductionOutputManualLineRuleDto> rules)
        {
            var table = new DataTable();
            table.Columns.Add("BillNo", typeof(string));
            table.Columns.Add("PlanTrackingNo", typeof(string));
            table.Columns.Add("MaterialKey", typeof(string));
            table.Columns.Add("MaterialCode", typeof(string));
            table.Columns.Add("MaterialId", typeof(string));
            table.Columns.Add("ValveCategory", typeof(string));
            table.Columns.Add("ProductionLine", typeof(string));
            table.Columns.Add("Remark", typeof(string));
            table.Columns.Add("Enable", typeof(bool));

            foreach (var rule in rules)
            {
                table.Rows.Add(
                    rule.BillNo,
                    rule.PlanTrackingNo,
                    rule.MaterialKey,
                    rule.MaterialCode,
                    rule.MaterialId,
                    rule.ValveCategory,
                    rule.ProductionLine,
                    rule.Remark,
                    rule.Enable);
            }

            return table;
        }

        public async Task<int> SaveManualLineRulesAsync(
            IReadOnlyCollection<WZProductionOutputManualLineRuleDto> rules,
            CancellationToken ct = default)
        {
            var normalizedRules = NormalizeManualLineRules(rules);
            if (normalizedRules.Count == 0)
            {
                return 0;
            }

            await EnsureManualLineRuleTableAsync(ct);

            var sqlConnection = _db.Database.GetDbConnection() as SqlConnection;
            if (sqlConnection == null)
            {
                throw new InvalidOperationException("WZ 人工映射规则批量入库需要 SQL Server 连接");
            }

            using var tx = await _db.Database.BeginTransactionAsync(ct);
            var sqlTransaction = tx.GetDbTransaction() as SqlTransaction;
            if (sqlTransaction == null)
            {
                throw new InvalidOperationException("WZ 人工映射规则批量入库必须使用 SQL Server 事务");
            }

            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync(ct);
            }

            try
            {
                using (var createCommand = new SqlCommand(@"
IF OBJECT_ID('tempdb..#WZProductionOutputManualLineRuleImport') IS NOT NULL
    DROP TABLE #WZProductionOutputManualLineRuleImport;

CREATE TABLE #WZProductionOutputManualLineRuleImport
(
    [BillNo] NVARCHAR(100) COLLATE DATABASE_DEFAULT NOT NULL,
    [PlanTrackingNo] NVARCHAR(255) COLLATE DATABASE_DEFAULT NOT NULL,
    [MaterialKey] NVARCHAR(100) COLLATE DATABASE_DEFAULT NOT NULL,
    [MaterialCode] NVARCHAR(100) COLLATE DATABASE_DEFAULT NOT NULL,
    [MaterialId] NVARCHAR(100) COLLATE DATABASE_DEFAULT NOT NULL,
    [ValveCategory] NVARCHAR(50) COLLATE DATABASE_DEFAULT NOT NULL,
    [ProductionLine] NVARCHAR(50) COLLATE DATABASE_DEFAULT NOT NULL,
    [Remark] NVARCHAR(500) COLLATE DATABASE_DEFAULT NULL,
    [Enable] BIT NOT NULL
);", sqlConnection, sqlTransaction))
                {
                    createCommand.CommandTimeout = 0;
                    await createCommand.ExecuteNonQueryAsync(ct);
                }

                var table = BuildManualLineRuleDataTable(normalizedRules);
                using (var bulk = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.CheckConstraints, sqlTransaction))
                {
                    bulk.DestinationTableName = "#WZProductionOutputManualLineRuleImport";
                    bulk.BatchSize = InsertBatchSize;
                    bulk.BulkCopyTimeout = 0;
                    foreach (DataColumn column in table.Columns)
                    {
                        bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    await bulk.WriteToServerAsync(table, ct);
                }

                using (var mergeCommand = new SqlCommand(@"
CREATE INDEX [IX_WZProductionOutputManualLineRuleImport_Key]
    ON #WZProductionOutputManualLineRuleImport([BillNo], [PlanTrackingNo], [MaterialCode], [MaterialId], [MaterialKey]);

CREATE TABLE #WZProductionOutputManualLineRuleMergeResult([Action] NVARCHAR(10) NOT NULL);

MERGE [dbo].[WZ_ProductionOutputManualLineRule] WITH (HOLDLOCK) AS target
USING #WZProductionOutputManualLineRuleImport AS source
ON target.[BillNo] = source.[BillNo]
   AND target.[PlanTrackingNo] = source.[PlanTrackingNo]
   AND target.[MaterialCode] = source.[MaterialCode]
   AND target.[MaterialId] = source.[MaterialId]
   AND target.[MaterialKey] = source.[MaterialKey]
WHEN MATCHED THEN
    UPDATE SET
        [ValveCategory] = source.[ValveCategory],
        [ProductionLine] = source.[ProductionLine],
        [Remark] = source.[Remark],
        [Enable] = source.[Enable],
        [ModifyDate] = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (
        [BillNo], [PlanTrackingNo], [MaterialKey], [MaterialCode], [MaterialId],
        [ValveCategory], [ProductionLine], [Remark], [Enable], [CreateDate], [ModifyDate]
    )
    VALUES (
        source.[BillNo], source.[PlanTrackingNo], source.[MaterialKey], source.[MaterialCode], source.[MaterialId],
        source.[ValveCategory], source.[ProductionLine], source.[Remark], source.[Enable], GETDATE(), GETDATE()
    )
OUTPUT $action INTO #WZProductionOutputManualLineRuleMergeResult;

SELECT COUNT(1) FROM #WZProductionOutputManualLineRuleMergeResult;", sqlConnection, sqlTransaction))
                {
                    mergeCommand.CommandTimeout = 0;
                    var result = await mergeCommand.ExecuteScalarAsync(ct);
                    var processed = Convert.ToInt32(result ?? 0);
                    await tx.CommitAsync(ct);
                    _logger.LogInformation("【WZ 人工规则】已写入/更新人工产线映射规则 {Processed}/{Total} 条", processed, normalizedRules.Count);
                    return processed;
                }
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private async Task<List<ResolvedLineAssignment>> LoadManualLineRuleAssignmentsAsync(
            IReadOnlyList<ProductionOutputDetailRow> unresolvedDetails,
            CancellationToken ct)
        {
            var assignments = new List<ResolvedLineAssignment>();
            if (unresolvedDetails == null || unresolvedDetails.Count == 0)
            {
                return assignments;
            }

            var billNos = unresolvedDetails
                .Select(x => NormalizeStr(x.BillNo))
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var wantedBillPlans = unresolvedDetails
                .Where(x => NormalizeStr(x.BillNo).Length > 0 && NormalizeStr(x.PlanTrackingNo).Length > 0)
                .Select(x => BuildBillPlanKey(x.BillNo, x.PlanTrackingNo))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (billNos.Count == 0 || wantedBillPlans.Count == 0)
            {
                return assignments;
            }

            await EnsureManualLineRuleTableAsync(ct);

            var connectionString = _db.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _db.Database.GetDbConnection().ConnectionString;
            }

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);

            foreach (var chunk in ChunkList(billNos, 500))
            {
                var parameterNames = chunk.Select((_, index) => $"@BillNo{index}").ToList();
                using var cmd = new SqlCommand($@"
SELECT
    ISNULL([BillNo], N'') AS [BillNo],
    ISNULL([PlanTrackingNo], N'') AS [PlanTrackingNo],
    ISNULL([MaterialKey], N'') AS [MaterialKey],
    ISNULL([MaterialCode], N'') AS [MaterialCode],
    ISNULL([MaterialId], N'') AS [MaterialId],
    ISNULL([ValveCategory], N'') AS [ValveCategory],
    ISNULL([ProductionLine], N'') AS [ProductionLine]
FROM [dbo].[WZ_ProductionOutputManualLineRule] WITH (NOLOCK)
WHERE [Enable] = 1
  AND [BillNo] IN ({string.Join(", ", parameterNames)});", conn);

                cmd.CommandTimeout = 120;
                for (var i = 0; i < chunk.Count; i++)
                {
                    cmd.Parameters.Add(parameterNames[i], SqlDbType.NVarChar, 100).Value = chunk[i];
                }

                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    var billNo = ReadString(reader, "BillNo");
                    var planTrackingNo = ReadString(reader, "PlanTrackingNo");
                    var billPlanKey = BuildBillPlanKey(billNo, planTrackingNo);
                    if (!wantedBillPlans.Contains(billPlanKey))
                    {
                        continue;
                    }

                    var valveCategory = NormalizeStr(ReadString(reader, "ValveCategory"));
                    var productionLine = NormalizeStr(ReadString(reader, "ProductionLine"));
                    if (valveCategory.Length == 0 || productionLine.Length == 0)
                    {
                        continue;
                    }

                    var materialKeys = BuildMaterialKeys(
                        ReadString(reader, "MaterialKey"),
                        ReadString(reader, "MaterialCode"),
                        ReadString(reader, "MaterialId"));

                    assignments.Add(new ResolvedLineAssignment
                    {
                        BillPlanKey = billPlanKey,
                        MaterialKeys = materialKeys,
                        AllowBillPlanFallback = materialKeys.Count == 0,
                        ValveCategory = valveCategory,
                        ProductionLine = productionLine,
                        ClassifyStatus = DetailStatusMatchedByManual,
                        Score = 200
                    });
                }
            }

            return assignments;
        }

        private static ValveLineRuleRequest BuildValveLineRuleRequest(
            OrderCycleLineCandidate candidate,
            RuleProductTextMode productTextMode)
        {
            return new ValveLineRuleRequest
            {
                Id = candidate.MatchKey,
                OrderApprovedDate = candidate.OrderApprovedDate,
                ReplyDeliveryDate = candidate.ReplyDeliveryDate,
                RequestedDeliveryDate = candidate.RequestedDeliveryDate,
                BodyMaterial = candidate.BodyMaterial,
                InnerMaterial = candidate.InnerMaterial,
                FlangeConnection = candidate.FlangeConnection,
                BonnetForm = candidate.BonnetForm,
                FlowCharacteristic = candidate.FlowCharacteristic,
                Actuator = candidate.Actuator,
                AccessoryConfig = candidate.AccessoryConfig,
                OutsourcedValveBody = candidate.OutsourcedValveBody,
                ValveCategory1 = candidate.ValveCategory1,
                ValveCategory = candidate.ValveCategory,
                SealFaceForm = candidate.SealFaceForm,
                SpecialProduct = candidate.SpecialProduct,
                PurchaseFlag = candidate.PurchaseFlag,
                ProductName = ResolveRuleProductText(candidate, productTextMode),
                NominalDiameter = candidate.NominalDiameter,
                NominalPressure = candidate.NominalPressure
            };
        }

        private static string BuildValveLineRuleCacheKey(
            OrderCycleLineCandidate candidate,
            RuleProductTextMode productTextMode)
        {
            var request = BuildValveLineRuleRequest(candidate, productTextMode);
            return string.Join("\u001E", new[]
            {
                productTextMode.ToString(),
                NormalizeStr(request.BodyMaterial),
                NormalizeStr(request.InnerMaterial),
                NormalizeStr(request.FlangeConnection),
                NormalizeStr(request.BonnetForm),
                NormalizeStr(request.FlowCharacteristic),
                NormalizeStr(request.Actuator),
                NormalizeStr(request.AccessoryConfig),
                NormalizeStr(request.OutsourcedValveBody),
                NormalizeStr(request.ValveCategory1),
                NormalizeStr(request.ValveCategory),
                NormalizeStr(request.SealFaceForm),
                NormalizeStr(request.SpecialProduct),
                NormalizeStr(request.PurchaseFlag),
                NormalizeStr(request.ProductName),
                NormalizeStr(request.NominalDiameter),
                NormalizeStr(request.NominalPressure)
            });
        }

        private async Task<int> ApplyOcpScheduleDatesAsync(List<ProductionOutputDetailRow> details, CancellationToken ct)
        {
            if (details == null || details.Count == 0)
            {
                return 0;
            }

            var billNos = details
                .Select(x => NormalizeStr(x.BillNo))
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (billNos.Count == 0)
            {
                return 0;
            }

            var wantedBillPlanKeys = details
                .Where(x => NormalizeStr(x.BillNo).Length > 0 && NormalizeStr(x.PlanTrackingNo).Length > 0)
                .Select(x => BuildBillPlanKey(x.BillNo, x.PlanTrackingNo))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var byEntryId = new Dictionary<long, DateTime>();
            var byBillPlan = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            foreach (var chunk in ChunkList(billNos, 500))
            {
                var rows = await _db.Set<OCP_OrderTracking>()
                    .AsNoTracking()
                    .Where(x => x.SOBillNo != null
                        && chunk.Contains(x.SOBillNo)
                        && x.PrdScheduleDate.HasValue)
                    .Select(x => new
                    {
                        EntryId = x.SOEntryID,
                        SalesOrderNo = x.SOBillNo,
                        PlanTrackingNo = x.MtoNo,
                        ProductionDate = x.PrdScheduleDate
                    })
                    .ToListAsync(ct);

                foreach (var row in rows)
                {
                    if (!wantedBillPlanKeys.Contains(BuildBillPlanKey(row.SalesOrderNo, row.PlanTrackingNo))
                        && (!row.EntryId.HasValue || row.EntryId.Value <= 0))
                    {
                        continue;
                    }

                    var date = row.ProductionDate!.Value.Date;
                    if (row.EntryId.HasValue && row.EntryId.Value > 0 && !byEntryId.ContainsKey(row.EntryId.Value))
                    {
                        byEntryId[row.EntryId.Value] = date;
                    }

                    var billPlanKey = BuildBillPlanKey(row.SalesOrderNo, row.PlanTrackingNo);
                    if (billPlanKey.Length > 1 && !byBillPlan.ContainsKey(billPlanKey))
                    {
                        byBillPlan[billPlanKey] = date;
                    }
                }
            }

            var updated = 0;
            foreach (var detail in details)
            {
                DateTime? scheduleDate = null;
                if (detail.EntryId.HasValue && detail.EntryId.Value > 0
                    && byEntryId.TryGetValue(detail.EntryId.Value, out var entryDate))
                {
                    scheduleDate = entryDate;
                }

                if (!scheduleDate.HasValue
                    && byBillPlan.TryGetValue(BuildBillPlanKey(detail.BillNo, detail.PlanTrackingNo), out var billPlanDate))
                {
                    scheduleDate = billPlanDate;
                }

                if (scheduleDate.HasValue && detail.ProductionDate.Date != scheduleDate.Value.Date)
                {
                    detail.ProductionDate = scheduleDate.Value.Date;
                    updated++;
                }
            }

            return updated;
        }

        private static (string Cat, string Line) BuildThresholdKey(string valveCategory, string productionLine)
            => (NormalizeStr(valveCategory), NormalizeStr(productionLine));

        private static decimal? ResolveThreshold(
            IReadOnlyDictionary<(string Cat, string Line), decimal> thresholds,
            string valveCategory,
            string productionLine,
            decimal? fallback = null)
        {
            var key = BuildThresholdKey(valveCategory, productionLine);
            return thresholds != null && thresholds.TryGetValue(key, out var threshold)
                ? threshold
                : fallback;
        }

        private async Task EnsureThresholdTableAsync(CancellationToken ct)
        {
            await _db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[dbo].[WZ_ProductionOutputThreshold]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WZ_ProductionOutputThreshold](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WZ_ProductionOutputThreshold] PRIMARY KEY,
        [ValveCategory] NVARCHAR(50) NOT NULL,
        [ProductionLine] NVARCHAR(50) NOT NULL,
        [CurrentThreshold] DECIMAL(18,6) NOT NULL,
        [CreateDate] DATETIME NULL,
        [ModifyDate] DATETIME NULL
    );

    CREATE UNIQUE INDEX [IX_WZ_ProductionOutputThreshold_ValveLine]
        ON [dbo].[WZ_ProductionOutputThreshold]([ValveCategory], [ProductionLine]);
END
", ct);
        }

        private async Task EnsureProductionOutputDetailTableAsync(CancellationToken ct)
        {
            await _db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[dbo].[WZ_ProductionOutputDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WZ_ProductionOutputDetail](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WZ_ProductionOutputDetail] PRIMARY KEY,
        [BusinessKey] NVARCHAR(300) NOT NULL,
        [EntryId] BIGINT NULL,
        [BillNo] NVARCHAR(100) NULL,
        [PlanTrackingNo] NVARCHAR(255) NULL,
        [Seq] INT NULL,
        [MaterialKey] NVARCHAR(100) NULL,
        [MaterialCode] NVARCHAR(100) NULL,
        [MaterialId] NVARCHAR(100) NULL,
        [SpecModel] NVARCHAR(255) NULL,
        [ProductModel] NVARCHAR(255) NULL,
        [ProductionDate] DATE NOT NULL,
        [ValveCategory] NVARCHAR(50) NOT NULL CONSTRAINT [DF_WZ_ProductionOutputDetail_ValveCategory] DEFAULT(N''),
        [ProductionLine] NVARCHAR(50) NOT NULL CONSTRAINT [DF_WZ_ProductionOutputDetail_ProductionLine] DEFAULT(N''),
        [Quantity] DECIMAL(18,6) NOT NULL,
        [ClassifyStatus] NVARCHAR(30) NOT NULL,
        [RawRowCount] INT NOT NULL,
        [LineCandidateCount] INT NOT NULL,
        [SourceStartDate] DATE NULL,
        [SourceEndDate] DATE NULL,
        [LastSyncTime] DATETIME NOT NULL,
        [CreateDate] DATETIME NULL,
        [ModifyDate] DATETIME NULL
    );
END;

IF COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'MaterialKey') IS NULL
BEGIN
    ALTER TABLE [dbo].[WZ_ProductionOutputDetail] ADD [MaterialKey] NVARCHAR(100) NULL;
END;

IF COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'MaterialCode') IS NULL
BEGIN
    ALTER TABLE [dbo].[WZ_ProductionOutputDetail] ADD [MaterialCode] NVARCHAR(100) NULL;
END;

IF COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'MaterialId') IS NULL
BEGIN
    ALTER TABLE [dbo].[WZ_ProductionOutputDetail] ADD [MaterialId] NVARCHAR(100) NULL;
END;

IF COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'SpecModel') IS NULL
BEGIN
    ALTER TABLE [dbo].[WZ_ProductionOutputDetail] ADD [SpecModel] NVARCHAR(255) NULL;
END;

IF COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'ProductModel') IS NULL
BEGIN
    ALTER TABLE [dbo].[WZ_ProductionOutputDetail] ADD [ProductModel] NVARCHAR(255) NULL;
END;
", ct);

            await _db.Database.ExecuteSqlRawAsync(@"
UPDATE [dbo].[WZ_ProductionOutputDetail]
SET
    [MaterialKey] = CASE
        WHEN (ISNULL([MaterialKey], N'') = N'') AND CHARINDEX(N'|M:', [BusinessKey]) > 0
             AND CHARINDEX(N'|E:', [BusinessKey], CHARINDEX(N'|M:', [BusinessKey]) + 3) > CHARINDEX(N'|M:', [BusinessKey])
            THEN SUBSTRING(
                [BusinessKey],
                CHARINDEX(N'|M:', [BusinessKey]) + 3,
                CHARINDEX(N'|E:', [BusinessKey], CHARINDEX(N'|M:', [BusinessKey]) + 3) - CHARINDEX(N'|M:', [BusinessKey]) - 3)
        ELSE [MaterialKey]
    END,
    [MaterialCode] = CASE
        WHEN ISNULL([MaterialCode], N'') = N''
             AND ISNULL([MaterialKey], N'') <> N''
             AND TRY_CONVERT(BIGINT, [MaterialKey]) IS NULL
            THEN [MaterialKey]
        ELSE [MaterialCode]
    END,
    [MaterialId] = CASE
        WHEN ISNULL([MaterialId], N'') = N''
             AND ISNULL([MaterialKey], N'') <> N''
             AND TRY_CONVERT(BIGINT, [MaterialKey]) IS NOT NULL
            THEN [MaterialKey]
        ELSE [MaterialId]
    END
WHERE (ISNULL([MaterialKey], N'') = N''
       AND CHARINDEX(N'|M:', [BusinessKey]) > 0
       AND CHARINDEX(N'|E:', [BusinessKey], CHARINDEX(N'|M:', [BusinessKey]) + 3) > CHARINDEX(N'|M:', [BusinessKey]))
   OR (ISNULL([MaterialCode], N'') = N''
       AND ISNULL([MaterialKey], N'') <> N''
       AND TRY_CONVERT(BIGINT, [MaterialKey]) IS NULL)
   OR (ISNULL([MaterialId], N'') = N''
       AND ISNULL([MaterialKey], N'') <> N''
       AND TRY_CONVERT(BIGINT, [MaterialKey]) IS NOT NULL);

UPDATE [dbo].[WZ_ProductionOutputDetail]
SET
    [MaterialCode] = CASE
        WHEN ISNULL([MaterialCode], N'') = N''
             AND ISNULL([MaterialKey], N'') <> N''
             AND TRY_CONVERT(BIGINT, [MaterialKey]) IS NULL
            THEN [MaterialKey]
        ELSE [MaterialCode]
    END,
    [MaterialId] = CASE
        WHEN ISNULL([MaterialId], N'') = N''
             AND ISNULL([MaterialKey], N'') <> N''
             AND TRY_CONVERT(BIGINT, [MaterialKey]) IS NOT NULL
            THEN [MaterialKey]
        ELSE [MaterialId]
    END
WHERE (ISNULL([MaterialCode], N'') = N''
       AND ISNULL([MaterialKey], N'') <> N''
       AND TRY_CONVERT(BIGINT, [MaterialKey]) IS NULL)
   OR (ISNULL([MaterialId], N'') = N''
       AND ISNULL([MaterialKey], N'') <> N''
       AND TRY_CONVERT(BIGINT, [MaterialKey]) IS NOT NULL);
", ct);

            await _db.Database.ExecuteSqlRawAsync(@"
IF COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'BusinessKey') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.indexes
       WHERE name = N'IX_WZ_ProductionOutputDetail_BusinessKey'
         AND object_id = OBJECT_ID(N'dbo.WZ_ProductionOutputDetail')
   )
BEGIN
    CREATE UNIQUE INDEX [IX_WZ_ProductionOutputDetail_BusinessKey]
        ON [dbo].[WZ_ProductionOutputDetail]([BusinessKey]);
END;

IF COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'BillNo') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'PlanTrackingNo') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'Seq') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'BusinessKey') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.indexes
       WHERE name = N'IX_WZ_ProductionOutputDetail_BillPlanSeq'
         AND object_id = OBJECT_ID(N'dbo.WZ_ProductionOutputDetail')
   )
BEGIN
    CREATE INDEX [IX_WZ_ProductionOutputDetail_BillPlanSeq]
        ON [dbo].[WZ_ProductionOutputDetail]([BillNo], [PlanTrackingNo], [Seq])
        INCLUDE ([BusinessKey]);
END;

IF COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'ClassifyStatus') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.indexes
       WHERE name = N'IX_WZ_ProductionOutputDetail_StatusDate'
         AND object_id = OBJECT_ID(N'dbo.WZ_ProductionOutputDetail')
   )
BEGIN
    CREATE INDEX [IX_WZ_ProductionOutputDetail_StatusDate]
        ON [dbo].[WZ_ProductionOutputDetail]([ClassifyStatus], [ProductionDate]);
END;

IF COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'ProductionDate') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'ValveCategory') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'ProductionLine') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'ClassifyStatus') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.indexes
       WHERE name = N'IX_WZ_ProductionOutputDetail_DateValveLineStatus'
         AND object_id = OBJECT_ID(N'dbo.WZ_ProductionOutputDetail')
   )
BEGIN
    CREATE INDEX [IX_WZ_ProductionOutputDetail_DateValveLineStatus]
        ON [dbo].[WZ_ProductionOutputDetail]([ProductionDate], [ValveCategory], [ProductionLine], [ClassifyStatus]);
END;

IF COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'ProductionDate') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'ValveCategory') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'ProductionLine') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'ClassifyStatus') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'BusinessKey') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'BillNo') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'PlanTrackingNo') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'Seq') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'MaterialCode') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'MaterialId') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'MaterialKey') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'SpecModel') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'ProductModel') IS NOT NULL
   AND COL_LENGTH(N'dbo.WZ_ProductionOutputDetail', N'Quantity') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.indexes
       WHERE name = N'IX_WZ_ProductionOutputDetail_CellDetails'
         AND object_id = OBJECT_ID(N'dbo.WZ_ProductionOutputDetail')
   )
BEGIN
    CREATE INDEX [IX_WZ_ProductionOutputDetail_CellDetails]
        ON [dbo].[WZ_ProductionOutputDetail]([ProductionDate], [ValveCategory], [ProductionLine], [ClassifyStatus])
        INCLUDE ([BusinessKey], [BillNo], [PlanTrackingNo], [Seq], [MaterialCode], [MaterialId], [MaterialKey], [SpecModel], [ProductModel], [Quantity]);
END;
", ct);
        }

        private async Task EnsureManualLineRuleTableAsync(CancellationToken ct)
        {
            await _db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[dbo].[WZ_ProductionOutputManualLineRule]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WZ_ProductionOutputManualLineRule](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WZ_ProductionOutputManualLineRule] PRIMARY KEY,
        [BillNo] NVARCHAR(100) NOT NULL CONSTRAINT [DF_WZ_ProductionOutputManualLineRule_BillNo] DEFAULT(N''),
        [PlanTrackingNo] NVARCHAR(255) NOT NULL CONSTRAINT [DF_WZ_ProductionOutputManualLineRule_PlanTrackingNo] DEFAULT(N''),
        [MaterialKey] NVARCHAR(100) NOT NULL CONSTRAINT [DF_WZ_ProductionOutputManualLineRule_MaterialKey] DEFAULT(N''),
        [MaterialCode] NVARCHAR(100) NOT NULL CONSTRAINT [DF_WZ_ProductionOutputManualLineRule_MaterialCode] DEFAULT(N''),
        [MaterialId] NVARCHAR(100) NOT NULL CONSTRAINT [DF_WZ_ProductionOutputManualLineRule_MaterialId] DEFAULT(N''),
        [ValveCategory] NVARCHAR(50) NOT NULL,
        [ProductionLine] NVARCHAR(50) NOT NULL,
        [Remark] NVARCHAR(500) NULL,
        [Enable] BIT NOT NULL CONSTRAINT [DF_WZ_ProductionOutputManualLineRule_Enable] DEFAULT(1),
        [CreateDate] DATETIME NULL,
        [ModifyDate] DATETIME NULL
    );
END;

IF COL_LENGTH(N'dbo.WZ_ProductionOutputManualLineRule', N'MaterialKey') IS NULL
BEGIN
    ALTER TABLE [dbo].[WZ_ProductionOutputManualLineRule] ADD [MaterialKey] NVARCHAR(100) NOT NULL CONSTRAINT [DF_WZ_ProductionOutputManualLineRule_MaterialKey] DEFAULT(N'');
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_WZ_ProductionOutputManualLineRule_Lookup'
      AND object_id = OBJECT_ID(N'dbo.WZ_ProductionOutputManualLineRule')
)
BEGIN
    CREATE INDEX [IX_WZ_ProductionOutputManualLineRule_Lookup]
        ON [dbo].[WZ_ProductionOutputManualLineRule]([Enable], [BillNo], [PlanTrackingNo], [MaterialCode], [MaterialId], [MaterialKey]);
END;
", ct);
        }

        private async Task<Dictionary<(string Cat, string Line), decimal>> LoadThresholdMapAsync(CancellationToken ct)
        {
            await EnsureThresholdTableAsync(ct);

            var rows = await _db.Set<WZ_ProductionOutputThreshold>()
                .AsNoTracking()
                .Select(x => new
                {
                    x.ValveCategory,
                    x.ProductionLine,
                    x.CurrentThreshold
                })
                .ToListAsync(ct);

            var result = new Dictionary<(string Cat, string Line), decimal>();
            foreach (var row in rows)
            {
                var key = BuildThresholdKey(row.ValveCategory, row.ProductionLine);
                if (key.Cat.Length == 0 || key.Line.Length == 0)
                {
                    continue;
                }

                result[key] = row.CurrentThreshold;
            }

            return result;
        }

        private static void ApplyThresholds(
            IEnumerable<WZ_ProductionOutput> rows,
            IReadOnlyDictionary<(string Cat, string Line), decimal> thresholds)
        {
            if (rows == null || thresholds == null || thresholds.Count == 0)
            {
                return;
            }

            foreach (var row in rows)
            {
                if (row == null)
                {
                    continue;
                }

                row.CurrentThreshold = ResolveThreshold(
                    thresholds,
                    row.ValveCategory,
                    row.ProductionLine,
                    row.CurrentThreshold);
            }
        }

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

        private static string BuildEsbPayload(DateTime startDate, DateTime endDate)
        {
            return JsonConvert.SerializeObject(new
            {
                FSTARTDATE = startDate.ToString("yyyy-MM-dd"),
                FENDDATE = endDate.ToString("yyyy-MM-dd")
            });
        }

        private static string TruncateForLog(string text, int maxLength = 300)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text.Length <= maxLength ? text : $"{text.Substring(0, maxLength)}...";
        }

        private static List<EsbRow> ParseEsbRows(string json, DateTime startDate, DateTime endDate)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<EsbRow>>(json) ?? new List<EsbRow>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"ESB 返回解析失败，时间段 {startDate:yyyy-MM-dd}~{endDate:yyyy-MM-dd}，响应片段：{TruncateForLog(json)}",
                    ex);
            }
        }

        private async Task<List<EsbRow>> RequestEsbRowsWithRetryAsync(HttpClient client, DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            var payloadJson = BuildEsbPayload(startDate, endDate);
            Exception? lastException = null;

            for (var attempt = 1; attempt <= MaxEsbRetryCount; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Post, EsbUrl)
                    {
                        Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
                    };

                    using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
                    var body = await resp.Content.ReadAsStringAsync(ct);
                    if (resp.IsSuccessStatusCode)
                    {
                        return ParseEsbRows(body, startDate, endDate);
                    }

                    var message = $"ESB 返回 {(int)resp.StatusCode}({resp.StatusCode})，时间段 {startDate:yyyy-MM-dd}~{endDate:yyyy-MM-dd}，响应片段：{TruncateForLog(body)}";
                    lastException = new HttpRequestException(message, null, resp.StatusCode);
                    _logger.LogWarning("【WZ 刷新】第 {Attempt}/{MaxAttempt} 次请求失败：{Message}", attempt, MaxEsbRetryCount, message);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "【WZ 刷新】第 {Attempt}/{MaxAttempt} 次请求异常，时间段 {S}~{E}",
                        attempt, MaxEsbRetryCount, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
                }

                if (attempt < MaxEsbRetryCount)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(EsbRetryDelayMilliseconds * attempt), ct);
                }
            }

            throw lastException ?? new InvalidOperationException(
                $"ESB 请求失败（未知异常），时间段 {startDate:yyyy-MM-dd}~{endDate:yyyy-MM-dd}");
        }

        private async Task<List<EsbRow>> RequestEsbRowsAdaptiveAsync(HttpClient client, DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            try
            {
                return await RequestEsbRowsWithRetryAsync(client, startDate, endDate, ct);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                var days = (endDate.Date - startDate.Date).Days + 1;
                if (days <= 1)
                {
                    throw;
                }

                var leftEnd = startDate.Date.AddDays((days / 2) - 1);
                if (leftEnd < startDate.Date || leftEnd >= endDate.Date)
                {
                    throw;
                }

                var rightStart = leftEnd.AddDays(1);

                _logger.LogWarning(ex,
                    "【WZ 刷新】时间段 {S}~{E} 请求失败，自动拆分为 {LS}~{LE} 与 {RS}~{RE} 后重试",
                    startDate.ToString("yyyy-MM-dd"),
                    endDate.ToString("yyyy-MM-dd"),
                    startDate.ToString("yyyy-MM-dd"),
                    leftEnd.ToString("yyyy-MM-dd"),
                    rightStart.ToString("yyyy-MM-dd"),
                    endDate.ToString("yyyy-MM-dd"));

                var leftRows = await RequestEsbRowsAdaptiveAsync(client, startDate, leftEnd, ct);
                var rightRows = await RequestEsbRowsAdaptiveAsync(client, rightStart, endDate, ct);

                leftRows.AddRange(rightRows);
                return leftRows;
            }
        }

        private async Task<List<OrderCycleLineCandidate>> LoadOrderCycleLineCandidatesAsync(
            IReadOnlyList<ProductionOutputDetailRow> unresolvedDetails,
            CancellationToken ct)
        {
            var candidates = new List<OrderCycleLineCandidate>();
            if (unresolvedDetails == null || unresolvedDetails.Count == 0)
            {
                return candidates;
            }

            var entryIds = unresolvedDetails
                .Where(x => x.EntryId.HasValue && x.EntryId.Value > 0)
                .Select(x => x.EntryId!.Value)
                .Distinct()
                .ToList();

            foreach (var chunk in ChunkList(entryIds, 1000))
            {
                var rows = await _db.Set<WZ_OrderCycleBase>()
                    .AsNoTracking()
                    .Where(x => x.FENTRYID.HasValue && chunk.Contains(x.FENTRYID.Value))
                    .Select(x => new OrderCycleLineCandidate
                    {
                        EntryId = x.FENTRYID,
                        SalesOrderNo = x.SalesOrderNo,
                        PlanTrackingNo = x.PlanTrackingNo,
                        ValveCategory = x.ValveCategory,
                        ProductionLine = x.ProductionLine,
                        AssignedProductionLine = x.AssignedProductionLine,
                        ProductionDate = x.ScheduleDate,
                        NominalDiameter = x.NominalDiameter,
                        SpecModel = x.GUI_GE_XING_HAO,
                        ProductName = x.ProductName
                    })
                    .ToListAsync(ct);

                candidates.AddRange(rows);
            }

            var billPlanKeys = unresolvedDetails
                .Where(x => NormalizeStr(x.BillNo).Length > 0 && NormalizeStr(x.PlanTrackingNo).Length > 0)
                .Select(x => BuildBillPlanKey(x.BillNo, x.PlanTrackingNo))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (billPlanKeys.Count > 0)
            {
                var billNos = unresolvedDetails
                    .Select(x => NormalizeStr(x.BillNo))
                    .Where(x => x.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var chunk in ChunkList(billNos, 1000))
                {
                    var rows = await _db.Set<WZ_OrderCycleBase>()
                        .AsNoTracking()
                        .Where(x => x.SalesOrderNo != null && chunk.Contains(x.SalesOrderNo))
                        .Select(x => new OrderCycleLineCandidate
                        {
                            EntryId = x.FENTRYID,
                            SalesOrderNo = x.SalesOrderNo,
                            PlanTrackingNo = x.PlanTrackingNo,
                            MaterialKey = x.MaterialCode,
                            MaterialKeys = BuildMaterialKeys(x.MaterialCode),
                            ValveCategory = x.ValveCategory,
                            ProductionLine = x.ProductionLine,
                            AssignedProductionLine = x.AssignedProductionLine,
                            ProductionDate = x.ScheduleDate,
                            NominalDiameter = x.NominalDiameter,
                            SpecModel = x.GUI_GE_XING_HAO,
                            ProductName = x.ProductName
                        })
                        .ToListAsync(ct);

                    candidates.AddRange(rows.Where(x => billPlanKeys.Contains(BuildBillPlanKey(x.SalesOrderNo, x.PlanTrackingNo))));
                }
            }

            return candidates;
        }

        private async Task<List<OrderCycleLineCandidate>> LoadOrderTrackingLineCandidatesAsync(
            IReadOnlyList<ProductionOutputDetailRow> unresolvedDetails,
            CancellationToken ct)
        {
            var candidates = new List<OrderCycleLineCandidate>();
            if (unresolvedDetails == null || unresolvedDetails.Count == 0)
            {
                return candidates;
            }

            var entryIds = unresolvedDetails
                .Where(x => x.EntryId.HasValue && x.EntryId.Value > 0)
                .Select(x => x.EntryId!.Value)
                .Distinct()
                .ToList();

            var orderRows = new List<OrderTrackingMaterialRow>();
            foreach (var chunk in ChunkList(entryIds, 1000))
            {
                var rows = await _db.Set<ERP_OrderTracking>()
                    .AsNoTracking()
                    .Where(x => chunk.Contains(x.FENTRYID))
                    .Select(x => new OrderTrackingMaterialRow
                    {
                        TrackingId = x.id,
                        EntryId = (long?)x.FENTRYID,
                        SalesOrderNo = x.FBILLNO,
                        PlanTrackingNo = x.FMTONO,
                        MaterialCode = x.FNUMBER,
                        Quantity = x.FQTY,
                        OrderApprovedDate = x.FAPPROVEDATE,
                        ReplyDeliveryDate = x.F_BLN_HFJHRQ,
                        RequestedDeliveryDate = x.F_ORA_DATETIME
                    })
                    .ToListAsync(ct);

                orderRows.AddRange(rows);
            }

            if (orderRows.Count == 0)
            {
                return candidates;
            }

            var materialKeys = orderRows
                .SelectMany(GetOrderTrackingMaterialKeys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var materialMap = await LoadMaterialMapAsync(materialKeys, ct);

            foreach (var order in orderRows)
            {
                var materialCode = NormalizeStr(order.MaterialCode);
                OCP_Material material = null;
                foreach (var key in GetOrderTrackingMaterialKeys(order))
                {
                    if (materialMap.TryGetValue(key, out material))
                    {
                        break;
                    }
                }

                var materialValveCategory = NormalizeStr(material?.ValveCategory);
                var syncProductionLine = NormalizeSyncProductionLineCandidate(material?.Workshop);
                var resolvedValveCategory = materialValveCategory;
                if (resolvedValveCategory.Length == 0)
                {
                    var categoryRule = ValveCategoryRuleJudge.TryJudgeBySpecOrProduct(
                        NormalizeStr(material?.SpecModel).Length > 0 ? material?.SpecModel : order.SpecModel,
                        NormalizeStr(material?.ProductModel).Length > 0 ? material?.ProductModel : order.ProductModel);
                    if (categoryRule.HasValue)
                    {
                        resolvedValveCategory = NormalizeStr(categoryRule.Value.Category);
                    }
                }

                candidates.Add(new OrderCycleLineCandidate
                {
                    EntryId = order.EntryId,
                    SalesOrderNo = NormalizeStr(order.SalesOrderNo),
                    PlanTrackingNo = NormalizeStr(order.PlanTrackingNo),
                    MaterialKey = materialCode,
                    MaterialKeys = BuildMaterialKeys(material, materialCode),
                    ValveCategory = resolvedValveCategory,
                    ProductionLine = syncProductionLine,
                    NominalDiameter = NormalizeStr(material?.NominalDiameter),
                    NominalPressure = NormalizeStr(material?.NominalPressure),
                    SpecModel = NormalizeStr(material?.SpecModel),
                    ProductName = NormalizeStr(material?.ProductModel).Length > 0
                        ? NormalizeStr(material?.ProductModel)
                        : NormalizeStr(order.MaterialName),
                    BodyMaterial = NormalizeStr(material?.BodyMaterial),
                    InnerMaterial = NormalizeStr(material?.TrimMaterial ?? material?.InnerMaterial),
                    FlangeConnection = NormalizeStr(material?.FlangeConnection),
                    BonnetForm = NormalizeStr(material?.BonnetForm),
                    FlowCharacteristic = NormalizeStr(material?.FlowCharacteristic),
                    Actuator = NormalizeStr(material?.ActuatorModel),
                    AccessoryConfig = NormalizeStr(material?.Accessories),
                    SealFaceForm = NormalizeStr(material?.FlangeSealType ?? material?.SealFaceForm),
                    OrderApprovedDate = order.OrderApprovedDate,
                    ReplyDeliveryDate = order.ReplyDeliveryDate,
                    RequestedDeliveryDate = order.RequestedDeliveryDate,
                    IsRuleServiceCandidate = true,
                    IsSyncProductionLineCandidate = syncProductionLine.Length > 0,
                    MatchKey = order.EntryId.HasValue
                        ? $"E:{order.EntryId.Value}"
                        : BuildBillPlanKey(order.SalesOrderNo, order.PlanTrackingNo)
                });
            }

            return candidates;
        }

        private async Task<List<OrderCycleLineCandidate>> LoadOcpOrderTrackingLineCandidatesAsync(
            IReadOnlyList<ProductionOutputDetailRow> unresolvedDetails,
            CancellationToken ct)
        {
            var candidates = new List<OrderCycleLineCandidate>();
            if (unresolvedDetails == null || unresolvedDetails.Count == 0)
            {
                return candidates;
            }

            var billNos = unresolvedDetails
                .Select(x => NormalizeStr(x.BillNo))
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (billNos.Count == 0)
            {
                return candidates;
            }

            var wantedKeys = unresolvedDetails
                .Where(x => NormalizeStr(x.BillNo).Length > 0 && NormalizeStr(x.PlanTrackingNo).Length > 0)
                .Select(x => BuildBillPlanKey(x.BillNo, x.PlanTrackingNo))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (wantedKeys.Count == 0)
            {
                return candidates;
            }

            _logger.LogInformation(
                "【WZ 补线】OCP订单跟踪查询开始：单据 {BillNos}，单据计划键 {Keys}",
                billNos.Count,
                wantedKeys.Count);

            var orderRows = new List<OrderTrackingMaterialRow>();
            var chunkIndex = 0;
            foreach (var chunk in ChunkList(billNos, 500))
            {
                chunkIndex++;
                var rows = await _db.Set<OCP_OrderTracking>()
                    .AsNoTracking()
                    .Where(x => x.SOBillNo != null && chunk.Contains(x.SOBillNo))
                    .Select(x => new OrderTrackingMaterialRow
                    {
                        TrackingId = x.Id,
                        EntryId = x.SOEntryID,
                        SalesOrderNo = x.SOBillNo,
                        PlanTrackingNo = x.MtoNo,
                        MaterialId = x.MaterialID,
                        MaterialCode = x.MaterialNumber,
                        MaterialName = x.MaterialName,
                        SpecModel = x.TopSpecification,
                        ProductModel = x.ProductionModel,
                        Quantity = x.OrderQty ?? 0m,
                        ProductionDate = x.PrdScheduleDate,
                        OrderApprovedDate = x.OrderAuditDate,
                        ReplyDeliveryDate = x.ReplyDeliveryDate,
                        RequestedDeliveryDate = x.DeliveryDate
                    })
                    .ToListAsync(ct);

                orderRows.AddRange(rows.Where(x => wantedKeys.Contains(BuildBillPlanKey(x.SalesOrderNo, x.PlanTrackingNo))));
                if (chunkIndex == 1 || chunkIndex % 10 == 0)
                {
                    _logger.LogInformation(
                        "【WZ 补线】OCP订单跟踪查询进度：已处理单据 {Processed}/{Total}，匹配行 {Rows}",
                        Math.Min(chunkIndex * 500, billNos.Count),
                        billNos.Count,
                        orderRows.Count);
                }
            }

            _logger.LogInformation("【WZ 补线】OCP订单跟踪查询完成：匹配行 {Rows}", orderRows.Count);

            if (orderRows.Count == 0)
            {
                return candidates;
            }

            var materialKeys = orderRows
                .SelectMany(GetOrderTrackingMaterialKeys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var materialMap = await LoadMaterialMapAsync(materialKeys, ct);

            foreach (var order in orderRows)
            {
                var materialCode = NormalizeStr(order.MaterialCode);
                OCP_Material material = null;
                foreach (var key in GetOrderTrackingMaterialKeys(order))
                {
                    if (materialMap.TryGetValue(key, out material))
                    {
                        break;
                    }
                }

                var materialValveCategory = NormalizeStr(material?.ValveCategory);
                var syncProductionLine = NormalizeSyncProductionLineCandidate(material?.Workshop);
                var resolvedValveCategory = materialValveCategory;
                if (resolvedValveCategory.Length == 0)
                {
                    var categoryRule = ValveCategoryRuleJudge.TryJudgeBySpecOrProduct(
                        NormalizeStr(material?.SpecModel).Length > 0 ? material?.SpecModel : order.SpecModel,
                        NormalizeStr(material?.ProductModel).Length > 0 ? material?.ProductModel : order.ProductModel);
                    if (categoryRule.HasValue)
                    {
                        resolvedValveCategory = NormalizeStr(categoryRule.Value.Category);
                    }
                }

                candidates.Add(new OrderCycleLineCandidate
                {
                    EntryId = order.EntryId,
                    SalesOrderNo = NormalizeStr(order.SalesOrderNo),
                    PlanTrackingNo = NormalizeStr(order.PlanTrackingNo),
                    MaterialKey = BuildMaterialKey(materialCode, MaterialIdToKey(order.MaterialId)),
                    MaterialKeys = BuildMaterialKeys(material, materialCode, MaterialIdToKey(order.MaterialId)),
                    ValveCategory = resolvedValveCategory,
                    ProductionLine = syncProductionLine,
                    ProductionDate = order.ProductionDate,
                    NominalDiameter = NormalizeStr(material?.NominalDiameter),
                    NominalPressure = NormalizeStr(material?.NominalPressure),
                    SpecModel = NormalizeStr(material?.SpecModel).Length > 0
                        ? NormalizeStr(material?.SpecModel)
                        : NormalizeStr(order.SpecModel),
                    ProductName = NormalizeStr(material?.ProductModel).Length > 0
                        ? NormalizeStr(material?.ProductModel)
                        : (NormalizeStr(order.ProductModel).Length > 0
                            ? NormalizeStr(order.ProductModel)
                            : NormalizeStr(order.MaterialName)),
                    BodyMaterial = NormalizeStr(material?.BodyMaterial),
                    InnerMaterial = NormalizeStr(material?.TrimMaterial ?? material?.InnerMaterial),
                    FlangeConnection = NormalizeStr(material?.FlangeConnection),
                    BonnetForm = NormalizeStr(material?.BonnetForm),
                    FlowCharacteristic = NormalizeStr(material?.FlowCharacteristic),
                    Actuator = NormalizeStr(material?.ActuatorModel),
                    AccessoryConfig = NormalizeStr(material?.Accessories),
                    SealFaceForm = NormalizeStr(material?.FlangeSealType ?? material?.SealFaceForm),
                    OrderApprovedDate = order.OrderApprovedDate,
                    ReplyDeliveryDate = order.ReplyDeliveryDate,
                    RequestedDeliveryDate = order.RequestedDeliveryDate,
                    IsRuleServiceCandidate = true,
                    IsSyncProductionLineCandidate = syncProductionLine.Length > 0,
                    MatchKey = order.EntryId.HasValue
                        ? $"E:{order.EntryId.Value}"
                        : BuildBillPlanKey(order.SalesOrderNo, order.PlanTrackingNo)
                });
            }

            return candidates;
        }

        private async Task<List<ResolvedLineAssignment>> ResolveLineCandidatesByRuleServiceAsync(
            List<OrderCycleLineCandidate> candidates,
            CancellationToken ct)
        {
            var resolvedAssignments = new List<ResolvedLineAssignment>();
            if (candidates == null || candidates.Count == 0)
            {
                return resolvedAssignments;
            }

            var eligible = candidates
                .Where(x => NormalizeStr(x.MatchKey).Length > 0
                    && HasRuleServiceInputFields(x))
                .ToList();

            if (eligible.Count == 0)
            {
                return resolvedAssignments;
            }

            var startedAt = DateTime.UtcNow;
            var deadline = startedAt.AddSeconds(ValveRuleTotalTimeoutSeconds);
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            var ruleLineCache = new Dictionary<string, RuleLineCacheEntry>(StringComparer.Ordinal);
            var totalUniqueRequests = 0;
            var totalCacheHits = 0;
            var totalFailedRequests = 0;

            _logger.LogInformation(
                "【WZ 补线】规则服务开始：候选 {Candidates}，批量大小 {BatchSize}，单批超时 {RequestTimeout} 秒，总预算 {TotalTimeout} 秒",
                eligible.Count,
                ValveRuleBatchSize,
                ValveRuleRequestTimeoutSeconds,
                ValveRuleTotalTimeoutSeconds);

            bool ApplyRuleLine(OrderCycleLineCandidate candidate, string productionLine, HashSet<string> successKeys)
            {
                candidate.ProductionLine = NormalizeStr(productionLine);
                candidate.ResolvedByRuleService = true;
                var resolved = ResolveOrderCycleLineCandidate(candidate);
                if (resolved == null)
                {
                    return false;
                }

                resolvedAssignments.Add(resolved);
                successKeys.Add(candidate.MatchKey);
                return true;
            }

            async Task<HashSet<string>> SendRuleBatchAsync(
                List<OrderCycleLineCandidate> batch,
                RuleProductTextMode productTextMode,
                int batchStartIndex)
            {
                var successKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (batch == null || batch.Count == 0)
                {
                    return successKeys;
                }

                var requestPayload = new List<ValveLineRuleRequest>();
                var requestMap = new Dictionary<string, (string CacheKey, List<OrderCycleLineCandidate> Candidates)>(StringComparer.OrdinalIgnoreCase);
                var groupedRequests = new Dictionary<string, List<OrderCycleLineCandidate>>(StringComparer.Ordinal);

                foreach (var candidate in batch)
                {
                    var cacheKey = BuildValveLineRuleCacheKey(candidate, productTextMode);
                    if (ruleLineCache.TryGetValue(cacheKey, out var cacheEntry))
                    {
                        totalCacheHits++;
                        if (cacheEntry.Success)
                        {
                            ApplyRuleLine(candidate, cacheEntry.ProductionLine, successKeys);
                        }

                        continue;
                    }

                    if (!groupedRequests.TryGetValue(cacheKey, out var group))
                    {
                        group = new List<OrderCycleLineCandidate>();
                        groupedRequests[cacheKey] = group;
                    }

                    group.Add(candidate);
                }

                foreach (var group in groupedRequests)
                {
                    var representative = group.Value[0];
                    var requestItem = BuildValveLineRuleRequest(representative, productTextMode);
                    requestItem.Id = representative.MatchKey;
                    requestPayload.Add(requestItem);
                    requestMap[requestItem.Id] = (group.Key, group.Value);
                }

                if (requestPayload.Count == 0)
                {
                    return successKeys;
                }

                try
                {
                    totalUniqueRequests += requestPayload.Count;
                    var json = JsonConvert.SerializeObject(requestPayload);
                    using var request = new HttpRequestMessage(HttpMethod.Post, ValveRuleServiceUrl)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };

                    var remaining = deadline - DateTime.UtcNow;
                    if (remaining <= TimeSpan.Zero)
                    {
                        _logger.LogWarning(
                            "【WZ 补线】规则服务总预算已耗尽，模式 {Mode}，批次 {Start}/{Total}",
                            productTextMode,
                            batchStartIndex + 1,
                            eligible.Count);
                        foreach (var group in groupedRequests)
                        {
                            ruleLineCache[group.Key] = new RuleLineCacheEntry { Success = false };
                        }
                        totalFailedRequests += requestPayload.Count;
                        return successKeys;
                    }

                    var timeout = remaining < TimeSpan.FromSeconds(ValveRuleRequestTimeoutSeconds)
                        ? remaining
                        : TimeSpan.FromSeconds(ValveRuleRequestTimeoutSeconds);

                    _logger.LogInformation(
                        "【WZ 补线】规则服务批次开始：模式 {Mode}，批次 {Start}/{Total}，请求 {Requests}，本批超时 {TimeoutSeconds} 秒",
                        productTextMode,
                        batchStartIndex + 1,
                        eligible.Count,
                        requestPayload.Count,
                        Math.Ceiling(timeout.TotalSeconds));

                    using var ruleCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    ruleCts.CancelAfter(timeout);
                    using var response = await client.SendAsync(request, ruleCts.Token);
                    var body = await response.Content.ReadAsStringAsync(ruleCts.Token);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("【WZ 补线】规则服务返回 {StatusCode}，响应片段：{Body}",
                            (int)response.StatusCode,
                            TruncateForLog(body));
                        foreach (var group in groupedRequests)
                        {
                            ruleLineCache[group.Key] = new RuleLineCacheEntry { Success = false };
                        }
                        totalFailedRequests += requestPayload.Count;
                        return successKeys;
                    }

                    var batchResponse = JsonConvert.DeserializeObject<ValveLineRuleBatchResponse>(body);
                    if (batchResponse?.Results == null || batchResponse.Results.Count == 0)
                    {
                        foreach (var group in groupedRequests)
                        {
                            ruleLineCache[group.Key] = new RuleLineCacheEntry { Success = false };
                        }
                        totalFailedRequests += requestPayload.Count;
                        return successKeys;
                    }

                    var returnedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var item in batchResponse.Results)
                    {
                        var id = NormalizeStr(item?.Id ?? item?.Result?.Id);
                        if (id.Length == 0 || !requestMap.TryGetValue(id, out var mapped))
                        {
                            continue;
                        }

                        returnedIds.Add(id);
                        if (item?.Success != true || item.Result == null)
                        {
                            ruleLineCache[mapped.CacheKey] = new RuleLineCacheEntry { Success = false };
                            totalFailedRequests++;
                            continue;
                        }

                        var productionLine = NormalizeStr(item.Result.ProductionLine);
                        ruleLineCache[mapped.CacheKey] = new RuleLineCacheEntry
                        {
                            Success = productionLine.Length > 0,
                            ProductionLine = productionLine
                        };

                        if (productionLine.Length == 0)
                        {
                            totalFailedRequests++;
                            continue;
                        }

                        foreach (var candidate in mapped.Candidates)
                        {
                            ApplyRuleLine(candidate, productionLine, successKeys);
                        }
                    }

                    foreach (var missing in requestMap.Where(x => !returnedIds.Contains(x.Key)))
                    {
                        ruleLineCache[missing.Value.CacheKey] = new RuleLineCacheEntry { Success = false };
                        totalFailedRequests++;
                    }
                }
                catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
                {
                    foreach (var group in groupedRequests)
                    {
                        ruleLineCache[group.Key] = new RuleLineCacheEntry { Success = false };
                    }
                    totalFailedRequests += requestPayload.Count;
                    _logger.LogWarning(ex,
                        "【WZ 补线】规则服务批量推断超时，模式 {Mode}，批次 {Start}/{Total}，超时 {TimeoutSeconds} 秒",
                        productTextMode,
                        batchStartIndex + 1,
                        eligible.Count,
                        ValveRuleRequestTimeoutSeconds);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    foreach (var group in groupedRequests)
                    {
                        ruleLineCache[group.Key] = new RuleLineCacheEntry { Success = false };
                    }
                    totalFailedRequests += requestPayload.Count;
                    _logger.LogWarning(ex,
                        "【WZ 补线】规则服务批量推断失败，模式 {Mode}，批次 {Start}/{Total}",
                        productTextMode,
                        batchStartIndex + 1,
                        eligible.Count);
                }

                return successKeys;
            }

            for (var i = 0; i < eligible.Count; i += ValveRuleBatchSize)
            {
                ct.ThrowIfCancellationRequested();
                if (DateTime.UtcNow >= deadline)
                {
                    _logger.LogWarning(
                        "【WZ 补线】规则服务总预算已耗尽，停止剩余批次：已处理 {Processed}/{Total}",
                        i,
                        eligible.Count);
                    break;
                }

                var batch = eligible.Skip(i).Take(ValveRuleBatchSize).ToList();
                var specSuccessKeys = await SendRuleBatchAsync(batch, RuleProductTextMode.SpecModelFirst, i);
                var fallbackBatch = batch
                    .Where(x => !specSuccessKeys.Contains(x.MatchKey)
                        && NormalizeStr(x.ProductName).Length > 0
                        && !string.Equals(
                            ResolveRuleProductText(x, RuleProductTextMode.SpecModelFirst),
                            ResolveRuleProductText(x, RuleProductTextMode.ProductNameFallback),
                            StringComparison.Ordinal))
                    .ToList();
                if (fallbackBatch.Count > 0)
                {
                    await SendRuleBatchAsync(fallbackBatch, RuleProductTextMode.ProductNameFallback, i);
                }
            }

            _logger.LogInformation(
                "【WZ 补线】规则服务去重完成：候选 {Candidates}，实际请求 {Requests}，缓存命中 {CacheHits}，失败请求 {FailedRequests}，补齐 {Resolved}",
                eligible.Count,
                totalUniqueRequests,
                totalCacheHits,
                totalFailedRequests,
                resolvedAssignments.Count);

            return resolvedAssignments;
        }

        private async Task<DetailBackfillSummary> BackfillDetailRowsAsync(
            List<ProductionOutputDetailRow> details,
            CancellationToken ct)
        {
            var unresolvedDetails = details?
                .Where(x => !IsSummarizableStatus(x.ClassifyStatus)
                    || NormalizeStr(x.ValveCategory).Length == 0
                    || NormalizeStr(x.ProductionLine).Length == 0)
                .ToList() ?? new List<ProductionOutputDetailRow>();

            var summary = new DetailBackfillSummary
            {
                Candidates = unresolvedDetails.Count
            };

            if (unresolvedDetails.Count == 0)
            {
                return summary;
            }

            _logger.LogInformation("【WZ 补线】开始回填产线：待处理明细 {Details}", unresolvedDetails.Count);
            var manualAssignments = await LoadManualLineRuleAssignmentsAsync(unresolvedDetails, ct);
            if (manualAssignments.Count > 0)
            {
                var lookups = BuildResolvedLineLookups(manualAssignments);
                var filled = ApplyResolvedLineAssignments(unresolvedDetails, lookups.ByEntryId, lookups.ByBillPlanMaterial, lookups.ByBillPlan);
                summary.FilledByManual += filled.FilledByManual;
                _logger.LogInformation(
                    "【WZ 补线】人工映射规则候选 {Candidates}，补齐 {Filled}",
                    manualAssignments.Count,
                    filled.FilledByManual);
            }

            unresolvedDetails = details
                .Where(x => !IsSummarizableStatus(x.ClassifyStatus)
                    || NormalizeStr(x.ValveCategory).Length == 0
                    || NormalizeStr(x.ProductionLine).Length == 0)
                .ToList();

            if (unresolvedDetails.Count == 0)
            {
                summary.RemainingMissingLine = details.Count(x => x.ClassifyStatus == DetailStatusMissingLine);
                summary.RemainingConflict = details.Count(x => x.ClassifyStatus == DetailStatusConflict);
                return summary;
            }

            var orderCycleCandidates = await LoadOrderCycleLineCandidatesAsync(unresolvedDetails, ct);
            _logger.LogInformation("【WZ 补线】ERP订单周期候选 {Candidates}", orderCycleCandidates.Count);
            var resolvedAssignments = new List<ResolvedLineAssignment>();
            foreach (var candidate in orderCycleCandidates)
            {
                var resolved = ResolveOrderCycleLineCandidate(candidate);
                if (resolved != null)
                {
                    resolvedAssignments.Add(resolved);
                }
            }

            if (resolvedAssignments.Count == 0)
            {
                summary.RemainingMissingLine = details.Count(x => x.ClassifyStatus == DetailStatusMissingLine);
                summary.RemainingConflict = details.Count(x => x.ClassifyStatus == DetailStatusConflict);
            }
            else
            {
                var lookups = BuildResolvedLineLookups(resolvedAssignments);
                var filled = ApplyResolvedLineAssignments(unresolvedDetails, lookups.ByEntryId, lookups.ByBillPlanMaterial, lookups.ByBillPlan);
                summary.FilledByManual += filled.FilledByManual;
                summary.FilledByOrderCycle += filled.FilledByOrderCycle;
                summary.FilledBySyncLine += filled.FilledBySyncLine;
                summary.FilledByRule += filled.FilledByRule;
            }

            var stillUnresolved = details
                .Where(x => !IsSummarizableStatus(x.ClassifyStatus)
                    || NormalizeStr(x.ValveCategory).Length == 0
                    || NormalizeStr(x.ProductionLine).Length == 0)
                .ToList();

            if (stillUnresolved.Count > 0)
            {
                var orderTrackingCandidates = await LoadOrderTrackingLineCandidatesAsync(stillUnresolved, ct);
                var directAssignments = orderTrackingCandidates
                    .Select(ResolveOrderCycleLineCandidate)
                    .OfType<ResolvedLineAssignment>()
                    .ToList();

                if (directAssignments.Count > 0)
                {
                    var lookups = BuildResolvedLineLookups(directAssignments);
                    var filled = ApplyResolvedLineAssignments(stillUnresolved, lookups.ByEntryId, lookups.ByBillPlanMaterial, lookups.ByBillPlan);
                    summary.FilledByManual += filled.FilledByManual;
                    summary.FilledByOrderCycle += filled.FilledByOrderCycle;
                    summary.FilledBySyncLine += filled.FilledBySyncLine;
                    summary.FilledByRule += filled.FilledByRule;
                }

                stillUnresolved = details
                    .Where(x => !IsSummarizableStatus(x.ClassifyStatus)
                        || NormalizeStr(x.ValveCategory).Length == 0
                        || NormalizeStr(x.ProductionLine).Length == 0)
                    .ToList();

                if (stillUnresolved.Count > 0)
                {
                    _logger.LogInformation(
                        "【WZ 补线】ERP直接回填后仍待处理 {Details}，尝试规则服务候选 {Candidates}",
                        stillUnresolved.Count,
                        orderTrackingCandidates.Count);
                    var ruleAssignments = await ResolveLineCandidatesByRuleServiceAsync(orderTrackingCandidates, ct);

                    if (ruleAssignments.Count > 0)
                    {
                        var lookups = BuildResolvedLineLookups(ruleAssignments);
                        var filled = ApplyResolvedLineAssignments(stillUnresolved, lookups.ByEntryId, lookups.ByBillPlanMaterial, lookups.ByBillPlan);
                        summary.FilledByManual += filled.FilledByManual;
                        summary.FilledByOrderCycle += filled.FilledByOrderCycle;
                        summary.FilledBySyncLine += filled.FilledBySyncLine;
                        summary.FilledByRule += filled.FilledByRule;
                    }
                }
            }

            stillUnresolved = details
                .Where(x => !IsSummarizableStatus(x.ClassifyStatus)
                    || NormalizeStr(x.ValveCategory).Length == 0
                    || NormalizeStr(x.ProductionLine).Length == 0)
                .ToList();

            if (stillUnresolved.Count > 0)
            {
                _logger.LogInformation("【WZ 补线】进入OCP订单跟踪回填：待处理明细 {Details}", stillUnresolved.Count);
                var ocpOrderTrackingCandidates = await LoadOcpOrderTrackingLineCandidatesAsync(stillUnresolved, ct);
                _logger.LogInformation("【WZ 补线】OCP订单跟踪候选 {Candidates}", ocpOrderTrackingCandidates.Count);
                var ocpDirectAssignments = ocpOrderTrackingCandidates
                    .Select(ResolveOrderCycleLineCandidate)
                    .OfType<ResolvedLineAssignment>()
                    .ToList();

                if (ocpDirectAssignments.Count > 0)
                {
                    var lookups = BuildResolvedLineLookups(ocpDirectAssignments);
                    var filled = ApplyResolvedLineAssignments(stillUnresolved, lookups.ByEntryId, lookups.ByBillPlanMaterial, lookups.ByBillPlan);
                    summary.FilledByManual += filled.FilledByManual;
                    summary.FilledByOrderCycle += filled.FilledByOrderCycle;
                    summary.FilledBySyncLine += filled.FilledBySyncLine;
                    summary.FilledByRule += filled.FilledByRule;
                }

                stillUnresolved = details
                    .Where(x => !IsSummarizableStatus(x.ClassifyStatus)
                        || NormalizeStr(x.ValveCategory).Length == 0
                        || NormalizeStr(x.ProductionLine).Length == 0)
                    .ToList();

                if (stillUnresolved.Count > 0)
                {
                    _logger.LogInformation(
                        "【WZ 补线】OCP直接回填后仍待处理 {Details}，尝试规则服务候选 {Candidates}",
                        stillUnresolved.Count,
                        ocpOrderTrackingCandidates.Count);
                    var ocpRuleAssignments = await ResolveLineCandidatesByRuleServiceAsync(ocpOrderTrackingCandidates, ct);

                    if (ocpRuleAssignments.Count > 0)
                    {
                        var lookups = BuildResolvedLineLookups(ocpRuleAssignments);
                        var filled = ApplyResolvedLineAssignments(stillUnresolved, lookups.ByEntryId, lookups.ByBillPlanMaterial, lookups.ByBillPlan);
                        summary.FilledByManual += filled.FilledByManual;
                        summary.FilledByOrderCycle += filled.FilledByOrderCycle;
                        summary.FilledBySyncLine += filled.FilledBySyncLine;
                        summary.FilledByRule += filled.FilledByRule;
                    }
                }
            }

            summary.RemainingMissingLine = details.Count(x => x.ClassifyStatus == DetailStatusMissingLine);
            summary.RemainingConflict = details.Count(x => x.ClassifyStatus == DetailStatusConflict);
            return summary;
        }

        private async Task<(List<ProductionOutputDetailRow> Details, int RawCount, int SkippedNoDate, int SkippedNoKey, int Matched, int MissingLine, int Conflict)>
            LoadProductionOutputDetailsFromOrderTrackingAsync(DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            var start = startDate.Date;
            var endExclusive = endDate.Date.AddDays(1);

            var orderRows = await _db.Set<OCP_OrderTracking>()
                .AsNoTracking()
                .Where(x => x.PrdScheduleDate.HasValue
                    && x.PrdScheduleDate.Value >= start
                    && x.PrdScheduleDate.Value < endExclusive)
                .Select(x => new OrderTrackingMaterialRow
                {
                    TrackingId = x.Id,
                    EntryId = x.SOEntryID,
                    SalesOrderNo = x.SOBillNo,
                    PlanTrackingNo = x.MtoNo,
                    MaterialId = x.MaterialID,
                    MaterialCode = x.MaterialNumber,
                    MaterialName = x.MaterialName,
                    SpecModel = x.TopSpecification,
                    ProductModel = x.ProductionModel,
                    Quantity = x.OrderQty ?? 0m,
                    ProductionDate = x.PrdScheduleDate,
                    OrderApprovedDate = x.OrderAuditDate,
                    ReplyDeliveryDate = x.ReplyDeliveryDate,
                    RequestedDeliveryDate = x.DeliveryDate
                })
                .ToListAsync(ct);

            var details = new List<ProductionOutputDetailRow>(orderRows.Count);
            var skippedNoDate = 0;
            var skippedNoKey = 0;
            foreach (var row in orderRows)
            {
                if (!row.ProductionDate.HasValue)
                {
                    skippedNoDate++;
                    continue;
                }

                var businessKey = BuildOrderTrackingBusinessKey(row);
                if (businessKey.Length == 0)
                {
                    skippedNoKey++;
                    continue;
                }

                details.Add(new ProductionOutputDetailRow
                {
                    BusinessKey = businessKey,
                    EntryId = row.EntryId,
                    BillNo = NormalizeStr(row.SalesOrderNo),
                    PlanTrackingNo = NormalizeStr(row.PlanTrackingNo),
                    Seq = null,
                    MaterialKey = BuildMaterialKey(row.MaterialCode, MaterialIdToKey(row.MaterialId)),
                    MaterialCode = NormalizeStr(row.MaterialCode),
                    MaterialId = MaterialIdToKey(row.MaterialId),
                    SpecModel = NormalizeStr(row.SpecModel),
                    ProductModel = NormalizeStr(row.ProductModel),
                    ProductionDate = row.ProductionDate.Value.Date,
                    ValveCategory = string.Empty,
                    ProductionLine = string.Empty,
                    Quantity = row.Quantity,
                    ClassifyStatus = DetailStatusMissingLine,
                    RawRowCount = 1,
                    LineCandidateCount = 0,
                    SourceStartDate = start,
                    SourceEndDate = endDate.Date
                });
            }

            var ocpMaterialEnriched = await EnrichDetailMaterialsFromOcpOrderTrackingAsync(details, ct);
            var materialEnriched = await EnrichDetailMaterialModelsAsync(details, ct);
            if (materialEnriched > 0)
            {
                _logger.LogInformation("【WZ OCP口径】已补充物料规格型号 {Rows} 行，OCP订单跟踪补齐 {OcpRows} 行", materialEnriched, ocpMaterialEnriched);
            }

            var duplicateKeys = details
                .GroupBy(x => x.BusinessKey, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();
            if (duplicateKeys.Count > 0)
            {
                _logger.LogWarning(
                    "【WZ OCP口径】检测到 {DuplicateKeyCount} 个重复业务键，样例：{Samples}",
                    duplicateKeys.Count,
                    string.Join("; ", duplicateKeys.Take(10).Select(g => $"{g.Key}:{g.Count()}")));
            }

            var backfill = await BackfillDetailRowsAsync(details, ct);
            LogUnresolvedDetailRows(details);

            var summarizable = details.Count(x => IsSummarizableStatus(x.ClassifyStatus));
            var missingLine = details.Count(x => x.ClassifyStatus == DetailStatusMissingLine);
            var conflict = details.Count(x => x.ClassifyStatus == DetailStatusConflict);

            _logger.LogInformation(
                "【WZ OCP口径】OCP行 {Raw}，明细 {Details}，可汇总 {Matched}，缺产线 {MissingLine}，产线冲突 {Conflict}，待补齐 {BackfillCandidates}，人工规则补齐 {FilledByManual}，WZ_OrderCycleBase补齐 {FilledByOrderCycle}，同步产线补齐 {FilledBySyncLine}，规则补齐 {FilledByRule}，无日期跳过 {NoDate}，无业务键跳过 {NoKey}",
                orderRows.Count,
                details.Count,
                summarizable,
                missingLine,
                conflict,
                backfill.Candidates,
                backfill.FilledByManual,
                backfill.FilledByOrderCycle,
                backfill.FilledBySyncLine,
                backfill.FilledByRule,
                skippedNoDate,
                skippedNoKey);

            return (details, orderRows.Count, skippedNoDate, skippedNoKey, summarizable, missingLine, conflict);
        }

        private static WZProductionOutputRefreshResultDto BuildRefreshResult(
            IReadOnlyList<ProductionOutputDetailRow> details,
            DateTime startDate,
            DateTime endDate,
            string source,
            int rawRows)
        {
            var safeDetails = details ?? Array.Empty<ProductionOutputDetailRow>();
            var summaryRows = safeDetails
                .GroupBy(x => new
                {
                    Date = x.ProductionDate.Date,
                    ValveCategory = NormalizeStr(x.ValveCategory).Length > 0
                        ? NormalizeStr(x.ValveCategory)
                        : UnknownValveCategory,
                    ProductionLine = IsSummarizableStatus(x.ClassifyStatus)
                        && NormalizeStr(x.ProductionLine).Length > 0
                            ? NormalizeStr(x.ProductionLine)
                            : UnknownProductionLine
                })
                .Select(g => new { Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            var unresolvedSamples = safeDetails
                .Where(x => !IsSummarizableStatus(x.ClassifyStatus)
                    || NormalizeStr(x.ValveCategory).Length == 0
                    || NormalizeStr(x.ProductionLine).Length == 0)
                .OrderBy(x => x.ProductionDate)
                .ThenBy(x => x.BillNo)
                .ThenBy(x => x.PlanTrackingNo)
                .Take(50)
                .Select(x => new WZProductionOutputUnresolvedSampleDto
                {
                    ProductionDate = x.ProductionDate.Date,
                    BillNo = NormalizeStr(x.BillNo),
                    PlanTrackingNo = NormalizeStr(x.PlanTrackingNo),
                    EntryId = x.EntryId,
                    ValveCategory = NormalizeStr(x.ValveCategory),
                    ProductionLine = NormalizeStr(x.ProductionLine),
                    Quantity = x.Quantity,
                    ClassifyStatus = NormalizeStr(x.ClassifyStatus),
                    RawRowCount = x.RawRowCount,
                    LineCandidateCount = x.LineCandidateCount,
                    LastSyncTime = null
                })
                .ToList();

            return new WZProductionOutputRefreshResultDto
            {
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                Source = source,
                RawRows = rawRows,
                DetailRows = safeDetails.Count,
                DistinctBills = safeDetails
                    .Select(x => NormalizeStr(x.BillNo))
                    .Where(x => x.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                DistinctBillPlans = safeDetails
                    .Where(x => NormalizeStr(x.BillNo).Length > 0 || NormalizeStr(x.PlanTrackingNo).Length > 0)
                    .Select(x => BuildBillPlanKey(x.BillNo, x.PlanTrackingNo))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                SummarizableRows = safeDetails.Count(x => IsSummarizableStatus(x.ClassifyStatus)),
                MissingLineRows = safeDetails.Count(x => x.ClassifyStatus == DetailStatusMissingLine),
                ConflictRows = safeDetails.Count(x => x.ClassifyStatus == DetailStatusConflict),
                DetailQuantity = safeDetails.Sum(x => x.Quantity),
                SummaryRows = summaryRows.Count,
                SummaryQuantity = summaryRows.Sum(x => x.Quantity),
                StatusBuckets = safeDetails
                    .GroupBy(x => NormalizeStr(x.ClassifyStatus))
                    .OrderBy(x => x.Key)
                    .Select(g => new WZProductionOutputStatusBucketDto
                    {
                        Status = g.Key,
                        Rows = g.Count(),
                        Quantity = g.Sum(x => x.Quantity)
                    })
                    .ToList(),
                UnresolvedSamples = unresolvedSamples
            };
        }

        private async Task<(List<ProductionOutputDetailRow> Details, int RawCount, int SkippedNoDate, int SkippedNoKey, int Matched, int MissingLine, int Conflict)>
            LoadProductionOutputDetailsAsync(HttpClient client, DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            var chunks = ChunkDates(startDate, endDate, ChunkDays).ToList();
            using var esbGate = new SemaphoreSlim(MaxEsbConcurrentRequests, MaxEsbConcurrentRequests);
            var chunkTasks = chunks.Select(async chunk =>
            {
                await esbGate.WaitAsync(ct);
                try
                {
                    ct.ThrowIfCancellationRequested();
                    var rows = await RequestEsbRowsAdaptiveAsync(client, chunk.S, chunk.E, ct);
                    _logger.LogInformation("  ├─ 段 {S}~{E}：ESB行 {Raw}",
                        chunk.S.ToString("yyyy-MM-dd"),
                        chunk.E.ToString("yyyy-MM-dd"),
                        rows.Count);

                    return (chunk.S, chunk.E, Rows: rows);
                }
                finally
                {
                    esbGate.Release();
                }
            }).ToList();

            var chunkResults = await Task.WhenAll(chunkTasks);
            var allRows = chunkResults
                .OrderBy(x => x.S)
                .SelectMany(x => x.Rows)
                .ToList();

            var build = BuildDetailRows(allRows, startDate, endDate);
            var ocpMaterialEnriched = await EnrichDetailMaterialsFromOcpOrderTrackingAsync(build.Details, ct);
            var materialEnriched = await EnrichDetailMaterialModelsAsync(build.Details, ct);
            var scheduleDateOverrides = await ApplyOcpScheduleDatesAsync(build.Details, ct);
            var backfill = await BackfillDetailRowsAsync(build.Details, ct);
            LogUnresolvedDetailRows(build.Details);
            var summarizable = build.Details.Count(x => IsSummarizableStatus(x.ClassifyStatus));
            var missingLine = build.Details.Count(x => x.ClassifyStatus == DetailStatusMissingLine);
            var conflict = build.Details.Count(x => x.ClassifyStatus == DetailStatusConflict);

            _logger.LogInformation(
                "【WZ 明细去重】ESB行 {Raw}，明细键 {Details}，可汇总 {Matched}，缺产线 {MissingLine}，产线冲突 {Conflict}，OCP物料补齐 {OcpMaterialEnriched}，物料规格补齐 {MaterialEnriched}，同步排产日期覆盖 {ScheduleDateOverrides}，待补齐 {BackfillCandidates}，人工规则补齐 {FilledByManual}，WZ_OrderCycleBase补齐 {FilledByOrderCycle}，同步产线补齐 {FilledBySyncLine}，规则补齐 {FilledByRule}，无日期跳过 {NoDate}，无业务键跳过 {NoKey}",
                allRows.Count,
                build.Details.Count,
                summarizable,
                missingLine,
                conflict,
                ocpMaterialEnriched,
                materialEnriched,
                scheduleDateOverrides,
                backfill.Candidates,
                backfill.FilledByManual,
                backfill.FilledByOrderCycle,
                backfill.FilledBySyncLine,
                backfill.FilledByRule,
                build.SkippedNoDate,
                build.SkippedNoKey);

            return (build.Details, allRows.Count, build.SkippedNoDate, build.SkippedNoKey, summarizable, missingLine, conflict);
        }

        private void LogUnresolvedDetailRows(IReadOnlyList<ProductionOutputDetailRow> details)
        {
            var unresolved = details?
                .Where(x => !IsSummarizableStatus(x.ClassifyStatus)
                    || NormalizeStr(x.ValveCategory).Length == 0
                    || NormalizeStr(x.ProductionLine).Length == 0)
                .ToList() ?? new List<ProductionOutputDetailRow>();

            if (unresolved.Count == 0)
            {
                return;
            }

            var topBills = unresolved
                .GroupBy(x => NormalizeStr(x.BillNo))
                .Select(g => new
                {
                    BillNo = g.Key.Length == 0 ? "(空订单号)" : g.Key,
                    Rows = g.Count(),
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Rows)
                .ThenBy(x => x.BillNo)
                .Take(10)
                .Select(x => $"{x.BillNo}:{x.Rows}/{x.Quantity:0.######}");

            var topDates = unresolved
                .GroupBy(x => x.ProductionDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Rows = g.Count(),
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Rows)
                .ThenBy(x => x.Date)
                .Take(10)
                .Select(x => $"{x.Date:yyyy-MM-dd}:{x.Rows}/{x.Quantity:0.######}");

            var samples = unresolved
                .OrderBy(x => x.ProductionDate)
                .ThenBy(x => x.BillNo)
                .ThenBy(x => x.PlanTrackingNo)
                .Take(10)
                .Select(x => $"{x.ProductionDate:yyyy-MM-dd}|{x.BillNo}|{x.PlanTrackingNo}|{x.EntryId}|{x.ClassifyStatus}|qty={x.Quantity:0.######}");

            _logger.LogWarning(
                "【WZ 未归属明细】剩余 {Rows} 行将归入未知产线。按订单Top：{TopBills}；按排产日期Top：{TopDates}；样例：{Samples}",
                unresolved.Count,
                string.Join("; ", topBills),
                string.Join("; ", topDates),
                string.Join("; ", samples));
        }

        private async Task ClearTableAsync(string tableName, CancellationToken ct)
        {
            var (truncateSql, deleteSql) = tableName switch
            {
                "WZ_ProductionOutput" => (
                    "TRUNCATE TABLE [dbo].[WZ_ProductionOutput];",
                    "DELETE FROM [dbo].[WZ_ProductionOutput];"),
                "WZ_ProductionOutputDetail" => (
                    "TRUNCATE TABLE [dbo].[WZ_ProductionOutputDetail];",
                    "DELETE FROM [dbo].[WZ_ProductionOutputDetail];"),
                _ => throw new ArgumentOutOfRangeException(nameof(tableName), tableName, "不支持清空该表")
            };

            try
            {
                await _db.Database.ExecuteSqlRawAsync(truncateSql, ct);
            }
            catch
            {
                await _db.Database.ExecuteSqlRawAsync(deleteSql, ct);
            }
        }

        private static DataTable BuildProductionOutputDetailDataTable(IReadOnlyList<ProductionOutputDetailRow> details)
        {
            var table = new DataTable();
            table.Columns.Add("BusinessKey", typeof(string));
            table.Columns.Add("EntryId", typeof(long));
            table.Columns.Add("BillNo", typeof(string));
            table.Columns.Add("PlanTrackingNo", typeof(string));
            table.Columns.Add("Seq", typeof(int));
            table.Columns.Add("MaterialKey", typeof(string));
            table.Columns.Add("MaterialCode", typeof(string));
            table.Columns.Add("MaterialId", typeof(string));
            table.Columns.Add("SpecModel", typeof(string));
            table.Columns.Add("ProductModel", typeof(string));
            table.Columns.Add("ProductionDate", typeof(DateTime));
            table.Columns.Add("ValveCategory", typeof(string));
            table.Columns.Add("ProductionLine", typeof(string));
            table.Columns.Add("Quantity", typeof(decimal));
            table.Columns.Add("ClassifyStatus", typeof(string));
            table.Columns.Add("RawRowCount", typeof(int));
            table.Columns.Add("LineCandidateCount", typeof(int));
            table.Columns.Add("SourceStartDate", typeof(DateTime));
            table.Columns.Add("SourceEndDate", typeof(DateTime));

            foreach (var item in details)
            {
                table.Rows.Add(
                    item.BusinessKey,
                    item.EntryId.HasValue ? item.EntryId.Value : DBNull.Value,
                    NormalizeStr(item.BillNo),
                    NormalizeStr(item.PlanTrackingNo),
                    item.Seq.HasValue ? item.Seq.Value : DBNull.Value,
                    NormalizeStr(item.MaterialKey),
                    NormalizeStr(item.MaterialCode),
                    NormalizeStr(item.MaterialId),
                    NormalizeStr(item.SpecModel),
                    NormalizeStr(item.ProductModel),
                    item.ProductionDate.Date,
                    NormalizeStr(item.ValveCategory),
                    NormalizeStr(item.ProductionLine),
                    item.Quantity,
                    NormalizeStr(item.ClassifyStatus),
                    item.RawRowCount,
                    item.LineCandidateCount,
                    item.SourceStartDate.Date,
                    item.SourceEndDate.Date);
            }

            return table;
        }

        private async Task<int> UpsertProductionOutputDetailsAsync(IReadOnlyList<ProductionOutputDetailRow> details, CancellationToken ct)
        {
            if (details == null || details.Count == 0)
            {
                return 0;
            }

            var sqlConnection = _db.Database.GetDbConnection() as SqlConnection;
            if (sqlConnection == null)
            {
                throw new InvalidOperationException("WZ 明细批量入库需要 SQL Server 连接");
            }

            var sqlTransaction = _db.Database.CurrentTransaction?.GetDbTransaction() as SqlTransaction;
            if (sqlTransaction == null)
            {
                throw new InvalidOperationException("WZ 明细批量入库必须在刷新事务内执行");
            }

            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync(ct);
            }

            using (var createCommand = new SqlCommand(@"
IF OBJECT_ID('tempdb..#WZProductionOutputDetailImport') IS NOT NULL
    DROP TABLE #WZProductionOutputDetailImport;

CREATE TABLE #WZProductionOutputDetailImport
(
    [BusinessKey] NVARCHAR(300) COLLATE DATABASE_DEFAULT NOT NULL,
    [EntryId] BIGINT NULL,
    [BillNo] NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL,
    [PlanTrackingNo] NVARCHAR(255) COLLATE DATABASE_DEFAULT NULL,
    [Seq] INT NULL,
    [MaterialKey] NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL,
    [MaterialCode] NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL,
    [MaterialId] NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL,
    [SpecModel] NVARCHAR(255) COLLATE DATABASE_DEFAULT NULL,
    [ProductModel] NVARCHAR(255) COLLATE DATABASE_DEFAULT NULL,
    [ProductionDate] DATE NOT NULL,
    [ValveCategory] NVARCHAR(50) COLLATE DATABASE_DEFAULT NOT NULL,
    [ProductionLine] NVARCHAR(50) COLLATE DATABASE_DEFAULT NOT NULL,
    [Quantity] DECIMAL(18,6) NOT NULL,
    [ClassifyStatus] NVARCHAR(30) COLLATE DATABASE_DEFAULT NOT NULL,
    [RawRowCount] INT NOT NULL,
    [LineCandidateCount] INT NOT NULL,
    [SourceStartDate] DATE NULL,
    [SourceEndDate] DATE NULL
);", sqlConnection, sqlTransaction))
            {
                createCommand.CommandTimeout = 0;
                await createCommand.ExecuteNonQueryAsync(ct);
            }

            var table = BuildProductionOutputDetailDataTable(details);
            using (var bulk = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.CheckConstraints, sqlTransaction))
            {
                bulk.DestinationTableName = "#WZProductionOutputDetailImport";
                bulk.BatchSize = InsertBatchSize;
                bulk.BulkCopyTimeout = 0;
                foreach (DataColumn column in table.Columns)
                {
                    bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                await bulk.WriteToServerAsync(table, ct);
            }

            using (var mergeCommand = new SqlCommand(@"
CREATE INDEX [IX_WZProductionOutputDetailImport_BusinessKey]
    ON #WZProductionOutputDetailImport([BusinessKey]);

CREATE INDEX [IX_WZProductionOutputDetailImport_EntryId]
    ON #WZProductionOutputDetailImport([EntryId])
    INCLUDE ([BusinessKey])
    WHERE [EntryId] IS NOT NULL;

CREATE INDEX [IX_WZProductionOutputDetailImport_BillPlanSeq]
    ON #WZProductionOutputDetailImport([BillNo], [PlanTrackingNo], [Seq])
    INCLUDE ([BusinessKey]);

CREATE TABLE #WZProductionOutputDetailMergeResult([Action] NVARCHAR(10) NOT NULL);

DELETE target
FROM [dbo].[WZ_ProductionOutputDetail] target
INNER JOIN #WZProductionOutputDetailImport source
    ON source.[EntryId] IS NOT NULL
   AND target.[BusinessKey] = CONCAT(N'E:', CONVERT(NVARCHAR(50), source.[EntryId]))
   AND target.[BusinessKey] <> source.[BusinessKey];

DELETE target
FROM [dbo].[WZ_ProductionOutputDetail] target
INNER JOIN #WZProductionOutputDetailImport source
    ON target.[BusinessKey] <> source.[BusinessKey]
   AND target.[BusinessKey] NOT LIKE N'%|M:%'
   AND target.[BusinessKey] NOT LIKE N'OCP:%'
   AND (target.[BillNo] = source.[BillNo] OR (target.[BillNo] IS NULL AND source.[BillNo] IS NULL))
   AND (target.[PlanTrackingNo] = source.[PlanTrackingNo] OR (target.[PlanTrackingNo] IS NULL AND source.[PlanTrackingNo] IS NULL))
   AND (target.[Seq] = source.[Seq] OR (target.[Seq] IS NULL AND source.[Seq] IS NULL));

MERGE [dbo].[WZ_ProductionOutputDetail] WITH (HOLDLOCK) AS target
USING #WZProductionOutputDetailImport AS source
ON target.[BusinessKey] = source.[BusinessKey]
WHEN MATCHED THEN
    UPDATE SET
        [EntryId] = source.[EntryId],
        [BillNo] = source.[BillNo],
        [PlanTrackingNo] = source.[PlanTrackingNo],
        [Seq] = source.[Seq],
        [MaterialKey] = source.[MaterialKey],
        [MaterialCode] = source.[MaterialCode],
        [MaterialId] = source.[MaterialId],
        [SpecModel] = source.[SpecModel],
        [ProductModel] = source.[ProductModel],
        [ProductionDate] = source.[ProductionDate],
        [ValveCategory] = source.[ValveCategory],
        [ProductionLine] = source.[ProductionLine],
        [Quantity] = source.[Quantity],
        [ClassifyStatus] = source.[ClassifyStatus],
        [RawRowCount] = source.[RawRowCount],
        [LineCandidateCount] = source.[LineCandidateCount],
        [SourceStartDate] = source.[SourceStartDate],
        [SourceEndDate] = source.[SourceEndDate],
        [LastSyncTime] = GETDATE(),
        [ModifyDate] = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (
        [BusinessKey], [EntryId], [BillNo], [PlanTrackingNo], [Seq],
        [MaterialKey], [MaterialCode], [MaterialId], [SpecModel], [ProductModel],
        [ProductionDate], [ValveCategory], [ProductionLine], [Quantity],
        [ClassifyStatus], [RawRowCount], [LineCandidateCount],
        [SourceStartDate], [SourceEndDate], [LastSyncTime], [CreateDate], [ModifyDate]
    )
    VALUES (
        source.[BusinessKey], source.[EntryId], source.[BillNo], source.[PlanTrackingNo], source.[Seq],
        source.[MaterialKey], source.[MaterialCode], source.[MaterialId], source.[SpecModel], source.[ProductModel],
        source.[ProductionDate], source.[ValveCategory], source.[ProductionLine], source.[Quantity],
        source.[ClassifyStatus], source.[RawRowCount], source.[LineCandidateCount],
        source.[SourceStartDate], source.[SourceEndDate], GETDATE(), GETDATE(), GETDATE()
    )
OUTPUT $action INTO #WZProductionOutputDetailMergeResult;

SELECT COUNT(1) FROM #WZProductionOutputDetailMergeResult;", sqlConnection, sqlTransaction))
            {
                mergeCommand.CommandTimeout = 0;
                var result = await mergeCommand.ExecuteScalarAsync(ct);
                var processed = Convert.ToInt32(result ?? 0);
                _logger.LogInformation("  ├─ 批量写入/更新 WZ 明细 {Written}/{Total} 行", processed, details.Count);
                return processed;
            }
        }

        private async Task<int> RebuildProductionOutputSummaryAsync(CancellationToken ct)
        {
            await ClearTableAsync("WZ_ProductionOutput", ct);

            var inserted = await _db.Database.ExecuteSqlRawAsync(@"
WITH normalized AS (
    SELECT
        d.[ProductionDate],
        CASE
            WHEN ISNULL(d.[ValveCategory], N'') <> N'' THEN d.[ValveCategory]
            ELSE N'未知阀类'
        END AS [ValveCategory],
        CASE
            WHEN d.[ClassifyStatus] IN ({0}, {1}, {2}, {3}, {4})
                 AND ISNULL(d.[ProductionLine], N'') <> N'' THEN d.[ProductionLine]
            ELSE N'未知产线'
        END AS [ProductionLine],
        d.[Quantity]
    FROM [dbo].[WZ_ProductionOutputDetail] d
)
INSERT INTO [dbo].[WZ_ProductionOutput]
    ([ProductionDate], [ValveCategory], [ProductionLine], [Quantity], [CurrentThreshold])
SELECT
    n.[ProductionDate],
    n.[ValveCategory],
    n.[ProductionLine],
    SUM(n.[Quantity]) AS [Quantity],
    MAX(t.[CurrentThreshold]) AS [CurrentThreshold]
FROM normalized n
LEFT JOIN [dbo].[WZ_ProductionOutputThreshold] t
    ON t.[ValveCategory] = n.[ValveCategory]
   AND t.[ProductionLine] = n.[ProductionLine]
GROUP BY n.[ProductionDate], n.[ValveCategory], n.[ProductionLine];

", DetailStatusMatched, DetailStatusMatchedByOrderCycle, DetailStatusMatchedBySyncLine, DetailStatusMatchedByRule, DetailStatusMatchedByManual);

            _logger.LogInformation("【WZ 汇总重算】已重建 WZ_ProductionOutput 聚合行：{Rows}", inserted);
            return inserted;
        }

        /// <summary>
        /// 刷新（全量重建）：按“修改日期窗口”拉取 ESB，重建明细去重表后重算产能汇总。
        /// 返回值：最终写入 WZ_ProductionOutput 的汇总行数。
        /// </summary>
        public async Task<int> RefreshAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
        {
            await _refreshGate.WaitAsync(ct); // 防并发
            try
            {
                if (endDate < startDate)
                    throw new ArgumentException("endDate 不能早于 startDate");

                await EnsureThresholdTableAsync(ct);
                await EnsureProductionOutputDetailTableAsync(ct);

                var client = _httpClientFactory.CreateClient("WZ");
                _logger.LogInformation("【WZ 刷新-全量重建】修改日窗口：{S} ~ {E}", startDate, endDate);
                var load = await LoadProductionOutputDetailsAsync(client, startDate.Date, endDate.Date, ct);

                using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    await ClearTableAsync("WZ_ProductionOutput", ct);
                    await ClearTableAsync("WZ_ProductionOutputDetail", ct);
                    await UpsertProductionOutputDetailsAsync(load.Details, ct);
                    var summaryRows = await RebuildProductionOutputSummaryAsync(ct);

                    await tx.CommitAsync(ct);
                    _logger.LogInformation("【WZ 刷新完成】明细键 {Details}，汇总行 {SummaryRows}", load.Details.Count, summaryRows);
                    return summaryRows;
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
        /// 预览：按 OCP_OrderTracking 排产日期窗口生成明细和汇总统计，不写库。
        /// </summary>
        public async Task<WZProductionOutputRefreshResultDto> PreviewFromOrderTrackingAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate 不能早于 startDate");

            await EnsureThresholdTableAsync(ct);
            await EnsureProductionOutputDetailTableAsync(ct);

            var load = await LoadProductionOutputDetailsFromOrderTrackingAsync(startDate.Date, endDate.Date, ct);
            return BuildRefreshResult(load.Details, startDate.Date, endDate.Date, "OCP_OrderTracking.preview", load.RawCount);
        }

        /// <summary>
        /// 刷新：按 OCP_OrderTracking 排产日期窗口重建 WZ 明细和汇总。
        /// </summary>
        public async Task<WZProductionOutputRefreshResultDto> RefreshFromOrderTrackingAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            await _refreshGate.WaitAsync(ct);
            try
            {
                if (endDate < startDate)
                    throw new ArgumentException("endDate 不能早于 startDate");

                await EnsureThresholdTableAsync(ct);
                await EnsureProductionOutputDetailTableAsync(ct);

                _logger.LogInformation("【WZ OCP口径刷新】排产日期窗口：{S} ~ {E}", startDate.Date, endDate.Date);
                var load = await LoadProductionOutputDetailsFromOrderTrackingAsync(startDate.Date, endDate.Date, ct);

                using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    await ClearTableAsync("WZ_ProductionOutput", ct);
                    await ClearTableAsync("WZ_ProductionOutputDetail", ct);
                    await UpsertProductionOutputDetailsAsync(load.Details, ct);
                    var summaryRows = await RebuildProductionOutputSummaryAsync(ct);

                    await tx.CommitAsync(ct);
                    var result = BuildRefreshResult(load.Details, startDate.Date, endDate.Date, "OCP_OrderTracking.refresh", load.RawCount);
                    result.SummaryRows = summaryRows;
                    _logger.LogInformation(
                        "【WZ OCP口径刷新完成】明细 {Details}，汇总行 {SummaryRows}，明细数量 {DetailQuantity}，汇总数量 {SummaryQuantity}",
                        result.DetailRows,
                        result.SummaryRows,
                        result.DetailQuantity,
                        result.SummaryQuantity);
                    return result;
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
        /// 增量刷新：按“修改日期窗口”拉取 ESB，幂等更新明细后重算汇总。
        /// </summary>
        public async Task<int> RefreshIncrementalAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
        {
            await _refreshGate.WaitAsync(ct);
            try
            {
                if (endDate < startDate)
                    throw new ArgumentException("endDate 不能早于 startDate");

                await EnsureThresholdTableAsync(ct);
                await EnsureProductionOutputDetailTableAsync(ct);

                var client = _httpClientFactory.CreateClient("WZ");
                _logger.LogInformation("【WZ 刷新-增量幂等】修改日窗口：{S} ~ {E}", startDate, endDate);
                var load = await LoadProductionOutputDetailsAsync(client, startDate.Date, endDate.Date, ct);

                using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    await UpsertProductionOutputDetailsAsync(load.Details, ct);
                    var summaryRows = await RebuildProductionOutputSummaryAsync(ct);

                    await tx.CommitAsync(ct);
                    _logger.LogInformation("【WZ 刷新-增量完成】明细键 {Details}，汇总行 {SummaryRows}", load.Details.Count, summaryRows);
                    return summaryRows;
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
        /// 查询：按阀体、产线、日期范围返回每日产量（从去重明细实时聚合，避免旧汇总缓存污染展示）
        /// 说明：这里的日期范围是针对 ProductionDate（排产日）的业务查询窗口。
        /// </summary>
        public async Task<List<WZ_ProductionOutput>> GetAsync(
            string valveCategory,
            string productionLine,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate 不能早于 startDate");

            return await QueryProductionOutputFromDetailsAsync(
                valveCategory,
                productionLine,
                startDate,
                endDate,
                ct);
        }

        private async Task<List<WZ_ProductionOutput>> QueryProductionOutputFromDetailsAsync(
            string valveCategory,
            string productionLine,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct)
        {
            await EnsureThresholdTableAsync(ct);
            await EnsureProductionOutputDetailTableAsync(ct);

            var rows = new List<WZ_ProductionOutput>();
            var connectionString = _db.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _db.Database.GetDbConnection().ConnectionString;
            }

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            using var cmd = new SqlCommand(@"
WITH normalized AS (
    SELECT
        d.[ProductionDate],
        CASE
            WHEN ISNULL(d.[ValveCategory], N'') <> N'' THEN d.[ValveCategory]
            ELSE N'未知阀类'
        END AS [ValveCategory],
        CASE
            WHEN d.[ClassifyStatus] IN (N'matched', N'matched_order_cycle', N'matched_sync_line', N'matched_rule', N'matched_manual')
                 AND ISNULL(d.[ProductionLine], N'') <> N'' THEN d.[ProductionLine]
            ELSE N'未知产线'
        END AS [ProductionLine],
        d.[Quantity]
    FROM [dbo].[WZ_ProductionOutputDetail] d WITH (NOLOCK)
    WHERE d.[ProductionDate] >= @StartDate
      AND d.[ProductionDate] <= @EndDate
)
SELECT
    n.[ProductionDate],
    n.[ValveCategory],
    n.[ProductionLine],
    SUM(n.[Quantity]) AS [Quantity],
    MAX(t.[CurrentThreshold]) AS [CurrentThreshold]
FROM normalized n
LEFT JOIN [dbo].[WZ_ProductionOutputThreshold] t WITH (NOLOCK)
    ON t.[ValveCategory] = n.[ValveCategory]
   AND t.[ProductionLine] = n.[ProductionLine]
WHERE (@ValveCategory = N'' OR n.[ValveCategory] = @ValveCategory)
  AND (@ProductionLine = N'' OR n.[ProductionLine] = @ProductionLine)
GROUP BY n.[ProductionDate], n.[ValveCategory], n.[ProductionLine]
ORDER BY n.[ProductionDate], n.[ValveCategory], n.[ProductionLine];", conn);

            AddDateRangeParameters(cmd, startDate.Date, endDate.Date);
            cmd.Parameters.Add("@ValveCategory", SqlDbType.NVarChar, 50).Value = NormalizeStr(valveCategory);
            cmd.Parameters.Add("@ProductionLine", SqlDbType.NVarChar, 50).Value = NormalizeStr(productionLine);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new WZ_ProductionOutput
                {
                    Id = 0,
                    ProductionDate = Convert.ToDateTime(reader["ProductionDate"]).Date,
                    ValveCategory = ReadString(reader, "ValveCategory"),
                    ProductionLine = ReadString(reader, "ProductionLine"),
                    Quantity = ReadDecimal(reader, "Quantity"),
                    CurrentThreshold = ReadNullableDecimal(reader, "CurrentThreshold")
                });
            }

            return rows;
        }

        public async Task<List<WZProductionOutputUnknownDetailDto>> GetUnknownDetailsAsync(
            DateTime startDate,
            DateTime endDate,
            int take = 100000,
            CancellationToken ct = default)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate 不能早于 startDate");

            await EnsureProductionOutputDetailTableAsync(ct);
            take = Math.Clamp(take, 1, 200000);

            var result = new List<WZProductionOutputUnknownDetailDto>();
            var connectionString = _db.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _db.Database.GetDbConnection().ConnectionString;
            }

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            using var cmd = new SqlCommand(@"
SELECT TOP (@Take)
    d.[ProductionDate],
    ISNULL(d.[BusinessKey], N'') AS [BusinessKey],
    ISNULL(d.[BillNo], N'') AS [BillNo],
    ISNULL(d.[PlanTrackingNo], N'') AS [PlanTrackingNo],
    d.[EntryId],
    d.[Seq],
    ISNULL(d.[MaterialKey], N'') AS [MaterialKey],
    CASE
        WHEN NULLIF(mat.[MaterialCode], N'') IS NOT NULL
             AND (ISNULL(d.[MaterialCode], N'') = N'' OR d.[MaterialCode] <> mat.[MaterialCode])
            THEN mat.[MaterialCode]
        ELSE ISNULL(d.[MaterialCode], N'')
    END AS [MaterialCode],
    ISNULL(d.[MaterialId], N'') AS [MaterialId],
    COALESCE(NULLIF(mat.[SpecModel], N''), NULLIF(mat.[ProductModel], N''), N'') AS [SpecModel],
    ISNULL(mat.[ProductModel], N'') AS [ProductModel],
    ISNULL(d.[ValveCategory], N'') AS [ValveCategory],
    ISNULL(d.[ProductionLine], N'') AS [ProductionLine],
    ISNULL(d.[Quantity], 0) AS [Quantity],
    ISNULL(d.[ClassifyStatus], N'') AS [ClassifyStatus],
    ISNULL(d.[RawRowCount], 0) AS [RawRowCount],
    ISNULL(d.[LineCandidateCount], 0) AS [LineCandidateCount],
    d.[LastSyncTime]
FROM [dbo].[WZ_ProductionOutputDetail] d WITH (NOLOCK)
OUTER APPLY (
    SELECT TOP (1)
        CONVERT(NVARCHAR(100), m.[MaterialCode]) AS [MaterialCode],
        CONVERT(NVARCHAR(255), ISNULL(m.[SpecModel], N'')) AS [SpecModel],
        CONVERT(NVARCHAR(255), ISNULL(m.[ProductModel], N'')) AS [ProductModel],
        m.[MaterialID]
    FROM [dbo].[OCP_Material] m WITH (NOLOCK)
    WHERE (NULLIF(d.[MaterialCode], N'') IS NOT NULL AND m.[MaterialCode] = d.[MaterialCode])
       OR (TRY_CONVERT(BIGINT, NULLIF(d.[MaterialId], N'')) IS NOT NULL AND m.[MaterialID] = TRY_CONVERT(BIGINT, NULLIF(d.[MaterialId], N'')))
       OR (TRY_CONVERT(BIGINT, NULLIF(d.[MaterialKey], N'')) IS NOT NULL AND m.[MaterialID] = TRY_CONVERT(BIGINT, NULLIF(d.[MaterialKey], N'')))
       OR (TRY_CONVERT(BIGINT, NULLIF(d.[MaterialCode], N'')) IS NOT NULL AND m.[MaterialID] = TRY_CONVERT(BIGINT, NULLIF(d.[MaterialCode], N'')))
    ORDER BY CASE
        WHEN NULLIF(d.[MaterialCode], N'') IS NOT NULL AND m.[MaterialCode] = d.[MaterialCode] THEN 0
        WHEN TRY_CONVERT(BIGINT, NULLIF(d.[MaterialId], N'')) IS NOT NULL AND m.[MaterialID] = TRY_CONVERT(BIGINT, NULLIF(d.[MaterialId], N'')) THEN 1
        WHEN TRY_CONVERT(BIGINT, NULLIF(d.[MaterialKey], N'')) IS NOT NULL AND m.[MaterialID] = TRY_CONVERT(BIGINT, NULLIF(d.[MaterialKey], N'')) THEN 2
        ELSE 3
    END
) mat
WHERE d.[ProductionDate] >= @StartDate
  AND d.[ProductionDate] <= @EndDate
  AND (d.[ClassifyStatus] NOT IN (N'matched', N'matched_order_cycle', N'matched_sync_line', N'matched_rule', N'matched_manual')
       OR ISNULL(d.[ValveCategory], N'') = N''
       OR ISNULL(d.[ProductionLine], N'') = N'')
ORDER BY d.[ProductionDate], d.[BillNo], d.[PlanTrackingNo], d.[Seq], d.[BusinessKey];", conn);

            AddDateRangeParameters(cmd, startDate.Date, endDate.Date);
            cmd.Parameters.Add("@Take", SqlDbType.Int).Value = take;

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var entryOrdinal = reader.GetOrdinal("EntryId");
                var seqOrdinal = reader.GetOrdinal("Seq");
                result.Add(new WZProductionOutputUnknownDetailDto
                {
                    ProductionDate = Convert.ToDateTime(reader["ProductionDate"]).Date,
                    BusinessKey = ReadString(reader, "BusinessKey"),
                    BillNo = ReadString(reader, "BillNo"),
                    PlanTrackingNo = ReadString(reader, "PlanTrackingNo"),
                    EntryId = reader.IsDBNull(entryOrdinal) ? null : Convert.ToInt64(reader.GetValue(entryOrdinal)),
                    Seq = reader.IsDBNull(seqOrdinal) ? null : Convert.ToInt32(reader.GetValue(seqOrdinal)),
                    MaterialKey = ReadString(reader, "MaterialKey"),
                    MaterialCode = ReadString(reader, "MaterialCode"),
                    MaterialId = ReadString(reader, "MaterialId"),
                    SpecModel = ReadString(reader, "SpecModel"),
                    ProductModel = ReadString(reader, "ProductModel"),
                    ValveCategory = ReadString(reader, "ValveCategory"),
                    ProductionLine = ReadString(reader, "ProductionLine"),
                    Quantity = ReadDecimal(reader, "Quantity"),
                    ClassifyStatus = ReadString(reader, "ClassifyStatus"),
                    RawRowCount = ReadInt(reader, "RawRowCount"),
                    LineCandidateCount = ReadInt(reader, "LineCandidateCount"),
                    LastSyncTime = ReadNullableDateTime(reader, "LastSyncTime")
                });
            }

            return result;
        }

        public async Task<List<WZProductionOutputCellDetailDto>> GetCellDetailsAsync(
            DateTime productionDate,
            string valveCategory,
            string productionLine,
            int take = 10000,
            CancellationToken ct = default)
        {
            take = Math.Clamp(take, 1, 50000);
            var page = await GetCellDetailsPageInternalAsync(
                productionDate,
                valveCategory,
                productionLine,
                page: 1,
                pageSize: take,
                maxPageSize: 50000,
                ct);

            return page.Items;
        }

        public async Task<WZProductionOutputCellDetailPageDto> GetCellDetailsPageAsync(
            DateTime productionDate,
            string valveCategory,
            string productionLine,
            int page = 1,
            int pageSize = 200,
            CancellationToken ct = default)
        {
            return await GetCellDetailsPageInternalAsync(
                productionDate,
                valveCategory,
                productionLine,
                page,
                pageSize,
                maxPageSize: 1000,
                ct);
        }

        private async Task<WZProductionOutputCellDetailPageDto> GetCellDetailsPageInternalAsync(
            DateTime productionDate,
            string valveCategory,
            string productionLine,
            int page,
            int pageSize,
            int maxPageSize,
            CancellationToken ct)
        {
            await EnsureProductionOutputDetailTableAsync(ct);
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, maxPageSize);

            var normalizedValve = NormalizeStr(valveCategory);
            if (normalizedValve.Length == 0) normalizedValve = UnknownValveCategory;

            var normalizedLine = NormalizeStr(productionLine);
            if (normalizedLine.Length == 0) normalizedLine = UnknownProductionLine;

            var isUnknownValve = string.Equals(normalizedValve, UnknownValveCategory, StringComparison.OrdinalIgnoreCase);
            var isUnknownLine = string.Equals(normalizedLine, UnknownProductionLine, StringComparison.OrdinalIgnoreCase);
            var filterSql = BuildCellDetailFilterSql(isUnknownValve, isUnknownLine);
            page = Math.Min(page, (int.MaxValue / pageSize) + 1);
            var offset = (page - 1) * pageSize;

            var result = new WZProductionOutputCellDetailPageDto
            {
                Page = page,
                PageSize = pageSize
            };
            var connectionString = _db.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _db.Database.GetDbConnection().ConnectionString;
            }

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            using var cmd = new SqlCommand($@"
WITH filtered AS (
    SELECT
        d.[ProductionDate],
        ISNULL(d.[BusinessKey], N'') AS [BusinessKey],
        ISNULL(d.[BillNo], N'') AS [BillNo],
        ISNULL(d.[PlanTrackingNo], N'') AS [PlanTrackingNo],
        d.[Seq],
        ISNULL(d.[MaterialCode], N'') AS [MaterialCode],
        ISNULL(d.[MaterialId], N'') AS [MaterialId],
        ISNULL(d.[MaterialKey], N'') AS [MaterialKey],
        COALESCE(NULLIF(d.[SpecModel], N''), NULLIF(d.[ProductModel], N''), N'') AS [SpecModel],
        ISNULL(d.[ProductModel], N'') AS [ProductModel],
        ISNULL(d.[Quantity], 0) AS [Quantity],
        ISNULL(d.[ClassifyStatus], N'') AS [ClassifyStatus]
    FROM [dbo].[WZ_ProductionOutputDetail] d WITH (NOLOCK)
    WHERE {filterSql}
)
SELECT
    COUNT(1) AS [TotalRows],
    ISNULL(SUM([Quantity]), 0) AS [TotalQuantity]
FROM filtered;

WITH filtered AS (
    SELECT
        d.[ProductionDate],
        ISNULL(d.[BusinessKey], N'') AS [BusinessKey],
        ISNULL(d.[BillNo], N'') AS [BillNo],
        ISNULL(d.[PlanTrackingNo], N'') AS [PlanTrackingNo],
        d.[Seq],
        ISNULL(d.[MaterialCode], N'') AS [MaterialCode],
        ISNULL(d.[MaterialId], N'') AS [MaterialId],
        ISNULL(d.[MaterialKey], N'') AS [MaterialKey],
        COALESCE(NULLIF(d.[SpecModel], N''), NULLIF(d.[ProductModel], N''), N'') AS [SpecModel],
        ISNULL(d.[ProductModel], N'') AS [ProductModel],
        ISNULL(d.[Quantity], 0) AS [Quantity],
        ISNULL(d.[ClassifyStatus], N'') AS [ClassifyStatus]
    FROM [dbo].[WZ_ProductionOutputDetail] d WITH (NOLOCK)
    WHERE {filterSql}
),
paged AS (
    SELECT *
    FROM filtered
    ORDER BY [BillNo], [PlanTrackingNo], [Seq], [MaterialCode], [MaterialId], [MaterialKey], [BusinessKey]
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
)
SELECT
    p.[ProductionDate],
    p.[BillNo],
    p.[PlanTrackingNo],
    p.[Seq],
    p.[MaterialCode],
    p.[MaterialId],
    p.[MaterialKey],
    p.[SpecModel],
    p.[ProductModel],
    p.[Quantity],
    p.[ClassifyStatus]
FROM paged p
ORDER BY p.[BillNo], p.[PlanTrackingNo], p.[Seq], p.[MaterialCode], p.[MaterialId], p.[MaterialKey], p.[BusinessKey];", conn);

            cmd.Parameters.Add("@ProductionDate", SqlDbType.Date).Value = productionDate.Date;
            cmd.Parameters.Add("@ValveCategory", SqlDbType.NVarChar, 50).Value = normalizedValve;
            cmd.Parameters.Add("@ProductionLine", SqlDbType.NVarChar, 50).Value = normalizedLine;
            cmd.Parameters.Add("@Offset", SqlDbType.Int).Value = offset;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                result.TotalRows = ReadInt(reader, "TotalRows");
                result.TotalQuantity = ReadDecimal(reader, "TotalQuantity");
            }

            await reader.NextResultAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var seqOrdinal = reader.GetOrdinal("Seq");
                result.Items.Add(new WZProductionOutputCellDetailDto
                {
                    ProductionDate = Convert.ToDateTime(reader["ProductionDate"]).Date,
                    BillNo = ReadString(reader, "BillNo"),
                    PlanTrackingNo = ReadString(reader, "PlanTrackingNo"),
                    Seq = reader.IsDBNull(seqOrdinal) ? null : Convert.ToInt32(reader.GetValue(seqOrdinal)),
                    MaterialCode = ReadString(reader, "MaterialCode"),
                    MaterialId = ReadString(reader, "MaterialId"),
                    MaterialKey = ReadString(reader, "MaterialKey"),
                    SpecModel = ReadString(reader, "SpecModel"),
                    ProductModel = ReadString(reader, "ProductModel"),
                    Quantity = ReadDecimal(reader, "Quantity"),
                    ClassifyStatus = ReadString(reader, "ClassifyStatus")
                });
            }

            return result;
        }

        private static string BuildCellDetailFilterSql(bool isUnknownValve, bool isUnknownLine)
        {
            var statusSql = GetSummarizableStatusSqlList();
            var sql = new StringBuilder("d.[ProductionDate] = @ProductionDate");
            sql.AppendLine();
            sql.Append(isUnknownValve
                ? "  AND (d.[ValveCategory] = N'' OR d.[ValveCategory] IS NULL)"
                : "  AND d.[ValveCategory] = @ValveCategory");
            sql.AppendLine();
            sql.Append(isUnknownLine
                ? $"  AND (d.[ClassifyStatus] NOT IN ({statusSql}) OR d.[ProductionLine] = N'' OR d.[ProductionLine] IS NULL)"
                : $"  AND d.[ClassifyStatus] IN ({statusSql}) AND d.[ProductionLine] = @ProductionLine");
            return sql.ToString();
        }

        private static string GetSummarizableStatusSqlList()
            => $"N'{DetailStatusMatched}', N'{DetailStatusMatchedByOrderCycle}', N'{DetailStatusMatchedBySyncLine}', N'{DetailStatusMatchedByRule}', N'{DetailStatusMatchedByManual}'";

        private async Task<List<ProductionOutputDetailRow>> LoadExistingUnresolvedDetailRowsAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct)
        {
            var result = new List<ProductionOutputDetailRow>();
            var connectionString = _db.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _db.Database.GetDbConnection().ConnectionString;
            }

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            using var cmd = new SqlCommand(@"
SELECT
    ISNULL(d.[BusinessKey], N'') AS [BusinessKey],
    d.[EntryId],
    ISNULL(d.[BillNo], N'') AS [BillNo],
    ISNULL(d.[PlanTrackingNo], N'') AS [PlanTrackingNo],
    d.[Seq],
    ISNULL(d.[MaterialKey], N'') AS [MaterialKey],
    CASE
        WHEN NULLIF(mat.[MaterialCode], N'') IS NOT NULL
             AND (ISNULL(d.[MaterialCode], N'') = N'' OR d.[MaterialCode] <> mat.[MaterialCode])
            THEN mat.[MaterialCode]
        ELSE ISNULL(d.[MaterialCode], N'')
    END AS [MaterialCode],
    ISNULL(d.[MaterialId], N'') AS [MaterialId],
    COALESCE(NULLIF(d.[SpecModel], N''), NULLIF(d.[ProductModel], N''), N'') AS [SpecModel],
    ISNULL(d.[ProductModel], N'') AS [ProductModel],
    d.[ProductionDate],
    ISNULL(d.[ValveCategory], N'') AS [ValveCategory],
    ISNULL(d.[ProductionLine], N'') AS [ProductionLine],
    ISNULL(d.[Quantity], 0) AS [Quantity],
    ISNULL(d.[ClassifyStatus], N'') AS [ClassifyStatus],
    ISNULL(d.[RawRowCount], 0) AS [RawRowCount],
    ISNULL(d.[LineCandidateCount], 0) AS [LineCandidateCount],
    d.[SourceStartDate],
    d.[SourceEndDate]
FROM [dbo].[WZ_ProductionOutputDetail] d WITH (NOLOCK)
OUTER APPLY (
    SELECT TOP (1)
        CONVERT(NVARCHAR(100), m.[MaterialCode]) AS [MaterialCode],
        m.[MaterialID]
    FROM [dbo].[OCP_Material] m WITH (NOLOCK)
    WHERE (NULLIF(d.[MaterialCode], N'') IS NOT NULL AND m.[MaterialCode] = d.[MaterialCode])
       OR (TRY_CONVERT(BIGINT, NULLIF(d.[MaterialId], N'')) IS NOT NULL AND m.[MaterialID] = TRY_CONVERT(BIGINT, NULLIF(d.[MaterialId], N'')))
       OR (TRY_CONVERT(BIGINT, NULLIF(d.[MaterialKey], N'')) IS NOT NULL AND m.[MaterialID] = TRY_CONVERT(BIGINT, NULLIF(d.[MaterialKey], N'')))
       OR (TRY_CONVERT(BIGINT, NULLIF(d.[MaterialCode], N'')) IS NOT NULL AND m.[MaterialID] = TRY_CONVERT(BIGINT, NULLIF(d.[MaterialCode], N'')))
    ORDER BY CASE
        WHEN NULLIF(d.[MaterialCode], N'') IS NOT NULL AND m.[MaterialCode] = d.[MaterialCode] THEN 0
        WHEN TRY_CONVERT(BIGINT, NULLIF(d.[MaterialId], N'')) IS NOT NULL AND m.[MaterialID] = TRY_CONVERT(BIGINT, NULLIF(d.[MaterialId], N'')) THEN 1
        WHEN TRY_CONVERT(BIGINT, NULLIF(d.[MaterialKey], N'')) IS NOT NULL AND m.[MaterialID] = TRY_CONVERT(BIGINT, NULLIF(d.[MaterialKey], N'')) THEN 2
        ELSE 3
    END
) mat
WHERE d.[ProductionDate] >= @StartDate
  AND d.[ProductionDate] <= @EndDate
  AND (d.[ClassifyStatus] NOT IN (N'matched', N'matched_order_cycle', N'matched_sync_line', N'matched_rule', N'matched_manual')
       OR ISNULL(d.[ValveCategory], N'') = N''
       OR ISNULL(d.[ProductionLine], N'') = N'')
ORDER BY d.[ProductionDate], d.[BillNo], d.[PlanTrackingNo], d.[Seq], d.[BusinessKey];", conn);

            AddDateRangeParameters(cmd, startDate.Date, endDate.Date);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var entryOrdinal = reader.GetOrdinal("EntryId");
                var seqOrdinal = reader.GetOrdinal("Seq");
                var sourceStart = ReadNullableDateTime(reader, "SourceStartDate")?.Date ?? startDate.Date;
                var sourceEnd = ReadNullableDateTime(reader, "SourceEndDate")?.Date ?? endDate.Date;
                result.Add(new ProductionOutputDetailRow
                {
                    BusinessKey = ReadString(reader, "BusinessKey"),
                    EntryId = reader.IsDBNull(entryOrdinal) ? null : Convert.ToInt64(reader.GetValue(entryOrdinal)),
                    BillNo = ReadString(reader, "BillNo"),
                    PlanTrackingNo = ReadString(reader, "PlanTrackingNo"),
                    Seq = reader.IsDBNull(seqOrdinal) ? null : Convert.ToInt32(reader.GetValue(seqOrdinal)),
                    MaterialKey = ReadString(reader, "MaterialKey"),
                    MaterialCode = ReadString(reader, "MaterialCode"),
                    MaterialId = ReadString(reader, "MaterialId"),
                    SpecModel = ReadString(reader, "SpecModel"),
                    ProductModel = ReadString(reader, "ProductModel"),
                    ProductionDate = Convert.ToDateTime(reader["ProductionDate"]).Date,
                    ValveCategory = ReadString(reader, "ValveCategory"),
                    ProductionLine = ReadString(reader, "ProductionLine"),
                    Quantity = ReadDecimal(reader, "Quantity"),
                    ClassifyStatus = ReadString(reader, "ClassifyStatus"),
                    RawRowCount = ReadInt(reader, "RawRowCount"),
                    LineCandidateCount = ReadInt(reader, "LineCandidateCount"),
                    SourceStartDate = sourceStart,
                    SourceEndDate = sourceEnd
                });
            }

            return result;
        }

        public async Task<WZProductionOutputRefreshResultDto> ReclassifyExistingDetailsAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate 不能早于 startDate");

            await EnsureProductionOutputDetailTableAsync(ct);
            var details = await LoadExistingUnresolvedDetailRowsAsync(startDate, endDate, ct);
            if (details.Count == 0)
            {
                return BuildRefreshResult(details, startDate, endDate, "existing-detail.reclassify", 0);
            }

            _logger.LogInformation(
                "【WZ 重新归属】开始重新计算现有未知/冲突明细：日期 {Start}~{End}，明细 {Details}，数量 {Quantity}",
                startDate.ToString("yyyy-MM-dd"),
                endDate.ToString("yyyy-MM-dd"),
                details.Count,
                details.Sum(x => x.Quantity));

            var ocpMaterialEnriched = await EnrichDetailMaterialsFromOcpOrderTrackingAsync(details, ct);
            var materialEnriched = await EnrichDetailMaterialModelsAsync(details, ct);
            var backfill = await BackfillDetailRowsAsync(details, ct);
            LogUnresolvedDetailRows(details);

            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                await UpsertProductionOutputDetailsAsync(details, ct);
                var summaryRows = await RebuildProductionOutputSummaryAsync(ct);
                await tx.CommitAsync(ct);

                var result = BuildRefreshResult(details, startDate, endDate, "existing-detail.reclassify", details.Count);

                _logger.LogInformation(
                    "【WZ 重新归属完成】处理 {Details} 行，OCP物料补齐 {OcpMaterialEnriched}，物料规格补齐 {MaterialEnriched}，人工规则补齐 {FilledByManual}，WZ_OrderCycleBase补齐 {FilledByOrderCycle}，同步产线补齐 {FilledBySyncLine}，规则补齐 {FilledByRule}，剩余缺产线 {MissingLine}，冲突 {Conflict}，汇总行 {SummaryRows}",
                    details.Count,
                    ocpMaterialEnriched,
                    materialEnriched,
                    backfill.FilledByManual,
                    backfill.FilledByOrderCycle,
                    backfill.FilledBySyncLine,
                    backfill.FilledByRule,
                    result.MissingLineRows,
                    result.ConflictRows,
                    summaryRows);

                return result;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<WZProductionOutputMaterialBackfillResultDto> BackfillMaterialModelsAsync(
            DateTime startDate,
            DateTime endDate,
            int batchSize = 5000,
            CancellationToken ct = default)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate 不能早于 startDate");

            batchSize = Math.Clamp(batchSize, 100, 20000);
            await EnsureProductionOutputDetailTableAsync(ct);

            var connectionString = _db.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _db.Database.GetDbConnection().ConnectionString;
            }

            var updatedRows = 0;
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);

            while (true)
            {
                using var cmd = new SqlCommand(@"
;WITH matched AS
(
    SELECT TOP (@BatchSize)
        d.[Id],
        CONVERT(NVARCHAR(200), ISNULL(m.[MaterialCode], N'')) AS [MaterialCode],
        CONVERT(NVARCHAR(255), ISNULL(m.[SpecModel], N'')) AS [SpecModel],
        CONVERT(NVARCHAR(255), ISNULL(m.[ProductModel], N'')) AS [ProductModel]
    FROM [dbo].[WZ_ProductionOutputDetail] d WITH (READPAST)
    INNER JOIN [dbo].[OCP_Material] m WITH (NOLOCK)
        ON m.[MaterialID] = TRY_CONVERT(BIGINT, NULLIF(LTRIM(RTRIM(d.[MaterialId])), N''))
    WHERE d.[ProductionDate] >= @StartDate
      AND d.[ProductionDate] <= @EndDate
      AND (
            (NULLIF(LTRIM(RTRIM(ISNULL(d.[MaterialCode], N''))), N'') IS NULL
                AND NULLIF(LTRIM(RTRIM(ISNULL(m.[MaterialCode], N''))), N'') IS NOT NULL)
          OR (NULLIF(LTRIM(RTRIM(ISNULL(d.[SpecModel], N''))), N'') IS NULL
                AND NULLIF(LTRIM(RTRIM(ISNULL(m.[SpecModel], N''))), N'') IS NOT NULL)
          OR (NULLIF(LTRIM(RTRIM(ISNULL(d.[ProductModel], N''))), N'') IS NULL
                AND NULLIF(LTRIM(RTRIM(ISNULL(m.[ProductModel], N''))), N'') IS NOT NULL)
      )
    ORDER BY d.[Id]
)
UPDATE d
SET
    [MaterialCode] = CASE
        WHEN NULLIF(LTRIM(RTRIM(ISNULL(d.[MaterialCode], N''))), N'') IS NULL
             AND NULLIF(LTRIM(RTRIM(ISNULL(matched.[MaterialCode], N''))), N'') IS NOT NULL
            THEN matched.[MaterialCode]
        ELSE d.[MaterialCode]
    END,
    [SpecModel] = CASE
        WHEN NULLIF(LTRIM(RTRIM(ISNULL(d.[SpecModel], N''))), N'') IS NULL
             AND NULLIF(LTRIM(RTRIM(ISNULL(matched.[SpecModel], N''))), N'') IS NOT NULL
            THEN matched.[SpecModel]
        ELSE d.[SpecModel]
    END,
    [ProductModel] = CASE
        WHEN NULLIF(LTRIM(RTRIM(ISNULL(d.[ProductModel], N''))), N'') IS NULL
             AND NULLIF(LTRIM(RTRIM(ISNULL(matched.[ProductModel], N''))), N'') IS NOT NULL
            THEN matched.[ProductModel]
        ELSE d.[ProductModel]
    END,
    [ModifyDate] = GETDATE()
FROM [dbo].[WZ_ProductionOutputDetail] d
INNER JOIN matched ON matched.[Id] = d.[Id];

SELECT @@ROWCOUNT;", conn);

                cmd.Parameters.Add("@BatchSize", SqlDbType.Int).Value = batchSize;
                AddDateRangeParameters(cmd, startDate.Date, endDate.Date);
                cmd.CommandTimeout = 500;

                var affected = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
                if (affected <= 0)
                {
                    break;
                }

                updatedRows += affected;
            }

            using (var cmd = new SqlCommand(@"
CREATE TABLE #WZ_MaterialBackfillTarget
(
    [Id] INT NOT NULL PRIMARY KEY,
    [EntryId] BIGINT NULL,
    [BillNo] NVARCHAR(100) COLLATE DATABASE_DEFAULT NULL,
    [PlanTrackingNo] NVARCHAR(255) COLLATE DATABASE_DEFAULT NULL,
    [NeedMaterialCode] BIT NOT NULL,
    [NeedSpecModel] BIT NOT NULL
);

INSERT INTO #WZ_MaterialBackfillTarget ([Id], [EntryId], [BillNo], [PlanTrackingNo], [NeedMaterialCode], [NeedSpecModel])
SELECT
    d.[Id],
    TRY_CONVERT(BIGINT, d.[EntryId]) AS [EntryId],
    NULLIF(LTRIM(RTRIM(ISNULL(d.[BillNo], N''))), N'') AS [BillNo],
    NULLIF(LTRIM(RTRIM(ISNULL(d.[PlanTrackingNo], N''))), N'') AS [PlanTrackingNo],
    CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(d.[MaterialCode], N''))), N'') IS NULL THEN 1 ELSE 0 END AS [NeedMaterialCode],
    CASE
        WHEN NULLIF(LTRIM(RTRIM(ISNULL(d.[SpecModel], N''))), N'') IS NULL
             AND NULLIF(LTRIM(RTRIM(ISNULL(d.[ProductModel], N''))), N'') IS NULL
            THEN 1
        ELSE 0
    END AS [NeedSpecModel]
FROM [dbo].[WZ_ProductionOutputDetail] d WITH (READPAST)
WHERE d.[ProductionDate] >= @StartDate
  AND d.[ProductionDate] <= @EndDate
  AND (
        NULLIF(LTRIM(RTRIM(ISNULL(d.[MaterialCode], N''))), N'') IS NULL
     OR (
            NULLIF(LTRIM(RTRIM(ISNULL(d.[SpecModel], N''))), N'') IS NULL
        AND NULLIF(LTRIM(RTRIM(ISNULL(d.[ProductModel], N''))), N'') IS NULL
        )
  );

;WITH matched AS
(
    SELECT
        t.[Id],
        CONVERT(NVARCHAR(200), COALESCE(NULLIF(m.[MaterialCode], N''), NULLIF(ot.[MaterialNumber], N''), N'')) AS [MaterialCode],
        CONVERT(NVARCHAR(255), COALESCE(NULLIF(m.[SpecModel], N''), NULLIF(ot.[TopSpecification], N''), NULLIF(m.[ProductModel], N''), NULLIF(ot.[ProductionModel], N''), N'')) AS [SpecModel],
        CONVERT(NVARCHAR(255), COALESCE(NULLIF(m.[ProductModel], N''), NULLIF(ot.[ProductionModel], N''), N'')) AS [ProductModel]
    FROM #WZ_MaterialBackfillTarget t
    OUTER APPLY
    (
        SELECT TOP (1)
            CONVERT(NVARCHAR(200), ISNULL(o.[MaterialNumber], N'')) COLLATE DATABASE_DEFAULT AS [MaterialNumber],
            CONVERT(NVARCHAR(255), ISNULL(o.[TopSpecification], N'')) COLLATE DATABASE_DEFAULT AS [TopSpecification],
            CONVERT(NVARCHAR(255), ISNULL(o.[ProductionModel], N'')) COLLATE DATABASE_DEFAULT AS [ProductionModel],
            o.[MaterialID]
        FROM [dbo].[OCP_OrderTracking] o WITH (NOLOCK)
        WHERE (t.[EntryId] IS NOT NULL AND o.[SOEntryID] = t.[EntryId])
           OR (t.[EntryId] IS NULL
               AND t.[BillNo] IS NOT NULL
               AND t.[PlanTrackingNo] IS NOT NULL
               AND o.[SOBillNo] COLLATE DATABASE_DEFAULT = t.[BillNo]
               AND o.[MtoNo] COLLATE DATABASE_DEFAULT = t.[PlanTrackingNo])
        ORDER BY CASE WHEN t.[EntryId] IS NOT NULL AND o.[SOEntryID] = t.[EntryId] THEN 0 ELSE 1 END
    ) ot
    OUTER APPLY
    (
        SELECT TOP (1)
            CONVERT(NVARCHAR(200), ISNULL(m0.[MaterialCode], N'')) COLLATE DATABASE_DEFAULT AS [MaterialCode],
            CONVERT(NVARCHAR(255), ISNULL(m0.[SpecModel], N'')) COLLATE DATABASE_DEFAULT AS [SpecModel],
            CONVERT(NVARCHAR(255), ISNULL(m0.[ProductModel], N'')) COLLATE DATABASE_DEFAULT AS [ProductModel]
        FROM [dbo].[OCP_Material] m0 WITH (NOLOCK)
        WHERE (ot.[MaterialID] IS NOT NULL AND m0.[MaterialID] = ot.[MaterialID])
           OR (NULLIF(LTRIM(RTRIM(ISNULL(ot.[MaterialNumber], N''))), N'') IS NOT NULL AND m0.[MaterialCode] COLLATE DATABASE_DEFAULT = ot.[MaterialNumber])
        ORDER BY CASE WHEN ot.[MaterialID] IS NOT NULL AND m0.[MaterialID] = ot.[MaterialID] THEN 0 ELSE 1 END
    ) m
    WHERE (
              t.[NeedMaterialCode] = 1
          AND NULLIF(LTRIM(RTRIM(COALESCE(NULLIF(m.[MaterialCode], N''), NULLIF(ot.[MaterialNumber], N''), N''))), N'') IS NOT NULL
          )
       OR (
              t.[NeedSpecModel] = 1
          AND NULLIF(LTRIM(RTRIM(COALESCE(NULLIF(m.[SpecModel], N''), NULLIF(ot.[TopSpecification], N''), NULLIF(m.[ProductModel], N''), NULLIF(ot.[ProductionModel], N''), N''))), N'') IS NOT NULL
          )
)
UPDATE d
SET
    [MaterialCode] = CASE
        WHEN NULLIF(LTRIM(RTRIM(ISNULL(d.[MaterialCode], N''))), N'') IS NULL
             AND NULLIF(LTRIM(RTRIM(ISNULL(matched.[MaterialCode], N''))), N'') IS NOT NULL
            THEN matched.[MaterialCode]
        ELSE d.[MaterialCode]
    END,
    [SpecModel] = CASE
        WHEN NULLIF(LTRIM(RTRIM(ISNULL(d.[SpecModel], N''))), N'') IS NULL
             AND NULLIF(LTRIM(RTRIM(ISNULL(matched.[SpecModel], N''))), N'') IS NOT NULL
            THEN matched.[SpecModel]
        ELSE d.[SpecModel]
    END,
    [ProductModel] = CASE
        WHEN NULLIF(LTRIM(RTRIM(ISNULL(d.[ProductModel], N''))), N'') IS NULL
             AND NULLIF(LTRIM(RTRIM(ISNULL(matched.[ProductModel], N''))), N'') IS NOT NULL
            THEN matched.[ProductModel]
        ELSE d.[ProductModel]
    END,
    [ModifyDate] = GETDATE()
FROM [dbo].[WZ_ProductionOutputDetail] d
INNER JOIN matched ON matched.[Id] = d.[Id];

SELECT @@ROWCOUNT;", conn))
            {
                AddDateRangeParameters(cmd, startDate.Date, endDate.Date);
                cmd.CommandTimeout = 500;
                updatedRows += Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            }

            WZProductionOutputMaterialBackfillResultDto result;
            using (var cmd = new SqlCommand(@"
SELECT
    COUNT(1) AS [TotalRows],
    SUM(CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL([MaterialCode], N''))), N'') IS NOT NULL THEN 1 ELSE 0 END) AS [RowsWithMaterialCode],
    SUM(CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL([SpecModel], N''))), N'') IS NOT NULL THEN 1 ELSE 0 END) AS [RowsWithSpecModel],
    SUM(CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL([ProductModel], N''))), N'') IS NOT NULL THEN 1 ELSE 0 END) AS [RowsWithProductModel]
FROM [dbo].[WZ_ProductionOutputDetail] WITH (NOLOCK)
WHERE [ProductionDate] >= @StartDate
  AND [ProductionDate] <= @EndDate;", conn))
            {
                AddDateRangeParameters(cmd, startDate.Date, endDate.Date);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                await reader.ReadAsync(ct);
                result = new WZProductionOutputMaterialBackfillResultDto
                {
                    StartDate = startDate.Date,
                    EndDate = endDate.Date,
                    UpdatedRows = updatedRows,
                    TotalRows = ReadInt(reader, "TotalRows"),
                    RowsWithMaterialCode = ReadInt(reader, "RowsWithMaterialCode"),
                    RowsWithSpecModel = ReadInt(reader, "RowsWithSpecModel"),
                    RowsWithProductModel = ReadInt(reader, "RowsWithProductModel")
                };
            }

            _logger.LogInformation(
                "【WZ 物料规格回填完成】日期 {Start}~{End}，更新 {UpdatedRows} 行，规格型号 {SpecRows}/{TotalRows}",
                startDate.ToString("yyyy-MM-dd"),
                endDate.ToString("yyyy-MM-dd"),
                result.UpdatedRows,
                result.RowsWithSpecModel,
                result.TotalRows);

            return result;
        }

        public async Task<WZProductionOutputSyncHealthDto> GetSyncHealthAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken ct = default)
        {
            var start = (startDate ?? DateTime.Today.AddDays(-14)).Date;
            var end = (endDate ?? DateTime.Today.AddDays(1)).Date;
            if (end < start)
            {
                throw new ArgumentException("endDate 不能早于 startDate");
            }

            await EnsureProductionOutputDetailTableAsync(ct);

            var health = new WZProductionOutputSyncHealthDto
            {
                StartDate = start,
                EndDate = end
            };

            var connectionString = _db.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _db.Database.GetDbConnection().ConnectionString;
            }

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);

            using (var cmd = new SqlCommand(@"
SELECT
    COUNT(1) AS DetailRows,
    SUM(CASE WHEN [ClassifyStatus] IN (N'matched', N'matched_order_cycle', N'matched_sync_line', N'matched_rule', N'matched_manual')
              AND ISNULL([ValveCategory], N'') <> N''
              AND ISNULL([ProductionLine], N'') <> N'' THEN 1 ELSE 0 END) AS SummarizableRows,
    SUM(CASE WHEN [ClassifyStatus] = N'missing_line'
              OR ISNULL([ValveCategory], N'') = N''
              OR ISNULL([ProductionLine], N'') = N'' THEN 1 ELSE 0 END) AS MissingLineRows,
    SUM(CASE WHEN [ClassifyStatus] = N'conflict' THEN 1 ELSE 0 END) AS ConflictRows,
    SUM(ISNULL([Quantity], 0)) AS DetailQuantity,
    MAX([LastSyncTime]) AS LastDetailSyncTime
FROM [dbo].[WZ_ProductionOutputDetail] WITH (NOLOCK)
WHERE [ProductionDate] >= @StartDate
  AND [ProductionDate] <= @EndDate;", conn))
            {
                AddDateRangeParameters(cmd, start, end);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    health.DetailRows = ReadInt(reader, "DetailRows");
                    health.SummarizableRows = ReadInt(reader, "SummarizableRows");
                    health.MissingLineRows = ReadInt(reader, "MissingLineRows");
                    health.ConflictRows = ReadInt(reader, "ConflictRows");
                    health.DetailQuantity = ReadDecimal(reader, "DetailQuantity");
                    health.LastDetailSyncTime = ReadNullableDateTime(reader, "LastDetailSyncTime");
                }
            }

            using (var cmd = new SqlCommand(@"
SELECT
    COUNT(1) AS SummaryRows,
    SUM(ISNULL([Quantity], 0)) AS SummaryQuantity
FROM [dbo].[WZ_ProductionOutput] WITH (NOLOCK)
WHERE [ProductionDate] >= @StartDate
  AND [ProductionDate] <= @EndDate;", conn))
            {
                AddDateRangeParameters(cmd, start, end);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    health.SummaryRows = ReadInt(reader, "SummaryRows");
                    health.SummaryQuantity = ReadDecimal(reader, "SummaryQuantity");
                }
            }

            using (var cmd = new SqlCommand(@"
SELECT
    COUNT(1) AS OcpRowsInProductionDateRange,
    SUM(CASE WHEN [PrdScheduleDate] IS NULL THEN 1 ELSE 0 END) AS OcpRowsMissingProductionDate
FROM [dbo].[OCP_OrderTracking] WITH (NOLOCK)
WHERE ([PrdScheduleDate] >= @StartDate AND [PrdScheduleDate] <= @EndDate)
   OR ([PrdScheduleDate] IS NULL
       AND [ESBModifyDate] >= @StartDate
       AND [ESBModifyDate] < DATEADD(day, 1, @EndDate));

SELECT
    MAX([ESBModifyDate]) AS LatestOcpEsbModifyDate,
    MAX([ModifyDate]) AS LatestOcpModifyDate
FROM [dbo].[OCP_OrderTracking] WITH (NOLOCK);", conn))
            {
                AddDateRangeParameters(cmd, start, end);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    health.OcpRowsInProductionDateRange = ReadInt(reader, "OcpRowsInProductionDateRange");
                    health.OcpRowsMissingProductionDate = ReadInt(reader, "OcpRowsMissingProductionDate");
                }

                if (await reader.NextResultAsync(ct) && await reader.ReadAsync(ct))
                {
                    health.LatestOcpEsbModifyDate = ReadNullableDateTime(reader, "LatestOcpEsbModifyDate");
                    health.LatestOcpModifyDate = ReadNullableDateTime(reader, "LatestOcpModifyDate");
                }
            }

            health.UnresolvedByBill = await QueryHealthBucketsAsync(conn, @"
SELECT TOP (20)
    ISNULL(NULLIF([BillNo], N''), N'(空订单号)') AS [KeyValue],
    COUNT(1) AS [Rows],
    SUM(ISNULL([Quantity], 0)) AS [Quantity]
FROM [dbo].[WZ_ProductionOutputDetail] WITH (NOLOCK)
WHERE [ProductionDate] >= @StartDate
  AND [ProductionDate] <= @EndDate
  AND ([ClassifyStatus] IN (N'missing_line', N'conflict')
       OR ISNULL([ValveCategory], N'') = N''
       OR ISNULL([ProductionLine], N'') = N'')
GROUP BY ISNULL(NULLIF([BillNo], N''), N'(空订单号)')
ORDER BY COUNT(1) DESC, ISNULL(NULLIF([BillNo], N''), N'(空订单号)');", start, end, ct);

            health.UnresolvedByDate = await QueryHealthBucketsAsync(conn, @"
SELECT TOP (20)
    CONVERT(varchar(10), [ProductionDate], 120) AS [KeyValue],
    COUNT(1) AS [Rows],
    SUM(ISNULL([Quantity], 0)) AS [Quantity]
FROM [dbo].[WZ_ProductionOutputDetail] WITH (NOLOCK)
WHERE [ProductionDate] >= @StartDate
  AND [ProductionDate] <= @EndDate
  AND ([ClassifyStatus] IN (N'missing_line', N'conflict')
       OR ISNULL([ValveCategory], N'') = N''
       OR ISNULL([ProductionLine], N'') = N'')
GROUP BY [ProductionDate]
ORDER BY COUNT(1) DESC, [ProductionDate];", start, end, ct);

            health.UnresolvedSamples = await QueryUnresolvedSamplesAsync(conn, start, end, ct);
            health.Status = health.DetailRows == 0
                ? "no_detail"
                : health.MissingLineRows > 0 || health.ConflictRows > 0
                    ? "warning"
                    : "ok";

            return health;
        }

        private static void AddDateRangeParameters(SqlCommand cmd, DateTime start, DateTime end)
        {
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@StartDate", SqlDbType.Date).Value = start.Date;
            cmd.Parameters.Add("@EndDate", SqlDbType.Date).Value = end.Date;
        }

        private static int ReadInt(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal))
            {
                return 0;
            }

            return Convert.ToInt32(reader.GetValue(ordinal));
        }

        private static decimal ReadDecimal(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal))
            {
                return 0m;
            }

            return Convert.ToDecimal(reader.GetValue(ordinal));
        }

        private static decimal? ReadNullableDecimal(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return Convert.ToDecimal(reader.GetValue(ordinal));
        }

        private static string ReadString(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? string.Empty : Convert.ToString(reader.GetValue(ordinal)) ?? string.Empty;
        }

        private static DateTime? ReadNullableDateTime(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return Convert.ToDateTime(reader.GetValue(ordinal));
        }

        private static async Task<List<WZProductionOutputHealthBucketDto>> QueryHealthBucketsAsync(
            SqlConnection conn,
            string sql,
            DateTime start,
            DateTime end,
            CancellationToken ct)
        {
            var result = new List<WZProductionOutputHealthBucketDto>();
            using var cmd = new SqlCommand(sql, conn);
            AddDateRangeParameters(cmd, start, end);
            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new WZProductionOutputHealthBucketDto
                {
                    Key = ReadString(reader, "KeyValue"),
                    Rows = ReadInt(reader, "Rows"),
                    Quantity = ReadDecimal(reader, "Quantity")
                });
            }

            return result;
        }

        private static async Task<List<WZProductionOutputUnresolvedSampleDto>> QueryUnresolvedSamplesAsync(
            SqlConnection conn,
            DateTime start,
            DateTime end,
            CancellationToken ct)
        {
            var result = new List<WZProductionOutputUnresolvedSampleDto>();
            using var cmd = new SqlCommand(@"
SELECT TOP (50)
    [ProductionDate],
    ISNULL([BillNo], N'') AS [BillNo],
    ISNULL([PlanTrackingNo], N'') AS [PlanTrackingNo],
    [EntryId],
    ISNULL([ValveCategory], N'') AS [ValveCategory],
    ISNULL([ProductionLine], N'') AS [ProductionLine],
    ISNULL([Quantity], 0) AS [Quantity],
    ISNULL([ClassifyStatus], N'') AS [ClassifyStatus],
    ISNULL([RawRowCount], 0) AS [RawRowCount],
    ISNULL([LineCandidateCount], 0) AS [LineCandidateCount],
    [LastSyncTime]
FROM [dbo].[WZ_ProductionOutputDetail] WITH (NOLOCK)
WHERE [ProductionDate] >= @StartDate
  AND [ProductionDate] <= @EndDate
  AND ([ClassifyStatus] IN (N'missing_line', N'conflict')
       OR ISNULL([ValveCategory], N'') = N''
       OR ISNULL([ProductionLine], N'') = N'')
ORDER BY [ProductionDate], [BillNo], [PlanTrackingNo], [EntryId];", conn);
            AddDateRangeParameters(cmd, start, end);
            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var entryOrdinal = reader.GetOrdinal("EntryId");
                result.Add(new WZProductionOutputUnresolvedSampleDto
                {
                    ProductionDate = Convert.ToDateTime(reader["ProductionDate"]).Date,
                    BillNo = ReadString(reader, "BillNo"),
                    PlanTrackingNo = ReadString(reader, "PlanTrackingNo"),
                    EntryId = reader.IsDBNull(entryOrdinal) ? null : Convert.ToInt64(reader.GetValue(entryOrdinal)),
                    ValveCategory = ReadString(reader, "ValveCategory"),
                    ProductionLine = ReadString(reader, "ProductionLine"),
                    Quantity = ReadDecimal(reader, "Quantity"),
                    ClassifyStatus = ReadString(reader, "ClassifyStatus"),
                    RawRowCount = ReadInt(reader, "RawRowCount"),
                    LineCandidateCount = ReadInt(reader, "LineCandidateCount"),
                    LastSyncTime = ReadNullableDateTime(reader, "LastSyncTime")
                });
            }

            return result;
        }

        /// <summary>
        /// 批量写入阈值：按阀体+产线更新 CurrentThreshold
        /// </summary>
        public async Task<int> UpdateThresholdsAsync(
            IReadOnlyCollection<(string ValveCategory, string ProductionLine, decimal Threshold)> thresholds,
            CancellationToken ct = default)
        {
            if (thresholds == null || thresholds.Count == 0) return 0;

            await EnsureThresholdTableAsync(ct);

            var affected = 0;
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                foreach (var item in thresholds)
                {
                    var valve = NormalizeStr(item.ValveCategory);
                    var line = NormalizeStr(item.ProductionLine);
                    if (valve.Length == 0 || line.Length == 0) continue;

                    affected += await _db.Database.ExecuteSqlRawAsync(@"
MERGE [dbo].[WZ_ProductionOutputThreshold] WITH (HOLDLOCK) AS target
USING (SELECT {0} AS [ValveCategory], {1} AS [ProductionLine], {2} AS [CurrentThreshold]) AS source
ON target.[ValveCategory] = source.[ValveCategory]
   AND target.[ProductionLine] = source.[ProductionLine]
WHEN MATCHED THEN
    UPDATE SET [CurrentThreshold] = source.[CurrentThreshold], [ModifyDate] = GETDATE()
WHEN NOT MATCHED THEN
    INSERT ([ValveCategory], [ProductionLine], [CurrentThreshold], [CreateDate], [ModifyDate])
    VALUES (source.[ValveCategory], source.[ProductionLine], source.[CurrentThreshold], GETDATE(), GETDATE());
", valve, line, item.Threshold);

                    await _db.Database.ExecuteSqlRawAsync(
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
        /// 排产优化汇总：只按 CapacityScheduleDate × 阀体 × 产线聚合，写入 WZ_PreProductionOutput_2
        /// </summary>
        private async Task<int> RefreshPreProductionOutputOptimizedAsync(CancellationToken ct = default)
        {
            await _preProductionOptimizeGate.WaitAsync(ct);
            try
            {
                var rows = await _db.Set<WZ_PreProductionOutput>()
                    .AsNoTracking()
                    .Select(p => new
                    {
                        p.CapacityScheduleDate,
                        p.ValveCategory,
                        p.ProductionLine,
                        p.Quantity
                    })
                    .ToListAsync(ct);

                var buckets = rows
                    .Where(r => r.CapacityScheduleDate.HasValue)
                    .Select(r => new
                    {
                        Date = r.CapacityScheduleDate!.Value.Date,
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
                        await _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [WZ_PreProductionOutput_2];", ct);
                    }
                    catch
                    {
                        await _db.Database.ExecuteSqlRawAsync("DELETE FROM [WZ_PreProductionOutput_2];", ct);
                    }

                    if (buckets.Count > 0)
                    {
                        var batch = new List<WZ_PreProductionOutput_2>(InsertBatchSize);
                        foreach (var item in buckets)
                        {
                            batch.Add(new WZ_PreProductionOutput_2
                            {
                                ProductionDate = item.Date,
                                ValveCategory = item.Cat,
                                ProductionLine = item.Line,
                                Quantity = item.Sum
                            });

                            if (batch.Count >= InsertBatchSize)
                            {
                                await _db.Set<WZ_PreProductionOutput_2>().AddRangeAsync(batch, ct);
                                await _db.SaveChangesAsync(ct);
                                batch.Clear();
                            }
                        }

                        if (batch.Count > 0)
                        {
                            await _db.Set<WZ_PreProductionOutput_2>().AddRangeAsync(batch, ct);
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
                _preProductionOptimizeGate.Release();
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

            var preQuery = _db.Set<WZ_PreProductionOutput_1>()
                .AsNoTracking()
                .Where(x => x.ProductionDate >= startDate && x.ProductionDate <= endDate);

            if (!string.IsNullOrWhiteSpace(valveCategory))
            {
                preQuery = preQuery.Where(x => x.ValveCategory == valveCategory);
            }

            if (!string.IsNullOrWhiteSpace(productionLine))
            {
                preQuery = preQuery.Where(x => x.ProductionLine == productionLine);
            }

            var actualRows = await QueryProductionOutputFromDetailsAsync(
                valveCategory,
                productionLine,
                startDate,
                endDate,
                ct);
            var preRows = await preQuery.ToListAsync(ct);
            var thresholdMap = await LoadThresholdMapAsync(ct);

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
                    CurrentThreshold = ResolveThreshold(thresholdMap, key.Item2, key.Item3, row.CurrentThreshold)
                };
            }

            foreach (var row in preRows)
            {
                var key = (row.ProductionDate.Date, NormalizeStr(row.ValveCategory), NormalizeStr(row.ProductionLine));
                if (merged.TryGetValue(key, out var existing))
                {
                    existing.Quantity += row.Quantity;
                    existing.CurrentThreshold ??= ResolveThreshold(thresholdMap, key.Item2, key.Item3);
                }
                else
                {
                    merged[key] = new WZ_ProductionOutput
                    {
                        ProductionDate = row.ProductionDate.Date,
                        ValveCategory = key.Item2,
                        ProductionLine = key.Item3,
                        Quantity = row.Quantity,
                        CurrentThreshold = ResolveThreshold(thresholdMap, key.Item2, key.Item3)
                    };
                }
            }

            return merged.Values
                .OrderBy(x => x.ProductionDate)
                .ThenBy(x => x.ValveCategory)
                .ThenBy(x => x.ProductionLine)
                .ToList();
        }

        /// <summary>
        /// 查询：合并实际产量与排产优化汇总数据
        /// </summary>
        public async Task<List<WZ_ProductionOutput>> GetWithOptimizedPreProductionAsync(
            string valveCategory,
            string productionLine,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate 不能早于 startDate");

            await RefreshPreProductionOutputOptimizedAsync(ct);

            var preQuery = _db.Set<WZ_PreProductionOutput_2>()
                .AsNoTracking()
                .Where(x => x.ProductionDate >= startDate && x.ProductionDate <= endDate);

            if (!string.IsNullOrWhiteSpace(valveCategory))
            {
                preQuery = preQuery.Where(x => x.ValveCategory == valveCategory);
            }

            if (!string.IsNullOrWhiteSpace(productionLine))
            {
                preQuery = preQuery.Where(x => x.ProductionLine == productionLine);
            }

            var actualRows = await QueryProductionOutputFromDetailsAsync(
                valveCategory,
                productionLine,
                startDate,
                endDate,
                ct);
            var preRows = await preQuery.ToListAsync(ct);
            var thresholdMap = await LoadThresholdMapAsync(ct);

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
                    CurrentThreshold = ResolveThreshold(thresholdMap, key.Item2, key.Item3, row.CurrentThreshold)
                };
            }

            foreach (var row in preRows)
            {
                var key = (row.ProductionDate.Date, NormalizeStr(row.ValveCategory), NormalizeStr(row.ProductionLine));
                if (merged.TryGetValue(key, out var existing))
                {
                    existing.Quantity += row.Quantity;
                    existing.CurrentThreshold ??= ResolveThreshold(thresholdMap, key.Item2, key.Item3);
                }
                else
                {
                    merged[key] = new WZ_ProductionOutput
                    {
                        ProductionDate = row.ProductionDate.Date,
                        ValveCategory = key.Item2,
                        ProductionLine = key.Item3,
                        Quantity = row.Quantity,
                        CurrentThreshold = ResolveThreshold(thresholdMap, key.Item2, key.Item3)
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
