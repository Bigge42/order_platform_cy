
<template>
	<vol-view ref="viewRef" :table="table" :columns="columns" :table-data="tableData"
		:searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions" :editFormFields="editFormFields"
		:editFormOptions="editFormOptions" @searchClick="loadData" @addClick="modelOpenBefore">
		<!--表格配置 -->
		<orderCharts ref="chartsRef"></orderCharts>
		<vol-table ref="tableRef" :min-height='200' :ck="false" :index="true" :url="tableUrl" @rowClick="modelOpenBefore"
			:loadAfter="searchAfter" :direction="direction" :columns="columns"
			:table-data="tableData">
		</vol-table>
	</vol-view>
</template>
<script setup>
	import orderCharts from '@/pagesCharts/Demo_Order/order-charts.vue'
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
		nextTick,
		onMounted
	} from "vue";
	const {
		proxy
	} = getCurrentInstance();

	//表格显示方式:list=列表显示，horizontal=表格显示
	const direction = ref('horizontal')
	//vol-view组件
	const viewRef = ref(null);
	//table组件
	const tableRef = ref(null);
	//表格数据，可以直接获取使用
	const tableData = ref([]);
	
	const chartsRef=ref()

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

	//查询后方法，res返回的查询结果
	const searchAfter = (res) => {
		nextTick(() => {
			viewRef.value.searchAfter(res);
			//每次查询可以重新加载图表数据
			//chartsRef.value.initData();
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

	//调用表格查询
	const loadData = (params) => {
		//生成查询条件
		params = params || viewRef.value.getSearchParameters();
		//params可以设置查询条件
		tableRef.value.load(params);
	}
	
	//调用加载图表数据
	// onMounted(()=>{
	// 	chartsRef.value.initData();
	// })
	
</script>
<style lang="less" scoped>
</style>