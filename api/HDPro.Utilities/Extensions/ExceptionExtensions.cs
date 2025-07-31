using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HDPro.Utilities.Extensions
{
    public static class ExceptionExtensions
    {


        /// <summary>
        /// 格式化异常信息
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="customMessage"></param>
        /// <returns></returns>
        public static  string Format(this Exception ex, string customMessage = "",bool isStackTrace=false)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"【错误信息】{customMessage}：{ex.Message}");
            if (isStackTrace)
            {
                sb.AppendLine($"【堆栈跟踪】：{ex.StackTrace}");
            }

            if (ex.InnerException != null)
            {
                sb.AppendLine($"【内部异常】：{ex.InnerException.Message}");
                if (isStackTrace)
                {
                    sb.AppendLine($"【内部异常堆栈跟踪】：{ex.InnerException.StackTrace}");
                }
            }

            if (ex is AggregateException)
            {
                var exs = (ex as AggregateException)?.InnerExceptions;
                if (exs != null)
                {
                    for (int i = 0; i < exs.Count; i++)
                    {
                        var item = exs[i];

                        sb.AppendLine($"【第{i + 1}个错误信息】{item.Message}");

                        if (ex.InnerException != null)
                        {
                            sb.AppendLine($"{item?.InnerException?.Message}");
                            if (item?.InnerException?.InnerException != null)
                            {
                                sb.AppendLine($"{item.InnerException.InnerException.Message}");
                            }
                        }
                        if (isStackTrace)
                        {
                            sb.AppendLine($"【第{i + 1}个错误堆栈跟踪】：{item.StackTrace}");

                            if (ex.InnerException != null)
                            {
                                sb.AppendLine($"{item?.InnerException?.StackTrace}");
                                if (item?.InnerException?.InnerException != null)
                                {
                                    sb.AppendLine($"{item.InnerException.InnerException.StackTrace}");
                                }
                            }
                        }
                    }
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// 获取OriginalException异常
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>

        public static Exception GetOriginalException(this Exception ex)
        {
            if (ex.InnerException == null) return ex;

            return ex.InnerException.GetOriginalException();
        }

        /// <summary>
        /// 获取EF Core详细异常信息，重点关注SQL语句和关键错误
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="customMessage">自定义消息</param>
        /// <returns>格式化的详细异常信息</returns>
        public static string GetDetailedEFCoreException(this Exception ex, string customMessage = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine($"【EF Core异常详细信息】{customMessage}");
            sb.AppendLine($"【异常时间】：{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"【异常类型】：{ex.GetType().Name}");
            sb.AppendLine($"【异常消息】：{ex.Message}");
            sb.AppendLine();

            // 处理DbUpdateException
            if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                sb.AppendLine("【Entity Framework更新异常详情】");
                sb.AppendLine($"【受影响的实体数量】：{dbEx.Entries?.Count ?? 0}");
                
                // 输出实体类型和状态，并检查字段长度问题
                if (dbEx.Entries != null)
                {
                    for (int i = 0; i < dbEx.Entries.Count; i++)
                    {
                        var entry = dbEx.Entries[i];
                        sb.AppendLine($"【实体 {i + 1}】类型: {entry.Entity.GetType().Name}, 状态: {entry.State}");
                        
                        // 检查字段长度超限问题（特别针对8152错误）
                        if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx8152 && sqlEx8152.Number == 8152)
                        {
                            sb.AppendLine("  【字段长度检查】:");
                            bool foundLengthIssue = false;
                            
                            foreach (var prop in entry.Properties)
                            {
                                var currentValue = prop.CurrentValue;
                                var maxLength = prop.Metadata.GetMaxLength();
                                
                                if (maxLength.HasValue && currentValue != null)
                                {
                                    string valueStr = currentValue.ToString();
                                    if (valueStr.Length > maxLength.Value)
                                    {
                                        sb.AppendLine($"    ⚠️ 字段 [{prop.Metadata.Name}] 长度超限:");
                                        sb.AppendLine($"      当前长度: {valueStr.Length}");
                                        sb.AppendLine($"      最大长度: {maxLength.Value}");
                                        sb.AppendLine($"      超出长度: {valueStr.Length - maxLength.Value}");
                                        sb.AppendLine($"      字段类型: {prop.Metadata.ClrType.Name}");
                                        sb.AppendLine($"      当前值: {(valueStr.Length > 100 ? valueStr.Substring(0, 100) + "..." : valueStr)}");
                                        foundLengthIssue = true;
                                    }
                                }
                            }
                            
                            if (!foundLengthIssue)
                            {
                                sb.AppendLine("    未检测到明显的字段长度超限问题，可能是数据库约束更严格");
                                sb.AppendLine("    【所有字符串字段信息】:");
                                foreach (var prop in entry.Properties.Where(p => p.Metadata.ClrType == typeof(string)))
                                {
                                    var currentValue = prop.CurrentValue;
                                    var maxLength = prop.Metadata.GetMaxLength();
                                    if (currentValue != null)
                                    {
                                        string valueStr = currentValue.ToString();
                                        sb.AppendLine($"      字段 [{prop.Metadata.Name}]: 长度={valueStr.Length}, 最大长度={maxLength?.ToString() ?? "未限制"}");
                                        if (valueStr.Length > 50)
                                        {
                                            sb.AppendLine($"        值: {valueStr.Substring(0, 50)}...");
                                        }
                                        else
                                        {
                                            sb.AppendLine($"        值: {valueStr}");
                                        }
                                    }
                                }
                            }
                        }
                        
                        // 检查NULL值问题（针对515错误）
                        if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx515 && sqlEx515.Number == 515)
                        {
                            sb.AppendLine("  【NULL值检查】:");
                            bool foundNullIssue = false;
                            
                            foreach (var prop in entry.Properties)
                            {
                                var currentValue = prop.CurrentValue;
                                var isNullable = prop.Metadata.IsNullable;
                                
                                if (!isNullable && (currentValue == null || (currentValue is string str && string.IsNullOrEmpty(str))))
                                {
                                    sb.AppendLine($"    ⚠️ 字段 [{prop.Metadata.Name}] 不允许为NULL但当前值为空:");
                                    sb.AppendLine($"      字段类型: {prop.Metadata.ClrType.Name}");
                                    sb.AppendLine($"      是否可空: {isNullable}");
                                    sb.AppendLine($"      当前值: {currentValue?.ToString() ?? "NULL"}");
                                    foundNullIssue = true;
                                }
                            }
                            
                            if (!foundNullIssue)
                            {
                                sb.AppendLine("    未检测到明显的NULL值问题");
                            }
                        }
                    }
                }
                sb.AppendLine();

                // 处理SQL Server特定异常
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    sb.AppendLine("【SQL Server异常详情】");
                    sb.AppendLine($"  SQL错误码: {sqlEx.Number}");
                    sb.AppendLine($"  SQL错误级别: {sqlEx.Class}");
                    sb.AppendLine($"  SQL错误状态: {sqlEx.State}");
                    sb.AppendLine($"  SQL服务器: {sqlEx.Server ?? "未知"}");
                    sb.AppendLine($"  SQL存储过程: {sqlEx.Procedure ?? "无"}");
                    sb.AppendLine($"  SQL行号: {sqlEx.LineNumber}");
                    sb.AppendLine($"  SQL消息: {sqlEx.Message}");
                    
                    // 根据错误码提供具体的错误说明
                    string errorDescription = GetSqlErrorDescription(sqlEx.Number);
                    if (!string.IsNullOrEmpty(errorDescription))
                    {
                        sb.AppendLine($"  错误说明: {errorDescription}");
                    }
                    
                    // 针对8152错误提供额外建议
                    if (sqlEx.Number == 8152)
                    {
                        sb.AppendLine($"  解决建议: 请检查上述标记为⚠️的字段，缩短数据长度或增加数据库字段长度");
                    }
                    
                    sb.AppendLine();
                }

                // 处理MySQL特定异常
                if (ex.InnerException?.GetType().Name.Contains("MySql") == true)
                {
                    sb.AppendLine("【MySQL异常详情】");
                    sb.AppendLine($"  MySQL错误消息: {ex.InnerException.Message}");
                    
                    // 尝试获取MySQL错误码
                    var mysqlEx = ex.InnerException;
                    var numberProperty = mysqlEx.GetType().GetProperty("Number");
                    if (numberProperty != null)
                    {
                        var errorNumber = numberProperty.GetValue(mysqlEx);
                        sb.AppendLine($"  MySQL错误码: {errorNumber}");
                        
                        string mysqlErrorDesc = GetMySqlErrorDescription(Convert.ToInt32(errorNumber));
                        if (!string.IsNullOrEmpty(mysqlErrorDesc))
                        {
                            sb.AppendLine($"  错误说明: {mysqlErrorDesc}");
                        }
                    }
                    sb.AppendLine();
                }
            }

            // 处理验证异常
            if (ex is System.ComponentModel.DataAnnotations.ValidationException validationEx)
            {
                sb.AppendLine("【数据验证异常详情】");
                sb.AppendLine($"  验证错误消息: {validationEx.Message}");
                if (validationEx.ValidationResult != null)
                {
                    sb.AppendLine($"  验证结果: {validationEx.ValidationResult.ErrorMessage}");
                    if (validationEx.ValidationResult.MemberNames != null)
                    {
                        sb.AppendLine($"  涉及字段: {string.Join(", ", validationEx.ValidationResult.MemberNames)}");
                    }
                }
                sb.AppendLine();
            }

            // 添加内部异常信息
            if (ex.InnerException != null)
            {
                sb.AppendLine("【内部异常信息】");
                sb.AppendLine($"  内部异常类型: {ex.InnerException.GetType().Name}");
                sb.AppendLine($"  内部异常消息: {ex.InnerException.Message}");
                
                // 递归处理内部异常
                var innerEx = ex.InnerException;
                int level = 1;
                while (innerEx.InnerException != null && level < 5) // 限制递归深度
                {
                    innerEx = innerEx.InnerException;
                    level++;
                    sb.AppendLine($"  第{level}层内部异常: {innerEx.GetType().Name} - {innerEx.Message}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取简化的EF Core异常信息，专注于SQL错误
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="customMessage">自定义消息</param>
        /// <returns>简化的异常信息</returns>
        public static string GetSimplifiedEFCoreException(this Exception ex, string customMessage = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine($"实体保存异常: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                sb.AppendLine($"内部异常: {ex.InnerException.Message}");
                
                // 获取SQL Server特定异常信息
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    sb.AppendLine($"SQL错误码: {sqlEx.Number}");
                    sb.AppendLine($"SQL错误信息: {sqlEx.Message}");
                    
                    // 添加错误说明
                    string errorDescription = GetSqlErrorDescription(sqlEx.Number);
                    if (!string.IsNullOrEmpty(errorDescription))
                    {
                        sb.AppendLine($"错误说明: {errorDescription}");
                    }
                }
                
                // 获取MySQL特定异常信息
                if (ex.InnerException?.GetType().Name.Contains("MySql") == true)
                {
                    var mysqlEx = ex.InnerException;
                    var numberProperty = mysqlEx.GetType().GetProperty("Number");
                    if (numberProperty != null)
                    {
                        var errorNumber = numberProperty.GetValue(mysqlEx);
                        sb.AppendLine($"MySQL错误码: {errorNumber}");
                        
                        string mysqlErrorDesc = GetMySqlErrorDescription(Convert.ToInt32(errorNumber));
                        if (!string.IsNullOrEmpty(mysqlErrorDesc))
                        {
                            sb.AppendLine($"错误说明: {mysqlErrorDesc}");
                        }
                    }
                    sb.AppendLine($"MySQL错误信息: {ex.InnerException.Message}");
                }
                
                // 捕获更深层的异常信息
                if (ex.InnerException.InnerException != null)
                {
                    sb.AppendLine($"更深层异常: {ex.InnerException.InnerException.Message}");
                }
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// 记录EF Core异常到HDLogHelper并抛出简化异常
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="customMessage">自定义消息</param>
        public static void LogAndThrowEFCoreException(this Exception ex, string customMessage = "实体保存异常")
        {
            try
            {
                // 记录详细异常信息到HDLogHelper
                string detailedMessage = ex.GetDetailedEFCoreException(customMessage);
                HDLogHelper.Log($"EFCore_Exception_{DateTime.Now:yyyyMMddHHmmss}", detailedMessage, "EFCore_Exceptions");
                
                // 获取简化的异常信息用于抛出
                string simplifiedMessage = ex.GetSimplifiedEFCoreException(customMessage);
                
                // 抛出包含关键信息的新异常
                throw new Exception(simplifiedMessage, ex);
            }
            catch (Exception logEx) when (!(logEx is Exception && logEx.Message.Contains("实体保存异常")))
            {
                // 如果记录日志时发生异常，使用基本方法
                Console.WriteLine($"记录EF Core异常时出错: {logEx.Message}");
                
                // 仍然抛出原始异常的简化版本
                string fallbackMessage = $"实体保存异常: {ex.Message}";
                if (ex.InnerException != null)
                {
                    fallbackMessage += $"\n内部异常: {ex.InnerException.Message}";
                }
                throw new Exception(fallbackMessage, ex);
            }
        }

        /// <summary>
        /// 获取SQL Server错误码说明
        /// </summary>
        /// <param name="errorNumber">错误码</param>
        /// <returns>错误说明</returns>
        private static string GetSqlErrorDescription(int errorNumber)
        {
            return errorNumber switch
            {
                2 => "找不到指定的文件",
                18 => "登录失败",
                102 => "SQL语法错误",
                207 => "列名无效",
                208 => "对象名无效",
                515 => "不能将NULL值插入到不允许NULL的列中",
                547 => "外键约束冲突",
                2601 => "不能在具有唯一索引的对象中插入重复键的行",
                2627 => "违反了PRIMARY KEY约束",
                8152 => "字符串或二进制数据会被截断",
                18456 => "用户登录失败",
                _ => ""
            };
        }

        /// <summary>
        /// 获取MySQL错误码说明
        /// </summary>
        /// <param name="errorNumber">错误码</param>
        /// <returns>错误说明</returns>
        private static string GetMySqlErrorDescription(int errorNumber)
        {
            return errorNumber switch
            {
                1062 => "违反唯一约束，重复键值",
                1048 => "列不能为NULL",
                1452 => "外键约束失败",
                1406 => "数据太长，超出列定义长度",
                1264 => "列值超出范围",
                1292 => "日期时间值不正确",
                _ => ""
            };
        }

        /// <summary>
        /// 将异常详细信息写入日志文件
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="customMessage">自定义消息</param>
        /// <param name="directory">指定目录，为空则使用默认目录</param>
        /// <returns>日志文件路径</returns>
        public static void WriteToFile(this Exception ex, string customMessage = "", string directory = "Exceptions")
        {
            try
            {
                string formattedMessage;
                
                // 如果是EF Core相关异常，使用详细的异常信息
                if (ex is Microsoft.EntityFrameworkCore.DbUpdateException || 
                    ex.InnerException is Microsoft.Data.SqlClient.SqlException ||
                    ex.InnerException?.GetType().Name.Contains("MySql") == true)
                {
                    formattedMessage = ex.GetDetailedEFCoreException(customMessage);
                }
                else
                {
                    formattedMessage = ex.Format(customMessage, true);
                }

                HDLogHelper.Log("EF_Exception_" + DateTime.Now.ToString("yyyyMMddHHmmss"), formattedMessage, directory);
            }
            catch (Exception logEx)
            {
                // 如果记录日志时发生异常，尝试使用基本方法记录
                Console.WriteLine($"记录异常时出错: {logEx.Message}");
                try
                {
                    string path = $"{AppDomain.CurrentDomain.BaseDirectory}\\Log\\{DateTime.Now.ToString("yyyy-MM-dd")}\\EmergencyExceptions";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    
                    string fileName = Path.Combine(path, $"Emergency_Exception_{DateTime.Now.ToString("yyyyMMddHHmmss")}.log");
                    File.WriteAllText(fileName, ex.Message + Environment.NewLine + ex.StackTrace);

                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 将EF Core异常信息输出到控制台（用于开发调试）
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="customMessage">自定义消息</param>
        public static void WriteToConsole(this Exception ex, string customMessage = "")
        {
            try
            {
                string detailedMessage;
                
                // 如果是EF Core相关异常，使用详细的异常信息
                if (ex is Microsoft.EntityFrameworkCore.DbUpdateException || 
                    ex.InnerException is Microsoft.Data.SqlClient.SqlException ||
                    ex.InnerException?.GetType().Name.Contains("MySql") == true)
                {
                    detailedMessage = ex.GetDetailedEFCoreException(customMessage);
                }
                else
                {
                    detailedMessage = ex.Format(customMessage, true);
                }

                Console.WriteLine("=".PadRight(80, '='));
                Console.WriteLine(detailedMessage);
                Console.WriteLine("=".PadRight(80, '='));
            }
            catch (Exception consoleEx)
            {
                Console.WriteLine($"输出异常信息到控制台时出错: {consoleEx.Message}");
                Console.WriteLine($"原始异常: {ex.Message}");
            }
        }
    }
}
