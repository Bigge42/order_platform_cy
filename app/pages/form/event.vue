<template>
	<view class="demo-form vol-bg">
		<view class="demo-pd-30">
			<vol-alert type="primary">
				<view>输入框、下拉框、日期、范围输入、级联、扫码、图片文件上传等选择及输入事件处理</view>
			</vol-alert>
		</view>
		<view class="form-content">
			<vol-form ref="formRef" :form-options="editFormOptions" :formFields="editFormFields"
				@extraClick="extraClick" @inputConfirm="inputConfirm" @onChange="onChange">
			</vol-form>
		</view>
		<view class="btns">
			<vol-button class="btn" type="default" @click="reset">重置表单</vol-button>
			<vol-button class="btn" type="default" @click="setInputFocus">设置焦点</vol-button>
			<vol-button class="btn" @click="validate" type="error">表单校验</vol-button>
		</view>
	</view>
</template>

<script setup>
	import {
		onLoad,
		onShow,
		onHide,
		onPageScroll
	} from '@dcloudio/uni-app'
	
	import {
		ref,
		reactive,
		defineProps,
		defineEmits,
		defineExpose,
		getCurrentInstance,
		onMounted
	} from 'vue'

	const {
		proxy
	} = getCurrentInstance()

	const formRef = ref(null)
	const reset = () => {
		formRef.value.reset();
		proxy.$toast('表单已重置')
	}
	const validate = () => {
		const b = formRef.value.validate();
		if (b) {
			proxy.$toast('校验成功')
		}
	}
	//扫一扫、额外按钮点击事件
	const extraClick = (item, fields) => {
		if (item.field == 'scanCode') {
			uni.scanCode({
				success: (res) => {
					editFormFields[item.field] = res.result;
					proxy.$toast('扫码成功')
				},
				fail() {
					proxy.$toast('扫码失败')
				}
			})
			return;
		}
		if (item.field == 'customInput') {
			editFormFields[item.field] = ~~(Math.random() * 1000000)
			proxy.$toast('点击了按钮')
		}
	}
	//回车事件
	const inputConfirm = (field, $e) => {
		if (field == 'inputText1') {
			proxy.$toast('触发了回车事件')
		}
	}
	//下拉框、日期、radio选择事件
	const onChange = (field, value, item, data) => {
		//选择时可以给其他下拉框重新绑定值,同时清空其他字段的值： editFormFields.字段=null
		// editFormOptions.find(c=>{
		// 	  if (c.field=='字段') {
		// 	  	  c.data=[{key:1,value:"名称"}]
		// 	  }
		// })
		
		if (field == 'selectVal') {
			proxy.$toast('选择了下拉框：' + value+':'+item.value)
			return;
		}
		if (field == 'selectListVal') {
			proxy.$toast('选择多选下拉框：' + value.join(','))
			return;
		}
		proxy.$toast('选择了：' + value)
		return;
	}
    
	//表单配置
	const editFormFields = reactive({
		inputText1: "",
		InputFocus: "",
		inputText4: "输入框后固定文本样式",
		customInput: "自定义输入框",
		scanCode: null,
		
		selectVal: "2",
		selectListVal: [], //多选这里的值是数组 
		dateValue: proxy.base.getDate(), //设置默认日期为当天
		datetimeValue: "2022-03-27 20:15",
		dateRange: ["2022-03-10", "2022-06-20"], //数组 
	});
	const editFormOptions = reactive([ {
			type: "input",
			"title": "监听回车",
			"required": true,
			placeholder: "按回车或手机按完成触发事件",
			"field": "inputText1",
		},
		{
			type: "input",
			"title": "设置焦点",
			placeholder: "自动获取焦点",
			"field": "InputFocus",
		},
		{
			"title": "自定义按钮",
			"field": "customInput",
			extra: {
				text: "按钮",
				button: true,
				type: "primary",
				icon: "search",
				color: "#ffff",
				size: 13
			}
		},
		{
			"title": "扫码事件",
			"field": "scanCode",
			placeholder: "请扫码",
			extra: {
				text: "扫一扫",
				icon: "scan",
				style: "margin-left:20rpx;align-items: center;;color:#007aff;font-size:26rpx",
				color: "#007aff",
				size: 18
			}
		},
		{
			"title": "额外文本",
			"required": false,
			"field": "inputText4",
			valueStyle: "color:red",
			extra: {
				text: "单位(KG)",
				style: "margin-left:20rpx;align-items: center;color:red;font-size:26rpx",
			}
		},
		{
			type: "group", //表单分组
			style: "color: #9e9e9e;font-size: 26rpx;",
			title: "下拉框单选、多选、日期、事件"
		},
		{
			"title": "下拉框",
			"field": "selectVal",
			type: "select",
			"required": true,
			data: [],
			key: "订单类型"
		},
		{
			"title": "下拉框多选",
			"field": "selectListVal",
			type: "selectList",
			"required": true,
			data: [],
			key: "订单类型"
		},
		{
			type: "group", //表单分组
			style: "font-weight: 500;font-size: 26rpx;color: #848383;",
			title: "日期设置min与max属性限制选择范围"
		},
		{
			"title": "日期",
			"required": true,
			"type": "date",
			"field": "dateValue",
			//设置时间选择范围，如果日期是datetim类型，时间后面加上时分秒
			//2023.04.02更新util->common.js才能使用获取日期的方法
			// min:'2023-04-01',
			// max:'2023-07-02'
			//设置只能选择半个月内的数据
			min: proxy.base.addDays(proxy.base.getDate(), -15),
			max: proxy.base.getDate()
		},
		{
			"title": "日期时分秒",
			"type": "datetime",
			"field": "datetimeValue"
		},
		{
			"title": "日期范围",
			"type": "date",
			range: true, //区间输入
			"field": "dateRange"
		}
	])

	const setInputFocus = () => {
		proxy.base.setFormFocus(editFormOptions,'InputFocus')
		proxy.$toast('设置焦点成功')
		
		//或
		// editFormOptions.forEach(x => {
		// 	if (x.field == 'InputFocus') {
		// 		x.focus = false; //必须先设置为false
		// 		setTimeout(() => {
		// 			x.focus = true;
		// 			proxy.$toast('设置焦点成功')
		// 		}, 500)
		// 	}
		// })
	}
	//可以在页面启动的时候设置默认焦点
	//setInputFocus();
	
	
	//页面加载时可以从后台获取给表单绑定值
	onLoad((options)=>{
		 //options获取跳转时参数
		 
		 // proxy.http.post("url",{参数},true).then(res=>{
		 // 	editFormFields.字段1=res.字段1;
		 // 	editFormFields.字段2=res.字段2;
		 // 	//注意如果是多选，或者区间，值应该是数组，见上面给的editFormFields的格式
		 
		 // 	//如果是图片，见上面editFormFields图片字段格式的说明
		 // 	editFormFields.字段3=(res.字段3||'').split(',')
		 // 	   .filter(c=>{return c})
		 // 	   .map(c=>{return {
		 // 		    orginUrl:c.split('/').pop(),
		 // 	         url:proxy.ipAddress+c
		 // 	   }})
		 //})
	})
	
</script>

<style scoped lang="less">
	.demo-form {
		display: flex;
		flex-direction: column;
	}

	.form-content {
		//flex: 1;//按钮显示到最底部
		overflow: auto;
	}

	.btns {
		// background: #fff;
		bottom: 40rpx;
		display: flex;
		padding: 20rpx 0;

		.btn {
			flex: 1;
		}
	}
</style>
