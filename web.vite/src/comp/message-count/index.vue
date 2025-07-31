<template>
  <div class="message-count-container">
    <div
      v-for="(item, index) in countList"
      :key="index"
      class="count-item"
      :class="{ 
        'clickable': item.clickable,
        'selected': selectedType && item.type === selectedType
      }"
      @click="handleItemClick(item, index)"
    >
      <div class="count-number">{{ item.count || 0 }}</div>
      <div class="count-label">{{ item.label }}</div>
      <div v-if="item.description" class="count-description">{{ item.description }}</div>
    </div>
  </div>
</template>

<script setup>
// 定义props
const props = defineProps({
  // 统计数据列表
  countList: {
    type: Array,
    default: () => [],
    validator: (value) => {
      return value.every(item => 
        typeof item === 'object' && 
        'label' in item && 
        'count' in item
      )
    }
  },
  // 是否显示点击效果
  showClickEffect: {
    type: Boolean,
    default: true
  },
  // 当前选中的类型
  selectedType: {
    type: String,
    default: null
  }
})

// 定义事件
const emit = defineEmits(['item-click', 'refresh'])

// 处理项目点击
const handleItemClick = (item, index) => {
  if (!item.clickable && !props.showClickEffect) return
  
  // 触发单个项目点击事件
  emit('item-click', {
    item,
    index,
    type: item.type || 'default'
  })
  
  // 如果需要刷新数据，触发刷新事件
  if (item.needRefresh) {
    emit('refresh', item.type || 'default')
  }
}
</script>

<style scoped>
.message-count-container {
  display: flex;
  gap: 20px;
  padding: 16px;
  background-color: var(--el-bg-color-page, #f5f7fa);
  border-radius: 8px;
  flex-wrap: wrap;
}

.count-item {
  flex: 1;
  min-width: 120px;
  padding: 20px 16px;
  background-color: var(--el-bg-color, #ffffff);
  border-radius: 8px;
  text-align: center;
  border: 1px solid var(--el-border-color-lighter, #ebeef5);
  transition: all 0.3s ease;
  position: relative;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.count-item.clickable {
  cursor: pointer;
}

.count-item.clickable:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  border-color: var(--el-color-primary, #409eff);
}

.count-item.clickable:active {
  transform: translateY(0);
}

/* 选中状态样式 */
.count-item.selected {
  border-color: var(--el-color-primary, #409eff);
  background-color: var(--el-color-primary-light-9, #ecf5ff);
  box-shadow: 0 2px 8px rgba(64, 158, 255, 0.3);
}

.count-item.selected .count-number {
  color: var(--el-color-primary, #409eff);
  font-weight: bold;
}

.count-item.selected .count-label {
  color: var(--el-color-primary, #409eff);
  font-weight: 600;
}

.count-number {
  font-size: 28px;
  font-weight: bold;
  color: var(--el-color-primary, #409eff);
  margin-bottom: 8px;
  line-height: 1;
}

.count-label {
  font-size: 14px;
  color: var(--el-text-color-regular, #606266);
  margin-bottom: 4px;
  font-weight: 500;
}

.count-description {
  font-size: 12px;
  color: var(--el-text-color-secondary, #909399);
  line-height: 1.2;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .message-count-container {
    gap: 12px;
    padding: 12px;
  }
  
  .count-item {
    min-width: 100px;
    padding: 16px 12px;
  }
  
  .count-number {
    font-size: 24px;
  }
  
  .count-label {
    font-size: 13px;
  }
  
  .count-description {
    font-size: 11px;
  }
}

@media (max-width: 480px) {
  .message-count-container {
    flex-direction: column;
    gap: 8px;
  }
  
  .count-item {
    min-width: auto;
  }
}

/* 特殊状态样式 */
.count-item[data-status="warning"] .count-number {
  color: var(--el-color-warning, #e6a23c);
}

.count-item[data-status="danger"] .count-number {
  color: var(--el-color-danger, #f56c6c);
}

.count-item[data-status="success"] .count-number {
  color: var(--el-color-success, #67c23a);
}

/* 选中状态下的特殊状态样式 */
.count-item.selected[data-status="warning"] {
  border-color: var(--el-color-warning, #e6a23c);
  background-color: var(--el-color-warning-light-9, #fdf6ec);
  box-shadow: 0 2px 8px rgba(230, 162, 60, 0.3);
}

.count-item.selected[data-status="danger"] {
  border-color: var(--el-color-danger, #f56c6c);
  background-color: var(--el-color-danger-light-9, #fef0f0);
  box-shadow: 0 2px 8px rgba(245, 108, 108, 0.3);
}

.count-item.selected[data-status="success"] {
  border-color: var(--el-color-success, #67c23a);
  background-color: var(--el-color-success-light-9, #f0f9ff);
  box-shadow: 0 2px 8px rgba(103, 194, 58, 0.3);
}
</style>