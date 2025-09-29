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
    [Entity(TableCnName = "自定义打印资料",TableName = "MES_SpecialPrintRequest",DBServer = "ServiceDbContext")]
    public partial class MES_SpecialPrintRequest:ServiceEntity
    {
        /// <summary>
       ///单据ID
       /// </summary>
       [Key]
       [Display(Name ="单据ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int uid { get; set; }

       /// <summary>
       ///产品编码(批次号)
       /// </summary>
       [Display(Name ="产品编码(批次号)")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string RetrospectCode { get; set; }

       /// <summary>
       ///设计位号
       /// </summary>
       [Display(Name ="设计位号")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string PositionNumber { get; set; }

       /// <summary>
       ///产品型号
       /// </summary>
       [Display(Name ="产品型号")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string ProductModel { get; set; }

       /// <summary>
       ///公称通径
       /// </summary>
       [Display(Name ="公称通径")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string NominalDiameter { get; set; }

       /// <summary>
       ///公称压力
       /// </summary>
       [Display(Name ="公称压力")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string NominalPressure { get; set; }

       /// <summary>
       ///阀体材质
       /// </summary>
       [Display(Name ="阀体材质")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string ValveBodyMaterial { get; set; }

       /// <summary>
       ///执行机构
       /// </summary>
       [Display(Name ="执行机构")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string ActuatorModel { get; set; }

       /// <summary>
       ///故障位
       /// </summary>
       [Display(Name ="故障位")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string FailPosition { get; set; }

       /// <summary>
       ///气源压力
       /// </summary>
       [Display(Name ="气源压力")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string AirSupplyPressure { get; set; }

       /// <summary>
       ///工作温度
       /// </summary>
       [Display(Name ="工作温度")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string OperatingTemperature { get; set; }

       /// <summary>
       ///额定行程
       /// </summary>
       [Display(Name ="额定行程")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string RatedStroke { get; set; }

       /// <summary>
       ///流量特性
       /// </summary>
       [Display(Name ="流量特性")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string FlowCharacteristic { get; set; }

       /// <summary>
       ///流量系数(CV值)
       /// </summary>
       [Display(Name ="流量系数(CV值)")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string FlowCoefficient { get; set; }

       /// <summary>
       ///自定义字段1
       /// </summary>
       [Display(Name ="自定义字段1")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key1 { get; set; }

       /// <summary>
       ///自定义值1
       /// </summary>
       [Display(Name ="自定义值1")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value1 { get; set; }

       /// <summary>
       ///自定义字段2
       /// </summary>
       [Display(Name ="自定义字段2")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key2 { get; set; }

       /// <summary>
       ///自定义值2
       /// </summary>
       [Display(Name ="自定义值2")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value2 { get; set; }

       /// <summary>
       ///自定义字段3
       /// </summary>
       [Display(Name ="自定义字段3")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key3 { get; set; }

       /// <summary>
       ///自定义值3
       /// </summary>
       [Display(Name ="自定义值3")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value3 { get; set; }

       /// <summary>
       ///自定义字段4
       /// </summary>
       [Display(Name ="自定义字段4")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key4 { get; set; }

       /// <summary>
       ///自定义值4
       /// </summary>
       [Display(Name ="自定义值4")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value4 { get; set; }

       /// <summary>
       ///自定义字段5
       /// </summary>
       [Display(Name ="自定义字段5")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key5 { get; set; }

       /// <summary>
       ///自定义值5
       /// </summary>
       [Display(Name ="自定义值5")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value5 { get; set; }

       /// <summary>
       ///自定义字段6
       /// </summary>
       [Display(Name ="自定义字段6")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key6 { get; set; }

       /// <summary>
       ///自定义值6
       /// </summary>
       [Display(Name ="自定义值6")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value6 { get; set; }

       /// <summary>
       ///自定义字段7
       /// </summary>
       [Display(Name ="自定义字段7")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key7 { get; set; }

       /// <summary>
       ///自定义值7
       /// </summary>
       [Display(Name ="自定义值7")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value7 { get; set; }

       /// <summary>
       ///自定义字段8
       /// </summary>
       [Display(Name ="自定义字段8")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key8 { get; set; }

       /// <summary>
       ///自定义值8
       /// </summary>
       [Display(Name ="自定义值8")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value8 { get; set; }

       /// <summary>
       ///自定义字段9
       /// </summary>
       [Display(Name ="自定义字段9")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key9 { get; set; }

       /// <summary>
       ///自定义值9
       /// </summary>
       [Display(Name ="自定义值9")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value9 { get; set; }

       /// <summary>
       ///自定义字段10
       /// </summary>
       [Display(Name ="自定义字段10")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key10 { get; set; }

       /// <summary>
       ///自定义值10
       /// </summary>
       [Display(Name ="自定义值10")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value10 { get; set; }

       /// <summary>
       ///自定义字段11
       /// </summary>
       [Display(Name ="自定义字段11")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key11 { get; set; }

       /// <summary>
       ///自定义值11
       /// </summary>
       [Display(Name ="自定义值11")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value11 { get; set; }

       /// <summary>
       ///自定义字段12
       /// </summary>
       [Display(Name ="自定义字段12")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key12 { get; set; }

       /// <summary>
       ///自定义值12
       /// </summary>
       [Display(Name ="自定义值12")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value12 { get; set; }

       /// <summary>
       ///自定义字段13
       /// </summary>
       [Display(Name ="自定义字段13")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key13 { get; set; }

       /// <summary>
       ///自定义值13
       /// </summary>
       [Display(Name ="自定义值13")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value13 { get; set; }

       /// <summary>
       ///自定义字段14
       /// </summary>
       [Display(Name ="自定义字段14")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key14 { get; set; }

       /// <summary>
       ///自定义值14
       /// </summary>
       [Display(Name ="自定义值14")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value14 { get; set; }

       /// <summary>
       ///自定义字段15
       /// </summary>
       [Display(Name ="自定义字段15")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Key15 { get; set; }

       /// <summary>
       ///自定义值15
       /// </summary>
       [Display(Name ="自定义值15")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string UDF_Value15 { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string Creator { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string Modifier { get; set; }

       /// <summary>
       ///修改时间
       /// </summary>
       [Display(Name ="修改时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ModifyDate { get; set; }

       /// <summary>
       ///创建人ID
       /// </summary>
       [Display(Name ="创建人ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreateID { get; set; }

       /// <summary>
       ///修改人ID
       /// </summary>
       [Display(Name ="修改人ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ModifyID { get; set; }

       
    }
}