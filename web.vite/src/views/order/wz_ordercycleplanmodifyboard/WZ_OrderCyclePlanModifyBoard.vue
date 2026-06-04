<template>
  <view-grid ref="grid"
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
             :modelOpenAfter="modelOpenAfter">
    <template #btnLeft>
      <div class="wz-planmodify-action">
        <el-button type="info"
                   size="small"
                   :loading="refreshLoading"
                   @click="handleRefreshOrders">刷新订单数据</el-button>
      </div>
    </template>
  </view-grid>
</template>

<script setup lang="jsx">
import extend from '@/extension/order/wz_ordercycleplanmodifyboard/WZ_OrderCyclePlanModifyBoard.jsx'
import viewOptions from './WZ_OrderCyclePlanModifyBoard/options.js'
import { ref, reactive, getCurrentInstance } from 'vue'
import { ElMessage } from 'element-plus'

const grid = ref(null)
const { proxy } = getCurrentInstance()
const {
  table,
  editFormFields,
  editFormOptions,
  searchFormFields,
  searchFormOptions,
  columns,
  detail,
  details
} = reactive(viewOptions())

const refreshLoading = ref(false)
let gridRef

const onInit = async ($vm) => {
  gridRef = $vm
  gridRef.queryFields = ['SalesOrderNo', 'PlanTrackingNo']
}

const onInited = async () => {}

const searchBefore = async (param) => {
  return true
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

const rowClick = ({ row, column, event }) => {}

const modelOpenBefore = async (row) => {
  return true
}

const modelOpenAfter = (row) => {}

const refreshGrid = () => {
  if (gridRef && gridRef.search) {
    gridRef.search()
  }
}

const handleRefreshOrders = async () => {
  if (refreshLoading.value) {
    return
  }

  refreshLoading.value = true
  try {
    const response = await proxy.http.post('/api/WZ_OrderCycleBase/refresh-order-tracking', {}, true)
    if (response?.status === false) {
      ElMessage.error(response.message || '订单数据刷新失败')
      return
    }

    ElMessage.success(response?.message || '订单数据刷新完成')
    refreshGrid()
  } catch (error) {
    ElMessage.error('订单数据刷新异常')
  } finally {
    refreshLoading.value = false
  }
}

defineExpose({})
</script>

<style scoped>
.wz-planmodify-action {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  background-color: #fff;
  border-radius: 4px;
}
</style>
