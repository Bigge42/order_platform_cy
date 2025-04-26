<template>
  <draggable
    v-model="fieldOptions2"
    @end="end"
    class="left-draggable-item"
    :group="{ name: 'componentsGroup', pull: 'clone', put: false }"
    animation="300"
    @start="onStart"
    :sort="false"
    :move="onMove"
  >
    <transition-group>
      <div
        :class="item.field == 1 ? 'item forbid' : 'item'"
        v-for="item in fieldOptions2"
        :key="item.field"
      >
        <i :class="item.icon"></i> {{ $ts(item.title) }}
      </div>
    </transition-group>
  </draggable>
</template>
<script setup lang="jsx">
import { VueDraggableNext as draggable } from "vue-draggable-next";
import { ref, reactive } from "vue";

const props = defineProps({
  fieldOptions: {
    type: Array,
    default: () => {
      return [];
    },
  },
  checkEnd: {
    type: Function,
    default: (field) => {
      return true;
    },
  },
});

const emit = defineEmits(["end"]);

const fieldOptions2 = props.fieldOptions;

// const fieldOptions = reactive([
//   {
//     id: 1,
//     title: "输入框",
//     type: "text",
//     value: "",
//     icon: "el-icon-document",
//   },
//   {
//     id: 2,
//     title: "textarea",
//     type: "textarea",
//     value: "",
//     icon: "el-icon-document-copy",
//   },
//   {
//     id: 3,
//     title: "日期",
//     type: "date",
//     icon: "el-icon-date",
//     value: null,
//   },
//   {
//     id: 4,
//     title: "switch",
//     type: "switch",
//     icon: "el-icon-turn-off",
//     value: 0,
//   },
// ]);

//左边往右边拖动时的事件
const end = (e) => {
  if (1 == 1 && e.from !== e.to) {
    let obj = Object.assign({}, fieldOptions2[e.oldIndex]);
    obj.field = new Date().valueOf();
    obj.width = 25;

    obj.readonly = false;
    obj.required = false;
    emit("end", obj);
    // this.currentComponents.splice(e.newIndex, 1, obj);
    // this.userComponents.splice(0);
    // this.userComponents.push(...this.currentComponents);
    // this.colWidth = 100;
    // this.currentIndex = e.newIndex; //this.currentComponents.length - 1;
    // this.currentItem = this.currentComponents[this.currentIndex];
  }
};
const onMove = (e, originalEvent) => {
  // this.moveId = e.relatedContext.element.id;
  return props.checkEnd(fieldOptions2[e.draggedContext.index].field);
};
const drag = ref(null);
const onStart = (e, e1) => {
  return false;
};
</script>
<style lang="less" scoped>
@import "./fields.less";
</style>
