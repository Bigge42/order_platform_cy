using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices
{
    public partial interface IORDER_NOTE_FLATService
    {
        /// <summary>
        /// 从 ERP 同步销售订单备注数据
        /// </summary>
        Task<WebResponseContent> SyncErpNotes();

        /// <summary>
        /// 更新拆分后的备注字段（note_body_actuator 等），并重置变更标记
        /// </summary>
        WebResponseContent UpdateNoteSplit(string sourceType, long sourceEntryId,
                                           string note_body_actuator,
                                           string note_accessory_debug,
                                           string note_pressure_leak,
                                           string note_packing);

        /// <summary>
        /// 获取内部备注和原始备注
        /// </summary>
        WebResponseContent GetNote(string sourceType, long sourceEntryId);
    }
}
