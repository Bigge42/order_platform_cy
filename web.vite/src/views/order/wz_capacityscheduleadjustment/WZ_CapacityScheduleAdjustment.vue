<template>
  <div class="capacity-adjustment-page">
    <section class="toolbar">
      <div class="title-block">
        <div class="page-title">异常排产调整工作台</div>
        <div class="page-subtitle">排产优化日期异常单独调整</div>
      </div>

      <div class="filters">
        <el-input
          v-model="query.keyword"
          clearable
          class="filter-keyword"
          placeholder="订单号 / 跟踪号 / 物料 / 规格"
          @keyup.enter="handleSearch"
        >
          <template #prefix>
            <el-icon><Search /></el-icon>
          </template>
        </el-input>
        <el-select v-model="query.abnormalType" class="filter-select" placeholder="异常类型">
          <el-option label="全部异常" value="all" />
          <el-option label="超 120%" value="overThreshold" />
          <el-option label="法定节假日" value="holiday" />
        </el-select>
        <el-select
          v-model="query.valveCategory"
          class="filter-select"
          clearable
          filterable
          placeholder="阀门类别"
        >
          <el-option v-for="item in valveCategories" :key="item" :label="item" :value="item" />
        </el-select>
        <el-select
          v-model="query.productionLine"
          class="filter-select"
          clearable
          filterable
          placeholder="产线"
        >
          <el-option v-for="item in productionLines" :key="item" :label="item" :value="item" />
        </el-select>
        <el-button type="primary" :icon="Search" :loading="listLoading" @click="handleSearch">
          查询
        </el-button>
        <el-button :icon="Refresh" :loading="listLoading || windowLoading" @click="reloadCurrent">
          刷新
        </el-button>
      </div>
    </section>

    <section class="summary-strip">
      <div class="summary-item">
        <span class="summary-label">异常总数</span>
        <strong>{{ pager.total }}</strong>
      </div>
      <div class="summary-item danger">
        <span class="summary-label">超 120%</span>
        <strong>{{ summary.overThresholdCount }}</strong>
      </div>
      <div class="summary-item warning">
        <span class="summary-label">法定节假日</span>
        <strong>{{ summary.holidayCount }}</strong>
      </div>
      <div class="summary-item">
        <span class="summary-label">当前产线</span>
        <strong>{{ currentLineText }}</strong>
      </div>
    </section>

    <main class="workbench">
      <section class="orders-panel">
        <div class="panel-header">
          <div>
            <div class="panel-title">异常订单</div>
            <div class="panel-meta">{{ pager.page }} / {{ pageCount }} 页</div>
          </div>
          <el-tag size="small" effect="plain">{{ pager.rows }} 条/页</el-tag>
        </div>

        <el-table
          v-loading="listLoading"
          :data="orders"
          height="calc(100vh - 318px)"
          border
          highlight-current-row
          :row-class-name="orderRowClass"
          empty-text="暂无异常排产数据"
          @row-click="handleRowClick"
        >
          <el-table-column label="异常" width="106" fixed>
            <template #default="{ row }">
              <el-tag :type="row.isOverThreshold ? 'danger' : 'warning'" size="small" effect="dark">
                {{ row.abnormalText }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="salesOrderNo" label="销售订单号" width="132" show-overflow-tooltip />
          <el-table-column prop="planTrackingNo" label="计划跟踪号" width="132" show-overflow-tooltip />
          <el-table-column prop="materialCode" label="物料编码" width="128" show-overflow-tooltip />
          <el-table-column prop="specModel" label="规格型号" min-width="220" show-overflow-tooltip />
          <el-table-column prop="valveCategory" label="阀门类别" width="118" show-overflow-tooltip />
          <el-table-column prop="productionLine" label="产线" width="100" show-overflow-tooltip />
          <el-table-column prop="orderQty" label="数量" width="90" align="right">
            <template #default="{ row }">{{ formatNumber(row.orderQty) }}</template>
          </el-table-column>
          <el-table-column label="排产优化日期" width="128">
            <template #default="{ row }">{{ formatDate(row.capacityScheduleDate) }}</template>
          </el-table-column>
        </el-table>

        <div class="pager">
          <el-pagination
            v-model:current-page="pager.page"
            v-model:page-size="pager.rows"
            small
            background
            layout="prev, pager, next, sizes"
            :page-sizes="[20, 30, 50, 100]"
            :total="pager.total"
            @current-change="loadList"
            @size-change="handleSizeChange"
          />
        </div>
      </section>

      <section class="capacity-panel">
        <div v-if="!selectedOrder" class="empty-state">
          <el-icon><WarningFilled /></el-icon>
          <span>请选择异常订单</span>
        </div>

        <template v-else>
          <div class="order-detail">
            <div class="order-main">
              <el-tag :type="selectedOrder.isOverThreshold ? 'danger' : 'warning'" effect="dark">
                {{ selectedOrder.abnormalText }}
              </el-tag>
              <div>
                <div class="order-title">
                  {{ selectedOrder.salesOrderNo || '-' }} / {{ selectedOrder.planTrackingNo || '-' }}
                </div>
                <div class="order-sub">{{ selectedOrder.materialCode || '-' }}</div>
              </div>
            </div>
            <div class="order-fields">
              <div>
                <span>规格型号</span>
                <strong>{{ selectedOrder.specModel || '-' }}</strong>
              </div>
              <div>
                <span>阀门类别</span>
                <strong>{{ selectedOrder.valveCategory || '-' }}</strong>
              </div>
              <div>
                <span>产线</span>
                <strong>{{ selectedOrder.productionLine || '-' }}</strong>
              </div>
              <div>
                <span>订单数量</span>
                <strong>{{ formatNumber(selectedOrder.orderQty) }}</strong>
              </div>
            </div>
          </div>

          <div class="capacity-header">
            <div>
              <div class="panel-title">产线产能窗口</div>
              <div class="panel-meta">
                {{ formatDate(windowData?.startDate) }} 至 {{ formatDate(windowData?.endDate) }}
              </div>
            </div>
            <div class="legend">
              <span class="legend-item normal">100% 内</span>
              <span class="legend-item reserve">120% 内</span>
              <span class="legend-item over">超 120%</span>
            </div>
          </div>

          <div v-loading="windowLoading" class="capacity-grid">
            <button
              v-for="day in capacityDays"
              :key="day.dateKey"
              type="button"
              class="capacity-day"
              :class="dayClass(day)"
              :disabled="!day.canSelect"
              @click="selectDay(day)"
            >
              <span class="day-top">
                <strong>{{ day.dayText }}</strong>
                <em>{{ day.weekName }}</em>
              </span>
              <span class="day-type">{{ day.dayType }}</span>
              <span class="day-numbers">
                <b>{{ formatNumber(day.projectedQuantity) }}</b>
                <i>/ {{ formatNumber(day.threshold) }}</i>
              </span>
              <span class="day-sub">基准 {{ formatNumber(day.quantity) }}</span>
              <span class="day-rate">{{ formatRate(day.projectedLoadRate) }}</span>
            </button>
          </div>

          <div class="save-bar">
            <div class="selected-date">
              <el-icon><Calendar /></el-icon>
              <span>选择日期</span>
              <strong>{{ selectedDate || '-' }}</strong>
            </div>
            <div class="selected-capacity">
              <span>插入后</span>
              <strong>{{ selectedDay ? formatNumber(selectedDay.projectedQuantity) : '-' }}</strong>
              <span>负载</span>
              <strong>{{ selectedDay ? formatRate(selectedDay.projectedLoadRate) : '-' }}</strong>
            </div>
            <el-button
              type="primary"
              :icon="Check"
              :loading="saveLoading"
              :disabled="!selectedDay || !selectedDay.canSelect"
              @click="saveAdjustment"
            >
              保存调整
            </el-button>
          </div>
        </template>
      </section>
    </main>
  </div>
</template>

<script setup>
import { computed, getCurrentInstance, onMounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { Calendar, Check, Refresh, Search, WarningFilled } from '@element-plus/icons-vue'

const { proxy } = getCurrentInstance()

const query = reactive({
  keyword: '',
  abnormalType: 'all',
  valveCategory: '',
  productionLine: ''
})

const pager = reactive({
  page: 1,
  rows: 30,
  total: 0
})

const summary = reactive({
  overThresholdCount: 0,
  holidayCount: 0
})

const orders = ref([])
const valveCategories = ref([])
const productionLines = ref([])
const selectedOrder = ref(null)
const windowData = ref(null)
const selectedDate = ref('')
const listLoading = ref(false)
const windowLoading = ref(false)
const saveLoading = ref(false)

const pageCount = computed(() => Math.max(1, Math.ceil((pager.total || 0) / pager.rows)))
const currentLineText = computed(() => selectedOrder.value?.productionLine || '未选择')
const capacityDays = computed(() => windowData.value?.days || [])
const selectedDay = computed(() => capacityDays.value.find((day) => day.dateKey === selectedDate.value))

const pick = (obj, lower, upper) => obj?.[lower] ?? obj?.[upper]

const normalizeOrder = (row) => ({
  id: pick(row, 'id', 'Id'),
  salesOrderNo: pick(row, 'salesOrderNo', 'SalesOrderNo') || '',
  planTrackingNo: pick(row, 'planTrackingNo', 'PlanTrackingNo') || '',
  materialCode: pick(row, 'materialCode', 'MaterialCode') || '',
  specModel: pick(row, 'specModel', 'SpecModel') || '',
  valveCategory: pick(row, 'valveCategory', 'ValveCategory') || '',
  productionLine: pick(row, 'productionLine', 'ProductionLine') || '',
  orderQty: Number(pick(row, 'orderQty', 'OrderQty') || 0),
  scheduleDate: pick(row, 'scheduleDate', 'ScheduleDate'),
  capacityScheduleDate: pick(row, 'capacityScheduleDate', 'CapacityScheduleDate'),
  isOverThreshold: Boolean(pick(row, 'isOverThreshold', 'IsOverThreshold')),
  isStatutoryHoliday: Boolean(pick(row, 'isStatutoryHoliday', 'IsStatutoryHoliday')),
  abnormalType: pick(row, 'abnormalType', 'AbnormalType') || '',
  abnormalText: pick(row, 'abnormalText', 'AbnormalText') || '',
  abnormalLevel: Number(pick(row, 'abnormalLevel', 'AbnormalLevel') || 0)
})

const normalizeDay = (day) => {
  const dateValue = pick(day, 'date', 'Date')
  return {
    date: dateValue,
    dateKey: formatDate(dateValue),
    dayText: formatDay(dateValue),
    weekName: pick(day, 'weekName', 'WeekName') || '',
    dayType: pick(day, 'dayType', 'DayType') || '',
    isWorkday: Boolean(pick(day, 'isWorkday', 'IsWorkday')),
    isSaturdayRestDay: Boolean(pick(day, 'isSaturdayRestDay', 'IsSaturdayRestDay')),
    isSundayRestDay: Boolean(pick(day, 'isSundayRestDay', 'IsSundayRestDay')),
    isStatutoryHoliday: Boolean(pick(day, 'isStatutoryHoliday', 'IsStatutoryHoliday')),
    isMakeupWorkday: Boolean(pick(day, 'isMakeupWorkday', 'IsMakeupWorkday')),
    isCurrentDate: Boolean(pick(day, 'isCurrentDate', 'IsCurrentDate')),
    actualQuantity: Number(pick(day, 'actualQuantity', 'ActualQuantity') || 0),
    normalOptimizedQuantity: Number(pick(day, 'normalOptimizedQuantity', 'NormalOptimizedQuantity') || 0),
    quantity: Number(pick(day, 'quantity', 'Quantity') || 0),
    insertQuantity: Number(pick(day, 'insertQuantity', 'InsertQuantity') || 0),
    projectedQuantity: Number(pick(day, 'projectedQuantity', 'ProjectedQuantity') || 0),
    threshold: Number(pick(day, 'threshold', 'Threshold') || 0),
    loadRate: pick(day, 'loadRate', 'LoadRate'),
    projectedLoadRate: pick(day, 'projectedLoadRate', 'ProjectedLoadRate'),
    status: pick(day, 'status', 'Status') || '',
    statusText: pick(day, 'statusText', 'StatusText') || '',
    canSelect: Boolean(pick(day, 'canSelect', 'CanSelect'))
  }
}

const normalizeWindow = (data) => ({
  order: normalizeOrder(pick(data, 'order', 'Order')),
  startDate: pick(data, 'startDate', 'StartDate'),
  endDate: pick(data, 'endDate', 'EndDate'),
  threshold: pick(data, 'threshold', 'Threshold'),
  days: (pick(data, 'days', 'Days') || []).map(normalizeDay)
})

async function loadList() {
  listLoading.value = true
  try {
    const result = await proxy.http.post(
      '/api/WZ/CapacityScheduleAdjustment/list',
      {
        page: pager.page,
        rows: pager.rows,
        keyword: query.keyword,
        abnormalType: query.abnormalType,
        valveCategory: query.valveCategory,
        productionLine: query.productionLine
      },
      false
    )

    const items = pick(result, 'items', 'Items') || []
    orders.value = items.map(normalizeOrder)
    pager.total = Number(pick(result, 'total', 'Total') || 0)
    summary.overThresholdCount = Number(pick(result, 'overThresholdCount', 'OverThresholdCount') || 0)
    summary.holidayCount = Number(pick(result, 'holidayCount', 'HolidayCount') || 0)
    valveCategories.value = pick(result, 'valveCategories', 'ValveCategories') || []
    productionLines.value = pick(result, 'productionLines', 'ProductionLines') || []

    if (!orders.value.some((row) => row.id === selectedOrder.value?.id)) {
      selectedOrder.value = orders.value[0] || null
      windowData.value = null
      selectedDate.value = ''
      if (selectedOrder.value) {
        await loadWindow(selectedOrder.value.id)
      }
    }
  } catch (error) {
    ElMessage.error(error?.message || error || '异常排产列表加载失败')
  } finally {
    listLoading.value = false
  }
}

async function loadWindow(id) {
  if (!id) {
    return
  }

  windowLoading.value = true
  try {
    const result = await proxy.http.get(`/api/WZ/CapacityScheduleAdjustment/window/${id}`, {}, false)
    windowData.value = normalizeWindow(result)
    selectedOrder.value = windowData.value.order
    const current = windowData.value.days.find((day) => day.isCurrentDate)
    const firstSelectable = windowData.value.days.find((day) => day.canSelect)
    selectedDate.value = (current?.canSelect ? current : firstSelectable)?.dateKey || ''
  } catch (error) {
    ElMessage.error(error?.message || error || '产能窗口加载失败')
  } finally {
    windowLoading.value = false
  }
}

function handleSearch() {
  pager.page = 1
  loadList()
}

function handleSizeChange() {
  pager.page = 1
  loadList()
}

async function reloadCurrent() {
  await loadList()
  if (selectedOrder.value?.id) {
    await loadWindow(selectedOrder.value.id)
  }
}

function handleRowClick(row) {
  selectedOrder.value = row
  selectedDate.value = ''
  loadWindow(row.id)
}

function selectDay(day) {
  if (!day.canSelect) {
    return
  }
  selectedDate.value = day.dateKey
}

async function saveAdjustment() {
  if (!selectedOrder.value?.id || !selectedDay.value?.canSelect) {
    ElMessage.warning('请选择 120% 以内的日期')
    return
  }

  saveLoading.value = true
  try {
    const result = await proxy.http.post(
      '/api/WZ/CapacityScheduleAdjustment/save',
      {
        id: selectedOrder.value.id,
        capacityScheduleDate: selectedDate.value
      },
      true
    )

    if (result?.success === false || result?.Success === false) {
      ElMessage.error(result.message || result.Message || '排产优化日期调整失败')
      return
    }

    ElMessage.success(result?.message || result?.Message || '排产优化日期已调整')
    await loadList()
    await loadWindow(selectedOrder.value.id)
  } catch (error) {
    ElMessage.error(error?.message || error || '排产优化日期调整失败')
  } finally {
    saveLoading.value = false
  }
}

function orderRowClass({ row }) {
  if (row.isOverThreshold) {
    return 'order-row-danger'
  }
  if (row.isStatutoryHoliday) {
    return 'order-row-warning'
  }
  return ''
}

function dayClass(day) {
  return [
    `status-${day.status || 'missing'}`,
    {
      selected: day.dateKey === selectedDate.value,
      current: day.isCurrentDate,
      holiday: day.isStatutoryHoliday,
      weekend: day.isSaturdayRestDay || day.isSundayRestDay,
      disabled: !day.canSelect
    }
  ]
}

function formatDate(value) {
  if (!value) {
    return ''
  }
  if (typeof value === 'string') {
    return value.slice(0, 10)
  }
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return ''
  }
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  return `${date.getFullYear()}-${month}-${day}`
}

function formatDay(value) {
  const text = formatDate(value)
  return text ? text.slice(5) : '-'
}

function formatNumber(value) {
  const number = Number(value || 0)
  return number.toLocaleString('zh-CN', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2
  })
}

function formatRate(value) {
  if (value === null || value === undefined || value === '') {
    return '-'
  }
  return `${Number(value).toFixed(1)}%`
}

onMounted(loadList)
</script>

<style scoped>
.capacity-adjustment-page {
  min-height: 100%;
  padding: 14px;
  background: #f5f7fb;
  color: #1f2937;
}

.toolbar,
.summary-strip,
.workbench,
.orders-panel,
.capacity-panel,
.order-detail,
.capacity-header,
.save-bar {
  width: 100%;
}

.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 14px;
  background: #fff;
  border: 1px solid #dbe2ee;
  border-radius: 6px;
}

.title-block {
  flex: 0 0 220px;
}

.page-title {
  font-size: 18px;
  font-weight: 700;
  color: #23277d;
  line-height: 26px;
}

.page-subtitle {
  margin-top: 2px;
  font-size: 12px;
  color: #6b7280;
}

.filters {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 8px;
}

.filter-keyword {
  width: 260px;
}

.filter-select {
  width: 136px;
}

.summary-strip {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 10px;
  margin-top: 10px;
}

.summary-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  min-height: 54px;
  padding: 10px 14px;
  background: #fff;
  border: 1px solid #dbe2ee;
  border-left: 4px solid #0079c1;
  border-radius: 6px;
}

.summary-item strong {
  font-size: 20px;
  color: #1f2937;
}

.summary-item.danger {
  border-left-color: #d03050;
}

.summary-item.warning {
  border-left-color: #d99000;
}

.summary-label {
  font-size: 12px;
  color: #6b7280;
}

.workbench {
  display: grid;
  grid-template-columns: minmax(520px, 47%) minmax(520px, 1fr);
  gap: 12px;
  margin-top: 10px;
}

.orders-panel,
.capacity-panel {
  min-width: 0;
  padding: 12px;
  background: #fff;
  border: 1px solid #dbe2ee;
  border-radius: 6px;
}

.panel-header,
.capacity-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 10px;
}

.panel-title {
  font-size: 15px;
  font-weight: 700;
  color: #303384;
  line-height: 22px;
}

.panel-meta {
  font-size: 12px;
  color: #6b7280;
}

.pager {
  display: flex;
  justify-content: flex-end;
  padding-top: 10px;
}

.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  height: calc(100vh - 230px);
  color: #6b7280;
  border: 1px dashed #cbd5e1;
  border-radius: 6px;
}

.order-detail {
  padding: 12px;
  border: 1px solid #dbe2ee;
  border-radius: 6px;
  background: #fbfcff;
}

.order-main {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

.order-title {
  font-size: 15px;
  font-weight: 700;
  color: #111827;
}

.order-sub {
  margin-top: 2px;
  font-size: 12px;
  color: #6b7280;
}

.order-fields {
  display: grid;
  grid-template-columns: 1.5fr repeat(3, minmax(0, 1fr));
  gap: 8px;
  margin-top: 12px;
}

.order-fields div {
  min-width: 0;
  padding: 8px 10px;
  background: #fff;
  border: 1px solid #e5e9f2;
  border-radius: 4px;
}

.order-fields span {
  display: block;
  font-size: 12px;
  color: #6b7280;
}

.order-fields strong {
  display: block;
  margin-top: 4px;
  overflow: hidden;
  color: #1f2937;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.capacity-header {
  margin-top: 14px;
}

.legend {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  justify-content: flex-end;
}

.legend-item {
  display: inline-flex;
  align-items: center;
  height: 22px;
  padding: 0 8px;
  border-radius: 999px;
  font-size: 12px;
  border: 1px solid transparent;
}

.legend-item.normal {
  color: #0f766e;
  background: #e6fffb;
  border-color: #99f6e4;
}

.legend-item.reserve {
  color: #9a6700;
  background: #fff7e6;
  border-color: #ffd591;
}

.legend-item.over {
  color: #c41d1d;
  background: #fff1f0;
  border-color: #ffccc7;
}

.capacity-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(116px, 1fr));
  gap: 8px;
  min-height: 310px;
}

.capacity-day {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  min-height: 118px;
  padding: 9px;
  text-align: left;
  cursor: pointer;
  background: #fff;
  border: 1px solid #dbe2ee;
  border-radius: 6px;
  transition: border-color 0.15s ease, box-shadow 0.15s ease, transform 0.15s ease;
}

.capacity-day:hover:not(:disabled) {
  border-color: #0079c1;
  box-shadow: 0 6px 14px rgba(0, 121, 193, 0.16);
  transform: translateY(-1px);
}

.capacity-day:disabled {
  cursor: not-allowed;
}

.capacity-day.selected {
  border-color: #23277d;
  box-shadow: inset 0 0 0 2px #23277d;
}

.capacity-day.current {
  outline: 2px solid #a8b3c9;
  outline-offset: -4px;
}

.capacity-day.status-normal {
  background: #f0fdfa;
  border-color: #99f6e4;
}

.capacity-day.status-reserve {
  background: #fff7e6;
  border-color: #ffd591;
}

.capacity-day.status-over,
.capacity-day.disabled {
  background: #fff1f0;
  border-color: #ffccc7;
}

.capacity-day.holiday {
  border-style: dashed;
}

.capacity-day.weekend:not(.holiday) {
  background-image: linear-gradient(135deg, rgba(168, 179, 201, 0.18), rgba(255, 255, 255, 0));
}

.day-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 6px;
}

.day-top strong {
  font-size: 15px;
  color: #111827;
}

.day-top em,
.day-type,
.day-sub,
.day-rate {
  font-style: normal;
  font-size: 12px;
  color: #6b7280;
}

.day-type {
  margin-top: 4px;
}

.day-numbers {
  display: flex;
  align-items: baseline;
  gap: 4px;
  margin-top: 8px;
}

.day-numbers b {
  color: #111827;
  font-size: 18px;
}

.day-numbers i {
  color: #6b7280;
  font-style: normal;
  font-size: 12px;
}

.day-sub {
  margin-top: 4px;
}

.day-rate {
  align-self: flex-start;
  margin-top: 6px;
  padding: 2px 6px;
  color: #111827;
  background: rgba(255, 255, 255, 0.72);
  border-radius: 999px;
}

.save-bar {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 14px;
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid #e5e9f2;
}

.selected-date,
.selected-capacity {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  color: #6b7280;
  font-size: 13px;
}

.selected-date strong,
.selected-capacity strong {
  color: #111827;
}

:deep(.order-row-danger td) {
  background: #fff1f0 !important;
}

:deep(.order-row-warning td) {
  background: #fffbe6 !important;
}

:deep(.el-table__row.current-row td) {
  background: #eef4ff !important;
}

@media (max-width: 1280px) {
  .toolbar {
    align-items: flex-start;
    flex-direction: column;
  }

  .filters {
    justify-content: flex-start;
  }

  .workbench {
    grid-template-columns: 1fr;
  }

  .order-fields {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 760px) {
  .summary-strip,
  .order-fields {
    grid-template-columns: 1fr;
  }

  .filter-keyword,
  .filter-select {
    width: 100%;
  }

  .filters {
    width: 100%;
  }

  .save-bar {
    align-items: stretch;
    flex-direction: column;
  }
}
</style>
