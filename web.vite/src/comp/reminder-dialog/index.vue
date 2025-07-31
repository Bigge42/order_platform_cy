<template>
  <div class="reminder-dialog">
    <CompDialog
      v-model="visible"
      title="催单"
      width="800"
      :height="height"
      :padding="0"
      :footer="true"
      :onModelClose="handleDialogClose"
    >
      <template #content>
        <div class="dialog-content">
          <el-form
            ref="formRef"
            :model="formData"
            :rules="formRules"
            label-width="120px"
            label-position="left"
          >
            <el-form-item label="业务类型">
              <el-input
                v-model="businessTypeLabel"
                placeholder="业务类型"
                readonly
                style="width: 100%"
              />
            </el-form-item>
            
            <el-form-item label="默认负责人" prop="DefaultResPerson">
              <el-input
                v-model="formData.DefaultResPerson"
                placeholder="默认负责人"
                readonly
              />
            </el-form-item>
            
            <el-form-item label="紧急等级" prop="UrgencyLevel">
              <el-select
                v-model="formData.UrgencyLevel"
                placeholder="请选择紧急等级"
                clearable
              >
                <el-option
                  v-for="item in urgentLevelOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                />
              </el-select>
            </el-form-item>
            
            <el-form-item label="指定回复时间" prop="AssignedReplyTime">
              <el-input
                v-model="formData.AssignedReplyTime"
                type="number"
                :min="1"
                :max="999"
                placeholder="请输入时间"
                clearable
              >
                <template #append>
                  <el-select 
                    v-model="formData.TimeUnit" 
                    placeholder="单位" 
                    style="width: 80px"
                  >
                    <el-option label="分钟" value="1" />
                    <el-option label="小时" value="2" />
                    <el-option label="天" value="3" />
                  </el-select>
                </template>
              </el-input>
            </el-form-item>
            
            <el-form-item label="指定负责人">
              <div class="assigned-persons-container">
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
            
            <el-form-item label="催单类型" prop="UrgentType">
              <el-radio-group v-model="formData.UrgentType">
                <el-radio :label="0">催交期</el-radio>
                <el-radio :label="1">催进度</el-radio>
              </el-radio-group>
            </el-form-item>
            
            <el-form-item label="催单内容" prop="UrgentContent">
              <el-input
                v-model="formData.UrgentContent"
                type="textarea"
                :rows="4"
                placeholder="请输入催单内容"
                maxlength="500"
                show-word-limit
              />
            </el-form-item>
          </el-form>
        </div>
      </template>
      
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="handleCancel">
            取消
          </el-button>
          <el-button 
            type="primary" 
            @click="handleSend"
          >
            发送
          </el-button>
        </div>
      </template>
    </CompDialog>
    
    <!-- 人员选择器 -->
    <PersonSelector
      v-model="personSelectorVisible"
      :selectedPersonId="selectedPersonId"
      @confirm="handlePersonConfirm"
      @cancel="handlePersonCancel"
    />
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick, getCurrentInstance } from 'vue'
import { User } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import PersonSelector from '@/comp/person-selector/index.vue'
import CompDialog from '@/comp/dialog/index.vue'

// Props
const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  data: {
    type: Object,
    default: () => ({})
  },
  height: {
    type: Number,
    default: 600
  },
  materialCode: {
    type: String,
    default: ''
  }
})

// Emits
const emit = defineEmits(['update:modelValue', 'send'])

// 响应式数据
const visible = ref(false)
const formRef = ref()
const personSelectorVisible = ref(false)
const selectedPersonId = ref('')
const { proxy } = getCurrentInstance()

// 表单数据
const formData = ref({
  contractNo: '',
  planTrackNo: '',
  BusinessType: '',
  DefaultResPerson: '',
  AssignedResPerson: null,
  UrgencyLevel: '',
  AssignedReplyTime: null,
  TimeUnit: '3',
  UrgentType: 0,
  UrgentContent: '',
  MaterialCode: ''
})

// 保存原始数据中的额外字段
const originalData = ref({})

// 业务类型选项
const businessTypeOptions = ref([])
// 业务类型显示标签
const businessTypeLabel = ref('')

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
      
      console.log('业务类型选项加载成功:', businessTypeOptions.value)
    } else {
      console.log('业务类型选项加载失败: 无数据返回')
    }
  } catch (error) {
    console.error('获取业务类型选项失败:', error)
    ElMessage.error('获取业务类型选项失败')
  }
}

// 根据业务类型key获取对应的label
const getBusinessTypeLabel = (businessTypeKey) => {
  console.log('getBusinessTypeLabel 调用:', businessTypeKey, businessTypeOptions.value)
  
  if (!businessTypeKey || !businessTypeOptions.value.length) {
    console.log('getBusinessTypeLabel 返回空:', { businessTypeKey, optionsLength: businessTypeOptions.value.length })
    return ''
  }
  
  const option = businessTypeOptions.value.find(item => item.value === businessTypeKey)
  const result = option ? option.label : businessTypeKey
  
  console.log('getBusinessTypeLabel 结果:', result)
  return result
}

// 默认负责人信息存储
const defaultResponsibleInfo = ref({
  name: '',
  loginName: ''
})

// 根据业务类型获取默认负责人
const loadDefaultResponsible = async (params = {}) => {
  const { businessType, supplierCode = null, materialCode = null } = params
  
  if (!businessType) return
  
  try {
    // 如果BusinessType为PO，调用新接口
    if (businessType === 'PO' || businessType === 'WW') {
      const requestParams = {
        businessType,
        "businessId": params.TrackID   // 跟踪表的主键ID
      }
      
      const result = await proxy.http.post('api/OCP_PurchaseSupplierMapping/GetSupplierResponsible', requestParams, false)
      
        defaultResponsibleInfo.value = {
          name: result?.data?.responsiblePersonName,
          loginName: result?.data?.responsiblePersonLoginName
        }

        formData.value.DefaultResPerson = result?.data?.responsiblePersonName

    } else if (businessType === 'JS') {
      // 如果BusinessType为JS，调用技术管理接口
      const requestParams = {
        MaterialCode: materialCode || ''
      }
      
      const result = await proxy.http.post('api/OCP_TechManagement/GetTechManagerByMaterialCode', requestParams, false)
      
      if (result && result.status && result.data) {
        formData.value.DefaultResPerson = result.data.ownerUserTrueName
        defaultResponsibleInfo.value = {
          name: result.data.ownerUserTrueName,
          loginName: result.data.ownerUserName || ''
        }
      } else {
        formData.value.DefaultResPerson = ''
        defaultResponsibleInfo.value = { name: '', loginName: '' }
      }
    } else {
      // 否则调用原接口逻辑不变
      const requestParams = {
        page: 1,
        rows: 30,
        sort: "CreateDate",
        order: "desc",
        wheres: `[{"name":"BusinessType","value":"${businessType}"}]`,
        resetPage: true
      }
      
      const result = await proxy.http.post('api/OCP_BusinessTypeResponsible/getPageData', requestParams, false)
      
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

// 表单验证规则
const formRules = ref({
  UrgencyLevel: [
    { required: true, message: '请选择紧急等级', trigger: 'change' }
  ],
  UrgentContent: [
    { required: true, message: '请输入催单内容', trigger: 'blur' }
  ]
})

// 下拉框选项
const urgentLevelOptions = ref([
  { label: '特急', value: '特急' },
  { label: 'A', value: 'A' },
  { label: 'B', value: 'B' },
  { label: 'C', value: 'C' }
])

// 时间单位选项
const timeUnitOptions = ref([
  { label: '分钟', value: '1' },
  { label: '小时', value: '2' },
  { label: '天', value: '3' }
])

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

// 监听formData中的BusinessType变化，更新显示标签
watch(
  () => formData.value.BusinessType,
  (newBusinessType) => {
    if (newBusinessType && businessTypeOptions.value.length > 0) {
      businessTypeLabel.value = getBusinessTypeLabel(newBusinessType)
    } else if (!newBusinessType) {
      businessTypeLabel.value = ''
    }
  },
  { immediate: true }
)

// 清除校验状态
const clearValidation = () => {
  nextTick(() => {
    if (formRef.value) {
      formRef.value.clearValidate()
    }
  })
}

// 监听props变化
watch(
  () => props.modelValue,
  async (newVal) => {
    visible.value = newVal
    if (newVal) {
      // 弹窗打开时加载业务类型数据
      await loadBusinessTypeOptions()
      
      // 加载完成后，如果有业务类型数据，则更新显示标签
      // 默认负责人的获取统一在props.data的watch中处理
      if (formData.value.BusinessType) {
        businessTypeLabel.value = getBusinessTypeLabel(formData.value.BusinessType)
      }
    } else {
      // 弹窗关闭时重置默认负责人信息和校验状态
      defaultResponsibleInfo.value = { name: '', loginName: '' }
      clearValidation()
    }
  },
  { immediate: true }
)

// 监听内部visible变化，同步到外部
watch(
  visible,
  (newVal) => {
    if (newVal !== props.modelValue) {
      emit('update:modelValue', newVal)
    }
  }
)

watch(
  () => props.data,
  (newData) => {
    if (newData && Object.keys(newData).length > 0) {
      // 保存原始数据
      originalData.value = { ...newData }
      
      // 填充表单数据
      formData.value = {
        BusinessType: newData.BusinessType || '',
        DefaultResPerson: '', // 先清空，后面通过API获取
        AssignedResPerson: newData.AssignedResPerson || null,
        UrgencyLevel: newData.UrgencyLevel || '',
        AssignedReplyTime: newData.AssignedReplyTime || newData.replyDays || null,
        TimeUnit: newData.TimeUnit || '3',
        UrgentType: newData.UrgentType !== undefined ? newData.UrgentType : 0,
        UrgentContent: newData.UrgentContent || '',
        MaterialCode: newData.MaterialCode || props.materialCode || ''
      }
      
      // 延迟设置业务类型显示标签，确保业务类型选项数据已加载
      nextTick(() => {
        const businessTypeValue = newData.BusinessType
        
        if (businessTypeValue && businessTypeOptions.value.length > 0) {
          businessTypeLabel.value = getBusinessTypeLabel(businessTypeValue)
          console.log('业务类型标签设置为:', businessTypeLabel.value)
        } else {
          console.log('业务类型标签设置条件不满足')
        }

        // 根据业务类型获取默认负责人
        if (businessTypeValue) {
          const params = {
            businessType: businessTypeValue,
            supplierCode: newData.SupplierCode || '',
            materialCode: newData.MaterialCode || props.materialCode || '',
            TrackID: newData.currentRowData?.TrackID || ''
          }
          loadDefaultResponsible(params)
        }
      })
    }
  },
  { immediate: true, deep: true }
)

// 方法
const handleCancel = () => {
  clearValidation()
  handleClose()
}

const handleClose = () => {
  clearValidation()
  visible.value = false
  emit('update:modelValue', false)
}

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

const handleSend = async () => {
  // 验证表单
  try {
    await formRef.value.validate()
  } catch (error) {
    return
  }
  
  // 如果没有选择指定负责人，则使用默认负责人的登录名
  const sendData = { ...formData.value }
  if (!sendData.AssignedResPerson) {
    sendData.AssignedResPerson = {
      id: defaultResponsibleInfo.value.loginName,
      name: defaultResponsibleInfo.value.name,
      userName: defaultResponsibleInfo.value.loginName,
      userTrueName: defaultResponsibleInfo.value.name
    }
  }
  
  // 合并原始数据中的额外字段（如currentRowData、mainOrderData等）
  const finalSendData = {
    ...originalData.value,
    ...sendData,
    // 添加默认负责人的loginName
    DefaultResPersonLoginName: defaultResponsibleInfo.value.loginName || ''
  }
  
  console.log('催单发送数据:', finalSendData)
  console.log('发送数据中的指定负责人:', finalSendData.AssignedResPerson)
  console.log('默认负责人信息:', defaultResponsibleInfo.value)
  console.log('原始数据:', originalData.value)
  
  // 先发送数据给外部
  emit('send', finalSendData)
  
  // 关闭弹窗
  handleClose()
  
  // 延迟重置状态，确保外部已接收到数据
  nextTick(() => {
    resetFormData()
  })
}

// 重置表单数据
const resetFormData = () => {
  formData.value = {
    contractNo: '',
    planTrackNo: '',
    BusinessType: '',
    DefaultResPerson: '',
    AssignedResPerson: null,
    UrgencyLevel: '',
    AssignedReplyTime: null,
    TimeUnit: '3',
    UrgentType: 0,
    UrgentContent: '',
    MaterialCode: ''
  }
  originalData.value = {}
  businessTypeLabel.value = ''
  defaultResponsibleInfo.value = { name: '', loginName: '' }
  
  // 清除表单验证状态
  nextTick(() => {
    if (formRef.value) {
      formRef.value.clearValidate()
    }
  })
}

const handleDialogClose = () => {
  resetFormData()
  handleClose()
  return true
}
</script>

<style scoped>
.dialog-content {
  padding: 24px;
  max-height: 600px;
  overflow-y: auto;
}

.assigned-persons-container {
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
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

:deep(.el-form-item__label) {
  font-weight: 500;
  color: #606266;
}

:deep(.el-select) {
  width: 100%;
}

:deep(.el-input-group__append .el-select) {
  width: auto;
}

:deep(.el-textarea__inner) {
  resize: vertical;
}
</style>