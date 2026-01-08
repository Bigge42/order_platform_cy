using System;
using System.Collections.Generic;

namespace HDPro.CY.Order.Models.WZProductionOutputDtos
{
    public sealed class CapacityScheduleRequestDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProductionLine { get; set; }
        public bool AllowSplit { get; set; } = true;
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
}
