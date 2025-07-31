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
    [Entity(TableCnName = "催单-回复",TableName = "OCP_UrgeReply",DBServer = "ServiceDbContext")]
    public partial class OCP_UrgeReply:ServiceEntity
    {
        /// <summary>
       ///催单记录ID
       /// </summary>
       [Key]
       [Display(Name ="催单记录ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int UrgeRecordID { get; set; }

       /// <summary>
       ///订单计划ID
       /// </summary>
       [Display(Name ="订单计划ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int PlanID { get; set; }

       /// <summary>
       ///物料分类
       /// </summary>
       [Display(Name ="物料分类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MaterialCategory { get; set; }

       /// <summary>
       ///物料编码
       /// </summary>
       [Display(Name ="物料编码")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string MaterialCode { get; set; }

       /// <summary>
       ///催单ID
       /// </summary>
       [Display(Name ="催单ID")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string UrgeID { get; set; }

       /// <summary>
       ///催单主题
       /// </summary>
       [Display(Name ="催单主题")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string UrgeSubject { get; set; }

       /// <summary>
       ///业务类型
       /// </summary>
       [Display(Name ="业务类型")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string BusinessType { get; set; }

       /// <summary>
       ///默认负责人
       /// </summary>
       [Display(Name ="默认负责人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string DefaultOwner { get; set; }

       /// <summary>
       ///指定负责人
       /// </summary>
       [Display(Name ="指定负责人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string AssignedOwner { get; set; }

       /// <summary>
       ///紧急等级
       /// </summary>
       [Display(Name ="紧急等级")]
       [MaxLength(20)]
       [Column(TypeName="nvarchar(20)")]
       [Editable(true)]
       public string PriorityLevel { get; set; }

       /// <summary>
       ///回复时限（天）
       /// </summary>
       [Display(Name ="回复时限（天）")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ReplyDeadlineDays { get; set; }

       /// <summary>
       ///催单正文
       /// </summary>
       [Display(Name ="催单正文")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string UrgeContent { get; set; }

       /// <summary>
       ///自动生成标识
       /// </summary>
       [Display(Name ="自动生成标识")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int AutoGenerateFlag { get; set; }

       /// <summary>
       ///交货日期
       /// </summary>
       [Display(Name ="交货日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? DeliveryDate { get; set; }

       /// <summary>
       ///进度
       /// </summary>
       [Display(Name ="进度")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Progress { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string Remarks { get; set; }

       /// <summary>
       ///创建人Id
       /// </summary>
       [Display(Name ="创建人Id")]
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
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       public string Creator { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
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

       
    }
}