using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using HDPro.Core.Utilities;
using HDPro.Core.Extensions;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IRepositories;

namespace HDPro.CY.Order.Services
{
    public partial class ORDER_NOTE_FLATService
    {
        private readonly IORDER_NOTE_FLATRepository _repository; // 访问数据库仓储

        [ActivatorUtilitiesConstructor]
        public ORDER_NOTE_FLATService(IORDER_NOTE_FLATRepository dbRepository, IHttpContextAccessor httpContextAccessor)
            : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
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
                string urlEntry = "http://10.11.0.101:8003/gateway/SalEntryBZToDDPT";
                string urlChange = "http://10.11.0.101:8003/gateway/SalChangeBZToDDPT";
                // 调用 ERP 接口获取 ENTRY 和 CHANGE 备注数据
                using HttpClient client = new HttpClient();
                string jsonEntry = await client.GetStringAsync(urlEntry);
                string jsonChange = await client.GetStringAsync(urlChange);
                var entryList = JsonConvert.DeserializeObject<List<ErpNoteDto>>(jsonEntry);
                var changeList = JsonConvert.DeserializeObject<List<ErpNoteDto>>(jsonChange);
                // 如果两个接口都没有返回数据
                if ((entryList == null || entryList.Count == 0) && (changeList == null || changeList.Count == 0))
                {
                    return response.Error("ERP接口未返回数据");
                }
                entryList ??= new List<ErpNoteDto>();
                changeList ??= new List<ErpNoteDto>();
                int inserted = 0;
                int updated = 0;
                // 处理 ENTRY 接口返回的数据
                foreach (var note in entryList)
                {
                    // 根据 source_type="ENTRY" 和 FENTRYID 查找现有记录
                    var entity = _repository.Find(x => x.source_type == "ENTRY" && x.source_entry_id == note.FENTRYID).FirstOrDefault();
                    if (entity != null)
                    {
                        // 已存在则更新内部备注和原始备注，bz_changed 设为 1（true）
                        entity.internal_note = note.F_BLN_BZ1;
                        entity.remark_raw = note.F_BLN_BZ;
                        entity.bz_changed = true;
                        entity.updated_at = DateTime.Now;
                        updated++;
                    }
                    else
                    {
                        // 不存在则插入新记录，bz_changed 默认设为 1（true）
                        var newEntity = new ORDER_NOTE_FLAT
                        {
                            source_type = "ENTRY",
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
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now,
                            bz_changed = true
                        };
                        _repository.Add(newEntity);
                        inserted++;
                    }
                }
                // 处理 CHANGE 接口返回的数据
                foreach (var note in changeList)
                {
                    var entity = _repository.Find(x => x.source_type == "CHANGE" && x.source_entry_id == note.FENTRYID).FirstOrDefault();
                    if (entity != null)
                    {
                        entity.internal_note = note.F_BLN_BZ1;
                        entity.remark_raw = note.F_BLN_BZ;
                        entity.bz_changed = true;
                        entity.updated_at = DateTime.Now;
                        updated++;
                    }
                    else
                    {
                        var newEntity = new ORDER_NOTE_FLAT
                        {
                            source_type = "CHANGE",
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
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now,
                            bz_changed = true
                        };
                        _repository.Add(newEntity);
                        inserted++;
                    }
                }
                // 将新增和修改的记录保存到数据库
                _repository.SaveChanges();
                return response.OK($"同步完成: 新增{inserted}条, 更新{updated}条");
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
    }
}
