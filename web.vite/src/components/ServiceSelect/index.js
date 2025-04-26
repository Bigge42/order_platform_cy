import select from './ServiceSelect.vue'
const ServiceSelect = {
    install: function (app) {
        app.component('service-select',select)
    }
}
export default ServiceSelect;