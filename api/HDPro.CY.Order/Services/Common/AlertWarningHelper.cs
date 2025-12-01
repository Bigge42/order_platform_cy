/*
 * 预警功能通用助手类
 * 用于在GetPageData中标记需要预警的数据行
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HDPro.Entity.DomainModels;
using Microsoft.Extensions.Logging;

namespace HDPro.CY.Order.Services.Common
{
    /// <summary>
    /// 预警功能通用助手类
    /// </summary>
    public static class AlertWarningHelper
    {
        /// <summary>
        /// 为数据行添加预警标记
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="rows">数据行列表</param>
        /// <param name="rules">预警规则列表</param>
        /// <param name="logger">日志记录器(可选)</param>
        public static void ApplyAlertWarning<T>(List<T> rows, List<OCP_AlertRules> rules, ILogger logger = null) where T : class
        {
            if (rows == null || !rows.Any() || rules == null || !rules.Any())
            {
                return;
            }

            logger?.LogInformation($"开始应用预警规则,数据行数: {rows.Count}, 规则数: {rules.Count}");

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 检查是否有ShouldAlert属性
            var shouldAlertProp = properties.FirstOrDefault(p => p.Name == "ShouldAlert" && p.PropertyType == typeof(bool));
            if (shouldAlertProp == null)
            {
                logger?.LogWarning($"实体类型 {type.Name} 没有ShouldAlert属性,无法应用预警标记");
                return;
            }

            int alertCount = 0;

            foreach (var row in rows)
            {
                bool shouldAlert = false;

                foreach (var rule in rules)
                {
                    if (CheckRowMatchesRule(row, rule, properties, logger))
                    {
                        shouldAlert = true;
                        break; // 只要有一个规则匹配就预警
                    }
                }

                if (shouldAlert)
                {
                    shouldAlertProp.SetValue(row, true);
                    alertCount++;
                }
            }

            logger?.LogInformation($"预警标记应用完成,预警行数: {alertCount}");
        }

        /// <summary>
        /// 检查数据行是否匹配预警规则
        /// </summary>
        private static bool CheckRowMatchesRule<T>(T row, OCP_AlertRules rule, PropertyInfo[] properties, ILogger logger) where T : class
        {
            try
            {
                // 1. 检查完成状态
                if (IsCompleted(row, rule, properties))
                {
                    return false; // 已完成,不预警
                }

                // 2. 检查日期字段
                var dateFields = rule.FieldName?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                foreach (var fieldName in dateFields)
                {
                    var trimmedFieldName = fieldName.Trim();
                    var dateProp = properties.FirstOrDefault(p => p.Name.Equals(trimmedFieldName, StringComparison.OrdinalIgnoreCase));

                    if (dateProp == null)
                    {
                        continue;
                    }

                    var dateValue = dateProp.GetValue(row);
                    if (dateValue == null)
                    {
                        continue;
                    }

                    DateTime targetDate;
                    if (dateValue is DateTime dt)
                    {
                        targetDate = dt;
                    }
                    else if (DateTime.TryParse(dateValue.ToString(), out DateTime parsedDate))
                    {
                        targetDate = parsedDate;
                    }
                    else
                    {
                        continue;
                    }

                    var now = DateTime.Now;

                    // 检查是否超期
                    if (rule.DayCount > 0)
                    {
                        var daysSinceTarget = (now - targetDate).TotalDays;
                        if (daysSinceTarget > rule.DayCount)
                        {
                            return true; // 超期预警
                        }
                    }

                    // 检查提前预警
                    if (rule.advanceWarningDays.HasValue && rule.advanceWarningDays.Value > 0)
                    {
                        var daysUntilTarget = (targetDate - now).TotalDays;
                        if (daysUntilTarget >= 0 && daysUntilTarget <= rule.advanceWarningDays.Value)
                        {
                            return true; // 提前预警
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"检查预警规则时发生异常,规则: {rule.RuleName}");
                return false;
            }
        }

        /// <summary>
        /// 检查是否已完成
        /// </summary>
        private static bool IsCompleted<T>(T row, OCP_AlertRules rule, PropertyInfo[] properties) where T : class
        {
            if (string.IsNullOrWhiteSpace(rule.FinishStatusField))
            {
                return false;
            }

            var finishProp = properties.FirstOrDefault(p => p.Name.Equals(rule.FinishStatusField, StringComparison.OrdinalIgnoreCase));
            if (finishProp == null)
            {
                return false;
            }

            var finishValue = finishProp.GetValue(row);

            // 根据完成判定方式判断
            if (rule.ConditionType == "1")//是否有值判断
            {
                // 有值就视为已完成
                return finishValue != null && !string.IsNullOrWhiteSpace(finishValue.ToString());
            }
            else if (rule.ConditionType == "2")//根据完成判定值来判断
            {
                // 值等于判定值时视为已完成
                if (finishValue == null)
                {
                    return false;
                }
                return finishValue.ToString() == rule.ConditionValue;
            }

            return false;
        }
    }
}

