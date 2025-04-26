<template>
  <div class="table-item">
    <div style="padding-top: 10px">
      <vol-form
        :labelWidth="90"
        ref="formRef"
        :formFields="formFields"
        :formRules="formRules"
      >
      </vol-form>
    </div>

    <div class="table-item-header">
      <vol-title icon="edit" title="表格编辑">
        功能:自定义页面使用框架表单volform做查询条件,内部自动绑定数据源,并调用生成页面接口,默认不需要写后台代码，减少大量工作</vol-title
      >
      <div class="table-item-buttons">
        <div>
          <el-button type="primary" @click="search" plain>查询</el-button>
        </div>
      </div>
    </div>
    <vol-table
      ref="tableRef"
      :ck="false"
      column-index
      :columns="columns"
      :max-height="maxHeight"
      :url="url"
      :pagination-hide="false"
      @loadBefore="loadBefore"
      :spanMethod="spanMethod"
    ></vol-table>
  </div>
</template>
<script setup>
import VolTable from "@/components/basic/VolTable.vue";
import VolForm from "@/components/basic/VolForm.vue";
import { ref, reactive, getCurrentInstance, nextTick } from "vue";
const { proxy } = getCurrentInstance();
const tableRef = ref();
const formRef = ref();
const maxHeight = ref(0);
//计算表格高度:屏幕高度减去其他标签高度
maxHeight.value = document.body.clientHeight - 320;
const url = ref("api/Demo_Order/getPageData");

const search = () => {
  tableRef.value.load();
};
//getSearchParameters
//表格数据加载前设置查询条件
const loadBefore = (param, callBack) => {
  //1.自动生成查询条件
  param.wheres = proxy.base.getSearchParameters(proxy, formFields, formRules);
  //2.手动设生成询条件
  // let wheres = [
  //   {
  //     name: "OrderNo",
  //     value: formFields.OrderNo,
  //     displayType: "like",
  //     //displayType查询类型可选值：
  //     //like        模糊查询
  //     //selectList  多选,对应sql的where xxx in (1,2,3)
  //     //thanorequal 大于等于
  //     //gt          大于
  //     //lessorequal 小于等于
  //     //lt          小于
  //     //in          对应sql的where xxx in (1,2,3)
  //     //notIn       对应sql的where xxx not in (1,2,3)
  //     //其他类型请在后台的查询方法设置
  //   }
  // ];
  // param.wheres.push(...wheres);
  callBack(true); //返回false，界面上不会显示
};

//合并单元格，自己根据rows计算要合并的行与列坐标
const spanMethod = ({ row, column, rowIndex, columnIndex }, rows) => {
  //spanMethod放在[表.js]文件中，与onInit方法同级
  //根据rowIndex, columnIndex 值按需要合并
  if (rowIndex % 3 === 0) {
    if (columnIndex === 7) {
      return [1, 2];
    } else if (columnIndex === 5) {
      return [0, 0];
    }
  }
  if (columnIndex === 1) {
    if (rowIndex % 2 === 0) {
      return {
        rowspan: 2,
        colspan: 1,
      };
    } else {
      return {
        rowspan: 0,
        colspan: 0,
      };
    }
  }
};

const columns = reactive([
  { field: "Order_Id", title: "Order_Id", type: "guid", width: 110, hidden: true },
  {
    field: "OrderNo",
    title: "订单编号",
    filterData: true, //开启筛选
    showOverflowTooltip: true, //内容超出时鼠标放上去提示文本
    align: "center", //居中显示：center,居左显示:left，靠右显示:right
    fixed: true, //固定列, 如果固定到右边，true改为"right"
    width: 100,
  },
  {
    field: "OrderType",
    title: "订单类型",
    titleStyle: "color:#0085ee", //标题样式
    tip: {
      //自定义表头提示
      text: "自定表头提示",
      icon: "el-icon-edit-outline",
      color: "#0085ee", //图标颜色
      click: () => {
        //图标点击事件
        proxy.$message.success("点击了表头");
      },
    },
    type: "int",
    bind: { key: "订单类型", data: [] },
    width: 70,
  },
  {
    field: "TotalPrice",
    title: "总价",
    type: "decimal",
    summary: true,
    width: 60,
    sort: true, //排序
    align: "center",
    summaryFormatter: (val, col, data, summaryData) => {
      //自定义格式化合计显示
      if (!val) {
        return "";
      }
      summaryData[0] = "汇总+"; //自定义合计名称
      //自定义合计值的格式
      val = (val + "").replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, "$&,");
      return "¥" + val;
    },
  },
  {
    field: "TotalQty",
    title: "总数量",
    type: "int",
    summary: true, //开启合计
    numberLength: 2, //保留两位小数
    width: 80,
    align: "center",
  },
  { field: "OrderDate", title: "订单日期", type: "date", width: 95 }, //如果要显示日期时分秒改为type: "datetime"
  { field: "Customer", title: "客户", type: "string", width: 80 },
  { field: "PhoneNo", title: "手机", type: "string", width: 110 },
  { field: "CreateDate", title: "创建时间", type: "datetime", width: 150 },
]);

//表单查询配置
//更多表单配置信息，见表单示例
const formFields = reactive({
  OrderNo: null,
  OrderType: null,
  TotalQty: [null, null],
  OrderDate: [null, null],
  OrderStatus: null,
});
const formRules = reactive([
  [
    { title: "订单编号", field: "OrderNo", type: "like" }, //like设置为模糊查询
    {
      dataKey: "订单类型",
      data: [],
      title: "订单类型",
      field: "OrderType",
      type: "select",
    },
    { title: "总数量", field: "TotalQty", type: "number", range: true },
    { title: "订单日期", range: true, field: "OrderDate", type: "datetime" },
    {
      dataKey: "订单状态",
      data: [],
      title: "订单状态",
      field: "OrderStatus",
      type: "select",
    },
  ],
]);
</script>

<style lang="less" scoped>
.table-item-header {
  display: flex;
  padding: 6px 0;
  .table-item-buttons {
    margin-left: auto;
  }
}
.small-text {
  font-size: 12px;
  color: #2196f3;
  padding-top: 10px;
  position: relative;
  top: 2px;
}
</style>
