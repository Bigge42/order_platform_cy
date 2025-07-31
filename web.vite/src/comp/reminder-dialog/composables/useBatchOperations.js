import { ref, reactive } from 'vue'

export function useBatchOperations() {
  // 批量操作状态
  const batchState = reactive({
    selectedRows: [],
    drawerVisible: false,
    formData: {
      urgencyLevel: '',
      assignedResPersonName: '',
      assignedResPerson: '',
      assignedReplyTime: '',
      timeUnit: '1',
      urgentType: null,
      urgentContent: ''
    },
    selectedRowsSnapshot: []
  })

  // 表格引用
  const tableRef = ref()

  // 重置批量操作状态
  const resetBatchState = () => {
    Object.assign(batchState, {
      selectedRows: [],
      drawerVisible: false,
      formData: {
        urgencyLevel: '',
        assignedResPersonName: '',
        assignedResPerson: '',
        assignedReplyTime: '',
        timeUnit: '1',
        urgentType: null,
        urgentContent: ''
      },
      selectedRowsSnapshot: []
    })
    
    // 清除表格选中状态
    if (tableRef.value) {
      tableRef.value.clearSelection()
    }
  }

  // 处理表格选择变化
  const handleSelectionChange = (selection) => {
    batchState.selectedRows = selection
  }

  // 打开批量编辑抽屉
  const handleBatchEdit = () => {
    if (batchState.selectedRows.length === 0) {
      console.warn('批量编辑：未选择任何行')
      return false
    }

    try {
      // 创建选中行的快照
      batchState.selectedRowsSnapshot = [...batchState.selectedRows]
      
      // 重置表单数据
      batchState.formData = {
        urgencyLevel: '',
        assignedResPersonName: '',
        assignedResPerson: '',
        assignedReplyTime: '',
        timeUnit: '1',
        urgentType: null,
        urgentContent: ''
      }

      batchState.drawerVisible = true
      return true
    } catch (error) {
      console.error('批量编辑初始化失败:', error)
      return false
    }
  }

  // 确认批量编辑
  const confirmBatchEdit = (tableData) => {
    try {
      // 批量更新逻辑：只更新抽屉中有值的字段
      batchState.selectedRowsSnapshot.forEach(selectedRow => {
        const rowIndex = tableData.findIndex(item => item === selectedRow)
        if (rowIndex >= 0) {
          // 只更新有值的字段
          Object.keys(batchState.formData).forEach(key => {
            const value = batchState.formData[key]
            if (value !== '' && value !== null && value !== undefined) {
              tableData[rowIndex][key] = value
            }
          })
        }
      })

      // 返回更新的行数
      const updatedCount = batchState.selectedRowsSnapshot.length
      
      // 重置状态
      resetBatchState()
      
      return {
        success: true,
        updatedCount,
        message: `批量编辑完成，已更新 ${updatedCount} 条记录`
      }
    } catch (error) {
      console.error('批量编辑更新失败:', error)
      return {
        success: false,
        error: error.message,
        message: '批量编辑失败'
      }
    }
  }







  return {
    // 状态
    batchState,
    tableRef,
    
    // 方法
    resetBatchState,
    handleSelectionChange,
    handleBatchEdit,
    confirmBatchEdit
  }
} 