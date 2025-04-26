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
      ref="tableRef"
      :ck="true"
      index
      drag-position="bottom"
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
<script setup>
import VolTable from "@/components/basic/VolTable-01.vue";
import { ref, reactive, getCurrentInstance } from "vue";

const { proxy } = getCurrentInstance();
const tableRef = ref();

const rowClick = ({ row, column, event }) => {
  console.log(column);
  tableRef.value.toggleRowSelection(row);
};
const getRow = () => {
  const rows = tableRef.value.getSelected();
  if (!rows.length) {
    proxy.$message.error("请选中行");
    return;
  }
  proxy.$message.success(JSON.stringify(rows));
};
const addRow = () => {
  tableData.push({ OrderNo: "D2022040600009" });
};
const delRow = () => {
  tableRef.value.delRow();
  proxy.$message.success("删除成功");
};
const clearRow = () => {
  tableData.splice(0);
  proxy.$message.success("数据已清空");
};

const columns = reactive([
  { field: "Order_Id", title: "Order_Id", type: "guid", width: 110, hidden: true },
  {
    field: "OrderNo",
    title: "订单编号",
    filterData: true,
    type: "string",
    width: 100,
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
  { field: "CreateDate", title: "创建时间", type: "datetime", width: 150 },
]);
const tableData = reactive([
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
    OrderType: 3,
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
    OrderNo: "D2022042100002",
    OrderType: 1,
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
    OrderType: 1,
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
]);
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
