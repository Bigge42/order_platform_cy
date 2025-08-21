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
       ///
       /// </summary>
       [Key]
       [Display(Name ="uid")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int uid { get; set; }

       /// <summary>
       ///自定义列1
       /// </summary>
       [Display(Name ="自定义列1")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string udf1_key { get; set; }

       /// <summary>
       ///自定义值1
       /// </summary>
       [Display(Name ="自定义值1")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string udf1_value { get; set; }

       /// <summary>
       ///自定义列2
       /// </summary>
       [Display(Name ="自定义列2")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string udf2_key { get; set; }

       /// <summary>
       ///自定义值2
       /// </summary>
       [Display(Name ="自定义值2")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string udf2_value { get; set; }

       
    }
}