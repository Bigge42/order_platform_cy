import VolSelectBox from './VolSelectBox.vue'
const SelectBox = {
  install: function (app) {
    // app.component('vol-select',selectTable);
    app.component('vol-select-box', VolSelectBox)
  }
}
export default SelectBox
