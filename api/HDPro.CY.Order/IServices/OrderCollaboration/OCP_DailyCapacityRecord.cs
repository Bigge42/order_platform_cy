using System.Threading.Tasks;
using HDPro.Core.BaseProvider;
using HDPro.Core.Infrastructure;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_DailyCapacityRecordService : IService<OCP_DailyCapacityRecord>
    {
        /// <summary>
        /// 手动同步指定日期范围的产能数据
        /// </summary>
        Task<WebResponseContent> SyncCapacityData(string startDate, string endDate);

        /// <summary>
        /// 按筛选条件获取产能记录列表
        /// </summary>
        Task<List<OCP_DailyCapacityRecord>> GetRecords(int? year, string productionLine, string valveCategory);
    }
}
