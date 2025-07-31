<template>
  <div class="order-count-container">
    <div class="count-item" :data-status="getItemStatus('total')">
      <div class="count-number">{{ orderData.totalCount || 0 }}</div>
      <div class="count-label">单据条数</div>
      <div class="count-description">总计订单数量</div>
    </div>
    
    <div class="count-item" :data-status="getItemStatus('pending')">
      <div class="count-number">{{ orderData.pendingCount || 0 }}</div>
      <div class="count-label">待完工</div>
      <div class="count-description">未完成订单</div>
    </div>
    
    <div class="count-item" :data-status="getItemStatus('overdue')">
      <div class="count-number">{{ orderData.overdueCount || 0 }}</div>
      <div class="count-label">当前超期</div>
      <div class="count-description">正在超期的订单</div>
    </div>
    
    <div class="count-item" :data-status="getItemStatus('overdueCompleted')">
      <div class="count-number">{{ orderData.overdueCompletedCount || 0 }}</div>
      <div class="count-label">超期完成</div>
      <div class="count-description">已超期完成的订单</div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

// 定义props
const props = defineProps({
  // 订单统计数据
  orderData: {
    type: Object,
    default: () => ({
      totalCount: 0,           // 单据条数
      pendingCount: 0,         // 待完工
      overdueCount: 0,         // 当前超期
      overdueCompletedCount: 0 // 超期完成
    })
  },
  // 是否显示描述信息
  showDescription: {
    type: Boolean,
    default: true
  }
})

// 根据数据类型获取状态
const getItemStatus = (type) => {
  switch (type) {
    case 'total':
      return 'primary'
    case 'pending':
      return 'warning'
    case 'overdue':
      return 'danger'
    case 'overdueCompleted':
      return 'success'
    default:
      return 'primary'
  }
}
</script>

<style scoped>
.order-count-container {
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

/* 不同状态的数字颜色 */
.count-item[data-status="primary"] .count-number {
  color: var(--el-color-primary, #409eff);
}

.count-item[data-status="warning"] .count-number {
  color: var(--el-color-warning, #e6a23c);
}

.count-item[data-status="danger"] .count-number {
  color: var(--el-color-danger, #f56c6c);
}

.count-item[data-status="success"] .count-number {
  color: var(--el-color-success, #67c23a);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .order-count-container {
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
  .order-count-container {
    flex-direction: column;
    gap: 8px;
  }
  
  .count-item {
    min-width: auto;
  }
}

/* 隐藏描述时的样式调整 */
.count-item:not(.show-description) .count-description {
  display: none;
}

.count-item:not(.show-description) .count-label {
  margin-bottom: 0;
}
</style>
