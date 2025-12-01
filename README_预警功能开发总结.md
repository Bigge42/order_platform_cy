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

