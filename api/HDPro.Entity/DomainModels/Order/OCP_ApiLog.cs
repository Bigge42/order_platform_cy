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
    [Entity(TableCnName = "接口日志表",TableName = "OCP_ApiLog",DBServer = "ServiceDbContext")]
    public partial class OCP_ApiLog:ServiceEntity
    {
        /// <summary>
       ///主键ID
       /// </summary>
       [Key]
       [Display(Name ="主键ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long ID { get; set; }

       /// <summary>
       ///接口名称
       /// </summary>
       [Display(Name ="接口名称")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string ApiName { get; set; }

       /// <summary>
       ///接口路径
       /// </summary>
       [Display(Name ="接口路径")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string ApiPath { get; set; }

       /// <summary>
       ///请求方法
       /// </summary>
       [Display(Name ="请求方法")]
       [MaxLength(20)]
       [Column(TypeName="nvarchar(20)")]
       [Editable(true)]
       public string HttpMethod { get; set; }

       /// <summary>
       ///请求参数
       /// </summary>
       [Display(Name ="请求参数")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string RequestParams { get; set; }

       /// <summary>
       ///响应数据长度（字节数）
       /// </summary>
       [Display(Name ="响应数据长度（字节数）")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? ResponseLength { get; set; }

       /// <summary>
       ///HTTP状态码
       /// </summary>
       [Display(Name ="HTTP状态码")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? StatusCode { get; set; }

       /// <summary>
       ///调用状态
       /// </summary>
       [Display(Name ="调用状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Status { get; set; }

       /// <summary>
       ///错误信息
       /// </summary>
       [Display(Name ="错误信息")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string ErrorMessage { get; set; }

       /// <summary>
       ///调用开始时间
       /// </summary>
       [Display(Name ="调用开始时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? StartTime { get; set; }

       /// <summary>
       ///调用结束时间
       /// </summary>
       [Display(Name ="调用结束时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? EndTime { get; set; }

       /// <summary>
       ///耗时（毫秒）
       /// </summary>
       [Display(Name ="耗时（毫秒）")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? ElapsedMs { get; set; }

       /// <summary>
       ///返回数据条数
       /// </summary>
       [Display(Name ="返回数据条数")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? DataCount { get; set; }

       /// <summary>
       ///返回结果
       /// </summary>
       [Display(Name ="返回结果")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string ResponseResult { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///创建日期
       /// </summary>
       [Display(Name ="创建日期")]
       [Column(TypeName="datetime")]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///创建人ID
       /// </summary>
       [Display(Name ="创建人ID")]
       [Column(TypeName="int")]
       public int? CreateID { get; set; }

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