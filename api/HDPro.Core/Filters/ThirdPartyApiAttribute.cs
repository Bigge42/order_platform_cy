using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.Configuration;
using HDPro.Core.Extensions;
using HDPro.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Linq;

namespace HDPro.Core.Filters
{
    /// <summary>
    /// 第三方API权限验证接口
    /// </summary>
    public interface IThirdPartyApiFilter : IFilterMetadata
    {
        Task OnAuthorizationAsync(AuthorizationFilterContext context);
    }

    /// <summary>
    /// 第三方API权限验证特性
    /// 使用方式: [ThirdPartyApi]
    /// </summary>
    public class ThirdPartyApiAttribute : Attribute, IThirdPartyApiFilter, IAllowAnonymous, IAsyncAuthorizationFilter
    {
        /// <summary>
        /// 权限标识，用于标记接口权限范围
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// 是否验证IP白名单
        /// </summary>
        public bool ValidateIP { get; set; } = true;

        /// <summary>
        /// 是否验证过期时间
        /// </summary>
        public bool ValidateExpire { get; set; } = true;

        public ThirdPartyApiAttribute(string permission = "")
        {
            Permission = permission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await ThirdPartyApiAuthorization.ValidationAsync(context, Permission, ValidateIP, ValidateExpire);
        }

        public async Task<AuthorizationFilterContext> OnAuthorizationInternalAsync(AuthorizationFilterContext context)
        {
            return await ThirdPartyApiAuthorization.ValidationAsync(context, Permission, ValidateIP, ValidateExpire);
        }
    }

    /// <summary>
    /// 第三方API权限验证核心逻辑
    /// </summary>
    public static class ThirdPartyApiAuthorization
    {
        public const string AppIdHeaderName = "X-App-Id";
        public const string AppSecretHeaderName = "X-App-Secret";
        public const string TimestampHeaderName = "X-Timestamp";
        public const string SignatureHeaderName = "X-Signature";

        /// <summary>
        /// 验证第三方API权限
        /// </summary>
        /// <param name="context">授权过滤器上下文</param>
        /// <param name="permission">权限标识</param>
        /// <param name="validateIP">是否验证IP</param>
        /// <param name="validateExpire">是否验证过期时间</param>
        /// <returns></returns>
        public static async Task<AuthorizationFilterContext> ValidationAsync(
            AuthorizationFilterContext context, 
            string permission = "", 
            bool validateIP = true, 
            bool validateExpire = true)
        {
            try
            {
                // 获取请求头信息
                var headers = context.HttpContext.Request.Headers;
                
                if (!headers.TryGetValue(AppIdHeaderName, out StringValues appIdValue) ||
                    !headers.TryGetValue(AppSecretHeaderName, out StringValues appSecretValue))
                {
                    return SetUnauthorizedResult(context, "缺少必要的认证头信息");
                }

                string appId = appIdValue.ToString();
                string appSecret = appSecretValue.ToString();

                if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
                {
                    return SetUnauthorizedResult(context, "AppId或AppSecret不能为空");
                }

                // 获取第三方应用服务
                var thirdPartyService = context.HttpContext.RequestServices.GetService<IThirdPartyAppService>();
                if (thirdPartyService == null)
                {
                    return SetUnauthorizedResult(context, "第三方应用服务未注册");
                }

                // 验证AppId和AppSecret
                var app = await thirdPartyService.ValidateAppAsync(appId, appSecret);
                if (app == null)
                {
                    return SetUnauthorizedResult(context, "无效的AppId或AppSecret");
                }

                // 验证应用是否启用
                if (app.IsEnabled != 1)
                {
                    return SetUnauthorizedResult(context, "应用已被禁用");
                }

                // 验证过期时间
                if (validateExpire && app.ExpireTime.HasValue && app.ExpireTime.Value < DateTime.Now)
                {
                    return SetUnauthorizedResult(context, "应用已过期");
                }

                // 验证IP白名单
                if (validateIP && !string.IsNullOrEmpty(app.AllowedIPs))
                {
                    string clientIP = GetClientIP(context.HttpContext);
                    if (!IsIPAllowed(clientIP, app.AllowedIPs))
                    {
                        return SetUnauthorizedResult(context, "IP地址不在白名单中");
                    }
                }

                // 验证签名（可选）
                if (headers.TryGetValue(SignatureHeaderName, out StringValues signatureValue) &&
                    headers.TryGetValue(TimestampHeaderName, out StringValues timestampValue))
                {
                    if (!ValidateSignature(context, appSecret, timestampValue, signatureValue))
                    {
                        return SetUnauthorizedResult(context, "签名验证失败");
                    }
                }

                // 验证权限（如果指定了权限标识）
                if (!string.IsNullOrEmpty(permission))
                {
                    bool hasPermission = await thirdPartyService.CheckPermissionAsync(appId, permission);
                    if (!hasPermission)
                    {
                        return SetUnauthorizedResult(context, $"没有权限访问: {permission}");
                    }
                }

                // 记录访问次数
                await thirdPartyService.IncrementAccessCountAsync(appId);

                // 设置当前第三方应用信息到HttpContext
                context.HttpContext.Items["ThirdPartyApp"] = app;
                context.HttpContext.Items["ThirdPartyAppId"] = appId;

                return context;
            }
            catch (Exception ex)
            {
                return SetUnauthorizedResult(context, $"认证过程发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置未授权结果
        /// </summary>
        private static AuthorizationFilterContext SetUnauthorizedResult(AuthorizationFilterContext context, string message)
        {
            context.Result = new ContentResult()
            {
                Content = new { message = message, status = false, code = 401 }.Serialize(),
                ContentType = "application/json",
                StatusCode = 401
            };
            return context;
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        private static string GetClientIP(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            string ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = httpContext.Connection.RemoteIpAddress?.ToString();
            }
            return ip ?? "unknown";
        }

        /// <summary>
        /// 验证IP是否在白名单中
        /// </summary>
        private static bool IsIPAllowed(string clientIP, string allowedIPs)
        {
            if (string.IsNullOrEmpty(allowedIPs))
                return true;

            var ipList = allowedIPs.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ip in ipList)
            {
                if (ip.Trim() == "*" || ip.Trim() == clientIP)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 验证签名
        /// </summary>
        private static bool ValidateSignature(AuthorizationFilterContext context, string appSecret, string timestamp, string signature)
        {
            try
            {
                // 验证时间戳（防重放攻击）
                if (long.TryParse(timestamp, out long ts))
                {
                    var requestTime = DateTimeOffset.FromUnixTimeSeconds(ts).DateTime;
                    if (Math.Abs((DateTime.UtcNow - requestTime).TotalMinutes) > 5) // 5分钟有效期
                    {
                        return false;
                    }
                }

                // 构建签名字符串
                var request = context.HttpContext.Request;
                var method = request.Method;
                var path = request.Path.Value;
                var query = request.QueryString.Value;
                
                string signString = $"{method}{path}{query}{timestamp}{appSecret}";
                string computedSignature = ComputeHmacSha256(signString, appSecret);

                return signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 计算HMAC-SHA256签名
        /// </summary>
        private static string ComputeHmacSha256(string data, string key)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }
    }
} 