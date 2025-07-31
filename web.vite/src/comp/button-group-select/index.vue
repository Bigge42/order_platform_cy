<!-- 按钮组选择组件 -->
<template>
  <div class="button-group-select">
    <div class="button-group">
      <div 
        v-for="option in options" 
        :key="option.value"
        :class="['group-btn', isSelected(option.value) ? 'btn-selected' : '']"
        @click="handleOptionClick(option.value)"
      >
        {{ option.label }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

// Props定义
const props = defineProps({
  // v-model绑定的值
  modelValue: {
    type: [String, Number, Array],
    default: () => []
  },
  // 选项数据，格式：[{label: '外购', value: 'external'}]
  options: {
    type: Array,
    default: () => []
  },
  // 选择模式：single(单选) | multiple(多选)
  mode: {
    type: String,
    default: 'multiple',
    validator: (value) => ['single', 'multiple'].includes(value)
  },
  // 是否禁用
  disabled: {
    type: Boolean,
    default: false
  }
})

// Emits定义
const emit = defineEmits(['update:modelValue', 'change'])

// 当前选中的值
const selectedValues = computed({
  get() {
    if (props.mode === 'single') {
      return props.modelValue
    }
    return Array.isArray(props.modelValue) ? props.modelValue : []
  },
  set(value) {
    emit('update:modelValue', value)
    emit('change', value)
  }
})

// 判断选项是否被选中
const isSelected = (value) => {
  if (props.mode === 'single') {
    return selectedValues.value === value
  }
  return selectedValues.value.includes(value)
}

// 处理选项点击
const handleOptionClick = (value) => {
  if (props.disabled) return
  
  if (props.mode === 'single') {
    // 单选模式
    const newValue = selectedValues.value === value ? null : value
    selectedValues.value = newValue
  } else {
    // 多选模式
    const currentValues = [...selectedValues.value]
    const index = currentValues.indexOf(value)
    
    if (index > -1) {
      // 如果已选中，则取消选中
      currentValues.splice(index, 1)
    } else {
      // 如果未选中，则添加选中
      currentValues.push(value)
    }
    
    selectedValues.value = currentValues
  }
}
</script>

<style lang="less" scoped>
.button-group-select {
  display: inline-block;
}

.button-group {
  display: flex;
  gap: 5px;
  flex-wrap: wrap;
}

.group-btn {
  height: 40px;
  padding: 0 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: white;
  color: #333;
  cursor: pointer;
  border: 1px solid #ddd;
  border-radius: 3px;
  transition: all 0.3s ease;
  font-size: 12px;
  white-space: nowrap;
  user-select: none;
  
  &:hover {
    border-color: var(--el-color-primary);
    color: var(--el-color-primary);
  }
  
  &.disabled {
    cursor: not-allowed;
    opacity: 0.6;
    
    &:hover {
      border-color: #ddd;
      color: #333;
    }
  }
}

.btn-selected {
  background-color: var(--el-color-primary);
  color: white;
  border-color: var(--el-color-primary);
  
  &:hover {
    background-color: var(--el-color-primary);
    color: white;
    border-color: var(--el-color-primary);
  }
  
  &.disabled {
    background-color: #ccc;
    border-color: #ccc;
    
    &:hover {
      background-color: #ccc;
      border-color: #ccc;
      color: white;
    }
  }
}

// 深色主题适配
.vol-theme-dark-aside & {
  .group-btn {
    background-color: #2c2c2c;
    color: #fff;
    border-color: #444;
    
    &:hover {
      border-color: var(--el-color-primary);
      color: var(--el-color-primary);
    }
  }
  
  .btn-selected {
    background-color: var(--el-color-primary);
    border-color: var(--el-color-primary);
    
    &:hover {
      background-color: var(--el-color-primary);
      color: white;
      border-color: var(--el-color-primary);
    }
  }
}

// 各种主题色适配
.vol-theme-red-aside & {
  .btn-selected {
    &:hover {
      background-color: rgb(237, 64, 20);
      color: white;
      border-color: rgb(237, 64, 20);
    }
  }
}

.vol-theme-orange-aside & {
  .btn-selected {
    &:hover {
      background-color: rgb(255, 153, 0);
      color: white;
      border-color: rgb(255, 153, 0);
    }
  }
}

.vol-theme-gradient_orange-aside & {
  .btn-selected {
    &:hover {
      background-color: #fdb890;
      color: white;
      border-color: #fdb890;
    }
  }
}

.vol-theme-green-aside & {
  .btn-selected {
    &:hover {
      background-color: rgb(25, 190, 107);
      color: white;
      border-color: rgb(25, 190, 107);
    }
  }
}

.vol-theme-blue-aside & {
  .btn-selected {
    &:hover {
      background-color: rgb(45, 140, 240);
      color: white;
      border-color: rgb(45, 140, 240);
    }
  }
}

.vol-theme-white-aside & {
  .btn-selected {
    &:hover {
      background-color: rgb(45, 140, 240);
      color: white;
      border-color: rgb(45, 140, 240);
    }
  }
}

// 响应式设计
@media (max-width: 768px) {
  .button-group {
    width: 100%;
  }
  
  .group-btn {
    flex: 1;
    min-width: 60px;
  }
}
</style>
