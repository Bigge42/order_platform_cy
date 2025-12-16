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

    <PersonSelector
      v-model="personSelectorVisible"
      :selected-person-id="selectedPersonId"
      @confirm="handlePersonConfirm"
      @cancel="handlePersonCancel"
    />
  </div>
</template>

<script setup>
import { ref, watch, computed, getCurrentInstance } from 'vue'
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

const {
  personSelectorVisible,
  selectedPersonId,
  resetPersonSelectorState,
  openPersonSelector,
  handlePersonConfirm,
  handlePersonCancel,
  applyPersonToTable
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
    negotiationReason: item.negotiationReason || '',
    negotiationDate: item.negotiationDate || '',
    negotiationContent: item.negotiationContent || '',
    assignedResPersonName: item.assignedResPersonName || '',
    assignedResPerson: item.assignedResPerson || null
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
}

const clearFieldValidation = (index, field) => {
  const prop = `tableData.${index}.${field}`
  formRef.value?.clearValidate?.(prop)
}

const onRemoveAssignedPerson = (rowIndex) => {
  const row = tableData.value[rowIndex]
  if (row) {
    row.assignedResPerson = null
    row.assignedResPersonName = ''
  }
}

watch(applyPersonToTable, (val) => {
  if (val?.targetRows) {
    const { targetRows, person } = val
    targetRows.forEach((row) => {
      row.assignedResPerson = person
      row.assignedResPersonName = person?.userTrueName || ''
    })
  }
})

const handleCancel = () => {
  emit('cancel')
  emit('update:modelValue', false)
  resetPersonSelectorState()
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
</style>
