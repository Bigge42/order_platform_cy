/*
 * K3Cloud查询请求模型
 */
using System.Collections.Generic;

namespace HDPro.CY.Order.Services.K3Cloud.Models
{
    /// <summary>
    /// K3Cloud查询请求模型
    /// </summary>
    public class K3CloudQueryRequest
    {
        /// <summary>
        /// 查询参数列表
        /// </summary>
        public List<K3CloudQueryParameter> parameters { get; set; } = new List<K3CloudQueryParameter>();
    }

    /// <summary>
    /// K3Cloud查询参数
    /// </summary>
    public class K3CloudQueryParameter
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public string FormId { get; set; }

        /// <summary>
        /// 字段键值
        /// </summary>
        public string FieldKeys { get; set; }

        /// <summary>
        /// 过滤条件
        /// </summary>
        public string FilterString { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string OrderString { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// 起始行
        /// </summary>
        public int StartRow { get; set; }
    }
} 