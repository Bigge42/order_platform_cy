<!-- 整机装配页面 -->
<template>
  <div class="machine-assembly-page">
    <!-- 顶部筛选组件 -->
    <TopFilter
      v-model="filterParams"
      :orders="orderOptions"
      :years="yearOptions"
      :months="monthOptions"
      :order-label="'生产单据号'"
      :year-label="'回复交付期(年份)'"
      :month-label="'回复交付期(月份)'"
      @change="handleFilterChange"
    />

    <!-- 装配统计组件 -->
    <OrderCount
      :order-data="assemblyStats"
    />

    <div class="assembly-reminders">
      <!-- 表格区域 -->
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
        />
      </div>
    </div>

    <!-- 留言板组件 -->
    <MessageBoard ref="messageBoardRef" />
    
    <!-- 催单弹窗组件 -->
    <FollowUpReminder
      v-model="followUpReminderVisible"
      :data="followUpReminderData"
      @send="handleFollowUpSend"
    />
  </div>
</template>

<script setup lang="jsx">
import { ref, reactive, onMounted, getCurrentInstance } from 'vue'
import { ElMessage } from 'element-plus'
import TopFilter from '@/comp/top-filter/index.vue'
import OrderCount from '@/comp/order-count/index.vue'
import MessageBoard from '@/comp/message-board/index.vue'
import FollowUpReminder from '@/comp/follow-up-reminder/index.vue'
import viewOptions from './options.js'

// 获取组件实例
const { proxy } = getCurrentInstance()
const grid = ref(null)
const messageBoardRef = ref(null)

// 从配置文件获取表格配置
const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

// 筛选参数
const filterParams = ref({
  order: '所有',
  year: '2025',
  month: 4
})

// 销售单据号选项
const orderOptions = ref([
  { label: '所有', value: '所有' },
  { label: 'SC202510001', value: 'SC202510001' },
  { label: 'SC202520002', value: 'SC202520002' },
  { label: 'SC202530003', value: 'SC202530003' },
  { label: 'SC202540004', value: 'SC202540004' },
  { label: 'SC202550005', value: 'SC202550005' }
])

// 年份选项
const yearOptions = ref([
  { label: '2024', value: '2024' },
  { label: '2025', value: '2025' },
  { label: '2026', value: '2026' }
])

// 月份选项
const monthOptions = ref([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12])

// 装配统计数据
const assemblyStats = ref({
  totalCount: 150,           // 发送消息数
  pendingCount: 50,          // 待回复消息
  overdueCount: 8,           // 已超期消息
  overdueCompletedCount: 92  // 已回复消息
})

// 扩展配置
const extend = reactive({})

// 表格数据管理
const tableData = ref({})
let gridRef // view-grid组件实例

// 催单弹窗相关
const followUpReminderVisible = ref(false)
const followUpReminderData = ref([])

// 模拟表格数据生成函数
const generateAssemblyData = () => {
  const data = []
  const urgencyLevels = ['高', '中', '低', '紧急']
  const statusOptions = ['正常', '延期', '超期', '完成']
  
  for (let i = 1; i <= 115; i++) {
    const currentDate = new Date(Date.now() - Math.random() * 30 * 24 * 60 * 60 * 1000)
    const standardDate = new Date(currentDate.getTime() + Math.random() * 15 * 24 * 60 * 60 * 1000)
    const actualDate = Math.random() > 0.3 ? new Date(standardDate.getTime() + (Math.random() - 0.5) * 10 * 24 * 60 * 60 * 1000) : null
    
    // 计算超期天数
    let overdueDays = 0
    if (actualDate) {
      overdueDays = Math.floor((actualDate - standardDate) / (1000 * 60 * 60 * 24))
    } else {
      // 如果还未完成，计算当前距离标准完工日期的天数
      overdueDays = Math.floor((new Date() - standardDate) / (1000 * 60 * 60 * 24))
    }
    
    const plannedQuantity = Math.floor(Math.random() * 100) + 50
    const completedQuantity = Math.floor(plannedQuantity * (0.3 + Math.random() * 0.7))
    const overdueQuantity = Math.floor(Math.random() * 20)
    const pendingQuantity = plannedQuantity - completedQuantity
    
    // 根据数据判断状态
    let status = '正常'
    if (overdueDays > 0) {
      status = '超期'
    } else if (overdueDays > -3 && pendingQuantity > 0) {
      status = '延期'
    } else if (pendingQuantity === 0) {
      status = '完成'
    }
    
    // 随机选择紧急等级
    const urgencyLevel = urgencyLevels[Math.floor(Math.random() * urgencyLevels.length)]
    
    const rowData = {
      id: i,
      productionNo: `SC${currentDate.getFullYear()}${String(Math.floor(Math.random() * 9000) + 1000)}${String(i).padStart(3, '0')}`,
      urgencyLevel: urgencyLevel,
      orderCount: Math.floor(Math.random() * 10) + 1,
      plannedQuantity: plannedQuantity,
      completedQuantity: completedQuantity,
      overdueQuantity: overdueQuantity,
      pendingQuantity: pendingQuantity,
      statusJudgment: status,
      standardCompletionDate: standardDate.toLocaleDateString(),
      actualCompletionDate: actualDate ? actualDate.toLocaleDateString() : '',
      overdueDays: overdueDays
    }
    
    data.push(rowData)
  }
  
  console.log('生成数据完成，数据量:', data.length)
  if (data.length > 0) {
    console.log('第一条数据:', data[0])
  }
  
  return data
}

// view-grid生命周期钩子
const onInit = async ($vm) => {
  gridRef = $vm

  gridRef.setFixedSearchForm(true);
  // 初始化数据
  await initializeData()
  
  console.log('装配页面初始化')
}

const onInited = async () => {
  console.log('装配页面初始化完成')
  
  // 重新初始化数据确保最新
  initTableData()
  
  // 组件初始化完成后，直接设置数据
  setTimeout(() => {
    loadTabData('assembly')
  }, 200) // 稍微延长等待时间确保组件完全初始化
  
  // 设置列的渲染函数
  columns.forEach(col => {
    // 紧急等级列
    if (col.field === 'urgencyLevel') {
      col.render = (h, { row, column, index }) => {
        const cellValue = row.urgencyLevel
        const levelMap = {
          '高': 'danger',
          '中': 'warning', 
          '低': 'success',
          '紧急': 'danger'
        }
        const type = levelMap[cellValue] || 'info'
        return <el-tag type={type}>{cellValue}</el-tag>
      }
    }
    
    // 超期数量列
    if (col.field === 'overdueQuantity') {
      col.render = (h, { row, column, index }) => {
        const cellValue = row.overdueQuantity
        return cellValue > 0 ? <span style={{ color: '#F56C6C' }}>{cellValue}</span> : cellValue
      }
    }
    
    // 状态判断列
    if (col.field === 'statusJudgment') {
      col.render = (h, { row, column, index }) => {
        const cellValue = row.statusJudgment
        const statusMap = {
          '正常': 'success',
          '延期': 'warning', 
          '超期': 'danger',
          '完成': 'info'
        }
        const type = statusMap[cellValue] || 'default'
        return <el-tag type={type}>{cellValue}</el-tag>
      }
    }
    
    // 超期天数列
    if (col.field === 'overdueDays') {
      col.render = (h, { row, column, index }) => {
        const cellValue = row.overdueDays
        if (cellValue > 0) {
          return <span style={{ color: '#F56C6C' }}>{cellValue}天</span>
        } else if (cellValue < 0) {
          return <span style={{ color: '#67C23A' }}>提前{Math.abs(cellValue)}天</span>
        } else {
          return <span style={{ color: '#909399' }}>按时完成</span>
        }
      }
    }
    
    // 操作列
    if (col.field === 'action') {
      col.render = (h, { row, column, index }) => {
        return (
          <div style={{ display: 'flex', gap: '6px', alignItems: 'center', justifyContent: 'center' }}>
            <el-button 
              type="warning" 
              size="small" 
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
            
            <el-dropdown
              trigger="click"
              v-slots={{
                dropdown: () => (
                  <el-dropdown-menu>
                    <el-dropdown-item>
                      <div
                        onClick={($e) => {
                          dropdownClick('留言', row, column, index, $e);
                        }}
                      >
                        留言
                      </div>
                    </el-dropdown-item>
                    <el-dropdown-item>
                      <div
                        onClick={($e) => {
                          dropdownClick('留言板', row, column, index, $e);
                        }}
                      >
                        留言板
                      </div>
                    </el-dropdown-item>
                  </el-dropdown-menu>
                )
              }}
            >
              <span style={{ fontSize: '13px', color: '#409eff', cursor: 'pointer' }} class="el-dropdown-link">
                更多<i class="el-icon-arrow-down" style={{ marginLeft: '4px' }}></i>
              </span>
            </el-dropdown>
          </div>
        )
      }
    }
  })
}

const searchBefore = async (param) => {
  // 使用本地数据，阻止后台查询
  return false
}

const searchAfter = async (rows, result) => {
  return true
}

const addBefore = async (formData) => {
  return true
}

const updateBefore = async (formData) => {
  return true
}

const rowClick = ({ row, column, event }) => {
  console.log('点击行:', row)
}

const modelOpenBefore = async (row) => {
  return true
}

const modelOpenAfter = (row) => {
  console.log('弹窗打开后:', row)
}

// 筛选条件变化处理
const handleFilterChange = (values) => {
  console.log('筛选条件变化:', values)
  filterParams.value = values
  
  // 根据筛选条件重新加载数据
  loadFilteredData(values)
}

// 根据筛选条件加载数据
const loadFilteredData = (filters) => {
  try {
    let filteredData = generateAssemblyData()
    
    // 根据销售单据号筛选
    if (filters.order && filters.order !== '所有') {
      filteredData = filteredData.filter(item => 
        item.productionNo.includes(filters.order)
      )
    }
    
    // 根据年份和月份筛选
    if (filters.year && filters.month) {
      filteredData = filteredData.filter(item => {
        const itemDate = new Date(item.standardCompletionDate)
        return itemDate.getFullYear() === parseInt(filters.year) && 
               itemDate.getMonth() + 1 === filters.month
      })
    }
    
    // 更新表格数据
    const tableInstance = grid.value?.getTable?.(true)
    if (tableInstance && tableInstance.rowData !== undefined) {
      tableInstance.rowData.splice(0)
      tableInstance.rowData.push(...filteredData)
      
      if (tableInstance.paginations) {
        tableInstance.paginations.total = filteredData.length
      }
    }
    
    // 更新统计数据
    updateStatsBasedOnFilter(filteredData)
    
  } catch (error) {
    console.error('筛选数据失败:', error)
    ElMessage.error('数据筛选失败')
  }
}

// 根据筛选结果更新统计
const updateStatsBasedOnFilter = (filteredData) => {
  const total = filteredData.length
  assemblyStats.value = {
    totalCount: total,
    pendingCount: Math.floor(total * 0.3),
    overdueCount: Math.floor(total * 0.05),
    overdueCompletedCount: Math.floor(total * 0.6)
  }
}

// 初始化表格数据
const initTableData = () => {
  const mockData = generateAssemblyData()
  tableData.value['assembly'] = mockData
}

// 加载指定tab的数据
const loadTabData = async (tabName = 'assembly') => {
  try {
    const currentData = tableData.value[tabName] || []
    
    // 添加调试信息
    console.log('准备加载数据，数据量:', currentData.length)
    if (currentData.length > 0) {
      console.log('第一条数据示例:', currentData[0])
    }
    
    // 直接操作表格数据
    const tableInstance = grid.value?.getTable?.(true)
    
    if (tableInstance && tableInstance.rowData !== undefined) {
      // 直接操作rowData（这是VolTable的数据数组）
      tableInstance.rowData.splice(0) // 清空
      tableInstance.rowData.push(...currentData) // 添加新数据
      
      // 设置分页信息
      if (tableInstance.paginations) {
        tableInstance.paginations.total = currentData.length
      }
      
      console.log('成功设置表格数据:', currentData.length, '条')
      console.log('表格当前数据:', tableInstance.rowData.slice(0, 2)) // 显示前两条数据用于调试
      return
    }
    
    // 如果方法1失败，尝试触发搜索
    if (grid.value?.search) {
      console.log('使用搜索方法加载数据')
      grid.value.search()
    }
  } catch (error) {
    console.error('设置表格数据失败:', error)
  }
}

// 初始化数据
const initializeData = async () => {
  try {
    // 初始化表格数据
    initTableData()
    
    await new Promise(resolve => setTimeout(resolve, 500))
    console.log('装配页面数据初始化完成')
  } catch (error) {
    ElMessage.error('数据初始化失败')
  }
}

// 催单操作
const handleUrge = (row) => {
  console.log('催单操作:', row)
  
  // 准备催单数据 - 将当前行数据转换为催单组件需要的格式
  const reminderData = [{
    id: row.id,
    contractNo: row.productionNo, // 使用生产单据号作为合同号
    planTrackNo: `PT${row.productionNo.slice(-6)}`, // 生成计划跟踪号
    business: 'production', // 默认业务类型为生产
    defaultResponsible: '生产部门', // 默认负责人
    assignedPersons: [], // 指定负责人，初始为空
    urgentLevel: row.urgencyLevel === '紧急' ? 'urgent' : row.urgencyLevel === '高' ? 'A' : row.urgencyLevel === '中' ? 'B' : 'C', // 根据紧急等级映射
    replyTime: 24, // 默认回复时间
    replyTimeUnit: 'hour', // 默认单位为小时
    messageContent: '' // 消息内容初始为空
  }]
  
  // 设置数据并打开弹窗
  followUpReminderData.value = reminderData
  followUpReminderVisible.value = true
}

// 处理催单发送
const handleFollowUpSend = (selectedData) => {
  console.log('发送催单数据:', selectedData)
  
  // 这里可以调用API发送催单消息
  // 示例：await sendFollowUpReminder(selectedData)
  
  ElMessage.success(`成功发送 ${selectedData.length} 条催单消息`)
  
  // 关闭弹窗
  followUpReminderVisible.value = false
}

// 协商操作
const handleNegotiate = (row) => {
  console.log('协商操作:', row)
  ElMessage.success(`对生产单据号 ${row.productionNo} 进行协商操作`)
}

// 留言操作
const handleMessage = (row) => {
  console.log('留言操作:', row)
  ElMessage.success(`对生产单据号 ${row.productionNo} 进行留言操作`)
}

// 留言板操作
const handleMessageBoard = (row) => {
  console.log('留言板操作:', row)
  // 打开留言板抽屉
  if (messageBoardRef.value) {
    messageBoardRef.value.open()
  }
  ElMessage.success(`查看生产单据号 ${row.productionNo} 的留言板`)
}

// 刷新数据
const refreshData = () => {
  loadTabData('assembly')
}

// 页面挂载时初始化
onMounted(() => {
  console.log('机器装配页面挂载')
  
  // 测试数据生成
  const testData = generateAssemblyData()
  console.log('测试生成的数据:', testData.slice(0, 3))
})

// 下拉菜单点击处理
const dropdownClick = (value, row, column, index, $e) => {
  switch(value) {
    case '留言':
      handleMessage(row)
      break
    case '留言板':
      handleMessageBoard(row)
      break
    default:
      console.log('未知的下拉操作:', value)
  }
}

// 暴露组件方法
defineExpose({
  refreshData: refreshData,
  filterData: loadFilteredData,
  openFollowUpReminder: (rowData) => {
    if (rowData) {
      handleUrge(rowData)
    } else {
      followUpReminderVisible.value = true
    }
  },
  closeFollowUpReminder: () => {
    followUpReminderVisible.value = false
  }
})
</script>

<style scoped>
.machine-assembly-page {
  padding: 20px;
  height: calc(100vh - 94px);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.assembly-reminders {
  flex: 1;
  overflow: hidden;
}

.table-section {
  height: 100%;
  overflow: hidden;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .machine-assembly-page {
    padding: 16px;
  }
}

@media (max-width: 768px) {
  .machine-assembly-page {
    padding: 12px;
  }
}
</style>