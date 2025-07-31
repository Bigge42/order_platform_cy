using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.Enums;

namespace HDPro.Core.SignalR
{
    public class MessageNotification
    {

        /// <summary>
        ///消息ID
        /// </summary>
        [Display(Name = "消息ID")]
        [MaxLength(36)]
        [Column(TypeName = "uniqueidentifier")]
        [Editable(true)]
        public Guid? NotificationId { get; set; }


        /// <summary>
        ///业务功能
        /// </summary>
        [Display(Name = "业务功能")]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        [Editable(true)]
        public string BusinessFunction { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "TableName")]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        [Editable(true)]
        public string TableName { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "TableKey")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        public string TableKey { get; set; }

        /// <summary>
        ///通知标题
        /// </summary>
        [Display(Name = "通知标题")]
        [MaxLength(2000)]
        [Column(TypeName = "nvarchar(2000)")]
        [Editable(true)]
        public string Title { get; set; }

        /// <summary>
        ///通知内容
        /// </summary>
        [Display(Name = "通知内容")]
        [Column(TypeName = "nvarchar(max)")]
        [Editable(true)]
        public string Content { get; set; }

        /// <summary>
        ///通知类型
        /// </summary>
        [Display(Name = "通知类型")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        public NotificationType NotificationType { get; set; } = NotificationType.系统;

        /// <summary>
        ///通知级别
        /// </summary>
        [Display(Name = "通知级别")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        public string Level { get; set; }


        /// <summary>
        ///接收用户id
        /// </summary>
        [Display(Name = "接收用户id")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? ReceiveUserId { get; set; }

        /// <summary>
        ///接收用户
        /// </summary>
        [Display(Name = "接收用户")]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        [Editable(true)]
        public string ReceiveUserName { get; set; }


        /// <summary>
        ///跳转地址
        /// </summary>
        [Display(Name = "跳转地址")]
        [MaxLength(255)]
        [Column(TypeName = "nvarchar(255)")]
        [Editable(true)]
        public string LinkUrl { get; set; }

        /// <summary>
        ///跳转类型
        /// </summary>
        [Display(Name = "跳转类型")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        public string LinkType { get; set; }

        /// <summary>
        ///备注
        /// </summary>
        [Display(Name = "备注")]
        [MaxLength(255)]
        [Column(TypeName = "nvarchar(255)")]
        [Editable(true)]
        public string Remark { get; set; }

        /// <summary>
        ///创建人ID
        /// </summary>
        [Display(Name = "发送人ID")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? CreateID { get; set; }

        /// <summary>
        ///创建人
        /// </summary>
        [Display(Name = "发送人")]
        [MaxLength(255)]
        [Column(TypeName = "nvarchar(255)")]
        [Editable(true)]
        public string Creator { get; set; }


    }
}
