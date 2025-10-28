using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.CY.Order.IServices.MaterialCallBoard
{
    public interface IMaterialCallWorkOrderSetService
    {
        /// <summary>全量快照写入并计算 ExistsInMCB</summary>
        Task<WebResponseContent> RefreshSnapshotAsync(List<MaterialCallWorkOrderCodeDto> list);

        /// <summary>按白名单删除叫料看板中“缺席”的记录</summary>
        Task<WebResponseContent> PruneAsync();
    }
}
