/*
 * ESB日志工厂
 * 提供统一的ESB日志创建服务
 */
using Microsoft.Extensions.Logging;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB
{
    /// <summary>
    /// ESB日志工厂
    /// 提供统一的ESB日志创建服务，简化各ESB服务的日志记录器创建
    /// </summary>
    public static class ESBLoggerFactory
    {
        /// <summary>
        /// 创建订单跟踪ESB日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>ESB日志记录器</returns>
        public static ESBLogger CreateOrderTrackingLogger(ILoggerFactory loggerFactory)
        {
            return new ESBLogger(loggerFactory, "订单跟踪");
        }

        /// <summary>
        /// 创建缺料运算结果ESB日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>ESB日志记录器</returns>
        public static ESBLogger CreateLackMaterialLogger(ILoggerFactory loggerFactory)
        {
            return new ESBLogger(loggerFactory, "缺料运算");
        }

        /// <summary>
        /// 创建采购订单ESB日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>ESB日志记录器</returns>
        public static ESBLogger CreatePurchaseOrderLogger(ILoggerFactory loggerFactory)
        {
            return new ESBLogger(loggerFactory, "采购订单");
        }

        /// <summary>
        /// 创建委外订单ESB日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>ESB日志记录器</returns>
        public static ESBLogger CreateSubOrderLogger(ILoggerFactory loggerFactory)
        {
            return new ESBLogger(loggerFactory, "委外订单");
        }

        /// <summary>
        /// 创建部件跟踪ESB日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>ESB日志记录器</returns>
        public static ESBLogger CreatePartTrackingLogger(ILoggerFactory loggerFactory)
        {
            return new ESBLogger(loggerFactory, "部件跟踪");
        }

        /// <summary>
        /// 创建整机跟踪ESB日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>ESB日志记录器</returns>
        public static ESBLogger CreateWholeUnitTrackingLogger(ILoggerFactory loggerFactory)
        {
            return new ESBLogger(loggerFactory, "整机跟踪");
        }

        /// <summary>
        /// 创建金工跟踪ESB日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>ESB日志记录器</returns>
        public static ESBLogger CreateMetalworkTrackingLogger(ILoggerFactory loggerFactory)
        {
            return new ESBLogger(loggerFactory, "金工跟踪");
        }

        /// <summary>
        /// 创建技术管理ESB日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>ESB日志记录器</returns>
        public static ESBLogger CreateTechManagementLogger(ILoggerFactory loggerFactory)
        {
            return new ESBLogger(loggerFactory, "技术管理");
        }

        /// <summary>
        /// 创建销售管理ESB日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>ESB日志记录器</returns>
        public static ESBLogger CreateSalesManagementLogger(ILoggerFactory loggerFactory)
        {
            return new ESBLogger(loggerFactory, "销售管理");
        }

        /// <summary>
        /// 创建删除数据查询ESB日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>ESB日志记录器</returns>
        public static ESBLogger CreateQueryDeletedDataLogger(ILoggerFactory loggerFactory)
        {
            return new ESBLogger(loggerFactory, "删除数据查询");
        }

        /// <summary>
        /// 创建通用ESB日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="esbType">ESB类型名称</param>
        /// <returns>ESB日志记录器</returns>
        public static ESBLogger CreateLogger(ILoggerFactory loggerFactory, string esbType)
        {
            return new ESBLogger(loggerFactory, esbType);
        }
    }
}
