using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Entity.DomainModels.Core
{
    /// <summary>
    /// 代码生成参数
    /// </summary>
    public class GenerateCodeParam
    {
        /// <summary>
        /// 表名列表
        /// </summary>
        public List<string> TableNames { get; set; }

        /// <summary>
        /// 数据库服务名
        /// </summary>
        public string DBServer { get; set; }

        /// <summary>
        /// 基类名称
        /// </summary>
        public string BaseClassName { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// 文件夹名称
        /// </summary>
        public string FolderName { get; set; }

        /// <summary>
        /// Vue路径
        /// </summary>
        public string VuePath { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortName { get; set; } = "Id";

        /// <summary>
        /// 父菜单ID
        /// </summary>
        public int? ParentMenuId { get; set; }

        public int? ParentId { get; set; }
    }
}
