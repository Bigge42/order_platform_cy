/*
 * 控制阀订单智能管理相关数据传输对象
 * 用于催单和协商业务的数据交互
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HDPro.CY.Order.Models
{
    /// <summary>
    /// 根据业务类型和业务主键查询请求模型
    /// </summary>
    public class BusinessKeyQueryRequest
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        [Required]
        public string BusinessType { get; set; }

        /// <summary>
        /// 业务主键
        /// </summary>
        [Required]
        public string BusinessKey { get; set; }
    }

    /// <summary>
    /// 催单信息DTO
    /// </summary>
    public class UrgentOrderDto
    {
        /// <summary>
        /// 催单ID
        /// </summary>
        public long UrgentOrderID { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// 业务主键
        /// </summary>
        public string BusinessKey { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNo { get; set; }

        /// <summary>
        /// 单据行号
        /// </summary>
        public string Seq { get; set; }

        /// <summary>
        /// 催单类型
        /// </summary>
        public int? UrgentType { get; set; }

        /// <summary>
        /// 催单内容
        /// </summary>
        public string UrgentContent { get; set; }

        /// <summary>
        /// 紧急程度
        /// </summary>
        public string UrgencyLevel { get; set; }

        /// <summary>
        /// 指定回复时间
        /// </summary>
        public decimal? AssignedReplyTime { get; set; }

        /// <summary>
        /// 时间单位
        /// </summary>
        public int? TimeUnit { get; set; }

        /// <summary>
        /// 催单状态
        /// </summary>
        public string UrgentStatus { get; set; }

        /// <summary>
        /// 默认负责人
        /// </summary>
        public string DefaultResPerson { get; set; }

        /// <summary>
        /// 默认负责人姓名
        /// </summary>
        public string DefaultResPersonName { get; set; }

        /// <summary>
        /// 指定负责人
        /// </summary>
        public string AssignedResPerson { get; set; }

        /// <summary>
        /// 指定负责人姓名
        /// </summary>
        public string AssignedResPersonName { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string PlanTraceNo { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 是否可回复（当前用户是指定负责人且没有回复记录时为true）
        /// </summary>
        public bool CanReply { get; set; }

        /// <summary>
        /// 催单回复列表
        /// </summary>
        public List<UrgentOrderReplyDto> Replies { get; set; } = new List<UrgentOrderReplyDto>();
    }

    /// <summary>
    /// 催单回复信息DTO
    /// </summary>
    public class UrgentOrderReplyDto
    {
        /// <summary>
        /// 回复ID
        /// </summary>
        public long ReplyID { get; set; }

        /// <summary>
        /// 催单ID
        /// </summary>
        public long UrgentOrderID { get; set; }

        /// <summary>
        /// 回复内容
        /// </summary>
        public string ReplyContent { get; set; }

        /// <summary>
        /// 回复人名称
        /// </summary>
        public string ReplyPersonName { get; set; }

        /// <summary>
        /// 回复人电话
        /// </summary>
        public string ReplyPersonPhone { get; set; }

        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime? ReplyTime { get; set; }

        /// <summary>
        /// 回复进度
        /// </summary>
        public string ReplyProgress { get; set; }

        /// <summary>
        /// 回复交期
        /// </summary>
        public DateTime? ReplyDeliveryDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Creator { get; set; }
    }

    /// <summary>
    /// 协商信息DTO
    /// </summary>
    public class NegotiationDto
    {
        /// <summary>
        /// 协商ID
        /// </summary>
        public long NegotiationID { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// 业务主键
        /// </summary>
        public string BusinessKey { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNo { get; set; }

        /// <summary>
        /// 单据行号
        /// </summary>
        public string Seq { get; set; }

        /// <summary>
        /// 协商类型
        /// </summary>
        public string NegotiationType { get; set; }

        /// <summary>
        /// 协商原因
        /// </summary>
        public string NegotiationReason { get; set; }

        /// <summary>
        /// 协商内容
        /// </summary>
        public string NegotiationContent { get; set; }

        /// <summary>
        /// 协商时间
        /// </summary>
        public DateTime? NegotiationDate { get; set; }

        /// <summary>
        /// 交货日期
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// 协商状态
        /// </summary>
        public string NegotiationStatus { get; set; }

        /// <summary>
        /// 默认负责人
        /// </summary>
        public string DefaultResPerson { get; set; }

        /// <summary>
        /// 默认负责人姓名
        /// </summary>
        public string DefaultResPersonName { get; set; }

        /// <summary>
        /// 指定负责人
        /// </summary>
        public string AssignedResPerson { get; set; }

        /// <summary>
        /// 指定负责人姓名
        /// </summary>
        public string AssignedResPersonName { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string PlanTraceNo { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 是否可回复（当前用户是指定负责人且没有回复记录时为true）
        /// </summary>
        public bool CanReply { get; set; }

        /// <summary>
        /// 协商回复列表
        /// </summary>
        public List<NegotiationReplyDto> Replies { get; set; } = new List<NegotiationReplyDto>();
    }

    /// <summary>
    /// 协商回复信息DTO
    /// </summary>
    public class NegotiationReplyDto
    {
        /// <summary>
        /// 回复ID
        /// </summary>
        public long ReplyID { get; set; }

        /// <summary>
        /// 协商ID
        /// </summary>
        public long NegotiationID { get; set; }

        /// <summary>
        /// 回复内容
        /// </summary>
        public string ReplyContent { get; set; }

        /// <summary>
        /// 回复人名称
        /// </summary>
        public string ReplyPersonName { get; set; }

        /// <summary>
        /// 回复人电话
        /// </summary>
        public string ReplyPersonPhone { get; set; }

        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime? ReplyTime { get; set; }

        /// <summary>
        /// 回复进度
        /// </summary>
        public string ReplyProgress { get; set; }

        /// <summary>
        /// 回复交期
        /// </summary>
        public DateTime? ReplyDeliveryDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Creator { get; set; }
    }

    /// <summary>
    /// 业务关联的催单和协商信息响应DTO
    /// </summary>
    public class BusinessOrderCollaborationDto
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// 业务主键
        /// </summary>
        public string BusinessKey { get; set; }

        /// <summary>
        /// 催单信息列表
        /// </summary>
        public List<UrgentOrderDto> UrgentOrders { get; set; } = new List<UrgentOrderDto>();

        /// <summary>
        /// 协商信息列表
        /// </summary>
        public List<NegotiationDto> Negotiations { get; set; } = new List<NegotiationDto>();

        /// <summary>
        /// 查询时间
        /// </summary>
        public DateTime QueryTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 数据汇总信息
        /// </summary>
        public BusinessCollaborationSummaryDto Summary { get; set; } = new BusinessCollaborationSummaryDto();
    }

    /// <summary>
    /// 业务协同汇总信息DTO
    /// </summary>
    public class BusinessCollaborationSummaryDto
    {
        /// <summary>
        /// 催单总数
        /// </summary>
        public int TotalUrgentOrders { get; set; }

        /// <summary>
        /// 催单已回复数
        /// </summary>
        public int RepliedUrgentOrders { get; set; }

        /// <summary>
        /// 催单待回复数
        /// </summary>
        public int PendingUrgentOrders { get; set; }

        /// <summary>
        /// 协商总数
        /// </summary>
        public int TotalNegotiations { get; set; }

        /// <summary>
        /// 协商已回复数
        /// </summary>
        public int RepliedNegotiations { get; set; }

        /// <summary>
        /// 协商待回复数
        /// </summary>
        public int PendingNegotiations { get; set; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        public DateTime? LastActivityTime { get; set; }
    }

    /// <summary>
    /// 根据业务类型和主键获取供应商负责人请求模型
    /// </summary>
    public class GetSupplierResponsibleRequest
    {
        /// <summary>
        /// 业务类型（采购、委外）
        /// </summary>
        [Required]
        public string BusinessType { get; set; }

        /// <summary>
        /// 业务主键（委外跟踪表、采购跟踪表的主键）
        /// </summary>
        [Required]
        public long BusinessId { get; set; }
    }

    /// <summary>
    /// 供应商负责人信息响应模型
    /// </summary>
    public class SupplierResponsibleResponse
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// 业务主键
        /// </summary>
        public long BusinessId { get; set; }

        /// <summary>
        /// 业务负责人姓名
        /// </summary>
        public string ResponsiblePersonName { get; set; }

        /// <summary>
        /// 业务负责人登录名
        /// </summary>
        public string ResponsiblePersonLoginName { get; set; }

        /// <summary>
        /// 是否找到供应商映射
        /// </summary>
        public bool HasSupplierMapping { get; set; }

        /// <summary>
        /// 是否使用默认负责人
        /// </summary>
        public bool IsDefaultResponsible { get; set; }

        /// <summary>
        /// 查询时间
        /// </summary>
        public DateTime QueryTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remarks { get; set; }
    }

    /// <summary>
    /// 批量获取默认负责人请求模型
    /// </summary>
    public class BatchGetDefaultResponsibleRequest
    {
        /// <summary>
        /// 业务项目列表
        /// </summary>
        [Required]
        public List<BusinessItem> BusinessItems { get; set; } = new List<BusinessItem>();
    }

    /// <summary>
    /// 业务项目
    /// </summary>
    public class BusinessItem
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        [Required]
        public string BusinessType { get; set; }

        /// <summary>
        /// 业务主键
        /// </summary>
        [Required]
        public string BusinessKey { get; set; }

        /// <summary>
        /// 物料编码（技术业务类型时可选，如不提供将根据业务主键从技术管理表自动获取）
        /// </summary>
        public string MaterialCode { get; set; }
    }

    /// <summary>
    /// 批量获取默认负责人响应模型
    /// </summary>
    public class BatchGetDefaultResponsibleResponse
    {
        /// <summary>
        /// 负责人信息列表
        /// </summary>
        public List<DefaultResponsibleInfo> ResponsibleInfos { get; set; } = new List<DefaultResponsibleInfo>();

        /// <summary>
        /// 查询时间
        /// </summary>
        public DateTime QueryTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 默认负责人信息
    /// </summary>
    public class DefaultResponsibleInfo
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// 业务主键
        /// </summary>
        public string BusinessKey { get; set; }

        /// <summary>
        /// 默认负责人登录名
        /// </summary>
        public string DefaultResponsibleLoginName { get; set; }

        /// <summary>
        /// 默认负责人姓名
        /// </summary>
        public string DefaultResponsibleName { get; set; }

        /// <summary>
        /// 是否找到负责人
        /// </summary>
        public bool Found { get; set; }

        /// <summary>
        /// 错误信息（未找到时的原因）
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 数据来源（供应商映射、技术管理、业务类型配置等）
        /// </summary>
        public string DataSource { get; set; }
    }
} 