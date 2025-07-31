using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using HDPro.Core.BaseProvider;
using Microsoft.EntityFrameworkCore;
using HDPro.Core.DBManager;
using Microsoft.Extensions.DependencyInjection;
using HDPro.Core.EFDbContext;
using Microsoft.AspNetCore.Http;

namespace HDPro.Core.Services
{
    /// <summary>
    /// 第三方应用服务实现
    /// </summary>
    public class ThirdPartyAppService : IThirdPartyAppService, IDependency
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ThirdPartyAppService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ThirdPartyAppService(
            IMemoryCache memoryCache,
            ILogger<ThirdPartyAppService> logger,
               IHttpContextAccessor httpContextAccessor)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 验证AppId和AppSecret
        /// </summary>
        public async Task<Sys_ThirdPartyApp> ValidateAppAsync(string appId, string appSecret)
        {
            try
            {
                // 先从缓存获取
                string cacheKey = $"ThirdPartyApp_{appId}";
                if (_memoryCache.TryGetValue(cacheKey, out Sys_ThirdPartyApp cachedApp))
                {
                    if (VerifyAppSecret(appSecret, cachedApp.AppSecret))
                    {
                        return cachedApp;
                    }
                    return null;
                }

                // 使用HttpContext中的DbContext，避免创建新实例
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var dbContext = httpContext.RequestServices.GetService<ServiceDbContext>();
                    if (dbContext != null)
                    {
                        var app = await dbContext.Set<Sys_ThirdPartyApp>()
                            .FirstOrDefaultAsync(x => x.AppId == appId);

                        if (app != null)
                        {
                            // 缓存5分钟
                            _memoryCache.Set(cacheKey, app, TimeSpan.FromMinutes(5));

                            if (VerifyAppSecret(appSecret, app.AppSecret))
                            {
                                return app;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证第三方应用失败: AppId={AppId}", appId);
                return null;
            }
        }

        /// <summary>
        /// 检查应用权限
        /// </summary>
        public async Task<bool> CheckPermissionAsync(string appId, string permission)
        {
            try
            {
                // 使用HttpContext中的DbContext，避免创建新实例
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var dbContext = httpContext.RequestServices.GetService<ServiceDbContext>();
                    if (dbContext != null)
                    {
                        var app = await dbContext.Set<Sys_ThirdPartyApp>()
                            .FirstOrDefaultAsync(x => x.AppId == appId && x.IsEnabled == 1);
                        return app != null;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查应用权限失败: AppId={AppId}, Permission={Permission}", appId, permission);
                return false;
            }
        }

        /// <summary>
        /// 增加访问次数
        /// </summary>
        public async Task IncrementAccessCountAsync(string appId)
        {
            try
            {
                // 使用HttpContext中的DbContext，避免创建新实例
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var dbContext = httpContext.RequestServices.GetService<ServiceDbContext>();
                    if (dbContext != null)
                    {
                        // 启用跟踪以便更新操作
                        dbContext.QueryTracking = true;
                        
                        var app = await dbContext.Set<Sys_ThirdPartyApp>()
                            .FirstOrDefaultAsync(x => x.AppId == appId);
                        
                        if (app != null)
                        {
                            app.AccessCount++;
                            app.LastAccessTime = DateTime.Now;
                            
                            await dbContext.SaveChangesAsync();
                            
                            // 更新缓存
                            string cacheKey = $"ThirdPartyApp_{appId}";
                            _memoryCache.Set(cacheKey, app, TimeSpan.FromMinutes(5));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新访问次数失败: AppId={AppId}", appId);
            }
        }

        /// <summary>
        /// 生成AppId
        /// </summary>
        public string GenerateAppId(string prefix = "CY1jOrd")
        {
            // 格式: 前缀 + 业务标识 + 日期 + 随机数
            string businessCode = "ASKBOX"; // 业务标识
            string dateCode = DateTime.Now.ToString("yyyyMMdd");
            string randomCode = GenerateRandomString(6, true);
            
            return $"{prefix}{businessCode}{dateCode}{randomCode}";
        }

        /// <summary>
        /// 生成AppSecret
        /// </summary>
        public string GenerateAppSecret(int length = 32)
        {
            // 生成包含大小写字母、数字和特殊字符的密钥
            return GenerateRandomString(length, false);
        }

        /// <summary>
        /// 验证AppSecret
        /// </summary>
        public bool VerifyAppSecret(string inputSecret, string storedSecret)
        {
            // 这里可以根据需要实现加密验证
            // 目前使用简单的字符串比较
            return string.Equals(inputSecret, storedSecret, StringComparison.Ordinal);
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        private string GenerateRandomString(int length, bool numbersOnly)
        {
            const string numbers = "0123456789";
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            const string specialChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789>*";

            string source = numbersOnly ? numbers : specialChars;
            
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes);
                
                var result = new StringBuilder(length);
                for (int i = 0; i < length; i++)
                {
                    result.Append(source[bytes[i] % source.Length]);
                }
                
                return result.ToString();
            }
        }
    }
} 