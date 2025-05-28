
<template>
	<vol-view ref="viewRef" :table="table" :columns="columns" :table-data="tableData"
		:searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions" :editFormFields="editFormFields"
		:editFormOptions="editFormOptions" @searchClick="loadData" @addClick="modelOpenBefore" :saveBefore="saveBefore"
		:saveAfter="saveAfter" :delBefore="delBefore">
		<!--增加删除按钮 -->
		<template #button>
			<view class="fx-center" @click="delClick" style="font-size: 26rpx;">
				<view style="position: relative;top:2rpx">
					<u-icon size="16" name="trash"></u-icon>
				</view>
				删除
			</view>
		</template>

		<!--表格配置 -->
		<view>
			<vol-alert>
				<view>点击单元格、行数据触发事件</view>
				<view>更多事件及按钮绑定，见自定义按钮页面文档</view>
			</vol-alert>
			<view class="btns">
				<view class="btn">
					<vol-button type="default" @click="()=>{direction='list'}">
						切换到列表显示
					</vol-button>
				</view>
				<view class="btn">
					<vol-button type="primary" @click="()=>{direction='horizontal'}">
						切换到表格显示
					</vol-button>
				</view>
			</view>
		</view>
		<vol-table ref="tableRef" :ck="true" :index="true" :url="tableUrl" :loadBefore="searchBefore"
			:loadAfter="searchAfter" :direction="direction" :titleField="table.titleField" :columns="columns"
			:table-data="tableData" :rowStyle="rowStyle" :columnStyle="columnStyle" @cellClick="cellClick"
			@rowClick="modelOpenBefore">
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


	//将template标签所有代码与下面这些配置添加到本地
	/**************自定义删除按钮位置***************/
	const delClick = () => {
		//获取选中的行
		const rows = tableRef.value.getSelectedRows();
		viewRef.value.del(rows)
	}

	/**************格式化配置部分***************/
	//设置整行背景颜色
	const rowStyle = (row, index, rows) => {
		if (row.OrderType == 3) {
			return {
				'background': 'rgb(197 229 255)'
			}
		}
	}
	//设置单元格背景颜色
	const columnStyle = (row, column, index) => {
		if (column.field == 'OrderNo') {
			return {
				'color': '#007aff'
			}
		}
	}

	//1.字段绑定click事件
	columns.forEach(col => {
		if (col.field == 'OrderNo') {
			col.click = true;
		}
	})
	//2.单元格点击事件
	const cellClick = (row, column, rowIndex, rows) => {
		//判断点击的哪个字段
		//注意上面columns字段配置要加上click:true
		if (column.field == 'OrderNo') {
			proxy.$toast('点击了[' + row[column.field] + ']');
		}
	}

	//(行点击事件)打开新建、编辑弹出框
	const modelOpenBefore = (row, index, obj, callback) => {
		proxy.$toast('点击了第[' + (index + 1) + ']行');
		//跳转到新页面编辑
		// uni.navigateTo({
		// 	url: "/pages/dbtest/Demo_Order/Demo_OrderEdit?id=" + ((row || {})[table.key] || ''),
		// 	fail(e) {
		// 		console.log(e)
		// 	}
		// })
	}
</script>
<style lang="less" scoped>
	.btns {
		padding: 20rpx;
	}
</style>