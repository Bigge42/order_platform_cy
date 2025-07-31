using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels.ESB;
using HDPro.Core.Configuration;
using HDPro.CY.Order.IServices.OrderCollaboration.ESB;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB
{
    /// <summary>
    /// ESB删除数据查询服务
    /// 用于查询订单协同平台各业务领域的删除数据主键
    /// </summary>
    public class QueryDeletedDataService : ESBBaseService, IQueryDeletedDataService
    {
        private readonly ESBLogger _esbLogger;

        public QueryDeletedDataService(
            IHttpClientFactory httpClientFactory,
            ILogger<QueryDeletedDataService> logger,
            ILoggerFactory loggerFactory)
            : base(httpClientFactory, logger, loggerFactory)
        {
            _esbLogger = ESBLoggerFactory.CreateQueryDeletedDataLogger(loggerFactory);
        }

        /// <summary>
        /// 查询单个业务类型的删除数据
        /// </summary>
        /// <param name="businessType">业务类型（DDGZ、BOMDJJD、DDJDCX、CGGZ、WWGZ、ZJGZ、BJGZ、JGGZ）</param>
        /// <returns>删除数据查询结果</returns>
        public async Task<WebResponseContent> QueryDeletedDataAsync(string businessType)
        {
            var response = new WebResponseContent();
            var (syncStartTime, executorInfo, _, _) = GetSyncOperationInfo();

            try
            {
                // 参数验证
                if (string.IsNullOrWhiteSpace(businessType))
                {
                    return response.Error("业务类型不能为空");
                }

                if (!ESBDeletedDataBusinessType.IsValidBusinessType(businessType))
                {
                    return response.Error($"不支持的业务类型：{businessType}");
                }

                var businessTypeName = ESBDeletedDataBusinessType.GetBusinessTypeName(businessType);
                var operationType = $"查询{businessTypeName}删除数据";

                LogInfoWithArgs("开始{OperationType}，执行者：{ExecutorInfo}，业务类型：{BusinessType}",
                    operationType, operationType, executorInfo, businessType);

                // 调用ESB接口
                var deletedDataList = await CallESBDeletedDataApi(businessType, operationType);

                var successMessage = $"{operationType}完成！执行者：{executorInfo}，" +
                    $"业务类型：{businessType}（{businessTypeName}），" +
                    $"返回删除记录：{deletedDataList?.Count ?? 0} 条，" +
                    $"耗时：{(DateTime.Now - syncStartTime).TotalSeconds:F2} 秒";

                LogInfo(successMessage, operationType);

                return response.OK(successMessage, deletedDataList ?? new List<ESBDeletedDataResponse>());
            }
            catch (Exception ex)
            {
                var errorMessage = LogSyncError($"查询{ESBDeletedDataBusinessType.GetBusinessTypeName(businessType)}删除数据", ex, syncStartTime);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 批量查询所有业务类型的删除数据
        /// </summary>
        /// <returns>所有业务类型的删除数据查询结果</returns>
        public async Task<WebResponseContent> QueryAllDeletedDataAsync()
        {
            var response = new WebResponseContent();
            var (syncStartTime, executorInfo, _, _) = GetSyncOperationInfo();
            var results = new Dictionary<string, object>();

            try
            {
                LogInfoWithArgs("开始批量查询所有业务领域删除数据，执行者：{ExecutorInfo}", "批量查询删除数据", executorInfo);

                var allBusinessTypes = ESBDeletedDataBusinessType.GetAllBusinessTypes();
                var totalDeletedCount = 0;
                var successCount = 0;
                var errorCount = 0;

                foreach (var businessType in allBusinessTypes)
                {
                    try
                    {
                        var businessTypeName = ESBDeletedDataBusinessType.GetBusinessTypeName(businessType);
                        var deletedData = await CallESBDeletedDataApi(businessType, $"查询{businessTypeName}删除数据");

                        results[businessType] = new
                        {
                            BusinessDomain = businessTypeName,
                            BusinessType = businessType,
                            DeletedCount = deletedData?.Count ?? 0,
                            DeletedIds = deletedData,
                            PrimaryKeyField = ESBDeletedDataBusinessType.GetPrimaryKeyDescription(businessType),
                            Success = true,
                            QueryTime = DateTime.Now
                        };

                        totalDeletedCount += deletedData?.Count ?? 0;
                        successCount++;

                        LogInfoWithArgs("查询{BusinessTypeName}删除数据成功，返回 {Count} 条记录",
                            "批量查询删除数据", businessTypeName, deletedData?.Count ?? 0);
                    }
                    catch (Exception ex)
                    {
                        var businessTypeName = ESBDeletedDataBusinessType.GetBusinessTypeName(businessType);
                        LogErrorWithArgs(ex, "查询{BusinessTypeName}删除数据失败：{ErrorMessage}",
                            "批量查询删除数据", businessTypeName, ex.Message);

                        results[businessType] = new
                        {
                            BusinessDomain = businessTypeName,
                            BusinessType = businessType,
                            DeletedCount = 0,
                            Success = false,
                            Error = ex.Message,
                            QueryTime = DateTime.Now
                        };

                        errorCount++;
                    }
                }

                var successMessage = $"批量查询所有业务领域删除数据完成！执行者：{executorInfo}，" +
                    $"查询业务类型：{allBusinessTypes.Count} 个，成功：{successCount} 个，失败：{errorCount} 个，" +
                    $"总删除记录：{totalDeletedCount} 条，耗时：{(DateTime.Now - syncStartTime).TotalSeconds:F2} 秒";

                LogInfo(successMessage, "批量查询删除数据");

                return response.OK(successMessage, results);
            }
            catch (Exception ex)
            {
                var errorMessage = LogSyncError("批量查询删除数据", ex, syncStartTime);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 调用ESB删除数据查询接口
        /// </summary>
        /// <param name="businessType">业务类型</param>
        /// <param name="operationType">操作类型（用于日志）</param>
        /// <returns>删除数据列表</returns>
        private async Task<List<ESBDeletedDataResponse>> CallESBDeletedDataApi(string businessType, string operationType)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();

                // 从配置文件获取ESB接口地址
                var baseUrl = AppSetting.ESB?.BaseUrl ?? "http://10.11.0.101:8003/gateway";
                var esbUrl = $"{baseUrl.TrimEnd('/')}/DDPTDELETE";

                // 构建请求参数
                var requestData = new ESBDeletedDataRequest
                {
                    CLASS = businessType.ToUpper()
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 从配置文件获取超时时间
                var timeoutMinutes = AppSetting.ESB?.DefaultTimeoutMinutes ?? 5;
                httpClient.Timeout = TimeSpan.FromMinutes(timeoutMinutes);

                LogInfoWithArgs("调用{OperationType}ESB接口：{EsbUrl}，超时设置：{TimeoutMinutes}分钟，参数：{JsonContent}",
                    operationType, operationType, esbUrl, timeoutMinutes, jsonContent);

                // 发送POST请求
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await httpClient.PostAsync(esbUrl, content);
                stopwatch.Stop();

                LogInfoWithArgs("{OperationType}ESB接口调用完成，耗时：{ElapsedSeconds:F2}秒",
                    operationType, operationType, stopwatch.Elapsed.TotalSeconds);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    LogErrorWithArgs("{OperationType}ESB接口调用失败，状态码：{StatusCode}，原因：{ReasonPhrase}，错误内容：{ErrorContent}",
                        operationType, operationType, response.StatusCode, response.ReasonPhrase, errorContent);
                    return new List<ESBDeletedDataResponse>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                LogInfoWithArgs("{OperationType}ESB接口返回数据长度：{Length}", operationType, operationType, responseContent?.Length ?? 0);

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    LogWarningWithArgs("{OperationType}ESB接口返回空响应", operationType, operationType);
                    return new List<ESBDeletedDataResponse>();
                }

                // 检查是否是错误响应
                if (IsErrorResponse(responseContent))
                {
                    var errorResponse = JsonConvert.DeserializeObject<ESBDeletedDataErrorResponse>(responseContent);
                    LogWarningWithArgs("{OperationType}ESB接口返回错误响应：{ErrorMessage}", operationType, operationType, errorResponse?.Msg);
                    return new List<ESBDeletedDataResponse>();
                }

                // 反序列化响应数据
                var deletedDataList = JsonConvert.DeserializeObject<List<ESBDeletedDataResponse>>(responseContent);

                LogInfoWithArgs("{OperationType}成功解析删除数据：{Count} 条", operationType, operationType, deletedDataList?.Count ?? 0);

                return deletedDataList ?? new List<ESBDeletedDataResponse>();
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || ex.Message.Contains("timeout"))
            {
                var timeoutMinutes = AppSetting.ESB?.DefaultTimeoutMinutes ?? 5;
                LogErrorWithArgs(ex, "{OperationType}ESB接口调用超时，超时设置：{TimeoutMinutes}分钟", operationType, operationType, timeoutMinutes);
                throw new TimeoutException($"{operationType}ESB接口调用超时（{timeoutMinutes}分钟）", ex);
            }
            catch (Exception ex)
            {
                LogErrorWithArgs(ex, "调用{OperationType}ESB接口异常", operationType, operationType);
                throw;
            }
        }

        /// <summary>
        /// 检查响应是否为错误响应
        /// </summary>
        /// <param name="responseContent">响应内容</param>
        /// <returns>是否为错误响应</returns>
        private bool IsErrorResponse(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
                return false;

            // 检查是否包含错误响应的特征字段
            return responseContent.Contains("\"code\"") && 
                   responseContent.Contains("\"Msg\"") && 
                   (responseContent.Contains("\"500\"") || responseContent.Contains("检查传入类型"));
        }

        /// <summary>
        /// 获取业务类型统计信息
        /// </summary>
        /// <returns>业务类型统计信息</returns>
        public WebResponseContent GetBusinessTypeStatistics()
        {
            try
            {
                var allBusinessTypes = ESBDeletedDataBusinessType.GetAllBusinessTypes();
                var statistics = allBusinessTypes.Select(businessType => new
                {
                    BusinessType = businessType,
                    BusinessTypeName = ESBDeletedDataBusinessType.GetBusinessTypeName(businessType),
                    PrimaryKeyField = ESBDeletedDataBusinessType.GetPrimaryKeyDescription(businessType),
                    Description = $"查询{ESBDeletedDataBusinessType.GetBusinessTypeName(businessType)}的删除数据主键"
                }).ToList();

                var result = new
                {
                    TotalBusinessTypes = allBusinessTypes.Count,
                    SupportedBusinessTypes = statistics,
                    ESBApiUrl = $"{AppSetting.ESB?.BaseUrl ?? "http://10.11.0.101:8003"}/gateway/DDPTDELETE",
                    Description = "订单协同平台删除数据查询接口，返回近三天的删除数据主键"
                };

                return new WebResponseContent().OK("获取业务类型统计信息成功", result);
            }
            catch (Exception ex)
            {
                LogError(ex, "获取业务类型统计信息异常", "查询删除数据统计");
                return new WebResponseContent().Error($"获取统计信息异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 验证业务类型参数
        /// </summary>
        /// <param name="businessType">业务类型</param>
        /// <returns>验证结果</returns>
        public WebResponseContent ValidateBusinessType(string businessType)
        {
            var response = new WebResponseContent();

            if (string.IsNullOrWhiteSpace(businessType))
            {
                return response.Error("业务类型不能为空");
            }

            if (!ESBDeletedDataBusinessType.IsValidBusinessType(businessType))
            {
                var supportedTypes = string.Join("、", ESBDeletedDataBusinessType.GetAllBusinessTypes());
                return response.Error($"不支持的业务类型：{businessType}。支持的业务类型：{supportedTypes}");
            }

            var businessTypeName = ESBDeletedDataBusinessType.GetBusinessTypeName(businessType);
            var primaryKeyField = ESBDeletedDataBusinessType.GetPrimaryKeyDescription(businessType);

            return response.OK("业务类型验证通过", new
            {
                BusinessType = businessType.ToUpper(),
                BusinessTypeName = businessTypeName,
                PrimaryKeyField = primaryKeyField,
                IsValid = true
            });
        }
    }
}