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
    [Entity(TableCnName = "委外未完跟踪",TableName = "OCP_SubOrderUnFinishTrack",DBServer = "ServiceDbContext")]
    public partial class OCP_SubOrderUnFinishTrack:ServiceEntity
    {
        /// <summary>
       ///采购订单编号
       /// </summary>
       [Display(Name ="采购订单编号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string POBillNo { get; set; }

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
       ///订单状态
       /// </summary>
       [Display(Name ="订单状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BillStatus { get; set; }

       /// <summary>
       ///计划跟踪号
       /// </summary>
       [Display(Name ="计划跟踪号")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string MtoNo { get; set; }

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
       ///规格型号
       /// </summary>
       [Display(Name ="规格型号")]
       [MaxLength(1000)]
       [Column(TypeName="nvarchar(1000)")]
       [Editable(true)]
       public string Specification { get; set; }

       /// <summary>
       ///物料分类
       /// </summary>
       [Display(Name ="物料分类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MaterialCategory { get; set; }

       /// <summary>
       ///委外订单编号
       /// </summary>
       [Display(Name ="委外订单编号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string SubOrderNo { get; set; }

       /// <summary>
       ///委外订单创建日期
       /// </summary>
       [Display(Name ="委外订单创建日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? SubOrderCreateDate { get; set; }

       /// <summary>
       ///委外订单审核日期
       /// </summary>
       [Display(Name ="委外订单审核日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? SubOrderAuditDate { get; set; }

       /// <summary>
       ///委外订单下达日期
       /// </summary>
       [Display(Name ="委外订单下达日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? SubOrderReleaseDate { get; set; }

       /// <summary>
       ///采购订单创建日期
       /// </summary>
       [Display(Name ="采购订单创建日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? POCreateDate { get; set; }

       /// <summary>
       ///委外领料日期
       /// </summary>
       [Display(Name ="委外领料日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PickDate { get; set; }

       /// <summary>
       ///采购订单审核日期
       /// </summary>
       [Display(Name ="采购订单审核日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? POApproveDate { get; set; }

       /// <summary>
       ///采购数量
       /// </summary>
       [Display(Name ="采购数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? POQty { get; set; }

       /// <summary>
       ///交货日期
       /// </summary>
       [Display(Name ="交货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? DeliveryDate { get; set; }

       /// <summary>
       ///送货单号
       /// </summary>
       [Display(Name ="送货单号")]
       [MaxLength(1000)]
       [Column(TypeName="nvarchar(1000)")]
       [Editable(true)]
       public string DeliveryNo { get; set; }

       /// <summary>
       ///最近回复交货日期
       /// </summary>
       [Display(Name ="最近回复交货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LastReplyDeliveryDate { get; set; }

       /// <summary>
       ///最近送货日期
       /// </summary>
       [Display(Name ="最近送货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LastDeliveryDate { get; set; }

       /// <summary>
       ///累计送货数量
       /// </summary>
       [Display(Name ="累计送货数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? TotalDeliveryQty { get; set; }

       /// <summary>
       ///最近到货日期
       /// </summary>
       [Display(Name ="最近到货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LastArrivalDate { get; set; }

       /// <summary>
       ///累计到货数量
       /// </summary>
       [Display(Name ="累计到货数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? TotalArrivalQty { get; set; }

       /// <summary>
       ///送货状态
       /// </summary>
       [Display(Name ="送货状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string DeliveryStatus { get; set; }

       /// <summary>
       ///未质检状态
       /// </summary>
       [Display(Name ="未质检状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string InspectionStatus { get; set; }

       /// <summary>
       ///最近质检日期
       /// </summary>
       [Display(Name ="最近质检日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LastInspectionDate { get; set; }

       /// <summary>
       ///累计合格数量
       /// </summary>
       [Display(Name ="累计合格数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? TotalQualifiedQty { get; set; }

       /// <summary>
       ///前处理状态
       /// </summary>
       [Display(Name ="前处理状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string PreprocessStatus { get; set; }

       /// <summary>
       ///最近前处理日期
       /// </summary>
       [Display(Name ="最近前处理日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LatestPreprocessDate { get; set; }

       /// <summary>
       ///入库状态
       /// </summary>
       [Display(Name ="入库状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string InstockStatus { get; set; }

       /// <summary>
       ///最晚入库日期
       /// </summary>
       [Display(Name ="最晚入库日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LastInstockDate { get; set; }

       /// <summary>
       ///累计入库数量
       /// </summary>
       [Display(Name ="累计入库数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? WMSInboundQty { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(1000)]
       [Column(TypeName="nvarchar(1000)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///订单备注
       /// </summary>
       [Display(Name ="订单备注")]
       [MaxLength(1000)]
       [Column(TypeName="nvarchar(1000)")]
       [Editable(true)]
       public string OrderRemark { get; set; }

       /// <summary>
       ///跟踪ID
       /// </summary>
       [Key]
       [Display(Name ="跟踪ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long TrackID { get; set; }

       /// <summary>
       ///委外订单ID
       /// </summary>
       [Display(Name ="委外订单ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? FID { get; set; }

       /// <summary>
       ///委外订单明细ID
       /// </summary>
       [Display(Name ="委外订单明细ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? FENTRYID { get; set; }

       /// <summary>
       ///总任务月
       /// </summary>
       [Display(Name ="总任务月")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string PlanTaskMonth { get; set; }

       /// <summary>
       ///总任务周
       /// </summary>
       [Display(Name ="总任务周")]
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
       ///物料ID
       /// </summary>
       [Display(Name ="物料ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? MaterialID { get; set; }

       /// <summary>
       ///入库数量
       /// </summary>
       [Display(Name ="入库数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? InstockQty { get; set; }

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
       ///行号
       /// </summary>
       [Display(Name ="行号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Seq { get; set; }

       
    }
}