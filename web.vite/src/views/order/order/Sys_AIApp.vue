<template>
  <view-grid
    ref="grid"
    :columns="columns"
    :detail="detail"
    :details="details"
    :editFormFields="editFormFields"
    :editFormOptions="editFormOptions"
    :searchFormFields="searchFormFields"
    :searchFormOptions="searchFormOptions"
    :table="table"
    :extend="extend"
    :onInit="onInit"
    :onInited="onInited"
    :searchBefore="searchBefore"
    :searchAfter="searchAfter"
    :addBefore="addBefore"
    :updateBefore="updateBefore"
    :rowClick="rowClick"
    :modelOpenBefore="modelOpenBefore"
    :modelOpenAfter="modelOpenAfter"
  />
</template>

<script setup lang="jsx">
import extend from "@/extension/order/order/Sys_AIApp.jsx";
import viewOptions from "./Sys_AIApp/options.js";
import { reactive, ref } from "vue";

const grid = ref(null);
const {
  table,
  editFormFields,
  editFormOptions,
  searchFormFields,
  searchFormOptions,
  columns,
  detail,
  details,
} = reactive(viewOptions());

let gridRef;
const onInit = async ($vm) => {
  gridRef = $vm;
};
const onInited = async () => {};
const searchBefore = async () => true;
const searchAfter = async () => true;
const addBefore = async () => true;
const updateBefore = async () => true;
const rowClick = () => {};
const modelOpenBefore = async () => true;
const modelOpenAfter = () => {};

defineExpose({});
</script>
