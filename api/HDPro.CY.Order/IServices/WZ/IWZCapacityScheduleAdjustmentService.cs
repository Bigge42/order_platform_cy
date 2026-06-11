using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices.WZ
{
    /// <summary>
    /// 异常排产调整工作台服务。
    /// </summary>
    public interface IWZCapacityScheduleAdjustmentService
    {
        Task<CapacityScheduleAdjustmentListResultDto> QueryAbnormalOrdersAsync(
            CapacityScheduleAdjustmentQueryDto query,
            CancellationToken cancellationToken = default);

        Task<CapacityScheduleAdjustmentWindowDto> GetAdjustmentWindowAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<CapacityScheduleAdjustmentSaveResultDto> SaveAdjustmentAsync(
            CapacityScheduleAdjustmentSaveDto dto,
            CancellationToken cancellationToken = default);
    }

    public sealed class CapacityScheduleAdjustmentQueryDto
    {
        public int Page { get; set; } = 1;
        public int Rows { get; set; } = 30;
        public string Keyword { get; set; }
        public string AbnormalType { get; set; }
        public string ValveCategory { get; set; }
        public string ProductionLine { get; set; }
    }

    public sealed class CapacityScheduleAdjustmentListResultDto
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int Rows { get; set; }
        public int DeliveryWarningCount { get; set; }
        public int OverThresholdCount { get; set; }
        public int HolidayCount { get; set; }
        public int SundayRestCount { get; set; }
        public List<CapacityScheduleAdjustmentOrderDto> Items { get; set; } = new();
        public List<string> ValveCategories { get; set; } = new();
        public List<string> ProductionLines { get; set; } = new();
    }

    public sealed class CapacityScheduleAdjustmentOrderDto
    {
        public int Id { get; set; }
        public string SalesOrderNo { get; set; } = string.Empty;
        public string PlanTrackingNo { get; set; } = string.Empty;
        public string MaterialCode { get; set; } = string.Empty;
        public string SpecModel { get; set; } = string.Empty;
        public string ValveCategory { get; set; } = string.Empty;
        public string ProductionLine { get; set; } = string.Empty;
        public decimal OrderQty { get; set; }
        public DateTime? ReplyDeliveryDate { get; set; }
        public DateTime? StandardDeliveryDate { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public DateTime? CapacityScheduleDate { get; set; }
        public bool IsDeliveryWarning { get; set; }
        public bool IsOverThreshold { get; set; }
        public bool IsStatutoryHoliday { get; set; }
        public bool IsSundayRestDay { get; set; }
        public string AbnormalType { get; set; } = string.Empty;
        public string AbnormalText { get; set; } = string.Empty;
        public int AbnormalLevel { get; set; }
    }

    public sealed class CapacityScheduleAdjustmentWindowDto
    {
        public CapacityScheduleAdjustmentOrderDto Order { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? Threshold { get; set; }
        public List<CapacityScheduleAdjustmentDayDto> Days { get; set; } = new();
    }

    public sealed class CapacityScheduleAdjustmentDayDto
    {
        public DateTime Date { get; set; }
        public string WeekName { get; set; } = string.Empty;
        public string DayType { get; set; } = string.Empty;
        public bool IsWorkday { get; set; }
        public bool IsSaturdayRestDay { get; set; }
        public bool IsSundayRestDay { get; set; }
        public bool IsStatutoryHoliday { get; set; }
        public bool IsMakeupWorkday { get; set; }
        public bool IsCurrentDate { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal NormalOptimizedQuantity { get; set; }
        public decimal Quantity { get; set; }
        public decimal InsertQuantity { get; set; }
        public decimal ProjectedQuantity { get; set; }
        public decimal? Threshold { get; set; }
        public decimal? LoadRate { get; set; }
        public decimal? ProjectedLoadRate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public bool CanSelect { get; set; }
    }

    public sealed class CapacityScheduleAdjustmentSaveDto
    {
        public int Id { get; set; }
        public DateTime CapacityScheduleDate { get; set; }
    }

    public sealed class CapacityScheduleAdjustmentSaveResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? OldCapacityScheduleDate { get; set; }
        public DateTime NewCapacityScheduleDate { get; set; }
        public int OverThresholdCount { get; set; }
        public int SyncedPreProductionRows { get; set; }
        public CapacityScheduleAdjustmentWindowDto Window { get; set; }
    }
}
