using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HDPro.Entity.SystemModels;

namespace HDPro.Entity.DomainModels.OrderCollaboration
{
    /// <summary>
    /// 循环仓库看板记录实体（与表 XhckkbRecord 完全对应）
    /// </summary>
    [Entity(TableCnName = "循环仓库看板记录", TableName = "XhckkbRecord", DBServer = "ServiceDbContext")]
    public class XhckkbRecord : ServiceEntity
    {
        /// <summary>订单明细主键</summary>
        [Key]
        [Display(Name = "订单明细主键")]
        [Column("FENTRYID", TypeName = "int")]
        [Required]
        public int FENTRYID { get; set; }

        [Display(Name = "单据编号")]
        [MaxLength(50)]
        [Column("FBILLNO", TypeName = "nvarchar(50)")]
        public string FBILLNO { get; set; }

        [Display(Name = "单据审核状态")]
        [MaxLength(50)]
        [Column("FDOCUMENTSTATUSNAME", TypeName = "nvarchar(50)")]
        public string FDOCUMENTSTATUSNAME { get; set; }

        [Display(Name = "物资大类")]
        [MaxLength(50)]
        [Column("F_ORA_LB", TypeName = "nvarchar(50)")]
        public string F_ORA_LB { get; set; }

        [Display(Name = "物料ID")]
        [Column("FMATERIALID", TypeName = "int")]
        public int? FMATERIALID { get; set; }

        [Display(Name = "一次排数量")]
        [Column("F_ORA_INTEGER", TypeName = "int")]
        public int? F_ORA_INTEGER { get; set; }

        [Display(Name = "行号")]
        [Column("FSeq", TypeName = "int")]
        public int? FSeq { get; set; }

        [Display(Name = "最低库存")]
        [Column("F_ORA_INTEGER1", TypeName = "int")]
        public int? F_ORA_INTEGER1 { get; set; }

        [Display(Name = "最高库存")]
        [Column("F_ORA_INTEGER2", TypeName = "int")]
        public int? F_ORA_INTEGER2 { get; set; }

        [Display(Name = "物资小类")]
        [MaxLength(50)]
        [Column("F_ORA_WZXL", TypeName = "nvarchar(50)")]
        public string F_ORA_WZXL { get; set; }

        [Display(Name = "负责人")]
        [MaxLength(50)]
        [Column("F_ORA_TEXT1", TypeName = "nvarchar(50)")]
        public string F_ORA_TEXT1 { get; set; }

        [Display(Name = "建议SP")]
        [MaxLength(50)]
        [Column("F_ORA_JJSP", TypeName = "nvarchar(50)")]
        public string F_ORA_JJSP { get; set; }

        [Display(Name = "管理编码")]
        [MaxLength(50)]
        [Column("F_ORA_TEXT3", TypeName = "nvarchar(50)")]
        public string F_ORA_TEXT3 { get; set; }

        [Display(Name = "库存策略")]
        [MaxLength(50)]
        [Column("F_ORA_TEXT4", TypeName = "nvarchar(50)")]
        public string F_ORA_TEXT4 { get; set; }

        [Display(Name = "供方库存")]
        [MaxLength(50)]
        [Column("F_ORA_TEXT5", TypeName = "nvarchar(50)")]
        public string F_ORA_TEXT5 { get; set; }

        [Display(Name = "供货周期")]
        [MaxLength(50)]
        [Column("F_ORA_TEXT6", TypeName = "nvarchar(50)")]
        public string F_ORA_TEXT6 { get; set; }

        [Display(Name = "年预计用量")]
        [MaxLength(50)]
        [Column("F_ORA_TEXT7", TypeName = "nvarchar(50)")]
        public string F_ORA_TEXT7 { get; set; }

        [Display(Name = "产品大类")]
        [MaxLength(50)]
        [Column("F_ORA_TEXT8", TypeName = "nvarchar(50)")]
        public string F_ORA_TEXT8 { get; set; }

        [Display(Name = "产品小类")]
        [MaxLength(50)]
        [Column("F_ORA_TEXT9", TypeName = "nvarchar(50)")]
        public string F_ORA_TEXT9 { get; set; }

        // —— 以下为审计字段，可保留以兼容 ServiceEntity 审计体系 ——
        [Display(Name = "创建人ID")]
        [Column(TypeName = "int")]
        public int? CreateID { get; set; }

        [Display(Name = "创建人")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string Creator { get; set; }

        [Display(Name = "创建时间")]
        [Column(TypeName = "datetime")]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "修改人ID")]
        [Column(TypeName = "int")]
        public int? ModifyID { get; set; }

        [Display(Name = "修改人")]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string Modifier { get; set; }

        [Display(Name = "修改时间")]
        [Column(TypeName = "datetime")]
        public DateTime? ModifyDate { get; set; }
    }
}
