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
          v-model="startDateModel"
          type="date" size="small" style="width:132px"
          :disabled-date="d => disabledStart(d)"
          @change="onStartChange"
        />
        <span class="ph-sub">至</span>
        <el-date-picker
          v-model="endDateModel"
          type="date" size="small" style="width:132px"
          :disabled-date="d => disabledEnd(d)"
          @change="onEndChange"
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

        <el-button type="primary" size="small" @click="loadData">加载数据</el-button>
        <el-button size="small" :loading="refreshLoading" @click="refreshCurrentMonth">同步当月数据</el-button>
        <el-button size="small" @click="exportData">导出数据</el-button>
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
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, nextTick, getCurrentInstance } from 'vue'
import { ElMessage } from 'element-plus'

/* ===== 尺寸参数 ===== */
const SIZE = { small: 12, big: 20 }
const GAP  = { normal: 2, compact: 1 }
const PAD  = { normal: 32, compact: 24 }

/* ===== 工具函数 ===== */
const fmtYMD = d => `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`
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
  big: false,
  compact: false,
})

const thrDialog = ref(false)
const thrDraft  = ref({}) // 弹窗草稿
const chartsEl  = ref(null)
const { proxy } = getCurrentInstance() || {}
const refreshLoading = ref(false)

/* 原始返回数据（用于导出） */
const rawRows = ref([])

/* 年份选项（±3 年） */
const yearOptions = Array.from({length:7}, (_,i)=> state.year - 3 + i)

/* 日期控件 */
const startDateModel = ref(state.rangeStart)
const endDateModel   = ref(state.rangeEnd)
const disabledStart  = d => d.getFullYear() !== state.year
const disabledEnd    = d => d.getFullYear() !== state.year
function onYearChange(){
  state.rangeStart = new Date(state.year,0,1)
  state.rangeEnd   = new Date(state.year,11,31)
  startDateModel.value = state.rangeStart
  endDateModel.value   = state.rangeEnd
  renderAll()
}
function onStartChange(val){
  const d = new Date(val); if (d > state.rangeEnd) { state.rangeEnd = d; endDateModel.value = d }
  state.rangeStart = d; renderAll()
}
function onEndChange(val){
  const d = new Date(val); if (d < state.rangeStart) { state.rangeStart = d; startDateModel.value = d }
  state.rangeEnd = d; renderAll()
}
function resetFullYear(){
  state.rangeStart = new Date(state.year,0,1)
  state.rangeEnd   = new Date(state.year,11,31)
  startDateModel.value = state.rangeStart
  endDateModel.value   = state.rangeEnd
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
  const cellSize = state.big ? SIZE.big : SIZE.small
  const gap = state.compact ? GAP.compact : GAP.normal
  const pad = state.compact ? PAD.compact : PAD.normal
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
    head.innerHTML = `<div class="name">${v}</div><div class="meta">${state.mode==='threshold'?'阈值红绿':'标准渐变'} · ${state.scope==='line'?'按产线':state.scope==='valve'?'按阀体':'全局'}</div>`
    wrapper.appendChild(head)

    const cols = daysCount, rows = cat.lines.length
    const width = pad*2 + cols*(cellSize+gap) - gap
    const height= pad*2 + rows*(cellSize+gap) - gap
    const svg=document.createElementNS(svgNS,'svg'); svg.setAttribute('width', width); svg.setAttribute('height', height)

    // 月份文本
    monthsStartIndex.forEach(k=>{
      const d=days[k]
      const x=pad + k*(cellSize+gap)
      const t=document.createElementNS(svgNS,'text'); t.setAttribute('x',x); t.setAttribute('y',pad-10)
      t.setAttribute('font-size','10'); t.setAttribute('fill','#475569'); t.textContent=`${d.getMonth()+1}月`
      svg.appendChild(t)
    })
    // 产线文本
    cat.lines.forEach((l,r)=>{
      const t=document.createElementNS(svgNS,'text'); t.setAttribute('x',8); t.setAttribute('y', pad + r*(cellSize+gap) + Math.min(9, cellSize-3))
      t.setAttribute('font-size','10'); t.setAttribute('fill','#64748b'); t.textContent=l
      svg.appendChild(t)
    })

    // 单元格
    cat.lines.forEach((l,r)=>{
      const arr = state.data?.[v]?.[l] || []
      let lineMax=0, lineMin=Infinity
      for(let i=0;i<cols;i++){ const val=arr[i]||0; if(val>lineMax) lineMax=val; if(val<lineMin) lineMin=val }
      if(lineMin===Infinity) lineMin=0

      for(let k=0;k<cols;k++){
        const x=pad + k*(cellSize+gap), y=pad + r*(cellSize+gap)
        const val = arr[k] ?? 0
        const thr = ensureThr(v, l)
        const aboveMax = Math.max(0, lineMax-thr), belowMax = Math.max(thr - lineMin, 0)
        const colorMax = state.scope==='line' ? lineMax : state.scope==='valve' ? valveMax[v] : globalMax
        const fill = state.mode==='threshold' ? colorFromThreshold(val, thr, aboveMax, belowMax) : colorFromValue(val, colorMax)

        const rect=document.createElementNS('http://www.w3.org/2000/svg','rect')
        rect.setAttribute('x', x); rect.setAttribute('y', y)
        rect.setAttribute('width', cellSize); rect.setAttribute('height', cellSize)
        rect.setAttribute('rx', 2); rect.setAttribute('ry', 2)
        rect.setAttribute('fill', fill); rect.setAttribute('stroke', '#fff'); rect.setAttribute('stroke-width', 1)
        rect.style.cursor='pointer'

        rect.addEventListener('click', ()=>{
          const input = window.prompt(`请输入 ${v}-${l}（${fmtYMD(days[k])}）的新产量`, String(val))
          if(input==null) return
          const num = Math.max(0, Math.floor(Number(input)||0))
          state.data[v][l][k] = num
          renderAll()
        })
        rect.addEventListener('mouseenter', ev=>{
          const tip = document.getElementById('tooltip')
          const lines = [
            `阀体：${v}`, `产线：${l}`, `日期：${fmtYMD(days[k])}`,
            `数量：${val}`, `阈值：${thr}`,
            (val>thr ? `状态：超阈值 +${val-thr}` : `状态：未超阈值 ${thr-val}`)
          ]
          tip.textContent = lines.join('\n'); tip.style.display='block'
          tip.style.left=(ev.clientX+12)+'px'; tip.style.top=(ev.clientY+12)+'px'
        })
        rect.addEventListener('mousemove', ev=>{
          const tip = document.getElementById('tooltip'); tip.style.left=(ev.clientX+12)+'px'; tip.style.top=(ev.clientY+12)+'px'
        })
        rect.addEventListener('mouseleave', ()=>{
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

    wrapper.appendChild(svg)
    container.appendChild(wrapper)
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
function saveThresholds(){
  state.thresholds = JSON.parse(JSON.stringify(thrDraft.value || {}))
  thrDialog.value = false
  renderAll()
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

    const rows =
      Array.isArray(res) ? res :
        Array.isArray(res?.data) ? res.data :
          Array.isArray(res?.Data) ? res.Data :
            Array.isArray(res?.result) ? res.result : []

    rawRows.value = rows // 记录原始数据供导出
    console.debug('[WZ_ProductionOutput] rows sample =>', rows?.slice?.(0,5))

    // 日期轴
    const days = daysBetween(state.rangeStart, state.rangeEnd)
    const idxMap = buildIdxMap(days)
    const daysCount = days.length

    // 动态收集 阀体→产线
    const catMap = new Map() // Map<string, Set<string>>
    for (const r of rows){
      const v = String(r.valveCategory ?? r.ValveCategory ?? '').trim()
      const l = String(r.productionLine ?? r.ProductionLine ?? '').trim()
      if (!v || !l) continue
      if (!catMap.has(v)) catMap.set(v, new Set())
      catMap.get(v).add(l)
    }
    // 排序
    const categories = Array.from(catMap.keys())
      .sort((a,b)=>a.localeCompare(b, 'zh-Hans-CN'))
      .map(v=>{
        const lines = Array.from(catMap.get(v)).sort((a,b)=>{
          const na=a.match(/\d+/)?.[0], nb=b.match(/\d+/)?.[0]
          if (na && nb && na!==nb) return Number(na)-Number(nb)
          return a.localeCompare(b, 'zh-Hans-CN')
        })
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
        if (typeof state.thresholds[cat.name][l] !== 'number') state.thresholds[cat.name][l] = 20
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
      ElMessage.success('数据加载成功')
    }
  }catch(e){
    console.error(e)
    ElMessage.error('加载失败，请查看控制台 Network/Console 日志')
  }
}

async function refreshCurrentMonth(){
  if (refreshLoading.value) return
  refreshLoading.value = true
  try{
    const res = await proxy?.http?.post('/api/WZ/ProductionOutput/refresh')
    const status = res?.status ?? res?.Status ?? res?.success ?? res?.Success
    if (status === false) {
      ElMessage.error(res?.message || res?.Message || '同步当月数据失败')
    } else {
      ElMessage.success(res?.message || res?.Message || '同步当月数据成功')
      await loadData()
    }
  }catch(e){
    console.error(e)
    ElMessage.error('同步当月数据失败')
  }finally{
    refreshLoading.value = false
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

.btn-amber{--el-button-bg-color:var(--amber);--el-button-text-color:#fff;border:none}

.ph-grid3{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:12px;padding:16px 16px 0}
.ph-card{background:var(--card);border:1px solid var(--border);border-radius:16px;box-shadow:0 1px 6px rgba(15,23,42,.04);padding:14px}
.stat .label{font-size:12px;color:var(--muted)}
.stat .value{font-size:22px;font-weight:700;margin-top:4px}
.stat .sub{font-size:12px;color:var(--slate400);margin-top:2px}

.ph-charts{display:flex;flex-direction:column;gap:16px;padding:0 16px 16px}
.valve .name{font-weight:600;font-size:13px;color:#111827}
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

/* 紧凑模式整体缩紧 */
.compact .ph-container{padding:8px 12px}
.compact .ph-grid3{gap:8px;padding:10px 12px 0}
.compact .ph-card{padding:10px;border-radius:12px}
.compact .ph-charts{gap:12px;padding:0 12px 12px}
</style>
