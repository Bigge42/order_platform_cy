/*****************************************************************************************
**  Author:jxx 2023
**  QQ:461857658
**  框架文档： http://doc.volcore.xyz/
*****************************************************************************************/

let extension = {
  components: {
    gridHeader: '',
    gridBody: '',
    gridFooter: '',
    modelHeader: '',
    modelBody: '',
    modelRight: '',
    modelFooter: ''
  },
  tableAction: '',
  buttons: { view: [], box: [], detail: [] },
  methods: {
    onInit() {
      this.queryFields = ['SalesOrderNo', 'PlanTrackingNo']
    },
    onInited() {},
    searchBefore(param) {
      return true
    },
    searchAfter(result) {
      return true
    },
    addBefore(formData) {
      return true
    },
    updateBefore(formData) {
      return true
    },
    rowClick({ row, column, event }) {},
    modelOpenAfter(row) {}
  }
}
export default extension
