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
    @initButtons="initButtons"
    @initDetailButtons="initDetailButtons"
    @initDetailColumns="initDetailColumns"
    @initSubDetailColumns="initSubDetailColumns"
    :details="details"
  >
    <template #header>
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
export default { name: 'Demo_Product_edit' }
</script>
<script setup lang="jsx">

import editOptions from './options.js'
import {
  ref,
  reactive,
  getCurrentInstance,
  
  
  
  nextTick
} from 'vue'
import { useRouter, useRoute } from 'vue-router'
const emit = defineEmits([])
const props = defineProps({})

//发起请求proxy.http.get/post
//消息提示proxy.$message.success()
const { proxy } = getCurrentInstance()

//这里表单与明细表参数，具体信息看options.js里面
const { key, tableName, tableCNName, editFormFields, editFormOptions, detail, details } = reactive(
  editOptions()
)

//获取路由参数
const route = useRoute()
//是否新建操作
let isAdd = !!route.query.id

//vol-edit组件对象
const edit = ref(null)

//表单标签文字显示宽度
const labelWidth = 90

//明细表高度
const detailHeight = 150

//初始化表单按钮
const initButtons = (buttons) => {
  //可以通过this.buttons判断有没有某些按钮权限，如果有再添加自定义按钮
  // if (buttons.some((x) => {return x.value === 'Add'})) {
  //splice在第三个按钮后面添加一个按钮,也可以使用buttons.push({})添加按钮
  buttons.splice(
    3,
    0,
    ...[
      {
        name: '测试按钮', //按钮名称
        icon: 'el-icon-edit-outline', 
        type: 'success', //type类型见:https://element-plus.org/zh-CN/component/button.html
        plain: true,
        onClick: () => {
          proxy.$message.success('点击了按钮')
        }
      },
      {
        name: '获取表单', //按钮名称
        icon: 'el-icon-edit-outline',
        plain: true,
        color: '#626aef',
        onClick: () => {
          proxy.$message.success(JSON.stringify(editFormFields))
        }
      },
      {
        name: '获取明细表数据', //按钮名称
        icon: 'el-icon-check',
        type: 'primary',
        plain: true,
        onClick: () => {
          const rowData = edit.value.getTable('Demo_ProductColor').rowData
          proxy.$message.success(`共[${rowData.length}]行数据`)
        }
      },
      {
        name: '获取明细表选中行',
        icon: 'el-icon-edit-outline',
        type: 'success',
        plain: true,
        onClick: () => {
          const rowData = edit.value.getTable('Demo_ProductColor').getSelected()
          proxy.$message.success(`选中了[${rowData.length}]行数据`)
        }
      }
    ]
  )
}

//初始化主从表按钮(注意不是一对多，只是主从表，一对多的看下面)
const initDetailButtons = (detailButtons) => {
  // 配置同上,detailButtons.splice()
}

//初始化一对多二级表格与按钮
const initDetailColumns = (detailColumns) => {
  detailColumns[0].buttons.push({
    name: '按钮', //按钮名称
    icon: 'el-icon-edit-outline',
    type: 'primary',
    plain: true,
    onClick: () => {
      proxy.$message.success('点击了二级明细按钮')
    }
  })

  //初始化表格按钮
  detailColumns[0].columns.forEach((col) => {
    if (col.field == 'Remark') {
      col.edit = null
      col.render = (h, { row, column, index }) => {
        return (
          <div>
            <el-button
              link
              onClick={($e) => {
                proxy.$message.success('点击了图标1')
              }}
              class="el-icon-search"
              style="color: #2196F3;cursor: pointer;"
            ></el-button>
            <el-button
              link
              onClick={($e) => {
                proxy.$message.success('点击了图标2')
              }}
              class="el-icon-date"
              style="margin-left:1px;color: #2196F3;cursor: pointer;"
            ></el-button>
            <span style="margin-left:5px">{row.Remark}</span>
          </div>
        )
      }
    }
  })

  detailColumns[0].columns.push({
    title: '操作',
    field: '操作',
    width: 80,
    align: 'center', // 'center',
    render: (h, { row, column, index }) => {
      return (
        <div>
          <el-button
            onClick={($e) => {
              proxy.$message.success('点击了表格按钮')
            }}
            type="primary"
            plain
            style="height:26px; padding: 10px !important;"
          >
            查看
          </el-button>
        </div>
      )
    }
  })
}

//初始化一对多三级表格与按钮
const initSubDetailColumns = (subDetailColumns) => {
  //操作同上initDetailColumns
}

defineExpose({edit})
</script>
