using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IRepositories;

namespace HDPro.CY.Order.Services
{
    public partial class ORDER_NOTE_FLATService
    {
        private const string DefaultErpBaseUrl = "http://10.11.0.101:8003";
        private const string SourceTypeEntry = "ENTRY";
        private const string SourceTypeChange = "CHANGE";

        private readonly IORDER_NOTE_FLATRepository _repository; // 访问数据库仓储
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _erpBaseUrl;
        private readonly string _erpEntryPath;
        private readonly string _erpChangePath;
        private readonly TimeSpan _erpTimeout;

        [ActivatorUtilitiesConstructor]
        public ORDER_NOTE_FLATService(
            IORDER_NOTE_FLATRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory = null)
            : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _httpClientFactory = httpClientFactory;

            var erpSection = configuration?.GetSection("ErpApis");
            _erpBaseUrl = erpSection?["Base"];
            _erpEntryPath = erpSection?["EntryPath"] ?? "/gateway/SalEntryBZToDDPT";
            _erpChangePath = erpSection?["ChangePath"] ?? "/gateway/SalChangeBZToDDPT";

            var timeoutSeconds = erpSection?.GetValue<int?>("TimeoutSeconds");
            if (timeoutSeconds.HasValue && timeoutSeconds.Value > 0)
            {
                _erpTimeout = TimeSpan.FromSeconds(timeoutSeconds.Value);
            }
            else
            {
                _erpTimeout = TimeSpan.FromSeconds(30);
            }
            // 多租户场景可在此初始化，普通场景不需要
            // base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加ORDER_NOTE_FLAT特有的初始化代码
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// </summary>
        protected override WebResponseContent ValidateCYOrderEntity(ORDER_NOTE_FLAT entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            // 在此处添加ORDER_NOTE_FLAT特有的数据验证逻辑
            return response;
        }

        /// <summary>
        /// 从 ERP 系统同步销售订单备注数据
        /// </summary>
        public async Task<WebResponseContent> SyncErpNotes()
        {
            var response = new WebResponseContent();
            try
            {
                var entryTask = FetchErpNotesAsync(_erpEntryPath, "ENTRY接口");
                var changeTask = FetchErpNotesAsync(_erpChangePath, "CHANGE接口");

                await Task.WhenAll(entryTask, changeTask);

                var entryList = entryTask.Result;
                var changeList = changeTask.Result;
                // 如果两个接口都没有返回数据
                if ((entryList == null || entryList.Count == 0) && (changeList == null || changeList.Count == 0))
                {
                    return response.Error("ERP接口未返回数据");
                }
                var now = DateTime.Now;
                var pendingNotes = new Dictionary<(string SourceType, long EntryId), ORDER_NOTE_FLAT>();

                void CollectNotes(IEnumerable<ErpNoteDto> source, string sourceType)
                {
                    if (source == null)
                    {
                        return;
                    }

                    foreach (var note in source)
                    {
                        if (note == null)
                        {
                            continue;
                        }

                        var key = (sourceType, note.FENTRYID);
                        pendingNotes[key] = new ORDER_NOTE_FLAT
                        {
                            source_type = sourceType,
                            source_entry_id = note.FENTRYID,
                            contract_no = note.F_BLN_CONTACTNONAME,
                            product_model = note.F_BLN_CPXH,
                            specification = note.FSPECIFICATION,
                            mto_no = note.FMTONO,
                            internal_note = note.F_BLN_BZ1,
                            remark_raw = note.F_BLN_BZ,
                            selection_guid = note.F_BLN_FGUID,
                            note_body_actuator = null,
                            note_accessory_debug = null,
                            note_pressure_leak = null,
                            note_packing = null,
                            created_at = now,
                            updated_at = now,
                            bz_changed = true
                        };
                    }
                }

                CollectNotes(entryList, SourceTypeEntry);
                CollectNotes(changeList, SourceTypeChange);

                var entities = pendingNotes.Values.ToList();
                if (entities.Count == 0)
                {
                    return response.Error("ERP接口未返回有效数据");
                }

                var (inserted, updated) = await _repository.UpsertNotesAsync(entities);

                return response.OK($"同步完成: 新增{inserted}条, 更新{updated}条");
            }
            catch (InvalidOperationException ex)
            {
                return response.Error(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return response.Error($"调用ERP接口失败：{ex.Message}");
            }
            catch (TaskCanceledException)
            {
                return response.Error("调用ERP接口超时，请稍后重试。");
            }
            catch (Exception ex)
            {
                // 捕获异常并返回错误信息
                return response.Error($"同步失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新拆分后的备注字段，并将变更标记重置为 0
        /// </summary>
        public WebResponseContent UpdateNoteSplit(string sourceType, long sourceEntryId,
                                                  string note_body_actuator,
                                                  string note_accessory_debug,
                                                  string note_pressure_leak,
                                                  string note_packing)
        {
            var response = new WebResponseContent();
            // 查找对应的记录
            var entity = _repository.Find(x => x.source_type == sourceType && x.source_entry_id == sourceEntryId).FirstOrDefault();
            if (entity == null)
            {
                return response.Error("未找到对应的备注记录");
            }
            // 更新指定的备注拆分类别字段，将 bz_changed 重置为 0（false）
            entity.note_body_actuator = note_body_actuator;
            entity.note_accessory_debug = note_accessory_debug;
            entity.note_pressure_leak = note_pressure_leak;
            entity.note_packing = note_packing;
            entity.bz_changed = false;
            entity.updated_at = DateTime.Now;
            _repository.SaveChanges();
            return response.OK("备注拆分更新成功");
        }

        /// <summary>
        /// 获取内部备注和原始备注字段
        /// </summary>
        public WebResponseContent GetNote(string sourceType, long sourceEntryId)
        {
            var response = new WebResponseContent();
            var entity = _repository.Find(x => x.source_type == sourceType && x.source_entry_id == sourceEntryId).FirstOrDefault();
            if (entity == null)
            {
                return response.Error("记录不存在");
            }
            // 仅返回 internal_note 和 remark_raw 两个字段
            var data = new
            {
                internal_note = entity.internal_note,
                remark_raw = entity.remark_raw
            };
            return response.OK("获取成功", data);
        }

        // 定义 ERP 接口返回数据的字段结构
        private class ErpNoteDto
        {
            public long FENTRYID { get; set; }
            public string F_BLN_BZ { get; set; }
            public string F_BLN_BZ1 { get; set; }
            public string F_BLN_CONTACTNONAME { get; set; }
            public string F_BLN_CPXH { get; set; }
            public string FSPECIFICATION { get; set; }
            public string FMTONO { get; set; }
            public string F_BLN_FGUID { get; set; }
        }

        private async Task<List<ErpNoteDto>> FetchErpNotesAsync(string path, string description)
        {
            var json = await GetErpJsonAsync(path, description);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<ErpNoteDto>();
            }

            try
            {
                var token = JsonConvert.DeserializeObject(json);
                if (token is JArray array)
                {
                    return array.ToObject<List<ErpNoteDto>>() ?? new List<ErpNoteDto>();
                }

                if (token is JObject obj && obj.TryGetValue("data", out var dataToken) && dataToken is JArray dataArray)
                {
                    return dataArray.ToObject<List<ErpNoteDto>>() ?? new List<ErpNoteDto>();
                }

                return JsonConvert.DeserializeObject<List<ErpNoteDto>>(json) ?? new List<ErpNoteDto>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"解析ERP接口({description})返回的数据失败：{ex.Message}");
            }
        }

        private async Task<string> GetErpJsonAsync(string path, string description)
        {
            var requestUri = BuildErpRequestUri(path);
            var client = _httpClientFactory?.CreateClient(nameof(ORDER_NOTE_FLATService));
            var shouldDispose = client == null;
            if (client == null)
            {
                client = new HttpClient();
            }

            if (_erpTimeout > TimeSpan.Zero)
            {
                client.Timeout = _erpTimeout;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"接口({description})返回异常，状态码 {(int)response.StatusCode} {response.ReasonPhrase}，响应内容：{content}");
                }

                return await response.Content.ReadAsStringAsync();
            }
            finally
            {
                if (shouldDispose)
                {
                    client.Dispose();
                }
            }
        }

        private Uri BuildErpRequestUri(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && Uri.TryCreate(path, UriKind.Absolute, out var absolute))
            {
                return absolute;
            }

            var baseUrl = string.IsNullOrWhiteSpace(_erpBaseUrl) ? DefaultErpBaseUrl : _erpBaseUrl;
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                throw new InvalidOperationException($"ERP接口基础地址配置错误：{baseUrl}");
            }

            var relativePath = (path ?? string.Empty).Trim();
            if (relativePath.Length > 0)
            {
                relativePath = relativePath.TrimStart('/');
            }

            return new Uri(baseUri, relativePath);
        }
    }
}
