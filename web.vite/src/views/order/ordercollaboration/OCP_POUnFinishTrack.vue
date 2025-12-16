<!--
采购跟踪
 -->
<template>
  <view-grid
    ref="grid"
    :columns="columns"
    :detail="detail"
    :details="details"
    :editFormFields="editFormFields"
    :editFormOptions="editFormOptions"
    :searchFormFields="searchFormFields"
    :searchFormOptions="searchFormOptions"
    :table="table"
    :extend="extend"
    :onInit="onInit"
    :onInited="onInited"
    :searchBefore="searchBefore"
    :searchAfter="searchAfter"
    :addBefore="addBefore"
    :updateBefore="updateBefore"
    :rowClick="rowClick"
    :modelOpenBefore="modelOpenBefore"
    :modelOpenAfter="modelOpenAfter"
  >
    <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
    <template #gridHeader> </template>
  </view-grid>

  <!-- 催单弹窗组件 -->
  <ReminderDialog
    v-model="followUpReminderVisible"
    :data="followUpReminderData"
    @send="handleFollowUpSend"
  />

  <!-- 批量催单弹窗组件 -->
  <BatchReminderDialog
    v-model="batchReminderDialogVisible"
    :data="batchReminderData"
    :business-type="'PO'"
    title="批量催单"
    width="90%"
    @confirm="handleBatchReminderConfirm"
    @cancel="handleBatchReminderCancel"
  />

  <!-- 批量协商弹窗组件 -->
  <BatchNegotiationDialog
    v-model="batchNegotiationDialogVisible"
    :data="batchNegotiationData"
    title="批量协商"
    width="90%"
    @confirm="handleBatchNegotiationConfirm"
    @cancel="handleBatchNegotiationCancel"
  />

  <!-- 发起协商弹窗组件 -->
  <NegotiationDialog
    v-model="negotiationDialogVisible"
    :data="negotiationDialogData"
    @confirm="handleNegotiationConfirm"
    @cancel="handleNegotiationCancel"
  />

  <!-- 留言板弹窗组件 -->
  <MessageBoard ref="messageBoardRef" />
</template>
<script setup lang="jsx">
import extend from '@/extension/order/ordercollaboration/OCP_POUnFinishTrack.jsx'
import viewOptions from './OCP_POUnFinishTrack/options.js'
import ReminderDialog from '@/comp/reminder-dialog/index.vue'
import BatchReminderDialog from '@/comp/reminder-dialog/batch.vue'
import NegotiationDialog from '@/comp/negotiation-dialog/index.vue'
import BatchNegotiationDialog from '@/comp/negotiation-dialog/batch.vue'
import MessageBoard from '@/comp/message-board/index.vue'
import { useRoute } from 'vue-router'
import { ref, reactive, getCurrentInstance, watch, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { applyAlertWarningStyle } from '@/utils/alertWarning'

const grid = ref(null)
const { proxy } = getCurrentInstance()
const route = useRoute()
//http请求，proxy.http.post/get
const viewOpts = reactive(viewOptions())
const {
  table,
  editFormFields,
  editFormOptions,
  searchFormFields,
  searchFormOptions,
  columns,
  detail,
  details
} = viewOpts

// 催单弹窗相关
const followUpReminderVisible = ref(false)
const followUpReminderData = ref({})

// 批量催单弹窗相关
const batchReminderDialogVisible = ref(false)
const batchReminderData = ref([])

// 批量协商弹窗相关
const batchNegotiationDialogVisible = ref(false)
const batchNegotiationData = ref([])

// 发起协商弹窗相关
const negotiationDialogVisible = ref(false)
const negotiationDialogData = ref({})

// 留言板ref
const messageBoardRef = ref(null)

let gridRef //对应[表.jsx]文件中this.使用方式一样
//生成对象属性初始化
const onInit = async ($vm) => {
  gridRef = $vm
  //设置分页条大小
  gridRef.pagination.sizes = [20, 50, 100, 200, 500, 1000]
  //设置默认分页数
  gridRef.pagination.size = 20
  // gridRef.setFixedSearchForm(true);

  // 设置快捷查询字段
  gridRef.queryFields = ['PlanTraceNo', 'MaterialCode', 'Urgency', 'POBillNo']

  //与jsx中的this.xx使用一样，只需将this.xx改为gridRef.xx

  const query = route.query
  for (const key in query) {
    if (query[key]) {
      // 对URL参数进行解码
      gridRef.searchFormFields[key] = decodeURIComponent(query[key])
    }
  }
}
//生成对象属性初始化后,操作明细表配置用到
const onInited = async () => {
  // 添加批量催单按钮
  gridRef.buttons.push({
    name: '批量催单',
    icon: 'el-icon-bell',
    type: 'warning',
    onClick: handleBatchReminder
  })

  // 添加批量协商按钮（参考批量催单逻辑）
  gridRef.buttons.push({
    name: '批量协商',
    icon: 'el-icon-chat-dot-round',
    type: 'primary',
    onClick: handleBatchNegotiate
  })

  // 应用预警样式(在表格初始化完成后,添加列之前应用)
  applyAlertWarningStyle(viewOpts.columns)

  // 添加功能列
  columns.push({
    field: '功能',
    title: '功能',
    width: 150,
    align: 'center',
    render: (h, { row, column, index }) => {
      return (
        <div>
          <el-button
            type="primary"
            link
            style={{ padding: '2px 4px', fontSize: '12px' }}
            onClick={($e) => handleJumpToShortage(row)}
          >
            跳转缺料
          </el-button>
          {/* <el-button 
                            type="primary" 
                            link 
                            style={{padding: '2px 4px', fontSize: '12px'}} 
                            onClick={($e) => handleJumpToPurchase(row)}>跳转采购</el-button> */}
        </div>
      )
    }
  })

  // 添加操作列
  columns.push({
    field: 'action',
    title: '操作',
    width: 200,
    align: 'center',
    fixed: 'right',
    render: (h, { row, column, index }) => {
      return (
        <div
          style={{ display: 'flex', gap: '6px', alignItems: 'center', justifyContent: 'center' }}
        >
          <el-button type="success" size="small" onClick={($e) => handleOpenMessageBoard(row)}>
            消息
          </el-button>
          <el-button
            type="warning"
            size="small"
            style={{ marginLeft: '0px' }}
            onClick={($e) => handleUrge(row)}
          >
            催单
          </el-button>
          <el-button
            type="primary"
            size="small"
            style={{ marginLeft: '0px' }}
            onClick={($e) => handleNegotiate(row)}
          >
            协商
          </el-button>
        </div>
      )
    }
  })

  // 配置Table表合计功能
  columns.forEach((col) => {
    if (col.field === 'InboundQty') {
      col.summary = {
        sum: true,
        summaryFormatter: (value) => {
          return value ? value.toLocaleString() : '0'
        },
        summaryLabel: '汇总'
      }
    }
  })
}
const searchBefore = async (param) => {
  //界面查询前,可以给param.wheres添加查询参数
  //返回false，则不会执行查询
  return true
}
const searchAfter = async (rows, result) => {
  return true
}
const addBefore = async (formData) => {
  //新建保存前formData为对象，包括明细表，可以给给表单设置值，自己输出看formData的值
  return true
}
const updateBefore = async (formData) => {
  //编辑保存前formData为对象，包括明细表、删除行的Id
  return true
}
const rowClick = ({ row, column, event }) => {
  //查询界面点击行事件
  // grid.value.toggleRowSelection(row); //单击行时选中当前行;
}
const modelOpenBefore = async (row) => {
  //弹出框打开后方法
  return true //返回false，不会打开弹出框
}
const modelOpenAfter = (row) => {
  //弹出框打开后方法,设置表单默认值,按钮操作等
}

// 催单操作
const handleUrge = (row) => {
  console.log('催单操作:', row)
  console.log('当前行完整数据:', row)

  // 准备催单数据 - 参考OCP_PurchaseOrderDetail的标准格式
  const reminderData = {
    SupplierCode: row.SupplierCode,
    BusinessType: 'PO',
    BusinessKey: row.FENTRYID,
    // 保存当前行信息，用于提交时使用
    currentRowData: row
  }

  console.log('准备的催单数据:', reminderData)

  // 设置数据并打开弹窗
  followUpReminderData.value = reminderData
  followUpReminderVisible.value = true
}

// 处理催单发送
const handleFollowUpSend = async (reminderData) => {
  console.log('发送催单数据:', reminderData)

  try {
    // 获取当前行数据
    const currentRow = reminderData.currentRowData

    // 添加防护性检查
    if (!currentRow) {
      console.error('当前行数据为空:', reminderData)
      ElMessage.error('获取当前行数据失败，请重新操作')
      return
    }

    console.log('当前行数据:', currentRow)

    // 构建提交数据，添加必要字段
    const submitData = {
      ...reminderData,
      // 行号
      Seq: currentRow.Seq || '',
      // 计划跟踪号
      PlanTraceNo: currentRow.PlanTraceNo || '',
      // 单据编号
      BillNo: currentRow.POBillNo || '',
      // 默认负责人姓名 - 取默认负责人的name
      DefaultResPersonName: reminderData.DefaultResPerson || '',
      // 默认负责人登录名 - 取默认负责人的loginName
      DefaultResPerson: reminderData.DefaultResPersonLoginName || '',
      // 采购负责人姓名（指定负责人）- 取选中人的userTrueName
      AssignedResPersonName: reminderData.AssignedResPerson
        ? reminderData.AssignedResPerson.userTrueName
        : '',
      // 原有的AssignedResPerson字段保持兼容
      AssignedResPerson: reminderData.AssignedResPerson
        ? reminderData.AssignedResPerson.userName
        : ''
    }

    // 移除临时保存的数据，避免提交到后端
    delete submitData.currentRowData
    delete submitData.DefaultResPersonLoginName

    console.log('最终提交数据:', submitData)

    // 调用催单接口
    const response = await proxy.http.post('/api/OCP_UrgentOrder/add', { mainData: submitData })

    if (response.status) {
      ElMessage.success(response.message || '催单发送成功')
      // 关闭弹窗
      followUpReminderVisible.value = false
      // 刷新表格数据
      gridRef.search()
    } else {
      ElMessage.error(response.message || '催单发送失败')
    }
  } catch (error) {
    console.error('催单发送失败:', error)
    ElMessage.error('催单发送失败，请稍后重试')
  }
}

// 批量催单处理
const handleBatchReminder = () => {
  // 获取选中的行数据
  const selectedRows = gridRef.getTable(true).getSelected()

  if (!selectedRows || selectedRows.length === 0) {
    ElMessage.warning('请先选择要催单的数据')
    return
  }

  // 根据用户指定的字段映射处理数据
  batchReminderData.value = selectedRows.map((row) => ({
    ...row,
    seq: row.Seq, // seq 对应数据中LineNumber
    billNo: row.POBillNo, // billNo 对应数据中POBillNo
    businessKey: row.FENTRYID // businessKey 对应数据中的FENTRYID
  }))
  batchReminderDialogVisible.value = true
}

// 批量催单确认处理
const handleBatchReminderConfirm = (eventData) => {
  console.log('批量催单确认:', eventData)

  // 打印接收到的数据
  console.log('用户编辑的催单数据:', eventData.data)
  console.log('原始选中的行数据:', eventData.originalData)
  console.log('数据总数:', eventData.totalCount)

  // 关闭弹窗
  batchReminderDialogVisible.value = false
}

// 批量催单取消处理
const handleBatchReminderCancel = () => {
  console.log('取消批量催单')
  batchReminderData.value = []
}

// 批量协商处理（参考批量催单逻辑）
const handleBatchNegotiate = () => {
  const selectedRows = gridRef.getTable(true).getSelected()

  if (!selectedRows || selectedRows.length === 0) {
    ElMessage.warning('请先选择要协商的数据')
    return
  }

  batchNegotiationData.value = selectedRows.map((row) => ({
    ...row,
    billNo: row.POBillNo,
    seq: row.Seq,
    planTraceNo: row.PlanTraceNo,
    businessKey: row.FENTRYID,
    businessType: 'PO',
    defaultResPersonName: row.DefaultResPersonName || '',
    defaultResPerson: row.DefaultResPerson || ''
  }))
  batchNegotiationDialogVisible.value = true
}

const handleBatchNegotiationConfirm = (eventData) => {
  console.log('批量协商确认:', eventData)
  batchNegotiationDialogVisible.value = false
}

const handleBatchNegotiationCancel = () => {
  console.log('取消批量协商')
  batchNegotiationData.value = []
}

// 协商操作
const handleNegotiate = (row) => {
  console.log('协商操作:', row)
  console.log('当前行完整数据:', row)

  // 准备协商数据 - 参考OCP_PurchaseOrderDetail的标准格式
  negotiationDialogData.value = {
    BusinessType: 'PO',
    BusinessKey: row.FENTRYID,
    BillNo: row.POBillNo,
    SupplierCode: row.SupplierCode,
    // 保存当前行信息，用于提交时使用
    currentRowData: row
  }

  // 打开发起协商弹窗
  negotiationDialogVisible.value = true
}

// 发起协商弹窗确认事件
const handleNegotiationConfirm = async (negotiationData) => {
  console.log('发起协商确认:', negotiationData)

  try {
    // 获取当前行数据
    const currentRow = negotiationData.currentRowData

    // 添加防护性检查
    if (!currentRow) {
      console.error('当前行数据为空:', negotiationData)
      ElMessage.error('获取当前行数据失败，请重新操作')
      return
    }

    console.log('当前行数据:', currentRow)

    // 构建提交数据，添加与催单相同的字段
    const submitData = {
      ...negotiationData,
      // 行号
      Seq: currentRow.Seq || '',
      // 计划跟踪号
      PlanTraceNo: currentRow.PlanTraceNo || '',
      // 单据编号
      BillNo: currentRow.POBillNo || '',
      // 默认负责人姓名 - 取默认负责人的name
      DefaultResPersonName: negotiationData.DefaultResPerson || '',
      // 默认负责人登录名 - 取默认负责人的loginName
      DefaultResPerson: negotiationData.DefaultResPersonLoginName || '',
      // 采购负责人姓名（指定负责人）- 取选中人的userTrueName
      AssignedResPersonName: negotiationData.AssignedResPerson
        ? negotiationData.AssignedResPerson.userTrueName
        : '',
      // 原有的AssignedResPerson字段保持兼容
      AssignedResPerson: negotiationData.AssignedResPerson
        ? negotiationData.AssignedResPerson.userName
        : ''
    }

    // 移除临时保存的数据，避免提交到后端
    delete submitData.currentRowData
    delete submitData.DefaultResPersonLoginName

    console.log('最终提交数据:', submitData)

    // 调用发起协商接口
    const response = await proxy.http.post('/api/OCP_Negotiation/add', { mainData: submitData })

    if (response.status) {
      ElMessage.success(response.message || '协商发起成功')
      // 关闭弹窗
      negotiationDialogVisible.value = false
      // 刷新表格数据
      gridRef.search()
    } else {
      ElMessage.error(response.message || '协商发起失败')
    }
  } catch (error) {
    console.error('发起协商失败:', error)
    ElMessage.error('协商发起失败，请稍后重试')
  }
}

// 发起协商弹窗取消事件
const handleNegotiationCancel = () => {
  console.log('取消发起协商')
  negotiationDialogData.value = {}
}

// 打开留言板
const handleOpenMessageBoard = (row) => {
  console.log('打开留言板:', row)
  // 传入业务类型和业务键值
  messageBoardRef.value?.open('PO', row.FENTRYID)
}

// 跳转缺料
const handleJumpToShortage = (row) => {
  console.log('跳转缺料:', row)
  const MaterialNumber = row.MaterialCode
  const BillNo = row.POBillNo

  proxy.$tabs.open({
    text: '采购缺料信息',
    path: '/OCP_LackMtrlResult_PO',
    query: {
      MaterialNumber: encodeURIComponent(MaterialNumber || ''),
      BillNo: encodeURIComponent(BillNo || '')
    }
  })

  proxy.$tabs.clearCache('OCP_LackMtrlResult_PO')
}

// 跳转采购
const handleJumpToPurchase = (row) => {
  console.log('跳转采购:', row)
  const MaterialNumber = row.MaterialCode
  const BillNo = row.POBillNo

  proxy.$tabs.open({
    text: '采购订单表',
    path: '/OCP_PurchaseOrder',
    query: {
      MaterialNumber: encodeURIComponent(MaterialNumber || ''),
      BillNo: encodeURIComponent(BillNo || '')
    }
  })

  proxy.$tabs.clearCache('OCP_PurchaseOrder')
}

//监听表单输入，做实时计算
//watch(() => editFormFields.字段,(newValue, oldValue) => {	})
//对外暴露数据
defineExpose({
  openFollowUpReminder: (rowData) => {
    if (rowData) {
      handleUrge(rowData)
    } else {
      followUpReminderVisible.value = true
    }
  },
  closeFollowUpReminder: () => {
    followUpReminderVisible.value = false
  },
  openNegotiationDialog: (rowData) => {
    if (rowData) {
      handleNegotiate(rowData)
    } else {
      negotiationDialogVisible.value = true
    }
  },
  closeNegotiationDialog: () => {
    negotiationDialogVisible.value = false
    negotiationDialogData.value = {}
  },
  openBatchReminderDialog: (selectedData) => {
    if (selectedData && selectedData.length > 0) {
      // 直接传递原始数据，让组件内部处理
      batchReminderData.value = selectedData
    }
    batchReminderDialogVisible.value = true
  },
  closeBatchReminderDialog: () => {
    batchReminderDialogVisible.value = false
    batchReminderData.value = []
  },
  openBatchNegotiationDialog: (selectedData) => {
    if (selectedData && selectedData.length > 0) {
      batchNegotiationData.value = selectedData
    }
    batchNegotiationDialogVisible.value = true
  },
  closeBatchNegotiationDialog: () => {
    batchNegotiationDialogVisible.value = false
    batchNegotiationData.value = []
  }
})
</script>
