using System;

namespace HDPro.CY.Order.Models.MaterialCallBoardDtos
{
    /// <summary>
    /// MaterialCallBoard 批量导入请求模型
    /// </summary>
    public class MaterialCallBoardBatchDto
    {
        /// <summary>
        /// 工单号，唯一标识叫料记录
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
        /// 叫料时间；为空时使用当前时间
        /// </summary>
        public DateTime? CalledAt { get; set; }
    }
}
