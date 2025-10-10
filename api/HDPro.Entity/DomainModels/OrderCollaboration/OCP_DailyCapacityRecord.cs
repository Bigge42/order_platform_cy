using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HDPro.Entity.SystemModels;

namespace HDPro.Entity.DomainModels
{
    /// <summary>
    /// 每日产能记录实体
    /// </summary>
    [Entity(TableCnName = "每日产能记录", TableName = "OCP_DailyCapacityRecord", DBServer = "ServiceDbContext")]
    public partial class OCP_DailyCapacityRecord : ServiceEntity
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column(TypeName = "int")]
        public int ID { get; set; }

        /// <summary>
        /// 排产日期（天）
        /// </summary>
        [Display(Name = "排产日期")]
        [Column(TypeName = "datetime")]
        [Required]
        public DateTime ProductionDate { get; set; }

        /// <summary>
        /// 生产线
        /// </summary>
        [Display(Name = "生产线")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        [Required]
        public string ProductionLine { get; set; }

        /// <summary>
        /// 阀门大类
        /// </summary>
        [Display(Name = "阀门大类")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        [Required]
        public string ValveCategory { get; set; }

        /// <summary>
        /// 当日产量
        /// </summary>
        [Display(Name = "产量")]
        [Column(TypeName = "int")]
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Display(Name = "创建人Id")]
        [Column(TypeName = "int")]
        public int? CreateID { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [Display(Name = "创建日期")]
        [Column(TypeName = "datetime")]
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Display(Name = "创建人")]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string Creator { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Display(Name = "修改人")]
        [MaxLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string Modifier { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>
        [Display(Name = "修改日期")]
        [Column(TypeName = "datetime")]
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// 修改人ID
        /// </summary>
        [Display(Name = "修改人Id")]
        [Column(TypeName = "int")]
        public int? ModifyID { get; set; }
    }
}
