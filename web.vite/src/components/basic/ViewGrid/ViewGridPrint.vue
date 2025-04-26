<template>
  <vol-box
    :footer="false"
    v-model="model"
    :height="height"
    :width="width"
    :padding="0"
    :lazy="true"
    :onModelClose="close"
    title="选择打印模板"
  >
    <slot></slot>
    <div class="print-model">
      <div
        class="print-name-item"
        @click="confirm(item)"
        v-for="(item, index) in templateArr"
        :key="index"
      >
        <a> {{ item.name }}</a>
      </div>
    </div>
    <template #footer>
      <div class="btns">
        <el-button type="default" plain size="small" @click="close">{{
          $ts("关闭")
        }}</el-button>
        <!-- <el-button type="primary" plain size="small" @click="$Message.error('点击确认')">确认</el-button> -->
      </div>
    </template>
  </vol-box>
</template>
<script setup>
import VolBox from "@/components/basic/VolBox.vue";

import { ref, reactive, getCurrentInstance } from "vue";

const emit = defineEmits(["printModelClose"]);
const height = ref(document.body.cl);
const width = ref(700);
const model = ref(false);

const { proxy } = getCurrentInstance();

let _ids, _table, _rows, _options;
const open = ({ ids, table, rows, options }) => {
  _ids = ids;
  _table = table;
  _rows = rows;
  _options = options;
  if (_options && typeof _options != "string") {
    _options = JSON.stringify(_options);
  }
  getPrintTemplateName();
  // _rows=rows;
};

const templateArr = reactive([]);
const getPrintTemplateName = () => {
  templateArr.length = 0;
  proxy.http
    .get("api/Sys_PrintOptions/getPrintTemplateName?table=" + _table)
    .then((result) => {
      if (result.length == 1) {
        confirm(result[0]);
        return;
      }
      model.value = true;
      templateArr.push(...result);
    });
};

const confirm = (item) => {
  let url =
    `${location.origin}/#/print?templateId=${item.id}&ids=${_ids.join(
      ","
    )}&table=${_table}&view=1&options=` + (_options || "");
  window.open(url, "_blank");
  //this.model=false;
};

const close = () => {
  model.value = false;
  emit("printModelClose");
};

defineExpose({
  open,
});
</script>

<style lang="less" scoped>
.print-model {
  min-height: 250px;
  max-height: calc(100vh - 120px);
  display: inline-block;
  padding: 10px;
  width: 100%;

  .print-name-item {
    float: left;
    width: 25%;
    font-size: 14px;
    color: #838383;
    padding: 5px;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
    -o-text-overflow: ellipsis;
  }

  .print-name-item:hover {
    cursor: pointer;
    color: #136aff;
  }
}

.btns {
  text-align: center;
}
</style>
