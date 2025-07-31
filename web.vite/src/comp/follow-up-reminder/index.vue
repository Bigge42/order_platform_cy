<template>
  <div class="follow-up-reminder-dialog">
    <CompDialog
      v-model="visible"
      title="催单"
      width="1200"
      :padding="0"
      :footer="true"
      :onModelClose="handleDialogClose"
    >
      <template #content>
        <div class="dialog-content">
          <!-- 表格区域 -->
          <el-table
            ref="tableRef"
            :data="currentPageData"
            stripe
            style="width: 100%"
            @selection-change="handleSelectionChange"
            row-key="id"
            :reserve-selection="true"
            border
          >
            <el-table-column type="selection" width="55" resizable />
            
            <el-table-column
              prop="index"
              label="序号"
              width="80"
              align="center"
              resizable
            >
              <template #default="{ $index }">
                {{ (currentPage - 1) * pageSize + $index + 1 }}
              </template>
            </el-table-column>
            
            <el-table-column
              prop="contractNo"
              label="合同号"
              width="150"
              show-overflow-tooltip
              resizable
            />
            
            <el-table-column
              prop="planTrackNo"
              label="计划跟踪号"
              width="150"
              show-overflow-tooltip
              resizable
            />
            
            <el-table-column
              prop="business"
              label="业务"
              width="120"
              resizable
            >
              <template #default="{ row, $index }">
                <el-select
                  v-model="row.business"
                  placeholder="请选择"
                  @change="updateRowData($index, 'business', row.business)"
                >
                  <el-option
                    v-for="item in businessOptions"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  />
                </el-select>
              </template>
            </el-table-column>
            
            <el-table-column
              prop="defaultResponsible"
              label="默认负责人"
              width="120"
              show-overflow-tooltip
              resizable
            />
            
            <el-table-column
              label="指定负责人"
              width="200"
              resizable
            >
              <template #default="{ row, $index }">
                <div class="responsible-person-cell" :class="{ 'has-persons': row.assignedPersons && row.assignedPersons.length > 0 }">
                  <div v-if="row.assignedPersons && row.assignedPersons.length > 0" class="selected-persons">
                    <el-tag
                      v-for="person in row.assignedPersons"
                      :key="person.id"
                      type="info"
                      size="large"
                      closable
                      @close="removeAssignedPerson($index, person.id)"
                      class="person-tag"
                    >
                      {{ person.name }}
                    </el-tag>
                    <!-- 有人员时显示图标按钮 -->
                    <el-button
                      type="primary"
                      size="small"
                      plain
                      @click="openPersonSelector($index)"
                      :icon="Plus"
                    />
                  </div>
                  <!-- 无人员时显示文字按钮 -->
                  <el-button
                    v-else
                    size="small"
                    type="primary"
                    @click="openPersonSelector($index)"
                    :icon="User"
                  >
                    选择人员
                  </el-button>
                </div>
              </template>
            </el-table-column>
            
            <el-table-column
              prop="urgentLevel"
              label="紧急等级"
              width="120"
              resizable
            >
              <template #default="{ row, $index }">
                <el-select
                  v-model="row.urgentLevel"
                  placeholder="请选择"
                  @change="updateRowData($index, 'urgentLevel', row.urgentLevel)"
                >
                  <el-option
                    v-for="item in urgentLevelOptions"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  />
                </el-select>
              </template>
            </el-table-column>
            
            <el-table-column
              prop="replyTime"
              label="指定回复时间"
              width="180"
              resizable
            >
              <template #default="{ row, $index }">
                <el-input
                  v-model="row.replyTime"
                  type="number"
                  :min="1"
                  :max="999"
                  size="small"
                  placeholder="请输入时间"
                  @input="updateRowData($index, 'replyTime', row.replyTime)"
                  class="input-with-select"
                >
                  <template #append>
                    <el-select 
                      v-model="row.replyTimeUnit" 
                      placeholder="单位" 
                      style="width: 80px"
                      @change="updateRowData($index, 'replyTimeUnit', row.replyTimeUnit)"
                    >
                      <el-option label="分钟" value="minute" />
                      <el-option label="小时" value="hour" />
                      <el-option label="天" value="day" />
                    </el-select>
                  </template>
                </el-input>
              </template>
            </el-table-column>
            
            <el-table-column
              prop="messageContent"
              label="消息内容"
              min-width="300"
              resizable
            >
              <template #default="{ row, $index }">
                <el-input
                  v-model="row.messageContent"
                  type="textarea"
                  :rows="2"
                  placeholder="请输入消息内容"
                  @input="updateRowData($index, 'messageContent', row.messageContent)"
                />
              </template>
            </el-table-column>
          </el-table>
          
          <!-- 分页区域 -->
          <div class="pagination-wrapper">
            <el-pagination
              v-model:current-page="currentPage"
              v-model:page-size="pageSize"
              :page-sizes="[20, 50, 100]"
              :total="tableData.length"
              layout="total, sizes, prev, pager, next, jumper"
              background
              @current-change="handlePageChange"
              @size-change="handleSizeChange"
            />
          </div>
        </div>
      </template>
      
      <template #footer>
        <div class="dialog-footer">
          <el-button 
            type="success" 
            @click="handleAutoGenerate"
          >
            自动生成内容发送
          </el-button>
          <el-button 
            type="primary" 
            @click="handleSend"
            :disabled="allSelectedRows.length === 0"
          >
            发送 ({{ allSelectedRows.length }})
          </el-button>
        </div>
      </template>
    </CompDialog>
    
    <!-- 人员选择器 -->
    <PersonSelector
      v-model="personSelectorVisible"
      :selectedPersonIds="currentRowPersonIds"
      @confirm="handlePersonConfirm"
      @cancel="handlePersonCancel"
    />
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick } from 'vue'
import { User, Plus } from '@element-plus/icons-vue'
import PersonSelector from '@/comp/person-selector/index.vue'
import CompDialog from '@/comp/dialog/index.vue'

// Props
const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  data: {
    type: Array,
    default: () => []
  }
})

// Emits
const emit = defineEmits(['update:modelValue', 'send'])

// 响应式数据
const visible = ref(false)
const currentPage = ref(1)
const pageSize = ref(20)
const tableData = ref([])
const allSelectedRows = ref([]) // 全局选中的行数据，记录所有页面的选中状态
const personSelectorVisible = ref(false)
const currentSelectingRowIndex = ref(-1)
const currentRowPersonIds = ref([])
const tableRef = ref()

// 下拉框选项
const businessOptions = ref([
  { label: '销售', value: 'sales' },
  { label: '采购', value: 'purchase' },
  { label: '生产', value: 'production' },
  { label: '财务', value: 'finance' }
])

const urgentLevelOptions = ref([
  { label: '特急', value: 'urgent' },
  { label: 'A', value: 'A' },
  { label: 'B', value: 'B' },
  { label: 'C', value: 'C' }
])

// 时间单位选项
const timeUnitOptions = ref([
  { label: '分钟', value: 'minute' },
  { label: '小时', value: 'hour' },
  { label: '天', value: 'day' }
])

// 计算属性
const currentPageData = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return tableData.value.slice(start, end)
})

// 监听props变化
watch(
  () => props.modelValue,
  (newVal) => {
    visible.value = newVal
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
    if (newData && newData.length > 0) {
      // 为每行数据添加必要的字段
      tableData.value = newData.map((item, index) => ({
        ...item,
        id: item.id || index, // 确保有唯一ID
        assignedPersons: item.assignedPersons || [],
        business: item.business || '',
        urgentLevel: item.urgentLevel || '',
        replyTime: item.replyTime || item.replyDays || null, // 兼容原来的replyDays字段
        replyTimeUnit: item.replyTimeUnit || 'day', // 默认单位为天
        messageContent: item.messageContent || ''
      }))
      
      // 重置选中状态
      allSelectedRows.value = []
      
      // 如果弹窗已打开，需要重新同步表格选中状态
      if (visible.value) {
        nextTick(() => {
          syncTableSelection()
        })
      }
    }
  },
  { immediate: true, deep: true }
)

// 监听弹窗打开，同步选中状态
watch(visible, (newVal) => {
  if (newVal) {
    nextTick(() => {
      syncTableSelection()
    })
  }
})

// 方法
const handleClose = () => {
  visible.value = false
  emit('update:modelValue', false)
}

const handleSelectionChange = (selection) => {
  // 移除当前页面在全局选中中的数据
  const currentPageIds = currentPageData.value.map(row => row.id)
  allSelectedRows.value = allSelectedRows.value.filter(row => !currentPageIds.includes(row.id))
  
  // 添加当前页面选中的数据到全局选中
  for (const row of selection) {
    // 从完整数据中找到对应的行（确保是最新数据）
    const fullRow = tableData.value.find(item => item.id === row.id)
    if (fullRow) {
      allSelectedRows.value.push(fullRow)
    }
  }
}

const handlePageChange = () => {
  nextTick(() => {
    syncTableSelection()
  })
}

const handleSizeChange = () => {
  nextTick(() => {
    syncTableSelection()
  })
}

const syncTableSelection = () => {
  if (!tableRef.value) return
  
  // 清除当前表格选中状态
  tableRef.value.clearSelection()
  
  // 根据全局选中状态设置当前页面的选中状态
  const currentPageIds = currentPageData.value.map(row => row.id)
  const selectedIds = allSelectedRows.value.map(row => row.id)
  
  for (const row of currentPageData.value) {
    if (selectedIds.includes(row.id)) {
      tableRef.value.toggleRowSelection(row, true)
    }
  }
}

const updateRowData = (index, field, value) => {
  const actualIndex = (currentPage.value - 1) * pageSize.value + index
  if (tableData.value[actualIndex]) {
    tableData.value[actualIndex][field] = value
    
    // 同时更新全局选中数据中的对应行
    const rowId = tableData.value[actualIndex].id
    const selectedRowIndex = allSelectedRows.value.findIndex(row => row.id === rowId)
    if (selectedRowIndex >= 0) {
      allSelectedRows.value[selectedRowIndex][field] = value
    }
  }
}

const openPersonSelector = (index) => {
  const actualIndex = (currentPage.value - 1) * pageSize.value + index
  currentSelectingRowIndex.value = actualIndex
  
  const row = tableData.value[actualIndex]
  currentRowPersonIds.value = row.assignedPersons ? row.assignedPersons.map(p => p.id) : []
  personSelectorVisible.value = true
}

const handlePersonConfirm = (persons) => {
  if (currentSelectingRowIndex.value >= 0) {
    tableData.value[currentSelectingRowIndex.value].assignedPersons = persons
    
    // 同时更新全局选中数据中的对应行
    const rowId = tableData.value[currentSelectingRowIndex.value].id
    const selectedRowIndex = allSelectedRows.value.findIndex(row => row.id === rowId)
    if (selectedRowIndex >= 0) {
      allSelectedRows.value[selectedRowIndex].assignedPersons = persons
    }
  }
  personSelectorVisible.value = false
  currentSelectingRowIndex.value = -1
}

const handlePersonCancel = () => {
  personSelectorVisible.value = false
  currentSelectingRowIndex.value = -1
}

const removeAssignedPerson = (index, personId) => {
  const actualIndex = (currentPage.value - 1) * pageSize.value + index
  const row = tableData.value[actualIndex]
  if (row.assignedPersons) {
    row.assignedPersons = row.assignedPersons.filter(p => p.id !== personId)
    
    // 同时更新全局选中数据中的对应行
    const selectedRowIndex = allSelectedRows.value.findIndex(selectedRow => selectedRow.id === row.id)
    if (selectedRowIndex >= 0) {
      allSelectedRows.value[selectedRowIndex].assignedPersons = row.assignedPersons
    }
  }
}

// 获取时间文本描述
const getReplyTimeText = (replyTime, replyTimeUnit) => {
  if (!replyTime) return ''
  
  const unitText = timeUnitOptions.value.find(opt => opt.value === replyTimeUnit)?.label || '天'
  return `${replyTime}${unitText}内`
}

const handleAutoGenerate = () => {
  if (allSelectedRows.value.length === 0) {
    return
  }
  
  // 为选中的行自动生成消息内容
  for (const row of allSelectedRows.value) {
    if (!row.messageContent) {
      const urgentText = urgentLevelOptions.value.find(opt => opt.value === row.urgentLevel)?.label || ''
      const businessText = businessOptions.value.find(opt => opt.value === row.business)?.label || ''
      const replyText = getReplyTimeText(row.replyTime, row.replyTimeUnit)
      
      row.messageContent = `【${urgentText}】${businessText} - 合同号：${row.contractNo}，计划跟踪号：${row.planTrackNo}，请${row.defaultResponsible}${replyText}及时处理。`
      
      // 同时更新tableData中的对应行
      const tableRowIndex = tableData.value.findIndex(tableRow => tableRow.id === row.id)
      if (tableRowIndex >= 0) {
        tableData.value[tableRowIndex].messageContent = row.messageContent
      }
    }
  }
  
  // 发送数据
  emit('send', allSelectedRows.value)
  // 重置状态
  resetComponentState()
  handleClose()
}

const handleSend = () => {
  if (allSelectedRows.value.length === 0) {
    return
  }
  
  emit('send', allSelectedRows.value)
  // 重置状态
  resetComponentState()
  handleClose()
}

// 重置组件状态
const resetComponentState = () => {
  allSelectedRows.value = []
  currentPage.value = 1
  personSelectorVisible.value = false
  currentSelectingRowIndex.value = -1
  currentRowPersonIds.value = []
}

const handleDialogClose = () => {
  resetComponentState()
  handleClose()
  return true
}
</script>

<style scoped>
.dialog-content {
  height: 100%;
  display: flex;
  flex-direction: column;
  padding: 16px;
  flex: 1;
}

.dialog-content .el-table {
  flex: 1;
}

.responsible-person-cell {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.responsible-person-cell.has-persons {
  flex-direction: row;
  align-items: center;
  gap: 0;
}

.selected-persons {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  align-items: center;
}

.person-tag {
  max-width: 80px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.pagination-wrapper {
  padding: 16px 0;
  display: flex;
  justify-content: center;
  border-top: 1px solid #ebeef5;
  margin-top: 16px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

:deep(.el-table .el-table__cell) {
  padding: 8px 0;
}

:deep(.el-input-number) {
  width: 100%;
}

:deep(.el-select) {
  width: 100%;
}
</style>
