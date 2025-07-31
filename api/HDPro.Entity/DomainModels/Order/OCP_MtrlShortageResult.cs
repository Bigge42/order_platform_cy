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
    [Entity(TableCnName = "缺料运算结果",TableName = "OCP_MtrlShortageResult",DBServer = "ServiceDbContext")]
    public partial class OCP_MtrlShortageResult:ServiceEntity
    {
        /// <summary>
       ///结果记录ID
       /// </summary>
       [Key]
       [Display(Name ="结果记录ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int ResultRecordID { get; set; }

       /// <summary>
       ///运算ID
       /// </summary>
       [Display(Name ="运算ID")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string OperationID { get; set; }

       /// <summary>
       ///订单计划ID
       /// </summary>
       [Display(Name ="订单计划ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int PlanID { get; set; }

       /// <summary>
       ///物料编码
       /// </summary>
       [Display(Name ="物料编码")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string MaterialCode { get; set; }

       /// <summary>
       ///物料名称
       /// </summary>
       [Display(Name ="物料名称")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string MaterialName { get; set; }

       /// <summary>
       ///物料属性
       /// </summary>
       [Display(Name ="物料属性")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MaterialProperty { get; set; }

       /// <summary>
       ///缺料分类
       /// </summary>
       [Display(Name ="缺料分类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ShortageCategory { get; set; }

       /// <summary>
       ///缺料数量
       /// </summary>
       [Display(Name ="缺料数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public decimal ShortageQuantity { get; set; }

       /// <summary>
       ///需求数量
       /// </summary>
       [Display(Name ="需求数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public decimal RequiredQuantity { get; set; }

       /// <summary>
       ///已下计划数量
       /// </summary>
       [Display(Name ="已下计划数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? PlannedQuantity { get; set; }

       /// <summary>
       ///库存数量
       /// </summary>
       [Display(Name ="库存数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? StockQuantity { get; set; }

       /// <summary>
       ///委外订单号
       /// </summary>
       [Display(Name ="委外订单号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string OutsourcingOrderNumber { get; set; }

       /// <summary>
       ///采购订单号
       /// </summary>
       [Display(Name ="采购订单号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string PurchaseOrderNumber { get; set; }

       /// <summary>
       ///生产单据号
       /// </summary>
       [Display(Name ="生产单据号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ProductionDocNumber { get; set; }

       /// <summary>
       ///来源单据
       /// </summary>
       [Display(Name ="来源单据")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string SourceDocument { get; set; }

       /// <summary>
       ///计划交货日期
       /// </summary>
       [Display(Name ="计划交货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PlannedDeliveryDate { get; set; }

       /// <summary>
       ///回复交货日期
       /// </summary>
       [Display(Name ="回复交货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ReplyDeliveryDate { get; set; }

       /// <summary>
       ///预计完工日期
       /// </summary>
       [Display(Name ="预计完工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? EstimatedCompletionDate { get; set; }

       /// <summary>
       ///发起人ID
       /// </summary>
       [Display(Name ="发起人ID")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string InitiatorID { get; set; }

       /// <summary>
       ///发起人姓名
       /// </summary>
       [Display(Name ="发起人姓名")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string InitiatorName { get; set; }

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
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       public string Creator { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
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

       
    }
}