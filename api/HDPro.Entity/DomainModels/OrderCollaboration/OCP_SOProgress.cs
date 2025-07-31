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
    [Entity(TableCnName = "销售订单进度查询",TableName = "OCP_SOProgress",DetailTable =  new Type[] { typeof(OCP_SOProgressDetail)},DetailTableCnName = "销售订单进度详情",DBServer = "ServiceDbContext")]
    public partial class OCP_SOProgress:ServiceEntity
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
       ///销售订单号
       /// </summary>
       [Display(Name ="销售订单号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BillNo { get; set; }

       /// <summary>
       ///订单日期
       /// </summary>
       [Display(Name ="订单日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? BillDate { get; set; }

       /// <summary>
       ///销售合同号
       /// </summary>
       [Display(Name ="销售合同号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string SalesContractNumber { get; set; }

       /// <summary>
       ///用户合同号
       /// </summary>
       [Display(Name ="用户合同号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string CustomerContractNumber { get; set; }

       /// <summary>
       ///合同类型
       /// </summary>
       [Display(Name ="合同类型")]
       [MaxLength(51)]
       [Column(TypeName="nvarchar(51)")]
       [Editable(true)]
       public string ContractType { get; set; }

       /// <summary>
       ///客户
       /// </summary>
       [Display(Name ="客户")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string Customer { get; set; }

       /// <summary>
       ///订单状态
       /// </summary>
       [Display(Name ="订单状态")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string BillStatus { get; set; }

       /// <summary>
       ///项目名称
       /// </summary>
       [Display(Name ="项目名称")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string ProjectName { get; set; }

       /// <summary>
       ///销售负责人
       /// </summary>
       [Display(Name ="销售负责人")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string SalesPerson { get; set; }

       /// <summary>
       ///数量
       /// </summary>
       [Display(Name ="数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? SaleQty { get; set; }

       /// <summary>
       ///入库数量
       /// </summary>
       [Display(Name ="入库数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? InstockQty { get; set; }

       /// <summary>
       ///待入库数量
       /// </summary>
       [Display(Name ="待入库数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? UnInstockQty { get; set; }

       /// <summary>
       ///出库数量
       /// </summary>
       [Display(Name ="出库数量")]
       [DisplayFormat(DataFormatString="18,6")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? OutStockQty { get; set; }

       /// <summary>
       ///创建人Id
       /// </summary>
       [Display(Name ="创建人Id")]
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
       ///修改人Id
       /// </summary>
       [Display(Name ="修改人Id")]
       [Column(TypeName="int")]
       public int? ModifyID { get; set; }

       /// <summary>
       ///销售订单主键
       /// </summary>
       [Display(Name ="销售订单主键")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? FID { get; set; }

       /// <summary>
       ///ESB修改日期
       /// </summary>
       [Display(Name ="ESB修改日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ESBModifyDate { get; set; }

       [Display(Name ="销售订单进度详情")]
       [ForeignKey("OrderID")]
       public List<OCP_SOProgressDetail> OCP_SOProgressDetail { get; set; }


       
    }
}