<template>
  <div class="filter-section">
    <!-- 销售单据号筛选 -->
    <div v-if="showOrderSelect" class="filter-item">
      <div class="filter-label">{{ orderLabel }}：</div>
      <el-select 
        v-model="currentOrder" 
        :placeholder="orderPlaceholder"
        class="filter-control"
        @change="handleOrderChange"
      >
        <el-option 
          v-for="order in orderOptions" 
          :key="order.value"
          :label="order.label" 
          :value="order.value"
        />
      </el-select>
    </div>
    
    <!-- 年份筛选 -->
    <div class="filter-item">
      <div class="filter-label">{{ yearLabel }}：</div>
      <el-select 
        v-model="currentYear" 
        :placeholder="yearPlaceholder"
        class="filter-control"
        @change="handleYearChange"
      >
        <el-option 
          v-for="year in yearOptions" 
          :key="year.value"
          :label="year.label" 
          :value="year.value"
        />
      </el-select>
    </div>
    
    <!-- 月份筛选 -->
    <div class="filter-item">
      <div class="filter-label">{{ monthLabel }}：</div>
      <ButtonGroupSelect
        v-model="currentMonth"
        :options="monthButtonOptions"
        mode="single"
        @change="handleMonthChange"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import ButtonGroupSelect from '@/comp/button-group-select/index.vue'

// Props定义
const props = defineProps({
  // v-model绑定的值，格式：{order: string, year: string, month: number}
  modelValue: {
    type: Object,
    default: () => ({ order: '', year: '', month: null })
  },
  // 是否显示销售单据号选择器
  showOrderSelect: {
    type: Boolean,
    default: true
  },
  // 销售单据号选项，格式：[{label: '所有', value: '所有'}]
  orders: {
    type: Array,
    default: () => [
      { label: '所有', value: '所有' }
    ]
  },
  // 年份选项，格式：[{label: '2025', value: '2025'}]
  years: {
    type: Array,
    default: () => [
      { label: '2024', value: '2024' },
      { label: '2025', value: '2025' },
      { label: '2026', value: '2026' }
    ]
  },
  // 月份选项
  months: {
    type: Array,
    default: () => [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]
  },
  // 销售单据号标签
  orderLabel: {
    type: String,
    default: '销售单据号'
  },
  // 年份标签
  yearLabel: {
    type: String,
    default: '年份'
  },
  // 月份标签
  monthLabel: {
    type: String,
    default: '月份'
  },
  // 销售单据号选择器占位符
  orderPlaceholder: {
    type: String,
    default: '请选择单据号'
  },
  // 年份选择器占位符
  yearPlaceholder: {
    type: String,
    default: '请选择年份'
  }
})

// Emits定义
const emit = defineEmits(['update:modelValue', 'change'])

// 响应式数据
const currentOrder = ref(props.modelValue?.order || '')
const currentYear = ref(props.modelValue?.year || '')
const currentMonth = ref(props.modelValue?.month || null)

// 计算属性
const orderOptions = computed(() => props.orders)
const yearOptions = computed(() => props.years)
const monthOptions = computed(() => props.months)

// 将月份数组转换为按钮组选项格式
const monthButtonOptions = computed(() => 
  props.months.map(month => ({
    label: String(month),
    value: month
  }))
)

// 处理销售单据号变化
const handleOrderChange = (order) => {
  currentOrder.value = order
  emitChange()
}

// 处理年份变化
const handleYearChange = (year) => {
  currentYear.value = year
  emitChange()
}

// 处理月份变化
const handleMonthChange = (month) => {
  console.log('月份选择变化:', month)
  emitChange()
}

// 发出变化事件
const emitChange = () => {
  const newValue = {
    order: currentOrder.value,
    year: currentYear.value,
    month: currentMonth.value
  }
  emit('update:modelValue', newValue)
  emit('change', newValue)
}

// 监听外部值变化
watch(() => props.modelValue, (newValue) => {
  if (newValue) {
    currentOrder.value = newValue.order || ''
    currentYear.value = newValue.year || ''
    currentMonth.value = newValue.month || null
  }
}, { deep: true })

// 初始化时如果有默认值，发出变化事件
if (props.modelValue?.order || props.modelValue?.year || props.modelValue?.month) {
  emitChange()
}
</script>

<style lang="less" scoped>
.filter-section {
  display: flex;
  background-color: #fff;
  padding: 15px;
  border-radius: 5px;
  margin-bottom: 20px;
  box-shadow: 0 2px 5px rgba(45, 140, 240, 0.25);
  gap: 20px;
  align-items: center;
}

.filter-item {
  display: flex;
  align-items: center;
}

.filter-label {
  margin-right: 10px;
  white-space: nowrap;
  font-size: 14px;
}

.filter-control {
  min-width: 120px;
}



// 适配不同主题色
:deep(.el-select .el-input.is-focus .el-input__inner) {
  border-color: var(--el-color-primary);
}

// 深色主题适配
.vol-theme-dark-aside & {
  .filter-section {
    background-color: #021d37;
    box-shadow: 0 2px 5px rgba(0, 21, 41, 0.5);
  }
  

}

// 各种主题色适配
.vol-theme-red-aside & {
  .filter-section {
    box-shadow: 0 2px 5px rgba(237, 64, 20, 0.25);
  }
  

}

.vol-theme-orange-aside & {
  .filter-section {
    box-shadow: 0 2px 5px rgba(255, 153, 0, 0.25);
  }
  

}

.vol-theme-gradient_orange-aside & {
  .filter-section {
    box-shadow: 0 2px 5px rgba(253, 184, 144, 0.25);
  }
  

}

.vol-theme-green-aside & {
  .filter-section {
    box-shadow: 0 2px 5px rgba(25, 190, 107, 0.25);
  }
  

}

.vol-theme-blue-aside & {
  .filter-section {
    box-shadow: 0 2px 5px rgba(45, 140, 240, 0.25);
  }
}

.vol-theme-white-aside & {
  .filter-section {
    box-shadow: 0 2px 5px rgba(45, 140, 240, 0.25);
  }
}
</style>
