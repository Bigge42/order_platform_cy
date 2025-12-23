using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IRepositories.MaterialCallBoard;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HDPro.CY.Order.Services.MaterialCallBoard
{
    public class MaterialCallWmsSyncService : IMaterialCallWmsSyncService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _cfg;
        private readonly IMaterialCallWorkOrderSetRepository _setRepo;
        private readonly ILogger<MaterialCallWmsSyncService> _logger;

        public MaterialCallWmsSyncService(
            IHttpClientFactory httpFactory,
            IConfiguration cfg,
            IMaterialCallWorkOrderSetRepository setRepo,
            ILogger<MaterialCallWmsSyncService> logger)
        {
            _httpFactory = httpFactory;
            _cfg = cfg;
            _setRepo = setRepo;
            _logger = logger;
        }

        public async Task<WebResponseContent> SyncSnapshotFromWmsAsync(bool pruneAfter = false)
        {
            var tokenUrl = _cfg["Wms:TokenUrl"];
            var reportUrl = _cfg["Wms:ReportUrl"];
            var apiKey = _cfg["Wms:ApiKey"];
            var reportId = _cfg["Wms:ReportId"];
            var warehouse = _cfg["Wms:Warehouse"];
            var pageSize = Math.Max(1, int.TryParse(_cfg["Wms:PageSize"], out var ps) ? ps : 5000);

            if (string.IsNullOrWhiteSpace(tokenUrl) || string.IsNullOrWhiteSpace(reportUrl))
                return ErrorWithCode("CONFIG_MISSING", "WMS 接口地址未配置");

            _logger.LogInformation(
                "WMS 同步开始，pruneAfter={PruneAfter}, warehouse={Warehouse}, pageSize={PageSize}",
                pruneAfter,
                warehouse,
                pageSize);

            var http = _httpFactory.CreateClient(nameof(MaterialCallWmsSyncService));
            http.Timeout = TimeSpan.FromMinutes(5);

            var wmsWatch = Stopwatch.StartNew();
            int? lastHttpStatusCode = null;
            string lastHttpReason = null;

            try
            {
                // 1) 拿 token
                var tokenReq = $"{tokenUrl}?apikey={Uri.EscapeDataString(apiKey ?? string.Empty)}";
                var tokenWatch = Stopwatch.StartNew();
                using var tokenResp = await http.GetAsync(tokenReq);
                var tokenText = await tokenResp.Content.ReadAsStringAsync();

                lastHttpStatusCode = (int)tokenResp.StatusCode;
                lastHttpReason = tokenResp.ReasonPhrase;

                _logger.LogInformation(
                    "WMS 同步 token 请求完成，status={StatusCode}, elapsed_ms={ElapsedMs}, body={Body}",
                    (int)tokenResp.StatusCode,
                    tokenWatch.ElapsedMilliseconds,
                    SafeTrim(tokenText));

                if (!tokenResp.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "WMS 同步阶段：调用 WMS 失败，step=token, status={StatusCode}, elapsed_ms={ElapsedMs}, reason={Reason}",
                        (int)tokenResp.StatusCode,
                        tokenWatch.ElapsedMilliseconds,
                        tokenResp.ReasonPhrase);

                    _logger.LogInformation(
                        "WMS 同步阶段：调用 WMS 结束，elapsed_ms={ElapsedMs}, last_status={LastStatus}, last_reason={LastReason}",
                        wmsWatch.ElapsedMilliseconds,
                        lastHttpStatusCode,
                        lastHttpReason);

                    return ErrorWithCode(
                        "WMS_TOKEN_HTTP_ERROR",
                        $"获取 WMS token 失败：HTTP {(int)tokenResp.StatusCode} {tokenResp.ReasonPhrase}; 响应：{SafeSnippet(tokenText)}");
                }

                var token = ExtractToken(tokenText);
                if (string.IsNullOrWhiteSpace(token))
                    return ErrorWithCode("WMS_TOKEN_PARSE_ERROR", "获取 WMS token 失败：响应缺少 token 字段");

                // 2) 分页拉取
                var all = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int skip = 0, total = -1, page = 1;
                var pullWatch = Stopwatch.StartNew();

                try
                {
                    while (true)
                    {
                        var pageWatch = Stopwatch.StartNew();
                        var body = BuildReportBody(reportId, warehouse, token, apiKey, skip, pageSize);
                        using var req = new StringContent(body, Encoding.UTF8, "application/json");
                        var resp = await http.PostAsync(reportUrl, req);
                        var text = await resp.Content.ReadAsStringAsync();

                        lastHttpStatusCode = (int)resp.StatusCode;
                        lastHttpReason = resp.ReasonPhrase;

                        _logger.LogInformation(
                            "WMS 同步调用 WMS，page={Page}, skip={Skip}, status={StatusCode}, elapsed_ms={ElapsedMs}",
                            page,
                            skip,
                            (int)resp.StatusCode,
                            pageWatch.ElapsedMilliseconds);

                        if (!resp.IsSuccessStatusCode)
                        {
                            var desc = string.IsNullOrWhiteSpace(resp.ReasonPhrase)
                                ? ((int)resp.StatusCode).ToString()
                                : $"{(int)resp.StatusCode} {resp.ReasonPhrase}";

                            var snippet = SafeSnippet(text);

                            _logger.LogWarning(
                                "WMS 同步调用 WMS 失败，status={StatusCode}, elapsed_ms={ElapsedMs}, body={Body}",
                                desc,
                                pageWatch.ElapsedMilliseconds,
                                SafeTrim(text));

                            _logger.LogInformation(
                                "WMS 同步阶段：调用 WMS 结束，elapsed_ms={ElapsedMs}, last_status={LastStatus}, last_reason={LastReason}",
                                wmsWatch.ElapsedMilliseconds,
                                lastHttpStatusCode,
                                lastHttpReason);

                            return ErrorWithCode(
                                "WMS_QUERY_HTTP_ERROR",
                                $"WMS 查询失败：HTTP {desc}；响应：{snippet}");
                        }

                        using var doc = JsonDocument.Parse(text);
                        var root = doc.RootElement;

                        // Flag 校验
                        if (root.TryGetProperty("Flag", out var flag) && flag.ValueKind == JsonValueKind.False)
                        {
                            var msg = root.TryGetProperty("ErrorMsg", out var e) ? e.GetString() : "WMS 返回 Flag=false";
                            _logger.LogWarning(
                                "WMS 同步调用 WMS 返回 Flag=false，page={Page}, elapsed_ms={ElapsedMs}, msg={Message}",
                                page,
                                pageWatch.ElapsedMilliseconds,
                                msg);

                            return ErrorWithCode("WMS_QUERY_FLAG_FALSE", $"WMS 查询失败：{msg}");
                        }

                        // 总量
                        if (total < 0 &&
                            root.TryGetProperty("PageInfo", out var pi) &&
                            pi.TryGetProperty("TotalCount", out var tc) &&
                            tc.TryGetInt32(out var totalCount))
                        {
                            total = totalCount;
                        }

                        // Data[]
                        if (root.TryGetProperty("Data", out var data) && data.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var row in data.EnumerateArray())
                            {
                                if (row.TryGetProperty("WorkOrderCode", out var w) && w.ValueKind == JsonValueKind.String)
                                {
                                    var code = w.GetString();
                                    if (!string.IsNullOrWhiteSpace(code)) all.Add(code.Trim());
                                }
                            }
                        }

                        if (total >= 0 && all.Count >= total) break; // 取满就停
                        page++;
                        skip = (page - 1) * pageSize;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "WMS 同步阶段：调用 WMS 异常，page={Page}, skip={Skip}, elapsed_ms={ElapsedMs}, last_status={LastStatus}, last_reason={LastReason}",
                        page,
                        skip,
                        wmsWatch.ElapsedMilliseconds,
                        lastHttpStatusCode,
                        lastHttpReason);
                    return ErrorWithCode("WMS_QUERY_EXCEPTION", "调用 WMS 接口异常：" + ex.Message);
                }

                _logger.LogInformation(
                    "WMS 同步调用 WMS 完成，pages={Pages}, total={Total}, elapsed_ms={ElapsedMs}, last_status={LastStatus}, last_reason={LastReason}",
                    page - 1,
                    total,
                    pullWatch.ElapsedMilliseconds,
                    lastHttpStatusCode,
                    lastHttpReason);

                _logger.LogInformation(
                    "WMS 同步阶段：调用 WMS 结束，elapsed_ms={ElapsedMs}, last_status={LastStatus}, last_reason={LastReason}",
                    wmsWatch.ElapsedMilliseconds,
                    lastHttpStatusCode,
                    lastHttpReason);

                // 3) 刷新白名单快照
                var codes = new List<string>(all);
                var snapshotWatch = Stopwatch.StartNew();
                int totalSet;
                int matched;
                _logger.LogInformation(
                    "WMS 同步阶段：写入快照开始，count={Count}",
                    codes.Count);
                try
                {
                    (totalSet, matched) = await _setRepo.RefreshSnapshotAsync(codes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "WMS 同步写入快照异常，elapsed_ms={ElapsedMs}, last_status={LastStatus}, last_reason={LastReason}",
                        snapshotWatch.ElapsedMilliseconds,
                        lastHttpStatusCode,
                        lastHttpReason);
                    return ErrorWithCode("WMS_SNAPSHOT_FAILED", "WMS 快照写入失败：" + ex.Message);
                }

                _logger.LogInformation(
                    "WMS 同步写入快照完成，elapsed_ms={ElapsedMs}, totalSet={TotalSet}, matched={Matched}, last_status={LastStatus}",
                    snapshotWatch.ElapsedMilliseconds,
                    totalSet,
                    matched,
                    lastHttpStatusCode);

                // 4) 可选：按白名单删除叫料看板中缺席项
                int deleted = 0;
                if (pruneAfter)
                {
                    var pruneWatch = Stopwatch.StartNew();
                    _logger.LogInformation(
                        "WMS 同步阶段：prune 删除开始，pruneAfter={PruneAfter}, last_status={LastStatus}",
                        pruneAfter,
                        lastHttpStatusCode);
                    try
                    {
                        deleted = await _setRepo.PruneMaterialCallBoardAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "WMS 同步 prune 删除异常，elapsed_ms={ElapsedMs}, last_status={LastStatus}, last_reason={LastReason}",
                            pruneWatch.ElapsedMilliseconds,
                            lastHttpStatusCode,
                            lastHttpReason);
                        return ErrorWithCode("WMS_PRUNE_FAILED", "WMS prune 删除失败：" + ex.Message);
                    }

                    _logger.LogInformation(
                        "WMS 同步 prune 删除完成，elapsed_ms={ElapsedMs}, deleted={Deleted}, last_status={LastStatus}",
                        pruneWatch.ElapsedMilliseconds,
                        deleted,
                        lastHttpStatusCode);
                }

                var ok = WebResponseContent.Instance.OK(
                    $"WMS 同步完成：白名单 {totalSet} 条（看板已存在 {matched} 条）" +
                    (pruneAfter ? $"，已删除缺席项 {deleted} 条" : ""));
                ok.Code = "WMS_SYNC_SUCCESS";
                _logger.LogInformation("WMS 同步完成，status=success, pruneAfter={PruneAfter}", pruneAfter);
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "WMS 同步发生异常: {Message}, elapsed_ms={ElapsedMs}, last_status={LastStatus}, last_reason={LastReason}",
                    ex.Message,
                    wmsWatch.ElapsedMilliseconds,
                    lastHttpStatusCode,
                    lastHttpReason);
                return ErrorWithCode("WMS_SYNC_EXCEPTION", "WMS 同步失败：" + ex.Message);
            }
        }

        private WebResponseContent ErrorWithCode(string code, string message)
        {
            var response = WebResponseContent.Instance.Error(message);
            response.Code = code;
            return response;
        }

        private static string? ExtractToken(string tokenResp)
        {
            try
            {
                var node = JsonNode.Parse(tokenResp);
                if (node is null) return null;

                if (node["token"] is JsonNode t1) return t1.ToString().Trim('"');
                if (node["Token"] is JsonNode t2) return t2.ToString().Trim('"');

                foreach (var kv in node.AsObject())
                {
                    if (string.Equals(kv.Key, "token", StringComparison.OrdinalIgnoreCase))
                        return kv.Value?.ToString().Trim('"');
                }
            }
            catch { /* ignore */ }
            return null;
        }

        private static string BuildReportBody(string? reportId, string? wh, string? token, string? apiKey, int skip, int pageSize)
        {
            return $$"""
            {
              "reportid": "{{reportId}}",
              "Search": {
                "QueryModel": {
                  "Items": [
                    { "Field": "WH_ID", "Method": "Contains", "Value": "{{wh}}" }
                  ]
                },
                "PageInfo": {
                  "SkipCount": "{{skip}}",
                  "PageSize": "{{pageSize}}",
                  "IsPaging": true,
                  "IsGetTotalCount": true,
                  "TotalCount": "否"
                }
              },
              "token": "{{token}}",
              "apiKey": "{{apiKey}}"
            }
            """;
        }

        private static string SafeTrim(string? text, int maxLength = 2000)
        {
            if (string.IsNullOrEmpty(text)) return "<空响应>";
            return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;
        }

        private static string SafeSnippet(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "<空响应>";
            return text.Length > 500 ? text.Substring(0, 500) + "..." : text;
        }
    }
}
