<template>
  <div class="order-home-dashboard">
    <section class="dashboard-header">
      <div>
        <div class="dashboard-kicker">订单协同平台</div>
        <h1>订单运营首页看板</h1>
        <p>基于订单跟踪、缺料、催单、协商、BOM、采购和生产底层数据实时汇总。</p>
      </div>
      <div class="header-actions">
        <div class="refresh-time">
          <span>数据来源</span>
          <strong>{{ sourceTablesText }}</strong>
          <em>更新 {{ refreshTime || '-' }}</em>
        </div>
        <el-button
          :icon="Refresh"
          :loading="dashboardLoading"
          type="primary"
          @click="refreshDashboard(true)"
        >
          刷新
        </el-button>
      </div>
    </section>

    <section class="filter-panel">
      <div class="filter-item">
        <span>日期范围</span>
        <el-select v-model="filters.dateRange" @change="applyFilters">
          <el-option
            v-for="item in dateRangeOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>
      <div class="filter-item">
        <span>业务类型</span>
        <el-select v-model="filters.businessType" @change="applyFilters">
          <el-option label="全部" value="all" />
          <el-option label="销售订单" value="sales" />
          <el-option label="生产订单" value="production" />
          <el-option label="采购协同" value="purchase" />
        </el-select>
      </div>
      <div class="filter-item">
        <span>客户</span>
        <el-select v-model="filters.customer" filterable @change="applyFilters">
          <el-option label="全部" value="all" />
          <el-option v-for="item in customerOptions" :key="item" :label="item" :value="item" />
        </el-select>
      </div>
      <div class="filter-item">
        <span>负责人</span>
        <el-select v-model="filters.owner" filterable @change="applyFilters">
          <el-option label="全部" value="all" />
          <el-option v-for="item in ownerOptions" :key="item" :label="item" :value="item" />
        </el-select>
      </div>
      <div class="quick-tabs">
        <button
          v-for="item in quickFilters"
          :key="item.value"
          :class="{ active: filters.risk === item.value }"
          @click="setRiskFilter(item.value)"
        >
          {{ item.label }}
        </button>
      </div>
    </section>

    <section class="kpi-grid" v-loading="dashboardLoading">
      <article
        v-for="item in kpiCards"
        :key="item.key"
        class="kpi-card"
        :class="item.level"
        @click="openTarget(item.path)"
      >
        <div class="kpi-icon">
          <el-icon>
            <component :is="item.icon" />
          </el-icon>
        </div>
        <div class="kpi-content">
          <div class="kpi-label">{{ item.label }}</div>
          <div class="kpi-value">
            {{ formatNumber(item.value) }}
            <span>{{ item.unit }}</span>
          </div>
          <div class="kpi-trend">{{ item.trend }}</div>
        </div>
      </article>
    </section>

    <section class="chart-grid">
      <article class="panel panel-wide">
        <div class="panel-header">
          <div>
            <h2>订单进度趋势</h2>
            <p>新建、交付完成与进行中订单走势</p>
          </div>
          <el-tag type="primary" effect="plain">{{ activeDateRangeLabel }}</el-tag>
        </div>
        <div ref="trendChartRef" class="chart-box"></div>
      </article>

      <article class="panel">
        <div class="panel-header">
          <div>
            <h2>协同消息状态</h2>
            <p>催单与协商消息处理情况</p>
          </div>
        </div>
        <div ref="messageChartRef" class="chart-box chart-donut"></div>
      </article>
    </section>

    <section class="diagnosis-grid">
      <article class="panel">
        <div class="panel-header">
          <div>
            <h2>缺料风险 Top 10</h2>
            <p>按未齐套数量排序</p>
          </div>
          <el-button text type="primary" @click="openTarget('/OCP_LackMtrlResult')">
            查看明细
            <el-icon><ArrowRight /></el-icon>
          </el-button>
        </div>
        <div ref="shortageChartRef" class="chart-box compact-chart"></div>
      </article>

      <article class="panel progress-panel">
        <div class="panel-header">
          <div>
            <h2>BOM / 采购 / 生产进度</h2>
            <p>关键链路完成率</p>
          </div>
        </div>
        <div class="progress-list">
          <div v-for="item in progressRows" :key="item.label" class="progress-row">
            <div class="progress-meta">
              <span>{{ item.label }}</span>
              <strong>{{ item.done }}%</strong>
            </div>
            <div class="stacked-progress">
              <span class="done" :style="{ width: item.done + '%' }">{{ item.done }}%</span>
              <span class="doing" :style="{ width: item.doing + '%' }">{{ item.doing }}%</span>
              <span class="todo" :style="{ width: item.todo + '%' }">{{ item.todo }}%</span>
            </div>
          </div>
        </div>
        <div class="progress-legend">
          <span><i class="done"></i>已完成</span>
          <span><i class="doing"></i>进行中</span>
          <span><i class="todo"></i>未开始</span>
        </div>
      </article>

      <article class="panel risk-panel">
        <div class="panel-header">
          <div>
            <h2>风险分布</h2>
            <p>按客户与状态拆解</p>
          </div>
        </div>
        <div class="risk-matrix">
          <div class="risk-head">客户 / 状态</div>
          <div class="risk-head">正常</div>
          <div class="risk-head">预警</div>
          <div class="risk-head">延期</div>
          <div class="risk-head">超期</div>
          <template v-for="row in riskRows" :key="row.customer">
            <div class="risk-name">{{ row.customer }}</div>
            <div class="risk-cell normal">{{ row.normal }}</div>
            <div class="risk-cell warning">{{ row.warning }}</div>
            <div class="risk-cell delay">{{ row.delay }}</div>
            <div class="risk-cell overdue">{{ row.overdue }}</div>
          </template>
        </div>
      </article>
    </section>

    <section class="panel todo-panel">
      <div class="panel-header">
        <div>
          <h2>待处理事项</h2>
          <p>点击查看或处理可跳转到对应业务页面</p>
        </div>
        <el-input
          v-model="keyword"
          :prefix-icon="Search"
          clearable
          placeholder="搜索单号、客户、负责人"
          @input="applyFilters"
        />
      </div>
      <el-table :data="filteredTodos" height="285" stripe v-loading="dashboardLoading">
        <el-table-column label="类型" min-width="90">
          <template #default="{ row }">
            <span class="type-dot" :class="row.level"></span>
            {{ row.type }}
          </template>
        </el-table-column>
        <el-table-column prop="code" label="单号" min-width="160" />
        <el-table-column prop="customer" label="客户 / 供应商" min-width="220" />
        <el-table-column prop="owner" label="负责人" width="110" />
        <el-table-column prop="deadline" label="截止时间" min-width="150" />
        <el-table-column label="状态" width="110">
          <template #default="{ row }">
            <el-tag :type="statusTypeMap[row.status] || 'info'" effect="light">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="openTarget(row.path)">查看</el-button>
            <el-button link type="primary" @click="openTarget(row.path)">处理</el-button>
          </template>
        </el-table-column>
      </el-table>
    </section>
  </div>
</template>

<script setup>
import * as echarts from 'echarts'
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import {
  ArrowRight,
  Box,
  ChatDotRound,
  Clock,
  Document,
  Refresh,
  Search,
  Van,
  Warning
} from '@element-plus/icons-vue'
import { useRouter } from 'vue-router'
import http from '@/api/http'

const router = useRouter()

const filters = reactive({
  dateRange: 'week',
  businessType: 'all',
  customer: 'all',
  owner: 'all',
  risk: 'all'
})

const dashboardData = reactive({
  source: {
    updatedAt: '',
    tables: []
  },
  customers: [],
  owners: [],
  kpis: [],
  trend: {
    labels: [],
    newOrders: [],
    completedOrders: [],
    runningOrders: []
  },
  message: {
    total: 0,
    pending: 0,
    replied: 0,
    overdue: 0
  },
  shortageTop: [],
  progressRows: [],
  riskRows: [],
  todos: []
})

const dateRangeOptions = [
  { label: '今日', value: 'today' },
  { label: '本周', value: 'week' },
  { label: '本月', value: 'month' },
  { label: '近 30 天', value: 'thirtyDays' }
]

const quickFilters = [
  { label: '全部', value: 'all' },
  { label: '只看缺料', value: 'shortage' },
  { label: '只看延期', value: 'delay' },
  { label: '只看待回复', value: 'reply' },
  { label: '只看超期', value: 'overdue' }
]

const iconMap = {
  running: Document,
  delay: Warning,
  shortage: Box,
  reply: ChatDotRound,
  overdue: Clock,
  delivery: Van
}

const defaultKpis = [
  {
    key: 'running',
    label: '进行中订单',
    value: 0,
    unit: '单',
    trend: '订单跟踪表汇总',
    level: 'primary',
    path: '/OCP_OrderTracking'
  },
  {
    key: 'delay',
    label: '延期风险',
    value: 0,
    unit: '单',
    trend: '按交期与入库进度判断',
    level: 'danger',
    path: '/OCP_OrderTracking'
  },
  {
    key: 'shortage',
    label: '缺料预警',
    value: 0,
    unit: '项',
    trend: '缺料结果表未齐套项',
    level: 'warning',
    path: '/OCP_LackMtrlResult'
  },
  {
    key: 'reply',
    label: '待回复协同',
    value: 0,
    unit: '条',
    trend: '催单与协商未回复',
    level: 'primary',
    path: '/message-center/reminder'
  },
  {
    key: 'overdue',
    label: '超期未回复',
    value: 0,
    unit: '条',
    trend: '需尽快闭环',
    level: 'danger',
    path: '/message-center/negotiate'
  },
  {
    key: 'delivery',
    label: '本周交付',
    value: 0,
    unit: '单',
    trend: '按最后入库日期统计',
    level: 'primary',
    path: '/OCP_OrderTracking'
  }
]

const statusTypeMap = {
  正常: 'success',
  预警: 'warning',
  缺料: 'warning',
  延期: 'danger',
  待回复: 'primary',
  已回复: 'success',
  超期: 'danger'
}

const keyword = ref('')
const refreshTime = ref('')
const dashboardLoading = ref(false)
const trendChartRef = ref(null)
const messageChartRef = ref(null)
const shortageChartRef = ref(null)

let trendChart
let messageChart
let shortageChart
let refreshTimer

const customerOptions = computed(() => dashboardData.customers || [])
const ownerOptions = computed(() => dashboardData.owners || [])

const activeDateRangeLabel = computed(() => {
  return dateRangeOptions.find((item) => item.value === filters.dateRange)?.label || '本周'
})

const sourceTablesText = computed(() => {
  const count = dashboardData.source?.tables?.length || 0
  return count ? `${count} 张底表` : '底表汇总'
})

const kpiCards = computed(() => {
  const rows = dashboardData.kpis?.length ? dashboardData.kpis : defaultKpis
  return rows.map((item) => ({
    ...item,
    icon: iconMap[item.key] || Document
  }))
})

const progressRows = computed(() => {
  const rows = dashboardData.progressRows?.length
    ? dashboardData.progressRows
    : [
        { label: 'BOM', done: 0, doing: 0, todo: 0 },
        { label: '采购', done: 0, doing: 0, todo: 0 },
        { label: '生产', done: 0, doing: 0, todo: 0 }
      ]
  return rows.map((item) => ({
    label: item.label,
    done: normalizePercent(item.done),
    doing: normalizePercent(item.doing),
    todo: normalizePercent(item.todo)
  }))
})

const riskRows = computed(() => dashboardData.riskRows || [])

const filteredTodos = computed(() => {
  const text = keyword.value.trim().toLowerCase()
  return (dashboardData.todos || []).filter((row) => {
    const matchRisk = filters.risk === 'all' || row.risk === filters.risk
    const matchCustomer = filters.customer === 'all' || row.customer === filters.customer
    const matchOwner = filters.owner === 'all' || row.owner === filters.owner
    const matchKeyword =
      !text ||
      String(row.code || '')
        .toLowerCase()
        .includes(text) ||
      String(row.customer || '')
        .toLowerCase()
        .includes(text) ||
      String(row.owner || '')
        .toLowerCase()
        .includes(text)
    return matchRisk && matchCustomer && matchOwner && matchKeyword
  })
})

const normalizeList = (value) => (Array.isArray(value) ? value : [])

const normalizeNumber = (value) => {
  const number = Number(value)
  return Number.isFinite(number) ? number : 0
}

const normalizePercent = (value) => {
  return Math.min(Math.max(Math.round(normalizeNumber(value)), 0), 100)
}

const formatNumber = (value) => {
  return normalizeNumber(value).toLocaleString('zh-CN')
}

const getResponseData = (response) => {
  const status = response?.status ?? response?.Status
  if (status === false) {
    throw new Error(response?.message || response?.Message || '获取首页看板失败')
  }
  return response?.data ?? response?.Data ?? response ?? {}
}

const getRequestParams = () => ({
  dateRange: filters.dateRange,
  businessType: filters.businessType,
  customer: filters.customer,
  owner: filters.owner,
  keyword: keyword.value.trim()
})

const syncDashboardData = (data) => {
  const source = data.source || data.Source || {}
  const trend = data.trend || data.Trend || {}
  const message = data.message || data.Message || {}

  dashboardData.source = {
    updatedAt: source.updatedAt || source.UpdatedAt || '',
    tables: normalizeList(source.tables || source.Tables)
  }
  dashboardData.customers = normalizeList(data.customers || data.Customers)
  dashboardData.owners = normalizeList(data.owners || data.Owners)
  dashboardData.kpis = normalizeList(data.kpis || data.Kpis || data.KPIs)
  dashboardData.trend = {
    labels: normalizeList(trend.labels || trend.Labels),
    newOrders: normalizeList(trend.newOrders || trend.NewOrders),
    completedOrders: normalizeList(trend.completedOrders || trend.CompletedOrders),
    runningOrders: normalizeList(trend.runningOrders || trend.RunningOrders)
  }
  dashboardData.message = {
    total: normalizeNumber(message.total || message.Total),
    pending: normalizeNumber(message.pending || message.Pending),
    replied: normalizeNumber(message.replied || message.Replied),
    overdue: normalizeNumber(message.overdue || message.Overdue)
  }
  dashboardData.shortageTop = normalizeList(data.shortageTop || data.ShortageTop)
  dashboardData.progressRows = normalizeList(data.progressRows || data.ProgressRows)
  dashboardData.riskRows = normalizeList(data.riskRows || data.RiskRows)
  dashboardData.todos = normalizeList(data.todos || data.Todos)
  refreshTime.value = dashboardData.source.updatedAt || formatNow()
}

const formatNow = () => {
  const now = new Date()
  const pad = (value) => String(value).padStart(2, '0')
  return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())} ${pad(
    now.getHours()
  )}:${pad(now.getMinutes())}`
}

const refreshDashboard = async (showMessage = false) => {
  dashboardLoading.value = true
  try {
    const response = await http.get('api/OCP_OrderTracking/GetHomeDashboard', {}, false, {
      params: getRequestParams()
    })
    syncDashboardData(getResponseData(response))
    await nextTick()
    renderCharts()
    if (showMessage === true) {
      ElMessage.success('首页看板已刷新')
    }
  } catch (error) {
    console.error('获取首页看板失败:', error)
    ElMessage.error(error?.message || '获取首页看板失败，请稍后重试')
  } finally {
    dashboardLoading.value = false
  }
}

const getTrendSeries = () => {
  const labels = dashboardData.trend.labels?.length ? dashboardData.trend.labels : ['暂无数据']
  const zeroSeries = labels.map(() => 0)
  return {
    labels,
    newOrders: dashboardData.trend.newOrders?.length ? dashboardData.trend.newOrders : zeroSeries,
    completedOrders: dashboardData.trend.completedOrders?.length
      ? dashboardData.trend.completedOrders
      : zeroSeries,
    runningOrders: dashboardData.trend.runningOrders?.length
      ? dashboardData.trend.runningOrders
      : zeroSeries
  }
}

const renderTrendChart = () => {
  if (!trendChartRef.value) return
  const trend = getTrendSeries()
  trendChart = trendChart || echarts.init(trendChartRef.value)
  trendChart.setOption(
    {
      color: ['#7cb5ec', '#0079c1', '#303384'],
      grid: { left: 42, right: 48, top: 56, bottom: 32 },
      tooltip: { trigger: 'axis' },
      legend: {
        top: 8,
        itemWidth: 12,
        itemHeight: 8,
        textStyle: { color: '#595959' },
        data: ['新建订单', '交付完成', '进行中订单']
      },
      xAxis: {
        type: 'category',
        data: trend.labels,
        axisLine: { lineStyle: { color: '#d8dee8' } },
        axisTick: { show: false }
      },
      yAxis: [
        {
          type: 'value',
          name: '订单数',
          min: 0,
          splitLine: { lineStyle: { color: '#edf0f5' } }
        },
        {
          type: 'value',
          name: '进行中',
          min: 0,
          splitLine: { show: false }
        }
      ],
      series: [
        { name: '新建订单', type: 'bar', barWidth: 18, data: trend.newOrders },
        { name: '交付完成', type: 'bar', barWidth: 18, data: trend.completedOrders },
        {
          name: '进行中订单',
          type: 'line',
          yAxisIndex: 1,
          smooth: true,
          symbolSize: 7,
          lineStyle: { width: 3 },
          data: trend.runningOrders
        }
      ]
    },
    true
  )
}

const renderMessageChart = () => {
  if (!messageChartRef.value) return
  const message = dashboardData.message || {}
  const total = normalizeNumber(message.total)
  const pending = normalizeNumber(message.pending)
  const replied = normalizeNumber(message.replied)
  const overdue = normalizeNumber(message.overdue)
  const chartData =
    total > 0
      ? [
          { name: '待回复', value: pending },
          { name: '已回复', value: replied },
          { name: '已超期', value: overdue }
        ]
      : [{ name: '暂无数据', value: 1, itemStyle: { color: '#e5e9e9' } }]

  messageChart = messageChart || echarts.init(messageChartRef.value)
  messageChart.setOption(
    {
      color: ['#0079c1', '#62bf5e', '#f24f5d'],
      tooltip: { trigger: 'item' },
      legend: {
        right: 16,
        top: 'center',
        orient: 'vertical',
        itemWidth: 10,
        itemHeight: 10,
        textStyle: { color: '#595959' },
        formatter: (name) => {
          const map = {
            待回复: `待回复 ${formatNumber(pending)}`,
            已回复: `已回复 ${formatNumber(replied)}`,
            已超期: `已超期 ${formatNumber(overdue)}`,
            暂无数据: '暂无数据'
          }
          return map[name] || name
        }
      },
      graphic: [
        {
          type: 'text',
          left: '33%',
          top: '42%',
          style: {
            text: `总数\n${formatNumber(total)}`,
            textAlign: 'center',
            fill: '#23277d',
            fontSize: 20,
            fontWeight: 700,
            lineHeight: 32
          }
        }
      ],
      series: [
        {
          name: '协同消息',
          type: 'pie',
          radius: ['52%', '74%'],
          center: ['36%', '52%'],
          avoidLabelOverlap: true,
          label: { show: false },
          data: chartData
        }
      ]
    },
    true
  )
}

const renderShortageChart = () => {
  if (!shortageChartRef.value) return
  const rows = dashboardData.shortageTop?.length
    ? dashboardData.shortageTop
    : [{ name: '暂无缺料', value: 0 }]
  shortageChart = shortageChart || echarts.init(shortageChartRef.value)
  shortageChart.setOption(
    {
      color: ['#0079c1'],
      grid: { left: 96, right: 32, top: 12, bottom: 18 },
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'shadow' },
        formatter: (items) => {
          const item = items?.[0]
          if (!item) return ''
          const row = rows[item.dataIndex] || {}
          const materialName = row.materialName ? `<br/>${row.materialName}` : ''
          return `${item.name}${materialName}<br/>未齐套数量：${formatNumber(item.value)}`
        }
      },
      xAxis: {
        type: 'value',
        splitLine: { lineStyle: { color: '#edf0f5' } },
        axisLine: { show: false }
      },
      yAxis: {
        type: 'category',
        inverse: true,
        data: rows.map((item) => item.name || '-'),
        axisTick: { show: false },
        axisLine: { show: false },
        axisLabel: {
          width: 88,
          overflow: 'truncate'
        }
      },
      series: [
        {
          type: 'bar',
          barWidth: 12,
          data: rows.map((item) => normalizeNumber(item.value)),
          label: {
            show: true,
            position: 'right',
            color: '#595959',
            fontSize: 12,
            formatter: ({ value }) => formatNumber(value)
          }
        }
      ]
    },
    true
  )
}

const resizeCharts = () => {
  trendChart?.resize()
  messageChart?.resize()
  shortageChart?.resize()
}

const renderCharts = () => {
  renderTrendChart()
  renderMessageChart()
  renderShortageChart()
}

const applyFilters = () => {
  window.clearTimeout(refreshTimer)
  refreshTimer = window.setTimeout(() => {
    refreshDashboard(false)
  }, 260)
}

const setRiskFilter = (value) => {
  filters.risk = value
}

const openTarget = (path) => {
  if (!path) return
  router.push(path)
}

onMounted(() => {
  refreshDashboard(false)
  window.addEventListener('resize', resizeCharts)
})

onBeforeUnmount(() => {
  window.clearTimeout(refreshTimer)
  window.removeEventListener('resize', resizeCharts)
  trendChart?.dispose()
  messageChart?.dispose()
  shortageChart?.dispose()
})
</script>

<style lang="less" scoped>
.order-home-dashboard {
  min-height: 100%;
  padding: 18px;
  box-sizing: border-box;
  background:
    linear-gradient(180deg, rgba(168, 179, 201, 0.22), rgba(255, 255, 255, 0) 230px),
    #fdfdfd;
  color: #595959;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  align-items: flex-start;
  margin-bottom: 14px;

  h1 {
    margin: 0;
    color: #303384;
    font-size: 28px;
    line-height: 1.25;
    font-weight: 800;
  }

  p {
    margin: 8px 0 0;
    color: #595959;
    font-size: 14px;
  }
}

.dashboard-kicker {
  margin-bottom: 8px;
  color: #0079c1;
  font-size: 13px;
  font-weight: 700;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.refresh-time {
  min-width: 190px;
  padding: 9px 12px;
  border: 1px solid #e5e9e9;
  border-radius: 8px;
  background: #ffffff;
  color: #9c9c9f;
  font-size: 12px;
  line-height: 1.45;

  span,
  strong,
  em {
    display: block;
  }

  strong {
    color: #303384;
    font-size: 14px;
    font-weight: 700;
  }

  em {
    font-style: normal;
  }
}

.filter-panel {
  display: grid;
  grid-template-columns: repeat(4, minmax(150px, 1fr)) minmax(360px, 1.45fr);
  gap: 12px;
  align-items: end;
  padding: 14px;
  margin-bottom: 14px;
  border: 1px solid #e5e9e9;
  border-radius: 8px;
  background: #ffffff;
  box-shadow: 0 8px 24px rgba(48, 51, 132, 0.06);
}

.filter-item {
  span {
    display: block;
    margin-bottom: 6px;
    color: #595959;
    font-size: 12px;
    font-weight: 700;
  }

  :deep(.el-select) {
    width: 100%;
  }
}

.quick-tabs {
  display: grid;
  grid-template-columns: repeat(5, minmax(72px, 1fr));
  gap: 6px;

  button {
    height: 32px;
    border: 1px solid #d9e1ec;
    border-radius: 6px;
    background: #fdfdfd;
    color: #595959;
    cursor: pointer;
    font-size: 12px;
    transition:
      color 0.2s,
      border-color 0.2s,
      background 0.2s;

    &.active {
      border-color: #0079c1;
      background: rgba(0, 121, 193, 0.1);
      color: #0079c1;
      font-weight: 700;
    }
  }
}

.kpi-grid {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: 12px;
  margin-bottom: 14px;
}

.kpi-card {
  display: flex;
  gap: 12px;
  min-height: 104px;
  padding: 16px;
  border: 1px solid #e5e9e9;
  border-radius: 8px;
  background: #ffffff;
  cursor: pointer;
  box-shadow: 0 8px 24px rgba(48, 51, 132, 0.06);
  transition:
    transform 0.2s,
    box-shadow 0.2s;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 12px 28px rgba(48, 51, 132, 0.12);
  }

  &.danger {
    border-color: rgba(242, 79, 93, 0.26);

    .kpi-icon {
      background: rgba(242, 79, 93, 0.1);
      color: #d9363e;
    }
  }

  &.warning {
    border-color: rgba(238, 173, 71, 0.3);

    .kpi-icon {
      background: rgba(238, 173, 71, 0.13);
      color: #b36b00;
    }
  }
}

.kpi-icon {
  display: grid;
  flex: 0 0 42px;
  width: 42px;
  height: 42px;
  place-items: center;
  border-radius: 8px;
  background: rgba(0, 121, 193, 0.1);
  color: #0079c1;
  font-size: 21px;
}

.kpi-content {
  min-width: 0;
}

.kpi-label {
  color: #595959;
  font-size: 13px;
  font-weight: 700;
}

.kpi-value {
  margin-top: 7px;
  color: #23277d;
  font-size: 28px;
  line-height: 1;
  font-weight: 800;

  span {
    margin-left: 4px;
    color: #9c9c9f;
    font-size: 13px;
    font-weight: 600;
  }
}

.kpi-trend {
  margin-top: 9px;
  color: #9c9c9f;
  font-size: 12px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.chart-grid,
.diagnosis-grid {
  display: grid;
  gap: 14px;
  margin-bottom: 14px;
}

.chart-grid {
  grid-template-columns: minmax(0, 2fr) minmax(320px, 1fr);
}

.diagnosis-grid {
  grid-template-columns: minmax(0, 1.2fr) minmax(320px, 0.95fr) minmax(360px, 1fr);
}

.panel {
  min-width: 0;
  padding: 16px;
  border: 1px solid #e5e9e9;
  border-radius: 8px;
  background: #ffffff;
  box-shadow: 0 8px 24px rgba(48, 51, 132, 0.06);
}

.panel-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;

  h2 {
    margin: 0;
    color: #303384;
    font-size: 16px;
    line-height: 1.3;
    font-weight: 800;
  }

  p {
    margin: 5px 0 0;
    color: #9c9c9f;
    font-size: 12px;
  }
}

.chart-box {
  width: 100%;
  height: 310px;
}

.chart-donut {
  height: 310px;
}

.compact-chart {
  height: 280px;
}

.progress-panel {
  min-height: 328px;
}

.progress-list {
  display: flex;
  flex-direction: column;
  gap: 18px;
  padding-top: 8px;
}

.progress-meta {
  display: flex;
  justify-content: space-between;
  margin-bottom: 7px;
  color: #595959;
  font-size: 13px;
  font-weight: 700;

  strong {
    color: #303384;
  }
}

.stacked-progress {
  display: flex;
  width: 100%;
  height: 24px;
  overflow: hidden;
  border-radius: 6px;
  background: #e5e9e9;

  span {
    min-width: 0;
    color: #ffffff;
    font-size: 11px;
    line-height: 24px;
    text-align: center;
    white-space: nowrap;
  }

  .done {
    background: #0079c1;
  }

  .doing {
    background: #62bf5e;
  }

  .todo {
    background: #a8b3c9;
  }
}

.progress-legend {
  display: flex;
  gap: 12px;
  margin-top: 18px;
  color: #9c9c9f;
  font-size: 12px;

  span {
    display: inline-flex;
    align-items: center;
    gap: 5px;
  }

  i {
    width: 8px;
    height: 8px;
    border-radius: 50%;

    &.done {
      background: #0079c1;
    }

    &.doing {
      background: #62bf5e;
    }

    &.todo {
      background: #a8b3c9;
    }
  }
}

.risk-matrix {
  display: grid;
  grid-template-columns: minmax(120px, 1.6fr) repeat(4, minmax(54px, 0.7fr));
  overflow: hidden;
  border: 1px solid #e5e9e9;
  border-radius: 8px;
}

.risk-head,
.risk-name,
.risk-cell {
  min-height: 38px;
  padding: 9px;
  border-right: 1px solid #e5e9e9;
  border-bottom: 1px solid #e5e9e9;
  box-sizing: border-box;
  font-size: 12px;
}

.risk-head {
  background: #f4f7fb;
  color: #303384;
  font-weight: 800;
}

.risk-name {
  color: #595959;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.risk-cell {
  text-align: center;
  font-weight: 800;

  &.normal {
    color: #248a3d;
  }

  &.warning {
    color: #b36b00;
  }

  &.delay,
  &.overdue {
    color: #d9363e;
  }
}

.todo-panel {
  margin-bottom: 10px;

  .panel-header {
    align-items: center;
  }

  :deep(.el-input) {
    width: 300px;
  }
}

.type-dot {
  display: inline-block;
  width: 8px;
  height: 8px;
  margin-right: 6px;
  border-radius: 50%;
  background: #0079c1;

  &.warning {
    background: #eead47;
  }

  &.danger {
    background: #f24f5d;
  }

  &.primary {
    background: #0079c1;
  }
}

@media (max-width: 1480px) {
  .kpi-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }

  .diagnosis-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .risk-panel {
    grid-column: 1 / -1;
  }
}

@media (max-width: 1080px) {
  .dashboard-header,
  .header-actions,
  .panel-header {
    flex-direction: column;
    align-items: stretch;
  }

  .filter-panel,
  .chart-grid,
  .diagnosis-grid {
    grid-template-columns: 1fr;
  }

  .kpi-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .quick-tabs {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }

  .todo-panel :deep(.el-input) {
    width: 100%;
  }
}

@media (max-width: 640px) {
  .order-home-dashboard {
    padding: 12px;
  }

  .dashboard-header h1 {
    font-size: 22px;
  }

  .kpi-grid {
    grid-template-columns: 1fr;
  }

  .quick-tabs {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .chart-box,
  .chart-donut,
  .compact-chart {
    height: 260px;
  }
}
</style>
