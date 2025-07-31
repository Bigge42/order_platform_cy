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
    [Entity(TableCnName = "催单回复表",TableName = "OCP_UrgentOrderReply",DBServer = "ServiceDbContext")]
    public partial class OCP_UrgentOrderReply:ServiceEntity
    {
        /// <summary>
       ///回复ID
       /// </summary>
       [Key]
       [Display(Name ="回复ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long ReplyID { get; set; }

       /// <summary>
       ///催单ID
       /// </summary>
       [Display(Name ="催单ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long UrgentOrderID { get; set; }

       /// <summary>
       ///回复内容
       /// </summary>
       [Display(Name ="回复内容")]
       [MaxLength(1000)]
       [Column(TypeName="nvarchar(1000)")]
       [Editable(true)]
       public string ReplyContent { get; set; }

       /// <summary>
       ///回复人名称
       /// </summary>
       [Display(Name ="回复人名称")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ReplyPersonName { get; set; }

       /// <summary>
       ///回复人电话
       /// </summary>
       [Display(Name ="回复人电话")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ReplyPersonPhone { get; set; }

       /// <summary>
       ///回复时间
       /// </summary>
       [Display(Name ="回复时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ReplyTime { get; set; }

       /// <summary>
       ///回复进度
       /// </summary>
       [Display(Name ="回复进度")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string ReplyProgress { get; set; }

       /// <summary>
       ///回复交期
       /// </summary>
       [Display(Name ="回复交期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ReplyDeliveryDate { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string Remarks { get; set; }

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

       
    }
}