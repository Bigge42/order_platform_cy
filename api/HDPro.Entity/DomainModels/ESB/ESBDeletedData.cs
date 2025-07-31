using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HDPro.Entity.DomainModels.ESB
{
    /// <summary>
    /// ESB删除数据查询请求
    /// </summary>
    public class ESBDeletedDataRequest
    {
        /// <summary>
        /// 数据类型
        /// DDGZ（订单跟踪）、BOMDJJD（BOM搭建进度）、DDJDCX（订单进度查询）、
        /// CGGZ（采购跟踪）、WWGZ（委外跟踪）、ZJGZ（整机跟踪）、BJGZ（部件跟踪）、JGGZ（金工跟踪）
        /// </summary>
        [JsonProperty("CLASS")]
        public string CLASS { get; set; }
    }

    /// <summary>
    /// ESB删除数据响应项
    /// </summary>
    public class ESBDeletedDataResponse
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [JsonProperty("FID")]
        public int FID { get; set; }

        /// <summary>
        /// 明细ID（部分业务类型使用）
        /// </summary>
        [JsonProperty("FENTRYID")]
        public int? FENTRYID { get; set; }
    }

    /// <summary>
    /// ESB删除数据错误响应
    /// </summary>
    public class ESBDeletedDataErrorResponse
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// 更新数量
        /// </summary>
        [JsonProperty("updatenum")]
        public string UpdateNum { get; set; }

        /// <summary>
        /// 插入数量
        /// </summary>
        [JsonProperty("insertnum")]
        public string InsertNum { get; set; }

        /// <summary>
        /// 错误数量
        /// </summary>
        [JsonProperty("Errornum")]
        public string ErrorNum { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        [JsonProperty("Msg")]
        public string Msg { get; set; }
    }

    /// <summary>
    /// 业务类型枚举
    /// </summary>
    public static class ESBDeletedDataBusinessType
    {
        /// <summary>
        /// 订单跟踪
        /// </summary>
        public const string DDGZ = "DDGZ";

        /// <summary>
        /// BOM搭建进度
        /// </summary>
        public const string BOMDJJD = "BOMDJJD";

        /// <summary>
        /// 订单进度查询
        /// </summary>
        public const string DDJDCX = "DDJDCX";

        /// <summary>
        /// 采购跟踪
        /// </summary>
        public const string CGGZ = "CGGZ";

        /// <summary>
        /// 委外跟踪
        /// </summary>
        public const string WWGZ = "WWGZ";

        /// <summary>
        /// 整机跟踪
        /// </summary>
        public const string ZJGZ = "ZJGZ";

        /// <summary>
        /// 部件跟踪
        /// </summary>
        public const string BJGZ = "BJGZ";

        /// <summary>
        /// 金工跟踪
        /// </summary>
        public const string JGGZ = "JGGZ";

        /// <summary>
        /// 获取所有支持的业务类型
        /// </summary>
        /// <returns>业务类型列表</returns>
        public static List<string> GetAllBusinessTypes()
        {
            return new List<string> { DDGZ, BOMDJJD, DDJDCX, CGGZ, WWGZ, ZJGZ, BJGZ, JGGZ };
        }

        /// <summary>
        /// 获取业务类型的中文名称
        /// </summary>
        /// <param name="businessType">业务类型代码</param>
        /// <returns>中文名称</returns>
        public static string GetBusinessTypeName(string businessType)
        {
            return businessType?.ToUpper() switch
            {
                DDGZ => "订单跟踪",
                BOMDJJD => "BOM搭建进度",
                DDJDCX => "订单进度查询",
                CGGZ => "采购跟踪",
                WWGZ => "委外跟踪",
                ZJGZ => "整机跟踪",
                BJGZ => "部件跟踪",
                JGGZ => "金工跟踪",
                _ => businessType
            };
        }

        /// <summary>
        /// 获取业务类型对应的主键字段说明
        /// </summary>
        /// <param name="businessType">业务类型代码</param>
        /// <returns>主键字段说明</returns>
        public static string GetPrimaryKeyDescription(string businessType)
        {
            return businessType?.ToUpper() switch
            {
                DDGZ => "销售明细FENTRYID",
                BOMDJJD => "销售明细FENTRYID",
                DDJDCX => "销售主表FID",
                CGGZ => "采购明细FENTRYID",
                WWGZ => "采购明细FENTRYID",
                ZJGZ => "生产订单明细FENTRYID",
                BJGZ => "生产订单明细FENTRYID",
                JGGZ => "生产订单明细FENTRYID",
                _ => "FID"
            };
        }

        /// <summary>
        /// 验证业务类型是否有效
        /// </summary>
        /// <param name="businessType">业务类型代码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidBusinessType(string businessType)
        {
            if (string.IsNullOrWhiteSpace(businessType))
                return false;

            return GetAllBusinessTypes().Contains(businessType.ToUpper());
        }
    }
}