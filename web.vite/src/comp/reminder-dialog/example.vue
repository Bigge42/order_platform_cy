<template>
  <div class="batch-reminder-example">
    <h2>批量催单组件使用示例</h2>

    <div class="example-section">
      <h3>基本用法</h3>
      <el-button type="primary" @click="openBasicDialog">打开批量催单弹窗</el-button>
    </div>

    <div class="example-section">
      <h3>禁用接口调用模式</h3>
      <el-button type="info" @click="openDataOnlyDialog">仅数据收集模式</el-button>
    </div>

    <div class="example-section">
      <h3>自定义业务类型</h3>
      <el-button type="success" @click="openCustomBusinessTypeDialog">自定义业务类型</el-button>
    </div>

    <!-- 批量催单弹窗 -->
    <BatchReminderDialog
      v-model="dialogVisible"
      :data="selectedData"
      :business-type="businessType"
      :title="dialogTitle"
      :width="dialogWidth"
      :enable-api-call="enableApiCall"
      @confirm="handleConfirm"
      @cancel="handleCancel"
    />

    <!-- 结果展示 -->
    <div v-if="lastResult" class="result-section">
      <h3>最后一次操作结果</h3>
      <pre>{{ JSON.stringify(lastResult, null, 2) }}</pre>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import BatchReminderDialog from './batch.vue'

// 响应式状态
const dialogVisible = ref(false)
const selectedData = ref([])
const businessType = ref('JS')
const dialogTitle = ref('批量催单')
const dialogWidth = ref('80%')
const enableApiCall = ref(true)
const lastResult = ref(null)

// 模拟数据
const mockData = [
  {
    id: '1',
    TechID: 'T001',
    billNo: 'WW202501001',
    seq: '1',
    DepartmentName: '技术部',
    MaterialNumber: 'M001',
    SOBillNo: 'SO001'
  },
  {
    id: '2',
    TechID: 'T002',
    billNo: 'WW202501002',
    seq: '2',
    DepartmentName: '生产部',
    MaterialNumber: 'M002',
    SOBillNo: 'SO002'
  },
  {
    id: '3',
    TechID: 'T003',
    billNo: 'WW202501003',
    seq: '3',
    DepartmentName: '质量部',
    MaterialNumber: 'M003',
    SOBillNo: 'SO003'
  }
]

// 事件处理
const openBasicDialog = () => {
  selectedData.value = mockData
  businessType.value = 'JS'
  dialogTitle.value = '批量催单'
  enableApiCall.value = true
  dialogVisible.value = true
}

const openDataOnlyDialog = () => {
  selectedData.value = mockData
  businessType.value = 'JS'
  dialogTitle.value = '批量催单（仅数据收集）'
  enableApiCall.value = false
  dialogVisible.value = true
}

const openCustomBusinessTypeDialog = () => {
  selectedData.value = mockData
  businessType.value = 'CG' // 采购业务类型
  dialogTitle.value = '批量催单（采购）'
  enableApiCall.value = true
  dialogVisible.value = true
}

const handleConfirm = (result) => {
  console.log('批量催单确认:', result)
  lastResult.value = result

  // 显示结果信息
  if (result.apiResult) {
    console.log('API调用结果:', result.apiResult)
  }
}

const handleCancel = () => {
  console.log('批量催单取消')
  lastResult.value = null
}
</script>

<style scoped>
.batch-reminder-example {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.example-section {
  margin-bottom: 20px;
  padding: 16px;
  border: 1px solid #ebeef5;
  border-radius: 4px;
}

.example-section h3 {
  margin-top: 0;
  color: #303133;
}

.result-section {
  margin-top: 20px;
  padding: 16px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.result-section pre {
  background-color: #ffffff;
  padding: 12px;
  border-radius: 4px;
  overflow-x: auto;
  font-size: 12px;
}
</style>
