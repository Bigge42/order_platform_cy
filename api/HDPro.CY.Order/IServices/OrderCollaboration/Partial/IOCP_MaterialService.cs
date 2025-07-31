/*
*所有关于OCP_Material类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_MaterialService
    {
        /// <summary>
        /// 从K3Cloud增量同步物料数据
        /// 基于本地物料表最大创建时间进行增量同步
        /// </summary>
        /// <param name="pageSize">每页数量，默认1000</param>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <returns>同步结果</returns>
        Task<WebResponseContent> SyncMaterialsFromK3CloudAsync(int pageSize = 1000, string customFilter = null);

        /// <summary>
        /// 从K3Cloud按时间范围同步物料数据
        /// 支持指定时间范围的物料数据同步
        /// </summary>
        /// <param name="startDate">开始日期 (yyyy-MM-dd)</param>
        /// <param name="endDate">结束日期 (yyyy-MM-dd)</param>
        /// <param name="pageSize">每页数量，默认1000</param>
        /// <returns>同步结果</returns>
        Task<WebResponseContent> SyncMaterialsFromK3CloudByDateRangeAsync(string startDate = null, string endDate = null, int pageSize = 1000);

        /// <summary>
        /// 获取K3Cloud物料数据（不保存到数据库）
        /// </summary>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <returns>物料数据</returns>
        Task<WebResponseContent> GetK3CloudMaterialsAsync(int pageIndex = 0, int pageSize = 1000, string filterString = null);

        /// <summary>
        /// 获取K3Cloud物料总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>物料总数</returns>
        Task<WebResponseContent> GetK3CloudMaterialCountAsync(string filterString = null);
    }
}
