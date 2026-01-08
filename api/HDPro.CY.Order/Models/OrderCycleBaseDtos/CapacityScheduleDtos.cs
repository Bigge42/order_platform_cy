using System;
using System.Collections.Generic;

namespace HDPro.CY.Order.Models.OrderCycleBaseDtos
{
    public class CapacityScheduleRequestDto
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public List<string> ProductionLines { get; set; }

        public bool RecalcAll { get; set; }

        public bool DryRun { get; set; }
    }

    public class CapacityScheduleResultDto
    {
        public Guid BatchId { get; set; }

        public int TotalOrders { get; set; }

        public int ScheduledOrders { get; set; }

        public int FailedOrders { get; set; }

        public List<FailedOrderDto> FailedOrdersDetail { get; set; } = new List<FailedOrderDto>();

        public List<DailyLoadDto> DailyLoads { get; set; } = new List<DailyLoadDto>();

        public TimeSpan Elapsed { get; set; }
    }

    public class FailedOrderDto
    {
        public int Id { get; set; }

        public string AssignedProductionLine { get; set; }

        public decimal? OrderQty { get; set; }

        public DateTime? ScheduleDate { get; set; }

        public string Reason { get; set; }
    }

    public class DailyLoadDto
    {
        public string ProductionLine { get; set; }

        public DateTime ProductionDate { get; set; }

        public decimal Quantity { get; set; }

        public decimal Threshold { get; set; }

        public bool IsOverThreshold { get; set; }
    }
}
