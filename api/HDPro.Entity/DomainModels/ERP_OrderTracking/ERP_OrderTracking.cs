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
    [Entity(TableCnName = "ERP直连表",TableName = "ERP_OrderTracking",DBServer = "ServiceDbContext")]
    public partial class ERP_OrderTracking:ServiceEntity
    {
        /// <summary>
       ///
       /// </summary>
       [Key]
       [Display(Name ="id")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long id { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="FENTRYID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long FENTRYID { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="FBILLNO")]
       [MaxLength(80)]
       [Column(TypeName="nvarchar(80)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string FBILLNO { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="FNUMBER")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string FNUMBER { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="FQTY")]
       [DisplayFormat(DataFormatString="18,4")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public decimal FQTY { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="FMTONO")]
       [MaxLength(80)]
       [Column(TypeName="nvarchar(80)")]
       [Editable(true)]
       public string FMTONO { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="FAPPROVEDATE")]
       [Column(TypeName="date")]
       [Editable(true)]
       public DateTime? FAPPROVEDATE { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="F_ORA_DATETIME")]
       [Column(TypeName="date")]
       [Editable(true)]
       public DateTime? F_ORA_DATETIME { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="F_BLN_HFJHRQ")]
       [Column(TypeName="date")]
       [Editable(true)]
       public DateTime? F_BLN_HFJHRQ { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="created_at")]
       [Column(TypeName="datetime2")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public DateTime created_at { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="updated_at")]
       [Column(TypeName="datetime2")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public DateTime updated_at { get; set; }

       
    }
}
