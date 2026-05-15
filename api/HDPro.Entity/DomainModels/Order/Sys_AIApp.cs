using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HDPro.Entity.SystemModels;

namespace HDPro.Entity.DomainModels
{
    [Entity(TableCnName = "AI小助手应用管理", TableName = "Sys_AIApp", DBServer = "ServiceDbContext")]
    public partial class Sys_AIApp : ServiceEntity
    {
        [Key]
        [Display(Name = "主键ID")]
        [Column(TypeName = "bigint")]
        [Required(AllowEmptyStrings = false)]
        public long Id { get; set; }

        [Display(Name = "应用名称")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string AppName { get; set; }

        [Display(Name = "应用类型")]
        [MaxLength(20)]
        [Column(TypeName = "nvarchar(20)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string AppType { get; set; }

        [Display(Name = "平台AppID")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string PlatformAppId { get; set; }

        [Display(Name = "平台AppKey")]
        [MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string PlatformAppKey { get; set; }

        [Display(Name = "应用描述")]
        [MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        [Editable(true)]
        public string Description { get; set; }

        [Display(Name = "应用图标")]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        [Editable(true)]
        public string Icon { get; set; }

        [Display(Name = "排序号")]
        [Column(TypeName = "int")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public int SortNo { get; set; }

        [Display(Name = "状态")]
        [Column(TypeName = "int")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public int Status { get; set; }

        [Display(Name = "备注")]
        [MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        [Editable(true)]
        public string Remark { get; set; }

        [Display(Name = "创建时间")]
        [Column(TypeName = "datetime")]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "创建人ID")]
        [Column(TypeName = "int")]
        public int? CreateID { get; set; }

        [Display(Name = "创建人")]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string Creator { get; set; }

        [Display(Name = "修改人")]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string Modifier { get; set; }

        [Display(Name = "修改时间")]
        [Column(TypeName = "datetime")]
        public DateTime? ModifyDate { get; set; }

        [Display(Name = "修改人ID")]
        [Column(TypeName = "int")]
        public int? ModifyID { get; set; }
    }
}
