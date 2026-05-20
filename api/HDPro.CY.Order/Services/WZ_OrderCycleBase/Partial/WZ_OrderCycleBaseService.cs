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

namespace HDPro.CY.Order.Services
{
    public partial class WZ_OrderCycleBaseService
    {
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
                    AssignedProductionLine = p.AssignedProductionLine != null && p.AssignedProductionLine != string.Empty
                        ? p.AssignedProductionLine
                        : p.ProductionLine,
                    StandardDeliveryDate = p.StandardDeliveryDate,
                    ReplyDeliveryDate = p.ReplyDeliveryDate,
                    RequestedDeliveryDate = p.RequestedDeliveryDate
                })
                .ToListAsync(cancellationToken);

            var summary = new CapacityScheduleSummary
            {
                Total = orders.Count
            };

            if (orders.Count == 0)
            {
                return summary;
            }

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
            foreach (var item in categoryLineDates)
            {
                var dates = item.Value.ToList();
                dates.Sort();
                capacityDateList[item.Key] = dates;
            }

            var updates = new List<WZ_OrderCycleBase>();

            foreach (var order in orders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!order.ScheduleDate.HasValue || order.OrderQty.GetValueOrDefault() <= 0)
                {
                    summary.Skipped++;
                    continue;
                }

                var cat = NormalizeCapacityText(order.ValveCategory);
                var line = NormalizeCapacityText(order.AssignedProductionLine);
                if (string.IsNullOrWhiteSpace(cat) || string.IsNullOrWhiteSpace(line))
                {
                    summary.MissingProductionOutput++;
                    summary.Failed++;
                    continue;
                }

                if (!capacityDateList.TryGetValue((cat, line), out var dates))
                {
                    dates = new List<DateTime>();
                    capacityDateList[(cat, line)] = dates;
                }

                var targetDate = order.ScheduleDate.Value.Date;
                var decision = ResolveCapacityScheduleDate(order, dates, capacityMap, thresholdMap, outputThresholdMap, cat, line, targetDate);
                if (!decision.CapacityDate.HasValue)
                {
                    if (string.Equals(decision.FailureReason, CapacityFailureReasons.MissingThreshold, StringComparison.Ordinal))
                    {
                        summary.MissingThreshold++;
                    }
                    else
                    {
                        summary.MissingProductionOutput++;
                    }

                    summary.Failed++;
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
                    case CapacityScheduleMode.BalancedOverflow:
                        summary.BalancedOverflowCount++;
                        break;
                }
            }

            if (updates.Count > 0)
            {
                foreach (var entity in updates)
                {
                    context.Attach(entity);
                    context.Entry(entity).Property(p => p.CapacityScheduleDate).IsModified = true;
                }

                await context.SaveChangesAsync(cancellationToken);

                foreach (var entity in updates)
                {
                    context.Entry(entity).State = EntityState.Detached;
                }
            }

            return summary;
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
            const string url = "http://10.11.10.101:8000/batch_infer?debug_trace=false";

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
            }

            summary.Failed = Math.Max(summary.Failed, summary.Total - summary.Succeeded);

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

        private static CapacityScheduleDecision ResolveCapacityScheduleDate(
            OrderCapacityCandidate order,
            List<DateTime> dates,
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

            EnsureCapacityWindow(dates, capacityMap, thresholdMap, outputThresholdMap, cat, line, startDate, endDate);
            if (dates.Count == 0)
            {
                return CapacityScheduleDecision.Fail(CapacityFailureReasons.MissingProductionOutput);
            }

            var targetAttempt = targetDate >= startDate && targetDate <= endDate
                ? TryAssignCapacityDate(capacityMap, cat, line, targetDate, quantity, 1M)
                : CapacityAssignAttempt.Fail(CapacityFailureReasons.OutOfCapacityWindow);
            if (targetAttempt.CapacityDate.HasValue)
            {
                return CapacityScheduleDecision.Success(targetAttempt.CapacityDate.Value, CapacityScheduleMode.NormalCapacity);
            }

            var adjustedAttempt = TryFindFirstAssignableDate(
                dates,
                capacityMap,
                cat,
                line,
                startDate,
                endDate,
                quantity,
                1M,
                _ => true);
            if (adjustedAttempt.CapacityDate.HasValue)
            {
                return CapacityScheduleDecision.Success(adjustedAttempt.CapacityDate.Value, CapacityScheduleMode.DeliveryAdjusted);
            }

            var dailyReserveAttempt = TryFindFirstAssignableDate(
                dates,
                capacityMap,
                cat,
                line,
                startDate,
                endDate,
                quantity,
                DailyReserveCapacityRatio,
                date => date.DayOfWeek != DayOfWeek.Saturday);
            if (dailyReserveAttempt.CapacityDate.HasValue)
            {
                return CapacityScheduleDecision.Success(dailyReserveAttempt.CapacityDate.Value, CapacityScheduleMode.DailyReserve);
            }

            var saturdayReserveAttempt = TryFindFirstAssignableDate(
                dates,
                capacityMap,
                cat,
                line,
                startDate,
                endDate,
                quantity,
                DailyReserveCapacityRatio,
                date => date.DayOfWeek == DayOfWeek.Saturday);
            if (saturdayReserveAttempt.CapacityDate.HasValue)
            {
                return CapacityScheduleDecision.Success(saturdayReserveAttempt.CapacityDate.Value, CapacityScheduleMode.SaturdayReserve);
            }

            var balancedAttempt = TryAssignBalancedOverflowDate(
                dates,
                capacityMap,
                cat,
                line,
                startDate,
                endDate,
                quantity);
            if (balancedAttempt.CapacityDate.HasValue)
            {
                return CapacityScheduleDecision.Success(balancedAttempt.CapacityDate.Value, CapacityScheduleMode.BalancedOverflow);
            }

            return CapacityScheduleDecision.Fail(PickFailureReason(
                targetAttempt.FailureReason,
                adjustedAttempt.FailureReason,
                dailyReserveAttempt.FailureReason,
                saturdayReserveAttempt.FailureReason,
                balancedAttempt.FailureReason));
        }

        private static bool TryGetCapacityWindow(OrderCapacityCandidate order, out DateTime startDate, out DateTime endDate)
        {
            startDate = order.StandardDeliveryDate?.Date ?? DateTime.MinValue;
            endDate = order.ReplyDeliveryDate?.Date ?? DateTime.MinValue;
            return order.StandardDeliveryDate.HasValue
                && order.ReplyDeliveryDate.HasValue
                && endDate >= startDate;
        }

        private static void EnsureCapacityWindow(
            List<DateTime> dates,
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

            var knownDates = new HashSet<DateTime>(dates);
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
                }
            }

            dates.Sort();
        }

        private static CapacityAssignAttempt TryFindFirstAssignableDate(
            List<DateTime> dates,
            Dictionary<(string Cat, string Line, DateTime Date), CapacityBucket> capacityMap,
            string cat,
            string line,
            DateTime startDate,
            DateTime endDate,
            decimal quantity,
            decimal capacityRatio,
            Func<DateTime, bool> datePredicate)
        {
            var index = FindFirstDateIndex(dates, startDate.Date);
            if (index < 0)
            {
                return CapacityAssignAttempt.Fail(CapacityFailureReasons.MissingProductionOutput);
            }

            var failureReason = CapacityFailureReasons.ThresholdExceeded;
            for (var i = index; i < dates.Count; i++)
            {
                var date = dates[i].Date;
                if (date > endDate.Date)
                {
                    break;
                }

                if (!datePredicate(date))
                {
                    continue;
                }

                var attempt = TryAssignCapacityDate(capacityMap, cat, line, date, quantity, capacityRatio);
                if (attempt.CapacityDate.HasValue)
                {
                    return attempt;
                }

                failureReason = PickFailureReason(failureReason, attempt.FailureReason);
            }

            return CapacityAssignAttempt.Fail(failureReason);
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
                var isEarlierTie = selectedDate.HasValue
                    && selectedLoadRate.HasValue
                    && projectedLoadRate == selectedLoadRate.Value
                    && date < selectedDate.Value;

                if (!selectedLoadRate.HasValue
                    || projectedLoadRate < selectedLoadRate.Value
                    || isEarlierTie)
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

            public decimal? OrderQty { get; set; }

            public string ValveCategory { get; set; } = string.Empty;

            public string AssignedProductionLine { get; set; } = string.Empty;

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
            BalancedOverflow
        }

        private static class CapacityFailureReasons
        {
            public const string MissingProductionOutput = "missing_production_output";
            public const string MissingThreshold = "missing_threshold";
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
