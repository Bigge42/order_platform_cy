using Microsoft.EntityFrameworkCore;
using HDPro.Utilities.Interceptors;
using System;

namespace HDPro.Utilities.Extensions
{
    /// <summary>
    /// DbContext扩展方法类
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// 使用Decimal拦截器，自动控制所有decimal类型属性的精度
        /// </summary>
        /// <param name="optionsBuilder">数据库上下文选项构造器</param>
        /// <returns>数据库上下文选项构造器</returns>
        public static DbContextOptionsBuilder UseDecimalInterceptor(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(new DecimalInterceptor());
            return optionsBuilder;
        }
    }
} 