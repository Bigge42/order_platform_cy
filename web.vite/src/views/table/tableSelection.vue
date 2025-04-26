<template>
  <div class="table-item">
    <div class="table-item-header">
      <div class="table-item-border"></div>
      <span class="table-item-text">分页/刷新数据保留复选框选择状态,复选框选中事件</span>
      <div class="small-text"></div>
      <div class="table-item-buttons">
        <div>
          <el-button type="primary" @click="getSelected" plain
            >获取所有分页选中行</el-button
          >
          <el-button type="primary" @click="clearSelection" plain
            >清除所有分页选中行</el-button
          >
          <el-button type="primary" @click="reload" color="#95d475" plain>刷新</el-button>
        </div>
      </div>
    </div>
    <div
      style="line-height: 12px; font-size: 13px; padding: 0px 0 10px 10px; color: #0f92fb"
    >
      解决问题/功能:多页或刷新表格数据后是否保留复选框选择状态,复选框选中事件(多页选择数据，最终显示获取所有分页选中的数据)
    </div>

    <!-- 
       1、必须设置row-key与reserveSelection属性后才能分页选择数据
       2、row-key为表格数据的唯一值字段(尽量是主键id字段)
       3、reserveSelection是否显示分页选中的数据
     -->
    <vol-table
      row-key="Id"
      :reserveSelection="true"
      ref="table"
      :url="url"
      index
      :columns="columns"
      :height="240"
      :pagination-hide="false"
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
      //接口返回数据，可以框架生成的接口getPageData
      //如果是自定义的接口，需要返回的数据格式：{total:100,rows:[]}
      url: "api/sys_log/getPageData",
      columns: [
        {
          field: "Id",
          title: "Id",
          type: "int",
          width: 90,
          hidden: true,
          readonly: true,
          require: true,
          align: "left",
        },
        {
          field: "BeginDate",
          title: "开始时间",
          type: "datetime",
          width: 120,
          align: "left",
          sortable: true,
        },
        {
          field: "UserName",
          title: "用户名称",
          type: "string",
          width: 90,
          align: "left",
        },
        {
          field: "Url",
          title: "请求地址",
          type: "string",
          width: 150,
          align: "left",
          showOverflowTooltip: true, //设置超出鼠标放上去提示
        },
        {
          field: "Success",
          title: "响应状态",
          type: "int",
          bind: { key: "restatus", data: [] },
          width: 80,
          align: "left",
        },
        { field: "ElapsedTime", title: "时长", type: "int", width: 60, align: "left" },
        {
          field: "RequestParameter",
          title: "请求参数",
          type: "string",
          width: 150,
          align: "left",
        },
        {
          field: "ExceptionInfo",
          title: "异常信息",
          type: "string",
          width: 70,
          align: "left",
        },
        { field: "UserIP", title: "用户IP", type: "string", width: 90, align: "left" },
      ],
    };
  },
  methods: {
    getSelected() {
      const rows = this.$refs.table.getSelected();
      if (!rows.length) {
        this.$message.error("请选中行");
        return;
      }
      this.$message.success(`共选中【${rows.length}】行`);
    },
    clearSelection() {
      this.$refs.table.clearSelection();
      this.$message.success("清除成功");
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
