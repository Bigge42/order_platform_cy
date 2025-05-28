
<template>
	<vol-view ref="viewRef" :table="table" :columns="columns" :table-data="tableData"
		:searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions" :editFormFields="editFormFields"
		:editFormOptions="editFormOptions" @searchClick="loadData" @addClick="modelOpenBefore" :saveBefore="saveBefore"
		:saveAfter="saveAfter" :delBefore="delBefore">
		<!--表格配置 -->
		<vol-alert>生成代码后接着配置分组、分类快速查询</vol-alert>
	    <view style="background: #fff;">
			<u-tabs :current="orderType" @click="tabClick" :list="tabs"></u-tabs>
		</view>
		<vol-table ref="tableRef" :ck="false" :index="true" :url="tableUrl" @rowClick="modelOpenBefore"
			:loadBefore="searchBefore" :loadAfter="searchAfter" :direction="direction" :titleField="table.titleField"
			:columns="columns" :table-data="tableData">
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
	const direction = ref('horizontal')

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


    const orderType = ref('0')
	const tabs = ref([
		{value: "0",name: "全部订单"},
		{value: "1",name: "销售订单"},
		{value: "5",name: "预约订单"},
		{value: "2",name: "采购订单"},
		{value: "4",name: "促销订单"},
		{value: "3",name: "退货订单"}
	])
	
	//tabs点击事件
	const tabClick = (item) => {
		orderType.value = item.value;
		loadData();
	}

	//查询前方法，可以设置查询条件(与生成页面文档上的searchBefore配置一致)
	const searchBefore = (params) => {
		//为全部时不过滤条件
		if (orderType.value !== '0') {
			//设置表格的查询条件
			params.wheres = [{
				name: "OrderType",
				value: orderType.value
			}]
		}
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
</script>
<style lang="less" scoped>
	.summary{
		padding: 20rpx 0;
		.txt{
			margin-left: 20rpx;
			font-size: 26rpx;
		}
	}
</style>