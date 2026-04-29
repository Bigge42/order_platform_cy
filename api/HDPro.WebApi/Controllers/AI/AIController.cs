using HDPro.Core.Configuration;
using HDPro.Core.ManageUser;
using HDPro.Core.UserManager;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Repositories;
using HDPro.Entity.DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HDPro.WebApi.Controllers.AI
{
    [Authorize]
    [Route("api/AI")]
    public class AIController : Controller
    {
        private const string DefaultDifyBaseUrl = "http://10.11.10.100";

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

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(long appId, IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return Json(WebResponseContent.Instance.Error("上传文件不能为空"));
            }

            var app = await GetAuthorizedAppAsync(appId);
            var error = ValidateProxyApp(appId, app);
            if (error != null)
            {
                return Json(error);
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(GetDifyTimeoutSeconds());

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildDifyUrl("files/upload", null));
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", app.PlatformAppKey.Trim());
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var form = new MultipartFormDataContent();
            await using var fileStream = file.OpenReadStream();
            using var fileContent = new StreamContent(fileStream);
            if (!string.IsNullOrWhiteSpace(file.ContentType))
            {
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            }

            form.Add(fileContent, "file", file.FileName ?? "upload.bin");
            form.Add(new StringContent(GetDifyUserId()), "user");
            httpRequest.Content = form;

            try
            {
                using var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Dify upload file failed. AIAppId:{AIAppId}, Status:{Status}, Body:{Body}",
                        app.Id,
                        (int)response.StatusCode,
                        TruncateForLog(responseBody));

                    return Json(WebResponseContent.Instance.Error(GetDifyErrorMessage(response.StatusCode, responseBody)));
                }

                return Json(WebResponseContent.Instance.OK(null, ParseJsonOrText(responseBody)));
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Dify upload file timeout. AIAppId:{AIAppId}", app.Id);
                return Json(WebResponseContent.Instance.Error("AI平台上传文件超时"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dify upload file exception. AIAppId:{AIAppId}", app.Id);
                return Json(WebResponseContent.Instance.Error("AI平台上传文件异常"));
            }
        }

        [HttpGet("PreviewFile")]
        public async Task<IActionResult> PreviewFile(long appId, string fileId, bool asAttachment = false)
        {
            if (string.IsNullOrWhiteSpace(fileId))
            {
                return Json(WebResponseContent.Instance.Error("文件ID不能为空"));
            }

            var app = await GetAuthorizedAppAsync(appId);
            var error = ValidateProxyApp(appId, app);
            if (error != null)
            {
                return Json(error);
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(GetDifyTimeoutSeconds());

            var query = new Dictionary<string, string>
            {
                ["user"] = GetDifyUserId()
            };
            if (asAttachment)
            {
                query["as_attachment"] = "true";
            }

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Get,
                BuildDifyUrl($"files/{EncodePath(fileId)}/preview", query));
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", app.PlatformAppKey.Trim());

            try
            {
                using var response = await client.SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    HttpContext.RequestAborted);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Dify preview file failed. AIAppId:{AIAppId}, FileId:{FileId}, Status:{Status}, Body:{Body}",
                        app.Id,
                        fileId,
                        (int)response.StatusCode,
                        TruncateForLog(responseBody));

                    return Json(WebResponseContent.Instance.Error(GetDifyErrorMessage(response.StatusCode, responseBody)));
                }

                Response.StatusCode = 200;
                Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                if (response.Content.Headers.ContentLength.HasValue)
                {
                    Response.ContentLength = response.Content.Headers.ContentLength.Value;
                }
                if (response.Content.Headers.ContentDisposition != null)
                {
                    Response.Headers["Content-Disposition"] = response.Content.Headers.ContentDisposition.ToString();
                }

                await using var stream = await response.Content.ReadAsStreamAsync(HttpContext.RequestAborted);
                await stream.CopyToAsync(Response.Body, 81920, HttpContext.RequestAborted);
                await Response.Body.FlushAsync(HttpContext.RequestAborted);
                return new EmptyResult();
            }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("AI preview file disconnected by client. AIAppId:{AIAppId}, FileId:{FileId}", app.Id, fileId);
                return new EmptyResult();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Dify preview file timeout. AIAppId:{AIAppId}, FileId:{FileId}", app.Id, fileId);
                return Json(WebResponseContent.Instance.Error("AI平台预览文件超时"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dify preview file exception. AIAppId:{AIAppId}, FileId:{FileId}", app.Id, fileId);
                return Json(WebResponseContent.Instance.Error("AI平台预览文件异常"));
            }
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

            var payload = CreateChatPayload(request, "blocking");
            return await SendDifyAsync(app, HttpMethod.Post, "chat-messages", body: payload);
        }

        [HttpPost("StreamMessage")]
        public async Task<IActionResult> StreamMessage([FromBody] SendMessageRequest request)
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

            var payload = CreateChatPayload(request, "streaming");
            return await SendDifyStreamAsync(app, "chat-messages", payload);
        }

        [HttpPost("StopMessage")]
        public async Task<IActionResult> StopMessage([FromBody] StopMessageRequest request)
        {
            if (request == null)
            {
                return Json(WebResponseContent.Instance.Error("请求参数不能为空"));
            }

            if (string.IsNullOrWhiteSpace(request.TaskId))
            {
                return Json(WebResponseContent.Instance.Error("任务ID不能为空"));
            }

            var app = await GetAuthorizedAppAsync(request.AppId);
            var error = ValidateProxyApp(request.AppId, app);
            if (error != null)
            {
                return Json(error);
            }

            var payload = new JObject
            {
                ["user"] = GetDifyUserId()
            };

            var path = $"chat-messages/{EncodePath(request.TaskId)}/stop";
            return await SendDifyAsync(app, HttpMethod.Post, path, body: payload);
        }

        [HttpGet("AppParameters")]
        public async Task<IActionResult> AppParameters(long appId)
        {
            var app = await GetAuthorizedAppAsync(appId);
            var error = ValidateProxyApp(appId, app);
            if (error != null)
            {
                return Json(error);
            }

            return await SendDifyAsync(app, HttpMethod.Get, "parameters");
        }

        [HttpGet("SuggestedQuestions")]
        public async Task<IActionResult> SuggestedQuestions(long appId, string messageId)
        {
            if (string.IsNullOrWhiteSpace(messageId))
            {
                return Json(WebResponseContent.Instance.Error("消息ID不能为空"));
            }

            var app = await GetAuthorizedAppAsync(appId);
            var error = ValidateProxyApp(appId, app);
            if (error != null)
            {
                return Json(error);
            }

            var query = new Dictionary<string, string>
            {
                ["user"] = GetDifyUserId()
            };

            var path = $"messages/{EncodePath(messageId)}/suggested";
            return await SendDifyAsync(app, HttpMethod.Get, path, query);
        }

        [HttpPost("RenameConversation")]
        public async Task<IActionResult> RenameConversation([FromBody] RenameConversationRequest request)
        {
            if (request == null)
            {
                return Json(WebResponseContent.Instance.Error("请求参数不能为空"));
            }

            if (string.IsNullOrWhiteSpace(request.ConversationId))
            {
                return Json(WebResponseContent.Instance.Error("会话ID不能为空"));
            }

            if (!request.AutoGenerate && string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(WebResponseContent.Instance.Error("会话名称不能为空"));
            }

            var app = await GetAuthorizedAppAsync(request.AppId);
            var error = ValidateProxyApp(request.AppId, app);
            if (error != null)
            {
                return Json(error);
            }

            var payload = new JObject
            {
                ["user"] = GetDifyUserId(),
                ["auto_generate"] = request.AutoGenerate
            };

            if (!request.AutoGenerate)
            {
                payload["name"] = request.Name.Trim();
            }

            var path = $"conversations/{EncodePath(request.ConversationId)}/name";
            return await SendDifyAsync(app, HttpMethod.Post, path, body: payload);
        }

        [HttpPost("DeleteConversation")]
        public async Task<IActionResult> DeleteConversation([FromBody] ConversationActionRequest request)
        {
            if (request == null)
            {
                return Json(WebResponseContent.Instance.Error("请求参数不能为空"));
            }

            if (string.IsNullOrWhiteSpace(request.ConversationId))
            {
                return Json(WebResponseContent.Instance.Error("会话ID不能为空"));
            }

            var app = await GetAuthorizedAppAsync(request.AppId);
            var error = ValidateProxyApp(request.AppId, app);
            if (error != null)
            {
                return Json(error);
            }

            var payload = new JObject
            {
                ["user"] = GetDifyUserId()
            };

            var path = $"conversations/{EncodePath(request.ConversationId)}";
            return await SendDifyAsync(app, HttpMethod.Delete, path, body: payload);
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

        private async Task<IActionResult> SendDifyStreamAsync(Sys_AIApp app, string path, JToken body)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(GetDifyTimeoutSeconds());

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildDifyUrl(path, null));
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", app.PlatformAppKey.Trim());
            httpRequest.Headers.Accept.Clear();
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            httpRequest.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");

            try
            {
                using var response = await client.SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    HttpContext.RequestAborted);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Dify stream request failed. AIAppId:{AIAppId}, Path:{Path}, Status:{Status}, Body:{Body}",
                        app.Id,
                        path,
                        (int)response.StatusCode,
                        TruncateForLog(responseBody));

                    return Json(WebResponseContent.Instance.Error(GetDifyErrorMessage(response.StatusCode, responseBody)));
                }

                Response.StatusCode = 200;
                Response.ContentType = "text/event-stream; charset=utf-8";
                Response.Headers["Cache-Control"] = "no-cache";
                Response.Headers["X-Accel-Buffering"] = "no";
                HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

                await using var stream = await response.Content.ReadAsStreamAsync(HttpContext.RequestAborted);
                var buffer = ArrayPool<byte>.Shared.Rent(8192);
                try
                {
                    while (true)
                    {
                        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), HttpContext.RequestAborted);
                        if (bytesRead <= 0)
                        {
                            break;
                        }

                        await Response.Body.WriteAsync(buffer.AsMemory(0, bytesRead), HttpContext.RequestAborted);
                        await Response.Body.FlushAsync(HttpContext.RequestAborted);
                    }
                }
                catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
                {
                    _logger.LogInformation("AI stream disconnected by client. AIAppId:{AIAppId}, Path:{Path}", app.Id, path);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }

                return new EmptyResult();
            }
            catch (TaskCanceledException ex) when (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Dify stream request timeout. AIAppId:{AIAppId}, Path:{Path}", app.Id, path);
                return Json(WebResponseContent.Instance.Error("AI平台请求超时"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dify stream request exception. AIAppId:{AIAppId}, Path:{Path}", app.Id, path);
                return Json(WebResponseContent.Instance.Error("AI平台请求异常"));
            }
        }

        private static JObject CreateChatPayload(SendMessageRequest request, string responseMode)
        {
            var payload = new JObject
            {
                ["inputs"] = request.Inputs ?? new JObject(),
                ["query"] = request.Query.Trim(),
                ["response_mode"] = responseMode,
                ["user"] = GetDifyUserId(),
                ["auto_generate_name"] = request.AutoGenerateName ?? true
            };

            if (!string.IsNullOrWhiteSpace(request.ConversationId))
            {
                payload["conversation_id"] = request.ConversationId.Trim();
            }

            if (request.Files?.HasValues == true)
            {
                payload["files"] = request.Files;
            }

            return payload;
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

        private static string EncodePath(string value)
        {
            return Uri.EscapeDataString((value ?? string.Empty).Trim());
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
        public bool? AutoGenerateName { get; set; }
    }

    public class StopMessageRequest
    {
        public long AppId { get; set; }
        public string TaskId { get; set; }
    }

    public class ConversationActionRequest
    {
        public long AppId { get; set; }
        public string ConversationId { get; set; }
    }

    public class RenameConversationRequest : ConversationActionRequest
    {
        public string Name { get; set; }
        public bool AutoGenerate { get; set; }
    }
}
