/*
 * SRM集成服务实现
 * 实现与SRM系统的数据交互功能
 */
using HDPro.Core.Configuration;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Utilities;
using HDPro.Core.ManageUser;
using HDPro.CY.Order.IServices.SRM;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.Models;
using HDPro.CY.Order.Services.Common;
using HDPro.Entity.DomainModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.SRM
{
    /// <summary>
    /// SRM集成服务实现
    /// 提供与SRM系统集成的具体实现
    /// </summary>
    public class SRMIntegrationService : ISRMIntegrationService, IDependency
    {
        private readonly ILogger<SRMIntegrationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly SRMConfig _srmConfig;

        public SRMIntegrationService(
            ILogger<SRMIntegrationService> logger,
            HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _srmConfig = GetSRMConfig();
            
            // 配置HttpClient
            ConfigureHttpClient();
        }

        /// <summary>
        /// 配置HttpClient
        /// </summary>
        private void ConfigureHttpClient()
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_srmConfig.Timeout);
            
            // 设置默认请求头
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("appID", _srmConfig.AppId);
            _httpClient.DefaultRequestHeaders.Add("tokenKey", _srmConfig.TokenKey);
            
            _logger.LogInformation("SRM HttpClient配置完成，超时时间: {Timeout}秒, AppId: {AppId}",
                _srmConfig.Timeout, _srmConfig.AppId);

            // Content-Type应该在发送请求时通过HttpContent设置，而不是在默认请求头中
            // _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }



        /// <summary>
        /// 推送催单数据到SRM系统
        /// </summary>
        public async Task<WebResponseContent> PushOrderAskAsync(List<OrderAskData> orderAskList, string operatorName = null)
        {
            try
            {
                if (orderAskList == null || !orderAskList.Any())
                {
                    _logger.LogWarning("推送催单数据到SRM系统失败：催单数据不能为空");
                    return WebResponseContent.Instance.Error("催单数据不能为空");
                }

                _logger.LogInformation("开始推送催单数据到SRM系统，数据条数: {Count}, 操作人: {Operator}", 
                    orderAskList.Count, operatorName ?? "SYSTEM");

                // 转换数据格式
                var srmRequest = ConvertToSRMRequest(orderAskList, operatorName);

                // 记录转换后的请求数据
                _logger.LogInformation("转换后的SRM请求数据: {@SrmRequest}", srmRequest);

                // 发送请求
                var response = await SendToSRMAsync(srmRequest);

                if (response.Status)
                {
                    _logger.LogInformation("成功推送催单数据到SRM系统，数据条数: {Count}, 响应消息: {Message}", 
                        orderAskList.Count, response.Message);
                }
                else
                {
                    _logger.LogError("推送催单数据到SRM系统失败，数据条数: {Count}, 错误消息: {Message}", 
                        orderAskList.Count, response.Message);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "推送催单数据到SRM系统时发生异常，数据条数: {Count}, 操作人: {Operator}", 
                    orderAskList?.Count ?? 0, operatorName ?? "SYSTEM");
                return WebResponseContent.Instance.Error($"推送催单数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 推送单个催单数据到SRM系统
        /// </summary>
        public async Task<WebResponseContent> PushSingleOrderAskAsync(OrderAskData orderAskData, string operatorName = null)
        {
            if (orderAskData == null)
            {
                _logger.LogWarning("推送单个催单数据到SRM系统失败：催单数据不能为空");
                return WebResponseContent.Instance.Error("催单数据不能为空");
            }

            _logger.LogInformation("开始推送单个催单数据到SRM系统，订单号: {OrderNo}, 行号: {OrderLine}, 操作人: {Operator}", 
                orderAskData.OrderNo, orderAskData.OrderLine, operatorName ?? "SYSTEM");

            return await PushOrderAskAsync(new List<OrderAskData> { orderAskData }, operatorName);
        }

        /// <summary>
        /// 验证SRM连接状态
        /// </summary>
        public async Task<WebResponseContent> ValidateConnectionAsync()
        {
            try
            {
                _logger.LogInformation("开始验证SRM连接状态");

                // 构建请求URL
                var requestUrl = $"{_srmConfig.BaseUrl}{_srmConfig.OrderAskEndpoint}";
                
                _logger.LogInformation("SRM连接验证请求URL: {RequestUrl}", requestUrl);

                // 发送HEAD请求验证连接（HEAD请求不需要请求体）
                var request = new HttpRequestMessage(HttpMethod.Head, requestUrl);
                
                // 记录请求头信息
                _logger.LogDebug("SRM连接验证请求头: {@RequestHeaders}", request.Headers);
                
                var response = await _httpClient.SendAsync(request);
               
                _logger.LogInformation("SRM连接验证响应，状态码: {StatusCode}, 响应头: {@ResponseHeaders}", 
                    response.StatusCode, response.Headers);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SRM连接验证成功，状态码: {StatusCode}", response.StatusCode);
                    return WebResponseContent.Instance.OK("SRM连接正常");
                }
                else
                {
                    _logger.LogWarning("SRM连接验证失败，状态码: {StatusCode}, 原因: {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return WebResponseContent.Instance.Error($"SRM连接失败，状态码: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证SRM连接时发生异常，配置URL: {BaseUrl}", _srmConfig.BaseUrl);
                return WebResponseContent.Instance.Error($"SRM连接验证失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取SRM配置信息
        /// </summary>
        public SRMConfig GetSRMConfig()
        {
            try
            {
                var config = new SRMConfig
                {
                    BaseUrl = AppSetting.GetSettingString("SRM:BaseUrl"),
                    OrderAskEndpoint = AppSetting.GetSettingString("SRM:OrderAskEndpoint"),
                    AppId = AppSetting.GetSettingString("SRM:AppId"),
                    TokenKey = AppSetting.GetSettingString("SRM:TokenKey"),
                    Timeout = int.TryParse(AppSetting.GetSettingString("SRM:Timeout"), out int timeout) ? timeout : 30
                };

                _logger.LogInformation("获取SRM配置成功: BaseUrl={BaseUrl}, OrderAskEndpoint={OrderAskEndpoint}, AppId={AppId}, Timeout={Timeout}秒",
                    config.BaseUrl, config.OrderAskEndpoint, config.AppId, config.Timeout);

                // 验证配置完整性
                if (string.IsNullOrEmpty(config.BaseUrl) || 
                    string.IsNullOrEmpty(config.AppId) || 
                    string.IsNullOrEmpty(config.TokenKey))
                {
                    _logger.LogError("SRM配置信息不完整，BaseUrl: {BaseUrl}, AppId: {AppId}, TokenKey: {TokenKey}",
                        config.BaseUrl, config.AppId, string.IsNullOrEmpty(config.TokenKey) ? "空" : "已配置");
                    throw new InvalidOperationException("SRM配置信息不完整，请检查appsettings.json中的SRM配置");
                }

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取SRM配置信息失败");
                throw;
            }
        }

        /// <summary>
        /// 转换为SRM请求格式
        /// </summary>
        private SRMOrderAskRequest ConvertToSRMRequest(List<OrderAskData> orderAskList, string operatorName)
        {
            var srmData = orderAskList.Select(item => new SRMOrderAskData
            {
                aid = item.AskId,
                order_no = item.OrderNo,
                order_line = item.OrderLine,
                lev = item.UrgencyLevel,
                stype = item.ServiceType,
                times = item.Times,
                bz = item.Remark
            }).ToList();

            var request = new SRMOrderAskRequest
            {
                stime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                op = operatorName ?? "SYSTEM",
                data = srmData
            };

            _logger.LogDebug("数据格式转换完成，原始数据条数: {OriginalCount}, 转换后数据条数: {ConvertedCount}, 操作时间: {OperationTime}",
                orderAskList.Count, srmData.Count, request.stime);

            return request;
        }

        /// <summary>
        /// 发送数据到SRM系统
        /// </summary>
        private async Task<WebResponseContent> SendToSRMAsync(SRMOrderAskRequest srmRequest)
        {
            var requestUrl = string.Empty;
            var requestJson = string.Empty;
            var responseContent = string.Empty;
            var statusCode = System.Net.HttpStatusCode.InternalServerError;
            ApiLogHelper logHelper = null;

            try
            {
                // 构建请求URL
                requestUrl = $"{_srmConfig.BaseUrl}{_srmConfig.OrderAskEndpoint}";

                // 序列化请求数据
                requestJson = JsonConvert.SerializeObject(srmRequest, Formatting.None);

                _logger.LogInformation("发送SRM请求开始 - URL: {RequestUrl}", requestUrl);
                _logger.LogInformation("发送SRM请求参数: {RequestJson}", requestJson);

                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                // 开始记录日志
                logHelper = ApiLogHelper.Start("SRM催单推送", requestUrl, "POST", requestJson, _logger)
                    .WithRemark($"操作人: {srmRequest?.op}")
                    .WithDataCount(srmRequest?.data?.Count);

                // 发送POST请求
                var response = await _httpClient.PostAsync(requestUrl, content);
                statusCode = response.StatusCode;
                responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("发送SRM请求完成 - 状态码: {StatusCode}, 响应长度: {ResponseLength}字符",
                    response.StatusCode, responseContent?.Length ?? 0);
                _logger.LogInformation("SRM响应内容: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    // 解析SRM响应
                    var srmResponse = JsonConvert.DeserializeObject<SRMResponse>(responseContent);

                    if (srmResponse != null)
                    {
                        _logger.LogInformation("SRM响应解析成功 - 结果码: {ResultCode}, 消息: {Message}",
                            srmResponse.res, srmResponse.msg);

                        if (srmResponse.res == 1)
                        {
                            _logger.LogInformation("SRM请求处理成功");
                            await logHelper.WithResponseResult(responseContent).LogSuccessAsync((int)statusCode, responseContent);
                            return WebResponseContent.Instance.OK("催单数据推送成功");
                        }
                        else
                        {
                            _logger.LogError("SRM请求处理失败，错误代码: {ErrorCode}, 错误消息: {ErrorMessage}",
                                srmResponse.res, srmResponse.msg);
                            await logHelper.WithResponseResult(responseContent).LogFailureAsync($"SRM返回错误: {srmResponse.msg}", (int)statusCode, responseContent);
                            return WebResponseContent.Instance.Error($"SRM返回错误: {srmResponse.msg}");
                        }
                    }
                    else
                    {
                        _logger.LogError("SRM响应解析失败，响应内容为空或格式错误");
                        await logHelper.WithResponseResult(responseContent).LogFailureAsync("无法解析SRM响应数据", (int)statusCode, responseContent);
                        return WebResponseContent.Instance.Error("无法解析SRM响应数据");
                    }
                }
                else
                {
                    _logger.LogError("SRM HTTP请求失败 - 状态码: {StatusCode}, 原因: {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);
                    await logHelper.WithResponseResult(responseContent).LogFailureAsync($"HTTP请求失败，状态码: {response.StatusCode}", (int)statusCode, responseContent);
                    return WebResponseContent.Instance.Error($"HTTP请求失败，状态码: {response.StatusCode}, 响应: {responseContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "发送SRM HTTP请求时发生网络异常 - URL: {RequestUrl}, 请求参数: {RequestJson}",
                    requestUrl, requestJson);
                if (logHelper != null)
                    await logHelper.LogExceptionAsync(ex);
                return WebResponseContent.Instance.Error($"网络请求失败: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "发送SRM请求时发生超时 - URL: {RequestUrl}, 超时时间: {Timeout}秒, 请求参数: {RequestJson}",
                    requestUrl, _srmConfig.Timeout, requestJson);
                if (logHelper != null)
                    await logHelper.LogFailureAsync($"请求超时(超时设置: {_srmConfig.Timeout}秒)");
                return WebResponseContent.Instance.Error("请求超时，请检查网络连接或增加超时时间");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "SRM JSON序列化或反序列化异常 - 请求JSON: {RequestJson}, 响应内容: {ResponseContent}",
                    requestJson, responseContent);
                if (logHelper != null)
                    await logHelper.WithResponseResult(responseContent).LogExceptionAsync(ex, (int?)statusCode);
                return WebResponseContent.Instance.Error($"数据格式错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送SRM请求时发生未知异常 - URL: {RequestUrl}, 请求参数: {RequestJson}, 响应状态码: {StatusCode}, 响应内容: {ResponseContent}",
                    requestUrl, requestJson, statusCode, responseContent);
                if (logHelper != null)
                    await logHelper.WithResponseResult(responseContent).LogExceptionAsync(ex, (int?)statusCode);
                return WebResponseContent.Instance.Error($"发送失败: {ex.Message}");
            }
        }
    }
} 