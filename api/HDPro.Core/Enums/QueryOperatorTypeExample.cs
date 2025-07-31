using System;
using System.Collections.Generic;
using System.Linq;

namespace HDPro.Core.Enums
{
    /// <summary>
    /// 搜索条件类
    /// </summary>
    public class SearchCondition
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }
    }

    /// <summary>
    /// QueryOperatorType 使用示例
    /// </summary>
    public static class QueryOperatorTypeExample
    {
        /// <summary>
        /// 演示如何使用 QueryOperatorType 枚举和扩展方法
        /// </summary>
        public static void Demo()
        {
            Console.WriteLine("=== QueryOperatorType 使用示例 ===");

            // 1. 获取所有查询操作符列表
            Console.WriteLine("\n1. 获取所有查询操作符列表:");
            var operatorList = QueryOperatorTypeExtensions.GetQueryOperatorList();
            foreach (var item in operatorList)
            {
                Console.WriteLine($"  {item.Key} -> {item.Value}");
            }

            // 2. 获取字典形式
            Console.WriteLine("\n2. 获取字典形式:");
            var operatorDict = QueryOperatorTypeExtensions.GetQueryOperatorDictionary();
            foreach (var kvp in operatorDict)
            {
                Console.WriteLine($"  {kvp.Key} -> {kvp.Value}");
            }

            // 3. 根据键获取值
            Console.WriteLine("\n3. 根据键获取值:");
            Console.WriteLine($"  'like' -> {QueryOperatorTypeExtensions.GetValueByKey("like")}");
            Console.WriteLine($"  '=' -> {QueryOperatorTypeExtensions.GetValueByKey("=")}");
            Console.WriteLine($"  'EMPTY' -> {QueryOperatorTypeExtensions.GetValueByKey("EMPTY")}");

            // 4. 根据值获取键
            Console.WriteLine("\n4. 根据值获取键:");
            Console.WriteLine($"  '模糊查询(包含)' -> {QueryOperatorTypeExtensions.GetKeyByValue("模糊查询(包含)")}");
            Console.WriteLine($"  '等于' -> {QueryOperatorTypeExtensions.GetKeyByValue("等于")}");
            Console.WriteLine($"  '空' -> {QueryOperatorTypeExtensions.GetKeyByValue("空")}");

            // 5. 枚举值使用
            Console.WriteLine("\n5. 枚举值使用:");
            var equalOperator = QueryOperatorType.Equal;
            Console.WriteLine($"  {equalOperator} -> {equalOperator.GetDescription()}");
            Console.WriteLine($"  {equalOperator} -> {equalOperator.GetKey()}");

            var likeOperator = QueryOperatorType.Like;
            Console.WriteLine($"  {likeOperator} -> {likeOperator.GetDescription()}");
            Console.WriteLine($"  {likeOperator} -> {likeOperator.GetKey()}");

            // 6. 根据键获取枚举值
            Console.WriteLine("\n6. 根据键获取枚举值:");
            var enum1 = QueryOperatorTypeExtensions.GetEnumByKey("like");
            Console.WriteLine($"  'like' -> {enum1} ({enum1?.GetDescription()})");

            var enum2 = QueryOperatorTypeExtensions.GetEnumByKey("EMPTY");
            Console.WriteLine($"  'EMPTY' -> {enum2} ({enum2?.GetDescription()})");

            // 7. 在查询条件中使用
            Console.WriteLine("\n7. 在查询条件中使用:");
            var searchConditions = new List<SearchCondition>
            {
                new SearchCondition { Field = "Name", Operator = "like", Value = "张三" },
                new SearchCondition { Field = "Age", Operator = "=", Value = "25" },
                new SearchCondition { Field = "Email", Operator = "EMPTY", Value = null }
            };

            foreach (var condition in searchConditions)
            {
                var operatorEnum = QueryOperatorTypeExtensions.GetEnumByKey(condition.Operator);
                Console.WriteLine($"  字段: {condition.Field}, 操作符: {condition.Operator} ({operatorEnum?.GetDescription()}), 值: {condition.Value}");
            }

            // 8. 获取特定类型的操作符
            Console.WriteLine("\n8. 获取特定类型的操作符:");
            var textOperators = operatorList.Where(x => x.Value.Contains("模糊") || x.Value.Contains("包含")).ToList();
            Console.WriteLine("  文本查询操作符:");
            foreach (var op in textOperators)
            {
                Console.WriteLine($"    {op.Key} -> {op.Value}");
            }

            var dateOperators = operatorList.Where(x => x.Value.Contains("date") || x.Value.Contains("time") || x.Value.Contains("年") || x.Value.Contains("月")).ToList();
            Console.WriteLine("  日期时间操作符:");
            foreach (var op in dateOperators)
            {
                Console.WriteLine($"    {op.Key} -> {op.Value}");
            }

            // 9. 构建查询条件示例
            Console.WriteLine("\n9. 构建查询条件示例:");
            var queryString = BuildQueryConditions(searchConditions);
            Console.WriteLine($"  生成的查询条件: {queryString}");
        }

        /// <summary>
        /// 构建查询条件的示例方法
        /// </summary>
        /// <param name="conditions">搜索条件列表</param>
        /// <returns>查询条件字符串</returns>
        public static string BuildQueryConditions(List<SearchCondition> conditions)
        {
            var result = new List<string>();

            foreach (var condition in conditions)
            {
                var operatorEnum = QueryOperatorTypeExtensions.GetEnumByKey(condition.Operator);
                if (operatorEnum == null) continue;

                switch (operatorEnum)
                {
                    case QueryOperatorType.Equal:
                        result.Add($"{condition.Field} = '{condition.Value}'");
                        break;
                    case QueryOperatorType.NotEqual:
                        result.Add($"{condition.Field} != '{condition.Value}'");
                        break;
                    case QueryOperatorType.Like:
                        result.Add($"{condition.Field} LIKE '%{condition.Value}%'");
                        break;
                    case QueryOperatorType.LikeStart:
                        result.Add($"{condition.Field} LIKE '{condition.Value}%'");
                        break;
                    case QueryOperatorType.LikeEnd:
                        result.Add($"{condition.Field} LIKE '%{condition.Value}'");
                        break;
                    case QueryOperatorType.Empty:
                        result.Add($"{condition.Field} IS NULL OR {condition.Field} = ''");
                        break;
                    case QueryOperatorType.NotEmpty:
                        result.Add($"{condition.Field} IS NOT NULL AND {condition.Field} != ''");
                        break;
                    default:
                        result.Add($"{condition.Field} {condition.Operator} '{condition.Value}'");
                        break;
                }
            }

            return string.Join(" AND ", result);
        }
    }
} 