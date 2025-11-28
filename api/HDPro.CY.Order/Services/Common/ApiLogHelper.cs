using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.ManageUser;
using HDPro.CY.Order.IRepositories;
using HDPro.Entity.DomainModels;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.Common
{
    /// <summary>
    /// API日志记录辅助类
    /// </summary>
    public class ApiLogHelper
    {
        private readonly string _apiName;
        private readonly string _apiPath;
        private readonly string _httpMethod;
        private readonly string _requestParams;
        private readonly DateTime _startTime;
        private readonly Stopwatch _stopwatch;
        private readonly ILogger _logger;

        private string _responseResult;
        private string _remark;
        private int? _dataCount;

        private ApiLogHelper(string apiName, string apiPath, string httpMethod, string requestParams, ILogger logger)
        {
            _apiName = apiName;
            _apiPath = apiPath;
            _httpMethod = httpMethod;
            _requestParams = requestParams;
            _startTime = DateTime.Now;
            _stopwatch = Stopwatch.StartNew();
            _logger = logger;
        }

        /// <summary>
        /// 开始记录API调用
        /// </summary>
        public static ApiLogHelper Start(string apiName, string apiPath, string httpMethod, string requestParams, ILogger logger = null)
        {
            return new ApiLogHelper(apiName, apiPath, httpMethod, requestParams, logger);
        }

        /// <summary>
        /// 设置响应结果
        /// </summary>
        public ApiLogHelper WithResponseResult(string responseResult)
        {
            _responseResult = responseResult;
            return this;
        }

        /// <summary>
        /// 设置备注
        /// </summary>
        public ApiLogHelper WithRemark(string remark)
        {
            _remark = remark;
            return this;
        }

        /// <summary>
        /// 设置数据条数
        /// </summary>
        public ApiLogHelper WithDataCount(int? dataCount)
        {
            _dataCount = dataCount;
            return this;
        }

        /// <summary>
        /// 记录成功日志
        /// </summary>
        public async Task LogSuccessAsync(int? statusCode = 200, string responseContent = null)
        {
            _stopwatch.Stop();
            await SaveLogAsync(
                statusCode: statusCode,
                status: 1,
                errorMessage: null,
                responseLength: responseContent?.Length ?? _responseResult?.Length
            );
        }

        /// <summary>
        /// 记录失败日志
        /// </summary>
        public async Task LogFailureAsync(string errorMessage, int? statusCode = null, string responseContent = null)
        {
            _stopwatch.Stop();
            await SaveLogAsync(
                statusCode: statusCode,
                status: 0,
                errorMessage: errorMessage,
                responseLength: responseContent?.Length ?? _responseResult?.Length
            );
        }

        /// <summary>
        /// 记录异常日志
        /// </summary>
        public async Task LogExceptionAsync(Exception ex, int? statusCode = null)
        {
            _stopwatch.Stop();
            await SaveLogAsync(
                statusCode: statusCode,
                status: 0,
                errorMessage: $"{ex.GetType().Name}: {ex.Message}",
                responseLength: null
            );
        }

        /// <summary>
        /// 保存日志到数据库
        /// </summary>
        private async Task SaveLogAsync(int? statusCode, int status, string errorMessage, long? responseLength)
        {
            try
            {
                var currentUser = UserContext.Current;
                var endTime = DateTime.Now;
                var elapsedMs = _stopwatch.ElapsedMilliseconds;

                var apiLog = new OCP_ApiLog
                {
                    ApiName = _apiName?.Length > 200 ? _apiName.Substring(0, 200) : _apiName,
                    ApiPath = _apiPath?.Length > 200 ? _apiPath.Substring(0, 200) : _apiPath,
                    HttpMethod = _httpMethod?.Length > 20 ? _httpMethod.Substring(0, 20) : _httpMethod,
                    RequestParams = _requestParams,
                    ResponseLength = responseLength,
                    StatusCode = statusCode,
                    Status = status,
                    ErrorMessage = errorMessage,
                    StartTime = _startTime,
                    EndTime = endTime,
                    ElapsedMs = elapsedMs,
                    DataCount = _dataCount,
                    ResponseResult = _responseResult,
                    Remark = _remark?.Length > 500 ? _remark.Substring(0, 500) : _remark,
                    CreateDate = DateTime.Now,
                    CreateID = currentUser?.UserId ?? 0,
                    Creator = currentUser?.UserName ?? "系统",
                    ModifyDate = DateTime.Now,
                    ModifyID = currentUser?.UserId ?? 0,
                    Modifier = currentUser?.UserName ?? "系统"
                };

                var repository = AutofacContainerModule.GetService<IOCP_ApiLogRepository>();
                if (repository != null)
                {
                    await repository.AddAsync(apiLog);
                    await repository.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "保存API调用日志到数据库失败: {ErrorMessage}", ex.Message);
            }
        }
    }
}

