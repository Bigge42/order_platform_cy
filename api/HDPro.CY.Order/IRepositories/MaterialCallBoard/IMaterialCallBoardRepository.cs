using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.CY.Order.IRepositories
{
    public interface IMaterialCallBoardRepository
    {
        /// <summary>批量 Upsert，返回 (inserted, updated)</summary>
        Task<(int inserted, int updated)> BulkUpsertAsync(List<MaterialCallBoardBatchDto> rows);
    }
}
