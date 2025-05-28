import App from './App'
// #ifndef VUE3
import Vue from 'vue'
import './uni.promisify.adaptor'
Vue.config.productionTip = false
App.mpType = 'app'
const app = new Vue({
	...App
})
app.$mount()
// #endif


// #ifdef VUE3
import common from './util/common.js'
import formConverter from '@/components/vol-form/vol-form-converter.js'
Object.assign(common,formConverter);
 
import translator from '@/translator/index.js'
import http from './util/http.js'
import store from './store'
import uviewPlus from '@/uni_modules/uview-plus'
import {
	createSSRApp
} from 'vue'
export function createApp() {
	const app = createSSRApp(App)
	translator.init(false, app);
	app.config.globalProperties.$global = {
		form: {
			border: false, //表单显示边框
			fontSize: '30rpx', //字体大小
			textAlign: 'right', //表单标签显示位置(左边、右边)：left、right
		},
		table: { //表格配置
			border: true, //是否显示表格边框
			padding: 8, //表格间距
			oddRowStyle: '#f8fcff', //偶数行背景颜色
			fontSize: '28rpx', //表格字体大小
			textAlign: 'left', //表格标签显示位置(左边、右边)：left、center、right
		}
	}


	app.config.globalProperties.http = http;
	app.config.globalProperties.$store = store;
	app.config.globalProperties.base = common;

	app.config.globalProperties.$toast = function(message, duration) {
		uni.showToast({
			icon: "none",
			title: message,
			duration: duration || 2000
		})
	}




	//	app.use(store)
	app.use(uviewPlus)

	return {
		app
	}
}
// #endif
