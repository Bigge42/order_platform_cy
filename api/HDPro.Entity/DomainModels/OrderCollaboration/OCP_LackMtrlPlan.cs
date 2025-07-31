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
    [Entity(TableCnName = "缺料运算方案",TableName = "OCP_LackMtrlPlan",DBServer = "ServiceDbContext")]
    public partial class OCP_LackMtrlPlan:ServiceEntity
    {
        /// <summary>
       ///运算ID
       /// </summary>
       [Key]
       [Display(Name ="运算ID")]
       [Column(TypeName="bigint")]
       [Required(AllowEmptyStrings=false)]
       public long ComputeID { get; set; }

       /// <summary>
       ///运算方案名
       /// </summary>
       [Display(Name ="运算方案名")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string PlanName { get; set; }

       /// <summary>
       ///运算条件
       /// </summary>
       [Display(Name ="运算条件")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string Filter { get; set; }

       /// <summary>
       ///创建人Id
       /// </summary>
       [Display(Name ="创建人Id")]
       [Column(TypeName="int")]
       public int? CreateID { get; set; }

       /// <summary>
       ///运算时间
       /// </summary>
       [Display(Name ="运算时间")]
       [Column(TypeName="datetime")]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///运算人
       /// </summary>
       [Display(Name ="运算人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
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
       ///是否默认方案
       /// </summary>
       [Display(Name ="是否默认方案")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? IsDefault { get; set; }

       
    }
}