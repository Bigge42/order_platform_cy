<template>
  <comp-dialog
    v-model="visible"
    title="回复"
    icon="el-icon-chat-dot-round"
    :width="800"
    :height="600"
    :on-model-close="handleClose"
    :destroy-on-close="true"
  >
    <template #content>
      <div class="reply-dialog-content">
        <el-form
          ref="formRef"
          :model="formData"
          :rules="rules"
          label-width="120px"
          size="default"
        >
          <!-- 只读字段 -->
          <el-row :gutter="20">
            <el-col :span="24">
              <el-form-item label="催单Id：">
                <el-input
                  v-model="formData.urgentOrderID"
                  readonly
                  placeholder="催单Id"
                />
              </el-form-item>
            </el-col>
          </el-row>

          <!-- 可编辑字段 -->
          <el-row :gutter="20">
            <el-col :span="12">
              <el-form-item label="回复交期：" prop="replyDeliveryDate">
                <el-date-picker
                  v-model="formData.replyDeliveryDate"
                  type="date"
                  placeholder="请选择回复交期"
                  style="width: 100%"
                  format="YYYY-MM-DD"
                  value-format="YYYY-MM-DD"
                />
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="20">
            <el-col :span="24">
              <el-form-item label="回复进度：" prop="replyProgress">
                <el-input
                  v-model="formData.replyProgress"
                  type="textarea"
                  :rows="4"
                  placeholder="请输入回复进度信息"
                  maxlength="100"
                  show-word-limit
                />
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="20">
            <el-col :span="24">
              <el-form-item label="回复内容：">
                <el-input
                  v-model="formData.replyContent"
                  type="textarea"
                  :rows="3"
                  placeholder="请输入回复内容"
                  maxlength="1000"
                  show-word-limit
                />
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="20">
            <el-col :span="24">
              <el-form-item label="备注：">
                <el-input
                  v-model="formData.remarks"
                  type="textarea"
                  :rows="3"
                  placeholder="请输入备注信息"
                  maxlength="500"
                  show-word-limit
                />
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </div>
    </template>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="handleClose">
          <i class="el-icon-close"></i>取消
        </el-button>
        <el-button type="primary" @click="handleSubmit" :loading="loading">
          <i class="el-icon-check"></i>确认回复
        </el-button>
      </div>
    </template>
  </comp-dialog>
</template>

<script>
import { ref, reactive, watch, defineComponent } from 'vue'
import { ElMessage } from 'element-plus'
import CompDialog from '@/comp/dialog/index.vue'

export default defineComponent({
  name: 'ReplyDialog',
  components: {
    CompDialog
  },
  props: {
    modelValue: {
      type: Boolean,
      default: false
    },
    messageId: {
      type: [String, Number],
      default: ''
    }
  },
  emits: ['update:modelValue', 'submit'],
  setup(props, { emit }) {
    // Refs
    const formRef = ref()
    const visible = ref(false)
    const loading = ref(false)

    // 表单数据
    const formData = reactive({
      urgentOrderID: '',
      replyDeliveryDate: '',
      replyProgress: '',
      replyContent: '',
      remarks: ''
    })

    // 表单验证规则
    const rules = {
      replyDeliveryDate: [
        { required: true, message: '请选择回复交期', trigger: 'change' }
      ],
      replyProgress: [
        { required: true, message: '请输入回复进度', trigger: 'blur' },
        { min: 1, max: 100, message: '回复进度长度在 1 到 100 个字符', trigger: 'blur' }
      ]
    }

    // 监听弹窗显示状态
    watch(() => props.modelValue, (newVal) => {
      visible.value = newVal
      if (newVal) {
        initFormData()
      }
    })

    watch(() => visible.value, (newVal) => {
      emit('update:modelValue', newVal)
    })

    // 初始化表单数据
    const initFormData = () => {
      formData.urgentOrderID = props.messageId
      formData.replyDeliveryDate = ''
      formData.replyProgress = ''
      formData.replyContent = ''
      formData.remarks = ''
      
      // 重置表单验证状态
      if (formRef.value) {
        formRef.value.clearValidate()
      }
    }

    // 关闭弹窗
    const handleClose = () => {
      visible.value = false
      return true
    }

    // 提交表单
    const handleSubmit = async () => {
      try {
        // 表单验证
        const valid = await formRef.value.validate()
        if (!valid) return

        loading.value = true

        // 准备提交数据
        const submitData = {
          urgentOrderID: formData.urgentOrderID,
          replyDeliveryDate: formData.replyDeliveryDate,
          replyProgress: formData.replyProgress.trim(),
          replyContent: formData.replyContent.trim(),
          remarks: formData.remarks.trim()
        }

        // 触发提交事件
        emit('submit', submitData)
        
        ElMessage.success('回复提交成功')
        handleClose()

      } catch (error) {
        console.error('表单验证失败:', error)
        ElMessage.error('请完善必填信息')
      } finally {
        loading.value = false
      }
    }

    // 暴露方法给父组件
    const open = (data = {}) => {
      visible.value = true
      if (data.messageId) formData.urgentOrderID = data.messageId
    }

    const close = () => {
      visible.value = false
    }

    return {
      formRef,
      visible,
      loading,
      formData,
      rules,
      handleClose,
      handleSubmit,
      open,
      close
    }
  }
})
</script>

<style lang="less" scoped>
.reply-dialog-content {
  height: 100%;
  overflow-y: auto;
  padding: 10px 0;

  .el-form {
    padding-right: 10px;
  }

  .el-form-item {
    margin-bottom: 18px;
  }

  // 只读输入框样式
  :deep(.el-input__inner[readonly]) {
    background-color: #f5f7fa;
    color: #606266;
  }
}

.dialog-footer {
  text-align: right;
  padding: 10px 0;
  border-top: 1px solid #f1f1f1;

  .el-button {
    margin-left: 10px;

    &:first-child {
      margin-left: 0;
    }
  }
}

// 响应式布局
@media (max-width: 768px) {
  .reply-dialog-content {
    .el-col {
      &:not(:last-child) {
        margin-bottom: 10px;
      }
    }
  }
}
</style>
