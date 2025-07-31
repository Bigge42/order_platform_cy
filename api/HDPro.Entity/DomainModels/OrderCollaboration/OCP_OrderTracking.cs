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
    [Entity(TableCnName = "订单跟踪",TableName = "OCP_OrderTracking",DBServer = "ServiceDbContext")]
    public partial class OCP_OrderTracking:ServiceEntity
    {
        /// <summary>
       ///项目名称
       /// </summary>
       [Display(Name ="项目名称")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string ProjectName { get; set; }

       /// <summary>
       ///销售员
       /// </summary>
       [Display(Name ="销售员")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string SalesPerson { get; set; }

       /// <summary>
       ///合同类型
       /// </summary>
       [Display(Name ="合同类型")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ContractType { get; set; }

       /// <summary>
       ///销售合同号
       /// </summary>
       [Display(Name ="销售合同号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ContractNo { get; set; }

       /// <summary>
       ///使用单位
       /// </summary>
       [Display(Name ="使用单位")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string UseUnit { get; set; }

       /// <summary>
       ///客户名称
       /// </summary>
       [Display(Name ="客户名称")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string CustName { get; set; }

       /// <summary>
       ///销售订单号
       /// </summary>
       [Display(Name ="销售订单号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string SOBillNo { get; set; }

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
       ///产品型号
       /// </summary>
       [Display(Name ="产品型号")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string ProductionModel { get; set; }

       /// <summary>
       ///规格型号
       /// </summary>
       [Display(Name ="规格型号")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string TopSpecification { get; set; }

       /// <summary>
       ///是否关联总任务
       /// </summary>
       [Display(Name ="是否关联总任务")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? IsJoinTask { get; set; }

       /// <summary>
       ///排产月份
       /// </summary>
       [Display(Name ="排产月份")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string ProScheduleYearMonth { get; set; }

       /// <summary>
       ///月度任务
       /// </summary>
       [Display(Name ="月度任务")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string PlanTaskMonth { get; set; }

       /// <summary>
       ///周任务
       /// </summary>
       [Display(Name ="周任务")]
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
       ///客户要货日期
       /// </summary>
       [Display(Name ="客户要货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? DeliveryDate { get; set; }

       /// <summary>
       ///回复交货日期
       /// </summary>
       [Display(Name ="回复交货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ReplyDeliveryDate { get; set; }

       /// <summary>
       ///运算日期
       /// </summary>
       [Display(Name ="运算日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ComputedDate { get; set; }

       /// <summary>
       ///中标日期
       /// </summary>
       [Display(Name ="中标日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? BidDate { get; set; }

       /// <summary>
       ///BOM创建日期
       /// </summary>
       [Display(Name ="BOM创建日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? BomCreateDate { get; set; }

       /// <summary>
       ///订单创建日期
       /// </summary>
       [Display(Name ="订单创建日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? OrderCreateDate { get; set; }

       /// <summary>
       ///订单审核日期
       /// </summary>
       [Display(Name ="订单审核日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? OrderAuditDate { get; set; }

       /// <summary>
       ///排产日期
       /// </summary>
       [Display(Name ="排产日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PrdScheduleDate { get; set; }

       /// <summary>
       ///计划确认日期
       /// </summary>
       [Display(Name ="计划确认日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PlanConfirmDate { get; set; }

       /// <summary>
       ///计划开工日期
       /// </summary>
       [Display(Name ="计划开工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PlanStartDate { get; set; }

       /// <summary>
       ///实际开工日期
       /// </summary>
       [Display(Name ="实际开工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? StartDate { get; set; }

       /// <summary>
       ///最近入库日期
       /// </summary>
       [Display(Name ="最近入库日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LastInStockDate { get; set; }

       /// <summary>
       ///订单数量
       /// </summary>
       [Display(Name ="订单数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? OrderQty { get; set; }

       /// <summary>
       ///入库数量
       /// </summary>
       [Display(Name ="入库数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? InstockQty { get; set; }

       /// <summary>
       ///最近出库日期
       /// </summary>
       [Display(Name ="最近出库日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? LastOutStockDate { get; set; }

       /// <summary>
       ///出库数量
       /// </summary>
       [Display(Name ="出库数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? OutStockQty { get; set; }

       /// <summary>
       ///未完数量
       /// </summary>
       [Display(Name ="未完数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? UnInstockQty { get; set; }

       /// <summary>
       ///交货情况
       /// </summary>
       [Display(Name ="交货情况")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MtoNoStatus { get; set; }

       /// <summary>
       ///订单状态
       /// </summary>
       [Display(Name ="订单状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BillStatus { get; set; }

       /// <summary>
       ///主键ID
       /// </summary>
       [Key]
       [Display(Name ="主键ID")]
       [Column(TypeName="bigint")]
       [Required(AllowEmptyStrings=false)]
       public long Id { get; set; }

       /// <summary>
       ///关联总任务单据号
       /// </summary>
       [Display(Name ="关联总任务单据号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string JoinTaskBillNo { get; set; }

       /// <summary>
       ///订单完成状态
       /// </summary>
       [Display(Name ="订单完成状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string FinishStatus { get; set; }

       /// <summary>
       ///订单ID
       /// </summary>
       [Display(Name ="订单ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? SOBillID { get; set; }

       /// <summary>
       ///订单明细ID
       /// </summary>
       [Display(Name ="订单明细ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? SOEntryID { get; set; }

       /// <summary>
       ///作废状态
       /// </summary>
       [Display(Name ="作废状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string CancelStatus { get; set; }

       /// <summary>
       ///订单明细金额
       /// </summary>
       [Display(Name ="订单明细金额")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? Amount { get; set; }

       /// <summary>
       ///业务终止
       /// </summary>
       [Display(Name ="业务终止")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MRPTERMINATESTATUS { get; set; }

       /// <summary>
       ///业务冻结
       /// </summary>
       [Display(Name ="业务冻结")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MRPFREEZESTATUS { get; set; }

       /// <summary>
       ///创建人Id
       /// </summary>
       [Display(Name ="创建人Id")]
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
       ///修改人Id
       /// </summary>
       [Display(Name ="修改人Id")]
       [Column(TypeName="int")]
       public int? ModifyID { get; set; }

       /// <summary>
       ///ESB修改日期
       /// </summary>
       [Display(Name ="ESB修改日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ESBModifyDate { get; set; }

       /// <summary>
       ///物料ID
       /// </summary>
       [Display(Name ="物料ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? MaterialID { get; set; }

       /// <summary>
       ///备料
       /// </summary>
       [Display(Name ="备料")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string PrepareMtrl { get; set; }

       
    }
}