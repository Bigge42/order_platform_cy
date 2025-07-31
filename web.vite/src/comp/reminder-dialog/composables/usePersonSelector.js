import { ref, reactive } from 'vue'

export function usePersonSelector() {
  // 人员选择器状态
  const personSelectorState = reactive({
    visible: false,
    selectedPersonId: '',
    currentEditingRowIndex: -1,
    isDrawerMode: false
  })

  // 重置人员选择器状态
  const resetPersonSelectorState = () => {
    Object.assign(personSelectorState, {
      visible: false,
      selectedPersonId: '',
      currentEditingRowIndex: -1,
      isDrawerMode: false
    })
  }

  // 打开人员选择器 - 表格行模式
  const openPersonSelector = (rowIndex, tableData) => {
    personSelectorState.currentEditingRowIndex = rowIndex
    personSelectorState.selectedPersonId = tableData[rowIndex]?.assignedResPerson || ''
    personSelectorState.isDrawerMode = false
    personSelectorState.visible = true
  }

  // 打开人员选择器 - 抽屉模式
  const openDrawerPersonSelector = (batchEditFormData) => {
    personSelectorState.selectedPersonId = batchEditFormData.assignedResPerson || ''
    personSelectorState.isDrawerMode = true
    personSelectorState.visible = true
  }

  // 处理人员确认选择
  const handlePersonConfirm = (person, tableData, batchEditFormData) => {
    const personInfo = {
      name: person.name || person.userTrueName || '',
      loginName: person.userName || person.id || ''
    }

    if (personSelectorState.isDrawerMode) {
      // 抽屉模式：更新抽屉表单数据
      batchEditFormData.assignedResPersonName = personInfo.name
      batchEditFormData.assignedResPerson = personInfo.loginName
    } else if (personSelectorState.currentEditingRowIndex >= 0) {
      // 单行模式：为指定行设置负责人
      const rowIndex = personSelectorState.currentEditingRowIndex
      tableData[rowIndex].assignedResPersonName = personInfo.name
      tableData[rowIndex].assignedResPerson = personInfo.loginName
    }

    resetPersonSelectorState()
    return personInfo
  }

  // 处理人员取消选择
  const handlePersonCancel = () => {
    resetPersonSelectorState()
  }

  // 移除指定负责人 - 表格行
  const removeAssignedPerson = (rowIndex, tableData) => {
    if (rowIndex >= 0 && rowIndex < tableData.length) {
      tableData[rowIndex].assignedResPersonName = ''
      tableData[rowIndex].assignedResPerson = ''
    }
  }

  // 移除指定负责人 - 抽屉
  const removeDrawerAssignedPerson = (batchEditFormData) => {
    batchEditFormData.assignedResPersonName = ''
    batchEditFormData.assignedResPerson = ''
  }





  return {
    // 状态
    personSelectorState,
    
    // 方法
    resetPersonSelectorState,
    openPersonSelector,
    openDrawerPersonSelector,
    handlePersonConfirm,
    handlePersonCancel,
    removeAssignedPerson,
    removeDrawerAssignedPerson
  }
} 