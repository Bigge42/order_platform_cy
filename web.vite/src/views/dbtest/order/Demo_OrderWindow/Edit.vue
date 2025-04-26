<template>
  <vol-edit
    ref="edit"
    labelPosition="left"
    :keyField="key"
    :tableName="tableName"
    :tableCNName="tableCNName"
    :labelWidth="labelWidth"
    :formFields="editFormFields"
    :formOptions="editFormOptions"
    :detail-height="0"
    :detail="detail"
    :details="details"
  >
    <template #header>
      <!-- <div><el-button @click="clickTest">111</el-button></div> -->
    </template>
  </vol-edit>
</template>
<script lang="jsx">
export default { name: "#table_edit" };
</script>
<script setup lang="jsx">

import editOptions from "./options.js";
import {
  defineComponent,
  ref,
  reactive,
  getCurrentInstance,
  
  
  
} from "vue";
import { useRouter, useRoute } from "vue-router";
const { proxy } = getCurrentInstance();
//发起请求proxy.http.get/post
//消息提示proxy.$message.success()

//这里表单与明细表参数，具体信息看options.js里面
const {
  key,
  tableName,
  tableCNName,
  editFormFields,
  editFormOptions,
  detail,
  details,
} = reactive(editOptions());
editFormOptions.forEach((ops) => {
  ops.forEach((x) => {
    if (
      [
        "ProjectCode",
        "ProjectName",
        "FacilityId",
        "FacilityNum",
        "SetPurchaseAmount",
        "PurchaseAmount",
        "SetMaterialAmount",
        "MaterialAmount",
      ].includes(x.field)
    ) {
      x.group = "基础信息";
    } else {
      x.group = "其他信息";
    }
  });
});

const route = useRoute();
//是否新建操作
let isAdd = !!route.query.id;

//vol-edit组件
const edit = ref(null);

//表单标签文字显示宽度
const labelWidth = 90;

const clickTest = () => {};
</script>
