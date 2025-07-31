using Microsoft.EntityFrameworkCore;
using HDPro.Core.Extensions;
using HDPro.Utilities.Extensions;
using System;
using System.Threading.Tasks;

namespace HDPro.Core.Extensions
{
    /// <summary>
    /// EF Core异常处理使用示例
    /// 展示如何使用增强的异常处理功能来获取详细的字段异常信息
    /// </summary>
    public class EFCoreExceptionHandlingExample
    {
        private readonly DbContext _dbContext;

        public EFCoreExceptionHandlingExample(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 示例1：使用SafeSaveChanges方法，自动记录详细异常信息
        /// </summary>
        public void Example1_SafeSaveChanges()
        {
            try
            {
                // 添加一些实体操作...
                // _dbContext.Set<YourEntity>().Add(entity);

                // 使用SafeSaveChanges，会自动捕获并记录详细的异常信息
                int affectedRows = _dbContext.SafeSaveChanges(
                    logToFile: true,           // 记录到文件
                    logToConsole: true,        // 输出到控制台
                    customMessage: "保存订单数据时发生异常"
                );

                Console.WriteLine($"成功保存，影响行数：{affectedRows}");
            }
            catch (Exception ex)
            {
                // 异常已经被记录，这里可以进行业务层面的处理
                Console.WriteLine($"保存失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 示例2：使用TrySaveChanges方法，不抛出异常，返回详细错误信息
        /// </summary>
        public void Example2_TrySaveChanges()
        {
            // 添加一些实体操作...
            // _dbContext.Set<YourEntity>().Add(entity);

            // 尝试保存，如果失败返回详细错误信息而不抛出异常
            bool success = _dbContext.TrySaveChanges(
                out string errorMessage,
                logToFile: true,
                customMessage: "尝试保存用户数据"
            );

            if (success)
            {
                Console.WriteLine("保存成功！");
            }
            else
            {
                Console.WriteLine("保存失败，详细错误信息：");
                Console.WriteLine(errorMessage);
                
                // 可以根据错误信息进行相应的业务处理
                // 例如：向用户显示友好的错误提示
            }
        }

        /// <summary>
        /// 示例3：异步版本的TrySaveChanges
        /// </summary>
        public async Task Example3_TrySaveChangesAsync()
        {
            // 添加一些实体操作...
            // _dbContext.Set<YourEntity>().Add(entity);

            var (success, errorMessage) = await _dbContext.TrySaveChangesAsync(
                logToFile: true,
                customMessage: "异步保存产品数据"
            );

            if (success)
            {
                Console.WriteLine("异步保存成功！");
            }
            else
            {
                Console.WriteLine("异步保存失败，详细错误信息：");
                Console.WriteLine(errorMessage);
            }
        }

        /// <summary>
        /// 示例4：在保存前检查待保存的实体信息
        /// </summary>
        public void Example4_CheckPendingChanges()
        {
            // 添加一些实体操作...
            // _dbContext.Set<YourEntity>().Add(entity);
            // _dbContext.Set<YourEntity>().Update(existingEntity);

            // 获取待保存的实体变更信息
            string pendingChangesInfo = _dbContext.GetPendingChangesInfo();
            Console.WriteLine("保存前的实体变更信息：");
            Console.WriteLine(pendingChangesInfo);

            // 验证实体数据
            var (isValid, validationErrors) = _dbContext.ValidateEntities();
            if (!isValid)
            {
                Console.WriteLine("实体验证失败：");
                foreach (var error in validationErrors)
                {
                    Console.WriteLine($"- {error}");
                }
                return;
            }

            // 执行保存
            try
            {
                _dbContext.SafeSaveChanges(customMessage: "保存验证通过的数据");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存时发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 示例5：手动使用详细异常信息方法
        /// </summary>
        public void Example5_ManualExceptionHandling()
        {
            try
            {
                // 执行一些可能出错的EF Core操作
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // 手动获取详细的异常信息
                string detailedInfo = ex.GetDetailedEFCoreException("手动处理的异常");
                
                // 写入文件
                ex.WriteToFile("手动处理异常", "Manual_Exceptions");
                
                // 输出到控制台
                ex.WriteToConsole("手动处理异常");
                
                // 也可以直接使用详细信息
                Console.WriteLine(detailedInfo);
            }
        }

        /// <summary>
        /// 示例6：在Repository模式中的使用
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
                    
                    // 使用安全的保存方法
                    await _context.SafeSaveChangesAsync(
                        logToFile: true,
                        logToConsole: false,
                        customMessage: $"创建订单 - OrderId: {order.GetHashCode()}"
                    );
                    
                    return (true, string.Empty);
                }
                catch (Exception ex)
                {
                    // 返回用户友好的错误信息
                    string userFriendlyMessage = GetUserFriendlyErrorMessage(ex);
                    return (false, userFriendlyMessage);
                }
            }

            private string GetUserFriendlyErrorMessage(Exception ex)
            {
                // 根据异常类型返回用户友好的错误信息
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    return sqlEx.Number switch
                    {
                        2627 => "订单号已存在，请使用不同的订单号",
                        547 => "关联的数据不存在，请检查相关信息",
                        8152 => "输入的数据过长，请缩短内容",
                        _ => "保存订单时发生数据库错误，请联系管理员"
                    };
                }

                if (ex is Microsoft.EntityFrameworkCore.DbUpdateException)
                {
                    return "保存数据时发生错误，请检查输入的数据是否正确";
                }

                return "保存订单时发生未知错误，请稍后重试";
            }
        }
    }
}

/*
使用说明：

1. SafeSaveChanges / SafeSaveChangesAsync
   - 自动捕获异常并记录详细信息
   - 仍然会抛出异常，保持原有的异常处理流程
   - 适合需要详细日志但不改变现有异常处理逻辑的场景

2. TrySaveChanges / TrySaveChangesAsync
   - 不抛出异常，返回成功状态和错误信息
   - 适合需要优雅处理错误的场景
   - 可以根据返回的错误信息进行相应的业务处理

3. GetPendingChangesInfo
   - 获取当前待保存的实体变更信息
   - 用于调试和日志记录
   - 可以在保存前检查将要执行的操作

4. ValidateEntities
   - 在保存前验证实体数据
   - 返回验证结果和错误列表
   - 可以提前发现数据验证问题

5. GetDetailedEFCoreException
   - 获取详细的EF Core异常信息
   - 包括实体属性、约束信息、SQL错误等
   - 可以手动调用以获取详细的异常分析

配置建议：
- 开发环境：启用控制台输出，便于调试
- 生产环境：只记录到文件，避免敏感信息泄露
- 根据业务需要选择合适的异常处理方式
*/ 