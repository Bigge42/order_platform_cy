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
  tableAction: 'Demo_Catalog', //指定某张表的权限(这里填写表名,默认不用填写)
  buttons: { view: [], box: [], detail: [] }, //扩展的按钮
  methods: {
    saveConfirm(callback){
          console.log('jsx','saveConfirm')
          callback();
    },
    //下面这些方法可以保留也可以删除
    onInit() {
      // this.gridRender = {
      //   data: {
      //     value1: '-1', //默认值下面v-model需要使用
      //     value2: '1',
      //     data: [] //声明默认值，下面item.data使用
      //   },
      //   h: (h, { item }) => {
      //     //这里也可以从后台返回数据，
      //     // this.http.post('url', {}).then((x) => {
      //     item.data = [
      //       { key: '-1', value: '全部' },
      //       { key: '1', value: '成功' },
      //       { key: '0', value: '异常' }
      //     ]
      //     // })

      //     //这里要什么就写什么标签
      //     return (
      //       <div style="position: relative;bottom: -6px;">
      //         <el-radio-group
      //           size="small"
      //           style="margin-left:15px;"
      //           v-model={item.value1}
      //           onChange={(v) => {
      //             this.$message.success(item.value1)
      //           }}
      //         >
      //           {item.data.map((c) => {
      //             return <el-radio-button label={c.key}>{c.value}</el-radio-button>
      //             // <el-radio  label={c.key}>{c.value}</el-radio>
      //           })}
      //         </el-radio-group>

      //         {/* <el-select
      //           style="width:120px;margin-left:15px;"
      //           v-model={item.value2}
      //           onChange={(v) => {
      //             this.$message.success(item.value2)
      //           }}
      //         >
      //           {item.data.map((c) => {
      //             return <el-option key={c.key} label={c.value} value={c.key} />
      //           })}
      //         </el-select> */}
      //       </div>
      //     )
      //   }
      // }

      //示例：设置修改新建、编辑弹出框字段标签的长度
      // this.boxOptions.labelWidth = 150;
      //显示所有查询条件
      this.setFiexdSearchForm(true)

      //设置主键字段
      this.rowKey = 'CatalogId'
    },
    dicInited(params) {
      //数据源加载完成时的方法,2022.04.04更新method.js文件后才能使用
    },
    onInited() {
      // this.height = this.height - 76
      //框架初始化配置后
      //如果要配置明细表,在此方法操作
      //this.detailOptions.columns.forEach(column=>{ });
      // this.boxOptions.height = 390

      //选项卡页面减去tabs高度
      if (this.$route.path == '/tabs') {
        this.height = this.height - 115
      }
    },
    /***加载后台数据见Demo_CatalogController.cs文件***/
    loadTreeChildren(tree, treeNode, resolve) {
      //加载子节点
      let url = `api/Demo_Catalog/getChildrenData?catalogId=${tree.CatalogId}`
      this.http.post(url, {}).then((result) => {
        resolve(result.rows)
      })
    },
    /***加载后台数据见Demo_CatalogController.cs文件***/
    searchBefore(params) {
      //查询前的方法，如果没有输入查询条件，默认显示一级节点的数据
      if (params.wheres.length == 0) {
        params.value = 1
      }
      return true
    },
    addAfter() {
      //新建后刷新下级联的数据字典
      this.initDicKeys()
      return true
    },
    updateAfter() {
      //编辑后刷新下级联的数据字典
      this.initDicKeys()
      return true
    },
    delAfter() {
      //删除后刷新下级联的数据字典
      this.initDicKeys()
      return true
    }
  }
}
export default extension
