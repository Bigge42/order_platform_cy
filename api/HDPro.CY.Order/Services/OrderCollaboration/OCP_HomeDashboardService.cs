using HDPro.CY.Order.IServices.OrderCollaboration;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OrderCollaboration
{
    /// <summary>
    /// 首页订单运营看板聚合服务。
    /// </summary>
    public class OCP_HomeDashboardService : IOCP_HomeDashboardService, IDependency
    {
        private readonly ServiceDbContext _dbContext;
        private readonly ILogger<OCP_HomeDashboardService> _logger;

        public OCP_HomeDashboardService(
            ServiceDbContext dbContext,
            ILogger<OCP_HomeDashboardService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<WebResponseContent> GetHomeDashboardAsync(
            string dateRange,
            string businessType,
            string customer,
            string owner,
            string keyword)
        {
            var response = new WebResponseContent();
            try
            {
                var now = DateTime.Now;
                var today = now.Date;
                var range = ResolveDashboardRange(dateRange, today);
                var keywordText = (keyword ?? string.Empty).Trim();
                var businessTypes = ResolveDashboardBusinessTypes(businessType);
                var db = _dbContext;

                var orderBaseQuery = db.Set<OCP_OrderTracking>()
                    .AsNoTracking()
                    .Where(x => string.IsNullOrEmpty(x.BillStatus) || !x.BillStatus.Contains("关闭"));

                var orderQuery = orderBaseQuery.Where(x =>
                    (x.OrderCreateDate ?? x.OrderAuditDate ?? x.CreateDate) >= range.Start &&
                    (x.OrderCreateDate ?? x.OrderAuditDate ?? x.CreateDate) < range.End);

                if (!DashboardIsAll(customer))
                {
                    orderQuery = orderQuery.Where(x => x.CustName == customer);
                }

                if (!DashboardIsAll(owner))
                {
                    orderQuery = orderQuery.Where(x => x.SalesPerson == owner);
                }

                if (!string.IsNullOrWhiteSpace(keywordText))
                {
                    orderQuery = orderQuery.Where(x =>
                        (!string.IsNullOrEmpty(x.SOBillNo) && x.SOBillNo.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.MtoNo) && x.MtoNo.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.CustName) && x.CustName.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.MaterialNumber) && x.MaterialNumber.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.MaterialName) && x.MaterialName.Contains(keywordText)));
                }

                var customers = await orderBaseQuery
                    .Where(x => !string.IsNullOrEmpty(x.CustName))
                    .Select(x => x.CustName)
                    .Distinct()
                    .OrderBy(x => x)
                    .Take(80)
                    .ToListAsync();

                var owners = await orderBaseQuery
                    .Where(x => !string.IsNullOrEmpty(x.SalesPerson))
                    .Select(x => x.SalesPerson)
                    .Distinct()
                    .OrderBy(x => x)
                    .Take(80)
                    .ToListAsync();

                var orderRows = await orderQuery
                    .OrderByDescending(x => x.OrderCreateDate ?? x.OrderAuditDate ?? x.CreateDate)
                    .Take(10000)
                    .ToListAsync();

                var runningCount = await orderQuery.CountAsync(x =>
                    ((x.OrderQty ?? 0) <= 0 && (string.IsNullOrEmpty(x.FinishStatus) || !x.FinishStatus.Contains("完成"))) ||
                    ((x.OrderQty ?? 0) > 0 && (x.InstockQty ?? 0) < (x.OrderQty ?? 0)));

                var delayCount = await orderQuery.CountAsync(x =>
                    (((x.ReplyDeliveryDate.HasValue && x.ReplyDeliveryDate.Value < today) ||
                      (!x.ReplyDeliveryDate.HasValue && x.DeliveryDate.HasValue && x.DeliveryDate.Value < today))) &&
                    (((x.OrderQty ?? 0) <= 0 && (string.IsNullOrEmpty(x.FinishStatus) || !x.FinishStatus.Contains("完成"))) ||
                     ((x.OrderQty ?? 0) > 0 && (x.InstockQty ?? 0) < (x.OrderQty ?? 0))));

                var weekStart = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
                var weekEnd = weekStart.AddDays(7);
                var weekDeliveryCount = await orderQuery.CountAsync(x =>
                    x.LastInStockDate.HasValue &&
                    x.LastInStockDate.Value >= weekStart &&
                    x.LastInStockDate.Value < weekEnd);

                var orderBillQuery = orderQuery
                    .Where(x => !string.IsNullOrEmpty(x.SOBillNo))
                    .Select(x => x.SOBillNo)
                    .Distinct();

                var lackQuery = db.Set<OCP_LackMtrlResult>()
                    .AsNoTracking()
                    .Where(x => x.CreateDate >= range.Start && x.CreateDate < range.End);

                if (!DashboardIsAll(customer) || !DashboardIsAll(owner))
                {
                    lackQuery = lackQuery.Where(x => orderBillQuery.Contains(x.SOBillNo));
                }

                if (!string.IsNullOrWhiteSpace(keywordText))
                {
                    lackQuery = lackQuery.Where(x =>
                        (!string.IsNullOrEmpty(x.SOBillNo) && x.SOBillNo.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.MtoNo) && x.MtoNo.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.MaterialNumber) && x.MaterialNumber.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.MaterialName) && x.MaterialName.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.SupplierName) && x.SupplierName.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.PurchaserName) && x.PurchaserName.Contains(keywordText)));
                }

                var shortageQuery = lackQuery.Where(x =>
                    (x.UnPlanedQty ?? 0) > 0 ||
                    ((x.NeedQty ?? 0) - (x.InventoryQty ?? 0) - (x.PlanedQty ?? 0)) > 0);

                var shortageCount = await shortageQuery.CountAsync();
                var shortageRows = await shortageQuery
                    .OrderByDescending(x => x.UnPlanedQty ?? 0)
                    .ThenByDescending(x => (x.NeedQty ?? 0) - (x.InventoryQty ?? 0) - (x.PlanedQty ?? 0))
                    .Take(1000)
                    .ToListAsync();

                var urgentQuery = db.Set<OCP_UrgentOrder>()
                    .AsNoTracking()
                    .Where(x => x.CreateDate >= range.Start && x.CreateDate < range.End);

                if (businessTypes.Length > 0)
                {
                    urgentQuery = urgentQuery.Where(x => businessTypes.Contains(x.BusinessType));
                }

                if (!DashboardIsAll(owner))
                {
                    urgentQuery = urgentQuery.Where(x =>
                        x.AssignedResPerson == owner ||
                        x.AssignedResPersonName == owner ||
                        x.DefaultResPerson == owner ||
                        x.DefaultResPersonName == owner);
                }

                if (!string.IsNullOrWhiteSpace(keywordText))
                {
                    urgentQuery = urgentQuery.Where(x =>
                        (!string.IsNullOrEmpty(x.BillNo) && x.BillNo.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.PlanTraceNo) && x.PlanTraceNo.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.MaterialNumber) && x.MaterialNumber.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.MaterialName) && x.MaterialName.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.SupplierName) && x.SupplierName.Contains(keywordText)));
                }

                var urgentRows = await urgentQuery.OrderByDescending(x => x.CreateDate).ToListAsync();
                var urgentIds = urgentRows.Select(x => x.UrgentOrderID).ToList();
                var repliedUrgentIds = urgentIds.Count == 0
                    ? new List<long>()
                    : await db.Set<OCP_UrgentOrderReply>()
                        .AsNoTracking()
                        .Where(x => urgentIds.Contains(x.UrgentOrderID))
                        .Select(x => x.UrgentOrderID)
                        .Distinct()
                        .ToListAsync();
                var repliedUrgentIdSet = repliedUrgentIds.ToHashSet();
                var urgentPending = Math.Max(urgentRows.Count - repliedUrgentIdSet.Count, 0);
                var urgentOverdue = urgentRows.Count(x =>
                    !repliedUrgentIdSet.Contains(x.UrgentOrderID) &&
                    x.CreateDate.HasValue &&
                    x.AssignedReplyTime.HasValue &&
                    x.TimeUnit.HasValue &&
                    CalculateDashboardDeadline(x.CreateDate.Value, x.AssignedReplyTime.Value, x.TimeUnit.Value) < now);

                var negotiationQuery = db.Set<OCP_Negotiation>()
                    .AsNoTracking()
                    .Where(x => x.CreateDate >= range.Start && x.CreateDate < range.End);

                if (businessTypes.Length > 0)
                {
                    negotiationQuery = negotiationQuery.Where(x => businessTypes.Contains(x.BusinessType));
                }

                if (!DashboardIsAll(owner))
                {
                    negotiationQuery = negotiationQuery.Where(x =>
                        x.AssignedResPerson == owner ||
                        x.AssignedResPersonName == owner ||
                        x.DefaultResPerson == owner ||
                        x.DefaultResPersonName == owner);
                }

                if (!string.IsNullOrWhiteSpace(keywordText))
                {
                    negotiationQuery = negotiationQuery.Where(x =>
                        (!string.IsNullOrEmpty(x.BillNo) && x.BillNo.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.PlanTraceNo) && x.PlanTraceNo.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.MaterialNumber) && x.MaterialNumber.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.MaterialName) && x.MaterialName.Contains(keywordText)) ||
                        (!string.IsNullOrEmpty(x.SupplierName) && x.SupplierName.Contains(keywordText)));
                }

                var negotiationRows = await negotiationQuery.OrderByDescending(x => x.CreateDate).ToListAsync();
                var negotiationIds = negotiationRows.Select(x => x.NegotiationID).ToList();
                var repliedNegotiationIds = negotiationIds.Count == 0
                    ? new List<long>()
                    : await db.Set<OCP_NegotiationReply>()
                        .AsNoTracking()
                        .Where(x => negotiationIds.Contains(x.NegotiationID))
                        .Select(x => x.NegotiationID)
                        .Distinct()
                        .ToListAsync();
                var repliedNegotiationIdSet = repliedNegotiationIds.ToHashSet();
                var negotiationPending = Math.Max(negotiationRows.Count - repliedNegotiationIdSet.Count, 0);
                var negotiationOverdue = negotiationRows.Count(x =>
                    !repliedNegotiationIdSet.Contains(x.NegotiationID) &&
                    x.CreateDate.HasValue &&
                    x.CreateDate.Value < now.AddDays(-7));

                var progressRows = new[]
                {
                    await BuildBomProgressAsync(range.Start, range.End),
                    await BuildPurchaseProgressAsync(range.Start, range.End),
                    await BuildProductionProgressAsync(range.Start, range.End)
                };

                var orderLookup = orderRows
                    .Where(x => !string.IsNullOrWhiteSpace(x.SOBillNo))
                    .GroupBy(x => x.SOBillNo, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

                var shortageKeySet = shortageRows
                    .Select(x => BuildDashboardOrderKey(x.SOBillNo, x.MtoNo))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var riskRows = orderRows
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.CustName) ? "未维护客户" : x.CustName)
                    .Select(group =>
                    {
                        var rows = group.ToList();
                        return new
                        {
                            customer = group.Key,
                            normal = rows.Count(x => GetDashboardRiskLevel(x, shortageKeySet, today) == "normal"),
                            warning = rows.Count(x => GetDashboardRiskLevel(x, shortageKeySet, today) == "warning"),
                            delay = rows.Count(x => GetDashboardRiskLevel(x, shortageKeySet, today) == "delay"),
                            overdue = rows.Count(x => GetDashboardRiskLevel(x, shortageKeySet, today) == "overdue")
                        };
                    })
                    .OrderByDescending(x => x.overdue)
                    .ThenByDescending(x => x.delay)
                    .ThenByDescending(x => x.warning)
                    .Take(5)
                    .ToList();

                var shortageTop = shortageRows
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.MaterialNumber) ? x.MaterialName ?? "未维护物料" : x.MaterialNumber)
                    .Select(group => new
                    {
                        name = group.Key,
                        value = decimal.ToDouble(group.Sum(GetDashboardShortageQty)),
                        materialName = group.Select(x => x.MaterialName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty
                    })
                    .OrderByDescending(x => x.value)
                    .Take(10)
                    .ToList();

                var trend = BuildDashboardTrend(orderRows, range.Start, range.End, today);
                var messageTotal = urgentRows.Count + negotiationRows.Count;
                var messagePending = urgentPending + negotiationPending;
                var messageReplied = repliedUrgentIdSet.Count + repliedNegotiationIdSet.Count;
                var messageOverdue = urgentOverdue + negotiationOverdue;

                var todos = BuildDashboardTodos(
                    orderRows,
                    shortageRows,
                    orderLookup,
                    urgentRows,
                    repliedUrgentIdSet,
                    negotiationRows,
                    repliedNegotiationIdSet,
                    today,
                    now)
                    .Where(x => string.IsNullOrWhiteSpace(keywordText) || DashboardTodoMatches(x, keywordText))
                    .Take(12)
                    .ToList();

                var data = new
                {
                    source = new
                    {
                        updatedAt = now.ToString("yyyy-MM-dd HH:mm"),
                        tables = new[]
                        {
                            "OCP_OrderTracking",
                            "OCP_LackMtrlResult",
                            "OCP_UrgentOrder",
                            "OCP_UrgentOrderReply",
                            "OCP_Negotiation",
                            "OCP_NegotiationReply",
                            "OCP_BOMProgress",
                            "OCP_POUnFinishTrack",
                            "OCP_PrdMOTracking"
                        }
                    },
                    customers,
                    owners,
                    kpis = new[]
                    {
                        new { key = "running", label = "进行中订单", value = runningCount, unit = "单", trend = $"订单跟踪表 {orderRows.Count} 条样本", level = "primary", path = "/OCP_OrderTracking" },
                        new { key = "delay", label = "延期风险", value = delayCount, unit = "单", trend = $"交期早于 {today:yyyy-MM-dd}", level = "danger", path = "/OCP_OrderTracking" },
                        new { key = "shortage", label = "缺料预警", value = shortageCount, unit = "项", trend = "缺料结果表未齐套项", level = "warning", path = "/OCP_LackMtrlResult" },
                        new { key = "reply", label = "待回复协同", value = messagePending, unit = "条", trend = $"催单 {urgentPending} / 协商 {negotiationPending}", level = "primary", path = "/message-center/reminder" },
                        new { key = "overdue", label = "超期未回复", value = messageOverdue, unit = "条", trend = $"催单 {urgentOverdue} / 协商 {negotiationOverdue}", level = "danger", path = "/message-center/negotiate" },
                        new { key = "delivery", label = "本周交付", value = weekDeliveryCount, unit = "单", trend = $"{weekStart:MM-dd} 至 {weekEnd.AddDays(-1):MM-dd}", level = "primary", path = "/OCP_OrderTracking" }
                    },
                    trend,
                    message = new
                    {
                        total = messageTotal,
                        pending = messagePending,
                        replied = messageReplied,
                        overdue = messageOverdue
                    },
                    shortageTop,
                    progressRows,
                    riskRows,
                    todos
                };

                return response.OK("获取首页看板成功", data, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取首页订单运营看板数据失败");
                return response.Error($"获取首页看板失败：{ex.Message}");
            }
        }

        private async Task<object> BuildBomProgressAsync(DateTime start, DateTime end)
        {
            var query = _dbContext.Set<OCP_BOMProgress>()
                .AsNoTracking()
                .Where(x => x.CreateDate >= start && x.CreateDate < end);
            var total = await query.CountAsync();
            var done = await query.CountAsync(x => x.ActualBuildDate.HasValue || (!string.IsNullOrEmpty(x.StatusFlag) && x.StatusFlag.Contains("完成")));
            var doing = await query.CountAsync(x => !x.ActualBuildDate.HasValue && !string.IsNullOrEmpty(x.StatusFlag) && (x.StatusFlag.Contains("中") || x.StatusFlag.Contains("进行")));
            return BuildDashboardProgressRow("BOM", done, doing, total);
        }

        private async Task<object> BuildPurchaseProgressAsync(DateTime start, DateTime end)
        {
            var query = _dbContext.Set<OCP_POUnFinishTrack>()
                .AsNoTracking()
                .Where(x => x.CreateDate >= start && x.CreateDate < end);
            var total = await query.CountAsync();
            var done = await query.CountAsync(x =>
                ((x.POQty ?? 0) > 0 && (x.WMSInboundQty ?? 0) >= (x.POQty ?? 0)) ||
                (!string.IsNullOrEmpty(x.InstockStatus) && (x.InstockStatus.Contains("完成") || x.InstockStatus.Contains("已入库"))));
            var doing = await query.CountAsync(x =>
                !(((x.POQty ?? 0) > 0 && (x.WMSInboundQty ?? 0) >= (x.POQty ?? 0)) ||
                  (!string.IsNullOrEmpty(x.InstockStatus) && (x.InstockStatus.Contains("完成") || x.InstockStatus.Contains("已入库")))) &&
                (x.POCreateDate.HasValue || x.POAuditDate.HasValue || x.LatestDeliveryDate.HasValue || x.LatestArrivalDate.HasValue));
            return BuildDashboardProgressRow("采购", done, doing, total);
        }

        private async Task<object> BuildProductionProgressAsync(DateTime start, DateTime end)
        {
            var query = _dbContext.Set<OCP_PrdMOTracking>()
                .AsNoTracking()
                .Where(x => x.CreateDate >= start && x.CreateDate < end);
            var total = await query.CountAsync();
            var done = await query.CountAsync(x =>
                ((x.ProductionQty ?? 0) > 0 && (x.InboundQty ?? 0) >= (x.ProductionQty ?? 0)) ||
                (!string.IsNullOrEmpty(x.BillStatus) && x.BillStatus.Contains("完成")) ||
                (!string.IsNullOrEmpty(x.ExecuteStatus) && x.ExecuteStatus.Contains("完成")));
            var doing = await query.CountAsync(x =>
                !(((x.ProductionQty ?? 0) > 0 && (x.InboundQty ?? 0) >= (x.ProductionQty ?? 0)) ||
                  (!string.IsNullOrEmpty(x.BillStatus) && x.BillStatus.Contains("完成")) ||
                  (!string.IsNullOrEmpty(x.ExecuteStatus) && x.ExecuteStatus.Contains("完成"))) &&
                (x.MaterialPickDate.HasValue ||
                 x.PreCompleteDate.HasValue ||
                 x.ValvePartCompleteDate.HasValue ||
                 x.PressureCompleteDate.HasValue ||
                 x.AccessoryInstallDate.HasValue ||
                 x.FinalInspectionDate.HasValue ||
                 x.PaintCompleteDate.HasValue ||
                 x.PackingDate.HasValue));
            return BuildDashboardProgressRow("生产", done, doing, total);
        }

        private static (DateTime Start, DateTime End) ResolveDashboardRange(string dateRange, DateTime today)
        {
            return (dateRange ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "today" => (today, today.AddDays(1)),
                "month" => (new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1)),
                "thirtydays" => (today.AddDays(-29), today.AddDays(1)),
                _ => (today.AddDays(-(((int)today.DayOfWeek + 6) % 7)), today.AddDays(-(((int)today.DayOfWeek + 6) % 7)).AddDays(7))
            };
        }

        private static bool DashboardIsAll(string value)
        {
            return string.IsNullOrWhiteSpace(value) || string.Equals(value, "all", StringComparison.OrdinalIgnoreCase);
        }

        private static string[] ResolveDashboardBusinessTypes(string businessType)
        {
            return (businessType ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "sales" => new[] { "SO" },
                "purchase" => new[] { "PO" },
                "production" => new[] { "ZP", "JG", "BJ" },
                _ => Array.Empty<string>()
            };
        }

        private static object BuildDashboardProgressRow(string label, int doneCount, int doingCount, int total)
        {
            var safeTotal = Math.Max(total, 0);
            var done = DashboardPercent(doneCount, safeTotal);
            var doing = DashboardPercent(doingCount, safeTotal);
            var todo = Math.Max(0, 100 - done - doing);
            return new { label, done, doing, todo, total = safeTotal };
        }

        private static int DashboardPercent(int value, int total)
        {
            if (total <= 0)
            {
                return 0;
            }
            return (int)Math.Round(value * 100M / total, 0, MidpointRounding.AwayFromZero);
        }

        private static DateTime? GetDashboardOrderDate(OCP_OrderTracking row)
        {
            return row?.OrderCreateDate ?? row?.OrderAuditDate ?? row?.CreateDate;
        }

        private static bool IsDashboardOrderCompleted(OCP_OrderTracking row)
        {
            if (row == null)
            {
                return false;
            }

            if ((row.OrderQty ?? 0) > 0 && (row.InstockQty ?? 0) >= (row.OrderQty ?? 0))
            {
                return true;
            }

            if (row.UnInstockQty.HasValue && row.UnInstockQty.Value <= 0)
            {
                return true;
            }

            return (!string.IsNullOrEmpty(row.FinishStatus) && row.FinishStatus.Contains("完成")) ||
                   (!string.IsNullOrEmpty(row.MtoNoStatus) && row.MtoNoStatus.Contains("完成"));
        }

        private static string GetDashboardRiskLevel(OCP_OrderTracking row, HashSet<string> shortageKeys, DateTime today)
        {
            if (row == null || IsDashboardOrderCompleted(row))
            {
                return "normal";
            }

            var targetDate = row.ReplyDeliveryDate ?? row.DeliveryDate;
            var orderKey = BuildDashboardOrderKey(row.SOBillNo, row.MtoNo);
            var hasShortage = !string.IsNullOrWhiteSpace(orderKey) && shortageKeys.Contains(orderKey);

            if (targetDate.HasValue && targetDate.Value.Date < today.AddDays(-3))
            {
                return "overdue";
            }

            if (targetDate.HasValue && targetDate.Value.Date < today)
            {
                return "delay";
            }

            if (hasShortage || (targetDate.HasValue && targetDate.Value.Date <= today.AddDays(3)))
            {
                return "warning";
            }

            return "normal";
        }

        private static string BuildDashboardOrderKey(string billNo, string mtoNo)
        {
            var bill = (billNo ?? string.Empty).Trim();
            var mto = (mtoNo ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(bill) && string.IsNullOrWhiteSpace(mto))
            {
                return string.Empty;
            }
            return $"{bill}__{mto}";
        }

        private static decimal GetDashboardShortageQty(OCP_LackMtrlResult row)
        {
            if (row == null)
            {
                return 0;
            }

            var unplanned = row.UnPlanedQty ?? 0;
            if (unplanned > 0)
            {
                return unplanned;
            }

            return Math.Max((row.NeedQty ?? 0) - (row.InventoryQty ?? 0) - (row.PlanedQty ?? 0), 0);
        }

        private static object BuildDashboardTrend(List<OCP_OrderTracking> orderRows, DateTime start, DateTime end, DateTime today)
        {
            var days = new List<DateTime>();
            var totalDays = Math.Max(1, (end.Date - start.Date).Days);
            if (totalDays <= 7)
            {
                for (var day = start.Date; day < end.Date; day = day.AddDays(1))
                {
                    days.Add(day);
                }
            }
            else
            {
                for (var i = 6; i >= 0; i--)
                {
                    days.Add(today.AddDays(-i));
                }
            }

            var labels = days.Select(x => x.ToString("MM-dd")).ToList();
            var newOrders = days
                .Select(day => orderRows.Count(x => GetDashboardOrderDate(x)?.Date == day.Date))
                .ToList();
            var completedOrders = days
                .Select(day => orderRows.Count(x => x.LastInStockDate.HasValue && x.LastInStockDate.Value.Date == day.Date))
                .ToList();
            var runningOrders = days
                .Select(day => orderRows.Count(x =>
                    GetDashboardOrderDate(x).HasValue &&
                    GetDashboardOrderDate(x).Value.Date <= day.Date &&
                    !IsDashboardOrderCompleted(x)))
                .ToList();

            return new { labels, newOrders, completedOrders, runningOrders };
        }

        private static DateTime CalculateDashboardDeadline(DateTime createDate, decimal assignedReplyTime, int timeUnit)
        {
            return timeUnit switch
            {
                1 => createDate.AddHours((double)assignedReplyTime),
                2 => createDate.AddDays((double)assignedReplyTime),
                3 => createDate.AddDays((double)assignedReplyTime * 7),
                _ => createDate
            };
        }

        private sealed class DashboardTodoItem
        {
            public string type { get; set; }
            public string code { get; set; }
            public string customer { get; set; }
            public string owner { get; set; }
            public string deadline { get; set; }
            public string status { get; set; }
            public string level { get; set; }
            public string risk { get; set; }
            public string path { get; set; }
        }

        private static List<DashboardTodoItem> BuildDashboardTodos(
            List<OCP_OrderTracking> orderRows,
            List<OCP_LackMtrlResult> shortageRows,
            Dictionary<string, OCP_OrderTracking> orderLookup,
            List<OCP_UrgentOrder> urgentRows,
            HashSet<long> repliedUrgentIdSet,
            List<OCP_Negotiation> negotiationRows,
            HashSet<long> repliedNegotiationIdSet,
            DateTime today,
            DateTime now)
        {
            var todos = new List<DashboardTodoItem>();

            todos.AddRange(shortageRows
                .OrderByDescending(GetDashboardShortageQty)
                .Take(4)
                .Select(row =>
                {
                    orderLookup.TryGetValue(row.SOBillNo ?? string.Empty, out var order);
                    return new DashboardTodoItem
                    {
                        type = "缺料",
                        code = row.SOBillNo ?? row.BillNo ?? "-",
                        customer = order?.CustName ?? "-",
                        owner = row.PurchaserName ?? order?.SalesPerson ?? "-",
                        deadline = FormatDashboardDate(row.PlanEndDate ?? row.F_BLN_HFJHRQ ?? row.F_ORA_DATETIME),
                        status = "缺料",
                        level = "warning",
                        risk = "shortage",
                        path = "/OCP_LackMtrlResult"
                    };
                }));

            todos.AddRange(orderRows
                .Where(row =>
                {
                    var level = GetDashboardRiskLevel(row, new HashSet<string>(), today);
                    return level == "delay" || level == "overdue";
                })
                .OrderBy(row => row.ReplyDeliveryDate ?? row.DeliveryDate ?? DateTime.MaxValue)
                .Take(4)
                .Select(row => new DashboardTodoItem
                {
                    type = "延期",
                    code = row.SOBillNo ?? "-",
                    customer = row.CustName ?? "-",
                    owner = row.SalesPerson ?? "-",
                    deadline = FormatDashboardDate(row.ReplyDeliveryDate ?? row.DeliveryDate),
                    status = "延期",
                    level = "danger",
                    risk = "delay",
                    path = "/OCP_OrderTracking"
                }));

            todos.AddRange(urgentRows
                .Where(row => !repliedUrgentIdSet.Contains(row.UrgentOrderID))
                .OrderBy(row => row.CreateDate ?? DateTime.MaxValue)
                .Take(4)
                .Select(row =>
                {
                    var deadline = row.CreateDate.HasValue && row.AssignedReplyTime.HasValue && row.TimeUnit.HasValue
                        ? CalculateDashboardDeadline(row.CreateDate.Value, row.AssignedReplyTime.Value, row.TimeUnit.Value)
                        : row.CreateDate;
                    var isOverdue = deadline.HasValue && deadline.Value < now;
                    return new DashboardTodoItem
                    {
                        type = "催单",
                        code = row.BillNo ?? "-",
                        customer = row.SupplierName ?? "-",
                        owner = row.AssignedResPersonName ?? row.AssignedResPerson ?? row.DefaultResPersonName ?? row.DefaultResPerson ?? "-",
                        deadline = FormatDashboardDate(deadline),
                        status = isOverdue ? "超期" : "待回复",
                        level = isOverdue ? "danger" : "primary",
                        risk = isOverdue ? "overdue" : "reply",
                        path = "/message-center/reminder"
                    };
                }));

            todos.AddRange(negotiationRows
                .Where(row => !repliedNegotiationIdSet.Contains(row.NegotiationID))
                .OrderBy(row => row.CreateDate ?? DateTime.MaxValue)
                .Take(4)
                .Select(row =>
                {
                    var isOverdue = row.CreateDate.HasValue && row.CreateDate.Value < now.AddDays(-7);
                    return new DashboardTodoItem
                    {
                        type = "协商",
                        code = row.BillNo ?? "-",
                        customer = row.SupplierName ?? "-",
                        owner = row.AssignedResPersonName ?? row.AssignedResPerson ?? row.DefaultResPersonName ?? row.DefaultResPerson ?? "-",
                        deadline = FormatDashboardDate(row.ReplyDeliveryDate ?? row.DeliveryDate ?? row.CreateDate),
                        status = isOverdue ? "超期" : "待回复",
                        level = isOverdue ? "danger" : "primary",
                        risk = isOverdue ? "overdue" : "reply",
                        path = "/message-center/negotiate"
                    };
                }));

            return todos
                .OrderByDescending(x => x.risk == "overdue")
                .ThenByDescending(x => x.risk == "delay")
                .ThenByDescending(x => x.risk == "shortage")
                .ThenBy(x => x.deadline)
                .ToList();
        }

        private static string FormatDashboardDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm") : "-";
        }

        private static bool DashboardTodoMatches(DashboardTodoItem row, string keyword)
        {
            var text = keyword ?? string.Empty;
            return (row.code ?? string.Empty).Contains(text) ||
                   (row.customer ?? string.Empty).Contains(text) ||
                   (row.owner ?? string.Empty).Contains(text) ||
                   (row.status ?? string.Empty).Contains(text) ||
                   (row.type ?? string.Empty).Contains(text);
        }

    }
}
