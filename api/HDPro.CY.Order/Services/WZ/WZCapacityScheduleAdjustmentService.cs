using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.IServices.WZ;
using HDPro.Entity.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HDPro.CY.Order.Services.WZ
{
    /// <summary>
    /// 异常排产调整工作台独立服务。
    /// 规则口径：
    /// 1. 工作台只拉取排产优化日期已标红超 120%，或落在 2026 法定节假日而标黄的异常订单。
    /// 2. 选择新日期时，窗口基准产能 = WZ_ProductionOutput 实际产能 + 非异常优化订单；红色超载单和黄色节假日单不参与基准统计。
    /// 3. 当前订单数量单独叠加为“插入后”产能，插入后不得超过 120% 阈值；保存后回写 WZ_OrderCycleBase.CapacityScheduleDate。
    /// 4. 保存完成后重新计算超阈值标记，并同步 WZ_PreProductionOutput / WZ_PreProductionOutput_2，保证 WZ_ProductionOutput 展示排产优化口径能看到手动调整结果。
    /// </summary>
    public class WZCapacityScheduleAdjustmentService : IWZCapacityScheduleAdjustmentService, IDependency
    {
        private const decimal DailyReserveCapacityRatio = 1.2M;
        private const int WindowDays = 15;

        private static readonly HashSet<DateTime> CapacityStatutoryHolidayDates2026 = new()
        {
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            new DateTime(2026, 1, 3),
            new DateTime(2026, 2, 15),
            new DateTime(2026, 2, 16),
            new DateTime(2026, 2, 17),
            new DateTime(2026, 2, 18),
            new DateTime(2026, 2, 19),
            new DateTime(2026, 2, 20),
            new DateTime(2026, 2, 21),
            new DateTime(2026, 2, 22),
            new DateTime(2026, 2, 23),
            new DateTime(2026, 4, 4),
            new DateTime(2026, 4, 5),
            new DateTime(2026, 4, 6),
            new DateTime(2026, 5, 1),
            new DateTime(2026, 5, 2),
            new DateTime(2026, 5, 3),
            new DateTime(2026, 5, 4),
            new DateTime(2026, 5, 5),
            new DateTime(2026, 6, 19),
            new DateTime(2026, 6, 20),
            new DateTime(2026, 6, 21),
            new DateTime(2026, 9, 25),
            new DateTime(2026, 9, 26),
            new DateTime(2026, 9, 27),
            new DateTime(2026, 10, 1),
            new DateTime(2026, 10, 2),
            new DateTime(2026, 10, 3),
            new DateTime(2026, 10, 4),
            new DateTime(2026, 10, 5),
            new DateTime(2026, 10, 6),
            new DateTime(2026, 10, 7)
        };

        private static readonly HashSet<DateTime> CapacityMakeupWorkdayDates2026 = new()
        {
            new DateTime(2026, 1, 4),
            new DateTime(2026, 2, 14),
            new DateTime(2026, 2, 28),
            new DateTime(2026, 5, 9),
            new DateTime(2026, 9, 20),
            new DateTime(2026, 10, 10)
        };

        private readonly ServiceDbContext _db;
        private readonly IWZ_OrderCycleBaseService _orderCycleBaseService;
        private readonly IWZProductionOutputService _productionOutputService;
        private readonly ILogger<WZCapacityScheduleAdjustmentService> _logger;

        public WZCapacityScheduleAdjustmentService(
            ServiceDbContext dbContext,
            IWZ_OrderCycleBaseService orderCycleBaseService,
            IWZProductionOutputService productionOutputService,
            ILogger<WZCapacityScheduleAdjustmentService> logger)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _orderCycleBaseService = orderCycleBaseService ?? throw new ArgumentNullException(nameof(orderCycleBaseService));
            _productionOutputService = productionOutputService ?? throw new ArgumentNullException(nameof(productionOutputService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CapacityScheduleAdjustmentListResultDto> QueryAbnormalOrdersAsync(
            CapacityScheduleAdjustmentQueryDto query,
            CancellationToken cancellationToken = default)
        {
            query ??= new CapacityScheduleAdjustmentQueryDto();
            var page = Math.Max(query.Page, 1);
            var rows = Math.Clamp(query.Rows <= 0 ? 30 : query.Rows, 1, 200);
            var keyword = NormalizeText(query.Keyword);
            var abnormalType = NormalizeText(query.AbnormalType);
            var valveCategory = NormalizeText(query.ValveCategory);
            var productionLine = NormalizeText(query.ProductionLine);

            var minHoliday = CapacityStatutoryHolidayDates2026.Min();
            var maxHoliday = CapacityStatutoryHolidayDates2026.Max();

            var dbQuery = _db.Set<WZ_OrderCycleBase>()
                .AsNoTracking()
                .Where(p => p.CapacityScheduleDate.HasValue
                    && (p.CapacityScheduleDateOverThreshold
                        || (p.CapacityScheduleDate.Value >= minHoliday && p.CapacityScheduleDate.Value <= maxHoliday)));

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                dbQuery = dbQuery.Where(p =>
                    (p.SalesOrderNo != null && p.SalesOrderNo.Contains(keyword))
                    || (p.PlanTrackingNo != null && p.PlanTrackingNo.Contains(keyword))
                    || (p.MaterialCode != null && p.MaterialCode.Contains(keyword))
                    || (p.GUI_GE_XING_HAO != null && p.GUI_GE_XING_HAO.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(valveCategory))
            {
                dbQuery = dbQuery.Where(p => p.ValveCategory != null && p.ValveCategory.Contains(valveCategory));
            }

            var candidates = await dbQuery
                .Select(p => new OrderRow
                {
                    Id = p.Id,
                    SalesOrderNo = p.SalesOrderNo,
                    PlanTrackingNo = p.PlanTrackingNo,
                    MaterialCode = p.MaterialCode,
                    SpecModel = p.GUI_GE_XING_HAO,
                    ValveCategory = p.ValveCategory,
                    AssignedProductionLine = p.AssignedProductionLine,
                    ProductionLine = p.ProductionLine,
                    NominalDiameter = p.NominalDiameter,
                    OrderQty = p.OrderQty,
                    ScheduleDate = p.ScheduleDate,
                    CapacityScheduleDate = p.CapacityScheduleDate,
                    CapacityScheduleDateOverThreshold = p.CapacityScheduleDateOverThreshold
                })
                .ToListAsync(cancellationToken);

            var abnormalRows = candidates
                .Select(BuildOrderDto)
                .Where(p => p.IsOverThreshold || p.IsStatutoryHoliday)
                .Where(p => string.IsNullOrWhiteSpace(abnormalType)
                    || string.Equals(abnormalType, "all", StringComparison.OrdinalIgnoreCase)
                    || (string.Equals(abnormalType, "overThreshold", StringComparison.OrdinalIgnoreCase) && p.IsOverThreshold)
                    || (string.Equals(abnormalType, "holiday", StringComparison.OrdinalIgnoreCase) && p.IsStatutoryHoliday))
                .Where(p => string.IsNullOrWhiteSpace(productionLine)
                    || NormalizeText(p.ProductionLine).Contains(productionLine, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.AbnormalLevel)
                .ThenBy(p => p.CapacityScheduleDate)
                .ThenBy(p => p.SalesOrderNo)
                .ThenBy(p => p.PlanTrackingNo)
                .ToList();

            return new CapacityScheduleAdjustmentListResultDto
            {
                Total = abnormalRows.Count,
                Page = page,
                Rows = rows,
                OverThresholdCount = abnormalRows.Count(p => p.IsOverThreshold),
                HolidayCount = abnormalRows.Count(p => p.IsStatutoryHoliday),
                Items = abnormalRows.Skip((page - 1) * rows).Take(rows).ToList(),
                ValveCategories = abnormalRows
                    .Select(p => p.ValveCategory)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(p => p)
                    .ToList(),
                ProductionLines = abnormalRows
                    .Select(p => p.ProductionLine)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(p => p)
                    .ToList()
            };
        }

        public async Task<CapacityScheduleAdjustmentWindowDto> GetAdjustmentWindowAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var order = await LoadOrderRowAsync(id, cancellationToken);
            var orderDto = BuildOrderDto(order);
            var anchorDate = (order.CapacityScheduleDate ?? order.ScheduleDate ?? DateTime.Today).Date;
            var startDate = anchorDate.AddDays(-WindowDays);
            var endDate = anchorDate.AddDays(WindowDays);
            var cat = NormalizeText(order.ValveCategory);
            var line = ResolveProductionLine(order);
            var orderQty = order.OrderQty.GetValueOrDefault();

            var actualMap = new Dictionary<DateTime, decimal>();
            var thresholdFromActualMap = new Dictionary<DateTime, decimal?>();
            if (!string.IsNullOrWhiteSpace(cat) && !string.IsNullOrWhiteSpace(line))
            {
                var actualRows = await _productionOutputService.GetAsync(cat, line, startDate, endDate, cancellationToken);
                foreach (var row in actualRows)
                {
                    var date = row.ProductionDate.Date;
                    actualMap[date] = actualMap.TryGetValue(date, out var quantity)
                        ? quantity + row.Quantity
                        : row.Quantity;
                    thresholdFromActualMap[date] = MergeThreshold(
                        thresholdFromActualMap.TryGetValue(date, out var current) ? current : null,
                        row.CurrentThreshold);
                }
            }

            var normalOptimizedMap = await LoadNormalOptimizedQuantityMapAsync(
                order.Id,
                cat,
                line,
                startDate,
                endDate,
                cancellationToken);

            var threshold = await ResolveThresholdAsync(cat, line, cancellationToken);
            if (!threshold.HasValue)
            {
                threshold = thresholdFromActualMap.Values
                    .Where(p => p.HasValue && p.Value > 0)
                    .Select(p => p.Value)
                    .DefaultIfEmpty()
                    .Max();
                if (threshold <= 0)
                {
                    threshold = null;
                }
            }

            var days = new List<CapacityScheduleAdjustmentDayDto>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var actualQuantity = actualMap.TryGetValue(date, out var actual) ? actual : 0M;
                var optimizedQuantity = normalOptimizedMap.TryGetValue(date, out var optimized) ? optimized : 0M;
                var quantity = actualQuantity + optimizedQuantity;
                var projectedQuantity = quantity + orderQty;
                var loadRate = CalculateRate(quantity, threshold);
                var projectedRate = CalculateRate(projectedQuantity, threshold);
                var status = ResolveCapacityStatus(projectedRate);

                days.Add(new CapacityScheduleAdjustmentDayDto
                {
                    Date = date,
                    WeekName = BuildWeekName(date),
                    DayType = BuildDayType(date),
                    IsWorkday = IsCapacityWorkday(date),
                    IsSaturdayRestDay = IsCapacitySaturdayRestDay(date),
                    IsSundayRestDay = IsCapacitySundayRestDay(date),
                    IsStatutoryHoliday = IsCapacityStatutoryHoliday(date),
                    IsMakeupWorkday = IsCapacityMakeupWorkday(date),
                    IsCurrentDate = order.CapacityScheduleDate.HasValue
                        && order.CapacityScheduleDate.Value.Date == date,
                    ActualQuantity = actualQuantity,
                    NormalOptimizedQuantity = optimizedQuantity,
                    Quantity = quantity,
                    InsertQuantity = orderQty,
                    ProjectedQuantity = projectedQuantity,
                    Threshold = threshold,
                    LoadRate = loadRate,
                    ProjectedLoadRate = projectedRate,
                    Status = status.Status,
                    StatusText = status.Text,
                    CanSelect = threshold.HasValue
                        && threshold.Value > 0
                        && projectedQuantity <= threshold.Value * DailyReserveCapacityRatio
                });
            }

            return new CapacityScheduleAdjustmentWindowDto
            {
                Order = orderDto,
                StartDate = startDate,
                EndDate = endDate,
                Threshold = threshold,
                Days = days
            };
        }

        public async Task<CapacityScheduleAdjustmentSaveResultDto> SaveAdjustmentAsync(
            CapacityScheduleAdjustmentSaveDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto == null || dto.Id <= 0)
            {
                throw new ArgumentException("请选择需要调整的异常订单");
            }

            var newDate = dto.CapacityScheduleDate.Date;
            var window = await GetAdjustmentWindowAsync(dto.Id, cancellationToken);
            var day = window.Days.FirstOrDefault(p => p.Date.Date == newDate);
            if (day == null)
            {
                throw new InvalidOperationException("调整日期必须在当前订单附近 15 天窗口内");
            }

            if (!day.CanSelect)
            {
                throw new InvalidOperationException("该日期插入后会超过 120% 产能阈值，不能保存");
            }

            var entity = await _db.Set<WZ_OrderCycleBase>()
                .AsTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);
            if (entity == null)
            {
                throw new InvalidOperationException("未找到需要调整的订单");
            }

            var oldDate = entity.CapacityScheduleDate?.Date;
            entity.CapacityScheduleDate = newDate;
            await _db.SaveChangesAsync(cancellationToken);

            await EnsureAdjustmentLogTableAsync(cancellationToken);
            await InsertAdjustmentLogAsync(entity, oldDate, newDate, cancellationToken);

            var overThresholdCount = await RefreshOverThresholdFlagsAsync(cancellationToken);
            var syncedRows = await _orderCycleBaseService.SyncPreProductionOutputAsync(cancellationToken);

            var cat = NormalizeText(entity.ValveCategory);
            var line = ResolveProductionLine(new OrderRow
            {
                AssignedProductionLine = entity.AssignedProductionLine,
                ProductionLine = entity.ProductionLine,
                ValveCategory = entity.ValveCategory,
                NominalDiameter = entity.NominalDiameter
            });

            try
            {
                await _productionOutputService.GetWithOptimizedPreProductionAsync(
                    cat,
                    line,
                    newDate.AddDays(-WindowDays),
                    newDate.AddDays(WindowDays),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "异常排产保存后刷新排产优化汇总失败，订单 {OrderCycleBaseId}", dto.Id);
                throw;
            }

            return new CapacityScheduleAdjustmentSaveResultDto
            {
                Success = true,
                Message = "排产优化日期已调整，并已同步排产优化汇总",
                OldCapacityScheduleDate = oldDate,
                NewCapacityScheduleDate = newDate,
                OverThresholdCount = overThresholdCount,
                SyncedPreProductionRows = syncedRows,
                Window = await GetAdjustmentWindowAsync(dto.Id, cancellationToken)
            };
        }

        private async Task<OrderRow> LoadOrderRowAsync(int id, CancellationToken cancellationToken)
        {
            var row = await _db.Set<WZ_OrderCycleBase>()
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new OrderRow
                {
                    Id = p.Id,
                    SalesOrderNo = p.SalesOrderNo,
                    PlanTrackingNo = p.PlanTrackingNo,
                    MaterialCode = p.MaterialCode,
                    SpecModel = p.GUI_GE_XING_HAO,
                    ValveCategory = p.ValveCategory,
                    AssignedProductionLine = p.AssignedProductionLine,
                    ProductionLine = p.ProductionLine,
                    NominalDiameter = p.NominalDiameter,
                    OrderQty = p.OrderQty,
                    ScheduleDate = p.ScheduleDate,
                    CapacityScheduleDate = p.CapacityScheduleDate,
                    CapacityScheduleDateOverThreshold = p.CapacityScheduleDateOverThreshold
                })
                .FirstOrDefaultAsync(cancellationToken);

            return row ?? throw new InvalidOperationException("未找到需要调整的订单");
        }

        private async Task<Dictionary<DateTime, decimal>> LoadNormalOptimizedQuantityMapAsync(
            int currentOrderId,
            string cat,
            string line,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(cat) || string.IsNullOrWhiteSpace(line))
            {
                return new Dictionary<DateTime, decimal>();
            }

            var rows = await _db.Set<WZ_OrderCycleBase>()
                .AsNoTracking()
                .Where(p => p.Id != currentOrderId
                    && p.CapacityScheduleDate.HasValue
                    && p.CapacityScheduleDate.Value >= startDate
                    && p.CapacityScheduleDate.Value <= endDate
                    && p.OrderQty.HasValue
                    && p.OrderQty.Value > 0
                    && !p.CapacityScheduleDateOverThreshold)
                .Select(p => new OrderRow
                {
                    Id = p.Id,
                    ValveCategory = p.ValveCategory,
                    AssignedProductionLine = p.AssignedProductionLine,
                    ProductionLine = p.ProductionLine,
                    NominalDiameter = p.NominalDiameter,
                    OrderQty = p.OrderQty,
                    CapacityScheduleDate = p.CapacityScheduleDate,
                    CapacityScheduleDateOverThreshold = p.CapacityScheduleDateOverThreshold
                })
                .ToListAsync(cancellationToken);

            var result = new Dictionary<DateTime, decimal>();
            foreach (var row in rows)
            {
                if (!row.CapacityScheduleDate.HasValue
                    || IsCapacityStatutoryHoliday(row.CapacityScheduleDate.Value)
                    || !string.Equals(NormalizeText(row.ValveCategory), cat, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(ResolveProductionLine(row), line, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var date = row.CapacityScheduleDate.Value.Date;
                result[date] = result.TryGetValue(date, out var quantity)
                    ? quantity + row.OrderQty.GetValueOrDefault()
                    : row.OrderQty.GetValueOrDefault();
            }

            return result;
        }

        private async Task<decimal?> ResolveThresholdAsync(
            string cat,
            string line,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(cat) || string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var rows = await _db.Set<WZ_ProductionOutputThreshold>()
                .AsNoTracking()
                .Select(p => new
                {
                    p.ValveCategory,
                    p.ProductionLine,
                    p.CurrentThreshold
                })
                .ToListAsync(cancellationToken);

            return rows
                .Where(p => string.Equals(NormalizeText(p.ValveCategory), cat, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(NormalizeText(p.ProductionLine), line, StringComparison.OrdinalIgnoreCase)
                    && p.CurrentThreshold > 0)
                .Select(p => (decimal?)p.CurrentThreshold)
                .DefaultIfEmpty()
                .Max();
        }

        private async Task<int> RefreshOverThresholdFlagsAsync(CancellationToken cancellationToken)
        {
            var orders = await _db.Set<WZ_OrderCycleBase>()
                .AsNoTracking()
                .Where(p => p.CapacityScheduleDate.HasValue && p.OrderQty.HasValue && p.OrderQty.Value > 0)
                .Select(p => new OrderRow
                {
                    Id = p.Id,
                    CapacityScheduleDate = p.CapacityScheduleDate,
                    OrderQty = p.OrderQty,
                    ValveCategory = p.ValveCategory,
                    AssignedProductionLine = p.AssignedProductionLine,
                    ProductionLine = p.ProductionLine,
                    NominalDiameter = p.NominalDiameter
                })
                .ToListAsync(cancellationToken);

            var thresholdMap = await LoadThresholdMapAsync(cancellationToken);
            var outputs = await _db.Set<WZ_ProductionOutput>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var outputThresholdMap = new Dictionary<(string Cat, string Line), decimal>();
            var dateQuantityMap = new Dictionary<(string Cat, string Line, DateTime Date), decimal>();

            foreach (var output in outputs)
            {
                var cat = NormalizeText(output.ValveCategory);
                var line = NormalizeText(output.ProductionLine);
                if (string.IsNullOrWhiteSpace(cat) || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var lineKey = (cat, line);
                if (output.CurrentThreshold.HasValue && output.CurrentThreshold.Value > 0)
                {
                    outputThresholdMap[lineKey] = outputThresholdMap.TryGetValue(lineKey, out var existingThreshold)
                        ? MergeThreshold(existingThreshold, output.CurrentThreshold).GetValueOrDefault(existingThreshold)
                        : output.CurrentThreshold.Value;
                }

                var dateKey = (cat, line, output.ProductionDate.Date);
                dateQuantityMap[dateKey] = dateQuantityMap.TryGetValue(dateKey, out var quantity)
                    ? quantity + output.Quantity
                    : output.Quantity;
            }

            var orderGroups = new Dictionary<(string Cat, string Line, DateTime Date), List<OrderRow>>();
            foreach (var order in orders)
            {
                var cat = NormalizeText(order.ValveCategory);
                var line = ResolveProductionLine(order);
                if (!order.CapacityScheduleDate.HasValue
                    || string.IsNullOrWhiteSpace(cat)
                    || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var key = (cat, line, order.CapacityScheduleDate.Value.Date);
                if (!orderGroups.TryGetValue(key, out var group))
                {
                    group = new List<OrderRow>();
                    orderGroups[key] = group;
                }

                group.Add(order);
            }

            var overThresholdIds = new List<int>();
            foreach (var group in orderGroups)
            {
                var key = group.Key;
                var threshold = ResolveThreshold(thresholdMap, outputThresholdMap, key.Cat, key.Line, null);
                if (!threshold.HasValue || threshold.Value <= 0)
                {
                    continue;
                }

                var baseQuantity = dateQuantityMap.TryGetValue(key, out var quantity) ? quantity : 0M;
                var orderQuantity = group.Value.Sum(order => order.OrderQty.GetValueOrDefault());
                if (baseQuantity + orderQuantity > threshold.Value * DailyReserveCapacityRatio)
                {
                    overThresholdIds.AddRange(group.Value.Select(order => order.Id));
                }
            }

            await _db.Set<WZ_OrderCycleBase>()
                .Where(p => p.CapacityScheduleDateOverThreshold)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.CapacityScheduleDateOverThreshold, false), cancellationToken);

            for (var index = 0; index < overThresholdIds.Count; index += 1000)
            {
                var batchIds = overThresholdIds.Skip(index).Take(1000).ToList();
                await _db.Set<WZ_OrderCycleBase>()
                    .Where(p => batchIds.Contains(p.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.CapacityScheduleDateOverThreshold, true), cancellationToken);
            }

            return overThresholdIds.Count;
        }

        private async Task<Dictionary<(string Cat, string Line), decimal>> LoadThresholdMapAsync(
            CancellationToken cancellationToken)
        {
            var rows = await _db.Set<WZ_ProductionOutputThreshold>()
                .AsNoTracking()
                .Select(p => new
                {
                    p.ValveCategory,
                    p.ProductionLine,
                    p.CurrentThreshold
                })
                .ToListAsync(cancellationToken);

            var result = new Dictionary<(string Cat, string Line), decimal>();
            foreach (var row in rows)
            {
                var cat = NormalizeText(row.ValveCategory);
                var line = NormalizeText(row.ProductionLine);
                if (!string.IsNullOrWhiteSpace(cat)
                    && !string.IsNullOrWhiteSpace(line)
                    && row.CurrentThreshold > 0)
                {
                    result[(cat, line)] = row.CurrentThreshold;
                }
            }

            return result;
        }

        private async Task EnsureAdjustmentLogTableAsync(CancellationToken cancellationToken)
        {
            await _db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[dbo].[WZ_CapacityScheduleAdjustmentLog]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WZ_CapacityScheduleAdjustmentLog](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WZ_CapacityScheduleAdjustmentLog] PRIMARY KEY,
        [OrderCycleBaseId] INT NOT NULL,
        [OldCapacityScheduleDate] DATE NULL,
        [NewCapacityScheduleDate] DATE NOT NULL,
        [SalesOrderNo] NVARCHAR(100) NULL,
        [PlanTrackingNo] NVARCHAR(100) NULL,
        [MaterialCode] NVARCHAR(100) NULL,
        [SpecModel] NVARCHAR(2000) NULL,
        [ValveCategory] NVARCHAR(100) NULL,
        [ProductionLine] NVARCHAR(100) NULL,
        [OrderQty] DECIMAL(18,6) NULL,
        [CreateDate] DATETIME NOT NULL CONSTRAINT [DF_WZ_CapacityScheduleAdjustmentLog_CreateDate] DEFAULT(GETDATE())
    );
END;
", cancellationToken);
        }

        private async Task InsertAdjustmentLogAsync(
            WZ_OrderCycleBase entity,
            DateTime? oldDate,
            DateTime newDate,
            CancellationToken cancellationToken)
        {
            var productionLine = ResolveProductionLine(new OrderRow
            {
                AssignedProductionLine = entity.AssignedProductionLine,
                ProductionLine = entity.ProductionLine,
                ValveCategory = entity.ValveCategory,
                NominalDiameter = entity.NominalDiameter
            });

            await _db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO [dbo].[WZ_CapacityScheduleAdjustmentLog]
(
    [OrderCycleBaseId],
    [OldCapacityScheduleDate],
    [NewCapacityScheduleDate],
    [SalesOrderNo],
    [PlanTrackingNo],
    [MaterialCode],
    [SpecModel],
    [ValveCategory],
    [ProductionLine],
    [OrderQty]
)
VALUES
(
    {entity.Id},
    {oldDate},
    {newDate},
    {entity.SalesOrderNo},
    {entity.PlanTrackingNo},
    {entity.MaterialCode},
    {entity.GUI_GE_XING_HAO},
    {entity.ValveCategory},
    {productionLine},
    {entity.OrderQty}
);", cancellationToken);
        }

        private static CapacityScheduleAdjustmentOrderDto BuildOrderDto(OrderRow row)
        {
            var isHoliday = IsCapacityStatutoryHoliday(row.CapacityScheduleDate);
            var isOverThreshold = row.CapacityScheduleDateOverThreshold;
            var abnormalType = isOverThreshold && isHoliday
                ? "overThresholdHoliday"
                : isOverThreshold
                    ? "overThreshold"
                    : isHoliday
                        ? "holiday"
                        : string.Empty;

            var abnormalText = isOverThreshold && isHoliday
                ? "超载/节假日"
                : isOverThreshold
                    ? "超 120%"
                    : isHoliday
                        ? "法定节假日"
                        : string.Empty;

            return new CapacityScheduleAdjustmentOrderDto
            {
                Id = row.Id,
                SalesOrderNo = NormalizeText(row.SalesOrderNo),
                PlanTrackingNo = NormalizeText(row.PlanTrackingNo),
                MaterialCode = NormalizeText(row.MaterialCode),
                SpecModel = NormalizeText(row.SpecModel),
                ValveCategory = NormalizeText(row.ValveCategory),
                ProductionLine = ResolveProductionLine(row),
                OrderQty = row.OrderQty.GetValueOrDefault(),
                ScheduleDate = row.ScheduleDate?.Date,
                CapacityScheduleDate = row.CapacityScheduleDate?.Date,
                IsOverThreshold = isOverThreshold,
                IsStatutoryHoliday = isHoliday,
                AbnormalType = abnormalType,
                AbnormalText = abnormalText,
                AbnormalLevel = isOverThreshold ? 2 : isHoliday ? 1 : 0
            };
        }

        private static string ResolveProductionLine(OrderRow row)
        {
            var assignedLine = NormalizeText(row.AssignedProductionLine);
            if (!string.IsNullOrWhiteSpace(assignedLine))
            {
                return assignedLine;
            }

            var ruleLine = NormalizeText(HDPro.CY.Order.Services.WZ_OrderCycleBaseService.CalcAssignedProductionLine(
                row.ProductionLine,
                row.ValveCategory,
                row.NominalDiameter));
            return !string.IsNullOrWhiteSpace(ruleLine)
                ? ruleLine
                : NormalizeText(row.ProductionLine);
        }

        private static decimal? ResolveThreshold(
            IReadOnlyDictionary<(string Cat, string Line), decimal> thresholds,
            IReadOnlyDictionary<(string Cat, string Line), decimal> outputThresholds,
            string valveCategory,
            string productionLine,
            decimal? fallback)
        {
            var key = (NormalizeText(valveCategory), NormalizeText(productionLine));
            if (thresholds != null && thresholds.TryGetValue(key, out var threshold))
            {
                return threshold;
            }

            if (outputThresholds != null && outputThresholds.TryGetValue(key, out var outputThreshold))
            {
                return outputThreshold;
            }

            return fallback;
        }

        private static decimal? MergeThreshold(decimal? current, decimal? incoming)
        {
            if (!incoming.HasValue)
            {
                return current;
            }

            if (!current.HasValue)
            {
                return incoming;
            }

            return Math.Max(current.Value, incoming.Value);
        }

        private static decimal? CalculateRate(decimal quantity, decimal? threshold)
        {
            if (!threshold.HasValue || threshold.Value <= 0)
            {
                return null;
            }

            return Math.Round(quantity * 100M / threshold.Value, 2, MidpointRounding.AwayFromZero);
        }

        private static (string Status, string Text) ResolveCapacityStatus(decimal? projectedRate)
        {
            if (!projectedRate.HasValue)
            {
                return ("missing", "无阈值");
            }

            if (projectedRate.Value <= 100M)
            {
                return ("normal", "100% 内");
            }

            if (projectedRate.Value <= 120M)
            {
                return ("reserve", "120% 内");
            }

            return ("over", "超 120%");
        }

        private static string BuildWeekName(DateTime date)
        {
            return CultureInfo.GetCultureInfo("zh-CN").DateTimeFormat.GetDayName(date.DayOfWeek);
        }

        private static string BuildDayType(DateTime date)
        {
            if (IsCapacityStatutoryHoliday(date))
            {
                return "法定节假日";
            }

            if (IsCapacityMakeupWorkday(date))
            {
                return "调休上班";
            }

            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                return "周六";
            }

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                return "周日";
            }

            return "工作日";
        }

        private static bool IsCapacityWorkday(DateTime date)
        {
            if (IsCapacityMakeupWorkday(date))
            {
                return true;
            }

            if (IsCapacityStatutoryHoliday(date))
            {
                return false;
            }

            return date.DayOfWeek != DayOfWeek.Saturday
                && date.DayOfWeek != DayOfWeek.Sunday;
        }

        private static bool IsCapacitySaturdayRestDay(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday
                && !IsCapacityMakeupWorkday(date)
                && !IsCapacityStatutoryHoliday(date);
        }

        private static bool IsCapacitySundayRestDay(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Sunday
                && !IsCapacityMakeupWorkday(date)
                && !IsCapacityStatutoryHoliday(date);
        }

        private static bool IsCapacityStatutoryHoliday(DateTime? date)
        {
            return date.HasValue && IsCapacityStatutoryHoliday(date.Value);
        }

        private static bool IsCapacityStatutoryHoliday(DateTime date)
        {
            return CapacityStatutoryHolidayDates2026.Contains(date.Date);
        }

        private static bool IsCapacityMakeupWorkday(DateTime date)
        {
            return CapacityMakeupWorkdayDates2026.Contains(date.Date);
        }

        private static string NormalizeText(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().Normalize(System.Text.NormalizationForm.FormKC);
        }

        private sealed class OrderRow
        {
            public int Id { get; set; }
            public string SalesOrderNo { get; set; }
            public string PlanTrackingNo { get; set; }
            public string MaterialCode { get; set; }
            public string SpecModel { get; set; }
            public string ValveCategory { get; set; }
            public string AssignedProductionLine { get; set; }
            public string ProductionLine { get; set; }
            public string NominalDiameter { get; set; }
            public decimal? OrderQty { get; set; }
            public DateTime? ScheduleDate { get; set; }
            public DateTime? CapacityScheduleDate { get; set; }
            public bool CapacityScheduleDateOverThreshold { get; set; }
        }
    }
}
