const loadDicData = async (props, proxy,route, emit, dicInfo, files) => {
  const uploadUrl = "api/" + props.tableName + "/upload";
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
  if (!keys.length) return
  //初始化数据字典
  const dicUrl = 'api/Sys_Dictionary/GetVueDictionary'
  let result = await proxy.http.post(dicUrl, keys, true).then((res) => {
    return res
  })
  for (let index = 0; index < result.length; index++) {
    const dicData = result[index]
    for (const key in dicInfo) {
      if (dicInfo[key].key != dicData.dicNo) {
        continue
      }
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
  emit('dicInited', result)
}

export { loadDicData }
