# ESB接口日志功能测试说明

## 功能概述

已在ESBBaseService中实现了自动记录ESB接口调用日志到OCP_ApiLog表的功能,包括:
- 接口名称、路径
- 请求参数、响应数据长度
- HTTP状态码、调用状态
- 调用开始/结束时间、耗时(毫秒)
- 返回数据条数
- 错误信息(如果有)

## 修改的文件

1. **api\HDPro.CY.Order\Services\OrderCollaboration\ESB\ESBBaseService.cs**
   - 添加了`LogApiCallAsync`方法用于记录API调用日志
   - 修改了`CallESBApi`方法,集成日志记录功能
   - 修改了`CallESBApiWithRequestData`方法,集成日志记录功能

## 测试方法

### 方法1: 通过现有ESB同步接口测试

1. 启动应用程序
2. 调用任意ESB同步接口,例如:
   - 整机跟踪同步: `POST /api/ESBSync/SyncWholeUnitTracking`
   - 采购订单同步: `POST /api/ESBSync/SyncPurchaseOrder`
   - 销售订单明细同步: `POST /api/ESBSync/SyncSalesOrderDetail`

3. 查询OCP_ApiLog表,验证日志是否正确记录:
```sql
SELECT TOP 10 
    ApiName,
    ApiPath,
    HttpMethod,
    StatusCode,
    Status,
    ElapsedMs,
    DataCount,
    ResponseLength,
    StartTime,
    EndTime,
    ErrorMessage,
    ResponseResult,
    Remark
FROM OCP_ApiLog
ORDER BY CreateDate DESC
```

### 方法2: 通过前端界面查看

1. 登录系统
2. 导航到"订单协同" -> "接口日志管理"
3. 查看ESB接口调用记录列表
4. 可以按时间、接口名称、状态等条件筛选查询

## 验证要点

### 成功调用的日志应包含:
- ✅ Status = 1 (成功)
- ✅ StatusCode = 200
- ✅ DataCount > 0 (如果有数据返回)
- ✅ ResponseLength > 0
- ✅ ElapsedMs > 0
- ✅ ErrorMessage 为空
- ✅ ResponseResult 包含"成功返回X条数据"

### 失败调用的日志应包含:
- ✅ Status = 0 (失败)
- ✅ StatusCode != 200 或为空
- ✅ ErrorMessage 包含错误详情
- ✅ DataCount = 0

### 超时调用的日志应包含:
- ✅ Status = 0 (失败)
- ✅ ErrorMessage 包含"超时"关键字
- ✅ Remark 包含超时设置信息

## 性能说明

- 日志记录采用异步方式,不会阻塞主业务流程
- 日志记录失败不会影响ESB接口调用的正常执行
- 日志记录异常会记录到文件日志中

## 注意事项

1. 确保OCP_ApiLog表已创建并有正确的权限
2. 确保ServiceDbContext已包含OCP_ApiLog实体
3. 日志记录会自动截断过长的字段以符合数据库字段长度限制:
   - ApiName: 最大200字符
   - ApiPath: 最大200字符
   - HttpMethod: 最大20字符
   - Remark: 最大500字符
   - RequestParams, ErrorMessage, ResponseResult: nvarchar(max)

## 前端界面优化

已优化前端列显示顺序和格式化:
- **状态列**: 成功显示绿色,失败显示红色
- **耗时列**: 超过10秒显示红色,超过5秒显示橙色
- **响应大小列**: 自动转换为KB/MB显示
- **列顺序**: 按重要性排序,最常用的信息放在前面
- **隐藏列**: 默认隐藏不常用的列(如请求参数、结束时间等),可通过列设置显示

## 后续优化建议

1. 可以添加定时任务清理过期日志(如保留最近3个月)
2. 可以添加日志统计分析功能,如:
   - 接口调用频率统计
   - 接口平均响应时间
   - 接口成功率统计
   - 慢接口告警(超过阈值)
3. 可以添加日志导出功能,方便问题分析
4. 可以添加实时监控面板,显示当前接口调用情况

