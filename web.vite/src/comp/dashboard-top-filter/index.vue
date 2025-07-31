<template>
  <div class="filter-section">
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
      <el-select 
        v-model="currentMonth" 
        :placeholder="monthPlaceholder"
        class="filter-control"
        @change="handleMonthChange"
      >
        <el-option 
          v-for="month in monthOptions" 
          :key="month.value"
          :label="month.label" 
          :value="month.value"
        />
      </el-select>
    </div>
    
    <!-- 周筛选 -->
    <div class="filter-item">
      <div class="filter-label">{{ weekLabel }}：</div>
      <ButtonGroupSelect
        v-model="currentWeeks"
        :options="weekOptions"
        mode="multiple"
        @change="handleWeekChange"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import ButtonGroupSelect from '@/comp/button-group-select/index.vue'

// Props定义
const props = defineProps({
  // v-model绑定的值，格式：{year: string, month: number, weeks: number[]}
  modelValue: {
    type: Object,
    default: () => ({ year: '', month: null, weeks: [] })
  },
  // 年份选项，格式：[{label: '2025', value: '2025'}]
  years: {
    type: Array,
    default: () => {
      const currentYear = new Date().getFullYear()
      return [
        { label: String(currentYear - 1), value: String(currentYear - 1) },
        { label: String(currentYear), value: String(currentYear) },
        { label: String(currentYear + 1), value: String(currentYear + 1) }
      ]
    }
  },
  // 月份选项，格式：[{label: '1月', value: 1}]
  months: {
    type: Array,
    default: () => [
      { label: '1月', value: 1 },
      { label: '2月', value: 2 },
      { label: '3月', value: 3 },
      { label: '4月', value: 4 },
      { label: '5月', value: 5 },
      { label: '6月', value: 6 },
      { label: '7月', value: 7 },
      { label: '8月', value: 8 },
      { label: '9月', value: 9 },
      { label: '10月', value: 10 },
      { label: '11月', value: 11 },
      { label: '12月', value: 12 }
    ]
  },
  // 周选项，格式：[{label: '第一周', value: 1}]
  weeks: {
    type: Array,
    default: () => [
      { label: '第一周', value: 1 },
      { label: '第二周', value: 2 },
      { label: '第三周', value: 3 },
      { label: '第四周', value: 4 }
    ]
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
  // 周标签
  weekLabel: {
    type: String,
    default: '周'
  },
  // 年份选择器占位符
  yearPlaceholder: {
    type: String,
    default: '请选择年份'
  },
  // 月份选择器占位符
  monthPlaceholder: {
    type: String,
    default: '请选择月份'
  }
})

// Emits定义
const emit = defineEmits(['update:modelValue', 'change'])

// 响应式数据
const currentYear = ref(props.modelValue?.year || '')
const currentMonth = ref(props.modelValue?.month || null)
const currentWeeks = ref(props.modelValue?.weeks || [])

// 计算属性
const yearOptions = computed(() => props.years)
const monthOptions = computed(() => props.months)
const weekOptions = computed(() => props.weeks)

// 处理年份变化
const handleYearChange = (year) => {
  currentYear.value = year
  emitChange()
}

// 处理月份变化
const handleMonthChange = (month) => {
  currentMonth.value = month
  emitChange()
}

// 处理周变化（支持多选）
const handleWeekChange = (weeks) => {
  console.log('周选择变化:', weeks)
  emitChange()
}

// 发出变化事件
const emitChange = () => {
  const newValue = {
    year: currentYear.value,
    month: currentMonth.value,
    weeks: [...currentWeeks.value]
  }
  emit('update:modelValue', newValue)
  emit('change', newValue)
}

// 监听外部值变化
watch(() => props.modelValue, (newValue) => {
  if (newValue) {
    currentYear.value = newValue.year || ''
    currentMonth.value = newValue.month || null
    currentWeeks.value = newValue.weeks || []
  }
}, { deep: true })

// 组件初始化时设置默认值
onMounted(() => {
  const now = new Date()
  const currentYearValue = String(now.getFullYear())
  const currentMonthValue = now.getMonth() + 1
  const currentWeekValue = getCurrentWeek()
  
  // 如果没有传入初始值，设置默认值
  if (!props.modelValue?.year && !props.modelValue?.month) {
    currentYear.value = currentYearValue
    currentMonth.value = currentMonthValue
    currentWeeks.value = [currentWeekValue]
    emitChange()
  }
})

// 计算当前是第几周
const getCurrentWeek = () => {
  const now = new Date()
  const year = now.getFullYear()
  const month = now.getMonth()
  const today = now.getDate()
  
  // 获取当月第一天是星期几 (0=周日, 1=周一, ..., 6=周六)
  const firstDay = new Date(year, month, 1).getDay()
  
  // 调整为周一为一周的开始 (0=周一, 1=周二, ..., 6=周日)
  const firstDayAdjusted = firstDay === 0 ? 6 : firstDay - 1
  
  // 计算当前日期是第几周
  const weekNumber = Math.ceil((today + firstDayAdjusted) / 7)
  
  // 确保周数在1-4范围内
  return Math.min(weekNumber, 4)
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
  flex-wrap: wrap;
}

.filter-item {
  display: flex;
  align-items: center;
}

.filter-label {
  margin-right: 10px;
  white-space: nowrap;
  font-size: 14px;
  color: #333;
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
  
  .filter-label {
    color: #fff;
  }
  

}

// 响应式设计
@media (max-width: 768px) {
  .filter-section {
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
  }
  
  .filter-item {
    width: 100%;
    justify-content: space-between;
  }
  
  .filter-control {
    min-width: 150px;
  }
}
</style>
