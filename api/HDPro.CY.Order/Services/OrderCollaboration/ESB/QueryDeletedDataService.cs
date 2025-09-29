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
using HDPro.Entity.DomainModels;
using HDPro.Core.UserManager;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.ManageUser;
using Dapper;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using HDPro.Core.Dapper;
using HDPro.Core.Extensions;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB
{
    /// <summary>
    /// ESB删除数据查询服务
    /// 用于查询、存储删除数据并执行业务表删除逻辑
    /// </summary>
    public class QueryDeletedDataService : ESBBaseService, IQueryDeletedDataService
    {
        private readonly ESBLogger _esbLogger;
        private readonly IOCP_DeletedDataRepository _deletedDataRepository;
        private readonly IOCP_OrderTrackingRepository _orderTrackingRepository;

        public QueryDeletedDataService(
            IHttpClientFactory httpClientFactory,
            ILogger<QueryDeletedDataService> logger,
            ILoggerFactory loggerFactory,
            IOCP_DeletedDataRepository deletedDataRepository,
            IOCP_OrderTrackingRepository orderTrackingRepository)
            : base(httpClientFactory, logger, loggerFactory)
        {
            _esbLogger = ESBLoggerFactory.CreateQueryDeletedDataLogger(loggerFactory);
            _deletedDataRepository = deletedDataRepository;
            _orderTrackingRepository = orderTrackingRepository;
        }

        /// <summary>
        /// 查询、存储并删除单个业务类型的数据（完整流程）
        /// </summary>
        /// <param name="businessType">业务类型（DDGZ、BOMDJJD、DDJDCX、CGGZ、WWGZ、ZJGZ、BJGZ、JGGZ）</param>
        /// <returns>完整流程处理结果</returns>
        public async Task<WebResponseContent> ProcessDeletedDataAsync(string businessType)
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
                LogInfoWithArgs("开始处理{BusinessTypeName}删除数据，执行者：{ExecutorInfo}",
                    "处理删除数据", businessTypeName, executorInfo);

                // 步骤1：查询ESB删除数据
                var deletedDataList = await CallESBDeletedDataApi(businessType, $"查询{businessTypeName}删除数据");
                var queryCount = deletedDataList?.Count ?? 0;

                if (queryCount == 0)
                {
                    var noDataMessage = $"处理{businessTypeName}删除数据完成，无数据需要处理";
                    LogInfo(noDataMessage, "处理删除数据");
                    return response.OK(noDataMessage, new
                    {
                        BusinessType = businessType,
                        BusinessTypeName = businessTypeName,
                        QueryCount = 0,
                        StoredCount = 0,
                        DeletedCount = 0,
                        SkippedCount = 0,
                        ExecutionTime = (DateTime.Now - syncStartTime).TotalSeconds
                    });
                }

                // 步骤2：存储删除数据到OCP_DeletedData表
                var (storedCount, storeSkippedCount) = await StoreDeletedDataAsync(businessType, deletedDataList);

                // 步骤3：执行业务表删除逻辑
                var (deletedCount, deleteSkippedCount) = await DeleteBusinessDataAsync(businessType, deletedDataList);

                var successMessage = $"处理{businessTypeName}删除数据完成！执行者：{executorInfo}，" +
                    $"查询：{queryCount} 条，存储：{storedCount} 条，删除：{deletedCount} 条，" +
                    $"跳过：{storeSkippedCount + deleteSkippedCount} 条，耗时：{(DateTime.Now - syncStartTime).TotalSeconds:F2} 秒";

                LogInfo(successMessage, "处理删除数据");

                return response.OK(successMessage, new
                {
                    BusinessType = businessType,
                    BusinessTypeName = businessTypeName,
                    QueryCount = queryCount,
                    StoredCount = storedCount,
                    DeletedCount = deletedCount,
                    SkippedCount = storeSkippedCount + deleteSkippedCount,
                    ExecutionTime = (DateTime.Now - syncStartTime).TotalSeconds
                });
            }
            catch (Exception ex)
            {
                var errorMessage = LogSyncError($"处理{ESBDeletedDataBusinessType.GetBusinessTypeName(businessType)}删除数据", ex, syncStartTime);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 查询、存储并删除所有业务类型的数据（完整流程）
        /// </summary>
        /// <returns>完整流程处理结果</returns>
        public async Task<WebResponseContent> ProcessAllDeletedDataAsync()
        {
            var response = new WebResponseContent();
            var (syncStartTime, executorInfo, _, _) = GetSyncOperationInfo();
            var results = new Dictionary<string, object>();

            try
            {
                LogInfoWithArgs("开始批量处理所有业务领域删除数据，执行者：{ExecutorInfo}", "批量处理删除数据", executorInfo);

                var allBusinessTypes = ESBDeletedDataBusinessType.GetAllBusinessTypes();
                var totalQueryCount = 0;
                var totalStoredCount = 0;
                var totalDeletedCount = 0;
                var totalSkippedCount = 0;
                var successCount = 0;
                var errorCount = 0;

                foreach (var businessType in allBusinessTypes)
                {
                    try
                    {
                        var businessTypeName = ESBDeletedDataBusinessType.GetBusinessTypeName(businessType);
                        
                        // 查询ESB删除数据
                        var deletedData = await CallESBDeletedDataApi(businessType, $"查询{businessTypeName}删除数据");
                        var queryCount = deletedData?.Count ?? 0;
                        totalQueryCount += queryCount;

                        int storedCount = 0;
                        int deletedCount = 0;
                        int skippedCount = 0;

                        if (deletedData != null && deletedData.Any())
                        {
                            // 存储删除数据
                            var (stored, storeSkipped) = await StoreDeletedDataAsync(businessType, deletedData);
                            storedCount = stored;
                            
                            // 删除业务数据
                            var (deleted, deleteSkipped) = await DeleteBusinessDataAsync(businessType, deletedData);
                            deletedCount = deleted;
                            
                            skippedCount = storeSkipped + deleteSkipped;
                            totalStoredCount += storedCount;
                            totalDeletedCount += deletedCount;
                            totalSkippedCount += skippedCount;
                        }

                        results[businessType] = new
                        {
                            BusinessDomain = businessTypeName,
                            BusinessType = businessType,
                            QueryCount = queryCount,
                            StoredCount = storedCount,
                            DeletedCount = deletedCount,
                            SkippedCount = skippedCount,
                            PrimaryKeyField = ESBDeletedDataBusinessType.GetPrimaryKeyDescription(businessType),
                            Success = true,
                            ProcessTime = DateTime.Now
                        };

                        successCount++;

                        LogInfoWithArgs("处理{BusinessTypeName}删除数据成功，查询：{QueryCount} 条，存储：{StoredCount} 条，删除：{DeletedCount} 条，跳过：{SkippedCount} 条",
                            "批量处理删除数据", businessTypeName, queryCount, storedCount, deletedCount, skippedCount);
                    }
                    catch (Exception ex)
                    {
                        var businessTypeName = ESBDeletedDataBusinessType.GetBusinessTypeName(businessType);
                        LogErrorWithArgs(ex, "处理{BusinessTypeName}删除数据失败：{ErrorMessage}",
                            "批量处理删除数据", businessTypeName, ex.Message);

                        results[businessType] = new
                        {
                            BusinessDomain = businessTypeName,
                            BusinessType = businessType,
                            QueryCount = 0,
                            StoredCount = 0,
                            DeletedCount = 0,
                            SkippedCount = 0,
                            Success = false,
                            Error = ex.Message,
                            ProcessTime = DateTime.Now
                        };

                        errorCount++;
                    }
                }

                var successMessage = $"批量处理所有业务领域删除数据完成！执行者：{executorInfo}，" +
                    $"处理业务类型：{allBusinessTypes.Count} 个，成功：{successCount} 个，失败：{errorCount} 个，" +
                    $"总查询：{totalQueryCount} 条，总存储：{totalStoredCount} 条，总删除：{totalDeletedCount} 条，总跳过：{totalSkippedCount} 条，" +
                    $"耗时：{(DateTime.Now - syncStartTime).TotalSeconds:F2} 秒";

                LogInfo(successMessage, "批量处理删除数据");

                return response.OK(successMessage, results);
            }
            catch (Exception ex)
            {
                var errorMessage = LogSyncError("批量处理删除数据", ex, syncStartTime);
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



      

        /// <summary>
        /// 存储删除数据到OCP_DeletedData表
        /// </summary>
        /// <param name="businessType">业务类型</param>
        /// <param name="deletedDataList">删除数据列表</param>
        /// <returns>存储结果(存储数量, 跳过数量)</returns>
        private async Task<(int StoredCount, int SkippedCount)> StoreDeletedDataAsync(string businessType, List<ESBDeletedDataResponse> deletedDataList)
        {
            if (deletedDataList == null || !deletedDataList.Any())
            {
                return (0, 0);
            }

            var businessTypeName = ESBDeletedDataBusinessType.GetBusinessTypeName(businessType);
            LogInfoWithArgs("开始存储{BusinessTypeName}删除数据，共 {Count} 条", "存储删除数据", businessTypeName, deletedDataList.Count);

            var storedCount = 0;
            var skippedCount = 0;
            var currentTime = DateTime.Now;
            var executorInfo = UserContext.Current?.UserName ?? "系统";

            foreach (var deletedData in deletedDataList)
            {
                try
                {
                    // 检查是否已存在（根据业务类型+FEntryID+FID去重）
                    var existingData = await _deletedDataRepository.FindAsyncFirst(x => 
                        x.BusinessType == businessType.ToUpper() && 
                        x.FID == deletedData.FID );

                    if (existingData != null)
                    {
                        skippedCount++;
                        continue;
                    }

                    // 创建新的删除数据记录
                    var newDeletedData = new OCP_DeletedData
                    {
                        BusinessType = businessType.ToUpper(),
                        FID = deletedData.FID,
                       // FENTRYID = deletedData.FID,
                        CreateDate = currentTime,
                        Creator = executorInfo,
                        CreateID = UserContext.Current?.UserId
                    };

                    await _deletedDataRepository.AddAsync(newDeletedData);
                    storedCount++;
                }
                catch (Exception ex)
                {
                    LogErrorWithArgs(ex, "存储单条删除数据异常，业务类型：{BusinessType}，FID：{FID}，FENTRYID：{FENTRYID}",
                        "存储删除数据", businessType, deletedData.FID, deletedData.FENTRYID);
                    skippedCount++;
                }
            }

            await _deletedDataRepository.SaveChangesAsync();

            LogInfoWithArgs("存储{BusinessTypeName}删除数据完成，总数：{TotalCount} 条，存储：{StoredCount} 条，跳过：{SkippedCount} 条",
                "存储删除数据", businessTypeName, deletedDataList.Count, storedCount, skippedCount);

            return (storedCount, skippedCount);
        }

        /// <summary>
        /// 根据删除数据删除对应的业务数据
        /// </summary>
        /// <param name="businessType">业务类型</param>
        /// <param name="deletedDataList">删除数据列表</param>
        /// <returns>删除结果(删除数量, 跳过数量)</returns>
        private async Task<(int DeletedCount, int SkippedCount)> DeleteBusinessDataAsync(string businessType, List<ESBDeletedDataResponse> deletedDataList)
        {
            if (deletedDataList == null || !deletedDataList.Any())
            {
                return (0, 0);
            }

            var businessTypeName = ESBDeletedDataBusinessType.GetBusinessTypeName(businessType);
            LogInfoWithArgs("开始删除{BusinessTypeName}业务数据，共 {Count} 条", "删除业务数据", businessTypeName, deletedDataList.Count);

            var deletedCount = 0;
            var skippedCount = 0;
            var dapperContext = _orderTrackingRepository.DapperContext;
            try
            {
                switch (businessType.ToUpper())
                {
                    case ESBDeletedDataBusinessType.DDGZ:
                        (deletedCount, skippedCount) = await DoDeleteBusinessDataAsync(dapperContext, deletedDataList, "OCP_OrderTracking", "SOEntryID");
                        break;
                    case ESBDeletedDataBusinessType.BOMDJJD:
                        (deletedCount, skippedCount) = await DoDeleteBusinessDataAsync(dapperContext, deletedDataList, "OCP_TechManagement", "SOEntryID");
                        break;
                    case ESBDeletedDataBusinessType.DDJDCX:
                        (deletedCount, skippedCount) = await DoDeleteBusinessDataAsync(dapperContext, deletedDataList, "OCP_SOProgress", "FID");
                        break;
                    case ESBDeletedDataBusinessType.CGGZ:
                        (deletedCount, skippedCount) = await DoDeleteBusinessDataAsync(dapperContext, deletedDataList, "OCP_POUnFinishTrack", "FENTRYID");
                        break;
                    case ESBDeletedDataBusinessType.WWGZ:
                        (deletedCount, skippedCount) = await DoDeleteBusinessDataAsync(dapperContext, deletedDataList, "OCP_SubOrderUnFinishTrack", "FENTRYID");
                        break;
                    case ESBDeletedDataBusinessType.ZJGZ:
                        (deletedCount, skippedCount) = await DoDeleteBusinessDataAsync(dapperContext, deletedDataList, "OCP_PrdMOTracking", "FENTRYID");
                        break;
                    case ESBDeletedDataBusinessType.BJGZ:
                        (deletedCount, skippedCount) = await DoDeleteBusinessDataAsync(dapperContext, deletedDataList, "OCP_PartUnFinishTracking", "FENTRYID");
                        break;
                    case ESBDeletedDataBusinessType.JGGZ:
                        (deletedCount, skippedCount) = await DoDeleteBusinessDataAsync(dapperContext, deletedDataList, "OCP_JGUnFinishTrack", "FENTRYID");
                        break;
                    default:
                        LogWarningWithArgs("业务类型{BusinessType}的删除逻辑暂未实现", "删除业务数据", businessType);
                        skippedCount = deletedDataList.Count;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogErrorWithArgs(ex, "删除{BusinessTypeName}业务数据事务异常", "删除业务数据", businessTypeName);
                throw;
            }

            LogInfoWithArgs("删除{BusinessTypeName}业务数据完成，总数：{TotalCount} 条，删除：{DeletedCount} 条，跳过：{SkippedCount} 条",
                "删除业务数据", businessTypeName, deletedDataList.Count, deletedCount, skippedCount);

            return (deletedCount, skippedCount);
        }

        /// <summary>
        /// 通用删除业务数据方法
        /// </summary>
        /// <param name="dapperContext">Dapper上下文</param>
        /// <param name="deletedDataList">删除数据列表</param>
        /// <param name="tableName">表名</param>
        /// <param name="fidField">FID字段名</param>
        /// <returns>删除结果(删除数量, 跳过数量)</returns>
        private async Task<(int DeletedCount, int SkippedCount)> DoDeleteBusinessDataAsync(dynamic dapperContext, List<ESBDeletedDataResponse> deletedDataList, string tableName, string fidField)
        {
            var deletedCount = 0;
            var skippedCount = 0;

            foreach (var deletedData in deletedDataList)
            {
                try
                {
                    string deleteSql;
                    object parameters;

                    // 如果FENTRYID为空，则匹配单据ID
                    deleteSql = $"DELETE FROM {tableName} WHERE {fidField} = @FID";
                    parameters = new { FID = deletedData.FID };

                    var affectedRows = await dapperContext.ExcuteNonQueryAsync(deleteSql, parameters);
                    
                    if (affectedRows > 0)
                    {
                        deletedCount += affectedRows;
                        LogInfoWithArgs("成功删除{TableName}数据 {AffectedRows} 条，FID：{FID}，FENTRYID：{FENTRYID}",
                            "删除业务数据", tableName, affectedRows, deletedData.FID, deletedData.FENTRYID);
                    }
                    else
                    {
                        LogWarningWithArgs("未找到匹配的{TableName}数据，FID：{FID}，FENTRYID：{FENTRYID}",
                            "删除业务数据", tableName, deletedData.FID, deletedData.FENTRYID);
                        skippedCount++;
                    }
                }
                catch (Exception ex)
                {
                    LogErrorWithArgs(ex, "删除单条{TableName}数据异常，FID：{FID}，FENTRYID：{FENTRYID}",
                        "删除业务数据", tableName, deletedData.FID, deletedData.FENTRYID);
                    skippedCount++;
                }
            }

            return (deletedCount, skippedCount);
        }
    }
}