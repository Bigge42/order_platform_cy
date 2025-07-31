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
    [Entity(TableCnName = "销售订单明细",TableName = "OCP_SalesOrderDetail",DBServer = "ServiceDbContext")]
    public partial class OCP_SalesOrderDetail:ServiceEntity
    {
        /// <summary>
       ///订单ID
       /// </summary>
       [Display(Name ="订单ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int OrderID { get; set; }

       /// <summary>
       ///明细ID
       /// </summary>
       [Key]
       [Display(Name ="明细ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int DetailID { get; set; }

       /// <summary>
       ///位号
       /// </summary>
       [Display(Name ="位号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string PositionNumber { get; set; }

       /// <summary>
       ///阀门型号
       /// </summary>
       [Display(Name ="阀门型号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ValveModel { get; set; }

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
       ///数量
       /// </summary>
       [Display(Name ="数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public decimal Quantity { get; set; }

       /// <summary>
       ///单价
       /// </summary>
       [Display(Name ="单价")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public decimal UnitPrice { get; set; }

       /// <summary>
       ///总价
       /// </summary>
       [Display(Name ="总价")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public decimal TotalPrice { get; set; }

       /// <summary>
       ///计划跟踪号
       /// </summary>
       [Display(Name ="计划跟踪号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string PlanTrackingNumber { get; set; }

       /// <summary>
       ///当前节点
       /// </summary>
       [Display(Name ="当前节点")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string CurrentNode { get; set; }

       /// <summary>
       ///ERP上一节点
       /// </summary>
       [Display(Name ="ERP上一节点")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ERPPreviousNode { get; set; }

       /// <summary>
       ///ERP完成时间
       /// </summary>
       [Display(Name ="ERP完成时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ERPCompletionTime { get; set; }

       /// <summary>
       ///MOM上一节点
       /// </summary>
       [Display(Name ="MOM上一节点")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MOMPreviousNode { get; set; }

       /// <summary>
       ///MOM完成时间
       /// </summary>
       [Display(Name ="MOM完成时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? MOMCompletionTime { get; set; }

       /// <summary>
       ///状态
       /// </summary>
       [Display(Name ="状态")]
       [MaxLength(20)]
       [Column(TypeName="nvarchar(20)")]
       [Editable(true)]
       public string Status { get; set; }

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