<template>
	<view class="demo-form vol-bg">
		<view class="demo-pd-30">
			<vol-alert type="primary">
				<view>常用表单组件、事件处理、格式化、数据绑定、数据字典自动转换等操作框架都内置好了，只需简单配置即可</view>
			</vol-alert>
		</view>
		<view class="form-content">
			<vol-form ref="formRef" :form-options="editFormOptions" :formFields="editFormFields">
			</vol-form>
		</view>
		<view class="btns">
			<vol-button class="btn" type="default" @click="reset">重置表单</vol-button>
			<vol-button class="btn" type="default" @click="setReadonly">{{readonly?'取消只读':'表单只读'}}</vol-button>
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
	//只读
	const readonly = ref(false)
	const setReadonly = () => {
		readonly.value = !readonly.value
		editFormOptions.forEach(item => {
			//这里可以哪些字段指定只读
			// if (item.field=='字段') {

			// }
			if (Array.isArray(item)) {
				item.forEach(x => {
					x.readonly = !x.readonly;
				})
			} else {
				item.readonly = !item.readonly;
			}
			proxy.$toast('设置成功')
		})


		//或者调用方法全部只读
		//formRef.value.setReadonly(readonly.value)
	}

	const editFormFields = reactive({
		inputText1: "这里是一个输入框",
		inputReaonly: "只读不可输入",
		inputText2: "按回车触发事件",
		inputText4: "输入框后固定文本样式",
		customInput: "自定义输入框",

		name: "小黄瓜",
		phone: "139100000066",
		height: 183,
		weight: 140,

		scanCode: null,
		textarea: "这里文字有点长有点长,这里有点长文字有点长这有点长。。",
		pwd: "12345",
		readonlyText: "只读输入框",
		cascader1: null,
		cascader2: null,
		cascader3: '004',
		selectVal: "2",
		selectListVal: [], //多选这里的值是数组 
		dateValue: proxy.base.getDate(), //设置默认日期为当天
		datetimeValue: "2022-03-27 20:15",
		dateRange: ["2022-03-10", "2022-06-20"], //数组 
		inputRange: [100000000, 900000000], //区间是数组


		province: "北京市,北京市,海淀区", //省市区县值必须以逗号隔开

		inputDecimal: null, //小数
		inputNumber: null, //数字
		switchValue: 1,
		radioVal: "1", //单选
		selectClickValue: "",
		dateClickValue: null,
		//注意图片格式，如果是后台返回的图片，在前端拼接下路径，如：
		//  "xx/a.png,xx/b.png".split(',').filter(c=>{return c})
		//    .map(c=>{return {
		//	        orginUrl:c.split('/').pop(),
		//          url:proxy.ipAddress+c
		//    }})
		//如果没有默认值，只需要设置imgs:[]
		imgs: [{
			orginUrl: "Upload/Tables/Sys_User/202411200151443700/wechat.jpg", //文件存储的路径
			url: "static/logo.png" //文件全路径
		}],
		files: [{
			orginUrl: "Upload/Tables/Sys_User/202411200151443700/wechat.jpg", //文件存储的路径
			url: "static/logo.png" //文件全路径
		}, {
			orginUrl: "/Upload/Tables/Sys_User/202412300231051917/NET8运行时新增功能.pdf", //文件存储的路径
			url: "static/logo.png" //文件全路径
		}],
	});
	const editFormOptions = reactive([{
			"title": "基础输入操作",
			style: "font-weight: 500;color: #9e9e9e;font-size: 26rpx;",
			type: "group"
		}, {
			type: "input",
			"title": "输入框",
			"required": true,
			"field": "inputText1",
		},
		{
			type: "input",
			"title": "只读字段",
			"readonly": true,
			"field": "inputReaonly",
		},

		{
			"title": "自定义按钮+颜色",
			"field": "customInput",
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
				style: "margin-left:20rpx;align-items: center;color:#848383;font-size:26rpx",
			}
		},
		{
			"title": "小数(手机上预览)",
			"type": "decimal",
			field: "inputDecimal" //只能输入小数

		},
		{
			"title": "整数(手机上预览)",
			"type": "number",
			field: "inputNumber" //只能输入数字 
		},
		{
			"title": "多字段显示及字段输入",
			style: "color: #9e9e9e;font-size: 26rpx;",
			type: "group"
		},
		[{
				"title": "姓名",
				"field": "name",
				type: "",
			},
			{
				"title": "电话",
				"field": "phone",
				type: "",
			}
		],
		[{
				"title": "身高(cm)",
				"field": "height",
				type: "decimal",
			},
			{
				"title": "体量(kg)",
				"field": "weight",
				type: "decimal",
			}
		],
		{
			"title": "密码框",
			"field": "pwd",
			"type": "password"
		},
		{
			"title": "只读框",
			"field": "readonlyText",
			"type": "text",
			readonly: true
		},
		{
			"title": "省市区县",
			"field": "province",
			type: "city" //type必须为city
		}, {
			"title": "多文本",
			"field": "textarea",
			type: "textarea"
		},
		{
			type: "group", //表单分组
			style: "color: #9e9e9e;font-size: 26rpx;",
			title: "下拉框单选、多选"
		},
		{
			"title": "下拉框",
			"field": "selectVal",
			type: "select",
			"required": true,
			data: [],
			// data: [{ //也可以手动绑定数据源，参照格式：
			// 	key: "1",
			// 	value: "正常"
			// }, {
			// 	key: "2",
			// 	value: "异常"
			// }],
			key: "订单类型"
		},
		{
			"title": "多选框",
			"field": "selectListVal",
			type: "selectList",
			"required": true,
			data: [],
			key: "订单类型"
		},
		{
			"title": "是否值",
			"type": "switch",
			"field": "switchValue"
		},
		{
			"title": "单选",
			"type": "radio",
			data: [{
				key: "1",
				value: "正常"
			}, {
				key: "2",
				value: "异常"
			}, {
				key: "2",
				value: "离线"
			}],
			key: "",
			placement: 'row', //布局方式，row横向，column纵向	,具体见uvivew文档
			//placement:"column",	
			"field": "radioVal"
		},
		{
			type: "group", //表单分组
			style: "font-weight: 500;font-size: 26rpx;color: #848383;",
			title: "设置checkStrictly=true,只能选择最后一级节点"
		},
		{
			"title": "树形级联",
			"field": "cascader1",
			type: "cascader",
			"required": true,
			checkStrictly: true, //是否只能选择最后一个节点,默认可以选择任意节点
			data: [],
			key: "tree_roles"
		},
		{
			"title": "树形级联(只能选最后一级)",
			"field": "cascader2",
			type: "cascader",
			"required": true,
			checkStrictly: false, //是否只能选择最后一个节点,默认可以选择任意节点
			data: [],
			key: "tree_roles"
		},
		{
			"title": "自定义级联",
			"field": "cascader3",
			type: "cascader",
			"required": true,
			checkStrictly: false, //是否只能选择最后一个节点,默认可以选择任意节点
			data: [{
				id: "001",
				parentId: null,
				name: "一级节点"
			}, {
				id: "002",
				parentId: "001",
				name: "二级节点"
			}, {
				id: "003",
				parentId: null,
				name: "三级节点"
			}, {
				id: "004",
				parentId: "003",
				name: "四级节点"
			}]
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
		},
		{
			"title": "区间输入",
			"type": "decimal", //number数字，text文本输入
			range: true, //区间输入
			"field": "inputRange"
		},
		{
			type: "group", //表单分组
			style: "font-weight: 500;font-size: 26rpx;color: #848383;",
			"title": "多图片、视频、文件上传",
		},
		{
			"title": "图片上传",
			"type": "img",
			readonly: false, //设置图片只读
			"url": "api/sys_user/upload", //后台框架自带的上传方法，如果涉及权限问题，请参照后台开发文档上重写权限来重写upload方法的权限
			"multiple": true, //从图上传
			"maxCount": 3, //最多只能上传3张图片
			"field": "imgs"
		}, {
			"title": "文件上传",
			"type": "file",
			readonly: false, //设置图片只读
			"url": "api/sys_user/upload", //后台框架自带的上传方法，如果涉及权限问题，请参照后台开发文档上重写权限来重写upload方法的权限
			"multiple": true, //从图上传
			"maxCount": 3, //最多只能上传3张图片
			"field": "files"
		},
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
		flex: 1;
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
