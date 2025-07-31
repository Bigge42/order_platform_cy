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
    [Entity(TableCnName = "消息记录",TableName = "Sys_NotificationLog",DBServer = "SysDbContext")]
    public partial class Sys_NotificationLog:SysEntity
    {
        /// <summary>
       ///主键ID
       /// </summary>
       [Key]
       [Display(Name ="主键ID")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public Guid NotificationLogId { get; set; }

       /// <summary>
       ///消息ID
       /// </summary>
       [Display(Name ="消息ID")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? NotificationId { get; set; }

       /// <summary>
       ///模板ID
       /// </summary>
       [Display(Name ="模板ID")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? NotificationTemplateId { get; set; }

       /// <summary>
       ///通知标题
       /// </summary>
       [Display(Name ="通知标题")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string NotificationTitle { get; set; }

       /// <summary>
       ///业务功能
       /// </summary>
       [Display(Name ="业务功能")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string BusinessFunction { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="TableName")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string TableName { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="TableKey")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string TableKey { get; set; }

       /// <summary>
       ///通知内容
       /// </summary>
       [Display(Name ="通知内容")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string NotificationContent { get; set; }

       /// <summary>
       ///通知类型
       /// </summary>
       [Display(Name ="通知类型")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string NotificationType { get; set; }

       /// <summary>
       ///通知级别
       /// </summary>
       [Display(Name ="通知级别")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string NotificationLevel { get; set; }

       /// <summary>
       ///接收用户id
       /// </summary>
       [Display(Name ="接收用户id")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ReceiveUserId { get; set; }

       /// <summary>
       ///接收用户帐号
       /// </summary>
       [Display(Name ="接收用户帐号")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string ReceiveUserName { get; set; }

       /// <summary>
       ///接收人
       /// </summary>
       [Display(Name ="接收人")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string ReceiveUserTrueName { get; set; }

       /// <summary>
       ///跳转地址
       /// </summary>
       [Display(Name ="跳转地址")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string LinkUrl { get; set; }

       /// <summary>
       ///跳转类型
       /// </summary>
       [Display(Name ="跳转类型")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string LinkType { get; set; }

       /// <summary>
       ///读取状态
       /// </summary>
       [Display(Name ="读取状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int IsRead { get; set; }

       /// <summary>
       ///读取时间
       /// </summary>
       [Display(Name ="读取时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ReadDate { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Enable")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Enable { get; set; }

       /// <summary>
       ///创建人ID
       /// </summary>
       [Display(Name ="创建人ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreateID { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string Creator { get; set; }

       /// <summary>
       ///接收时间
       /// </summary>
       [Display(Name ="接收时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CreateDate { get; set; }

       
    }
}