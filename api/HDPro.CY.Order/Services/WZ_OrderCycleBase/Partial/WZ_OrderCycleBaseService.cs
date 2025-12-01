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

            var orderTrackingList = await orderTrackingContext.Set<OCP_OrderTracking>()
                .AsNoTracking()
                .Where(p => p.PrdScheduleDate == null)
                .ToListAsync(cancellationToken);

            if (orderTrackingList.Count == 0)
            {
                return 0;
            }

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
                return 0;
            }

            await orderCycleContext.SaveChangesAsync(cancellationToken);

            return updatedCount + toInsert.Count;
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

            if (materialInfo != null)
            {
                target.InnerMaterial = materialInfo.InnerMaterial;
                target.FlangeConnection = materialInfo.FlangeConnection;
                target.BonnetForm = materialInfo.BonnetForm;
                target.FlowCharacteristic = materialInfo.FlowCharacteristic;
                target.Actuator = materialInfo.ActuatorModel;
                target.SealFaceForm = materialInfo.SealFaceForm;
                target.ProductName = materialInfo.ProductModel;
                target.NominalDiameter = materialInfo.NominalDiameter;
                target.NominalPressure = materialInfo.NominalPressure;
            }
        }
    }
}