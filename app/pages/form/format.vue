<template>
	<view class="demo-form vol-bg">
		<view class="demo-pd-30">
			<vol-alert type="primary">
				<view>可设置设置标题、值及行颜色</view>
			</vol-alert>
		</view>
		<view class="form-content">
			<vol-form ref="formRef" :form-options="editFormOptions" :formFields="editFormFields"
				@extraClick="extraClick" :getStyle="getStyle">
			</vol-form>
		</view>
		<view class="btns">
			<vol-button class="btn" type="default" @click="reset">重置表单</vol-button>
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
		//formRef.value.reset();
		proxy.base.resetForm(editFormFields, editFormOptions)
		proxy.$toast('表单已重置')
	}
	const validate = () => {
		const b = formRef.value.validate();
		if (b) {
			proxy.$toast('校验成功')
		}
	}


	//fields表单全部的值，option表单配置。 isValue=true右边值的样式。isValue=false左边值的样式
	const getStyle = (fields, option, isValue) => {
		//判断字段来设置样式
		if (option.field == 'dyValue') {
			////可以根据不同的值判断设置不同的类型
			// if (fields.dyValue=='xx') {
			// }
			
			//右边值的样式
			if (isValue) {
				return {'font-weight':'700','font-size':'32rpx',color:'#f09102'}
			}
			//左边标题的样式
			if (!isValue) {
				return {color:'red'}
			}
		}
		console.log(fields, option, isValue)
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
		if (item.field == 'customInput' || item.field == 'customInput2') {
			editFormFields[item.field] = ~~(Math.random() * 1000000)
			proxy.$toast('点击了按钮')
		}
	}

	//表单配置
	const editFormFields = reactive({
		titleColor: "20000",
		colorValue: 10000,
		inputText1: "",
		InputFocus: "",
		dyValue: 36000,
		inputText4: "输入框后固定文本样式",
		customInput: "自定义输入框",
		customInput2: "",
		scanCode: null,
	});
	const editFormOptions = reactive([{
			"title": "标题样式",
			titleStyle: "color:#007aff", //标题样式
			"field": "titleColor"
		}, {
			"title": "输入框值样式",
			valueStyle: "color:#007aff", //标题样式
			"field": "colorValue"
		},
		{
			type: "group"
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
		}, {
			"title": "自定义按钮+颜色",
			"field": "customInput2",
			itemStyle: "background:rgb(232 244 255)", //行样式
			titleStyle: "color:#007aff", //标题样式
			valueStyle: "color:red", //值样式
			extra: {
				text: "按钮",
				button: true,
				type: "primary",
				icon: "search",
				color: "#ffff",
				size: 12
			}
		},
		{
			type: "group",
			style:"font-size:26rpx;padding-top:10rpx",
			title:"可根据不同值与字段动态设置[标题、值]的样式"
		},
		{
			"title": "动态样式",
			"field": "dyValue"
		},
		{
			type: "group"
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
		}

	])

	//可以在页面启动的时候设置默认焦点
	//setInputFocus();

	//页面加载时可以从后台获取给表单绑定值
	onLoad((options) => {
		//options获取跳转时参数
		//调用接口获取表单的值
		// proxy.http.post("url",{参数},true).then(res=>{
			//const fields=res;
			////表单设置值，注意fields的类型应该是json数组格式：{字段1:值1,字段2:值2}
			//proxy.base.resetForm(editFormFields, editFormOptions,fields)
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
