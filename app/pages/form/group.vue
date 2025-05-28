<template>
	<view class="demo-form vol-bg">
		<view class="demo-pd-30">
			<vol-alert type="primary">
				<view>多个字段按分组显示在一行，减少页面幅度，并支持编辑功能，同时适用于自动生成的页面</view>
			</vol-alert>
		</view>
		<view class="form-content">
			<vol-form ref="formRef" :form-options="editFormOptions" :formFields="editFormFields">
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
		getCurrentInstance
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

	const editFormFields = reactive({
		processCode: "G202412080000001",
		processLine: "测试第二路线第一工序",
		materialAmount: 100,
		jobAmount: 5,
		cj: "(ICR)成品部",
		major: 1,
		remark: "测试第二路线第一工序测试第二路线"
	});
	const editFormOptions = reactive([{
			type: "input",
			"title": "工序编号",
			"required": true,
			"field": "processCode",

		},
		{
			type: "input",
			"title": "工艺路线",
			"readonly": true,
			"field": "processLine",
		},
		{
			type: "group"
		},
		[{
				"title": "用料数量",
				type: "decimal",
				"field": "materialAmount",
			},
			{
				"title": "作业次数",
				type: "number",
				"field": "jobAmount",
			}
		],
		[{
			"title": "生产车间",
			"field": "cj",
		}, {
			"title": "关键工序",
			"field": "major",
			type: "select",
			data: [{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"},{key:"a",value:"aaaa"}],
			//dataKey: "enable"
		}],
		{
			type: "group"
		},
		{
			"title": "工序备注",
			"field": "remark",
			"type": "input"
		}
	])
	
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
