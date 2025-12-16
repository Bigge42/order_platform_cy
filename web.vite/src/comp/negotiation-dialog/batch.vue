<template>
  <div class="batch-negotiation-dialog">
    <CompDialog
      v-model="visible"
      :title="props.title"
      :width="props.width"
      :footer="true"
      :padding="0"
    >
      <template #content>
        <div class="dialog-content">
          <div class="batch-operations" v-if="tableData.length > 0">
            <span class="record-count">已选中 {{ tableData.length }} 条记录</span>
            <div class="batch-buttons">
              <el-button
                size="small"
                type="primary"
                @click="onBatchEdit"
                :disabled="batchState.selectedRows.length === 0"
              >
                批量编辑 ({{ batchState.selectedRows.length }})
              </el-button>
            </div>
          </div>

          <el-form ref="formRef" :model="{ tableData }" label-width="0">
            <el-table
              ref="tableRef"
              :data="tableData"
              stripe
              border
              height="100%"
              empty-text="暂无数据"
              @selection-change="handleSelectionChange"
            >
              <el-table-column type="selection" width="55" align="center" />
              <el-table-column prop="billNo" label="采购订单编号" width="140" align="center" />
              <el-table-column prop="seq" label="行号" width="80" align="center" />
              <el-table-column prop="planTraceNo" label="计划跟踪号" width="140" align="center" />
              <el-table-column prop="defaultResPersonName" label="默认负责人" width="120" align="center" />
              <el-table-column label="协商原因" width="180" align="center">
                <template #default="{ row, $index }">
                  <el-form-item
                    :prop="`tableData.${$index}.negotiationReason`"
                    :rules="[{ required: true, message: '请选择协商原因', trigger: 'change' }]"
                    style="margin: 0"
                  >
                    <el-select
                      v-model="row.negotiationReason"
                      placeholder="请选择协商原因"
                      size="small"
                      style="width: 100%"
                      clearable
                      @change="clearFieldValidation($index, 'negotiationReason')"
                    >
                      <el-option
                        v-for="option in reasonOptions"
                        :key="option.value"
                        :label="option.label"
                        :value="option.value"
                      />
                    </el-select>
                  </el-form-item>
                </template>
              </el-table-column>
              <el-table-column label="协商日期" width="160" align="center">
                <template #default="{ row, $index }">
                  <el-form-item
                    :prop="`tableData.${$index}.negotiationDate`"
                    :rules="[{ required: true, message: '请选择协商日期', trigger: 'change' }]"
                    style="margin: 0"
                  >
                    <el-date-picker
                      v-model="row.negotiationDate"
                      type="date"
                      placeholder="请选择日期"
                      size="small"
                      style="width: 100%"
                      format="YYYY-MM-DD"
                      value-format="YYYY-MM-DD"
                      @change="clearFieldValidation($index, 'negotiationDate')"
                    />
                  </el-form-item>
                </template>
              </el-table-column>
              <el-table-column label="指定负责人" width="200" align="center">
                <template #default="{ row, $index }">
                  <div class="assigned-person-container">
                    <el-tag
                      v-if="row.assignedResPersonName"
                      type="info"
                      size="small"
                      closable
                      @close="onRemoveAssignedPerson($index)"
                      class="person-tag"
                    >
                      {{ row.assignedResPersonName }}
                    </el-tag>
                    <el-button
                      size="small"
                      type="primary"
                      link
                      @click="openPersonSelector($index, tableData)"
                      class="select-person-btn"
                    >
                      {{ row.assignedResPersonName ? '重新选择' : '选择人员' }}
                    </el-button>
                  </div>
                </template>
              </el-table-column>
              <el-table-column label="协商内容" min-width="220" align="left">
                <template #default="{ row, $index }">
                  <el-form-item
                    :prop="`tableData.${$index}.negotiationContent`"
                    :rules="[{ required: true, message: '请输入协商内容', trigger: 'blur' }]"
                    style="margin: 0"
                  >
                    <el-input
                      v-model="row.negotiationContent"
                      type="textarea"
                      :rows="2"
                      placeholder="请输入协商内容"
                      size="small"
                      @input="clearFieldValidation($index, 'negotiationContent')"
                    />
                  </el-form-item>
                </template>
              </el-table-column>
            </el-table>
          </el-form>
        </div>
      </template>

      <template #footer>
        <div class="dialog-footer">
          <el-button @click="handleCancel">取消</el-button>
          <el-button type="primary" @click="handleConfirm">确定</el-button>
        </div>
      </template>
    </CompDialog>

    <el-drawer
      v-model="batchState.drawerVisible"
      title="批量编辑"
      direction="rtl"
      size="360px"
      :append-to-body="false"
      :before-close="onDrawerCancel"
    >
      <template #header>
        <h4>批量编辑 ({{ batchState.selectedRowsSnapshot.length }} 条记录)</h4>
      </template>

      <template #default>
        <div class="drawer-content">
          <el-form :model="batchState.formData" label-width="90px" label-position="top">
            <el-form-item label="协商原因">
              <el-select v-model="batchState.formData.negotiationReason" placeholder="请选择">
                <el-option
                  v-for="option in reasonOptions"
                  :key="option.value"
                  :label="option.label"
                  :value="option.value"
                />
              </el-select>
            </el-form-item>

            <el-form-item label="协商日期">
              <el-date-picker
                v-model="batchState.formData.negotiationDate"
                type="date"
                placeholder="请选择日期"
                format="YYYY-MM-DD"
                value-format="YYYY-MM-DD"
              />
            </el-form-item>

            <el-form-item label="指定负责人">
              <div class="assigned-person-container">
                <el-tag
                  v-if="batchState.formData.assignedResPersonName"
                  type="info"
                  size="small"
                  closable
                  @close="removeDrawerAssignedPerson(batchState.formData)"
                  class="person-tag"
                >
                  {{ batchState.formData.assignedResPersonName }}
                </el-tag>
                <el-button
                  size="small"
                  type="primary"
                  link
                  @click="openDrawerPersonSelector(batchState.formData)"
                  class="select-person-btn"
                >
                  {{ batchState.formData.assignedResPersonName ? '重新选择' : '选择人员' }}
                </el-button>
              </div>
            </el-form-item>

            <el-form-item label="协商内容">
              <el-input
                v-model="batchState.formData.negotiationContent"
                type="textarea"
                :rows="3"
                placeholder="请输入协商内容"
              />
            </el-form-item>
          </el-form>
        </div>
      </template>

      <template #footer>
        <div class="drawer-footer">
          <el-button @click="onDrawerCancel">取消</el-button>
          <el-button type="primary" @click="onDrawerConfirm">应用</el-button>
        </div>
      </template>
    </el-drawer>

    <PersonSelector
      v-model="personSelectorState.visible"
      :selected-person-id="personSelectorState.selectedPersonId"
      @confirm="onPersonConfirm"
      @cancel="onPersonCancel"
    />
  </div>
</template>

<script setup>
import { ref, watch, computed, getCurrentInstance, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import CompDialog from '@/comp/dialog/index.vue'
import PersonSelector from '@/comp/person-selector/index.vue'
import { usePersonSelector } from '@/comp/reminder-dialog/composables/usePersonSelector.js'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  data: {
    type: Array,
    default: () => []
  },
  title: {
    type: String,
    default: '批量协商'
  },
  width: {
    type: [String, Number],
    default: '90%'
  }
})

const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

const { proxy } = getCurrentInstance()
const visible = computed({
  get() {
    return props.modelValue
  },
  set(value) {
    emit('update:modelValue', value)
  }
})

const formRef = ref(null)
const tableRef = ref(null)
const tableData = ref([])
const reasonOptions = ref([])
const selectedRows = ref([])

const getDefaultBatchFormData = () => ({
  negotiationReason: '',
  negotiationDate: '',
  assignedResPersonName: '',
  assignedResPerson: '',
  negotiationContent: ''
})

const batchState = reactive({
  drawerVisible: false,
  selectedRows: [],
  selectedRowsSnapshot: [],
  formData: getDefaultBatchFormData()
})

const {
  personSelectorState,
  resetPersonSelectorState,
  openPersonSelector,
  openDrawerPersonSelector,
  handlePersonConfirm,
  handlePersonCancel,
  removeAssignedPerson,
  removeDrawerAssignedPerson
} = usePersonSelector()

watch(
  () => props.data,
  (newVal) => {
    formatTableData(newVal)
  },
  { immediate: true }
)

watch(
  () => props.modelValue,
  (value) => {
    if (value) {
      loadReasonOptions()
    }
  }
)

const formatTableData = (dataSource = []) => {
  tableData.value = dataSource.map((item) => ({
    ...item,
    billNo: item.billNo || item.POBillNo || '',
    seq: item.seq ?? item.Seq ?? '',
    planTraceNo: item.planTraceNo || item.PlanTraceNo || '',
    businessType: item.businessType || 'PO',
    businessKey: item.businessKey || item.FENTRYID,
    defaultResPersonName: item.defaultResPersonName || item.DefaultResPersonName || '',
    defaultResPerson: item.defaultResPerson || item.DefaultResPerson || '',
    negotiationReason: item.negotiationReason || '',
    negotiationDate: item.negotiationDate || '',
    negotiationContent: item.negotiationContent || '',
    assignedResPersonName: item.assignedResPersonName || '',
    assignedResPerson: item.assignedResPerson || ''
  }))
}

const loadReasonOptions = async () => {
  try {
    const params = {
      page: 1,
      rows: 30,
      sort: 'OrderNo,CreateDate',
      order: 'desc',
      wheres: '[]',
      value: 110,
      tableName: null,
      isCopyClick: false
    }

    const result = await proxy.http.post('api/Sys_Dictionary/getDetailPage', params, false)

    if (result && result.rows) {
      reasonOptions.value = result.rows
        .filter((item) => item.Enable === 1)
        .map((item) => ({
          value: item.DicValue,
          label: item.DicName
        }))
    }
  } catch (error) {
    console.error('获取协商原因选项失败:', error)
    ElMessage.error('获取协商原因选项失败')
  }
}

const handleSelectionChange = (val) => {
  selectedRows.value = val
  batchState.selectedRows = val
}

const clearFieldValidation = (index, field) => {
  const prop = `tableData.${index}.${field}`
  formRef.value?.clearValidate?.(prop)
}

const clearFormValidation = () => {
  formRef.value?.clearValidate?.()
}

const onRemoveAssignedPerson = (rowIndex) => {
  removeAssignedPerson(rowIndex, tableData.value)
}

const onBatchEdit = () => {
  if (batchState.selectedRows.length === 0) {
    ElMessage.warning('请先选择需要批量编辑的行')
    return
  }
  batchState.selectedRowsSnapshot = [...batchState.selectedRows]
  batchState.drawerVisible = true
}

const applyBatchEdit = () => {
  const { negotiationReason, negotiationDate, assignedResPersonName, assignedResPerson, negotiationContent } =
    batchState.formData

  batchState.selectedRowsSnapshot.forEach((row) => {
    if (negotiationReason) row.negotiationReason = negotiationReason
    if (negotiationDate) row.negotiationDate = negotiationDate
    if (negotiationContent) row.negotiationContent = negotiationContent

    if (assignedResPersonName || assignedResPerson) {
      row.assignedResPersonName = assignedResPersonName
      row.assignedResPerson = assignedResPerson
    }
  })

  clearFormValidation()
}

const resetBatchState = () => {
  batchState.drawerVisible = false
  batchState.selectedRowsSnapshot = []
  Object.assign(batchState.formData, getDefaultBatchFormData())
}

const onDrawerConfirm = () => {
  applyBatchEdit()
  resetBatchState()
}

const onDrawerCancel = () => {
  resetBatchState()
}

const handleCancel = () => {
  emit('cancel')
  emit('update:modelValue', false)
  resetPersonSelectorState()
  resetBatchState()
}

const validateTable = async () => {
  if (!formRef.value) return false
  try {
    await formRef.value.validate()
    return true
  } catch (error) {
    return false
  }
}

const handleConfirm = async () => {
  if (tableData.value.length === 0) {
    ElMessage.warning('请选择要协商的数据')
    return
  }

  const valid = await validateTable()
  if (!valid) {
    ElMessage.error('请完善必填字段')
    return
  }

  emit('confirm', {
    data: tableData.value,
    selected: selectedRows.value,
    totalCount: tableData.value.length,
    originalData: props.data
  })
  emit('update:modelValue', false)
  resetPersonSelectorState()
  resetBatchState()
}

const onPersonConfirm = (person) => {
  handlePersonConfirm(person, tableData.value, batchState.formData)
}

const onPersonCancel = () => {
  handlePersonCancel()
}
</script>

<style scoped>
.batch-negotiation-dialog {
  width: 100%;
}

.dialog-content {
  padding: 12px;
}

.batch-operations {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.record-count {
  font-size: 14px;
  color: #606266;
}

.batch-buttons {
  display: flex;
  align-items: center;
  gap: 8px;
}

.assigned-person-container {
  display: flex;
  align-items: center;
  gap: 6px;
  justify-content: center;
}

.person-tag {
  margin-right: 4px;
}

.select-person-btn {
  padding: 0;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.drawer-content {
  padding: 0 12px;
}

.drawer-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 10px 0;
}
</style>
