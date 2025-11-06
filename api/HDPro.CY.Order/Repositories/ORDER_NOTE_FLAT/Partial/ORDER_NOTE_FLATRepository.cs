using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Repositories
{
    public partial class ORDER_NOTE_FLATRepository
    {
        /// <summary>
        /// 使用手写SQL对备注数据进行批量插入或更新
        /// </summary>
        /// <param name="notes">待处理的备注集合</param>
        /// <returns>返回新增与更新数量</returns>
        public async Task<(int Inserted, int Updated)> UpsertNotesAsync(IEnumerable<ORDER_NOTE_FLAT> notes)
        {
            if (notes == null)
            {
                return (0, 0);
            }

            var noteList = notes
                .Where(n => n != null)
                .GroupBy(n => new { n.source_type, n.source_entry_id })
                .Select(g => g.Last())
                .ToList();

            if (noteList.Count == 0)
            {
                return (0, 0);
            }

            const string sql = @"
IF EXISTS (SELECT 1 FROM ORDER_NOTE_FLAT WHERE source_type = @source_type AND source_entry_id = @source_entry_id)
BEGIN
    UPDATE ORDER_NOTE_FLAT
    SET contract_no = @contract_no,
        product_model = @product_model,
        specification = @specification,
        mto_no = @mto_no,
        internal_note = @internal_note,
        remark_raw = @remark_raw,
        selection_guid = @selection_guid,
        updated_at = @updated_at,
        bz_changed = @bz_changed
    WHERE source_type = @source_type AND source_entry_id = @source_entry_id;

    SELECT 0;
END
ELSE
BEGIN
    INSERT INTO ORDER_NOTE_FLAT
    (source_type, source_entry_id, contract_no, product_model, specification, mto_no,
     internal_note, selection_guid, remark_raw, note_body_actuator, note_accessory_debug,
     note_pressure_leak, note_packing, created_at, updated_at, bz_changed)
    VALUES
    (@source_type, @source_entry_id, @contract_no, @product_model, @specification, @mto_no,
     @internal_note, @selection_guid, @remark_raw, @note_body_actuator, @note_accessory_debug,
     @note_pressure_leak, @note_packing, @created_at, @updated_at, @bz_changed);

    SELECT 1;
END";

            var inserted = 0;
            var updated = 0;

            var dapper = DapperContext.BeginTrans();
            try
            {
                foreach (var note in noteList)
                {
                    var resultObj = await dapper.ExecuteScalarAsync(sql, new
                    {
                        note.source_type,
                        note.source_entry_id,
                        note.contract_no,
                        note.product_model,
                        note.specification,
                        note.mto_no,
                        note.internal_note,
                        note.selection_guid,
                        note.remark_raw,
                        note.note_body_actuator,
                        note.note_accessory_debug,
                        note.note_pressure_leak,
                        note.note_packing,
                        note.created_at,
                        note.updated_at,
                        note.bz_changed
                    });

                    var result = Convert.ToInt32(resultObj ?? 0);
                    if (result > 0)
                    {
                        inserted++;
                    }
                    else
                    {
                        updated++;
                    }
                }

                dapper.Commit();
            }
            catch
            {
                dapper.Rollback();
                throw;
            }

            return (inserted, updated);
        }
    }
}
