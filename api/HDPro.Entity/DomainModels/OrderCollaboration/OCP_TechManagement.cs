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
    [Entity(TableCnName = "技术管理表",TableName = "OCP_TechManagement",DBServer = "ServiceDbContext")]
    public partial class OCP_TechManagement:ServiceEntity
    {
        /// <summary>
       ///销售合同号
       /// </summary>
       [Display(Name ="销售合同号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string SalesContractNo { get; set; }

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
       public string PlanTraceNo { get; set; }

       /// <summary>
       ///排产月份
       /// </summary>
       [Display(Name ="排产月份")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
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
       ///BOM创建日期
       /// </summary>
       [Display(Name ="BOM创建日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? BOMCreateDate { get; set; }

       /// <summary>
       ///要求完成日期
       /// </summary>
       [Display(Name ="要求完成日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? RequiredCompletionDate { get; set; }

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
       public string ProductModel { get; set; }

       /// <summary>
       ///公称通径
       /// </summary>
       [Display(Name ="公称通径")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string NominalDiameter { get; set; }

       /// <summary>
       ///公称压力
       /// </summary>
       [Display(Name ="公称压力")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string NominalPressure { get; set; }

       /// <summary>
       ///流量特性
       /// </summary>
       [Display(Name ="流量特性")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string FlowCharacteristic { get; set; }

       /// <summary>
       ///填料形式
       /// </summary>
       [Display(Name ="填料形式")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string PackingForm { get; set; }

       /// <summary>
       ///法兰连接方式
       /// </summary>
       [Display(Name ="法兰连接方式")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string FlangeConnection { get; set; }

       /// <summary>
       ///执行机构型号
       /// </summary>
       [Display(Name ="执行机构型号")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string ActuatorModel { get; set; }

       /// <summary>
       ///执行机构行程
       /// </summary>
       [Display(Name ="执行机构行程")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string ActuatorStroke { get; set; }

       /// <summary>
       ///销售数量
       /// </summary>
       [Display(Name ="销售数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? SalesQty { get; set; }

       /// <summary>
       ///超期天数
       /// </summary>
       [Display(Name ="超期天数")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? OverdueDays { get; set; }

       /// <summary>
       ///标准天数
       /// </summary>
       [Display(Name ="标准天数")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? StandardDays { get; set; }

       /// <summary>
       ///设计人
       /// </summary>
       [Display(Name ="设计人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Designer { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Remarks { get; set; }

       /// <summary>
       ///技术ID
       /// </summary>
       [Key]
       [Display(Name ="技术ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long TechID { get; set; }

       /// <summary>
       ///物料ID
       /// </summary>
       [Display(Name ="物料ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? MaterialID { get; set; }

       /// <summary>
       ///订单状态
       /// </summary>
       [Display(Name ="订单状态")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string OrderStatus { get; set; }

       /// <summary>
       ///是否有BOM
       /// </summary>
       [Display(Name ="是否有BOM")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? HasBOM { get; set; }

       /// <summary>
       ///要求完工时间
       /// </summary>
       [Display(Name ="要求完工时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? RequiredFinishTime { get; set; }

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
       ///销售订单明细ID
       /// </summary>
       [Display(Name ="销售订单明细ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? SOEntryID { get; set; }

       /// <summary>
       ///关联总任务单据号
       /// </summary>
       [Display(Name ="关联总任务单据号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string JoinTaskBillNo { get; set; }

       /// <summary>
       ///是否关联总任务
       /// </summary>
       [Display(Name ="是否关联总任务")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? IsJoinTask { get; set; }

       /// <summary>
       ///物料属性
       /// </summary>
       [Display(Name ="物料属性")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ErpClsid { get; set; }

       /// <summary>
       ///ESB修改日期
       /// </summary>
       [Display(Name ="ESB修改日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ESBModifyDate { get; set; }

       /// <summary>
       ///评审意见
       /// </summary>
       [Display(Name ="评审意见")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string FPSYJ { get; set; }

       
    }
}