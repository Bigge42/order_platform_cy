using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IServices.WZ;
using HDPro.Entity.DomainModels.OrderCollaboration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HDPro.CY.Order.Services.WZ
{
    /// <summary>
    /// 循环仓库看板手动更新服务实现
    /// </summary>
    public class XhckkbService : IXhckkbService, IDependency
    {
        private readonly ServiceDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<XhckkbService> _logger;

        public XhckkbService(
            ServiceDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            ILogger<XhckkbService> logger)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 手动更新循环仓库看板数据：调用接口获取最新数据，清空原有表数据并插入新数据。
        /// </summary>
        public async Task<WebResponseContent> ManualUpdateXhckkbAsync()
        {
            var res = new WebResponseContent();
            _logger.LogInformation("开始手动更新循环仓库看板数据…");

            try
            {
                // ① ESB 地址（后续可改为配置）
                const string apiUrl = "http://10.11.0.101:8003/gateway/DataCenter/XHCKKBSearch";
                _logger.LogInformation("调用接口(POST): {Url}", apiUrl);

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(5);

                // 很多 ESB 要求有 JSON 请求体
                var httpResp = await client.PostAsync(apiUrl, new StringContent("{}", Encoding.UTF8, "application/json"));
                var respText = await httpResp.Content.ReadAsStringAsync();

                _logger.LogInformation("接口响应: {StatusCode}, 内容前200字: {Text}",
                    httpResp.StatusCode,
                    !string.IsNullOrEmpty(respText) && respText.Length > 200 ? respText.Substring(0, 200) : respText);

                if (!httpResp.IsSuccessStatusCode)
                {
                    _logger.LogError("接口调用失败，状态码: {Status}, 返回: {Body}", httpResp.StatusCode, respText);
                    return res.Error($"接口调用失败，状态码: {httpResp.StatusCode}");
                }

                // ② 解析到 DTO（兼容 data/数组/内嵌 JSON 字符串）
                List<XhckkbDto> dtos = TryParseDtos(respText);
                if (dtos == null || dtos.Count == 0)
                {
                    _logger.LogWarning("接口返回数据为空或不匹配。");
                    return res.Error("接口返回数据为空或格式不正确");
                }

                _logger.LogInformation("接口返回原始 {Count} 条 DTO。", dtos.Count);

                // ③ DTO → 实体 + 清洗（长度裁剪、控制字符清理）
                var mapped = dtos.Select(d => new XhckkbRecord
                {
                    FENTRYID = d.FENTRYID,  // 必填主键

                    FBILLNO = Crop(d.FBILLNO, 50),
                    FDOCUMENTSTATUSNAME = Crop(d.FDOCUMENTSTATUSNAME, 50),
                    F_ORA_LB = Crop(d.F_ORA_LB, 50),

                    FMATERIALID = d.FMATERIALID,
                    F_ORA_INTEGER = d.F_ORA_INTEGER,
                    FSeq = d.FSeq,
                    F_ORA_INTEGER1 = d.F_ORA_INTEGER1,
                    F_ORA_INTEGER2 = d.F_ORA_INTEGER2,

                    F_ORA_WZXL = Crop(d.F_ORA_WZXL, 50),
                    F_ORA_TEXT1 = Crop(d.F_ORA_TEXT1, 50),
                    F_ORA_JJSP = Crop(d.F_ORA_JJSP, 50),
                    F_ORA_TEXT3 = Crop(d.F_ORA_TEXT3, 50),
                    F_ORA_TEXT4 = Crop(d.F_ORA_TEXT4, 50),
                    F_ORA_TEXT5 = Crop(RemoveCtlChars(d.F_ORA_TEXT5), 50),
                    F_ORA_TEXT6 = Crop(d.F_ORA_TEXT6, 50),
                    F_ORA_TEXT7 = Crop(d.F_ORA_TEXT7, 50),
                    F_ORA_TEXT8 = Crop(d.F_ORA_TEXT8, 50),
                    F_ORA_TEXT9 = Crop(d.F_ORA_TEXT9, 50),
                }).ToList();

                // 过滤非法主键（NULL 不会出现，但 0/负数要过滤）
                var invalidCount = mapped.Count(x => x.FENTRYID <= 0);
                if (invalidCount > 0)
                {
                    var sample = string.Join(" | ", mapped.Where(x => x.FENTRYID <= 0).Take(5)
                        .Select(s => $"FENTRYID={s.FENTRYID}, FBILLNO={s.FBILLNO}"));
                    _logger.LogWarning("检测到 {Cnt} 条 FENTRYID<=0 的记录，将被跳过。示例：{Sample}", invalidCount, sample);
                }
                mapped = mapped.Where(x => x.FENTRYID > 0).ToList();

                // 主键去重（保留最后一条）
                var beforeDedup = mapped.Count;
                var deduped = mapped
                    .GroupBy(x => x.FENTRYID)
                    .Select(g => g.Last())
                    .ToList();
                var dupCount = beforeDedup - deduped.Count;
                if (dupCount > 0)
                {
                    _logger.LogWarning("发现并移除重复主键 {DupCount} 个。", dupCount);
                }

                if (deduped.Count == 0)
                {
                    return res.Error("清洗后无有效数据（可能 FENTRYID 全部无效）。");
                }

                _logger.LogInformation("样本记录：{Sample}",
                    string.Join(" || ",
                        deduped.Take(3)
                            .Select(s => $"FENTRYID={s.FENTRYID}, FBILLNO={s.FBILLNO}, WZXL={s.F_ORA_WZXL}")));

                // ④ 事务：清空 → 分批插入
                await using var tran = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation("清空表 [dbo].[XhckkbRecord] …");
                    await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[XhckkbRecord]");

                    const int batchSize = 1000; // 可根据数据量调整
                    for (int i = 0; i < deduped.Count; i += batchSize)
                    {
                        var chunk = deduped.Skip(i).Take(batchSize).ToList();
                        _dbContext.Set<XhckkbRecord>().AddRange(chunk);
                        await _dbContext.SaveChangesAsync();
                        _logger.LogInformation("已写入 {Written}/{Total} 条…", Math.Min(i + batchSize, deduped.Count), deduped.Count);
                    }

                    await tran.CommitAsync();
                    _logger.LogInformation("写入完成，共插入 {Count} 条。", deduped.Count);
                    return res.OK($"更新成功，插入 {deduped.Count} 条数据（去重 {dupCount}）");
                }
                catch (DbUpdateException ex)
                {
                    await tran.RollbackAsync();
                    var baseMsg = ex.GetBaseException()?.Message ?? ex.Message;

                    // 打印触发失败的实体样本
                    try
                    {
                        var errSnap = string.Join(" || ", ex.Entries
                            .Take(3)
                            .Select(e =>
                            {
                                if (e.Entity is XhckkbRecord r)
                                    return $"FENTRYID={r.FENTRYID}, FBILLNO={r.FBILLNO}";
                                return e.Entity?.GetType().Name ?? "Unknown";
                            }));
                        _logger.LogError(ex, "写库失败（DbUpdateException）。Base: {Base}. 失败样本: {Snap}", baseMsg, errSnap);
                    }
                    catch { /* ignore */ }

                    return res.Error($"数据更新失败：{ClassifyDbError(baseMsg)}（{baseMsg}）");
                }
                catch (Exception ex)
                {
                    await tran.RollbackAsync();
                    _logger.LogError(ex, "写库失败（一般异常）。");
                    return res.Error($"数据更新失败：{ex.GetBaseException()?.Message ?? ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用循环仓库看板接口过程中出现异常。");
                return res.Error($"接口调用异常：{ex.GetBaseException()?.Message ?? ex.Message}");
            }
        }

        #region 辅助方法

        // 解析 DTO，兼容 data / Data / result / Result / 直接数组 / data 为字符串 的情况
        private static List<XhckkbDto> TryParseDtos(string respText)
        {
            try
            {
                var trimmed = (respText ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) return null;

                var root = JToken.Parse(trimmed);

                JToken dataToken = null;
                if (root.Type == JTokenType.Array)
                {
                    dataToken = root;
                }
                else
                {
                    dataToken = root["data"] ?? root["Data"] ?? root["result"] ?? root["Result"];
                    if (dataToken == null && root is JObject obj)
                    {
                        var firstArrayProp = obj.Properties().FirstOrDefault(p => p.Value?.Type == JTokenType.Array);
                        dataToken = firstArrayProp?.Value;
                    }
                }

                if (dataToken == null)
                {
                    // 直接按数组解析（有些返回就是数组）
                    return JsonConvert.DeserializeObject<List<XhckkbDto>>(trimmed);
                }
                else if (dataToken.Type == JTokenType.String)
                {
                    return JsonConvert.DeserializeObject<List<XhckkbDto>>(dataToken.Value<string>());
                }
                else if (dataToken.Type == JTokenType.Array)
                {
                    return dataToken.ToObject<List<XhckkbDto>>();
                }

                // 兜底
                return JsonConvert.DeserializeObject<List<XhckkbDto>>(dataToken.ToString());
            }
            catch
            {
                return null;
            }
        }

        // 裁剪到 NVARCHAR(50)
        private static string Crop(string s, int maxLen)
            => string.IsNullOrEmpty(s) ? s : (s.Length <= maxLen ? s : s.Substring(0, maxLen));

        // 去掉不可见控制字符（避免入库异常）
        private static string RemoveCtlChars(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var arr = s.Where(ch => !char.IsControl(ch) || ch == '\r' || ch == '\n' || ch == '\t').ToArray();
            return new string(arr);
        }

        // 归类常见底层异常（便于前端提示）
        private static string ClassifyDbError(string baseMsg)
        {
            if (string.IsNullOrEmpty(baseMsg)) return "数据库错误";
            var m = baseMsg;

            if (m.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) &&
                (m.Contains("duplicate", StringComparison.OrdinalIgnoreCase) || m.Contains("重复", StringComparison.OrdinalIgnoreCase)))
                return "主键重复（可能接口返回了重复的 FENTRYID）";

            if (m.Contains("Cannot insert the value NULL", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("不能将值 NULL 插入列", StringComparison.OrdinalIgnoreCase))
                return "存在必填字段为 NULL（重点检查 FENTRYID 以及必填列）";

            if (m.Contains("String or binary data would be truncated", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("字符串或二进制数据将被截断", StringComparison.OrdinalIgnoreCase))
                return "存在字符串超长（已做 50 长度裁剪，如仍报错请检查数据库列长度与实体是否一致）";

            if (m.Contains("conversion failed", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("转换失败", StringComparison.OrdinalIgnoreCase))
                return "存在类型转换失败（例如将非数字写入 int 列）";

            return "数据库写入失败";
        }

        // 与接口字段严格匹配的 DTO（字段名大小写与下划线完全对齐）
        private class XhckkbDto
        {
            [JsonProperty("FENTRYID")] public int FENTRYID { get; set; }
            [JsonProperty("FBILLNO")] public string FBILLNO { get; set; }
            [JsonProperty("FDOCUMENTSTATUSNAME")] public string FDOCUMENTSTATUSNAME { get; set; }
            [JsonProperty("F_ORA_LB")] public string F_ORA_LB { get; set; }
            [JsonProperty("FMATERIALID")] public int? FMATERIALID { get; set; }
            [JsonProperty("F_ORA_INTEGER")] public int? F_ORA_INTEGER { get; set; }
            [JsonProperty("FSeq")] public int? FSeq { get; set; }
            [JsonProperty("F_ORA_INTEGER1")] public int? F_ORA_INTEGER1 { get; set; }
            [JsonProperty("F_ORA_INTEGER2")] public int? F_ORA_INTEGER2 { get; set; }
            [JsonProperty("F_ORA_WZXL")] public string F_ORA_WZXL { get; set; }
            [JsonProperty("F_ORA_TEXT1")] public string F_ORA_TEXT1 { get; set; }
            [JsonProperty("F_ORA_JJSP")] public string F_ORA_JJSP { get; set; }
            [JsonProperty("F_ORA_TEXT3")] public string F_ORA_TEXT3 { get; set; }
            [JsonProperty("F_ORA_TEXT4")] public string F_ORA_TEXT4 { get; set; }
            [JsonProperty("F_ORA_TEXT5")] public string F_ORA_TEXT5 { get; set; }
            [JsonProperty("F_ORA_TEXT6")] public string F_ORA_TEXT6 { get; set; }
            [JsonProperty("F_ORA_TEXT7")] public string F_ORA_TEXT7 { get; set; }
            [JsonProperty("F_ORA_TEXT8")] public string F_ORA_TEXT8 { get; set; }
            [JsonProperty("F_ORA_TEXT9")] public string F_ORA_TEXT9 { get; set; }
        }

        #endregion
    }
}
