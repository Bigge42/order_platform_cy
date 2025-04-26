<template>
  <vol-edit
    ref="edit"
    :keyField="options.key"
    :tableName="options.tableName"
    :tableCNName="options.tableCNName"
    :labelWidth="100"
    :formFields="options.editFormFields"
    :formOptions="options.editFormOptions"
    :detail="options.detail"
    @initButtons="initButtons"
    @initDetailButtons="initDetailButtons"
    :loadFormAfter="loadFormAfter"
  >
    <template #header>
      <el-alert
        title="header位置,生成的新页面编辑同样支持自定义组件与业务处理"
        type="success"
      />
    </template>
    <!-- 
    <template #content>
      <el-alert title="slot数据槽content位置" type="warning" />
    </template>

    <template #footer>
      <el-alert title="slot数据槽footer位置" type="error" />
    </template> -->
  </vol-edit>
</template>
<script>
import { defineComponent, ref, reactive, getCurrentInstance } from "vue";
import { useRouter, useRoute } from "vue-router";
import editOptions from "./options.js";
export default defineComponent({
  setup(props, context) {
    const edit = ref(null);

    const options = reactive({
      //表单字段
      editFormFields: {},
      //表单配置
      editFormOptions: [],
      //明细表信息
      detail: {
        cnName: "",
        table: "",
        url: "",
        columns: [],
        sortName: "",
        key: "",
      },
      details: [],
    });
    Object.assign(options, editOptions());

    //表单按钮
    const formButtons = reactive([]);
    //初始化主表按钮
    const initButtons = (buttons) => {
      //设置表单按钮隐藏
      //buttons[0].hidden=true;
      //记录表单按钮
      formButtons.push(...buttons);
    };
    //初始化明细表按钮
    const initDetailButtons = (buttons) => {};

    //表单数据添加载方法
    const loadFormAfter = (result) => {
      //显示表单按钮(判断表单的值)
      // if (options.formFields.字段==='xxx') {
      //   // formButtons[0].hidden=false;
      // }
    };

    return { edit, options, initButtons, initDetailButtons, loadFormAfter };
  },
  /****************************************/
  //这里是vue2语法,不想写vue3语法就在这里写vue2
  data() {
    return {};
  },
  methods: {},
  created() {
    //this.options
  },
});
</script>
