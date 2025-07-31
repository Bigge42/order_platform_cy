// 紧急等级选项
export const URGENCY_LEVEL_OPTIONS = [
  { label: '特急', value: '特急' },
  { label: 'A', value: 'A' },
  { label: 'B', value: 'B' },
  { label: 'C', value: 'C' }
]

// 时间单位选项
export const TIME_UNIT_OPTIONS = [
  { label: '分钟', value: '1' },
  { label: '小时', value: '2' },
  { label: '天', value: '3' }
]

// 催单类型选项
export const URGENT_TYPE_OPTIONS = [
  { label: '催交期', value: 0 },
  { label: '催进度', value: 1 }
]

// 默认表单数据
export const DEFAULT_ROW_DATA = {
  businessType: '',
  defaultResPersonName: '',
  defaultResPerson: '',
  urgencyLevel: '',
  assignedReplyTime: '',
  timeUnit: '3', // 默认为天
  assignedResPersonName: '',
  assignedResPerson: '',
  urgentType: 0, // 默认为催交期
  urgentContent: ''
}

// 默认批量编辑表单数据
export const DEFAULT_BATCH_EDIT_DATA = {
  urgencyLevel: '',
  assignedResPersonName: '',
  assignedResPerson: '',
  assignedReplyTime: '',
  timeUnit: '1', // 批量编辑默认为分钟
  urgentType: null,
  urgentContent: ''
}





 