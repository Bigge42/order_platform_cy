import store from '@/store/index'

function getKey(key) {
  return 'filter:' + (key.replaceAll('/', ''))
}

export default {
  getName(key) { //获取缓存的名称数组
    key = getKey(key) + ":name";
    let nameArr = store.getters.data()[key];
    if (nameArr) {
      return nameArr;
    }
    store.getters.data()[key] = []
    try {
      store.getters.data()[key] = JSON.parse(localStorage.getItem(key)) || []
    } catch (error) {
      console.log(error)
    }
    return store.getters.data()[key];
  },
  getData(key, name) {
    key = getKey(key)
    if (store.getters.data()[key]) {
      if (name) {
        return store.getters.data()[key].find(c => { return c.name === name }).value || []
      }
      return store.getters.data()[key]
    }
    if (!store.getters.data()[key]) {
      store.getters.data()[key] = []
    }
    let cacheTxt = localStorage.getItem(key)
    if (cacheTxt) {
      try {
        store.getters.data()[key] = JSON.parse(cacheTxt)
      } catch (error) {
        console.log(error)
      }
    }
    if (name) {
      return store.getters.data()[key].find(c => { return c.name === name }).value || []
    }
    return store.getters.data()[key]
  },
  setData(key, name, value) {
    if (!value || !(value + '').trim()) {
      return
    }
    let data = this.getData(key);
    key = getKey(key)
    // data = store.getters.data()[key]
    if (!data) {
      store.getters.data()[key] = []
    }
    data = store.getters.data()[key]
    let obj = data.find((c) => {
      return c.name == name
    })
    if (obj) {
      obj.value = value
    } else {
      data.push({ name: name, value: value })
    }
    
    //移除数据源
    data = data.map(c => {
      let item = { name: c.name };
      let valueData = c.value.map(x => {
        let dicData = x.data;
        if (dicData && dicData.length) {
          let valueClone = {};
          Object.keys(x).forEach(k => {
            valueClone[k] = x[k]
          })
          valueClone.data = [];
          return valueClone;
        }
        return x;
      })
      item.value = valueData
      return item;
    })
    // console.log(data)
    // return;

    //这里序列化应该忽略data数据源
    localStorage.setItem(key, JSON.stringify(data))
    //缓存名称
    this.setNameCache(key, data)
  },
  removeData(key, name) {
    let data = this.getData(key);
    key = getKey(key)
    let index = data.findIndex((c) => {
      return c.name == name
    })
    if (index == -1) {
      return;
    }
    data.splice(index, 1);
    localStorage.setItem(key, JSON.stringify(data))
    //缓存名称
    this.setNameCache(key, data)
  },
  setNameCache(key, data) {
    //缓存名称
    let nameKey = key + ':name';
    const nameArr = data.map(c => { return c.name });
    store.getters.data()[nameKey].splice(0)
    store.getters.data()[nameKey].push(...nameArr)
    localStorage.setItem(nameKey, JSON.stringify(nameArr))
  },
  initFormData(rowOptions, result) { //计算字段与标签宽度
    let data = rowOptions.map(x => {
      result.fields[x.field] = x.value;
      let obj = {
        field: x.field,
        title: x.title,
        required: x.required,
        width: x.width,
        type: x.type
      }

      if (x.data) {
        obj.data = x.data;
        obj.dataKey = x.dataKey;
      } else {
        obj.type = x.searchType;
      }
      if (x.range) {
        obj.range = true;
        if (['date', 'datetime', 'month'].indexOf(x.type) != -1) {
          //  obj.range=true;
          obj.type = x.type;
        }
        if (!Array.isArray(result.fields[x.field]) || result.fields[x.field].length != 2) {
          result.fields[x.field] = [null, null]
        }
      }
      return obj;
    })
    result.form.push(data);
  },
  getFormOptions(formOptions) {//生成表单信息
    let result = { form: [], fields: {} };
    let rowOptions = [];
    let rowWidth = 0;
    formOptions.forEach(item => {
      if (!item.width) {
        item.width = 25;
      }
      rowWidth += item.width;
      if (rowWidth <= 100) {
        rowOptions.push(item)
      } else {
        this.initFormData(rowOptions, result);
        rowOptions = []
        rowWidth = item.width;
        rowOptions.push(item)
      }
    })
    if (rowOptions.length) {
      this.initFormData(rowOptions, result);
    }
    return result;
  }
}
