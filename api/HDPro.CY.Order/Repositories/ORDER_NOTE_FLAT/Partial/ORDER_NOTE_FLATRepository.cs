using System;
using System.Linq;
using System.Collections.Generic;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.Models;

namespace HDPro.CY.Order.Repositories
{
    /// <summary>
    /// 手动仓储扩展：基于 IORDER_NOTE_FLATRepository 的扩展方法
    /// </summary>
    public static class ORDER_NOTE_FLATRepositoryManualExtensions
    {
        private static string Nz(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        /// <summary>
        /// 批量查现有 source_entry_id 集合
        /// </summary>
        public static HashSet<long> GetExistingIds(this IORDER_NOTE_FLATRepository repo, IEnumerable<long> ids)
        {
            if (ids == null) return new HashSet<long>();
            var set = ids.Where(x => x > 0).Distinct().ToList();
            if (set.Count == 0) return new HashSet<long>();
            return repo.Find(x => set.Contains(x.source_entry_id))
                       .Select(x => x.source_entry_id)
                       .ToHashSet();
        }

        /// <summary>
        /// 仅当不存在时插入（ENTRY 规则），返回是否插入成功
        /// </summary>
        public static bool TryInsertEntry(this IORDER_NOTE_FLATRepository repo, ErpEntryDto dto)
        {
            if (dto == null || dto.FENTRYID <= 0) return false;
            var exist = repo.Find(x => x.source_entry_id == dto.FENTRYID).FirstOrDefault();
            if (exist != null) return false;

            var entity = new ORDER_NOTE_FLAT
            {
                source_type = "ENTRY",
                source_entry_id = dto.FENTRYID,
                contract_no = Nz(dto.F_BLN_CONTACTNONAME),
                product_model = Nz(dto.F_BLN_CPXH),
                specification = Nz(dto.FSPECIFICATION),
                mto_no = Nz(dto.FMTONO),
                // 映射：备注->remark_raw，内部->internal_note
                remark_raw = Nz(dto.F_BLN_BZ1),
                internal_note = Nz(dto.F_BLN_BZ),
                selection_guid = Nz(dto.F_BLN_FGUID),
                note_body_actuator = null,
                note_accessory_debug = null,
                note_pressure_leak = null,
                note_packing = null,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                // 需求：ENTRY 新增时 bz_changed=0
                bz_changed = false
            };
            repo.Add(entity);
            return true;
        }

        /// <summary>
        /// 仅当已存在时更新（CHANGE 规则）：只更新 internal_note、remark_raw 并置 bz_changed=1
        /// </summary>
        public static bool TryUpdateChange(this IORDER_NOTE_FLATRepository repo, ErpChangeDto dto)
        {
            if (dto == null || dto.FENTRYID <= 0) return false;
            var entity = repo.Find(x => x.source_entry_id == dto.FENTRYID).FirstOrDefault();
            if (entity == null) return false;

            entity.remark_raw = Nz(dto.F_BLN_BZ1);
            entity.internal_note = Nz(dto.F_BLN_BZ);
            entity.bz_changed = true;
            entity.updated_at = DateTime.UtcNow;

            // ★ 关键：标记为已修改，确保 SaveChanges() 落库
            repo.Update(entity, true);
            return true;
        }


        /// <summary>
        /// 通过 source_entry_id 取简要备注对
        /// </summary>
        public static (string RemarkRaw, string InternalNote)? GetNotePair(this IORDER_NOTE_FLATRepository repo, long sourceEntryId)
        {
            var e = repo.Find(x => x.source_entry_id == sourceEntryId)
                        .Select(x => new { x.remark_raw, x.internal_note })
                        .FirstOrDefault();
            return e == null ? ((string, string)?)null : (e.remark_raw, e.internal_note);
        }

        /// <summary>
        /// 更新四段拆分备注字段 + 记录修改人；并将 bz_changed 置为 false
        /// </summary>
        public static bool UpdateNoteDetails(
            this IORDER_NOTE_FLATRepository repo,
            long sourceEntryId,
            string note_body_actuator,
            string note_accessory_debug,
            string note_pressure_leak,
            string note_packing,
            string modifiedBy)
        {
            var e = repo.Find(x => x.source_entry_id == sourceEntryId).FirstOrDefault();
            if (e == null) return false;

            e.note_body_actuator = note_body_actuator;
            e.note_accessory_debug = note_accessory_debug;
            e.note_pressure_leak = note_pressure_leak;
            e.note_packing = note_packing;

            // ★ 新增：修改人
            e.modified_by = Nz(modifiedBy);

            // ★ 新增：提交拆分后，业务要求 bz_changed = false
            e.bz_changed = false;

            e.updated_at = DateTime.UtcNow;

            // 关键：标记为已修改，确保 SaveChanges 生效
            repo.Update(e, true);
            return true;
        }

#if DEBUG
        /// <summary>
        /// 测试用：若不存在则种一条 ENTRY 基线（bz_changed=0），仅含最小字段
        /// </summary>
        public static bool EnsureExistsForTest(this IORDER_NOTE_FLATRepository repo, long sourceEntryId)
        {
            var exist = repo.Find(x => x.source_entry_id == sourceEntryId).FirstOrDefault();
            if (exist != null) return false;

            var entity = new ORDER_NOTE_FLAT
            {
                source_type = "ENTRY",
                source_entry_id = sourceEntryId,
                remark_raw = null,
                internal_note = null,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                bz_changed = false,
                note_body_actuator = null,
                note_accessory_debug = null,
                note_pressure_leak = null,
                note_packing = null
            };
            repo.Add(entity);
            return true;
        }

        /// <summary>
        /// 测试用：套用一笔 CHANGE 更新（只改 remark_raw/internal_note，并置 bz_changed=1）
        /// </summary>
        public static bool ApplyChangeForTest(this IORDER_NOTE_FLATRepository repo, long sourceEntryId, string remarkRaw, string internalNote)
        {
            var e = repo.Find(x => x.source_entry_id == sourceEntryId).FirstOrDefault();
            if (e == null) return false;

            e.remark_raw = Nz(remarkRaw);
            e.internal_note = Nz(internalNote);
            e.bz_changed = true;
            e.updated_at = DateTime.UtcNow;

            // ★ 关键：标记为已修改
            repo.Update(e, true);
            return true;
        }
#endif
    }

}
