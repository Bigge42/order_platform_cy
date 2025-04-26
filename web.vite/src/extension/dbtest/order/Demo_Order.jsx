import modelHeader from './orderModelHeader'
let extension = {
  components: {
    //查询界面扩展组件
    gridHeader: '',
    //自定义列表页面
    gridBody: '',
    gridFooter: '',
    //新建、编辑弹出框扩展组件
    modelHeader: modelHeader,
    modelBody: '',
    modelFooter: ''
  },
  text: '', //界面上的提示文字
  tableAction: '', //指定某张表的权限(这里填写表名,默认不用填写)
  buttons: { view: [], box: [], detail: [] }, //扩展的按钮
  methods: {
    searchAfter(rows) {
      return true
    },
    //下面这些方法可以保留也可以删除
    onInit() {
      // console.log('oninit')
      // //设置表格筛选
      // this.maxBtnLength = 5
      // this.queryFields = ['CreateDate']
    },
    onInited() {
     
    },
    saveConfirm(callback,formData,isAdd){
      callback()
    }
  }
}
export default extension
