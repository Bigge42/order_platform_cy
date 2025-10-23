using System;

namespace HDPro.CY.Order.Models.MaterialCallBoardDtos
{
    /// <summary>
    /// 叫料看板批量导入/更新 DTO（字段可按你的表结构增减，不影响编译）
    /// </summary>
    public class MaterialCallBoardBatchDto
    {
        public string WorkOrderNo { get; set; }      // 工单号
        public string PlanTrackNo { get; set; }      // 计划跟踪号
        public string ProductCode { get; set; }      // 产品编号
        public string CallerName { get; set; }       // 叫料人
        public DateTime CalledAt { get; set; }       // 叫料时间
    }
}
