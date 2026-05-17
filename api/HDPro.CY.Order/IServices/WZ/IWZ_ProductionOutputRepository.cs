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

        /// <summary>
        /// 查询同步健康状态：明细刷新时间、源数据新鲜度、未归属明细和汇总覆盖情况。
        /// </summary>
        Task<WZProductionOutputSyncHealthDto> GetSyncHealthAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken ct = default);

        /// <summary>
        /// 导出未归属或冲突明细：用于人工补充规则。
        /// </summary>
        Task<List<WZProductionOutputUnknownDetailDto>> GetUnknownDetailsAsync(
            DateTime startDate,
            DateTime endDate,
            int take = 100000,
            CancellationToken ct = default);

        /// <summary>
        /// 查询指定热力图格子的订单明细。
        /// </summary>
        Task<List<WZProductionOutputCellDetailDto>> GetCellDetailsAsync(
            DateTime productionDate,
            string valveCategory,
            string productionLine,
            int take = 10000,
            CancellationToken ct = default);

        /// <summary>
        /// 保存人工产线映射规则，后续同步/重新归类会优先使用。
        /// </summary>
        Task<int> SaveManualLineRulesAsync(
            IReadOnlyCollection<WZProductionOutputManualLineRuleDto> rules,
            CancellationToken ct = default);

        /// <summary>
        /// 重新计算现有未归属明细的产线：不重拉源数据，只回写指定生产日期范围内的未知/冲突明细。
        /// </summary>
        Task<WZProductionOutputRefreshResultDto> ReclassifyExistingDetailsAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);

        /// <summary>
        /// 预览：按 OCP_OrderTracking 的排产日期窗口生成 WZ 明细和汇总统计，不写入数据库。
        /// </summary>
        Task<WZProductionOutputRefreshResultDto> PreviewFromOrderTrackingAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);

        /// <summary>
        /// 刷新：按 OCP_OrderTracking 的排产日期窗口重建 WZ 明细，并重算产能汇总。
        /// </summary>
        Task<WZProductionOutputRefreshResultDto> RefreshFromOrderTrackingAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);
    }

    public sealed class WZProductionOutputRefreshResultDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public int RawRows { get; set; }
        public int DetailRows { get; set; }
        public int DistinctBills { get; set; }
        public int DistinctBillPlans { get; set; }
        public int SummarizableRows { get; set; }
        public int MissingLineRows { get; set; }
        public int ConflictRows { get; set; }
        public decimal DetailQuantity { get; set; }
        public int SummaryRows { get; set; }
        public decimal SummaryQuantity { get; set; }
        public List<WZProductionOutputStatusBucketDto> StatusBuckets { get; set; } = new();
        public List<WZProductionOutputUnresolvedSampleDto> UnresolvedSamples { get; set; } = new();
    }

    public sealed class WZProductionOutputStatusBucketDto
    {
        public string Status { get; set; } = string.Empty;
        public int Rows { get; set; }
        public decimal Quantity { get; set; }
    }

    public sealed class WZProductionOutputSyncHealthDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? LastDetailSyncTime { get; set; }
        public DateTime? LatestOcpEsbModifyDate { get; set; }
        public DateTime? LatestOcpModifyDate { get; set; }
        public int DetailRows { get; set; }
        public int SummarizableRows { get; set; }
        public int MissingLineRows { get; set; }
        public int ConflictRows { get; set; }
        public decimal DetailQuantity { get; set; }
        public int SummaryRows { get; set; }
        public decimal SummaryQuantity { get; set; }
        public int OcpRowsInProductionDateRange { get; set; }
        public int OcpRowsMissingProductionDate { get; set; }
        public List<WZProductionOutputHealthBucketDto> UnresolvedByBill { get; set; } = new();
        public List<WZProductionOutputHealthBucketDto> UnresolvedByDate { get; set; } = new();
        public List<WZProductionOutputUnresolvedSampleDto> UnresolvedSamples { get; set; } = new();
    }

    public sealed class WZProductionOutputHealthBucketDto
    {
        public string Key { get; set; } = string.Empty;
        public int Rows { get; set; }
        public decimal Quantity { get; set; }
    }

    public sealed class WZProductionOutputUnresolvedSampleDto
    {
        public DateTime ProductionDate { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public string PlanTrackingNo { get; set; } = string.Empty;
        public long? EntryId { get; set; }
        public string ValveCategory { get; set; } = string.Empty;
        public string ProductionLine { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string ClassifyStatus { get; set; } = string.Empty;
        public int RawRowCount { get; set; }
        public int LineCandidateCount { get; set; }
        public DateTime? LastSyncTime { get; set; }
    }

    public sealed class WZProductionOutputUnknownDetailDto
    {
        public DateTime ProductionDate { get; set; }
        public string BusinessKey { get; set; } = string.Empty;
        public string BillNo { get; set; } = string.Empty;
        public string PlanTrackingNo { get; set; } = string.Empty;
        public long? EntryId { get; set; }
        public int? Seq { get; set; }
        public string MaterialKey { get; set; } = string.Empty;
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialId { get; set; } = string.Empty;
        public string ValveCategory { get; set; } = string.Empty;
        public string ProductionLine { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string ClassifyStatus { get; set; } = string.Empty;
        public int RawRowCount { get; set; }
        public int LineCandidateCount { get; set; }
        public DateTime? LastSyncTime { get; set; }
    }

    public sealed class WZProductionOutputCellDetailDto
    {
        public DateTime ProductionDate { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public string PlanTrackingNo { get; set; } = string.Empty;
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialId { get; set; } = string.Empty;
        public string MaterialKey { get; set; } = string.Empty;
        public int? Seq { get; set; }
        public decimal Quantity { get; set; }
        public string ClassifyStatus { get; set; } = string.Empty;
    }

    public sealed class WZProductionOutputManualLineRuleDto
    {
        public string BillNo { get; set; } = string.Empty;
        public string PlanTrackingNo { get; set; } = string.Empty;
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialId { get; set; } = string.Empty;
        public string MaterialKey { get; set; } = string.Empty;
        public string ValveCategory { get; set; } = string.Empty;
        public string ProductionLine { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public bool Enable { get; set; } = true;
    }
}
