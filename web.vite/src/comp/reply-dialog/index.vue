<!-- 留言弹窗组件 -->
<template>
  <comp-dialog
    v-model="dialogVisible"
    :title="title"
    :width="width"
    :height="height"
    :icon="icon"
    :on-model-close="handleClose"
  >
    <template #content>
      <div class="reply-dialog-content">
        <!-- 留言输入区域 -->
        <div class="reply-input-wrapper">
          <div class="reply-label">
            {{ label }}
            <span v-if="required" class="required-mark">*</span>
          </div>
          <el-input
            v-model="messageContent"
            type="textarea"
            :placeholder="placeholder"
            :rows="rows"
            :maxlength="maxLength"
            :show-word-limit="showWordLimit"
            resize="vertical"
            class="reply-textarea"
            @input="handleInput"
          />
        </div>
        

      </div>
    </template>
    
    <template #footer>
      <div class="reply-dialog-footer">
        <el-button @click="handleCancel">
          取消
        </el-button>
        <el-button 
          type="primary" 
          :loading="loading"
          :disabled="!canSubmit"
          @click="handleConfirm"
        >
          {{ confirmText }}
        </el-button>
      </div>
    </template>
  </comp-dialog>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { ElMessage } from 'element-plus'
import CompDialog from '@/comp/dialog/index.vue'

// Props 定义
const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  title: {
    type: String,
    default: '留言'
  },
  label: {
    type: String,
    default: '留言内容'
  },
  placeholder: {
    type: String,
    default: '请输入留言内容...'
  },
  width: {
    type: [String, Number],
    default: 600
  },
  height: {
    type: Number,
    default: 400
  },
  icon: {
    type: String,
    default: 'el-icon-chat-dot-round'
  },
  rows: {
    type: Number,
    default: 6
  },
  maxLength: {
    type: Number,
    default: 500
  },
  showWordLimit: {
    type: Boolean,
    default: true
  },
  required: {
    type: Boolean,
    default: true
  },

  confirmText: {
    type: String,
    default: '发送'
  },
  loading: {
    type: Boolean,
    default: false
  },
  // 预设内容
  content: {
    type: String,
    default: ''
  }
})

// Emits 定义
const emit = defineEmits(['update:modelValue', 'confirm', 'cancel', 'input'])

// 响应式数据
const dialogVisible = ref(false)
const messageContent = ref('')

// 计算属性
const canSubmit = computed(() => {
  if (!props.required) return true
  return messageContent.value && messageContent.value.trim().length > 0
})

// 监听 v-model
watch(() => props.modelValue, (newVal) => {
  dialogVisible.value = newVal
  if (newVal && props.content) {
    messageContent.value = props.content
  }
})

watch(() => dialogVisible.value, (newVal) => {
  emit('update:modelValue', newVal)
})

// 监听预设内容变化
watch(() => props.content, (newVal) => {
  if (newVal !== undefined) {
    messageContent.value = newVal
  }
})

// 方法
const handleClose = () => {
  return true // 允许关闭
}

const handleCancel = () => {
  dialogVisible.value = false
  messageContent.value = ''
  emit('cancel')
}

const handleConfirm = () => {
  if (!canSubmit.value) {
    ElMessage.warning('请输入留言内容')
    return
  }
  
  const content = messageContent.value.trim()
  emit('confirm', content)
  
  // 如果没有外部loading控制，则关闭弹窗并清空内容
  if (!props.loading) {
    dialogVisible.value = false
    messageContent.value = ''
  }
}

const handleInput = (value) => {
  emit('input', value)
}

// 对外暴露方法
defineExpose({
  open: (content = '') => {
    messageContent.value = content
    dialogVisible.value = true
  },
  close: () => {
    dialogVisible.value = false
  },
  clear: () => {
    messageContent.value = ''
  },
  setContent: (content) => {
    messageContent.value = content
  }
})
</script>

<style lang="less" scoped>
.reply-dialog-content {
  .reply-input-wrapper {
    
    .reply-label {
      margin-bottom: 8px;
      font-size: 14px;
      color: #606266;
      font-weight: 500;
      
      .required-mark {
        color: #f56c6c;
        margin-left: 2px;
      }
    }
    
    .reply-textarea {
      :deep(.el-textarea__inner) {
        height: 220px;
        min-height: 120px;
        resize: vertical;
        font-family: inherit;
        line-height: 1.5;
      }
    }
  }
  

}

.reply-dialog-footer {
  text-align: right;
  
  .el-button {
    margin-left: 12px;
    
    &:first-child {
      margin-left: 0;
    }
  }
}
</style> 