//此js文件是用来自定义扩展业务代码，可以扩展一些自定义页面或者重新配置生成的代码
import { h, resolveComponent } from 'vue'

import gridHeader from './Demo_Product2/Demo_ProductGridHeader.vue'
import gridFooter from './Demo_Product2/Demo_ProductGridFooter.vue'
import modelBody from './Demo_Product2/Demo_ProductModelBody.vue'

let extension = {
  components: {
    //查询界面扩展组件
    gridHeader: gridHeader,
    // gridBody: {
    //   render() {
    //     return [
    //       h(
    //         resolveComponent('el-alert'),
    //         {
    //           style: { 'margin-bottom': '5px' },
    //           'show-icon': true,
    //           type: 'success',
    //           closable: false,
    //           title:
    //             '与上面区别：可通过属性配置明细表显示为左右结构,点击编辑或者新建可以查看效果(文档：前端开发文档->编辑弹出框三级明细左右结构显示)'
    //         },
    //         ''
    //       )
    //     ]
    //   }
    // },
    gridFooter: gridFooter,
    //新建、编辑弹出框扩展组件
    modelHeader: '',
    modelBody: modelBody,
    modelFooter: ''
  },
  tableAction: '', //指定某张表的权限(这里填写表名,默认不用填写)
  buttons: { view: [], box: [], detail: [] }, //扩展的按钮
  text: '一对多更快、更高效。。。',
  methods: {
    initAuditColumn() {},
    onInited() {
      this.dragPosition = 'bottom' //设置拖动改变表格高度
      //设置明细表显示顺序
      this.multiple.horizontal = true //一对多水平显示(二级表与三级表是否左右结构显示)
      this.multiple.leftWidth = 600 //左边二级表宽度(默认不用设置)
      //this.multiple.rightWidth = this.boxOptions.width - 600; //左边三级表宽度(默认不用设置)

      this.height = this.height-310;

      //设置主表合计
      // this.summary = true;
      //如果有明细表,设置明细表合计
      // this.detailOptions.summary = true;

      //设置主表求字段，后台需要实现SummaryExpress方法
      // this.columns.forEach((x) => {
      //   if (x.field == 'Price') {
      //     x.summary = true;
      //     //计算平均值
      //     //x.summary = 'avg';
      //     //设置小数显示位数(默认2位)
      //     // x.numberLength = 4;
      //   }
      // });

      if (this.$route.path == '/Demo_Product2') {
        this.details.forEach((x) => {
          x.columns.forEach((item) => {
            if (['CreateDate', 'Creator', 'Modifier', 'ModifyDate'].indexOf(item.field) != -1) {
              item.hidden = true
            }
          })
        })

        this.subDetails.forEach((x) => {
          x.columns.forEach((item) => {
            if (['CreateDate', 'Creator', 'Modifier', 'ModifyDate'].indexOf(item.field) != -1) {
              item.hidden = true
            }
          })
        })
      }
    },
    //明细表点击行事件
    // detailRowClick({ row, column, event, item }) {
    //   this.getTable(item.table).$refs.table.toggleRowSelection(row);
    // },
    onInit() {
      this.showCustom = false
      this.showTableAudit = false
      this.columns.forEach((x) => {
        if (x.field == 'ProductCode') {
          x.width=150;
          x.render = (h, { row, column, index }) => {
            return (
              <el-popover placement="right" title="提示信息" width={250} trigger="hover">
                {{
                  reference:()=> (
                    <div>
                      <div><i class="el-icon-warning-outline"></i> {row.ProductCode}</div>
                    </div>
                  ),
                  default: () => {
                    return (
                      <div style="line-height:1.5;">
                        <div>商品编号：{row.ProductCode}</div>
                        <div>商品价格：{row.Price}</div>
                        <div>创建时间：{row.CreateDate}</div>
                      </div>
                    )
                  }
                }}
              </el-popover>
            )
          }
        }
      })
    },
    searchDetailBefore(param, table, item) {
      //明细表查询前方法

      return true
    },
    searchDetailAfter(data, table, item) {
      //明细表查询后方法
      return true
    },
    modelOpenAfter() {},

    searchAfter(result) {
      //查询后，清空原来记录选中行的id
      // this.$store.getters.data().ProductId = null;
      return true
    },
    //操作步骤1：主表点击事件加载明细数据
    rowClick({ row, column, event }) {
      //点击主表table行数据，加载主界面下面的两个table数据
      //vuex
      //缓存当前选中行的主键id
      this.$store.getters.data().ProductId = row.ProductId

      //清除原来选中的行
      this.$refs.table.$refs.table.clearSelection()

      //查询界面点击行事件
      this.$refs.table.$refs.table.toggleRowSelection(row, true) //单击行时选中当前行;

      //调用下面选项卡的查询
      //见Demo_ProductGridFooter.vue文件rowClick方法
      this.$refs.gridFooter.rowClick(row)
    }
  }
}
export default extension
