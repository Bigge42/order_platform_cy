<!-- src/views/order/wz_productionoutput/WZ_ProductionOutput.vue -->
<template>
  <div :class="[{ compact: state.compact }]">
    <!-- 顶部工具栏 -->
    <header class="ph-header">
      <div class="ph-container">
        <div style="min-width:260px;flex:1 1 auto">
          <div class="ph-title">产能 / 产线看板</div>
          <div class="ph-sub">同一年内选择日期；支持大格+数字/紧凑视图；渐变或阈值红绿</div>
        </div>

        <!-- 年份 -->
        <el-select v-model="state.year" size="small" style="width:112px" @change="onYearChange">
          <el-option v-for="y in yearOptions" :key="y" :label="`${y} 年`" :value="y" />
        </el-select>

        <!-- 颜色范围 -->
        <el-select v-model="state.scope" size="small" style="width:156px" @change="renderAll">
          <el-option label="颜色范围：按产线" value="line" />
          <el-option label="颜色范围：按阀体" value="valve" />
          <el-option label="颜色范围：全局" value="global" />
        </el-select>

        <!-- 配色方案 -->
        <el-select v-model="state.mode" size="small" style="width:140px" @change="renderAll">
          <el-option label="配色：标准渐变" value="gradient" />
          <el-option label="配色：阈值红绿" value="threshold" />
        </el-select>

        <!-- 日期范围（同一年） -->
        <span class="ph-sub">范围：</span>
        <el-date-picker
          v-model="dateRangeModel"
          type="daterange"
          size="small"
          style="width:248px"
          unlink-panels
          range-separator="至"
          start-placeholder="开始日期"
          end-placeholder="结束日期"
          :disabled-date="disabledRange"
        />

        <el-button class="btn-ghost" size="small" @click="resetFullYear">重置全年</el-button>
        <el-button class="btn-ghost" size="small" @click="toggleBig">
          {{ state.big ? '标准尺寸' : '大格 + 显示数字' }}
        </el-button>
        <el-button class="btn-ghost" size="small" @click="toggleCompact">
          {{ state.compact ? '还原布局' : '紧凑视图' }}
        </el-button>
        <el-button class="btn-amber" size="small" @click="openThresholdModal">编辑阈值</el-button>

        <!-- 可选筛选（按需传给后端） -->
        <el-input v-model="valveCategory" placeholder="阀体类别(可空)" size="small" style="width:140px" clearable />
        <el-input v-model="productionLine" placeholder="产线(可空)" size="small" style="width:120px" clearable />

        <el-button v-if="canSyncData" type="primary" size="small" :loading="syncLoading" @click="syncData">同步数据</el-button>
        <el-button type="primary" size="small" @click="loadData">加载数据</el-button>
        <el-button-group class="mode-switch" :style="{ '--mode-color': currentViewMode.color }">
          <el-button size="small" :class="modeButtonClass('actual')" @click="loadData">仅看实际</el-button>
          <el-button size="small" :class="modeButtonClass('preproduction')" @click="loadPreProduction">展示预排产</el-button>
          <el-button size="small" :class="modeButtonClass('optimized')" @click="loadOptimizedPreProduction">展示排产优化</el-button>
        </el-button-group>
        <span class="mode-current" :style="{ color: currentViewMode.color, borderColor: currentViewMode.color }">
          当前口径：{{ currentViewMode.label }}
        </span>
        <el-button size="small" @click="exportData">导出数据</el-button>
        <el-button size="small" :loading="unknownExportLoading" @click="exportUnknownData">导出未知产线</el-button>
      </div>
    </header>

    <!-- 统计卡片 -->
    <section class="ph-grid3" id="stats">
      <div class="ph-card stat">
        <div class="label">可视范围总产量</div>
        <div class="value">{{ stats.total.toLocaleString() }}</div>
        <div class="sub">{{ stats.rangeText }}</div>
      </div>
      <div class="ph-card stat">
        <div class="label">可视范围日均（每条产线）</div>
        <div class="value">{{ stats.avg }}</div>
        <div class="sub">{{ lineCountText }}</div>
      </div>
      <div class="ph-card stat">
        <div class="label">可视范围峰值</div>
        <div class="value">{{ stats.peak.value }}</div>
        <div class="sub">{{ stats.peak.sub }}</div>
      </div>
    </section>

    <!-- 主图（每个阀体一个 SVG 热力图） -->
    <section id="charts" class="ph-charts" ref="chartsEl"></section>

    <!-- 悬浮提示 -->
    <div id="tooltip" class="tooltip" style="display:none"></div>

    <!-- 同步弹窗 -->
    <el-dialog
      v-model="syncDialog"
      title="产能数据同步"
      width="860px"
      class="sync-dialog"
    >
      <div class="sync-summary">
        <div>
          <div class="sync-title">最近定时增量更新记录</div>
          <div class="sync-sub">近一年指同步接口入参时间窗口，返回数据仍按接口里的排产日期归集产能。</div>
        </div>
        <el-button size="small" :loading="syncHistoryLoading" @click="loadSyncHistory">刷新记录</el-button>
      </div>

      <el-table
        :data="syncHistory"
        size="small"
        border
        stripe
        v-loading="syncHistoryLoading"
        empty-text="暂无定时增量记录"
        style="width:100%;margin-top:12px"
      >
        <el-table-column label="开始时间" width="154">
          <template #default="{ row }">{{ fmtDateTime(row.startTime ?? row.StartTime) }}</template>
        </el-table-column>
        <el-table-column label="结束时间" width="154">
          <template #default="{ row }">{{ fmtDateTime(row.endTime ?? row.EndTime) }}</template>
        </el-table-column>
        <el-table-column label="结果" width="80" align="center">
          <template #default="{ row }">
            <el-tag size="small" :type="(row.success ?? row.Success) ? 'success' : 'danger'">
              {{ (row.success ?? row.Success) ? '成功' : '失败' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="耗时" width="82" align="right">
          <template #default="{ row }">{{ row.elapsedSeconds ?? row.ElapsedSeconds ?? '-' }} 秒</template>
        </el-table-column>
        <el-table-column label="返回内容" min-width="260" show-overflow-tooltip>
          <template #default="{ row }">{{ row.responseContent ?? row.ResponseContent ?? row.errorMsg ?? row.ErrorMsg ?? '' }}</template>
        </el-table-column>
      </el-table>

      <div class="sync-window">
        <div class="label">初始化全量同步窗口</div>
        <div class="value">{{ syncRangeText }}</div>
      </div>

      <template #footer>
        <el-button @click="syncDialog=false">关闭</el-button>
        <el-button type="primary" :loading="syncLoading" @click="runFullSync">初始化全量同步（近一年）</el-button>
      </template>
    </el-dialog>

    <!-- 阈值弹窗 -->
    <el-dialog
      v-model="thrDialog"
      title="编辑阈值"
      width="720px"
      class="threshold-dialog"
    >
      <div class="grid-lines">
        <div v-for="cat in state.categories" :key="cat.name" class="thr-section">
          <div class="thr-title">{{ cat.name }}</div>
          <div class="thr-lines">
            <div v-for="l in cat.lines" :key="l" class="line-edit">
              <span class="line-name">{{ l }}</span>
              <!-- 受控写法，避免 v-model 函数调用报错 -->
              <el-input-number
                :model-value="getThr(cat.name, l)"
                @update:modelValue="val => setThr(cat.name, l, val)"
                :min="0"
                :step="1"
              />
            </div>
          </div>
        </div>
      </div>
      <template #footer>
        <el-button @click="thrDialog=false">取消</el-button>
        <el-button type="primary" @click="saveThresholds">保存</el-button>
      </template>
    </el-dialog>

    <!-- 单元格明细弹窗 -->
    <el-dialog
      v-model="cellDetailDialog"
      title="产能占用明细"
      width="980px"
      class="cell-detail-dialog"
    >
      <div class="detail-summary">
        <div>
          <div class="detail-title">{{ cellDetailContext.date }} · {{ cellDetailContext.valveCategory }} · {{ cellDetailContext.productionLine }}</div>
          <div class="detail-sub">格子数量 {{ formatQty(cellDetailContext.quantity) }}，明细合计 {{ formatQty(cellDetailTotal) }}</div>
        </div>
        <el-tag size="small" type="info">{{ cellDetailPageText }}</el-tag>
      </div>
      <el-table
        :data="cellDetailRows"
        size="small"
        border
        stripe
        v-loading="cellDetailLoading"
        empty-text="当前格子暂无订单明细"
        max-height="520"
        style="width:100%;margin-top:12px"
      >
        <el-table-column label="订单号" prop="billNo" min-width="150" show-overflow-tooltip />
        <el-table-column label="计划跟踪号" prop="planTrackingNo" min-width="180" show-overflow-tooltip />
        <el-table-column label="物料号" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">{{ row.materialCode || row.MaterialCode || row.materialId || row.MaterialId || row.materialKey || row.MaterialKey || '-' }}</template>
        </el-table-column>
        <el-table-column label="规格型号" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">{{ row.specModel || row.SpecModel || row.productModel || row.ProductModel || '-' }}</template>
        </el-table-column>
        <el-table-column label="序号" prop="seq" width="72" align="right" />
        <el-table-column label="数量" width="92" align="right">
          <template #default="{ row }">{{ formatQty(row.quantity ?? row.Quantity ?? 0) }}</template>
        </el-table-column>
        <el-table-column label="状态" prop="classifyStatus" width="132" show-overflow-tooltip />
      </el-table>
      <div class="detail-pager">
        <span class="detail-page-info">{{ cellDetailPageText }}</span>
        <el-pagination
          v-if="cellDetailPager.totalRows > cellDetailPager.pageSize"
          small
          background
          layout="prev, pager, next, jumper"
          :current-page="cellDetailPager.page"
          :page-size="cellDetailPager.pageSize"
          :total="cellDetailPager.totalRows"
          :disabled="cellDetailLoading"
          @current-change="loadCellDetailsPage"
        />
      </div>
      <template #footer>
        <el-button @click="cellDetailDialog=false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, nextTick, getCurrentInstance } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import store from '@/store/index'

/* ===== 尺寸参数 ===== */
const SIZE = { small: 12, big: 20 }
const GAP  = { normal: 2, compact: 1 }
const PAD  = { normal: 32, compact: 24 }
const LABEL_GAP = { normal: 10, compact: 6 }
const LINE_LABEL_WIDTH = { normal: 76, compact: 68, max: 140 }
const GITHUB = { size: 9, gap: 2, pad: 22, labelGap: 16, rows: 7 }
const MONTH_SCALE_MAX_DAYS = 45
const WEEKDAY_TEXT = ['日', '一', '二', '三', '四', '五', '六']
const GITHUB_WEEKDAYS = ['周一', '周二', '周三', '周四', '周五', '周六', '周日']
const DATE_AXIS = {
  monthColor: '#475569',
  dateColor: '#64748b',
  tickColor: '#cbd5e1',
  monthLineColor: '#dbe3ee',
  hoverColor: '#303384',
  weekdayColor: '#64748b',
  weekendColor: '#b45309',
  weekendBg: '#fff7ed'
}
const SUMMARY_AXIS = {
  textColor: '#334155',
  mutedColor: '#94a3b8',
  labelColor: '#475569',
  cellFill: '#dbeafe',
  cellEmptyFill: '#f8fafc',
  cellBorder: '#e2e8f0'
}

/* ===== 工具函数 ===== */
const fmtYMD = d => `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`
const fmtDateTime = value => {
  if (!value) return '-'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return String(value).replace('T', ' ').slice(0, 19)
  return `${fmtYMD(d)} ${String(d.getHours()).padStart(2,'0')}:${String(d.getMinutes()).padStart(2,'0')}:${String(d.getSeconds()).padStart(2,'0')}`
}
function getInterfaceSyncRange(){
  const end = new Date()
  const start = new Date(end)
  start.setFullYear(start.getFullYear() - 1)
  return { start: fmtYMD(start), end: fmtYMD(end) }
}
function daysBetween(start, end) {
  const s = new Date(start.getFullYear(), start.getMonth(), start.getDate())
  const e = new Date(end.getFullYear(), end.getMonth(), end.getDate())
  const arr = []
  for (let d = new Date(s); d <= e; d.setDate(d.getDate() + 1)) arr.push(new Date(d))
  return arr
}
function buildIdxMap(days) { return new Map(days.map((d,i)=>[fmtYMD(d), i])) }
const lerp = (a,b,t) => a + (b-a) * t
function colorFromValue(value,max){
  if(max<=0 || value<=0) return '#e5f0ff'
  const t=Math.min(1,value/max), l=lerp(90,35,t), s=lerp(60,85,t)
  return `hsl(220 ${s}% ${l}%)`
}
function colorFromThreshold(value, threshold, maxAbove, maxBelow){
  if(threshold==null || Number.isNaN(threshold)) return colorFromValue(value, Math.max(threshold||0, value))
  if(value>threshold){
    const span=Math.max(1,maxAbove||value-threshold), t=Math.min(1,(value-threshold)/span)
    const l=lerp(90,35,t), s=lerp(60,85,t); return `hsl(0 ${s}% ${l}%)`      // 红
  }else{
    const span=Math.max(1,maxBelow||threshold), t=Math.min(1,(threshold-value)/span)
    const l=lerp(90,35,t), s=lerp(40,75,t); return `hsl(140 ${s}% ${l}%)`    // 绿
  }
}
function textColorForCell(val, thr, colorMax, mode, aboveMaxSpan, belowMaxSpan){
  let strong=0
  if(mode==='gradient'){ strong = colorMax>0 ? (val/colorMax) : 0 }
  else{
    if(val>thr){ const span=Math.max(1, aboveMaxSpan); strong = (val-thr)/span }
    else       { const span=Math.max(1, belowMaxSpan); strong = (thr-val)/span }
  }
  return strong>0.6 ? '#ffffff' : '#0f172a'
}
const getProductionDateStr = rec => {
  const v = rec?.ProductionDate ?? rec?.productionDate
  if (!v) return ''
  // 不做复杂时区解析，直接取前 10 位，足够与索引匹配
  return String(v).slice(0,10)
}
const getYearMonth = rec => {
  const v = rec?.ProductionDate ?? rec?.productionDate ?? ''
  // 导出时按“YYYY-MM”
  return String(v).slice(0,7)
}
const cellKeySeparator = '\u001F'
function buildCellKey(valve, line, date){
  const d = typeof date === 'string' ? date : fmtYMD(date)
  return [String(valve || '').trim(), String(line || '').trim(), d].join(cellKeySeparator)
}
function actualBaselineSignature(){
  return [
    fmtYMD(state.rangeStart),
    fmtYMD(state.rangeEnd),
    valveCategory.value?.trim() || '',
    productionLine.value?.trim() || ''
  ].join(cellKeySeparator)
}
function parseRowsResponse(res){
  return Array.isArray(res) ? res :
    Array.isArray(res?.data) ? res.data :
      Array.isArray(res?.Data) ? res.Data :
        Array.isArray(res?.result) ? res.result : []
}
function buildCellBaseline(rows){
  const map = {}
  for (const r of rows || []){
    const v = String(r.valveCategory ?? r.ValveCategory ?? '').trim()
    const l = String(r.productionLine ?? r.ProductionLine ?? '').trim()
    const d = getProductionDateStr(r)
    const q = Number(r.quantity ?? r.Quantity ?? 0)
    if (!v || !l || !d || !Number.isFinite(q)) continue
    const key = buildCellKey(v, l, d)
    map[key] = Number(map[key] || 0) + q
  }
  return map
}
function storeActualBaseline(rows){
  actualCellBaseline.value = buildCellBaseline(rows)
  actualCellBaselineMeta.value = actualBaselineSignature()
}
async function ensureActualBaseline(){
  if (actualCellBaselineMeta.value === actualBaselineSignature()) return
  const qs = new URLSearchParams()
  qs.set('start', fmtYMD(state.rangeStart))
  qs.set('end', fmtYMD(state.rangeEnd))
  if (valveCategory.value?.trim()) qs.set('valveCategory', valveCategory.value.trim())
  if (productionLine.value?.trim()) qs.set('productionLine', productionLine.value.trim())
  const res = await proxy?.http?.get(`/api/WZ/ProductionOutput?${qs.toString()}`, {}, true)
  storeActualBaseline(parseRowsResponse(res))
}
function getActualCellValue(valve, line, date){
  const value = actualCellBaseline.value?.[buildCellKey(valve, line, date)]
  const num = Number(value ?? 0)
  return Number.isFinite(num) ? num : 0
}
function hasChangedCell(valve, line, date, value){
  if (viewMode.value === 'actual' || actualCellBaselineMeta.value !== actualBaselineSignature()) return false
  return Math.abs(Number(value || 0) - getActualCellValue(valve, line, date)) > 0.000001
}
function formatSignedQty(value){
  const num = Number(value || 0)
  if (num > 0) return `+${formatQty(num)}`
  return formatQty(num)
}

/* ===== 状态（动态阀体/产线） ===== */
const state = reactive({
  year: new Date().getFullYear(),
  rangeStart: new Date(new Date().getFullYear(),0,1),
  rangeEnd:   new Date(new Date().getFullYear(),11,31),
  scope: 'line',       // line|valve|global
  mode: 'gradient',    // gradient|threshold
  categories: [],      // [{ name, lines: [] }]
  data: {},            // { [valve]: { [line]: number[] } }
  thresholds: {},      // { [valve]: { [line]: number } }
  githubLine: {},      // { [valve]: string | null }
  big: false,
  compact: false,
})

const thrDialog = ref(false)
const thrDraft  = ref({}) // 弹窗草稿
const chartsEl  = ref(null)
const { proxy } = getCurrentInstance() || {}
const viewMode = ref('actual')
const viewModeOptions = {
  actual: { label: '仅看实际', color: '#303384', cellBorder: '#ffffff' },
  preproduction: { label: '展示预排产', color: '#0079C1', cellBorder: '#0079C1' },
  optimized: { label: '展示排产优化', color: '#046BB6', cellBorder: '#046BB6' }
}
const currentViewMode = computed(() => viewModeOptions[viewMode.value] || viewModeOptions.actual)
const syncLoading = ref(false)
const syncDialog = ref(false)
const syncHistoryLoading = ref(false)
const syncHistory = ref([])
const unknownExportLoading = ref(false)
const manualRuleImportLoading = ref(false)
const manualRuleFileInput = ref(null)
const cellDetailDialog = ref(false)
const cellDetailLoading = ref(false)
const cellDetailRows = ref([])
const cellDetailPager = reactive({
  page: 1,
  pageSize: 200,
  totalRows: 0,
  totalQuantity: 0
})
const cellDetailContext = reactive({
  date: '',
  valveCategory: '',
  productionLine: '',
  quantity: 0
})
let cellDetailRequestSeq = 0
const cellDetailTotal = computed(() => Number(cellDetailPager.totalQuantity || 0))
const cellDetailPageText = computed(() => {
  const total = Number(cellDetailPager.totalRows || 0)
  if (!total) return '共 0 条'
  const page = Math.max(1, Number(cellDetailPager.page || 1))
  const pageSize = Math.max(1, Number(cellDetailPager.pageSize || 200))
  const start = (page - 1) * pageSize + 1
  const end = Math.min(start + cellDetailRows.value.length - 1, total)
  return `共 ${total} 条，当前 ${start}-${end}`
})
const canSyncData = computed(() => {
  const userInfo = store.getters.getUserInfo?.() || {}
  const names = [
    userInfo.userName,
    userInfo.UserName,
    userInfo.loginName,
    userInfo.LoginName,
    store.getters.getUserName?.(),
    store.getters.getLoginName?.()
  ]
  return names.some(name => String(name || '').trim().toLowerCase() === 'cyadmin')
})
const syncRangeText = computed(() => {
  const { start, end } = getInterfaceSyncRange()
  return `${start} ~ ${end}`
})

/* 原始返回数据（用于导出） */
const rawRows = ref([])
const actualCellBaseline = ref({})
const actualCellBaselineMeta = ref('')

/* 年份选项（±3 年） */
const yearOptions = Array.from({length:7}, (_,i)=> state.year - 3 + i)

/* 日期控件 */
const disabledRange  = d => d.getFullYear() !== state.year
const dateRangeModel = computed({
  get(){
    return [state.rangeStart, state.rangeEnd]
  },
  set(value){
    if (!Array.isArray(value) || value.length < 2 || !value[0] || !value[1]) return
    const start = new Date(value[0])
    const end = new Date(value[1])
    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return
    state.rangeStart = start <= end ? start : end
    state.rangeEnd = start <= end ? end : start
    renderAll()
  }
})
function onYearChange(){
  state.rangeStart = new Date(state.year,0,1)
  state.rangeEnd   = new Date(state.year,11,31)
  renderAll()
}
function resetFullYear(){
  state.rangeStart = new Date(state.year,0,1)
  state.rangeEnd   = new Date(state.year,11,31)
  renderAll()
}
function toggleBig(){ state.big=!state.big; renderAll() }
function toggleCompact(){ state.compact=!state.compact; nextTick(()=>renderAll()) }

/* 额外查询参数（可空） */
const valveCategory  = ref('')
const productionLine = ref('')

/* 统计卡片 */
const stats = reactive({
  total: 0,
  avg: 0,
  peak: { value: 0, sub: '-' },
  rangeText: ''
})
const lineCountText = computed(()=>{
  const n = state.categories.reduce((acc,c)=> acc + c.lines.length, 0)
  return `${n} 条线`
})

/* 阈值（绘图时用，缺省 20） */
function ensureThr(valve, line){
  if(!state.thresholds[valve]) state.thresholds[valve] = {}
  if(typeof state.thresholds[valve][line] !== 'number') state.thresholds[valve][line] = 20
  return state.thresholds[valve][line]
}

/* 阈值对话框受控取值/赋值 */
function getThr(valve, line){
  const v = thrDraft.value?.[valve]?.[line]
  return typeof v === 'number' ? v : 20
}
function setThr(valve, line, val){
  if (!thrDraft.value[valve]) thrDraft.value[valve] = {}
  thrDraft.value[valve][line] = Number(val ?? 0)
}
function formatQty(value){
  const num = Number(value ?? 0)
  if (!Number.isFinite(num)) return '0'
  return num.toLocaleString('zh-CN', { maximumFractionDigits: 6 })
}
function isUnknownValveCategory(value){
  return String(value || '').trim() === '未知阀类'
}
function compareCategoryName(a, b){
  const au = isUnknownValveCategory(a)
  const bu = isUnknownValveCategory(b)
  if (au !== bu) return au ? 1 : -1
  return String(a || '').localeCompare(String(b || ''), 'zh-Hans-CN')
}
function compareLineName(a, b){
  const au = String(a || '').trim() === '未知产线'
  const bu = String(b || '').trim() === '未知产线'
  if (au !== bu) return au ? 1 : -1
  const na = String(a || '').match(/\d+/)?.[0]
  const nb = String(b || '').match(/\d+/)?.[0]
  if (na && nb && na !== nb) return Number(na) - Number(nb)
  return String(a || '').localeCompare(String(b || ''), 'zh-Hans-CN')
}
function estimateLineLabelWidth(lines){
  const minWidth = state.compact ? LINE_LABEL_WIDTH.compact : LINE_LABEL_WIDTH.normal
  const maxTextWidth = (lines || []).reduce((max, line) => {
    const width = Array.from(String(line || '')).reduce((sum, ch) => {
      return sum + (/[\u4e00-\u9fa5]/.test(ch) ? 10 : 6)
    }, 0)
    return Math.max(max, width)
  }, 0)
  return Math.min(LINE_LABEL_WIDTH.max, Math.max(minWidth, maxTextWidth))
}
function getGithubLine(valve){
  return state.githubLine?.[valve] || null
}
function toggleGithubLine(valve, line){
  if (!state.githubLine) state.githubLine = {}
  state.githubLine[valve] = state.githubLine[valve] === line ? null : line
  renderAll()
}
function setSvgAttrs(el, attrs){
  Object.entries(attrs).forEach(([key, value]) => {
    if (value == null) return
    el.setAttribute(key, String(value))
  })
  return el
}
function createSvgEl(svgNS, tag, attrs = {}){
  return setSvgAttrs(document.createElementNS(svgNS, tag), attrs)
}
function isWeekend(date){
  const day = date.getDay()
  return day === 0 || day === 6
}
function weekdayText(date){
  return WEEKDAY_TEXT[date.getDay()]
}
function formatAxisQty(value){
  const num = Number(value || 0)
  if (!Number.isFinite(num)) return '0'
  const abs = Math.abs(num)
  if (abs >= 10000) {
    const digits = abs >= 100000 ? 0 : 1
    return `${(num / 10000).toLocaleString('zh-CN', { maximumFractionDigits: digits })}万`
  }
  return formatQty(num)
}
function estimateTextWidth(text, fontSize = 10){
  return Array.from(String(text || '')).reduce((sum, ch) => {
    if (/[\u4e00-\u9fa5]/.test(ch)) return sum + fontSize
    if (/[,\.\-]/.test(ch)) return sum + fontSize * 0.35
    return sum + fontSize * 0.58
  }, 0)
}
function bindSvgTooltip(el, content){
  if (!content) return
  el.addEventListener('mouseenter', ev => {
    const tip = document.getElementById('tooltip')
    if (!tip) return
    tip.textContent = content
    tip.style.display = 'block'
    tip.style.left = `${ev.clientX + 12}px`
    tip.style.top = `${ev.clientY + 12}px`
  })
  el.addEventListener('mousemove', ev => {
    const tip = document.getElementById('tooltip')
    if (!tip) return
    tip.style.left = `${ev.clientX + 12}px`
    tip.style.top = `${ev.clientY + 12}px`
  })
  el.addEventListener('mouseleave', () => {
    const tip = document.getElementById('tooltip')
    if (tip) tip.style.display = 'none'
  })
}
function shortDateText(date, daysCount){
  if (daysCount <= 45) return String(date.getDate())
  return `${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
}
function buildDateAxisTicks(days, useGithub, githubOffset, cellSize, gap){
  const daysCount = days.length
  const stepPx = cellSize + gap
  const minGap = useGithub ? 52 : daysCount <= 45 ? stepPx - 0.1 : daysCount <= 100 ? 46 : 76
  let lastX = -Infinity
  const ticks = []

  days.forEach((d, k) => {
    const xIndex = useGithub ? Math.floor((k + githubOffset) / GITHUB.rows) : k
    const xCenter = xIndex * stepPx + cellSize / 2
    const isRangeStart = k === 0
    const isMonthStart = d.getDate() === 1
    const isWeekStart = useGithub ? (k + githubOffset) % GITHUB.rows === 0 : d.getDay() === 1
    let shouldShow = isRangeStart || isMonthStart

    if (!shouldShow) {
      if (useGithub) shouldShow = isWeekStart
      else if (daysCount <= 45) shouldShow = true
      else if (daysCount <= 100) shouldShow = k % 3 === 0
      else shouldShow = isWeekStart
    }

    if (!shouldShow) return
    if (!isRangeStart && !isMonthStart && xCenter - lastX < minGap) return

    ticks.push({
      index: k,
      xIndex,
      label: shortDateText(d, daysCount),
      isMonthStart
    })
    lastX = xCenter
  })

  return ticks
}

/* 渲染（SVG） */
function renderAll(){
  const days = daysBetween(state.rangeStart, state.rangeEnd)
  const daysCount = days.length
  if (daysCount <= 0) return

  // 统计
  let total=0, peakVal=-1, peak={ valve:'-', line:'-', date:'-', value:0 }
  for(const cat of state.categories){
    for(const l of cat.lines){
      const arr = state.data?.[cat.name]?.[l] || []
      for(let i=0;i<daysCount;i++){
        const vv=arr[i] ?? 0
        total+=vv
        if(vv>peakVal){ peakVal=vv; peak={ valve:cat.name, line:l, date:fmtYMD(days[i]), value:vv } }
      }
    }
  }
  stats.total = total
  const totalLines = state.categories.reduce((acc,c)=> acc + c.lines.length, 0) || 1
  stats.avg   = Math.round((total/(totalLines*daysCount))*100)/100
  stats.peak  = { value: peak.value, sub: `${peak.valve} · ${peak.line} · ${peak.date}` }
  stats.rangeText = `${fmtYMD(days[0])} ~ ${fmtYMD(days[daysCount-1])}（${daysCount} 天）`

  // 画布
  const baseCellSize = state.big ? SIZE.big : SIZE.small
  const baseGap = state.compact ? GAP.compact : GAP.normal
  const basePad = state.compact ? PAD.compact : PAD.normal
  const baseLabelGap = state.compact ? LABEL_GAP.compact : LABEL_GAP.normal
  const svgNS='http://www.w3.org/2000/svg'
  const container = chartsEl.value
  container.style.gap = state.compact ? '12px' : '16px'
  container.style.padding = state.compact ? '0 12px 12px' : '0 16px 16px'
  container.innerHTML=''

  // max（line/valve/global）
  const valveMax = {}
  let globalMax = 0
  for(const cat of state.categories){
    let m=0
    for(const l of cat.lines){
      const arr = state.data?.[cat.name]?.[l] || []
      for(let i=0;i<daysCount;i++){ m = Math.max(m, arr[i] || 0) }
    }
    valveMax[cat.name] = m
    globalMax = Math.max(globalMax, m)
  }

  // 阈值跨度
  let above=0, below=0
  for(const cat of state.categories){
    for(const l of cat.lines){
      const thr = ensureThr(cat.name, l)
      const arr = state.data?.[cat.name]?.[l] || []
      for(let i=0;i<daysCount;i++){
        const val = arr[i] || 0
        if(val>thr) above=Math.max(above, val-thr); else below=Math.max(below, thr-val)
      }
    }
  }

  // 月份起点
  const monthsStartIndex = []
  days.forEach((d,i)=>{ if(d.getDate()===1) monthsStartIndex.push(i) })

  // 各阀体图
  for(const cat of state.categories){
    const v = cat.name
    const wrapper=document.createElement('div'); wrapper.className='ph-card valve'
    wrapper.style.padding = state.compact ? '10px' : '14px'
    wrapper.style.borderRadius = state.compact ? '12px' : '16px'

    const head=document.createElement('div'); head.className='valve-head'
    const modeMeta = currentViewMode.value
    head.innerHTML = `<div class="name"><span>${v}</span><span class="mode-badge" style="border-color:${modeMeta.color};color:${modeMeta.color}">${modeMeta.label}</span></div><div class="meta">${state.mode==='threshold'?'阈值红绿':'标准渐变'} · ${state.scope==='line'?'按产线':state.scope==='valve'?'按阀体':'全局'} · 右侧产线合计 · 底部日合计</div>`
    wrapper.appendChild(head)

    const cols = daysCount, rows = cat.lines.length
    const githubLine = getGithubLine(v)
    const useGithub = Boolean(githubLine)
    const isMonthScale = !useGithub && daysCount <= MONTH_SCALE_MAX_DAYS
    const visibleLines = useGithub ? cat.lines.filter(line => line === githubLine) : cat.lines
    const cellSize = useGithub ? (state.big ? SIZE.big : GITHUB.size) : baseCellSize
    const gap = useGithub ? (state.big ? baseGap : GITHUB.gap) : baseGap
    const pad = useGithub ? (state.big ? basePad : GITHUB.pad) : basePad
    const labelGap = useGithub ? (state.big ? baseLabelGap : GITHUB.labelGap) : baseLabelGap
    const githubOffset = useGithub ? (days[0].getDay() + 6) % 7 : 0
    const lineLabelWidth = estimateLineLabelWidth(useGithub ? [githubLine] : cat.lines)
    const labelX = Math.max(pad, lineLabelWidth)
    const padX = labelX + labelGap
    const weekCols = useGithub ? Math.ceil((daysCount + githubOffset) / GITHUB.rows) : cols
    const githubRows = GITHUB.rows
    const gridRows = useGithub ? githubRows : rows
    const gridTop = state.big ? (isMonthScale ? 64 : 54) : state.compact ? (isMonthScale ? 50 : 40) : (isMonthScale ? 56 : 44)
    const gridHeight = gridRows * (cellSize + gap) - gap
    const gridWidth = weekCols * (cellSize + gap) - gap
    const gridRight = padX + gridWidth
    const rowTotals = useGithub ? Array(GITHUB.rows).fill(0) : visibleLines.map(() => 0)
    const columnTotals = Array(weekCols).fill(0)
    visibleLines.forEach((line, lineIndex) => {
      const arr = state.data?.[v]?.[line] || []
      for (let k = 0; k < cols; k++) {
        const val = Number(arr[k] || 0)
        if (!Number.isFinite(val)) continue
        const weekIndex = useGithub ? Math.floor((k + githubOffset) / GITHUB.rows) : k
        const rowIndex = useGithub ? (k + githubOffset) % GITHUB.rows : lineIndex
        rowTotals[rowIndex] += val
        columnTotals[weekIndex] += val
      }
    })
    const rowTotalLabels = rowTotals.map(formatAxisQty)
    const rowSummaryGap = state.compact ? 7 : 10
    const rowSummaryWidth = Math.min(
      112,
      Math.max(state.compact ? 48 : 58, ...rowTotalLabels.map(label => estimateTextWidth(label, 10) + 14))
    )
    const rowSummaryX = gridRight + rowSummaryGap
    const bottomSummaryGap = state.compact ? 8 : 10
    const bottomSummaryHeight = state.big ? 58 : state.compact ? 46 : 52
    const bottomSummaryY = gridTop + gridHeight + bottomSummaryGap
    const axisMonthY = 13
    const axisDateY = isMonthScale ? gridTop - 24 : gridTop - 12
    const axisWeekdayY = isMonthScale ? gridTop - 10 : null
    const axisTickTop = isMonthScale ? gridTop - 6 : gridTop - 8
    const axisTickBottom = gridTop - 3
    const width = rowSummaryX + rowSummaryWidth + pad
    const height= bottomSummaryY + bottomSummaryHeight + pad
    const svg=document.createElementNS(svgNS,'svg'); svg.setAttribute('width', width); svg.setAttribute('height', height)

    if (isMonthScale) {
      days.forEach((d, k) => {
        const x = padX + k * (cellSize + gap)
        if (isWeekend(d)) {
          svg.appendChild(createSvgEl(svgNS, 'rect', {
            x,
            y: gridTop - 2,
            width: cellSize,
            height: gridHeight + 4,
            rx: 2,
            ry: 2,
            fill: DATE_AXIS.weekendBg
          }))
        }
        if (d.getDay() === 1 && k > 0) {
          svg.appendChild(createSvgEl(svgNS, 'line', {
            x1: x - gap / 2,
            y1: gridTop - 8,
            x2: x - gap / 2,
            y2: gridTop + gridHeight,
            stroke: DATE_AXIS.tickColor,
            'stroke-width': 1,
            'stroke-dasharray': '2 2'
          }))
        }
      })
    }

    // 月份文本
    monthsStartIndex.forEach(k=>{
      const d=days[k]
      const xIndex = useGithub ? Math.floor((k + githubOffset) / GITHUB.rows) : k
      const x=padX + xIndex*(cellSize+gap)
      const t=createSvgEl(svgNS, 'text', {
        x,
        y: axisMonthY,
        'font-size': 10,
        fill: DATE_AXIS.monthColor
      })
      t.textContent=`${d.getMonth()+1}月`
      svg.appendChild(t)

      const monthLine=createSvgEl(svgNS, 'line', {
        x1: x,
        y1: axisTickTop,
        x2: x,
        y2: gridTop + gridHeight,
        stroke: DATE_AXIS.monthLineColor,
        'stroke-width': 1
      })
      svg.appendChild(monthLine)
    })

    // 日期横坐标：短日期按范围自适应显示，避免全年视图挤在一起
    buildDateAxisTicks(days, useGithub, githubOffset, cellSize, gap).forEach(tick=>{
      const x = padX + tick.xIndex*(cellSize+gap) + cellSize/2
      const tickLine=createSvgEl(svgNS, 'line', {
        x1: x,
        y1: axisTickTop,
        x2: x,
        y2: axisTickBottom,
        stroke: tick.isMonthStart ? DATE_AXIS.monthColor : DATE_AXIS.tickColor,
        'stroke-width': tick.isMonthStart ? 1.2 : 1
      })
      svg.appendChild(tickLine)

      const t=createSvgEl(svgNS, 'text', {
        x,
        y: axisDateY,
        'text-anchor': 'middle',
        'font-size': 10,
        fill: tick.isMonthStart ? DATE_AXIS.monthColor : DATE_AXIS.dateColor
      })
      if (isMonthScale) {
        const d = days[tick.index]
        t.textContent = String(d.getDate())
        t.setAttribute('fill', isWeekend(d) ? DATE_AXIS.weekendColor : DATE_AXIS.dateColor)
        svg.appendChild(t)

        const weekday = createSvgEl(svgNS, 'text', {
          x,
          y: axisWeekdayY,
          'text-anchor': 'middle',
          'font-size': 10,
          fill: isWeekend(d) ? DATE_AXIS.weekendColor : DATE_AXIS.weekdayColor
        })
        weekday.textContent = weekdayText(d)
        svg.appendChild(weekday)
        return
      }
      t.textContent = tick.label
      svg.appendChild(t)
    })

    const hoverRect = createSvgEl(svgNS, 'rect', {
      y: gridTop - 2,
      width: cellSize,
      height: gridHeight + 4,
      fill: DATE_AXIS.hoverColor,
      opacity: 0.08,
      display: 'none',
      'pointer-events': 'none'
    })
    const hoverLabel = createSvgEl(svgNS, 'g', {
      display: 'none',
      'pointer-events': 'none'
    })
    const hoverLabelBg = createSvgEl(svgNS, 'rect', {
      rx: 3,
      ry: 3,
      height: 16,
      fill: DATE_AXIS.hoverColor,
      opacity: 0.95
    })
    const hoverLabelText = createSvgEl(svgNS, 'text', {
      'font-size': 10,
      fill: '#ffffff',
      'text-anchor': 'middle'
    })
    hoverLabel.appendChild(hoverLabelBg)
    hoverLabel.appendChild(hoverLabelText)

    const showHoverGuide = (dayIndex, x) => {
      const label = fmtYMD(days[dayIndex]).slice(5)
      const labelWidth = Math.max(38, label.length * 6 + 10)
      const labelX = Math.min(Math.max(x + cellSize / 2 - labelWidth / 2, padX), width - labelWidth - 4)
      hoverRect.setAttribute('x', String(x))
      hoverRect.setAttribute('display', 'block')
      setSvgAttrs(hoverLabelBg, {
        x: labelX,
        y: gridTop - 30,
        width: labelWidth
      })
      setSvgAttrs(hoverLabelText, {
        x: labelX + labelWidth / 2,
        y: gridTop - 18
      })
      hoverLabelText.textContent = label
      hoverLabel.setAttribute('display', 'block')
    }
    const hideHoverGuide = () => {
      hoverRect.setAttribute('display', 'none')
      hoverLabel.setAttribute('display', 'none')
    }

    // 产线文本
    if (useGithub) {
      GITHUB_WEEKDAYS.forEach((label, rowIndex) => {
        const t=document.createElementNS(svgNS,'text')
        t.setAttribute('x', labelX)
        t.setAttribute('y', gridTop + rowIndex*(cellSize+gap) + Math.min(9, cellSize-3))
        t.setAttribute('text-anchor','end')
        t.setAttribute('font-size','10')
        t.setAttribute('fill', rowIndex >= 5 ? DATE_AXIS.weekendColor : '#64748b')
        t.textContent=label
        t.style.cursor = 'pointer'
        t.addEventListener('click', ()=> toggleGithubLine(v, githubLine))
        svg.appendChild(t)
      })
    } else {
      cat.lines.forEach((l,r)=>{
        const t=document.createElementNS(svgNS,'text')
        t.setAttribute('x', labelX)
        t.setAttribute('y', gridTop + r*(cellSize+gap) + Math.min(9, cellSize-3))
        t.setAttribute('text-anchor','end')
        t.setAttribute('font-size','10')
        t.setAttribute('fill','#64748b')
        t.textContent=l
        t.style.cursor = 'pointer'
        t.addEventListener('click', ()=> toggleGithubLine(v, l))
        svg.appendChild(t)
      })
    }

    const rowSummaryHeader = createSvgEl(svgNS, 'text', {
      x: rowSummaryX + rowSummaryWidth / 2,
      y: axisWeekdayY || axisDateY,
      'text-anchor': 'middle',
      'font-size': 10,
      fill: SUMMARY_AXIS.labelColor
    })
    rowSummaryHeader.textContent = '合计'
    svg.appendChild(rowSummaryHeader)

    svg.appendChild(createSvgEl(svgNS, 'line', {
      x1: gridRight + rowSummaryGap / 2,
      y1: gridTop,
      x2: gridRight + rowSummaryGap / 2,
      y2: gridTop + gridHeight,
      stroke: SUMMARY_AXIS.cellBorder,
      'stroke-width': 1
    }))

    const maxRowTotal = Math.max(0, ...rowTotals)
    rowTotals.forEach((sum, rowIndex) => {
      const y = gridTop + rowIndex * (cellSize + gap)
      const barWidth = maxRowTotal > 0 && sum > 0 ? Math.max(2, (rowSummaryWidth - 4) * sum / maxRowTotal) : 0
      const bg = createSvgEl(svgNS, 'rect', {
        x: rowSummaryX,
        y: y - 1,
        width: rowSummaryWidth,
        height: cellSize + 2,
        rx: 3,
        ry: 3,
        fill: SUMMARY_AXIS.cellEmptyFill,
        stroke: SUMMARY_AXIS.cellBorder,
        'stroke-width': 1
      })
      const bar = createSvgEl(svgNS, 'rect', {
        x: rowSummaryX + 2,
        y: y + 2,
        width: barWidth,
        height: Math.max(2, cellSize - 4),
        rx: 2,
        ry: 2,
        fill: SUMMARY_AXIS.cellFill
      })
      const t = createSvgEl(svgNS, 'text', {
        x: rowSummaryX + rowSummaryWidth - 5,
        y: y + cellSize / 2 + 3,
        'text-anchor': 'end',
        'font-size': 10,
        fill: SUMMARY_AXIS.textColor
      })
      t.textContent = rowTotalLabels[rowIndex] || '0'
      bindSvgTooltip(bg, `行合计：${formatQty(sum)}`)
      bindSvgTooltip(bar, `行合计：${formatQty(sum)}`)
      bindSvgTooltip(t, `行合计：${formatQty(sum)}`)
      svg.appendChild(bg)
      svg.appendChild(bar)
      svg.appendChild(t)
    })

    svg.appendChild(createSvgEl(svgNS, 'line', {
      x1: padX,
      y1: bottomSummaryY - bottomSummaryGap / 2,
      x2: gridRight,
      y2: bottomSummaryY - bottomSummaryGap / 2,
      stroke: SUMMARY_AXIS.cellBorder,
      'stroke-width': 1
    }))

    const bottomLabel = createSvgEl(svgNS, 'text', {
      x: labelX,
      y: bottomSummaryY + 11,
      'text-anchor': 'end',
      'font-size': 10,
      fill: SUMMARY_AXIS.labelColor
    })
    bottomLabel.textContent = useGithub ? '周合计' : '日合计'
    svg.appendChild(bottomLabel)

    const maxColumnTotal = Math.max(0, ...columnTotals)
    const columnBarBaseY = bottomSummaryY + (state.big ? 18 : 15)
    const columnBarMaxHeight = state.big ? 14 : 11
    columnTotals.forEach((sum, columnIndex) => {
      const x = padX + columnIndex * (cellSize + gap)
      const barHeight = maxColumnTotal > 0 && sum > 0 ? Math.max(2, columnBarMaxHeight * sum / maxColumnTotal) : 0
      const title = useGithub
        ? `周合计：${formatQty(sum)}`
        : `${fmtYMD(days[columnIndex])} 日合计：${formatQty(sum)}`
      const bar = createSvgEl(svgNS, 'rect', {
        x,
        y: columnBarBaseY - barHeight,
        width: cellSize,
        height: barHeight,
        rx: 2,
        ry: 2,
        fill: sum > 0 ? colorFromValue(sum, maxColumnTotal) : SUMMARY_AXIS.cellEmptyFill
      })
      bindSvgTooltip(bar, title)
      svg.appendChild(bar)

      const t = createSvgEl(svgNS, 'text', {
        x: x + cellSize / 2,
        y: bottomSummaryY + bottomSummaryHeight - 4,
        'text-anchor': 'end',
        'font-size': 9,
        fill: sum > 0 ? SUMMARY_AXIS.textColor : SUMMARY_AXIS.mutedColor,
        transform: `rotate(-55 ${x + cellSize / 2} ${bottomSummaryY + bottomSummaryHeight - 4})`
      })
      t.textContent = formatAxisQty(sum)
      bindSvgTooltip(t, title)
      svg.appendChild(t)
    })

    // 单元格
    cat.lines.forEach((l,r)=>{
      if (useGithub && l !== githubLine) return
      const arr = state.data?.[v]?.[l] || []
      let lineMax=0, lineMin=Infinity
      for(let i=0;i<cols;i++){ const val=arr[i]||0; if(val>lineMax) lineMax=val; if(val<lineMin) lineMin=val }
      if(lineMin===Infinity) lineMin=0

      for(let k=0;k<cols;k++){
        const weekIndex = useGithub ? Math.floor((k + githubOffset) / GITHUB.rows) : k
        const dayIndex = useGithub ? (k + githubOffset) % GITHUB.rows : r
        const x=padX + weekIndex*(cellSize+gap), y=gridTop + dayIndex*(cellSize+gap)
        const val = arr[k] ?? 0
        const thr = ensureThr(v, l)
        const aboveMax = Math.max(0, lineMax-thr), belowMax = Math.max(thr - lineMin, 0)
        const colorMax = state.scope==='line' ? lineMax : state.scope==='valve' ? valveMax[v] : globalMax
        const fill = state.mode==='threshold' ? colorFromThreshold(val, thr, aboveMax, belowMax) : colorFromValue(val, colorMax)
        const changed = hasChangedCell(v, l, days[k], val)

        const rect=document.createElementNS('http://www.w3.org/2000/svg','rect')
        rect.setAttribute('x', x); rect.setAttribute('y', y)
        rect.setAttribute('width', cellSize); rect.setAttribute('height', cellSize)
        rect.setAttribute('rx', 2); rect.setAttribute('ry', 2)
        rect.setAttribute('fill', fill)
        rect.setAttribute('stroke', cellBorderColor(changed))
        rect.setAttribute('stroke-width', cellBorderWidth(changed))
        rect.style.cursor='pointer'

        rect.addEventListener('click', ()=>{
          openCellDetails(v, l, days[k], val)
        })
        rect.addEventListener('mouseenter', ev=>{
          showHoverGuide(k, x)
          const tip = document.getElementById('tooltip')
          const lines = [
            `口径：${currentViewMode.value.label}`, `阀体：${v}`, `产线：${l}`, `日期：${fmtYMD(days[k])}`,
            `数量：${val}`, `阈值：${thr}`,
            (val>thr ? `状态：超阈值 +${val-thr}` : `状态：未超阈值 ${thr-val}`)
          ]
          if (changed) {
            const actualVal = getActualCellValue(v, l, days[k])
            lines.push(`实际：${formatQty(actualVal)}`, `差异：${formatSignedQty(val - actualVal)}`)
          }
          tip.textContent = lines.join('\n'); tip.style.display='block'
          tip.style.left=(ev.clientX+12)+'px'; tip.style.top=(ev.clientY+12)+'px'
        })
        rect.addEventListener('mousemove', ev=>{
          const tip = document.getElementById('tooltip'); tip.style.left=(ev.clientX+12)+'px'; tip.style.top=(ev.clientY+12)+'px'
        })
        rect.addEventListener('mouseleave', ()=>{
          hideHoverGuide()
          const tip = document.getElementById('tooltip'); tip.style.display='none'
        })

        svg.appendChild(rect)

        if(state.big){
          const txt=document.createElementNS('http://www.w3.org/2000/svg','text')
          const fontSize = Math.max(9, Math.min(12, Math.floor(cellSize*0.7)))
          txt.setAttribute('x', x + cellSize/2); txt.setAttribute('y', y + cellSize/2 + Math.ceil(fontSize/3))
          txt.setAttribute('text-anchor','middle'); txt.style.userSelect='none'; txt.style.pointerEvents='none'
          txt.setAttribute('font-size', String(fontSize))
          txt.setAttribute('fill', textColorForCell(val, thr, colorMax, state.mode, aboveMax, belowMax))
          txt.textContent = String(val)
          svg.appendChild(txt)
        }
      }
    })

    svg.appendChild(hoverRect)
    svg.appendChild(hoverLabel)
    wrapper.appendChild(svg)
    container.appendChild(wrapper)
  }
}

async function openCellDetails(valve, line, date, quantity){
  const tip = document.getElementById('tooltip')
  if (tip) tip.style.display = 'none'

  const dateText = fmtYMD(date)
  cellDetailContext.date = dateText
  cellDetailContext.valveCategory = valve
  cellDetailContext.productionLine = line
  cellDetailContext.quantity = Number(quantity || 0)
  cellDetailRows.value = []
  cellDetailPager.page = 1
  cellDetailPager.pageSize = 200
  cellDetailPager.totalRows = 0
  cellDetailPager.totalQuantity = 0
  cellDetailDialog.value = true

  await loadCellDetailsPage(1)
}

async function loadCellDetailsPage(page = 1){
  if (!cellDetailContext.date) return

  const currentSeq = ++cellDetailRequestSeq
  const nextPage = Math.max(1, Number(page || 1))
  cellDetailLoading.value = true

  try{
    const qs = new URLSearchParams({
      date: cellDetailContext.date,
      valveCategory: cellDetailContext.valveCategory,
      productionLine: cellDetailContext.productionLine,
      page: String(nextPage),
      pageSize: String(cellDetailPager.pageSize || 200)
    })
    const res = await proxy?.http?.get(`/api/WZ/ProductionOutput/details-page?${qs.toString()}`, {}, true)
    if (currentSeq !== cellDetailRequestSeq) return

    const payload = res?.data ?? res?.Data ?? res?.result ?? res
    const rows =
      Array.isArray(payload?.items) ? payload.items :
        Array.isArray(payload?.Items) ? payload.Items : []

    cellDetailRows.value = rows
    cellDetailPager.page = Number(payload?.page ?? payload?.Page ?? nextPage) || nextPage
    cellDetailPager.pageSize = Number(payload?.pageSize ?? payload?.PageSize ?? cellDetailPager.pageSize) || 200
    cellDetailPager.totalRows = Number(payload?.totalRows ?? payload?.TotalRows ?? rows.length) || 0
    cellDetailPager.totalQuantity = Number(payload?.totalQuantity ?? payload?.TotalQuantity ?? 0) || 0
  }catch(e){
    console.error(e)
    if (currentSeq === cellDetailRequestSeq) {
      ElMessage.error('格子明细加载失败')
    }
  }finally{
    if (currentSeq === cellDetailRequestSeq) {
      cellDetailLoading.value = false
    }
  }
}

/* 阈值弹窗 */
function openThresholdModal(){
  // 基于当前 thresholds 深拷贝，并补齐所有（阀体-产线）
  const draft = JSON.parse(JSON.stringify(state.thresholds || {}))
  state.categories.forEach(cat => {
    if (!draft[cat.name]) draft[cat.name] = {}
    cat.lines.forEach(l => {
      if (typeof draft[cat.name][l] !== 'number') draft[cat.name][l] = 20
    })
  })
  thrDraft.value = draft
  thrDialog.value = true
}
async function saveThresholds(){
  const draft = JSON.parse(JSON.stringify(thrDraft.value || {}))
  const payload = []
  Object.entries(draft).forEach(([valve, lines]) => {
    Object.entries(lines || {}).forEach(([line, value]) => {
      const num = Number(value ?? 0)
      payload.push({ valveCategory: valve, productionLine: line, threshold: num })
    })
  })

  try{
    const res = await proxy?.http?.post('/api/WZ/ProductionOutput/thresholds', payload)
    const status = res?.status ?? res?.Status ?? res?.success ?? res?.Success
    if (status === false) {
      ElMessage.error(res?.message || res?.Message || '阈值保存失败')
      return
    }
  }catch(e){
    console.error(e)
    ElMessage.error('阈值保存失败')
    return
  }

  state.thresholds = draft
  applyCurrentThresholdToRows()
  thrDialog.value = false
  renderAll()
  ElMessage.success('阈值保存成功')
}

/* 加载数据（动态阀体/产线，开始/结束日期即可） */
async function loadData(){
  try{
    const start = fmtYMD(state.rangeStart)
    const end   = fmtYMD(state.rangeEnd)

    const qs = new URLSearchParams()
    qs.set('start', start)
    qs.set('end',   end)
    if (valveCategory.value?.trim())  qs.set('valveCategory', valveCategory.value.trim())
    if (productionLine.value?.trim()) qs.set('productionLine', productionLine.value.trim())

    const url = `/api/WZ/ProductionOutput?${qs.toString()}`
    console.debug('[WZ_ProductionOutput] GET =>', url)
    const res = await proxy?.http?.get(url, {}, true)

    const rows = parseRowsResponse(res)
    viewMode.value = 'actual'
    storeActualBaseline(rows)
    applyRows(rows, '数据加载成功')
  }catch(e){
    console.error(e)
    ElMessage.error('加载失败，请查看控制台 Network/Console 日志')
  }
}

async function syncData(){
  if (syncLoading.value) return
  if (!canSyncData.value) {
    ElMessage.warning('只有 cyadmin 可以同步数据')
    return
  }
  syncDialog.value = true
  await loadSyncHistory()
}

async function loadSyncHistory(){
  if (syncHistoryLoading.value) return
  syncHistoryLoading.value = true
  try{
    const res = await proxy?.http?.get('/api/WZ/ProductionOutput/sync-history?take=10', {}, true)
    syncHistory.value =
      Array.isArray(res) ? res :
        Array.isArray(res?.data) ? res.data :
          Array.isArray(res?.Data) ? res.Data :
            Array.isArray(res?.result) ? res.result : []
  }catch(e){
    console.error(e)
    ElMessage.error('同步记录加载失败')
  }finally{
    syncHistoryLoading.value = false
  }
}

async function runFullSync(){
  if (syncLoading.value) return
  const { start, end } = getInterfaceSyncRange()
  try{
    await ElMessageBox.confirm(
      `确认按同步接口入参时间 ${start} ~ ${end} 初始化全量同步？该操作会重建产能明细与汇总表。`,
      '初始化全量同步',
      { type: 'warning', confirmButtonText: '开始同步', cancelButtonText: '取消' }
    )
  }catch{
    return
  }

  syncLoading.value = true
  try{
    const payload = { start, end }
    const res = await proxy?.http?.post('/api/WZ/ProductionOutput/refresh', payload)
    const inserted = res?.inserted ?? res?.data?.inserted ?? res?.Data?.inserted
    const range = res?.range ?? res?.data?.range ?? res?.Data?.range
    if (typeof inserted === 'number') {
      ElMessage.success(`同步完成：${range || '指定范围'}，写入 ${inserted} 条`)
    } else {
      ElMessage.success('同步完成')
    }
  }catch(e){
    console.error(e)
    ElMessage.error('同步失败，请查看控制台 Network/Console 日志')
  }finally{
    syncLoading.value = false
    await loadSyncHistory()
  }
}

async function loadPreProduction(){
  try{
    await ensureActualBaseline().catch(e => console.warn('[WZ_ProductionOutput] actual baseline load failed', e))
    const start = fmtYMD(state.rangeStart)
    const end   = fmtYMD(state.rangeEnd)
    const payload = {
      start,
      end,
      valveCategory: valveCategory.value?.trim() || '',
      productionLine: productionLine.value?.trim() || ''
    }
    const res = await proxy?.http?.post('/api/WZ/ProductionOutput/preproduction/merge', payload)
    const rows = parseRowsResponse(res)
    viewMode.value = 'preproduction'
    applyRows(rows, '预排产合并数据加载成功')
  }catch(e){
    console.error(e)
    ElMessage.error('预排产合并加载失败')
  }
}

async function loadOptimizedPreProduction(){
  try{
    await ensureActualBaseline().catch(e => console.warn('[WZ_ProductionOutput] actual baseline load failed', e))
    const start = fmtYMD(state.rangeStart)
    const end   = fmtYMD(state.rangeEnd)
    const payload = {
      start,
      end,
      valveCategory: valveCategory.value?.trim() || '',
      productionLine: productionLine.value?.trim() || ''
    }
    const res = await proxy?.http?.post('/api/WZ/ProductionOutput/preproduction/optimize', payload)
    const rows = parseRowsResponse(res)
    viewMode.value = 'optimized'
    applyRows(rows, '排产优化合并数据加载成功')
  }catch(e){
    console.error(e)
    ElMessage.error('排产优化加载失败')
  }
}

function modeButtonClass(mode){
  return viewMode.value === mode ? 'is-mode-active' : ''
}

function cellBorderColor(changed){
  return changed ? (currentViewMode.value.cellBorder || '#ffffff') : '#ffffff'
}

function cellBorderWidth(changed){
  return changed ? (state.big ? 2.6 : 1.8) : 1
}

function applyRows(rows, successMessage){
  rawRows.value = rows // 记录原始数据供导出
  console.debug('[WZ_ProductionOutput] rows sample =>', rows?.slice?.(0,5))

  // 日期轴
  const days = daysBetween(state.rangeStart, state.rangeEnd)
  const idxMap = buildIdxMap(days)
  const daysCount = days.length

  // 动态收集 阀体→产线
  const catMap = new Map() // Map<string, Set<string>>
  const thresholdMap = {}
  for (const r of rows){
    const v = String(r.valveCategory ?? r.ValveCategory ?? '').trim()
    const l = String(r.productionLine ?? r.ProductionLine ?? '').trim()
    const thr = Number(r.currentThreshold ?? r.CurrentThreshold)
    if (!v || !l) continue
    if (!catMap.has(v)) catMap.set(v, new Set())
    catMap.get(v).add(l)
    if (!Number.isNaN(thr)) {
      if (!thresholdMap[v]) thresholdMap[v] = {}
      if (typeof thresholdMap[v][l] !== 'number') thresholdMap[v][l] = thr
    }
  }
  // 排序
  const categories = Array.from(catMap.keys())
    .sort(compareCategoryName)
    .map(v=>{
      const lines = Array.from(catMap.get(v)).sort(compareLineName)
      return { name:v, lines }
    })
  state.categories = categories

  // 初始化矩阵 & 阈值
  state.data = {}
  for (const cat of state.categories){
    if(!state.data[cat.name]) state.data[cat.name] = {}
    if(!state.thresholds[cat.name]) state.thresholds[cat.name] = {}
    for (const l of cat.lines){
      state.data[cat.name][l] = Array(daysCount).fill(0)
      if (typeof thresholdMap?.[cat.name]?.[l] === 'number') {
        state.thresholds[cat.name][l] = thresholdMap[cat.name][l]
      } else if (typeof state.thresholds[cat.name][l] !== 'number') {
        state.thresholds[cat.name][l] = 20
      }
    }
  }

  // 填充
  let filled = 0
  for (const r of rows){
    const v = String(r.valveCategory ?? r.ValveCategory ?? '').trim()
    const l = String(r.productionLine ?? r.ProductionLine ?? '').trim()
    const d = getProductionDateStr(r) // 'YYYY-MM-DD'
    const q = Number(r.quantity ?? r.Quantity ?? 0)
    const i = idxMap.get(d)
    if (i==null || !state.data?.[v]?.[l]) continue
    state.data[v][l][i] += q
    filled++
  }
  console.debug(`[WZ_ProductionOutput] categories=${state.categories.length}, lines=${state.categories.reduce((a,c)=>a+c.lines.length,0)}, filled=${filled}`)

  renderAll()
  if (rows.length === 0) {
    ElMessage.warning('接口返回为空，请检查筛选条件或后端聚合。')
  } else if (filled === 0) {
    ElMessage.warning('未匹配到当前日期范围内的数据（确认 productionDate 是否在所选范围内）。')
  } else {
    ElMessage.success(successMessage)
  }
}

/* 导出数据（CSV，UTF-8 BOM，Excel可直接打开） */
function exportData(){
  if (!rawRows.value?.length) {
    ElMessage.warning('没有可导出的数据，请先“加载数据”')
    return
  }
  const headers = ['排产年月','阀体种类','生产线','排产量']
  const lines = rawRows.value.map(r => {
    const ym = getYearMonth(r).replaceAll('"','""')
    const vc = String(r.valveCategory ?? r.ValveCategory ?? '').replaceAll('"','""')
    const pl = String(r.productionLine ?? r.ProductionLine ?? '').replaceAll('"','""')
    const qt = Number(r.quantity ?? r.Quantity ?? 0)
    return `"${ym}","${vc}","${pl}",${qt}`
  })
  const csv = [headers.join(','), ...lines].join('\r\n')
  const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' })
  const start = fmtYMD(state.rangeStart), end = fmtYMD(state.rangeEnd)
  const fileName = `产线排产_${start}_${end}.csv`
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
  ElMessage.success('导出完成')
}

function csvCell(value){
  const text = String(value ?? '').replaceAll('"','""')
  return `"${text}"`
}

function downloadCsv(fileName, headers, rows){
  const csv = [headers.join(','), ...rows].join('\r\n')
  const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

function readFileAsText(file){
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(String(reader.result || ''))
    reader.onerror = () => reject(reader.error || new Error('文件读取失败'))
    reader.readAsText(file, 'utf-8')
  })
}

function parseCsvRows(text){
  const rows = []
  let row = []
  let cell = ''
  let quoted = false
  const source = String(text || '').replace(/^\uFEFF/, '')

  for (let i = 0; i < source.length; i++) {
    const ch = source[i]
    if (quoted) {
      if (ch === '"' && source[i + 1] === '"') {
        cell += '"'
        i++
      } else if (ch === '"') {
        quoted = false
      } else {
        cell += ch
      }
      continue
    }

    if (ch === '"') {
      quoted = true
    } else if (ch === ',') {
      row.push(cell)
      cell = ''
    } else if (ch === '\n') {
      row.push(cell)
      if (row.some(v => String(v || '').trim())) rows.push(row)
      row = []
      cell = ''
    } else if (ch !== '\r') {
      cell += ch
    }
  }

  row.push(cell)
  if (row.some(v => String(v || '').trim())) rows.push(row)
  return rows
}

function buildCsvHeaderMap(headers){
  const map = new Map()
  headers.forEach((header, index) => {
    map.set(String(header || '').trim().toLowerCase(), index)
  })
  return map
}

function csvValue(row, headerMap, aliases){
  for (const name of aliases) {
    const index = headerMap.get(String(name).trim().toLowerCase())
    if (index != null) return String(row[index] ?? '').trim()
  }
  return ''
}

function cleanManualValue(value, unknownText = ''){
  const text = String(value || '').trim()
  if (!text || (unknownText && text === unknownText)) return ''
  return text
}

function openManualRuleImport(){
  if (!canSyncData.value) {
    ElMessage.warning('只有 cyadmin 可以导入人工规则')
    return
  }
  manualRuleFileInput.value?.click?.()
}

async function importManualRulesFromFile(event){
  const input = event?.target
  const file = input?.files?.[0]
  if (!file || manualRuleImportLoading.value) return

  manualRuleImportLoading.value = true
  try{
    const text = await readFileAsText(file)
    const csvRows = parseCsvRows(text)
    if (csvRows.length < 2) {
      ElMessage.warning('CSV 没有可导入的数据行')
      return
    }

    const headerMap = buildCsvHeaderMap(csvRows[0])
    const aliases = {
      billNo: ['单据号', '订单号', 'BillNo', 'billNo'],
      planTrackingNo: ['计划跟踪号', 'PlanTrackingNo', 'planTrackingNo'],
      materialKey: ['物料键', 'MaterialKey', 'materialKey'],
      materialCode: ['物料编码', '物料号', 'MaterialCode', 'materialCode'],
      materialId: ['物料ID', 'MaterialId', 'materialId'],
      manualValveCategory: ['人工阀体种类', '人工阀体类别', '人工阀类', 'ManualValveCategory'],
      valveCategory: ['阀体种类', '阀体类别', 'ValveCategory', 'valveCategory'],
      manualProductionLine: ['人工生产线', '人工产线', 'ManualProductionLine'],
      productionLine: ['生产线', '产线', 'ProductionLine', 'productionLine'],
      remark: ['规则备注', '备注', 'Remark', 'remark']
    }

    const ruleMap = new Map()
    for (const row of csvRows.slice(1)) {
      const billNo = csvValue(row, headerMap, aliases.billNo)
      const planTrackingNo = csvValue(row, headerMap, aliases.planTrackingNo)
      const materialCode = csvValue(row, headerMap, aliases.materialCode)
      const materialId = csvValue(row, headerMap, aliases.materialId)
      const materialKey = csvValue(row, headerMap, aliases.materialKey)
      const valveCategory = cleanManualValue(
        csvValue(row, headerMap, aliases.manualValveCategory)
          || csvValue(row, headerMap, aliases.valveCategory),
        '未知阀类'
      )
      const productionLine = cleanManualValue(
        csvValue(row, headerMap, aliases.manualProductionLine)
          || csvValue(row, headerMap, aliases.productionLine),
        '未知产线'
      )
      if (!billNo || !planTrackingNo || !valveCategory || !productionLine) continue

      const key = [billNo, planTrackingNo, materialCode, materialId, materialKey].join('\u001F')
      ruleMap.set(key, {
        billNo,
        planTrackingNo,
        materialCode,
        materialId,
        materialKey,
        valveCategory,
        productionLine,
        remark: csvValue(row, headerMap, aliases.remark),
        enable: true
      })
    }

    const rules = Array.from(ruleMap.values())
    if (!rules.length) {
      ElMessage.warning('没有识别到可导入的人工规则，请填写“人工阀体种类”和“人工生产线”')
      return
    }

    const res = await proxy?.http?.post('/api/WZ/ProductionOutput/manual-line-rules', rules)
    const saved = Number(res?.saved ?? res?.data?.saved ?? res?.Data?.saved ?? rules.length)
    ElMessage.success(`已导入 ${saved} 条人工规则，下次同步或重新归类会优先使用`)
  }catch(e){
    console.error(e)
    ElMessage.error('人工规则导入失败')
  }finally{
    manualRuleImportLoading.value = false
    if (input) input.value = ''
  }
}

async function exportUnknownData(){
  if (unknownExportLoading.value) return
  const start = fmtYMD(state.rangeStart)
  const end = fmtYMD(state.rangeEnd)
  const qs = new URLSearchParams()
  qs.set('start', start)
  qs.set('end', end)
  qs.set('take', '200000')

  unknownExportLoading.value = true
  try{
    const res = await proxy?.http?.get(`/api/WZ/ProductionOutput/unknown-details?${qs.toString()}`, {}, true)
    const rows =
      Array.isArray(res) ? res :
        Array.isArray(res?.data) ? res.data :
          Array.isArray(res?.Data) ? res.Data :
            Array.isArray(res?.result) ? res.result : []

    if (!rows.length) {
      ElMessage.warning('当前日期范围没有未知产线明细')
      return
    }

    const headers = ['排产日期','单据号','计划跟踪号','行号','EntryId','物料键','物料编码','物料ID','规格型号','阀体种类','生产线','数量','状态','业务键','人工阀体种类','人工生产线','规则备注']
    const lines = rows.map(r => {
      const valve = r.valveCategory ?? r.ValveCategory
      const line = r.productionLine ?? r.ProductionLine
      const manualValve = cleanManualValue(valve, '未知阀类')
      return [
        getProductionDateStr(r),
        r.billNo ?? r.BillNo,
        r.planTrackingNo ?? r.PlanTrackingNo,
        r.seq ?? r.Seq,
        r.entryId ?? r.EntryId,
        r.materialKey ?? r.MaterialKey,
        r.materialCode ?? r.MaterialCode,
        r.materialId ?? r.MaterialId,
        r.specModel ?? r.SpecModel ?? r.productModel ?? r.ProductModel,
        valve,
        line,
        Number(r.quantity ?? r.Quantity ?? 0),
        r.classifyStatus ?? r.ClassifyStatus,
        r.businessKey ?? r.BusinessKey,
        manualValve,
        '',
        ''
      ].map(csvCell).join(',')
    })

    downloadCsv(`未知产线明细_${start}_${end}.csv`, headers, lines)
    ElMessage.success(`已导出 ${rows.length} 条未知产线明细`)
  }catch(e){
    console.error(e)
    ElMessage.error('未知产线明细导出失败')
  }finally{
    unknownExportLoading.value = false
  }
}

function applyCurrentThresholdToRows(){
  if (!rawRows.value?.length) return
  rawRows.value.forEach(row => {
    const valve = String(row.valveCategory ?? row.ValveCategory ?? '').trim()
    const line = String(row.productionLine ?? row.ProductionLine ?? '').trim()
    const thr = state.thresholds?.[valve]?.[line]
    const value = typeof thr === 'number' ? thr : 20
    row.CurrentThreshold = value
    row.currentThreshold = value
  })
}

/* 首次渲染空图 */
onMounted(()=>{ renderAll() })
</script>

<style scoped>
:root{
  --bg:#f8fafc; --card:#fff; --muted:#64748b; --text:#0f172a; --border:#e2e8f0;
  --amber:#f59e0b; --slate400:#94a3b8;
}
.ph-title{font-weight:700;font-size:18px}
.ph-sub{font-size:12px;color:var(--muted)}
.ph-header{position:sticky;top:0;backdrop-filter:saturate(150%) blur(6px);background:rgba(255,255,255,.8);border-bottom:1px solid var(--border);z-index:10}
.ph-container{width:100%;padding:12px 16px;display:flex;flex-wrap:wrap;gap:8px;align-items:center}
.manual-rule-input{display:none}

.btn-amber{--el-button-bg-color:var(--amber);--el-button-text-color:#fff;border:none}
.mode-switch{--mode-color:#303384}
.mode-switch :deep(.el-button){border-color:#cbd5e1;color:#334155;background:#fff}
.mode-switch :deep(.el-button.is-mode-active){border-color:var(--mode-color);color:var(--mode-color);background:#fdfdfd;font-weight:600;box-shadow:inset 0 0 0 1px var(--mode-color)}
.mode-current{height:24px;display:inline-flex;align-items:center;border:1px solid;border-radius:4px;padding:0 8px;background:#fff;font-size:12px;font-weight:600}

.ph-grid3{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:12px;padding:16px 16px 0}
.ph-card{background:var(--card);border:1px solid var(--border);border-radius:16px;box-shadow:0 1px 6px rgba(15,23,42,.04);padding:14px}
.stat .label{font-size:12px;color:var(--muted)}
.stat .value{font-size:22px;font-weight:700;margin-top:4px}
.stat .sub{font-size:12px;color:var(--slate400);margin-top:2px}

.ph-charts{display:flex;flex-direction:column;gap:16px;padding:0 16px 16px}
.valve .name{font-weight:600;font-size:13px;color:#111827;display:flex;align-items:center;gap:8px;flex-wrap:wrap}
.mode-badge{display:inline-flex;align-items:center;height:18px;border:1px solid;border-radius:4px;padding:0 6px;font-size:11px;font-weight:600;background:#fff}
.valve .meta{font-size:11px;color:#6b7280}
svg text{font-family:inherit}

.tooltip{position:fixed;z-index:50;pointer-events:none;background:#fff;border:1px solid var(--border);border-radius:8px;padding:6px 8px;font-size:12px;color:#334155;box-shadow:0 8px 24px rgba(15,23,42,.08);white-space:pre}

/* 阈值弹窗 */
.grid-lines{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:12px}
.thr-title{font-weight:600;color:#334155;margin:6px 0 8px}
.thr-lines{display:grid;grid-template-columns:repeat(1,minmax(0,1fr));gap:8px}
.line-edit{display:flex;align-items:center;gap:8px;font-size:12px;color:#475569}
.line-name{min-width:80px;text-align:right}
.threshold-dialog :deep(.el-dialog){--el-dialog-padding-primary:24px}
.threshold-dialog :deep(.el-dialog__header){padding:20px 24px 18px !important;border-bottom:1px solid var(--border)}
.threshold-dialog :deep(.el-dialog__body){padding:20px 24px 26px !important}
.threshold-dialog :deep(.el-dialog__footer){padding:18px 24px 22px !important}

.sync-dialog :deep(.el-dialog__header){padding:20px 24px 16px !important;border-bottom:1px solid var(--border)}
.sync-dialog :deep(.el-dialog__body){padding:18px 24px 22px !important}
.sync-dialog :deep(.el-dialog__footer){padding:16px 24px 20px !important;border-top:1px solid var(--border)}
.sync-summary{display:flex;align-items:flex-start;justify-content:space-between;gap:16px}
.sync-title{font-weight:600;color:#111827;font-size:14px}
.sync-sub{font-size:12px;color:#64748b;margin-top:4px;line-height:1.5}
.sync-window{display:flex;align-items:center;justify-content:space-between;gap:12px;margin-top:14px;padding:10px 12px;background:#f8fafc;border:1px solid var(--border);border-radius:8px}
.sync-window .label{font-size:12px;color:#64748b}
.sync-window .value{font-weight:600;color:#111827}

.cell-detail-dialog :deep(.el-dialog__header){padding:20px 24px 16px !important;border-bottom:1px solid var(--border)}
.cell-detail-dialog :deep(.el-dialog__body){padding:18px 24px 22px !important}
.cell-detail-dialog :deep(.el-dialog__footer){padding:16px 24px 20px !important;border-top:1px solid var(--border)}
.detail-summary{display:flex;align-items:flex-start;justify-content:space-between;gap:16px}
.detail-title{font-weight:600;color:#111827;font-size:14px}
.detail-sub{font-size:12px;color:#64748b;margin-top:4px;line-height:1.5}
.detail-pager{display:flex;align-items:center;justify-content:space-between;gap:12px;margin-top:12px}
.detail-page-info{font-size:12px;color:#64748b}

/* 紧凑模式整体缩紧 */
.compact .ph-container{padding:8px 12px}
.compact .ph-grid3{gap:8px;padding:10px 12px 0}
.compact .ph-card{padding:10px;border-radius:12px}
.compact .ph-charts{gap:12px;padding:0 12px 12px}
</style>
