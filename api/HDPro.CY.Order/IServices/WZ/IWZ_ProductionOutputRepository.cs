using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HDPro.Entity.DomainModels.OrderCollaboration;

namespace HDPro.CY.Order.IServices.WZ
{
    /// <summary>                 
    /// 产线产量（热力图数据）服务接口
    /// </summary>
    public partial interface IWZProductionOutputService
    {
        /// <summary>
        /// 手动刷新：按时间范围从 ESB 获取数据，清空表并重建聚合缓存
        /// </summary>
        /// <returns>插入的记录数</returns>
        Task<int> RefreshAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

        /// <summary>
        /// 查询：按阀体、产线、时间范围获取每日产量
        /// </summary>
        Task<List<WZ_ProductionOutput>> GetAsync(
            string valveCategory,
            string productionLine,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);
    }
}
