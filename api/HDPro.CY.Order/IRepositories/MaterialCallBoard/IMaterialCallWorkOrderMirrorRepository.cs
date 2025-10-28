using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IRepositories.MaterialCallBoard
{
    public interface IMaterialCallWorkOrderSetRepository
    {
        /// <summary>
        /// 全量快照：用传入 codes 全量替换 MaterialCallWorkOrderSet，
        /// 并计算 ExistsInMCB（与 MaterialCallBoard.WorkOrderNo 的匹配情况）。
        /// 返回：total(白名单总数), matched(看板存在数)
        /// </summary>
        Task<(int total, int matched)> RefreshSnapshotAsync(List<string> codes);

        /// <summary>
        /// 删除 MaterialCallBoard 中“白名单不存在”的 WorkOrderNo。
        /// 返回：deleted（删除条数）
        /// </summary>
        Task<int> PruneMaterialCallBoardAsync();
    }
}
