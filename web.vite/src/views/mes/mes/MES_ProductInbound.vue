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
  >
    <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
    <template #gridHeader> </template>
  </view-grid>
</template>
<script setup lang="jsx">
import extend from '@/extension/mes/mes/MES_ProductInbound.jsx'
import viewOptions from './MES_ProductInbound/options.js'
import { ref, reactive, getCurrentInstance, watch, onMounted } from 'vue'
const grid = ref(null)
const { proxy } = getCurrentInstance()
//http请求，proxy.http.post/get
const {
  table,
  editFormFields,
  editFormOptions,
  searchFormFields,
  searchFormOptions,
  columns,
  detail,
  details
} = reactive(viewOptions())
columns.forEach((x) => {
  if (x.field == 'InboundQuantity') {
    x.summary = true
  }
})
let gridRef //对应[表.jsx]文件中this.使用方式一样
//生成对象属性初始化
const onInit = async ($vm) => {
  gridRef = $vm
  gridRef.setFixedSearchForm(true)
  editFormOptions.forEach((x) => {
    x.forEach((item) => {
      if (item.field == 'MaterialCode' || item.field == 'SpecificationModel') {
        item.readonly = true
      } else if (item.field == 'MaterialName') {
        item.type = 'selectTable'
        item.url = 'api/MES_Material/getPageData'
        item.columns = [
          { field: 'MaterialName', title: '物料名称', width: 90, search: true },
          { field: 'MaterialCode', title: '物料编码', width: 120, search: true },
          {
            field: 'CatalogID',
            title: '物料分类',
            bind: { key: '物料分类', data: [] },
            width: 80
          },
          { field: 'Specification', title: '规格型号', width: 110 },
          {
            field: 'Unit',
            title: '单位',
            bind: { key: '物料单位', data: [] },
            width: 70
          },
          { field: 'Remarks', title: '备注信息', width: 150 }
        ]
        item.selectWidth = 760
        item.height = 280
        item.onSelect = (rows) => {
          //给表单字段设置值
          editFormFields.MaterialName = rows[0].MaterialName
          editFormFields.MaterialCode = rows[0].MaterialCode
          editFormFields.SpecificationModel = rows[0].Specification
        }
        item.loadBefore = (param, callback) => {
            //表单没有输入查询的走表格输入框的查询条件
          const b = param.wheres.some((c) => {
            return c.name == 'MaterialName'
          })
          if (!b) {
            param.wheres.push({
              name: 'MaterialName',
              value: editFormFields.MaterialName,
              displayType: 'like'
            })
          }
          callback(true)
        }
        item.loadAfter = (rows, callback, result) => {
          callback(true)
        }
      }
    })
  })
}
//生成对象属性初始化后,操作明细表配置用到
const onInited = async () => {}
const searchBefore = async (param) => {
  //界面查询前,可以给param.wheres添加查询参数
  //返回false，则不会执行查询
  return true
}
const searchAfter = async (rows, result) => {
  return true
}
const addBefore = async (formData) => {
  //新建保存前formData为对象，包括明细表，可以给给表单设置值，自己输出看formData的值
  return true
}
const updateBefore = async (formData) => {
  //编辑保存前formData为对象，包括明细表、删除行的Id
  return true
}
const rowClick = ({ row, column, event }) => {
  //查询界面点击行事件
  // grid.value.toggleRowSelection(row); //单击行时选中当前行;
}
const modelOpenBefore = async (row) => {
  //弹出框打开后方法
  return true //返回false，不会打开弹出框
}
const modelOpenAfter = (row) => {
  //弹出框打开后方法,设置表单默认值,按钮操作等
}
//监听表单输入，做实时计算
//watch(() => editFormFields.字段,(newValue, oldValue) => {	})
//对外暴露数据
defineExpose({})
</script>
