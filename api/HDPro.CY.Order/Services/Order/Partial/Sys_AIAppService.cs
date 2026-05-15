using HDPro.CY.Order.IRepositories;
using HDPro.Core.Configuration;
using HDPro.Core.Extensions;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services
{
    public partial class Sys_AIAppService
    {
        private readonly ISys_AIAppRepository _repository;
        private readonly ILogger<Sys_AIAppService> _logger;

        [ActivatorUtilitiesConstructor]
        public Sys_AIAppService(
            ISys_AIAppRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<Sys_AIAppService> logger
        )
            : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _logger = logger;
        }

        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
        }

        protected override WebResponseContent ValidateCYOrderEntity(Sys_AIApp entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            if (!response.Status)
            {
                return response;
            }

            if (entity.SortNo < 0)
            {
                return response.Error("\u6392\u5e8f\u53f7\u4e0d\u80fd\u5c0f\u4e8e0");
            }

            if (entity.Status != 0 && entity.Status != 1)
            {
                return response.Error("\u72b6\u6001\u53ea\u80fd\u4e3a\u542f\u7528\u6216\u505c\u7528");
            }

            return response;
        }

        public async Task<WebResponseContent> GetPlatformAppsAsync(string keyword = null)
        {
            try
            {
                using var session = await CreateConsoleSessionAsync();
                var platformApps = await GetConsoleAppsAsync(session, keyword);
                var platformIds = platformApps
                    .Select(x => x.PlatformAppId)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var platformKeyStateMap = await GetPlatformKeyStateMapAsync(session, platformIds);

                var existingApps = platformIds.Count == 0
                    ? new List<Sys_AIApp>()
                    : await _repository.DbContext.Set<Sys_AIApp>()
                        .AsNoTracking()
                        .Where(x => platformIds.Contains(x.PlatformAppId))
                        .Select(x => new Sys_AIApp
                        {
                            Id = x.Id,
                            AppName = x.AppName,
                            AppType = x.AppType,
                            PlatformAppId = x.PlatformAppId,
                            PlatformAppKey = x.PlatformAppKey,
                            Status = x.Status
                        })
                        .ToListAsync();

                var existingMap = existingApps
                    .GroupBy(x => x.PlatformAppId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

                var items = platformApps
                    .Select(x =>
                    {
                        existingMap.TryGetValue(x.PlatformAppId ?? string.Empty, out var localApp);
                        platformKeyStateMap.TryGetValue(x.PlatformAppId ?? string.Empty, out var platformKeyState);
                        return new
                        {
                            platformAppId = x.PlatformAppId,
                            appName = x.AppName,
                            appType = x.AppType,
                            description = x.Description,
                            icon = x.Icon,
                            exists = localApp != null,
                            localId = localApp?.Id,
                            localStatus = localApp == null ? (int?)null : localApp.Status,
                            hasLocalKey = !string.IsNullOrWhiteSpace(localApp?.PlatformAppKey),
                            hasPlatformKey = platformKeyState?.HasPlatformKey == true,
                            platformKeyCount = platformKeyState?.PlatformKeyCount ?? 0,
                            platformKeyTotalCount = platformKeyState?.TotalPlatformKeyCount ?? 0,
                            platformKeyUnavailableCount = platformKeyState?.UnavailablePlatformKeyCount ?? 0,
                            platformKeyReadFailed = platformKeyState?.ReadFailed == true
                        };
                    })
                    .OrderByDescending(x => x.exists)
                    .ThenBy(x => x.appName ?? string.Empty)
                    .ToList();

                return WebResponseContent.Instance.OK("\u83b7\u53d6\u5e73\u53f0\u5e94\u7528\u6210\u529f", new
                {
                    total = items.Count,
                    items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get platform AI apps failed. Keyword:{Keyword}", keyword);
                return WebResponseContent.Instance.Error($"\u83b7\u53d6\u5e73\u53f0\u5e94\u7528\u5931\u8d25\uff1a{ex.Message}");
            }
        }

        public async Task<WebResponseContent> SyncFromPlatformAsync(List<string> platformAppIds)
        {
            var selectedIds = (platformAppIds ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (selectedIds.Count == 0)
            {
                return WebResponseContent.Instance.Error("\u8bf7\u5148\u9009\u62e9\u9700\u8981\u540c\u6b65\u7684\u5e94\u7528");
            }

            try
            {
                using var session = await CreateConsoleSessionAsync();
                var platformApps = await GetConsoleAppsAsync(session, null);
                var platformAppMap = platformApps
                    .Where(x => !string.IsNullOrWhiteSpace(x.PlatformAppId))
                    .GroupBy(x => x.PlatformAppId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

                var selectedApps = selectedIds
                    .Where(x => platformAppMap.ContainsKey(x))
                    .Select(x => platformAppMap[x])
                    .ToList();

                var selectedPlatformIds = selectedApps
                    .Select(x => x.PlatformAppId)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var existingApps = selectedPlatformIds.Count == 0
                    ? new List<Sys_AIApp>()
                    : await _repository.DbContext.Set<Sys_AIApp>()
                        .Where(x => selectedPlatformIds.Contains(x.PlatformAppId))
                        .ToListAsync();

                var existingMap = existingApps
                    .GroupBy(x => x.PlatformAppId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

                var inserted = 0;
                var updated = 0;
                var createdKeys = 0;
                var details = new List<SyncFromPlatformItemResult>();

                foreach (var selectedId in selectedIds)
                {
                    if (!platformAppMap.TryGetValue(selectedId, out var platformApp))
                    {
                        details.Add(new SyncFromPlatformItemResult
                        {
                            PlatformAppId = selectedId,
                            AppName = selectedId,
                            Success = false,
                            Action = "missing",
                            Message = "\u5e73\u53f0\u672a\u8fd4\u56de\u8be5\u5e94\u7528\uff0c\u8bf7\u5237\u65b0\u5217\u8868\u540e\u91cd\u8bd5"
                        });
                        continue;
                    }

                    try
                    {
                        var syncResult = await SyncSinglePlatformAppAsync(session, platformApp, existingMap);
                        details.Add(syncResult);

                        if (!syncResult.Success)
                        {
                            continue;
                        }

                        if (string.Equals(syncResult.Action, "inserted", StringComparison.OrdinalIgnoreCase))
                        {
                            inserted++;
                        }
                        else if (string.Equals(syncResult.Action, "updated", StringComparison.OrdinalIgnoreCase))
                        {
                            updated++;
                        }

                        if (syncResult.CreatedPlatformKey)
                        {
                            createdKeys++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Sync single AI app from platform failed. PlatformAppId:{PlatformAppId}, AppName:{AppName}",
                            platformApp.PlatformAppId,
                            platformApp.AppName);

                        details.Add(new SyncFromPlatformItemResult
                        {
                            PlatformAppId = platformApp.PlatformAppId,
                            AppName = platformApp.AppName,
                            Success = false,
                            Action = "failed",
                            Message = ex.Message
                        });
                    }
                }

                var successCount = details.Count(x => x.Success);
                var failedCount = details.Count - successCount;
                var message = BuildSyncSummaryMessage(successCount, failedCount, inserted, updated);

                return WebResponseContent.Instance.OK(
                    message,
                    new
                    {
                        total = details.Count,
                        totalSelected = selectedIds.Count,
                        matchedCount = selectedApps.Count,
                        successCount,
                        failedCount,
                        inserted,
                        updated,
                        createdKeys,
                        details = details.Select(x => new
                        {
                            platformAppId = x.PlatformAppId,
                            appName = x.AppName,
                            success = x.Success,
                            action = x.Action,
                            keyAction = x.KeyAction,
                            message = x.Message
                        }).ToList()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sync AI apps from platform failed. Count:{Count}", selectedIds.Count);
                return WebResponseContent.Instance.Error($"\u540c\u6b65\u5e73\u53f0\u5e94\u7528\u5931\u8d25\uff1a{ex.Message}");
            }
        }

        public async Task<WebResponseContent> CreatePlatformAppKeyAsync(string platformAppId)
        {
            var normalizedPlatformAppId = (platformAppId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedPlatformAppId))
            {
                return WebResponseContent.Instance.Error("\u7f3a\u5c11\u5e73\u53f0 AppID\uff0c\u65e0\u6cd5\u521b\u5efa AppKey");
            }

            try
            {
                using var session = await CreateConsoleSessionAsync();
                var existingKey = await GetBestAvailableAppKeyAsync(session, normalizedPlatformAppId);
                if (existingKey != null)
                {
                    return WebResponseContent.Instance.OK(
                        "\u5e73\u53f0\u5df2\u5b58\u5728\u53ef\u7528 AppKey\uff0c\u65e0\u9700\u91cd\u590d\u521b\u5efa",
                        new
                        {
                            platformAppId = normalizedPlatformAppId,
                            created = false
                        });
                }

                await CreatePlatformAppKeyInternalAsync(session, normalizedPlatformAppId);
                return WebResponseContent.Instance.OK(
                    "\u5e73\u53f0 AppKey \u521b\u5efa\u6210\u529f\uff0c\u8bf7\u91cd\u65b0\u540c\u6b65\u5e94\u7528",
                    new
                    {
                        platformAppId = normalizedPlatformAppId,
                        created = true
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Create platform AI app key failed. PlatformAppId:{PlatformAppId}",
                    normalizedPlatformAppId);
                return WebResponseContent.Instance.Error($"\u521b\u5efa\u5e73\u53f0 AppKey \u5931\u8d25\uff1a{ex.Message}");
            }
        }

        private async Task<SyncFromPlatformItemResult> SyncSinglePlatformAppAsync(
            ConsoleSession session,
            PlatformAppInfo platformApp,
            Dictionary<string, Sys_AIApp> existingMap)
        {
            Sys_AIApp currentEntity = null!;

            try
            {
                var platformKey = await GetBestAvailableAppKeyAsync(session, platformApp.PlatformAppId);
                if (platformKey == null || string.IsNullOrWhiteSpace(platformKey.Token))
                {
                    return new SyncFromPlatformItemResult
                    {
                        PlatformAppId = platformApp.PlatformAppId,
                        AppName = platformApp.AppName,
                        Success = false,
                        Action = "failed",
                        KeyAction = "required",
                        Message = BuildPlatformKeyRequiredMessage()
                    };
                }

                var appKey = platformKey.Token;
                var key = platformApp.PlatformAppId ?? string.Empty;
                if (existingMap.TryGetValue(key, out var localApp))
                {
                    currentEntity = localApp;
                    var preserveLocalKey = !string.IsNullOrWhiteSpace(localApp.PlatformAppKey);

                    localApp.AppName = platformApp.AppName;
                    localApp.AppType = platformApp.AppType;
                    localApp.Description = platformApp.Description;
                    localApp.Icon = platformApp.Icon;
                    if (!preserveLocalKey)
                    {
                        localApp.PlatformAppKey = appKey;
                    }

                    localApp.SetModifyDefaultVal();
                    var validation = ValidateCYOrderEntity(localApp);
                    if (!validation.Status)
                    {
                        throw new InvalidOperationException(validation.Message ?? "\u540c\u6b65\u6821\u9a8c\u5931\u8d25");
                    }

                    await _repository.DbContext.SaveChangesAsync();
                    return new SyncFromPlatformItemResult
                    {
                        PlatformAppId = localApp.PlatformAppId,
                        AppName = localApp.AppName,
                        Success = true,
                        Action = "updated",
                        KeyAction = preserveLocalKey ? "preserved-local" : "reused",
                        CreatedPlatformKey = false,
                        Message = BuildSyncSuccessMessage("updated", preserveLocalKey)
                    };
                }

                var entity = new Sys_AIApp
                {
                    AppName = platformApp.AppName,
                    AppType = platformApp.AppType,
                    PlatformAppId = platformApp.PlatformAppId,
                    PlatformAppKey = appKey,
                    Description = platformApp.Description,
                    Icon = platformApp.Icon,
                    SortNo = 0,
                    Status = 1
                };
                entity.SetCreateDefaultVal();
                currentEntity = entity;

                var insertValidation = ValidateCYOrderEntity(entity);
                if (!insertValidation.Status)
                {
                    throw new InvalidOperationException(insertValidation.Message ?? "\u540c\u6b65\u6821\u9a8c\u5931\u8d25");
                }

                _repository.DbContext.Set<Sys_AIApp>().Add(entity);
                await _repository.DbContext.SaveChangesAsync();
                existingMap[key] = entity;

                return new SyncFromPlatformItemResult
                {
                    PlatformAppId = entity.PlatformAppId,
                    AppName = entity.AppName,
                    Success = true,
                    Action = "inserted",
                    KeyAction = "reused",
                    CreatedPlatformKey = false,
                    Message = BuildSyncSuccessMessage("inserted", false)
                };
            }
            catch
            {
                await CleanupFailedSyncEntityAsync(currentEntity);
                throw;
            }
        }

        private async Task CleanupFailedSyncEntityAsync(Sys_AIApp entity)
        {
            if (entity == null)
            {
                return;
            }

            var entry = _repository.DbContext.Entry(entity);
            if (entry == null)
            {
                return;
            }

            if (entry.State == EntityState.Added)
            {
                entry.State = EntityState.Detached;
                return;
            }

            if (entry.State == EntityState.Modified)
            {
                await entry.ReloadAsync();
            }
        }

        private static string BuildSyncSummaryMessage(int successCount, int failedCount, int inserted, int updated)
        {
            return $"\u540c\u6b65\u5b8c\u6210\uff0c\u6210\u529f {successCount} \u6761\uff0c\u5931\u8d25 {failedCount} \u6761\uff0c\u65b0\u589e {inserted} \u6761\uff0c\u66f4\u65b0 {updated} \u6761";
        }

        private static string BuildSyncSuccessMessage(string action, bool preserveLocalKey)
        {
            var actionText = string.Equals(action, "inserted", StringComparison.OrdinalIgnoreCase)
                ? "\u65b0\u589e\u6210\u529f"
                : "\u66f4\u65b0\u6210\u529f";

            if (preserveLocalKey)
            {
                return $"{actionText}\uff0c\u4fdd\u7559\u672c\u5730 AppKey";
            }

            return $"{actionText}\uff0c\u5df2\u83b7\u53d6\u5e73\u53f0\u53ef\u7528 AppKey";
        }

        private static string BuildPlatformKeyRequiredMessage()
        {
            return "\u5e73\u53f0\u65e0\u53ef\u7528 AppKey\uff0c\u8bf7\u5148\u521b\u5efa\u5e73\u53f0 AppKey \u540e\u518d\u540c\u6b65";
        }

        private async Task<List<PlatformAppInfo>> GetConsoleAppsAsync(ConsoleSession session, string keyword)
        {
            var apps = new List<PlatformAppInfo>();
            const int pageSize = 100;
            var page = 1;

            while (true)
            {
                var requestUrl = BuildAppsRequestUrl(session.ConsoleApiUrl, page, pageSize, keyword);
                var root = await SendConsoleRequestAsync(session, HttpMethod.Get, requestUrl);
                var items = GetArrayValue(root, "data", "items")
                    ?? GetArrayValue(root, "items")
                    ?? GetArrayValue(root, "data");

                if (items == null || items.Count == 0)
                {
                    break;
                }

                apps.AddRange(items
                    .OfType<JToken>()
                    .Select(MapPlatformApp)
                    .Where(x => !string.IsNullOrWhiteSpace(x.PlatformAppId)));

                var total = GetIntValue(root, "data", "total")
                    ?? GetIntValue(root, "total");
                var hasMore = GetBoolValue(root, "data", "has_more")
                    ?? GetBoolValue(root, "has_more");
                if (items.Count < pageSize)
                {
                    break;
                }

                if (total.HasValue && apps.Count >= total.Value)
                {
                    break;
                }

                if (hasMore.HasValue && !hasMore.Value)
                {
                    break;
                }

                page++;
                if (page > 100)
                {
                    break;
                }
            }

            return apps
                .GroupBy(x => x.PlatformAppId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToList();
        }

        private async Task<ConsoleSession> CreateConsoleSessionAsync()
        {
            var consoleApiUrl = GetConsoleApiUrl();
            var loginOptions = GetConsoleLoginOptions();

            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(GetTimeoutSeconds())
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OCP-SysAIApp-Sync/1.0");

            var session = new ConsoleSession(client, handler.CookieContainer, consoleApiUrl);
            await LoginConsoleAsync(session, loginOptions);
            return session;
        }

        private async Task LoginConsoleAsync(ConsoleSession session, ConsoleLoginOptions loginOptions)
        {
            var loginUrl = $"{session.ConsoleApiUrl}/login";
            var attempts = BuildConsoleLoginAttempts(loginOptions);
            if (attempts.Count == 0)
            {
                throw new InvalidOperationException("\u672a\u751f\u6210\u6709\u6548\u7684 Dify \u63a7\u5236\u53f0\u767b\u5f55\u8bf7\u6c42\uff0c\u8bf7\u68c0\u67e5 Dify \u914d\u7f6e");
            }

            HttpStatusCode lastStatusCode = HttpStatusCode.Unauthorized;
            string lastBody = string.Empty;
            var attemptedModes = new List<string>();

            foreach (var attempt in attempts)
            {
                var payload = new JObject
                {
                    [attempt.LoginField] = attempt.Account,
                    ["password"] = EncodeConsolePassword(loginOptions.Password, attempt.PasswordEncoding),
                    ["language"] = "zh-Hans",
                    ["remember_me"] = true
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, loginUrl)
                {
                    Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json")
                };

                AddConsoleHeaders(session, request, includeCsrf: false);
                using var response = await session.Client.SendAsync(request);
                lastStatusCode = response.StatusCode;
                lastBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TrySetConsoleSessionSecurity(session, lastBody);
                    return;
                }

                attemptedModes.Add(attempt.Description);
                if (IsHtmlResponse(lastBody))
                {
                    break;
                }
            }

            throw new InvalidOperationException(BuildConsoleLoginErrorMessage(
                session.ConsoleApiUrl,
                lastBody,
                lastStatusCode,
                attemptedModes,
                loginOptions
            ));
        }

        private async Task<PlatformAppKeyInfo> GetBestAvailableAppKeyAsync(ConsoleSession session, string platformAppId)
        {
            var keys = await GetPlatformAppKeysAsync(session, platformAppId);
            return keys
                .Where(x => x.IsUsable)
                .OrderByDescending(x => x.LastUsedAt ?? long.MinValue)
                .ThenByDescending(x => x.CreatedAt ?? long.MinValue)
                .ThenByDescending(x => x.KeyId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }

        private async Task<PlatformAppKeyInfo> CreatePlatformAppKeyInternalAsync(ConsoleSession session, string platformAppId)
        {
            var requestUrl = $"{session.ConsoleApiUrl}/apps/{Uri.EscapeDataString(platformAppId)}/api-keys";
            await SendConsoleRequestAsync(session, HttpMethod.Post, requestUrl);

            var bestKey = await GetBestAvailableAppKeyAsync(session, platformAppId);
            if (bestKey != null && !string.IsNullOrWhiteSpace(bestKey.Token))
            {
                return bestKey;
            }

            throw new InvalidOperationException("\u5e73\u53f0 AppKey \u521b\u5efa\u540e\u672a\u80fd\u83b7\u53d6\u5230\u53ef\u7528 token\uff0c\u8bf7\u5237\u65b0\u540e\u91cd\u8bd5");
        }

        private async Task<List<PlatformAppKeyInfo>> GetPlatformAppKeysAsync(ConsoleSession session, string platformAppId)
        {
            var requestUrl = $"{session.ConsoleApiUrl}/apps/{Uri.EscapeDataString(platformAppId)}/api-keys";
            var root = await SendConsoleRequestAsync(session, HttpMethod.Get, requestUrl);
            var items = GetArrayValue(root, "data")
                ?? GetArrayValue(root, "items")
                ?? new JArray();

            return items
                .OfType<JToken>()
                .Select(MapPlatformAppKey)
                .ToList();
        }

        private async Task<Dictionary<string, PlatformAppKeyState>> GetPlatformKeyStateMapAsync(
            ConsoleSession session,
            List<string> platformAppIds)
        {
            var result = new Dictionary<string, PlatformAppKeyState>(StringComparer.OrdinalIgnoreCase);
            foreach (var platformAppId in platformAppIds ?? new List<string>())
            {
                if (string.IsNullOrWhiteSpace(platformAppId))
                {
                    continue;
                }

                result[platformAppId] = await GetPlatformKeyStateAsync(session, platformAppId);
            }

            return result;
        }

        private async Task<PlatformAppKeyState> GetPlatformKeyStateAsync(ConsoleSession session, string platformAppId)
        {
            try
            {
                var keys = await GetPlatformAppKeysAsync(session, platformAppId);
                var usableCount = keys.Count(x => x.IsUsable);
                var totalCount = keys.Count;

                return new PlatformAppKeyState
                {
                    HasPlatformKey = usableCount > 0,
                    PlatformKeyCount = usableCount,
                    TotalPlatformKeyCount = totalCount,
                    UnavailablePlatformKeyCount = Math.Max(totalCount - usableCount, 0),
                    ReadFailed = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Get platform app key state failed. PlatformAppId:{PlatformAppId}",
                    platformAppId);

                return new PlatformAppKeyState
                {
                    HasPlatformKey = false,
                    PlatformKeyCount = 0,
                    TotalPlatformKeyCount = 0,
                    UnavailablePlatformKeyCount = 0,
                    ReadFailed = true
                };
            }
        }

        private async Task<JToken> SendConsoleRequestAsync(ConsoleSession session, HttpMethod method, string url, JToken body = null)
        {
            using var request = new HttpRequestMessage(method, url);
            AddConsoleHeaders(session, request, includeCsrf: method != HttpMethod.Get);
            if (body != null)
            {
                request.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
            }

            using var response = await session.Client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(GetResponseMessage(responseBody, response.StatusCode));
            }

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return new JObject();
            }

            return JToken.Parse(responseBody);
        }

        private static void AddConsoleHeaders(ConsoleSession session, HttpRequestMessage request, bool includeCsrf)
        {
            var origin = GetConsoleOrigin(session.ConsoleApiUrl);
            request.Headers.Referrer = new Uri(origin);
            request.Headers.TryAddWithoutValidation("Origin", origin.TrimEnd('/'));

            if (!string.IsNullOrWhiteSpace(session.AccessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
            }

            if (!string.IsNullOrWhiteSpace(session.CsrfToken))
            {
                request.Headers.TryAddWithoutValidation("X-CSRF-Token", session.CsrfToken);
            }
        }

        private static string BuildAppsRequestUrl(string consoleApiUrl, int page, int limit, string keyword)
        {
            var url = $"{consoleApiUrl}/apps?page={page}&limit={limit}";
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                url += $"&name={Uri.EscapeDataString(keyword.Trim())}";
            }

            return url;
        }

        private static PlatformAppInfo MapPlatformApp(JToken token)
        {
            var appName = token["name"]?.ToString()?.Trim();
            var description = token["description"]?.ToString()?.Trim();

            return new PlatformAppInfo
            {
                PlatformAppId = token["id"]?.ToString()?.Trim(),
                AppName = string.IsNullOrWhiteSpace(appName) ? "\u672a\u547d\u540d\u5e94\u7528" : appName,
                AppType = NormalizeAppType(token["mode"]?.ToString()),
                Description = description,
                Icon = token["icon"]?.ToString()?.Trim()
            };
        }

        private static PlatformAppKeyInfo MapPlatformAppKey(JToken token)
        {
            var key = new PlatformAppKeyInfo
            {
                KeyId = GetStringValue(token, "id")?.Trim() ?? string.Empty,
                Token = GetStringValue(token, "token")?.Trim() ?? string.Empty,
                LastUsedAt = GetLongValue(token, "last_used_at"),
                CreatedAt = GetLongValue(token, "created_at"),
                ExpiresAt = GetLongValue(token, "expires_at")
                    ?? GetLongValue(token, "expired_at")
                    ?? GetLongValue(token, "expire_at")
                    ?? GetLongValue(token, "end_at"),
                RevokedAt = GetLongValue(token, "revoked_at")
                    ?? GetLongValue(token, "disabled_at"),
                Status = GetStringValue(token, "status")?.Trim() ?? string.Empty,
                IsActive = GetBoolValue(token, "is_active")
                    ?? GetBoolValue(token, "active"),
                IsEnabled = GetBoolValue(token, "enabled"),
                IsValid = GetBoolValue(token, "is_valid"),
                IsDisabled = GetBoolValue(token, "disabled")
            };
            key.IsUsable = IsPlatformAppKeyUsable(key);
            return key;
        }

        private static bool IsPlatformAppKeyUsable(PlatformAppKeyInfo key)
        {
            if (key == null || string.IsNullOrWhiteSpace(key.Token))
            {
                return false;
            }

            if (key.IsDisabled == true || key.IsValid == false || key.IsEnabled == false || key.IsActive == false)
            {
                return false;
            }

            var status = (key.Status ?? string.Empty).Trim().ToLowerInvariant();
            if (status == "disabled"
                || status == "revoked"
                || status == "expired"
                || status == "inactive"
                || status == "invalid")
            {
                return false;
            }

            if (key.RevokedAt.HasValue && key.RevokedAt.Value > 0)
            {
                return false;
            }

            var nowUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (key.ExpiresAt.HasValue && key.ExpiresAt.Value > 0 && key.ExpiresAt.Value <= nowUnixSeconds)
            {
                return false;
            }

            return true;
        }

        private static string NormalizeAppType(string mode)
        {
            var normalized = (mode ?? string.Empty).Trim().ToLowerInvariant();
            if (normalized == "agent-chat")
            {
                return "agent";
            }

            if (normalized == "workflow")
            {
                return "workflow";
            }

            return "chat";
        }

        private static string GetConsoleApiUrl()
        {
            var consoleApiUrl = GetSetting("Dify:ConsoleApiUrl", "AI:DifyConsoleApiUrl");
            if (!string.IsNullOrWhiteSpace(consoleApiUrl))
            {
                return NormalizeConsoleApiUrl(consoleApiUrl);
            }

            var baseUrl = GetSetting("Dify:BaseUrl", "AI:DifyBaseUrl");
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("\u672a\u914d\u7f6e Dify:BaseUrl\uff0c\u65e0\u6cd5\u83b7\u53d6\u5e73\u53f0\u5e94\u7528");
            }

            return NormalizeConsoleApiUrl(baseUrl);
        }

        private static int GetTimeoutSeconds()
        {
            if (!int.TryParse(GetSetting("Dify:TimeoutSeconds"), out var seconds) || seconds <= 0)
            {
                return 120;
            }

            return seconds;
        }

        private static string GetSetting(params string[] keys)
        {
            foreach (var key in keys)
            {
                var value = AppSetting.GetSettingString(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }

        private static JArray GetArrayValue(JToken token, params string[] path)
        {
            return GetTokenValue(token, path) as JArray;
        }

        private static string GetStringValue(JToken token, params string[] path)
        {
            return GetTokenValue(token, path)?.ToString();
        }

        private static int? GetIntValue(JToken token, params string[] path)
        {
            return GetTokenValue(token, path)?.Value<int?>();
        }

        private static long? GetLongValue(JToken token, params string[] path)
        {
            var valueToken = GetTokenValue(token, path);
            if (valueToken == null || valueToken.Type == JTokenType.Null || valueToken.Type == JTokenType.Undefined)
            {
                return null;
            }

            if (valueToken.Type == JTokenType.Integer)
            {
                return valueToken.Value<long?>();
            }

            if (long.TryParse(valueToken.ToString(), out var longValue))
            {
                return longValue;
            }

            if (DateTimeOffset.TryParse(valueToken.ToString(), out var dateTimeOffset))
            {
                return dateTimeOffset.ToUnixTimeSeconds();
            }

            return null;
        }

        private static bool? GetBoolValue(JToken token, params string[] path)
        {
            return GetTokenValue(token, path)?.Value<bool?>();
        }

        private static JToken GetTokenValue(JToken token, params string[] path)
        {
            var current = token;
            foreach (var segment in path)
            {
                if (!(current is JObject currentObject))
                {
                    return null;
                }

                current = currentObject[segment];
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }

        private static ConsoleLoginOptions GetConsoleLoginOptions()
        {
            var employeeNo = GetSetting(
                "Dify:ConsoleEmployeeNo",
                "Dify:ConsoleAccount",
                "Dify:ConsoleUsername",
                "AI:DifyConsoleEmployeeNo"
            );
            var email = GetSetting("Dify:ConsoleEmail", "Dify:Email", "AI:DifyConsoleEmail");
            var password = GetSetting("Dify:ConsolePassword", "Dify:Password", "AI:DifyConsolePassword");
            var loginMode = NormalizeConsoleLoginMode(GetSetting("Dify:ConsoleLoginMode", "AI:DifyConsoleLoginMode"));
            var passwordEncoding = NormalizeConsolePasswordEncoding(
                GetSetting("Dify:ConsolePasswordEncoding", "AI:DifyConsolePasswordEncoding")
            );

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "\u672a\u914d\u7f6e Dify \u63a7\u5236\u53f0\u5bc6\u7801\uff0c\u8bf7\u8865\u5145 Dify:ConsolePassword"
                );
            }

            if (loginMode == "employee_no" && string.IsNullOrWhiteSpace(employeeNo))
            {
                throw new InvalidOperationException(
                    "\u5f53\u524d Dify:ConsoleLoginMode=employee_no\uff0c\u8bf7\u8865\u5145 Dify:ConsoleEmployeeNo"
                );
            }

            if (loginMode == "email" && string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException(
                    "\u5f53\u524d Dify:ConsoleLoginMode=email\uff0c\u8bf7\u8865\u5145 Dify:ConsoleEmail"
                );
            }

            if (string.IsNullOrWhiteSpace(employeeNo) && string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException(
                    "\u672a\u914d\u7f6e Dify \u63a7\u5236\u53f0\u8d26\u53f7\uff0c\u8bf7\u81f3\u5c11\u8865\u5145 Dify:ConsoleEmployeeNo \u6216 Dify:ConsoleEmail\uff0c\u5e76\u914d\u7f6e Dify:ConsolePassword"
                );
            }

            return new ConsoleLoginOptions
            {
                EmployeeNo = employeeNo,
                Email = email,
                Password = password,
                LoginMode = loginMode,
                PasswordEncoding = passwordEncoding
            };
        }

        private static List<ConsoleLoginAttempt> BuildConsoleLoginAttempts(ConsoleLoginOptions loginOptions)
        {
            var attempts = new List<ConsoleLoginAttempt>();
            if (loginOptions == null)
            {
                return attempts;
            }

            if (loginOptions.LoginMode == "employee_no")
            {
                AddEmployeeNoLoginAttempts(attempts, loginOptions.EmployeeNo, loginOptions.PasswordEncoding);
                return attempts;
            }

            if (loginOptions.LoginMode == "email")
            {
                AddEmailLoginAttempts(attempts, loginOptions.Email, loginOptions.PasswordEncoding);
                return attempts;
            }

            AddEmployeeNoLoginAttempts(attempts, loginOptions.EmployeeNo, loginOptions.PasswordEncoding);
            AddEmailLoginAttempts(attempts, loginOptions.Email, loginOptions.PasswordEncoding);
            return attempts;
        }

        private static void AddEmployeeNoLoginAttempts(List<ConsoleLoginAttempt> attempts, string employeeNo, string passwordEncoding)
        {
            if (string.IsNullOrWhiteSpace(employeeNo))
            {
                return;
            }

            AddConsoleLoginAttempt(attempts, "employee_no", employeeNo, "base64");
            if (passwordEncoding == "plain")
            {
                attempts.RemoveAll(x =>
                    x.LoginField.Equals("employee_no", StringComparison.OrdinalIgnoreCase)
                    && x.PasswordEncoding.Equals("base64", StringComparison.OrdinalIgnoreCase)
                );
                AddConsoleLoginAttempt(attempts, "employee_no", employeeNo, "plain");
                return;
            }

            if (passwordEncoding == "base64")
            {
                return;
            }

            AddConsoleLoginAttempt(attempts, "employee_no", employeeNo, "plain");
        }

        private static void AddEmailLoginAttempts(List<ConsoleLoginAttempt> attempts, string email, string passwordEncoding)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            AddConsoleLoginAttempt(attempts, "email", email, "plain");
            if (passwordEncoding == "base64")
            {
                attempts.RemoveAll(x =>
                    x.LoginField.Equals("email", StringComparison.OrdinalIgnoreCase)
                    && x.PasswordEncoding.Equals("plain", StringComparison.OrdinalIgnoreCase)
                );
                AddConsoleLoginAttempt(attempts, "email", email, "base64");
                return;
            }

            if (passwordEncoding == "plain")
            {
                return;
            }

            AddConsoleLoginAttempt(attempts, "email", email, "base64");
        }

        private static void AddConsoleLoginAttempt(
            List<ConsoleLoginAttempt> attempts,
            string loginField,
            string account,
            string passwordEncoding)
        {
            if (attempts.Any(x =>
                x.LoginField.Equals(loginField, StringComparison.OrdinalIgnoreCase)
                && x.Account.Equals(account, StringComparison.OrdinalIgnoreCase)
                && x.PasswordEncoding.Equals(passwordEncoding, StringComparison.OrdinalIgnoreCase)
            ))
            {
                return;
            }

            attempts.Add(new ConsoleLoginAttempt
            {
                LoginField = loginField,
                Account = account,
                PasswordEncoding = passwordEncoding,
                Description = $"{loginField}/{passwordEncoding}"
            });
        }

        private static string NormalizeConsoleLoginMode(string loginMode)
        {
            var normalized = (loginMode ?? string.Empty).Trim().ToLowerInvariant();
            if (normalized == "employee_no" || normalized == "employeeno" || normalized == "account" || normalized == "username")
            {
                return "employee_no";
            }

            if (normalized == "email" || normalized == "mail")
            {
                return "email";
            }

            return "auto";
        }

        private static string NormalizeConsolePasswordEncoding(string passwordEncoding)
        {
            var normalized = (passwordEncoding ?? string.Empty).Trim().ToLowerInvariant();
            if (normalized == "base64" || normalized == "encrypted")
            {
                return "base64";
            }

            if (normalized == "plain" || normalized == "raw")
            {
                return "plain";
            }

            return "auto";
        }

        private static string EncodeConsolePassword(string password, string passwordEncoding)
        {
            if (passwordEncoding == "base64")
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(password ?? string.Empty));
            }

            return password ?? string.Empty;
        }

        private static string NormalizeConsoleApiUrl(string rawUrl)
        {
            var url = (rawUrl ?? string.Empty).Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            if (url.EndsWith("/console/api", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            if (url.EndsWith("/console", StringComparison.OrdinalIgnoreCase))
            {
                return $"{url}/api";
            }

            if (url.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
            {
                url = url.Substring(0, url.Length - 3).TrimEnd('/');
            }

            return $"{url}/console/api";
        }

        private static string GetConsoleOrigin(string consoleApiUrl)
        {
            var uri = new Uri(EnsureTrailingSlash(consoleApiUrl));
            return $"{uri.Scheme}://{uri.Authority}/";
        }

        private static string EnsureTrailingSlash(string url)
        {
            return url.EndsWith("/", StringComparison.Ordinal) ? url : $"{url}/";
        }

        private static string GetResponseMessage(string responseBody, HttpStatusCode statusCode)
        {
            var bodyMessage = TryReadBodyMessage(responseBody);
            if (!string.IsNullOrWhiteSpace(bodyMessage))
            {
                return bodyMessage;
            }

            return $"\u5e73\u53f0\u63a5\u53e3\u8c03\u7528\u5931\u8d25\uff0c\u72b6\u6001\u7801\uff1a{(int)statusCode}";
        }

        private static string BuildConsoleLoginErrorMessage(
            string consoleApiUrl,
            string responseBody,
            HttpStatusCode statusCode,
            List<string> attemptedModes,
            ConsoleLoginOptions loginOptions)
        {
            var attemptsText = attemptedModes == null || attemptedModes.Count == 0
                ? string.Empty
                : $"\uff0c\u5df2\u5c1d\u8bd5\uff1a{string.Join("\u3001", attemptedModes.Distinct())}";

            if (IsHtmlResponse(responseBody))
            {
                return $"\u8bf7\u68c0\u67e5 Dify \u63a7\u5236\u53f0 API \u5730\u5740\u914d\u7f6e\uff0c\u5f53\u524d\u5b9e\u9645\u8bf7\u6c42\u7684 ConsoleApiUrl \u4e3a {consoleApiUrl}\uff0c\u767b\u5f55\u63a5\u53e3\u8fd4\u56de\u4e86 HTML \u9875\u9762\u800c\u4e0d\u662f JSON\u3002\u671f\u671b\u683c\u5f0f\u5e94\u7c7b\u4f3c http://host/console/api";
            }

            if (statusCode == HttpStatusCode.Unauthorized)
            {
                var tip = string.IsNullOrWhiteSpace(loginOptions?.EmployeeNo)
                    ? "\u82e5\u5f53\u524d\u767b\u5f55\u9875\u5df2\u5173\u95ed\u90ae\u7bb1\u767b\u5f55\uff0c\u8bf7\u6539\u914d Dify:ConsoleEmployeeNo \u5e76\u4f7f\u7528\u7f51\u9875\u767b\u5f55\u5bc6\u7801"
                    : "\u8bf7\u68c0\u67e5 Dify:ConsoleEmployeeNo/Dify:ConsoleEmail \u4e0e Dify:ConsolePassword \u662f\u5426\u4e3a\u6709\u6548\u7684 Dify \u63a7\u5236\u53f0\u8d26\u53f7";
                return $"\u63a7\u5236\u53f0\u767b\u5f55\u88ab\u62d2\u7edd\uff0c{tip}{attemptsText}\u3002\u63a5\u53e3\uff1a{consoleApiUrl}/login";
            }

            return $"\u63a7\u5236\u53f0\u767b\u5f55\u5931\u8d25\uff1a{GetResponseMessage(responseBody, statusCode)}{attemptsText}\uff0c\u63a5\u53e3\uff1a{consoleApiUrl}/login";
        }

        private static void TrySetConsoleSessionSecurity(ConsoleSession session, string responseBody)
        {
            var cookieUri = new Uri(EnsureTrailingSlash(session.ConsoleApiUrl));
            var csrfToken = session.CookieContainer
                .GetCookies(cookieUri)
                .Cast<Cookie>()
                .FirstOrDefault(x => x.Name.Equals("csrf_token", StringComparison.OrdinalIgnoreCase))
                ?.Value;

            if (!string.IsNullOrWhiteSpace(csrfToken))
            {
                session.CsrfToken = csrfToken;
            }

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return;
            }

            try
            {
                var root = JToken.Parse(responseBody);
                var accessToken = root["data"]?["access_token"]?.ToString()
                    ?? root["access_token"]?.ToString()
                    ?? root["token"]?.ToString();
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    session.AccessToken = accessToken.Trim();
                }
            }
            catch
            {
            }
        }

        private static string TryReadBodyMessage(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return null;
            }

            try
            {
                var root = JToken.Parse(responseBody);
                return root["message"]?.ToString()
                    ?? root["msg"]?.ToString()
                    ?? root["error"]?.ToString()
                    ?? root["detail"]?.ToString();
            }
            catch
            {
                return responseBody.Length > 300 ? responseBody.Substring(0, 300) : responseBody;
            }
        }

        private static bool IsHtmlResponse(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return false;
            }

            var body = responseBody.TrimStart();
            return body.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase)
                || body.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class ConsoleSession : IDisposable
        {
            public ConsoleSession(HttpClient client, CookieContainer cookieContainer, string consoleApiUrl)
            {
                Client = client;
                CookieContainer = cookieContainer;
                ConsoleApiUrl = consoleApiUrl.TrimEnd('/');
            }

            public HttpClient Client { get; }

            public CookieContainer CookieContainer { get; }

            public string ConsoleApiUrl { get; }

            public string CsrfToken { get; set; } = string.Empty;

            public string AccessToken { get; set; } = string.Empty;

            public void Dispose()
            {
                Client.Dispose();
            }
        }

        private sealed class PlatformAppInfo
        {
            public string PlatformAppId { get; set; } = string.Empty;

            public string AppName { get; set; } = string.Empty;

            public string AppType { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public string Icon { get; set; } = string.Empty;
        }

        private sealed class PlatformAppKeyState
        {
            public bool HasPlatformKey { get; set; }

            public int PlatformKeyCount { get; set; }

            public int TotalPlatformKeyCount { get; set; }

            public int UnavailablePlatformKeyCount { get; set; }

            public bool ReadFailed { get; set; }
        }

        private sealed class PlatformAppKeyInfo
        {
            public string KeyId { get; set; } = string.Empty;

            public string Token { get; set; } = string.Empty;

            public long? LastUsedAt { get; set; }

            public long? CreatedAt { get; set; }

            public long? ExpiresAt { get; set; }

            public long? RevokedAt { get; set; }

            public string Status { get; set; } = string.Empty;

            public bool? IsActive { get; set; }

            public bool? IsEnabled { get; set; }

            public bool? IsValid { get; set; }

            public bool? IsDisabled { get; set; }

            public bool IsUsable { get; set; }
        }

        private sealed class ConsoleLoginOptions
        {
            public string EmployeeNo { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;

            public string LoginMode { get; set; } = "auto";

            public string PasswordEncoding { get; set; } = "auto";
        }

        private sealed class ConsoleLoginAttempt
        {
            public string LoginField { get; set; } = string.Empty;

            public string Account { get; set; } = string.Empty;

            public string PasswordEncoding { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;
        }

        private sealed class SyncFromPlatformItemResult
        {
            public string PlatformAppId { get; set; } = string.Empty;

            public string AppName { get; set; } = string.Empty;

            public bool Success { get; set; }

            public string Action { get; set; } = string.Empty;

            public string KeyAction { get; set; } = string.Empty;

            public bool CreatedPlatformKey { get; set; }

            public string Message { get; set; } = string.Empty;
        }
    }
}
