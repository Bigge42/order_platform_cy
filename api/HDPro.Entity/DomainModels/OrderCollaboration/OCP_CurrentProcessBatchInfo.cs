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
    [Entity(TableCnName = "当前工序批次信息",TableName = "OCP_CurrentProcessBatchInfo",DBServer = "ServiceDbContext")]
    public partial class OCP_CurrentProcessBatchInfo:ServiceEntity
    {
        /// <summary>
       ///工序ID
       /// </summary>
       [Key]
       [Display(Name ="工序ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long ProcessID { get; set; }

       /// <summary>
       ///详情ID
       /// </summary>
       [Display(Name ="详情ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? DetailID { get; set; }

       /// <summary>
       ///销售单据号
       /// </summary>
       [Display(Name ="销售单据号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string SOBillNo { get; set; }

       /// <summary>
       ///生产单据号
       /// </summary>
       [Display(Name ="生产单据号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MOBillNo { get; set; }

       /// <summary>
       ///计划跟踪号
       /// </summary>
       [Display(Name ="计划跟踪号")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string MtoNo { get; set; }

       /// <summary>
       ///产品编号
       /// </summary>
       [Display(Name ="产品编号")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string ProductNo { get; set; }

       /// <summary>
       ///位号
       /// </summary>
       [Display(Name ="位号")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string PositionNo { get; set; }

       /// <summary>
       ///当前工序
       /// </summary>
       [Display(Name ="当前工序")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string CurrentProcess { get; set; }

       /// <summary>
       ///创建人Id
       /// </summary>
       [Display(Name ="创建人Id")]
       [Column(TypeName="int")]
       public int? CreateID { get; set; }

       /// <summary>
       ///当前工序执行状态
       /// </summary>
       [Display(Name ="当前工序执行状态")]
       [MaxLength(51)]
       [Column(TypeName="nvarchar(51)")]
       [Editable(true)]
       public string CurrentProcessStatus { get; set; }

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
       ///工单编号
       /// </summary>
       [Display(Name ="工单编号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? WorkOrder_ID { get; set; }

       
    }
}