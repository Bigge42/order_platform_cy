using System;

namespace HDPro.Entity.DomainModels.MaterialCallBoard.dto
{
    /// <summary>
    /// MaterialCallBoard 批量写入数据传输对象
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
