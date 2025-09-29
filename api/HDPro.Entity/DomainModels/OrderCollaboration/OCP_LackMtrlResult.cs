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
    [Entity(TableCnName = "缺料运算结果",TableName = "OCP_LackMtrlResult",DBServer = "ServiceDbContext")]
    public partial class OCP_LackMtrlResult:ServiceEntity
    {
        /// <summary>
       ///销售合同号
       /// </summary>
       [Display(Name ="销售合同号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ContractNo { get; set; }

       /// <summary>
       ///销售单据号
       /// </summary>
       [Display(Name ="销售单据号")]
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
       ///整机物料编码
       /// </summary>
       [Display(Name ="整机物料编码")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string TopMaterialNumber { get; set; }

       /// <summary>
       ///产品型号
       /// </summary>
       [Display(Name ="产品型号")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string ProductCategory { get; set; }

       /// <summary>
       ///规格型号
       /// </summary>
       [Display(Name ="规格型号")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Specification { get; set; }

       /// <summary>
       ///整机规格型号
       /// </summary>
       [Display(Name ="整机规格型号")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string TopSpecification { get; set; }

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
       ///物料属性
       /// </summary>
       [Display(Name ="物料属性")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ErpClsid { get; set; }

       /// <summary>
       ///物料分类
       /// </summary>
       [Display(Name ="物料分类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MaterialCategory { get; set; }

       /// <summary>
       ///需求数量
       /// </summary>
       [Display(Name ="需求数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? NeedQty { get; set; }

       /// <summary>
       ///库存数量
       /// </summary>
       [Display(Name ="库存数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? InventoryQty { get; set; }

       /// <summary>
       ///已下计划(在途,在制)
       /// </summary>
       [Display(Name ="已下计划(在途,在制)")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? PlanedQty { get; set; }

       /// <summary>
       ///未下计划数量
       /// </summary>
       [Display(Name ="未下计划数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? UnPlanedQty { get; set; }

       /// <summary>
       ///单据编号
       /// </summary>
       [Display(Name ="单据编号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BillNo { get; set; }

       /// <summary>
       ///单据分类
       /// </summary>
       [Display(Name ="单据分类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BillType { get; set; }

       /// <summary>
       ///采购数量
       /// </summary>
       [Display(Name ="采购数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? POQty { get; set; }

       /// <summary>
       ///供应商
       /// </summary>
       [Display(Name ="供应商")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string SupplierName { get; set; }

       /// <summary>
       ///负责人
       /// </summary>
       [Display(Name ="负责人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string PurchaserName { get; set; }

       /// <summary>
       ///计划开始日期
       /// </summary>
       [Display(Name ="计划开始日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PlanStartDate { get; set; }

       /// <summary>
       ///计划完成日期
       /// </summary>
       [Display(Name ="计划完成日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PlanEndDate { get; set; }

       /// <summary>
       ///采购申请审核日期
       /// </summary>
       [Display(Name ="采购申请审核日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CGSQAuditDate { get; set; }

       /// <summary>
       ///委外订单下达日期
       /// </summary>
       [Display(Name ="委外订单下达日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? WWReleaseDate { get; set; }

       /// <summary>
       ///领料日期
       /// </summary>
       [Display(Name ="领料日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PickMtrlDate { get; set; }

       /// <summary>
       ///生产实际开工日期
       /// </summary>
       [Display(Name ="生产实际开工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? StartDate { get; set; }

       /// <summary>
       ///生产订单创建日期
       /// </summary>
       [Display(Name ="生产订单创建日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? MOCreateDate { get; set; }

       /// <summary>
       ///生产计划完工日期
       /// </summary>
       [Display(Name ="生产计划完工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? FinishDate { get; set; }

       /// <summary>
       ///采购订单创建日期
       /// </summary>
       [Display(Name ="采购订单创建日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? POCreateDate { get; set; }

       /// <summary>
       ///采购订单审核日期
       /// </summary>
       [Display(Name ="采购订单审核日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? POAuditDate { get; set; }

       /// <summary>
       ///计划交货日期
       /// </summary>
       [Display(Name ="计划交货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PlanDeliveryDate { get; set; }

       /// <summary>
       ///客户要货日期
       /// </summary>
       [Display(Name ="客户要货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? F_ORA_DATETIME { get; set; }

       /// <summary>
       ///销售订单回复交货日期
       /// </summary>
       [Display(Name ="销售订单回复交货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? F_BLN_HFJHRQ { get; set; }

       /// <summary>
       ///插单日期
       /// </summary>
       [Display(Name ="插单日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? F_RLRP_CDRQ { get; set; }

       /// <summary>
       ///运算时间
       /// </summary>
       [Display(Name ="运算时间")]
       [Column(TypeName="datetime")]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///运算人
       /// </summary>
       [Display(Name ="运算人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       public string Creator { get; set; }

       /// <summary>
       ///主建
       /// </summary>
       [Key]
       [Display(Name ="主建")]
       [Column(TypeName="bigint")]
       [Required(AllowEmptyStrings=false)]
       public long ID { get; set; }

       /// <summary>
       ///运算ID
       /// </summary>
       [Display(Name ="运算ID")]
       [Column(TypeName="bigint")]
       public long? ComputeID { get; set; }

       /// <summary>
       ///创建人Id
       /// </summary>
       [Display(Name ="创建人Id")]
       [Column(TypeName="int")]
       public int? CreateID { get; set; }

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
       ///ESB主键
       /// </summary>
       [Display(Name ="ESB主键")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string ESBID { get; set; }

       /// <summary>
       ///任务时间
       /// </summary>
       [Display(Name ="任务时间")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string PlanTaskTime { get; set; }

       /// <summary>
       ///物料ID
       /// </summary>
       [Display(Name ="物料ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? TopMaterialID { get; set; }

       /// <summary>
       ///物料ID
       /// </summary>
       [Display(Name ="物料ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? MaterialID { get; set; }

       
    }
}