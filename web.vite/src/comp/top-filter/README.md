# 年月选择器组件 (YearMonthSelector)

## 概述
一个支持年份下拉选择和月份按钮组选择的复合组件，支持 v-model 双向绑定，适配项目所有主题色。

## 功能特性
- 📅 年份下拉选择器
- 🔘 月份按钮组选择
- 🔄 v-model 双向绑定支持
- 🎨 自动适配项目所有主题色
- ⚙️ 高度可配置化
- 📱 响应式设计

## 使用方法

### 基础用法
```vue
<template>
  <YearMonthSelector v-model="dateSelection" />
</template>

<script setup>
import { ref } from 'vue'
import YearMonthSelector from '@/comp/year-month-selector/index.vue'

const dateSelection = ref({ year: '2025', month: 4 })
</script>
```

### 完整配置用法
```vue
<template>
  <YearMonthSelector
    v-model="dateSelection"
    :years="yearOptions"
    :months="monthOptions"
    year-label="选择年份"
    month-label="选择月份"
    year-placeholder="请选择年份"
    @change="handleDateChange"
  />
</template>

<script setup>
import { ref } from 'vue'
import YearMonthSelector from '@/comp/year-month-selector/index.vue'

const dateSelection = ref({ year: '2025', month: 4 })

const yearOptions = [
  { label: '2023', value: '2023' },
  { label: '2024', value: '2024' },
  { label: '2025', value: '2025' },
  { label: '2026', value: '2026' }
]

const monthOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]

const handleDateChange = (newDate) => {
  console.log('日期变化:', newDate)
  // 处理业务逻辑
}
</script>
```

## API 文档

### Props
| 属性名 | 类型 | 默认值 | 说明 |
|--------|------|---------|------|
| `modelValue` | Object | `{ year: '', month: null }` | v-model 绑定值 |
| `years` | Array | `[{label: '2024', value: '2024'}, ...]` | 年份选项数组 |
| `months` | Array | `[1,2,3...12]` | 月份选项数组 |
| `yearLabel` | String | `'年份'` | 年份标签文本 |
| `monthLabel` | String | `'月份'` | 月份标签文本 |
| `yearPlaceholder` | String | `'请选择年份'` | 年份选择器占位符 |

### Events
| 事件名 | 参数 | 说明 |
|--------|------|------|
| `update:modelValue` | `{ year: string, month: number }` | v-model 更新事件 |
| `change` | `{ year: string, month: number }` | 值变化事件 |

### 数据格式
```javascript
// modelValue 数据格式
{
  year: '2025',    // 字符串类型的年份
  month: 4         // 数字类型的月份
}

// years 数据格式
[
  { label: '2024', value: '2024' },
  { label: '2025', value: '2025' }
]

// months 数据格式
[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]
```

## 样式说明

### 主题适配
组件自动适配以下项目主题：
- `vol-theme-dark-aside` - 深色主题
- `vol-theme-red-aside` - 红色主题
- `vol-theme-orange-aside` - 橙色主题
- `vol-theme-gradient_orange-aside` - 橙色渐变主题
- `vol-theme-green-aside` - 绿色主题
- `vol-theme-blue-aside` - 蓝色主题
- `vol-theme-white-aside` - 白色主题

### 自定义样式
如需自定义样式，可以通过 CSS 变量覆盖：
```css
.year-month-selector {
  --primary-color: #your-color;
}
```

## 实际项目集成示例

在 `Home.vue` 中的使用示例：
```vue
<template>
  <div class="filter-section">
    <div class="filter-item">
      <div class="filter-label">销售单据号：</div>
      <el-select v-model="selectedOrder" placeholder="所有" class="filter-control">
        <el-option label="所有" value="所有"></el-option>
      </el-select>
    </div>
    <YearMonthSelector
      v-model="dateSelection"
      :years="yearOptions"
      :months="monthOptions"
      year-label="回复交付期(年份)"
      month-label="回复交付期(月份)"
      @change="handleDateChange"
    />
  </div>
</template>
```

## 注意事项

1. **数据类型**：确保年份为字符串类型，月份为数字类型
2. **主题适配**：组件会自动检测并适配当前项目主题
3. **响应式**：组件支持响应式布局，在小屏幕上会自动调整
4. **性能**：组件使用 Vue 3 Composition API，具有良好的性能表现

## 更新历史

- `v1.0.0` - 初始版本，支持基础年月选择功能
- 支持所有项目主题色自动适配
- 支持 v-model 双向绑定
- 支持自定义配置和事件回调 