const saveData = (
  proxy,
  props,
  id,
  isAdd,
  delKeys,
  currentRow,
  getTable,
  detailsRef,
  subDetails,
  saveExecting
) => {
  const editFormFields = proxy.base.getFormValues(props.formFields, props.formOptions)

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
    formData.detailData = getDetailRows(props, getTable())
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
          itemDetail.data = detailsRef.value.getDiffRows(c.table, c.key, itemDetail.data, c.detail)
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
const getDetailRows = (props, table) => {
  let detailData = table.rowData
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

export { saveData }