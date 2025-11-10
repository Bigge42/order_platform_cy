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
    [Entity(TableCnName = "当月完成BOM情况",TableName = "vw_OCP_Tech_BOM_Status_Monthly",DBServer = "ServiceDbContext")]
    public partial class vw_OCP_Tech_BOM_Status_Monthly:ServiceEntity
    {
        /// <summary>
       ///计划号
       /// </summary>
       [Key]
       [Display(Name ="计划号")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string PlanTraceNo { get; set; }

       /// <summary>
       ///物料编码
       /// </summary>
       [Display(Name ="物料编码")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string MaterialCode { get; set; }

       /// <summary>
       ///产品型号
       /// </summary>
       [Display(Name ="产品型号")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string ProductModel { get; set; }

       /// <summary>
       ///订单编号
       /// </summary>
       [Display(Name ="订单编号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string OrderNo { get; set; }

       /// <summary>
       ///订单日期
       /// </summary>
       [Display(Name ="订单日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? OrderDate { get; set; }

       /// <summary>
       ///订单审核日期
       /// </summary>
       [Display(Name ="订单审核日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? OrderAuditDate { get; set; }

       /// <summary>
       ///BOM创建日期
       /// </summary>
       [Display(Name ="BOM创建日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? BomCreateDate { get; set; }

       /// <summary>
       ///BOM存在时间(天)
       /// </summary>
       [Display(Name ="BOM存在时间(天)")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? BomAgeDays { get; set; }

       /// <summary>
       ///BOM创建延迟天数
       /// </summary>
       [Display(Name ="BOM创建延迟天数")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? BomDelayDays { get; set; }

       /// <summary>
       ///BOM已缺失天数
       /// </summary>
       [Display(Name ="BOM已缺失天数")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? BomMissingDays { get; set; }

       /// <summary>
       ///BOM创建人
       /// </summary>
       [Display(Name ="BOM创建人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string TCBomCreator { get; set; }

       /// <summary>
       ///物料名称
       /// </summary>
       [Display(Name ="物料名称")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string MaterialName { get; set; }

       /// <summary>
       ///CV
       /// </summary>
       [Display(Name ="CV")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string CV { get; set; }

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
       ///法兰标准
       /// </summary>
       [Display(Name ="法兰标准")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string FlangeStandard { get; set; }

       /// <summary>
       ///密封面形式
       /// </summary>
       [Display(Name ="密封面形式")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string SealFaceForm { get; set; }

       /// <summary>
       ///阀体材质
       /// </summary>
       [Display(Name ="阀体材质")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string BodyMaterial { get; set; }

       /// <summary>
       ///阀内件材质
       /// </summary>
       [Display(Name ="阀内件材质")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string TrimMaterial { get; set; }

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
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Remarks { get; set; }

       /// <summary>
       ///数量
       /// </summary>
       [Display(Name ="数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? Qty { get; set; }

       /// <summary>
       ///回复交货日期
       /// </summary>
       [Display(Name ="回复交货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ReplyDeliveryDate { get; set; }

       
    }
}