import { getGridTableRef } from './ViewGridRef.js'

import { getUrl, searchExec } from './ViewGridProvider.jsx'
import {onDelete} from './ViewGridEventButton.jsx'
//行内编辑
export const editTableAddRow = (proxy, props, row, index) => {
  if (!proxy.tableAddRowBefore.call(proxy, row, index)) {
    return
  }
  if (!props.tableAddRowBefore(row, index)) {
    return
  }
  const tableRef = getGridTableRef(proxy, props, true)
  const newRow = proxy.getDefaultRow.call(proxy, row, index) || {}
  Object.assign(newRow, props.getDefaultRow(row, index))
  tableRef.rowData.splice(index + 1, 0, newRow)
  setTimeout(() => {
    tableRef.edit.rowIndex = index + 1
  }, 50)
}
export default (proxy, props, ctx, dataConfig) => {
  return {
    initEditTable: () => {
      if (!props.table.editTable) {
        return
      }
      let w = 40
      let hasAdd, hasEdit, hasDel
      const buttons = dataConfig.buttons.value
      buttons.forEach((c) => {
        if (c.value === 'Add') {
          // c.hidden = true;
          hasAdd = true
          w += 40
        } else if (c.value === 'Update') {
          c.hidden = true
          hasEdit = true
          // w += 20;
        } else if (c.value === 'Delete') {
          hasDel = true
          w += 40
        }
      })
      if (!hasAdd && !hasEdit && !hasDel) {
        return
      }
      // dataConfig.doubleEdit.value = true;
      props.columns.push({
        title: '操作',
        field: '操作',
        width: w,
        fixed: 'right',
        align: 'center',
        render: (h, { row, column, index }) => {
          return (
            <div>
              {hasAdd ? (
                <el-button
                  onClick={($e) => {
                    $e.stopPropagation()
                    editTableAddRow(proxy, props, row, index)
                  }}
                  type="primary"
                  link
                  icon="Plus"
                ></el-button>
              ) : null}
              {hasEdit || hasAdd ? (
                <el-button
                  onClick={($e) => {
                    $e.stopPropagation()
                    editTableSave(proxy, props, row)
                  }}
                  type="success"
                  link
                  icon="Check"
                ></el-button>
              ) : null}
              {hasDel ? (
                <el-button
                  link
                  onClick={($e) => {
                    $e.stopPropagation()
                    editTableDelRow(proxy, props,row, index)
                  }}
                  v-show={hasDel}
                  type="danger"
                  icon="Delete"
                ></el-button>
              ) : null}
            </div>
          )
        }
      })
    }
  }
}

 const editTableSave = async (proxy, props, row) => {
  const mainData = {}
  Object.keys(row).forEach((key) => {
    let _val = row[key]
    if (Array.isArray(_val)) {
      if (_val.some((c) => c.path)) {
        _val = _val.map((c) => c.path).join(',')
      } else {
        _val = _val.join(',')
      }
    }
    mainData[key] = _val
  })
  const params = {
    mainData: mainData
  }
  const isUpdate = !!row[props.table.key]
  let url = getUrl(isUpdate ? 'update' : 'add', null, props.table)
  if (!isUpdate) {
    if (
      (await proxy.addBefore.call(proxy, params, row)) === false ||
      !(await proxy.addBeforeAsync.call(proxy, params, row))
    ) {
      return
    }
    if (
      (await props.addBefore(params, row)) === false ||
      !(await props.addBeforeAsync(params, row))
    ) {
      return
    }
  } else {
    if (
      (await proxy.updateBefore.call(proxy, params, row)) === false ||
      !(await proxy.updateBeforeAsync.call(proxy, params, row))
    ) {
      return
    }
    if (
      (await props.updateBefore(params, row)) === false ||
      !(await props.updateBeforeAsync(params, row))
    ) {
      return
    }
  }

  let tigger = false
  await proxy
    .$confirm(proxy.$ts('确认要保存数据吗?'), proxy.$ts('提示'), {
      confirmButtonText: proxy.$ts('确定'),
      cancelButtonText: proxy.$ts('取消'),
      type: 'warning',
      center: true
    })
    .then(async () => {
      if (tigger) return
      tigger = true
      const url = getUrl(isUpdate ? 'update' : 'add', null, props.table)
      proxy.http.post(url, params, proxy.$ts('正在处理') + '....').then((x) => {
        if (!x.status) return proxy.$error(x.message)
        const name = isUpdate ? 'updateAfter' : 'addAfter'

        if (!proxy[name].call(proxy, x, params, row)) {
          return
        }
        if (!props[name](x, params, row)) {
          return
        }

        proxy.$message.success(x.message)
        getGridTableRef(proxy, props, true).edit.rowIndex = -1
        searchExec(proxy, params, false)
      })
    })
}

const editTableDelRow = (proxy, props, row, index) => {
  if (!row[props.table.key]) {
    const tableRef = getGridTableRef(proxy, props, true)
    tableRef.rowData.splice(index, 1)
    tableRef.edit.rowIndex = -1
    return
  }
  onDelete(proxy,props,row)
}
