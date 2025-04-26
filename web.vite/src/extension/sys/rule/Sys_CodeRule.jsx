
let extension = {
  components: {
    //查询界面扩展组件
    gridHeader: '',
    gridBody: '',
    gridFooter: '',
    //新建、编辑弹出框扩展组件
    modelHeader: '',
    modelBody: '',
    modelRight: '',
    modelFooter: ''
  },
  tableAction: '', //指定某张表的权限(这里填写表名,默认不用填写)
  buttons: { view: [], box: [], detail: [] }, //扩展的按钮
  methods: {
    //下面这些方法可以保留也可以删除
    onInit() {
      const tableOption = this.getFormOption('TableName')
      tableOption.type = 'select'
      tableOption.required = true
      tableOption.data = []
      tableOption.onChange = (val) => {
        this.editFormFields.TableCNName = tableOption.data.find((x) => {
          return x.key == val
        }).value
        this.editFormFields.Field="";
        this.editFormFields.OrderFiled="";
        this.getTableFields()
      }

      //前缀
      this.getFormOption('PrefixCode').required = true

      const ruleOption = this.getFormOption('RuleType')
      ruleOption.type = 'radio'
      ruleOption.data = [
        { key: 'yyyy', value: '年' },
        { key: 'yyyyMM', value: '年月' },
        { key: 'yyyyMMdd', value: '年月日' }
        // { key: 'year', value: '编号每年从1开始自增' },
        // { key: 'none', value: '编号累计自增' }
      ]
      ruleOption.onChange = () => {
        this.initViewCode()
      }

      const ruleIncrementalOption = this.getFormOption('RuleIncremental')
      ruleIncrementalOption.required = true
      ruleIncrementalOption.type = 'select'
      ruleIncrementalOption.data = [
        { key: 'day', value: '编号每天从1开始自增' },
        { key: 'month', value: '编号每月从1开始自增' },
        { key: 'year', value: '编号每年从1开始自增' },
        { key: 'none', value: '编号累计自增' }
      ]
      ruleIncrementalOption.onChange = () => {
        this.initViewCode()
      }

      const lenOption = this.getFormOption('ValueLen')
      lenOption.min = 2
      lenOption.max = 20
      lenOption.onKeyPress = () => {
        this.initViewCode()
      }

      const fieldOption = this.getFormOption('Field')
      fieldOption.data = []
      fieldOption.type = 'select'
      fieldOption.required = true
      fieldOption.onChange = (val) => {
        this.editFormFields.CodeText1 = fieldOption.data.find((x) => {
          return x.key == val
        }).value
      }

      const orderFiled = this.getFormOption('OrderFiled')
      orderFiled.data = []
      orderFiled.type = 'select'

      orderFiled.required = true
      orderFiled.onChange = (val) => {
        this.editFormFields.CodeText2 = orderFiled.data.find((x) => {
          return x.key == val
        }).value
      }

      this.columns.forEach((x) => {
        if (x.field == 'Field') {
          x.formatter = (row) => {
            return this.$ts(row.CodeText1) + '(' + row.Field + ')'
          }
        } else if (x.field == 'OrderFiled') {
          x.formatter = (row) => {
            return this.$ts(row.CodeText2) + '(' + row.OrderFiled + ')'
          }
        }
      })

      this.getFormOption('PrefixCode').onKeyPress = () => {
        this.initViewCode()
      }

      this.getFormOption('ConcatenationSymbol').onKeyPress = () => {
        this.initViewCode()
      }

       const codeOption= this.getFormOption("Code");
       codeOption.label=true;
       codeOption.inputStyle={'color':'#0690fe','font-size':'16px','font-weight':'bold','letter-spacing':'1px'}
    },
    onInited() {},
    initViewCode() {
      setTimeout(() => {
        let code = []
        code.push(this.editFormFields.PrefixCode || 'P')
        let date = this.base.getDate().replace(/-/g, '')
        if (this.editFormFields.RuleType == 'yyyy') {
          code.push(date.substring(0, 4))
        } else if (this.editFormFields.RuleType == 'yyyyMM') {
          code.push(date.substring(0, 6))
        } else {
          code.push(date.substring(0, 8))
        }
        code.push(String(1).padStart(this.editFormFields.ValueLen, '0'))

        this.editFormFields.Code = code.join(this.editFormFields.ConcatenationSymbol || '')
      }, 100)
    },
    searchBefore(param) {
      //界面查询前,可以给param.wheres添加查询参数
      //返回false，则不会执行查询
      return true
    },
    searchAfter(result) {
      //查询后，result返回的查询数据,可以在显示到表格前处理表格的值
      return true
    },
    addBefore(formData) {
      //新建保存前formData为对象，包括明细表，可以给给表单设置值，自己输出看formData的值
      return true
    },
    updateBefore(formData) {
      //编辑保存前formData为对象，包括明细表、删除行的Id
      return true
    },
    rowClick({ row, column, event }) {
      //查询界面点击行事件
      // this.$refs.table.$refs.table.toggleRowSelection(row); //单击行时选中当前行;
    },
    modelOpenBefore() {
      const tableOption = this.getFormOption('TableName')
      if (tableOption.data.length) {
        return
      }
      let permission = this.$store.getters.getPermission()
      const tables = permission
        .filter((x) => {
          return (
            (!x.children || !x.children.length) &&
            x.tableName &&
            x.tableName.length > 2
          )
        })
        .map((x) => {
          return { key: x.tableName, value: x.name }
        })
      let url = 'api/Sys_CodeRule/getTableInfo'
      let tableArr = tables.map((x) => {
        return x.key
      })
      this.http.post(url, tableArr, false).then((res) => {
        tableOption.data = tables.filter((x) => {
          return res.includes(x.key)
        })
      })
      return true
    },
    getTableFields() {
      let url = 'api/Sys_CodeRule/getFields?table=' + this.editFormFields.TableName
      this.http.get(url).then((x) => {
        this.getFormOption('Field').data = x.filter((c) => {
          return c.columnType == 'string'
        })
        this.getFormOption('OrderFiled').data = x.filter((c) => {
          return c.columnType == 'date' || c.columnType == 'datetime'
        })
      })
    },
    modelOpenAfter(row) {
      if (this.currentAction == 'Add') {
        this.getFormOption('Field').data = []
        this.getFormOption('OrderFiled').data = []
        this.editFormFields.ValueLen = 4
        this.initViewCode()
        return
      }
      this.editFormFields.ValueLen = this.editFormFields.ValueLen * 1
      this.getTableFields()
    }
  }
}
export default extension
