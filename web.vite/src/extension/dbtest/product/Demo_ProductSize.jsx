let extension = {
  components: {
    //查询界面扩展组件
    gridHeader: '',
    gridBody: '',
    gridFooter: '',
    //新建、编辑弹出框扩展组件
    modelHeader: '',
    modelBody: '',
    modelFooter: ''
  },
  tableAction: '', //指定某张表的权限(这里填写表名,默认不用填写)
  buttons: { view: [], box: [], detail: [] }, //扩展的按钮
  methods: {
    //下面这些方法可以保留也可以删除
    onInit() {

      //一对多左右结构
      if (this.$route.path == '/Demo_Product2') {
        this.load = false;
        this.columns.forEach((x) => {
          if (
            ['Creator', 'Modifier', 'ModifyDate'].indexOf(
              x.field
            ) != -1
          ) {
            x.hidden = true;
          }
        });
      }
      this.showCustom = false;
    },
    onInited() {
     
      //设置表格高度
      this.height=180;

      //页面打开时不加载数据
      if (this.$route.path.indexOf('/Demo_Product') != -1) {
        this.load = false;
        this.buttons.forEach((x) => {
          if (['Add', 'Update', 'Delete'].indexOf(x.value) != -1) {
            x.hidden = false;
          } else {
            x.hidden = true;
          }
        });
      }
      this.singleSearch = null;
    },
    searchBefore(param) {
      //明细表查询前方法

      //操作步骤3：主表点击行时,设置查询条件
      if (this.$route.path.indexOf('/Demo_Product') != -1) {
        //产品管理界面必须选中行数据后才能查询
        if (!this.$store.getters.data().ProductId) {
          // this.$message.error('请选中产品行数据');
          return false;
        }
        //查询选中行的数据
        param.wheres.push({
          name: 'ProductId', //查询字段
          value: this.$store.getters.data().ProductId //查询值为主表的主键id值
        });
      }
      return true;
    },
    addBefore(params) {
      //新建保存前
      //操作步骤5：将主表选行的值添加到明细表中(注意代码生成器，明细表的ProductId字段必须设置编辑为0，并生成下model)

      params.mainData.ProductId = this.$store.getters.data().ProductId;
      //查询后，result返回的查询数据,可以在显示到表格前处理表格的值
      return true;
    },
    modelOpenBeforeAsync() {
      //操作步骤4：打开弹出框时判断是否为新建操作，新建时必须让选中行数据

      //新建时产品管理界面必须选中行数据后才能弹出框
      if (this.$route.path == '/Demo_Product') {
        if (this.currentAction == 'Add') {
          if (!this.$store.getters.data().ProductId) {
            this.$message.error('请选中产品行数据');
            return false;
          }
        }
      }
      return true;
    }
  }
};
export default extension;
