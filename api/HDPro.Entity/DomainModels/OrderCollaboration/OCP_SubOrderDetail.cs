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
    [Entity(TableCnName = "委外订单明细表",TableName = "OCP_SubOrderDetail",DBServer = "ServiceDbContext")]
    public partial class OCP_SubOrderDetail:ServiceEntity
    {
        /// <summary>
       ///明细ID
       /// </summary>
       [Key]
       [Display(Name ="明细ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long DetailID { get; set; }

       /// <summary>
       ///主表ID
       /// </summary>
       [Display(Name ="主表ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? ID { get; set; }

       /// <summary>
       ///委外订单ID
       /// </summary>
       [Display(Name ="委外订单ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? FID { get; set; }

       /// <summary>
       ///委外订单明细ID
       /// </summary>
       [Display(Name ="委外订单明细ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? FENTRYID { get; set; }

       /// <summary>
       ///行号
       /// </summary>
       [Display(Name ="行号")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Seq { get; set; }

       /// <summary>
       ///计划跟踪号
       /// </summary>
       [Display(Name ="计划跟踪号")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string MtoNo { get; set; }

       /// <summary>
       ///物料ID
       /// </summary>
       [Display(Name ="物料ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? MaterialID { get; set; }

       /// <summary>
       ///物料编码
       /// </summary>
       [Display(Name ="物料编码")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string MaterialNumber { get; set; }

       /// <summary>
       ///物料名称
       /// </summary>
       [Display(Name ="物料名称")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string MaterialName { get; set; }

       /// <summary>
       ///物料类型
       /// </summary>
       [Display(Name ="物料类型")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string MaterialType { get; set; }

       /// <summary>
       ///供应商ID
       /// </summary>
       [Display(Name ="供应商ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? SupplierID { get; set; }

       /// <summary>
       ///供应商编码
       /// </summary>
       [Display(Name ="供应商编码")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string SupplierCode { get; set; }

       /// <summary>
       ///送货单号
       /// </summary>
       [Display(Name ="送货单号")]
       [MaxLength(1000)]
       [Column(TypeName="nvarchar(1000)")]
       [Editable(true)]
       public string DeliveryNo { get; set; }

       /// <summary>
       ///采购数量
       /// </summary>
       [Display(Name ="采购数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? PurchaseQty { get; set; }

       /// <summary>
       ///入库数量
       /// </summary>
       [Display(Name ="入库数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? InstockQty { get; set; }

       /// <summary>
       ///未完数量
       /// </summary>
       [Display(Name ="未完数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? UnfinishedQty { get; set; }

       /// <summary>
       ///超期数量
       /// </summary>
       [Display(Name ="超期数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? OverdueQty { get; set; }

       /// <summary>
       ///超期时长
       /// </summary>
       [Display(Name ="超期时长")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? OverdueDays { get; set; }

       /// <summary>
       ///领料日期
       /// </summary>
       [Display(Name ="领料日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? PickDate { get; set; }

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