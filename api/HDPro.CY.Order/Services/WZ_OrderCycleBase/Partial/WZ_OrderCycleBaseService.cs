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
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using HDPro.CY.Order.Models.WZProductionOutputDtos;
using System.Diagnostics;

namespace HDPro.CY.Order.Services
{
    public partial class WZ_OrderCycleBaseService
    {
        private readonly IWZ_OrderCycleBaseRepository _repository;//访问数据库
        private readonly IOCP_OrderTrackingRepository _orderTrackingRepository;
        private readonly IOCP_MaterialRepository _materialRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        [ActivatorUtilitiesConstructor]
        public WZ_OrderCycleBaseService(
            IWZ_OrderCycleBaseRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IOCP_OrderTrackingRepository orderTrackingRepository,
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

        private static string NormalizeStr(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        public async Task<CapacityScheduleResultDto> CalculateCapacityScheduleAsync(
            CapacityScheduleRequestDto request,
            CancellationToken cancellationToken = default)
        {
            request ??= new CapacityScheduleRequestDto();

            var stopwatch = Stopwatch.StartNew();
            var context = _repository?.DbContext
                ?? throw new InvalidOperationException("订单周期仓储未正确初始化");

            var startDate = request.FromDate?.Date ?? request.StartDate?.Date;
            var endDate = request.ToDate?.Date ?? request.EndDate?.Date;
            var lineFilter = NormalizeStr(request.ProductionLine);
            var productionLines = request.ProductionLines?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeStr)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (!string.IsNullOrWhiteSpace(lineFilter))
            {
                productionLines.Add(lineFilter);
            }

            productionLines = productionLines
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var ordersQuery = context.Set<WZ_OrderCycleBase>()
                .Where(o => o.OrderQty != null && o.OrderQty > 0);

            if (productionLines.Count > 0)
            {
                ordersQuery = ordersQuery.Where(o => productionLines.Contains(o.AssignedProductionLine));
            }

            if (!request.RecalcAll && startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.ScheduleDate == null || o.ScheduleDate >= startDate.Value);
            }

            if (!request.RecalcAll && endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.ScheduleDate == null || o.ScheduleDate <= endDate.Value);
            }

            var orders = await ordersQuery
                .OrderBy(o => o.ScheduleDate)
                .ThenBy(o => o.Id)
                .ToListAsync(cancellationToken);

            if (orders.Count == 0)
                return new CapacityScheduleResultDto { BatchId = Guid.NewGuid(), Elapsed = stopwatch.Elapsed };

            var minDate = startDate ?? orders.Min(o => o.ScheduleDate ?? DateTime.Today).Date;
            var maxDate = endDate ?? orders.Max(o => o.ScheduleDate ?? DateTime.Today).Date;
            var outputQuery = context.Set<WZ_ProductionOutput>()
                .AsNoTracking()
                .Where(o => o.ProductionDate >= minDate && o.ProductionDate <= maxDate);

            if (productionLines.Count > 0)
            {
                outputQuery = outputQuery.Where(o => productionLines.Contains(o.ProductionLine));
            }

            var outputs = await outputQuery.ToListAsync(cancellationToken);
            var outputLookup = outputs.ToDictionary(
                o => (Date: o.ProductionDate.Date, Cat: NormalizeStr(o.ValveCategory), Line: NormalizeStr(o.ProductionLine)),
                o => o);

            var currentQuantities = outputs.ToDictionary(
                o => (Date: o.ProductionDate.Date, Cat: NormalizeStr(o.ValveCategory), Line: NormalizeStr(o.ProductionLine)),
                o => o.Quantity);

            var failedOrders = new List<FailedOrderDto>();
            var touchedKeys = new HashSet<(DateTime Date, string Cat, string Line)>();
            var scheduledOrders = 0;

            foreach (var order in orders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var orderQty = order.OrderQty ?? 0m;
                var scheduleDate = order.ScheduleDate?.Date;
                var assignedLine = NormalizeStr(order.AssignedProductionLine);
                var valveCategory = NormalizeStr(order.ValveCategory);

                if (string.IsNullOrWhiteSpace(assignedLine))
                {
                    failedOrders.Add(new FailedOrderDto
                    {
                        Id = order.Id,
                        ProductionLine = assignedLine,
                        OrderQty = order.OrderQty,
                        ScheduleDate = order.ScheduleDate,
                        Reason = "指派产线为空"
                    });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(valveCategory))
                {
                    failedOrders.Add(new FailedOrderDto
                    {
                        Id = order.Id,
                        ProductionLine = assignedLine,
                        OrderQty = order.OrderQty,
                        ScheduleDate = order.ScheduleDate,
                        Reason = "阀门类别为空"
                    });
                    continue;
                }

                if (!scheduleDate.HasValue)
                {
                    failedOrders.Add(new FailedOrderDto
                    {
                        Id = order.Id,
                        ProductionLine = assignedLine,
                        OrderQty = order.OrderQty,
                        ScheduleDate = order.ScheduleDate,
                        Reason = "排产日期为空"
                    });
                    continue;
                }

                if (orderQty <= 0)
                {
                    if (!request.DryRun)
                    {
                        order.CapacityScheduleDate = scheduleDate.Value.Date;
                    }
                    scheduledOrders++;
                    continue;
                }

                var remaining = orderQty;
                var currentDate = scheduleDate.Value.Date;

                while (remaining > 0)
                {
                    var key = (Date: currentDate, Cat: valveCategory, Line: assignedLine);
                    currentQuantities.TryGetValue(key, out var currentQty);
                    touchedKeys.Add(key);

                    var threshold = 0m;
                    if (outputLookup.TryGetValue(key, out var output) && output.CurrentThreshold.HasValue)
                    {
                        threshold = output.CurrentThreshold.Value;
                    }

                    var effectiveThreshold = threshold > 0 ? threshold : decimal.MaxValue;
                    var available = effectiveThreshold - currentQty;

                    if (available <= 0)
                    {
                        currentDate = currentDate.AddDays(1);
                        continue;
                    }

                    var appliedQty = request.AllowSplit ? Math.Min(remaining, available) : remaining;
                    if (!request.AllowSplit && appliedQty > available)
                    {
                        currentDate = currentDate.AddDays(1);
                        continue;
                    }

                    remaining -= appliedQty;
                    currentQty += appliedQty;
                    currentQuantities[key] = currentQty;

                    if (remaining > 0)
                    {
                        currentDate = currentDate.AddDays(1);
                    }
                }

                if (!request.DryRun)
                {
                    order.CapacityScheduleDate = currentDate;
                }
                scheduledOrders++;
            }

            if (!request.DryRun)
            {
                await context.SaveChangesAsync(cancellationToken);
            }

            var dailyLoads = new List<DailyLoadDto>();
            var allKeys = outputLookup.Keys.Concat(currentQuantities.Keys).Distinct().ToList();
            foreach (var key in allKeys)
            {
                currentQuantities.TryGetValue(key, out var quantity);
                outputLookup.TryGetValue(key, out var output);
                var threshold = output?.CurrentThreshold ?? 0m;
                dailyLoads.Add(new DailyLoadDto
                {
                    ProductionLine = key.Line,
                    ProductionDate = key.Date,
                    Quantity = quantity,
                    Threshold = threshold,
                    IsOverThreshold = threshold > 0 && quantity > threshold
                });
            }

            stopwatch.Stop();
            return new CapacityScheduleResultDto
            {
                BatchId = Guid.NewGuid(),
                TotalOrders = orders.Count,
                ScheduledOrders = scheduledOrders,
                FailedOrders = failedOrders.Count,
                FailedOrdersDetail = failedOrders,
                DailyLoads = dailyLoads
                    .OrderBy(x => x.ProductionDate)
                    .ThenBy(x => x.ProductionLine)
                    .ToList(),
                Elapsed = stopwatch.Elapsed
            };
        }

        /// <summary>
        /// 从订单跟踪表同步数据到订单周期基础表
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>同步的总行数</returns>
        public async Task<int> SyncFromOrderTrackingAsync(CancellationToken cancellationToken = default)
        {
            var orderTrackingContext = _orderTrackingRepository?.DbContext
                ?? throw new InvalidOperationException("订单跟踪仓储未正确初始化");
            var materialContext = _materialRepository?.DbContext
                ?? throw new InvalidOperationException("物料仓储未正确初始化");
            var orderCycleContext = _repository?.DbContext
                ?? throw new InvalidOperationException("订单周期仓储未正确初始化");

            /*.Where(p => p.PrdScheduleDate == null)*/
            var orderTrackingList = await orderTrackingContext.Set<OCP_OrderTracking>()
                .AsNoTracking()
                .Where(p => p.BillStatus == "正常" && p.MtoNoStatus != "冻结" && p.MtoNoStatus != "终止")
                .ToListAsync(cancellationToken);

            var materialNumbers = orderTrackingList.Where(p => !string.IsNullOrWhiteSpace(p.MaterialNumber))
                .Select(p => p.MaterialNumber)
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

            var salesOrderNos = orderTrackingList.Where(p => !string.IsNullOrWhiteSpace(p.SOBillNo))
                .Select(p => p.SOBillNo)
                .Distinct()
                .ToList();

            var planTrackingNos = orderTrackingList.Where(p => !string.IsNullOrWhiteSpace(p.MtoNo))
                .Select(p => p.MtoNo)
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

                    if (orderTracking == null || string.IsNullOrWhiteSpace(orderTracking.SOBillNo) || string.IsNullOrWhiteSpace(orderTracking.MtoNo))
                    {
                        continue;
                    }

                    materialDict.TryGetValue(orderTracking.MaterialNumber ?? string.Empty, out var materialInfo);

                    var key = $"{orderTracking.SOBillNo}__{orderTracking.MtoNo}";

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
                    .Select(p => new WZ_PreProductionOutput
                    {
                        ProductionDate = p.ScheduleDate,
                        CapacityScheduleDate = p.CapacityScheduleDate,
                        ValveCategory = p.ValveCategory,
                        ProductionLine = p.AssignedProductionLine,
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
        /// 调用 Python 阀门规则服务批量计算周期及排产信息
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>成功回填的行数</returns>
        public async Task<ValveRuleBatchSummary> BatchCallValveRuleServiceAsync(CancellationToken cancellationToken = default)
        {
            if (_httpClientFactory == null)
            {
                throw new InvalidOperationException("HttpClientFactory 未注册，无法调用规则服务");
            }

            var context = _repository.DbContext;
            var entities = await context.Set<WZ_OrderCycleBase>()
                .Where(p => p.OrderApprovedDate.HasValue && p.ReplyDeliveryDate.HasValue && p.RequestedDeliveryDate.HasValue)
                .OrderBy(p => p.Id)
                .ToListAsync(cancellationToken);

            if (entities.Count == 0)
            {
                return new ValveRuleBatchSummary();
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            const string url = "http://10.11.10.101:8000/batch_infer";

            const int batchSize = 200;
            var summary = new ValveRuleBatchSummary
            {
                Total = entities.Count
            };

            for (var i = 0; i < entities.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batchEntities = entities.Skip(i).Take(batchSize).ToList();
                if (batchEntities.Count == 0)
                {
                    continue;
                }

                summary.BatchCount++;

                var requestPayload = BuildValveRuleRequests(batchEntities);
                var json = JsonConvert.SerializeObject(requestPayload);

                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                using var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                var batchResponse = JsonConvert.DeserializeObject<ValveRuleBatchResponse>(body) ?? new ValveRuleBatchResponse();

                if (batchResponse.Results == null || batchResponse.Results.Count == 0)
                {
                    if (!string.IsNullOrWhiteSpace(batchResponse.LogFile))
                    {
                        summary.LogFiles.Add(batchResponse.LogFile);
                    }
                    continue;
                }

                var entityMap = batchEntities.ToDictionary(p => p.Id, p => p);
                var updatedEntities = new List<WZ_OrderCycleBase>();
                var updated = 0;
                var batchSuccess = 0;
                var batchFailed = 0;
                var processedIds = new HashSet<int>();

                foreach (var item in batchResponse.Results)
                {
                    var matchedId = TryParseId(item?.Id) ?? TryParseId(item?.Result?.Id);
                    if (!matchedId.HasValue)
                    {
                        batchFailed++;
                        continue;
                    }

                    processedIds.Add(matchedId.Value);

                    if (!entityMap.TryGetValue(matchedId.Value, out var entity))
                    {
                        batchFailed++;
                        continue;
                    }

                    if (item.Success != true || item.Result == null)
                    {
                        batchFailed++;
                        continue;
                    }

                    entity.FixedCycleDays = item.Result.FixedCycleDays;
                    entity.ProductionLine = item.Result.ProductionLine;
                    entity.StandardDeliveryDate = TryParseDate(item.Result.StandardDeliveryDate);
                    entity.ScheduleDate = TryParseDate(item.Result.ScheduleDate);

                    batchSuccess++;
                    updated++;
                    updatedEntities.Add(entity);
                }

                summary.Succeeded += batchSuccess;
                var missingCount = Math.Max(0, batchEntities.Count - processedIds.Count);
                summary.Failed += batchFailed + missingCount;

                if (updatedEntities.Count > 0)
                {
                    context.UpdateRange(updatedEntities);
                    await context.SaveChangesAsync(cancellationToken);
                    summary.Updated += updated;
                }

                if (!string.IsNullOrWhiteSpace(batchResponse.LogFile))
                {
                    summary.LogFiles.Add(batchResponse.LogFile);
                }
            }

            summary.Failed = Math.Max(summary.Failed, summary.Total - summary.Succeeded);

            return summary;
        }

        private static List<ValveRuleRequest> BuildValveRuleRequests(List<WZ_OrderCycleBase> items)
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
                    ValveCategory = item.ValveCategory,
                    SealFaceForm = item.SealFaceForm,
                    SpecialProduct = item.SpecialProduct,
                    PurchaseFlag = item.PurchaseFlag,
                    ProductName = item.ProductName,
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
                        && (p.ValveCategory == null || p.ValveCategory == string.Empty)
                        && p.ProductName != null
                        && p.ProductName != string.Empty)
                    .OrderBy(p => p.Id)
                    .Select(p => new
                    {
                        p.Id,
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
                    var result = ValveCategoryRuleJudge.TryJudge(item.ProductName);
                    if (!result.HasValue)
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

        private sealed class ValveRuleRequest
        {
            [JsonProperty("id")]
            public string Id { get; set; }

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

        private static void MapFields(OCP_OrderTracking orderTracking, OCP_Material materialInfo, WZ_OrderCycleBase target)
        {
            target.SalesOrderNo = orderTracking.SOBillNo;
            target.PlanTrackingNo = orderTracking.MtoNo;

            target.OrderApprovedDate = orderTracking.OrderAuditDate;
            target.ReplyDeliveryDate = orderTracking.ReplyDeliveryDate;
            target.RequestedDeliveryDate = orderTracking.DeliveryDate;
            target.MaterialCode = orderTracking.MaterialNumber;
            target.OrderQty = orderTracking.OrderQty;
            target.FENTRYID = orderTracking.SOEntryID;

            if (materialInfo != null)
            {
                target.InnerMaterial = materialInfo.TrimMaterial;
                target.FlangeConnection = materialInfo.FlangeConnection;
                target.BonnetForm = materialInfo.BonnetForm;
                target.FlowCharacteristic = materialInfo.FlowCharacteristic;
                target.Actuator = materialInfo.ActuatorModel;
                target.BodyMaterial = materialInfo.BodyMaterial;
                target.SealFaceForm = materialInfo.SealFaceForm;
                target.ProductName = materialInfo.ProductModel;
                target.NominalDiameter = materialInfo.NominalDiameter;
                target.NominalPressure = materialInfo.NominalPressure;
            }
        }
    }
}
