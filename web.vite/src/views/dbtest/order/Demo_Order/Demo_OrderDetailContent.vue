<template>
  <div class="demo-content">
    <span>这里可以使用slot自定义内容</span>
    <div>
      <label style="width: 60px">下拉框：</label>
      <el-select style="width: 100px" v-model="selectValue" @change="onChange">
        <el-option value="1" label="选项一"></el-option>
        <el-option value="2" label="选项二"></el-option>
      </el-select>
    </div>
    <div>
      <label style="width: 60px; margin-left: 10px">扫描框：</label>
      <el-input
        style="width: 100px"
        v-model="inputValue"
        placeholder="回车监听"
        @change="inputChange"
      ></el-input>
    </div>
  </div>
</template>
<script setup lang="jsx">
import {
  ref,
  reactive,
  getCurrentInstance,
  
  
  
} from "vue";
const props = defineProps({});
const emit = defineEmits(["detailContentOnChange"]);
const { proxy } = getCurrentInstance();

const selectValue = ref("");
const inputValue = ref();

//下拉框选择时与生成的vue文件通讯
const onChange = (val) => {
  emit("detailContentOnChange", selectValue.value);
};

const inputChange = () => {
  proxy.$message.success("按了回车");
};

defineExpose({}); //向外暴露方法
</script>
<style scoped lang="less">
.demo-content {
  display: flex;
  justify-content: end;
  align-items: baseline;

  > span {
    font-size: 13px;
    margin-right: 30px;
  }
}
</style>
