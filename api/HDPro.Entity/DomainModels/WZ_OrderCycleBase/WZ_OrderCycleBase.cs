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
    [Entity(TableCnName = "排产智能体优化看板",TableName = "WZ_OrderCycleBase",DBServer = "ServiceDbContext")]
    public partial class WZ_OrderCycleBase:ServiceEntity
    {
        /// <summary>
       ///
       /// </summary>
       [Key]
       [Display(Name ="Id")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int Id { get; set; }

       /// <summary>
       ///销售订单号
       /// </summary>
       [Display(Name ="销售订单号")]
       [MaxLength(80)]
       [Column(TypeName="nvarchar(80)")]
       [Editable(true)]
       public string SalesOrderNo { get; set; }

       /// <summary>
       ///计划跟踪号
       /// </summary>
       [Display(Name ="计划跟踪号")]
       [MaxLength(80)]
       [Column(TypeName="nvarchar(80)")]
       [Editable(true)]
       public string PlanTrackingNo { get; set; }

       /// <summary>
       ///订单审核日期
       /// </summary>
       [Display(Name ="订单审核日期")]
       [Column(TypeName="date")]
       [Editable(true)]
       public DateTime? OrderApprovedDate { get; set; }

       /// <summary>
       ///回复交货日期
       /// </summary>
       [Display(Name ="回复交货日期")]
       [Column(TypeName="date")]
       [Editable(true)]
       public DateTime? ReplyDeliveryDate { get; set; }

       /// <summary>
       ///要货日期
       /// </summary>
       [Display(Name ="要货日期")]
       [Column(TypeName="date")]
       [Editable(true)]
       public DateTime? RequestedDeliveryDate { get; set; }

       /// <summary>
       ///标准交货日期
       /// </summary>
       [Display(Name ="标准交货日期")]
       [Column(TypeName="date")]
       [Editable(true)]
       public DateTime? StandardDeliveryDate { get; set; }

       /// <summary>
       ///排产日期
       /// </summary>
       [Display(Name ="排产日期")]
       [Column(TypeName="date")]
       [Editable(true)]
       public DateTime? ScheduleDate { get; set; }

       /// <summary>
       ///物料编码
       /// </summary>
       [Display(Name ="物料编码")]
       [MaxLength(80)]
       [Column(TypeName="nvarchar(80)")]
       [Editable(true)]
       public string MaterialCode { get; set; }

       /// <summary>
       ///阀体材质
       /// </summary>
       [Display(Name ="阀体材质")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string BodyMaterial { get; set; }

       /// <summary>
       ///内件材质
       /// </summary>
       [Display(Name ="内件材质")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string InnerMaterial { get; set; }

       /// <summary>
       ///法兰连接方式
       /// </summary>
       [Display(Name ="法兰连接方式")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string FlangeConnection { get; set; }

       /// <summary>
       ///上盖形式
       /// </summary>
       [Display(Name ="上盖形式")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string BonnetForm { get; set; }

       /// <summary>
       ///流量特性
       /// </summary>
       [Display(Name ="流量特性")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string FlowCharacteristic { get; set; }

       /// <summary>
       ///执行机构
       /// </summary>
       [Display(Name ="执行机构")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Actuator { get; set; }

       /// <summary>
       ///外购阀体
       /// </summary>
       [Display(Name ="外购阀体")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string OutsourcedValveBody { get; set; }

       /// <summary>
       ///阀门类别
       /// </summary>
       [Display(Name ="阀门类别")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string ValveCategory { get; set; }

       /// <summary>
       ///密封面形式
       /// </summary>
       [Display(Name ="密封面形式")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string SealFaceForm { get; set; }

       /// <summary>
       ///特品
       /// </summary>
       [Display(Name ="特品")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string SpecialProduct { get; set; }

       /// <summary>
       ///外购标志
       /// </summary>
       [Display(Name ="外购标志")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string PurchaseFlag { get; set; }

       /// <summary>
       ///产品名称
       /// </summary>
       [Display(Name ="产品名称")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string ProductName { get; set; }

       /// <summary>
       ///公称通径
       /// </summary>
       [Display(Name ="公称通径")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string NominalDiameter { get; set; }

       /// <summary>
       ///公称压力
       /// </summary>
       [Display(Name ="公称压力")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string NominalPressure { get; set; }

       /// <summary>
       ///固定周期(天)
       /// </summary>
       [Display(Name ="固定周期(天)")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? FixedCycleDays { get; set; }

       /// <summary>
       ///生产线
       /// </summary>
       [Display(Name ="生产线")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ProductionLine { get; set; }

       
    }
}