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
  gridRef.queryFields = ['SOBillNo', 'MtoNo']
}

const onInited = async () => {
  const exportButton = gridRef?.buttons?.find((button) => button.value === 'Export')
  if (exportButton) {
    exportButton.onClick = handleExport
  }
}

const searchBefore = async (param) => {
  param.value = 'planModifyBoardFullOrders'
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

const getVisibleExportColumns = (columnList) => {
  const exportColumns = []
  columnList.forEach((column) => {
    if (column.hidden || column.render) {
      return
    }
    if (column.children?.length) {
      exportColumns.push(...getVisibleExportColumns(column.children))
      return
    }
    if (!column.field) {
      return
    }
    exportColumns.push({
      field: column.field,
      title: column.title || column.field,
      width: column.width,
      type: column.type
    })
  })
  return exportColumns
}

const handleExport = async () => {
  const exportColumns = getVisibleExportColumns(columns)
  if (!exportColumns.length) {
    ElMessage.warning('没有可导出的显示列')
    return
  }

  const wheres = proxy.base.getSearchParameters(gridRef, searchFormFields, searchFormOptions) || []
  const param = {
    order: gridRef.$refs.table.paginations.order,
    sort: gridRef.$refs.table.paginations.sort,
    wheres,
    value: 'planModifyBoardFullOrders',
    columns: exportColumns.map((column) => column.field),
    customerParams: {
      planModifyBoardColumns: JSON.stringify(exportColumns)
    }
  }

  if (!param.wheres.some((where) => where.name === table.key)) {
    const ids = gridRef
      .getSelectRows()
      .map((row) => row[table.key])
      .join(',')
    if (ids) {
      param.wheres.push({
        name: table.key,
        value: ids,
        displayType: 'selectList'
      })
    }
  }

  param.wheres = JSON.stringify(param.wheres)
  proxy.http.download(
    '/api/OCP_OrderTracking/PlanModifyBoardExport',
    param,
    `${table.cnName}.xlsx`,
    'loading....'
  )
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
