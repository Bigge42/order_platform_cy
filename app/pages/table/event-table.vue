<template>
	<view class="table-demo">
		<view style="padding:0">
			<vol-alert type="primary">
				<view>1、行点击事件、单元格点击事件、自动分页无限滚动</view>
				<view>2、数据加载事件、单元格颜色、背景颜色、搜索</view>
			</vol-alert>
		</view>
		<view class="search">
			<u-search @search="loadData" placeholder="请输名称" v-model="name" @custom="loadData" @clear="loadData"
				:showAction="true" clearabled actionText="搜索"></u-search>
		</view>
		<vol-table ref="tableRef" index url="api/Sys_Region/getPageData" :height='0' direction="horizontal"
			:table-data="tableData" :loadBefore="loadBefore" :loadAfter="loadAfter" :columns="columns"
			@cellClick="cellClick" @rowClick="rowClick" :columnStyle="columnStyle">
		</vol-table>
	</view>
</template>

<script setup>
	import {
		ref,
		defineProps,
		defineEmits,
		defineExpose,
		computed,
		getCurrentInstance,
		nextTick
	} from 'vue'

	const {
		proxy
	} = getCurrentInstance();

	const name = ref('');
	const tableRef = ref(null);
	//表格加载前设置条件
	const loadBefore = (params) => {
			console.log('数据加载后:loadBefore')
		params.wheres.push({
			name: "name",
			value: name.value,
			displayType: 'like'
		})
		return true;//返回false不会加载数据
	}
	//表格加载后方法
	const loadAfter = (res) => {
		console.log('数据加载后:loadAfter')
		return true;
	}
	const loadData = () => {
		//刷新表格
		tableRef.value.load()
	}
	//单元格点击事件
	const cellClick = (row, column, rowIndex, rows) => {
		//判断点击的哪个字段
		//注意下面columns字段配置要加上click:true
		if (column.field == 'code') {
			proxy.$toast('点击了[' + row[column.field] + ']');
		}
	}

	//行点击事件
	const rowClick = (row, rowIndex, rows) => {
		proxy.$toast('点击了第[' + (rowIndex + 1) + ']行');
	}

	const columnStyle = (row, column, rowIndex) => {
		//可根据row.字段判断值设置颜色
		if (column.field == 'code' && rowIndex % 2 == 1) {
			return {
				'color': '#007aff',
				'background': '#c0dfff'
			}
		}
	}

	const columns = ref([{
			field: 'id',
			title: 'id',
			type: 'int',
			width: 110,
			hidden: true
		},
		{
			field: 'code',
			title: '编码',
			type: 'string',
			width: 80,
			click: true
		},
		// {
		// 	field: '操作',
		// 	title: '操作',
		// 	width: 80,
		//     btn:true
		// },
		{
			field: 'name',
			title: '名称',
			type: 'string',
			width: 120,
		},
		{
			field: 'parentId',
			title: '上级编码',
			type: 'int',
			width: 80
		},
		{
			field: 'level',
			title: '级别',
			type: 'int',
			width: 50
		},
		{
			field: 'mername',
			title: '完整地址',
			type: 'string',
			width: 200
		},
		{
			field: 'Lng',
			title: '经度',
			type: 'float',
			width: 100
		},
		{
			field: 'Lat',
			title: '纬度',
			type: 'float',
			width: 100
		},
		{
			field: 'pinyin',
			title: '拼音',
			type: 'string',
			width: 150
		}
	]);
	const tableData = ref([]);
</script>

<style scoped lang="less">
	.table-demo {
		height: 100%;
		background: #fbfbfb;
		
	}
   .search {
		padding: 10px;
		background: #fff;
	}
</style>
