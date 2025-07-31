<template>
	<view class="demo-form vol-bg">
		<view class="demo-pd-30">
			<vol-alert type="primary">
				<view>监听表单输入或值变化时给其他字段设置值或计算值，同时适用于自动生成的页面</view>
			</vol-alert>
		</view>
		<view class="form-content">
			<vol-form ref="formRef" :getStyle="getStyle" :form-options="editFormOptions" :formFields="editFormFields">
			</vol-form>
		</view>
		<view class="btns">
			<vol-button class="btn" type="default" @click="reset">重置表单</vol-button>
			<vol-button class="btn" @click="validate" type="primary">表单校验</vol-button>
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
		watch,
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

	const getStyle = (fields, option, isValue) => {
		if (option.field === 'input4' && isValue) {
			return {
				color: 'red',
				'font-size': '36rpx',
				'font-weight': 700
			}
		}
	}

	//表单配置
	const editFormFields = reactive({
		input1: 100,
		input2: 200,
		input3: 120,
		input4: 320,
	});
	const editFormOptions = reactive([{
			type: "number",
			"title": "物料数量",
			"required": true,
			"field": "input1",
			extra: {
				text: "箱",
				style: "margin-left:10rpx;font-size:28rpx;color:rgb(181 185 190)",
			}
		},
		{
			type: "decimal",
			"title": "进货单价",
			"required": true,
			"field": "input2",
			extra: {
				text: "元",
				style: "margin-left:10rpx;font-size:28rpx;color:rgb(181 185 190)",
			}
		},
		{
			type: "decimal",
			"title": "运费",
			"required": true,
			"field": "input3",
			extra: {
				text: "元",
				style: "margin-left:10rpx;font-size:28rpx;color:#007aff",
			}
		}, {
			type: "group"
		}, {
			type: "input",
			"title": "合计成本",
			readonly: true,
			"field": "input4",
			itemStyle: "background:rgb(232 244 255);color:#007aff;", //行样式
			titleStyle: "color:#007aff", //标题样式
			valueStyle: "color:red", //值样式
			extra: {
				text: "元",
				style: "margin-left:10rpx;font-size:28rpx;color:rgb(181 185 190)",
			}
		},
	])


	const setTotalValue = () => {
		console.log(222)
		editFormFields.input4 = (((editFormFields.input1 * 1.0 || 0) +
			(editFormFields.input2 * 1.0 || 0) +
			(editFormFields.input3 * 1.0 || 0)) * 1.0).toFixed(2) * 1.0 //保留两位小数
	}

	//监听三个输入框的变化
	watch(
		() => editFormFields.input1,
		(newValue, oldValue) => {
			setTotalValue();
		})

	watch(
		() => editFormFields.input2,
		(newValue, oldValue) => {
			setTotalValue();
		})

	watch(
		() => editFormFields.input3,
		(newValue, oldValue) => {
			setTotalValue();
		})

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
