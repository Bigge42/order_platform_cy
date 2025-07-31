using Microsoft.EntityFrameworkCore;
using HDPro.Core.Extensions;
using HDPro.Utilities.Extensions;
using System;
using System.Threading.Tasks;

namespace HDPro.Core.Extensions
{
    /// <summary>
    /// EF Core简化异常处理使用示例
    /// 专注于SQL错误信息，不输出详细的实体字段信息
    /// </summary>
    public class EFCoreSimplifiedExceptionExample
    {
        private readonly DbContext _dbContext;

        public EFCoreSimplifiedExceptionExample(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 示例1：使用SafeSaveChanges，自动记录到HDLogHelper并抛出简化异常
        /// </summary>
        public void Example1_SafeSaveChangesWithAutoLogging()
        {
            try
            {
                // 添加一些实体操作...
                // _dbContext.Set<YourEntity>().Add(entity);

                // 使用SafeSaveChanges，会自动记录详细信息到HDLogHelper，抛出简化异常
                int affectedRows = _dbContext.SafeSaveChanges(
                    logToFile: true,           // 记录详细信息到HDLogHelper
                    customMessage: "保存订单数据"
                );

                Console.WriteLine($"成功保存，影响行数：{affectedRows}");
            }
            catch (Exception ex)
            {
                // 这里捕获的是简化后的异常，包含关键SQL错误信息
                Console.WriteLine($"保存失败：{ex.Message}");
                
                // 异常消息示例：
                // 实体保存异常: An error occurred while saving the entity changes.
                // 内部异常: String or binary data would be truncated.
                // SQL错误码: 8152
                // SQL错误信息: String or binary data would be truncated.
                // 错误说明: 字符串或二进制数据会被截断
            }
        }

        /// <summary>
        /// 示例2：使用TrySaveChanges，不抛出异常，返回简化错误信息
        /// </summary>
        public void Example2_TrySaveChangesWithSimplifiedError()
        {
            // 添加一些实体操作...
            // _dbContext.Set<YourEntity>().Add(entity);

            // 尝试保存，如果失败返回简化错误信息
            bool success = _dbContext.TrySaveChanges(
                out string errorMessage,
                logToFile: true,           // 记录详细信息到HDLogHelper
                customMessage: "尝试保存用户数据"
            );

            if (success)
            {
                Console.WriteLine("保存成功！");
            }
            else
            {
                Console.WriteLine("保存失败，错误信息：");
                Console.WriteLine(errorMessage);
                
                // 可以根据错误信息进行相应的业务处理
                if (errorMessage.Contains("SQL错误码: 2627"))
                {
                    Console.WriteLine("检测到主键冲突，请检查数据唯一性");
                }
                else if (errorMessage.Contains("SQL错误码: 8152"))
                {
                    Console.WriteLine("检测到数据长度超限，请缩短输入内容");
                }
            }
        }

        /// <summary>
        /// 示例3：手动使用LogAndThrowEFCoreException
        /// </summary>
        public void Example3_ManualLogAndThrow()
        {
            try
            {
                // 执行一些可能出错的EF Core操作
                _dbContext.SaveChanges();
            }
            catch (Exception ex) when (ex is DbUpdateException)
            {
                // 手动记录到HDLogHelper并抛出简化异常
                ex.LogAndThrowEFCoreException("手动处理的实体保存异常");
            }
        }

        /// <summary>
        /// 示例4：只获取简化异常信息，不记录日志
        /// </summary>
        public void Example4_GetSimplifiedErrorOnly()
        {
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception ex) when (ex is DbUpdateException)
            {
                // 只获取简化的异常信息，不记录到文件
                string simplifiedError = ex.GetSimplifiedEFCoreException("获取简化错误信息");
                
                Console.WriteLine("简化的错误信息：");
                Console.WriteLine(simplifiedError);
                
                // 可以根据需要决定是否重新抛出异常
                // throw new Exception(simplifiedError, ex);
            }
        }

        /// <summary>
        /// 示例5：在Repository模式中的使用
        /// </summary>
        public class OrderRepository
        {
            private readonly DbContext _context;

            public OrderRepository(DbContext context)
            {
                _context = context;
            }

            public async Task<(bool Success, string ErrorMessage)> CreateOrderAsync(object order)
            {
                try
                {
                    // _context.Set<Order>().Add(order);
                    
                    // 使用安全的保存方法，自动记录详细信息到HDLogHelper
                    await _context.SafeSaveChangesAsync(
                        logToFile: true,
                        customMessage: $"创建订单 - OrderId: {order.GetHashCode()}"
                    );
                    
                    return (true, string.Empty);
                }
                catch (Exception ex)
                {
                    // 返回用户友好的错误信息
                    string userFriendlyMessage = GetUserFriendlyErrorMessage(ex.Message);
                    return (false, userFriendlyMessage);
                }
            }

            public (bool Success, string ErrorMessage) UpdateOrder(object order)
            {
                // _context.Set<Order>().Update(order);
                
                // 使用TrySaveChanges，不抛出异常
                bool success = _context.TrySaveChanges(
                    out string errorMessage,
                    logToFile: true,
                    customMessage: $"更新订单 - OrderId: {order.GetHashCode()}"
                );

                if (success)
                {
                    return (true, string.Empty);
                }
                else
                {
                    string userFriendlyMessage = GetUserFriendlyErrorMessage(errorMessage);
                    return (false, userFriendlyMessage);
                }
            }

            private string GetUserFriendlyErrorMessage(string errorMessage)
            {
                // 根据简化的异常信息返回用户友好的错误信息
                if (errorMessage.Contains("SQL错误码: 2627"))
                {
                    return "订单号已存在，请使用不同的订单号";
                }
                else if (errorMessage.Contains("SQL错误码: 547"))
                {
                    return "关联的数据不存在，请检查相关信息";
                }
                else if (errorMessage.Contains("SQL错误码: 8152"))
                {
                    return "输入的数据过长，请缩短内容";
                }
                else if (errorMessage.Contains("SQL错误码: 515"))
                {
                    return "必填字段不能为空，请检查输入";
                }
                else if (errorMessage.Contains("MySQL错误码: 1062"))
                {
                    return "数据重复，请检查唯一性约束";
                }
                else if (errorMessage.Contains("MySQL错误码: 1048"))
                {
                    return "必填字段不能为空";
                }
                else if (errorMessage.Contains("MySQL错误码: 1452"))
                {
                    return "外键约束失败，请检查关联数据";
                }

                return "保存数据时发生错误，请联系管理员";
            }
        }
    }
}

/*
简化异常处理的优势：

1. 专注关键信息
   - 只输出SQL错误码、错误信息和说明
   - 不输出冗长的实体字段详情
   - 便于快速定位问题

2. 自动日志记录
   - 详细信息自动记录到HDLogHelper
   - 简化信息用于异常抛出
   - 分离了日志记录和异常处理

3. 用户友好
   - 可以根据SQL错误码提供用户友好的错误提示
   - 避免向用户暴露技术细节
   - 便于前端错误处理

4. 性能优化
   - 减少了异常信息的构建时间
   - 降低了内存占用
   - 提高了异常处理效率

使用建议：
- 开发环境：使用SafeSaveChanges，便于调试
- 生产环境：使用TrySaveChanges，优雅处理错误
- 关键业务：结合两种方式，确保数据安全
*/ 