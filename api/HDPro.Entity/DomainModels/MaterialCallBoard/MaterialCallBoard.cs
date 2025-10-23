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
    [Entity(TableCnName = "附件库房发料看板",TableName = "MaterialCallBoard",DBServer = "ServiceDbContext")]
    public partial class MaterialCallBoard:ServiceEntity
    {
        /// <summary>
       ///
       /// </summary>
       [Key]
       [Display(Name ="WorkOrderNo")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string WorkOrderNo { get; set; }

       /// <summary>
       ///计划跟踪号
       /// </summary>
       [Display(Name ="计划跟踪号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string PlanTrackNo { get; set; }

       /// <summary>
       ///产品编号
       /// </summary>
       [Display(Name ="产品编号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ProductCode { get; set; }

       /// <summary>
       ///计划跟踪号
       /// </summary>
       [Display(Name ="计划跟踪号")]
       [MaxLength(64)]
       [Column(TypeName="nvarchar(64)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string CallerName { get; set; }

       /// <summary>
       ///叫料时间
       /// </summary>
       [Display(Name ="叫料时间")]
       [Column(TypeName="datetime2")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public DateTime CalledAt { get; set; }

       
    }
}