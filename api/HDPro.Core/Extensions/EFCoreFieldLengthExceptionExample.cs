using Microsoft.EntityFrameworkCore;
using HDPro.Utilities.Extensions;
using HDPro.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HDPro.Core.Extensions
{
    /// <summary>
    /// EF Core字段长度异常处理示例
    /// 展示如何使用增强的异常处理来快速定位字段长度超限问题
    /// </summary>
    public class EFCoreFieldLengthExceptionExample
    {
        private readonly DbContext _context;

        public EFCoreFieldLengthExceptionExample(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 示例1：使用详细异常处理保存实体，自动检测字段长度问题
        /// </summary>
        public async Task<bool> SaveEntityWithLengthCheckAsync<T>(T entity) where T : class
        {
            try
            {
                _context.Set<T>().Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // 使用增强的异常处理，自动检测字段长度超限问题
                string detailedError = ex.GetDetailedEFCoreException("保存实体时发生异常");
                
                // 记录详细异常信息到日志
                // HDLogHelper.WriteLog(typeof(EFCoreFieldLengthExceptionExample), detailedError);
                
                // 抛出简化的异常信息给用户
                throw new Exception($"保存失败: {ex.GetSimplifiedEFCoreException()}");
            }
        }

        /// <summary>
        /// 示例2：批量保存时的字段长度检查
        /// </summary>
        public async Task<bool> SaveMultipleEntitiesAsync<T>(params T[] entities) where T : class
        {
            try
            {
                foreach (var entity in entities)
                {
                    _context.Set<T>().Add(entity);
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // 使用LogAndThrowEFCoreException一键式处理
                ex.LogAndThrowEFCoreException("批量保存实体时发生异常");
                return false; // 不会执行到这里
            }
        }

        /// <summary>
        /// 示例3：更新实体时的字段长度检查
        /// </summary>
        public async Task<bool> UpdateEntityWithLengthCheckAsync<T>(T entity) where T : class
        {
            try
            {
                _context.Set<T>().Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // 专门处理DbUpdateException，获取详细的字段信息
                string detailedInfo = ex.GetDetailedEFCoreException("更新实体时字段长度异常");
                
                // 记录到日志
                // HDLogHelper.WriteLog(typeof(EFCoreFieldLengthExceptionExample), detailedInfo);
                
                // 检查是否是字段长度问题
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 8152)
                {
                    throw new Exception("数据长度超出限制，请检查输入的数据长度是否符合要求");
                }
                
                throw new Exception($"更新失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例4：使用TrySaveChanges方法进行安全保存
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> TrySaveEntityAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Add(entity);
            
            // 使用扩展方法进行安全保存
            var result = await _context.TrySaveChangesAsync();
            
            if (!result.Success)
            {
                // 错误信息已经自动记录到HDLogHelper
                // 这里只需要返回用户友好的错误信息
                return (false, result.ErrorMessage);
            }
            
            return (true, "保存成功");
        }

        /// <summary>
        /// 示例5：预检查实体字段长度
        /// </summary>
        public string ValidateEntityFieldLengths<T>(T entity) where T : class
        {
            try
            {
                var entry = _context.Entry(entity);
                var validationResults = new List<string>();
                
                foreach (var prop in entry.Properties)
                {
                    var currentValue = prop.CurrentValue;
                    var maxLength = prop.Metadata.GetMaxLength();
                    
                    if (maxLength.HasValue && currentValue != null)
                    {
                        string valueStr = currentValue.ToString();
                        if (valueStr.Length > maxLength.Value)
                        {
                            validationResults.Add($"字段 [{prop.Metadata.Name}] 长度超限: 当前{valueStr.Length}字符，最大允许{maxLength.Value}字符");
                        }
                    }
                }
                
                return validationResults.Any() 
                    ? string.Join("; ", validationResults)
                    : "所有字段长度验证通过";
            }
            catch (Exception ex)
            {
                return $"验证过程中发生异常: {ex.Message}";
            }
        }

        /// <summary>
        /// 示例6：Repository模式中的字段长度异常处理
        /// </summary>
        public class EntityRepository<T> where T : class
        {
            private readonly DbContext _context;

            public EntityRepository(DbContext context)
            {
                _context = context;
            }

            public async Task<T> AddAsync(T entity)
            {
                try
                {
                    _context.Set<T>().Add(entity);
                    await _context.SaveChangesAsync();
                    return entity;
                }
                catch (Exception ex)
                {
                    // 使用增强的异常处理
                    string detailedError = ex.GetDetailedEFCoreException($"添加{typeof(T).Name}实体时发生异常");
                    // HDLogHelper.WriteLog(typeof(EntityRepository<T>), detailedError);
                    
                    // 根据异常类型返回不同的用户友好信息
                    if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                    {
                        switch (sqlEx.Number)
                        {
                            case 8152:
                                throw new Exception("输入的数据长度超出限制，请检查字段内容长度");
                            case 515:
                                throw new Exception("必填字段不能为空");
                            case 2627:
                                throw new Exception("数据重复，违反唯一性约束");
                            default:
                                throw new Exception($"数据库操作失败: {sqlEx.Message}");
                        }
                    }
                    
                    throw new Exception($"添加{typeof(T).Name}失败: {ex.Message}");
                }
            }

            public async Task<T> UpdateAsync(T entity)
            {
                try
                {
                    _context.Set<T>().Update(entity);
                    await _context.SaveChangesAsync();
                    return entity;
                }
                catch (Exception ex)
                {
                    // 一键式异常处理和日志记录
                    ex.LogAndThrowEFCoreException($"更新{typeof(T).Name}实体时发生异常");
                    return null; // 不会执行到这里
                }
            }
        }
    }
} 