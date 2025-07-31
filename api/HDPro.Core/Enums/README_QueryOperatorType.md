# QueryOperatorType 枚举使用说明

## 概述

`QueryOperatorType` 是一个用于定义查询操作符类型的枚举，包含了常用的数据库查询操作符和前端控件类型。该枚举提供了丰富的扩展方法，方便在代码中使用。

## 枚举值列表

| 枚举值 | 键 | 描述 |
|--------|----|----|
| Equal | = | 等于 |
| NotEqual | != | 不等于 |
| Like | like | 模糊查询(包含) |
| LikeStart | likeStart | 模糊查询(左包含) |
| LikeEnd | likeEnd | 模糊查询(右包含) |
| Textarea | textarea | textarea |
| Switch | switch | switch |
| Select | select | select |
| SelectList | selectList | select多选 |
| Year | year | 年 |
| Date | date | date(年月日) |
| DateTime | datetime | datetime(年月日时分秒) |
| Month | month | year_month |
| Time | time | time |
| Cascader | cascader | 级联 |
| TreeSelect | treeSelect | 树形级联tree-select |
| SelectTable | selectTable | 下拉框Table搜索 |
| Checkbox | checkbox | checkbox |
| Radio | radio | radio |
| Range | range | 区间查询 |
| Mail | mail | mail |
| Number | number | number |
| Decimal | decimal | decimal |
| MultipleInput | multipleInput | 批量查询 |
| Empty | EMPTY | 空 |
| NotEmpty | NOT_EMPTY | 非空 |

## 扩展方法

### 1. GetQueryOperatorList()
获取所有查询操作符的键值对列表。

```csharp
var operatorList = QueryOperatorTypeExtensions.GetQueryOperatorList();
foreach (var item in operatorList)
{
    Console.WriteLine($"{item.Key} -> {item.Value}");
}
```

### 2. GetQueryOperatorDictionary()
获取所有查询操作符的字典。

```csharp
var operatorDict = QueryOperatorTypeExtensions.GetQueryOperatorDictionary();
string value = operatorDict["like"]; // "模糊查询(包含)"
```

### 3. GetValueByKey(string key)
根据键获取对应的值。

```csharp
string value = QueryOperatorTypeExtensions.GetValueByKey("like"); // "模糊查询(包含)"
```

### 4. GetKeyByValue(string value)
根据值获取对应的键。

```csharp
string key = QueryOperatorTypeExtensions.GetKeyByValue("模糊查询(包含)"); // "like"
```

### 5. GetDescription()
获取枚举值的描述信息。

```csharp
var enumValue = QueryOperatorType.Like;
string description = enumValue.GetDescription(); // "模糊查询(包含)"
```

### 6. GetKey()
根据枚举值获取对应的键。

```csharp
var enumValue = QueryOperatorType.Like;
string key = enumValue.GetKey(); // "like"
```

### 7. GetEnumByKey(string key)
根据键获取对应的枚举值。

```csharp
var enumValue = QueryOperatorTypeExtensions.GetEnumByKey("like"); // QueryOperatorType.Like
```

## 使用示例

### 基本使用

```csharp
using HDPro.Core.Enums;

// 获取所有操作符
var operators = QueryOperatorTypeExtensions.GetQueryOperatorList();

// 根据键获取值
string description = QueryOperatorTypeExtensions.GetValueByKey("like");

// 枚举值使用
var likeOperator = QueryOperatorType.Like;
string key = likeOperator.GetKey();
string desc = likeOperator.GetDescription();
```

### 在查询条件中使用

```csharp
public class SearchCondition
{
    public string Field { get; set; }
    public string Operator { get; set; }
    public object Value { get; set; }
}

public string BuildQueryConditions(List<SearchCondition> conditions)
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
            case QueryOperatorType.Like:
                result.Add($"{condition.Field} LIKE '%{condition.Value}%'");
                break;
            case QueryOperatorType.Empty:
                result.Add($"{condition.Field} IS NULL OR {condition.Field} = ''");
                break;
            // ... 其他操作符
        }
    }

    return string.Join(" AND ", result);
}
```

### 前端控件类型判断

```csharp
public bool IsDateType(string operatorKey)
{
    var enumValue = QueryOperatorTypeExtensions.GetEnumByKey(operatorKey);
    if (enumValue == null) return false;

    return enumValue == QueryOperatorType.Date || 
           enumValue == QueryOperatorType.DateTime || 
           enumValue == QueryOperatorType.Year || 
           enumValue == QueryOperatorType.Month || 
           enumValue == QueryOperatorType.Time;
}
```

## 注意事项

1. 枚举值使用 `Description` 特性来提供中文描述
2. 键值映射是硬编码的，确保键的唯一性
3. 扩展方法提供了类型安全的操作
4. 支持空值检查，避免运行时异常

## 相关文件

- `QueryOperatorType.cs` - 枚举定义和扩展方法
- `QueryOperatorTypeExample.cs` - 使用示例
- `README_QueryOperatorType.md` - 本文档 