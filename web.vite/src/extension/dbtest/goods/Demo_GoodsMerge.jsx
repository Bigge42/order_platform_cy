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

      this.text='<span style="color: #0f84ff;">生成页面简单配置完成字段合并</span>';
      this.queryFields=['GoodsName','CatalogId']


      //设置table表格文字超出后换行显示
      this.textInline = false

      //设置是否可用字段为switch编辑状态
      let column = this.columns.find((x) => {
        return x.field == 'Enable'
      })
      column.edit = {
        type: 'switch',
        keep: true
      }
      //是否可用字段设置切换事件并保存到数据库
      column.onChange = (value, row, tableData) => {
        let url = `api/Demo_Goods/updateStatus?goodsId=${row.GoodsId}&enable=${row.Enable}`
        this.http.get(url, {}, true).then((result) => {
          // this.$Message.success(result);
        })
      }

      this.columns.forEach((x) => {
        if (['CatalogId', 'GoodsCode', 'Img'].includes(x.field)) {
          x.hidden = true
        } else if (x.field == 'GoodsName') {
          x.width = 240;
          x.align="center";
          x.render = (h, { row, column, index }) => {
            //手动转换分类的字典编号
            let item = this.columns
              .find((c) => {
                return c.field == 'CatalogId'
              })
              .bind.data.find((c) => {
                return c.key == row.CatalogId
              })
              //获取转换的字典
            let catalog = row.CatalogId
            if (item) {
              catalog = item.value
            }

            return (
              <div style="display:flex;padding:5px;cursor: pointer;text-align: left;">
                <img
                 onClick={()=>{this.viewImg(row)}}
                  style="height: 80px;width:70px;border-radius: 5px;object-fit: cover;"
                  src={this.http.ipAddress + row.Img}
                />
                <div style="line-height: 1.2;margin:0 10px;font-size: 13px;" onClick={()=>{this.edit(row)}}>
                  <div style="color: #0f84ff;margin-bottom:5px;">{row.GoodsName}</div>
                  <div style="margin-bottom:5px;">
                    分类：<el-tag type="success">{catalog}</el-tag>{' '}
                    <span style="margin-left:10px;margin-right:5px;font-size:12px;">¥</span>
                    <span style="font-weight:bolder;color:red;font-size:15px;">{row.Price}</span>
                  </div>
                  <div>编码：{row.GoodsCode}</div>
                </div>
              </div>
            )
          }
        }
      })
    },
    viewImg(row){//预览图片
          this.$refs.table.viewImg(row,{field:"Img"});
    },
    searchBefore(param) {
      return true
    },
    modelOpenAfter(row) {}
  }
}
export default extension
