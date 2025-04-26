//初始化主表数据选择
export const initFormSelectTable = (gridRef) => {
  //配置编辑表单下拉框table搜索选项
  const item = gridRef.getFormOption('Customer')
  item.disabled = false
  //配置请求的接口地址
  //可以使用生成的页面接口，注意接口权限问题，如果提示没有权限,参照后台后开发文档上的重写权限示例
  //item.url = 'api/Demo_Customer/getPageData';

  //尽量自定义接口，见下面的文档描述，或者Demo_CustomerController类的方法Search
  item.url = 'api/Demo_Customer/search'
  item.labelRender = (h, {}) => {
    return (
      <div>
        <el-popover placement="top-start" title="下拉table搜索" width="600" trigger="hover">
          {{
            default: () => {
              return (
                <div style="font-size:13px;line-height:2">
                  快速搜索自定义表格内容，支持数据自绑定、分页、上下键+回车选择数据
                </div>
              )
            },
            reference: () => {
              return (
                <div style="color:#1598ff">
                  <span class="el-icon-warning-outline" style="font-size:13px"></span> 客户
                </div>
              )
            }
          }}
        </el-popover>
      </div>
    )
  }
  //设置显示的字段
  item.columns = [
    {
      field: 'Customer_Id',
      title: 'Customer_Id',
      width: 110,
      hidden: true
    },
    //设置search:true,则字段可以搜索
    {
      field: 'Customer',
      title: '客户',
      width: 80,
      search: true
    }, //search是否开启表格上方的字段搜索
    {
      field: 'PhoneNo',
      title: '手机',
      width: 110,
      search: true
    },
    {
      field: 'Province',
      title: '省',
      type: 'select',
      bind: { key: '省', data: [] },
      width: 80,
      search: true
    },
    {
      field: 'DetailAddress',
      title: '详细地址',
      width: 120
    }
  ]

  //选中table数据后，回写到表单
  item.onSelect = (rows) => {
    gridRef.editFormFields.Customer = rows[0].Customer
    gridRef.editFormFields.PhoneNo = rows[0].PhoneNo
  }

  /****下面的这些都是可以选配置，上面的是必填的******/

  //(输入框搜索)表格数据加载前处理
  item.loadBefore = (param, callback) => {
    //方式1、手动设置查询条件
    // param.wheres.push({
    //       name:"Customer",
    //       value:this.editFormFields.Customer,
    //       displayType:"like"
    // })
    //方式2、给param.value设置值，后台手动处理查询条件
    param.value = gridRef.editFormFields.Customer
    callback(true)
  }

  /****************下面这些配置不是必须的**************/
  //表格数据加载后处理
  item.loadAfter = (rows, callback, result) => {
    callback(true)
  }

  //设置弹出框高度(默认200)
  item.height = 200
  //设置弹出框宽度(默认500)
  //item.width = 400;
  // item.textInline = false; //设置表格超出自动换行显示
  //设置表格是否单选
  item.single = true
  //设置是否显示分页
  item.paginationHide = false
}

//初始化明细数据选择
export const initDetailTable = (gridRef) => {
  //明细表合计时表单多个字段设置值(与上面的示例区别在于这里)
  gridRef.detailOptions.columns.forEach((col) => {
    //给数量Qty字段合计自定义显示格式、同时与表单联动显示
    if (col.field == 'Qty') {
      //value:Qty字段合计的结果
      //rows:明细表的全部数据
      //summaryArrData:所有合计的全部对象
      col.summaryFormatter = (qtyValue, column, rows, summaryArrData) => {
        //明细表输入或者值变化后给表单字段设置值
        gridRef.editFormFields.TotalQty = qtyValue

        //从明细表rows找到价格字段，手动计算合计
        let priceValue = 0
        rows.forEach((x) => {
          priceValue += x.Price || 0
        })

        //明细表数量字段+价格字段计算结果给总价设置值
        gridRef.editFormFields.TotalPrice =( qtyValue * priceValue).toFixed(2)*1.0

        //这里的return qtyValue一定要写上,自定义返回格式,return qtyValue+'件'
        return qtyValue + '件'
      }
    }
  })

  //配置编辑表单下拉框table搜索选项
  gridRef.detailOptions.columns.forEach((item) => {
    if (item.field == 'GoodsName') {
      item.readonly = false
      //配置请求的接口地址
      //可以使用生成的页面接口，注意接口权限问题，如果提示没有权限,参照后台后开发文档上的重写权限示例
      //item.url = 'api/Demo_Goods/getPageData';

      //尽量自定义接口，见下面的文档描述，或者Demo_GoodsController类的方法Search
      item.url = 'api/Demo_Goods/search'

      //设置显示的字段
      item.columns = [
        {
          field: 'GoodsName',
          title: '商品名称',
          type: 'string',
          width: 120
        },
        {
          field: 'GoodsCode',
          title: '商品编号',
          type: 'string',
          width: 100
        },
        {
          field: 'Specs',
          title: '规格',
          type: 'string',
          width: 60,
          align: 'left'
        },
        { field: 'Price', title: '单价', type: 'decimal', width: 60 },
        { field: 'Remark', title: '备注', type: 'string', width: 100 }
      ]

      //选中table数据后，回写到表单
      //editRow:当前正在编辑的行
      //rows:选中的行
      item.onSelect = (editRow, rows) => {
        editRow.GoodsName = rows[0].GoodsName
        editRow.GoodsCode = rows[0].GoodsCode
        editRow.Price = rows[0].Price
      }

      /****下面的这些都是可以选配置，上面的是必填的******/

      //(输入框搜索)表格数据加载前处理
      //editRow:当前正在编辑的行
      //param:请求的参数
      item.loadBefore = (editRow, param, callback) => {
        //方式1、手动设置查询条件
        // param.wheres.push({
        //       name:"GoodsName",
        //       value:row.GoodsName,
        //       displayType:"like"
        // })
        //方式2、给param.value设置值，后台手动处理查询条件
        param.value = editRow.GoodsName
        callback(true)
      }

      /****************下面这些配置不是必须的**************/
      //表格数据加载后处理
      //editRow:当前正在编辑的行
      //rows:后台返回的数据
      item.loadAfter = (editRow, rows, callback, result) => {
        callback(true)
      }

      //设置弹出框高度(默认200)
      item.height = 200
      //设置弹出框宽度(默认500)
      item.selectWidth = 500
      item.textInline = true //设置表格超出自动换行显示
      //设置表格是否单选
      item.single = true
      //设置是否显示分页
      item.paginationHide = true
    }
  })
}
