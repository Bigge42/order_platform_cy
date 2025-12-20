# ESB同步实体跟踪冲突修复说明

## 问题概述

在采购未完跟踪ESB数据同步过程中，出现了EF Core实体跟踪冲突错误：

```
The instance of entity type 'OCP_POUnFinishTrack' cannot be tracked because another instance with the same key value for {'TrackID'} is already being tracked.
```

## 问题原因

### 根本原因
1. **主键冲突**: `OCP_POUnFinishTrack` 实体的主键 `TrackID` 是自增长字段
2. **重复匹配**: 同一批次中有多条ESB数据匹配到同一个现有记录（通过 `FENTRYID` 匹配）
3. **重复跟踪**: 同一个 `TrackID` 的实体被多次添加到 EF Core 的 ChangeTracker 中
4. **EF限制**: EF Core 不允许跟踪具有相同主键的多个实体实例

### 场景示例
假设有以下ESB数据：
- ESB数据1: FENTRYID = 100
- ESB数据2: FENTRYID = 100 (重复)

数据库中已存在：
- 记录A: TrackID = 1, FENTRYID = 100

处理流程：
1. ESB数据1 匹配到记录A，将记录A添加到 toUpdate 列表
2. ESB数据2 也匹配到记录A，再次将记录A添加到 toUpdate 列表
3. 执行 UpdateRange 时，EF Core 发现同一个 TrackID=1 的实体被添加了两次
4. 抛出实体跟踪冲突错误

## 解决方案

### 1. 查询时使用 AsNoTracking

**位置**: `PurchaseOrderUnFinishTrackESBSyncService.QueryExistingRecords`

**修改前**:
```csharp
return await Task.Run(() =>
    _repository.FindAsIQueryable(x => x.FENTRYID.HasValue && entryIds.Contains(x.FENTRYID.Value))
    .ToList());
```

**修改后**:
```csharp
return await Task.Run(() =>
    _repository.FindAsIQueryable(x => x.FENTRYID.HasValue && entryIds.Contains(x.FENTRYID.Value))
    .AsNoTracking()  // 🔧 避免自动跟踪
    .ToList());
```

**作用**: 查询时不自动跟踪实体，后续通过 UpdateRange 显式跟踪

### 2. 批量操作前清理 ChangeTracker

**位置**: `PurchaseOrderUnFinishTrackESBSyncService.ExecuteBatchOperations`

**新增代码**:
```csharp
// 清理 ChangeTracker，确保干净状态
_repository.DbContext.ChangeTracker.Clear();

// 去重处理
var distinctToUpdate = toUpdate.GroupBy(x => x.TrackID).Select(g => g.First()).ToList();
var distinctToInsert = toInsert.GroupBy(x => x.TrackID).Select(g => g.First()).ToList();

// 记录警告
if (distinctToUpdate.Count < toUpdate.Count)
{
    ESBLogger.LogWarning($"检测到 {toUpdate.Count - distinctToUpdate.Count} 条重复的更新记录已被去重");
}
```

**作用**: 
- 清除所有已跟踪的实体
- 对 toUpdate 和 toInsert 列表去重
- 记录警告日志

### 3. 防止重复匹配

**位置**: `ESBSyncServiceBase.ProcessSingleBatch`

**新增代码**:
```csharp
// 使用 HashSet 跟踪已处理的现有记录
var processedExistingRecords = new HashSet<TEntity>();

foreach (var esbData in batchData)
{
    var existingRecord = existingRecords.FirstOrDefault(x => IsEntityMatch(x, esbData));

    if (existingRecord != null)
    {
        // 检查是否已处理过
        if (!processedExistingRecords.Contains(existingRecord))
        {
            // 更新现有记录
            MapESBDataToEntityWithCache(esbData, existingRecord, ...);
            toUpdate.Add(existingRecord);
            processedExistingRecords.Add(existingRecord);
        }
        else
        {
            // 记录警告
            _logger.LogWarning($"{GetOperationType()}：检测到重复匹配，已跳过");
        }
    }
}
```

**作用**: 防止同一个现有记录被多次添加到 toUpdate 列表

## 测试验证

### 测试步骤

1. **准备测试数据**
   - 在数据库中创建一条采购未完跟踪记录
   - 准备包含重复 FENTRYID 的ESB测试数据

2. **执行同步**
   - 调用采购未完跟踪ESB同步接口
   - 观察日志输出

3. **验证结果**
   - 确认同步成功，无实体跟踪冲突错误
   - 检查日志中的警告信息
   - 验证数据库中的数据正确性

### 预期结果

✅ **成功场景**:
- 同步完成，无错误
- 日志中可能有去重警告（如果存在重复数据）
- 数据库中的数据正确更新

⚠️ **警告信息**:
```
检测到 X 条重复的更新记录已被去重
采购未完跟踪：检测到重复匹配，ESB数据键=XXX，已跳过重复更新
```

## 影响范围

### 直接影响
- ✅ 采购未完跟踪ESB同步服务

### 间接影响
所有继承自 `ESBSyncServiceBase` 的服务都将受益：
- ✅ 委外未完跟踪同步
- ✅ 部件未完跟踪同步
- ✅ 金工未完跟踪同步
- ✅ 订单跟踪同步
- ✅ 其他ESB同步服务

## 注意事项

1. ⚠️ **不要自动提交**: 请在充分测试后手动提交代码
2. 📝 **关注日志**: 如果频繁出现去重警告，需要检查ESB数据源是否有问题
3. 🔍 **数据验证**: 同步后验证数据的准确性和完整性
4. 🧪 **回归测试**: 测试其他ESB同步服务，确保基类修改没有引入新问题

