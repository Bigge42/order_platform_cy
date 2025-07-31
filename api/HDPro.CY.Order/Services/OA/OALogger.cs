/*
 * OA专用日志记录器
 * 将OA消息和流程相关的日志记录到独立的日志文件中
 */
using Microsoft.Extensions.Logging;

namespace HDPro.CY.Order.Services.OA
{
    /// <summary>
    /// OA专用日志服务
    /// 使用专用的日志记录器，将OA相关日志记录到独立的日志文件中
    /// 支持OA消息推送、流程发起、Token获取等操作的日志记录
    /// </summary>
    public class OALogger
    {
        private readonly ILogger _logger;
        private readonly string _oaType;

        public OALogger(ILoggerFactory loggerFactory, string oaType)
        {
            _oaType = oaType ?? "OA";
            // 使用OA.{类型}作为日志记录器名称，对应NLog配置中的"OA.*"规则
            _logger = loggerFactory.CreateLogger($"OA.{_oaType}");
        }

        #region 普通日志记录方法

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void LogInfo(string message)
        {
            _logger.LogInformation("[{OaType}] {Message}", _oaType, message);
        }

        /// <summary>
        /// 记录信息日志（带参数）
        /// </summary>
        /// <param name="message">日志消息模板</param>
        /// <param name="args">参数</param>
        public void LogInfo(string message, params object[] args)
        {
            _logger.LogInformation($"[{_oaType}] {message}", args);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">警告消息</param>
        public void LogWarning(string message)
        {
            _logger.LogWarning("[{OaType}] {Message}", _oaType, message);
        }

        /// <summary>
        /// 记录警告日志（带参数）
        /// </summary>
        /// <param name="message">警告消息模板</param>
        /// <param name="args">参数</param>
        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning($"[{_oaType}] {message}", args);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void LogDebug(string message)
        {
            _logger.LogDebug("[{OaType}] {Message}", _oaType, message);
        }

        /// <summary>
        /// 记录调试日志（带参数）
        /// </summary>
        /// <param name="message">日志消息模板</param>
        /// <param name="args">参数</param>
        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug($"[{_oaType}] {message}", args);
        }

        /// <summary>
        /// 记录跟踪日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void LogTrace(string message)
        {
            _logger.LogTrace("[{OaType}] {Message}", _oaType, message);
        }

        #endregion

        #region 错误日志记录方法

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">错误消息</param>
        public void LogError(string message)
        {
            _logger.LogError("[{OaType}-错误] {Message}", _oaType, message);
        }

        /// <summary>
        /// 记录错误日志（带异常）
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="message">错误消息</param>
        public void LogError(Exception ex, string message)
        {
            _logger.LogError(ex, "[{OaType}-错误] {Message}", _oaType, message);
        }

        /// <summary>
        /// 记录错误日志（带参数）
        /// </summary>
        /// <param name="message">错误消息模板</param>
        /// <param name="args">参数</param>
        public void LogError(string message, params object[] args)
        {
            _logger.LogError($"[{_oaType}-错误] {message}", args);
        }

        /// <summary>
        /// 记录错误日志（带异常和参数）
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="message">错误消息模板</param>
        /// <param name="args">参数</param>
        public void LogError(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, $"[{_oaType}-错误] {message}", args);
        }

        #endregion

        #region OA专用日志方法

        /// <summary>
        /// 记录Token获取操作日志
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="success">是否成功</param>
        /// <param name="message">消息</param>
        public void LogTokenOperation(string loginName, bool success, string message)
        {
            var status = success ? "成功" : "失败";
            LogInfo("Token获取{Status} - 登录名: {LoginName}, 消息: {Message}", status, loginName, message);
        }

        /// <summary>
        /// 记录消息推送操作日志
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="success">是否成功</param>
        /// <param name="message">消息</param>
        public void LogMessageOperation(string senderLoginName, List<string> receiverLoginNames, bool success, string message)
        {
            var status = success ? "成功" : "失败";
            var receivers = receiverLoginNames != null ? string.Join(",", receiverLoginNames) : "无";
            LogInfo("消息推送{Status} - 发送者: {Sender}, 接收者: {Receivers}, 消息: {Message}", 
                status, senderLoginName, receivers, message);
        }

        /// <summary>
        /// 记录流程发起操作日志
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="templateCode">模板代码</param>
        /// <param name="success">是否成功</param>
        /// <param name="message">消息</param>
        public void LogProcessOperation(string loginName, string templateCode, bool success, string message)
        {
            var status = success ? "成功" : "失败";
            LogInfo("流程发起{Status} - 登录名: {LoginName}, 模板: {TemplateCode}, 消息: {Message}", 
                status, loginName, templateCode, message);
        }

        /// <summary>
        /// 记录HTTP请求日志
        /// </summary>
        /// <param name="method">HTTP方法</param>
        /// <param name="url">请求URL</param>
        /// <param name="statusCode">状态码</param>
        /// <param name="duration">耗时（毫秒）</param>
        public void LogHttpRequest(string method, string url, int statusCode, double duration)
        {
            LogInfo("HTTP请求 - {Method} {Url}, 状态码: {StatusCode}, 耗时: {Duration}ms", 
                method, url, statusCode, duration);
        }

        /// <summary>
        /// 记录请求参数日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="requestJson">请求JSON</param>
        public void LogRequestParameters(string operationType, string requestJson)
        {
            LogDebug("{OperationType}请求参数: {RequestJson}", operationType, requestJson);
        }

        /// <summary>
        /// 记录响应内容日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="responseContent">响应内容</param>
        public void LogResponseContent(string operationType, string responseContent)
        {
            LogDebug("{OperationType}响应内容: {ResponseContent}", operationType, responseContent);
        }

        #endregion
    }
}
