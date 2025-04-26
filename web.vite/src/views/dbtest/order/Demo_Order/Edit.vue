<template>
  <vol-edit
    ref="edit"
    :keyField="key"
    :tableName="tableName"
    :tableCNName="tableCNName"
    :labelWidth="labelWidth"
    :formFields="editFormFields"
    :formOptions="editFormOptions"
    :detail-height="detailHeight"
    :detail="detail"
    :details="details"
  >
    <template #header>
      <el-button @click="test"> 刷新合计</el-button>
      <!-- 自定义数据槽显示 -->
    </template>
    <template #content>
      <!-- 自定义数据槽显示 -->
    </template>
    <template #foooter>
      <!-- 自定义数据槽显示 -->
    </template>
  </vol-edit>
</template>
<script lang="jsx">
export default { name: "#Demo_Order_edit" };
</script>
<script setup lang="jsx">
import editOptions from "./options.js";
import {
  ref,
  reactive,
  getCurrentInstance,
  
  
  
  nextTick,
} from "vue";
import { useRouter, useRoute } from "vue-router";
const emit = defineEmits([]);
const props = defineProps({});

//发起请求proxy.http.get/post
//消息提示proxy.$message.success()
const { proxy } = getCurrentInstance();

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

detail.columns.forEach((x) => {
  if (x.field == "Qty") {
    x.summary = true;
  }
  if (x.field == "Price") {
    x.summary = true;
    // x.summaryFormatter = (qtyValue, column, rows, summaryArrData) => {
    //   //明细表输入或者值变化后给表单字段设置值
    //   //
    //   //表单字段如果需要输入，参照上面  编辑表单只读与默认值 示例

    //   //这里的return qtyValue一定要写上,自定义返回格式,return qtyValue+'件'
    //   return qtyValue + "件";
    // };
  }
});

//获取路由参数
const route = useRoute();
//是否新建操作
let isAdd = !route.query.id;

//vol-edit组件对象
const edit = ref(null);

//表单标签文字显示宽度
const labelWidth = 90;

//明细表高度
const detailHeight = 240;

const test = () => {
  edit.value.getTable().updateSummary("Price");
};

defineExpose({});
</script>
