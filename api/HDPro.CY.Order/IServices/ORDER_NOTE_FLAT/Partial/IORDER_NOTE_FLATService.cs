using HDPro.Core.Utilities;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices
{
    public partial interface IORDER_NOTE_FLATService
    {
        Task<WebResponseContent> SyncErpNotes();
        WebResponseContent GetNoteById(long sourceEntryId);
        WebResponseContent UpdateNoteDetails(long sourceEntryId,
                                             string note_body_actuator,
                                             string note_accessory_debug,
                                             string note_pressure_leak,
                                             string note_packing);
    }
}
