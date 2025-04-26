let provinceOption, //省
  cityOption, //市
  countyOption //县

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
  tableAction: 'Demo_Customer', //指定某张表的权限(这里填写表名,默认不用填写)
  buttons: { view: [], box: [], detail: [] }, //扩展的按钮
  methods: {
    onInit() {
      this.editFormOptions.forEach((options) => {
        options.forEach((item) => {
          //省的配置参数
          if (item.field == 'Province') {
            provinceOption = item
          }
          //市的配置参数
          if (item.field == 'City') {
            cityOption = item
          }
          //县的配置参数
          if (item.field == 'County') {
            countyOption = item
          }
        })
      })
      //省选择事件
      provinceOption.onChange = (value, option) => {
        let url = 'api/Sys_Region/getList?code=' + value
        //给市重新绑定数据源
        this.http.get(url, {}, true).then((result) => {
          cityOption.data = result
        })
      }
      //市选择事件
      cityOption.onChange = (value, option) => {
        let url = 'api/Sys_Region/getList?code=' + value
        //给县重新绑定数据源
        this.http.get(url, {}, true).then((result) => {
          countyOption.data = result
        })
      }
      //设置二级表头
      this.initSecondColumns()
    },
    onInited() {
      //选项卡页面减去tabs高度
      if (this.$route.path == '/tabs') {
        this.height = this.height - 110
      }

      //在订单管理的选择数据弹出框显示此页面时，重新调整页面高度与隐藏其他的按钮
      if (this.$route.path == '/Demo_Order') {
        this.height = 550 - 150
        //订单页面隐藏按钮
        this.buttons.forEach((item) => {
          if (item.value != 'Search') {
            item.hidden = true
          }
        })
      }

      //设置表单分组,通过splice在指定的行位置添加信息
      this.editFormOptions.splice(0, 0, [
        {
          colSize: 12,
          render: (h) => {
            return (
              <div style="display: flex;margin-bottom: -4px;line-height: 20px; padding-bottom: 2px;border-bottom: 1px solid #e4e4e4;margin-left: 10px">
                <div style="height: 14px; background: #1794f8; width: 4px; border-radius: 10px;margin-top: 2px;"></div>
                <div style="padding-left: 6px; font-weight: bold; font-size: 13px;">基本信息</div>
              </div>
            )
          }
        }
      ])

      //设置表单分组,通过splice在第2行位置添加信息
      this.editFormOptions.splice(2, 0, [
        {
          colSize: 12,
          render: (h) => {
            return (
              <div style="display: flex;margin-bottom: -4px;line-height: 20px; padding-bottom: 2px;border-bottom: 1px solid #e4e4e4;margin-left: 10px">
                <div style="height: 14px; background: #1794f8; width: 4px; border-radius: 10px;margin-top: 2px;"></div>
                <div style="padding-left: 6px; font-weight: bold; font-size: 13px;">地址信息</div>
              </div>
            )
          }
        }
      ])
    },
    initSecondColumns() {
      //设置二级表头
      this.columns.splice(0)
      //设置二级表头
      this.columns.push(
        ...[
          {
            field: '基础信息',
            title: '基础信息',
            type: 'string',
            align: 'center',
            children: [
              {
                field: 'Customer_Id',
                title: 'Customer_Id',
                type: 'int',
                width: 110,
                hidden: true,
                readonly: true,
                require: true,
                align: 'left'
              },
              {
                field: 'Customer',
                title: '客户',
                type: 'string',
                link: true,
                width: 100,
                require: true,
                align: 'left',
                sort: true
              },
              {
                field: 'PhoneNo',
                title: '手机',
                type: 'string',
                width: 130,
                require: true,
                align: 'left'
              }
            ]
          },
          {
            field: '地址信息',
            title: '地址信息',
            type: 'string',
            align: 'center',
            children: [
              {
                field: 'Province',
                title: '省',
                type: 'string',
                bind: { key: '省', data: [] },
                width: 80,
                require: true,
                align: 'left'
              },
              {
                field: 'City',
                title: '市',
                type: 'string',
                bind: { key: '市', data: [] },
                width: 80,
                require: true,
                align: 'left'
              },
              {
                field: 'County',
                title: '县',
                type: 'string',
                bind: { key: '县', data: [] },
                width: 120,
                align: 'left'
              },
              {
                field: 'DetailAddress',
                title: '详细地址',
                type: 'string',
                width: 120,
                align: 'left'
              }
            ]
          },
          {
            field: '其它信息',
            title: '其它信息',
            type: 'string',
            align: 'center',
            children: [
              { field: 'Remark', title: '备注', type: 'string', width: 100, align: 'left' },
              {
                field: 'CreateID',
                title: 'CreateID',
                type: 'int',
                width: 80,
                hidden: true,
                align: 'left'
              },
              { field: 'Creator', title: '创建人', type: 'string', width: 100, align: 'left' },
              {
                field: 'CreateDate',
                title: '创建时间',
                type: 'datetime',
                width: 145,
                align: 'left',
                sort: true
              },
              {
                field: 'ModifyID',
                title: 'ModifyID',
                type: 'int',
                width: 80,
                hidden: true,
                align: 'left'
              },
              {
                field: 'Modifier',
                title: '修改人',
                type: 'string',
                width: 100,
                hidden: true,
                align: 'left'
              },
              {
                field: 'ModifyDate',
                title: '修改时间',
                type: 'datetime',
                width: 145,
                hidden: true,
                align: 'left',
                sort: true
              }
            ]
          }
        ]
      )
    },
    searchBefore(params) {
      //订单管理页面打开弹出框设置查询条件
      if (this.$route.path == '/Demo_Order') {
        params.wheres.push({
          name: '查询的字段',
          //订单管理查询打开时全局缓存的值,见Demo_Order.js第78行与orderModelHeader.vue第133行(2022.12.06)
          value: this.$store.getters.data().orderId
        })
      }
      return true
    },
    modelOpenAfter(row) {
      //点击编辑/新建按钮打开弹出框

      //新建操作市与县的数据源清空
      if (this.currentAction == 'Add') {
        cityOption.data = [] //市
        countyOption.data = [] //县
      } else {
        //编辑操作重新绑定当省对应下的市、县的数据
        let url = 'api/Sys_Region/getList?code=' + this.editFormFields.Province //row.Province;
        //给市重新绑定数据源
        this.http.get(url, {}, true).then((result) => {
          cityOption.data = result
        })

        //给县重新绑定数据源
        url = 'api/Sys_Region/getList?code=' + this.editFormFields.City //row.City;
        this.http.get(url, {}, true).then((result) => {
          countyOption.data = result
        })
      }
    }
  }
}
export default extension
