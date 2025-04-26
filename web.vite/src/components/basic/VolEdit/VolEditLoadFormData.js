const loadFormData = async (props, proxy, emit, dicInfo, files, id, buttons, currentRow) => {
  if (!id.value) {
    return
  }
  const url = `api/${props.tableName}/getPageData`
  const params = {
    page: 1,
    row: 1,
    wheres: JSON.stringify([{ name: props.keyField, value: id.value }])
  }
  props.loadFormBefore(params, async (x) => {
    if (!x) {
      return
    }

    let result = await proxy.http.post(url, params, true).then((res) => {
      if (!res.rows) {
        proxy.$message.error(proxy.$ts('参数不正确'))
        return
      }
      return res
    })

    if (!result) return

    if (result.rows[0].hasOwnProperty('AuditStatus') && result.rows[0].AuditStatus === 1) {
      buttons.forEach((x) => {
        if (x.value == 'Audit') {
          x.readonly = true
        }
      })
    }
    Object.assign(currentRow, result.rows[0])
    for (const key in props.formFields) {
      let val = result.rows[0][key]

      if (val === null || val === '' || val === undefined) {
        props.formFields[key] = Array.isArray(props.formFields[key]) ? [] : null
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

    props.loadFormAfter(result, () => {})

    emit('loadFormAfter', result)
  })
}

export { loadFormData }
