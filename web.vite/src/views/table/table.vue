<template>
  <div class="container demo-tabs">
    <div class="content">
      <el-tabs v-model="activeName" @tab-click="handleClick">
        <el-tab-pane v-for="(item, index) in tabs" :name="item.name" :key="index">
          <template #label>
            <div class="custom-tabs-label">
              <span>{{ item.name }}</span>
            </div>
          </template>
        </el-tab-pane>
      </el-tabs>
      <component :is="current"></component>
    </div>
  </div>
</template>
<script lang="jsx">
import { ref, reactive, shallowRef, defineAsyncComponent, watch } from "vue";
import { useRoute } from "vue-router";
import tableInfo from "./tableInfo.vue";
import tableEdit from "./tableEdit.vue";
import tableRender from "./tableRender.vue";
import tableMultipleHeader from "./tableMultipleHeader.vue";
import tableSearch from "./tableSearch.vue";
import tableTree from "./tableTree.vue";
import tableApi from "./tableApi.vue";
import tableFormat from "./tableFormat.vue";
export default {
  components: {
    tableInfo,
    tableApi,
    tableEdit,
    tableRender,
    tableMultipleHeader,
    tableSearch,
    tableTree,
    tableFormat,
  },
  setup(props) {
    const tabs = reactive([
      { name: "基础表格+事件处理", com: "tableInfo" },
      { name: "绑定接口+跨页选择", com: "tableApi" },
      { name: "表格编辑", com: "tableEdit" },
      { name: "自定义渲染+按钮", com: "tableRender" },
      { name: "多级表头", com: "tableMultipleHeader" },
      { name: "树形结构", com: "tableTree" },
      { name: "表单+查询+合并单元格", com: "tableSearch" },
      { name: "格式化+背景颜色+提示", com: "tableFormat" },
    ]);
    const activeName = ref("基础表格+事件处理");
    const current = ref("tableInfo");
    const handleClick = (item) => {
      current.value = tabs[item.index].com;
    };
    const route = useRoute();

    watch(
      () => route.query.index,
      (newId, oldId) => {
        if (route.name == "table") {
          current.value = tabs[newId] ? tabs[newId].com : tabs[0].com;
          activeName.value = tabs[newId] ? tabs[newId].name : tabs[0].name;
        }
      }
    );
    return {
      activeName,
      current,
      tabs,
      handleClick,
    };
  },
};
</script>
<style lang="less" scoped>
.container {
  height: calc(100vh - 95px);
  padding: 10px;
  background: #f3f7fc99;

  .content {
    padding: 10px;
    padding-top: 0;
    background: #fff;
  }

  .table-item {
    // margin-bottom: 10px;
    background: #fff;
  }
}
.demo-tabs ::v-deep(.el-tabs__nav-wrap:after),
.demo-tabs ::v-deep(.el-tabs__active-bar) {
  height: 1px;
}
.demo-tabs ::v-deep(.el-tabs__header) {
  margin: 0px;
}
.demo-tabs ::v-deep(.el-tabs__item) {
  padding: 0 16px !important;
}
</style>
