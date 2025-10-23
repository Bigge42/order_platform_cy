/*
 *所有关于MaterialCallBoard类的业务代码接口应在此处编写
*/
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;

namespace HDPro.CY.Order.IServices.MaterialCallBoard
{
    public partial interface IMaterialCallBoardService
    {
        Task<WebResponseContent> BatchUpsertAsync(IEnumerable<MaterialCallBoardBatchDto> entries);
    }
}
