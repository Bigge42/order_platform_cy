# ButtonGroupSelect 按钮组选择组件

一个通用的按钮组选择组件，支持单选和多选模式，提供灵活的选项配置。

## 功能特性

- ✅ 支持单选和多选模式
- ✅ 支持v-model双向绑定
- ✅ 完整的主题色适配
- ✅ 深色主题支持
- ✅ 响应式设计
- ✅ 禁用状态支持
- ✅ 完善的hover状态处理

## 基础用法

### 多选模式（默认）

```vue
<template>
  <div>
    <!-- 多选模式 -->
    <ButtonGroupSelect
      v-model="selectedTypes"
      :options="typeOptions"
      @change="handleChange"
    />
    
    <p>已选择：{{ selectedTypes }}</p>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import ButtonGroupSelect from '@/comp/button-group-select/index.vue'

const selectedTypes = ref([])

const typeOptions = ref([
  { label: '外购', value: 'external' },
  { label: '自制', value: 'internal' }
])

const handleChange = (values) => {
  console.log('选择变化:', values)
}
</script>
```

### 单选模式

```vue
<template>
  <div>
    <!-- 单选模式 -->
    <ButtonGroupSelect
      v-model="selectedType"
      :options="typeOptions"
      mode="single"
      @change="handleChange"
    />
    
    <p>已选择：{{ selectedType }}</p>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import ButtonGroupSelect from '@/comp/button-group-select/index.vue'

const selectedType = ref(null)

const typeOptions = ref([
  { label: '待处理', value: 'pending' },
  { label: '处理中', value: 'processing' },
  { label: '已完成', value: 'completed' }
])

const handleChange = (value) => {
  console.log('选择变化:', value)
}
</script>
```

## API

### Props

| 参数 | 说明 | 类型 | 可选值 | 默认值 |
|------|------|------|--------|--------|
| modelValue | 绑定值 | String / Number / Array | — | [] |
| options | 选项数据 | Array | — | [] |
| mode | 选择模式 | String | single / multiple | multiple |
| disabled | 是否禁用 | Boolean | — | false |

### Options数据格式

```javascript
const options = [
  {
    label: '显示文本',  // 必需，按钮显示的文本
    value: 'option1'   // 必需，选项的值
  },
  {
    label: '选项二',
    value: 'option2'
  }
]
```

### Events

| 事件名 | 说明 | 回调参数 |
|--------|------|----------|
| change | 选择项变化时触发 | (value) |
| update:modelValue | v-model更新时触发 | (value) |

## 使用示例

### 完整示例

```vue
<template>
  <div class="demo">
    <!-- 采购类型选择 -->
    <div class="selection-group">
      <div class="group-label">采购类型：</div>
      <ButtonGroupSelect
        v-model="selectedPurchaseTypes"
        :options="purchaseTypeOptions"
        mode="multiple"
        @change="handlePurchaseTypeChange"
      />
    </div>
    
    <!-- 产品类型选择 -->
    <div class="selection-group">
      <div class="group-label">产品类型：</div>
      <ButtonGroupSelect
        v-model="selectedProductTypes"
        :options="productTypeOptions"
        mode="multiple"
        @change="handleProductTypeChange"
      />
    </div>
    
    <!-- 状态选择（单选） -->
    <div class="selection-group">
      <div class="group-label">订单状态：</div>
      <ButtonGroupSelect
        v-model="selectedStatus"
        :options="statusOptions"
        mode="single"
        @change="handleStatusChange"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import ButtonGroupSelect from '@/comp/button-group-select/index.vue'

// 采购类型
const selectedPurchaseTypes = ref([])
const purchaseTypeOptions = ref([
  { label: '外购', value: 'external' },
  { label: '自制', value: 'internal' }
])

// 产品类型
const selectedProductTypes = ref([])
const productTypeOptions = ref([
  { label: '毛坯', value: 'blank' },
  { label: '阀体', value: 'valve_body' },
  { label: '阀体部件', value: 'valve_parts' },
  { label: '管道附件', value: 'pipe_fittings' },
  { label: '执行机构', value: 'actuator' },
  { label: '附件', value: 'accessories' }
])

// 订单状态（单选）
const selectedStatus = ref(null)
const statusOptions = ref([
  { label: '全部', value: 'all' },
  { label: '待处理', value: 'pending' },
  { label: '处理中', value: 'processing' },
  { label: '已完成', value: 'completed' }
])

const handlePurchaseTypeChange = (values) => {
  console.log('采购类型变化:', values)
}

const handleProductTypeChange = (values) => {
  console.log('产品类型变化:', values)
}

const handleStatusChange = (value) => {
  console.log('状态变化:', value)
}
</script>

<style scoped>
.demo {
  padding: 20px;
}

.selection-group {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
  gap: 15px;
}

.group-label {
  min-width: 80px;
  font-size: 14px;
  color: #333;
}
</style>
```

## 样式定制

组件支持完整的主题色适配，包括：
- 默认主题
- 深色主题
- 红色主题
- 橙色主题
- 渐变橙色主题
- 绿色主题
- 蓝色主题
- 白色主题

### 自定义样式

如果需要自定义样式，可以通过CSS变量或者deep选择器：

```vue
<style>
/* 自定义按钮高度 */
:deep(.group-btn) {
  height: 36px;
}

/* 自定义按钮间距 */
:deep(.button-group) {
  gap: 8px;
}
</style>
```

## 注意事项

1. **数据格式**：options数组中的每个对象必须包含label和value字段
2. **v-model值**：单选模式下为单个值，多选模式下为数组
3. **响应式**：组件在移动端会自动适配布局
4. **主题适配**：组件会自动适配项目的主题色配置

## 更新日志

### v1.0.0
- 初始版本发布
- 支持单选和多选模式
- 完整的主题色适配
- 响应式设计支持 