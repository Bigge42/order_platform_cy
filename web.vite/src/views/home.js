export const test11 = () => {alert('test11')}
export const test21 = () => {}
import { ref, getCurrentInstance } from "vue";
export const datav = () => {
    return {
        v1:ref('v123'),
        v2:ref('v234')
    }
}


