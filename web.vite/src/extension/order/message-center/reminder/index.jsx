const extension = {
  components: {},
  tableAction: '', //指定某张表的权限(这里填写表名,默认不用填写)
  buttons: { view: [], box: [], detail: [] }, //扩展的按钮
  methods: {
    onInit() {
      //框架初始化配置前
      //确保隐藏搜索相关功能
      this.table.hideSearch = true;
      
      this.ck = false;
      
      this.table.showColumns = true;
    },
    onInited() {
      for (const btn of this.buttons) {
        if(btn.name === '高级查询'){
          btn.hidden=true;
        }
      }
    },
    searchBefore(param) {
      //界面查询前,可以给param.wheres添加查询参数
      //返回false，则不会执行查询
      return true;
    },
    searchAfter(result) {
      //查询后，result返回的查询数据,可以在显示到表格前处理表格的值
      return true;
    },
    addBefore(formData) {
      //新建保存前formData为对象，包括明细表，可以给给表单设置值，自己输出看formData的值
      return true;
    },
    updateBefore(formData) {
      //编辑保存前formData为对象，包括明细表、删除行的Id
      return true;
    },
    rowClick({ row, column, event }) {
      //查询界面点击行事件
    },
    modelOpenAfter(row) {
      //点击编辑、新建按钮弹出框后，可以在此处写逻辑
    },
  }
};
export default extension; 