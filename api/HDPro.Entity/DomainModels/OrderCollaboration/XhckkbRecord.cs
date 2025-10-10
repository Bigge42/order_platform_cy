using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HDPro.Entity.SystemModels;

namespace HDPro.Entity.DomainModels.OrderCollaboration
{
    // 循环仓库看板记录实体
    [Entity(TableCnName = "循环仓库看板记录", TableName = "XhckkbRecord", DBServer = "ServiceDbContext")]
    public class XhckkbRecord : ServiceEntity
    {
        /// <summary>
        /// 接口数据明细ID
        /// </summary>
        [Display(Name = "明细ID")]
        [Column(TypeName = "bigint")]
        public long FENTRYID { get; set; }

        /// <summary>
        /// 接口单据编号
        /// </summary>
        [Display(Name = "单据编号")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string FBILLNO { get; set; }

        /// <summary>
        /// 仓库名称（示例字段）
        /// </summary>
        [Display(Name = "仓库名称")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string FSTOCKNAME { get; set; }

        /// <summary>
        /// 数量（示例字段）
        /// </summary>
        [Display(Name = "数量")]
        [DisplayFormat(DataFormatString = "18,6")]
        [Column(TypeName = "decimal(18,6)")]
        public decimal? FQTY { get; set; }

        /// <summary>
        /// 日期（示例字段）
        /// </summary>
        [Display(Name = "日期")]
        [Column(TypeName = "datetime")]
        public DateTime? FDATE { get; set; }

        // ... 根据接口文档补充其他字段属性 ...

        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Display(Name = "ID")]
        [Column(TypeName = "bigint")]
        [Required]
        public long ID { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Display(Name = "创建人ID")]
        [Column(TypeName = "int")]
        public int? CreateID { get; set; }

        /// <summary>
        /// 创建人姓名
        /// </summary>
        [Display(Name = "创建人")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string Creator { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Display(Name = "创建时间")]
        [Column(TypeName = "datetime")]
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 修改人ID
        /// </summary>
        [Display(Name = "修改人ID")]
        [Column(TypeName = "int")]
        public int? ModifyID { get; set; }

        /// <summary>
        /// 修改人姓名
        /// </summary>
        [Display(Name = "修改人")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string Modifier { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Display(Name = "修改时间")]
        [Column(TypeName = "datetime")]
        public DateTime? ModifyDate { get; set; }
    }
}
