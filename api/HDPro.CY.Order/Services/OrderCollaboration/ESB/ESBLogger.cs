/*
 * ESB通用日志服务
 * 提供分离的错误日志和普通日志记录功能
 * 支持多种ESB接口的日志记录，日志文件存储到以日期命名的文件夹中
 */
using Microsoft.Extensions.Logging;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB
{
    /// <summary>
    /// ESB通用日志服务
    /// 使用专用的日志记录器，将错误日志和普通日志分开存储到以日期命名的文件夹中
    /// 支持多种ESB接口的日志记录
    /// </summary>
    public class ESBLogger
    {
        private readonly ILogger _logger;
        private readonly string _esbType;

        public ESBLogger(ILoggerFactory loggerFactory, string esbType)
        {
            _esbType = esbType ?? "Unknown";
            // 使用ESB.{类型}作为日志记录器名称，对应NLog配置中的"ESB.*"规则
            _logger = loggerFactory.CreateLogger($"ESB.{_esbType}");
        }

        #region 普通日志记录方法

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void LogInfo(string message)
        {
            _logger.LogInformation("[{EsbType}] {Message}", _esbType, message);
        }

        /// <summary>
        /// 记录信息日志（带格式化参数）
        /// </summary>
        /// <param name="message">日志消息模板</param>
        /// <param name="args">格式化参数</param>
        public void LogInfo(string message, params object[] args)
        {
            _logger.LogInformation($"[{_esbType}] {message}", args);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void LogWarning(string message)
        {
            _logger.LogWarning("[{EsbType}] {Message}", _esbType, message);
        }

        /// <summary>
        /// 记录警告日志（带格式化参数）
        /// </summary>
        /// <param name="message">日志消息模板</param>
        /// <param name="args">格式化参数</param>
        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning($"[{_esbType}] {message}", args);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void LogDebug(string message)
        {
            _logger.LogDebug("[{EsbType}] {Message}", _esbType, message);
        }

        /// <summary>
        /// 记录跟踪日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void LogTrace(string message)
        {
            _logger.LogTrace("[{EsbType}] {Message}", _esbType, message);
        }

        #endregion

        #region 错误日志记录方法

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">错误消息</param>
        public void LogError(string message)
        {
            _logger.LogError("[{EsbType}-错误] {Message}", _esbType, message);
        }

        /// <summary>
        /// 记录错误日志（带异常信息）
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="message">错误消息</param>
        public void LogError(Exception ex, string message)
        {
            _logger.LogError(ex, "[{EsbType}-错误] {Message}", _esbType, message);
        }

        /// <summary>
        /// 记录错误日志（带格式化参数）
        /// </summary>
        /// <param name="message">错误消息模板</param>
        /// <param name="args">格式化参数</param>
        public void LogError(string message, params object[] args)
        {
            _logger.LogError($"[{_esbType}-错误] {message}", args);
        }

        /// <summary>
        /// 记录错误日志（带异常信息和格式化参数）
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="message">错误消息模板</param>
        /// <param name="args">格式化参数</param>
        public void LogError(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, $"[{_esbType}-错误] {message}", args);
        }

        #endregion

        #region 同步操作专用日志方法

        /// <summary>
        /// 记录同步开始日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="executorInfo">执行者信息</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        public void LogSyncStart(string operationType, string executorInfo, string startDate, string endDate)
        {
            LogInfo("开始{OperationType}ESB数据同步，执行者：{ExecutorInfo}，时间范围：{StartDate} 到 {EndDate}", 
                operationType, executorInfo, startDate, endDate);
        }

        /// <summary>
        /// 记录同步完成日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="executorInfo">执行者信息</param>
        /// <param name="totalCount">总数据量</param>
        /// <param name="validCount">有效数据量</param>
        /// <param name="processedCount">处理数据量</param>
        /// <param name="syncStartTime">同步开始时间</param>
        /// <returns>成功消息</returns>
        public string LogSyncComplete(string operationType, string executorInfo, int totalCount, int validCount, int processedCount, DateTime syncStartTime)
        {
            var syncEndTime = DateTime.Now;
            var totalTime = (syncEndTime - syncStartTime).TotalSeconds;
            
            var successMessage = $"{operationType}ESB数据同步完成！执行者：{executorInfo}，" +
                $"获取数据：{totalCount} 条，有效数据：{validCount} 条，" +
                $"处理记录：{processedCount} 条，耗时：{totalTime:F2} 秒";
            
            LogInfo(successMessage);
            return successMessage;
        }

        /// <summary>
        /// 记录同步错误日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="ex">异常信息</param>
        /// <param name="syncStartTime">同步开始时间</param>
        /// <returns>错误消息</returns>
        public string LogSyncError(string operationType, Exception ex, DateTime syncStartTime)
        {
            var syncEndTime = DateTime.Now;
            var totalTime = (syncEndTime - syncStartTime).TotalSeconds;
            var errorMessage = $"同步{operationType}ESB数据失败，耗时：{totalTime:F2} 秒，错误：{ex.Message}";
            
            LogError(ex, errorMessage);
            return errorMessage;
        }

        /// <summary>
        /// 记录数据验证错误日志
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="validationError">验证错误信息</param>
        /// <param name="dataInfo">数据信息</param>
        public void LogValidationError(string dataType, string validationError, string dataInfo = null)
        {
            var message = $"{dataType}数据验证失败：{validationError}";
            if (!string.IsNullOrEmpty(dataInfo))
            {
                message += $"，数据信息：{dataInfo}";
            }
            LogError(message);
        }

        /// <summary>
        /// 记录数据处理错误日志
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="processingError">处理错误信息</param>
        /// <param name="dataInfo">数据信息</param>
        /// <param name="ex">异常对象</param>
        public void LogProcessingError(string dataType, string processingError, string dataInfo = null, Exception ex = null)
        {
            var message = $"{dataType}数据处理失败：{processingError}";
            if (!string.IsNullOrEmpty(dataInfo))
            {
                message += $"，数据信息：{dataInfo}";
            }
            
            if (ex != null)
            {
                LogError(ex, message);
            }
            else
            {
                LogError(message);
            }
        }

        #endregion
    }
}
