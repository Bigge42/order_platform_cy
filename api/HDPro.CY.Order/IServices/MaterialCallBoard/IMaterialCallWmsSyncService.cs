using System.Threading.Tasks;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.IServices.MaterialCallBoard
{
    /// <summary>从 WMS 拉取 WorkOrderCode 并刷新白名单快照（可选连带清理叫料看板）</summary>
    public interface IMaterialCallWmsSyncService
    {
        /// <param name="pruneAfter">是否在刷新快照后，按白名单删除 MaterialCallBoard 中缺席的记录</param>
        Task<WebResponseContent> SyncSnapshotFromWmsAsync(bool pruneAfter = false);
    }
}
