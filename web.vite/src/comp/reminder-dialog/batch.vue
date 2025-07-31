<template>
  <div class="batch-reminder-dialog">
    <CompDialog
      v-model="visible"
      :title="props.title"
      :width="props.width"
      :footer="true"
      :padding="0"
    >
      <template #content>
        <div class="dialog-content">
          <!-- 批量操作区域 -->
          <div class="batch-operations" v-if="tableData.length > 0">
            <el-button
              size="small"
              type="primary"
              @click="onBatchEdit"
              :disabled="batchState.selectedRows.length === 0"
            >
              批量编辑 ({{ batchState.selectedRows.length }})
            </el-button>
          </div>

          <el-form ref="batchFormRef" :model="{ tableData }" label-width="0">
            <el-table
              ref="tableRef"
              :data="tableData"
              v-loading="isLoading"
              :element-loading-text="loadingText"
              stripe
              border
              height="100%"
              empty-text="暂无数据"
              @selection-change="handleSelectionChange"
            >
              <el-table-column type="selection" width="55" align="center" />
              <el-table-column label="业务类型" width="120" align="center">
                <template #default="{ row }">
                  {{ getBusinessTypeLabel(row.businessType) }}
                </template>
              </el-table-column>
              <el-table-column
                prop="defaultResPersonName"
                label="默认负责人"
                width="120"
                align="center"
              />
              <el-table-column label="紧急等级" width="100" align="center">
                <template #default="{ row, $index }">
                  <el-form-item
                    :prop="`tableData.${$index}.urgencyLevel`"
                    :rules="[{ required: true, message: '请选择紧急等级', trigger: 'change' }]"
                    style="margin: 0"
                  >
                    <el-select
                      v-model="row.urgencyLevel"
                      placeholder="请选择"
                      size="small"
                      style="width: 100%"
                      @change="clearFieldValidation($index, 'urgencyLevel')"
                    >
                      <el-option
                        v-for="option in URGENCY_LEVEL_OPTIONS"
                        :key="option.value"
                        :label="option.label"
                        :value="option.value"
                      />
                    </el-select>
                  </el-form-item>
                </template>
              </el-table-column>
              <el-table-column label="指定回复时间" width="180" align="center">
                <template #default="{ row }">
                  <el-input
                    class="assigned-reply-time-input"
                    v-model="row.assignedReplyTime"
                    type="number"
                    :min="1"
                    :max="999"
                    placeholder="请输入时间"
                    size="small"
                    clearable
                  >
                    <template #append>
                      <el-select
                        v-model="row.timeUnit"
                        placeholder="单位"
                        style="width: 60px"
                        size="small"
                      >
                        <el-option
                          v-for="option in TIME_UNIT_OPTIONS"
                          :key="option.value"
                          :label="option.label"
                          :value="option.value"
                        />
                      </el-select>
                    </template>
                  </el-input>
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
              <el-table-column label="催单类型" width="120" align="center">
                <template #default="{ row }">
                  <el-select
                    v-model="row.urgentType"
                    placeholder="请选择"
                    size="small"
                    style="width: 100%"
                  >
                    <el-option
                      v-for="option in URGENT_TYPE_OPTIONS"
                      :key="option.value"
                      :label="option.label"
                      :value="option.value"
                    />
                  </el-select>
                </template>
              </el-table-column>
              <el-table-column label="催单内容" min-width="200" align="left">
                <template #default="{ row, $index }">
                  <el-form-item
                    :prop="`tableData.${$index}.urgentContent`"
                    :rules="[{ required: true, message: '请输入催单内容', trigger: 'blur' }]"
                    style="margin: 0"
                  >
                    <el-input
                      v-model="row.urgentContent"
                      type="textarea"
                      :rows="2"
                      placeholder="请输入催单内容"
                      size="small"
                      @input="clearFieldValidation($index, 'urgentContent')"
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
          <el-button @click="handleCancel" :disabled="apiState.submitting">取消</el-button>
          <el-button
            type="primary"
            @click="handleConfirm"
            :loading="apiState.submitting"
            :disabled="apiState.submitting"
          >
            {{ apiState.submitting ? '提交中...' : '确定' }}
          </el-button>
        </div>
      </template>

      <!-- 批量编辑抽屉 - 放在弹窗内部 -->
      <el-drawer
        v-model="batchState.drawerVisible"
        title="批量编辑"
        direction="rtl"
        size="400px"
        :append-to-body="false"
        :before-close="onDrawerCancel"
      >
        <template #header>
          <h4>批量编辑 ({{ batchState.selectedRowsSnapshot.length }} 条记录)</h4>
        </template>

        <template #default>
          <div class="drawer-content">
            <el-form :model="batchState.formData" label-width="100px" label-position="top">
              <!-- 紧急等级 -->
              <el-form-item label="紧急等级">
                <el-select
                  v-model="batchState.formData.urgencyLevel"
                  placeholder="请选择紧急等级"
                  clearable
                  style="width: 100%"
                >
                  <el-option
                    v-for="option in URGENCY_LEVEL_OPTIONS"
                    :key="option.value"
                    :label="option.label"
                    :value="option.value"
                  />
                </el-select>
              </el-form-item>

              <!-- 指定负责人 -->
              <el-form-item label="指定负责人">
                <div class="drawer-person-selector">
                  <el-tag
                    v-if="batchState.formData.assignedResPersonName"
                    type="info"
                    size="default"
                    closable
                    @close="onClearDrawerPerson"
                    class="drawer-person-tag"
                  >
                    {{ batchState.formData.assignedResPersonName }}
                  </el-tag>
                  <el-button
                    type="primary"
                    link
                    @click="onDrawerPersonSelector"
                    class="drawer-select-person-btn"
                  >
                    {{ batchState.formData.assignedResPersonName ? '重新选择' : '选择人员' }}
                  </el-button>
                </div>
              </el-form-item>

              <!-- 指定回复时间 -->
              <el-form-item label="指定回复时间">
                <el-input
                  v-model="batchState.formData.assignedReplyTime"
                  type="number"
                  :min="1"
                  :max="999"
                  placeholder="请输入时间"
                  clearable
                  style="width: 100%"
                >
                  <template #append>
                    <el-select
                      v-model="batchState.formData.timeUnit"
                      placeholder="单位"
                      style="width: 80px"
                    >
                      <el-option
                        v-for="option in TIME_UNIT_OPTIONS"
                        :key="option.value"
                        :label="option.label"
                        :value="option.value"
                      />
                    </el-select>
                  </template>
                </el-input>
              </el-form-item>

              <!-- 催单类型 -->
              <el-form-item label="催单类型">
                <el-select
                  v-model="batchState.formData.urgentType"
                  placeholder="请选择催单类型"
                  clearable
                  style="width: 100%"
                >
                  <el-option
                    v-for="option in URGENT_TYPE_OPTIONS"
                    :key="option.value"
                    :label="option.label"
                    :value="option.value"
                  />
                </el-select>
              </el-form-item>

              <!-- 催单内容 -->
              <el-form-item label="催单内容">
                <el-input
                  v-model="batchState.formData.urgentContent"
                  type="textarea"
                  :rows="4"
                  placeholder="请输入催单内容"
                  style="width: 100%"
                />
              </el-form-item>
            </el-form>
          </div>
        </template>

        <template #footer>
          <div class="drawer-footer">
            <el-button @click="onDrawerCancel">取消</el-button>
            <el-button type="primary" @click="onDrawerConfirm">确定</el-button>
          </div>
        </template>
      </el-drawer>
    </CompDialog>

    <!-- 人员选择器 -->
    <PersonSelector
      v-model="personSelectorState.visible"
      :selectedPersonId="personSelectorState.selectedPersonId"
      @confirm="onPersonConfirm"
      @cancel="onPersonCancel"
    />
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import CompDialog from '@/comp/dialog/index.vue'
import PersonSelector from '@/comp/person-selector/index.vue'
import { useBatchReminderAPI } from './composables/useBatchReminderAPI.js'
import { useFormValidation } from './composables/useFormValidation.js'
import { usePersonSelector } from './composables/usePersonSelector.js'
import { useBatchOperations } from './composables/useBatchOperations.js'
import {
  URGENCY_LEVEL_OPTIONS,
  TIME_UNIT_OPTIONS,
  URGENT_TYPE_OPTIONS,
  DEFAULT_ROW_DATA
} from './constants/options.js'

// Props定义
const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  data: {
    type: Array,
    default: () => []
  },
  businessType: {
    type: [String, Number],
    default: null
  },
  title: {
    type: String,
    default: '批量催单'
  },
  width: {
    type: [String, Number],
    default: '80%'
  },
  enableApiCall: {
    type: Boolean,
    default: true
  }
})

// Events定义
const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

// 使用组合函数
const {
  apiState,
  businessTypeOptions,
  businessTypeLoading,
  defaultResponsibleState,
  resetAPIState,
  fetchBusinessTypeOptions,
  fetchDefaultResponsible,
  executeBatchReminder
} = useBatchReminderAPI()

const { batchFormRef, validateBatchForm, clearFormValidation, clearFieldValidation } =
  useFormValidation()

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

const {
  batchState,
  tableRef,
  resetBatchState,
  handleSelectionChange,
  handleBatchEdit,
  confirmBatchEdit
} = useBatchOperations()

// 基础状态
const visible = ref(false)
const tableData = ref([])

// 计算属性
const totalCount = computed(() => props.data.length)
const isLoading = computed(() => defaultResponsibleState.loading || businessTypeLoading.value)
const loadingText = computed(() => {
  const texts = []
  if (businessTypeLoading.value) texts.push('加载业务类型')
  if (defaultResponsibleState.loading) texts.push('获取默认负责人')
  return texts.length > 0 ? `正在${texts.join('和')}...` : '加载中...'
})

// 根据业务类型值获取显示标签
const getBusinessTypeLabel = (businessTypeValue) => {
  if (!businessTypeValue || !businessTypeOptions.value.length) {
    return businessTypeValue || '-'
  }
  const option = businessTypeOptions.value.find((item) => item.value === businessTypeValue)
  return option ? option.label : businessTypeValue
}

// 初始化表格数据
const initializeTableData = () => {
  tableData.value = props.data.map((row) => ({
    ...DEFAULT_ROW_DATA,
    businessType: props.businessType || '',
    _originalData: row
  }))
}

// 完整初始化数据
const initializeData = async () => {
  initializeTableData()
  await fetchBusinessTypeOptions(props.businessType)

  const responsibleMap = await fetchDefaultResponsible(props.data, props.businessType)
  if (responsibleMap) {
    tableData.value.forEach((row) => {
      const businessKey = String(row._originalData.businessKey || '')
      const info = responsibleMap.get(businessKey)

      if (info && info.found) {
        row.defaultResPersonName = info.defaultResponsibleName || ''
        row.defaultResPerson = info.defaultResponsibleLoginName || ''
      }
    })
  }

  clearFormValidation()
}

// 重置所有状态
const resetAllState = () => {
  tableData.value = []
  resetAPIState('all')
  resetPersonSelectorState()
  resetBatchState()
  clearFormValidation()
}

// 按钮事件处理
const handleConfirm = async () => {
  try {
    const processedData = tableData.value.map((row) => ({
      ...row,
      defaultResPersonName: row.defaultResPersonName || '',
      defaultResPerson: row.defaultResPerson || '',
      assignedResPersonName: row.assignedResPersonName || '',
      assignedResPerson: row.assignedResPerson || ''
    }))

    const confirmData = {
      data: processedData,
      originalData: props.data,
      totalCount: totalCount.value
    }

    if (props.enableApiCall) {
      const isValid = await validateBatchForm()
      if (!isValid) return

      const result = await executeBatchReminder(tableData.value, props.data, props.businessType)
      confirmData.apiResult = result
    }

    emit('confirm', confirmData)
    visible.value = false
  } catch (error) {
    console.error('批量催单处理异常:', error)
  }
}

const handleCancel = () => {
  emit('cancel')
  visible.value = false
}

// 人员选择器事件处理
const onPersonConfirm = (person) => {
  handlePersonConfirm(person, tableData.value, batchState.formData)
}

const onPersonCancel = () => {
  handlePersonCancel()
}

const onRemoveAssignedPerson = (rowIndex) => {
  removeAssignedPerson(rowIndex, tableData.value)
}

// 批量操作事件处理
const onBatchEdit = () => {
  handleBatchEdit()
}

const onDrawerConfirm = () => {
  const result = confirmBatchEdit(tableData.value)
  if (result.success) {
    clearFormValidation()
  }
}

const onDrawerCancel = () => {
  resetBatchState()
}

const onDrawerPersonSelector = () => {
  openDrawerPersonSelector(batchState.formData)
}

const onClearDrawerPerson = () => {
  removeDrawerAssignedPerson(batchState.formData)
}
watch(
  () => props.modelValue,
  (newVal) => {
    visible.value = newVal
    if (newVal) {
      // 弹窗打开时初始化数据
      initializeData()
    }
  },
  { immediate: true }
)

// 监听visible变化
watch(visible, (newVal) => {
  emit('update:modelValue', newVal)
  if (!newVal) {
    // 弹窗关闭时完全重置状态
    resetAllState()
  }
})

// 监听数据变化，重新计算当前页数据
watch(
  () => props.data,
  () => {
    if (visible.value) {
      initializeTableData()
    }
  },
  { deep: true }
)

// 监听提交状态变化
watch(
  () => apiState.submitting,
  (newVal) => {
    if (newVal) {
      apiState.error = null
      apiState.result = null
    }
  }
)
</script>

<style lang="less">
// CSS变量定义
.batch-reminder-dialog {
  --border-color: #ebeef5;
  --bg-light: #f5f7fa;
  --bg-white: #fafafa;
  --text-primary: #303133;
  --text-secondary: #909399;
  --primary-color: #409eff;
  --hover-color: #c0c4cc;
  --padding-base: 16px;
  --padding-small: 8px;
  --gap-base: 12px;
  --radius-base: 4px;
  --font-size-base: 14px;
  --font-size-small: 12px;
  --font-size-mini: 11px;

  // 对话框内容
  .dialog-content {
    display: flex;
    flex-direction: column;
    height: 100%;
    padding: var(--padding-base);
    min-height: 0;

    .el-table {
      flex: 1;
      min-height: 0;

      :deep(.el-table__inner-wrapper) {
        height: 100%;
      }

      :deep(.el-table__body-wrapper) {
        flex: 1;
        overflow-y: auto;

        &::-webkit-scrollbar {
          width: 6px;
          height: 6px;
        }

        &::-webkit-scrollbar-thumb {
          background-color: #c1c1c1;
          border-radius: 3px;
        }
      }

      :deep(.el-table__header-wrapper) {
        background-color: var(--bg-light);
        flex-shrink: 0;
      }

      // 表格内表单控件
      :deep(.el-table__body .el-form-item) {
        margin-bottom: 0;

        .el-form-item__error {
          position: static;
          padding-top: 2px;
          font-size: var(--font-size-mini);
          line-height: 1.2;
        }
      }
    }
  }

  // 底部按钮区域
  .dialog-footer {
    display: flex;
    justify-content: flex-end;
    gap: var(--gap-base);
    padding: var(--padding-base) 24px;
    border-top: 1px solid var(--border-color);
    background-color: var(--bg-white);

    .el-button {
      min-width: 80px;
      transition: all 0.2s ease;

      &:hover:not(:disabled) {
        transform: translateY(-1px);
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      }

      &:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }
    }
  }

  // 批量操作区域
  .batch-operations {
    padding: var(--gap-base) 0;
    border-bottom: 1px solid var(--border-color);
    margin-bottom: var(--gap-base);
    display: flex;
    align-items: center;
    gap: var(--gap-base);
  }

  // 人员选择器
  .assigned-person-container {
    display: flex;
    flex-direction: column;
    gap: 6px;
    align-items: center;
    padding: 4px;

    .person-tag {
      max-width: 140px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      font-size: var(--font-size-small);
    }

    .select-person-btn {
      font-size: var(--font-size-small);
      padding: 0;
      height: auto;
      min-height: auto;

      &:hover {
        text-decoration: underline;
      }
    }

    .el-button {
      padding: 4px !important;
    }
  }

  // 抽屉样式
  :deep(.el-drawer) {
    position: absolute;
    z-index: 1000;
  }
}

// 抽屉内容
.drawer-content {
  padding: 20px;

  .el-form-item {
    margin-bottom: 20px;

    .el-form-item__label {
      font-weight: 500;
      color: var(--text-primary);
      margin-bottom: var(--padding-small);
    }
  }

  .drawer-person-selector {
    display: flex;
    flex-direction: column;
    gap: var(--padding-small);
    align-items: flex-start;

    .drawer-person-tag {
      max-width: 100%;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .drawer-select-person-btn {
      font-size: var(--font-size-base);
      padding: 0;
      height: auto;
      min-height: auto;

      &:hover {
        text-decoration: underline;
      }
    }
  }
}

// 抽屉底部
.drawer-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--gap-base);
  padding: var(--padding-base) 20px;
  border-top: 1px solid var(--border-color);
  background-color: var(--bg-white);

  .el-button {
    min-width: 80px;
    transition: all 0.2s ease;

    &:hover {
      transform: translateY(-1px);
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }
  }
}

// 响应式设计
@media (max-width: 768px) {
  .batch-reminder-dialog {
    .dialog-content {
      padding: var(--padding-small);

      .batch-operations {
        flex-direction: column;
        align-items: flex-start;
        gap: var(--padding-small);

        .el-button {
          width: 100%;
          justify-content: center;
        }
      }

      .el-table {
        font-size: var(--font-size-small);

        :deep(.el-table__header th),
        :deep(.el-table__body td) {
          padding: var(--padding-small) 0;
          font-size: var(--font-size-small);
        }
      }

      .assigned-person-container {
        gap: 4px;

        .person-tag {
          max-width: 100px;
          font-size: var(--font-size-mini);
        }

        .select-person-btn {
          font-size: var(--font-size-mini);
        }
      }
    }

    .dialog-footer {
      padding: var(--gap-base) var(--padding-base);

      .el-button {
        min-width: 60px;
        font-size: var(--font-size-small);
      }
    }

    :deep(.el-drawer) {
      width: 90% !important;

      .drawer-content {
        padding: var(--gap-base);

        .el-form-item {
          margin-bottom: var(--padding-base);
        }

        .drawer-person-selector {
          gap: 6px;

          .drawer-person-tag,
          .drawer-select-person-btn {
            font-size: var(--font-size-small);
          }
        }
      }

      .drawer-footer {
        padding: var(--gap-base);
        flex-direction: column;
        gap: var(--padding-small);

        .el-button {
          width: 100%;
          margin: 0;
        }
      }
    }
  }
}

// 特殊元素样式
.assigned-reply-time-input .el-input__inner {
  border: none !important;
}
</style>
