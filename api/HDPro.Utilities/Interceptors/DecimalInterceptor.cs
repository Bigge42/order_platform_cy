using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using HDPro.Utilities.Attributes;
using HDPro.Utilities.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HDPro.Utilities.Interceptors
{
    /// <summary>
    /// Decimal值拦截器，自动控制所有实体中的decimal类型字段的精度
    /// 防止数据库decimal溢出问题
    /// </summary>
    public class DecimalInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context != null)
            {
                ProcessDecimalProperties(eventData.Context);
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
            {
                ProcessDecimalProperties(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ProcessDecimalProperties(DbContext context)
        {
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType();
                var decimalProps = entityType.GetProperties()
                    .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

                foreach (var prop in decimalProps)
                {
                    // 跳过未修改的属性（对于Modified状态的实体）
                    if (entry.State == EntityState.Modified && 
                        !entry.Property(prop.Name).IsModified)
                    {
                        continue;
                    }

                    // 获取字段的精度信息
                    int precision = GetPrecisionFromProperty(prop);
                    
                    // 处理非空值
                    if (prop.PropertyType == typeof(decimal))
                    {
                        decimal originalValue = (decimal)prop.GetValue(entry.Entity);
                        decimal roundedValue = originalValue.ControlPrecision(precision);
                        
                        if (originalValue != roundedValue)
                        {
                            prop.SetValue(entry.Entity, roundedValue);
                        }
                    }
                    // 处理可空值
                    else if (prop.PropertyType == typeof(decimal?))
                    {
                        decimal? originalValue = (decimal?)prop.GetValue(entry.Entity);
                        if (originalValue.HasValue)
                        {
                            decimal roundedValue = originalValue.Value.ControlPrecision(precision);
                            
                            if (originalValue.Value != roundedValue)
                            {
                                prop.SetValue(entry.Entity, roundedValue);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从属性的特性中获取精度信息
        /// </summary>
        private int GetPrecisionFromProperty(PropertyInfo prop)
        {
            // 默认精度
            int defaultPrecision = 6;
            
            // 优先从DecimalPrecision特性获取精度
            var decimalPrecisionAttr = prop.GetCustomAttribute<DecimalPrecisionAttribute>();
            if (decimalPrecisionAttr != null)
            {
                return decimalPrecisionAttr.Precision;
            }
            
            // 其次从DisplayFormat特性获取精度
            var displayFormatAttr = prop.GetCustomAttribute<DisplayFormatAttribute>();
            if (displayFormatAttr != null && !string.IsNullOrEmpty(displayFormatAttr.DataFormatString))
            {
                return DecimalExtension.ParsePrecision(displayFormatAttr.DataFormatString);
            }
            
            // 最后从Column特性的TypeName获取精度
            var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
            if (columnAttr != null && !string.IsNullOrEmpty(columnAttr.TypeName))
            {
                string typeName = columnAttr.TypeName.ToLower();
                if (typeName == "decimal(18,2)")
                {
                    return 2;
                }
                else if (typeName.StartsWith("decimal"))
                {
                    // 尝试解析decimal(x,y)格式
                    int startBracket = typeName.IndexOf('(');
                    int comma = typeName.IndexOf(',');
                    int endBracket = typeName.IndexOf(')');
                    
                    if (startBracket > 0 && comma > startBracket && endBracket > comma)
                    {
                        string precisionStr = typeName.Substring(comma + 1, endBracket - comma - 1).Trim();
                        if (int.TryParse(precisionStr, out int parsedPrecision))
                        {
                            return parsedPrecision;
                        }
                    }
                }
            }
            
            return defaultPrecision;
        }
    }
} 