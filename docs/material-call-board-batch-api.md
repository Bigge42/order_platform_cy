# MaterialCallBoard 批量上传接口使用说明

## 接口概览
- **URL**：`POST /api/MaterialCallBoard/batch-upload`
- **请求体**：`List<MaterialCallBoardBatchDto>`
- **数据模型命名空间**：`HDPro.CY.Order.Models.MaterialCallBoardDtos`
- **返回类型**：`WebResponseContent`

## 请求参数
请求体为 `MaterialCallBoardBatchDto` 对象数组，字段如下：

| 字段 | 类型 | 是否必填 | 说明 |
| ---- | ---- | -------- | ---- |
| `WorkOrderNo` | `string` | 是 | 工单号，唯一标识本次叫料记录 |
| `PlanTrackNo` | `string` | 是 | 计划跟踪号 |
| `ProductCode` | `string` | 是 | 产品编号 |
| `CallerName` | `string` | 是 | 叫料人 |
| `CalledAt` | `DateTime` | 是 | 叫料时间（UTC+8） |

示例请求：

```json
[
  {
    "workOrderNo": "WO20240101",
    "planTrackNo": "PTN-001",
    "productCode": "PRD-1001",
    "callerName": "张三",
    "calledAt": "2024-05-01T08:30:00"
  },
  {
    "workOrderNo": "WO20240102",
    "planTrackNo": "PTN-002",
    "productCode": "PRD-1002",
    "callerName": "李四",
    "calledAt": "2024-05-01T09:15:00"
  }
]
```

> 注意：
> - 数组内每个对象必须提供完整字段。
> - 服务端会自动去除字符串首尾空格，`CalledAt` 为空时默认写入当前时间。

## 返回示例

### 成功
```json
{
  "status": true,
  "code": 200,
  "msg": "批量导入成功",
  "data": null
}
```

### 失败
```json
{
  "status": false,
  "code": 400,
  "msg": "第1条数据的工单号不能为空；第2条数据的计划跟踪号不能为空",
  "data": null
}
```

## 验证与业务规则
1. `WorkOrderNo`、`PlanTrackNo`、`ProductCode`、`CallerName` 均不能为空。
2. 若数据库中已存在相同 `WorkOrderNo` 的记录，则执行更新；否则插入新记录。
3. 接口在单次请求内使用数据库事务，保证批量操作的原子性。
4. 返回结果统一使用 `WebResponseContent`，外部系统可通过 `status` 字段判断执行是否成功。

## 常见问题
- **返回 400 且提示字段不能为空**：请检查请求体中对应字段是否缺失或仅包含空白字符。
- **返回“批量导入失败: ...”**：可能是数据库异常或并发冲突，可根据返回的详细异常信息排查。
- **无法引用 DTO**：请确认项目引用了 `HDPro.CY.Order` 程序集，并使用命名空间 `HDPro.CY.Order.Models.MaterialCallBoardDtos`。
