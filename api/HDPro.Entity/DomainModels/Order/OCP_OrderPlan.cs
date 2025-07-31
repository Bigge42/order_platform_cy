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
    [Entity(TableCnName = "订单计划",TableName = "OCP_OrderPlan",DBServer = "ServiceDbContext")]
    public partial class OCP_OrderPlan:ServiceEntity
    {
        /// <summary>
       ///计划ID
       /// </summary>
       [Key]
       [Display(Name ="计划ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int PlanID { get; set; }

       /// <summary>
       ///订单组ID
       /// </summary>
       [Display(Name ="订单组ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int OrderGroupID { get; set; }

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
       public DateTime? BOMCreationDate { get; set; }

       /// <summary>
       ///订单审核日期
       /// </summary>
       [Display(Name ="订单审核日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? OrderReviewDate { get; set; }

       /// <summary>
       ///要货日期
       /// </summary>
       [Display(Name ="要货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? RequiredDeliveryDate { get; set; }

       /// <summary>
       ///回复交货日期
       /// </summary>
       [Display(Name ="回复交货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ReplyDeliveryDate { get; set; }

       /// <summary>
       ///排产日期
       /// </summary>
       [Display(Name ="排产日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ScheduleDate { get; set; }

       /// <summary>
       ///计划确认时间
       /// </summary>
       [Display(Name ="计划确认时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PlanConfirmationTime { get; set; }

       /// <summary>
       ///计划开工日期
       /// </summary>
       [Display(Name ="计划开工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PlannedStartDate { get; set; }

       /// <summary>
       ///实际开工日期
       /// </summary>
       [Display(Name ="实际开工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ActualStartDate { get; set; }

       /// <summary>
       ///订单完成状态
       /// </summary>
       [Display(Name ="订单完成状态")]
       [MaxLength(20)]
       [Column(TypeName="nvarchar(20)")]
       [Editable(true)]
       public string OrderStatus { get; set; }

       /// <summary>
       ///订单数量
       /// </summary>
       [Display(Name ="订单数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public decimal OrderQuantity { get; set; }

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