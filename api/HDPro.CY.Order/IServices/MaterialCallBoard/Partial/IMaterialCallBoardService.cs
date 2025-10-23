/*
*所有关于MaterialCallBoard类的业务代码接口应在此处编写
*/
using HDPro.Core.Utilities;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices.MaterialCallBoard
{
    public partial interface IMaterialCallBoardService
    {
        /// <summary>
        /// 批量新增或更新 MaterialCallBoard 数据
        /// </summary>
        /// <param name="payload">请求数据集合</param>
        /// <returns>处理结果</returns>
        Task<WebResponseContent> BatchUpsertAsync(IEnumerable<MaterialCallBoardBatchDto> payload);
    }
}
