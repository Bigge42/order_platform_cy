<template>
  <div class="table-item">
    <div class="table-item-header">
      <div class="table-item-border"></div>
      <span class="table-item-text">基础表格、拖拽排序</span>
      <div class="small-text">功能: 自动绑定数据源，行点击事件、拖拽排序行排序</div>
      <div class="table-item-buttons">
        <div>
          <el-button type="primary" @click="addRow" plain>添加行</el-button>
          <el-button type="primary" @click="delRow" color="#f89898" plain
            >删除行</el-button
          >
          <el-button type="primary" @click="getRow" plain>获取选中行</el-button>
          <el-button type="primary" @click="clearRow" color="#95d475" plain
            >清空行</el-button
          >
        </div>
      </div>
    </div>
    <vol-table
      ref="table"
      :ck="true"
      index
      :tableData="tableData"
      @rowClick="rowClick"
      :columns="columns"
      :max-height="400"
      :sortable="true"
      :pagination-hide="true"
      :load-key="true"
      :column-index="true"
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
      columns: [
        { field: "Order_Id", title: "Order_Id", type: "guid", width: 110, hidden: true },
        {
          field: "OrderNo",
          title: "订单编号",
          filterData: true,
          type: "string",
          width: 130,
        },
        {
          field: "OrderType",
          title: "订单类型",
          filterData: true,
          type: "int",
          bind: { key: "订单类型", data: [] },
          width: 70,
        },
        {
          field: "TotalPrice",
          title: "总价",
          filterData: true,
          type: "decimal",
          summary: true,
          width: 60,
          align: "center",
          summaryFormatter: (val, col, data, summaryData) => {
            //自定义格式化显示
            if (!val) {
              return "";
            }
            val = (val + "").replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, "$&,");
            return "¥" + val;
          },
        },
        {
          field: "TotalQty",
          title: "总数量",
          type: "int",
          summary: true,
          width: 80,
          align: "center",
        },
        { field: "OrderDate", title: "订单日期", type: "date", width: 95 },
        { field: "CustomerId", title: "客户", type: "int", width: 80, hidden: true },
        { field: "Customer", title: "客户", type: "string", width: 80 },
        { field: "PhoneNo", title: "手机", type: "string", width: 110 },
        { field: "CreateDate", title: "创建时间", type: "datetime", width: 120 },
      ],
      tableData: [
        {
          OrderNo: "D2023082400001",
          OrderType: 3,
          TotalPrice: 10000,
          TotalQty: 20000,
          OrderDate: "2023-08-09 00:00:00",
          CustomerId: null,
          Customer: "阮星竹",
          PhoneNo: "18612009000",
          OrderStatus: 3,
          Remark: null,
          CreateDate: "2023-08-24 00:52:52",
        },
        {
          OrderNo: "D2022042100002",
          OrderType: 2,
          TotalPrice: 9000,
          TotalQty: 45,
          OrderDate: "2022-04-21 00:00:00",
          CustomerId: null,
          Customer: "王语嫣",
          PhoneNo: "18612349000",
          OrderStatus: 1,
          Remark: "90000",
          CreateDate: "2022-04-21 22:35:49",
        },
        {
          OrderNo: "D2022040600003",
          OrderType: 2,
          TotalPrice: 100,
          TotalQty: 100,
          OrderDate: "2022-04-06 00:00:00",
          CustomerId: null,
          Customer: "王语嫣",
          PhoneNo: "18612349000",
          OrderStatus: 2,
          Remark: null,
          CreateDate: "2022-04-06 01:14:00",
        },
      ],
    };
  },
  methods: {
    rowClick({ row, column, event }) {
      console.log(column);
      this.$refs.table.$refs.table.toggleRowSelection(row);
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
      this.tableData.push({ OrderNo: "D2022040600009" });
    },
    delRow() {
      this.$refs.table.delRow();
      this.$message.success("删除成功");
    },
    clearRow() {
      this.tableData.splice(0);
      this.$message.success("数据已清空");
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
