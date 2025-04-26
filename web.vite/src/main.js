import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import ElementPlus from 'element-plus'
// import 'element-plus/lib/theme-chalk/index.css';
import 'element-plus/dist/index.css'
import './assets/element-icon/icon.css'
import './assets/bootstrap-icons/font/bootstrap-icons.min.css'
import base from './uitils/common'
import VolProvider from './components/VolProvider'
import http from './api/http'
// import 'dayjs/locale/zh-cn'
// import locale from 'element-plus/lib/locale/lang/zh-cn'
import translator from './uitils/translator'
import permission from './api/permission'
import viewgird from './components/basic/ViewGrid'
import ServiceSelect from './components/ServiceSelect'
import VolEdit from './components/basic/VolEdit'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

const app = createApp(App)

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

import VolSelectBox from './components/basic/VolSelectBox'
import VolSelectTable from './components/basic/VolSelectTable'
translator.init(app)
app.config.globalProperties.base = Object.assign(base, VolProvider)
app.config.globalProperties.http = http
app.config.globalProperties.$tabs = {}
app.config.globalProperties.permission = permission


//自定义全局配置生成页面
app.config.globalProperties.$grid = {
  // test1() {
  // },
  // test2(){
  // },
}

app.config.globalProperties.$global = {
  dataVersion: '', //版本号字段(数据表字段必须是varchar类型，长度50)，解决多人编辑同一条数据时被替换的问题(2024.06.10)
  layout: 'left', //菜单布局方式：classics=经典导航，top=顶部导航,left=侧边导航
  theme: 'dark', //默认布局颜色：dark、blue、red、orange、green
  menuSearch: true, //菜单是否启用搜索功能
  navSearch: true, //导航是否启用菜单搜索功能2024.06.26
  table: {
    showOverflowTooltip:false,//表格内容超出时是否显示提示文本
    smallCell: true, //表格单元格大小
    useTag: true, //table组件下拉框数据源的字段是否显示背景颜色
    showAudit: true, //表格是否显示【查看流程】
    boxAudit: false, //编辑弹出框中显示审批按钮2024.07.03(与上面的showAudit同时开启才会生效)
    cascaderFullPath: true //级联数据源是否显示完整路径
  },
  border: true,
  lang: true, //是否使用多语言
  labelPosition: 'top', //表单(弹出框表单)标签显示位置,可选值，top、left，2023.07.04
  db: true, //是否使用分库
  signalR: true, //是否开启signalR
  fixedSearch: false, //2024.07.21增加全局固定查询条件
  gridPadding: false, //2024.07.21增加生成页面padding间距
  fullscreen: true, //弹出框显示最大化按钮
  audit: {
    //审核选项
    data: [
      { text: '通过', value: 1 },
      { text: '拒绝', value: 3 },
      { text: '驳回', value: 4 }
    ],
    auditType: [
      //自定义审批类型，注意:key必须>=10,并且不能重复
      // { key: 10, value: '提交人部门主管' },
      // { key: 20, value: '提交人直属上级' }
    ],
    status: [0, 2] //审核中的数据
    // 待审核 = 0,
    // 审核通过 = 1,
    // 审核中 = 2,
    // 审核未通过 = 3,
    // 驳回 = 4
  }
}

import DeptSelect from './components/DeptSelect'
import VolTitle from './components/basic/VolTitle'
import VolTable from './components/basic/VolTable/index'
import VolForm from './components/basic/VolForm/index'
import VolBox from './components/basic/VolBox/index'
//工作台管理(2024.06.10)
import VueGridLayout from 'vue-grid-layout'
import './components/VolDashboard/style.css'
import dashboard from './components/VolDashboard/vol-dashboard.js'
app
  .use(store)
  .use(ElementPlus, { size: 'default' })
  .use(router)
  .use(DeptSelect)
  .use(ServiceSelect)
  .use(VolEdit)
  .use(VolTitle)
   .use(VolTable)
  .use(VolForm)
  .use(VolBox)
  .use(viewgird)
  .use(VolSelectBox)
  //工作台管理(2024.06.10)
  .use(VolSelectTable)
  .use(VueGridLayout)
  .use(dashboard)
  .mount('#app')
app.config.globalProperties.$Message = app.config.globalProperties.$message
