<template>
  <div class="table-item">
    <div class="table-item-header">
      <vol-title icon="edit" title="表格编辑">
        功能：点击单元格编辑、api加载数据、自动分页、单元格颜色、进度条、行点击编辑前方法、自定义按钮等</vol-title
      >
      <div class="table-item-buttons">
        <div>
          <el-button type="primary" @click="addRow" plain>添加行</el-button>
          <el-button type="primary" @click="delRow" color="#f89898" plain
            >删除行</el-button
          >
          <el-button type="primary" @click="getRow" plain>获取选中行</el-button>
          <el-button type="primary" @click="reload" color="#95d475" plain>刷新</el-button>
        </div>
      </div>
    </div>
    <vol-table
      ref="table"
      :url="url"
      index
      :columns="columns"
      :height="200"
      :pagination-hide="false"
      :load-key="true"
      :column-index="true"
      :beginEdit="beginEdit"
      :endEditBefore="endEditBefore"
    ></vol-table>
  </div>
</template>
<script lang="jsx">
import VolTable from "@/components/basic/VolTable.vue";
export default {
  components: {
    "vol-table": VolTable,
  },
  data() {
    return {
      //接口返回数据，可以框架生成的接口getPageData
      //如果是自定义的接口，需要返回的数据格式：{total:100,rows:[]}
      url: "api/Demo_Order/getPageData",
      columns: [
        { field: "Order_Id", title: "Order_Id", type: "guid", width: 110, hidden: true },
        {
          field: "OrderNo",
          title: "输入框",
          type: "string",
          width: 90,
          edit: { type: "text" },
          onKeyPress: (row, column, $e) => {
            if ($e && $e.keyCode == 13) {
              this.$refs.table.toNextCell(null, row, "OrderNo", true);
            }
          },
        },
        {
          field: "OrderType",
          title: "下拉框",
          type: "int",
          bind: { key: "订单状态", data: [] },
          width: 90,
          edit: { type: "select" },
        },
        {
          field: "TotalQty",
          title: "数字",
          precision: 0,
          type: "int",
          width: 80,
          align: "center",
          edit: { type: "number", keep: true },
          extra: { text: "%", style: "margin-left:5px;padding-top:4px;" },
        }, //precision小数位数
        {
          field: "OrderDate",
          title: "日期",
          type: "date",
          width: 95,
          edit: { type: "date" },
        },

        {
          field: "Customer",
          title: "下拉table",
          type: "",
          edit: { type: "selectTable" },
          width: 100,
          url: "api/Demo_Customer/getPageData",
          columns: [
            {
              field: "Customer_Id",
              title: "Customer_Id",
              type: "int",
              width: 110,
              hidden: true,
            },
            //设置search:true,则字段可以搜索
            { field: "Customer", title: "客户", type: "string", width: 80, search: true }, //search是否开启表格上方的字段搜索
            {
              field: "PhoneNo",
              title: "手机",
              type: "string",
              width: 110,
              search: false,
            },
            {
              field: "Province",
              title: "省",
              type: "string",
              bind: { key: "省", data: [] },
              width: 80,
              search: false,
            },
            { field: "DetailAddress", title: "详细地址", type: "string", width: 120 },
          ],
          //选拔数据后回显
          onSelect: (editRow, rows) => {
            editRow.Customer = rows[0].Customer;
            editRow.PhoneNo = rows[0].PhoneNo;
          },
          loadBefore: (editRow, params, callback) => {
            //方式1、手动设置查询条件
            // param.wheres.push({
            //       name:"GoodsName",
            //       value:row.GoodsName,
            //       displayType:"like"
            // })
            //方式2、给param.value设置值，后台手动处理查询条件
            params.value = editRow.GoodsName;
            callback(true);
          },
          paginationHide: false, //显示分页
          height: 137, //表格高度
          single: true, //单选
        },
        {
          field: "PhoneNo",
          title: "下拉(联动)",
          type: "string",
          width: 90,
          //单元格颜色
          cellStyle: (row, rowIndex, columnIndex) => {
            if (row.TotalQty >= 100) {
              return { background: "rgb(246 250 253)" };
            } else {
              return { background: "rgb(204 204 204)" };
            }
          },
        },
        {
          field: "Price",
          title: "进度条",
          type: "string",
          width: 110,
          render: (h, { row, column, index }) => {
            if (index % 2 === 1) {
              //90改为row.字段
              return <el-progress stroke-width={4} percentage={90} />;
            }
            //10改为row.字段
            return (
              <el-progress
                stroke-width={4}
                color="#67c23a"
                show-text={true}
                percentage={10}
              />
            );
          },
        },
        {
          title: "操作",
          field: "操作",
          width: 150,
          align: "center", // 'center',
          render: (h, { row, column, index }) => {
            return (
              <div>
                <el-button
                  onClick={($e) => {
                    $e.stopPropagation();
                    this.editClick(row, column, index);
                  }}
                  type="primary"
                  plain
                  style="height:26px; padding: 10px !important;"
                >
                  编辑
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
                    this.btn2Click(row, $e);
                  }}
                  v-show={index % 2 === 1}
                  type="success"
                  plain
                  style="height:26px;padding: 10px !important;"
                >
                  修改
                </el-button>

                <el-button
                  onClick={($e) => {
                    this.btn2Click(row, $e);
                  }}
                  v-show={index % 2 === 0}
                  type="warning"
                  plain
                  style="height:26px;padding: 10px !important;"
                >
                  设置
                </el-button>

                <el-dropdown
                  onClick={(value) => {
                    this.dropdownClick(value);
                  }}
                  trigger="click"
                  v-slots={{
                    dropdown: () => (
                      <el-dropdown-menu>
                        <el-dropdown-item>
                          <div
                            onClick={() => {
                              this.dropdownClick("京酱肉丝", row, column);
                            }}
                          >
                            京酱肉丝
                          </div>
                        </el-dropdown-item>
                        <el-dropdown-item>
                          <div
                            onClick={() => {
                              this.dropdownClick("驴肉火烧", row, column);
                            }}
                          >
                            驴肉火烧
                          </div>
                        </el-dropdown-item>
                      </el-dropdown-menu>
                    ),
                  }}
                >
                  <span
                    style="font-size: 13px;color: #409eff;margin: 5px 0 0 10px;"
                    class="el-dropdown-link"
                  >
                    更多<i class="el-icon-arrow-right"></i>
                  </span>
                </el-dropdown>
              </div>
            );
          },
        },
      ],
    };
  },
  methods: {
    editClick(row, column, index) {
      this.$refs.table.edit.rowIndex = index;
    },
    beginEdit(row, column, index) {
      //点击行进入编辑状态

      return true; //false不会进入编辑
    },
    endEditBefore(row, column, index) {
      //结束编辑时
      this.$message.success("第" + (index + 1) + "行结束编辑");
      return true; //false不会结束编辑
    },

    getRow() {
      const rows = this.$refs.table.getSelected();
      if (!rows.length) {
        this.$message.error("请选中行");
        return;
      }
      this.$message.success(JSON.stringify(rows));
    },
    addRow() {
      this.$refs.table.addRow({ OrderNo: "D2022040600009" });
      //如果批量添加行。请使用：
      //this.$refs.table.rowData.push(...[{ OrderNo: "D2022040600009" },{ OrderNo: "D2022040600009" }])
    },
    delRow() {
      this.$refs.table.delRow();
      this.$message.success("删除成功");
    },
    reload() {
      this.$refs.table.load(null, true);
      this.$message.success("刷新成功");
    },
  },
};
</script>
<style lang="less" scoped>
.table-item-header {
  display: flex;
  align-items: center;
  padding: 6px;

  .table-item-border {
    height: 15px;
    background: rgb(33, 150, 243);
    width: 5px;
    border-radius: 10px;
    position: relative;
    margin-right: 5px;
  }

  .table-item-text {
    font-weight: bolder;
  }

  .table-item-buttons {
    flex: 1;
    text-align: right;
  }

  .small-text {
    font-size: 12px;
    color: #2196f3;
    margin-left: 10px;
    position: relative;
    top: 2px;
  }
}
</style>
