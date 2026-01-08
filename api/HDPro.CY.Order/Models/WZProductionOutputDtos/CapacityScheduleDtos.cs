using System;
using System.Collections.Generic;

namespace HDPro.CY.Order.Models.WZProductionOutputDtos
{
    public sealed class CapacityScheduleRequestDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProductionLine { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<string> ProductionLines { get; set; } = new();
        public bool RecalcAll { get; set; }
        public bool AllowSplit { get; set; } = true;
        public bool DryRun { get; set; }
    }

    public sealed class CapacityScheduleResultDto
    {
        public int OrderId { get; set; }
        public string AssignedProductionLine { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public decimal? OrderQty { get; set; }
        public DateTime? CapacityScheduleDate { get; set; }
        public decimal? RemainingQty { get; set; }
        public List<CapacityScheduleStepDto> Steps { get; set; } = new();
    }

    public sealed class CapacityScheduleStepDto
    {
        public DateTime Date { get; set; }
        public decimal RemainingCapacity { get; set; }
        public decimal AppliedQty { get; set; }
        public decimal CurrentQuantity { get; set; }
        public decimal Threshold { get; set; }
    }

    public sealed class CapacityScheduleBatchResultDto
    {
        public Guid BatchId { get; set; }
        public int TotalOrders { get; set; }
        public int ScheduledOrders { get; set; }
        public int FailedOrders { get; set; }
        public List<FailedOrderDto> FailedOrdersDetail { get; set; } = new();
        public List<DailyLoadDto> DailyLoads { get; set; } = new();
        public TimeSpan Elapsed { get; set; }
    }

    public sealed class FailedOrderDto
    {
        public int Id { get; set; }
        public string ProductionLine { get; set; }
        public decimal? OrderQty { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string Reason { get; set; }
    }

    public sealed class DailyLoadDto
    {
        public string ProductionLine { get; set; }
        public DateTime ProductionDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Threshold { get; set; }
        public bool IsOverThreshold { get; set; }
    }
}
