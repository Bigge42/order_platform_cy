using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace HDPro.Core.Enums
{
    /// <summary>
    /// 查询操作符类型枚举
    /// </summary>
    public enum QueryOperatorType
    {
        /// <summary>
        /// 等于
        /// </summary>
        [Description("等于")]
        Equal = 1,

        /// <summary>
        /// 不等于
        /// </summary>
        [Description("不等于")]
        NotEqual = 2,

        /// <summary>
        /// 模糊查询(包含)
        /// </summary>
        [Description("模糊查询(包含)")]
        Like = 3,

        /// <summary>
        /// 模糊查询(左包含)
        /// </summary>
        [Description("模糊查询(左包含)")]
        LikeStart = 4,

        /// <summary>
        /// 模糊查询(右包含)
        /// </summary>
        [Description("模糊查询(右包含)")]
        LikeEnd = 5,

        /// <summary>
        /// textarea
        /// </summary>
        [Description("textarea")]
        Textarea = 6,

        /// <summary>
        /// switch
        /// </summary>
        [Description("switch")]
        Switch = 7,

        /// <summary>
        /// select
        /// </summary>
        [Description("select")]
        Select = 8,

        /// <summary>
        /// select多选
        /// </summary>
        [Description("select多选")]
        SelectList = 9,

        /// <summary>
        /// 年
        /// </summary>
        [Description("年")]
        Year = 10,

        /// <summary>
        /// date(年月日)
        /// </summary>
        [Description("date(年月日)")]
        Date = 11,

        /// <summary>
        /// datetime(年月日时分秒)
        /// </summary>
        [Description("datetime(年月日时分秒)")]
        DateTime = 12,

        /// <summary>
        /// year_month
        /// </summary>
        [Description("year_month")]
        Month = 13,

        /// <summary>
        /// time
        /// </summary>
        [Description("time")]
        Time = 14,

        /// <summary>
        /// 级联
        /// </summary>
        [Description("级联")]
        Cascader = 15,

        /// <summary>
        /// 树形级联tree-select
        /// </summary>
        [Description("树形级联tree-select")]
        TreeSelect = 16,

        /// <summary>
        /// 下拉框Table搜索
        /// </summary>
        [Description("下拉框Table搜索")]
        SelectTable = 17,

        /// <summary>
        /// checkbox
        /// </summary>
        [Description("checkbox")]
        Checkbox = 18,

        /// <summary>
        /// radio
        /// </summary>
        [Description("radio")]
        Radio = 19,

        /// <summary>
        /// 区间查询
        /// </summary>
        [Description("区间查询")]
        Range = 20,

        /// <summary>
        /// mail
        /// </summary>
        [Description("mail")]
        Mail = 21,

        /// <summary>
        /// number
        /// </summary>
        [Description("number")]
        Number = 22,

        /// <summary>
        /// decimal
        /// </summary>
        [Description("decimal")]
        Decimal = 23,

        /// <summary>
        /// 批量查询
        /// </summary>
        [Description("批量查询")]
        MultipleInput = 24,

        /// <summary>
        /// 空
        /// </summary>
        [Description("空")]
        Empty = 25,

        /// <summary>
        /// 非空
        /// </summary>
        [Description("非空")]
        NotEmpty = 26
    }

    /// <summary>
    /// 查询操作符类型扩展方法
    /// </summary>
    public static class QueryOperatorTypeExtensions
    {
        /// <summary>
        /// 获取查询操作符的键值对列表
        /// </summary>
        /// <returns>键值对列表</returns>
        public static List<KeyValuePair<string, string>> GetQueryOperatorList()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("=", "等于"),
                new KeyValuePair<string, string>("!=", "不等于"),
                new KeyValuePair<string, string>("like", "模糊查询(包含)"),
                new KeyValuePair<string, string>("likeStart", "模糊查询(左包含)"),
                new KeyValuePair<string, string>("likeEnd", "模糊查询(右包含)"),
                new KeyValuePair<string, string>("textarea", "textarea"),
                new KeyValuePair<string, string>("switch", "switch"),
                new KeyValuePair<string, string>("select", "select"),
                new KeyValuePair<string, string>("selectList", "select多选"),
                new KeyValuePair<string, string>("year", "年"),
                new KeyValuePair<string, string>("date", "date(年月日)"),
                new KeyValuePair<string, string>("datetime", "datetime(年月日时分秒)"),
                new KeyValuePair<string, string>("month", "year_month"),
                new KeyValuePair<string, string>("time", "time"),
                new KeyValuePair<string, string>("cascader", "级联"),
                new KeyValuePair<string, string>("treeSelect", "树形级联tree-select"),
                new KeyValuePair<string, string>("selectTable", "下拉框Table搜索"),
                new KeyValuePair<string, string>("checkbox", "checkbox"),
                new KeyValuePair<string, string>("radio", "radio"),
                new KeyValuePair<string, string>("range", "区间查询"),
                new KeyValuePair<string, string>("mail", "mail"),
                new KeyValuePair<string, string>("number", "number"),
                new KeyValuePair<string, string>("decimal", "decimal"),
                new KeyValuePair<string, string>("multipleInput", "批量查询"),
                new KeyValuePair<string, string>("EMPTY", "空"),
                new KeyValuePair<string, string>("NOT_EMPTY", "非空")
            };
        }

        /// <summary>
        /// 获取查询操作符的字典
        /// </summary>
        /// <returns>字典</returns>
        public static Dictionary<string, string> GetQueryOperatorDictionary()
        {
            return GetQueryOperatorList().ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// 根据键获取值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public static string GetValueByKey(string key)
        {
            var dict = GetQueryOperatorDictionary();
            return dict.ContainsKey(key) ? dict[key] : string.Empty;
        }

        /// <summary>
        /// 根据值获取键
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>键</returns>
        public static string GetKeyByValue(string value)
        {
            var list = GetQueryOperatorList();
            var item = list.FirstOrDefault(x => x.Value == value);
            return item.Key ?? string.Empty;
        }

        /// <summary>
        /// 获取枚举的描述信息
        /// </summary>
        /// <param name="enumValue">枚举值</param>
        /// <returns>描述信息</returns>
        public static string GetDescription(this QueryOperatorType enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            return attribute?.Description ?? enumValue.ToString();
        }

        /// <summary>
        /// 根据枚举值获取对应的键
        /// </summary>
        /// <param name="enumValue">枚举值</param>
        /// <returns>键</returns>
        public static string GetKey(this QueryOperatorType enumValue)
        {
            var keyMap = new Dictionary<QueryOperatorType, string>
            {
                { QueryOperatorType.Equal, "=" },
                { QueryOperatorType.NotEqual, "!=" },
                { QueryOperatorType.Like, "like" },
                { QueryOperatorType.LikeStart, "likeStart" },
                { QueryOperatorType.LikeEnd, "likeEnd" },
                { QueryOperatorType.Textarea, "textarea" },
                { QueryOperatorType.Switch, "switch" },
                { QueryOperatorType.Select, "select" },
                { QueryOperatorType.SelectList, "selectList" },
                { QueryOperatorType.Year, "year" },
                { QueryOperatorType.Date, "date" },
                { QueryOperatorType.DateTime, "datetime" },
                { QueryOperatorType.Month, "month" },
                { QueryOperatorType.Time, "time" },
                { QueryOperatorType.Cascader, "cascader" },
                { QueryOperatorType.TreeSelect, "treeSelect" },
                { QueryOperatorType.SelectTable, "selectTable" },
                { QueryOperatorType.Checkbox, "checkbox" },
                { QueryOperatorType.Radio, "radio" },
                { QueryOperatorType.Range, "range" },
                { QueryOperatorType.Mail, "mail" },
                { QueryOperatorType.Number, "number" },
                { QueryOperatorType.Decimal, "decimal" },
                { QueryOperatorType.MultipleInput, "multipleInput" },
                { QueryOperatorType.Empty, "EMPTY" },
                { QueryOperatorType.NotEmpty, "NOT_EMPTY" }
            };

            return keyMap.ContainsKey(enumValue) ? keyMap[enumValue] : string.Empty;
        }

        /// <summary>
        /// 根据键获取枚举值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>枚举值</returns>
        public static QueryOperatorType? GetEnumByKey(string key)
        {
            var keyMap = new Dictionary<string, QueryOperatorType>
            {
                { "=", QueryOperatorType.Equal },
                { "!=", QueryOperatorType.NotEqual },
                { "like", QueryOperatorType.Like },
                { "likeStart", QueryOperatorType.LikeStart },
                { "likeEnd", QueryOperatorType.LikeEnd },
                { "textarea", QueryOperatorType.Textarea },
                { "switch", QueryOperatorType.Switch },
                { "select", QueryOperatorType.Select },
                { "selectList", QueryOperatorType.SelectList },
                { "year", QueryOperatorType.Year },
                { "date", QueryOperatorType.Date },
                { "datetime", QueryOperatorType.DateTime },
                { "month", QueryOperatorType.Month },
                { "time", QueryOperatorType.Time },
                { "cascader", QueryOperatorType.Cascader },
                { "treeSelect", QueryOperatorType.TreeSelect },
                { "selectTable", QueryOperatorType.SelectTable },
                { "checkbox", QueryOperatorType.Checkbox },
                { "radio", QueryOperatorType.Radio },
                { "range", QueryOperatorType.Range },
                { "mail", QueryOperatorType.Mail },
                { "number", QueryOperatorType.Number },
                { "decimal", QueryOperatorType.Decimal },
                { "multipleInput", QueryOperatorType.MultipleInput },
                { "EMPTY", QueryOperatorType.Empty },
                { "NOT_EMPTY", QueryOperatorType.NotEmpty }
            };

            return keyMap.ContainsKey(key) ? keyMap[key] : (QueryOperatorType?)null;
        }
    }
} 