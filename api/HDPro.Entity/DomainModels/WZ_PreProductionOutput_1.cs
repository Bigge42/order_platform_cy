using HDPro.Entity.SystemModels;
using HDPro.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace HDPro.Entity.DomainModels
{
    [Entity(TableCnName = "预排产输出汇总", TableName = "WZ_PreProductionOutput_1", DBServer = "ServiceDbContext")]
    public class WZ_PreProductionOutput_1 : ServiceEntity
    {
        [Key]
        [Display(Name = "ID")]
        [Column(TypeName = "int")]
        [Required]
        [Editable(true)]
        public int Id { get; set; }

        [Display(Name = "生产日期")]
        [Column(TypeName = "date")]
        [Required]
        [Editable(true)]
        public DateTime ProductionDate { get; set; }

        [Display(Name = "阀门类别")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        [Required]
        [Editable(true)]
        public string ValveCategory { get; set; }

        [Display(Name = "产线")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        [Required]
        [Editable(true)]
        public string ProductionLine { get; set; }

        [Display(Name = "数量")]
        [DisplayFormat(DataFormatString = "18,6")]
        [Column(TypeName = "decimal(18,6)")]
        [Required]
        [Editable(true)]
        public decimal Quantity { get; set; }
    }
}
