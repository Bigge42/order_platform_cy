/*
 * SRM集成相关数据传输对象
 * 用于与SRM系统进行数据交互
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HDPro.CY.Order.Models
{
    /// <summary>
    /// SRM催单请求模型
    /// </summary>
    public class SRMOrderAskRequest
    {
        /// <summary>
        /// 发送SRM时间
        /// </summary>
        [Required]
        public string stime { get; set; }

        /// <summary>
        /// 发送人
        /// </summary>
        public string op { get; set; }

        /// <summary>
        /// 催单数据数组
        /// </summary>
        [Required]
        public List<SRMOrderAskData> data { get; set; }
    }

    /// <summary>
    /// SRM催单数据模型
    /// </summary>
    public class SRMOrderAskData
    {
        /// <summary>
        /// 催单ID
        /// </summary>
        [Required]
        public string aid { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [Required]
        public string order_no { get; set; }

        /// <summary>
        /// 订单行
        /// </summary>
        [Required]
        public int order_line { get; set; }

        /// <summary>
        /// 紧急程度
        /// </summary>
        public string lev { get; set; }

        /// <summary>
        /// 业务类型（确认交期/确认进度等）
        /// </summary>
        [Required]
        public string stype { get; set; }

        /// <summary>
        /// 第几次催单
        /// </summary>
        public string times { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string bz { get; set; }
    }

    /// <summary>
    /// SRM响应模型
    /// </summary>
    public class SRMResponse
    {
        /// <summary>
        /// 结果状态（1=成功，-1=失败）
        /// </summary>
        public int res { get; set; }

        /// <summary>
        /// 错误信息消息
        /// </summary>
        public string msg { get; set; }
    }

    /// <summary>
    /// 催单推送请求模型（用于API接口）
    /// </summary>
    public class PushOrderAskRequest
    {
        /// <summary>
        /// 发送人
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 催单数据列表
        /// </summary>
        [Required]
        public List<OrderAskData> OrderAskList { get; set; }
    }

    /// <summary>
    /// 催单数据模型（用于API接口）
    /// </summary>
    public class OrderAskData
    {
        /// <summary>
        /// 催单ID
        /// </summary>
        [Required]
        public string AskId { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [Required]
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单行
        /// </summary>
        [Required]
        public int OrderLine { get; set; }

        /// <summary>
        /// 紧急程度
        /// </summary>
        public string UrgencyLevel { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        [Required]
        public string ServiceType { get; set; }

        /// <summary>
        /// 第几次催单
        /// </summary>
        public string Times { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }

    /// <summary>
    /// SRM配置模型
    /// </summary>
    public class SRMConfig
    {
        /// <summary>
        /// SRM基础URL
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// 催单接口端点
        /// </summary>
        public string OrderAskEndpoint { get; set; }

        /// <summary>
        /// 应用ID
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Token密钥
        /// </summary>
        public string TokenKey { get; set; }

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int Timeout { get; set; } = 30;
    }
} 