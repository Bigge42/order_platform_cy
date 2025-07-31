using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.BaseProvider;
using HDPro.Core.Configuration;
using HDPro.Core.EFDbContext;
using HDPro.Core.Enums;
using HDPro.Utilities.Extensions;
using HDPro.Utilities;
using static Dapper.SqlMapper;

namespace HDPro.Core.Extensions
{
    public static class DbContextExtension
    {

        public static int Update<TSource>(this BaseDbContext dbContext, TSource entity, string[] properties, bool saveChanges = false) where TSource : class
        {
            return dbContext.UpdateRange<TSource>(new List<TSource>() { entity }, properties, saveChanges);
        }
        public static int Update<TSource>(this BaseDbContext dbContext, TSource entity, bool saveChanges = false) where TSource : class
        {
            return dbContext.UpdateRange<TSource>(new List<TSource>() { entity }, new string[0], saveChanges);
        }
        public static int UpdateRange<TSource>(this BaseDbContext dbContext, IEnumerable<TSource> entities, Expression<Func<TSource, object>> properties, bool saveChanges = false) where TSource : class
        {
            return dbContext.UpdateRange<TSource>(entities, properties?.GetExpressionProperty(), saveChanges);
        }
        public static int UpdateRange<TSource>(this BaseDbContext dbContext, IEnumerable<TSource> entities, bool saveChanges = false) where TSource : class
        {
            return dbContext.UpdateRange<TSource>(entities, new string[0], saveChanges);
        }


        public static int UpdateRange<TSource>(this BaseDbContext dbContext, IEnumerable<TSource> entities, string[] properties, bool saveChanges = false) where TSource : class
        {
            if (properties != null && properties.Length > 0)
            {
                PropertyInfo[] entityProperty = typeof(TSource).GetProperties()
                        .Where(x => x.GetCustomAttribute<NotMappedAttribute>() == null).ToArray();
                string keyName = entityProperty.GetKeyName();
                if (properties.Contains(keyName))
                {
                    properties = properties.Where(x => x != keyName).ToArray();
                }
                properties = properties.Where(x => entityProperty.Select(s => s.Name).Contains(x)).ToArray();
            }
            foreach (TSource item in entities)
            {
                if (properties == null || properties.Length == 0)
                {
                    dbContext.Entry<TSource>(item).State = EntityState.Modified;
                    continue;
                }
                var entry = dbContext.Entry(item);
                properties.ToList().ForEach(x =>
                {
                    entry.Property(x).IsModified = true;
                });
            }
            if (!saveChanges)
            {
                return 0;
            }
            else {
                dbContext.SaveChanges();
            }
            return entities.Count();
        }


        /// <summary>
        /// 通过主键批量删除
        /// </summary>
        /// <param name="keys">主键key</param>
        /// <param name="delList">是否连明细一起删除</param>
        /// <returns></returns>
        public static int DeleteWithKeys<T>(this BaseDbContext dbContext, object[] keys, bool saveChange = false) where T : class
        {
            var keyPro = typeof(T).GetKeyProperty();
            foreach (var key in keys.Distinct())
            {
                T entity = Activator.CreateInstance<T>();
                keyPro.SetValue(entity, key.ChangeType(keyPro.PropertyType));
                dbContext.Entry<T>(entity).State = EntityState.Deleted;
            }
            if (saveChange)
            {
                dbContext.SaveChanges();
            }
            return keys.Length;
        }

        public static int Delete<T>(this BaseDbContext dbContext, [NotNull] Expression<Func<T, bool>> wheres, bool saveChange = false) where T : class
        {
            var keyProperty = typeof(T).GetKeyProperty();
            string keyName = typeof(T).GetKeyProperty().Name;
            var expression = keyName.GetExpression<T, object>();
            var ids = dbContext.Set<T>().Where(wheres).Select(expression).ToList();
            List<T> list = new List<T>();
            foreach (var id in ids)
            {
                T entity = Activator.CreateInstance<T>();
                keyProperty.SetValue(entity, id);
                list.Add(entity);
            }
            dbContext.RemoveRange(list);
            if (saveChange)
            {
                return dbContext.SaveChanges();
            }
            return 0;
        }
        /// <summary>
        /// 过滤逻辑删除 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<T> FilterLogicDel<T>(this IQueryable<T> query) where T:class
        {
            if (!string.IsNullOrEmpty(AppSetting.LogicDelField))
            {
                if (typeof(T).GetProperty(AppSetting.LogicDelField) != null)
                {
                    var expression = AppSetting.LogicDelField.CreateExpression<T>((int)DelStatus.正常, LinqExpressionType.Equal);
                    return query.Where(expression);
                }
            }
            return query;
        }

        /// <summary>
        /// 安全的SaveChanges方法，自动捕获和记录详细的EF Core异常信息
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="logToFile">是否记录到文件，默认true</param>
        /// <param name="logToConsole">是否输出到控制台，默认在开发环境下为true</param>
        /// <param name="customMessage">自定义异常消息</param>
        /// <returns>受影响的行数</returns>
        public static int SafeSaveChanges(this DbContext dbContext, bool logToFile = true, bool logToConsole = false, string customMessage = "")
        {
            try
            {
                return dbContext.SaveChanges();
            }
            catch (Exception ex) when (ex is Microsoft.EntityFrameworkCore.DbUpdateException || 
                                      ex.InnerException is Microsoft.Data.SqlClient.SqlException ||
                                      ex.InnerException?.GetType().Name.Contains("MySql") == true)
            {
                if (logToFile)
                {
                    // 使用LogAndThrowEFCoreException记录到HDLogHelper并抛出简化异常
                    ex.LogAndThrowEFCoreException(customMessage);
                }
                else
                {
                    // 不记录文件，直接抛出简化异常
                    string simplifiedMessage = ex.GetSimplifiedEFCoreException(customMessage);
                    throw new Exception(simplifiedMessage, ex);
                }
                
                // 这行代码不会执行到，但为了编译器满意
                throw;
            }
            catch (Exception ex)
            {
                // 非EF Core异常，直接抛出
                if (logToConsole)
                {
                    Console.WriteLine($"数据库操作异常: {ex.Message}");
                }
                throw;
            }
        }

        /// <summary>
        /// 安全的SaveChangesAsync方法，自动捕获和记录详细的EF Core异常信息
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="logToFile">是否记录到文件，默认true</param>
        /// <param name="logToConsole">是否输出到控制台，默认在开发环境下为true</param>
        /// <param name="customMessage">自定义异常消息</param>
        /// <returns>受影响的行数</returns>
        public static async Task<int> SafeSaveChangesAsync(this DbContext dbContext, bool logToFile = true, bool logToConsole = false, string customMessage = "")
        {
            try
            {
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is Microsoft.EntityFrameworkCore.DbUpdateException || 
                                      ex.InnerException is Microsoft.Data.SqlClient.SqlException ||
                                      ex.InnerException?.GetType().Name.Contains("MySql") == true)
            {
                if (logToFile)
                {
                    // 使用LogAndThrowEFCoreException记录到HDLogHelper并抛出简化异常
                    ex.LogAndThrowEFCoreException(customMessage);
                }
                else
                {
                    // 不记录文件，直接抛出简化异常
                    string simplifiedMessage = ex.GetSimplifiedEFCoreException(customMessage);
                    throw new Exception(simplifiedMessage, ex);
                }
                
                // 这行代码不会执行到，但为了编译器满意
                throw;
            }
            catch (Exception ex)
            {
                // 非EF Core异常，直接抛出
                if (logToConsole)
                {
                    Console.WriteLine($"数据库操作异常: {ex.Message}");
                }
                throw;
            }
        }

        /// <summary>
        /// 尝试保存更改，如果失败则返回详细的错误信息而不抛出异常
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="errorMessage">输出的错误信息</param>
        /// <param name="logToFile">是否记录到文件</param>
        /// <param name="customMessage">自定义异常消息</param>
        /// <returns>是否保存成功</returns>
        public static bool TrySaveChanges(this DbContext dbContext, out string errorMessage, bool logToFile = true, string customMessage = "")
        {
            try
            {
                dbContext.SaveChanges();
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                // 获取简化的异常信息
                if (ex is Microsoft.EntityFrameworkCore.DbUpdateException || 
                    ex.InnerException is Microsoft.Data.SqlClient.SqlException ||
                    ex.InnerException?.GetType().Name.Contains("MySql") == true)
                {
                    errorMessage = ex.GetSimplifiedEFCoreException(customMessage);
                    
                    // 记录详细信息到文件
                    if (logToFile)
                    {
                        try
                        {
                            string detailedMessage = ex.GetDetailedEFCoreException(customMessage);
                            HDLogHelper.Log($"EFCore_TrySaveChanges_{DateTime.Now:yyyyMMddHHmmss}", detailedMessage, "EFCore_TrySaveChanges_Errors");
                        }
                        catch (Exception logEx)
                        {
                            Console.WriteLine($"记录EF Core异常时出错: {logEx.Message}");
                        }
                    }
                }
                else
                {
                    errorMessage = ex.Format(customMessage, true);
                }

                return false;
            }
        }

        /// <summary>
        /// 尝试异步保存更改，如果失败则返回详细的错误信息而不抛出异常
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="logToFile">是否记录到文件</param>
        /// <param name="customMessage">自定义异常消息</param>
        /// <returns>保存结果和错误信息</returns>
        public static async Task<(bool Success, string ErrorMessage)> TrySaveChangesAsync(this DbContext dbContext, bool logToFile = true, string customMessage = "")
        {
            try
            {
                await dbContext.SaveChangesAsync();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                // 获取简化的异常信息
                string errorMessage;
                if (ex is Microsoft.EntityFrameworkCore.DbUpdateException || 
                    ex.InnerException is Microsoft.Data.SqlClient.SqlException ||
                    ex.InnerException?.GetType().Name.Contains("MySql") == true)
                {
                    errorMessage = ex.GetSimplifiedEFCoreException(customMessage);
                    
                    // 记录详细信息到文件
                    if (logToFile)
                    {
                        try
                        {
                            string detailedMessage = ex.GetDetailedEFCoreException(customMessage);
                            HDLogHelper.Log($"EFCore_TrySaveChangesAsync_{DateTime.Now:yyyyMMddHHmmss}", detailedMessage, "EFCore_TrySaveChanges_Errors");
                        }
                        catch (Exception logEx)
                        {
                            Console.WriteLine($"记录EF Core异常时出错: {logEx.Message}");
                        }
                    }
                }
                else
                {
                    errorMessage = ex.Format(customMessage, true);
                }

                return (false, errorMessage);
            }
        }

        /// <summary>
        /// 获取当前DbContext中所有待保存的实体信息（用于调试）
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <returns>实体信息字符串</returns>
        public static string GetPendingChangesInfo(this DbContext dbContext)
        {
            var sb = new StringBuilder();
            sb.AppendLine("【待保存的实体变更信息】");
            sb.AppendLine($"【检查时间】：{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine();

            var entries = dbContext.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            if (!entries.Any())
            {
                sb.AppendLine("没有待保存的变更。");
                return sb.ToString();
            }

            sb.AppendLine($"【待保存实体数量】：{entries.Count}");
            sb.AppendLine();

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                sb.AppendLine($"【实体 {i + 1}】");
                sb.AppendLine($"  实体类型: {entry.Entity.GetType().Name}");
                sb.AppendLine($"  实体状态: {entry.State}");

                if (entry.State == EntityState.Modified)
                {
                    sb.AppendLine("  【修改的属性】:");
                    foreach (var prop in entry.Properties.Where(p => p.IsModified))
                    {
                        sb.AppendLine($"    {prop.Metadata.Name}: {prop.OriginalValue} → {prop.CurrentValue}");
                    }
                }
                else if (entry.State == EntityState.Added)
                {
                    sb.AppendLine("  【新增的属性值】:");
                    foreach (var prop in entry.Properties)
                    {
                        sb.AppendLine($"    {prop.Metadata.Name}: {prop.CurrentValue}");
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    sb.AppendLine("  【删除的实体属性】:");
                    foreach (var prop in entry.Properties)
                    {
                        sb.AppendLine($"    {prop.Metadata.Name}: {prop.OriginalValue}");
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// 验证实体数据并返回验证结果
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <returns>验证结果</returns>
        public static (bool IsValid, List<string> ValidationErrors) ValidateEntities(this DbContext dbContext)
        {
            var validationErrors = new List<string>();
            var entries = dbContext.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in entries)
            {
                var entity = entry.Entity;
                var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(entity);
                var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

                bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                    entity, validationContext, validationResults, true);

                if (!isValid)
                {
                    foreach (var validationResult in validationResults)
                    {
                        var memberNames = validationResult.MemberNames.Any() 
                            ? string.Join(", ", validationResult.MemberNames)
                            : "未知字段";
                        
                        validationErrors.Add($"{entity.GetType().Name}.{memberNames}: {validationResult.ErrorMessage}");
                    }
                }
            }

            return (validationErrors.Count == 0, validationErrors);
        }
    }
}
