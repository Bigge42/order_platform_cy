using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.CY.Order.IServices.MaterialCallBoard
{
    /// <summary>
    /// MaterialCallBoard 批量导入应用服务（独立命名，避免与代码生成器冲突）
    /// </summary>
    public interface IMaterialCallBatchService
    {
        Task<WebResponseContent> BatchUpsertAsync(List<MaterialCallBoardBatchDto> data);
    }
}
