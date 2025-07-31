<!-- 缺料运算看板 -->
<template>
  <div class="mtrl-shortage-dashboard">
    <!-- 筛选器组件 -->
    <DashboardTopFilter
      v-model="filterData"
      year-label="统计年份"
      month-label="统计月份"
      week-label="统计周"
      @change="handleFilterChange"
    />
    
    <!-- 数据汇总组件 -->
    <DashboardDataSummary
      :summary-data="summaryData"
    />
    
    <!-- 查询条件按钮组 -->
    <div class="query-conditions">
      <!-- 第一部分：外购/自制 -->
      <div class="condition-group">
        <div class="condition-label">采购类型：</div>
        <ButtonGroupSelect
          v-model="selectedPurchaseTypes"
          :options="purchaseTypes"
          mode="multiple"
          @change="handlePurchaseTypeChange"
        />
      </div>
      
      <!-- 第二部分：产品类型 -->
      <div class="condition-group">
        <div class="condition-label">产品类型：</div>
        <ButtonGroupSelect
          v-model="selectedProductTypes"
          :options="productTypes"
          mode="multiple"
          @change="handleProductTypeChange"
        />
      </div>
    </div>
    
    <!-- 看板内容 -->
    <div class="dashboard-content">
      <div class="table-container">
        <!-- 缺料情况分析表格 -->
        <div class="table-wrapper">
          <div class="table-header">缺料情况分析</div>
          <el-table
            :data="materialShortageDataWithTotal"
            border
            stripe
          >
            <el-table-column prop="materialName" label="物料名称" min-width="150" fixed="left" />
            <el-table-column prop="demandQty" label="需求数" min-width="100" align="right" />
            <el-table-column prop="plannedQty" label="已下计划" min-width="100" align="right" />
            <el-table-column prop="inventoryQty" label="库存" min-width="100" align="right" />
            <el-table-column prop="unplannedQty" label="未下计划" min-width="100" align="right" />
            <el-table-column prop="purchaseQty" label="采购数量" min-width="100" align="right" />
            <el-table-column prop="notInStockQty" label="未入库数量" min-width="120" align="right" />
          </el-table>
        </div>

        <!-- 送货情况分析表格 -->
        <div class="table-wrapper">
          <div class="table-header">送货情况分析</div>
          <el-table
            :data="deliveryAnalysisDataWithTotal"
            border
            stripe
          >
            <el-table-column prop="supplierName" label="供应商/车间" min-width="150" fixed="left" />
            <el-table-column prop="notInStockQty" label="未入库数量" min-width="120" align="right" />
            <el-table-column prop="overdueQty" label="已超期数量" min-width="120" align="right" />
            <el-table-column prop="notInStockRatio" label="未入库比" min-width="100" align="right">
              <template #default="{ row }">
                <span v-if="row.supplierName === '总计'">--</span>
                <span v-else>{{ row.notInStockRatio }}%</span>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import DashboardTopFilter from '@/comp/dashboard-top-filter/index.vue'
import DashboardDataSummary from '@/comp/dashboard-data-summary/index.vue'
import ButtonGroupSelect from '@/comp/button-group-select/index.vue'

// 筛选条件数据
const filterData = ref({ 
  year: '', 
  month: null, 
  weeks: [] 
})

// 采购类型选项
const purchaseTypes = ref([
  { label: '外购', value: 'external' },
  { label: '自制', value: 'internal' }
])

// 产品类型选项
const productTypes = ref([
  { label: '毛坯', value: 'blank' },
  { label: '阀体', value: 'valve_body' },
  { label: '阀体部件', value: 'valve_parts' },
  { label: '管道附件', value: 'pipe_fittings' },
  { label: '执行机构', value: 'actuator' },
  { label: '附件', value: 'accessories' }
])

// 选中的采购类型
const selectedPurchaseTypes = ref([])

// 选中的产品类型
const selectedProductTypes = ref([])

// 数据汇总数据
const summaryData = ref([
  {
    label: '总任务订单',
    total: 16,
    unit: '个任务单',
    items: [
      {
        count: 12,
        label: '已在订单单',
        percentage: 75,
        description: '开始单单',
        status: 'success'
      },
      {
        count: 4,
        label: '未开始订单',
        percentage: 25,
        description: '未开始单单',
        status: 'warning'
      }
    ]
  },
  {
    label: '总任务套数',
    total: 891,
    unit: '个套数',
    items: [
      {
        count: 860,
        label: '已在套数',
        percentage: 97,
        description: '开始单单',
        status: 'success'
      },
      {
        count: 31,
        label: '未开始套数',
        percentage: 3,
        description: '未开始单单',
        status: 'warning'
      }
    ]
  }
])

// 缺料情况分析数据
const materialShortageData = ref([
  {
    materialName: '钢材A',
    demandQty: 1000,
    plannedQty: 800,
    inventoryQty: 150,
    unplannedQty: 200,
    purchaseQty: 650,
    notInStockQty: 100
  },
  {
    materialName: '铸件B',
    demandQty: 500,
    plannedQty: 400,
    inventoryQty: 80,
    unplannedQty: 100,
    purchaseQty: 320,
    notInStockQty: 50
  },
  {
    materialName: '阀体C',
    demandQty: 300,
    plannedQty: 250,
    inventoryQty: 40,
    unplannedQty: 50,
    purchaseQty: 210,
    notInStockQty: 30
  },
  {
    materialName: '密封件D',
    demandQty: 1200,
    plannedQty: 1000,
    inventoryQty: 200,
    unplannedQty: 200,
    purchaseQty: 800,
    notInStockQty: 120
  }
])

// 送货情况分析数据
const deliveryAnalysisData = ref([
  {
    supplierName: '供应商A',
    notInStockQty: 100,
    overdueQty: 20,
    notInStockRatio: 15.5
  },
  {
    supplierName: '供应商B',
    notInStockQty: 50,
    overdueQty: 10,
    notInStockRatio: 8.2
  },
  {
    supplierName: '车间一',
    notInStockQty: 30,
    overdueQty: 5,
    notInStockRatio: 6.8
  },
  {
    supplierName: '车间二',
    notInStockQty: 120,
    overdueQty: 35,
    notInStockRatio: 18.3
  }
])

// 缺料情况分析数据（包含总计行）
const materialShortageDataWithTotal = computed(() => {
  const data = [...materialShortageData.value]
  
  // 计算总计行
  const totalRow = {
    materialName: '总计',
    demandQty: data.reduce((sum, item) => sum + item.demandQty, 0),
    plannedQty: data.reduce((sum, item) => sum + item.plannedQty, 0),
    inventoryQty: data.reduce((sum, item) => sum + item.inventoryQty, 0),
    unplannedQty: data.reduce((sum, item) => sum + item.unplannedQty, 0),
    purchaseQty: data.reduce((sum, item) => sum + item.purchaseQty, 0),
    notInStockQty: data.reduce((sum, item) => sum + item.notInStockQty, 0)
  }
  
  data.push(totalRow)
  return data
})

// 送货情况分析数据（包含总计行）
const deliveryAnalysisDataWithTotal = computed(() => {
  const data = [...deliveryAnalysisData.value]
  
  // 计算总计行
  const totalRow = {
    supplierName: '总计',
    notInStockQty: data.reduce((sum, item) => sum + item.notInStockQty, 0),
    overdueQty: data.reduce((sum, item) => sum + item.overdueQty, 0),
    notInStockRatio: 0 // 总计行不计算比率
  }
  
  data.push(totalRow)
  return data
})

// 处理采购类型变化
const handlePurchaseTypeChange = (types) => {
  console.log('采购类型变化:', types)
  handleQueryConditionChange()
}

// 处理产品类型变化
const handleProductTypeChange = (types) => {
  console.log('产品类型变化:', types)
  handleQueryConditionChange()
}

// 处理查询条件变化
const handleQueryConditionChange = () => {
  console.log('查询条件变化:', {
    purchaseTypes: selectedPurchaseTypes.value,
    productTypes: selectedProductTypes.value
  })
  
  // 这里可以根据查询条件调用API获取数据
  if (filterData.value.year && filterData.value.month) {
    fetchDashboardData({
      ...filterData.value,
      purchaseTypes: selectedPurchaseTypes.value,
      productTypes: selectedProductTypes.value
    })
  }
}

// 处理筛选条件变化
const handleFilterChange = (newFilter) => {
  console.log('筛选条件变化:', newFilter)
  
  // 这里可以根据筛选条件调用API获取数据
  if (newFilter.year && newFilter.month) {
    fetchDashboardData({
      ...newFilter,
      purchaseTypes: selectedPurchaseTypes.value,
      productTypes: selectedProductTypes.value
    })
  }
}

// 获取看板数据
const fetchDashboardData = (filter) => {
  console.log(`获取${filter.year}年${filter.month}月的缺料数据`)
  if (filter.weeks.length > 0) {
    console.log(`筛选第${filter.weeks.join(',')}周的数据`)
  }
  if (filter.purchaseTypes.length > 0) {
    console.log(`筛选采购类型: ${filter.purchaseTypes.join(',')}`)
  }
  if (filter.productTypes.length > 0) {
    console.log(`筛选产品类型: ${filter.productTypes.join(',')}`)
  }
}
</script>

<style scoped>
.mtrl-shortage-dashboard {
  padding: 20px;
}

.query-conditions {
  background-color: #fff;
  padding: 15px;
  border-radius: 5px;
  margin-bottom: 20px;
  display: flex;
  gap: 40px;
  align-items: center;
  flex-wrap: wrap;
}

.condition-group {
  display: flex;
  align-items: center;
}

.condition-label {
  margin-right: 15px;
  white-space: nowrap;
  font-size: 14px;
  color: #333;
  min-width: 80px;
}



.dashboard-content {
  margin-top: 20px;
}

.table-container {
  display: flex;
  gap: 20px;
  margin-top: 20px;
}

.table-wrapper {
  flex: 1;
  background-color: #fff;
  border-radius: 5px;
  overflow: hidden;
}

.table-header {
  background-color: #f5f7fa;
  padding: 15px 20px;
  font-size: 16px;
  font-weight: 600;
  color: #333;
  border-bottom: 1px solid #e4e7ed;
}

/* 表格撑满宽度 */
:deep(.el-table) {
  width: 100%;
  table-layout: fixed;
}

:deep(.el-table .el-table__header-wrapper),
:deep(.el-table .el-table__body-wrapper) {
  width: 100% !important;
}

/* 总计行样式 */
:deep(.el-table__row:last-child) {
  background-color: #f0f2f5 !important;
  font-weight: 600;
  color: #333;
}

:deep(.el-table__row:last-child td) {
  background-color: #f0f2f5 !important;
}

.filter-info {
  background-color: #f5f5f5;
  padding: 15px;
  border-radius: 5px;
  margin: 20px 0;
}

.filter-info p {
  margin: 5px 0;
  font-size: 14px;
}

.filter-info p:first-child {
  font-weight: bold;
  margin-bottom: 10px;
}

/* 深色主题适配 */
.vol-theme-dark-aside {
  .query-conditions {
    background-color: #021d37;
  }
  
  .condition-label {
    color: #fff;
  }
  
  .table-wrapper {
    background-color: #021d37;
  }
  
  .table-header {
    background-color: #1a3a58;
    color: #fff;
    border-bottom-color: #2c5f8a;
  }
  
  :deep(.el-table__row:last-child) {
    background-color: #1a3a58 !important;
    color: #fff;
  }
  
  :deep(.el-table__row:last-child td) {
    background-color: #1a3a58 !important;
  }
}

/* 响应式设计 */
@media (max-width: 768px) {
  .query-conditions {
    flex-direction: column;
    gap: 15px;
    align-items: flex-start;
  }
  
  .condition-group {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
    width: 100%;
  }
  
  .condition-label {
    min-width: auto;
    margin-bottom: 5px;
  }
  
  .table-container {
    flex-direction: column;
    gap: 15px;
  }
  
  .table-wrapper {
    min-width: 100%;
  }
}
</style>