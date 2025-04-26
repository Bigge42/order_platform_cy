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
    :detail-height="148"
    :detail="detail"
    :details="details"
  >
    <template #header>
      <el-alert
        title="新窗口编辑模式：已支持零代码自动构建生成一对多、多对多新窗口模式页面，并支持自定义业务扩展"
        type="warning"
        show-icon
        :closable="false"
      />
    </template>
    <template #content>
      <el-alert
        style="margin-top: 10px; margin-bottom: -10px"
        title="N个明细表：代码生成器可快速构建明细表,并且不限制明细表数量；或许一周的工作量使用框架最多1小时完成"
        type="success"
        show-icon
        :closable="false"
      />
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
  
  
  
  nextTick,
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
        "SetWorkTimes",
        "WorkTimes",
        "SetDeliveryAmount",
        "DeliveryAmount",
      ].includes(x.field)
    ) {
      x.group = "基础信息";
    } else if (["img1", "img2", "img3", "img4", "remark"].includes(x.field)) {
      x.group = "供应商资质";
    } else {
      x.group = "供应商信息";
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

nextTick(() => {
  edit.value.getTable("Pm_ControlDetail").rowData.push(
    ...[
      {
        ItemName: "设备费用",
        CurrCostAmount: 1000,
        PreCostAmount: 800,
        Memo: "设备费用...",
        CreateID: 1,
        Creator: "admin",
        CreateDate: "2022-01-01",
        ModifyID: 2,
        Modifier: "admin",
        ModifyDate: "2022-01-05",
      },
      {
        ItemName: "材料费用",
        CurrCostAmount: 1500,
        PreCostAmount: 1200,
        Memo: "材料费用...",
        CreateID: 3,
        Creator: "admin",
        CreateDate: "2022-01-02",
        ModifyID: 4,
        Modifier: "admin",
        ModifyDate: "2022-01-06",
      },
      {
        ItemName: "工时费用",
        CurrCostAmount: 2000,
        PreCostAmount: 1800,
        Memo: "工时费用...",
        CreateID: 5,
        Creator: "admin",
        CreateDate: "2022-01-03",
        ModifyID: 6,
        Modifier: "admin",
        ModifyDate: "2022-01-07",
      },
    ]
  );
});

const clickTest = () => {
  edit.value.getTable("Pm_ControlDetail").rowData.push({});
};
</script>
