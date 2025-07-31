export default function () {
  return {
    value: null,
    editChange: {
      //新建数据后，变更为编辑状态
      type: Boolean,
      default: false
    },
    sortable: {
      //明细表拖动排序
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
    },
    printBefore:{
       type:Function,
       default:(rows)=>{
        return true;
       }
    }
  }
}
