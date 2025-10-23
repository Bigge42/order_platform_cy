using System;

namespace HDPro.CY.Order.Models.MaterialCallBoardDtos
{
    /// <summary>
    /// MaterialCallBoard 批量导入 DTO
    /// </summary>
    public class MaterialCallBoardBatchDto
    {
        public string WorkOrderNo { get; set; }
        public string PlanTrackNo { get; set; }
        public string ProductCode { get; set; }
        public string CallerName { get; set; }
        public DateTime? CalledAt { get; set; } // 可空，后端默认 DateTime.Now
    }
}
