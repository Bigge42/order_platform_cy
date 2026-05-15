using HDPro.Entity.SystemModels;
using HDPro.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace HDPro.Entity.DomainModels
{
    [Entity(TableCnName = "产线产能阈值", TableName = "WZ_ProductionOutputThreshold", DBServer = "ServiceDbContext")]
    public class WZ_ProductionOutputThreshold : ServiceEntity
    {
        [Key]
        [Display(Name = "ID")]
        [Column(TypeName = "int")]
        [Required]
        [Editable(true)]
        public int Id { get; set; }

        [Display(Name = "阀门类别")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        [Required]
        [Editable(true)]
        public string ValveCategory { get; set; } = string.Empty;

        [Display(Name = "产线")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        [Required]
        [Editable(true)]
        public string ProductionLine { get; set; } = string.Empty;

        [Display(Name = "当前阈值")]
        [DisplayFormat(DataFormatString = "18,6")]
        [Column(TypeName = "decimal(18,6)")]
        [Required]
        [Editable(true)]
        public decimal CurrentThreshold { get; set; }

        [Display(Name = "创建时间")]
        [Column(TypeName = "datetime")]
        [Editable(true)]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "更新时间")]
        [Column(TypeName = "datetime")]
        [Editable(true)]
        public DateTime? ModifyDate { get; set; }
    }
}
