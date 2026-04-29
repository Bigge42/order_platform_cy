import { defineAsyncComponent } from 'vue'

const appTypes = [
  { key: 'chat', value: '\u5bf9\u8bdd\u578b' },
  { key: 'agent', value: 'Agent' },
  { key: 'workflow', value: 'Workflow' }
]

const statuses = [
  { key: 1, value: '\u542f\u7528' },
  { key: 0, value: '\u505c\u7528' }
]

const syncButtonName = '\u4eceAI\u5c0f\u52a9\u624b\u5e73\u53f0\u540c\u6b65'

function setOptionData(options, field, data) {
  options.forEach((row) => {
    row.forEach((item) => {
      if (item.field === field) {
        item.data = data
      }
    })
  })
}

let extension = {
  components: {
    gridHeader: defineAsyncComponent(() => import('./Sys_AIApp/Sys_AIAppGridHeader.vue')),
    gridBody: '',
    gridFooter: '',
    modelHeader: '',
    modelBody: '',
    modelRight: '',
    modelFooter: ''
  },
  tableAction: 'Sys_AIApp',
  buttons: { view: [], box: [], detail: [] },
  methods: {
    onInit() {
      setOptionData(this.editFormOptions, 'AppType', appTypes)
      setOptionData(this.editFormOptions, 'Status', statuses)
      setOptionData(this.searchFormOptions, 'AppType', appTypes)
      setOptionData(this.searchFormOptions, 'Status', statuses)

      const appTypeColumn = this.columns.find((x) => x.field === 'AppType')
      if (appTypeColumn) appTypeColumn.bind = { data: appTypes }

      const statusColumn = this.columns.find((x) => x.field === 'Status')
      if (statusColumn) statusColumn.bind = { data: statuses }

      const canManage = this.buttons.some((x) => x.value === 'Add' || x.value === 'Update')
      if (canManage && !this.buttons.some((x) => x.name === syncButtonName)) {
        this.buttons.push({
          name: syncButtonName,
          icon: 'el-icon-refresh',
          type: 'success',
          onClick: function () {
            this.$refs.gridHeader.open()
          }
        })
      }
    },
    modelOpenAfter() {
      if (this.currentAction === 'Add') {
        this.editFormFields.AppType = 'chat'
        this.editFormFields.SortNo = 0
        this.editFormFields.Status = 1
      }
    }
  }
}

export default extension
