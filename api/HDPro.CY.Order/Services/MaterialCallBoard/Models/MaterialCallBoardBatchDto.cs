using System;

namespace HDPro.CY.Order.Services.MaterialCallBoardModels
{
    /// <summary>
    /// 批量写入 MaterialCallBoard 的请求项
    /// </summary>
    public class MaterialCallBoardBatchDto
    {
        /// <summary>
        /// 工单号
        /// </summary>
        public string WorkOrderNo { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string PlanTrackNo { get; set; }

        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// 叫料人
        /// </summary>
        public string CallerName { get; set; }

        /// <summary>
        /// 叫料时间
        /// </summary>
        public DateTime CalledAt { get; set; }
    }
}
