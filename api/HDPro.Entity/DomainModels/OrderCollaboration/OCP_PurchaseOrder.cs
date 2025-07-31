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
    [Entity(TableCnName = "采购订单表",TableName = "OCP_PurchaseOrder",DetailTable =  new Type[] { typeof(OCP_PurchaseOrderDetail)},DetailTableCnName = "采购订单明细表",DBServer = "ServiceDbContext")]
    public partial class OCP_PurchaseOrder:ServiceEntity
    {
        /// <summary>
       ///订单ID
       /// </summary>
       [Key]
       [Display(Name ="订单ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long OrderID { get; set; }

       /// <summary>
       ///单据编号
       /// </summary>
       [Display(Name ="单据编号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BillNo { get; set; }

       /// <summary>
       ///采购订单ID
       /// </summary>
       [Display(Name ="采购订单ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? FID { get; set; }

       /// <summary>
       ///任务时间月
       /// </summary>
       [Display(Name ="任务时间月")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string PlanTaskMonth { get; set; }

       /// <summary>
       ///任务时间周
       /// </summary>
       [Display(Name ="任务时间周")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string PlanTaskWeek { get; set; }

       /// <summary>
       ///紧急等级
       /// </summary>
       [Display(Name ="紧急等级")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string Urgency { get; set; }

       /// <summary>
       ///业务类型
       /// </summary>
       [Display(Name ="业务类型")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BusinessType { get; set; }

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
       ///供应商名称
       /// </summary>
       [Display(Name ="供应商名称")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string SupplierName { get; set; }

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
       ///采购负责人
       /// </summary>
       [Display(Name ="采购负责人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string PurchasePerson { get; set; }

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

       [Display(Name ="采购订单明细表")]
       [ForeignKey("OrderID")]
       public List<OCP_PurchaseOrderDetail> OCP_PurchaseOrderDetail { get; set; }


       
    }
}