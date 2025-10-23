# MaterialCallBoard 批量导入接口使用说明

## 接口概览
- **请求方法**：`POST`
- **请求地址**：`/api/MaterialCallBoard/batch-upload`
- **数据格式**：`application/json`
- **鉴权说明**：沿用现有 `MaterialCallBoard` 控制器的权限策略，如需开放给外部系统，请在网关或调用方补充认证与签名机制。

该接口用于一次性批量新增或更新 `MaterialCallBoard` 数据。系统会根据工单号（`WorkOrderNo`）判断记录是否存在：

- 数据库中存在相同工单号时执行更新。
- 不存在时创建新记录。

## 请求体结构
请求体为 `MaterialCallBoardBatchDto` 对象数组（定义在 `HDPro.CY.Order.Services.MaterialCallBoard.Models` 命名空间中），字段如下：

| 字段名 | 类型 | 是否必填 | 说明 |
| --- | --- | --- | --- |
| `WorkOrderNo` | string | 是 | 工单号，作为幂等键，不能为空且单次请求内需保持唯一。 |
| `PlanTrackNo` | string | 是 | 计划跟踪号。 |
| `ProductCode` | string | 是 | 产品编号。 |
| `CallerName` | string | 是 | 叫料人。 |
| `CalledAt` | string (ISO 8601) | 是 | 叫料时间，必须能被解析为日期时间。 |

> **注意**：服务端会自动去除字段前后空格。`CalledAt` 建议以 `yyyy-MM-ddTHH:mm:ss`（UTC 或约定时区）格式提交。

### 示例
```json
[
  {
    "workOrderNo": "WO-20240515001",
    "planTrackNo": "PT-8899",
    "productCode": "PRD-10001",
    "callerName": "张三",
    "calledAt": "2024-05-15T10:30:00"
  },
  {
    "workOrderNo": "WO-20240515002",
    "planTrackNo": "PT-9900",
    "productCode": "PRD-10002",
    "callerName": "李四",
    "calledAt": "2024-05-15T10:45:00"
  }
]
```

## 响应
接口统一返回 `WebResponseContent` 对象的 JSON 表示。

### 成功响应
```json
{
  "status": true,
  "code": 200,
  "msg": "批量写入成功，新增 1 条，更新 1 条",
  "data": null
}
```

- `msg` 中会返回本次批量写入的新增/更新统计。

### 失败响应
服务端会在以下场景返回失败：

- 请求体为空或所有元素为 `null`。
- 任意记录缺少必填字段。
- 单次请求内存在重复工单号。
- 数据库操作异常。

示例：
```json
{
  "status": false,
  "code": 400,
  "msg": "数据校验失败",
  "data": [
    "第1条数据的工单号不能为空",
    "存在重复的工单号: WO-20240515001"
  ]
}
```

## 幂等性与数据清洗
- 接口会自动去除字符串字段前后空格。
- 若同一工单号在一次请求中出现多次，以最后一条为准。
- 建议调用方按业务需求自行处理去重与排序，保证提交数据的唯一性与正确性。

## 建议调用步骤
1. 调用前先在调用方系统校验字段完整性与时间格式。
2. 将待同步的记录转换为上述 JSON 数组并发送 HTTP POST 请求。
3. 根据返回的 `status` 和 `msg` 判定是否成功，如失败请参考 `data` 中的详细错误。
4. 若需要追踪调用，可记录接口返回值及请求负载，便于排查问题。

## 调试方法
- 在开发或测试环境中，可使用 Postman、curl 等工具直接构造请求验证接口行为。
- 若需要查看服务端日志，请结合 WebAPI 层的日志配置或 APM 工具定位。

