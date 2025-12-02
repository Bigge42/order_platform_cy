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
    [Entity(TableCnName = "预警规则表",TableName = "OCP_AlertRules",DBServer = "ServiceDbContext")]
    public partial class OCP_AlertRules:ServiceEntity
    {
        /// <summary>
       ///提前预警天数
       /// </summary>
       [Display(Name ="提前预警天数")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? advanceWarningDays { get; set; }

       /// <summary>
       ///完成状态字段
       /// </summary>
       [Display(Name ="完成状态字段")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string FinishStatusField { get; set; }

       /// <summary>
       ///完成判定方式
       /// </summary>
       [Display(Name ="完成判定方式")]
       [MaxLength(20)]
       [Column(TypeName="nvarchar(20)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ConditionType { get; set; }

       /// <summary>
       ///完成判定值
       /// </summary>
       [Display(Name ="完成判定值")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ConditionValue { get; set; }

       /// <summary>
       ///责任人（OA接受人）
       /// </summary>
       [Display(Name ="责任人（OA接受人）")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string ResponsiblePersonName { get; set; }

       /// <summary>
       ///责任人登录名
       /// </summary>
       [Display(Name ="责任人登录名")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       public string ResponsiblePersonLoginName { get; set; }

       /// <summary>
       ///推送周期
       /// </summary>
       [Display(Name ="推送周期")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string PushInterval { get; set; }

       /// <summary>
       ///是否触发OA流程
       /// </summary>
       [Display(Name ="是否触发OA流程")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int TriggerOA { get; set; }

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
       ///任务状态
       /// </summary>
       [Display(Name ="任务状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int TaskStatus { get; set; }

       /// <summary>
       ///主键ID
       /// </summary>
       [Key]
       [Display(Name ="主键ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long ID { get; set; }

       /// <summary>
       ///规则名称
       /// </summary>
       [Display(Name ="规则名称")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string RuleName { get; set; }

       /// <summary>
       ///预警页面
       /// </summary>
       [Display(Name ="预警页面")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string AlertPage { get; set; }

       /// <summary>
       ///字段名
       /// </summary>
       [Display(Name ="字段名")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string FieldName { get; set; }

       /// <summary>
       ///阈值天数
       /// </summary>
       [Display(Name ="阈值天数")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int DayCount { get; set; }

       
    }
}