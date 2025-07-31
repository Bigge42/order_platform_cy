<template>
  <div class="attach">
    <div>{{ $ts("上传附件") }}：</div>
    <vol-form
      :labelWidth="90"
      ref="form"
      labelPosition="left"
      :loadKey="true"
      :formFields="formFields"
      :formRules="formRules"
    >
    </vol-form>
  </div>
</template>
<script setup>
import { ref, reactive, onMounted, getCurrentInstance } from "vue";
// 获取当前实例
const { proxy } = getCurrentInstance();
// 定义表单的 ref
const form = ref(null);
// 定义表单字段数据
const formFields = reactive({
  files: [],
});

// 定义表单规则数据
const formRules = reactive([
  [
    {
      title: "",
      field: "files",
      type: "file", //指定上传类型excel/img/file
      multiple: false,
      maxFile: 1,
      maxSize: 200,
      url: "api/sys_workFlow/Upload",
    },
  ],
]);
const show = (attachInfo) => {
  formFields.files = [];
  formRules[0][0].multiple = attachInfo.attachQty > 1;
  formRules[0][0].maxFile = attachInfo.attachQty * 1 || 1;
};

const getFile = () => {
  return formFields.files
    .map((x) => {
      return x.path;
    })
    .join(",");
};

defineExpose({
  show,
  getFile,
});
</script>
<style scoped lang="less">
.attach {
  > div {
    margin: 7px 0;
    color: #606266;
  }
  //   display: flex;
}
</style>
