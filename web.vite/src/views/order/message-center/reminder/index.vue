<!-- 消息提醒中心 -->
<template>
  <div class="reminder-page">
    <!-- 消息统计组件 -->
    <div class="stats-section">
      <div class="section-header">
        <h3 class="section-title">消息统计概览</h3>
        <!-- <div class="search-controls">
          <div class="search-fields">
            <div class="search-item">
              <span class="search-label">计划跟踪号：</span>
              <el-input
                v-model="searchParams.planTrackNo"
                placeholder="请输入计划跟踪号"
                clearable
                style="width: 200px;"
              />
            </div>
          </div>
          <el-button 
            type="primary" 
            @click="refreshAllStats"
            :loading="refreshLoading"
            title="刷新统计数据"
          >
            查询
          </el-button>
        </div> -->
      </div>
      <MessageCount 
        :count-list="messageStats"
        :selected-type="selectedMessageType"
        @item-click="handleStatClick"
        @refresh="handleRefresh"
      />
    </div>

    <!-- 消息列表区域 -->
    <div class="message-section">
      <!-- Tabs切换器 -->
      <div class="tabs-header">
        <el-tabs 
          v-model="activeTab" 
          @tab-click="handleTabClick"
        >
          <el-tab-pane 
            v-for="tab in messageTabs" 
            :key="tab.name"
            :label="tab.label" 
            :name="tab.name"
          />
        </el-tabs>
      </div>

      <!-- 表格容器 -->
      <div class="table-section">
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
        </view-grid>
      </div>
    </div>

    <!-- 回复弹窗 -->
    <ReplyDialog
      v-model="replyDialogVisible"
      :message-id="currentMessage.id"
      @submit="handleReplySubmit"
    />



    <MessageBoard ref="messageBoardRef" />
  </div>
</template>

<script setup lang="jsx">
import { ref, reactive, onMounted, getCurrentInstance, watch } from 'vue'
import { ElMessage } from 'element-plus'
import MessageCount from '@/comp/message-count/index.vue'
import MessageBoard from '@/comp/message-board/index.vue'
import ReplyDialog from '@/comp/reminder-dialog/reply.vue'
import http from '@/api/http'
import viewOptions from './options.js'
import extend from '@/extension/order/message-center/reminder/index.jsx'

// 获取组件实例
const { proxy } = getCurrentInstance()
const grid = ref(null)
const messageBoardRef = ref(null)

// 从配置文件获取表格配置
const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

// 页面状态
const refreshLoading = ref(false)
const activeTab = ref('ALL')
const selectedMessageType = ref(null)

// 查询参数
const searchParams = ref({
  planTrackNo: ''
})

// 回复弹窗相关状态
const replyDialogVisible = ref(false)
const currentMessage = ref({
  id: ''
})





// Tab配置
const messageTabs = ref([
  { name: 'ALL', label: '全部消息' },
  { name: 'PO', label: '采购' },
  { name: 'SO', label: '销售' },
  { name: 'JS', label: '技术' },
  { name: 'JH', label: '计划' },
  { name: 'ZP', label: '装配' },
  { name: 'JG', label: '金工' },
  { name: 'WW', label: '委外' },
  { name: 'BJ', label: '备件'}
])

// 消息统计数据
const messageStats = ref([
  {
    label: '发送消息数',
    count: 0,
    // description: '点击查看发送历史',
    clickable: true,
    type: 'sent',
    status: 'success',
    needRefresh: false
  },
  {
    label: '待回复消息',
    count: 0,
    // description: '点击查看待处理',
    clickable: true,
    needRefresh: true,
    type: 'pending',
    status: 'warning'
  },
  {
    label: '已超期消息',
    count: 0,
    // description: '点击查看超期列表',
    clickable: true,
    type: 'overdue',
    status: 'danger'
  },
  {
    label: '已回复消息',
    count: 0,
    // description: '点击查看回复记录',
    clickable: true,
    type: 'replied',
    status: 'success'
  }
])

// 表格数据管理
let gridRef // view-grid组件实例

// view-grid组件生命周期钩子
const onInit = async ($vm) => {
  gridRef = $vm
  
  // 获取统计数据
  await fetchStatistics()

  // 添加留言板按钮
  // gridRef.buttons.push({
  //   name: '留言板',
  //   icon: 'el-icon-message',
  //   type: 'primary',
  //   onClick: function () {
  //     console.log('留言板');
  //     messageBoardRef.value.open();
  //   }
  // })

  columns.push({
    field: 'action',
    title: '操作',
    width: 100,
    align: 'center',
    fixed: 'right',
    render: (h, { row, column, index }) => {
      return (
        <div>
          <el-button type="primary" onClick={() => {
            replyMessage(row)
          }}>回复</el-button>
        </div>
      )
    }
  })
}

const onInited = async () => {
  // 组件初始化完成后，触发搜索
  setTimeout(() => {
    if (gridRef && gridRef.search) {
      gridRef.search()
    }
  }, 200)
}

const searchBefore = async (param) => {
  // 构造查询条件数组
  let whereConditions = []
  
  // 添加业务类型查询条件（根据当前tab）
  if (activeTab.value && activeTab.value !== 'ALL') {
    whereConditions.push({
      name: "BusinessType",
      value: activeTab.value,
      displayType: "="
    })
  }
  
  // 添加计划跟踪号查询条件
  if (searchParams.value.planTrackNo) {
    whereConditions.push({
      name: "PlanTraceNo", 
      value: searchParams.value.planTrackNo,
      displayType: "like"
    })
  }
  
  // 如果有查询条件，设置到参数中
  if (whereConditions.length > 0) {
    param.wheres = JSON.stringify(whereConditions)
  }
  
  return true
}

const searchAfter = async (rows, result) => {
  // 处理API返回的数据，更新统计信息
  if (result && result.total !== undefined) {
    // 根据返回的数据更新统计数据
    updateStatsFromApiResult(result)
  }
  return true
}

// 获取催单统计数据
const fetchStatistics = async () => {
  try {
    const response = await http.get('/api/OCP_UrgentOrder/GetStatistics')
    if (response && response.status) {
      const stats = response.data || {}
      
      // 更新统计数据
      messageStats.value = [
        {
          label: '发送消息数',
          count: stats.sentCount || 0,
          // description: '点击查看发送历史',
          clickable: true,
          type: 'sent',
          status: 'success',
          needRefresh: false
        },
        {
          label: '待回复消息',
          count: stats.pendingCount || 0,
          // description: '点击查看待处理',
          clickable: true,
          needRefresh: true,
          type: 'pending',
          status: 'warning'
        },
        {
          label: '已超期消息',
          count: stats.overdueCount || 0,
          // description: '点击查看超期列表',
          clickable: true,
          type: 'overdue',
          status: 'danger'
        },
        {
          label: '已回复消息',
          count: stats.repliedCount || 0,
          // description: '点击查看回复记录',
          clickable: true,
          type: 'replied',
          status: 'success'
        }
      ]
    }
  } catch (error) {
    console.error('获取统计数据失败:', error)
    ElMessage.error('获取统计数据失败')
  }
}

// 根据API结果更新统计数据
const updateStatsFromApiResult = (result) => {
  // 处理API返回的数据，不需要重复调用统计接口
  console.log('表格数据更新:', result)
}

const addBefore = async (formData) => {
  return true
}

const updateBefore = async (formData) => {
  return true
}

const rowClick = ({ row, column, event }) => {
  // 查询界面点击行事件
}

const modelOpenBefore = async (row) => {
  return true
}

const modelOpenAfter = (row) => {
  // 弹出框打开后方法
}



// Tab点击事件
const handleTabClick = async (tab) => {
  console.log('切换到tab：', tab.props?.name)
  activeTab.value = tab.props?.name || 'ALL'
  
  // 切换tab时触发搜索，会自动应用BusinessType查询条件
  if (gridRef && gridRef.search) {
    gridRef.search()
  }
}

// 回复消息
const replyMessage = (row) => {
  currentMessage.value = {
    id: row.UrgentOrderID
  }
  replyDialogVisible.value = true
}

// 处理统计项点击
const handleStatClick = ({ item, index, type }) => {
  return;
  console.log('点击了统计项：', { item, index, type })
  
  // 更新选中状态
  selectedMessageType.value = type
  
  // 根据类型执行不同的操作
  switch (type) {
    case 'sent':
      fetchSentMessages()
      break
    case 'pending':
      fetchPendingMessages()
      break
    case 'overdue':
      fetchOverdueMessages()
      break
    case 'replied':
      fetchRepliedMessages()
      break
    default:
      ElMessage.info(`点击了：${item.label}`)
  }
}

// 处理数据刷新
const handleRefresh = async (type) => {
  return;
  console.log('需要刷新数据：', type)
  
  ElMessage.info(`正在刷新 ${getDetailTitle(type)} 数据...`)
  
  try {
    await refreshStatsByType(type)
    ElMessage.success('数据刷新成功')
  } catch (error) {
    ElMessage.error('数据刷新失败')
  }
}

// 获取详情标题
const getDetailTitle = (type) => {
  const titleMap = {
    sent: '发送消息历史',
    pending: '待回复消息',
    overdue: '已超期消息',
    replied: '已回复消息'
  }
  return titleMap[type] || '消息详情'
}

// 模拟数据查询方法
const fetchSentMessages = async () => {
  try {
    await new Promise(resolve => setTimeout(resolve, 500))
    ElMessage.success('发送消息数据加载完成')
  } catch (error) {
    ElMessage.error('获取发送消息数据失败')
  }
}

const fetchPendingMessages = async () => {
  try {
    await new Promise(resolve => setTimeout(resolve, 500))
    ElMessage.success('待回复消息数据加载完成')
  } catch (error) {
    ElMessage.error('获取待回复消息数据失败')
  }
}

const fetchOverdueMessages = async () => {
  try {
    await new Promise(resolve => setTimeout(resolve, 500))
    ElMessage.warning('发现超期消息，请及时处理')
  } catch (error) {
    ElMessage.error('获取超期消息数据失败')
  }
}

const fetchRepliedMessages = async () => {
  try {
    await new Promise(resolve => setTimeout(resolve, 500))
    ElMessage.success('已回复消息数据加载完成')
  } catch (error) {
    ElMessage.error('获取已回复消息数据失败')
  }
}

// 按类型刷新统计数据
const refreshStatsByType = async (type) => {
  await new Promise(resolve => setTimeout(resolve, 1000))
  
  const targetIndex = messageStats.value.findIndex(item => item.type === type)
  if (targetIndex !== -1) {
    const randomCount = Math.floor(Math.random() * 100) + 1
    messageStats.value[targetIndex].count = randomCount
  }
}

// 刷新全部统计数据
const refreshAllStats = async () => {
  refreshLoading.value = true
  try {
    // 只触发表格搜索，会自动应用当前的查询条件
    if (gridRef && gridRef.search) {
      gridRef.search()
    }
    
  } catch (error) {
    ElMessage.error('查询失败')
  } finally {
    refreshLoading.value = false
  }
}

// 页面挂载时初始化
onMounted(() => {
  // 不在这里初始化，因为view-grid的onInit会处理初始化
})

// 对外暴露数据
defineExpose({})

// 处理回复提交
const handleReplySubmit = async (formData) => {
  try {
    // 处理回复提交逻辑
    console.log('回复提交:', formData)
    
    // 构造API请求数据
    const requestData = {
      mainData: {
        UrgentOrderID: formData.UrgentOrderID,
        ReplyContent: formData.ReplyContent || null,
        ReplyPersonName: null, // 这里可以从用户信息中获取
        ReplyPersonPhone: null, // 这里可以从用户信息中获取
        ReplyTime: null, // 服务端会自动设置当前时间
        ReplyProgress: formData.ReplyProgress || null,
        ReplyDeliveryDate: formData.ReplyDeliveryDate || null,
        Remarks: formData.Remarks || null,
        ReplyID: null // 新增时为null，服务端会自动生成
      },
      detailData: null,
      delKeys: null
    }
    
    // 调用催单回复API
    const response = await http.post('/api/OCP_UrgentOrderReply/add', requestData)
    
    if (response && response.status) {
      ElMessage.success(response.message || '回复提交成功')
      
      // 刷新表格数据
      if (gridRef && gridRef.search) {
        gridRef.search()
      }
      
      // 刷新统计数据
      await refreshStatsByType('pending')
    } else {
      throw new Error(response?.message || '提交失败')
    }
    
  } catch (error) {
    console.error('回复提交失败:', error)
    ElMessage.error(error.message || '回复提交失败，请重试')
  }
}




</script>

<style scoped>
.reminder-page {
  padding: 20px;
  margin: 0 auto;
  height: calc(100vh - 94px);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.stats-section {
  margin-bottom: 24px;
  flex-shrink: 0;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.search-controls {
  display: flex;
  align-items: center;
  gap: 12px;
}

.search-fields {
  display: flex;
  align-items: center;
}

.search-item {
  display: flex;
  align-items: center;
  margin-right: 12px;
}

.search-label {
  margin-right: 8px;
  color: var(--el-text-color-regular);
  font-size: 14px;
  white-space: nowrap;
}

.search-item :deep(.el-input) {
  height: 32px;
}

.search-item :deep(.el-input__wrapper) {
  height: 32px;
}

.section-title {
  color: var(--el-text-color-primary);
  font-size: 18px;
  font-weight: 500;
  margin: 0;
  padding-left: 8px;
  border-left: 4px solid var(--el-color-primary);
}

.message-section {
  overflow: hidden;
  height: calc(100% - 200px);
}

.tabs-header {
  flex-shrink: 0;
}

:deep(.el-tabs__content) {
  display: none;
}

.table-section {
  height: calc(100% - 40px);
  overflow: hidden;
}
</style> 

<style>
.el-tabs__header {
  margin-bottom: 0;
}
</style>