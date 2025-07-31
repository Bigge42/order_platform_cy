using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HDPro.Core.Configuration;
using HDPro.Core.Utilities;
using HDPro.Core.ManageUser;
using HDPro.Utilities;
using System.Linq;
using HDPro.Entity.DomainModels;
using HDPro.Core.BaseProvider;
using HDPro.Entity.SystemModels;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB
{
    /// <summary>
    /// ESB基础服务类，提供通用的ESB接口调用功能
    /// </summary>
    public class ESBBaseService
    {
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _loggerFactory;

        public ESBBaseService(IHttpClientFactory httpClientFactory, ILogger logger, ILoggerFactory loggerFactory = null)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// 获取指定类型的ESB日志记录器
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <returns>ESB日志记录器</returns>
        private ESBLogger GetESBLogger(string operationType)
        {
            if (_loggerFactory == null)
                return null;

            return new ESBLogger(_loggerFactory, operationType);
        }

        /// <summary>
        /// 记录信息日志（优先使用ESB日志记录器）
        /// </summary>
        protected void LogInfo(string message, string operationType = null)
        {
            var esbLogger = GetESBLogger(operationType);
            if (esbLogger != null)
                esbLogger.LogInfo(message);
            else
                _logger.LogInformation(message);
        }

        /// <summary>
        /// 记录信息日志（优先使用ESB日志记录器，带参数）
        /// </summary>
        protected void LogInfoWithArgs(string message, string operationType, params object[] args)
        {
            var esbLogger = GetESBLogger(operationType);
            if (esbLogger != null)
                esbLogger.LogInfo(message, args);
            else
                _logger.LogInformation(message, args);
        }

        /// <summary>
        /// 记录警告日志（优先使用ESB日志记录器）
        /// </summary>
        protected void LogWarning(string message, string operationType = null)
        {
            var esbLogger = GetESBLogger(operationType);
            if (esbLogger != null)
                esbLogger.LogWarning(message);
            else
                _logger.LogWarning(message);
        }

        /// <summary>
        /// 记录警告日志（优先使用ESB日志记录器，带参数）
        /// </summary>
        protected void LogWarningWithArgs(string message, string operationType, params object[] args)
        {
            var esbLogger = GetESBLogger(operationType);
            if (esbLogger != null)
                esbLogger.LogWarning(message, args);
            else
                _logger.LogWarning(message, args);
        }

        /// <summary>
        /// 记录错误日志（优先使用ESB日志记录器）
        /// </summary>
        protected void LogError(Exception ex, string message, string operationType = null)
        {
            var esbLogger = GetESBLogger(operationType);
            if (esbLogger != null)
                esbLogger.LogError(ex, message);
            else
                _logger.LogError(ex, message);
        }

        /// <summary>
        /// 记录错误日志（优先使用ESB日志记录器，无异常）
        /// </summary>
        protected void LogError(string message, string operationType = null)
        {
            var esbLogger = GetESBLogger(operationType);
            if (esbLogger != null)
                esbLogger.LogError(message);
            else
                _logger.LogError(message);
        }

        /// <summary>
        /// 记录错误日志（优先使用ESB日志记录器，带参数）
        /// </summary>
        protected void LogErrorWithArgs(string message, string operationType, params object[] args)
        {
            var esbLogger = GetESBLogger(operationType);
            if (esbLogger != null)
                esbLogger.LogError(message, args);
            else
                _logger.LogError(message, args);
        }

        /// <summary>
        /// 记录错误日志（优先使用ESB日志记录器，带异常和参数）
        /// </summary>
        protected void LogErrorWithArgs(Exception ex, string message, string operationType, params object[] args)
        {
            var esbLogger = GetESBLogger(operationType);
            if (esbLogger != null)
                esbLogger.LogError(ex, message, args);
            else
                _logger.LogError(ex, message, args);
        }

        /// <summary>
        /// 调用ESB接口获取数据的通用方法
        /// </summary>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <param name="apiPath">API路径</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <param name="operationType">操作类型（用于日志）</param>
        /// <returns>ESB数据列表</returns>
        public async Task<List<T>> CallESBApi<T>(string apiPath, string startDate, string endDate, string operationType)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                
                // 从配置文件获取ESB接口地址
                var baseUrl = AppSetting.ESB?.BaseUrl ?? "http://10.11.0.101:8003";
                var esbUrl = $"{baseUrl.TrimEnd('/')}/{apiPath}";
                
                // 构建请求参数
                var requestData = new
                {
                    FSTARTDATE = startDate,
                    FENDDATE = endDate
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // 从配置文件获取超时时间
                var timeoutMinutes = AppSetting.ESB?.DefaultTimeoutMinutes ?? 5;
                httpClient.Timeout = TimeSpan.FromMinutes(timeoutMinutes);

                LogInfoWithArgs("调用{OperationType}ESB接口：{EsbUrl}，参数：{JsonContent}", operationType, operationType, esbUrl, jsonContent);

                // 发送POST请求
                var response = await httpClient.PostAsync(esbUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    LogErrorWithArgs("{OperationType}ESB接口调用失败，状态码：{StatusCode}，原因：{ReasonPhrase}",
                        operationType, operationType, response.StatusCode, response.ReasonPhrase);
                    return new List<T>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                LogInfoWithArgs("{OperationType}ESB接口返回数据长度：{Length}", operationType, operationType, responseContent?.Length ?? 0);
                HDLogHelper.Log($"{operationType}ESBApi", responseContent);

                // 反序列化响应数据
                var dataList = JsonConvert.DeserializeObject<List<T>>(responseContent);
                
                return dataList ?? new List<T>();
            }
            catch (Exception ex)
            {
                LogErrorWithArgs(ex, "调用{OperationType}ESB接口异常", operationType, operationType);
                throw;
            }
        }

        /// <summary>
        /// 调用ESB接口的通用方法（自定义请求参数）
        /// </summary>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <param name="apiPath">API路径</param>
        /// <param name="requestData">请求参数对象</param>
        /// <param name="operationType">操作类型（用于日志）</param>
        /// <param name="customBaseUrl">自定义基础URL（可选）</param>
        /// <returns>ESB接口返回的数据列表</returns>
        public async Task<List<T>> CallESBApiWithRequestData<T>(string apiPath, object requestData, string operationType, string customBaseUrl = null)
        {
            // 从配置文件获取超时时间
            var timeoutMinutes = AppSetting.ESB?.DefaultTimeoutMinutes ?? 15;

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();

                // 确定ESB接口地址
                var baseUrl = customBaseUrl ?? AppSetting.ESB?.BaseUrl ?? "http://10.11.0.101:8003";

                // 根据API路径确定完整URL
                string esbUrl;
                if (apiPath.StartsWith("/gateway/DataCenter/"))
                {
                    // 数据中心接口
                    esbUrl = $"{baseUrl.TrimEnd('/')}{apiPath}";
                }
                else if (apiPath.StartsWith("/gateway/SearchMOMProcedure") ||
                         apiPath.StartsWith("/gateway/SearchERPSalOrderEntry"))
                {
                    // 其他gateway接口
                    esbUrl = $"{baseUrl.TrimEnd('/')}{apiPath}";
                }
                else
                {
                    // 默认添加gateway前缀
                    esbUrl = $"{baseUrl.TrimEnd('/')}/{apiPath}";
                }

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

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
                    return new List<T>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                LogInfoWithArgs("{OperationType}ESB接口返回数据长度：{Length}", operationType, operationType, responseContent?.Length ?? 0);
                //HDLogHelper.Log($"{operationType}ESBApi", responseContent);

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    LogWarningWithArgs("{OperationType}ESB接口返回空响应", operationType, operationType);
                    return new List<T>();
                }

                // 反序列化响应数据
                var dataList = JsonConvert.DeserializeObject<List<T>>(responseContent);

                return dataList ?? new List<T>();
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || ex.Message.Contains("timeout"))
            {
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
        /// 解析日期时间字符串的通用方法，支持多种日期格式，并验证SQL Server DateTime范围
        /// </summary>
        /// <param name="dateString">日期字符串</param>
        /// <returns>解析后的日期时间，失败或超出范围时返回null</returns>
        public DateTime? ParseDateTime(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            // SQL Server DateTime有效范围
            var sqlMinDate = new DateTime(1753, 1, 1);
            var sqlMaxDate = new DateTime(9999, 12, 31, 23, 59, 59);

            // 尝试多种日期格式解析
            string[] formats = {
                "yyyy-MM-dd HH:mm:ss.fff",    // 包含毫秒
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "yyyy/MM/dd HH:mm:ss",
                "yyyy/MM/dd",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy",
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy",
                "yyyy年MM月dd日 HH:mm:ss",
                "yyyy年MM月dd日",
                "yyyyMMdd HH:mm:ss",
                "yyyyMMdd",
                "yyyy-M-d H:m:s",            // 单数字月日时分秒
                "yyyy-M-d"
            };

            var trimmedDateString = dateString.Trim();

            DateTime parsedDate = DateTime.MinValue;
            bool parseSuccess = false;

            // 先尝试精确格式匹配
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(trimmedDateString, format, null, System.Globalization.DateTimeStyles.None, out parsedDate))
                {
                    parseSuccess = true;
                    break;
                }
            }

            // 尝试通用解析
            if (!parseSuccess && DateTime.TryParse(trimmedDateString, out parsedDate))
            {
                parseSuccess = true;
            }

            // 特殊处理：如果包含T分隔符的ISO 8601格式
            if (!parseSuccess && trimmedDateString.Contains('T'))
            {
                if (DateTime.TryParse(trimmedDateString.Replace('T', ' '), out parsedDate))
                {
                    parseSuccess = true;
                }
            }

            if (!parseSuccess)
            {
                // 记录解析失败的日志（仅警告，不抛异常）
                LogWarningWithArgs("无法解析日期格式：'{DateString}'，请检查数据源格式", "ESB", dateString);
                return null;
            }

            // 验证日期是否在SQL Server DateTime有效范围内
            if (parsedDate < sqlMinDate || parsedDate > sqlMaxDate)
            {
                LogWarningWithArgs("日期超出SQL Server DateTime有效范围：'{DateString}' -> {ParsedDate}，有效范围：{MinDate} 到 {MaxDate}",
                    "ESB", dateString, parsedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    sqlMinDate.ToString("yyyy-MM-dd"), sqlMaxDate.ToString("yyyy-MM-dd"));
                return null;
            }

            return parsedDate;
        }

        /// <summary>
        /// 获取默认时间范围
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>时间范围元组</returns>
        public (string startDate, string endDate) GetDateRange(string startDate, string endDate)
        {
            var defaultSyncDays = AppSetting.ESB?.DefaultSyncDays ?? 5;
            
            if (string.IsNullOrEmpty(startDate))
            {
                startDate = DateTime.Now.AddDays(-defaultSyncDays).ToString("yyyy-MM-dd");
            }
            if (string.IsNullOrEmpty(endDate))
            {
                endDate = DateTime.Now.ToString("yyyy-MM-dd");
            }

            return (startDate, endDate);
        }

        /// <summary>
        /// 获取同步操作的基本信息
        /// </summary>
        /// <returns>操作信息元组</returns>
        public (DateTime syncStartTime, string executorInfo, int currentUserId, string currentUserName) GetSyncOperationInfo()
        {
            var syncStartTime = DateTime.Now;
            var executorInfo = UserContext.Current?.UserName ?? "系统定时同步";
            var currentUserId = UserContext.Current?.UserId ?? 1;
            var currentUserName = UserContext.Current?.UserName ?? "系统同步";

            return (syncStartTime, executorInfo, currentUserId, currentUserName);
        }

        /// <summary>
        /// 记录同步操作日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="executorInfo">执行者信息</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        public void LogSyncStart(string operationType, string executorInfo, string startDate, string endDate)
        {
            LogInfoWithArgs("开始{OperationType}ESB数据同步，执行者：{ExecutorInfo}，时间范围：{StartDate} 到 {EndDate}",
                operationType, operationType, executorInfo, startDate, endDate);
        }

        /// <summary>
        /// 记录同步完成日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="executorInfo">执行者信息</param>
        /// <param name="totalCount">总数据量</param>
        /// <param name="validCount">有效数据量</param>
        /// <param name="processedCount">处理数据量</param>
        /// <param name="syncStartTime">同步开始时间</param>
        /// <returns>成功消息</returns>
        public string LogSyncComplete(string operationType, string executorInfo, int totalCount, int validCount, int processedCount, DateTime syncStartTime)
        {
            var syncEndTime = DateTime.Now;
            var totalTime = (syncEndTime - syncStartTime).TotalSeconds;

            var successMessage = $"{operationType}ESB数据同步完成！执行者：{executorInfo}，" +
                $"获取数据：{totalCount} 条，有效数据：{validCount} 条，" +
                $"处理记录：{processedCount} 条，耗时：{totalTime:F2} 秒";

            LogInfo(successMessage, operationType);
            return successMessage;
        }

        /// <summary>
        /// 记录同步错误日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="ex">异常信息</param>
        /// <param name="syncStartTime">同步开始时间</param>
        /// <returns>错误消息</returns>
        public string LogSyncError(string operationType, Exception ex, DateTime syncStartTime)
        {
            var syncEndTime = DateTime.Now;
            var totalTime = (syncEndTime - syncStartTime).TotalSeconds;
            var errorMessage = $"同步{operationType}ESB数据失败，耗时：{totalTime:F2} 秒，错误：{ex.Message}";

            LogError(ex, errorMessage, operationType);
            return errorMessage;
        }
    }
} 