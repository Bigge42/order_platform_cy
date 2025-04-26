import gridHeader from './Demo_OrderTabsGridHeader.vue'
let extension = {
  components: {
    //查询界面扩展组件
    gridHeader: gridHeader,
    //自定义列表页面
    gridBody: '',
    gridFooter: '',
    //新建、编辑弹出框扩展组件
    modelHeader: '',
    modelBody: '',
    modelFooter: ''
  },
  text: '', //界面上的提示文字
  tableAction: '', //指定某张表的权限(这里填写表名,默认不用填写)
  methods: {
    onInit() {
      //这里可以设置默认不加载数据，通过gridHeader内调用接口获取信息后再加载数据
      // this.load=false;
      
      //自定义合计显示格式
      this.columns.forEach((x) => {
        if (x.field == 'TotalPrice') {
          x.summary = true
          x.align = 'center'
          x.width = 80
          x.summaryFormatter = (val, column, result, summaryData) => {
            if (!val) {
              return '0.00'
            }
            summaryData[0] = '汇总'
            return (
              '￥' + (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',') //+ '元'
            )
          }
          //计算平均值
          //设置小数显示位数(默认2位)
          // x.numberLength = 4;
        }
      })
    },
    onInited() {
      //这里一定要设置table显示高度减去gridHeader的高度，否则会出现两个滚动条
      this.height = this.height - 45
    },
    delAfter() { //删除、新建、编辑后刷新tabs数据
      return this.reloadData()
    },
    updateAfter() {
      return this.reloadData()
    },
    addAfter() {
      return this.reloadData()
    },
    reloadData() {
      this.$refs.gridHeader.initData();
      return true
    }
  }
}
export default extension
