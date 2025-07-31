<template>
  <div class="dashboard-data-summary">
    <!-- 标题 -->
    <div v-if="title" class="summary-title">{{ title }}</div>
    
    <!-- 数据汇总块 -->
    <div class="summary-container">
      <div 
        v-for="(block, blockIndex) in summaryData" 
        :key="blockIndex"
        class="summary-block"
      >
        <!-- 总数展示 -->
        <div class="summary-total-item">
          <div class="total-left">
            <div class="total-label">{{ block.label }}</div>
          </div>
          <div class="total-right">
            <div class="total-number">{{ block.total }}</div>
            <div class="total-unit">{{ block.unit }}</div>
          </div>
        </div>
        
        <!-- 详细项目 -->
        <div class="summary-details">
          <div 
            v-for="(item, itemIndex) in block.items" 
            :key="itemIndex"
            class="detail-item"
            :data-status="item.status || 'primary'"
          >
            <div class="detail-count">{{ item.count }}</div>
            <div class="detail-label">{{ item.label }}</div>
            <div class="detail-percentage">{{ item.percentage }}%</div>
            <div class="detail-description">{{ item.description }}</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue'

// 定义props
const props = defineProps({
  // 汇总标题
  title: {
    type: String,
    default: ''
  },
  // 汇总数据
  summaryData: {
    type: Array,
    default: () => []
  }
})

/*
summaryData 数据结构示例：
[
  {
    label: '总任务订单',
    total: 16,
    unit: '个任务单',
    icon: '📋',
    items: [
      {
        count: 12,
        label: '已在订单单',
        percentage: 75,
        description: '开始单单',
        status: 'success'
      },
      {
        count: 4,
        label: '未开始订单',
        percentage: 25,
        description: '未开始单单',
        status: 'warning'
      }
    ]
  },
  {
    label: '总任务套数',
    total: 891,
    unit: '个套数',
    icon: '📦',
    items: [
      {
        count: 860,
        label: '已在套数',
        percentage: 97,
        description: '开始单单',
        status: 'success'
      },
      {
        count: 31,
        label: '未开始套数',
        percentage: 3,
        description: '未开始单单',
        status: 'warning'
      }
    ]
  }
]
*/
</script>

<style scoped>
.dashboard-data-summary {
  margin: 20px 0;
}

.summary-title {
  font-size: 18px;
  font-weight: bold;
  color: var(--el-text-color-primary, #303133);
  margin-bottom: 16px;
  padding-left: 12px;
  border-left: 4px solid var(--el-color-primary, #409eff);
}

.summary-container {
  display: flex;
  gap: 16px;
  padding: 12px;
  background-color: var(--el-bg-color-page, #f5f7fa);
  border-radius: 8px;
  flex-wrap: wrap;
}

.summary-block {
  flex: 1;
  min-width: 280px;
  background-color: var(--el-bg-color, #ffffff);
  border-radius: 8px;
  padding: 12px;
  border: 1px solid var(--el-border-color-lighter, #ebeef5);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
}

.summary-block:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.summary-total-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 10px;
  padding-bottom: 6px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  gap: 16px;
}

.total-left {
  flex: 0 0 auto;
  max-width: 60%;
}

.total-label {
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary, #303133);
  line-height: 1.2;
}

.total-right {
  text-align: right;
  flex: 0 0 auto;
}

.total-number {
  font-size: 28px;
  font-weight: bold;
  color: var(--el-color-primary, #409eff);
  line-height: 1;
  margin-bottom: 1px;
}

.total-unit {
  font-size: 12px;
  color: var(--el-text-color-regular, #606266);
}

.summary-details {
  display: flex;
  gap: 8px;
}

.detail-item {
  flex: 1;
  text-align: center;
  padding: 12px 8px;
  background-color: var(--el-bg-color-page, #f5f7fa);
  border-radius: 6px;
  border: 1px solid var(--el-border-color-lighter, #ebeef5);
  transition: all 0.3s ease;
}

.detail-item:hover {
  background-color: var(--el-bg-color, #ffffff);
  border-color: var(--el-border-color, #dcdfe6);
}

.detail-count {
  font-size: 22px;
  font-weight: bold;
  margin-bottom: 4px;
  line-height: 1;
}

.detail-label {
  font-size: 13px;
  color: var(--el-text-color-regular, #606266);
  margin-bottom: 2px;
  font-weight: 500;
}

.detail-percentage {
  font-size: 15px;
  font-weight: bold;
  margin-bottom: 2px;
}

.detail-description {
  font-size: 11px;
  color: var(--el-text-color-secondary, #909399);
  line-height: 1.2;
}

/* 不同状态的数字颜色 */
.detail-item[data-status="primary"] .detail-count,
.detail-item[data-status="primary"] .detail-percentage {
  color: var(--el-color-primary, #409eff);
}

.detail-item[data-status="success"] .detail-count,
.detail-item[data-status="success"] .detail-percentage {
  color: var(--el-color-success, #67c23a);
}

.detail-item[data-status="warning"] .detail-count,
.detail-item[data-status="warning"] .detail-percentage {
  color: var(--el-color-warning, #e6a23c);
}

.detail-item[data-status="danger"] .detail-count,
.detail-item[data-status="danger"] .detail-percentage {
  color: var(--el-color-danger, #f56c6c);
}

.detail-item[data-status="info"] .detail-count,
.detail-item[data-status="info"] .detail-percentage {
  color: var(--el-color-info, #909399);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .summary-container {
    gap: 12px;
    padding: 8px;
  }
  
  .summary-block {
    min-width: 260px;
    padding: 10px;
  }
  
  .total-number {
    font-size: 24px;
  }
  
  .detail-count {
    font-size: 18px;
  }
  
  .detail-percentage {
    font-size: 13px;
  }
}

@media (max-width: 480px) {
  .summary-container {
    flex-direction: column;
    gap: 12px;
  }
  
  .summary-block {
    min-width: auto;
  }
  
  .summary-details {
    flex-direction: column;
    gap: 12px;
  }
  
  .detail-item {
    padding: 12px;
  }
}
</style> 