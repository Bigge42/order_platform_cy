# 年月周选择器组件 (DashboardTopFilter)

## 概述
一个支持年份、月份下拉选择和周按钮组多选的复合组件，支持 v-model 双向绑定，适配项目所有主题色。

## 功能特性
- 📅 年份下拉选择器（默认当年）
- 🗓️ 月份下拉选择器（默认当月）
- 📊 周按钮组选择（支持多选：第一周、第二周、第三周、第四周）
- 🔄 v-model 双向绑定支持
- 🎨 自动适配项目所有主题色
- ⚙️ 高度可配置化
- 📱 响应式设计

## 使用方法

### 基础用法
```vue
<template>
  <DashboardTopFilter v-model="filterSelection" />
</template>

<script setup>
import { ref } from 'vue'
import DashboardTopFilter from '@/comp/dashboard-top-filter/index.vue'

const filterSelection = ref({ year: '', month: null, weeks: [] })
</script>
```

### 完整配置用法
```vue
<template>
  <DashboardTopFilter
    v-model="filterSelection"
    :years="yearOptions"
    :months="monthOptions"
    :weeks="weekOptions"
    year-label="选择年份"
    month-label="选择月份"
    week-label="选择周"
    year-placeholder="请选择年份"
    month-placeholder="请选择月份"
    @change="handleFilterChange"
  />
</template>

<script setup>
import { ref } from 'vue'
import DashboardTopFilter from '@/comp/dashboard-top-filter/index.vue'

const filterSelection = ref({ year: '2025', month: 1, weeks: [1, 2] })

const yearOptions = [
  { label: '2023', value: '2023' },
  { label: '2024', value: '2024' },
  { label: '2025', value: '2025' },
  { label: '2026', value: '2026' }
]

const monthOptions = [
  { label: '1月', value: 1 },
  { label: '2月', value: 2 },
  // ... 其他月份
]

const weekOptions = [
  { label: '第一周', value: 1 },
  { label: '第二周', value: 2 },
  { label: '第三周', value: 3 },
  { label: '第四周', value: 4 }
]

const handleFilterChange = (newFilter) => {
  console.log('筛选条件变化:', newFilter)
  // 处理业务逻辑
}
</script>
```

## API 文档

### Props
| 属性名 | 类型 | 默认值 | 说明 |
|--------|------|---------|------|
| `modelValue` | Object | `{ year: '', month: null, weeks: [] }` | v-model 绑定值 |
| `years` | Array | 当前年份前后各一年 | 年份选项数组 |
| `months` | Array | 1-12月 | 月份选项数组 |
| `weeks` | Array | 第一周到第四周 | 周选项数组 |
| `yearLabel` | String | `'年份'` | 年份标签文本 |
| `monthLabel` | String | `'月份'` | 月份标签文本 |
| `weekLabel` | String | `'周'` | 周标签文本 |
| `yearPlaceholder` | String | `'请选择年份'` | 年份选择器占位符 |
| `monthPlaceholder` | String | `'请选择月份'` | 月份选择器占位符 |

### Events
| 事件名 | 参数 | 说明 |
|--------|------|------|
| `update:modelValue` | `{ year: string, month: number, weeks: number[] }` | v-model 更新事件 |
| `change` | `{ year: string, month: number, weeks: number[] }` | 值变化事件 |

### 数据格式
```javascript
// modelValue 数据格式
{
  year: '2025',         // 字符串类型的年份
  month: 1,             // 数字类型的月份
  weeks: [1, 2, 3]      // 数字数组类型的周（支持多选）
}

// years 数据格式
[
  { label: '2024', value: '2024' },
  { label: '2025', value: '2025' }
]

// months 数据格式
[
  { label: '1月', value: 1 },
  { label: '2月', value: 2 }
]

// weeks 数据格式
[
  { label: '第一周', value: 1 },
  { label: '第二周', value: 2 },
  { label: '第三周', value: 3 },
  { label: '第四周', value: 4 }
]
```

## 默认行为
- **年份**：自动设置为当前年份
- **月份**：自动设置为当前月份
- **周**：初始为空数组，支持多选
- **年份选项**：默认提供当前年份前后各一年的选项
- **月份选项**：默认提供1-12月的选项

## 样式说明

### 主题适配
组件自动适配以下项目主题：
- `vol-theme-dark-aside` - 深色主题
- 其他项目定义的主题色

### 周按钮样式
- 未选中：白色背景，灰色边框
- 选中：主题色背景，白色文字
- 悬停：主题色边框和文字
- 支持多选，可以同时选择多个周

### 响应式设计
- 在小屏幕（768px以下）会自动调整为垂直布局
- 各筛选项会自动换行适配

## 实际项目集成示例

```vue
<template>
  <div class="dashboard-page">
    <DashboardTopFilter
      v-model="dashboardFilter"
      year-label="统计年份"
      month-label="统计月份"
      week-label="统计周"
      @change="handleFilterChange"
    />
    
    <!-- 其他内容 -->
    <div class="dashboard-content">
      <!-- 根据筛选条件显示内容 -->
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import DashboardTopFilter from '@/comp/dashboard-top-filter/index.vue'

const dashboardFilter = ref({ year: '', month: null, weeks: [] })

const handleFilterChange = (filter) => {
  console.log('筛选条件:', filter)
  // 调用API获取数据
  fetchDashboardData(filter)
}

const fetchDashboardData = (filter) => {
  // 根据筛选条件获取数据
  console.log(`获取${filter.year}年${filter.month}月第${filter.weeks.join(',')}周的数据`)
}

// 监听筛选条件变化
watch(dashboardFilter, (newFilter) => {
  if (newFilter.year && newFilter.month) {
    fetchDashboardData(newFilter)
  }
}, { deep: true })
</script>
```

## 注意事项

1. **数据类型**：确保年份为字符串类型，月份为数字类型，周为数字数组类型
2. **默认值**：组件会自动设置当前年月为默认值
3. **多选周**：周选择支持多选，点击已选中的周会取消选中
4. **主题适配**：组件会自动检测并适配当前项目主题
5. **响应式**：组件支持响应式布局，在小屏幕上会自动调整
6. **性能**：使用 Vue 3 Composition API，具有良好的性能表现

## 更新历史

- `v1.0.0` - 初始版本
  - 支持年份、月份下拉选择
  - 支持周按钮组多选
  - 支持 v-model 双向绑定
  - 支持项目主题色自动适配
  - 支持响应式设计 