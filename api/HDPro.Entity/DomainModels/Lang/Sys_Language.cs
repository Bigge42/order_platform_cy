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
    [Entity(TableCnName = "语言管理",TableName = "Sys_Language")]
    public partial class Sys_Language:SysEntity
    {
        /// <summary>
       ///
       /// </summary>
       [Key]
       [Display(Name ="Id")]
       [Column(TypeName="int")]
       [Required(AllowEmptyStrings=false)]
       public int Id { get; set; }

       /// <summary>
       ///简体中文
       /// </summary>
       [Display(Name ="简体中文")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ZHCN { get; set; }

       /// <summary>
       ///繁体中文
       /// </summary>
       [Display(Name ="繁体中文")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string ZHTW { get; set; }

       /// <summary>
       ///英语
       /// </summary>
       [Display(Name ="英语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string English { get; set; }

       /// <summary>
       ///法语
       /// </summary>
       [Display(Name ="法语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string French { get; set; }

       /// <summary>
       ///西班牙语
       /// </summary>
       [Display(Name ="西班牙语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Spanish { get; set; }

       /// <summary>
       ///俄语
       /// </summary>
       [Display(Name ="俄语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Russian { get; set; }

       /// <summary>
       ///阿拉伯语
       /// </summary>
       [Display(Name ="阿拉伯语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Arabic { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Module")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       public string Module { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="IsPackageContent")]
       [Column(TypeName="int")]
       public int? IsPackageContent { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CreateId")]
       [Column(TypeName="int")]
       public int? CreateId { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Creator")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       public string Creator { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ModifyId")]
       [Column(TypeName="int")]
       public int? ModifyId { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ModifyDate")]
       [Column(TypeName="datetime")]
       public DateTime? ModifyDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Modifier")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       public string Modifier { get; set; }

       
    }
}