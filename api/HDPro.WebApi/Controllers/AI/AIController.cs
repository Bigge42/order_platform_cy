using HDPro.Core.Configuration;
using HDPro.Core.ManageUser;
using HDPro.Core.UserManager;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Repositories;
using HDPro.Entity.DomainModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.WebApi.Controllers.AI
{
    [Route("api/AI")]
    public class AIController : Controller
    {
        private const string DefaultDifyBaseUrl = "http://10.11.10.101";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AIController> _logger;

        public AIController(IHttpClientFactory httpClientFactory, ILogger<AIController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("Apps")]
        public async Task<IActionResult> Apps(string keyword = "")
        {
            var query = GetAuthorizedAppQuery();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.AppName.Contains(keyword) ||
                    (x.Description != null && x.Description.Contains(keyword)));
            }

            var apps = await query
                .OrderBy(x => x.SortNo)
                .ThenBy(x => x.Id)
                .Select(x => new
                {
                    id = x.Id,
                    appName = x.AppName,
                    appType = x.AppType,
                    description = x.Description,
                    icon = x.Icon,
                    sortNo = x.SortNo
                })
                .ToListAsync();

            return Json(WebResponseContent.Instance.OK(null, apps));
        }

        [HttpGet("AppInfo")]
        public async Task<IActionResult> AppInfo(long appId)
        {
            if (appId <= 0)
            {
                return Json(WebResponseContent.Instance.Error("应用ID无效"));
            }

            var app = await GetAuthorizedAppQuery()
                .Where(x => x.Id == appId)
                .Select(x => new
                {
                    id = x.Id,
                    appName = x.AppName,
                    appType = x.AppType,
                    description = x.Description,
                    icon = x.Icon,
                    sortNo = x.SortNo
                })
                .FirstOrDefaultAsync();

            if (app == null)
            {
                return Json(WebResponseContent.Instance.Error("无权限访问该AI应用"));
            }

            return Json(WebResponseContent.Instance.OK(null, app));
        }

        [HttpGet("Conversations")]
        public async Task<IActionResult> Conversations(long appId, string lastId = "", int limit = 20)
        {
            var app = await GetAuthorizedAppAsync(appId);
            var error = ValidateProxyApp(appId, app);
            if (error != null)
            {
                return Json(error);
            }

            var query = new Dictionary<string, string>
            {
                ["user"] = GetDifyUserId(),
                ["limit"] = ClampLimit(limit).ToString()
            };

            if (!string.IsNullOrWhiteSpace(lastId))
            {
                query["last_id"] = lastId.Trim();
            }

            return await SendDifyAsync(app, HttpMethod.Get, "conversations", query);
        }

        [HttpGet("Messages")]
        public async Task<IActionResult> Messages(long appId, string conversationId, string firstId = "", int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                return Json(WebResponseContent.Instance.Error("会话ID不能为空"));
            }

            var app = await GetAuthorizedAppAsync(appId);
            var error = ValidateProxyApp(appId, app);
            if (error != null)
            {
                return Json(error);
            }

            var query = new Dictionary<string, string>
            {
                ["conversation_id"] = conversationId.Trim(),
                ["user"] = GetDifyUserId(),
                ["limit"] = ClampLimit(limit).ToString()
            };

            if (!string.IsNullOrWhiteSpace(firstId))
            {
                query["first_id"] = firstId.Trim();
            }

            return await SendDifyAsync(app, HttpMethod.Get, "messages", query);
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (request == null)
            {
                return Json(WebResponseContent.Instance.Error("请求参数不能为空"));
            }

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return Json(WebResponseContent.Instance.Error("消息内容不能为空"));
            }

            var app = await GetAuthorizedAppAsync(request.AppId);
            var error = ValidateProxyApp(request.AppId, app);
            if (error != null)
            {
                return Json(error);
            }

            var payload = new JObject
            {
                ["inputs"] = request.Inputs ?? new JObject(),
                ["query"] = request.Query.Trim(),
                ["response_mode"] = "blocking",
                ["user"] = GetDifyUserId(),
                ["auto_generate_name"] = true
            };

            if (!string.IsNullOrWhiteSpace(request.ConversationId))
            {
                payload["conversation_id"] = request.ConversationId.Trim();
            }

            if (request.Files?.HasValues == true)
            {
                payload["files"] = request.Files;
            }

            return await SendDifyAsync(app, HttpMethod.Post, "chat-messages", body: payload);
        }

        private async Task<Sys_AIApp> GetAuthorizedAppAsync(long appId)
        {
            if (appId <= 0)
            {
                return null;
            }

            return await GetAuthorizedAppQuery().FirstOrDefaultAsync(x => x.Id == appId);
        }

        private IQueryable<Sys_AIApp> GetAuthorizedAppQuery()
        {
            var appRepository = Sys_AIAppRepository.Instance;
            var roleAIAppRepository = Sys_RoleAIAppRepository.Instance;
            var query = appRepository.FindAsIQueryable(x => x.Status == 1);

            var roleIds = UserContext.Current.RoleIds ?? Array.Empty<int>();
            if (!UserContext.Current.IsSuperAdmin)
            {
                var authorizedAppIds = roleAIAppRepository
                    .FindAsIQueryable(auth => auth.Enable == 1 && roleIds.Contains(auth.Role_Id))
                    .Select(auth => auth.AIAppId);

                query = query.Where(app => authorizedAppIds.Contains(app.Id));
            }

            return query;
        }

        private static WebResponseContent ValidateProxyApp(long appId, Sys_AIApp app)
        {
            if (appId <= 0)
            {
                return WebResponseContent.Instance.Error("应用ID无效");
            }

            if (app == null)
            {
                return WebResponseContent.Instance.Error("无权限访问该AI应用");
            }

            if (string.IsNullOrWhiteSpace(app.PlatformAppKey))
            {
                return WebResponseContent.Instance.Error("AI应用未配置AppKey");
            }

            return null;
        }

        private async Task<IActionResult> SendDifyAsync(
            Sys_AIApp app,
            HttpMethod method,
            string path,
            Dictionary<string, string> query = null,
            JToken body = null)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(GetDifyTimeoutSeconds());

            using var httpRequest = new HttpRequestMessage(method, BuildDifyUrl(path, query));
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", app.PlatformAppKey.Trim());
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (body != null)
            {
                httpRequest.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");
            }

            try
            {
                using var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Dify request failed. AIAppId:{AIAppId}, Path:{Path}, Status:{Status}, Body:{Body}",
                        app.Id,
                        path,
                        (int)response.StatusCode,
                        TruncateForLog(responseBody));

                    return Json(WebResponseContent.Instance.Error(GetDifyErrorMessage(response.StatusCode, responseBody)));
                }

                return Json(WebResponseContent.Instance.OK(null, ParseJsonOrText(responseBody)));
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Dify request timeout. AIAppId:{AIAppId}, Path:{Path}", app.Id, path);
                return Json(WebResponseContent.Instance.Error("AI平台请求超时"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dify request exception. AIAppId:{AIAppId}, Path:{Path}", app.Id, path);
                return Json(WebResponseContent.Instance.Error("AI平台请求异常"));
            }
        }

        private static string BuildDifyUrl(string path, Dictionary<string, string> query)
        {
            var url = $"{GetDifyBaseUrl()}/{path.TrimStart('/')}";
            if (query == null || query.Count == 0)
            {
                return url;
            }

            var queryString = string.Join("&", query
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));

            return string.IsNullOrWhiteSpace(queryString) ? url : $"{url}?{queryString}";
        }

        private static string GetDifyBaseUrl()
        {
            var baseUrl = AppSetting.GetSettingString("Dify:BaseUrl")
                ?? AppSetting.GetSettingString("AI:DifyBaseUrl")
                ?? DefaultDifyBaseUrl;

            baseUrl = baseUrl.Trim().TrimEnd('/');
            return baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase)
                ? baseUrl
                : $"{baseUrl}/v1";
        }

        private static int GetDifyTimeoutSeconds()
        {
            if (!int.TryParse(AppSetting.GetSettingString("Dify:TimeoutSeconds"), out var seconds))
            {
                seconds = 120;
            }

            return Math.Max(10, Math.Min(seconds, 300));
        }

        private static int ClampLimit(int limit)
        {
            return Math.Max(1, Math.Min(limit, 100));
        }

        private static string GetDifyUserId()
        {
            var user = UserContext.Current.UserInfo;
            return string.IsNullOrWhiteSpace(user?.UserName)
                ? $"ocp-{UserContext.Current.UserId}"
                : user.UserName.Trim();
        }

        private static JToken ParseJsonOrText(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return new JObject();
            }

            try
            {
                return JToken.Parse(body);
            }
            catch (JsonException)
            {
                return JValue.CreateString(body);
            }
        }

        private static string GetDifyErrorMessage(HttpStatusCode statusCode, string body)
        {
            var message = "";
            try
            {
                var token = JToken.Parse(body);
                message = token["message"]?.ToString()
                    ?? token["error"]?.ToString()
                    ?? token["detail"]?.ToString();
            }
            catch
            {
                message = "";
            }

            return string.IsNullOrWhiteSpace(message)
                ? $"AI平台请求失败({(int)statusCode})"
                : $"AI平台请求失败({(int)statusCode})：{message}";
        }

        private static string TruncateForLog(string text, int maxLength = 300)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text.Length <= maxLength ? text : $"{text.Substring(0, maxLength)}...";
        }
    }

    public class SendMessageRequest
    {
        public long AppId { get; set; }
        public string Query { get; set; }
        public string ConversationId { get; set; }
        public JObject Inputs { get; set; }
        public JArray Files { get; set; }
    }
}
