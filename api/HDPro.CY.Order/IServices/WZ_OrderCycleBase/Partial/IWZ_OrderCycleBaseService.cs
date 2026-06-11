/*
*所有关于WZ_OrderCycleBase类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace HDPro.CY.Order.IServices
{
    public partial interface IWZ_OrderCycleBaseService
    {
        Task<int> SyncFromOrderTrackingAsync(DateTime? approvedDateStart, DateTime? approvedDateEnd, CancellationToken cancellationToken = default);

        Task<ValveRuleBatchSummary> BatchCallValveRuleServiceAsync(
            CancellationToken cancellationToken = default,
            string progressTaskId = null);

        ValveRuleTaskProgress CreateValveRuleTaskProgress(string taskId);

        ValveRuleTaskProgress GetValveRuleTaskProgress(string taskId);

        ValveRuleTaskProgress MarkValveRuleTaskProgressFailed(string taskId, string message);

        ValveRuleTaskProgress CreateInitializeSchedulingTaskProgress(string taskId);

        ValveRuleTaskProgress GetInitializeSchedulingTaskProgress(string taskId);

        ValveRuleTaskProgress MarkInitializeSchedulingTaskProgressFailed(string taskId, string message);

        Task<int> FillValveCategoryByRuleAsync(int batchSize = 1000);

        Task<AssignedProductionLineBatchSummary> BatchAssignProductionLineByRuleAsync(int batchSize = 1000, CancellationToken cancellationToken = default);

        Task<int> SyncPreProductionOutputAsync(CancellationToken cancellationToken = default);

        Task<CapacityScheduleSummary> CalculateCapacityScheduleDateAsync(CancellationToken cancellationToken = default);

        Task<InitializeSchedulingSummary> InitializeSchedulingAsync(
            int batchSize = 1000,
            CancellationToken cancellationToken = default,
            string progressTaskId = null);

        Task<SchedulePredictionReceiveSummary> ReceiveSchedulePredictionReviewAsync(
            IReadOnlyCollection<SchedulePredictionReviewReceiveDto> items,
            CancellationToken cancellationToken = default);

        WebResponseContent ExportSchedulePredictionReview(PageDataOptions pageData);

        string GetAssignedProductionLineSql();
    }

    public sealed class ValveRuleBatchSummary
    {
        /// <summary>
        /// 本次提交到规则服务的总行数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 规则服务返回 success=true 的条数
        /// </summary>
        public int Succeeded { get; set; }

        /// <summary>
        /// 规则服务返回失败或未能匹配到实体的条数
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// 实际更新到数据库的条数
        /// </summary>
        public int Updated { get; set; }

        /// <summary>
        /// 本次分批数量，便于前端展示进度
        /// </summary>
        public int BatchCount { get; set; }

        /// <summary>
        /// 规则服务返回的日志文件路径集合
        /// </summary>
        public List<string> LogFiles { get; set; } = new List<string>();
    }

    public sealed class ValveRuleTaskProgress
    {
        public string TaskId { get; set; }

        public string Status { get; set; }

        public string Stage { get; set; }

        public string Message { get; set; }

        public int Total { get; set; }

        public int Processed { get; set; }

        public int Succeeded { get; set; }

        public int Failed { get; set; }

        public int Updated { get; set; }

        public int BatchCount { get; set; }

        public int TotalBatchCount { get; set; }

        public int Percent { get; set; }

        public List<string> LogFiles { get; set; } = new List<string>();

        public string Error { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? FinishedAt { get; set; }
    }

    public sealed class AssignedProductionLineBatchSummary
    {
        /// <summary>
        /// 本次处理的总行数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 实际更新的行数
        /// </summary>
        public int Updated { get; set; }

        /// <summary>
        /// 未命中规则或值未变化的行数
        /// </summary>
        public int Skipped { get; set; }

        /// <summary>
        /// 处理失败的行数
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// 失败的主键集合
        /// </summary>
        public List<int> FailedIds { get; set; } = new List<int>();

        /// <summary>
        /// 对照校验用 SQL
        /// </summary>
        public string SqlPreview { get; set; }
    }

    public sealed class CapacityScheduleSummary
    {
        /// <summary>
        /// 本次参与计算的订单总数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 成功回填排产优化日期的订单数
        /// </summary>
        public int Updated { get; set; }

        /// <summary>
        /// 跳过的订单数（缺少必要字段等）
        /// </summary>
        public int Skipped { get; set; }

        /// <summary>
        /// 计算失败的订单数
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// 未找到对应产线产量记录的订单数
        /// </summary>
        public int MissingProductionOutput { get; set; }

        /// <summary>
        /// 产量记录缺少阈值的订单数
        /// </summary>
        public int MissingThreshold { get; set; }

        /// <summary>
        /// 拆单排产的订单数
        /// </summary>
        public int SplitCount { get; set; }

        /// <summary>
        /// 未超日产能并直接确认排产的订单数
        /// </summary>
        public int NormalCapacityCount { get; set; }

        /// <summary>
        /// 货期可调整并重新落到可用产能日期的订单数
        /// </summary>
        public int DeliveryAdjustedCount { get; set; }

        /// <summary>
        /// 启用日产能 120% 预留产能的订单数
        /// </summary>
        public int DailyReserveCount { get; set; }

        /// <summary>
        /// 启用周六预留产能的订单数
        /// </summary>
        public int SaturdayReserveCount { get; set; }

        /// <summary>
        /// Sunday 120% reserve capacity count.
        /// </summary>
        public int SundayReserveCount { get; set; }

        /// <summary>
        /// 区间内全部超过 120% 后均匀摊排的订单数
        /// </summary>
        public int BalancedOverflowCount { get; set; }

        /// <summary>
        /// 排产优化日期最终负载超过120%产能阈值的订单数
        /// </summary>
        public int OverThresholdCount { get; set; }

        /// <summary>
        /// 按排产日期兜底补齐排产优化日期的订单数
        /// </summary>
        public int FallbackScheduleDateCount { get; set; }
    }

    public sealed class InitializeSchedulingSummary
    {
        public ValveRuleBatchSummary ValveRule { get; set; }

        public int ValveCategoryUpdated { get; set; }

        public AssignedProductionLineBatchSummary AssignedProductionLine { get; set; }

        public CapacityScheduleSummary CapacitySchedule { get; set; }

        public int PreProductionOutputSynced { get; set; }

        public int RemainingNonBjBlankCapacityScheduleDate { get; set; }

        public List<string> Warnings { get; set; } = new List<string>();
    }

    public sealed class SchedulePredictionReceiveSummary
    {
        public int Received { get; set; }

        public int Saved { get; set; }

        public int Skipped { get; set; }
    }

    public sealed class SchedulePredictionReviewReceiveDto
    {
        public long? PredictionResultId { get; set; }

        public int? OrderCycleBaseId { get; set; }

        public string InputFingerprint { get; set; }

        public string RequestBatchNo { get; set; }

        public string ProductName { get; set; }

        public string SpecModel { get; set; }

        public string ValveCategory { get; set; }

        public string NominalDiameter { get; set; }

        public string NominalPressure { get; set; }

        public string ProductionLine { get; set; }

        public int? FixedCycleDays { get; set; }

        public DateTime? PredictedScheduleDate { get; set; }

        public DateTime? StandardDeliveryDate { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public int? MatchedRuleCount { get; set; }

        public string UsedFieldsJson { get; set; }

        public string CandidateSuggestionsJson { get; set; }

        public string FailureReason { get; set; }

        public string FailureMessage { get; set; }
    }
}
