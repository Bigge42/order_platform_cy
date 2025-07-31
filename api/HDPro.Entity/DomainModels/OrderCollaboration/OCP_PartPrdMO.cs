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
    [Entity(TableCnName = "部件生产订单表",TableName = "OCP_PartPrdMO",DetailTable =  new Type[] { typeof(OCP_PartPrdMODetail)},DetailTableCnName = "部件生产订单明细表",DBServer = "ServiceDbContext")]
    public partial class OCP_PartPrdMO:ServiceEntity
    {
        /// <summary>
       ///主键ID
       /// </summary>
       [Key]
       [Display(Name ="主键ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long ID { get; set; }

       /// <summary>
       ///生产单据号
       /// </summary>
       [Display(Name ="生产单据号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ProductionOrderNo { get; set; }

       /// <summary>
       ///生产订单ID
       /// </summary>
       [Display(Name ="生产订单ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? FID { get; set; }

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
       ///生产订单审核时间
       /// </summary>
       [Display(Name ="生产订单审核时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? MOAuditDate { get; set; }

       /// <summary>
       ///生产类型
       /// </summary>
       [Display(Name ="生产类型")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ProductionType { get; set; }

       /// <summary>
       ///生产数量
       /// </summary>
       [Display(Name ="生产数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? ProductionQty { get; set; }

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
       ///超期天数
       /// </summary>
       [Display(Name ="超期天数")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? OverdueDays { get; set; }

       /// <summary>
       ///操作
       /// </summary>
       [Display(Name ="操作")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string Operation { get; set; }

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

       [Display(Name ="部件生产订单明细表")]
       [ForeignKey("ID")]
       public List<OCP_PartPrdMODetail> OCP_PartPrdMODetail { get; set; }


       
    }
}