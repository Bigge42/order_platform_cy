# 预警功能开发总结

## 会话主要目的

实现一个通用的预警功能,能够根据预警规则表(OCP_AlertRules)的配置,在表格页面中自动为满足条件的数据行添加背景色预警。

**重要架构调整**: 将预警判断逻辑从前端移到后端,前端只负责样式应用,符合前后端分离的最佳实践。

## 完成的主要任务

### 1. 后端开发

#### 1.1 预警规则查询接口
- **文件**: `api\HDPro.WebApi\Controllers\Order\Partial\OCP_AlertRulesController.cs`
- **功能**: 添加`GetAlertRulesByPage`接口,根据页面名称查询启用状态的预警规则
- **接口路径**: `GET /api/OCP_AlertRules/GetAlertRulesByPage?pageName={pageName}`

### 2. 后端预警处理服务

#### 2.1 预警助手类
- **文件**: `api\HDPro.CY.Order\Services\Common\AlertWarningHelper.cs`
- **功能**: 通用的预警数据处理逻辑
- **核心方法**:
  - `ApplyAlertWarning<T>(List<T> rows, List<OCP_AlertRules> rules, ILogger logger)`: 为数据行添加预警标记
  - `CheckRowMatchesRule<T>(T row, OCP_AlertRules rule, PropertyInfo[] properties, ILogger logger)`: 检查数据行是否匹配预警规则
  - `IsCompleted<T>(T row, OCP_AlertRules rule, PropertyInfo[] properties)`: 检查是否已完成

#### 2.2 实体类扩展
- **文件**: `api\HDPro.Entity\DomainModels\OrderCollaboration\partial\OCP_TechManagement.cs`
- **修改**: 添加`ShouldAlert`属性(不映射到数据库)
```csharp
[NotMapped]
public bool ShouldAlert { get; set; }
```

#### 2.3 Service层集成
- **文件**: `api\HDPro.CY.Order\Services\OrderCollaboration\Partial\OCP_TechManagementService.cs`
- **修改**: 在`GetPageDataOnExecuted`中调用预警助手类
- **新增方法**: `ApplyAlertWarningToData(List<OCP_TechManagement> list)`

### 3. 前端开发

#### 3.1 预警样式工具类
- **文件**: `web.vite\src\utils\alertWarning.js`
- **功能**: 根据后端返回的`ShouldAlert`字段应用样式
- **核心函数**:
  - `applyAlertWarningStyle(columns, customStyle)`: 应用预警样式到表格列
  - `clearAlertWarningStyle(columns)`: 清除预警样式

#### 3.2 示例页面更新
- **文件**: `web.vite\src\views\order\ordercollaboration\OCP_TechManagement.vue`
- **修改**: 在`searchAfter`方法中应用预警样式
- **代码**:
```javascript
import { applyAlertWarningStyle } from '@/utils/alertWarning'

const searchAfter = async (rows, result) => {
  // 应用预警样式(后端已经在数据中添加了ShouldAlert字段)
  applyAlertWarningStyle(columns)
  return true
}
```

### 4. 文档编写

#### 4.1 使用说明文档
- **文件**: `docs\预警功能使用说明.md`
- **内容**:
  - 功能概述和核心特性
  - 技术架构说明
  - 详细使用方法
  - 预警规则配置说明
  - 配置示例和注意事项

#### 4.2 快速开始指南
- **文件**: `docs\预警功能快速开始.md`
- **内容**: 5分钟快速上手指南

## 关键决策和解决方案

### 1. 架构设计 ⭐重要调整

**最终决策**: 将预警判断逻辑移到后端,前端只负责样式应用

**理由**:
- **职责分离**: 业务逻辑应该在后端处理,前端只负责展示
- **性能优化**: 避免前端遍历大量数据进行判断
- **数据一致性**: 后端统一处理,确保预警逻辑的一致性
- **安全性**: 预警规则在后端处理,不暴露给前端
- **可维护性**: 预警逻辑集中在后端,便于维护和调试

**实现方式**:
1. 后端在`GetPageData`的`GetPageDataOnExecuted`中调用`AlertWarningHelper`
2. 为每条数据添加`ShouldAlert`字段(不映射到数据库)
3. 前端只需根据`ShouldAlert`字段应用样式

### 2. 预警判断逻辑

**决策**: 支持两种完成状态判断方式
- "根据完成判定值来判断": 字段值等于特定值时视为已完成
- "是否有值判断": 字段有值时视为已完成

**理由**:
- 覆盖不同业务场景
- 灵活性高,适应性强

### 3. 样式应用策略

**决策**: 在`searchAfter`中应用预警样式,而不是在`onInit`中

**理由**:
- 确保每次查询后都能应用最新的预警规则
- 避免初始化时的时序问题
- 符合Vue组件生命周期

### 4. 数据流程

**数据流程**:
1. 前端发起查询请求
2. 后端执行查询,获取数据
3. 后端在`GetPageDataOnExecuted`中:
   - 获取预警规则
   - 调用`AlertWarningHelper.ApplyAlertWarning`
   - 为满足条件的数据行设置`ShouldAlert = true`
4. 后端返回数据(包含`ShouldAlert`字段)
5. 前端在`searchAfter`中调用`applyAlertWarningStyle`
6. 前端根据`ShouldAlert`字段应用黄色背景样式

## 使用的技术栈

### 后端
- **语言**: C#
- **框架**: ASP.NET Core
- **ORM**: Entity Framework Core
- **架构模式**: Repository + Service模式

### 前端
- **框架**: Vue 3 (Composition API)
- **UI库**: Element Plus
- **HTTP客户端**: Axios
- **编程范式**: 组合式函数(Composables)

## 修改的文件

### 新增文件
1. `api\HDPro.CY.Order\Services\Common\AlertWarningHelper.cs` - 后端预警助手类
2. `web.vite\src\utils\alertWarning.js` - 前端预警样式工具类
3. `docs\预警功能使用说明.md` - 使用说明文档
4. `README_预警功能开发总结.md` - 本文件

### 修改文件
1. `api\HDPro.WebApi\Controllers\Order\Partial\OCP_AlertRulesController.cs` - 添加查询接口
2. `api\HDPro.Entity\DomainModels\OrderCollaboration\partial\OCP_AlertRules.cs` - 添加advanceWarningDays字段
3. `api\HDPro.Entity\DomainModels\OrderCollaboration\partial\OCP_TechManagement.cs` - 添加ShouldAlert属性
4. `api\HDPro.CY.Order\Services\OrderCollaboration\Partial\OCP_TechManagementService.cs` - 集成预警逻辑
5. `web.vite\src\views\order\ordercollaboration\OCP_TechManagement.vue` - 应用预警样式

## 代码设计原则

### 1. 单一职责原则(SRP)
- 预警规则查询、样式应用、缓存管理分别独立
- 每个函数只负责一个功能

### 2. KISS原则(Keep It Simple, Stupid)
- 提供简化版API: `applyAlertWarningToColumns`
- 使用方式简单,一行代码即可应用

### 3. 开闭原则(OCP)
- 通过配置扩展功能,无需修改代码
- 支持自定义样式,满足不同需求

### 4. 复用性
- 工具类和组合式函数可在多个页面复用
- 避免重复代码

## 扩展到其他页面

要在其他页面应用预警功能,需要三步:

### 步骤1: 为实体类添加ShouldAlert属性

在实体类的partial文件中添加:
```csharp
[NotMapped]
public bool ShouldAlert { get; set; }
```

### 步骤2: 在Service的GetPageData中集成预警逻辑

```csharp
using HDPro.CY.Order.Services.Common;

GetPageDataOnExecuted = (PageGridData<实体类> grid) =>
{
    if (grid.rows != null && grid.rows.Any())
    {
        // 应用预警标记
        ApplyAlertWarningToData(grid.rows);
    }
};

private void ApplyAlertWarningToData(List<实体类> list)
{
    try
    {
        var alertRulesService = AutofacContainerModule.GetService<IOCP_AlertRulesService>();
        var rules = alertRulesService.FindAsyncAsync(x =>
            x.AlertPage == "页面名称" &&
            x.TaskStatus == 1).GetAwaiter().GetResult();

        if (rules != null && rules.Any())
        {
            AlertWarningHelper.ApplyAlertWarning(list, rules.ToList(), _logger);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "应用预警标记时发生异常");
    }
}
```

### 步骤3: 在前端页面应用预警样式

```javascript
import { applyAlertWarningStyle } from '@/utils/alertWarning'

const searchAfter = async (rows, result) => {
  applyAlertWarningStyle(columns)
  return true
}
```

### 步骤4: 配置预警规则

在`OCP_AlertRules`表中添加预警规则配置。

## 测试建议

1. **功能测试**:
   - 配置预警规则
   - 访问OCP_TechManagement页面
   - 验证满足条件的行是否显示黄色背景

2. **性能测试**:
   - 测试大数据量下的性能表现
   - 验证缓存机制是否生效

3. **兼容性测试**:
   - 测试与现有cellStyle的兼容性
   - 验证多个预警规则同时生效的情况

## 后续优化建议

1. **增强功能**:
   - 支持更多预警条件(如数值范围、字符串匹配等)
   - 支持多级预警(不同严重程度使用不同颜色)
   - 支持预警提示信息(鼠标悬停显示预警原因)

2. **性能优化**:
   - 实现预警规则的增量更新
   - 优化大数据量场景下的渲染性能

3. **用户体验**:
   - 添加预警规则管理界面
   - 提供预警规则测试功能
   - 支持预警规则的导入导出

## 总结

本次开发成功实现了一个通用、灵活、易用的预警功能系统。通过合理的架构设计和代码封装,实现了"一次开发,多处复用"的目标。前端只需一行代码即可应用预警功能,后端通过配置即可管理预警规则,大大提高了开发效率和系统的可维护性。

---

## 2025-12-02 预警规则状态优化 - 启用/暂停双状态设计

### 会话主要目的
优化预警规则的任务状态管理,采用"启用/暂停"双状态设计,避免频繁创建/删除定时任务,提升性能和用户体验。

### 完成的主要任务

#### 1. 修改AlertRuleTaskStatus枚举
- **文件**: `api\HDPro.Entity\DomainModels\OrderCollaboration\Enums\AlertRuleTaskStatus.cs`
- **修改**:
  - `Paused = 0` (暂停 - 定时任务存在但暂停执行)
  - `Enabled = 1` (启用 - 定时任务运行中)
  - 移除 `Stopped` 状态
- **理由**: 只需要启用和暂停两种状态,通过Quartz的暂停/恢复功能管理任务

#### 2. 修改AlertRulesSchedulerService调度服务
- **文件**: `api\HDPro.CY.Order\Services\OrderCollaboration\AlertRulesSchedulerService.cs`
- **核心修改**:
  - 重构 `CreateOrUpdateAlertRuleJobAsync` 方法:
    - 如果任务不存在,创建新任务(根据状态决定是否立即暂停)
    - 如果任务已存在,调用 `PauseJob` 或 `ResumeJob` (不重新创建)
  - 修改 `GetActiveAlertRulesWithScheduleAsync`,包含启用和暂停两种状态
- **优化**: 避免频繁删除/创建任务,提升性能

#### 3. 修改AlertRulesController控制器
- **文件**: `api\HDPro.WebApi\Controllers\Order\AlertRulesController.cs`
- **修改**:
  - 移除 `switch` 语句,统一调用 `CreateOrUpdateAlertRuleJobAsync`
  - 该方法会自动根据状态处理暂停/恢复逻辑
- **简化**: 代码更简洁,逻辑更清晰

#### 4. 修改Vue页面按钮显示逻辑
- **文件**: `web.vite\src\views\order\ordercollaboration\OCP_AlertRules.vue`
- **修改内容**:
  - 权限配置改为 `pause` 和 `enable`
  - 按钮显示逻辑:
    - 暂停状态(0) → 显示"启用"按钮
    - 启用状态(1) → 显示"暂停"按钮
  - 保留"执行一次"和"日志"按钮
- **用户体验提升**: 界面简洁,操作直观

#### 5. 更新相关文档
- **文件**:
  - `docs\预警功能使用说明.md`
  - `docs\预警功能架构说明.md`
- **修改**: 将TaskStatus字段说明更新为"(0=暂停, 1=启用)"

### 关键决策和解决方案

#### 1. 为什么采用启用/暂停双状态设计?

**设计理由**:
1. **性能优化**: 暂停时不删除任务,恢复时不重新创建,避免频繁操作Quartz调度器
2. **快速切换**: 暂停→启用只需调用 `ResumeJob`,响应速度快
3. **保留任务状态**: 任务的执行历史、下次执行时间等信息得以保留
4. **符合业务场景**: 预警规则可能需要临时暂停,但不需要完全删除

#### 2. 状态切换逻辑

**初始启用**:
```
创建新的定时任务 → 启动任务
```

**启用 → 暂停**:
```
调用 scheduler.PauseJob(jobKey) → 任务暂停但不删除
```

**暂停 → 启用**:
```
调用 scheduler.ResumeJob(jobKey) → 任务恢复运行
```

#### 3. 按钮显示逻辑

**优化后**:
- 暂停状态 → 显示"启用"按钮(绿色/primary)
- 启用状态 → 显示"暂停"按钮(橙色/warning)
- 逻辑清晰,操作简单

### 使用的技术栈

**后端**:
- C# / ASP.NET Core
- Entity Framework Core
- Quartz.NET (定时任务调度)
  - `PauseJob()` - 暂停任务
  - `ResumeJob()` - 恢复任务

**前端**:
- Vue 3 (Composition API)
- Element Plus
- JSX语法(用于动态渲染按钮)

### 修改的文件

1. `api\HDPro.Entity\DomainModels\OrderCollaboration\Enums\AlertRuleTaskStatus.cs` - 修改枚举定义
2. `api\HDPro.CY.Order\Services\OrderCollaboration\AlertRulesSchedulerService.cs` - 重构调度逻辑
3. `api\HDPro.WebApi\Controllers\Order\AlertRulesController.cs` - 简化状态处理
4. `web.vite\src\views\order\ordercollaboration\OCP_AlertRules.vue` - 优化按钮显示逻辑
5. `docs\预警功能使用说明.md` - 更新文档
6. `docs\预警功能架构说明.md` - 更新文档

### 代码设计原则

#### 1. 性能优先
- 避免频繁创建/删除定时任务
- 利用Quartz的暂停/恢复功能

#### 2. KISS原则(Keep It Simple, Stupid)
- 只保留必要的两种状态
- 代码逻辑简洁明了

#### 3. 用户体验优先
- 根据当前状态智能显示操作按钮
- 减少用户的认知负担

### 测试建议

1. **功能测试**:
   - 测试初始启用,验证任务创建并运行
   - 测试启用→暂停,验证任务暂停但不删除
   - 测试暂停→启用,验证任务恢复运行(不重新创建)
   - 验证任务的下次执行时间是否保留

2. **UI测试**:
   - 验证暂停状态时只显示"启用"按钮
   - 验证启用状态时只显示"暂停"按钮
   - 验证按钮点击后状态正确更新

3. **性能测试**:
   - 对比暂停/恢复 vs 删除/创建的性能差异
   - 验证频繁切换状态时的系统响应速度

### 核心代码逻辑

#### CreateOrUpdateAlertRuleJobAsync 方法逻辑:

```csharp
if (任务已存在)
{
    if (状态 == 暂停)
        暂停任务 (PauseJob)
    else
        恢复任务 (ResumeJob)
}
else
{
    创建新任务
    if (状态 == 暂停)
        立即暂停任务
}
```

### 总结

本次优化成功实现了预警规则的启用/暂停双状态设计:
- ✅ 采用Quartz的暂停/恢复功能,避免频繁创建/删除任务
- ✅ 提升了性能和响应速度
- ✅ 优化了用户体验,界面更简洁
- ✅ 更新了相关文档
- ✅ 代码更简洁,更易维护

**核心改进**: 从"停止/启用/暂停"三状态简化为"暂停/启用"双状态,利用Quartz的原生功能实现快速切换,避免频繁操作调度器。

---

## 2024-12-20 ESB同步实体跟踪冲突修复

### 会话主要目的
修复采购未完跟踪ESB数据同步时出现的EF Core实体跟踪冲突错误。

### 问题描述
**错误信息**:
```
同步采购未完跟踪ESB数据失败，耗时：9.89 秒，错误：采购未完跟踪批量数据库操作失败：采购未完跟踪数据批量操作失败：The instance of entity type 'OCP_POUnFinishTrack' cannot be tracked because another instance with the same key value for {'TrackID'} is already being tracked.
```

**根本原因**:
1. `OCP_POUnFinishTrack` 实体的主键 `TrackID` 是自增长字段
2. 在批量更新时，如果同一批次中有多条ESB数据匹配到同一个现有记录（通过 `FENTRYID` 匹配）
3. 这导致同一个 `TrackID` 的实体被多次添加到 EF Core 的 ChangeTracker 中
4. EF Core 不允许跟踪具有相同主键的多个实体实例

### 完成的主要任务

#### 1. 修复 `PurchaseOrderUnFinishTrackESBSyncService.cs`

**文件**: `api\HDPro.CY.Order\Services\OrderCollaboration\ESB\Purchase\PurchaseOrderUnFinishTrackESBSyncService.cs`

**修改1**: 查询现有记录时使用 `AsNoTracking()`
```csharp
protected override async Task<List<OCP_POUnFinishTrack>> QueryExistingRecords(List<object> keys)
{
    var entryIds = keys.Cast<int>().Select(x => (long)x).Distinct().ToList();
    return await Task.Run(() =>
        _repository.FindAsIQueryable(x => x.FENTRYID.HasValue && entryIds.Contains(x.FENTRYID.Value))
        .AsNoTracking()  // 🔧 关键修复：使用 AsNoTracking 避免实体跟踪冲突
        .ToList());
}
```

**修改2**: 批量操作前清理 ChangeTracker 并去重
```csharp
protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_POUnFinishTrack> toUpdate, List<OCP_POUnFinishTrack> toInsert)
{
    return await Task.Run(() => _repository.DbContextBeginTransaction(() =>
    {
        try
        {
            // 🔧 关键修复：在批量操作前清理 ChangeTracker，避免实体跟踪冲突
            _repository.DbContext.ChangeTracker.Clear();

            // 🔧 去重处理：确保 toUpdate 和 toInsert 中没有重复的 TrackID
            var distinctToUpdate = toUpdate.GroupBy(x => x.TrackID).Select(g => g.First()).ToList();
            var distinctToInsert = toInsert.GroupBy(x => x.TrackID).Select(g => g.First()).ToList();

            if (distinctToUpdate.Count < toUpdate.Count)
            {
                ESBLogger.LogWarning($"检测到 {toUpdate.Count - distinctToUpdate.Count} 条重复的更新记录已被去重");
            }

            // ... 后续批量操作
        }
    }));
}
```

#### 2. 修复基类 `ESBSyncServiceBase.cs`

**文件**: `api\HDPro.CY.Order\Services\OrderCollaboration\ESB\ESBSyncServiceBase.cs`

**修改**: 防止同一个现有记录被多次匹配
```csharp
private async Task<int> ProcessSingleBatch(List<TESBData> batchData, int currentUserId, string currentUserName)
{
    // 批量查询现有记录
    var keys = batchData.Select(GetEntityKey).Distinct().ToList();
    var existingRecords = await QueryExistingRecords(keys);

    var toUpdate = new List<TEntity>();
    var toInsert = new List<TEntity>();

    // 🔧 关键修复：使用 HashSet 跟踪已处理的现有记录，避免重复添加到 toUpdate
    var processedExistingRecords = new HashSet<TEntity>();

    foreach (var esbData in batchData)
    {
        var existingRecord = existingRecords.FirstOrDefault(x => IsEntityMatch(x, esbData));

        if (existingRecord != null)
        {
            // 🔧 检查该记录是否已经被处理过
            if (!processedExistingRecords.Contains(existingRecord))
            {
                // 更新现有记录
                MapESBDataToEntityWithCache(esbData, existingRecord, ...);
                toUpdate.Add(existingRecord);
                processedExistingRecords.Add(existingRecord);
            }
            else
            {
                // 记录警告：同一个现有记录被多个ESB数据匹配
                _logger.LogWarning($"{GetOperationType()}：检测到重复匹配，ESB数据键={GetEntityKey(esbData)}，已跳过重复更新");
            }
        }
        else
        {
            // 创建新记录
            var newRecord = new TEntity();
            MapESBDataToEntityWithCache(esbData, newRecord, ...);
            toInsert.Add(newRecord);
        }
    }
}
```

### 关键决策和解决方案

#### 1. 使用 AsNoTracking 查询
**决策**: 在查询现有记录时使用 `AsNoTracking()`
**理由**:
- 避免 EF Core 自动跟踪查询出来的实体
- 后续通过 `UpdateRange` 显式跟踪需要更新的实体
- 防止同一实体被重复跟踪

#### 2. 清理 ChangeTracker
**决策**: 在批量操作前调用 `ChangeTracker.Clear()`
**理由**:
- 清除所有已跟踪的实体
- 确保批量操作时的干净状态
- 避免之前的查询操作留下的跟踪状态

#### 3. 去重处理
**决策**: 使用 `GroupBy` 对 `toUpdate` 和 `toInsert` 去重
**理由**:
- 防止同一个 `TrackID` 的实体被多次添加
- 记录警告日志，便于发现数据问题
- 保证数据一致性

#### 4. 防止重复匹配
**决策**: 使用 `HashSet` 跟踪已处理的现有记录
**理由**:
- 防止同一个现有记录被多个ESB数据匹配
- 记录警告日志，便于发现业务逻辑问题
- 提高代码健壮性

### 使用的技术栈
- **EF Core**: Entity Framework Core 实体跟踪机制
- **LINQ**: 数据查询和去重
- **C# 泛型**: 基类通用处理逻辑
- **日志记录**: ESBLogger 记录警告和错误

### 修改的文件
1. `api\HDPro.CY.Order\Services\OrderCollaboration\ESB\Purchase\PurchaseOrderUnFinishTrackESBSyncService.cs`
   - 添加 `AsNoTracking()` 到查询方法
   - 添加 `ChangeTracker.Clear()` 和去重逻辑到批量操作方法

2. `api\HDPro.CY.Order\Services\OrderCollaboration\ESB\ESBSyncServiceBase.cs`
   - 添加 `HashSet` 防止重复匹配
   - 添加警告日志记录

### 影响范围
- **直接影响**: 采购未完跟踪ESB同步服务
- **间接影响**: 所有继承自 `ESBSyncServiceBase` 的ESB同步服务都将受益于基类的修复
  - 委外未完跟踪同步
  - 部件未完跟踪同步
  - 金工未完跟踪同步
  - 订单跟踪同步
  - 等其他ESB同步服务

### 测试建议
1. 测试采购未完跟踪ESB同步功能，确保不再出现实体跟踪冲突错误
2. 检查日志，观察是否有重复匹配的警告信息
3. 验证同步后的数据准确性
4. 测试其他ESB同步服务，确保基类修改没有引入新问题

### 注意事项
⚠️ **不要自动git提交修改** - 请在充分测试后手动提交

---

## 2024-12-20 批量修复所有ESB同步服务的实体跟踪冲突

### 会话主要目的
参照 `OCP_POUnFinishTrack` 的修复方案，为所有其他ESB同步服务添加相同的实体跟踪冲突修复。

### 修复的服务列表

已完成修复的7个ESB同步服务：

1. **OCP_SubOrderUnFinishTrack** - 委外未完跟踪
   - 文件：`api\HDPro.CY.Order\Services\OrderCollaboration\ESB\SubOrder\SubOrderUnFinishTrackESBSyncService.cs`
   - 匹配字段：`FENTRYID`

2. **OCP_TechManagement** - 技术管理
   - 文件：`api\HDPro.CY.Order\Services\OrderCollaboration\ESB\TechManagement\TechManagementESBSyncService.cs`
   - 匹配字段：`SOEntryID`

3. **OCP_PartUnFinishTracking** - 部件未完跟踪
   - 文件：`api\HDPro.CY.Order\Services\OrderCollaboration\ESB\Part\PartUnFinishTrackESBSyncService.cs`
   - 匹配字段：`ESBID`

4. **OCP_PrdMOTracking** - 整机跟踪
   - 文件：`api\HDPro.CY.Order\Services\OrderCollaboration\ESB\WholeUnit\WholeUnitTrackingESBSyncService.cs`
   - 匹配字段：`ESBID`

5. **OCP_JGUnFinishTrack** - 金工未完跟踪
   - 文件：`api\HDPro.CY.Order\Services\OrderCollaboration\ESB\Metalwork\MetalworkUnFinishTrackESBSyncService.cs`
   - 匹配字段：`ESBID`

6. **OCP_OrderTracking** - 订单跟踪
   - 文件：`api\HDPro.CY.Order\Services\OrderCollaboration\ESB\OrderTracking\OrderTrackingESBSyncService.cs`
   - 匹配字段：`SOEntryID`

7. **OCP_LackMtrlResult** - 缺料运算结果
   - 文件：`api\HDPro.CY.Order\Services\OrderCollaboration\ESB\LackMaterial\LackMtrlResultESBSyncService.cs`
   - 匹配字段：`ESBID`

### 统一的修复方案

每个服务都应用了以下两处修复：

#### 1. QueryExistingRecords 方法修复
```csharp
protected override async Task<List<TEntity>> QueryExistingRecords(List<object> keys)
{
    var keyList = keys.Cast<TKeyType>().Distinct().ToList();  // 添加 Distinct()
    return await Task.Run(() =>
        _repository.FindAsIQueryable(x => keyList.Contains(x.MatchField))
        .AsNoTracking()  // 🔧 关键修复：使用 AsNoTracking 避免实体跟踪冲突
        .ToList());
}
```

#### 2. ExecuteBatchOperations 方法修复
```csharp
protected override async Task<WebResponseContent> ExecuteBatchOperations(List<TEntity> toUpdate, List<TEntity> toInsert)
{
    return await Task.Run(() => _repository.DbContextBeginTransaction(() =>
    {
        try
        {
            // 🔧 关键修复：在批量操作前清理 ChangeTracker，避免实体跟踪冲突
            _repository.DbContext.ChangeTracker.Clear();

            // 🔧 去重处理：确保 toUpdate 和 toInsert 中没有重复的匹配字段
            var distinctToUpdate = toUpdate.GroupBy(x => x.MatchField).Select(g => g.First()).ToList();
            var distinctToInsert = toInsert.GroupBy(x => x.MatchField).Select(g => g.First()).ToList();

            if (distinctToUpdate.Count < toUpdate.Count)
            {
                ESBLogger.LogWarning($"检测到 {toUpdate.Count - distinctToUpdate.Count} 条重复的更新记录已被去重");
            }
            if (distinctToInsert.Count < toInsert.Count)
            {
                ESBLogger.LogWarning($"检测到 {toInsert.Count - distinctToInsert.Count} 条重复的插入记录已被去重");
            }

            // 使用去重后的列表进行批量操作
            if (distinctToUpdate.Any())
            {
                _repository.UpdateRange(distinctToUpdate, false);
            }
            if (distinctToInsert.Any())
            {
                _repository.AddRange(distinctToInsert, false);
            }

            _repository.SaveChanges();

            return webResponse.OK($"批量操作成功，更新 {distinctToUpdate.Count} 条，新增 {distinctToInsert.Count} 条");
        }
        catch (Exception ex)
        {
            ESBLogger.LogError(ex, "批量操作失败");
            return webResponse.Error($"批量操作失败：{ex.Message}");
        }
    }));
}
```

### 关键技术点

1. **AsNoTracking()**: 查询时不跟踪实体，避免自动跟踪导致的冲突
2. **ChangeTracker.Clear()**: 批量操作前清理所有已跟踪的实体
3. **Distinct()**: 对查询键值去重，避免重复查询
4. **GroupBy 去重**: 对待更新/插入列表按匹配字段去重
5. **警告日志**: 记录去重信息，便于发现数据问题

### 修改的文件总结

| 序号 | 文件路径 | 实体 | 匹配字段 |
|------|---------|------|---------|
| 1 | `SubOrder\SubOrderUnFinishTrackESBSyncService.cs` | OCP_SubOrderUnFinishTrack | FENTRYID |
| 2 | `TechManagement\TechManagementESBSyncService.cs` | OCP_TechManagement | SOEntryID |
| 3 | `Part\PartUnFinishTrackESBSyncService.cs` | OCP_PartUnFinishTracking | ESBID |
| 4 | `WholeUnit\WholeUnitTrackingESBSyncService.cs` | OCP_PrdMOTracking | ESBID |
| 5 | `Metalwork\MetalworkUnFinishTrackESBSyncService.cs` | OCP_JGUnFinishTrack | ESBID |
| 6 | `OrderTracking\OrderTrackingESBSyncService.cs` | OCP_OrderTracking | SOEntryID |
| 7 | `LackMaterial\LackMtrlResultESBSyncService.cs` | OCP_LackMtrlResult | ESBID |
| 8 | `Purchase\PurchaseOrderUnFinishTrackESBSyncService.cs` | OCP_POUnFinishTrack | FENTRYID |

### 影响范围

✅ **全面覆盖**: 所有主要的ESB同步服务都已修复
✅ **统一方案**: 使用相同的修复模式，便于维护
✅ **向后兼容**: 修复不影响现有功能，只是增强了健壮性

### 测试建议

1. **逐个测试**: 依次测试每个ESB同步服务
2. **观察日志**: 检查是否有去重警告信息
3. **数据验证**: 确认同步后的数据准确性
4. **压力测试**: 测试大批量数据同步场景
5. **异常场景**: 测试包含重复数据的ESB数据源

### 预期效果

✅ **消除错误**: 不再出现实体跟踪冲突错误
✅ **数据准确**: 去重逻辑确保数据一致性
✅ **可观测性**: 警告日志帮助发现数据质量问题
✅ **性能优化**: AsNoTracking 减少不必要的跟踪开销

### 注意事项

⚠️ **不要自动git提交修改** - 请在充分测试后手动提交
⚠️ **关注警告日志** - 如果频繁出现去重警告，需要检查ESB数据源
⚠️ **数据验证** - 同步后务必验证数据的准确性和完整性

