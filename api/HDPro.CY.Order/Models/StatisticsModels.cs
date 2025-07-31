/*
 * 消息统计相关的数据传输对象
 */
using System;
using System.Collections.Generic;

namespace HDPro.CY.Order.Models
{
    /// <summary>
    /// 消息统计数据DTO
    /// </summary>
    public class MessageStatisticsDto
    {
        /// <summary>
        /// 发送消息数
        /// </summary>
        public int SentCount { get; set; }

        /// <summary>
        /// 待回复消息数
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// 已超期消息数
        /// </summary>
        public int OverdueCount { get; set; }

        /// <summary>
        /// 已回复消息数
        /// </summary>
        public int RepliedCount { get; set; }

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatisticsTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 按业务类型统计消息数据DTO
    /// </summary>
    public class BusinessTypeStatisticsDto
    {
        /// <summary>
        /// 业务类型代码
        /// </summary>
        public string BusinessTypeCode { get; set; }

        /// <summary>
        /// 业务类型名称
        /// </summary>
        public string BusinessTypeName { get; set; }

        /// <summary>
        /// 发送消息数
        /// </summary>
        public int SentCount { get; set; }

        /// <summary>
        /// 待回复消息数
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// 已超期消息数
        /// </summary>
        public int OverdueCount { get; set; }

        /// <summary>
        /// 已回复消息数
        /// </summary>
        public int RepliedCount { get; set; }
    }

    /// <summary>
    /// 业务类型消息统计结果DTO
    /// </summary>
    public class BusinessTypeMessageStatisticsDto
    {
        /// <summary>
        /// 全部消息统计
        /// </summary>
        public BusinessTypeStatisticsDto AllMessages { get; set; }

        /// <summary>
        /// 各业务类型统计列表
        /// </summary>
        public List<BusinessTypeStatisticsDto> BusinessTypeList { get; set; }

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatisticsTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 汇总消息统计数据DTO
    /// </summary>
    public class SummaryStatisticsDto
    {
        /// <summary>
        /// 总的发送消息数
        /// </summary>
        public int TotalSentCount { get; set; }

        /// <summary>
        /// 总的待回复消息数
        /// </summary>
        public int TotalPendingCount { get; set; }

        /// <summary>
        /// 总的已超期消息数
        /// </summary>
        public int TotalOverdueCount { get; set; }

        /// <summary>
        /// 总的已回复消息数
        /// </summary>
        public int TotalRepliedCount { get; set; }

        /// <summary>
        /// 详细分类统计
        /// </summary>
        public StatisticsDetailsDto Details { get; set; }

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatisticsTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 统计详细分类数据DTO
    /// </summary>
    public class StatisticsDetailsDto
    {
        /// <summary>
        /// 催单统计数据
        /// </summary>
        public MessageStatisticsDto UrgentOrder { get; set; }

        /// <summary>
        /// 协商统计数据
        /// </summary>
        public MessageStatisticsDto Negotiation { get; set; }
    }
} 