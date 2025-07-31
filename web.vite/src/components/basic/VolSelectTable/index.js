import VolSelectTable from './VolSelectTable.vue'
const SelectTable = {
  install: function (app) {
    // app.component('vol-select',selectTable);
    app.component('vol-select-table', VolSelectTable)
  }
}
export default SelectTable
