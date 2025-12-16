<template>
  <div class="batch-negotiation-dialog">
    <CompDialog v-model="visible" :title="title" :width="width" :footer="true" :padding="0">
      <template #content>
        <div class="dialog-content">
          <el-alert
            v-if="tableData.length === 0"
            type="warning"
            title="暂无可编辑的协商数据"
            show-icon
            class="mb-2"
          />

          <el-form ref="formRef" :model="{ tableData }" label-width="0">
            <el-table :data="tableData" stripe border height="100%" empty-text="暂无数据">
              <el-table-column type="index" label="#" width="50" align="center" />
              <el-table-column prop="billNo" label="采购订单编号" width="160" align="center" />
              <el-table-column prop="seq" label="行号" width="80" align="center" />
              <el-table-column prop="planTrackNo" label="计划跟踪号" width="160" align="center" />
              <el-table-column prop="businessKey" label="业务键" width="200" align="center" />
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
                      filterable
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
                </template>
              </el-table-column>
              <el-table-column label="协商日期" width="180" align="center">
                <template #default="{ row, $index }">
                  <el-form-item
                    :prop="`tableData.${$index}.negotiationDate`"
                    :rules="[{ required: true, message: '请选择协商日期', trigger: 'change' }]"
                    style="margin: 0"
                  >
                    <el-date-picker
                      v-model="row.negotiationDate"
                      type="date"
                      placeholder="请选择"
                      size="small"
                      style="width: 100%"
                      format="YYYY-MM-DD"
                      value-format="YYYY-MM-DD"
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
                      @close="removeAssignedPerson($index)"
                      class="person-tag"
                    >
                      {{ row.assignedResPersonName }}
                    </el-tag>
                    <el-button
                      size="small"
                      type="primary"
                      link
                      @click="openPersonSelector($index)"
                      class="select-person-btn"
                    >
                      {{ row.assignedResPersonName ? '重新选择' : '选择人员' }}
                    </el-button>
                  </div>
                </template>
              </el-table-column>
              <el-table-column label="协商内容" min-width="240" align="left">
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
                      clearable
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
      :selectedPersonId="currentSelectedPersonId"
      @confirm="handlePersonConfirm"
      @cancel="handlePersonCancel"
    />
  </div>
</template>

<script setup>
import { ref, watch, computed, getCurrentInstance, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import CompDialog from '@/comp/dialog/index.vue'
import PersonSelector from '@/comp/person-selector/index.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  data: { type: Array, default: () => [] },
  title: { type: String, default: '批量协商' },
  width: { type: [String, Number], default: '90%' }
})

const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

const visible = ref(false)
const formRef = ref(null)
const tableData = ref([])
const reasonOptions = ref([])

const personSelectorVisible = ref(false)
const currentRowIndex = ref(-1)
const currentSelectedPersonId = computed(() => {
  const row = tableData.value[currentRowIndex.value]
  const person = row?.assignedResPerson
  if (!person) return ''
  return person.id || person.userId || person.userName || ''
})

const { proxy } = getCurrentInstance()

const normalizeRow = (row = {}) => ({
  ...row,
  billNo: row.billNo || row.POBillNo || '',
  seq: row.seq ?? row.Seq ?? '',
  planTrackNo: row.planTrackNo || row.PlanTraceNo || row.PlanTrackNo || row.TrackId || row.PlanTrackId || '',
  trackId: row.trackId || row.TrackId || '',
  businessKey: row.businessKey || row.FENTRYID || '',
  negotiationReason: row.negotiationReason || row.NegotiationReason || '',
  negotiationDate: row.negotiationDate || row.NegotiationDate || '',
  assignedResPerson: row.assignedResPerson || row.AssignedResPerson || null,
  assignedResPersonName: row.assignedResPersonName || row.AssignedResPersonName || '',
  negotiationContent: row.negotiationContent || row.NegotiationContent || ''
})

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
    if (result?.rows) {
      reasonOptions.value = result.rows
        .filter((item) => item.Enable === 1)
        .map((item) => ({ value: item.DicValue, label: item.DicName }))
    }
  } catch (error) {
    console.error('获取协商原因选项失败:', error)
    ElMessage.error('获取协商原因选项失败')
  }
}

const handleCancel = () => {
  visible.value = false
  emit('cancel')
}

const validateRows = async () => {
  if (!tableData.value.length) {
    ElMessage.warning('暂无可提交的协商数据')
    return false
  }

  const form = formRef.value
  if (form) {
    try {
      await form.validate()
      return true
    } catch (error) {
      return false
    }
  }

  for (let i = 0; i < tableData.value.length; i++) {
    const row = tableData.value[i]
    if (!row.negotiationReason) {
      ElMessage.warning(`第 ${i + 1} 行请选择协商原因`)
      return false
    }
    if (!row.negotiationDate) {
      ElMessage.warning(`第 ${i + 1} 行请选择协商日期`)
      return false
    }
    if (!row.negotiationContent) {
      ElMessage.warning(`第 ${i + 1} 行请输入协商内容`)
      return false
    }
  }

  return true
}

const handleConfirm = async () => {
  const valid = await validateRows()
  if (!valid) return

  const payload = tableData.value.map((row) => ({
    billNo: row.billNo || '',
    seq: row.seq ?? '',
    planTrackNo: row.planTrackNo || row.trackId || '',
    trackId: row.trackId || row.planTrackNo || '',
    businessKey: row.businessKey || '',
    negotiationReason: row.negotiationReason,
    negotiationDate: row.negotiationDate,
    negotiationContent: row.negotiationContent,
    assignedResPerson: row.assignedResPerson?.userName || row.assignedResPerson?.id || row.assignedResPerson || '',
    assignedResPersonName:
      row.assignedResPerson?.userTrueName || row.assignedResPersonName || row.assignedResPerson?.name || ''
  }))

  emit('confirm', payload)
}

const openPersonSelector = (index) => {
  currentRowIndex.value = index
  personSelectorVisible.value = true
}

const handlePersonConfirm = (person) => {
  if (currentRowIndex.value === -1) return
  const targetRow = tableData.value[currentRowIndex.value]
  targetRow.assignedResPerson = person
  targetRow.assignedResPersonName = person?.userTrueName || person?.name || ''
  personSelectorVisible.value = false
  currentRowIndex.value = -1
}

const handlePersonCancel = () => {
  personSelectorVisible.value = false
  currentRowIndex.value = -1
}

const removeAssignedPerson = (index) => {
  const row = tableData.value[index]
  if (!row) return
  row.assignedResPerson = null
  row.assignedResPersonName = ''
}

watch(
  () => props.modelValue,
  (val) => {
    visible.value = val
  }
)

watch(
  () => props.data,
  (val) => {
    tableData.value = (val || []).map((row) => normalizeRow(row))
  },
  { immediate: true, deep: true }
)

watch(visible, (val) => emit('update:modelValue', val))

onMounted(() => {
  loadReasonOptions()
})
</script>

<style scoped>
.dialog-content {
  padding: 16px;
  max-height: 70vh;
  overflow: auto;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 12px 0;
}

.assigned-person-container {
  display: flex;
  align-items: center;
  gap: 6px;
  justify-content: center;
}

.person-tag {
  margin: 0;
}

.select-person-btn {
  padding: 0;
}

.mb-2 {
  margin-bottom: 12px;
}
</style>
