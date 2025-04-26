//此js文件是用来自定义扩展业务代码，可以扩展一些自定义页面或者重新配置生成的代码
import { h, resolveComponent } from 'vue'

import gridHeader from './Demo_Product/Demo_ProductGridHeader.vue'
import gridFooter from './Demo_Product/Demo_ProductGridFooter.vue'
import modelBody from './Demo_Product/Demo_ProductModelBody.vue'

let extension = {
  components: {
    //查询界面扩展组件
    gridHeader: gridHeader,
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
              title: '一对多(支持多个二级明细与三级明细表)零代码全自动生成,点击新建或编辑查看...'
            },
            ''
          )
        ]
      }
    },
    gridFooter: gridFooter,
    //新建、编辑弹出框扩展组件
    modelHeader: '',
    modelBody: modelBody,
    modelFooter: ''
  },
  tableAction: '', //指定某张表的权限(这里填写表名,默认不用填写)
  buttons: { view: [], box: [], detail: [] }, //扩展的按钮
  text: '更快、更高效。。。',
  methods: {

    //下面这些方法可以保留也可以删除
    btn2Click(row, column, index, $e) {
      $e.stopPropagation()
      this.$message.success('点击按钮')
    },
    btn1Click(row, column, index, $e) {
      $e.stopPropagation()
      this.$message.success('点击按钮')
      //可以直接调用编辑
      //this.edit(row)
    },
    dropdownClick(value, row, column, index, $e) {
      $e.stopPropagation()
      this.$message.success('点击按钮组:' + value)
    },

    priceBtnClick(btnIndex, row, column, index, $e) {
      $e.stopPropagation()
      this.$message.success('点击了第' + btnIndex + '个图标')
    },
    onInited() {
      this.editFormOptions.forEach((x) => {
        x.forEach((ops) => {
          //"ProductName","ProductCode","Price","Remark","Creator","CreateDate","Modifier","ModifyDate"
          if (['ProductName', 'ProductCode', 'Price', 'Remark'].indexOf(ops.field) != -1) {
            ops.group = '基础信息'
          } else if (['Creator', 'CreateDate', 'Modifier', 'ModifyDate'].indexOf(ops.field) != -1) {
            ops.group = '系统信息'
          }
        })
      })

      this.showCustom=false;
      this.maxBtnLength=6;
      this.height = this.height / 2-100; // 300;

      //设置主表合计
      this.summary = true
      //如果有明细表,设置明细表合计
      // this.detailOptions.summary = true;

      //设置主表求字段，后台需要实现SummaryExpress方法
      // this.columns.forEach((x) => {
      //   if (x.field == 'Price') {
      //     x.summary = true
      //     x.align = 'center'
      //     x.width = 120
      //     x.summaryFormatter = (val, column, result, summaryData) => {
      //       if (!val) {
      //         return '0.00'
      //       }
      //       summaryData[0] = '汇总'
      //       return '￥' + (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',') + '元'
      //     }
      //     //计算平均值
      //     //x.summary = 'avg';
      //     //设置小数显示位数(默认2位)
      //     // x.numberLength = 4;
      //   }
      // })

      //可以通过this.buttons判断有没有某些按钮权限，如果有再添加自定义按钮

      if (
        this.buttons.some((x) => {
          return x.value === 'Add'
        })
      ) {
        //splice在第三个按钮后面添加一个按钮
        this.buttons.splice(3, 0, {
          name: '自定义按钮', //按钮名称
          icon: 'el-icon-document', //按钮图标https://element.eleme.cn/#/zh-CN/component/icon
          type: 'primary', //按钮样式vue2版本见iview文档button，vue3版本见element ui文档button
          plain: true,
          onClick: function () {
            this.$message.success('点击了按钮')
          }
        })
      }

      this.buttons.splice(3, 0, {
        name: '弹出框1', //按钮名称
        icon: 'el-icon-document', //按钮图标https://element.eleme.cn/#/zh-CN/component/icon
        type: 'primary', //按钮样式vue2版本见iview文档button，vue3版本见element ui文档button
        plain: true,
        onClick: function () {
          let rows = this.getSelectRows()
          if (!rows.length) {
            return this.$message.error('请选中行数据')
          }
          this.$refs.gridHeader.openModel1(rows)
        }
      })

      //splice在第三个按钮后面添加一个按钮

      let index = this.columns.findIndex((x) => {
        return x.field == 'Remark'
      })

      this.columns.splice(index, 0, {
        title: '按钮', //按钮名称
        field: '按钮',
        align: 'center',
        render: (h, { row, column, index }) => {
          return (
            <div>
              <el-button
                onClick={($e) => {
                  $e.stopPropagation()
                  this.$refs.gridHeader.openModel2(row, column, index)
                }}
                style=" height:23px;padding: 0px 8px !important;"
                size="small"
                type="primary"
                plain
              >
                弹出框2
              </el-button>
            </div>
          )
        }
      })

      //价格按钮前面增加按钮，onClick
      this.columns.find((x) => {
        return x.field == 'Price'
      }).render = (h, { row, column, index }) => {
        return (
          <div>
            <i
              onClick={($e) => {
                this.priceBtnClick(1, row, column, index, $e)
              }}
              class="el-icon-search"
              style="color: #2196F3;cursor: pointer;"
            ></i>
            <i
              onClick={($e) => {
                this.priceBtnClick(2, row, column, index, $e)
              }}
              class="el-icon-date"
              style="margin-left:6px;color: #2196F3;cursor: pointer;"
            ></i>
            <span style="margin-left:5px">{row.Price}</span>
          </div>
        )
      }

      //表格上添加自定义按钮
      this.columns.push({
        title: '操作',
        field: '操作',
        width: 150,
        align: 'center', // 'center',
        render: (h, { row, column, index }) => {
          return (
            <div>
              <el-button
                onClick={($e) => {
                  this.btn1Click(row, column, index, $e)
                }}
                type="primary"
                plain
                link
              >
                查看
              </el-button>

              {/* 通过条件判断,要显示的按钮 */}
              {/*  {
                      index % 2 === 1 
                      ?<el-button>修改</el-button>
                      : <el-button>设置</el-button>
                  } */}

              {/* 通过v-show控制按钮隐藏与显示
                下面的index % 2 === 1换成：row.字段==值 */}
              <el-button
                onClick={($e) => {
                  this.btn2Click(row, $e)
                }}
                v-show={index % 2 === 1}
                type="primary"
                plain
                link
              >
                修改
              </el-button>

              <el-button
                onClick={($e) => {
                  this.btn2Click(row, $e)
                }}
                v-show={index % 2 === 0}
                type="primary"
                link
              >
                设置
              </el-button>

              <el-dropdown
                trigger="hover"
                v-slots={{
                  dropdown: () => (
                    ////这里也可以改为循环设置下按钮，其他按钮一样，使用map循环
                    // [
                    //   { name: "京酱肉丝", value: "1" },
                    //   { name: "驴肉火烧", value: "2" }
                    // ].map(x => {
                    //   return <a>{x.name}</a>
                    // })
                    <el-dropdown-menu>
                      <el-dropdown-item>
                        <div
                          onClick={($e) => {
                            this.dropdownClick('京酱肉丝', row, column, index, $e)
                          }}
                        >
                          <a>京酱肉丝</a>
                        </div>
                      </el-dropdown-item>
                      <el-dropdown-item>
                        <div
                          onClick={($e) => {
                            this.dropdownClick('驴肉火烧', row, column, index, $e)
                          }}
                        >
                          驴肉火烧
                        </div>
                      </el-dropdown-item>
                    </el-dropdown-menu>
                  )
                }}
              >
                <el-button type="primary" style="margin-left:10px;margin-top:3px" link>更多<i class="el-icon-arrow-down"></i></el-button>
                {/* <span
                  style="font-size: 13px;color: #409eff;margin: 5px 0 0 10px;"
                  class="el-dropdown-link"
                >
                  更多<i class="el-icon-arrow-right"></i>
                </span> */}
              </el-dropdown>
            </div>
          )
        }
      })
    },
    onInit() {
      this.showTableAudit=false;
      this.columns.forEach((x) => {
        if (x.field == 'Modifier' || x.field == 'ModifyDate') {
          x.hidden = true
        }
      })

      //明细表导入
      this.details[0].buttons.unshift({
        type: 'danger',
        plain: true,
        name: '导入',
        value: 'import',
        hidden: false,
        icon: 'el-icon-upload2',
        onClick: () => {
          this.upload.url = `${this.http.ipAddress}api/Demo_ProductColor/${this.const.IMPORT}?table=1`
          this.upload.template.url = `${this.http.ipAddress}api/Demo_ProductColor/${this.const.DOWNLOADTEMPLATE}`
          //定义下载模板的文件名
          this.upload.template.fileName = '文件名'
          this.detailOptions.cnName = '导入标题'
          this.upload.excel = true
        }
      })
    },
    importAfter(data) {
      this.getTable('Demo_ProductColor').rowData.unshift(...data.data)
      return false
    },
    //设置明细表数据源显示与隐藏
    setDetailData(value, souce) {
      //找到第一个明细表Unit的数据源， this.details对象可以调试输出看，也可以直接生成的vue文件里面的details对象
      const binData = this.details[0].columns.find((c) => {
        return c.field == 'Unit'
      }).bind.data
      //循环数据源，满足条件的就显示，不满足的就隐藏
      binData.forEach((detail) => {
        //方式一：主表下拉框选中值=1时，隐藏明细表数据源key=='件的数据'，其他的显示出来
        if (value == '1') {
          detail.hidden = detail.key == '件'
        } else if (value == '0') {
          detail.hidden = detail.key == '条'
        }
        //方式二：上面只是举例，实际情况明细表的数据源自定义sql，可以返回一个标识属于哪个分组
        //如：select xx1 as key,xx2 as value,'10000' as groupValue from table

        //分组标识数据源属于分类，这样就可以批量设置数据源显示与隐藏
        //上面的数据源联动可以改为:
        // 主表下拉框选中值=1时
        // if (value == "1") {
        //   detail.hidden = detail.groupValue == '10000'
        // } else if (value == "0") {
        //   detail.hidden =  detail.groupValue == '10010'
        // }
      })

      //方式三：从后台返回要显示的数据源的key
      // let url = 'api/xx/getData?value=' + value;
      // this.http.get(url, {}, false).then(keys => {
      //   //从后台返回明细表Unit字段的数据源的key
      //   binData.forEach(c => {
      //     //返回的keys在明细表数据源中存在就显示
      //     c.hidden = keys.indexOf(c.key) == -1
      //   })
      // })
    },
    searchDetailBefore(param, table, item) {
      //明细表查询前方法

      return true
    },
    searchDetailAfter(data, table, item) {
      //明细表查询后方法
      return true
    },
    modelOpenAfter() {
      //弹出框打开后方法，为编辑时，设置明细表默认下拉框的数据源
      if (this.currentAction == 'Update') {
        this.setDetailData(this.editFormFields.Unit)
      }
    },

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
