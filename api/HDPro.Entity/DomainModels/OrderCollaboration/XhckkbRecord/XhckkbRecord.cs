using Newtonsoft.Json;
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
    [Entity(TableCnName = "循环仓库看板",TableName = "XhckkbRecord",DBServer = "ServiceDbContext")]
    public partial class XhckkbRecord:ServiceEntity
    {
        /// <summary>
       ///订单明细主键
       /// </summary>
       [Key]
       [Display(Name ="订单明细主键")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int FENTRYID { get; set; }

       /// <summary>
       ///单据编号
       /// </summary>
       [Display(Name ="单据编号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string FBILLNO { get; set; }

       /// <summary>
       ///单据审核状态
       /// </summary>
       [Display(Name ="单据审核状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string FDOCUMENTSTATUSNAME { get; set; }

       /// <summary>
       ///物资大类
       /// </summary>
       [Display(Name ="物资大类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string F_ORA_LB { get; set; }

       /// <summary>
       ///物料ID
       /// </summary>
       [Display(Name ="物料ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? FMATERIALID { get; set; }

       /// <summary>
       ///一次排的数量
       /// </summary>
       [Display(Name ="一次排的数量")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? F_ORA_INTEGER { get; set; }

       /// <summary>
       ///行号
       /// </summary>
       [Display(Name ="行号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? FSeq { get; set; }

       /// <summary>
       ///最低库存
       /// </summary>
       [Display(Name ="最低库存")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? F_ORA_INTEGER1 { get; set; }

       /// <summary>
       ///最高库存
       /// </summary>
       [Display(Name ="最高库存")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? F_ORA_INTEGER2 { get; set; }

       /// <summary>
       ///物资小类
       /// </summary>
       [Display(Name ="物资小类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string F_ORA_WZXL { get; set; }

       /// <summary>
       ///负责人
       /// </summary>
       [Display(Name ="负责人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string F_ORA_TEXT1 { get; set; }

       /// <summary>
       ///建议SP
       /// </summary>
       [Display(Name ="建议SP")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string F_ORA_JJSP { get; set; }

       /// <summary>
       ///管理编码
       /// </summary>
       [Display(Name ="管理编码")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string F_ORA_TEXT3 { get; set; }

       /// <summary>
       ///库存策略
       /// </summary>
       [Display(Name ="库存策略")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string F_ORA_TEXT4 { get; set; }

       /// <summary>
       ///供方库存
       /// </summary>
       [Display(Name ="供方库存")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string F_ORA_TEXT5 { get; set; }

       /// <summary>
       ///供货周期
       /// </summary>
       [Display(Name ="供货周期")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string F_ORA_TEXT6 { get; set; }

       /// <summary>
       ///年预计用量
       /// </summary>
       [Display(Name ="年预计用量")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string F_ORA_TEXT7 { get; set; }

       /// <summary>
       ///产品大类
       /// </summary>
       [Display(Name ="产品大类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string F_ORA_TEXT8 { get; set; }

       /// <summary>
       ///产品小类
       /// </summary>
       [Display(Name ="产品小类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string F_ORA_TEXT9 { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CreateDate")]
       [MaxLength(50)]
       [JsonIgnore]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string CreateDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CreateID")]
       [MaxLength(50)]
       [JsonIgnore]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string CreateID { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Creator")]
       [MaxLength(50)]
       [JsonIgnore]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Creator { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Modifier")]
       [MaxLength(50)]
       [JsonIgnore]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Modifier { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ModifyDate")]
       [MaxLength(50)]
       [JsonIgnore]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ModifyDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ModifyID")]
       [MaxLength(50)]
       [JsonIgnore]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ModifyID { get; set; }

       
    }
}
