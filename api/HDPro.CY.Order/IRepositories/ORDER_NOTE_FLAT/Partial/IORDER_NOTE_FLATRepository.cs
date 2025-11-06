using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.IRepositories
{
    public partial interface IORDER_NOTE_FLATRepository
    {
        /// <summary>
        /// 使用手写SQL批量插入或更新ERP同步的备注数据
        /// </summary>
        /// <param name="notes">需要处理的备注实体集合</param>
        /// <returns>返回新增与更新的记录数</returns>
        Task<(int Inserted, int Updated)> UpsertNotesAsync(IEnumerable<ORDER_NOTE_FLAT> notes);
    }
}
