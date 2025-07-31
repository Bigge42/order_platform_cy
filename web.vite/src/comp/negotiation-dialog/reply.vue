<!-- 协商回复弹窗组件 -->
<template>
  <comp-dialog
    v-model="visible"
    :title="title"
    :width="width"
    :height="height"
    icon="el-icon-chat-line-round"
    :on-model-close="handleClose"
  >
    <template #content>
      <div class="negotiation-reply-content">
        <el-form
          ref="formRef"
          :model="formData"
          :rules="formRules"
          label-width="100px"
          label-position="left"
        >
          <el-form-item label="协商日期">
            <div class="date-comparison">
                <div class="date-item">
                  <span class="date-label">当前值</span>
                  <el-date-picker
                    :model-value="currentDate"
                    type="date"
                    style="width: 100%"
                    format="YYYY-MM-DD"
                    value-format="YYYY-MM-DD"
                    readonly
                    disabled
                    class="readonly-date-picker"
                  />
                </div>
                <div class="date-item">
                  <span class="date-label">变更值</span>
                  <el-date-picker
                    v-model="formData.replyDeliveryDate"
                    type="date"
                    placeholder="请选择变更日期"
                    style="width: 100%"
                    format="YYYY-MM-DD"
                    value-format="YYYY-MM-DD"
                  />
                </div>
            </div>
          </el-form-item>

          <el-form-item label="协商状态" prop="negotiationStatus">
            <el-radio-group v-model="formData.negotiationStatus">
              <el-radio value="已同意">同意</el-radio>
              <el-radio value="拒绝">拒绝</el-radio>
            </el-radio-group>
          </el-form-item>

          <el-form-item label="处理回复" prop="replyContent">
            <el-input
              v-model="formData.replyContent"
              type="textarea"
              :rows="8"
              placeholder="请输入处理回复内容"
              maxlength="1000"
              show-word-limit
              resize="none"
            />
          </el-form-item>
        </el-form>
      </div>
    </template>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="handleCancel">取消</el-button>
        <el-button type="primary" @click="handleConfirm">发送回复</el-button>
      </div>
    </template>
  </comp-dialog>
</template>

<script setup>
import { ref, computed, watch, nextTick } from 'vue'
import { ChatLineRound } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import CompDialog from '@/comp/dialog/index.vue'

// Props 定义
const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  data: {
    type: Object,
    default: () => ({})
  },
  title: {
    type: String,
    default: '协商回复'
  },
  width: {
    type: [Number, String],
    default: 800
  },
  height: {
    type: Number,
    default: 500
  }
})

// Emits 定义
const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

// 响应式数据
const visible = ref(false)
const formRef = ref(null)

// 表单数据
const formData = ref({
  replyDeliveryDate: '',
  replyContent: '',
  negotiationStatus: ''
})

// 当前日期（从传入数据中获取）
const currentDate = computed(() => {
  return props.data?.NegotiationDate || ''
})

// 表单验证规则
const formRules = ref({
  negotiationStatus: [
    { required: true, message: '请选择协商状态', trigger: 'change' }
  ],
  replyContent: [
    { required: true, message: '请输入处理回复', trigger: 'blur' },
    { min: 5, message: '处理回复至少输入5个字符', trigger: 'blur' },
    { max: 1000, message: '处理回复不能超过1000个字符', trigger: 'blur' }
  ]
})

// 监听 modelValue 变化
watch(
  () => props.modelValue,
  (newVal) => {
    visible.value = newVal
  },
  { immediate: true }
)

// 监听 visible 变化，同步到父组件
watch(
  () => visible.value,
  (newVal) => {
    emit('update:modelValue', newVal)
    
    // 弹窗打开时重置表单
    if (newVal) {
      nextTick(() => {
        resetForm()
      })
    }
  }
)

// 重置表单
const resetForm = () => {
  if (formRef.value) {
    formRef.value.resetFields()
  }
  formData.value = {
    replyDeliveryDate: '',
    replyContent: '',
    negotiationStatus: ''
  }
}

// 处理关闭事件
const handleClose = () => {
  visible.value = false
  return true
}

// 处理取消事件
const handleCancel = () => {
  resetForm()
  emit('cancel')
  visible.value = false
}

// 处理确认事件
const handleConfirm = async () => {
  try {
    // 表单验证
    await formRef.value.validate()
    
    // 准备提交数据
    const submitData = {
      negotiationID: props.data.NegotiationID, // 协商ID
      replyContent: formData.value.replyContent,
      replyDeliveryDate: formData.value.replyDeliveryDate,
      negotiationStatus: formData.value.negotiationStatus
    }
    
    console.log('协商回复确认', submitData)
    emit('confirm', submitData)
    
    // 重置表单并关闭弹窗
    resetForm()
    visible.value = false
    
  } catch (error) {
    console.log('表单验证失败:', error)
    ElMessage.warning('请完善必填信息')
  }
}

// 对外暴露的方法
defineExpose({
  open: () => {
    visible.value = true
  },
  close: () => {
    resetForm()
    visible.value = false
  },
  resetForm
})
</script>

<style lang="less" scoped>
.negotiation-reply-content {
  padding: 20px;
  min-height: 350px;
  
  .el-form {
    .el-form-item {
      margin-bottom: 20px;
      
      &:last-child {
        margin-bottom: 0;
      }
    }
    
    .el-form-item__label {
      font-weight: 500;
      color: var(--el-text-color-primary);
    }
    
    .date-comparison {
      width: 100%;
      display: flex;
      gap: 16px;
      
      .date-item {
        flex: 1;
        display: flex;
        align-items: center;
        
        .date-label {
          min-width: 40px;
          font-size: 14px;
          color: var(--el-text-color-regular);
          margin-right: 12px;
          white-space: nowrap;
        }
        
        .el-input,
        .el-date-editor {
          flex: 1;
        }
        
        .readonly-date-picker {
          :deep(.el-input__wrapper) {
            background-color: var(--el-fill-color-light);
            cursor: not-allowed;
          }
          
          :deep(.el-input__inner) {
            background-color: var(--el-fill-color-light);
            cursor: not-allowed;
          }
        }
      }
      
      // 响应式设计：在小屏幕下恢复垂直布局
      @media (max-width: 768px) {
        flex-direction: column;
        gap: 12px;
      }
    }
    
    .el-radio-group {
      .el-radio {
        margin-right: 20px;
        
        &:last-child {
          margin-right: 0;
        }
      }
    }
    
    .el-textarea {
      .el-textarea__inner {
        font-family: inherit;
        resize: none;
      }
    }
  }
}

.dialog-footer {
  text-align: right;
  padding: 6px 12px 0 0;
  
  .el-button + .el-button {
    margin-left: 12px;
  }
}
</style> 