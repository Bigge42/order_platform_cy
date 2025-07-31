<template>
  <div class="message-list">
    <div v-if="list.length === 0" class="empty-state">
      <el-empty :description="type === 'urgent' ? '暂无催单消息' : '暂无协商消息'" />
    </div>
    
    <div v-else>
      <div 
        v-for="item in list" 
        :key="getItemKey(item)"
        class="message-item"
        @click="handleItemClick(item)"
      >
        <!-- 消息头部 -->
        <div class="message-header">
          <div class="message-title">
            <el-icon class="message-icon" :class="type === 'urgent' ? 'urgent-icon' : 'negotiate-icon'">
              <Bell v-if="type === 'urgent'" />
              <ChatDotSquare v-else />
            </el-icon>
            <span class="message-type-label">{{ type === 'urgent' ? '催单' : '协商' }}</span>
          </div>
          <div class="message-time">
            {{ formatTime(item.CreateDate || item.createDate) }}
          </div>
        </div>

        <!-- 消息内容 -->
        <div class="message-content">
          <!-- 单据编号 -->
          <div v-if="item.BillNo" class="content-item">
            <span class="content-label">单据编号：</span>
            <span class="content-value">{{ item.BillNo }}</span>
          </div>

          <!-- 催单相关字段 -->
          <template v-if="type === 'urgent'">
            <div v-if="item.UrgentContent" class="content-item">
              <span class="content-label">催单内容：</span>
              <span class="content-value">{{ item.UrgentContent }}</span>
            </div>
            <div v-if="item.UrgencyLevel" class="content-item">
              <span class="content-label">紧急等级：</span>
              <el-tag 
                :type="getUrgencyLevelType(item.UrgencyLevel)" 
                size="small"
                class="content-tag"
              >
                {{ item.UrgencyLevel }}
              </el-tag>
            </div>
            <div v-if="item.UrgentStatus" class="content-item">
              <span class="content-label">催单状态：</span>
              <el-tag 
                :type="getUrgentStatusType(item.UrgentStatus)" 
                size="small"
                class="content-tag"
              >
                {{ item.UrgentStatus }}
              </el-tag>
            </div>
          </template>

          <!-- 协商相关字段 -->
          <template v-if="type === 'negotiate'">
            <div v-if="item.NegotiationType" class="content-item">
              <span class="content-label">协商类型：</span>
              <el-tag 
                type="warning" 
                size="small"
                class="content-tag"
              >
                {{ item.NegotiationType }}
              </el-tag>
            </div>
            <div v-if="item.NegotiationReason" class="content-item">
              <span class="content-label">协商原因：</span>
              <span class="content-value">{{ item.NegotiationReason }}</span>
            </div>
            <div v-if="item.NegotiationDate" class="content-item">
              <span class="content-label">协商日期：</span>
              <span class="content-value">{{ formatTime(item.NegotiationDate) }}</span>
            </div>
            <div v-if="item.NegotiationContent" class="content-item">
              <span class="content-label">协商内容：</span>
              <span class="content-value">{{ item.NegotiationContent }}</span>
            </div>
            <div v-if="item.NegotiationStatus" class="content-item">
              <span class="content-label">协商状态：</span>
              <el-tag 
                :type="getNegotiationStatusType(item.NegotiationStatus)" 
                size="small"
                class="content-tag"
              >
                {{ item.NegotiationStatus }}
              </el-tag>
            </div>
          </template>
        </div>


      </div>
    </div>
  </div>
</template>

<script setup>
import { Bell, ChatDotSquare } from '@element-plus/icons-vue'

// 定义props
const props = defineProps({
  list: {
    type: Array,
    default: () => []
  },
  type: {
    type: String,
    required: true,
    validator: value => ['urgent', 'negotiate'].includes(value)
  }
})

// 定义emits
const emit = defineEmits(['itemClick'])

// 获取项目的唯一key
const getItemKey = (item) => {
  if (props.type === 'urgent') {
    return `urgent_${item.UrgentOrderID || item.id || Math.random()}`
  } else {
    return `negotiate_${item.NegotiationID || item.id || Math.random()}`
  }
}

// 处理项目点击
const handleItemClick = (item) => {
  emit('itemClick', item, false)
}

// 格式化时间
const formatTime = (timeStr) => {
  if (!timeStr) return ''
  
  try {
    const date = new Date(timeStr)
    return date.toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch (error) {
    return timeStr
  }
}

// 获取紧急等级标签类型
const getUrgencyLevelType = (level) => {
  if (!level) return 'info'
  
  const levelStr = level.toString().toLowerCase()
  if (levelStr.includes('高') || levelStr.includes('紧急') || levelStr.includes('urgent')) {
    return 'danger'
  } else if (levelStr.includes('中') || levelStr.includes('medium')) {
    return 'warning'
  } else if (levelStr.includes('低') || levelStr.includes('low')) {
    return 'info'
  }
  return 'info'
}

// 获取催单状态标签类型
const getUrgentStatusType = (status) => {
  if (!status) return 'info'
  
  const statusStr = status.toString().toLowerCase()
  if (statusStr.includes('已') || statusStr.includes('完成') || statusStr.includes('处理')) {
    return 'success'
  } else if (statusStr.includes('催单中') || statusStr.includes('进行中')) {
    return 'warning'
  } else if (statusStr.includes('超时') || statusStr.includes('逾期')) {
    return 'danger'
  }
  return 'info'
}

// 获取协商状态标签类型
const getNegotiationStatusType = (status) => {
  if (!status) return 'info'
  
  const statusStr = status.toString().toLowerCase()
  if (statusStr.includes('已协商') || statusStr.includes('同意') || statusStr.includes('完成')) {
    return 'success'
  } else if (statusStr.includes('协商中') || statusStr.includes('进行中')) {
    return 'warning'
  } else if (statusStr.includes('拒绝') || statusStr.includes('失败')) {
    return 'danger'
  }
  return 'info'
}
</script>

<style scoped lang="less">
.message-list {
  max-height: 400px;
  overflow-y: auto;
  padding: 8px 12px;
}

.empty-state {
  padding: 20px;
  text-align: center;
}

.message-item {
  background: #ffffff;
  border: 1px solid #e4e7ed;
  border-radius: 6px;
  margin-bottom: 8px;
  padding: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
  
  &:hover {
    border-color: #409eff;
    box-shadow: 0 2px 6px rgba(64, 158, 255, 0.12);
    transform: translateY(-1px);
  }
  
  &:last-child {
    margin-bottom: 0;
  }
}

.message-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  padding-bottom: 6px;
  border-bottom: 1px solid #f5f5f5;
}

.message-title {
  display: flex;
  align-items: center;
  gap: 6px;
}

.message-icon {
  font-size: 14px;
  
  &.urgent-icon {
    color: #f56c6c;
  }
  
  &.negotiate-icon {
    color: #e6a23c;
  }
}

.message-type-label {
  font-weight: 600;
  font-size: 13px;
  color: #303133;
}

.message-time {
  font-size: 11px;
  color: #909399;
}

.message-content {
  margin-bottom: 0;
}

.content-item {
  display: flex;
  align-items: flex-start;
  margin-bottom: 6px;
  
  &:last-child {
    margin-bottom: 0;
  }
}

.content-label {
  font-size: 12px;
  color: #606266;
  font-weight: 500;
  min-width: 70px;
  flex-shrink: 0;
  line-height: 1.4;
}

.content-value {
  font-size: 12px;
  color: #303133;
  flex: 1;
  word-break: break-all;
  line-height: 1.4;
}

.content-tag {
  font-weight: 500;
  font-size: 11px;
}

/* 滚动条样式 */
.message-list::-webkit-scrollbar {
  width: 6px;
}

.message-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.message-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.message-list::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}
</style>