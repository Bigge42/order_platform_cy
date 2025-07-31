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
    [Entity(TableCnName = "单据过滤方案",TableName = "Sys_FilterPlan",DBServer = "SysDbContext")]
    public partial class Sys_FilterPlan:SysEntity
    {
        /// <summary>
       ///主建
       /// </summary>
       [Key]
       [Display(Name ="主建")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long ID { get; set; }

       /// <summary>
       ///单据名称
       /// </summary>
       [Display(Name ="单据名称")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BillName { get; set; }

       /// <summary>
       ///方案名称
       /// </summary>
       [Display(Name ="方案名称")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Name { get; set; }

       /// <summary>
       ///方案内容
       /// </summary>
       [Display(Name ="方案内容")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string Content { get; set; }

       /// <summary>
       ///用户
       /// </summary>
       [Display(Name ="用户")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string UserIds { get; set; }

       /// <summary>
       ///角色
       /// </summary>
       [Display(Name ="角色")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string RoleIds { get; set; }

       /// <summary>
       ///是否默认
       /// </summary>
       [Display(Name ="是否默认")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? IsDefault { get; set; }

       /// <summary>
       ///创建人Id
       /// </summary>
       [Display(Name ="创建人Id")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreateID { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(20)]
       [Column(TypeName="nvarchar(20)")]
       [Editable(true)]
       public string Creator { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(20)]
       [Column(TypeName="nvarchar(20)")]
       [Editable(true)]
       public string Modifier { get; set; }

       /// <summary>
       ///修改日期
       /// </summary>
       [Display(Name ="修改日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ModifyDate { get; set; }

       /// <summary>
       ///修改人Id
       /// </summary>
       [Display(Name ="修改人Id")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ModifyID { get; set; }

       /// <summary>
       ///系统内置
       /// </summary>
       [Display(Name ="系统内置")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? IsSystem { get; set; }

       
    }
}