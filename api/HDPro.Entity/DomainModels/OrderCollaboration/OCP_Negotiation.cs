/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果数据库字段发生变化，请在代码生器重新生成此Model
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDPro.Entity.SystemModels;

namespace HDPro.Entity.DomainModels
{
    [Entity(TableCnName = "协商",TableName = "OCP_Negotiation",DBServer = "ServiceDbContext")]
    public partial class OCP_Negotiation:ServiceEntity
    {
        /// <summary>
       ///协商ID
       /// </summary>
       [Key]
       [Display(Name ="协商ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long NegotiationID { get; set; }

       /// <summary>
       ///业务类型
       /// </summary>
       [Display(Name ="业务类型")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BusinessType { get; set; }

       /// <summary>
       ///计划跟踪号
       /// </summary>
       [Display(Name ="计划跟踪号")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string PlanTraceNo { get; set; }

       /// <summary>
       ///紧急等级
       /// </summary>
       [Display(Name ="紧急等级")]
       [MaxLength(20)]
       [Column(TypeName="nvarchar(20)")]
       [Editable(true)]
       public string UrgencyLevel { get; set; }

       /// <summary>
       ///业务主键
       /// </summary>
       [Display(Name ="业务主键")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string BusinessKey { get; set; }

       /// <summary>
       ///单据编号
       /// </summary>
       [Display(Name ="单据编号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BillNo { get; set; }

       /// <summary>
       ///单据行号
       /// </summary>
       [Display(Name ="单据行号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Seq { get; set; }

       /// <summary>
       ///物料编码
       /// </summary>
       [Display(Name ="物料编码")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string MaterialNumber { get; set; }

       /// <summary>
       ///物料名称
       /// </summary>
       [Display(Name ="物料名称")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string MaterialName { get; set; }

       /// <summary>
       ///规格型号
       /// </summary>
       [Display(Name ="规格型号")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Specification { get; set; }

       /// <summary>
       ///协商类型
       /// </summary>
       [Display(Name ="协商类型")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string NegotiationType { get; set; }

       /// <summary>
       ///协商状态
       /// </summary>
       [Display(Name ="协商状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string NegotiationStatus { get; set; }

       /// <summary>
       ///协商内容
       /// </summary>
       [Display(Name ="协商内容")]
       [MaxLength(1000)]
       [Column(TypeName="nvarchar(1000)")]
       [Editable(true)]
       public string NegotiationContent { get; set; }

       /// <summary>
       ///协商原因
       /// </summary>
       [Display(Name ="协商原因")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string NegotiationReason { get; set; }

       /// <summary>
       ///协商日期
       /// </summary>
       [Display(Name ="协商日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? NegotiationDate { get; set; }

       /// <summary>
       ///默认负责人
       /// </summary>
       [Display(Name ="默认负责人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string DefaultResPerson { get; set; }

       /// <summary>
       ///默认负责人姓名
       /// </summary>
       [Display(Name ="默认负责人姓名")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string DefaultResPersonName { get; set; }

       /// <summary>
       ///指定负责人
       /// </summary>
       [Display(Name ="指定负责人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string AssignedResPerson { get; set; }

       /// <summary>
       ///指定负责人姓名
       /// </summary>
       [Display(Name ="指定负责人姓名")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string AssignedResPersonName { get; set; }

       /// <summary>
       ///事项说明
       /// </summary>
       [Display(Name ="事项说明")]
       [MaxLength(1000)]
       [Column(TypeName="nvarchar(1000)")]
       [Editable(true)]
       public string MatterDescription { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string Remarks { get; set; }

       /// <summary>
       ///创建人ID
       /// </summary>
       [Display(Name ="创建人ID")]
       [Column(TypeName="int")]
       public int? CreateID { get; set; }

       /// <summary>
       ///创建日期
       /// </summary>
       [Display(Name ="创建日期")]
       [Column(TypeName="datetime")]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(20)]
       [Column(TypeName="nvarchar(20)")]
       public string Creator { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(20)]
       [Column(TypeName="nvarchar(20)")]
       public string Modifier { get; set; }

       /// <summary>
       ///修改日期
       /// </summary>
       [Display(Name ="修改日期")]
       [Column(TypeName="datetime")]
       public DateTime? ModifyDate { get; set; }

       /// <summary>
       ///修改人ID
       /// </summary>
       [Display(Name ="修改人ID")]
       [Column(TypeName="int")]
       public int? ModifyID { get; set; }

       /// <summary>
       ///交货日期
       /// </summary>
       [Display(Name ="交货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? DeliveryDate { get; set; }

       /// <summary>
       ///业务类型子类
       /// </summary>
       [Display(Name ="业务类型子类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BusinessTypeChild { get; set; }

       /// <summary>
       ///供应商代码
       /// </summary>
       [Display(Name ="供应商代码")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string SupplierCode { get; set; }

       /// <summary>
       ///供应商名称
       /// </summary>
       [Display(Name ="供应商名称")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string SupplierName { get; set; }

       /// <summary>
       ///回复交期
       /// </summary>
       [Display(Name ="回复交期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ReplyDeliveryDate { get; set; }

       
    }
}