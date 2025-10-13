<!-- 产线热力图看板（按 ValveCategory → ProductionLine → 每天 ProductionDate 汇总 Quantity） -->
<template>
  <!-- 顶部筛选区 -->
  <div class="filters flex items-center gap-4 mb-4">
    <div class="flex items-center gap-1">
      <span class="text-sm text-gray-700">年份：</span>
      <el-select v-model="selectedYear" size="small" style="width: 100px">
        <el-option v-for="y in yearOptions" :key="y.value" :label="y.label" :value="y.value" />
      </el-select>
    </div>
    <div class="flex items-center gap-1">
      <span class="text-sm text-gray-700">日期范围：</span>
      <el-date-picker v-model="dateRange" type="daterange" size="small"
                      range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" />
    </div>
    <div class="flex items-center gap-1">
      <span class="text-sm text-gray-700">颜色方案：</span>
      <el-select v-model="colorScheme" size="small" style="width: 120px">
        <el-option value="gradient" label="标准渐变" />
        <el-option value="threshold" label="阈值红绿" />
      </el-select>
    </div>
    <div class="flex items-center gap-1">
      <span class="text-sm text-gray-700">视图：</span>
      <el-radio-group v-model="viewMode" size="small">
        <el-radio-button label="large">大格+数字</el-radio-button>
        <el-radio-button label="compact">紧凑</el-radio-button>
      </el-radio-group>
    </div>
    <el-button type="primary" size="small" @click="loadData">加载数据</el-button>
    <el-button size="small" @click="refreshData">刷新数据</el-button>
  </div>

  <!-- 中部热力图 -->
  <div class="heatmap-display">
    <div v-if="categoriesData.length === 0" class="text-center text-gray-500">
      请先选择筛选条件并点击“加载数据”
    </div>

    <div v-else>
      <div v-for="cat in categoriesData" :key="cat.name" class="category-block mb-6">
        <div class="category-title font-bold mb-2">{{ cat.name }}</div>

        <div class="heatmap-container flex">
          <!-- 动态产线名 -->
          <div class="labels-column mr-2">
            <div v-for="ln in cat.lines" :key="ln.lineName"
                 class="line-label text-sm text-gray-700"
                 :style="{ height: cellSize + 'px', lineHeight: cellSize + 'px' }">
              {{ ln.lineName }}
            </div>
          </div>

          <!-- 热力格 -->
          <div class="heatmap-grid flex flex-col gap-0.5 overflow-x-auto" style="flex:1">
            <div class="heatmap-row flex gap-0.5" v-for="ln in cat.lines" :key="ln.lineName">
              <el-tooltip v-for="(qty, i) in ln.data" :key="i"
                          :content="`${cat.name} / ${ln.lineName} / ${dateList[i]} / 数量: ${qty}`"
                          placement="top" effect="dark">
                <div class="heatmap-cell" :class="viewMode" :style="getCellStyle(qty)">
                  <span v-if="viewMode==='large'">{{ qty }}</span>
                </div>
              </el-tooltip>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- 底部图例 -->
  <div class="legend mt-4">
    <div v-if="colorScheme==='gradient'" class="flex items-center">
      <span class="text-sm text-gray-700">{{ minQuantity }}</span>
      <div class="flex-1 h-4 mx-2" :style="{ background: gradientStyle }"></div>
      <span class="text-sm text-gray-700">{{ maxQuantity }}</span>
    </div>
    <div v-else class="flex items-center">
      <div class="w-4 h-4 mr-1" style="background-color:#ff4d4f;"></div>
      <span class="text-sm text-gray-700 mr-4">≤ 阈值（{{ thresholdValue }}）</span>
      <div class="w-4 h-4 mr-1" style="background-color:#52c41a;"></div>
      <span class="text-sm text-gray-700">＞ 阈值（{{ thresholdValue }}）</span>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, computed, nextTick, getCurrentInstance, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'

/* ============ 全局/依赖 ============ */
const { proxy } = getCurrentInstance() // 使用项目封装的 proxy.http

/* ============ 顶部筛选状态 ============ */
const nowY = new Date().getFullYear()
const yearOptions = [
  { label: String(nowY - 1), value: nowY - 1 },
  { label: String(nowY),     value: nowY },
  { label: String(nowY + 1), value: nowY + 1 }
]
const selectedYear = ref(nowY)
const dateRange = ref([new Date(`${nowY}-01-01`), new Date(`${nowY}-12-31`)])
const colorScheme = ref('gradient') // gradient|threshold
const viewMode = ref('large')       // large|compact
const cellSize = ref(30)
watch(viewMode, v => { cellSize.value = v === 'large' ? 30 : 12 })
watch(selectedYear, y => {
  dateRange.value = [new Date(`${y}-01-01`), new Date(`${y}-12-31`)]
})

/* ============ 页面数据状态 ============ */
const categoriesData = ref([]) // [{ name, lines:[{lineName,data:number[]}] }]
const dateList = ref([])
const minQuantity = ref(0)
const maxQuantity = ref(0)
const thresholdValue = ref(0)

/* ============ 工具函数 ============ */
function fmtDate(d) {
  const x = new Date(d)
  const m = String(x.getMonth() + 1).padStart(2, '0')
  const day = String(x.getDate()).padStart(2, '0')
  return `${x.getFullYear()}-${m}-${day}`
}
// 仅使用 ProductionDate（兼容 productionDate 以防大小写差异）
function getProductionDateStr(rec) {
  const v = rec?.ProductionDate ?? rec?.productionDate
  if (!v) return ''
  // 支持 "YYYY-MM-DD" / "YYYY-MM-DDTHH:mm:ss" / Date
  const d = new Date(String(v))
  if (!isNaN(d)) return fmtDate(d)
  return String(v).slice(0,10)
}

/* ============ API 调用 ============ */
async function loadData() {
  if (!dateRange.value || dateRange.value.length !== 2) return ElMessage.warning('请选择日期范围')
  const start = fmtDate(dateRange.value[0])
  const end   = fmtDate(dateRange.value[1])
  try {
    const res = await proxy.http.get(`api/WZ/ProductionOutput?start=${start}&end=${end}`, {}, true)
    const rows = Array.isArray(res) ? res : (res?.data ?? [])
    // rows 已是后端聚合后的“日×阀体×产线”，前端只需组织为热力图结构
    processData(rows)
    ElMessage.success('数据加载成功')
  } catch (e) {
    console.error(e)
    ElMessage.error('加载失败')
  }
}

async function refreshData() {
  if (!dateRange.value || dateRange.value.length !== 2) return ElMessage.warning('请选择日期范围')
  const start = fmtDate(dateRange.value[0])
  const end   = fmtDate(dateRange.value[1])
  try {
    const r = await proxy.http.post('api/WZ/ProductionOutput/refresh', { start, end }, true)
    if (r && r.status === false) return ElMessage.error(r.message || '刷新失败')
    await loadData()
  } catch (e) {
    console.error(e)
    ElMessage.error('刷新失败')
  }
}

/* ============ 数据组织（动态产线名） ============ */
function processData(records) {
  // 1) 构造日期轴
  const s = dateRange.value[0], e = dateRange.value[1]
  const dates = []
  const idxMap = new Map()
  const cur = new Date(s)
  while (cur <= e) {
    const ds = fmtDate(cur)
    idxMap.set(ds, dates.length)
    dates.push(ds)
    cur.setDate(cur.getDate() + 1)
  }
  dateList.value = dates

  // 2) 分组：Map<ValveCategory, Map<ProductionLine, number[]>>
  const groups = new Map()
  for (const r of (records || [])) {
    const cat  = String(r.valveCategory ?? r.ValveCategory ?? '').trim()
    const line = String(r.productionLine ?? r.ProductionLine ?? '').trim()
    const dStr = getProductionDateStr(r) // 仅取 ProductionDate
    const qty  = Number(r.quantity ?? r.Quantity ?? 0)
    if (!cat || !line || !dStr) continue
    const i = idxMap.get(dStr)
    if (i == null) continue

    if (!groups.has(cat)) groups.set(cat, new Map())
    const lm = groups.get(cat)
    if (!lm.has(line)) lm.set(line, Array(dates.length).fill(0))
    lm.get(line)[i] += qty
  }

  // 3) 类别与产线排序（类别优先：直通阀/球阀/蝶阀；其余按字典序；产线名“自然+字典序”）
  const pref = ['直通阀', '球阀', '蝶阀']
  const cats = Array.from(groups.keys()).sort((a,b)=>{
    const ia = pref.indexOf(a), ib = pref.indexOf(b)
    if (ia !== -1 || ib !== -1) return (ia === -1 ? 999 : ia) - (ib === -1 ? 999 : ib)
    return a.localeCompare(b, 'zh-Hans-CN')
  })

  const list = []
  for (const c of cats) {
    const lm = groups.get(c)
    const lines = Array.from(lm.keys()).sort((a,b)=>{
      const na = a.match(/\d+/)?.[0], nb = b.match(/\d+/)?.[0]
      if (na && nb && na !== nb) return Number(na) - Number(nb)
      return a.localeCompare(b, 'zh-Hans-CN')
    })
    list.push({ name:c, lines: lines.map(n => ({ lineName:n, data: lm.get(n) })) })
  }
  categoriesData.value = list

  // 4) 统计 min/max & 阈值（中位数）
  const vals = []
  let minV = Infinity, maxV = -Infinity
  list.forEach(c => c.lines.forEach(l => l.data.forEach(v => {
    vals.push(v); if (v<minV) minV=v; if (v>maxV) maxV=v
  })))
  if (!vals.length) { minV = 0; maxV = 0 }
  minQuantity.value = (minV === Infinity ? 0 : minV)
  maxQuantity.value = (maxV === -Infinity ? 0 : maxV)
  vals.sort((a,b)=>a-b)
  thresholdValue.value = vals.length
    ? (vals.length % 2 ? vals[(vals.length-1)/2] : Math.floor((vals[vals.length/2-1] + vals[vals.length/2]) / 2))
    : 0

  // 5) 同步滚动
  nextTick(() => {
    const grids = document.querySelectorAll('.heatmap-grid')
    grids.forEach(g => { g.removeEventListener('scroll', onSyncScroll); g.addEventListener('scroll', onSyncScroll) })
  })
}

/* ============ 滚动同步 ============ */
let syncing = false
function onSyncScroll(e) {
  if (syncing) return
  syncing = true
  const left = e.target.scrollLeft
  document.querySelectorAll('.heatmap-grid').forEach(g => { if (g !== e.target) g.scrollLeft = left })
  syncing = false
}

/* ============ 颜色映射 ============ */
function getCellStyle(v) {
  let bg = ''
  if (colorScheme.value === 'gradient') {
    const minV = minQuantity.value, maxV = maxQuantity.value
    if (maxV === minV) {
      bg = maxV === 0 ? '#eeeeee' : '#7fbfff'
    } else {
      const t = (v - minV) / (maxV - minV)
      const r = Math.round(255 + (0   - 255) * t) // 255->0
      const g = Math.round(255 + (123 - 255) * t) // 255->123
      const b = Math.round(255 + (255 - 255) * t) // 255->255
      bg = `rgb(${r},${g},${b})`
    }
  } else {
    bg = v <= thresholdValue.value ? '#ff4d4f' : '#52c41a'
  }
  // 自动文字色
  let r=255,g=255,b=255
  if (bg.startsWith('#')) {
    const h=bg.slice(1); r=parseInt(h.slice(0,2),16); g=parseInt(h.slice(2,4),16); b=parseInt(h.slice(4,6),16)
  } else if (bg.startsWith('rgb')) {
    const m=bg.match(/\d+/g); if (m){ r=+m[0]; g=+m[1]; b=+m[2] }
  }
  const lum = 0.299*r + 0.587*g + 0.114*b
  return { backgroundColor: bg, color: lum < 140 ? '#fff' : '#000' }
}
const gradientStyle = computed(() => 'linear-gradient(to right,#ffffff,#007BFF)')

/* ============ 卸载清理 ============ */
onUnmounted(() => {
  document.querySelectorAll('.heatmap-grid').forEach(g => g.removeEventListener('scroll', onSyncScroll))
})
</script>

<style scoped>
.category-title { font-size:16px; }
.labels-column   { min-width: 100px; } /* 自定义产线名更友好 */
.line-label      { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

.heatmap-cell { display:flex; align-items:center; justify-content:center; }
.heatmap-cell.large   { width:30px; height:30px; }
.heatmap-cell.compact { width:12px; height:12px; }
</style>
