<template>
  <div class="table-item">
    <div class="table-item-header">
      <div class="table-item-border"></div>
      <vol-title icon="edit" title="树形结构"> 功能：树形结构</vol-title>
      <div class="table-item-buttons">
        <div>
          <el-button type="primary" @click="search" color="#95d475" plain>查询</el-button>
        </div>
      </div>
    </div>
    <!-- 
         lazy：是否懒加载，如果需要显示所有数据不延迟加载，请将lazy设置为false,去掉loadTreeChildren,并且后台返回全部数据
         rowKey：主键字段
         rowParentField：父级id字段
         defaultExpandAll：默认展开所有节点，lazy=false时生效
        expandRowKeys：默认展开的行数据主键id，lazy=false时生效
     -->
    <vol-table
      :lazy="true"
      rowKey="Role_Id"
      rowParentField="ParentId"
      @loadBefore="loadBefore"
      @loadAfter="loadAfter"
      :defaultExpandAll="false"
      :expandRowKeys="[1]"
      :loadTreeChildren="loadTreeChildren"
      ref="tableRef"
      :url="url"
      index
      :columns="columns"
      :height="maxHeight"
      :pagination-hide="false"
      :ck="true"
      :column-index="true"
    ></vol-table>
  </div>
</template>

<script lang="jsx" setup>
import VolTable from "@/components/basic/VolTable.vue";
import { ref, reactive, getCurrentInstance } from "vue";
const { proxy } = getCurrentInstance();
const tableRef = ref();
//接口返回数据，可以框架生成的接口getPageData
//如果是自定义的接口，需要返回的数据格式：{total:100,rows:[]}
const url = ref("api/sys_role/getPageData");
//计算表格高度
const maxHeight = ref(0);
maxHeight.value = document.body.clientHeight - 250;
/***延迟加载子节点数据，见Sys_RoleController.cs文件***/
const loadTreeChildren = (tree, treeNode, resolve) => {
  //加载子节点
  let url = `api/Sys_Role/getTreeTableChildrenData?roleId=${tree.Role_Id}`; //Role_Id换为表的主键字段
  proxy.http.post(url, {}).then((result) => {
    resolve(result.rows);
  });
};

const loadBefore = (params, callBack) => {
  //调用后台接口前处理
  //设置查询条件
  callBack(true); //false不会调用后台接口
};
//查询后方法
const loadAfter = (rows, callBack, result) => {
  callBack(true);
};
const search = () => {
  tableRef.value.load(null, true);
  proxy.$message.success("查询成功");
};

const columns = reactive([
  { field: "RoleName", title: "角色名称", width: 140 },
  { field: "Role_Id", title: "角色ID", width: 70 },
  {
    field: "Enable",
    title: "是否启用",
    bind: { key: "enable", data: [] },
    width: 90,
  },
  { field: "Creator", title: "创建人", width: 130 },
  {
    field: "CreateDate",
    title: "创建时间",
    width: 150,
  },
  {
    field: "Modifier",
    title: "修改人",
    width: 100,
  },
  {
    field: "ModifyDate",
    title: "修改时间",
    width: 180,
  },
]);
</script>
<style lang="less" scoped>
.table-item-header {
  display: flex;
  align-items: center;
  padding: 6px 0;
  .table-item-buttons {
    flex: 1;
    text-align: right;
  }
}
</style>
