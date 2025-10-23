using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.CY.Order.IServices.MaterialCallBoard
{
    public interface IMaterialCallBatchService
    {
        Task<WebResponseContent> BatchUpsertAsync(List<MaterialCallBoardBatchDto> data);
    }
}
