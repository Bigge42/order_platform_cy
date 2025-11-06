<!doctype html>
<html lang="zh-CN">
<head>
  <meta charset="utf-8" />
  <title>技术BOM月度分析看板</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <!-- CDN（无外网请替换为内网静态源） -->
  <link rel="stylesheet" href="./lib/element-plus/index.css">
  <script src="./lib/vue.global.prod.js"></script>
  <script src="./lib/element-plus/index.full.min.js"></script>
  <script src="./lib/echarts.min.js"></script>
  <script>
    // 本地依赖健壮性校验（无外网环境）
    (function(){
      var ok = !!(window.Vue && window.ElementPlus && window.echarts);
      if(!ok){
        document.write('<div style="margin:12px;padding:10px;border:1px solid #ef4444;color:#991b1b;background:#fee2e2;border-radius:8px;">'+
          '本地依赖未找到：请将 <code>vue.global.prod.js</code>、<code>element-plus/index.full.min.js</code>、<code>element-plus/index.css</code>、<code>echarts.min.js</code> 复制到 <code>./lib/</code> 目录下。'+
          '</div>');
      }
    })();
  </script>
  <style>
    :root { --bg:#f7f9fc; --card:#ffffff; --line:#e5e7eb; --text:#334155; --text2:#0f172a; }
    *{ box-sizing:border-box; }
    body { background:var(--bg); color:var(--text); margin:0; padding:16px; font-family:system-ui, -apple-system, Segoe UI, Roboto, "Helvetica Neue", Arial; }
    .page-title { font-size:22px; margin:4px 0 12px; color:var(--text2); }
    .toolbar { display:flex; gap:8px; flex-wrap:wrap; align-items:center; margin-bottom:12px; }
    .kpis { display:grid; grid-template-columns: repeat(4, 1fr); gap:12px; margin-bottom:12px; }
    .kpi { background:var(--card); border:1px solid var(--line); border-radius:12px; padding:12px; }
    .kpi .label { font-size:12px; opacity:0.85; }
    .kpi .value { font-size:22px; margin-top:6px; color:var(--text2); }
    .grid { display:grid; grid-template-columns: 1.5fr 1.5fr 1fr; grid-auto-rows: 340px; gap:12px; }
    .panel { background:var(--card); border:1px solid var(--line); border-radius:12px; padding:10px; display:flex; flex-direction:column; overflow:hidden; }
    .panel h3 { margin:0 0 8px; font-size:14px; color:var(--text2); display:flex; align-items:center; gap:6px; }
    .chart { flex:1; min-height:0; }
    .footer-note { margin-top:6px; font-size:12px; opacity:0.8; }
    code { background: rgba(15,23,42,0.04); padding:2px 6px; border-radius:6px; border:1px solid var(--line); color:#0f172a; }
    @media (max-width:1200px){ .grid { grid-template-columns:1fr; grid-auto-rows:320px; } .kpis { grid-template-columns:repeat(2, 1fr); } }
  </style>
</head>
<body>
<div id="app">
  <div class="page-title">当月技术BOM分析看板（视图：<code>dbo.vw_OCP_Tech_BOM_Status_Monthly</code>）</div>
  <el-alert type="info" show-icon :closable="false" style="margin-bottom:10px;">
    说明：通过上方“月份选择”筛选 <code>OrderAuditDate</code>，所有图表与 KPI 仅统计该月；热力图仅统计 <code>BomDelayDays &gt; 0</code> 的单量。
  </el-alert>

  <!-- 工具条 -->
  <div class="toolbar">
    <el-date-picker v-model="q.month" type="month" placeholder="选择月份" format="YYYY-MM" value-format="YYYY-MM" style="width:160px" @change="reload()"></el-date-picker>
    <el-input v-model="q.materialCode" placeholder="物料编码关键词" clearable style="width:220px" @keyup.enter="reload()"></el-input>
    <el-checkbox v-model="q.onlyWithoutModel">仅无产品型号</el-checkbox>
    <el-switch v-model="demo" active-text="演示数据" inactive-text="接口数据"></el-switch>
    <el-button type="primary" :loading="loading" @click="reload()">查询</el-button>
    <el-button @click="exportCSV()" :disabled="rows.length===0">导出CSV（当前筛选）</el-button>
    <span class="footer-note">接口自动发现：<code>{{ apiDesc }}</code></span>
  </div>

  <!-- KPI -->
  <div class="kpis">
    <div class="kpi"><div class="label">当月总单数</div><div class="value">{{ fmtNum(metrics.total) }}</div></div>
    <div class="kpi"><div class="label">BOM及时率</div><div class="value">{{ (metrics.timelyRate*100).toFixed(1) }}%</div></div>
    <div class="kpi"><div class="label">BOM创建延期中位数（天）</div><div class="value">{{ metrics.medianOverdueDays ?? '—' }}</div></div>
    <div class="kpi"><div class="label">缺BOM累计天数平均（天）</div><div class="value">{{ metrics.avgMissingDays ?? '—' }}</div></div>
  </div>

  <!-- 图表区 -->
  <div class="grid">
    <div class="panel">
      <h3>BOM创建延期天数（饼图）</h3>
      <div id="pieDelayOverdue" class="chart"></div>
    </div>

    <div class="panel">
      <h3>审核日期单量（单月热力图，仅统计 &gt;0）</h3>
      <div id="calMonth" class="chart"></div>
    </div>

    <div class="panel">
      <h3>缺BOM累计天数 Top10 物料</h3>
      <div id="barMissingTop" class="chart"></div>
    </div>
  </div>
</div>

<script>
  // ===== Vue / ElementPlus 全局 =====
  const { createApp, ref, reactive, onMounted, nextTick } = Vue;
  const { ElMessage } = ElementPlus; // CDN 场景

  // ===== jxx 代码生成配置（合并你的两份 export default）======
  const jxx_OCP_TechManagement = {
    table:{ key:'TechID', url:'/OCP_TechManagement/', sortName:'CreateDate' }
  };
  const jxx_vw_OCP_Tech_BOM_Status_Monthly = {
    table:{ key:'PlanTraceNo', url:'/vw_OCP_Tech_BOM_Status_Monthly/', sortName:'订单日期' },
    searchFormFields:{ PlanTraceNo:'', MaterialCode:'', ProductModel:'', TCBomCreator:'' }
  };

  // ===== 工具函数（声明在前避免提升问题）======
  const trim = s => (s==null ? '' : (''+s).trim());
  const toDateStr = s => { if(!s) return null; const d=new Date(s); if(isNaN(d)) return null; return d.toISOString().slice(0,10); };
  const fmtNum = n => (n==null ? '—' : n.toLocaleString());
  const varGet = (name) => getComputedStyle(document.documentElement).getPropertyValue(name).trim() || '#334155';
  const pad = n => (n<10?'0'+n:''+n);
  function currentMonthStr(){ const d=new Date(); return `${d.getFullYear()}-${pad(d.getMonth()+1)}`; }
  function isEmptyModel(v){ const t = trim(v).toUpperCase(); return !t || t==='-' || t==='N/A' || t==='NA' || t==='（空）' || t==='NULL' || t==='无' || t==='未知'; }
  function monthMatch(dateStr, ym){ if(!dateStr||!ym) return false; const s=toDateStr(dateStr); if(!s) return false; return s.slice(0,7)===ym; }
  function quantile(sorted, qt){ if(!sorted.length) return null; const pos=(sorted.length-1)*qt; const base=Math.floor(pos); const rest=pos-base; return sorted[base]+(sorted[base+1]!==undefined?rest*(sorted[base+1]-sorted[base]):0); }
  function safeMonth(obj){ try{ const v = obj && obj.month; return v && typeof v==='string' ? v : currentMonthStr(); }catch(_){ return currentMonthStr(); } }

  // 仅统计延期>0 的天数并分桶
  function buildDelayOverdueBins(items){
    const order = ['1','2','3','4~7','8~14','15~30','31~60','>60'];
    const map = new Map(order.map(k=>[k,0]));
    items.forEach(v => {
      const d = Number(v);
      if (!Number.isFinite(d) || d<=0) return; // 只看延期
      let key;
      if (d===1) key='1'; else if (d===2) key='2'; else if (d===3) key='3'; else if (d<=7) key='4~7'; else if (d<=14) key='8~14'; else if (d<=30) key='15~30'; else if (d<=60) key='31~60'; else key='>60';
      map.set(key, map.get(key)+1);
    });
    const data = order.map(k=>map.get(k));
    const pie = order.map((k,i)=>({ name:k, value:data[i] }));
    const total = data.reduce((a,b)=>a+b,0);
    return { order, data, pie, total };
  }

  // ===== jxx 接口自动发现与统一适配 =====
  function withSlash(s){ return s.endsWith('/')?s:(s+'/'); }
  function normalizeBase(urlBase){ return withSlash(urlBase||'/vw_OCP_Tech_BOM_Status_Monthly/'); }

  /**
   * 自动尝试常见模式：
   * 1) GET  {base}getPageData?Page=1&Rows=500&...  （.NET/Vol常见）
   * 2) POST {base}getPageData  body:{Page,Rows,Query}
   * 3) GET  {base}page 或 list 或 search
   * 4) GET  {base}  （有些网关将 getPageData 映射为默认 GET）
   */
  async function autoPageFetch(base, params, body){
    const candidates = [
      { m:'GET',  p:'getPageData', qp:{ Page:params.pageNo, Rows:params.pageSize, ...params.extra } },
      { m:'POST', p:'getPageData', body:{ Page:params.pageNo, Rows:params.pageSize, Query: params.extra } },
      { m:'GET',  p:'page',        qp:{ page:params.pageNo, rows:params.pageSize, ...params.extra } },
      { m:'GET',  p:'list',        qp:{ pageNo:params.pageNo, pageSize:params.pageSize, ...params.extra } },
      { m:'GET',  p:'search',      qp:{ pageNo:params.pageNo, pageSize:params.pageSize, ...params.extra } },
      { m:'GET',  p:'',            qp:{ pageNo:params.pageNo, pageSize:params.pageSize, ...params.extra } },
    ];

    let lastErr = null;
    for (const c of candidates){
      try{
        const url = new URL(c.p, location.origin + base).toString();
        let resp;
        if (c.m==='GET'){
          const u = new URL(url);
          Object.entries(c.qp||{}).forEach(([k,v])=>{ if(v!==undefined&&v!==null&&v!=='') u.searchParams.append(k,v); });
          resp = await fetch(u.toString(), { method:'GET', credentials:'include' });
        }else{
          resp = await fetch(url, { method:'POST', credentials:'include', headers:{'Content-Type':'application/json'}, body: JSON.stringify(c.body||{}) });
        }
        if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
        const data = await resp.json();
        const parsed = unifyPageResult(data);
        return { ok:true, data: parsed, used: c };
      }catch(e){ lastErr = e; }
    }
    return { ok:false, error: lastErr };
  }

  // 兼容多种返回结构
  function unifyPageResult(payload){
    const p = payload||{};
    // 常见几种：{rows,total}、{data:{rows,total}}、{data:{list,total}}、{list,totalCount}
    let rows = p.rows || p.list || (p.data && (p.data.rows||p.data.list)) || [];
    let total = p.total || p.totalCount || (p.data && (p.data.total||p.data.totalCount)) || (Array.isArray(rows)? rows.length : 0);
    if (!Array.isArray(rows) && typeof rows==='object') rows = Object.values(rows);
    return { rows, total };
  }

  // 字段统一：兼容大写/小写字段名
  function mapRow(r){
    const pick = (obj, ...names)=>{ for(const n of names){ if(obj[n]!==undefined) return obj[n]; } return undefined; };
    return {
      planTraceNo: pick(r,'PlanTraceNo','planTraceNo'),
      materialCode: pick(r,'MaterialCode','materialCode','MaterialNumber','materialNumber'),
      materialName: pick(r,'MaterialName','materialName'),
      productModel: pick(r,'ProductModel','productModel'),
      orderNo: pick(r,'OrderNo','orderNo','SOBillNo','soBillNo'),
      orderDate: pick(r,'OrderDate','orderDate'),
      orderAuditDate: pick(r,'OrderAuditDate','orderAuditDate'),
      bomCreateDate: pick(r,'BomCreateDate','bomCreateDate','BOMCreateDate'),
      bomDelayDays: pick(r,'BomDelayDays','bomDelayDays'),
      bomMissingDays: pick(r,'BomMissingDays','bomMissingDays'),
      tcBomCreator: pick(r,'TCBomCreator','tcBomCreator','TCBOMCreator'),
      cv: pick(r,'CV','cv'),
      qty: pick(r,'Qty','qty'),
      replyDeliveryDate: pick(r,'ReplyDeliveryDate','replyDeliveryDate'),
      remarks: pick(r,'Remarks','remarks')
    };
  }

  createApp({
    setup(){
      // ===== 基本状态 =====
      const q = reactive({ month: currentMonthStr(), materialCode:'', onlyWithoutModel:false });
      const demo = ref(true);
      const loading = ref(false);
      const rows = ref([]);
      const metrics = reactive({ total:0, timelyRate:0, medianOverdueDays:null, avgMissingDays:null });
      const apiDesc = ref('演示数据');

      // ===== 演示数据 =====
      const demoRowsAll = [
        { planTraceNo:'PTN-001', materialCode:'CY-1001', materialName:'调节阀体A', productModel:'ZT-50', orderNo:'SO001', orderDate:'2025-10-02', orderAuditDate:'2025-10-03', bomCreateDate:'2025-10-05', bomDelayDays:2,  bomMissingDays:0,  tcBomCreator:'Zhang San', cv:'12.5', qty:2, replyDeliveryDate:'2025-11-01', remarks:'' },
        { planTraceNo:'PTN-002', materialCode:'CY-1002', materialName:'阀芯B',    productModel:'',      orderNo:'SO002', orderDate:'2025-10-04', orderAuditDate:'2025-10-04', bomCreateDate:null,         bomDelayDays:null,bomMissingDays:22, tcBomCreator:'Li Si',     cv:'8.0',  qty:1, replyDeliveryDate:'2025-11-05', remarks:'缺BOM' },
        { planTraceNo:'PTN-003', materialCode:'CY-1003', materialName:'执行机构C', productModel:'EA-20', orderNo:'SO003', orderDate:'2025-10-06', orderAuditDate:'2025-10-07', bomCreateDate:'2025-10-06', bomDelayDays:-1, bomMissingDays:0,  tcBomCreator:'Wang Wu',   cv:'—',   qty:4, replyDeliveryDate:'2025-12-10', remarks:'' },
        { planTraceNo:'PTN-005', materialCode:'CY-1005', materialName:'阀体组件E', productModel:'ZT-80', orderNo:'SO005', orderDate:'2025-10-12', orderAuditDate:'2025-10-13', bomCreateDate:'2025-10-20', bomDelayDays:7,  bomMissingDays:0,  tcBomCreator:'Li Si',     cv:'15.0', qty:1, replyDeliveryDate:'2025-12-01', remarks:'' },
        { planTraceNo:'PTN-101', materialCode:'CY-2001', materialName:'阀座F',    productModel:'',      orderNo:'SO101',orderDate:'2025-11-02', orderAuditDate:'2025-11-05', bomCreateDate:'2025-11-08', bomDelayDays:3,  bomMissingDays:0,  tcBomCreator:'Zhao Liu', cv:'10.0', qty:2, replyDeliveryDate:'2025-12-10', remarks:'' }
      ];

      // ===== 过滤与计算 =====
      function applyFilters(all){
        const ym = safeMonth(q);
        return (all||[]).filter(r => monthMatch(r.orderAuditDate, ym))
          .filter(r => !q.materialCode || (r.materialCode||'').includes(q.materialCode))
          .filter(r => !q.onlyWithoutModel || isEmptyModel(r.productModel));
      }
      function calcMetrics(){
        const arr = rows.value || [];
        metrics.total = arr.length;
        const delaysAll = arr.map(r => r.bomDelayDays).filter(v => v!==null && v!==undefined && !isNaN(v)).map(Number).sort((a,b)=>a-b);
        const timely = delaysAll.filter(v => v <= 0).length; metrics.timelyRate = delaysAll.length ? timely / delaysAll.length : 0;
        const delaysOver = delaysAll.filter(v => v>0).sort((a,b)=>a-b);
        const med = quantile(delaysOver, 0.5); metrics.medianOverdueDays = (med==null) ? null : Math.round(med);
        const missingDays = arr.map(r => r.bomMissingDays).filter(v => v!=null && !isNaN(v) && v>0).map(Number);
        const avg = missingDays.length ? missingDays.reduce((a,b)=>a+b,0)/missingDays.length : null; metrics.avgMissingDays = (avg==null) ? null : Number(avg.toFixed(1));
      }

      // ===== 图表渲染 =====
      let pieDelayOverdue, calMonth, barMissingTop;
      function renderCharts(){
        const el = id => document.getElementById(id);

        // 饼图：BOM创建延期>0
        const delayDays = rows.value.map(r=>r.bomDelayDays).filter(v => v!==null && v!==undefined && !isNaN(v));
        const bins = buildDelayOverdueBins(delayDays);
        if (el('pieDelayOverdue')){
          pieDelayOverdue = pieDelayOverdue || echarts.init(el('pieDelayOverdue'));
          pieDelayOverdue.setOption({
            backgroundColor:'transparent', tooltip:{ trigger:'item', formatter: p=>`${p.name} 天：${p.value}（${p.percent}%）` }, legend:{ top:0, textStyle:{ color:varGet('--text') } },
            series:[{ type:'pie', radius:['35%','70%'], avoidLabelOverlap:false, label:{ formatter:'{b}: {c} ({d}%)', color:varGet('--text') }, labelLine:{ length:12, length2:10 }, data: bins.pie }]
          });
        }

        // 热力图：仅统计 BomDelayDays>0
        const ym = safeMonth(q); const y = Number(ym.slice(0,4)); const m = Number(ym.slice(5,7));
        const daysInMonth = new Date(y, m, 0).getDate();
        const heatMap = {}; rows.value.forEach(r => { const ds = toDateStr(r.orderAuditDate); if (!ds) return; if (r.bomDelayDays==null || isNaN(r.bomDelayDays) || Number(r.bomDelayDays) <= 0) return; heatMap[ds] = (heatMap[ds]||0)+1; });
        const days = []; let maxV = 0;
        for(let d=1; d<=daysInMonth; d++){ const ds = `${ym}-${pad(d)}`; const v = heatMap[ds]||0; days.push([ds, v]); maxV = Math.max(maxV, v); }
        if (el('calMonth')){
          calMonth = calMonth || echarts.init(el('calMonth'));
          calMonth.setOption({
            backgroundColor:'transparent', tooltip:{ formatter:p=> `${p.data[0]}：${p.data[1]} 单（延期>0）` },
            visualMap:{ min:0, max: Math.max(1, maxV), calculable:true, orient:'horizontal', left:'center', bottom:0, inRange:{ color:['#e2e8f0','#94a3b8','#334155'] } },
            calendar:{ range: ym, left: 20, right: 20, top: 30, bottom: 40, splitLine:{ lineStyle:{ color:varGet('--line') } }, itemStyle:{ borderWidth:1, borderColor:varGet('--line') }, dayLabel:{ color:varGet('--text') }, monthLabel:{ show:false }, yearLabel:{ show:false } },
            series:[{ type:'heatmap', coordinateSystem:'calendar', data: days }]
          });
        }

        // Top10 条形图：缺BOM累计天数
        const missAgg = {}; rows.value.forEach(r => { if (r.bomMissingDays && Number(r.bomMissingDays) > 0) { const code = trim(r.materialCode || '(空)'); const v = Number(r.bomMissingDays); if (!missAgg[code] || v > missAgg[code]) missAgg[code] = v; } });
        const missTop = Object.entries(missAgg).sort((a,b)=>b[1]-a[1]).slice(0,10);
        const mx = missTop.map(t=>t[0]).reverse(); const my = missTop.map(t=>t[1]).reverse();
        if (el('barMissingTop')){
          barMissingTop = barMissingTop || echarts.init(el('barMissingTop'));
          barMissingTop.setOption({ backgroundColor:'transparent', tooltip:{ trigger:'axis' }, xAxis:{ type:'value', axisLabel:{ color:varGet('--text') }, splitLine:{ lineStyle:{ color:varGet('--line') } } }, yAxis:{ type:'category', data:mx, axisLabel:{ color:varGet('--text') }, axisLine:{ lineStyle:{ color:varGet('--line') } } }, grid:{ left:110, right:20, top:20, bottom:20 }, series:[{ type:'bar', data:my, barWidth:'50%' }] });
        }

        // 自适应
        if (!window.__bom_resize_bound_v6){ window.__bom_resize_bound_v6 = true; window.addEventListener('resize', () => { pieDelayOverdue && pieDelayOverdue.resize(); calMonth && calMonth.resize(); barMissingTop && barMissingTop.resize(); }); }
      }

      // ===== 调用 jxx 接口（自动发现） =====
      async function fetchFromJxx(){
        const base = normalizeBase(jxx_vw_OCP_Tech_BOM_Status_Monthly.table.url);
        const ym = safeMonth(q);
        const start = ym + '-01';
        const end = ym + '-31'; // 月末兜底，后端可自行截断
        const extra = {
          // 常见过滤键位（后端识别哪个都行；前端依然会再过滤一次保障正确）
          MaterialCode: q.materialCode || undefined,
          ProductModel: undefined,
          TCBomCreator: undefined,
          OrderAuditDateBegin: start,
          OrderAuditDateEnd: end,
          month: ym
        };
        const res = await autoPageFetch(base, { pageNo:1, pageSize:500, extra });
        if (!res.ok) throw res.error || new Error('auto discover failed');
        apiDesc.value = `${base}${res.used.p || ''} [${res.used.m}]`;
        const mapped = (res.data.rows||[]).map(mapRow);
        return mapped;
      }

      // ===== 获取全量并过滤到当前月 =====
      async function fetchAll(){
        loading.value = true;
        try{
          let list;
          if (demo.value){ list = demoRowsAll; apiDesc.value = '演示数据'; }
          else { list = await fetchFromJxx(); }
          rows.value = applyFilters(list);
          calcMetrics();
          await nextTick();
          renderCharts();
        }catch(e){
          ElMessage && ElMessage.error ? ElMessage.error('数据获取失败：'+ e.message + '（已自动切换为演示数据）') : alert('数据获取失败：'+ e.message);
          rows.value = applyFilters(demoRowsAll); apiDesc.value = '演示数据(降级)'; calcMetrics(); await nextTick(); renderCharts();
        }finally{ loading.value = false; }
      }

      function exportCSV(){
        const cols = ['planTraceNo','materialCode','materialName','productModel','orderNo','orderDate','orderAuditDate','bomCreateDate','bomDelayDays','bomMissingDays','tcBomCreator','cv','qty','replyDeliveryDate'];
        const header = cols.join(',');
        const lines = rows.value.map(r => cols.map(k => { const v = r[k] ?? ''; const s = (''+v).replace(/\"/g,'""'); return `"${s}"`; }).join(','));
        const csv = [header].concat(lines).join('\r\n');
        const blob = new Blob([csv], {type:'text/csv;charset=utf-8;'}); const url = URL.createObjectURL(blob);
        const a = document.createElement('a'); a.href = url; a.download = `bom_${safeMonth(q)}.csv`; a.click(); URL.revokeObjectURL(url);
      }

      // ===== 自测（保留旧断言 + 新增接口适配用例） =====
      function runSelfTests(){
        try{
          console.group('%cBOM Dashboard SelfTests','color:#2563eb');
          // 分桶
          const sample = [1,1,2,3,4,5,12,16,45,75,0,-1,null,undefined,NaN];
          const r = buildDelayOverdueBins(sample); const idx = (k)=>r.order.indexOf(k);
          console.assert(r.data[idx('1')]===2, 'bins: 1-day count');
          console.assert(r.data[idx('2')]===1, 'bins: 2-day count');
          console.assert(r.data[idx('3')]===1, 'bins: 3-day count');
          console.assert(r.data[idx('4~7')]===2, 'bins: 4~7 count');
          console.assert(r.data[idx('8~14')]===1, 'bins: 8~14 count');
          console.assert(r.data[idx('15~30')]===1, 'bins: 15~30 count');
          console.assert(r.data[idx('31~60')]===1, 'bins: 31~60 count');
          console.assert(r.data[idx('>60')]===1, 'bins: >60 count');
          // 月匹配
          console.assert(monthMatch('2025-10-15','2025-10')===true, 'monthMatch true');
          console.assert(monthMatch('2025-11-01','2025-10')===false, 'monthMatch false');
          // 中位数
          const bak = rows.value; rows.value = [
            { bomDelayDays:-1, bomMissingDays:0, orderAuditDate:'2025-10-01' },
            { bomDelayDays:0,  bomMissingDays:0, orderAuditDate:'2025-10-01' },
            { bomDelayDays:1,  bomMissingDays:0, orderAuditDate:'2025-10-02' },
            { bomDelayDays:2,  bomMissingDays:0, orderAuditDate:'2025-10-02' },
            { bomDelayDays:100,bomMissingDays:0, orderAuditDate:'2025-10-03' },
          ]; calcMetrics(); console.assert(metrics.medianOverdueDays===2, 'medianOverdueDays should be 2'); rows.value = bak;
          // 热力图>0 计数
          const bakQ = { ...q }; q.month = '2025-10'; rows.value = [
            { orderAuditDate:'2025-10-05', bomDelayDays:0 },
            { orderAuditDate:'2025-10-05', bomDelayDays:2 },
            { orderAuditDate:'2025-10-06', bomDelayDays:-1 },
            { orderAuditDate:'2025-10-06', bomDelayDays:3 },
          ]; const heatMap = {}; rows.value.forEach(r => { const ds = toDateStr(r.orderAuditDate); if (!ds) return; if (r.bomDelayDays==null || isNaN(r.bomDelayDays) || Number(r.bomDelayDays) <= 0) return; heatMap[ds] = (heatMap[ds]||0)+1; });
          console.assert(heatMap['2025-10-05']===1 && heatMap['2025-10-06']===1, 'calendar counts only >0'); Object.assign(q, bakQ);
          // 字段映射：大写输入应被正确转换
          const m = mapRow({ MaterialCode:'X', TCBomCreator:'A', CV:'1', OrderAuditDate:'2025-10-01', BomDelayDays:3, BomMissingDays:5 });
          console.assert(m.materialCode==='X' && m.tcBomCreator==='A' && m.cv==='1', 'mapRow uppercase mapping');
          // ElMessage
          console.assert((typeof ElMessage==='function') || (ElementPlus && typeof ElementPlus.ElMessage==='function'), 'ElMessage available');
          console.groupEnd();
        }catch(err){ console.warn('SelfTests failed:', err); }
      }

      async function reload(){ await fetchAll(); }
      onMounted(() => { fetchAll().then(runSelfTests); });

      return { q, demo, rows, metrics, loading, reload, fmtNum, exportCSV, apiDesc };
    }
  }).use(ElementPlus).mount('#app');
</script>
</body>
</html>
