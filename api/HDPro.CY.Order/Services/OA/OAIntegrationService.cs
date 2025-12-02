/*
 * OA集成服务
 * 提供与OA系统集成的公共服务功能
 * 包括获取Token和推送消息等功能
 */
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Utilities;
using HDPro.Core.ManageUser;
using HDPro.CY.Order.IServices.OA;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.Services.Common;
using HDPro.Entity.DomainModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OA
{
    /// <summary>
    /// OA集成服务
    /// 提供与OA系统的集成功能
    /// </summary>
    public class OAIntegrationService : IOAIntegrationService, IDependency
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OAIntegrationService> _logger;
        private readonly IConfiguration _configuration;

        // OA专用日志记录器
        private readonly OALogger _oaMessageLogger;
        private readonly OALogger _oaProcessLogger;
        private readonly OALogger _oaTokenLogger;
        private readonly OALogger _shareholderMessageLogger;
        private readonly OALogger _shareholderProcessLogger;
        
        // OA系统配置
        private readonly string _oaBaseUrl;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _templateCode;
        private readonly string _appName;
        private readonly bool _useShareholderOA;
        
        // 股份OA系统配置
        private readonly string _shareholderOABaseUrl;
        private readonly string _shareholderOAExternalBaseUrl;
        private readonly string _shareholderUserName;
        private readonly string _shareholderPassword;
        private readonly string _shareholderTemplateCode;
        private readonly string _shareholderAppName;
        private readonly bool _useExternal;

        public OAIntegrationService(
            HttpClient httpClient,
            ILogger<OAIntegrationService> logger,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;

            // 初始化OA专用日志记录器
            _oaMessageLogger = OALoggerFactory.CreateMessageLogger(loggerFactory);
            _oaProcessLogger = OALoggerFactory.CreateProcessLogger(loggerFactory);
            _oaTokenLogger = OALoggerFactory.CreateTokenLogger(loggerFactory);
            _shareholderMessageLogger = OALoggerFactory.CreateShareholderMessageLogger(loggerFactory);
            _shareholderProcessLogger = OALoggerFactory.CreateShareholderProcessLogger(loggerFactory);
            
            // 从配置文件读取OA系统配置
            _oaBaseUrl = _configuration["OA:BaseUrl"] ?? "http://10.11.0.81:8090";
            _userName = _configuration["OA:UserName"] ?? "bhgpbjd";
            _password = _configuration["OA:Password"] ?? "ce77172d-47fe-4d13-a138-ca9dcb4e0bcc";
            _templateCode = _configuration["OA:TemplateCode"] ?? "tjf_ddjqbg";
            _appName = _configuration["OA:AppName"] ?? "collaboration";
            _useShareholderOA = bool.Parse(_configuration["OA:UseShareholderOA"] ?? "false");
            
            // 从配置文件读取股份OA系统配置
            _shareholderOABaseUrl = _configuration["ShareholderOA:BaseUrl"] ?? "http://10.95.0.2:8082";
            _shareholderOAExternalBaseUrl = _configuration["ShareholderOA:ExternalBaseUrl"] ?? "http://oa.sicc.com.cn:8082";
            _shareholderUserName = _configuration["ShareholderOA:UserName"] ?? "rest_tjf";
            _shareholderPassword = _configuration["ShareholderOA:Password"] ?? "98b73e39-a8ef-4e1a-932c-200ee5025d85";
            _shareholderTemplateCode = _configuration["ShareholderOA:TemplateCode"] ?? "tjf_ddjqbg";
            _shareholderAppName = _configuration["ShareholderOA:AppName"] ?? "collaboration";
            _useExternal = bool.Parse(_configuration["ShareholderOA:UseExternal"] ?? "false");
            
            _logger.LogInformation("OA集成服务初始化完成 - BaseUrl: {BaseUrl}, UserName: {UserName}, TemplateCode: {TemplateCode}, AppName: {AppName}, UseShareholderOA: {UseShareholderOA}",
                _oaBaseUrl, _userName, _templateCode, _appName, _useShareholderOA);
            _logger.LogInformation("股份OA配置 - BaseUrl: {ShareholderBaseUrl}, UserName: {ShareholderUserName}, UseExternal: {UseExternal}",
                _shareholderOABaseUrl, _shareholderUserName, _useExternal);
        }

        /// <summary>
        /// 映射管理员账号到OA系统登录名
        /// 对于管理员账号（admin、cyadmin），映射为默认的OA登录名 015206
        /// </summary>
        /// <param name="loginName">原始登录名</param>
        /// <returns>映射后的OA登录名</returns>
        private string MapAdminLoginName(string loginName)
        {
            if (string.IsNullOrEmpty(loginName))
            {
                return loginName;
            }

            // 管理员账号映射到默认OA登录名
            if (loginName.Equals("admin", StringComparison.OrdinalIgnoreCase) || 
                loginName.Equals("cyadmin", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("管理员账号 {OriginalLoginName} 映射为 015206", loginName);
                return "015206";
            }

            return loginName;
        }

        /// <summary>
        /// 批量映射登录名列表中的管理员账号
        /// </summary>
        /// <param name="loginNames">原始登录名列表</param>
        /// <returns>映射后的登录名列表</returns>
        private List<string> MapAdminLoginNames(List<string> loginNames)
        {
            if (loginNames == null || !loginNames.Any())
            {
                return loginNames;
            }

            return loginNames.Select(MapAdminLoginName).ToList();
        }

        /// <summary>
        /// 获取OA系统Token
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <returns>Token字符串</returns>
        public async Task<WebResponseContent> GetTokenAsync(string loginName)
        {
            var response = new WebResponseContent();
            var tokenUrl = string.Empty;
            var requestJson = string.Empty;
            var responseContent = string.Empty;
            int? statusCode = null;
            ApiLogHelper logHelper = null;

            try
            {
                if (string.IsNullOrEmpty(loginName))
                {
                    _oaTokenLogger.LogWarning("获取OA Token失败：登录名不能为空");
                    response.Status = false;
                    response.Message = "登录名不能为空";
                    return response;
                }

                // 管理员账号映射到默认OA登录名
                loginName = MapAdminLoginName(loginName);
                tokenUrl = $"{_oaBaseUrl}/seeyon/rest/token";

                var requestBody = new
                {
                    userName = _userName,
                    password = _password,
                    loginName = loginName
                };

                requestJson = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                _oaTokenLogger.LogInfo("开始获取OA Token - URL: {TokenUrl}, LoginName: {LoginName}", tokenUrl, loginName);
                _oaTokenLogger.LogRequestParameters("OA Token", requestJson);

                // 开始记录日志
                logHelper = ApiLogHelper.Start("OA获取Token", tokenUrl, "POST", requestJson, _logger)
                    .WithRemark($"LoginName: {loginName}");

                var httpResponse = await _httpClient.PostAsync(tokenUrl, content);
                responseContent = await httpResponse.Content.ReadAsStringAsync();
                statusCode = (int)httpResponse.StatusCode;

                _oaTokenLogger.LogHttpRequest("POST", tokenUrl, statusCode.Value, 0);
                _oaTokenLogger.LogResponseContent("OA Token", responseContent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var tokenResponse = JsonConvert.DeserializeObject<OATokenResponse>(responseContent);

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Id))
                    {
                        response.Status = true;
                        response.Data = tokenResponse;
                        response.Message = "获取Token成功";

                        _oaTokenLogger.LogTokenOperation(loginName, true, $"TokenId: {tokenResponse.Id}, UserId: {tokenResponse.BindingUser?.Id}, UserName: {tokenResponse.BindingUser?.Name}");

                        // 记录成功日志
                        await logHelper.WithResponseResult(responseContent).LogSuccessAsync(statusCode, responseContent);
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Token响应格式错误";
                        _oaTokenLogger.LogTokenOperation(loginName, false, "Token响应格式错误，无法解析Token信息");

                        // 记录失败日志
                        await logHelper.WithResponseResult(responseContent).LogFailureAsync("Token响应格式错误", statusCode, responseContent);
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = $"获取Token失败，HTTP状态码: {httpResponse.StatusCode}";
                    _oaTokenLogger.LogTokenOperation(loginName, false, $"HTTP状态码: {httpResponse.StatusCode}, 原因: {httpResponse.ReasonPhrase}");

                    // 记录失败日志
                    await logHelper.WithResponseResult(responseContent).LogFailureAsync($"HTTP状态码: {httpResponse.StatusCode}", statusCode, responseContent);
                }
            }
            catch (HttpRequestException ex)
            {
                response.Status = false;
                response.Message = $"获取Token网络异常: {ex.Message}";
                _oaTokenLogger.LogError(ex, "获取OA Token时发生网络异常 - URL: {TokenUrl}, 请求参数: {RequestJson}", tokenUrl, requestJson);

                // 记录异常日志
                if (logHelper != null)
                    await logHelper.LogExceptionAsync(ex, statusCode);
            }
            catch (JsonException ex)
            {
                response.Status = false;
                response.Message = $"获取Token数据格式异常: {ex.Message}";
                _oaTokenLogger.LogError(ex, "获取OA Token时JSON序列化异常 - 请求JSON: {RequestJson}, 响应内容: {ResponseContent}", requestJson, responseContent);

                // 记录异常日志
                if (logHelper != null)
                    await logHelper.WithResponseResult(responseContent).LogExceptionAsync(ex, statusCode);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"获取Token异常: {ex.Message}";
                _oaTokenLogger.LogError(ex, "获取OA Token时发生未知异常 - URL: {TokenUrl}, 登录名: {LoginName}", tokenUrl, loginName);

                // 记录异常日志
                if (logHelper != null)
                    await logHelper.LogExceptionAsync(ex, statusCode);
            }

            return response;
        }

        /// <summary>
        /// 推送消息到OA系统
        /// </summary>
        /// <param name="token">认证Token</param>
        /// <param name="sendUserId">发送者用户ID</param>
        /// <param name="loginNames">接收者登录名列表</param>
        /// <param name="content">消息内容</param>
        /// <param name="urls">附件或扩展链接数组（可为空）</param>
        /// <returns>推送结果</returns>
        public async Task<WebResponseContent> SendMessageAsync(
            string token, 
            string sendUserId, 
            List<string> loginNames, 
            string content, 
            List<string> urls = null)
        {
            var response = new WebResponseContent();
            var messageUrl = string.Empty;
            var requestJson = string.Empty;
            var responseContent = string.Empty;
            
            try
            {
                // 参数验证
                if (string.IsNullOrEmpty(token))
                {
                    _oaMessageLogger.LogWarning("推送OA消息失败：Token不能为空");
                    response.Status = false;
                    response.Message = "Token不能为空";
                    return response;
                }

                if (string.IsNullOrEmpty(sendUserId))
                {
                    _oaMessageLogger.LogWarning("推送OA消息失败：发送者用户ID不能为空");
                    response.Status = false;
                    response.Message = "发送者用户ID不能为空";
                    return response;
                }

                if (loginNames == null || loginNames.Count == 0)
                {
                    _oaMessageLogger.LogWarning("推送OA消息失败：接收者登录名列表不能为空");
                    response.Status = false;
                    response.Message = "接收者登录名列表不能为空";
                    return response;
                }

                if (string.IsNullOrEmpty(content))
                {
                    _oaMessageLogger.LogWarning("推送OA消息失败：消息内容不能为空");
                    response.Status = false;
                    response.Message = "消息内容不能为空";
                    return response;
                }

                // 映射接收者登录名中的管理员账号
                loginNames = MapAdminLoginNames(loginNames);

                messageUrl = $"{_oaBaseUrl}/seeyon/rest/message?token={token}";
                
                var requestBody = new
                {
                    sendUserId = sendUserId,
                    loginNames = loginNames,
                    content = content,
                    url = urls ?? new List<string>()
                };

                requestJson = JsonConvert.SerializeObject(requestBody);
                var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                _oaMessageLogger.LogInfo("开始推送OA消息 - URL: {MessageUrl}, 发送者: {SendUserId}, 接收者: {Receivers}, 内容长度: {ContentLength}字符",
                    messageUrl, sendUserId, string.Join(",", loginNames), content.Length);
                _oaMessageLogger.LogRequestParameters("OA消息推送", requestJson);

                // 记录请求开始时间
                var startTime = DateTime.Now;

                var httpResponse = await _httpClient.PostAsync(messageUrl, httpContent);
                responseContent = await httpResponse.Content.ReadAsStringAsync();

                // 记录请求结束时间和耗时
                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                _oaMessageLogger.LogHttpRequest("POST", messageUrl, (int)httpResponse.StatusCode, duration.TotalMilliseconds);
                _oaMessageLogger.LogResponseContent("OA消息推送", responseContent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var messageResponse = JsonConvert.DeserializeObject<OAMessageResponse>(responseContent);
                    
                    if (messageResponse != null)
                    {
                        _logger.LogInformation("OA消息响应解析成功 - 结果码: {Result}, 错误码: {ErrorNumber}, 错误消息: {ErrorMessage}",
                            messageResponse.Result, messageResponse.ErrorNumber, messageResponse.ErrorMessage);

                        if (messageResponse.Result == 1)
                        {
                            response.Status = true;
                            response.Message = "消息推送成功";
                            
                            _logger.LogInformation("OA消息推送成功，接收者: {Receivers}", string.Join(",", loginNames));
                        }
                        else
                        {
                            response.Status = false;
                            response.Message = $"消息推送失败: {messageResponse.ErrorMessage ?? "未知错误"}";
                            _logger.LogError("OA消息推送失败 - 错误码: {ErrorNumber}, 错误消息: {ErrorMessage}",
                                messageResponse.ErrorNumber, messageResponse.ErrorMessage);
                        }
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "消息响应格式错误";
                        _logger.LogError("OA消息响应解析失败，响应内容为空或格式错误");
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = $"消息推送失败，HTTP状态码: {httpResponse.StatusCode}";
                    _logger.LogError("OA消息HTTP请求失败 - 状态码: {StatusCode}, 原因: {ReasonPhrase}",
                        httpResponse.StatusCode, httpResponse.ReasonPhrase);
                }
            }
            catch (HttpRequestException ex)
            {
                response.Status = false;
                response.Message = $"推送消息网络异常: {ex.Message}";
                _logger.LogError(ex, "推送OA消息时发生网络异常 - URL: {MessageUrl}, 请求参数: {RequestJson}",
                    messageUrl, requestJson);
            }
            catch (JsonException ex)
            {
                response.Status = false;
                response.Message = $"推送消息数据格式异常: {ex.Message}";
                _logger.LogError(ex, "推送OA消息时JSON序列化异常 - 请求JSON: {RequestJson}, 响应内容: {ResponseContent}",
                    requestJson, responseContent);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"推送消息异常: {ex.Message}";
                _logger.LogError(ex, "推送OA消息时发生未知异常 - URL: {MessageUrl}, 发送者: {SendUserId}, 接收者: {Receivers}",
                    messageUrl, sendUserId, loginNames != null ? string.Join(",", loginNames) : "空");
            }

            return response;
        }

        /// <summary>
        /// 获取Token并推送消息（便捷方法）
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="content">消息内容</param>
        /// <param name="urls">附件或扩展链接数组（可为空）</param>
        /// <returns>推送结果</returns>
        public async Task<WebResponseContent> GetTokenAndSendMessageAsync(
            string senderLoginName,
            List<string> receiverLoginNames, 
            string content, 
            List<string> urls = null)
        {
            var response = new WebResponseContent();
            
            try
            {
                _logger.LogInformation("开始获取Token并推送OA消息 - 发送者: {SenderLoginName}, 接收者: {Receivers}, 内容长度: {ContentLength}字符",
                    senderLoginName, string.Join(",", receiverLoginNames ?? new List<string>()), content?.Length ?? 0);

                // 先获取Token和用户信息
                var tokenResult = await GetTokenAsync(senderLoginName);
                if (!tokenResult.Status)
                {
                    response.Status = false;
                    response.Message = $"获取Token失败: {tokenResult.Message}";
                    _logger.LogError("获取Token失败，无法推送消息: {ErrorMessage}", tokenResult.Message);
                    return response;
                }

                // 从Token响应中提取Token和发送者用户ID
                var tokenResponse = tokenResult.Data as OATokenResponse;
                if (tokenResponse == null)
                {
                    response.Status = false;
                    response.Message = "Token响应数据格式错误";
                    _logger.LogError("Token响应数据格式错误，无法推送消息");
                    return response;
                }

                var token = tokenResponse.Id;
                var sendUserId = tokenResponse.BindingUser?.Id.ToString();

                if (string.IsNullOrEmpty(sendUserId))
                {
                    response.Status = false;
                    response.Message = "无法获取发送者用户ID";
                    _logger.LogError("无法从Token响应中获取发送者用户ID");
                    return response;
                }

                _logger.LogInformation("成功获取Token，开始推送消息 - TokenId: {TokenId}, SendUserId: {SendUserId}",
                    token, sendUserId);

                // 使用Token和发送者ID推送消息
                return await SendMessageAsync(token, sendUserId, receiverLoginNames, content, urls);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"获取Token并推送消息异常: {ex.Message}";
                _logger.LogError(ex, "获取Token并推送OA消息时发生异常 - 发送者: {SenderLoginName}",
                    senderLoginName);
            }

            return response;
        }

        /// <summary>
        /// 推送订单相关消息的便捷方法
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="orderType">订单类型</param>
        /// <param name="messageType">消息类型（如：催单、协商等）</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="customContent">自定义消息内容（可选）</param>
        /// <returns>推送结果</returns>
        public async Task<WebResponseContent> SendOrderMessageAsync(
            string senderLoginName,
            string orderNo,
            string orderType,
            string messageType,
            List<string> receiverLoginNames,
            string customContent = null)
        {
            var response = new WebResponseContent();
            
            try
            {
                // 构建消息内容
                var content = customContent ?? $"【{messageType}】协同平台通知：{orderType}订单 {orderNo} 需要您的处理，请及时查看。";
                
                _logger.LogInformation("开始推送订单股份OA消息 - 发送者: {SenderLoginName}, 订单号: {OrderNo}, 订单类型: {OrderType}, 消息类型: {MessageType}, 接收者: {Receivers}",
                    senderLoginName, orderNo, orderType, messageType, string.Join(",", receiverLoginNames ?? new List<string>()));

                return await GetShareholderTokenAndSendMessageAsync(senderLoginName, receiverLoginNames, content);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"推送订单消息异常: {ex.Message}";
                _logger.LogError(ex, "推送订单消息时发生异常 - 订单号: {OrderNo}, 消息类型: {MessageType}",
                    orderNo, messageType);
            }

            return response;
        }

        /// <summary>
        /// 发起OA流程
        /// </summary>
        /// <param name="token">认证Token</param>
        /// <param name="formData">表单数据</param>
        /// <returns>流程发起结果</returns>
        public async Task<WebResponseContent> StartProcessAsync(string token, OAFormData formData)
        {
            var response = new WebResponseContent();
            var processUrl = string.Empty;
            var requestJson = string.Empty;
            var responseContent = string.Empty;
            
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("发起OA流程失败：Token不能为空");
                    response.Status = false;
                    response.Message = "Token不能为空";
                    return response;
                }

                if (formData == null)
                {
                    _logger.LogWarning("发起OA流程失败：表单数据不能为空");
                    response.Status = false;
                    response.Message = "表单数据不能为空";
                    return response;
                }

                processUrl = $"{_oaBaseUrl}/seeyon/rest/bpm/process/start?token={token}";
                
                var requestBody = new
                {
                    appName = _appName,
                    data = new
                    {
                        data = new
                        {
                            formmain_0894 = formData
                        },
                        templateCode = _templateCode,
                        draft = "0"
                    }
                };

                requestJson = JsonConvert.SerializeObject(requestBody);
                var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                _logger.LogInformation("开始发起OA流程 - URL: {ProcessUrl}, 模板代码: {TemplateCode}, 应用名称: {AppName}",
                    processUrl, _templateCode, _appName);
                _logger.LogInformation("OA流程请求参数: {RequestJson}", requestJson);

                // 记录请求开始时间
                var startTime = DateTime.Now;

                var httpResponse = await _httpClient.PostAsync(processUrl, httpContent);
                responseContent = await httpResponse.Content.ReadAsStringAsync();

                // 记录请求结束时间和耗时
                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                _logger.LogInformation("OA流程请求完成 - 状态码: {StatusCode}, 耗时: {Duration}ms, 响应长度: {ResponseLength}字符",
                    httpResponse.StatusCode, duration.TotalMilliseconds, responseContent?.Length ?? 0);
                
                _logger.LogInformation("OA流程响应内容: {ResponseContent}", responseContent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var processResponse = JsonConvert.DeserializeObject<OAProcessResponse>(responseContent);
                    
                    if (processResponse != null)
                    {
                        _logger.LogInformation("OA流程响应解析成功 - 状态码: {Code}, 流程ID: {ProcessId}, 主题: {Subject}",
                            processResponse.Code, processResponse.Data?.ProcessId, processResponse.Data?.Subject);

                        if (processResponse.Code == 0)
                        {
                            response.Status = true;
                            response.Data = processResponse.Data;
                            response.Message = "流程发起成功";
                            
                            _logger.LogInformation("OA流程发起成功，流程ID: {ProcessId}", processResponse.Data?.ProcessId);
                        }
                        else
                        {
                            response.Status = false;
                            response.Message = $"流程发起失败: {processResponse.Message ?? "未知错误"}";
                            _logger.LogError("OA流程发起失败 - 错误码: {Code}, 错误消息: {Message}",
                                processResponse.Code, processResponse.Message);
                        }
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "流程响应格式错误";
                        _logger.LogError("OA流程响应解析失败，响应内容为空或格式错误");
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = $"流程发起失败，HTTP状态码: {httpResponse.StatusCode}";
                    _logger.LogError("OA流程HTTP请求失败 - 状态码: {StatusCode}, 原因: {ReasonPhrase}",
                        httpResponse.StatusCode, httpResponse.ReasonPhrase);
                }
            }
            catch (HttpRequestException ex)
            {
                response.Status = false;
                response.Message = $"发起流程网络异常: {ex.Message}";
                _logger.LogError(ex, "发起OA流程时发生网络异常 - URL: {ProcessUrl}, 请求参数: {RequestJson}",
                    processUrl, requestJson);
            }
            catch (JsonException ex)
            {
                response.Status = false;
                response.Message = $"发起流程数据格式异常: {ex.Message}";
                _logger.LogError(ex, "发起OA流程时JSON序列化异常 - 请求JSON: {RequestJson}, 响应内容: {ResponseContent}",
                    requestJson, responseContent);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"发起流程异常: {ex.Message}";
                _logger.LogError(ex, "发起OA流程时发生未知异常 - URL: {ProcessUrl}, 模板代码: {TemplateCode}",
                    processUrl, _templateCode);
            }

            return response;
        }

        /// <summary>
        /// 获取Token并发起流程（便捷方法）
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="formData">表单数据</param>
        /// <returns>流程发起结果</returns>
        public async Task<WebResponseContent> GetTokenAndStartProcessAsync(string loginName, OAFormData formData)
        {
            var response = new WebResponseContent();
            
            try
            {
                _logger.LogInformation("开始获取Token并发起OA流程 - 登录名: {LoginName}",
                    loginName);

                // 先获取Token
                var tokenResult = await GetTokenAsync(loginName);
                if (!tokenResult.Status)
                {
                    response.Status = false;
                    response.Message = $"获取Token失败: {tokenResult.Message}";
                    _logger.LogError("获取Token失败，无法发起流程: {ErrorMessage}", tokenResult.Message);
                    return response;
                }

                // 从Token响应中提取Token
                var tokenResponse = tokenResult.Data as OATokenResponse;
                if (tokenResponse == null)
                {
                    response.Status = false;
                    response.Message = "Token响应数据格式错误";
                    _logger.LogError("Token响应数据格式错误，无法发起流程");
                    return response;
                }

                _logger.LogInformation("成功获取Token，开始发起流程 - TokenId: {TokenId}",
                    tokenResponse.Id);

                // 使用Token发起流程
                return await StartProcessAsync(tokenResponse.Id, formData);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"获取Token并发起流程异常: {ex.Message}";
                _logger.LogError(ex, "获取Token并发起OA流程时发生异常 - 登录名: {LoginName}",
                    loginName);
            }

            return response;
        }

        #region 股份OA流程接口实现

        /// <summary>
        /// 获取股份OA系统Token
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <returns>Token字符串</returns>
        public async Task<WebResponseContent> GetShareholderTokenAsync(string loginName)
        {
            var response = new WebResponseContent();
            var tokenUrl = string.Empty;
            var requestJson = string.Empty;
            var responseContent = string.Empty;
            
            try
            {
                if (string.IsNullOrEmpty(loginName))
                {
                    _logger.LogWarning("获取股份OA Token失败：登录名不能为空");
                    response.Status = false;
                    response.Message = "登录名不能为空";
                    return response;
                }

                // 管理员账号映射到默认OA登录名
                loginName = MapAdminLoginName(loginName);
                
                // 根据配置选择使用内网还是外网地址
                var baseUrl = _useExternal ? _shareholderOAExternalBaseUrl : _shareholderOABaseUrl;
                tokenUrl = $"{baseUrl}/seeyon/rest/token";
                
                var requestBody = new
                {
                    userName = _shareholderUserName,
                    password = _shareholderPassword,
                    loginName = loginName
                };

                requestJson = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                _logger.LogInformation("开始获取股份OA Token - URL: {TokenUrl}, LoginName: {LoginName}", tokenUrl, loginName);
                _logger.LogInformation("股份OA Token请求参数: {RequestJson}", requestJson);

                // 记录请求开始时间
                var startTime = DateTime.Now;

                var httpResponse = await _httpClient.PostAsync(tokenUrl, content);
                responseContent = await httpResponse.Content.ReadAsStringAsync();

                // 记录请求结束时间和耗时
                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                _logger.LogInformation("股份OA Token请求完成 - 状态码: {StatusCode}, 耗时: {Duration}ms, 响应长度: {ResponseLength}字符",
                    httpResponse.StatusCode, duration.TotalMilliseconds, responseContent?.Length ?? 0);
                
                _logger.LogInformation("股份OA Token响应内容: {ResponseContent}", responseContent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var tokenResponse = JsonConvert.DeserializeObject<OATokenResponse>(responseContent);
                    
                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Id))
                    {
                        response.Status = true;
                        response.Data = tokenResponse; // 返回完整的Token响应对象
                        response.Message = "获取Token成功";
                        
                        _logger.LogInformation("获取股份OA Token成功 - TokenId: {TokenId}, UserId: {UserId}, UserName: {UserName}",
                            tokenResponse.Id, tokenResponse.BindingUser?.Id, tokenResponse.BindingUser?.Name);
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Token响应格式错误";
                        _logger.LogError("股份OA Token响应格式错误，无法解析Token信息");
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = $"获取Token失败，HTTP状态码: {httpResponse.StatusCode}";
                    _logger.LogError("获取股份OA Token失败 - 状态码: {StatusCode}, 原因: {ReasonPhrase}",
                        httpResponse.StatusCode, httpResponse.ReasonPhrase);
                }
            }
            catch (HttpRequestException ex)
            {
                response.Status = false;
                response.Message = $"获取Token网络异常: {ex.Message}";
                _logger.LogError(ex, "获取股份OA Token时发生网络异常 - URL: {TokenUrl}, 请求参数: {RequestJson}",
                    tokenUrl, requestJson);
            }
            catch (JsonException ex)
            {
                response.Status = false;
                response.Message = $"获取Token数据格式异常: {ex.Message}";
                _logger.LogError(ex, "获取股份OA Token时JSON序列化异常 - 请求JSON: {RequestJson}, 响应内容: {ResponseContent}",
                    requestJson, responseContent);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"获取Token异常: {ex.Message}";
                _logger.LogError(ex, "获取股份OA Token时发生未知异常 - URL: {TokenUrl}, 登录名: {LoginName}",
                    tokenUrl, loginName);
            }

            return response;
        }

        /// <summary>
        /// 发起股份OA流程
        /// </summary>
        /// <param name="token">认证Token</param>
        /// <param name="formData">股份表单数据</param>
        /// <returns>流程发起结果</returns>
        public async Task<WebResponseContent> StartShareholderProcessAsync(string token, ShareholderOAFormData formData)
        {
            var response = new WebResponseContent();
            var processUrl = string.Empty;
            var requestJson = string.Empty;
            var responseContent = string.Empty;
            
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("发起股份OA流程失败：Token不能为空");
                    response.Status = false;
                    response.Message = "Token不能为空";
                    return response;
                }

                if (formData == null)
                {
                    _logger.LogWarning("发起股份OA流程失败：表单数据不能为空");
                    response.Status = false;
                    response.Message = "表单数据不能为空";
                    return response;
                }

                // 根据配置选择使用内网还是外网地址
                var baseUrl = _useExternal ? _shareholderOAExternalBaseUrl : _shareholderOABaseUrl;
                processUrl = $"{baseUrl}/seeyon/rest/bpm/process/start?token={token}";
                
                var requestBody = new
                {
                    appName = _shareholderAppName,
                    data = new
                    {
                        data = new
                        {
                            formmain_7280 = formData
                        },
                        templateCode = _shareholderTemplateCode,
                        draft = "0"
                    }
                };

                requestJson = JsonConvert.SerializeObject(requestBody);
                var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                _logger.LogInformation("开始发起股份OA流程 - URL: {ProcessUrl}, 模板代码: {TemplateCode}, 应用名称: {AppName}",
                    processUrl, _shareholderTemplateCode, _shareholderAppName);
                _logger.LogInformation("股份OA流程请求参数: {RequestJson}", requestJson);

                // 记录请求开始时间
                var startTime = DateTime.Now;

                var httpResponse = await _httpClient.PostAsync(processUrl, httpContent);
                responseContent = await httpResponse.Content.ReadAsStringAsync();

                // 记录请求结束时间和耗时
                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                _logger.LogInformation("股份OA流程请求完成 - 状态码: {StatusCode}, 耗时: {Duration}ms, 响应长度: {ResponseLength}字符",
                    httpResponse.StatusCode, duration.TotalMilliseconds, responseContent?.Length ?? 0);
                
                _logger.LogInformation("股份OA流程响应内容: {ResponseContent}", responseContent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var processResponse = JsonConvert.DeserializeObject<OAProcessResponse>(responseContent);
                    
                    if (processResponse != null)
                    {
                        _logger.LogInformation("股份OA流程响应解析成功 - 状态码: {Code}, 流程ID: {ProcessId}, 主题: {Subject}",
                            processResponse.Code, processResponse.Data?.ProcessId, processResponse.Data?.Subject);

                        if (processResponse.Code == 0)
                        {
                            response.Status = true;
                            response.Data = processResponse.Data;
                            response.Message = "流程发起成功";
                            
                            _logger.LogInformation("股份OA流程发起成功，流程ID: {ProcessId}", processResponse.Data?.ProcessId);
                        }
                        else
                        {
                            response.Status = false;
                            response.Message = $"流程发起失败: {processResponse.Message ?? "未知错误"}";
                            _logger.LogError("股份OA流程发起失败 - 错误码: {Code}, 错误消息: {Message}",
                                processResponse.Code, processResponse.Message);
                        }
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "流程响应格式错误";
                        _logger.LogError("股份OA流程响应解析失败，响应内容为空或格式错误");
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = $"流程发起失败，HTTP状态码: {httpResponse.StatusCode}";
                    _logger.LogError("股份OA流程HTTP请求失败 - 状态码: {StatusCode}, 原因: {ReasonPhrase}",
                        httpResponse.StatusCode, httpResponse.ReasonPhrase);
                }
            }
            catch (HttpRequestException ex)
            {
                response.Status = false;
                response.Message = $"发起流程网络异常: {ex.Message}";
                _logger.LogError(ex, "发起股份OA流程时发生网络异常 - URL: {ProcessUrl}, 请求参数: {RequestJson}",
                    processUrl, requestJson);
            }
            catch (JsonException ex)
            {
                response.Status = false;
                response.Message = $"发起流程数据格式异常: {ex.Message}";
                _logger.LogError(ex, "发起股份OA流程时JSON序列化异常 - 请求JSON: {RequestJson}, 响应内容: {ResponseContent}",
                    requestJson, responseContent);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"发起流程异常: {ex.Message}";
                _logger.LogError(ex, "发起股份OA流程时发生未知异常 - URL: {ProcessUrl}, 模板代码: {TemplateCode}",
                    processUrl, _shareholderTemplateCode);
            }

            return response;
        }

        /// <summary>
        /// 获取Token并发起股份流程（便捷方法）
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="formData">股份表单数据</param>
        /// <returns>流程发起结果</returns>
        public async Task<WebResponseContent> GetShareholderTokenAndStartProcessAsync(string loginName, ShareholderOAFormData formData)
        {
            var response = new WebResponseContent();
            
            try
            {
                _logger.LogInformation("开始获取Token并发起股份OA流程 - 登录名: {LoginName}",
                    loginName);

                // 先获取Token
                var tokenResult = await GetShareholderTokenAsync(loginName);
                if (!tokenResult.Status)
                {
                    response.Status = false;
                    response.Message = $"获取Token失败: {tokenResult.Message}";
                    _logger.LogError("获取股份Token失败，无法发起流程: {ErrorMessage}", tokenResult.Message);
                    return response;
                }

                // 从Token响应中提取Token
                var tokenResponse = tokenResult.Data as OATokenResponse;
                if (tokenResponse == null)
                {
                    response.Status = false;
                    response.Message = "Token响应数据格式错误";
                    _logger.LogError("股份Token响应数据格式错误，无法发起流程");
                    return response;
                }

                _logger.LogInformation("成功获取股份Token，开始发起流程 - TokenId: {TokenId}",
                    tokenResponse.Id);

                // 使用Token发起流程
                return await StartShareholderProcessAsync(tokenResponse.Id, formData);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"获取Token并发起股份流程异常: {ex.Message}";
                _logger.LogError(ex, "获取Token并发起股份OA流程时发生异常 - 登录名: {LoginName}",
                    loginName);
            }

            return response;
        }

        /// <summary>
        /// 智能发起流程（根据配置自动选择使用原有OA还是股份OA）
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="processData">流程数据</param>
        /// <returns>流程发起结果</returns>
        public async Task<WebResponseContent> SmartStartProcessAsync(string loginName, SmartOAProcessData processData)
        {
            var response = new WebResponseContent();
            
            try
            {
                if (processData == null)
                {
                    response.Status = false;
                    response.Message = "流程数据不能为空";
                    _logger.LogWarning("智能发起OA流程失败：流程数据不能为空");
                    return response;
                }

                _logger.LogInformation("智能发起OA流程 - 登录名: {LoginName}, 订单号: {OrderNo}, 使用股份OA: {UseShareholderOA}, 指定负责人: {AssignedPerson}",
                    loginName, processData.OrderNo, _useShareholderOA, processData.AssignedResponsiblePerson);

                if (_useShareholderOA)
                {
                    // 使用股份OA流程 - 先获取token，然后用于人员查询和流程发起
                    string sharedToken = null;
                    
                    // 先获取Token
                    var tokenResult = await GetShareholderTokenAsync(loginName);
                    if (tokenResult.Status && tokenResult.Data is OATokenResponse tokenResponse)
                    {
                        sharedToken = tokenResponse.Id;
                        _logger.LogInformation("成功获取股份OA Token - TokenId: {TokenId}, 将用于人员查询和流程发起", sharedToken);
                    }
                    else
                    {
                        _logger.LogWarning("获取股份OA Token失败 - 错误: {Error}, 将继续使用原有方式", tokenResult.Message);
                    }

                    // 如果有指定负责人，使用共享token获取对应的OA人员ID作为审批人
                    if (!string.IsNullOrEmpty(processData.AssignedResponsiblePerson))
                    {
                        try
                        {
                            // 使用共享token查询人员信息，避免重复获取token
                            var userResult = await GetShareholderOAUserByCodeAsync(processData.AssignedResponsiblePerson, sharedToken);
                            if (userResult.Status && userResult.Data is ShareholderOAUserInfo userInfo)
                            {
                                processData.ApproverUserId = userInfo.Id.ToString();
                                _logger.LogInformation("成功获取指定负责人OA人员ID（使用共享token） - 工号: {EmployeeCode}, 人员ID: {UserId}, 姓名: {UserName}",
                                    processData.AssignedResponsiblePerson, userInfo.Id, userInfo.Name);
                            }
                            else
                            {
                                _logger.LogWarning("未能获取指定负责人OA人员ID（使用共享token） - 工号: {EmployeeCode}, 错误: {Error}",
                                    processData.AssignedResponsiblePerson, userResult.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "获取指定负责人OA人员ID时发生异常（使用共享token） - 工号: {EmployeeCode}",
                                processData.AssignedResponsiblePerson);
                        }
                    }

                    // 获取发起人员工号（自动处理管理员账号映射）
                    var currentUserEmployeeCode = GetInitiatorEmployeeCode();

                    // 构建股份OA表单数据
                    var shareholderFormData = new ShareholderOAFormData
                    {
                        DataSource = processData.DataSource ?? "协同平台",
                        SupplierCode = processData.SupplierCode ?? "",
                        NegotiationNo = processData.NegotiationNo ?? "",
                        SupplierName = processData.SupplierName ?? "",
                        OrderNo = processData.OrderNo ?? "",
                        SourceOrderNo = processData.SourceOrderNo ?? "",
                        DeliveryStatus = processData.DeliveryStatus ?? "",
                        OrderDate = processData.OrderDate ?? "",
                        OrderRemark = processData.OrderRemark ?? "",
                        MaterialName = processData.MaterialName ?? "",
                        MaterialCode = processData.MaterialCode ?? "",
                        Specification = processData.Specification ?? "",
                        DrawingNo = processData.DrawingNo ?? "",
                        Material = processData.Material ?? "",
                        Unit = processData.Unit ?? "",
                        PurchaseQuantity = processData.PurchaseQuantity ?? "",
                        ProductionCycle = processData.ProductionCycle ?? "",
                        Shortage = processData.Shortage ?? "",
                        PlanTrackingNo = processData.PlanTrackingNo ?? "",
                        FirstRequiredDeliveryDate = processData.FirstRequiredDeliveryDate ?? "",
                        ChangedDeliveryDate = processData.ChangedDeliveryDate ?? "",
                        ExecutiveOrganization = processData.ExecutiveOrganization ?? "",
                        OrderEntryLineNo = processData.OrderEntryLineNo,
                        SupplierExceptionReply = processData.SupplierExceptionReply ?? "",
                        ApproverUserId = processData.ApproverUserId ?? "",
                        InitiatorEmployeeCode = currentUserEmployeeCode // 设置发起人员工号
                    };

                    // 如果有共享token，直接使用token发起流程，否则使用原有方式
                    if (!string.IsNullOrEmpty(sharedToken))
                    {
                        _logger.LogInformation("使用共享Token发起股份OA流程 - TokenId: {TokenId}, 订单号: {OrderNo}", sharedToken, processData.OrderNo);
                        return await StartShareholderProcessAsync(sharedToken, shareholderFormData);
                    }
                    else
                    {
                        _logger.LogInformation("使用原有方式发起股份OA流程（重新获取Token） - 订单号: {OrderNo}", processData.OrderNo);
                        return await GetShareholderTokenAndStartProcessAsync(loginName, shareholderFormData);
                    }
                }
                else
                {
                    // 使用原有OA流程
                    // 如果有指定负责人，使用股份OA查询人员信息（因为人员数据在股份OA中）
                    if (!string.IsNullOrEmpty(processData.AssignedResponsiblePerson))
                    {
                        try
                        {
                            var userResult = await GetShareholderOAUserByCodeAsync(processData.AssignedResponsiblePerson);
                            if (userResult.Status && userResult.Data is ShareholderOAUserInfo userInfo)
                            {
                                processData.ApproverUserId = userInfo.Id.ToString();
                                _logger.LogInformation("成功获取指定负责人OA人员ID（原有OA流程） - 工号: {EmployeeCode}, 人员ID: {UserId}, 姓名: {UserName}",
                                    processData.AssignedResponsiblePerson, userInfo.Id, userInfo.Name);
                            }
                            else
                            {
                                _logger.LogWarning("未能获取指定负责人OA人员ID（原有OA流程） - 工号: {EmployeeCode}, 错误: {Error}",
                                    processData.AssignedResponsiblePerson, userResult.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "获取指定负责人OA人员ID时发生异常（原有OA流程） - 工号: {EmployeeCode}",
                                processData.AssignedResponsiblePerson);
                        }
                    }

                    var originalFormData = new OAFormData
                    {
                        DataSource = "协同平台",
                        SupplierCode = processData.SupplierCode ?? "",
                        SupplierName = processData.SupplierName ?? "",
                        OrderNo = processData.OrderNo ?? "",
                        SourceOrderNo = processData.SourceOrderNo ?? "",
                        DeliveryStatus = processData.DeliveryStatus ?? "",
                        OrderDate = processData.OrderDate ?? "",
                        OrderRemark = processData.OrderRemark ?? "",
                        MaterialName = processData.MaterialName ?? "",
                        MaterialCode = processData.MaterialCode ?? "",
                        Specification = processData.Specification ?? "",
                        DrawingNo = processData.DrawingNo ?? "",
                        Material = processData.Material ?? "",
                        Unit = processData.Unit ?? "",
                        PurchaseQuantity = processData.PurchaseQuantity ?? "",
                        Shortage = processData.Shortage ?? "",
                        PlanTrackingNo = processData.PlanTrackingNo ?? "",
                        FirstRequiredDeliveryDate = processData.FirstRequiredDeliveryDate ?? "",
                        ExecutiveOrganization = processData.ExecutiveOrganization ?? "",
                        ChangedDeliveryDate = processData.ChangedDeliveryDate ?? "",
                        SupplierExceptionReply = processData.SupplierExceptionReply ?? "",
                        ApproverUserId = processData.ApproverUserId ?? ""
                    };

                    return await GetTokenAndStartProcessAsync(loginName, originalFormData);
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"智能发起流程异常: {ex.Message}";
                _logger.LogError(ex, "智能发起OA流程时发生异常 - 登录名: {LoginName}, 订单号: {OrderNo}",
                    loginName, processData?.OrderNo);
            }

            return response;
        }
        #region 私有辅助方法

        /// <summary>
        /// 获取发起人员工号
        /// 对于管理员账号（admin、cyadmin），使用默认工号 015206
        /// 对于普通用户，使用当前登录用户的工号
        /// </summary>
        /// <returns>发起人员工号</returns>
        private string GetInitiatorEmployeeCode()
        {
            try
            {
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                var currentUserName = currentUser?.UserInfo?.UserName;

                // 如果无法获取用户信息，使用默认值
                if (string.IsNullOrEmpty(currentUserName))
                {
                    _logger.LogWarning("无法获取当前用户信息，使用默认发起人员工号: 015206");
                    return "015206";
                }

                // 管理员账号映射到默认工号
                if (currentUserName.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                    currentUserName.Equals("cyadmin", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("管理员账号 {UserName} 使用默认发起人员工号: 015206", currentUserName);
                    return "015206";
                }

                // 普通用户使用实际工号
                _logger.LogInformation("用户 {UserName} 使用发起人员工号: {EmployeeCode}", currentUserName, currentUserName);
                return currentUserName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取发起人员工号时发生异常，使用默认值: 015206");
                return "015206";
            }
        }

        #endregion
        /// <summary>
        /// 发起OA合同变更评审流程
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="contractChangeData">合同变更数据</param>
        /// <returns>流程发起结果</returns>
        public async Task<WebResponseContent> StartContractChangeReviewProcessAsync(string loginName, ContractChangeReviewData contractChangeData)
        {
            var response = new WebResponseContent();
            var processUrl = string.Empty;
            var requestJson = string.Empty;
            var responseContent = string.Empty;
            
            try
            {
                if (string.IsNullOrEmpty(loginName))
                {
                    _logger.LogWarning("发起OA合同变更评审流程失败：登录名不能为空");
                    response.Status = false;
                    response.Message = "登录名不能为空";
                    return response;
                }

                if (contractChangeData == null)
                {
                    _logger.LogWarning("发起OA合同变更评审流程失败：合同变更数据不能为空");
                    response.Status = false;
                    response.Message = "合同变更数据不能为空";
                    return response;
                }

                // 先获取股份OA Token（合同变更评审流程使用股份OA）
                var tokenResult = await GetShareholderTokenAsync(loginName);
                if (!tokenResult.Status)
                {
                    response.Status = false;
                    response.Message = $"获取Token失败: {tokenResult.Message}";
                    _logger.LogError("获取股份Token失败，无法发起合同变更评审流程: {ErrorMessage}", tokenResult.Message);
                    return response;
                }

                // 从Token响应中提取Token
                var tokenResponse = tokenResult.Data as OATokenResponse;
                if (tokenResponse == null)
                {
                    response.Status = false;
                    response.Message = "Token响应数据格式错误";
                    _logger.LogError("股份Token响应数据格式错误，无法发起合同变更评审流程");
                    return response;
                }

                var token = tokenResponse.Id;

                // 根据配置选择使用内网还是外网地址
                var baseUrl = _useExternal ? _shareholderOAExternalBaseUrl : _shareholderOABaseUrl;
                processUrl = $"{baseUrl}/seeyon/rest/bpm/process/start?token={token}";
                
                var requestBody = new
                {
                    appName = "collaboration",
                    data = new
                    {
                        data = new
                        {
                            formmain_7046 = contractChangeData
                        },
                        templateCode = "xsbgpslc_ddxtpt",
                        draft = "0"
                    }
                };

                requestJson = JsonConvert.SerializeObject(requestBody);
                var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                _logger.LogInformation("开始发起OA合同变更评审流程 - URL: {ProcessUrl}, 合同号: {ContractNo}, 计划号: {PlanNo}",
                    processUrl, contractChangeData.ContractNo, contractChangeData.PlanNo);
                _logger.LogInformation("OA合同变更评审流程请求参数: {RequestJson}", requestJson);

                // 记录请求开始时间
                var startTime = DateTime.Now;

                var httpResponse = await _httpClient.PostAsync(processUrl, httpContent);
                responseContent = await httpResponse.Content.ReadAsStringAsync();

                // 记录请求结束时间和耗时
                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                _logger.LogInformation("OA合同变更评审流程请求完成 - 状态码: {StatusCode}, 耗时: {Duration}ms, 响应长度: {ResponseLength}字符",
                    httpResponse.StatusCode, duration.TotalMilliseconds, responseContent?.Length ?? 0);
                
                _logger.LogInformation("OA合同变更评审流程响应内容: {ResponseContent}", responseContent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var processResponse = JsonConvert.DeserializeObject<OAProcessResponse>(responseContent);
                    
                    if (processResponse != null)
                    {
                        _logger.LogInformation("OA合同变更评审流程响应解析成功 - 状态码: {Code}, 流程ID: {ProcessId}, 主题: {Subject}",
                            processResponse.Code, processResponse.Data?.ProcessId, processResponse.Data?.Subject);

                        if (processResponse.Code == 0)
                        {
                            response.Status = true;
                            response.Data = processResponse.Data;
                            response.Message = "合同变更评审流程发起成功";
                            
                            _logger.LogInformation("OA合同变更评审流程发起成功，流程ID: {ProcessId}, 合同号: {ContractNo}",
                                processResponse.Data?.ProcessId, contractChangeData.ContractNo);
                        }
                        else
                        {
                            response.Status = false;
                            response.Message = $"合同变更评审流程发起失败: {processResponse.Message ?? "未知错误"}";
                            _logger.LogError("OA合同变更评审流程发起失败 - 错误码: {Code}, 错误消息: {Message}",
                                processResponse.Code, processResponse.Message);
                        }
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "合同变更评审流程响应格式错误";
                        _logger.LogError("OA合同变更评审流程响应解析失败，响应内容为空或格式错误");
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = $"合同变更评审流程发起失败，HTTP状态码: {httpResponse.StatusCode}";
                    _logger.LogError("OA合同变更评审流程HTTP请求失败 - 状态码: {StatusCode}, 原因: {ReasonPhrase}",
                        httpResponse.StatusCode, httpResponse.ReasonPhrase);
                }
            }
            catch (HttpRequestException ex)
            {
                response.Status = false;
                response.Message = $"发起合同变更评审流程网络异常: {ex.Message}";
                _logger.LogError(ex, "发起OA合同变更评审流程时发生网络异常 - URL: {ProcessUrl}, 请求参数: {RequestJson}",
                    processUrl, requestJson);
            }
            catch (JsonException ex)
            {
                response.Status = false;
                response.Message = $"发起合同变更评审流程数据格式异常: {ex.Message}";
                _logger.LogError(ex, "发起OA合同变更评审流程时JSON序列化异常 - 请求JSON: {RequestJson}, 响应内容: {ResponseContent}",
                    requestJson, responseContent);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"发起合同变更评审流程异常: {ex.Message}";
                _logger.LogError(ex, "发起OA合同变更评审流程时发生未知异常 - URL: {ProcessUrl}, 合同号: {ContractNo}",
                    processUrl, contractChangeData?.ContractNo);
            }

            return response;
        }

        /// <summary>
        /// 推送消息到股份OA系统
        /// </summary>
        /// <param name="token">认证Token</param>
        /// <param name="sendUserId">发送者用户ID</param>
        /// <param name="loginNames">接收者登录名列表</param>
        /// <param name="content">消息内容</param>
        /// <param name="urls">附件或扩展链接数组（可为空）</param>
        /// <returns>推送结果</returns>
        public async Task<WebResponseContent> SendShareholderMessageAsync(
            string token,
            string sendUserId,
            List<string> loginNames,
            string content,
            List<string>? urls = null)
        {
            var response = new WebResponseContent();
            var messageUrl = string.Empty;
            var requestJson = string.Empty;
            var responseContent = string.Empty;

            try
            {
                // 参数验证
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("推送股份OA消息失败：Token不能为空");
                    response.Status = false;
                    response.Message = "Token不能为空";
                    return response;
                }

                if (string.IsNullOrEmpty(sendUserId))
                {
                    _logger.LogWarning("推送股份OA消息失败：发送者用户ID不能为空");
                    response.Status = false;
                    response.Message = "发送者用户ID不能为空";
                    return response;
                }

                if (loginNames == null || loginNames.Count == 0)
                {
                    _logger.LogWarning("推送股份OA消息失败：接收者登录名列表不能为空");
                    response.Status = false;
                    response.Message = "接收者登录名列表不能为空";
                    return response;
                }

                if (string.IsNullOrEmpty(content))
                {
                    _logger.LogWarning("推送股份OA消息失败：消息内容不能为空");
                    response.Status = false;
                    response.Message = "消息内容不能为空";
                    return response;
                }

                // 映射接收者登录名中的管理员账号
                loginNames = MapAdminLoginNames(loginNames);

                // 根据配置选择使用内网还是外网地址
                var baseUrl = _useExternal ? _shareholderOAExternalBaseUrl : _shareholderOABaseUrl;
                messageUrl = $"{baseUrl}/seeyon/rest/message?token={token}";

                var requestBody = new
                {
                    sendUserId = sendUserId,
                    loginNames = loginNames,
                    content = content,
                    url = urls ?? new List<string>()
                };

                requestJson = JsonConvert.SerializeObject(requestBody);
                var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                _logger.LogInformation("开始推送股份OA消息 - URL: {MessageUrl}, 发送者: {SendUserId}, 接收者: {Receivers}, 内容长度: {ContentLength}字符",
                    messageUrl, sendUserId, string.Join(",", loginNames), content.Length);
                _logger.LogInformation("股份OA消息请求参数: {RequestJson}", requestJson);

                // 记录请求开始时间
                var startTime = DateTime.Now;

                var httpResponse = await _httpClient.PostAsync(messageUrl, httpContent);
                responseContent = await httpResponse.Content.ReadAsStringAsync();

                // 记录请求结束时间和耗时
                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                _logger.LogInformation("股份OA消息请求完成 - 状态码: {StatusCode}, 耗时: {Duration}ms, 响应长度: {ResponseLength}字符",
                    httpResponse.StatusCode, duration.TotalMilliseconds, responseContent?.Length ?? 0);

                _logger.LogInformation("股份OA消息响应内容: {ResponseContent}", responseContent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var messageResponse = JsonConvert.DeserializeObject<OAMessageResponse>(responseContent);

                    if (messageResponse != null)
                    {
                        if (messageResponse.Result == 1)
                        {
                            response.Status = true;
                            response.Message = "消息推送成功";
                            response.Data = messageResponse;

                            _logger.LogInformation("股份OA消息推送成功 - 发送者: {SendUserId}, 接收者: {Receivers}",
                                sendUserId, string.Join(",", loginNames));
                        }
                        else
                        {
                            response.Status = false;
                            response.Message = $"消息推送失败: {messageResponse.ErrorMessage}";
                            response.Data = messageResponse;

                            _logger.LogError("股份OA消息推送失败 - 错误码: {ErrorNumber}, 错误信息: {ErrorMessage}",
                                messageResponse.ErrorNumber, messageResponse.ErrorMessage);
                        }
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "消息响应格式错误";
                        _logger.LogError("股份OA消息响应格式错误，无法解析响应信息");
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = $"消息推送失败，HTTP状态码: {httpResponse.StatusCode}";
                    _logger.LogError("股份OA消息HTTP请求失败 - 状态码: {StatusCode}, 原因: {ReasonPhrase}",
                        httpResponse.StatusCode, httpResponse.ReasonPhrase);
                }
            }
            catch (HttpRequestException ex)
            {
                response.Status = false;
                response.Message = $"推送消息网络异常: {ex.Message}";
                _logger.LogError(ex, "推送股份OA消息时发生网络异常 - URL: {MessageUrl}, 请求参数: {RequestJson}",
                    messageUrl, requestJson);
            }
            catch (JsonException ex)
            {
                response.Status = false;
                response.Message = $"推送消息数据格式异常: {ex.Message}";
                _logger.LogError(ex, "推送股份OA消息时JSON序列化异常 - 请求JSON: {RequestJson}, 响应内容: {ResponseContent}",
                    requestJson, responseContent);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"推送消息异常: {ex.Message}";
                _logger.LogError(ex, "推送股份OA消息时发生未知异常 - URL: {MessageUrl}, 发送者: {SendUserId}",
                    messageUrl, sendUserId);
            }

            return response;
        }

        /// <summary>
        /// 获取Token并推送消息到股份OA系统（便捷方法）
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="content">消息内容</param>
        /// <param name="urls">附件或扩展链接数组（可为空）</param>
        /// <returns>推送结果</returns>
        public async Task<WebResponseContent> GetShareholderTokenAndSendMessageAsync(
            string senderLoginName,
            List<string> receiverLoginNames,
            string content,
            List<string>? urls = null)
        {
            var response = new WebResponseContent();

            try
            {
                // 参数验证
                if (string.IsNullOrEmpty(senderLoginName))
                {
                    _logger.LogWarning("获取股份OA Token并推送消息失败：发送者登录名不能为空");
                    response.Status = false;
                    response.Message = "发送者登录名不能为空";
                    return response;
                }

                if (receiverLoginNames == null || receiverLoginNames.Count == 0)
                {
                    _logger.LogWarning("获取股份OA Token并推送消息失败：接收者登录名列表不能为空");
                    response.Status = false;
                    response.Message = "接收者登录名列表不能为空";
                    return response;
                }

                if (string.IsNullOrEmpty(content))
                {
                    _logger.LogWarning("获取股份OA Token并推送消息失败：消息内容不能为空");
                    response.Status = false;
                    response.Message = "消息内容不能为空";
                    return response;
                }

                _logger.LogInformation("开始获取股份OA Token并推送消息 - 发送者: {SenderLoginName}, 接收者: {Receivers}, 内容长度: {ContentLength}字符",
                    senderLoginName, string.Join(",", receiverLoginNames), content.Length);

                // 1. 获取Token
                var tokenResponse = await GetShareholderTokenAsync(senderLoginName);
                if (!tokenResponse.Status)
                {
                    response.Status = false;
                    response.Message = $"获取Token失败: {tokenResponse.Message}";
                    _logger.LogError("获取股份OA Token失败，无法推送消息 - 发送者: {SenderLoginName}, 错误: {Error}",
                        senderLoginName, tokenResponse.Message);
                    return response;
                }

                var tokenData = tokenResponse.Data as OATokenResponse;
                if (tokenData == null || string.IsNullOrEmpty(tokenData.Id))
                {
                    response.Status = false;
                    response.Message = "Token数据格式错误";
                    _logger.LogError("股份OA Token数据格式错误，无法推送消息");
                    return response;
                }

                // 2. 推送消息
                var sendUserId = tokenData.BindingUser?.Id.ToString() ?? senderLoginName;
                var messageResponse = await SendShareholderMessageAsync(tokenData.Id, sendUserId, receiverLoginNames, content, urls);

                response.Status = messageResponse.Status;
                response.Message = messageResponse.Message;
                response.Data = messageResponse.Data;

                if (messageResponse.Status)
                {
                    _logger.LogInformation("股份OA Token获取并消息推送成功 - 发送者: {SenderLoginName}, 接收者: {Receivers}",
                        senderLoginName, string.Join(",", receiverLoginNames));
                }
                else
                {
                    _logger.LogError("股份OA消息推送失败 - 发送者: {SenderLoginName}, 错误: {Error}",
                        senderLoginName, messageResponse.Message);
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"获取Token并推送消息异常: {ex.Message}";
                _logger.LogError(ex, "获取股份OA Token并推送消息时发生未知异常 - 发送者: {SenderLoginName}",
                    senderLoginName);
            }

            return response;
        }

        /// <summary>
        /// 同时发送消息到原OA和股份OA系统
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="content">消息内容</param>
        /// <param name="urls">附件或扩展链接数组（可为空）</param>
        /// <returns>推送结果（包含两个OA系统的发送结果）</returns>
        public async Task<WebResponseContent> SendToBothOASystemsAsync(
            string senderLoginName,
            List<string> receiverLoginNames,
            string content,
            List<string>? urls = null)
        {
            var result = new BothOASystemsResult();
            var response = new WebResponseContent();

            try
            {
                _logger.LogInformation("开始同时发送消息到原OA和股份OA系统 - 发送者: {SenderLoginName}, 接收者: {Receivers}, 内容长度: {ContentLength}字符",
                    senderLoginName, string.Join(",", receiverLoginNames), content.Length);

                // 并行发送到两个OA系统
                var originalOATask = GetTokenAndSendMessageAsync(senderLoginName, receiverLoginNames, content, urls);
                var shareholderOATask = GetShareholderTokenAndSendMessageAsync(senderLoginName, receiverLoginNames, content, urls);

                // 等待两个任务完成
                var originalOAResult = await originalOATask;
                var shareholderOAResult = await shareholderOATask;

                // 构建结果
                result.OriginalOA = new OASystemResult
                {
                    Success = originalOAResult.Status,
                    Message = originalOAResult.Message,
                    SystemName = "原OA系统",
                    SendTime = DateTime.Now
                };

                result.ShareholderOA = new OASystemResult
                {
                    Success = shareholderOAResult.Status,
                    Message = shareholderOAResult.Message,
                    SystemName = "股份OA系统",
                    SendTime = DateTime.Now
                };

                // 设置响应结果
                response.Status = result.OverallSuccess;
                response.Message = result.SummaryMessage;
                response.Data = result;

                if (result.OverallSuccess)
                {
                    _logger.LogInformation("双OA系统消息发送完成 - 原OA: {OriginalResult}, 股份OA: {ShareholderResult}",
                        result.OriginalOA.Success ? "成功" : "失败",
                        result.ShareholderOA.Success ? "成功" : "失败");
                }
                else
                {
                    _logger.LogWarning("双OA系统消息发送失败 - 原OA: {OriginalMessage}, 股份OA: {ShareholderMessage}",
                        result.OriginalOA.Message,
                        result.ShareholderOA.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "双OA系统消息发送异常 - 发送者: {SenderLoginName}, 接收者: {Receivers}",
                    senderLoginName, string.Join(",", receiverLoginNames));

                response.Status = false;
                response.Message = $"双OA系统消息发送异常: {ex.Message}";
                response.Data = result;
            }

            return response;
        }

        /// <summary>
        /// 根据人员工号获取股份OA人员ID
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <param name="token">认证Token（可选，如果为空则使用默认Token）</param>
        /// <returns>人员信息</returns>
        public async Task<WebResponseContent> GetShareholderOAUserByCodeAsync(string employeeCode, string token = null)
        {
            var response = new WebResponseContent();
            var requestUrl = string.Empty;
            var responseContent = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(employeeCode))
                {
                    _shareholderMessageLogger.LogWarning("获取股份OA人员信息失败：人员工号不能为空");
                    response.Status = false;
                    response.Message = "人员工号不能为空";
                    return response;
                }

                // 如果没有提供Token，动态获取Token
                if (string.IsNullOrEmpty(token))
                {
                    _shareholderMessageLogger.LogInfo("未提供Token，开始动态获取股份OA Token - 工号: {EmployeeCode}", employeeCode);
                    
                    // 使用工号作为登录名获取Token
                    var tokenResult = await GetShareholderTokenAsync(employeeCode);
                    
                    if (!tokenResult.Status)
                    {
                        _shareholderMessageLogger.LogWarning("动态获取股份OA Token失败 - 工号: {EmployeeCode}, 错误: {Error}", 
                            employeeCode, tokenResult.Message);
                        
                        // Token获取失败时，尝试使用默认Token作为备用方案
                        token = "96cedcd0-2006-4af6-aafd-42e01e9dc4c0";
                        _shareholderMessageLogger.LogInfo("使用默认Token作为备用方案 - 工号: {EmployeeCode}", employeeCode);
                    }
                    else
                    {
                        // 从Token响应中提取Token
                        var tokenResponse = tokenResult.Data as OATokenResponse;
                        if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Id))
                        {
                            token = tokenResponse.Id;
                            _shareholderMessageLogger.LogInfo("成功获取股份OA Token - 工号: {EmployeeCode}, TokenId: {TokenId}", 
                                employeeCode, tokenResponse.Id);
                        }
                        else
                        {
                            // Token响应格式错误时，使用默认Token作为备用方案
                            token = "96cedcd0-2006-4af6-aafd-42e01e9dc4c0";
                            _shareholderMessageLogger.LogWarning("Token响应格式错误，使用默认Token作为备用方案 - 工号: {EmployeeCode}", employeeCode);
                        }
                    }
                }

                // 构建请求URL
                var baseUrl = _useExternal ? _shareholderOAExternalBaseUrl : _shareholderOABaseUrl;
                requestUrl = $"{baseUrl}/seeyon/rest/orgMembers/code/{employeeCode}?token={token}";

                _shareholderMessageLogger.LogInfo("开始获取股份OA人员信息 - URL: {RequestUrl}, EmployeeCode: {EmployeeCode}", requestUrl, employeeCode);

                // 记录请求开始时间
                var startTime = DateTime.Now;

                var httpResponse = await _httpClient.GetAsync(requestUrl);
                responseContent = await httpResponse.Content.ReadAsStringAsync();

                // 记录请求结束时间和耗时
                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                _shareholderMessageLogger.LogHttpRequest("GET", requestUrl, (int)httpResponse.StatusCode, duration.TotalMilliseconds);
                _shareholderMessageLogger.LogResponseContent("股份OA人员信息", responseContent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    // 解析响应数据，返回的是数组格式
                    var userInfoList = JsonConvert.DeserializeObject<List<ShareholderOAUserInfo>>(responseContent);
                    
                    if (userInfoList != null && userInfoList.Any())
                    {
                        var userInfo = userInfoList.First();
                        
                        response.Status = true;
                        response.Data = userInfo;
                        response.Message = "获取人员信息成功";
                        
                        _shareholderMessageLogger.LogInfo("获取股份OA人员信息成功 - 工号: {EmployeeCode}, 人员ID: {UserId}, 姓名: {UserName}, 部门: {Department}",
                            employeeCode, userInfo.Id, userInfo.Name, userInfo.OrgDepartmentName);
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "未找到对应的人员信息";
                        
                        _shareholderMessageLogger.LogWarning("未找到对应的人员信息 - 工号: {EmployeeCode}", employeeCode);
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = $"请求失败，状态码: {httpResponse.StatusCode}";
                    
                    _shareholderMessageLogger.LogError("获取股份OA人员信息失败 - 状态码: {StatusCode}, 响应内容: {ResponseContent}",
                        httpResponse.StatusCode, responseContent);
                }
            }
            catch (JsonException jsonEx)
            {
                _shareholderMessageLogger.LogError(jsonEx, "解析股份OA人员信息响应时发生JSON异常 - 响应内容: {ResponseContent}", responseContent);
                response.Status = false;
                response.Message = $"解析响应数据失败: {jsonEx.Message}";
            }
            catch (HttpRequestException httpEx)
            {
                _shareholderMessageLogger.LogError(httpEx, "获取股份OA人员信息时发生HTTP请求异常 - URL: {RequestUrl}", requestUrl);
                response.Status = false;
                response.Message = $"HTTP请求失败: {httpEx.Message}";
            }
            catch (TaskCanceledException timeoutEx)
            {
                _shareholderMessageLogger.LogError(timeoutEx, "获取股份OA人员信息请求超时 - URL: {RequestUrl}", requestUrl);
                response.Status = false;
                response.Message = "请求超时";
            }
            catch (Exception ex)
            {
                _shareholderMessageLogger.LogError(ex, "获取股份OA人员信息时发生未知异常 - URL: {RequestUrl}", requestUrl);
                response.Status = false;
                response.Message = $"获取人员信息时发生异常: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// 获取Token并查询股份OA人员信息（参照GetShareholderTokenAndStartProcessAsync模式）
        /// </summary>
        /// <param name="employeeCode">人员工号</param>
        /// <returns>人员信息查询结果</returns>
        public async Task<WebResponseContent> GetShareholderTokenAndQueryUserAsync(string employeeCode)
        {
            var response = new WebResponseContent();
            
            try
            {
                if (string.IsNullOrEmpty(employeeCode))
                {
                    _shareholderMessageLogger.LogWarning("获取Token并查询股份OA人员信息失败：人员工号不能为空");
                    response.Status = false;
                    response.Message = "人员工号不能为空";
                    return response;
                }

                _shareholderMessageLogger.LogInfo("开始获取Token并查询股份OA人员信息 - 工号: {EmployeeCode}", employeeCode);

                // 先获取Token
                var tokenResult = await GetShareholderTokenAsync(employeeCode);
                if (!tokenResult.Status)
                {
                    response.Status = false;
                    response.Message = $"获取Token失败: {tokenResult.Message}";
                    _shareholderMessageLogger.LogError("获取股份Token失败，无法查询人员信息: {ErrorMessage}", tokenResult.Message);
                    return response;
                }

                // 从Token响应中提取Token
                var tokenResponse = tokenResult.Data as OATokenResponse;
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Id))
                {
                    response.Status = false;
                    response.Message = "Token响应数据格式错误";
                    _shareholderMessageLogger.LogError("股份Token响应数据格式错误，无法查询人员信息");
                    return response;
                }

                _shareholderMessageLogger.LogInfo("成功获取股份Token，开始查询人员信息 - TokenId: {TokenId}, 工号: {EmployeeCode}",
                    tokenResponse.Id, employeeCode);

                // 使用Token查询人员信息
                return await GetShareholderOAUserByCodeAsync(employeeCode, tokenResponse.Id);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"获取Token并查询股份人员信息异常: {ex.Message}";
                _shareholderMessageLogger.LogError(ex, "获取Token并查询股份OA人员信息时发生异常 - 工号: {EmployeeCode}", employeeCode);
            }

            return response;
        }

        #endregion

    }

    /// <summary>
    /// OA Token响应模型
    /// </summary>
    public class OATokenResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("bindingUser")]
        public OABindingUser BindingUser { get; set; }
    }

    /// <summary>
    /// OA绑定用户信息
    /// </summary>
    public class OABindingUser
    {
        [JsonProperty("loginState")]
        public string LoginState { get; set; }

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("loginName")]
        public string LoginName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("loginAccount")]
        public long LoginAccount { get; set; }

        [JsonProperty("loginAccountName")]
        public string LoginAccountName { get; set; }
    }

    /// <summary>
    /// OA消息推送响应模型
    /// </summary>
    public class OAMessageResponse
    {
        [JsonProperty("result")]
        public int Result { get; set; }

        [JsonProperty("errorNumber")]
        public int ErrorNumber { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// OA表单数据模型
    /// </summary>
    public class OAFormData
    {
        /// <summary>
        /// 数据来源：SRM/协同平台
        /// </summary>
        [JsonProperty("数据来源")]
        public string DataSource { get; set; }

        /// <summary>
        /// 供方编码
        /// </summary>
        [JsonProperty("供方编码")]
        public string SupplierCode { get; set; }

        /// <summary>
        /// 供方名称
        /// </summary>
        [JsonProperty("供方名称")]
        public string SupplierName { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [JsonProperty("订单号")]
        public string OrderNo { get; set; }

        /// <summary>
        /// 源单号
        /// </summary>
        [JsonProperty("源单号")]
        public string SourceOrderNo { get; set; }

        /// <summary>
        /// 送货状态
        /// </summary>
        [JsonProperty("送货状态")]
        public string DeliveryStatus { get; set; }

        /// <summary>
        /// 订单日期
        /// </summary>
        [JsonProperty("订单日期")]
        public string OrderDate { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        [JsonProperty("订单备注")]
        public string OrderRemark { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        [JsonProperty("物料名称")]
        public string MaterialName { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        [JsonProperty("物料编码")]
        public string MaterialCode { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        [JsonProperty("规格型号")]
        public string Specification { get; set; }

        /// <summary>
        /// 图号
        /// </summary>
        [JsonProperty("图号")]
        public string DrawingNo { get; set; }

        /// <summary>
        /// 材质
        /// </summary>
        [JsonProperty("材质")]
        public string Material { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        [JsonProperty("单位")]
        public string Unit { get; set; }

        /// <summary>
        /// 采购量
        /// </summary>
        [JsonProperty("采购量")]
        public string PurchaseQuantity { get; set; }

        /// <summary>
        /// 差缺
        /// </summary>
        [JsonProperty("差缺")]
        public string Shortage { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// </summary>
        [JsonProperty("计划跟踪号")]
        public string PlanTrackingNo { get; set; }

        /// <summary>
        /// 第一次要求交期（变更前交期）
        /// </summary>
        [JsonProperty("第一次要求交期")]
        public string FirstRequiredDeliveryDate { get; set; }

        /// <summary>
        /// 执行机构
        /// </summary>
        [JsonProperty("执行机构")]
        public string ExecutiveOrganization { get; set; }

        /// <summary>
        /// 变更的交期
        /// </summary>
        [JsonProperty("变更的交期")]
        public string ChangedDeliveryDate { get; set; }

        /// <summary>
        /// 供方异常回复
        /// </summary>
        [JsonProperty("供方异常回复")]
        public string SupplierExceptionReply { get; set; }

        /// <summary>
        /// 审批人ID（指定负责人对应的OA人员ID）
        /// </summary>
        [JsonProperty("审批人")]
        public string ApproverUserId { get; set; }
    }

    /// <summary>
    /// 股份OA表单数据模型
    /// </summary>
    public class ShareholderOAFormData
    {
        /// <summary>
        /// 数据来源：协同平台/SRM
        /// </summary>
        [JsonProperty("数据来源")]
        public string DataSource { get; set; }

        /// <summary>
        /// 供方编码
        /// </summary>
        [JsonProperty("供方编码")]
        public string SupplierCode { get; set; }

        /// <summary>
        /// 协商编号
        /// </summary>
        [JsonProperty("协商编号")]
        public string NegotiationNo { get; set; }

        /// <summary>
        /// 供方名称
        /// </summary>
        [JsonProperty("供方名称")]
        public string SupplierName { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [JsonProperty("订单号")]
        public string OrderNo { get; set; }

        /// <summary>
        /// 源单号
        /// </summary>
        [JsonProperty("源单号")]
        public string SourceOrderNo { get; set; }

        /// <summary>
        /// 送货状态
        /// </summary>
        [JsonProperty("送货状态")]
        public string DeliveryStatus { get; set; }

        /// <summary>
        /// 订单日期
        /// </summary>
        [JsonProperty("订单日期")]
        public string OrderDate { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        [JsonProperty("订单备注")]
        public string OrderRemark { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        [JsonProperty("物料名称")]
        public string MaterialName { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        [JsonProperty("物料编码")]
        public string MaterialCode { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        [JsonProperty("规格型号")]
        public string Specification { get; set; }

        /// <summary>
        /// 图号
        /// </summary>
        [JsonProperty("图号")]
        public string DrawingNo { get; set; }

        /// <summary>
        /// 材质
        /// </summary>
        [JsonProperty("材质")]
        public string Material { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        [JsonProperty("单位")]
        public string Unit { get; set; }

        /// <summary>
        /// 采购量
        /// </summary>
        [JsonProperty("采购量")]
        public string PurchaseQuantity { get; set; }

        /// <summary>
        /// 生产周期
        /// </summary>
        [JsonProperty("生产周期")]
        public string ProductionCycle { get; set; }

        /// <summary>
        /// 差缺
        /// </summary>
        [JsonProperty("差缺")]
        public string Shortage { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// </summary>
        [JsonProperty("计划跟踪号")]
        public string PlanTrackingNo { get; set; }

        /// <summary>
        /// 第一次要求交期
        /// </summary>
        [JsonProperty("第一次要求交期")]
        public string FirstRequiredDeliveryDate { get; set; }

        /// <summary>
        /// 变更的交期
        /// </summary>
        [JsonProperty("变更的交期")]
        public string ChangedDeliveryDate { get; set; }

        /// <summary>
        /// 执行机构
        /// </summary>
        [JsonProperty("执行机构")]
        public string ExecutiveOrganization { get; set; }

        /// <summary>
        /// 订单分录行号
        /// </summary>
        [JsonProperty("订单分录行号")]
        public int? OrderEntryLineNo { get; set; }

        /// <summary>
        /// 供方异常回复
        /// </summary>
        [JsonProperty("供方异常回复")]
        public string SupplierExceptionReply { get; set; }

        /// <summary>
        /// 审批人ID（指定负责人对应的OA人员ID）
        /// </summary>
        [JsonProperty("审批人")]
        public string ApproverUserId { get; set; }

        /// <summary>
        /// 发起人员工号
        /// </summary>
        [JsonProperty("发起人员工号")]
        public string InitiatorEmployeeCode { get; set; }
    }

    /// <summary>
    /// OA流程响应模型
    /// </summary>
    public class OAProcessResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("data")]
        public OAProcessData Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// OA流程数据模型
    /// </summary>
    public class OAProcessData
    {
        [JsonProperty("workitems")]
        public List<OAWorkItem> WorkItems { get; set; }

        [JsonProperty("app_bussiness_data")]
        public string AppBusinessData { get; set; }

        [JsonProperty("processId")]
        public string ProcessId { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("errorMsg")]
        public string ErrorMsg { get; set; }
    }

    /// <summary>
    /// OA工作项模型
    /// </summary>
    public class OAWorkItem
    {
        [JsonProperty("nodeName")]
        public string NodeName { get; set; }

        [JsonProperty("userLoginName")]
        public string UserLoginName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("nodeId")]
        public string NodeId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }
    }

    /// <summary>
    /// 智能OA流程数据传输对象
    /// 用于智能发起流程的参数封装
    /// </summary>
    public class SmartOAProcessData
    {
        /// <summary>
        /// 数据来源：协同平台/SRM
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// 供方编码
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 协商编号
        /// </summary>
        public string NegotiationNo { get; set; }

        /// <summary>
        /// 供方名称
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 源单号
        /// </summary>
        public string SourceOrderNo { get; set; }

        /// <summary>
        /// 送货状态
        /// </summary>
        public string DeliveryStatus { get; set; }

        /// <summary>
        /// 订单日期
        /// </summary>
        public string OrderDate { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        public string OrderRemark { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specification { get; set; }

        /// <summary>
        /// 图号
        /// </summary>
        public string DrawingNo { get; set; }

        /// <summary>
        /// 材质
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 采购量
        /// </summary>
        public string PurchaseQuantity { get; set; }

        /// <summary>
        /// 生产周期
        /// </summary>
        public string ProductionCycle { get; set; }

        /// <summary>
        /// 差缺
        /// </summary>
        public string Shortage { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string PlanTrackingNo { get; set; }

        /// <summary>
        /// 第一次要求交期
        /// </summary>
        public string FirstRequiredDeliveryDate { get; set; }

        /// <summary>
        /// 执行机构
        /// </summary>
        public string ExecutiveOrganization { get; set; }

        /// <summary>
        /// 变更的交期
        /// </summary>
        public string ChangedDeliveryDate { get; set; }

        /// <summary>
        /// 订单分录行号
        /// </summary>
        public int? OrderEntryLineNo { get; set; }

        /// <summary>
        /// 供方异常回复
        /// </summary>
        public string SupplierExceptionReply { get; set; }

        /// <summary>
        /// 指定负责人工号（用于获取对应的OA人员ID）
        /// </summary>
        public string AssignedResponsiblePerson { get; set; }

        /// <summary>
        /// 审批人ID（指定负责人对应的OA人员ID）
        /// </summary>
        public string ApproverUserId { get; set; }
    }

    /// <summary>
    /// 合同变更评审数据模型
    /// </summary>
    public class ContractChangeReviewData
    {
        /// <summary>
        /// 客户类型ID
        /// 网点：-661824289608153598
        /// 自签：-1675890569785356551
        /// </summary>
        [JsonProperty("客户类型")]
        public string CustomerType { get; set; }

        /// <summary>
        /// 计划号（多条数据用分号隔开）
        /// </summary>
        [JsonProperty("计划号")]
        public string PlanNo { get; set; }

        /// <summary>
        /// 合同号
        /// </summary>
        [JsonProperty("合同号")]
        public string ContractNo { get; set; }

        /// <summary>
        /// 规格型号（多条数据用分号隔开）
        /// </summary>
        [JsonProperty("规格型号")]
        public string Specification { get; set; }

        /// <summary>
        /// 变更前价格（多条数据累加）
        /// </summary>
        [JsonProperty("变更前价格")]
        public string BeforeChangePrice { get; set; }

        /// <summary>
        /// 变更前下浮（计算公式：(目录价累加-售价累加)/目录价累加*100）
        /// </summary>
        [JsonProperty("变更前下浮")]
        public string BeforeChangeDiscount { get; set; }
    }

    /// <summary>
    /// 双OA系统发送结果
    /// </summary>
    public class BothOASystemsResult
    {
        /// <summary>
        /// 原OA系统发送结果
        /// </summary>
        public OASystemResult OriginalOA { get; set; }

        /// <summary>
        /// 股份OA系统发送结果
        /// </summary>
        public OASystemResult ShareholderOA { get; set; }

        /// <summary>
        /// 整体是否成功（至少一个系统发送成功即认为成功）
        /// </summary>
        public bool OverallSuccess => OriginalOA?.Success == true || ShareholderOA?.Success == true;

        /// <summary>
        /// 汇总消息
        /// </summary>
        public string SummaryMessage
        {
            get
            {
                var messages = new List<string>();
                if (OriginalOA != null)
                {
                    messages.Add($"原OA: {(OriginalOA.Success ? "成功" : "失败")} - {OriginalOA.Message}");
                }
                if (ShareholderOA != null)
                {
                    messages.Add($"股份OA: {(ShareholderOA.Success ? "成功" : "失败")} - {ShareholderOA.Message}");
                }
                return string.Join("; ", messages);
            }
        }
    }

    /// <summary>
    /// 单个OA系统发送结果
    /// </summary>
    public class OASystemResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 系统名称
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }
    }

    /// <summary>
    /// 股份OA人员信息
    /// </summary>
    public class ShareholderOAUserInfo
    {
        /// <summary>
        /// 组织账号ID
        /// </summary>
        [JsonProperty("orgAccountId")]
        public long OrgAccountId { get; set; }

        /// <summary>
        /// 人员ID
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// 人员姓名
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 人员工号
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("createTime")]
        public long CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [JsonProperty("updateTime")]
        public long UpdateTime { get; set; }

        /// <summary>
        /// 排序ID
        /// </summary>
        [JsonProperty("sortId")]
        public int SortId { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// 外部类型
        /// </summary>
        [JsonProperty("externalType")]
        public int ExternalType { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 组织级别ID
        /// </summary>
        [JsonProperty("orgLevelId")]
        public long OrgLevelId { get; set; }

        /// <summary>
        /// 组织岗位ID
        /// </summary>
        [JsonProperty("orgPostId")]
        public long OrgPostId { get; set; }

        /// <summary>
        /// 组织部门ID
        /// </summary>
        [JsonProperty("orgDepartmentId")]
        public long OrgDepartmentId { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; set; }

        /// <summary>
        /// 是否内部员工
        /// </summary>
        [JsonProperty("isInternal")]
        public bool IsInternal { get; set; }

        /// <summary>
        /// 是否可登录
        /// </summary>
        [JsonProperty("isLoginable")]
        public bool IsLoginable { get; set; }

        /// <summary>
        /// 是否虚拟
        /// </summary>
        [JsonProperty("isVirtual")]
        public bool IsVirtual { get; set; }

        /// <summary>
        /// 是否已分配
        /// </summary>
        [JsonProperty("isAssigned")]
        public bool IsAssigned { get; set; }

        /// <summary>
        /// 是否管理员
        /// </summary>
        [JsonProperty("isAdmin")]
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        [JsonProperty("isValid")]
        public bool IsValid { get; set; }

        /// <summary>
        /// 状态值
        /// </summary>
        [JsonProperty("state")]
        public int State { get; set; }

        /// <summary>
        /// 属性信息
        /// </summary>
        [JsonProperty("properties")]
        public ShareholderOAUserProperties Properties { get; set; }

        /// <summary>
        /// 拼音
        /// </summary>
        [JsonProperty("pinyin")]
        public string Pinyin { get; set; }

        /// <summary>
        /// 拼音首字母
        /// </summary>
        [JsonProperty("pinyinhead")]
        public string PinyinHead { get; set; }

        /// <summary>
        /// 登录名
        /// </summary>
        [JsonProperty("loginName")]
        public string LoginName { get; set; }

        /// <summary>
        /// 是否访客
        /// </summary>
        [JsonProperty("guest")]
        public bool Guest { get; set; }

        /// <summary>
        /// 实体类型
        /// </summary>
        [JsonProperty("entityType")]
        public string EntityType { get; set; }

        /// <summary>
        /// 是否访问者
        /// </summary>
        [JsonProperty("visitor")]
        public bool Visitor { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        [JsonProperty("telNumber")]
        public string TelNumber { get; set; }

        /// <summary>
        /// 是否分配状态
        /// </summary>
        [JsonProperty("isAssignedStatus")]
        public int IsAssignedStatus { get; set; }

        /// <summary>
        /// 办公电话
        /// </summary>
        [JsonProperty("officeNum")]
        public string OfficeNum { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [JsonProperty("gender")]
        public int Gender { get; set; }

        /// <summary>
        /// 全名
        /// </summary>
        [JsonProperty("fullName")]
        public string FullName { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        [JsonProperty("valid")]
        public bool Valid { get; set; }

        /// <summary>
        /// 实际排序ID
        /// </summary>
        [JsonProperty("realSortId")]
        public int RealSortId { get; set; }

        /// <summary>
        /// 组织账号名称
        /// </summary>
        [JsonProperty("orgAccountName")]
        public string OrgAccountName { get; set; }

        /// <summary>
        /// 组织岗位名称
        /// </summary>
        [JsonProperty("orgPostName")]
        public string OrgPostName { get; set; }

        /// <summary>
        /// 组织部门名称
        /// </summary>
        [JsonProperty("orgDepartmentName")]
        public string OrgDepartmentName { get; set; }

        /// <summary>
        /// 组织级别名称
        /// </summary>
        [JsonProperty("orgLevelName")]
        public string OrgLevelName { get; set; }
    }

    /// <summary>
    /// 股份OA人员属性信息
    /// </summary>
    public class ShareholderOAUserProperties
    {
        /// <summary>
        /// 生日
        /// </summary>
        [JsonProperty("birthday")]
        public string Birthday { get; set; }

        /// <summary>
        /// 政治面貌
        /// </summary>
        [JsonProperty("politics")]
        public int Politics { get; set; }

        /// <summary>
        /// 网站
        /// </summary>
        [JsonProperty("website")]
        public string Website { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }

        /// <summary>
        /// 头像ID
        /// </summary>
        [JsonProperty("imageid")]
        public string ImageId { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [JsonProperty("gender")]
        public int Gender { get; set; }

        /// <summary>
        /// 学历
        /// </summary>
        [JsonProperty("degree")]
        public string Degree { get; set; }

        /// <summary>
        /// 邮政地址
        /// </summary>
        [JsonProperty("postAddress")]
        public string PostAddress { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        [JsonProperty("emailaddress")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// 汇报人
        /// </summary>
        [JsonProperty("reporter")]
        public string Reporter { get; set; }

        /// <summary>
        /// 博客
        /// </summary>
        [JsonProperty("blog")]
        public string Blog { get; set; }

        /// <summary>
        /// 入职日期
        /// </summary>
        [JsonProperty("hiredate")]
        public string HireDate { get; set; }

        /// <summary>
        /// 外部岗位级别
        /// </summary>
        [JsonProperty("extPostLevel")]
        public string ExtPostLevel { get; set; }

        /// <summary>
        /// 微信
        /// </summary>
        [JsonProperty("weixin")]
        public string Weixin { get; set; }

        /// <summary>
        /// 微博
        /// </summary>
        [JsonProperty("weibo")]
        public string Weibo { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        [JsonProperty("telnumber")]
        public string TelNumber { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        [JsonProperty("postalcode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// 教育背景
        /// </summary>
        [JsonProperty("eduBack")]
        public int EduBack { get; set; }

        /// <summary>
        /// 办公电话
        /// </summary>
        [JsonProperty("officenumber")]
        public string OfficeNumber { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        [JsonProperty("location")]
        public string Location { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        [JsonProperty("idnum")]
        public string IdNum { get; set; }
    }
} 