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

  <!-- 人员选择器 -->
  <PersonSelectorMulti
    v-model="personSelectorVisible"
    :selectedPersonIds="selectedPersonIds"
    :selectedPersonNames="selectedPersonNames"
    @confirm="handlePersonConfirm"
    @cancel="handlePersonCancel"
  />
</template>
<script setup lang="jsx">
import extend from '@/extension/order/ordercollaboration/OCP_AlertRules.jsx'
import viewOptions from './OCP_AlertRules/options.js'
import PersonSelectorMulti from '@/comp/person-selector-multi/index.vue'
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

let gridRef //对应[表.jsx]文件中this.使用方式一样

// 当前编辑行数据
const currentEditRow = ref(null)

// 人员选择器相关
const personSelectorVisible = ref(false)
const selectedPersonIds = ref('')
const selectedPersonNames = ref('')

const getPermission = () => {
  const permission = {
    pause: false,
    enable: false,
    Execute: false,
    Log: false
  }

  const keys = Object.keys(permission)

  const permissions = proxy.$store.getters.getPermission()

  keys.forEach((key) => {
    permission[key] = permissions.find((x) => x.id === 370)?.permission?.includes(key)
  })

  return permission
}

//生成对象属性初始化
const onInit = async ($vm) => {
  gridRef = $vm
  //gridRef.setFixedSearchForm(true);
  //与jsx中的this.xx使用一样，只需将this.xx改为gridRef.xx

  const personOption = gridRef.getFormOption('ResponsiblePersonName')
  personOption.extra = {
    render: (h, {}) => {
      return (
        <div>
          <el-button
            type="primary"
            link
            onClick={() => {
              openPersonSelector()
            }}
          >
            <i class="el-icon-search">选择责任人</i>
          </el-button>
        </div>
      )
    }
  }

  const PushIntervalOption = gridRef.getFormOption('PushInterval')
  PushIntervalOption.extra = {
    render: (h, {}) => {
      return (
        <div>
          <el-button
            type="primary"
            link
            onClick={() => {
              window.open('https://cron.qqe2.com/', '_blank')
            }}
          >
            <i class="el-icon-search">查看</i>
          </el-button>
        </div>
      )
    }
  }

  // 预警页面字段联动配置
  const alertPageOption = gridRef.getFormOption('AlertPage')
  alertPageOption.onChange = async (val, option) => {
    const fieldNameOption = gridRef.getFormOption('FieldName')
    const finishStatusFieldOption = gridRef.getFormOption('FinishStatusField')

    if (val) {
      try {
        // 调用接口获取表字段
        const response = await proxy.http.get(
          `/api/Sys_TableFieldDefinition/GetFieldsByTableName?tableName=${val}`,
          {},
          true
        )

        if (response.status && response.data) {
          const fieldData = response.data

          // 更新字段名下拉框数据源
          fieldNameOption.data = fieldData

          // 更新完成状态字段下拉框数据源
          finishStatusFieldOption.data = fieldData

          // 清空之前选中的值
          editFormFields.FieldName = []
          editFormFields.FinishStatusField = ''

          console.log('字段数据加载成功:', fieldData)
        } else {
          // 清空数据源和选中值
          fieldNameOption.data = []
          finishStatusFieldOption.data = []

          editFormFields.FieldName = []
          editFormFields.FinishStatusField = ''
        }
      } catch (error) {
        // 清空数据源和选中值
        fieldNameOption.data = []
        finishStatusFieldOption.data = []

        editFormFields.FieldName = []
        editFormFields.FinishStatusField = ''
        console.error('获取字段数据失败:', error)
        proxy.$message.error('获取字段数据失败')
      }
    } else {
      // 清空数据源和选中值
      fieldNameOption.data = []
      finishStatusFieldOption.data = []

      editFormFields.FieldName = []
      editFormFields.FinishStatusField = ''
    }
  }

  const permissionMap = getPermission()

  columns.push({
    field: '操作',
    title: '操作',
    width: 280,
    align: 'center',
    fixed: 'right',
    render: (h, { row, column, index }) => {
      // 根据当前状态决定显示哪个按钮
      // TaskStatus: 0=暂停, 1=启用
      const isEnabled = row.TaskStatus === 1

      return (
        <div>
          {/* 当状态为暂停时,显示启用按钮 */}
          {!isEnabled && permissionMap.enable && (
            <el-button
              type="primary"
              link
              style={{ padding: '2px 4px', fontSize: '12px' }}
              onClick={() => handleTaskStatus(row, 1)}
            >
              启用
            </el-button>
          )}
          {/* 当状态为启用时,显示暂停按钮 */}
          {isEnabled && permissionMap.pause && (
            <el-button
              type="warning"
              link
              style={{ padding: '2px 4px', fontSize: '12px' }}
              onClick={() => handleTaskStatus(row, 0)}
            >
              暂停
            </el-button>
          )}
          {permissionMap.Execute && (
            <el-button
              type="success"
              link
              style={{ padding: '2px 4px', fontSize: '12px' }}
              onClick={() => handleExecuteRule(row)}
            >
              执行一次
            </el-button>
          )}
          {permissionMap.Log && (
            <el-button
              type="info"
              link
              style={{ padding: '2px 4px', fontSize: '12px' }}
              onClick={() => handleViewLog(row)}
            >
              日志
            </el-button>
          )}
        </div>
      )
    }
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
  // 初始化字段名和完成状态字段的数据源为空
  const fieldNameOption = gridRef.getFormOption('FieldName')
  fieldNameOption.data = []

  const finishStatusFieldOption = gridRef.getFormOption('FinishStatusField')
  finishStatusFieldOption.data = []
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
  currentEditRow.value = row
  //弹出框打开后方法
  return true //返回false，不会打开弹出框
}
const modelOpenAfter = (row) => {
  //弹出框打开后方法,设置表单默认值,按钮操作等
}

// 人员选择相关方法
const openPersonSelector = () => {
  // 从当前编辑行获取已选中的人员信息
  selectedPersonIds.value = currentEditRow.value?.ResponsiblePersonLoginName || ''
  selectedPersonNames.value = currentEditRow.value?.ResponsiblePersonName || ''
  personSelectorVisible.value = true
}

const handlePersonConfirm = (selectedData) => {
  console.log('接收到选中的人员数据:', selectedData)

  // 将选中人员信息赋值给表单字段
  if (selectedData) {
    editFormFields.ResponsiblePersonLoginName = selectedData.ids || ''
    editFormFields.ResponsiblePersonName = selectedData.names || ''
    currentEditRow.value = {
      ...currentEditRow.value,
      ResponsiblePersonLoginName: selectedData.ids,
      ResponsiblePersonName: selectedData.names
    }
    console.log('已赋值 - 登录名:', editFormFields.ResponsiblePersonLoginName)
    console.log('已赋值 - 真实姓名:', editFormFields.ResponsiblePersonName)
  }

  personSelectorVisible.value = false
}

const handlePersonCancel = () => {
  personSelectorVisible.value = false
}

// 预警规则任务状态操作
const handleTaskStatus = async (row, taskStatus) => {
  try {
    const response = await proxy.http.post(
      `/api/AlertRules/task-status/${row.ID}/${taskStatus}`,
      {},
      true
    )

    if (response.status) {
      // 直接使用后端返回的消息
      proxy.$message.success(response.message || '操作成功')
      // 刷新表格数据
      gridRef.search()
    } else {
      proxy.$message.error(response.message || '操作失败')
    }
  } catch (error) {
    console.error('预警规则状态操作失败:', error)
    proxy.$message.error('网络请求失败，请稍后重试')
  }
}

// 执行预警规则
const handleExecuteRule = async (row) => {
  try {
    const response = await proxy.http.post(`/api/AlertRules/execute/${row.ID}`, {}, true)

    if (response.status) {
      proxy.$message.success(response.message || '执行成功')
    } else {
      proxy.$message.error(response.message || '执行失败')
    }
  } catch (error) {
    console.error('执行预警规则失败:', error)
    proxy.$message.error('网络请求失败，请稍后重试')
  }
}

// 查看日志
const handleViewLog = (row) => {
  // 设置当前预警规则ID到全局数据中，供日志组件使用
  proxy.$store.getters.data().quartzId = row.ID
  // 打开日志弹窗
  grid.value.$refs.gridBody.open()
}

//监听表单输入，做实时计算
//watch(() => editFormFields.字段,(newValue, oldValue) => {	})
//对外暴露数据
defineExpose({})
</script>
