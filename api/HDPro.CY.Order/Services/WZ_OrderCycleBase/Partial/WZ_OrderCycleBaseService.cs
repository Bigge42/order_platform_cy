/*
 *所有关于WZ_OrderCycleBase类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*WZ_OrderCycleBaseService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Enums;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.Core.UserManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.EntityFrameworkCore.Storage;

namespace HDPro.CY.Order.Services
{
    public partial class WZ_OrderCycleBaseService
    {
        private static readonly ConcurrentDictionary<string, ValveRuleTaskProgress> ValveRuleTaskProgressStore = new ConcurrentDictionary<string, ValveRuleTaskProgress>(StringComparer.OrdinalIgnoreCase);

        private readonly IWZ_OrderCycleBaseRepository _repository;//访问数据库
        private readonly IERP_OrderTrackingRepository _orderTrackingRepository;
        private readonly IOCP_MaterialRepository _materialRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        [ActivatorUtilitiesConstructor]
        public WZ_OrderCycleBaseService(
            IWZ_OrderCycleBaseRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IERP_OrderTrackingRepository orderTrackingRepository,
            IOCP_MaterialRepository materialRepository,
            IHttpClientFactory httpClientFactory
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _orderTrackingRepository = orderTrackingRepository;
            _materialRepository = materialRepository;
            _httpClientFactory = httpClientFactory;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加WZ_OrderCycleBase特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加WZ_OrderCycleBase特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加WZ_OrderCycleBase特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(WZ_OrderCycleBase entity)
        {
            var response = base.ValidateCYOrderEntity(entity);

            // 在此处添加WZ_OrderCycleBase特有的数据验证逻辑

            return response;
        }

        public override PageGridData<WZ_OrderCycleBase> GetPageData(PageDataOptions options)
        {
            if (!ShouldUseWarningFirstSort(options))
            {
                return base.GetPageData(options);
            }

            options = ValidatePageOptions(options, out IQueryable<WZ_OrderCycleBase> queryable, IsMultiTenancy);
            if (QueryRelativeExpression != null)
            {
                queryable = QueryRelativeExpression.Invoke(queryable);
            }

            var pageGridData = new PageGridData<WZ_OrderCycleBase>();
            if (options.Export)
            {
                queryable = ApplyWarningFirstSort(queryable, options);
                if (Limit > 0)
                {
                    queryable = queryable.Take(Limit);
                }

                pageGridData.rows = FilterOrderCycleBaseAuthFields(queryable);
            }
            else
            {
                if (SummaryExpress != null)
                {
                    pageGridData.summary = SummaryExpress.Invoke(queryable);
                }

                queryable = ApplyWarningFirstSort(queryable, options);
                queryable = repository.IQueryablePage(
                    queryable,
                    options.Page,
                    options.Rows,
                    out var rowCount,
                    new Dictionary<string, QueryOrderBy>());
                pageGridData.rows = FilterOrderCycleBaseAuthFields(queryable);
                pageGridData.total = rowCount;
            }

            GetPageDataOnExecuted?.Invoke(pageGridData);
            return pageGridData;
        }

        private static bool ShouldUseWarningFirstSort(PageDataOptions options)
        {
            var sort = options?.Sort?.Trim();
            return string.IsNullOrEmpty(sort)
                || string.Equals(sort, nameof(WZ_OrderCycleBase.Id), StringComparison.OrdinalIgnoreCase);
        }

        private static IQueryable<WZ_OrderCycleBase> ApplyWarningFirstSort(
            IQueryable<WZ_OrderCycleBase> queryable,
            PageDataOptions options)
        {
            var holidayDates = CapacityStatutoryHolidayDates2026.ToArray();
            var makeupWorkdayDates = CapacityMakeupWorkdayDates2026.ToArray();
            var sundayRestDates = CapacitySundayRestDates2026.ToArray();

            // 默认分页前先按页面颜色提示置顶，再沿用原来的 Id 顺序。
            var warningQuery = queryable.Select(row => new
            {
                Row = row,
                WarningSort =
                    row.ReplyDeliveryDate.HasValue
                    && row.StandardDeliveryDate.HasValue
                    && row.ReplyDeliveryDate.Value.Date < row.StandardDeliveryDate.Value.Date
                        ? 4
                        : row.CapacityScheduleDateOverThreshold
                            ? 3
                            : row.CapacityScheduleDate.HasValue
                              && holidayDates.Contains(row.CapacityScheduleDate.Value.Date)
                                ? 2
                                : row.CapacityScheduleDate.HasValue
                                  && !holidayDates.Contains(row.CapacityScheduleDate.Value.Date)
                                  && !makeupWorkdayDates.Contains(row.CapacityScheduleDate.Value.Date)
                                  && sundayRestDates.Contains(row.CapacityScheduleDate.Value.Date)
                                    ? 1
                                    : 0
            });

            var orderedQuery = warningQuery.OrderByDescending(row => row.WarningSort);
            return string.Equals(options?.Order, "asc", StringComparison.OrdinalIgnoreCase)
                ? orderedQuery.ThenBy(row => row.Row.Id).Select(row => row.Row)
                : orderedQuery.ThenByDescending(row => row.Row.Id).Select(row => row.Row);
        }

        private static List<WZ_OrderCycleBase> FilterOrderCycleBaseAuthFields(IQueryable<WZ_OrderCycleBase> queryable)
        {
            var tableName = nameof(WZ_OrderCycleBase);
            var authFields = RoleContext.GetCurrentRoleAuthFields(tableName);
            if (authFields.Length == 0)
            {
                return queryable.ToList();
            }

            var source = typeof(WZ_OrderCycleBase);
            var target = typeof(WZ_OrderCycleBase);
            var parameter = Expression.Parameter(source, "t");
            var assignments = new List<MemberAssignment>();
            var hideFields = TableColumnContext.GetTableHideFields(tableName);
            var fields = source.GetProperties()
                .Where(property => authFields.Contains(property.Name) || hideFields.Contains(property.Name))
                .Select(property => property.Name)
                .ToList();

            foreach (var field in fields)
            {
                var sourceProperty = source.GetProperty(field);
                var targetProperty = target.GetProperty(field);
                if (sourceProperty == null || targetProperty == null)
                {
                    continue;
                }

                var memberAccess = Expression.MakeMemberAccess(parameter, sourceProperty);
                assignments.Add(Expression.Bind(targetProperty, memberAccess));
            }

            var memberInit = Expression.MemberInit(Expression.New(target), assignments);
            var expression = (Expression<Func<WZ_OrderCycleBase, WZ_OrderCycleBase>>)Expression.Lambda(memberInit, parameter);
            return queryable.Select(expression).ToList();
        }

        public override WebResponseContent Export(PageDataOptions pageData)
        {
            var response = new WebResponseContent();
            try
            {
                pageData ??= new PageDataOptions();
                pageData.Export = true;

                var exportColumns = GetOrderCycleBaseExportColumns(pageData);
                if (!exportColumns.Any())
                {
                    return response.Error("未获取到导出列，请刷新页面后重试");
                }

                pageData.Columns = exportColumns.Select(column => column.Field).ToArray();
                var list = GetPageData(pageData).rows ?? new List<WZ_OrderCycleBase>();

                var folder = DateTime.Now.ToString("yyyyMMdd");
                var savePath = $"Download/ExcelExport/{folder}/".MapPath();
                var fileName = $"排产智能体优化看板{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                var fullPath = Path.Combine(savePath, fileName);
                WriteOrderCycleBaseExcel(list, exportColumns, fullPath);
                return response.OK(null, fullPath);
            }
            catch (Exception ex)
            {
                return response.Error($"导出失败：{ex.Message}");
            }
        }

        private static List<OrderCycleBaseExportColumn> GetOrderCycleBaseExportColumns(PageDataOptions pageData)
        {
            var properties = typeof(WZ_OrderCycleBase).GetProperties()
                .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

            var fields = pageData?.Columns != null && pageData.Columns.Length > 0
                ? pageData.Columns
                : properties.Values
                    .Where(property => property.GetCustomAttributes(typeof(DisplayAttribute), true).Any())
                    .Select(property => property.Name)
                    .ToArray();

            return fields
                .Where(field => !string.IsNullOrWhiteSpace(field)
                    && properties.ContainsKey(field)
                    && !string.Equals(field, nameof(WZ_OrderCycleBase.CapacityScheduleDateOverThreshold), StringComparison.OrdinalIgnoreCase))
                .GroupBy(field => field, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var property = properties[group.First()];
                    return new OrderCycleBaseExportColumn
                    {
                        Field = property.Name,
                        Title = property.GetCustomAttributes(typeof(DisplayAttribute), true)
                            .OfType<DisplayAttribute>()
                            .FirstOrDefault()
                            ?.Name ?? property.Name,
                        Type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType
                    };
                })
                .ToList();
        }

        private static void WriteOrderCycleBaseExcel(
            List<WZ_OrderCycleBase> list,
            List<OrderCycleBaseExportColumn> exportColumns,
            string fullPath)
        {
            var properties = typeof(WZ_OrderCycleBase).GetProperties()
                .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("sheet1");

            for (var columnIndex = 0; columnIndex < exportColumns.Count; columnIndex++)
            {
                var exportColumn = exportColumns[columnIndex];
                var cell = worksheet.Cells[1, columnIndex + 1];
                cell.Value = exportColumn.Title;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.Gray);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Column(columnIndex + 1).Width = 16D;
            }

            for (var rowIndex = 0; rowIndex < list.Count; rowIndex++)
            {
                var row = list[rowIndex];
                var isDeliveryLaterThanStandard = IsReplyDeliveryDateLaterThanStandard(row);
                var isCapacityHoliday = IsCapacityStatutoryHoliday(row.CapacityScheduleDate);
                for (var columnIndex = 0; columnIndex < exportColumns.Count; columnIndex++)
                {
                    var exportColumn = exportColumns[columnIndex];
                    var property = properties[exportColumn.Field];
                    var cell = worksheet.Cells[rowIndex + 2, columnIndex + 1];
                    var value = property.GetValue(row);
                    SetOrderCycleBaseCellValue(cell, value, exportColumn.Type);

                    if (isDeliveryLaterThanStandard)
                    {
                        ApplyRedWarningCellStyle(cell);
                        continue;
                    }

                    if (isCapacityHoliday)
                    {
                        ApplyHolidayWarningCellStyle(cell);
                    }

                    if (!string.Equals(exportColumn.Field, nameof(WZ_OrderCycleBase.CapacityScheduleDate), StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (row.CapacityScheduleDateOverThreshold)
                    {
                        ApplyRedWarningCellStyle(cell);
                    }
                    else if (isCapacityHoliday)
                    {
                        ApplyHolidayWarningCellStyle(cell);
                    }
                    else if (row.CapacityScheduleDate.HasValue && IsCapacitySundayRestDay(row.CapacityScheduleDate.Value))
                    {
                        ApplySundayReserveCellStyle(cell);
                    }
                }
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFilter = true;
                worksheet.View.FreezePanes(2, 1);
            }

            package.SaveAs(new FileInfo(fullPath));
        }

        private static bool IsReplyDeliveryDateLaterThanStandard(WZ_OrderCycleBase row)
        {
            return row?.ReplyDeliveryDate.HasValue == true
                && row.StandardDeliveryDate.HasValue
                && row.ReplyDeliveryDate.Value.Date < row.StandardDeliveryDate.Value.Date;
        }

        private static void ApplyRedWarningCellStyle(ExcelRange cell)
        {
            cell.Style.Font.Color.SetColor(Color.FromArgb(208, 48, 80));
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 241, 240));
        }

        private static void ApplySundayReserveCellStyle(ExcelRange cell)
        {
            cell.Style.Font.Color.SetColor(Color.FromArgb(140, 90, 0));
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 247, 214));
        }

        private static void ApplyHolidayWarningCellStyle(ExcelRange cell)
        {
            cell.Style.Font.Color.SetColor(Color.FromArgb(140, 90, 0));
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 244, 199));
        }

        private static void SetOrderCycleBaseCellValue(ExcelRange cell, object value, Type type)
        {
            if (value == null)
            {
                cell.Value = null;
                return;
            }

            if (type == typeof(DateTime) && value is DateTime dateTime)
            {
                cell.Value = dateTime;
                cell.Style.Numberformat.Format = "yyyy-mm-dd";
                return;
            }

            cell.Value = value;
        }

        /// <summary>
        /// 从订单跟踪表同步数据到订单周期基础表
        /// </summary>
        /// <param name="approvedDateStart">审核日期起</param>
        /// <param name="approvedDateEnd">审核日期止</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>同步的总行数</returns>
        public async Task<int> SyncFromOrderTrackingAsync(DateTime? approvedDateStart, DateTime? approvedDateEnd, CancellationToken cancellationToken = default)
        {
            var orderTrackingContext = _orderTrackingRepository?.DbContext
                ?? throw new InvalidOperationException("订单跟踪仓储未正确初始化");
            var materialContext = _materialRepository?.DbContext
                ?? throw new InvalidOperationException("物料仓储未正确初始化");
            var orderCycleContext = _repository?.DbContext
                ?? throw new InvalidOperationException("订单周期仓储未正确初始化");

            /*.Where(p => p.BillStatus == "正常" && p.MtoNoStatus != "冻结" && p.MtoNoStatus != "终止")*/
            var orderTrackingQuery = orderTrackingContext.Set<ERP_OrderTracking>()
                .AsNoTracking()
                .Where(p => p.FBILLNO != null
                    && !p.FBILLNO.StartsWith("W")
                    && !p.FBILLNO.Contains("JWX"));

            if (approvedDateStart.HasValue)
            {
                var startDate = approvedDateStart.Value.Date;
                orderTrackingQuery = orderTrackingQuery.Where(p => p.FAPPROVEDATE >= startDate);
            }

            if (approvedDateEnd.HasValue)
            {
                var endDate = approvedDateEnd.Value.Date;
                orderTrackingQuery = orderTrackingQuery.Where(p => p.FAPPROVEDATE <= endDate);
            }

            var orderTrackingList = await orderTrackingQuery.ToListAsync(cancellationToken);

            var materialNumbers = orderTrackingList.Where(p => !string.IsNullOrWhiteSpace(p.FNUMBER))
                .Select(p => p.FNUMBER)
                .Distinct()
                .ToList();

            var materialDict = materialNumbers.Count == 0
                ? new Dictionary<string, OCP_Material>(StringComparer.OrdinalIgnoreCase)
                : (await materialContext.Set<OCP_Material>()
                    .AsNoTracking()
                    .Where(p => materialNumbers.Contains(p.MaterialCode))
                    .ToListAsync(cancellationToken))
                    .GroupBy(p => p.MaterialCode ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var salesOrderNos = orderTrackingList.Where(p => !string.IsNullOrWhiteSpace(p.FBILLNO))
                .Select(p => p.FBILLNO)
                .Distinct()
                .ToList();

            var planTrackingNos = orderTrackingList.Select(p => NormalizePlanTrackingNo(p.FMTONO))
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .ToList();

            using var transaction = await orderCycleContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                try
                {
                    await orderCycleContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [WZ_OrderCycleBase];", cancellationToken);
                }
                catch
                {
                    await orderCycleContext.Database.ExecuteSqlRawAsync("DELETE FROM [WZ_OrderCycleBase];", cancellationToken);
                }

                if (orderTrackingList.Count == 0)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return 0;
                }

                var existingRecords = await orderCycleContext.Set<WZ_OrderCycleBase>()
                    .Where(p => salesOrderNos.Contains(p.SalesOrderNo) && planTrackingNos.Contains(p.PlanTrackingNo))
                    .ToListAsync(cancellationToken);

                var existingDict = new Dictionary<string, WZ_OrderCycleBase>(StringComparer.OrdinalIgnoreCase);
                foreach (var record in existingRecords)
                {
                    var key = $"{record.SalesOrderNo}__{record.PlanTrackingNo}";
                    if (existingDict.ContainsKey(key))
                    {
                        continue;
                    }

                    existingDict[key] = record;
                }

                var toInsert = new List<WZ_OrderCycleBase>();
                var updatedCount = 0;

                foreach (var orderTracking in orderTrackingList)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var planTrackingNo = NormalizePlanTrackingNo(orderTracking?.FMTONO);
                    if (orderTracking == null || string.IsNullOrWhiteSpace(orderTracking.FBILLNO) || string.IsNullOrWhiteSpace(planTrackingNo))
                    {
                        continue;
                    }

                    materialDict.TryGetValue(orderTracking.FNUMBER ?? string.Empty, out var materialInfo);

                    var key = $"{orderTracking.FBILLNO}__{planTrackingNo}";

                    if (existingDict.TryGetValue(key, out var existing))
                    {
                        MapFields(orderTracking, materialInfo, existing);
                        updatedCount++;
                        continue;
                    }

                    var newEntity = new WZ_OrderCycleBase();
                    MapFields(orderTracking, materialInfo, newEntity);
                    toInsert.Add(newEntity);
                }

                if (toInsert.Count > 0)
                {
                    _repository.AddRange(toInsert);
                }

                if (updatedCount == 0 && toInsert.Count == 0)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return 0;
                }

                await orderCycleContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return updatedCount + toInsert.Count;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// 从订单周期基础表同步预排产输出数据
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>同步的总行数</returns>
        public async Task<int> SyncPreProductionOutputAsync(CancellationToken cancellationToken = default)
        {
            var context = _repository?.DbContext
                ?? throw new InvalidOperationException("订单周期仓储未正确初始化");

            using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [WZ_PreProductionOutput];", cancellationToken);
                }
                catch
                {
                    await context.Database.ExecuteSqlRawAsync("DELETE FROM [WZ_PreProductionOutput];", cancellationToken);
                }

                var outputs = await context.Set<WZ_OrderCycleBase>()
                    .AsNoTracking()
                    .Where(p => p.ScheduleDate.HasValue
                        && p.OrderQty.HasValue
                        && p.ValveCategory != null
                        && p.ValveCategory != string.Empty
                        && ((p.AssignedProductionLine != null && p.AssignedProductionLine != string.Empty)
                            || (p.ProductionLine != null && p.ProductionLine != string.Empty)))
                    .Select(p => new WZ_PreProductionOutput
                    {
                        ProductionDate = p.ScheduleDate,
                        CapacityScheduleDate = p.CapacityScheduleDate,
                        ValveCategory = p.ValveCategory,
                        ProductionLine = p.AssignedProductionLine != null && p.AssignedProductionLine != string.Empty
                            ? p.AssignedProductionLine
                            : p.ProductionLine,
                        Quantity = p.OrderQty ?? 0M
                    })
                    .ToListAsync(cancellationToken);

                if (outputs.Count == 0)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return 0;
                }

                context.Set<WZ_PreProductionOutput>().AddRange(outputs);
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return outputs.Count;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// 完整执行排产初始化链路，确保规则服务、品类、产线、优化日期和预排产输出同步在同一次操作内闭环。
        /// </summary>
        public async Task<InitializeSchedulingSummary> InitializeSchedulingAsync(
            int batchSize = 1000,
            CancellationToken cancellationToken = default,
            string progressTaskId = null)
        {
            if (batchSize <= 0)
            {
                batchSize = 1000;
            }

            PublishValveRuleTaskProgress(progressTaskId, progress =>
            {
                progress.Status = "running";
                progress.Stage = "调用规则服务";
                progress.Message = "正在计算标准交货日期和排产日期";
                progress.Total = 6;
                progress.Processed = 0;
                progress.Percent = 5;
            });

            var summary = new InitializeSchedulingSummary
            {
                ValveRule = await BatchCallValveRuleServiceAsync(cancellationToken)
            };
            ClearOrderCycleChangeTracker();

            PublishValveRuleTaskProgress(progressTaskId, progress =>
            {
                progress.Status = "running";
                progress.Stage = "回填阀门品类";
                progress.Message = "正在按规则补齐阀门品类";
                progress.Total = 6;
                progress.Processed = 1;
                progress.Percent = 30;
            });
            summary.ValveCategoryUpdated = await FillValveCategoryByRuleAsync(batchSize);
            ClearOrderCycleChangeTracker();

            PublishValveRuleTaskProgress(progressTaskId, progress =>
            {
                progress.Status = "running";
                progress.Stage = "分配产线";
                progress.Message = "正在按规则分配产线";
                progress.Total = 6;
                progress.Processed = 2;
                progress.Updated = summary.ValveCategoryUpdated;
                progress.Percent = 45;
            });
            summary.AssignedProductionLine = await BatchAssignProductionLineByRuleAsync(batchSize, cancellationToken);
            ClearOrderCycleChangeTracker();

            PublishValveRuleTaskProgress(progressTaskId, progress =>
            {
                progress.Status = "running";
                progress.Stage = "产能排产";
                progress.Message = "正在按产线产能池计算排产优化日期";
                progress.Total = 6;
                progress.Processed = 3;
                progress.Updated = summary.ValveCategoryUpdated + (summary.AssignedProductionLine?.Updated ?? 0);
                progress.Percent = 60;
            });
            summary.CapacitySchedule = await CalculateCapacityScheduleDateAsync(cancellationToken);
            ClearOrderCycleChangeTracker();

            PublishValveRuleTaskProgress(progressTaskId, progress =>
            {
                progress.Status = "running";
                progress.Stage = "同步预排产输出";
                progress.Message = "正在同步预排产输出数据";
                progress.Total = 6;
                progress.Processed = 4;
                progress.Updated = summary.CapacitySchedule?.Updated ?? 0;
                progress.Percent = 78;
            });
            summary.PreProductionOutputSynced = await SyncPreProductionOutputAsync(cancellationToken);
            ClearOrderCycleChangeTracker();

            PublishValveRuleTaskProgress(progressTaskId, progress =>
            {
                progress.Status = "running";
                progress.Stage = "收尾检查";
                progress.Message = "正在检查未补齐数据和异常提示";
                progress.Total = 6;
                progress.Processed = 5;
                progress.Updated = summary.CapacitySchedule?.Updated ?? 0;
                progress.Percent = 92;
            });
            summary.RemainingNonBjBlankCapacityScheduleDate = await CountNonBjBlankCapacityScheduleDateAsync(cancellationToken);

            if (summary.ValveRule?.Failed > 0)
            {
                summary.Warnings.Add($"规则服务失败 {summary.ValveRule.Failed} 条");
            }

            if (summary.AssignedProductionLine?.Failed > 0)
            {
                summary.Warnings.Add($"产线规则分配失败 {summary.AssignedProductionLine.Failed} 条");
            }

            if (summary.CapacitySchedule?.MissingThreshold > 0)
            {
                summary.Warnings.Add($"阈值缺失 {summary.CapacitySchedule.MissingThreshold} 条");
            }

            if (summary.CapacitySchedule?.MissingProductionOutput > 0)
            {
                summary.Warnings.Add($"排产优化未命中产能数据 {summary.CapacitySchedule.MissingProductionOutput} 条");
            }

            if (summary.RemainingNonBjBlankCapacityScheduleDate > 0)
            {
                summary.Warnings.Add($"非 BJ 物料排产优化日期仍为空 {summary.RemainingNonBjBlankCapacityScheduleDate} 条");
            }

            PublishValveRuleTaskProgress(progressTaskId, progress =>
            {
                progress.Status = "success";
                progress.Stage = "初始化完成";
                progress.Message = summary.Warnings.Count > 0
                    ? $"排产初始化完成，仍有需处理项：{string.Join("；", summary.Warnings)}"
                    : "排产初始化完成";
                progress.Total = 6;
                progress.Processed = 6;
                progress.Succeeded = 6;
                progress.Failed = 0;
                progress.Updated = summary.CapacitySchedule?.Updated ?? 0;
                progress.Percent = 100;
            });

            return summary;
        }

        private void ClearOrderCycleChangeTracker()
        {
            _repository?.DbContext?.ChangeTracker.Clear();
        }

        private async Task<int> CountNonBjBlankCapacityScheduleDateAsync(CancellationToken cancellationToken)
        {
            var context = _repository?.DbContext
                ?? throw new InvalidOperationException("订单周期仓储未正确初始化");

            return await context.Set<WZ_OrderCycleBase>()
                .AsNoTracking()
                .CountAsync(p => !p.CapacityScheduleDate.HasValue
                    && (p.MaterialCode == null || !p.MaterialCode.Trim().ToUpper().StartsWith("BJ")),
                    cancellationToken);
        }

        /// <summary>
        /// 接收 10.101 汇总后的空排产日期预测数据，仅写入人工核对表，不参与现有排产逻辑。
        /// </summary>
        public async Task<SchedulePredictionReceiveSummary> ReceiveSchedulePredictionReviewAsync(
            IReadOnlyCollection<SchedulePredictionReviewReceiveDto> items,
            CancellationToken cancellationToken = default)
        {
            var context = _repository?.DbContext
                ?? throw new InvalidOperationException("订单周期仓储未正确初始化");

            var summary = new SchedulePredictionReceiveSummary
            {
                Received = items?.Count ?? 0
            };

            await EnsureSchedulePredictionReviewTableAsync(context, cancellationToken);

            if (items == null || items.Count == 0)
            {
                return summary;
            }

            var uniqueItems = new Dictionary<string, SchedulePredictionReviewReceiveDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                var fingerprint = NormalizePredictionText(item?.InputFingerprint);
                if (string.IsNullOrWhiteSpace(fingerprint))
                {
                    summary.Skipped++;
                    continue;
                }

                item.InputFingerprint = fingerprint;
                if (!uniqueItems.ContainsKey(fingerprint))
                {
                    uniqueItems[fingerprint] = item;
                }
                else
                {
                    summary.Skipped++;
                }
            }

            if (uniqueItems.Count == 0)
            {
                return summary;
            }

            var table = BuildSchedulePredictionReviewDataTable(uniqueItems.Values);
            var connection = context.Database.GetDbConnection();
            if (connection is not SqlConnection sqlConnection)
            {
                throw new InvalidOperationException("排产预测核对表增量入库需要 SQL Server 连接");
            }

            var shouldClose = sqlConnection.State != ConnectionState.Open;
            if (shouldClose)
            {
                await sqlConnection.OpenAsync(cancellationToken);
            }

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                if (transaction.GetDbTransaction() is not SqlTransaction sqlTransaction)
                {
                    throw new InvalidOperationException("排产预测核对表增量入库必须使用 SQL Server 事务");
                }

                var createTempSql = @"
IF OBJECT_ID('tempdb..#WZ_OrderCycleSchedulePredictionReviewImport') IS NOT NULL
    DROP TABLE #WZ_OrderCycleSchedulePredictionReviewImport;

CREATE TABLE #WZ_OrderCycleSchedulePredictionReviewImport
(
    [PredictionResultId] BIGINT NULL,
    [OrderCycleBaseId] INT NULL,
    [InputFingerprint] NVARCHAR(64) COLLATE DATABASE_DEFAULT NOT NULL,
    [RequestBatchNo] NVARCHAR(64) COLLATE DATABASE_DEFAULT NULL,
    [ProductName] NVARCHAR(200) COLLATE DATABASE_DEFAULT NULL,
    [SpecModel] NVARCHAR(200) COLLATE DATABASE_DEFAULT NULL,
    [ValveCategory] NVARCHAR(2000) COLLATE DATABASE_DEFAULT NULL,
    [NominalDiameter] NVARCHAR(50) COLLATE DATABASE_DEFAULT NULL,
    [NominalPressure] NVARCHAR(50) COLLATE DATABASE_DEFAULT NULL,
    [ProductionLine] NVARCHAR(50) COLLATE DATABASE_DEFAULT NULL,
    [FixedCycleDays] INT NULL,
    [PredictedScheduleDate] DATE NULL,
    [StandardDeliveryDate] DATE NULL,
    [ConfidenceScore] DECIMAL(18,6) NULL,
    [MatchedRuleCount] INT NULL,
    [UsedFieldsJson] NVARCHAR(MAX) COLLATE DATABASE_DEFAULT NULL,
    [CandidateSuggestionsJson] NVARCHAR(MAX) COLLATE DATABASE_DEFAULT NULL,
    [FailureReason] NVARCHAR(200) COLLATE DATABASE_DEFAULT NULL,
    [FailureMessage] NVARCHAR(500) COLLATE DATABASE_DEFAULT NULL
);";

                await using (var command = new SqlCommand(createTempSql, sqlConnection, sqlTransaction))
                {
                    command.CommandTimeout = 0;
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                using (var bulk = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.CheckConstraints, sqlTransaction))
                {
                    bulk.DestinationTableName = "#WZ_OrderCycleSchedulePredictionReviewImport";
                    bulk.BatchSize = table.Rows.Count;
                    bulk.BulkCopyTimeout = 0;
                    foreach (DataColumn column in table.Columns)
                    {
                        bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    await bulk.WriteToServerAsync(table, cancellationToken);
                }

                var mergeSql = @"
CREATE INDEX [IX_WZ_OrderCycleSchedulePredictionReviewImport_Key]
    ON #WZ_OrderCycleSchedulePredictionReviewImport([InputFingerprint]);

CREATE TABLE #WZ_OrderCycleSchedulePredictionReviewMergeResult([Action] NVARCHAR(10) NOT NULL);

MERGE [dbo].[WZ_OrderCycleSchedulePredictionReview] WITH (HOLDLOCK) AS target
USING #WZ_OrderCycleSchedulePredictionReviewImport AS source
ON target.[InputFingerprint] = source.[InputFingerprint] COLLATE DATABASE_DEFAULT
WHEN MATCHED THEN UPDATE SET
    [PredictionResultId] = source.[PredictionResultId],
    [OrderCycleBaseId] = source.[OrderCycleBaseId],
    [RequestBatchNo] = source.[RequestBatchNo],
    [ProductName] = source.[ProductName],
    [SpecModel] = source.[SpecModel],
    [ValveCategory] = source.[ValveCategory],
    [NominalDiameter] = source.[NominalDiameter],
    [NominalPressure] = source.[NominalPressure],
    [ProductionLine] = source.[ProductionLine],
    [FixedCycleDays] = source.[FixedCycleDays],
    [PredictedScheduleDate] = source.[PredictedScheduleDate],
    [StandardDeliveryDate] = source.[StandardDeliveryDate],
    [ConfidenceScore] = source.[ConfidenceScore],
    [MatchedRuleCount] = source.[MatchedRuleCount],
    [UsedFieldsJson] = source.[UsedFieldsJson],
    [CandidateSuggestionsJson] = source.[CandidateSuggestionsJson],
    [FailureReason] = source.[FailureReason],
    [FailureMessage] = source.[FailureMessage],
    [ReviewStatus] = ISNULL(NULLIF(target.[ReviewStatus], N''), N'待核对'),
    [LastSeenAt] = GETDATE(),
    [SeenCount] = ISNULL(target.[SeenCount], 0) + 1,
    [IsActive] = 1
WHEN NOT MATCHED BY TARGET THEN INSERT
(
    [PredictionResultId],
    [OrderCycleBaseId],
    [InputFingerprint],
    [RequestBatchNo],
    [ProductName],
    [SpecModel],
    [ValveCategory],
    [NominalDiameter],
    [NominalPressure],
    [ProductionLine],
    [FixedCycleDays],
    [PredictedScheduleDate],
    [StandardDeliveryDate],
    [ConfidenceScore],
    [MatchedRuleCount],
    [UsedFieldsJson],
    [CandidateSuggestionsJson],
    [FailureReason],
    [FailureMessage],
    [ReviewStatus],
    [ReviewRemark],
    [FirstSeenAt],
    [LastSeenAt],
    [SeenCount],
    [IsActive]
)
VALUES
(
    source.[PredictionResultId],
    source.[OrderCycleBaseId],
    source.[InputFingerprint],
    source.[RequestBatchNo],
    source.[ProductName],
    source.[SpecModel],
    source.[ValveCategory],
    source.[NominalDiameter],
    source.[NominalPressure],
    source.[ProductionLine],
    source.[FixedCycleDays],
    source.[PredictedScheduleDate],
    source.[StandardDeliveryDate],
    source.[ConfidenceScore],
    source.[MatchedRuleCount],
    source.[UsedFieldsJson],
    source.[CandidateSuggestionsJson],
    source.[FailureReason],
    source.[FailureMessage],
    N'待核对',
    NULL,
    GETDATE(),
    GETDATE(),
    1,
    1
)
OUTPUT $action INTO #WZ_OrderCycleSchedulePredictionReviewMergeResult;

SELECT COUNT(1) FROM #WZ_OrderCycleSchedulePredictionReviewMergeResult;";

                await using (var command = new SqlCommand(mergeSql, sqlConnection, sqlTransaction))
                {
                    command.CommandTimeout = 0;
                    var result = await command.ExecuteScalarAsync(cancellationToken);
                    summary.Saved = Convert.ToInt32(result);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (shouldClose)
                {
                    await sqlConnection.CloseAsync();
                }
            }

            return summary;
        }

        /// <summary>
        /// 导出空排产日期预测核对数据，导出内容不包含订单号、计划跟踪号等订单信息。
        /// </summary>
        public WebResponseContent ExportSchedulePredictionReview(PageDataOptions pageData)
        {
            var response = new WebResponseContent();
            try
            {
                var context = _repository?.DbContext
                    ?? throw new InvalidOperationException("订单周期仓储未正确初始化");

                EnsureSchedulePredictionReviewTableAsync(context, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                var list = (
                    from review in context.Set<WZ_OrderCycleSchedulePredictionReview>().AsNoTracking()
                    join orderCycle in context.Set<WZ_OrderCycleBase>().AsNoTracking()
                        on review.OrderCycleBaseId equals (int?)orderCycle.Id
                    where review.IsActive && !orderCycle.ScheduleDate.HasValue
                    orderby review.LastSeenAt descending, review.Id descending
                    select review
                ).ToList();

                var folder = DateTime.Now.ToString("yyyyMMdd");
                var savePath = $"Download/ExcelExport/{folder}/".MapPath();
                var fileName = $"排产日期预测核对{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                var fullPath = Path.Combine(savePath, fileName);
                WriteSchedulePredictionReviewExcel(list, fullPath);
                return response.OK(null, fullPath);
            }
            catch (Exception ex)
            {
                return response.Error($"导出预测数据失败：{ex.Message}");
            }
        }

        private static async Task EnsureSchedulePredictionReviewTableAsync(DbContext context, CancellationToken cancellationToken)
        {
            var sql = @"
IF OBJECT_ID(N'[dbo].[WZ_OrderCycleSchedulePredictionReview]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WZ_OrderCycleSchedulePredictionReview](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WZ_OrderCycleSchedulePredictionReview] PRIMARY KEY,
        [PredictionResultId] BIGINT NULL,
        [OrderCycleBaseId] INT NULL,
        [InputFingerprint] NVARCHAR(64) NOT NULL,
        [RequestBatchNo] NVARCHAR(64) NULL,
        [ProductName] NVARCHAR(200) NULL,
        [SpecModel] NVARCHAR(200) NULL,
        [ValveCategory] NVARCHAR(2000) NULL,
        [NominalDiameter] NVARCHAR(50) NULL,
        [NominalPressure] NVARCHAR(50) NULL,
        [ProductionLine] NVARCHAR(50) NULL,
        [FixedCycleDays] INT NULL,
        [PredictedScheduleDate] DATE NULL,
        [StandardDeliveryDate] DATE NULL,
        [ConfidenceScore] DECIMAL(18,6) NULL,
        [MatchedRuleCount] INT NULL,
        [UsedFieldsJson] NVARCHAR(MAX) NULL,
        [CandidateSuggestionsJson] NVARCHAR(MAX) NULL,
        [FailureReason] NVARCHAR(200) NULL,
        [FailureMessage] NVARCHAR(500) NULL,
        [ReviewStatus] NVARCHAR(50) NOT NULL CONSTRAINT [DF_WZ_OrderCycleSchedulePredictionReview_ReviewStatus] DEFAULT(N'待核对'),
        [ReviewRemark] NVARCHAR(500) NULL,
        [FirstSeenAt] DATETIME NOT NULL CONSTRAINT [DF_WZ_OrderCycleSchedulePredictionReview_FirstSeenAt] DEFAULT(GETDATE()),
        [LastSeenAt] DATETIME NOT NULL CONSTRAINT [DF_WZ_OrderCycleSchedulePredictionReview_LastSeenAt] DEFAULT(GETDATE()),
        [SeenCount] INT NOT NULL CONSTRAINT [DF_WZ_OrderCycleSchedulePredictionReview_SeenCount] DEFAULT(1),
        [IsActive] BIT NOT NULL CONSTRAINT [DF_WZ_OrderCycleSchedulePredictionReview_IsActive] DEFAULT(1)
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_WZ_OrderCycleSchedulePredictionReview_InputFingerprint'
      AND object_id = OBJECT_ID(N'dbo.WZ_OrderCycleSchedulePredictionReview')
)
BEGIN
    CREATE UNIQUE INDEX [UX_WZ_OrderCycleSchedulePredictionReview_InputFingerprint]
        ON [dbo].[WZ_OrderCycleSchedulePredictionReview]([InputFingerprint]);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_WZ_OrderCycleSchedulePredictionReview_IsActive_LastSeenAt'
      AND object_id = OBJECT_ID(N'dbo.WZ_OrderCycleSchedulePredictionReview')
)
BEGIN
    CREATE INDEX [IX_WZ_OrderCycleSchedulePredictionReview_IsActive_LastSeenAt]
        ON [dbo].[WZ_OrderCycleSchedulePredictionReview]([IsActive], [LastSeenAt] DESC);
END;";

            await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        private static DataTable BuildSchedulePredictionReviewDataTable(IEnumerable<SchedulePredictionReviewReceiveDto> items)
        {
            var table = new DataTable();
            table.Columns.Add("PredictionResultId", typeof(long));
            table.Columns.Add("OrderCycleBaseId", typeof(int));
            table.Columns.Add("InputFingerprint", typeof(string));
            table.Columns.Add("RequestBatchNo", typeof(string));
            table.Columns.Add("ProductName", typeof(string));
            table.Columns.Add("SpecModel", typeof(string));
            table.Columns.Add("ValveCategory", typeof(string));
            table.Columns.Add("NominalDiameter", typeof(string));
            table.Columns.Add("NominalPressure", typeof(string));
            table.Columns.Add("ProductionLine", typeof(string));
            table.Columns.Add("FixedCycleDays", typeof(int));
            table.Columns.Add("PredictedScheduleDate", typeof(DateTime));
            table.Columns.Add("StandardDeliveryDate", typeof(DateTime));
            table.Columns.Add("ConfidenceScore", typeof(decimal));
            table.Columns.Add("MatchedRuleCount", typeof(int));
            table.Columns.Add("UsedFieldsJson", typeof(string));
            table.Columns.Add("CandidateSuggestionsJson", typeof(string));
            table.Columns.Add("FailureReason", typeof(string));
            table.Columns.Add("FailureMessage", typeof(string));

            foreach (var item in items)
            {
                var row = table.NewRow();
                row["PredictionResultId"] = ToDbValue(item.PredictionResultId);
                row["OrderCycleBaseId"] = ToDbValue(item.OrderCycleBaseId);
                row["InputFingerprint"] = Truncate(NormalizePredictionText(item.InputFingerprint), 64);
                row["RequestBatchNo"] = ToDbValue(Truncate(NormalizePredictionText(item.RequestBatchNo), 64));
                row["ProductName"] = ToDbValue(Truncate(NormalizePredictionText(item.ProductName), 200));
                row["SpecModel"] = ToDbValue(Truncate(NormalizePredictionText(item.SpecModel), 200));
                row["ValveCategory"] = ToDbValue(Truncate(NormalizePredictionText(item.ValveCategory), 2000));
                row["NominalDiameter"] = ToDbValue(Truncate(NormalizePredictionText(item.NominalDiameter), 50));
                row["NominalPressure"] = ToDbValue(Truncate(NormalizePredictionText(item.NominalPressure), 50));
                row["ProductionLine"] = ToDbValue(Truncate(NormalizePredictionText(item.ProductionLine), 50));
                row["FixedCycleDays"] = ToDbValue(item.FixedCycleDays);
                row["PredictedScheduleDate"] = ToDbValue(item.PredictedScheduleDate?.Date);
                row["StandardDeliveryDate"] = ToDbValue(item.StandardDeliveryDate?.Date);
                row["ConfidenceScore"] = ToDbValue(item.ConfidenceScore);
                row["MatchedRuleCount"] = ToDbValue(item.MatchedRuleCount);
                row["UsedFieldsJson"] = ToDbValue(NormalizePredictionText(item.UsedFieldsJson));
                row["CandidateSuggestionsJson"] = ToDbValue(NormalizePredictionText(item.CandidateSuggestionsJson));
                row["FailureReason"] = ToDbValue(Truncate(NormalizePredictionText(item.FailureReason), 200));
                row["FailureMessage"] = ToDbValue(Truncate(NormalizePredictionText(item.FailureMessage), 500));
                table.Rows.Add(row);
            }

            return table;
        }

        private static void WriteSchedulePredictionReviewExcel(List<WZ_OrderCycleSchedulePredictionReview> list, string fullPath)
        {
            var exportColumns = new List<SchedulePredictionReviewExportColumn>
            {
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.ProductName), Title = "产品名称", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.SpecModel), Title = "规格型号", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.ValveCategory), Title = "阀门类别", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.NominalDiameter), Title = "公称通径", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.NominalPressure), Title = "公称压力", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.ProductionLine), Title = "最高可能生产线", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.FixedCycleDays), Title = "最高可能固定周期", Type = typeof(int) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.PredictedScheduleDate), Title = "最高可能排产日期", Type = typeof(DateTime) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.StandardDeliveryDate), Title = "标准交货日期", Type = typeof(DateTime) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.ConfidenceScore), Title = "置信度", Type = typeof(decimal) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.UsedFieldsJson), Title = "使用字段", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.CandidateSuggestionsJson), Title = "候选结果", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.FailureReason), Title = "失败原因", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.FailureMessage), Title = "失败说明", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.ReviewStatus), Title = "核对状态", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.ReviewRemark), Title = "核对备注", Type = typeof(string) },
                new SchedulePredictionReviewExportColumn { Field = nameof(WZ_OrderCycleSchedulePredictionReview.LastSeenAt), Title = "最近发现时间", Type = typeof(DateTime) }
            };

            var properties = typeof(WZ_OrderCycleSchedulePredictionReview).GetProperties()
                .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("预测核对");

            for (var columnIndex = 0; columnIndex < exportColumns.Count; columnIndex++)
            {
                var exportColumn = exportColumns[columnIndex];
                var cell = worksheet.Cells[1, columnIndex + 1];
                cell.Value = exportColumn.Title;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.Gray);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Column(columnIndex + 1).Width = 18D;
            }

            for (var rowIndex = 0; rowIndex < list.Count; rowIndex++)
            {
                var row = list[rowIndex];
                for (var columnIndex = 0; columnIndex < exportColumns.Count; columnIndex++)
                {
                    var exportColumn = exportColumns[columnIndex];
                    var property = properties[exportColumn.Field];
                    var cell = worksheet.Cells[rowIndex + 2, columnIndex + 1];
                    SetOrderCycleBaseCellValue(cell, property.GetValue(row), exportColumn.Type);
                }
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFilter = true;
                worksheet.View.FreezePanes(2, 1);
            }

            package.SaveAs(new FileInfo(fullPath));
        }

        private static object ToDbValue(object value)
        {
            return value == null ? DBNull.Value : value;
        }

        private static string NormalizePredictionText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength);
        }

        /// <summary>
        /// 基于产线产量与阈值计算产能排产日期
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>计算结果摘要</returns>
        public async Task<CapacityScheduleSummary> CalculateCapacityScheduleDateAsync(CancellationToken cancellationToken = default)
        {
            var context = _repository?.DbContext
                ?? throw new InvalidOperationException("订单周期仓储未正确初始化");

            var orders = await context.Set<WZ_OrderCycleBase>()
                .AsNoTracking()
                .Where(p => p.ScheduleDate.HasValue
                    && p.OrderQty.HasValue
                    && ((p.AssignedProductionLine != null && p.AssignedProductionLine != string.Empty)
                        || (p.ProductionLine != null && p.ProductionLine != string.Empty)))
                .OrderBy(p => p.ScheduleDate)
                .ThenBy(p => p.Id)
                .Select(p => new OrderCapacityCandidate
                {
                    Id = p.Id,
                    ScheduleDate = p.ScheduleDate,
                    OrderQty = p.OrderQty,
                    ValveCategory = p.ValveCategory,
                    AssignedProductionLine = p.AssignedProductionLine,
                    ProductionLine = p.ProductionLine,
                    NominalDiameter = p.NominalDiameter,
                    StandardDeliveryDate = p.StandardDeliveryDate,
                    ReplyDeliveryDate = p.ReplyDeliveryDate,
                    RequestedDeliveryDate = p.RequestedDeliveryDate
                })
                .ToListAsync(cancellationToken);

            var summary = new CapacityScheduleSummary
            {
                Total = orders.Count
            };

            if (orders.Count > 0)
            {
                var thresholdMap = await LoadCapacityThresholdMapAsync(context, cancellationToken);
                var outputs = await context.Set<WZ_ProductionOutput>()
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var capacityMap = new Dictionary<(string Cat, string Line, DateTime Date), CapacityBucket>();
                var categoryLineDates = new Dictionary<(string Cat, string Line), HashSet<DateTime>>();
                var outputThresholdMap = new Dictionary<(string Cat, string Line), decimal>();

                foreach (var output in outputs)
                {
                    if (output == null)
                    {
                        continue;
                    }

                    var cat = NormalizeCapacityText(output.ValveCategory);
                    var line = NormalizeCapacityText(output.ProductionLine);
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

                    var date = output.ProductionDate.Date;
                    var key = (cat, line, date);

                    if (!capacityMap.TryGetValue(key, out var bucket))
                    {
                        bucket = new CapacityBucket
                        {
                            Quantity = output.Quantity,
                            Threshold = ResolveCapacityThreshold(thresholdMap, outputThresholdMap, cat, line, output.CurrentThreshold)
                        };
                        capacityMap[key] = bucket;
                    }
                    else
                    {
                        bucket.Quantity += output.Quantity;
                        bucket.Threshold = MergeThreshold(
                            bucket.Threshold,
                            ResolveCapacityThreshold(thresholdMap, outputThresholdMap, cat, line, output.CurrentThreshold));
                    }

                    if (!categoryLineDates.TryGetValue(lineKey, out var dates))
                    {
                        dates = new HashSet<DateTime>();
                        categoryLineDates[lineKey] = dates;
                    }

                    dates.Add(date);
                }

                var capacityDateList = new Dictionary<(string Cat, string Line), List<DateTime>>();
                var capacityDateSets = new Dictionary<(string Cat, string Line), HashSet<DateTime>>();
                foreach (var item in categoryLineDates)
                {
                    var dateSet = item.Value;
                    var dates = dateSet.ToList();
                    dates.Sort();
                    capacityDateList[item.Key] = dates;
                    capacityDateSets[item.Key] = dateSet;
                }

                var updates = new List<WZ_OrderCycleBase>();
                var clearCapacityScheduleIds = new List<int>();

                foreach (var order in orders)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!order.ScheduleDate.HasValue || order.OrderQty.GetValueOrDefault() <= 0)
                    {
                        summary.Skipped++;
                        continue;
                    }

                    var targetDate = order.ScheduleDate.Value.Date;
                    var cat = NormalizeCapacityText(order.ValveCategory);
                    var line = ResolveCapacityLine(order);
                    if (string.IsNullOrWhiteSpace(cat) || string.IsNullOrWhiteSpace(line))
                    {
                        updates.Add(new WZ_OrderCycleBase
                        {
                            Id = order.Id,
                            CapacityScheduleDate = targetDate
                        });
                        summary.Updated++;
                        summary.FallbackScheduleDateCount++;
                        continue;
                    }

                    var capacityLineKey = (cat, line);
                    if (!capacityDateList.TryGetValue(capacityLineKey, out var dates))
                    {
                        dates = new List<DateTime>();
                        capacityDateList[capacityLineKey] = dates;
                    }

                    if (!capacityDateSets.TryGetValue(capacityLineKey, out var knownDates))
                    {
                        knownDates = new HashSet<DateTime>();
                        capacityDateSets[capacityLineKey] = knownDates;
                    }

                    var decision = ResolveCapacityScheduleDate(order, dates, knownDates, capacityMap, thresholdMap, outputThresholdMap, cat, line, targetDate);
                    if (!decision.CapacityDate.HasValue)
                    {
                        if (!ShouldFallbackCapacityScheduleDate(decision.FailureReason))
                        {
                            clearCapacityScheduleIds.Add(order.Id);
                            summary.Skipped++;
                            continue;
                        }

                        updates.Add(new WZ_OrderCycleBase
                        {
                            Id = order.Id,
                            CapacityScheduleDate = targetDate
                        });

                        summary.Updated++;
                        summary.FallbackScheduleDateCount++;
                        continue;
                    }

                    updates.Add(new WZ_OrderCycleBase
                    {
                        Id = order.Id,
                        CapacityScheduleDate = decision.CapacityDate
                    });

                    summary.Updated++;
                    switch (decision.Mode)
                    {
                        case CapacityScheduleMode.NormalCapacity:
                            summary.NormalCapacityCount++;
                            break;
                        case CapacityScheduleMode.DeliveryAdjusted:
                            summary.DeliveryAdjustedCount++;
                            break;
                        case CapacityScheduleMode.DailyReserve:
                            summary.DailyReserveCount++;
                            break;
                        case CapacityScheduleMode.SaturdayReserve:
                            summary.SaturdayReserveCount++;
                            break;
                        case CapacityScheduleMode.SundayReserve:
                            summary.SundayReserveCount++;
                            break;
                        case CapacityScheduleMode.BalancedOverflow:
                            summary.BalancedOverflowCount++;
                            break;
                    }
                }

                await UpdateCapacityScheduleDatesAsync(context, updates, cancellationToken);
                await ClearCapacityScheduleDatesAsync(context, clearCapacityScheduleIds, cancellationToken);
            }

            var fallbackUpdated = await FillBlankCapacityScheduleDateByScheduleDateAsync(context, cancellationToken);
            summary.Updated += fallbackUpdated;
            summary.FallbackScheduleDateCount += fallbackUpdated;
            summary.OverThresholdCount = await UpdateCapacityScheduleDateOverThresholdFlagsAsync(context, cancellationToken);

            return summary;
        }

        private static Task<int> FillBlankCapacityScheduleDateByScheduleDateAsync(DbContext context, CancellationToken cancellationToken)
        {
            return context.Set<WZ_OrderCycleBase>()
                .Where(p => p.ScheduleDate.HasValue
                    && !p.CapacityScheduleDate.HasValue
                    && (!p.OrderQty.HasValue
                        || p.OrderQty.Value <= 0
                        || ((p.AssignedProductionLine == null || p.AssignedProductionLine == string.Empty)
                            && (p.ProductionLine == null || p.ProductionLine == string.Empty))))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.CapacityScheduleDate, p => p.ScheduleDate), cancellationToken);
        }

        private static bool ShouldFallbackCapacityScheduleDate(string failureReason)
        {
            return !string.Equals(failureReason, CapacityFailureReasons.BaseCapacityReached, StringComparison.Ordinal);
        }

        private const int CapacityScheduleUpdateBatchSize = 1000;

        private static async Task UpdateCapacityScheduleDatesAsync(DbContext context, List<WZ_OrderCycleBase> updates, CancellationToken cancellationToken)
        {
            if (updates == null || updates.Count == 0)
            {
                return;
            }

            var updateGroups = updates
                .Where(p => p.CapacityScheduleDate.HasValue)
                .GroupBy(p => p.CapacityScheduleDate.Value.Date);

            foreach (var group in updateGroups)
            {
                var capacityScheduleDate = (DateTime?)group.Key;
                var ids = group
                    .Select(p => p.Id)
                    .Distinct()
                    .ToList();

                for (var index = 0; index < ids.Count; index += CapacityScheduleUpdateBatchSize)
                {
                    var batchIds = ids.Skip(index).Take(CapacityScheduleUpdateBatchSize).ToList();
                    await context.Set<WZ_OrderCycleBase>()
                        .Where(p => batchIds.Contains(p.Id))
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(p => p.CapacityScheduleDate, capacityScheduleDate), cancellationToken);
                }
            }
        }

        private static async Task ClearCapacityScheduleDatesAsync(DbContext context, List<int> ids, CancellationToken cancellationToken)
        {
            if (ids == null || ids.Count == 0)
            {
                return;
            }

            DateTime? emptyDate = null;
            var distinctIds = ids.Distinct().ToList();
            for (var index = 0; index < distinctIds.Count; index += CapacityScheduleUpdateBatchSize)
            {
                var batchIds = distinctIds.Skip(index).Take(CapacityScheduleUpdateBatchSize).ToList();
                await context.Set<WZ_OrderCycleBase>()
                    .Where(p => batchIds.Contains(p.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.CapacityScheduleDate, emptyDate)
                        .SetProperty(p => p.CapacityScheduleDateOverThreshold, false), cancellationToken);
            }
        }

        private static async Task<int> UpdateCapacityScheduleDateOverThresholdFlagsAsync(DbContext context, CancellationToken cancellationToken)
        {
            var orders = await context.Set<WZ_OrderCycleBase>()
                .AsNoTracking()
                .Where(p => p.CapacityScheduleDate.HasValue && p.OrderQty.HasValue && p.OrderQty.Value > 0)
                .Select(p => new OrderCapacityCandidate
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

            var thresholdMap = await LoadCapacityThresholdMapAsync(context, cancellationToken);
            var outputs = await context.Set<WZ_ProductionOutput>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var outputThresholdMap = new Dictionary<(string Cat, string Line), decimal>();
            var dateQuantityMap = new Dictionary<(string Cat, string Line, DateTime Date), decimal>();

            foreach (var output in outputs)
            {
                var cat = NormalizeCapacityText(output.ValveCategory);
                var line = NormalizeCapacityText(output.ProductionLine);
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

            var orderGroups = new Dictionary<(string Cat, string Line, DateTime Date), List<OrderCapacityCandidate>>();
            foreach (var order in orders)
            {
                if (!order.CapacityScheduleDate.HasValue)
                {
                    continue;
                }

                var cat = NormalizeCapacityText(order.ValveCategory);
                var line = ResolveCapacityLine(order);
                if (string.IsNullOrWhiteSpace(cat) || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var key = (cat, line, order.CapacityScheduleDate.Value.Date);
                if (!orderGroups.TryGetValue(key, out var group))
                {
                    group = new List<OrderCapacityCandidate>();
                    orderGroups[key] = group;
                }

                group.Add(order);
            }

            var overThresholdIds = new List<int>();
            foreach (var group in orderGroups)
            {
                var key = group.Key;
                var threshold = ResolveCapacityThreshold(thresholdMap, outputThresholdMap, key.Cat, key.Line, null);
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

            await context.Set<WZ_OrderCycleBase>()
                .Where(p => p.CapacityScheduleDateOverThreshold)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.CapacityScheduleDateOverThreshold, false), cancellationToken);

            for (var index = 0; index < overThresholdIds.Count; index += 1000)
            {
                var batchIds = overThresholdIds.Skip(index).Take(1000).ToList();
                await context.Set<WZ_OrderCycleBase>()
                    .Where(p => batchIds.Contains(p.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.CapacityScheduleDateOverThreshold, true), cancellationToken);
            }

            return overThresholdIds.Count;
        }

        /// <summary>
        /// 调用 Python 阀门规则服务批量计算周期及排产信息
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>成功回填的行数</returns>
        public ValveRuleTaskProgress CreateValveRuleTaskProgress(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                taskId = Guid.NewGuid().ToString("N");
            }

            var now = DateTime.Now;
            var progress = new ValveRuleTaskProgress
            {
                TaskId = taskId,
                Status = "running",
                Stage = "等待开始",
                Message = "智能体优化任务已创建",
                StartedAt = now,
                UpdatedAt = now
            };

            ValveRuleTaskProgressStore[taskId] = progress;
            return CloneValveRuleTaskProgress(progress);
        }

        public ValveRuleTaskProgress GetValveRuleTaskProgress(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return null;
            }

            return ValveRuleTaskProgressStore.TryGetValue(taskId, out var progress)
                ? CloneValveRuleTaskProgress(progress)
                : null;
        }

        public ValveRuleTaskProgress MarkValveRuleTaskProgressFailed(string taskId, string message)
        {
            return PublishValveRuleTaskProgress(taskId, progress =>
            {
                progress.Status = "failed";
                progress.Stage = "执行失败";
                progress.Message = string.IsNullOrWhiteSpace(message) ? "智能体优化失败" : message;
                progress.Error = message;
                progress.FinishedAt = DateTime.Now;
                progress.Percent = progress.Percent > 0 ? progress.Percent : 100;
            });
        }

        public ValveRuleTaskProgress CreateInitializeSchedulingTaskProgress(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                taskId = Guid.NewGuid().ToString("N");
            }

            var now = DateTime.Now;
            var progress = new ValveRuleTaskProgress
            {
                TaskId = taskId,
                Status = "running",
                Stage = "等待开始",
                Message = "排产初始化任务已创建",
                Total = 6,
                Processed = 0,
                Percent = 1,
                StartedAt = now,
                UpdatedAt = now
            };

            ValveRuleTaskProgressStore[taskId] = progress;
            return CloneValveRuleTaskProgress(progress);
        }

        public ValveRuleTaskProgress GetInitializeSchedulingTaskProgress(string taskId)
        {
            return GetValveRuleTaskProgress(taskId);
        }

        public ValveRuleTaskProgress MarkInitializeSchedulingTaskProgressFailed(string taskId, string message)
        {
            return PublishValveRuleTaskProgress(taskId, progress =>
            {
                progress.Status = "failed";
                progress.Stage = "执行失败";
                progress.Message = string.IsNullOrWhiteSpace(message) ? "排产初始化失败" : message;
                progress.Error = message;
                progress.FinishedAt = DateTime.Now;
                progress.Percent = progress.Percent > 0 ? progress.Percent : 100;
            });
        }

        private static ValveRuleTaskProgress PublishValveRuleTaskProgress(string taskId, Action<ValveRuleTaskProgress> update)
        {
            if (string.IsNullOrWhiteSpace(taskId) || update == null)
            {
                return null;
            }

            var now = DateTime.Now;
            var progress = ValveRuleTaskProgressStore.AddOrUpdate(
                taskId,
                _ =>
                {
                    var created = new ValveRuleTaskProgress
                    {
                        TaskId = taskId,
                        Status = "running",
                        StartedAt = now
                    };
                    update(created);
                    NormalizeValveRuleTaskProgress(created, now);
                    return created;
                },
                (_, existing) =>
                {
                    var next = CloneValveRuleTaskProgress(existing);
                    update(next);
                    NormalizeValveRuleTaskProgress(next, now);
                    return next;
                });

            return CloneValveRuleTaskProgress(progress);
        }

        private static void NormalizeValveRuleTaskProgress(ValveRuleTaskProgress progress, DateTime updatedAt)
        {
            progress.UpdatedAt = updatedAt;
            progress.LogFiles ??= new List<string>();
            if (progress.Total > 0)
            {
                progress.Processed = Math.Max(0, Math.Min(progress.Processed, progress.Total));
                progress.Percent = Math.Max(progress.Percent, (int)Math.Round(progress.Processed * 100D / progress.Total));
            }

            if (string.Equals(progress.Status, "success", StringComparison.OrdinalIgnoreCase)
                || string.Equals(progress.Status, "failed", StringComparison.OrdinalIgnoreCase))
            {
                progress.Percent = 100;
                progress.FinishedAt ??= updatedAt;
            }
            else
            {
                progress.Percent = Math.Max(0, Math.Min(progress.Percent, 99));
            }
        }

        private static ValveRuleTaskProgress CloneValveRuleTaskProgress(ValveRuleTaskProgress source)
        {
            if (source == null)
            {
                return null;
            }

            return new ValveRuleTaskProgress
            {
                TaskId = source.TaskId,
                Status = source.Status,
                Stage = source.Stage,
                Message = source.Message,
                Total = source.Total,
                Processed = source.Processed,
                Succeeded = source.Succeeded,
                Failed = source.Failed,
                Updated = source.Updated,
                BatchCount = source.BatchCount,
                TotalBatchCount = source.TotalBatchCount,
                Percent = source.Percent,
                LogFiles = source.LogFiles == null ? new List<string>() : new List<string>(source.LogFiles),
                Error = source.Error,
                StartedAt = source.StartedAt,
                UpdatedAt = source.UpdatedAt,
                FinishedAt = source.FinishedAt
            };
        }

        public async Task<ValveRuleBatchSummary> BatchCallValveRuleServiceAsync(
            CancellationToken cancellationToken = default,
            string progressTaskId = null)
        {
            if (_httpClientFactory == null)
            {
                throw new InvalidOperationException("HttpClientFactory 未注册，无法调用规则服务");
            }

            if (!string.IsNullOrWhiteSpace(progressTaskId))
            {
                PublishValveRuleTaskProgress(progressTaskId, progress =>
                {
                    progress.Status = "running";
                    progress.Stage = "读取待优化数据";
                    progress.Message = "正在查询可优化订单";
                    progress.Percent = 1;
                });
            }

            var context = _repository.DbContext;
            var entities = await context.Set<WZ_OrderCycleBase>()
                .Where(p => p.OrderApprovedDate.HasValue && p.ReplyDeliveryDate.HasValue && p.RequestedDeliveryDate.HasValue)
                .OrderBy(p => p.Id)
                .ToListAsync(cancellationToken);

            if (entities.Count == 0)
            {
                PublishValveRuleTaskProgress(progressTaskId, progress =>
                {
                    progress.Status = "success";
                    progress.Stage = "无待优化数据";
                    progress.Message = "没有需要提交到智能体优化的数据";
                    progress.Total = 0;
                    progress.Processed = 0;
                    progress.Percent = 100;
                });
                return new ValveRuleBatchSummary();
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            const string url = "http://10.11.10.101:8000/batch_infer?debug_trace=false";

            const int batchSize = 200;
            var summary = new ValveRuleBatchSummary
            {
                Total = entities.Count
            };
            var totalBatchCount = (int)Math.Ceiling(entities.Count / (double)batchSize);

            PublishValveRuleTaskProgress(progressTaskId, progress =>
            {
                progress.Status = "running";
                progress.Stage = "准备调用规则服务";
                progress.Message = $"共 {entities.Count} 条，预计 {totalBatchCount} 批";
                progress.Total = entities.Count;
                progress.TotalBatchCount = totalBatchCount;
                progress.Percent = 2;
            });

            for (var i = 0; i < entities.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batchEntities = entities.Skip(i).Take(batchSize).ToList();
                if (batchEntities.Count == 0)
                {
                    continue;
                }

                summary.BatchCount++;
                PublishValveRuleTaskProgress(progressTaskId, progress =>
                {
                    progress.Status = "running";
                    progress.Stage = "调用规则服务";
                    progress.Message = $"正在处理第 {summary.BatchCount}/{totalBatchCount} 批";
                    progress.Total = summary.Total;
                    progress.Processed = Math.Min(i, summary.Total);
                    progress.Succeeded = summary.Succeeded;
                    progress.Failed = summary.Failed;
                    progress.Updated = summary.Updated;
                    progress.BatchCount = summary.BatchCount;
                    progress.TotalBatchCount = totalBatchCount;
                    progress.LogFiles = new List<string>(summary.LogFiles);
                });

                var entityMap = batchEntities.ToDictionary(p => p.Id, p => p);
                var updatedEntities = new List<WZ_OrderCycleBase>();
                var successIds = new HashSet<int>();

                async Task SendValveRuleBatchAsync(List<WZ_OrderCycleBase> requestEntities, ValveRuleProductTextMode productTextMode)
                {
                    if (requestEntities == null || requestEntities.Count == 0)
                    {
                        return;
                    }

                    var requestPayload = BuildValveRuleRequests(requestEntities, productTextMode);
                    var json = JsonConvert.SerializeObject(requestPayload);

                    using var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };

                    using var response = await client.SendAsync(request, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    var batchResponse = JsonConvert.DeserializeObject<ValveRuleBatchResponse>(body) ?? new ValveRuleBatchResponse();

                    if (!string.IsNullOrWhiteSpace(batchResponse.LogFile))
                    {
                        summary.LogFiles.Add(batchResponse.LogFile);
                    }

                    if (batchResponse.Results == null || batchResponse.Results.Count == 0)
                    {
                        return;
                    }

                    foreach (var item in batchResponse.Results)
                    {
                        var matchedId = TryParseId(item?.Id) ?? TryParseId(item?.Result?.Id);
                        if (!matchedId.HasValue)
                        {
                            continue;
                        }

                        if (!entityMap.TryGetValue(matchedId.Value, out var entity))
                        {
                            continue;
                        }

                        if (item.Success != true || item.Result == null)
                        {
                            continue;
                        }

                        entity.FixedCycleDays = item.Result.FixedCycleDays;
                        entity.ProductionLine = item.Result.ProductionLine;
                        entity.StandardDeliveryDate = TryParseDate(item.Result.StandardDeliveryDate);
                        entity.ScheduleDate = TryParseDate(item.Result.ScheduleDate);

                        if (successIds.Add(entity.Id))
                        {
                            updatedEntities.Add(entity);
                        }
                    }
                }

                await SendValveRuleBatchAsync(batchEntities, ValveRuleProductTextMode.SpecModelFirst);
                var fallbackEntities = batchEntities
                    .Where(p => !successIds.Contains(p.Id)
                        && !string.IsNullOrWhiteSpace(p.ProductName)
                        && !string.Equals(
                            ResolveValveRuleProductText(p, ValveRuleProductTextMode.SpecModelFirst),
                            ResolveValveRuleProductText(p, ValveRuleProductTextMode.ProductNameFallback),
                            StringComparison.Ordinal))
                    .ToList();
                if (fallbackEntities.Count > 0)
                {
                    await SendValveRuleBatchAsync(fallbackEntities, ValveRuleProductTextMode.ProductNameFallback);
                }

                summary.Succeeded += successIds.Count;
                summary.Failed += Math.Max(0, batchEntities.Count - successIds.Count);

                if (updatedEntities.Count > 0)
                {
                    context.UpdateRange(updatedEntities);
                    await context.SaveChangesAsync(cancellationToken);
                    summary.Updated += updatedEntities.Count;
                }

                PublishValveRuleTaskProgress(progressTaskId, progress =>
                {
                    progress.Status = "running";
                    progress.Stage = "批次完成";
                    progress.Message = $"第 {summary.BatchCount}/{totalBatchCount} 批完成";
                    progress.Total = summary.Total;
                    progress.Processed = Math.Min(i + batchEntities.Count, summary.Total);
                    progress.Succeeded = summary.Succeeded;
                    progress.Failed = summary.Failed;
                    progress.Updated = summary.Updated;
                    progress.BatchCount = summary.BatchCount;
                    progress.TotalBatchCount = totalBatchCount;
                    progress.LogFiles = new List<string>(summary.LogFiles);
                });
            }

            summary.Failed = Math.Max(summary.Failed, summary.Total - summary.Succeeded);

            PublishValveRuleTaskProgress(progressTaskId, progress =>
            {
                progress.Status = "success";
                progress.Stage = "执行完成";
                progress.Message = $"智能体优化完成，成功 {summary.Succeeded} 条，更新 {summary.Updated} 条";
                progress.Total = summary.Total;
                progress.Processed = summary.Total;
                progress.Succeeded = summary.Succeeded;
                progress.Failed = summary.Failed;
                progress.Updated = summary.Updated;
                progress.BatchCount = summary.BatchCount;
                progress.TotalBatchCount = totalBatchCount;
                progress.LogFiles = new List<string>(summary.LogFiles);
                progress.Percent = 100;
            });

            return summary;
        }

        private enum ValveRuleProductTextMode
        {
            SpecModelFirst,
            ProductNameFallback
        }

        private static string ResolveValveRuleProductText(WZ_OrderCycleBase item, ValveRuleProductTextMode mode)
        {
            if (item == null)
            {
                return string.Empty;
            }

            var specModel = NormalizeCapacityText(item.GUI_GE_XING_HAO);
            var productName = NormalizeCapacityText(item.ProductName);
            return mode == ValveRuleProductTextMode.SpecModelFirst
                ? (!string.IsNullOrWhiteSpace(specModel) ? specModel : productName)
                : (!string.IsNullOrWhiteSpace(productName) ? productName : specModel);
        }

        private static List<ValveRuleRequest> BuildValveRuleRequests(List<WZ_OrderCycleBase> items, ValveRuleProductTextMode productTextMode)
        {
            var requests = new List<ValveRuleRequest>(items.Count);
            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                requests.Add(new ValveRuleRequest
                {
                    Id = item.Id.ToString(),
                    SourceScheduleDate = item.ScheduleDate,
                    NeedSchedulePrediction = !item.ScheduleDate.HasValue,
                    OrderApprovedDate = item.OrderApprovedDate,
                    ReplyDeliveryDate = item.ReplyDeliveryDate,
                    RequestedDeliveryDate = item.RequestedDeliveryDate,
                    BodyMaterial = item.BodyMaterial,
                    InnerMaterial = item.InnerMaterial,
                    FlangeConnection = item.FlangeConnection,
                    BonnetForm = item.BonnetForm,
                    FlowCharacteristic = item.FlowCharacteristic,
                    Actuator = item.Actuator,
                    OutsourcedValveBody = item.OutsourcedValveBody,
                    ValveCategory1 = item.ValveCategory1,
                    ValveCategory = item.ValveCategory,
                    SealFaceForm = item.SealFaceForm,
                    SpecialProduct = item.SpecialProduct,
                    PurchaseFlag = item.PurchaseFlag,
                    ProductName = ResolveValveRuleProductText(item, productTextMode),
                    ProductNameRaw = item.ProductName,
                    SpecModelRaw = item.GUI_GE_XING_HAO,
                    NominalDiameter = item.NominalDiameter,
                    NominalPressure = item.NominalPressure
                });
            }

            return requests;
        }

        public async Task<int> FillValveCategoryByRuleAsync(int batchSize = 1000)
        {
            if (batchSize <= 0)
            {
                batchSize = 1000;
            }

            var context = _repository?.DbContext
                ?? throw new InvalidOperationException("订单周期仓储未正确初始化");

            var updatedTotal = 0;
            var lastId = 0;

            while (true)
            {
                var batch = await context.Set<WZ_OrderCycleBase>()
                    .AsNoTracking()
                    .Where(p => p.Id > lastId
                        && (p.MaterialCode == null || !p.MaterialCode.Trim().ToUpper().StartsWith("BJ"))
                        && (((p.ValveCategory == null || p.ValveCategory == string.Empty)
                                && ((p.ProductName != null && p.ProductName != string.Empty)
                                    || (p.GUI_GE_XING_HAO != null && p.GUI_GE_XING_HAO != string.Empty)))
                            || (p.GUI_GE_XING_HAO != null && p.GUI_GE_XING_HAO.Trim().ToUpper().StartsWith("VFR"))
                            || (p.ProductName != null && p.ProductName.Trim().ToUpper().StartsWith("VFR"))))
                    .OrderBy(p => p.Id)
                    .Select(p => new
                    {
                        p.Id,
                        p.GUI_GE_XING_HAO,
                        p.ProductName,
                        p.ValveCategory
                    })
                    .Take(batchSize)
                    .ToListAsync();

                if (batch.Count == 0)
                {
                    break;
                }

                var entitiesToUpdate = new List<WZ_OrderCycleBase>();
                foreach (var item in batch)
                {
                    var result = ValveCategoryRuleJudge.TryJudgeBySpecOrProduct(item.GUI_GE_XING_HAO, item.ProductName);
                    if (!result.HasValue)
                    {
                        continue;
                    }

                    if (string.Equals(item.ValveCategory, result.Value.Category, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    entitiesToUpdate.Add(new WZ_OrderCycleBase
                    {
                        Id = item.Id,
                        ValveCategory = result.Value.Category
                    });
                }

                if (entitiesToUpdate.Count > 0)
                {
                    foreach (var entity in entitiesToUpdate)
                    {
                        context.Attach(entity);
                        context.Entry(entity).Property(p => p.ValveCategory).IsModified = true;
                    }

                    updatedTotal += entitiesToUpdate.Count;
                    await context.SaveChangesAsync();

                    foreach (var entity in entitiesToUpdate)
                    {
                        context.Entry(entity).State = EntityState.Detached;
                    }
                }

                lastId = batch[batch.Count - 1].Id;
            }

            return updatedTotal;
        }

        public async Task<AssignedProductionLineBatchSummary> BatchAssignProductionLineByRuleAsync(int batchSize = 1000, CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0)
            {
                batchSize = 1000;
            }

            var context = _repository?.DbContext
                ?? throw new InvalidOperationException("订单周期仓储未正确初始化");

            var summary = new AssignedProductionLineBatchSummary
            {
                SqlPreview = BuildAssignedProductionLineSql()
            };

            var lastId = 0;

            while (true)
            {
                var batch = await context.Set<WZ_OrderCycleBase>()
                    .AsNoTracking()
                    .Where(p => p.Id > lastId
                        && p.ProductionLine != null
                        && p.ProductionLine != string.Empty)
                    .OrderBy(p => p.Id)
                    .Select(p => new
                    {
                        p.Id,
                        p.ProductionLine,
                        p.ValveCategory,
                        p.NominalDiameter,
                        p.AssignedProductionLine
                    })
                    .Take(batchSize)
                    .ToListAsync(cancellationToken);

                if (batch.Count == 0)
                {
                    break;
                }

                summary.Total += batch.Count;

                var entitiesToUpdate = new List<WZ_OrderCycleBase>();
                var updateIds = new List<int>();

                foreach (var item in batch)
                {
                    try
                    {
                        var newValue = CalcAssignedProductionLine(item.ProductionLine, item.ValveCategory, item.NominalDiameter);
                        if (string.IsNullOrEmpty(newValue))
                        {
                            newValue = NormalizeCapacityText(item.ProductionLine);
                        }

                        if (string.IsNullOrEmpty(newValue)
                            || string.Equals(newValue, item.AssignedProductionLine, StringComparison.Ordinal))
                        {
                            summary.Skipped++;
                            continue;
                        }

                        entitiesToUpdate.Add(new WZ_OrderCycleBase
                        {
                            Id = item.Id,
                            AssignedProductionLine = newValue
                        });
                        updateIds.Add(item.Id);
                    }
                    catch
                    {
                        summary.Failed++;
                        summary.FailedIds.Add(item.Id);
                    }
                }

                if (entitiesToUpdate.Count > 0)
                {
                    using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                    try
                    {
                        foreach (var entity in entitiesToUpdate)
                        {
                            context.Attach(entity);
                            context.Entry(entity).Property(p => p.AssignedProductionLine).IsModified = true;
                        }

                        await context.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        summary.Updated += entitiesToUpdate.Count;
                    }
                    catch
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        summary.Failed += entitiesToUpdate.Count;
                        summary.FailedIds.AddRange(updateIds);
                    }
                    finally
                    {
                        foreach (var entity in entitiesToUpdate)
                        {
                            context.Entry(entity).State = EntityState.Detached;
                        }
                    }
                }

                lastId = batch[batch.Count - 1].Id;
            }

            return summary;
        }

        public string GetAssignedProductionLineSql()
        {
            return BuildAssignedProductionLineSql();
        }

        public static string CalcAssignedProductionLine(string productionLine, string valveCategory, string nominalDiameter)
        {
            var normalizedProductionLine = productionLine?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedProductionLine))
            {
                return null;
            }

            var normalizedValveCategory = valveCategory?.Trim();
            var normalizedNominalDiameter = nominalDiameter?.Trim();

            if (normalizedProductionLine.StartsWith("旋转", StringComparison.Ordinal)
                && string.Equals(normalizedValveCategory, "蝶阀", StringComparison.Ordinal))
            {
                if (ButterflyGroup1.Contains(normalizedNominalDiameter))
                {
                    return "蝶阀1";
                }

                if (ButterflyGroup2.Contains(normalizedNominalDiameter))
                {
                    return "蝶阀2";
                }

                if (ButterflyGroup3.Contains(normalizedNominalDiameter))
                {
                    return "蝶阀3";
                }

                return "蝶阀4";
            }

            if (!string.IsNullOrWhiteSpace(normalizedValveCategory)
                && normalizedValveCategory.StartsWith("直通", StringComparison.Ordinal))
            {
                return normalizedProductionLine;
            }

            if (SoftSealProductionLines.Contains(normalizedProductionLine)
                && string.Equals(normalizedValveCategory, "软密封球阀", StringComparison.Ordinal))
            {
                return normalizedProductionLine + "A";
            }

            return null;
        }

        private static string BuildAssignedProductionLineSql()
        {
            return @"UPDATE dbo.WZ_OrderCycleBase
SET AssignedProductionLine = CASE
    WHEN ProductionLine IS NOT NULL AND LTRIM(RTRIM(ProductionLine)) <> N''
         AND ProductionLine LIKE N'旋转%' AND ValveCategory = N'蝶阀' THEN
        CASE
            WHEN LTRIM(RTRIM(NominalDiameter)) IN (N'DN50', N'DN65', N'DN80', N'DN100', N'DN125', N'DN150') THEN N'蝶阀1'
            WHEN LTRIM(RTRIM(NominalDiameter)) IN (N'DN200', N'DN250', N'DN300', N'DN100') THEN N'蝶阀2'
            WHEN LTRIM(RTRIM(NominalDiameter)) IN (N'DN350', N'DN400', N'DN450', N'DN500', N'DN600') THEN N'蝶阀3'
            ELSE N'蝶阀4'
        END
    WHEN ProductionLine IS NOT NULL AND LTRIM(RTRIM(ProductionLine)) <> N''
         AND ValveCategory LIKE N'直通%' THEN LTRIM(RTRIM(ProductionLine))
    WHEN ProductionLine IS NOT NULL AND LTRIM(RTRIM(ProductionLine)) <> N''
         AND ProductionLine IN (N'旋转1', N'旋转2', N'旋转3', N'旋转4', N'旋转5')
         AND ValveCategory = N'软密封球阀' THEN ProductionLine + N'A'
    ELSE AssignedProductionLine
END
WHERE ProductionLine IS NOT NULL AND LTRIM(RTRIM(ProductionLine)) <> N'';";
        }

        private static readonly HashSet<string> ButterflyGroup1 = new HashSet<string>(StringComparer.Ordinal)
        {
            "DN50", "DN65", "DN80", "DN100", "DN125", "DN150"
        };

        private static readonly HashSet<string> ButterflyGroup2 = new HashSet<string>(StringComparer.Ordinal)
        {
            "DN200", "DN250", "DN300", "DN100"
        };

        private static readonly HashSet<string> ButterflyGroup3 = new HashSet<string>(StringComparer.Ordinal)
        {
            "DN350", "DN400", "DN450", "DN500", "DN600"
        };

        private static readonly HashSet<string> SoftSealProductionLines = new HashSet<string>(StringComparer.Ordinal)
        {
            "旋转1", "旋转2", "旋转3", "旋转4", "旋转5"
        };

        private static int? TryParseId(string id)
        {
            return int.TryParse(id, out var value) ? value : null;
        }

        private static DateTime? TryParseDate(string date)
        {
            return DateTime.TryParse(date, out var value) ? value : null;
        }

        private static string NormalizeLine(string value)
        {
            return NormalizeCapacityText(value);
        }

        private static string NormalizeCapacityText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().Normalize(NormalizationForm.FormKC);
        }

        private static string ResolveCapacityLine(OrderCapacityCandidate order)
        {
            var assignedLine = NormalizeCapacityText(order.AssignedProductionLine);
            if (!string.IsNullOrWhiteSpace(assignedLine))
            {
                return assignedLine;
            }

            var ruleLine = NormalizeCapacityText(CalcAssignedProductionLine(
                order.ProductionLine,
                order.ValveCategory,
                order.NominalDiameter));
            if (!string.IsNullOrWhiteSpace(ruleLine))
            {
                return ruleLine;
            }

            return NormalizeCapacityText(order.ProductionLine);
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

        private static decimal? ResolveCapacityThreshold(
            IReadOnlyDictionary<(string Cat, string Line), decimal> thresholds,
            IReadOnlyDictionary<(string Cat, string Line), decimal> outputThresholds,
            string valveCategory,
            string productionLine,
            decimal? fallback)
        {
            if (thresholds != null
                && thresholds.TryGetValue((NormalizeCapacityText(valveCategory), NormalizeCapacityText(productionLine)), out var threshold))
            {
                return threshold;
            }

            if (outputThresholds != null
                && outputThresholds.TryGetValue((NormalizeCapacityText(valveCategory), NormalizeCapacityText(productionLine)), out var outputThreshold))
            {
                return outputThreshold;
            }

            return fallback;
        }

        private static async Task<Dictionary<(string Cat, string Line), decimal>> LoadCapacityThresholdMapAsync(
            DbContext context,
            CancellationToken cancellationToken)
        {
            try
            {
                var rows = await context.Set<WZ_ProductionOutputThreshold>()
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
                    var cat = NormalizeCapacityText(row.ValveCategory);
                    var line = NormalizeCapacityText(row.ProductionLine);
                    if (string.IsNullOrWhiteSpace(cat) || string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    result[(cat, line)] = row.CurrentThreshold;
                }

                return result;
            }
            catch
            {
                return new Dictionary<(string Cat, string Line), decimal>();
            }
        }

        private static int FindFirstDateIndex(List<DateTime> dates, DateTime targetDate)
        {
            if (dates == null || dates.Count == 0)
            {
                return -1;
            }

            var index = dates.BinarySearch(targetDate);
            if (index >= 0)
            {
                return index;
            }

            index = ~index;
            return index < dates.Count ? index : -1;
        }

        private const decimal DailyReserveCapacityRatio = 1.2M;
        private const int LongDeliveryGapThresholdDays = 35;
        private const int ReplyLeadWindowMinDays = 25;
        private const int ReplyLeadWindowMaxDays = 35;

        // 2026 年业务日历依据国务院办公厅关于 2026 年部分节假日安排的通知维护；
        // 后续年份发布后，在这里补充节假日与调休上班日，保证排产查找和标色口径一致。
        private static readonly HashSet<DateTime> CapacityStatutoryHolidayDates2026 = new HashSet<DateTime>
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

        private static readonly HashSet<DateTime> CapacityMakeupWorkdayDates2026 = new HashSet<DateTime>
        {
            new DateTime(2026, 1, 4),
            new DateTime(2026, 2, 14),
            new DateTime(2026, 2, 28),
            new DateTime(2026, 5, 9),
            new DateTime(2026, 9, 20),
            new DateTime(2026, 10, 10)
        };

        private static readonly HashSet<DateTime> CapacitySundayRestDates2026 = new HashSet<DateTime>(
            Enumerable.Range(0, 365)
                .Select(dayOffset => new DateTime(2026, 1, 1).AddDays(dayOffset))
                .Where(date => date.DayOfWeek == DayOfWeek.Sunday
                    && !CapacityMakeupWorkdayDates2026.Contains(date.Date)
                    && !CapacityStatutoryHolidayDates2026.Contains(date.Date)));

        private static CapacityScheduleDecision ResolveCapacityScheduleDate(
            OrderCapacityCandidate order,
            List<DateTime> dates,
            HashSet<DateTime> knownDates,
            Dictionary<(string Cat, string Line, DateTime Date), CapacityBucket> capacityMap,
            IReadOnlyDictionary<(string Cat, string Line), decimal> thresholdMap,
            IReadOnlyDictionary<(string Cat, string Line), decimal> outputThresholdMap,
            string cat,
            string line,
            DateTime targetDate)
        {
            var quantity = order.OrderQty.GetValueOrDefault();
            if (quantity <= 0)
            {
                return CapacityScheduleDecision.Fail(CapacityFailureReasons.MissingProductionOutput);
            }

            if (!TryGetCapacityWindow(order, out var startDate, out var endDate))
            {
                return CapacityScheduleDecision.Fail(CapacityFailureReasons.MissingProductionOutput);
            }

            EnsureCapacityWindow(dates, knownDates, capacityMap, thresholdMap, outputThresholdMap, cat, line, startDate, endDate);
            if (dates.Count == 0)
            {
                return CapacityScheduleDecision.Fail(CapacityFailureReasons.MissingProductionOutput);
            }

            var preferredAttempt = TryResolveForwardAssignableDate(
                dates,
                capacityMap,
                cat,
                line,
                startDate,
                endDate,
                targetDate,
                quantity,
                DailyReserveCapacityRatio);
            if (preferredAttempt.CapacityDate.HasValue)
            {
                return preferredAttempt;
            }

            var overflowStartDate = ResolveCapacitySearchStartDate(startDate, endDate, targetDate);
            var balancedAttempt = overflowStartDate.HasValue
                ? TryAssignBalancedOverflowDate(
                    dates,
                    capacityMap,
                    cat,
                    line,
                    overflowStartDate.Value,
                    endDate,
                    quantity)
                : CapacityAssignAttempt.Fail(CapacityFailureReasons.OutOfCapacityWindow);
            if (balancedAttempt.CapacityDate.HasValue)
            {
                return CapacityScheduleDecision.Success(balancedAttempt.CapacityDate.Value, CapacityScheduleMode.BalancedOverflow);
            }

            return CapacityScheduleDecision.Fail(PickFailureReason(
                preferredAttempt.FailureReason,
                balancedAttempt.FailureReason));
        }

        private static bool TryGetCapacityWindow(OrderCapacityCandidate order, out DateTime startDate, out DateTime endDate)
        {
            startDate = order.StandardDeliveryDate?.Date ?? DateTime.MinValue;
            endDate = order.ReplyDeliveryDate?.Date ?? DateTime.MinValue;
            if (!order.StandardDeliveryDate.HasValue || !order.ReplyDeliveryDate.HasValue)
            {
                return false;
            }

            if (endDate < startDate)
            {
                endDate = startDate;
                return true;
            }

            if ((endDate - startDate).TotalDays > LongDeliveryGapThresholdDays)
            {
                var targetStartDate = endDate.AddDays(-ReplyLeadWindowMaxDays);
                var targetEndDate = endDate.AddDays(-ReplyLeadWindowMinDays);

                if (targetEndDate >= startDate)
                {
                    startDate = targetStartDate > startDate ? targetStartDate : startDate;
                    endDate = targetEndDate;
                }
            }

            return true;
        }

        private static void EnsureCapacityWindow(
            List<DateTime> dates,
            HashSet<DateTime> knownDates,
            Dictionary<(string Cat, string Line, DateTime Date), CapacityBucket> capacityMap,
            IReadOnlyDictionary<(string Cat, string Line), decimal> thresholdMap,
            IReadOnlyDictionary<(string Cat, string Line), decimal> outputThresholdMap,
            string cat,
            string line,
            DateTime startDate,
            DateTime endDate)
        {
            var threshold = ResolveCapacityThreshold(thresholdMap, outputThresholdMap, cat, line, null);
            if (!threshold.HasValue)
            {
                return;
            }

            knownDates ??= new HashSet<DateTime>(dates);
            var addedDate = false;
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var key = (cat, line, date);
                if (!capacityMap.ContainsKey(key))
                {
                    capacityMap[key] = new CapacityBucket
                    {
                        Quantity = 0M,
                        Threshold = threshold
                    };
                }

                if (knownDates.Add(date))
                {
                    dates.Add(date);
                    addedDate = true;
                }
            }

            if (addedDate)
            {
                dates.Sort();
            }
        }

        private static DateTime? ResolveCapacitySearchStartDate(DateTime startDate, DateTime endDate, DateTime targetDate)
        {
            var searchStartDate = targetDate.Date > startDate.Date ? targetDate.Date : startDate.Date;
            return searchStartDate <= endDate.Date ? searchStartDate : null;
        }

        // 排产优化规则：
        // 1. 原排产日优先；若插入前负载未满 100%，允许当前订单一次性推到 120% 以内。
        // 2. 原排产日放不下时，先在取值范围内按 100% 查找：业务工作日 -> 周六休息日 -> 周日休息日 -> 法定节假日。
        // 3. 范围内 100% 都放不下后，再按 120% 查找，顺序仍是业务工作日 -> 周六休息日 -> 周日休息日 -> 法定节假日。
        // 4. 业务工作日包含调休上班日，排除法定节假日；预留 120% 只能用于“插入前未满 100%”的日期。
        // 5. 只有所有候选日期插入后都会超过 120% 时，才进入均摊超载日期。
        private static CapacityScheduleDecision TryResolveForwardAssignableDate(
            List<DateTime> dates,
            Dictionary<(string Cat, string Line, DateTime Date), CapacityBucket> capacityMap,
            string cat,
            string line,
            DateTime startDate,
            DateTime endDate,
            DateTime targetDate,
            decimal quantity,
            decimal reserveCapacityRatio)
        {
            var searchStartDate = ResolveCapacitySearchStartDate(startDate, endDate, targetDate);
            if (!searchStartDate.HasValue)
            {
                return CapacityScheduleDecision.Fail(CapacityFailureReasons.OutOfCapacityWindow);
            }

            var index = FindFirstDateIndex(dates, searchStartDate.Value);
            if (index < 0)
            {
                return CapacityScheduleDecision.Fail(CapacityFailureReasons.MissingProductionOutput);
            }

            var failureReason = CapacityFailureReasons.ThresholdExceeded;
            var preferredDateAttempt = TryAssignPreferredCapacityDate(
                capacityMap,
                cat,
                line,
                targetDate,
                searchStartDate.Value,
                endDate,
                quantity,
                reserveCapacityRatio);
            if (preferredDateAttempt.CapacityDate.HasValue)
            {
                return preferredDateAttempt;
            }

            failureReason = PickFailureReason(failureReason, preferredDateAttempt.FailureReason);

            var workdayNormalAttempt = TryFindForwardNormalCapacityDate(
                dates,
                capacityMap,
                cat,
                line,
                index,
                endDate,
                quantity,
                targetDate,
                IsCapacityWorkday,
                CapacityScheduleMode.DeliveryAdjusted);
            if (workdayNormalAttempt.CapacityDate.HasValue)
            {
                return workdayNormalAttempt;
            }

            var saturdayNormalAttempt = TryFindForwardNormalCapacityDate(
                dates,
                capacityMap,
                cat,
                line,
                index,
                endDate,
                quantity,
                targetDate,
                IsCapacitySaturdayRestDay,
                CapacityScheduleMode.SaturdayReserve);
            if (saturdayNormalAttempt.CapacityDate.HasValue)
            {
                return saturdayNormalAttempt;
            }

            var sundayNormalAttempt = TryFindForwardNormalCapacityDate(
                dates,
                capacityMap,
                cat,
                line,
                index,
                endDate,
                quantity,
                targetDate,
                IsCapacitySundayRestDay,
                CapacityScheduleMode.SundayReserve);
            if (sundayNormalAttempt.CapacityDate.HasValue)
            {
                return sundayNormalAttempt;
            }

            var holidayNormalAttempt = TryFindForwardNormalCapacityDate(
                dates,
                capacityMap,
                cat,
                line,
                index,
                endDate,
                quantity,
                targetDate,
                IsCapacityStatutoryHoliday,
                CapacityScheduleMode.SundayReserve);
            if (holidayNormalAttempt.CapacityDate.HasValue)
            {
                return holidayNormalAttempt;
            }

            var workdayReserveAttempt = TryFindForwardReserveCapacityDate(
                dates,
                capacityMap,
                cat,
                line,
                index,
                endDate,
                quantity,
                reserveCapacityRatio,
                targetDate,
                IsCapacityWorkday,
                CapacityScheduleMode.DailyReserve);
            if (workdayReserveAttempt.CapacityDate.HasValue)
            {
                return workdayReserveAttempt;
            }

            var saturdayReserveAttempt = TryFindForwardReserveCapacityDate(
                dates,
                capacityMap,
                cat,
                line,
                index,
                endDate,
                quantity,
                reserveCapacityRatio,
                targetDate,
                IsCapacitySaturdayRestDay,
                CapacityScheduleMode.SaturdayReserve);
            if (saturdayReserveAttempt.CapacityDate.HasValue)
            {
                return saturdayReserveAttempt;
            }

            var sundayReserveAttempt = TryFindForwardReserveCapacityDate(
                dates,
                capacityMap,
                cat,
                line,
                index,
                endDate,
                quantity,
                reserveCapacityRatio,
                targetDate,
                IsCapacitySundayRestDay,
                CapacityScheduleMode.SundayReserve);
            if (sundayReserveAttempt.CapacityDate.HasValue)
            {
                return sundayReserveAttempt;
            }

            var holidayReserveAttempt = TryFindForwardReserveCapacityDate(
                dates,
                capacityMap,
                cat,
                line,
                index,
                endDate,
                quantity,
                reserveCapacityRatio,
                targetDate,
                IsCapacityStatutoryHoliday,
                CapacityScheduleMode.SundayReserve);
            if (holidayReserveAttempt.CapacityDate.HasValue)
            {
                return holidayReserveAttempt;
            }

            return CapacityScheduleDecision.Fail(PickFailureReason(
                failureReason,
                workdayNormalAttempt.FailureReason,
                saturdayNormalAttempt.FailureReason,
                sundayNormalAttempt.FailureReason,
                holidayNormalAttempt.FailureReason,
                workdayReserveAttempt.FailureReason,
                saturdayReserveAttempt.FailureReason,
                sundayReserveAttempt.FailureReason,
                holidayReserveAttempt.FailureReason));
        }

        private static CapacityScheduleDecision TryAssignPreferredCapacityDate(
            Dictionary<(string Cat, string Line, DateTime Date), CapacityBucket> capacityMap,
            string cat,
            string line,
            DateTime targetDate,
            DateTime searchStartDate,
            DateTime endDate,
            decimal quantity,
            decimal reserveCapacityRatio)
        {
            var date = targetDate.Date;
            if (date < searchStartDate.Date || date > endDate.Date)
            {
                return CapacityScheduleDecision.Fail(CapacityFailureReasons.OutOfCapacityWindow);
            }

            var normalAttempt = TryAssignCapacityDate(capacityMap, cat, line, date, quantity, 1M);
            if (normalAttempt.CapacityDate.HasValue)
            {
                var mode = IsCapacityWorkday(date)
                    ? CapacityScheduleMode.NormalCapacity
                    : ResolveReserveCapacityMode(date);
                return CapacityScheduleDecision.Success(date, mode);
            }

            var reserveAttempt = TryAssignReserveCapacityDate(capacityMap, cat, line, date, quantity, reserveCapacityRatio);
            return reserveAttempt.CapacityDate.HasValue
                ? CapacityScheduleDecision.Success(date, ResolveReserveCapacityMode(date))
                : CapacityScheduleDecision.Fail(PickFailureReason(normalAttempt.FailureReason, reserveAttempt.FailureReason));
        }

        private static CapacityScheduleDecision TryFindForwardNormalCapacityDate(
            List<DateTime> dates,
            Dictionary<(string Cat, string Line, DateTime Date), CapacityBucket> capacityMap,
            string cat,
            string line,
            int startIndex,
            DateTime endDate,
            decimal quantity,
            DateTime targetDate,
            Func<DateTime, bool> datePredicate,
            CapacityScheduleMode mode)
        {
            var failureReason = CapacityFailureReasons.ThresholdExceeded;
            for (var i = startIndex; i < dates.Count; i++)
            {
                var date = dates[i].Date;
                if (date > endDate.Date)
                {
                    break;
                }

                if (date == targetDate.Date || !datePredicate(date))
                {
                    continue;
                }

                var normalAttempt = TryAssignCapacityDate(capacityMap, cat, line, date, quantity, 1M);
                if (normalAttempt.CapacityDate.HasValue)
                {
                    return CapacityScheduleDecision.Success(date, mode);
                }

                failureReason = PickFailureReason(failureReason, normalAttempt.FailureReason);
            }

            return CapacityScheduleDecision.Fail(failureReason);
        }

        private static CapacityScheduleDecision TryFindForwardReserveCapacityDate(
            List<DateTime> dates,
            Dictionary<(string Cat, string Line, DateTime Date), CapacityBucket> capacityMap,
            string cat,
            string line,
            int startIndex,
            DateTime endDate,
            decimal quantity,
            decimal reserveCapacityRatio,
            DateTime targetDate,
            Func<DateTime, bool> datePredicate,
            CapacityScheduleMode mode)
        {
            var failureReason = CapacityFailureReasons.ThresholdExceeded;
            for (var i = startIndex; i < dates.Count; i++)
            {
                var date = dates[i].Date;
                if (date > endDate.Date)
                {
                    break;
                }

                if (date == targetDate.Date || !datePredicate(date))
                {
                    continue;
                }

                var reserveAttempt = TryAssignReserveCapacityDate(capacityMap, cat, line, date, quantity, reserveCapacityRatio);
                if (reserveAttempt.CapacityDate.HasValue)
                {
                    return CapacityScheduleDecision.Success(date, mode);
                }

                failureReason = PickFailureReason(failureReason, reserveAttempt.FailureReason);
            }

            return CapacityScheduleDecision.Fail(failureReason);
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

        private static CapacityScheduleMode ResolveReserveCapacityMode(DateTime date)
        {
            if (IsCapacityWorkday(date))
            {
                return CapacityScheduleMode.DailyReserve;
            }

            if (IsCapacitySaturdayRestDay(date))
            {
                return CapacityScheduleMode.SaturdayReserve;
            }

            return CapacityScheduleMode.SundayReserve;
        }

        private static CapacityAssignAttempt TryAssignCapacityDate(
            Dictionary<(string Cat, string Line, DateTime Date), CapacityBucket> capacityMap,
            string cat,
            string line,
            DateTime date,
            decimal quantity,
            decimal capacityRatio)
        {
            if (!capacityMap.TryGetValue((cat, line, date.Date), out var bucket))
            {
                return CapacityAssignAttempt.Fail(CapacityFailureReasons.MissingProductionOutput);
            }

            if (!bucket.Threshold.HasValue)
            {
                return CapacityAssignAttempt.Fail(CapacityFailureReasons.MissingThreshold);
            }

            var capacityLimit = bucket.Threshold.Value * capacityRatio;
            if (bucket.Quantity + quantity <= capacityLimit)
            {
                bucket.Quantity += quantity;
                return CapacityAssignAttempt.Success(date.Date);
            }

            return CapacityAssignAttempt.Fail(CapacityFailureReasons.ThresholdExceeded);
        }

        private static CapacityAssignAttempt TryAssignReserveCapacityDate(
            Dictionary<(string Cat, string Line, DateTime Date), CapacityBucket> capacityMap,
            string cat,
            string line,
            DateTime date,
            decimal quantity,
            decimal capacityRatio)
        {
            if (!capacityMap.TryGetValue((cat, line, date.Date), out var bucket))
            {
                return CapacityAssignAttempt.Fail(CapacityFailureReasons.MissingProductionOutput);
            }

            if (!bucket.Threshold.HasValue || bucket.Threshold.Value <= 0)
            {
                return CapacityAssignAttempt.Fail(CapacityFailureReasons.MissingThreshold);
            }

            if (bucket.Quantity >= bucket.Threshold.Value)
            {
                return CapacityAssignAttempt.Fail(CapacityFailureReasons.BaseCapacityReached);
            }

            var capacityLimit = bucket.Threshold.Value * capacityRatio;
            if (bucket.Quantity + quantity <= capacityLimit)
            {
                bucket.Quantity += quantity;
                return CapacityAssignAttempt.Success(date.Date);
            }

            return CapacityAssignAttempt.Fail(CapacityFailureReasons.ThresholdExceeded);
        }

        private static CapacityAssignAttempt TryAssignBalancedOverflowDate(
            List<DateTime> dates,
            Dictionary<(string Cat, string Line, DateTime Date), CapacityBucket> capacityMap,
            string cat,
            string line,
            DateTime startDate,
            DateTime endDate,
            decimal quantity)
        {
            CapacityBucket? selectedBucket = null;
            DateTime? selectedDate = null;
            decimal? selectedLoadRate = null;
            var failureReason = CapacityFailureReasons.MissingProductionOutput;

            var index = FindFirstDateIndex(dates, startDate.Date);
            if (index < 0)
            {
                return CapacityAssignAttempt.Fail(failureReason);
            }

            for (var i = index; i < dates.Count; i++)
            {
                var date = dates[i].Date;
                if (date > endDate.Date)
                {
                    break;
                }

                if (!capacityMap.TryGetValue((cat, line, date), out var bucket))
                {
                    continue;
                }

                if (!bucket.Threshold.HasValue || bucket.Threshold.Value <= 0)
                {
                    failureReason = CapacityFailureReasons.MissingThreshold;
                    continue;
                }

                var projectedLoadRate = (bucket.Quantity + quantity) / bucket.Threshold.Value;
                if (projectedLoadRate <= DailyReserveCapacityRatio)
                {
                    failureReason = PickFailureReason(
                        failureReason,
                        bucket.Quantity >= bucket.Threshold.Value
                            ? CapacityFailureReasons.BaseCapacityReached
                            : CapacityFailureReasons.ThresholdExceeded);
                    continue;
                }

                var isLaterTie = selectedDate.HasValue
                    && selectedLoadRate.HasValue
                    && projectedLoadRate == selectedLoadRate.Value
                    && date > selectedDate.Value;

                if (!selectedLoadRate.HasValue
                    || projectedLoadRate < selectedLoadRate.Value
                    || isLaterTie)
                {
                    selectedLoadRate = projectedLoadRate;
                    selectedBucket = bucket;
                    selectedDate = date;
                }
            }

            if (selectedDate.HasValue && selectedBucket != null)
            {
                selectedBucket.Quantity += quantity;
                return CapacityAssignAttempt.Success(selectedDate.Value);
            }

            return CapacityAssignAttempt.Fail(failureReason);
        }

        private static string PickFailureReason(params string[] reasons)
        {
            if (reasons != null && reasons.Any(p => string.Equals(p, CapacityFailureReasons.MissingThreshold, StringComparison.Ordinal)))
            {
                return CapacityFailureReasons.MissingThreshold;
            }

            if (reasons != null && reasons.Any(p => string.Equals(p, CapacityFailureReasons.BaseCapacityReached, StringComparison.Ordinal)))
            {
                return CapacityFailureReasons.BaseCapacityReached;
            }

            if (reasons != null && reasons.Any(p => string.Equals(p, CapacityFailureReasons.ThresholdExceeded, StringComparison.Ordinal)))
            {
                return CapacityFailureReasons.ThresholdExceeded;
            }

            return CapacityFailureReasons.MissingProductionOutput;
        }

        private sealed class OrderCapacityCandidate
        {
            public int Id { get; set; }

            public DateTime? ScheduleDate { get; set; }

            public DateTime? CapacityScheduleDate { get; set; }

            public decimal? OrderQty { get; set; }

            public string ValveCategory { get; set; } = string.Empty;

            public string AssignedProductionLine { get; set; } = string.Empty;

            public string ProductionLine { get; set; } = string.Empty;

            public string NominalDiameter { get; set; } = string.Empty;

            public DateTime? StandardDeliveryDate { get; set; }

            public DateTime? ReplyDeliveryDate { get; set; }

            public DateTime? RequestedDeliveryDate { get; set; }
        }

        private sealed class CapacityBucket
        {
            public decimal Quantity { get; set; }

            public decimal? Threshold { get; set; }
        }

        private enum CapacityScheduleMode
        {
            NormalCapacity,
            DeliveryAdjusted,
            DailyReserve,
            SaturdayReserve,
            SundayReserve,
            BalancedOverflow
        }

        private static class CapacityFailureReasons
        {
            public const string MissingProductionOutput = "missing_production_output";
            public const string MissingThreshold = "missing_threshold";
            public const string BaseCapacityReached = "base_capacity_reached";
            public const string ThresholdExceeded = "threshold_exceeded";
            public const string OutOfCapacityWindow = "out_of_capacity_window";
        }

        private sealed class CapacityAssignAttempt
        {
            public DateTime? CapacityDate { get; set; }

            public string FailureReason { get; set; } = string.Empty;

            public static CapacityAssignAttempt Success(DateTime capacityDate)
            {
                return new CapacityAssignAttempt
                {
                    CapacityDate = capacityDate.Date
                };
            }

            public static CapacityAssignAttempt Fail(string failureReason)
            {
                return new CapacityAssignAttempt
                {
                    FailureReason = failureReason
                };
            }
        }

        private sealed class CapacityScheduleDecision
        {
            public DateTime? CapacityDate { get; set; }

            public CapacityScheduleMode Mode { get; set; }

            public string FailureReason { get; set; } = string.Empty;

            public static CapacityScheduleDecision Success(DateTime capacityDate, CapacityScheduleMode mode)
            {
                return new CapacityScheduleDecision
                {
                    CapacityDate = capacityDate.Date,
                    Mode = mode
                };
            }

            public static CapacityScheduleDecision Fail(string failureReason)
            {
                return new CapacityScheduleDecision
                {
                    FailureReason = failureReason
                };
            }
        }

        private sealed class OrderCycleBaseExportColumn
        {
            public string Field { get; set; } = string.Empty;

            public string Title { get; set; } = string.Empty;

            public Type Type { get; set; } = typeof(string);
        }

        private sealed class SchedulePredictionReviewExportColumn
        {
            public string Field { get; set; } = string.Empty;

            public string Title { get; set; } = string.Empty;

            public Type Type { get; set; } = typeof(string);
        }

        private sealed class ValveRuleRequest
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("SourceScheduleDate")]
            public DateTime? SourceScheduleDate { get; set; }

            [JsonProperty("NeedSchedulePrediction")]
            public bool NeedSchedulePrediction { get; set; }

            [JsonProperty("OrderApprovedDate")]
            public DateTime? OrderApprovedDate { get; set; }

            [JsonProperty("ReplyDeliveryDate")]
            public DateTime? ReplyDeliveryDate { get; set; }

            [JsonProperty("RequestedDeliveryDate")]
            public DateTime? RequestedDeliveryDate { get; set; }

            [JsonProperty("fa_ti_cai_zhi")]
            public string BodyMaterial { get; set; }

            [JsonProperty("nei_jian_cai_zhi")]
            public string InnerMaterial { get; set; }

            [JsonProperty("fa_lan_lian_jie")]
            public string FlangeConnection { get; set; }

            [JsonProperty("shang_gai_xing_shi")]
            public string BonnetForm { get; set; }

            [JsonProperty("liu_liang_te_xing")]
            public string FlowCharacteristic { get; set; }

            [JsonProperty("zhi_xing_ji_gou")]
            public string Actuator { get; set; }

            [JsonProperty("wai_gou_fa_ti")]
            public string OutsourcedValveBody { get; set; }

            [JsonProperty("fa_men_da_lei")]
            public string ValveCategory1 { get; set; }

            [JsonProperty("fa_men_lei_bie")]
            public string ValveCategory { get; set; }

            [JsonProperty("mi_feng_mian_xing_shi")]
            public string SealFaceForm { get; set; }

            [JsonProperty("te_pin")]
            public string SpecialProduct { get; set; }

            [JsonProperty("wai_gou_biao_zhi")]
            public string PurchaseFlag { get; set; }

            [JsonProperty("chan_pin_ming_cheng")]
            public string ProductName { get; set; }

            [JsonProperty("_product_name_raw")]
            public string ProductNameRaw { get; set; }

            [JsonProperty("_spec_model_raw")]
            public string SpecModelRaw { get; set; }

            [JsonProperty("gong_cheng_tong_jing")]
            public string NominalDiameter { get; set; }

            [JsonProperty("gong_cheng_ya_li")]
            public string NominalPressure { get; set; }
        }

        private sealed class ValveRuleResult
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("sheng_chan_xian")]
            public string ProductionLine { get; set; }

            [JsonProperty("gu_ding_zhou_qi")]
            public int? FixedCycleDays { get; set; }

            [JsonProperty("StandardDeliveryDate")]
            public string StandardDeliveryDate { get; set; }

            [JsonProperty("ScheduleDate")]
            public string ScheduleDate { get; set; }
        }

        private sealed class ValveRuleResponseItem
        {
            [JsonProperty("index")]
            public int Index { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("result")]
            public ValveRuleResult Result { get; set; }
        }

        private sealed class ValveRuleBatchResponse
        {
            [JsonProperty("total")]
            public int Total { get; set; }

            [JsonProperty("results")]
            public List<ValveRuleResponseItem> Results { get; set; }

            [JsonProperty("log_file")]
            public string LogFile { get; set; }
        }

        private static void MapFields(ERP_OrderTracking orderTracking, OCP_Material materialInfo, WZ_OrderCycleBase target)
        {
            target.SalesOrderNo = orderTracking.FBILLNO;
            target.PlanTrackingNo = NormalizePlanTrackingNo(orderTracking.FMTONO);

            target.OrderApprovedDate = orderTracking.FAPPROVEDATE;
            target.ReplyDeliveryDate = orderTracking.F_BLN_HFJHRQ;
            target.RequestedDeliveryDate = orderTracking.F_ORA_DATETIME;
            target.MaterialCode = orderTracking.FNUMBER;
            target.OrderQty = orderTracking.FQTY;
            target.FENTRYID = orderTracking.FENTRYID;

            if (materialInfo != null)
            {
                target.InnerMaterial = materialInfo.TrimMaterial;
                target.FlangeConnection = materialInfo.FlangeConnection;
                target.BonnetForm = materialInfo.BonnetForm;
                target.FlowCharacteristic = materialInfo.FlowCharacteristic;
                target.Actuator = materialInfo.ActuatorModel;
                target.BodyMaterial = materialInfo.BodyMaterial;
                target.SealFaceForm = materialInfo.FlangeSealType;
                target.ProductName = materialInfo.ProductModel;
                target.GUI_GE_XING_HAO = materialInfo.SpecModel;
                target.NominalDiameter = materialInfo.NominalDiameter;
                target.NominalPressure = materialInfo.NominalPressure;
            }
        }

        private static string NormalizePlanTrackingNo(string planTrackingNo)
        {
            return string.IsNullOrWhiteSpace(planTrackingNo) ? null : planTrackingNo;
        }
    }
}
