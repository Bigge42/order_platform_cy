<template>
  <div class="edit-container">
    <div class="edit-form-header">
      <div class="edit-form-header-table-name">
        <el-icon><Edit /></el-icon>
        <div class="name">
          {{ $ts(tableCNName) }}{{ id ? '(' + $ts('编辑') + ')' : '(' + $ts('新建') + ')' }}
        </div>
         <slot name="title"></slot>
      </div>
      <div class="edit-form-buttons form-buttons">
        <slot name="btn"></slot>
        <template v-for="(btn, index) in buttons" :key="index">
          <el-dropdown v-if="btn.drop" :type="btn.type" :color="btn.color" @click="() => {}">
            <el-button :plain="btn.plain" :type="btn.type" :color="btn.color" size="small">
              {{ $ts(item.name) }}<i class="el-icon-arrow-down el-icon-right"></i>
            </el-button>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item
                  @click="() => {}"
                  :name="item.name"
                  v-show="!item.hidden"
                  v-for="(item, dIndex) in btn.data"
                  :key="dIndex"
                >
                  <i :class="item.icon"></i>
                  {{ $ts(item.name) }}</el-dropdown-item
                >
              </el-dropdown-menu>
            </template>
          </el-dropdown>
          <el-button
            v-else
            :disabled="btn.readonly"
            :plain="btn.plain"
            :type="btn.type"
            :color="btn.color"
            v-show="!btn.hidden"
            @click="btnClick(btn)"
          >
            <i :class="btn.icon"></i>
            {{ $ts(btn.name) }}
          </el-button>
        </template>
      </div>
    </div>
    <el-scrollbar style="flex: 1; height: 0; padding: 12px">
      <slot name="header"></slot>
      <div class="edit-form">
        <!-- -->
        <vol-form
          :load-key="false"
          ref="form"
          :label-position="labelPosition"
          :label-width="labelWidth"
          :formRules="formOptions"
          :formFields="formFields"
          @dicInited="dicInited"
          :select2Count="select2Count"
        ></vol-form>
      </div>

      <slot name="content"></slot>

      <!-- 一对多明细表 -->
      <div class="detail-table">
        <detail-table
          borderCard=""
          ref="detailsRef"
          @loadBefore="loadDetailTableBefore"
          @loadAfter="loadDetailTableAfter"
          @rowChange="detailRowOnChange"
          @rowClick="rowClick"
          @tabsClick="tabsClick"
          v-if="details.length"
          :main-table="tableName"
          :height="detailHeight"
          :data="details"
          @onSortEnd="onSortEnd"
        >
        </detail-table>
      </div>

      <!-- 一对多三级明细表 -->
      <div class="detail-table">
        <detail-table
          borderCard=""
          ref="subDetailsRef"
          @loadBefore="loadSubDetailTableBefore"
          @loadAfter="loadSubDetailTableAfter"
          @rowChange="detailRowOnChange"
          @rowClick="rowClick"
          @tabsClick="tabsClick"
          v-if="subDetails.length"
          :main-table="tableName"
          :height="detailHeight"
          :data="subDetails"
          @onSortEnd="onSortEnd"
        >
        </detail-table>
      </div>

      <!-- 主从表 -->
      <div v-if="detail.columns.length" class="edit-form edit-detail">
        <div class="edit-form-header-title">
          <div class="edit-form-header-table-name">
            <div class="border"></div>
            <div class="name">{{ $ts(detail.cnName) }}</div>
          </div>
          <div class="edit-form-buttons">
            <template v-for="(btn, index) in detailButtons" :key="index">
              <el-dropdown v-if="btn.drop" @click="() => {}">
                <el-button link plain size="small">
                  {{ $ts(item.name) }}<i class="el-icon-arrow-down el-icon-right"></i>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item
                      @click="() => {}"
                      :name="item.name"
                      v-show="!item.hidden"
                      v-for="(item, dIndex) in btn.data"
                      :key="dIndex"
                    >
                      <i :class="item.icon"></i>
                      {{ $ts(item.name) }}</el-dropdown-item
                    >
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
              <el-button v-else link v-show="!btn.hidden" @click="btnClick(btn)">
                <i :class="btn.icon"></i>
                {{ $ts(btn.name) }}
              </el-button>
            </template>
          </div>
        </div>
        <vol-table
          ref="table"
          @loadBefore="loadDetailTableBefore"
          @loadAfter="loadDetailTableAfter"
          @rowClick="rowClick"
          :url="detail.url"
          :load-key="true"
          :index="true"
          :columns="detail.columns"
          :pagination="detail.pagination"
          :height="detailHeight"
          :pagination-hide="!!detail.paginationHide"
          :beginEdit="beginEdit"
          :endEditBefore="endEditBefore"
          @dicInited="
            (dicData) => {
              dicInited(dicData, detail.table)
            }
          "
          :summary="true"
          :column-index="columnIndex"
          :ck="ck"
          :text-inline="textInline"
          @onSortEnd="onSortEnd"
        ></vol-table>
      </div>

      <slot name="footer"></slot>
    </el-scrollbar>

    <ViewGridAudit @auditClick="saveAudit" :option="autitTableOptions" ref="audit"> </ViewGridAudit>
  </div>
</template>
<script>
import { ElMessage, ElMessageBox } from 'element-plus'

import {
  defineComponent,
  ref,
  reactive,
  watch,
  getCurrentInstance,
  defineAsyncComponent
} from 'vue'
import { useRouter, useRoute } from 'vue-router'
import store from '@/store/index'
import http from '@/api/http.js'
import VolForm from '@/components/basic/VolForm.vue'
import VolBox from '@/components/basic/VolBox.vue'
import VolTable from '@/components/basic/VolTable.vue'
//import AuditHis from '@/components/basic/ViewGrid/AuditHis';
import ViewGridDetails from '../ViewGrid/ViewGridDetails.vue'
export default defineComponent({
  name: 'vol-edit',
  components: {
    'vol-form': VolForm,
    'vol-table': VolTable,
    'vol-box': VolBox,
    'detail-table': ViewGridDetails,
    // 'audit-his': AuditHis,
    ViewGridAudit: defineAsyncComponent(() => import('../ViewGrid/ViewGridAudit'))
  },
  props: {
    value: null,
    editChange: {
      //新建数据后，变更为编辑状态
      type: Boolean,
      default: false
    },
    keyField: {
      //主键字段
      type: String,
      default: ''
    },
    tableName: {
      type: String,
      default: ''
    },
    tableCNName: {
      type: String,
      default: '表名'
    },
    labelPosition: {
      //标签显示位置
      type: String,
      default: ''
    },
    labelWidth: {
      type: Number, //表单标签宽度
      default: 100
    },
    formFields: {
      type: Object,
      default: {}
    },
    formOptions: {
      type: Object,
      default: () => {
        return []
      }
    },
    detail: {
      type: Object,
      default: () => {
        return {
          cnName: '',
          table: '',
          columns: [],
          url: '',
          paginationHide: false, //明细表隐藏分页
          pagination: { size: 100, sortName: '' },
          height: 0 //明细表高度
        }
      }
    },
    details: {
      //一对多
      type: Array,
      default: () => {
        return []
      }
    },
    columnIndex: {
      type: Boolean,
      default: false
    },
    ck: {
      //明细表复选框
      type: Boolean,
      default: true
    },
    textInline: {
      //明细表不换行显示
      type: Boolean,
      default: true
    },
    loadFormBefore: {
      //表单加载前
      type: Function,
      default: (params, callback) => {
        callback(true)
      }
    },
    loadFormAfter: {
      //表单加载后
      type: Function,
      default: (result, callback) => {
        callback(true)
      }
    },
    loadTableBefore: {
      //明细表加载前
      type: Function,
      default: (params, callback, table, item) => {
        callback(true)
      }
    },
    loadTableAfter: {
      //明细表加载后
      type: Function,
      default: (params, result, callback, table, item) => {
        callback(true)
      }
    },
    addBefore: {
      //新建前
      type: Function,
      default: (formData, callback) => {
        callback(true)
      }
    },
    addAfter: {
      //新建后
      type: Function,
      default: (formData, callback) => {
        callback(true)
      }
    },
    updateBefore: {
      //更新前
      type: Function,
      default: (formData, callback) => {
        callback(true)
      }
    },
    updateAfter: {
      //更新后
      type: Function,
      default: (params, callback) => {
        callback(true)
      }
    },
    delRow: {
      type: Function,
      default: (rows) => {
        return true
      }
    },
    addRow: {
      //明细表添加行
      type: Function,
      default: () => {
        return {}
      }
    },
    detailHeight: {
      //明细表高度
      type: Number,
      default: 240
    },
    submitChangeRows: {
      //只提交变化的数据
      type: Boolean,
      default: true
    },
    select2Count: {
      //超出数量显示select2组件
      type: Number,
      default: 5000
    }
  },
  emits: [
    'dicInited',
    'beginEdit',
    'endEditBefore',
    'loadFormAfter',
    'initButtons',
    'initDetailButtons',
    'initDetailColumns',
    'initSubDetailColumns',
    'detailRowChange',
    'onSortEnd'
  ],
  setup(props, { emit }) {
    const { appContext, proxy } = getCurrentInstance()
    const router = useRouter()

    const route = useRoute()
    const id = ref(null)
    let isAdd = true
    if (props.value || route.query.id) {
      isAdd = false
      id.value = props.value || route.query.id
    }
    if (!props.detail.height) {
      props.detail.height = 500
    }

    //主表按钮
    const buttons = reactive([])
    //明细表按钮
    const detailButtons = reactive([])

    const detailsRef = ref(null)

    //三级表配置
    const subDetails = ref([])
    //三级表table对象
    const subDetailsRef = ref(null)

    const form = ref(null)

    //明细表refs
    const table = ref(null)
    const getTable = (_tableName) => {
      if (props.details.length) {
        //二级明细表
        if (
          props.details.some((c) => {
            return c.table == _tableName
          })
        ) {
          return detailsRef.value.$refs[_tableName][0]
        }
        //三级明细表
        return subDetailsRef.value.$refs[_tableName][0]
      }
      return table.value
    }

    //刷新主表数据
    const reloadMainTable = () => {
      let editSearch = store.getters.data()[props.tableName + '_edit']
      editSearch && editSearch()
    }

    const convertDetailSubmitData = (detailData, columns) => {
      // formData.detailData = this.$refs.detail.rowData;
      const types = ['selectList', 'cascader', 'treeSelect']
      let _fields = columns
        .filter((c) => {
          return types.indexOf(c.type) != -1 || types.indexOf(c.edit && c.edit.type) != -1
        })
        .map((c) => {
          return c.field
        })
      //2022.06.20增加保存时对明细表下拉框多选的判断
      if (_fields.length) {
        detailData = JSON.parse(JSON.stringify(detailData))
        detailData.forEach((row) => {
          for (let index = 0; index < _fields.length; index++) {
            const _field = _fields[index]
            if (Array.isArray(row[_field])) {
              row[_field] = row[_field].join(',')
            }
          }
        })
      }
      return detailData
    }
    //保存
    const save = () => {
      form.value.validate((result) => {
        if (!result) {
          return
        }
        saveExecute()
      })
    }

    const saveExecute = async () => {
      const editFormFields = {}
      let formFields = props.formFields
      for (const key in formFields) {
        if (Array.isArray(formFields[key])) {
          let u = formFields[key].some((x) => {
            return x.path
          })
          if (u) {
            let allPath = formFields[key].map((x) => {
              return x.path
            })
            editFormFields[key] = allPath.join(',')
          } else if (dicInfo[key] && dicInfo[key].type == 'cascader') {
            editFormFields[key] = formFields[key][formFields[key].length - 1] || null
          } else {
            editFormFields[key] = formFields[key].join(',')
          }
        } else if (typeof formFields[key] == 'function') {
          try {
            editFormFields[key] = formFields[key]()
          } catch (error) {}
        } else {
          editFormFields[key] = formFields[key]
        }
      }
      if (id.value) {
        editFormFields[props.keyField] = id.value
      }
      let formData = {
        mainData: editFormFields,
        detailData: null,
        delKeys: delKeys
      }

      //2024.06.10增加数据版本管理
      if (proxy.$global.dataVersion) {
        formData.dataVersionField = proxy.$global.dataVersion
        if (!isAdd && proxy.$global.dataVersion) {
          formData.dataVersionValue = currentRow[proxy.$global.dataVersion]
        }
      }

      //生成明细表数据d
      if (props.detail.columns.length) {
        formData.detailData = getDetailRows()
      }
      //生成一对多数据
      if (Array.isArray(props.details) && props.details.length) {
        formData.details = proxy.details.map((c) => {
          if (c.columns) {
            let itemDetail = {
              table: c.table,
              delKeys: c.delKeys,
              data: convertDetailSubmitData(getTable(c.table).rowData, c.columns)
            }
            //只提交变更的明细表数据2024.08.30
            if (proxy.submitChangeRows) {
              itemDetail.data = detailsRef.value.getDiffRows(
                c.table,
                c.key,
                itemDetail.data,
                c.detail
              )
            }
            return itemDetail
          }
          return {
            table: c.table,
            delKeys: c.delKeys,
            data: []
          }
        })
      }
      //记录三级明细删除信息
      if (subDetails.value && subDetails.value.length) {
        formData.subDelInfo = subDetails.value.map((x) => {
          return { table: x.table, delKeys: x.delKeys }
        })
      }

      let status = true
      proxy[isAdd ? 'addBefore' : 'updateBefore'](formData, (result) => {
        // status = result;
        if (!result) {
          return
        }
        saveExecting(formData)
      })
      if (!status) return
    }

    const reloadDetail = () => {
      //刷新一对多表数据
      if (props.details.length) {
        props.details.forEach((item) => {
          getDetailData(item)
        })
      } else {
        table.value.load()
      }
    }
    const saveExecting = (formData) => {
      const saveUrl = `api/${props.tableName}/${isAdd ? 'add' : 'update'}`
      http.post(saveUrl, formData, true).then((x) => {
        ElMessage({
          type: x.status ? 'success' : 'error',
          message: x.message
        })
        if (!x.status) {
          return
        }

        reloadMainTable()

        proxy[isAdd ? 'addAfter' : 'updateAfter'](x, (result) => {})
        delKeys.splice(0)
        if (isAdd) {
          //新建后变更为编辑状态
          if (props.editChange) {
            id.value = JSON.parse(x.data).data[props.keyField]
            isAdd = false
            getData()
            reloadDetail()
            return
          }
          if (x.data) {
            form.value.reset()
          }
          //重置明细表数据
          table.value && table.value.reset()
          return
        } else {
          reloadDetail()
        }
      })
    }

    const getDetailRows = () => {
      let detailData = table.value.rowData
      let _fields = props.detail.columns
        .filter((c) => {
          return c.type == 'selectList' || (c.edit && c.edit.type == 'selectList')
        })
        .map((c) => {
          return c.field
        })
      //2022.06.20增加保存时对明细表下拉框多选的判断
      if (_fields.length) {
        detailData = JSON.parse(JSON.stringify(detailData))
        detailData.forEach((row) => {
          for (let index = 0; index < _fields.length; index++) {
            const _field = _fields[index]
            if (Array.isArray(row[_field])) {
              row[_field] = row[_field].join(',')
            }
          }
        })
      }
      return detailData
    }
    //删除
    const delClick = () => {
      if (isAdd) {
        return
      }
      ElMessageBox.confirm('确定要删除此数据吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        center: true,
        type: 'warning'
      }).then(() => {
        const delUrl = `api/${props.tableName}/del`
        http.post(delUrl, [id.value], true).then((x) => {
          ElMessage({
            type: x.status ? 'success' : 'error',
            message: x.message
          })
          if (!x.status) {
            return
          }
          reloadMainTable()
          proxy.$tabs.close(route.path)
        })
      })
    }

    let permission = store.getters.getPermission('/' + props.tableName)
    if (!permission || !permission.permission) {
      permission = store.getters.getPermission(route.path)
    }

    //初始明细表配置
    const initDetailButtons = (_buttons, item) => {
      if (!item) {
        item = {}
      }
      _buttons.push({
        name: '刷新',
        hidden: false,
        icon: 'el-icon-refresh',
        onClick: () => {
          getDetailData(item)
        }
      })

      _buttons.push({
        name: '删除行',
        hidden: false,
        readonly: false,
        icon: 'el-icon-delete',
        onClick: () => {
          delRow(item)
        }
      })

      _buttons.push({
        name: '添加行',
        readonly: false,
        hidden: false,
        icon: 'el-icon-plus',
        onClick: () => {
          detailAddRow(item)
        }
      })
    }

    permission = permission ? permission.permission || [] : []

    const hasBtn = permission.includes('Add') || permission.includes('Update')

    //初始明细表、多级明细表权限按钮
    const initPermissionButtons = () => {
      if (hasBtn) {
        buttons.push({
          name: '保存',
          hidden: false,
          readonly: false,
          icon: 'el-icon-check',
          plain: true,
          value: 'Save',
          type: 'primary',
          // color:"#F56C6C",
          onClick: () => {
            save()
          }
        })
        if (proxy.details.length) {
          proxy.details.forEach((item) => {
            // item.pagination = { total: 0, size: 100, sortName: item.sortName }
            // item.paginationHide = false
            // item.columns.forEach((col) => {
            //   if (col.link) {
            //     col.link = false
            //   }
            // })
            // //设置默认加载明细表数据
            // if (item.load === undefined) {
            //   item.load = true
            // }
            initDetailButtons(item.buttons, item)
          })
        } else {
          initDetailButtons(detailButtons, {})
        }
      } else {
        //没有新建与编辑权限的设置为只读
        props.formOptions.forEach((options) => {
          options.forEach((op) => {
            op.readonly = true
          })
        })
        proxy.details.forEach((item) => {
          if (item.columns) {
            item.columns.forEach((op) => {
              op.readonly = true
            })
          }
        })
        if (proxy.detail.columns) {
          proxy.detail.columns.forEach((op) => {
            op.readonly = true
          })
        }
      }
      if (permission.includes('Audit') && !isAdd) {
        buttons.unshift({
          name: '审核',
          readonly: false,
          hidden: false,
          value: 'Audit',
          icon: 'el-icon-edit-outline',
          onClick: () => {
            auditClick()
          }
        })
      }
      if (id.value) {
        if (permission.includes('Delete')) {
          buttons.unshift({
            name: '删除',
            readonly: false,
            hidden: false,
            plain: true,
            value: 'Delete',
            type: 'danger',
            color: '#F56C6C',
            icon: 'el-icon-delete',
            onClick: () => {
              delClick()
            }
          })
        }
      }
      buttons.unshift({
        name: '关闭',
        readonly: false,
        hidden: false,
        plain: true,
        value: 'Close',
        //  type: 'info',
        //  color: "#fcfffa",
        icon: 'el-icon-switch-button',
        onClick: () => {
          proxy.$tabs.close(route.path)
        }
      })
    }

    //初始化二级明细表配置
    const initDetailColumns = () => {
      if (props.details.length) {
        if (props.detail && Array.isArray(props.detail.columns)) {
          props.detail.columns = []
        }
        props.details.forEach((item) => {
          item.pagination = Object.assign(
            { total: 0, size: 100, sortName: item.sortName },
            item.pagination
          )
          item.paginationHide = false
          item.columns.forEach((col) => {
            if (col.link) {
              col.link = false
            }
          })
          //设置默认加载明细表数据
          if (item.load === undefined) {
            item.load = true
          }

          //三级明细表
          if (item.detail) {
            //固定明细表高度
            // this.detailOptions.height = 200
            item.detail.height = 200
            /// this.detailHeight = 220
            item.single = true
            item.detail.columnIndex = false
            subDetails.value.push(item.detail)
          }
        })
      } else {
        if (!props.detail.pagination) {
          props.detail.pagination = { total: 0, size: 100 } //, sortName: item.sortName }
        }
      }
    }
    //初始化三级明细表配置
    const initSubDetailColumns = () => {
      subDetails.value.forEach((item) => {
        item.paginationHide = false
        item.pagination = { total: 0, size: 100, sortName: '' }
        if (item.load === undefined) {
          item.load = false
        }
        if (hasBtn) {
          item.buttons.push(
            {
              name: '添加行',
              icon: 'el-icon-plus',
              type: 'primary',
              hidden: false,
              plain: true,
              onClick: (table, item, index) => {
                detailAddRow(item, true)
              }
            },
            {
              type: 'danger',
              plain: true,
              name: '删除行',
              hidden: false,
              icon: 'el-icon-delete',
              onClick: (table, item, index) => {
                delRow(item, true)
              }
            }
          )
        } else {
          item.columns.forEach((x) => {
            x.readonly = true
          })
        }
      })
    }

    // buttons.unshift({
    //   name: '刷新',
    //   hidden: false,
    //   icon: 'el-icon-refresh',
    //   click: () => {}
    // });

    //明细表选择加载三级明细表
    const detailRowOnChange = (row, item) => {
      //获取三级明细子表
      if (!item || !item.detail) {
        return
      }
      emit('detailRowChange', row, item)

      let table = item.detail.table
      //三级表对象
      let subRef = getTable(table)
      if (!subRef) {
        return
      }

      //三级明细表数据
      let subRows = row[table]
      if (subRows) {
        subRef.rowData = subRows
        return
      }
      if (!subRows) {
        subRows = []
        row[table] = subRows
      }
      subRef.rowData = subRows
      //处于编辑状态时没有明细表数据，从后台加载数据
      if (!isAdd && !subRows.length) {
        //从接口添加数据
        subRef.load(null, true)
      }
    }

    // const loadInternalDetailTableBefore = (param, callBack, table, item) => {
    //   //加载明细表数据之前,需要设定查询的主表的ID
    //   //每次只要加载明细表格数据就重置删除明细的值
    //   item.delKeys = []
    //   let key = props.keyField
    //   if (this.currentRow && this.currentRow.hasOwnProperty(key)) {
    //     param.value = this.currentRow[key]
    //   }
    //   if (this.isMultiple) {
    //     if (!param.tableName) {
    //       ;(param.value = this.currentRow[this.table.key]), (param.tableName = table)
    //     }
    //   }
    //   return loadDetailTableBefore(param, callBack, table, item)
    // }

    //明细(二)表加载前
    const loadDetailTableBefore = (param, callback, table, item) => {
      //新建不添加数据
      if (!id.value) {
        callback(false)
        return
      }
      param.value = id.value

      if (table) {
        param.tableName = table
      }
      resetDelKeys(item)
      proxy.loadTableBefore(
        param,
        (x) => {
          callback(x)
        },
        table,
        item
      )
    }

    //三级明细表加载前
    const loadSubDetailTableBefore = (param, callback, table, item) => {
      //获取二级明细表配置
      if (item) {
        param.table = item.table
        param.detailTable = item.secondTable
        param.tableName = item.table
        let detailRows = getTable(item.secondTable).getSelected()
        if (!detailRows || detailRows.length == 0) {
          callback(false)
          return
        }
        param.value = detailRows[0][item.secondKey]
      }
      let rows
      if (!param.value) {
        //获取三级表当前选中的明细行数据,并且设置二级明细表的id作为查询条件
        let secondKey = item.secondKey
        //获取二级明细表选中的行
        // rows = this.getTableRef(item.secondTable).getSelected();
        rows = getCurrentDetailSelectRows(item.secondTable)
        if (!rows) {
          callback(false)
          return
        }
        param.value = rows[0][secondKey]
      }

      //三级明细查询前
      //新建时禁止加载明细
      if (isAdd) {
        callback(false)
        return false
      }
      proxy.loadTableBefore(
        param,
        (x) => {
          callback(x)
        },
        table,
        item
      )
    }

    //明细表加载后
    const loadDetailTableAfter = (rows, result, table, item) => {
      proxy.loadTableAfter(rows, result, (x) => {}, table, item)
      return true
    }

    //三级明细表加载后
    const loadSubDetailTableAfter = (rows, result, table, item) => {
      item.keys = []
      //给二级明细表设置值
      let row = getTable(item.secondTable).getSelected()[0]
      row[item.table] = rows
      proxy.loadTableAfter(rows, result, (x) => {}, table, item)
      return true
    }
    //二级表切换时设置三级表选中
    const tabsClick = (table) => {
      let obj = props.details.find((x) => {
        return x.table == table
      })
      //设置三级明细表选中
      if (obj) {
        if (subDetailsRef.value) {
          if (obj.detail) {
            subDetailsRef.value.setTable(obj.detail.table)
          } else {
            subDetailsRef.value.setTable('')
          }
        }
        return
      }
      obj = subDetails.value.find((x) => {
        return x.table == table
      })
      //设置二级明细表选中
      if (obj) {
        detailsRef.value.setTable(obj.secondTable)
      }
    }

    const rowClick = () => {}

    //开始编辑
    const beginEdit = (row, column, index) => {
      emit('beginEdit', row, column, index)
      return true
    }

    //结束编辑前
    const endEditBefore = (row, column, index) => {
      emit('endEditBefore', row, column, index)
      return true
    }

    //是否二级明细表
    const isDetailTable = (table) => {
      if (table) {
        if (
          props.details.some((x) => {
            return x.table == table
          })
        ) {
          return true
        }
      }
      return false
    }

    //获取二级或者三级明细表中文名称
    const getTableName = (table) => {
      //获取明细表
      if (!props.details.length) {
        return '未配置二级或三级明细表'
      }

      let ops = props.details.find((x) => {
        return x.table == table
      })
      if (ops) {
        return ops.cnName
      }

      return subDetails.value.find((x) => {
        return x.table == table
      }).cnName
    }

    //获取二级或者三级明细表中选中的行
    const getCurrentDetailSelectRows = (table) => {
      let rows = getTable(table).getSelected()
      if (!rows.length) {
        proxy.$message.error(
          proxy.$ts('请选中明细表数据') + ':【' + proxy.$ts(getTableName(table)) + '】'
        )
        return null
      }
      return rows
    }
    //添加行
    const detailAddRow = (item, isSub) => {
      //三级表添加行时必须选中二级表数据
      if (isSub && !getCurrentDetailSelectRows(item.secondTable)) {
        return
      }
      if (!item) {
        item = {}
      }
      const tableRef = getTable(item.table)

      let _row = props.addRow(item.table, item)
      //二级明细表
      if (props.details) {
        //二级表添加行，并且有三级明细表,添加三级明细表默认值
        if (!isSub && item.detail && item.detail.table) {
          _row[item.detail.table] = []
        }
      }
      tableRef.rowData.push(_row || {})
    }

    const delKeys = []
    //一、二、三级明细删除操作
    const delRow = (item, isSub) => {
      if (!item) {
        item = {}
      }
      const selectRows = getTable(item.table).getSelected()

      if (!selectRows.length) {
        ElMessage({
          type: 'error',
          message: proxy.$ts('请选择行数据')
        })
        return
      }

      if (!props.delRow(selectRows, item)) {
        return
      }
      //  proxy.$emit('delRow',dsel)

      ElMessageBox.confirm(proxy.$ts('确定要删除选中的行吗?'), proxy.$ts('提示'), {
        confirmButtonText: proxy.$ts('确定'),
        cancelButtonText: proxy.$ts('取消'),
        center: true,
        type: 'warning'
      }).then(() => {
        let key = item.table ? item.key : props.detail.key
        //记录删除的行数据
        const _keys = selectRows
          .filter((x) => {
            return x.hasOwnProperty(key) && x[key]
          })
          .map((v) => {
            return v[key]
          })
        //reloadMainTable()
        resetDelKeys(item, _keys)
        getTable(item.table).delRow()
        updateDetailTableSummaryTotal(item.table)
      })
    }

    const updateDetailTableSummaryTotal = (table) => {
      // //2021.09.25增加明细表删除、修改时重新计算行数与汇总
      // //2021.12.12增加明细表判断(强制刷新合计时会用到)
      // if (!props.detail.columns.length) {
      //   return
      // }
      //删除或新增行时重新设置显示的总行数

      const refTable = getTable(table)
      refTable.paginations.total = refTable.rowData.length
      //重新设置合计
      if (refTable.summary) {
        refTable.columns.forEach((column) => {
          if (column.summary) {
            refTable.getInputSummaries(null, null, null, column)
          }
        })
      }
    }

    const workFlowSteps = reactive([])
    const audit = ref(null)
    //获取审核节点信息
    const auditClick = () => {
      let _row = JSON.parse(JSON.stringify(props.formFields))
      _row[props.keyField] = id.value
      console.log(_row)
      audit.value.open([_row], null, '', false)
    }
    const saveAudit = (params, rows, callback) => {
      //保存审核
      let keys = rows.map((x) => {
        return x[props.keyField]
      })
      // if (!this.auditBefore(keys, rows)) {
      //   return;
      // }
      let url = `api/${props.tableName}/audit?auditReason=${params.reason}&auditStatus=${params.value}`
      http.post(url, keys, 'loading....').then((x) => {
        // if (!this.auditAfter(x, keys)) {
        //   return;
        // }
        if (!x.status) return proxy.$message.error(x.message)

        callback && callback(x)
        proxy.$message.success(x.message)
        //this.refresh();
      })
    }
    //当前操作的行数据
    const currentRow = reactive({})

    //编辑页面加载表单数据
    const execGetPageData = (url, params) => {
      http.post(url, params, true).then((result) => {
        if (!result.rows) {
          ElMessage({
            type: 'error',
            message: proxy.$ts('参数不正确')
          })
          return
        }
        if (result.rows[0].hasOwnProperty('AuditStatus')) {
          if (result.rows[0].AuditStatus === 1) {
            buttons.forEach((x) => {
              if (x.value == 'Audit') {
                x.readonly = true
              }
            })
          }
        }
        Object.assign(currentRow, result.rows[0])
        for (const key in props.formFields) {
          let val = result.rows[0][key]

          if (val === null || val === '' || val === undefined) {
            if (Array.isArray(props.formFields[key])) {
              props.formFields[key] = []
            } else {
              props.formFields[key] = null
            }
            continue
          }
          //文件处理
          if (files.indexOf(key) != -1) {
            props.formFields[key] = val.split(',').map((x) => {
              let index = x.lastIndexOf('/')
              return {
                name: x.substr(x.length, index - 1),
                path: x
              }
            })
            continue
          }

          if (dicInfo[key]) {
            //多选
            if (['checkbox', 'selectList', 'treeSelect'].indexOf(dicInfo[key].type) != -1) {
              //多选
              val = val.split(',')
              if (dicInfo[key].isNumber) {
                props.formFields[key] = val.map((x) => {
                  return x * 1
                })
              }
              props.formFields[key] = val
              continue
            }
            if (dicInfo[key].type == 'cascader') {
              let orginData = []
              props.formOptions.forEach((option) => {
                option.forEach((item) => {
                  if (item.field == key) {
                    orginData = item.orginData || []
                  }
                })
              })
              let treeVal = proxy.base.getTreeAllParent(val, orginData)
              props.formFields[key] = treeVal.map((x) => {
                return x.id
              })
              //console.log('编辑级联');
              continue
            }
            if (dicInfo[key].isNumber) {
              if (typeof val === 'string') {
                props.formFields[key] = val + ''
              } else {
                props.formFields[key] = val * 1
              }
            } else {
              props.formFields[key] = val + ''
            }
            continue
          }

          props.formFields[key] = dicInfo[key] ? val + '' : val
          //校验图片、多选字段设置值
          //校验字段值类型
        }

        proxy.loadFormAfter(result, () => {})

        emit('loadFormAfter', result)
      })
    }

    const getData = () => {
      if (!id.value) {
        return
      }
      const url = `api/${props.tableName}/getPageData`
      const params = {
        page: 1,
        row: 1,
        wheres: JSON.stringify([{ name: props.keyField, value: id.value }])
      }
      //proxy.$emit('loadFormBefore', params, (x) => {
      proxy.loadFormBefore(params, (x) => {
        if (!x) {
          return
        }
        execGetPageData(url, params)
      })
    }

    const getDetailData = (item, row) => {
      if (!id.value) {
        return
      }
      // let query = {
      //   value: row ? row[this.table.key] : this.currentRow[this.table.key],
      //   tableName: table
      // };
      // getTable(item.table).load(query, true)
      // delKeys.length = 0
      // resetDelKeys(item)
      getTable(item.table).load({}, true)
      // http.post(props.detail.url,{page:})
    }

    const resetDelKeys = (item, _keys) => {
      if (props.details.length) {
        if (!item.delKeys) {
          item.delKeys = []
        }
        if (_keys && _keys.length) {
          item.delKeys.push(..._keys)
        } else {
          item.delKeys = []
        }
        return
      }
      if (_keys && _keys.length) {
        delKeys.push(..._keys)
      } else {
        delKeys.length = 0
      }
    }

    // if (id.value) {
    //   getData();
    //   //获取明细表数据
    //   // getDetailData();
    // }
    //图片与文件上传字段
    const files = []
    const dicInfo = reactive({})
    const uploadUrl = 'api/' + props.tableName + '/upload'
    const initDic = () => {
      const keys = []
      props.formOptions.forEach((options) => {
        options.forEach((op) => {
          if (route.query.audit) {
            op.readonly = true
          }
          if (op.type == 'editor') {
            op.url = uploadUrl
          } else if (['img', 'excel', 'file'].indexOf(op.type) != -1) {
            files.push(op.field)
            if (!op.url) {
              op.url = uploadUrl
            }
          } else if (op.dataKey) {
            dicInfo[op.field] = {
              data: [],
              type: op.type,
              isNumber: false,
              key: op.dataKey
            }
            keys.push(op.dataKey)
            op.data = dicInfo[op.field].data
          }
        })
      })
      if (!keys.length) {
        getData()
        return
      }
      //初始化数据字典
      const dicUrl = 'api/Sys_Dictionary/GetVueDictionary'
      http.post(dicUrl, keys, true).then((result) => {
        for (let index = 0; index < result.length; index++) {
          const dicData = result[index]
          for (const key in dicInfo) {
            if (dicInfo[key].key == dicData.dicNo) {
              //生成级联数据
              if (dicInfo[key].type == 'cascader' || dicInfo[key].type == 'treeSelect') {
                let _data = JSON.parse(JSON.stringify(dicData.data))
                let cascaderArr = proxy.base.convertTree(_data, (node, data, isRoot) => {
                  if (!node.inited) {
                    node.inited = true
                    node.label = node.value
                    node.value = node.key
                  }
                })
                props.formOptions.forEach((option) => {
                  option.forEach((item) => {
                    if (item.dataKey == dicData.dicNo) {
                      item.orginData = dicData.data
                      item.data = cascaderArr
                      if (!item.hasOwnProperty('checkStrictly')) {
                        item.checkStrictly = true
                      }
                    }
                  })
                })
              } //select2组件
              else if (dicData.data.length >= props.select2Count && !dicData.data[0].label) {
                dicData.data.forEach((item) => {
                  item.label = item.value
                  item.value = item.key
                })
              }
              dicInfo[key].data.splice(0)
              dicInfo[key].data.push(...dicData.data)
              if (dicData.data.length) {
                dicInfo[key].isNumber = typeof dicData.data[0].key !== 'string'
              }
            }
          }
        }
        dicInited(result)
        //获取数据
        getData()
      })
    }
    initDic()

    const btnClick = (item) => {
      if (item.click) {
        item.click()
      } else if (item.onClick) {
        item.onClick()
      }
    }
    const autitTableOptions = reactive({
      key: props.keyField,
      tableName: props.tableName,
      url: props.tableName,
      tableCNName: props.tableCNName
    })

    //表单字典加载后方法
    const dicInited = (dicData, table) => {
      emit('dicInited', dicData, table)
    }

    const loadDetail = (table) => {
      getDetailData({ table: table })
    }
    //重置表单
    const reset = () => {
      form.value && form.value.reset()
      //重置明细表数据
      table.value && table.value.reset()
      getData()
    }

    //初始化表单、明细表权限按钮
    initPermissionButtons()
    //初始化主从子表、二级表配置
    initDetailColumns()
    //初始化三级明细表配置
    initSubDetailColumns()

    //向edit.vue页面传递初始化信息
    emit('initButtons', buttons)
    //主从表按钮
    emit('initDetailButtons', detailButtons)

    //二级表配置
    if (props.details.length) {
      emit('initDetailColumns', props.details)
    }
    //三级表配置
    if (subDetails.value.length) {
      emit('initSubDetailColumns', subDetails.value)
    }

    const onSortEnd = (rows, newIndex, oldIndex) => {
      emit('onSortEnd', rows, newIndex, oldIndex)
    }

    // 监听contentId
    watch(
      () => props.value,
      (newValue, oldValue) => {
        if (route.query.id) {
          return;
        }
        if (oldValue && newValue) {
          id.value = newValue
          isAdd = false
          getData()
          return
        }
        if (!newValue) {
          isAdd = true
        }
      },
      { deep: true, immediate: true }
    )

    return {
      id,
      dicInfo,
      getData,
      buttons,
      detailButtons,
      loadDetailTableBefore,
      loadDetailTableAfter,
      loadSubDetailTableBefore,
      loadSubDetailTableAfter,
      tabsClick,
      rowClick,
      beginEdit,
      endEditBefore,
      form,
      table,
      detailsRef,
      detailAddRow,
      workFlowSteps,
      currentRow,
      btnClick,
      autitTableOptions,
      audit,
      saveAudit,
      getTable,
      save,
      getDetailData,
      loadDetail,
      dicInited,
      subDetails,
      subDetailsRef,
      reset,
      detailRowOnChange,
      onSortEnd
    }
  }
})
</script>
<style lang="less" scoped>
@import './edit.less';

// .form-buttons {
//   font-weight: 500;
// }
</style>
