<template>
	<view class="table-demo">
		<view style="padding:0">
			<vol-alert type="primary">
				<view>1、显示复选框、行号，获取选中的行、删除行</view>
			</vol-alert>
		</view>
		<view class="btns">
			<view class="btn"><vol-button size="small" type="default" @click="getRows">获取选中行</vol-button></view>
			<view class="btn"><vol-button size="small" type="default" @click="setRows">设置选中行</vol-button></view>
			<view class="btn"><vol-button size="small" type="default" @click="delRows">删除选中行</vol-button></view>
			<view class="btn"><vol-button size="small" type="default" @click="clearRows">清除选中行</vol-button></view>
		</view>
		<view class="search">
			<u-search @search="loadData" placeholder="请输订单编号" v-model="orderNo" @custom="loadData" @clear="loadData"
				:showAction="true" clearabled actionText="搜索"></u-search>
		</view>
		<vol-table ref="tableRef" ck index url="api/demo_order/getPageData" :height='0' direction="horizontal"
			:table-data="tableData" :loadBefore="loadBefore" :loadAfter="loadAfter" :columns="columns"
			@ckChangeAll="ckChangeAll" @ckChange="ckChange">
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

	const orderNo = ref('')
	const tableRef = ref(null);

	//表格加载前设置条件
	const loadBefore = (params) => {
		//订单编号搜索
		params.wheres.push({
			name: "OrderNo",
			value: orderNo.value,
			displayType: 'like'
		})
		return true;
	}
	//表格加载后方法
	const loadAfter = (res) => {
		return true;
	}

	const loadData = () => {
		//刷新表格
		tableRef.value.load()
	}

	//获取选中行
	const getRows = () => {
		const rows = tableData.value.filter(x => {
			return x.ck
		});
		proxy.$toast(`当前选中[${rows.length}]条数据`)
	}
	//设置选中行
	const setRows = () => {
		tableData.value.forEach((row, index) => {
			if (index < 3) {
				row.ck = true;
			} else {
				row.ck = false;
			}
		})
		proxy.$toast(`设置了前3行数据选中`)
	}
	//删除选中的行
	const delRows = () => {
		let rows = tableRef.value.getSelectedRows();
		if (!rows.length) {
			proxy.$toast(`请选中要删除的行`)
			return;
		}
		uni.showModal({
			title: "确定要删除选中的行吗",
			success: (res) => {
				if (res.confirm) {
					rows = tableRef.value.delRows();
					//rows删除的行数据
					if (rows.length) {
						proxy.$toast(`删除了[${rows.length}]条数据`)
					}
				}
			}
		})
	}
	//清除选中的行
	const clearRows = () => {
		tableRef.value.clearSelectedRows();
		proxy.$toast(`清除成功`)
	}

	//全选操作
	const ckChangeAll = (rows, isChecked) => {
		proxy.$toast(isChecked ? '全选' : '取消全选')
		//获取选中的行
		//const rows = tableRef.value.getSelectedRows();
	}
	//单选选中操作
	const ckChange = (row, isChecked, index) => {
		if (isChecked) {
			proxy.$toast(`选中了第${index+1}行`)
			return
		}
		proxy.$toast(`取消选中第${index+1}行`)
		return
	}

	const columns = ref([{
			field: 'Order_Id',
			title: 'Order_Id',
			type: 'guid',
			hidden: true,
			readonly: true
		},
		{
			field: 'OrderType',
			title: '订单类型',
			type: 'select',
			bind: {
				key: '订单类型',
				data: []
			},
			width: 60
		},
		{
			field: 'TotalPrice',
			title: '总价',
			type: 'decimal',
			width: 30
		},
		{
			field: 'TotalQty',
			title: '总数量',
			type: 'int',
			width: 45
		},
		{
			field: 'OrderDate',
			title: '订单日期',
			type: 'date',
			width: 100
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
	}

	.btns {
		padding: 20rpx;
		padding-bottom: 0;
	}
</style>