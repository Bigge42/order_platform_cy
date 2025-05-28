
<template>
	<vol-view ref="viewRef" :table="table" :columns="columns" :table-data="tableData"
		:searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions" :editFormFields="editFormFields"
		:editFormOptions="editFormOptions" @searchClick="loadData" @addClick="modelOpenBefore" :saveBefore="saveBefore"
		:saveAfter="saveAfter" :delBefore="delBefore">
		<!--表格配置 -->
		<vol-alert>
			<view>1. 生成代码后自定义列表颜色,更多配置见table示例</view>
			<view>2. 新建点添加或者行数据进入编辑页面也有配置</view>
			<view>3. 设置文本靠左显示</view>
		</vol-alert>
		<vol-table ref="tableRef" :ck="false" :index="true" :url="tableUrl" @rowClick="modelOpenBefore"
			:loadBefore="searchBefore" :loadAfter="searchAfter" :direction="direction" :titleField="table.titleField"
			:columns="columns" :table-data="tableData" :getStyle="getStyle" labelPosition="left">
		</vol-table>
	</vol-view>
</template>
<script setup>
	import options from "./Demo_OrderOptions.js";
	import {
		onLoad
	} from '@dcloudio/uni-app'
	import {
		ref,
		reactive,
		getCurrentInstance,
		defineEmits,
		defineExpose,
		defineProps,
		watch,
		nextTick
	} from "vue";
	const {
		proxy
	} = getCurrentInstance();
	//发起请求proxy.http.get/post
	//消息提示proxy.$toast()

	//表格显示方式:list=列表显示，horizontal=表格显示
	const direction = ref('list')

	//vol-view组件
	const viewRef = ref(null);
	//table组件
	const tableRef = ref(null);

	//表格数据，可以直接获取使用
	const tableData = ref([]);


	//编辑、查询、表格配置
	//要对table注册事件、格式化、按钮等，看移动端文档上的table示例配置
	//表单配置看移动端文档上的表单示例配置，searchFormOptions查询配置，editFormOptions编辑配置
	const {
		table,
		searchFormFields,
		searchFormOptions,
		editFormFields,
		editFormOptions,
		columns
	} = reactive(options());
	const tableUrl = ref('api/' + table.tableName + '/getPageData');

	//tabs点击事件
	const tabClick = (item) => {
		orderType.value = item.value;
		loadData();
	}

	//查询前方法，可以设置查询条件(与生成页面文档上的searchBefore配置一致)
	const searchBefore = (params) => {
		return true;
	}

	//查询后方法，res返回的查询结果
	const searchAfter = (res) => {
		nextTick(() => {
			viewRef.value.searchAfter(res);
		})
		return true;
	}

	//打开新建、编辑弹出框
	const modelOpenBefore = (row, index, obj, callback) => {
		//跳转到新页面编辑
		uni.navigateTo({
			url: "/pages/dbtest/Demo_Order/Demo_OrderEdit?id=" + ((row || {})[table.key] || ''),
			fail(e) {
				console.log(e)
			}
		})
	}

	//新建、编辑保存前
	const saveBefore = (formData, isAdd, callback) => {
		callback(true); //返回false，不会保存
	}

	//新建、编辑保存后
	const saveAfter = (res, isAdd) => {}

	//主表删除前方法
	const delBefore = (ids, rows, result) => {
		return true; //返回false不会执行删除
	}

	//调用表格查询
	const loadData = (params) => {
		//生成查询条件
		params = params || viewRef.value.getSearchParameters();
		//params可以设置查询条件
		tableRef.value.load(params);
	}

	//如果是其他页面跳转过来的，获取页面跳转参数
	onLoad((ops) => {})

	defineExpose({
		//对外暴露数据
	})

	/**************格式化配置部分***************/

	//格式化内容
	columns.forEach(x => {
		//设置字段的固定样式
		if (x.field == 'TotalPrice') {
			x.itemStyle = "background:rgb(232 244 255);"; //行样式
			x.titleStyle = "color:#007aff;"; //标题样式
			x.valueStyle = "color:red;"; //值样式
			x.extra = {
				text: "元",
				style: "font-size:26rpx;margin-left:6rpx;color: #6f6b6b;"
			}
		} else if (x.field == 'TotalQty') { //数量字段后面添加单位文本 
			x.extra = {
				text: "箱",
				style: "font-size:26rpx;margin-left:6rpx;color: #6f6b6b;"
			}
		}
	})

	//动态设置右边值的样式
	const getStyle = (fields, item, isValue) => {
		//isValue=true右边值的样式，false：左边标题样式，自己判断
		if (item.field == 'TotalQty') {
			//设置右边值的样式
			if (isValue) {
				if (fields[item.field] >= 300) {
					return 'color:red;font-weight:700;font-size:36rpx;'
				} else if (fields[item.field] >= 200) {
					return 'color:#40c400;font-weight:700;font-size:36rpx;'
				} else {
					return 'color:#007aff;font-weight:700;font-size:36rpx;'
				}
			}
			//设置左边标签样式,可以判断不同的值设置不同的标签颜色
			return 'color: #007aff;'
		}
	}
</script>
<style lang="less" scoped>
	.summary {
		padding: 20rpx 0;

		.txt {
			margin-left: 20rpx;
			font-size: 26rpx;
		}
	}
</style>