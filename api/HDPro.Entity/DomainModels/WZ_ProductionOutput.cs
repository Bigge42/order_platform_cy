using HDPro.Entity.SystemModels;
using HDPro.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

[Entity(TableCnName = "产线产量", TableName = "WZ_ProductionOutput", DBServer = "ServiceDbContext")]
public class WZ_ProductionOutput : ServiceEntity
{
    [Key]
    [Display(Name = "ID")]
    [Column(TypeName = "int")]
    [Required]
    [Editable(true)]
    public int Id { get; set; }

    [Display(Name = "日期")]
    [Column(TypeName = "datetime")]  // 或者使用 date 类型  
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

    [Display(Name = "公称通径")]
    [MaxLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    [Editable(true)]
    public string NominalDiameter { get; set; }

    [Display(Name = "规则直判产线")]
    [MaxLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    [Editable(true)]
    public string AssignedProductionLine { get; set; }

    [Display(Name = "数量")]
    [DisplayFormat(DataFormatString = "18,6")]  // 数量精度, 对应 decimal(18,6)  
    [Column(TypeName = "decimal")]
    [Required]
    [Editable(true)]
    public decimal Quantity { get; set; }
}
