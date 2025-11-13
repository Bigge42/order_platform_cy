# 项目开发日志

## 2025-11-13 - 缺料运算结果导出功能优化

### 会话主要目的
为缺料运算结果(OCP_LackMtrlResult)的导出功能添加运算方案条件过滤,确保导出的数据与查询显示的数据一致。

### 完成的主要任务
1. 在 `exportBefore` 钩子函数中添加运算方案ID(ComputeID)作为导出条件
2. 在 `exportBefore` 钩子函数中添加单据类型(BillType)作为导出条件
3. 实现 Vue 组件与 JSX 扩展之间的数据共享机制(通过 sessionStorage)
4. 添加导出前的数据验证,确保用户已选择运算方案

### 关键决策和解决方案

#### 问题
导出功能需要考虑运算方案条件,但 JSX 扩展文件无法直接访问 Vue 组件中的响应式数据(`selectedSchemeId`)。

#### 解决方案
采用 sessionStorage 作为中间存储,实现 Vue 组件与 JSX 扩展之间的数据共享:

1. **Vue 组件端**:在选择运算方案时,将 `selectedSchemeId` 保存到 sessionStorage
   - 在 `selectScheme` 函数中保存
   - 在 `loadSchemes` 函数自动选中方案时保存

2. **JSX 扩展端**:在 `exportBefore` 钩子中从 sessionStorage 读取运算方案ID
   - 读取 `OCP_LackMtrlResult_selectedSchemeId`
   - 添加到 `param.wheres` 查询条件中
   - 如果未选中方案,提示用户并阻止导出

#### 实现逻辑
参照查询功能(`searchBefore`)的实现方式:
```javascript
// 查询时的条件
if (selectedSchemeId.value) {
    param.wheres.push({ name: 'ComputeID', value: selectedSchemeId.value })
    return true;
}

// 导出时的条件(从 sessionStorage 获取)
const selectedSchemeId = sessionStorage.getItem('OCP_LackMtrlResult_selectedSchemeId');
if (selectedSchemeId) {
    param.wheres.push({ name: 'ComputeID', value: selectedSchemeId });
} else {
    this.$message.warning('请先选择一个运算方案');
    return false;
}
```

### 技术栈
- **前端框架**: Vue 3 (Composition API)
- **扩展机制**: JSX 扩展文件
- **数据存储**: sessionStorage
- **UI 组件**: Element Plus

### 修改的文件

#### 1. `web.vite/src/extension/order/ordercollaboration/OCP_LackMtrlResult.jsx`
**修改内容**:
- 在 `exportBefore` 方法中添加 BillType 条件映射
- 在 `exportBefore` 方法中从 sessionStorage 读取运算方案ID
- 添加 ComputeID 条件到导出参数的 wheres 数组
- 添加未选中方案时的提示和阻止逻辑

**关键代码**:
```javascript
exportBefore(param) {
  // 添加BillType条件
  const BillTypeMap = {
    '#/OCP_LackMtrlResult_PO': '标准采购',
    '#/OCP_LackMtrlResult_WO': '标准委外',
    '#/OCP_LackMtrlResult_MO_JG': '金工车间',
    '#/OCP_LackMtrlResult_MO_BJ': '部件车间',
  };

  if (BillTypeMap[sourceKey]) {
    param.wheres.push({ name: 'BillType', value: BillTypeMap[sourceKey] });
  }

  // 从 sessionStorage 获取运算方案ID
  const selectedSchemeId = sessionStorage.getItem('OCP_LackMtrlResult_selectedSchemeId');
  if (selectedSchemeId) {
    param.wheres.push({ name: 'ComputeID', value: selectedSchemeId });
  } else {
    this.$message.warning('请先选择一个运算方案');
    return false;
  }

  return true;
}
```

#### 2. `web.vite/src/views/order/ordercollaboration/OCP_LackMtrlResult.vue`
**修改内容**:
- 在 `selectScheme` 函数中添加 sessionStorage 保存逻辑
- 在 `loadSchemes` 函数中自动选中方案时添加 sessionStorage 保存逻辑

**关键代码**:
```javascript
// 选择运算方案时保存
const selectScheme = (scheme) => {
    selectedSchemeId.value = scheme.ComputeID
    sessionStorage.setItem('OCP_LackMtrlResult_selectedSchemeId', scheme.ComputeID)
    // ...
}

// 自动选中默认方案或第一个方案时保存
if (defaultScheme) {
    selectedSchemeId.value = defaultScheme.ComputeID
    sessionStorage.setItem('OCP_LackMtrlResult_selectedSchemeId', defaultScheme.ComputeID)
} else if (newData.length > 0) {
    selectedSchemeId.value = newData[0].ComputeID
    sessionStorage.setItem('OCP_LackMtrlResult_selectedSchemeId', newData[0].ComputeID)
}
```

### 功能特性
1. **数据一致性**: 导出的数据与查询显示的数据完全一致
2. **用户体验**: 未选中运算方案时给予明确提示
3. **防重复**: 检查是否已存在 ComputeID 条件,避免重复添加
4. **自动保存**: 选择方案或自动选中方案时自动保存到 sessionStorage
5. **完整过滤**: 同时考虑 BillType、BusinessType 和 ComputeID 三个维度的过滤条件

### 注意事项
- sessionStorage 的数据在浏览器标签页关闭后会清除
- 如果需要跨标签页共享,可以考虑使用 localStorage
- 导出功能会自动应用供应商字段权限控制(通过 BusinessType 参数)

