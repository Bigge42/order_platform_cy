using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using HDPro.Core.Utilities;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.Models;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.Repositories; // 引入扩展方法命名空间
using Newtonsoft.Json.Linq;   // ★ 新增：解析不规则/嵌套 JSON
using System.Text;            // ★ 新增：POST 空 JSON 的编码

namespace HDPro.CY.Order.Services
{
    public partial class ORDER_NOTE_FLATService
    {
        private readonly IORDER_NOTE_FLATRepository _repository;

        [ActivatorUtilitiesConstructor]
        public ORDER_NOTE_FLATService(IORDER_NOTE_FLATRepository dbRepository, IHttpContextAccessor httpContextAccessor)
            : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
        }

        protected override void InitCYOrderSpecific() => base.InitCYOrderSpecific();
        protected override WebResponseContent ValidateCYOrderEntity(ORDER_NOTE_FLAT entity)
            => base.ValidateCYOrderEntity(entity);

        /// <summary>
        /// 1) 增量同步
        /// ENTRY：FENTRYID 不存在才插；插入 bz_changed=0
        /// CHANGE：FENTRYID 存在才更；仅更新 internal_note & remark_raw，且 bz_changed=1
        /// </summary>
        public async Task<WebResponseContent> SyncErpNotes()
        {
            var resp = new WebResponseContent();
            try
            {
                const string urlEntry = "http://10.11.0.101:8003/gateway/SalEntryBZToDDPT";
                const string urlChange = "http://10.11.0.101:8003/gateway/SalChangeBZToDDPT";

                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };

                // ① 拉 ENTRY（先 GET，失败再 POST 兜底），并把可能的“嵌套数组”拉平
                var entryFetch = await FetchWithFallbackAsync(client, urlEntry);
                if (!entryFetch.Ok)
                    return resp.Error($"ENTRY接口失败：{entryFetch.Error}（url={urlEntry}，method={entryFetch.Method}，status={entryFetch.Status}）");

                var entryList = ParseListSafely<ErpEntryDto>(entryFetch.Body); // 兼容 [ ... ] 或 [ [ ... ] ]
                entryList ??= new List<ErpEntryDto>();

                // ② 拉 CHANGE（同样兜底）
                var changeFetch = await FetchWithFallbackAsync(client, urlChange);
                if (!changeFetch.Ok)
                    return resp.Error($"CHANGE接口失败：{changeFetch.Error}（url={urlChange}，method={changeFetch.Method}，status={changeFetch.Status}）");

                var changeList = ParseListSafely<ErpChangeDto>(changeFetch.Body);
                changeList ??= new List<ErpChangeDto>();

                int inserted = 0, updated = 0, skipped = 0;

                // ---------------- ENTRY：仅新增，bz_changed=0 ----------------
                if (entryList.Count > 0)
                {
                    var ids = entryList.Where(x => x.FENTRYID > 0).Select(x => x.FENTRYID);
                    var existSet = _repository.GetExistingIds(ids); // 手动仓储扩展方法

                    foreach (var it in entryList)
                    {
                        if (it.FENTRYID <= 0) { skipped++; continue; }
                        if (existSet.Contains(it.FENTRYID)) { skipped++; continue; }

                        if (_repository.TryInsertEntry(it)) inserted++;
                        else skipped++;
                    }
                }

                // ---------------- CHANGE：仅更新，bz_changed=1 ----------------
                if (changeList.Count > 0)
                {
                    foreach (var it in changeList)
                    {
                        if (it.FENTRYID <= 0) { skipped++; continue; }

                        if (_repository.TryUpdateChange(it)) updated++;
                        else skipped++; // 本地没有则跳过
                    }
                }

                _repository.SaveChanges();
                return resp.OK($"同步完成：新增 {inserted}，更新 {updated}，跳过 {skipped}");
            }
            catch (Exception ex)
            {
                return resp.Error($"同步失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 先 GET；非 2xx 再 POST 一个空 JSON 兜底。返回体里带 method/status，方便定位 404 来源。
        /// </summary>
        private static async Task<(bool Ok, string Body, string Error, int Status, string Method)> FetchWithFallbackAsync(HttpClient client, string url)
        {
            try
            {
                // 尝试 GET
                using (var r = await client.GetAsync(url))
                {
                    var body = await r.Content.ReadAsStringAsync();
                    if (r.IsSuccessStatusCode) return (true, body, null, (int)r.StatusCode, "GET");
                    // GET 非成功，再试 POST（有些网关只支持 POST）
                    using var content = new StringContent("{}", Encoding.UTF8, "application/json");
                    using var p = await client.PostAsync(url, content);
                    var pBody = await p.Content.ReadAsStringAsync();
                    if (p.IsSuccessStatusCode) return (true, pBody, null, (int)p.StatusCode, "POST");
                    return (false, pBody, $"GET {(int)r.StatusCode}/{r.ReasonPhrase}；POST {(int)p.StatusCode}/{p.ReasonPhrase}", (int)p.StatusCode, "POST");
                }
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message, 0, "GET");
            }
        }

        /// <summary>
        /// ERP 返回有时是 [ {..},{..} ]，也可能是 [ [ {..} ] ] 或包在对象里；这里尽量“拉平”成 List<T>
        /// </summary>
        private static List<T> ParseListSafely<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<T>();
            var token = JToken.Parse(json);

            // 最常见：数组
            if (token is JArray arr)
            {
                // 若是嵌套数组 [ [ {...}, ... ] ] -> 打平
                if (arr.Count > 0 && arr[0] is JArray)
                {
                    var flat = new List<T>();
                    foreach (var inner in arr.OfType<JArray>())
                    {
                        flat.AddRange(inner.ToObject<List<T>>() ?? new List<T>());
                    }
                    return flat;
                }
                return arr.ToObject<List<T>>() ?? new List<T>();
            }

            // 某些网关包一层对象 { data:[...] } / { rows:[...] }
            if (token is JObject obj)
            {
                foreach (var key in new[] { "data", "rows", "list", "result" })
                {
                    if (obj.TryGetValue(key, out var v) && v is JArray a)
                    {
                        return a.ToObject<List<T>>() ?? new List<T>();
                    }
                }
            }

            // 兜底：尝试按单对象解析
            var single = token.ToObject<T>();
            return single != null ? new List<T> { single } : new List<T>();
        }


        /// <summary>
        /// 2) 通过 source_entry_id 获取 remark_raw、internal_note
        /// </summary>
        public WebResponseContent GetNoteById(long sourceEntryId)
        {
            var resp = new WebResponseContent();
            var pair = _repository.GetNotePair(sourceEntryId);
            if (pair == null) return resp.Error("记录不存在");

            return resp.OK("获取成功", new
            {
                source_entry_id = sourceEntryId,
                remark_raw = pair.Value.RemarkRaw,
                internal_note = pair.Value.InternalNote
            });
        }

        /// <summary>
        /// 3) 更新四个拆分字段（不改 remark_raw/internal_note/bz_changed）
        /// </summary>
        public WebResponseContent UpdateNoteDetails(long sourceEntryId,
                                                    string note_body_actuator,
                                                    string note_accessory_debug,
                                                    string note_pressure_leak,
                                                    string note_packing)
        {
            var resp = new WebResponseContent();

            if (!_repository.UpdateNoteDetails(sourceEntryId,
                                               note_body_actuator,
                                               note_accessory_debug,
                                               note_pressure_leak,
                                               note_packing))
            {
                return resp.Error("未找到对应记录");
            }

            _repository.SaveChanges();
            return resp.OK("更新成功");
        }
    }
}
