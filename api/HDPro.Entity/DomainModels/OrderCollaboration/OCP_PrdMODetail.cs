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
    [Entity(TableCnName = "整机生产订单明细表",TableName = "OCP_PrdMODetail",DBServer = "ServiceDbContext")]
    public partial class OCP_PrdMODetail:ServiceEntity
    {
        /// <summary>
       ///明细ID
       /// </summary>
       [Key]
       [Display(Name ="明细ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long DetailID { get; set; }

       /// <summary>
       ///主表ID
       /// </summary>
       [Display(Name ="主表ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? ID { get; set; }

       /// <summary>
       ///生产订单ID
       /// </summary>
       [Display(Name ="生产订单ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? FID { get; set; }

       /// <summary>
       ///生产订单明细ID
       /// </summary>
       [Display(Name ="生产订单明细ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? FENTRYID { get; set; }

       /// <summary>
       ///行号
       /// </summary>
       [Display(Name ="行号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Seq { get; set; }

       /// <summary>
       ///计划跟踪号
       /// </summary>
       [Display(Name ="计划跟踪号")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string PlanTraceNo { get; set; }

       /// <summary>
       ///销售合同号
       /// </summary>
       [Display(Name ="销售合同号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string SalesContractNo { get; set; }

       /// <summary>
       ///销售单据号
       /// </summary>
       [Display(Name ="销售单据号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string SalesDocumentNo { get; set; }

       /// <summary>
       ///生产订单状态
       /// </summary>
       [Display(Name ="生产订单状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ProductionOrderStatus { get; set; }

       /// <summary>
       ///物料ID
       /// </summary>
       [Display(Name ="物料ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? MaterialID { get; set; }

       /// <summary>
       ///物料编码
       /// </summary>
       [Display(Name ="物料编码")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string MaterialNumber { get; set; }

       /// <summary>
       ///物料名称
       /// </summary>
       [Display(Name ="物料名称")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string MaterialName { get; set; }

       /// <summary>
       ///产品大类
       /// </summary>
       [Display(Name ="产品大类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ProductCategory { get; set; }

       /// <summary>
       ///任务时间月
       /// </summary>
       [Display(Name ="任务时间月")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string PlanTaskMonth { get; set; }

       /// <summary>
       ///任务时间周
       /// </summary>
       [Display(Name ="任务时间周")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string PlanTaskWeek { get; set; }

       /// <summary>
       ///紧急等级
       /// </summary>
       [Display(Name ="紧急等级")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string Urgency { get; set; }

       /// <summary>
       ///计划数量
       /// </summary>
       [Display(Name ="计划数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? PlanQty { get; set; }

       /// <summary>
       ///已入库数量
       /// </summary>
       [Display(Name ="已入库数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? InboundQty { get; set; }

       /// <summary>
       ///未入库数量
       /// </summary>
       [Display(Name ="未入库数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? UnInboundQty { get; set; }

       /// <summary>
       ///超期数量
       /// </summary>
       [Display(Name ="超期数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? OverdueQty { get; set; }

       /// <summary>
       ///生产订单领料日期
       /// </summary>
       [Display(Name ="生产订单领料日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PickMtrlDate { get; set; }

       /// <summary>
       ///计划完工日期
       /// </summary>
       [Display(Name ="计划完工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PlanCompleteDate { get; set; }

       /// <summary>
       ///实际完工日期
       /// </summary>
       [Display(Name ="实际完工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ActualCompleteDate { get; set; }

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

       
    }
}