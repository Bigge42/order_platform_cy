import { h, resolveComponent } from 'vue'
let extension = {
  components: {
    //查询界面扩展组件
    gridHeader: '',
    gridBody: {
      render() {
        return [
          h(
            resolveComponent('el-alert'),
            {
              style: { 'margin-bottom': '5px' },
              'show-icon': true,
              type: 'success',
              closable: false,
              title: '5分钟零代码即可完一个工作看台,支持不同用户权限管理、自定义接口、sql、分库、动态租户、及自定义查询等功能。'
            },
            ''
          )
        ]
      }
    },
    gridFooter: '',
    //新建、编辑弹出框扩展组件
    modelHeader: '',
    modelBody: '',
    modelRight: '',
    modelFooter: ''
  },
  tableAction: '', //指定某张表的权限(这里填写表名,默认不用填写)
  buttons: { view: [], box: [], detail: [] }, //扩展的按钮
  methods: {
    //下面这些方法可以保留也可以删除
    onInit() {
      //框架初始化配置前，
      if (
        this.buttons.some((c) => {
          return c.value == 'Add' || c.value == 'Update'
        })
      ) {
        this.columns.push({
          title: '操作',
          field: 'op',
          align: 'center',
          width: 120,
          render: (h, { row, column, index }) => {
            return (
              <div>
                <el-button
                  link
                  onClick={($e) => {
                    this.$tabs.open({
                      path:"/DashboardEdit",
                      text:"工作台模板设计",
                      query:{
                        id:row.DashboardId
                      }
                    })
                  }}
                  style="color: #2196F3;cursor: pointer;margin-right:10px"
                >
                  <Setting style=" height: 1em; margin-right: 2px" />
                  {this.$ts('模板设置')}
                </el-button>
                <el-button
                  link
                  onClick={($e) => {
                    this.$tabs.open({
                      path:"/DashboardPreview",
                      text:row.DashboardName+"(预览)",
                      query:{
                        id:row.DashboardId
                      }
                    })
                  }}
                  style="color: #2196F3;cursor: pointer;margin-right:10px"
                >
                  <Monitor style="height: 1em; margin-right: 2px" />
                  {this.$ts('预览')}
                </el-button>
              </div>
            )
          }
          // formatter: (row) => {
          //   return '<a style="cursor: pointer;color: #2196F3;">'+this.$ts('模板设置')+'</a>'
          // },
          // click: (row) => {
          //  // console.log( this.$refs.gridHeader)
          //   this.$refs.gridHeader.open(row,row.TableName);
          // }
        })
      }
    },
    onInited() {
      this.height = this.height - 40
      //框架初始化配置后
      //如果要配置明细表,在此方法操作
      //this.detailOptions.columns.forEach(column=>{ });
    },
    searchBefore(param) {
      //界面查询前,可以给param.wheres添加查询参数
      //返回false，则不会执行查询
      return true
    },
    searchAfter(result) {
      //查询后，result返回的查询数据,可以在显示到表格前处理表格的值
      return true
    },
    addBefore(formData) {
      //新建保存前formData为对象，包括明细表，可以给给表单设置值，自己输出看formData的值
      return true
    },
    updateBefore(formData) {
      //编辑保存前formData为对象，包括明细表、删除行的Id
      return true
    },
    rowClick({ row, column, event }) {
      //查询界面点击行事件
      // this.$refs.table.$refs.table.toggleRowSelection(row); //单击行时选中当前行;
    },
    modelOpenAfter(row) {
      //点击编辑、新建按钮弹出框后，可以在此处写逻辑，如，从后台获取数据
      //(1)判断是编辑还是新建操作： this.currentAction=='Add';
      //(2)给弹出框设置默认值
      //(3)this.editFormFields.字段='xxx';
      //如果需要给下拉框设置默认值，请遍历this.editFormOptions找到字段配置对应data属性的key值
      //看不懂就把输出看：console.log(this.editFormOptions)
    }
  }
}
export default extension
