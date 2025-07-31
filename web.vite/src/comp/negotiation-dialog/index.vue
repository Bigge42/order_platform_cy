<!-- 发起协商弹窗组件 -->
<template>
  <comp-dialog
    v-model="visible"
    :title="title"
    :width="width"
    :height="height"
    icon="el-icon-chat-dot-round"
    :on-model-close="handleClose"
  >
    <template #content>
      <div class="negotiation-dialog-content">
        <el-form
          ref="formRef"
          :model="formData"
          :rules="formRules"
          label-width="100px"
          label-position="left"
        >
          <el-form-item label="协商原因" prop="NegotiationReason">
            <el-select
              v-model="formData.NegotiationReason"
              placeholder="请选择协商原因"
              style="width: 100%"
              clearable
            >
              <el-option
                v-for="option in reasonOptions"
                :key="option.value"
                :label="option.label"
                :value="option.value"
              />
            </el-select>
          </el-form-item>

          <el-form-item label="业务类型">
            <el-input
              v-model="businessTypeLabel"
              placeholder="业务类型"
              readonly
              style="width: 100%"
            />
          </el-form-item>

          <el-form-item label="协商日期" prop="NegotiationDate">
            <el-date-picker
              v-model="formData.NegotiationDate"
              type="date"
              placeholder="请选择协商日期"
              style="width: 100%"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD"
            />
          </el-form-item>

          <el-form-item label="默认负责人" prop="DefaultResPerson">
            <el-input
              v-model="formData.DefaultResPerson"
              placeholder="默认负责人"
              readonly
              style="width: 100%"
            />
          </el-form-item>

          <el-form-item label="指定负责人">
            <div class="assigned-person-container">
              <el-tag
                v-if="formData.AssignedResPerson"
                type="info"
                size="large"
                closable
                @close="removeAssignedPerson"
                class="person-tag"
              >
                {{ formData.AssignedResPerson.name }}
              </el-tag>
              <el-button
                size="small"
                type="primary"
                @click="openPersonSelector"
                :icon="User"
              >
                {{ formData.AssignedResPerson ? '重新选择' : '选择人员' }}
              </el-button>
            </div>
          </el-form-item>

          <el-form-item label="协商内容" prop="NegotiationContent">
            <el-input
              v-model="formData.NegotiationContent"
              type="textarea"
              :rows="6"
              placeholder="请输入协商内容"
              maxlength="500"
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
        <el-button type="primary" @click="handleConfirm">发起协商</el-button>
      </div>
    </template>
  </comp-dialog>

  <!-- 人员选择器 -->
  <PersonSelector
    v-model="personSelectorVisible"
    :selectedPersonId="selectedPersonId"
    @confirm="handlePersonConfirm"
    @cancel="handlePersonCancel"
  />
</template>

<script setup>
import { ref, computed, watch, nextTick, onMounted, getCurrentInstance } from 'vue'
import { ChatDotRound, User } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import CompDialog from '@/comp/dialog/index.vue'
import PersonSelector from '@/comp/person-selector/index.vue'

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
    default: '发起协商'
  },
  width: {
    type: [Number, String],
    default: 800
  },
  height: {
    type: Number,
    default: 600
  }
})

// Emits 定义
const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

// 响应式数据
const visible = ref(false)
const formRef = ref(null)
const personSelectorVisible = ref(false)
const selectedPersonId = ref('')
const { proxy } = getCurrentInstance()

// 表单数据
const formData = ref({
  NegotiationReason: '',
  NegotiationDate: '',
  BusinessType: '',
  DefaultResPerson: '',
  AssignedResPerson: null,
  NegotiationContent: ''
})

// 协商原因选项
const reasonOptions = ref([])
// 业务类型选项
const businessTypeOptions = ref([])
// 业务类型显示标签
const businessTypeLabel = ref('')

// 获取协商原因字典数据
const loadReasonOptions = async () => {
  try {
    const params = {
      page: 1,
      rows: 30,
      sort: "OrderNo,CreateDate",
      order: "desc",
      wheres: "[]",
      value: 110,
      tableName: null,
      isCopyClick: false
    }
    
    const result = await proxy.http.post('api/Sys_Dictionary/getDetailPage', params, false)
    
    if (result && result.rows) {
      reasonOptions.value = result.rows
        .filter(item => item.Enable === 1) // 只显示启用的选项
        .map(item => ({
          value: item.DicValue,
          label: item.DicName
        }))
    }
  } catch (error) {
    console.error('获取协商原因选项失败:', error)
    ElMessage.error('获取协商原因选项失败')
  }
}

// 获取业务类型字典数据
const loadBusinessTypeOptions = async () => {
  try {
    const params = {
      page: 1,
      rows: 30,
      sort: "OrderNo,CreateDate",
      order: "desc",
      wheres: "[]",
      value: 109,
      tableName: null,
      isCopyClick: false
    }
    
    const result = await proxy.http.post('api/Sys_Dictionary/getDetailPage', params, false)
    
    if (result && result.rows) {
      businessTypeOptions.value = result.rows
        .filter(item => item.Enable === 1) // 只显示启用的选项
        .map(item => ({
          value: item.DicValue,
          label: item.DicName
        }))
    }
  } catch (error) {
    console.error('获取业务类型选项失败:', error)
    ElMessage.error('获取业务类型选项失败')
  }
}

// 默认负责人信息存储
const defaultResponsibleInfo = ref({
  name: '',
  loginName: ''
})

// 根据业务类型获取默认负责人
const loadDefaultResponsible = async (businessType, supplierCode = null) => {
  if (!businessType) return
  
  try {
    // 如果BusinessType为PO，调用新接口
    if (businessType === 'PO') {
      const params = {
        page: 1,
        rows: 30,
        sort: "CreateDate",
        order: "desc",
        wheres: `[{"name":"Code","value":"${supplierCode || ''}"}]`,
        resetPage: true
      }
      
      const result = await proxy.http.post('api/OCP_PurchaseSupplierMapping/getPageData', params, false)
      
      if (result && result.rows && result.rows.length > 0) {
        const row = result.rows[0]
        formData.value.DefaultResPerson = row.BusinessPersonName
        defaultResponsibleInfo.value = {
          name: row.BusinessPersonName,
          loginName: row.BusinessPersonLoginName || ''
        }
      } else {
        formData.value.DefaultResPerson = ''
        defaultResponsibleInfo.value = { name: '', loginName: '' }
      }
    } else {
      // 否则调用原接口逻辑不变
      const params = {
        page: 1,
        rows: 30,
        sort: "CreateDate",
        order: "desc",
        wheres: `[{"name":"BusinessType","value":"${businessType}"}]`,
        resetPage: true
      }
      
      const result = await proxy.http.post('api/OCP_BusinessTypeResponsible/getPageData', params, false)
      
      if (result && result.rows && result.rows.length > 0) {
        const row = result.rows[0]
        formData.value.DefaultResPerson = row.DefaultResponsibleName
        defaultResponsibleInfo.value = {
          name: row.DefaultResponsibleName,
          loginName: row.DefaultResponsibleLoginName || ''
        }
      } else {
        formData.value.DefaultResPerson = ''
        defaultResponsibleInfo.value = { name: '', loginName: '' }
      }
    }
  } catch (error) {
    console.error('获取默认负责人失败:', error)
    formData.value.DefaultResPerson = ''
    defaultResponsibleInfo.value = { name: '', loginName: '' }
  }
}

// 根据业务类型key获取对应的label
const getBusinessTypeLabel = (businessTypeKey) => {
  if (!businessTypeKey || !businessTypeOptions.value.length) {
    return ''
  }
  
  const option = businessTypeOptions.value.find(item => item.value === businessTypeKey)
  return option ? option.label : businessTypeKey
}

// 表单验证规则
const formRules = ref({
  NegotiationReason: [
    { required: true, message: '请选择协商原因', trigger: 'change' }
  ],
  NegotiationDate: [
    { required: true, message: '请选择协商日期', trigger: 'change' }
  ],
  NegotiationContent: [
    { required: true, message: '请输入协商内容', trigger: 'blur' },
    { min: 5, message: '协商内容至少输入5个字符', trigger: 'blur' }
  ]
})

// 清除校验状态
const clearValidation = () => {
  nextTick(() => {
    if (formRef.value) {
      formRef.value.clearValidate()
    }
  })
}

// 人员选择相关方法
const openPersonSelector = () => {
  selectedPersonId.value = formData.value.AssignedResPerson ? formData.value.AssignedResPerson.id : ''
  personSelectorVisible.value = true
}

const handlePersonConfirm = (person) => {
  console.log('接收到选中的人员数据:', person)
  formData.value.AssignedResPerson = person
  personSelectorVisible.value = false
  console.log('已设置指定负责人:', formData.value.AssignedResPerson)
}

const handlePersonCancel = () => {
  personSelectorVisible.value = false
}

const removeAssignedPerson = () => {
  formData.value.AssignedResPerson = null
}

// 监听业务类型选项数据变化，用于更新显示标签
watch(
  () => businessTypeOptions.value,
  (newOptions) => {
    if (newOptions.length > 0 && formData.value.BusinessType) {
      businessTypeLabel.value = getBusinessTypeLabel(formData.value.BusinessType)
    }
  },
  { deep: true }
)

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
    
    // 弹窗打开时，根据传入的数据重置表单
    if (newVal && props.data) {
      nextTick(() => {
        resetForm()
        // 根据props.data设置默认值
        if (props.data.NegotiationDate) {
          formData.value.NegotiationDate = props.data.NegotiationDate
        }
        if (props.data.BusinessType) {
          formData.value.BusinessType = props.data.BusinessType
          businessTypeLabel.value = getBusinessTypeLabel(props.data.BusinessType)
          // 根据业务类型获取默认负责人，如果是PO类型需要传入SupplierCode
          const supplierCode = props.data.SupplierCode || ''
          loadDefaultResponsible(props.data.BusinessType, supplierCode)
        }
        if (props.data.AssignedResPerson) {
          formData.value.AssignedResPerson = props.data.AssignedResPerson
        }
      })
    } else if (!newVal) {
      // 弹窗关闭时重置默认负责人信息和校验状态
      defaultResponsibleInfo.value = { name: '', loginName: '' }
      clearValidation()
    }
  }
)

// 重置表单
const resetForm = () => {
  if (formRef.value) {
    formRef.value.resetFields()
  }
  formData.value = {
    NegotiationReason: '',
    NegotiationDate: '',
    BusinessType: '',
    DefaultResPerson: '',
    AssignedResPerson: null,
    NegotiationContent: ''
  }
  businessTypeLabel.value = ''
  defaultResponsibleInfo.value = { name: '', loginName: '' }
}

// 处理关闭事件
const handleClose = () => {
  clearValidation()
  visible.value = false
  return true
}

// 处理取消事件
const handleCancel = () => {
  clearValidation()
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
      ...props.data, // 包含行数据
      ...formData.value, // 包含表单数据
      // 添加默认负责人的loginName
      DefaultResPersonLoginName: defaultResponsibleInfo.value.loginName || ''
    }
    
    // 如果没有选择指定负责人，则使用默认负责人的登录名
    if (!submitData.AssignedResPerson) {
      submitData.AssignedResPerson = {
        id: defaultResponsibleInfo.value.loginName,
        name: defaultResponsibleInfo.value.name,
        userName: defaultResponsibleInfo.value.loginName,
        userTrueName: defaultResponsibleInfo.value.name
      }
    }
    
    console.log('发起协商确认', submitData)
    console.log('提交数据中的指定负责人:', submitData.AssignedResPerson)
    console.log('默认负责人信息:', defaultResponsibleInfo.value)
    
    // 先发送数据给外部
    emit('confirm', submitData)
    
    // 关闭弹窗
    visible.value = false
    
    // 延迟重置表单，确保外部已接收到数据
    nextTick(() => {
      resetForm()
    })
    
  } catch (error) {
    console.log('表单验证失败:', error)
    ElMessage.warning('请完善必填信息')
  }
}

// 组件挂载时加载字典数据
onMounted(() => {
  loadReasonOptions()
  loadBusinessTypeOptions()
})

// 对外暴露的方法
defineExpose({
  open: () => {
    visible.value = true
  },
  close: () => {
    clearValidation()
    resetForm()
    visible.value = false
  },
  resetForm,
  clearValidation
})
</script>

<style lang="less" scoped>
.negotiation-dialog-content {
  padding: 20px;
  min-height: 400px;
  
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
    
    .el-select,
    .el-date-editor {
      width: 100%;
    }
    
    .el-textarea {
      .el-textarea__inner {
        font-family: inherit;
        resize: none;
      }
    }
  }
}

.assigned-person-container {
  display: flex;
  flex-direction: row;
  gap: 12px;
  align-items: center;
}

.person-tag {
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.dialog-footer {
  text-align: right;
  padding: 6px 12px 0 0;
  
  .el-button + .el-button {
    margin-left: 12px;
  }
}
</style> 