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
    [Entity(TableCnName = "整机跟踪表",TableName = "OCP_PrdMOTracking",DBServer = "ServiceDbContext")]
    public partial class OCP_PrdMOTracking:ServiceEntity
    {
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
       ///生产单据号
       /// </summary>
       [Display(Name ="生产单据号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ProductionOrderNo { get; set; }

       /// <summary>
       ///生产订单状态
       /// </summary>
       [Display(Name ="生产订单状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BillStatus { get; set; }

       /// <summary>
       ///物料编码
       /// </summary>
       [Display(Name ="物料编码")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MaterialCode { get; set; }

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
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Specification { get; set; }

       /// <summary>
       ///工单编号
       /// </summary>
       [Display(Name ="工单编号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MOBillNo { get; set; }

       /// <summary>
       ///MES批次号
       /// </summary>
       [Display(Name ="MES批次号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MESBatchNo { get; set; }

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
       ///物料属性
       /// </summary>
       [Display(Name ="物料属性")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ErpClsid { get; set; }

       /// <summary>
       ///执行状态
       /// </summary>
       [Display(Name ="执行状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ExecuteStatus { get; set; }

       /// <summary>
       ///领料日期
       /// </summary>
       [Display(Name ="领料日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? MaterialPickDate { get; set; }

       /// <summary>
       ///预装完工日期
       /// </summary>
       [Display(Name ="预装完工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PreCompleteDate { get; set; }

       /// <summary>
       ///阀体部件装配及执行机构完工日期
       /// </summary>
       [Display(Name ="阀体部件装配及执行机构完工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ValvePartCompleteDate { get; set; }

       /// <summary>
       ///强压泄漏试验完工日期
       /// </summary>
       [Display(Name ="强压泄漏试验完工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PressureCompleteDate { get; set; }

       /// <summary>
       ///附件安装及调试日期
       /// </summary>
       [Display(Name ="附件安装及调试日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? AccessoryInstallDate { get; set; }

       /// <summary>
       ///终检日期
       /// </summary>
       [Display(Name ="终检日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? FinalInspectionDate { get; set; }

       /// <summary>
       ///油漆完工日期
       /// </summary>
       [Display(Name ="油漆完工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PaintCompleteDate { get; set; }

       /// <summary>
       ///装箱日期
       /// </summary>
       [Display(Name ="装箱日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PackingDate { get; set; }

       /// <summary>
       ///装箱检验日期
       /// </summary>
       [Display(Name ="装箱检验日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PackingInspectionDate { get; set; }

       /// <summary>
       ///生产数量
       /// </summary>
       [Display(Name ="生产数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? ProductionQty { get; set; }

       /// <summary>
       ///入库数量
       /// </summary>
       [Display(Name ="入库数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? InboundQty { get; set; }

       /// <summary>
       ///回复日期
       /// </summary>
       [Display(Name ="回复日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ReplyDate { get; set; }

       /// <summary>
       ///超期天数
       /// </summary>
       [Display(Name ="超期天数")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? OverdueDays { get; set; }

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
       ///生产订单ID
       /// </summary>
       [Display(Name ="生产订单ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? FID { get; set; }

       /// <summary>
       ///物料ID
       /// </summary>
       [Display(Name ="物料ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? MaterialID { get; set; }

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
       ///ESB主键
       /// </summary>
       [Display(Name ="ESB主键")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string ESBID { get; set; }

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
       public DateTime? CompleteDate { get; set; }

       
    }
}