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
    [Entity(TableCnName = "备注拆分工具",TableName = "ORDER_NOTE_FLAT",DBServer = "ServiceDbContext")]
    public partial class ORDER_NOTE_FLAT:ServiceEntity
    {
        /// <summary>
       ///
       /// </summary>
       [Display(Name ="source_type")]
       [MaxLength(16)]
       [Column(TypeName="varchar(16)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string source_type { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Key]
       [Display(Name ="source_entry_id")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long source_entry_id { get; set; }

       /// <summary>
       ///销售合同号
       /// </summary>
       [Display(Name ="销售合同号")]
       [MaxLength(80)]
       [Column(TypeName="nvarchar(80)")]
       [Editable(true)]
       public string contract_no { get; set; }

       /// <summary>
       ///产品型号
       /// </summary>
       [Display(Name ="产品型号")]
       [MaxLength(120)]
       [Column(TypeName="nvarchar(120)")]
       [Editable(true)]
       public string product_model { get; set; }

       /// <summary>
       ///规格型号
       /// </summary>
       [Display(Name ="规格型号")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string specification { get; set; }

       /// <summary>
       ///计划跟踪号
       /// </summary>
       [Display(Name ="计划跟踪号")]
       [MaxLength(80)]
       [Column(TypeName="nvarchar(80)")]
       [Editable(true)]
       public string mto_no { get; set; }

       /// <summary>
       ///内部信息传递
       /// </summary>
       [Display(Name ="内部信息传递")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string internal_note { get; set; }

       /// <summary>
       ///选型ID
       /// </summary>
       [Display(Name ="选型ID")]
       [MaxLength(64)]
       [Column(TypeName="nvarchar(64)")]
       [Editable(true)]
       public string selection_guid { get; set; }

       /// <summary>
       ///原始备注
       /// </summary>
       [Display(Name ="原始备注")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string remark_raw { get; set; }

       /// <summary>
       ///阀体及执行机构装配备注
       /// </summary>
       [Display(Name ="阀体及执行机构装配备注")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string note_body_actuator { get; set; }

       /// <summary>
       ///附件安装及调试备注
       /// </summary>
       [Display(Name ="附件安装及调试备注")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string note_accessory_debug { get; set; }

       /// <summary>
       ///强压泄漏试验备注
       /// </summary>
       [Display(Name ="强压泄漏试验备注")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string note_pressure_leak { get; set; }

       /// <summary>
       ///装箱备注
       /// </summary>
       [Display(Name ="装箱备注")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string note_packing { get; set; }

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

       /// <summary>
       ///发生变更
       /// </summary>
       [Display(Name ="发生变更")]
       [Column(TypeName="bit")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public bool bz_changed { get; set; }

        /// <summary>
        /// 为 ORDER_NOTE_FLAT 扩展“修改人”字段
        /// </summary>
   
        /// <summary>
        /// 修改人（前端在 updateNoteDetails 时传入）
        /// </summary>
        public string modified_by { get; set; }
  
    }
}