// import Vue from 'vue'
import store from './../store'
import en from './locale/en.json'
import http from '@/util/http.js'
//默认中文
var currentLang = 'zh-CN';
var lang_storage_key = 'pro_lang';

function $ts(key) {
	// console.log(store.getters.local)
	// console.log(store.getters.local[key])
	if (isCN) {
		return key;
	}
	return store.getters.local[key] || key;
}

function $tst(key, templateVal) {
	// if (key === undefined || key === null) {
	// 	return '';
	// }
	//替换模板
	return $ts(key).replace('{$ts}', templateVal);
}
let $tsArr = function(arr, toLower) {
	if (isCN) return arr.join('');
	let ts_string = '';
	for (let index = 0; index < arr.length; index++) {
		//toLower除了第一个，其他转小写
		if (toLower) {
			ts_string = ts_string + " " + ((store.getters.local[arr[index]] || arr[index]).toLocaleLowerCase());
		} else {
			ts_string = ts_string + " " + (store.getters.local[arr[index]] || arr[index]);
		}
	}
	return ts_string;
};

function $loadSource() {
	let langType = uni.getStorageSync(lang_storage_key);
	if (!langType) {
		langType = (uni.getSystemInfoSync() || {}).language;
		//中文
		if (langType == 'zh' || langType.substring(0, 3) == 'zh-') {
			langType = "zh-CN";
		}
	}

	$changeSource(langType, true);
}
let isCN=false;
export function $changeSource(langType, isInit, source) {
	if (currentLang == langType) {
		return;
	}
	currentLang = langType;
	switch (currentLang) {
		case "zh-CN":
			source = {}; //捷克语
			break;
		case 'en':
			source = en; //捷克语
			break;
		case 'de':
			source = de; //德语
			break;

		default:
			source = 'zh-CN'; //其他的默认英语
			break;
	}
	uni.setStorageSync(lang_storage_key, langType);
	store.commit("setLocal", source);
	isCN = currentLang == 'zh-CN';
	if (isCN) {
		source = {};
		Object.keys(en).map(x => {
			return source[x] = x;
		})
		store.commit("setLocal", source);
		return;
	}
	let url = 'lang/' + currentLang + '.js?r=' + Math.random();
	http.get(url, {}, false, (msg) => {
		console.log(msg)
		//store.commit("setLocal", en);
		store.commit("setLocal", {});
	}).then(result => {
		store.commit("setLocal", result);
		let pages = getCurrentPages(); //获取所有页面栈实例列表
		let prevPage = pages[pages.length - 2]; //上一页页面实例

		if (prevPage && prevPage.setTabLang) {
			setTimeout(() => {
				prevPage.setTabLang()
			}, 500)
			return;
		}

		prevPage = pages[pages.length - 1];
		if (prevPage && prevPage.setTabLang) {
			setTimeout(() => {
				prevPage.setTabLang()
			}, 300)
			return;
		}
	})
}
import tabBarChange from './tabBarChangeLanguage.js'

export default {
	init(useLang, app) {
		app.config.globalProperties.$tsTabBar = tabBarChange;
		app.config.globalProperties.useLang = useLang;

		app.config.globalProperties.$ts = $ts;
		app.config.globalProperties.$tst = $tst;
		app.config.globalProperties.$tsArr = $tsArr;
		if (!useLang) {
			return;
		}
		$loadSource();
		app.config.globalProperties.$changeSource = $changeSource;
	}
}
