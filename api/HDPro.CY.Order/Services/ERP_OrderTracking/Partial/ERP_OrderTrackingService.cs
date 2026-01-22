/*
 *所有关于ERP_OrderTracking类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*ERP_OrderTrackingService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using HDPro.Core.Configuration;
using HDPro.CY.Order.Services.Common;
using System.Diagnostics;
using System.Globalization;

namespace HDPro.CY.Order.Services
{
    public partial class ERP_OrderTrackingService
    {
        private readonly IERP_OrderTrackingRepository _repository;//访问数据库
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ERP_OrderTrackingService> _logger;

        [ActivatorUtilitiesConstructor]
        public ERP_OrderTrackingService(
            IERP_OrderTrackingRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ILogger<ERP_OrderTrackingService> logger
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加ERP_OrderTracking特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加ERP_OrderTracking特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加ERP_OrderTracking特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(ERP_OrderTracking entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加ERP_OrderTracking特有的数据验证逻辑
            
            return response;
        }

        public async Task<WebResponseContent> SyncERPOrderTrackingAsync()
        {
            var response = new WebResponseContent();
            var syncStartTime = DateTime.Now;
            var operationType = "ERP订单跟踪";

            try
            {
                var apiPath = AppSetting.ESB?.Apis?.ERPOrderTrackingESBSyncService;
                var baseUrl = AppSetting.ESB?.BaseUrl ?? "http://10.11.0.101:8003";
                var timeoutMinutes = AppSetting.ESB?.DefaultTimeoutMinutes ?? 15;

                if (string.IsNullOrWhiteSpace(apiPath))
                {
                    return response.Error("ERP订单跟踪同步接口未配置，请检查ESB配置");
                }

                var normalizedBaseUrl = baseUrl?.TrimEnd('/');
                var normalizedApiPath = apiPath?.TrimStart('/');

                if (normalizedBaseUrl.EndsWith("/gateway", StringComparison.OrdinalIgnoreCase))
                {
                    if (normalizedApiPath.StartsWith("gateway/", StringComparison.OrdinalIgnoreCase))
                    {
                        normalizedApiPath = normalizedApiPath.Substring("gateway/".Length);
                    }
                }
                else if (!normalizedApiPath.StartsWith("gateway/", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedApiPath = $"gateway/{normalizedApiPath}";
                }

                var requestUrl = $"{normalizedBaseUrl}/{normalizedApiPath}";
                var requestJson = "{}";

                ApiLogHelper logHelper = null;
                int? statusCode = null;

                using var httpClient = _httpClientFactory.CreateClient(nameof(ERP_OrderTrackingService));
                httpClient.Timeout = TimeSpan.FromMinutes(timeoutMinutes);

                logHelper = ApiLogHelper.Start($"{operationType}同步接口", requestUrl, "POST", requestJson, _logger)
                    .WithRemark($"超时设置：{timeoutMinutes}分钟");

                _logger.LogInformation("开始{OperationType}同步，url={Url}", operationType, requestUrl);

                var stopwatch = Stopwatch.StartNew();
                using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var httpResponse = await httpClient.PostAsync(requestUrl, content);
                stopwatch.Stop();

                statusCode = (int)httpResponse.StatusCode;
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    "{OperationType}同步接口调用完成，status={StatusCode}，耗时={ElapsedSeconds:F2}秒",
                    operationType,
                    statusCode,
                    stopwatch.Elapsed.TotalSeconds);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorMessage = $"状态码：{httpResponse.StatusCode}，原因：{httpResponse.ReasonPhrase}";
                    _logger.LogWarning("{OperationType}同步接口调用失败，{ErrorMessage}", operationType, errorMessage);
                    if (logHelper != null)
                    {
                        await logHelper.WithDataCount(0).LogFailureAsync(errorMessage, statusCode, responseContent);
                    }
                    return response.Error($"{operationType}同步失败：{errorMessage}");
                }

                var dataList = JsonConvert.DeserializeObject<List<ERPOrderTrackingDto>>(responseContent)
                    ?? new List<ERPOrderTrackingDto>();
                var totalCount = dataList.Count;

                if (logHelper != null)
                {
                    await logHelper.WithDataCount(totalCount).LogSuccessAsync(statusCode, responseContent);
                }

                var toInsert = new List<ERP_OrderTracking>();
                var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var skippedCount = 0;
                var duplicateCount = 0;

                foreach (var item in dataList)
                {
                    if (item == null)
                    {
                        skippedCount++;
                        continue;
                    }

                    if (item.FENTRYID <= 0)
                    {
                        skippedCount++;
                        _logger.LogWarning("{OperationType}同步跳过无效FENTRYID记录", operationType);
                        continue;
                    }

                    var billNo = item.FBILLNO?.Trim();
                    var number = item.FNUMBER?.Trim();

                    if (string.IsNullOrWhiteSpace(billNo) || string.IsNullOrWhiteSpace(number))
                    {
                        skippedCount++;
                        _logger.LogWarning("{OperationType}同步跳过缺少关键字段记录，FENTRYID={EntryId}", operationType, item.FENTRYID);
                        continue;
                    }

                    if (!decimal.TryParse(item.FQTY?.ToString(), out var qty) || qty < 0)
                    {
                        skippedCount++;
                        _logger.LogWarning("{OperationType}同步跳过FQTY异常记录，FENTRYID={EntryId}，FQTY={Qty}", operationType, item.FENTRYID, item.FQTY);
                        continue;
                    }

                    var key = $"{billNo}|{item.FENTRYID}";
                    if (!seenKeys.Add(key))
                    {
                        duplicateCount++;
                        continue;
                    }

                    var mtoNo = item.FMTONO?.Trim();
                    if (string.IsNullOrWhiteSpace(mtoNo))
                    {
                        mtoNo = null;
                    }

                    toInsert.Add(new ERP_OrderTracking
                    {
                        FENTRYID = item.FENTRYID,
                        FBILLNO = billNo,
                        FNUMBER = number,
                        FQTY = qty,
                        FMTONO = mtoNo,
                        FAPPROVEDATE = ParseErpDate(item.FAPPROVEDATE),
                        F_ORA_DATETIME = ParseErpDate(item.F_ORA_DATETIME),
                        F_BLN_HFJHRQ = ParseErpDate(item.F_BLN_HFJHRQ),
                        created_at = DateTime.UtcNow,
                        updated_at = DateTime.UtcNow
                    });
                }

                var saveResult = await Task.Run(() => _repository.DbContextBeginTransaction(() =>
                {
                    var webResponse = new WebResponseContent();
                    try
                    {
                        _repository.DbContext.ChangeTracker.Clear();
                        _repository.DbContext.Database.ExecuteSqlRaw("DELETE FROM ERP_OrderTracking");

                        if (toInsert.Any())
                        {
                            _repository.AddRange(toInsert, false);
                            _repository.SaveChanges();
                        }

                        return webResponse.OK($"ERP订单跟踪数据清空并同步完成，写入{toInsert.Count}条");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{OperationType}同步写入数据库失败", operationType);
                        return webResponse.Error($"{operationType}同步写入数据库失败：{ex.Message}");
                    }
                }));

                if (!saveResult.Status)
                {
                    return saveResult;
                }

                var syncEndTime = DateTime.Now;
                var totalSeconds = (syncEndTime - syncStartTime).TotalSeconds;
                var message = $"{operationType}同步完成！获取数据{totalCount}条，写入{toInsert.Count}条，" +
                              $"跳过{skippedCount}条，重复{duplicateCount}条，耗时{totalSeconds:F2}秒";

                _logger.LogInformation(message);
                return response.OK(message);
            }
            catch (Exception ex)
            {
                var syncEndTime = DateTime.Now;
                var totalSeconds = (syncEndTime - syncStartTime).TotalSeconds;
                var errorMessage = $"{operationType}同步失败，耗时{totalSeconds:F2}秒，错误：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        private static DateTime? ParseErpDate(string dateValue)
        {
            if (string.IsNullOrWhiteSpace(dateValue))
            {
                return null;
            }

            var trimmed = dateValue.Trim();
            if (DateTime.TryParseExact(trimmed, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return parsed.Date;
            }

            if (DateTime.TryParse(trimmed, out parsed))
            {
                return parsed.Date;
            }

            return null;
        }

        private class ERPOrderTrackingDto
        {
            public string FAPPROVEDATE { get; set; }
            public long FENTRYID { get; set; }
            public string F_ORA_DATETIME { get; set; }
            public string FNUMBER { get; set; }
            public object FQTY { get; set; }
            public string FBILLNO { get; set; }
            public string FMTONO { get; set; }
            public string F_BLN_HFJHRQ { get; set; }
        }
  }
} 
