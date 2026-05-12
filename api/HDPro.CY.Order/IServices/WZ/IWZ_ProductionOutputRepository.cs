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
        /// 增量刷新：按时间范围从 ESB 获取新增产量，按日期/阀体/产线累加到现有缓存
        /// </summary>
        /// <returns>参与增量累加的聚合键数量</returns>
        Task<int> RefreshIncrementalAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

        /// <summary>
        /// 查询：按阀体、产线、时间范围获取每日产量
        /// </summary>
        Task<List<WZ_ProductionOutput>> GetAsync(
            string valveCategory,
            string productionLine,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);

        /// <summary>
        /// 批量更新阈值：按阀体与产线持久化阈值，并同步回当前产量缓存
        /// </summary>
        Task<int> UpdateThresholdsAsync(
            IReadOnlyCollection<(string ValveCategory, string ProductionLine, decimal Threshold)> thresholds,
            CancellationToken ct = default);

        /// <summary>
        /// 查询：合并实际产量与预排产汇总数据
        /// </summary>
        Task<List<WZ_ProductionOutput>> GetWithPreProductionAsync(
            string valveCategory,
            string productionLine,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);

        /// <summary>
        /// 查询：合并实际产量与排产优化汇总数据
        /// </summary>
        Task<List<WZ_ProductionOutput>> GetWithOptimizedPreProductionAsync(
            string valveCategory,
            string productionLine,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);
    }
}
