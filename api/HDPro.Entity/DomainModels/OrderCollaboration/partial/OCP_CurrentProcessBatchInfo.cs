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
    
    public partial class OCP_CurrentProcessBatchInfo
    {

        /// <summary>
        /// 当前工序执行状态
        /// </summary>
        [Display(Name = "当前工序执行状态")]
        [MaxLength(20)]
        [Column(TypeName = "nvarchar(20)")]
        [Editable(true)]
        [NotMapped] // 如果数据库表中没有此字段，请保留此属性
        public string ProcessState { get; set; }
    }
}