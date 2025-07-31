/*
 * OA日志工厂
 * 提供统一的OA日志创建服务
 */
using Microsoft.Extensions.Logging;

namespace HDPro.CY.Order.Services.OA
{
    /// <summary>
    /// OA日志工厂
    /// 提供统一的OA日志创建服务，简化各OA服务的日志记录器创建
    /// </summary>
    public static class OALoggerFactory
    {
        /// <summary>
        /// 创建OA消息日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>OA日志记录器</returns>
        public static OALogger CreateMessageLogger(ILoggerFactory loggerFactory)
        {
            return new OALogger(loggerFactory, "消息");
        }

        /// <summary>
        /// 创建OA流程日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>OA日志记录器</returns>
        public static OALogger CreateProcessLogger(ILoggerFactory loggerFactory)
        {
            return new OALogger(loggerFactory, "流程");
        }

        /// <summary>
        /// 创建股份OA消息日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>OA日志记录器</returns>
        public static OALogger CreateShareholderMessageLogger(ILoggerFactory loggerFactory)
        {
            return new OALogger(loggerFactory, "股份消息");
        }

        /// <summary>
        /// 创建股份OA流程日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>OA日志记录器</returns>
        public static OALogger CreateShareholderProcessLogger(ILoggerFactory loggerFactory)
        {
            return new OALogger(loggerFactory, "股份流程");
        }

        /// <summary>
        /// 创建OA Token日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>OA日志记录器</returns>
        public static OALogger CreateTokenLogger(ILoggerFactory loggerFactory)
        {
            return new OALogger(loggerFactory, "Token");
        }

        /// <summary>
        /// 创建通用OA日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <returns>OA日志记录器</returns>
        public static OALogger CreateGeneralLogger(ILoggerFactory loggerFactory)
        {
            return new OALogger(loggerFactory, "通用");
        }

        /// <summary>
        /// 创建自定义类型的OA日志记录器
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="oaType">OA类型</param>
        /// <returns>OA日志记录器</returns>
        public static OALogger CreateCustomLogger(ILoggerFactory loggerFactory, string oaType)
        {
            return new OALogger(loggerFactory, oaType);
        }
    }
}
