/*
 * BOM展开相关模型
 */
using System.Collections.Generic;

namespace HDPro.CY.Order.Services.K3Cloud.Models
{
    /// <summary>
    /// BOM展开请求DTO
    /// </summary>
    public class BomExpandRequestDto
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialNumber { get; set; }
    }

    /// <summary>
    /// BOM展开项DTO
    /// </summary>
    public class BomExpandItemDto
    {
        /// <summary>
        /// BOM层级(0表示顶层)
        /// </summary>
        public int BomLevel { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 分子(用量计算)
        /// </summary>
        public decimal Numerator { get; set; }

        /// <summary>
        /// 分母(用量计算)
        /// </summary>
        public decimal Denominator { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specification { get; set; }

        /// <summary>
        /// 父级分录ID
        /// </summary>
        public string ParentEntryId { get; set; }

        /// <summary>
        /// 分录ID
        /// </summary>
        public string EntryId { get; set; }

        /// <summary>
        /// 单位编码
        /// </summary>
        public string UnitNumber { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }
    }

    /// <summary>
    /// 服务结果
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ServiceResult<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static ServiceResult<T> Success(T data, string message = "操作成功")
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static ServiceResult<T> Failure(string message)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Message = message,
                Data = default(T)
            };
        }
    }
}

