using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IRepositories.MaterialCallBoard;
using HDPro.CY.Order.IServices.MaterialCallBoard;
using Microsoft.Extensions.Configuration;

namespace HDPro.CY.Order.Services.MaterialCallBoard
{
    public class MaterialCallWmsSyncService : IMaterialCallWmsSyncService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _cfg;
        private readonly IMaterialCallWorkOrderSetRepository _setRepo;

        public MaterialCallWmsSyncService(
            IHttpClientFactory httpFactory,
            IConfiguration cfg,
            IMaterialCallWorkOrderSetRepository setRepo)
        {
            _httpFactory = httpFactory;
            _cfg = cfg;
            _setRepo = setRepo;
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
                return WebResponseContent.Instance.Error("WMS 接口地址未配置");

            var http = _httpFactory.CreateClient(nameof(MaterialCallWmsSyncService));
            http.Timeout = TimeSpan.FromMinutes(5);

            try
            {
                // 1) 拿 token
                var tokenReq = $"{tokenUrl}?apikey={Uri.EscapeDataString(apiKey ?? string.Empty)}";
                var tokenResp = await http.GetStringAsync(tokenReq);
                var token = ExtractToken(tokenResp);
                if (string.IsNullOrWhiteSpace(token))
                    return WebResponseContent.Instance.Error("获取 WMS token 失败");

                // 2) 分页拉取
                var all = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int skip = 0, total = -1, page = 1;

                while (true)
                {
                    var body = BuildReportBody(reportId, warehouse, token, apiKey, skip, pageSize);
                    using var req = new StringContent(body, Encoding.UTF8, "application/json");
                    var resp = await http.PostAsync(reportUrl, req);
                    var text = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode)
                    {
                        var desc = string.IsNullOrWhiteSpace(resp.ReasonPhrase)
                            ? ((int)resp.StatusCode).ToString()
                            : $"{(int)resp.StatusCode} {resp.ReasonPhrase}";

                        var snippet = string.IsNullOrWhiteSpace(text)
                            ? "<空响应>"
                            : (text.Length > 500 ? text.Substring(0, 500) + "..." : text);

                        return WebResponseContent.Instance.Error(
                            $"WMS 查询失败：HTTP {desc}；响应：{snippet}");
                    }

                    using var doc = JsonDocument.Parse(text);
                    var root = doc.RootElement;

                    // Flag 校验
                    if (root.TryGetProperty("Flag", out var flag) && flag.ValueKind == JsonValueKind.False)
                    {
                        var msg = root.TryGetProperty("ErrorMsg", out var e) ? e.GetString() : "WMS 返回 Flag=false";
                        return WebResponseContent.Instance.Error($"WMS 查询失败：{msg}");
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

                // 3) 刷新白名单快照
                var codes = new List<string>(all);
                var (totalSet, matched) = await _setRepo.RefreshSnapshotAsync(codes);

                // 4) 可选：按白名单删除叫料看板中缺席项
                int deleted = 0;
                if (pruneAfter)
                    deleted = await _setRepo.PruneMaterialCallBoardAsync();

                return WebResponseContent.Instance.OK(
                    $"WMS 同步完成：白名单 {totalSet} 条（看板已存在 {matched} 条）" +
                    (pruneAfter ? $"，已删除缺席项 {deleted} 条" : ""));
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error("WMS 同步失败：" + ex.Message);
            }
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
    }
}
