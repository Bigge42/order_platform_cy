using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IServices;
using Microsoft.AspNetCore.Authorization;

namespace HDPro.CY.Order.Controllers
{
    public partial class ORDER_NOTE_FLATController
    {
        /// <summary>
        /// 1) 增量同步（ENTRY: 仅新增；CHANGE: 仅更新）
        /// </summary>
        [HttpPost("syncErpNotes")]
        [AllowAnonymous]
        public async Task<WebResponseContent> SyncErpNotes([FromServices] IORDER_NOTE_FLATService service)
            => await service.SyncErpNotes();

        /// <summary>
        /// 2) 按 source_entry_id 获取 remark_raw、internal_note
        /// </summary>
        [HttpGet("{sourceEntryId:long}/note")]
        [AllowAnonymous]
        public WebResponseContent GetNoteById(long sourceEntryId, [FromServices] IORDER_NOTE_FLATService service)
            => service.GetNoteById(sourceEntryId);

        /// <summary>
        /// 3) 更新四个拆分字段
        /// </summary>
        [HttpPost("updateNoteDetails")]
        [AllowAnonymous]
        public WebResponseContent UpdateNoteDetails([FromBody] NoteDetailsUpdateModel model,
                                                    [FromServices] IORDER_NOTE_FLATService service)
        {
            if (model == null || model.source_entry_id <= 0)
                return new WebResponseContent().Error("参数错误：source_entry_id");

            return service.UpdateNoteDetails(model.source_entry_id,
                                             model.note_body_actuator,
                                             model.note_accessory_debug,
                                             model.note_pressure_leak,
                                             model.note_packing);
        }

        public class NoteDetailsUpdateModel
        {
            public long source_entry_id { get; set; }
            public string note_body_actuator { get; set; }
            public string note_accessory_debug { get; set; }
            public string note_pressure_leak { get; set; }
            public string note_packing { get; set; }
        }
    }
}
