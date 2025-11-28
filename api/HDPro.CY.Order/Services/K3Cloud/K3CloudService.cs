/*
 * K3Cloud服务实现类
 */
using HDPro.CY.Order.Services.K3Cloud.Models;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.Utilities;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.ManageUser;

namespace HDPro.CY.Order.Services.K3Cloud
{
    /// <summary>
    /// K3Cloud服务实现类
    /// </summary>
    public class K3CloudService : IK3CloudService, IDependency
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<K3CloudService> _logger;
        private readonly K3CloudConfig _config;
        private string _sessionId;
        private DateTime _loginTime;
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(2); // 会话超时时间

        public K3CloudService(HttpClient httpClient, IConfiguration configuration, ILogger<K3CloudService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _config = LoadConfig();
        }

        /// <summary>
        /// 从配置文件加载K3Cloud配置
        /// </summary>
        /// <returns></returns>
        private K3CloudConfig LoadConfig()
        {
            var config = new K3CloudConfig();
            _configuration.GetSection("K3Cloud").Bind(config);
            
            // 如果配置文件中没有配置，使用默认值
            if (string.IsNullOrEmpty(config.ServerUrl))
            {
                config.ServerUrl = "http://10.11.0.37";
                config.AcctId = "670b7e13fee721";
                config.Username = "ysy";
                config.AppId = "205601_x+5r1xgvSulb74+Exc2sUZzN3r2+xCLK";
                config.AppSecret = "218b484120fa49eaadec99af59e89594";
                config.Lcid = 2052;
            }
            
            return config;
        }

        /// <summary>
        /// 记录API调用日志到数据库
        /// </summary>
        protected async Task LogApiCallAsync(
            string apiName,
            string apiPath,
            string httpMethod,
            string requestParams,
            long? responseLength,
            int? statusCode,
            int status,
            string errorMessage,
            DateTime startTime,
            DateTime endTime,
            long elapsedMs,
            int? dataCount = null,
            string responseResult = null,
            string remark = null)
        {
            try
            {
                var currentUser = UserContext.Current;
                var apiLog = new OCP_ApiLog
                {
                    ApiName = apiName?.Length > 200 ? apiName.Substring(0, 200) : apiName,
                    ApiPath = apiPath?.Length > 200 ? apiPath.Substring(0, 200) : apiPath,
                    HttpMethod = httpMethod?.Length > 20 ? httpMethod.Substring(0, 20) : httpMethod,
                    RequestParams = requestParams,
                    ResponseLength = responseLength,
                    StatusCode = statusCode,
                    Status = status,
                    ErrorMessage = errorMessage,
                    StartTime = startTime,
                    EndTime = endTime,
                    ElapsedMs = elapsedMs,
                    DataCount = dataCount,
                    ResponseResult = responseResult,
                    Remark = remark?.Length > 500 ? remark.Substring(0, 500) : remark,
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
                _logger.LogError(ex, "保存K3Cloud API调用日志到数据库失败: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// 检查会话是否有效
        /// </summary>
        /// <returns></returns>
        private bool IsSessionValid()
        {
            return !string.IsNullOrEmpty(_sessionId) &&
                   DateTime.Now - _loginTime < _sessionTimeout;
        }

        /// <summary>
        /// 登录K3Cloud
        /// </summary>
        /// <returns></returns>
        public async Task<K3CloudLoginResponse> LoginAsync()
        {
            var startTime = DateTime.Now;
            var endTime = DateTime.Now;
            int? statusCode = null;
            string responseContent = null;
            string requestJson = null;

            try
            {
                                 // 如果会话仍然有效，直接返回成功
                 if (IsSessionValid())
                 {
                     return new K3CloudLoginResponse
                     {
                         KDSVCSessionId = _sessionId,
                         LoginResultType = 1
                     };
                 }

                var loginRequest = new K3CloudLoginRequest
                {
                    acctid = _config.AcctId,
                    username = _config.Username,
                    appId = _config.AppId,
                    appSecret = _config.AppSecret,
                    lcid = _config.Lcid
                };

                requestJson = JsonConvert.SerializeObject(loginRequest);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                _logger.LogInformation($"正在登录K3Cloud，URL: {_config.LoginUrl}");

                startTime = DateTime.Now;
                var response = await _httpClient.PostAsync(_config.LoginUrl, content);
                responseContent = await response.Content.ReadAsStringAsync();
                endTime = DateTime.Now;
                statusCode = (int)response.StatusCode;

                _logger.LogInformation($"K3Cloud登录响应: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonConvert.DeserializeObject<K3CloudLoginResponse>(responseContent);

                    if (loginResponse.IsSuccess)
                    {
                        _sessionId = loginResponse.KDSVCSessionId;
                        _loginTime = DateTime.Now;
                        _logger.LogInformation($"K3Cloud登录成功，SessionId: {_sessionId}");

                        // 记录成功日志
                        await LogApiCallAsync(
                            "K3Cloud登录",
                            _config.LoginUrl,
                            "POST",
                            requestJson,
                            responseContent?.Length,
                            statusCode,
                            1,
                            null,
                            startTime,
                            endTime,
                            (long)(endTime - startTime).TotalMilliseconds,
                            null,
                            null,
                            $"用户: {_config.Username}"
                        );
                    }
                    else
                    {
                        _logger.LogError($"K3Cloud登录失败: {loginResponse.Message}");

                        // 记录失败日志
                        await LogApiCallAsync(
                            "K3Cloud登录",
                            _config.LoginUrl,
                            "POST",
                            requestJson,
                            responseContent?.Length,
                            statusCode,
                            0,
                            loginResponse.Message,
                            startTime,
                            endTime,
                            (long)(endTime - startTime).TotalMilliseconds,
                            null,
                            null,
                            $"用户: {_config.Username}"
                        );
                    }

                    return loginResponse;
                }
                else
                {
                    _logger.LogError($"K3Cloud登录请求失败，状态码: {response.StatusCode}，响应: {responseContent}");

                    // 记录失败日志
                    await LogApiCallAsync(
                        "K3Cloud登录",
                        _config.LoginUrl,
                        "POST",
                        requestJson,
                        responseContent?.Length,
                        statusCode,
                        0,
                        $"HTTP状态码: {response.StatusCode}",
                        startTime,
                        endTime,
                        (long)(endTime - startTime).TotalMilliseconds,
                        null,
                        null,
                        $"用户: {_config.Username}"
                    );

                    return new K3CloudLoginResponse
                    {
                        Message = $"登录请求失败，状态码: {response.StatusCode}",
                        LoginResultType = 0
                    };
                }
            }
            catch (Exception ex)
            {
                endTime = DateTime.Now;
                _logger.LogError(ex, "K3Cloud登录异常");

                // 记录异常日志
                await LogApiCallAsync(
                    "K3Cloud登录",
                    _config.LoginUrl,
                    "POST",
                    requestJson,
                    responseContent?.Length,
                    statusCode,
                    0,
                    $"异常: {ex.Message}",
                    startTime,
                    endTime,
                    (long)(endTime - startTime).TotalMilliseconds,
                    null,
                    null,
                    $"用户: {_config.Username}"
                );

                return new K3CloudLoginResponse
                {
                    Message = $"登录异常: {ex.Message}",
                    LoginResultType = 0
                };
            }
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<K3CloudQueryResponse<T>> ExecuteQueryAsync<T>(K3CloudQueryRequest request)
        {
            var startTime = DateTime.Now;
            var endTime = DateTime.Now;
            int? statusCode = null;
            string responseContent = null;
            string requestJson = null;

            try
            {
                // 确保已登录
                var loginResponse = await LoginAsync();
                if (!loginResponse.IsSuccess)
                {
                    return new K3CloudQueryResponse<T>
                    {
                        IsSuccess = false,
                        Message = $"登录失败: {loginResponse.Message}"
                    };
                }

                requestJson = JsonConvert.SerializeObject(request);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                // 设置Cookie
                var cookieValue = $"ASP.NET_SessionId=m5rgvx01kcrkw31iybroi3ue; kdservice-sessionid={_sessionId}";
                _httpClient.DefaultRequestHeaders.Remove("Cookie");
                _httpClient.DefaultRequestHeaders.Add("Cookie", cookieValue);

                _logger.LogInformation($"正在执行K3Cloud查询，URL: {_config.QueryUrl}，参数: {requestJson}");

                startTime = DateTime.Now;
                var response = await _httpClient.PostAsync(_config.QueryUrl, content);
                responseContent = await response.Content.ReadAsStringAsync();
                endTime = DateTime.Now;
                statusCode = (int)response.StatusCode;

                _logger.LogInformation($"K3Cloud查询响应: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var queryResponse = JsonConvert.DeserializeObject<K3CloudQueryResponse<T>>(responseContent);

                    // 记录日志
                    await LogApiCallAsync(
                        "K3Cloud查询",
                        _config.QueryUrl,
                        "POST",
                        requestJson,
                        responseContent?.Length,
                        statusCode,
                        queryResponse?.IsSuccess == true ? 1 : 0,
                        queryResponse?.IsSuccess == true ? null : queryResponse?.Message,
                        startTime,
                        endTime,
                        (long)(endTime - startTime).TotalMilliseconds,
                        null,
                        null,
                        $"FormId: {request?.parameters?.FirstOrDefault()?.FormId}"
                    );

                    return queryResponse;
                }
                else
                {
                    _logger.LogError($"K3Cloud查询请求失败，状态码: {response.StatusCode}，响应: {responseContent}");

                    // 记录失败日志
                    await LogApiCallAsync(
                        "K3Cloud查询",
                        _config.QueryUrl,
                        "POST",
                        requestJson,
                        responseContent?.Length,
                        statusCode,
                        0,
                        $"HTTP状态码: {response.StatusCode}",
                        startTime,
                        endTime,
                        (long)(endTime - startTime).TotalMilliseconds,
                        null,
                        null,
                        $"FormId: {request?.parameters?.FirstOrDefault()?.FormId}"
                    );

                    return new K3CloudQueryResponse<T>
                    {
                        IsSuccess = false,
                        Message = $"查询请求失败，状态码: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                endTime = DateTime.Now;
                _logger.LogError(ex, "K3Cloud查询异常");

                // 记录异常日志
                await LogApiCallAsync(
                    "K3Cloud查询",
                    _config.QueryUrl,
                    "POST",
                    requestJson,
                    responseContent?.Length,
                    statusCode,
                    0,
                    $"异常: {ex.Message}",
                    startTime,
                    endTime,
                    (long)(endTime - startTime).TotalMilliseconds,
                    null,
                    null,
                    $"FormId: {request?.parameters?.FirstOrDefault()?.FormId}"
                );

                return new K3CloudQueryResponse<T>
                {
                    IsSuccess = false,
                    Message = $"查询异常: {ex.Message}",
                    ErrorStackTrace = ex.StackTrace
                };
            }
        }

        /// <summary>
        /// 获取物料总数
        /// </summary>
        /// <param name="filterString"></param>
        /// <returns></returns>
        public async Task<int> GetMaterialCountAsync(string filterString = null)
        {
            try
            {
                var filter = string.IsNullOrEmpty(filterString) 
                    ? "FDocumentStatus='C' and FForbidStatus='A'" 
                    : filterString;

                var request = new K3CloudQueryRequest
                {
                    parameters = new List<K3CloudQueryParameter>
                    {
                        new K3CloudQueryParameter
                        {
                            FormId = "BD_Material",
                            FieldKeys = "count(1) as count",
                            FilterString = filter
                        }
                    }
                };

                var response = await ExecuteQueryAsync<K3CloudCountData>(request);
                
                if (response.IsSuccess && response.Data?.Count > 0)
                {
                    return response.Data[0].CountValue;
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取物料总数异常");
                return 0;
            }
        }

        /// <summary>
        /// 分页获取物料数据
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="filterString"></param>
        /// <param name="orderString"></param>
        /// <returns></returns>
        public async Task<K3CloudQueryResponse<K3CloudMaterialData>> GetMaterialsAsync(
            int pageIndex = 0, 
            int pageSize = 1000, 
            string filterString = null, 
            string orderString = "FModifyDate")
        {
            try
            {
                var filter = string.IsNullOrEmpty(filterString) 
                    ? "FDocumentStatus='C' and FForbidStatus='A'" 
                    : filterString;

                var request = new K3CloudQueryRequest
                {
                    parameters = new List<K3CloudQueryParameter>
                    {
                        new K3CloudQueryParameter
                        {
                            FormId = "BD_Material",
                            FieldKeys = @"FNumber as Number,
FName as Name,
FMaterialID,
FErpClsID,
FSpecification,
F_BLN_CPXH,
F_BLN_Gctj,
F_BLN_Gcyl,
F_BLN_CV,
F_BLN_Fj,
F_BLN_DwgNum,
F_BLN_Material,
F_BLN_LLTX,
F_BLN_TLXS,
F_BLN_FLLJFS,
F_BLN_ZXJGXH,
F_BLN_ZXJGXC,
F_BLN_Flbz,
F_BLN_Ftcz,
F_BLN_Fljcz,
F_BLN_Flmfmxs,
F_TC_RELEASER,
FStockId.FNumber as FStockNumber,
FWorkShopId.FName as FWorkShopId,
FIsBOM,
FBaseUnitId.FNumber as FBaseUnitNumber,
FCreateDate,
FModifyDate,
FDocumentStatus,
FForbidStatus",
                            FilterString = filter,
                            OrderString = orderString,
                            Limit = pageSize,
                            StartRow = pageIndex * pageSize
                        } 
                    }
                };

                return await ExecuteQueryAsync<K3CloudMaterialData>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取物料数据异常");
                return new K3CloudQueryResponse<K3CloudMaterialData>
                {
                    IsSuccess = false,
                    Message = $"获取物料数据异常: {ex.Message}",
                    ErrorStackTrace = ex.StackTrace
                };
            }
        }

        /// <summary>
        /// 获取供应商总数
        /// </summary>
        /// <param name="filterString"></param>
        /// <returns></returns>
        public async Task<int> GetSupplierCountAsync(string filterString = null)
        {
            try
            {
                var filter = string.IsNullOrEmpty(filterString) 
                    ? "FDocumentStatus='C' and FForbidStatus='A'" 
                    : filterString;

                var request = new K3CloudQueryRequest
                {
                    parameters = new List<K3CloudQueryParameter>
                    {
                        new K3CloudQueryParameter
                        {
                            FormId = "BD_Supplier",
                            FieldKeys = "count(1) as count",
                            FilterString = filter
                        }
                    }
                };

                var response = await ExecuteQueryAsync<K3CloudCountData>(request);
                
                if (response.IsSuccess && response.Data?.Count > 0)
                {
                    return response.Data[0].CountValue;
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取供应商总数异常");
                return 0;
            }
        }

        /// <summary>
        /// 分页获取供应商数据
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="filterString"></param>
        /// <param name="orderString"></param>
        /// <returns></returns>
        public async Task<K3CloudQueryResponse<K3CloudSupplierData>> GetSuppliersAsync(
            int pageIndex = 0, 
            int pageSize = 1000, 
            string filterString = null, 
            string orderString = "FNumber")
        {
            try
            {
                var filter = string.IsNullOrEmpty(filterString) 
                    ? "FDocumentStatus='C' and FForbidStatus='A'" 
                    : filterString;

                var request = new K3CloudQueryRequest
                {
                    parameters = new List<K3CloudQueryParameter>
                    {
                        new K3CloudQueryParameter
                        {
                            FormId = "BD_Supplier",
                            FieldKeys = @"FNumber as Number,
FName as Name,
FSupplierId,
FDocumentStatus,
FForbidStatus,
FDescription,
FCreatorId.FName as FCreatorName,
FModifierId.FName as FModifierName,
FCreateDate,
FModifyDate,
FShortName,
F_ORA_GHFW,
FStaffId.FName as FCenterPurchaserName,
FAddress ,
FSOCIALCRECODE,
FSupplyClassify as FSupplyClassifyName,
FTel as FContactTel,
FMobile as FContactMobile",
                            FilterString = filter,
                            OrderString = orderString,
                            Limit = pageSize,
                            StartRow = pageIndex * pageSize
                        }
                    }
                };

                return await ExecuteQueryAsync<K3CloudSupplierData>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取供应商数据异常");
                return new K3CloudQueryResponse<K3CloudSupplierData>
                {
                    IsSuccess = false,
                    Message = $"获取供应商数据异常: {ex.Message}",
                    ErrorStackTrace = ex.StackTrace
                };
            }
        }

        /// <summary>
        /// 获取客户总数
        /// </summary>
        /// <param name="filterString"></param>
        /// <returns></returns>
        public async Task<int> GetCustomerCountAsync(string filterString = null)
        {
            try
            {
                var filter = string.IsNullOrEmpty(filterString) 
                    ? "FDocumentStatus='C' and FForbidStatus='A'" 
                    : filterString;

                var request = new K3CloudQueryRequest
                {
                    parameters = new List<K3CloudQueryParameter>
                    {
                        new K3CloudQueryParameter
                        {
                            FormId = "BD_Customer",
                            FieldKeys = "count(1) as count",
                            FilterString = filter
                        }
                    }
                };

                var response = await ExecuteQueryAsync<K3CloudCountData>(request);
                
                if (response.IsSuccess && response.Data?.Count > 0)
                {
                    return response.Data[0].CountValue;
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取客户总数异常");
                return 0;
            }
        }

        /// <summary>
        /// 分页获取客户数据
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="filterString"></param>
        /// <param name="orderString"></param>
        /// <returns></returns>
        public async Task<K3CloudQueryResponse<K3CloudCustomerData>> GetCustomersAsync(
            int pageIndex = 0, 
            int pageSize = 1000, 
            string filterString = null, 
            string orderString = "FNumber")
        {
            try
            {
                var filter = string.IsNullOrEmpty(filterString) 
                    ? "FDocumentStatus='C' and FForbidStatus='A'" 
                    : filterString;

                var request = new K3CloudQueryRequest
                {
                    parameters = new List<K3CloudQueryParameter>
                    {
                        new K3CloudQueryParameter
                        {
                            FormId = "BD_Customer",
                            FieldKeys = @"FNumber as Number,
FName as Name,
FCUSTID as FCustomerId,
FDocumentStatus,
FForbidStatus,
FCreatorId.FName as FCreatorName,
FModifierId.FName as FModifierName,
FCreateDate,
FModifyDate,
FTEL,
FSELLER.FName as FSalesmanName,
FAddress as FAddress,
FSOCIALCRECODE as FSOCIALCRECODE",
                            FilterString = filter,
                            OrderString = orderString,
                            Limit = pageSize,
                            StartRow = pageIndex * pageSize
                        }
                    }
                };

                return await ExecuteQueryAsync<K3CloudCustomerData>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取客户数据异常");
                return new K3CloudQueryResponse<K3CloudCustomerData>
                {
                    IsSuccess = false,
                    Message = $"获取客户数据异常: {ex.Message}",
                    ErrorStackTrace = ex.StackTrace
                };
            }
        }

        /// <summary>
        /// BOM展开
        /// </summary>
        /// <param name="materialNumber">物料编码</param>
        /// <returns>BOM展开结果</returns>
        public async Task<ServiceResult<List<BomExpandItemDto>>> ExpandBomAsync(string materialNumber)
        {
            try
            {
                // 验证参数
                if (string.IsNullOrWhiteSpace(materialNumber))
                {
                    return ServiceResult<List<BomExpandItemDto>>.Failure("物料编码不能为空！");
                }

                // 确保已登录
                var loginResponse = await LoginAsync();
                if (!loginResponse.IsSuccess)
                {
                    return ServiceResult<List<BomExpandItemDto>>.Failure("上下文丢失，请重新登录！");
                }

                // 构建请求（使用K3Cloud要求的格式）
                var requestWrapper = new BomExpandRequestWrapper
                {
                    parameters = new List<BomExpandRequestDto>
                    {
                        new BomExpandRequestDto
                        {
                            MaterialNumber = materialNumber
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(requestWrapper);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 设置Cookie
                var cookieValue = $"ASP.NET_SessionId=m5rgvx01kcrkw31iybroi3ue; kdservice-sessionid={_sessionId}";
                _httpClient.DefaultRequestHeaders.Remove("Cookie");
                _httpClient.DefaultRequestHeaders.Add("Cookie", cookieValue);

                _logger.LogInformation("正在执行BOM展开，URL: {BomExpandUrl}，参数: {RequestJson}", _config.BomExpandUrl, json);

                var response = await _httpClient.PostAsync(_config.BomExpandUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("BOM展开响应: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ServiceResult<List<BomExpandItemDto>>>(responseContent);

                    if (result == null)
                    {
                        return ServiceResult<List<BomExpandItemDto>>.Failure("BOM展开响应解析失败");
                    }

                    return result;
                }
                else
                {
                    _logger.LogError("BOM展开请求失败，状态码: {StatusCode}，响应: {ResponseContent}", response.StatusCode, responseContent);
                    return ServiceResult<List<BomExpandItemDto>>.Failure($"BOM展开请求失败，状态码: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BOM展开异常，物料编码: {MaterialNumber}", materialNumber);
                return ServiceResult<List<BomExpandItemDto>>.Failure($"BOM展开异常: {ex.Message}");
            }
        }
    }
}