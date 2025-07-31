# 批量催单组件 (BatchReminderDialog)

批量催单弹窗组件是一个可复用的 Vue 组件，用于处理多条记录的催单操作，支持数据收集、编辑和接口提交。

## 功能特性

- ✅ 批量数据展示和编辑
- ✅ 表格内嵌表单控件
- ✅ 人员选择器集成
- ✅ 批量编辑功能
- ✅ 数据验证
- ✅ 接口调用和错误处理

- ✅ 响应式设计
- ✅ 可选的接口调用模式

## 基本用法

```vue
<template>
  <BatchReminderDialog
    v-model="visible"
    :data="selectedRows"
    :business-type="'JS'"
    title="批量催单"
    @confirm="handleConfirm"
    @cancel="handleCancel"
  />
</template>

<script setup>
import BatchReminderDialog from '@/comp/reminder-dialog/batch.vue'

const visible = ref(false)
const selectedRows = ref([])

const handleConfirm = (result) => {
  console.log('用户编辑的数据:', result.data)
  console.log('API调用结果:', result.apiResult)
}

const handleCancel = () => {
  console.log('用户取消操作')
}
</script>
```

## Props

| 参数          | 类型          | 默认值     | 说明                   |
| ------------- | ------------- | ---------- | ---------------------- |
| modelValue    | Boolean       | false      | 弹窗显示状态           |
| data          | Array         | []         | 原始数据数组           |
| businessType  | String/Number | null       | 业务类型，用于字典查询 |
| title         | String        | '批量催单' | 弹窗标题               |
| width         | String/Number | '80%'      | 弹窗宽度               |
| enableApiCall | Boolean       | true       | 是否启用接口调用       |

## Events

| 事件名            | 参数                    | 说明             |
| ----------------- | ----------------------- | ---------------- |
| update:modelValue | (visible: boolean)      | 弹窗显示状态变化 |
| confirm           | (result: ConfirmResult) | 确定按钮点击     |
| cancel            | ()                      | 取消按钮点击     |

### ConfirmResult 数据结构

```typescript
interface ConfirmResult {
  data: Array<ProcessedData> // 用户编辑后的数据
  originalData: Array // 原始传入数据
  totalCount: number // 数据总数
  apiResult?: APIResult // API调用结果（仅在启用接口调用时）
}

interface APIResult {
  success: boolean // 请求是否成功
  message: string // 响应消息
  successCount: number // 成功数量
  failedCount: number // 失败数量
  failedItems: Array<FailedItem> // 失败项详情
}
```

## 数据字段说明

### 输入数据字段

组件接收原始行数据，会自动转换为可编辑的表格数据。原始数据应包含以下字段（用于业务键提取）：

- `id/Id/ID` - 主键
- `billNo/BillNo/orderNo/OrderNo` - 单据号
- `seq/Seq/sequence/Sequence` - 序号

### 表格编辑字段

| 字段                       | 类型   | 说明                           | 是否必填 |
| -------------------------- | ------ | ------------------------------ | -------- |
| UrgencyLevel               | String | 紧急等级（特急/A/B/C）         | 是       |
| AssignedReplyTime          | String | 指定回复时间（1-999）          | 否       |
| TimeUnit                   | String | 时间单位（1-分钟/2-小时/3-天） | 否       |
| AssignedResPersonName      | String | 指定负责人姓名                 | 否       |
| AssignedResPersonLoginName | String | 指定负责人登录名               | 否       |
| UrgentType                 | Number | 催单类型（0-催交期/1-催进度）  | 否       |
| UrgentContent              | String | 催单内容                       | 是       |

## 使用场景

### 1. 启用接口调用模式（默认）

```vue
<BatchReminderDialog
  v-model="visible"
  :data="selectedRows"
  :business-type="'JS'"
  :enable-api-call="true"
  @confirm="handleConfirm"
/>
```

在此模式下，组件会：

- 验证用户输入的数据
- 调用 `/api/OCP_UrgentOrder/BatchAdd` 接口
- 处理接口响应和错误
- 显示成功/失败提示

### 2. 纯数据收集模式

```vue
<BatchReminderDialog
  v-model="visible"
  :data="selectedRows"
  :business-type="'JS'"
  :enable-api-call="false"
  @confirm="handleConfirm"
/>
```

在此模式下，组件只收集和编辑数据，不调用接口，保持与原有逻辑的兼容性。

### 3. 自定义业务类型

```vue
<BatchReminderDialog
  v-model="visible"
  :data="selectedRows"
  :business-type="'CG'"
  title="批量催单（采购）"
  @confirm="handleConfirm"
/>
```

不同的业务类型会从字典接口获取对应的显示标签。

## 错误处理

组件内置了完善的错误处理机制：

1. **数据验证错误** - 显示具体的验证失败信息
2. **网络错误** - 显示网络连接失败提示
3. **服务器错误** - 显示服务器返回的错误信息
4. **业务错误** - 显示批量处理的失败详情

## 样式定制

组件支持通过 CSS 变量进行样式定制：

```css
.batch-reminder-dialog {
  --primary-color: #409eff;
  --success-color: #67c23a;
  --warning-color: #e6a23c;
  --danger-color: #f56c6c;
}
```

## 注意事项

1. **数据格式** - 确保传入的原始数据包含必要的业务键字段
2. **业务类型** - 不同业务类型需要配置对应的字典数据
3. **权限控制** - 组件不处理权限验证，需要在父组件中控制
4. **性能考虑** - 大数据量时建议分批处理

## 更新日志

### v2.0.0

- 新增接口调用功能
- 新增数据验证机制
- 新增可选的接口调用模式
- 优化用户体验和样式

### v1.0.0

- 基础的批量数据编辑功能
- 人员选择器集成
- 批量编辑功能
