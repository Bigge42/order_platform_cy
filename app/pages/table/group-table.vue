<template>
	<view class="table-demo">
		<view style="padding:0">
			<vol-alert type="primary">
				<view>1、表格自动分页、数据字典自动转换、api查询数据</view>
				<view>2、多字段分组显示,只需简单配置可实现大量功能</view>
			</vol-alert>
		</view>
		<view class="search">
			<u-search @search="loadData" placeholder="请输订单编号" v-model="orderNo" @custom="loadData" @clear="loadData"
				:showAction="true" clearabled actionText="搜索"></u-search>
		</view>
		<view style="background: #fff;">
			<u-tabs :current="orderType" @click="tabClick" :list="tabs"></u-tabs>
		</view>
		<view class="table-content">
			<vol-table ref="tableRef" index url="api/demo_order/getPageData" :height='0' direction="list"
				titleField="OrderNo" :table-data="tableData" :loadBefore="loadBefore" :loadAfter="loadAfter"
				:columns="columns" :labelWidth="70">
				<template #header="scope">
					<view class="scope-header">
						<view class="btns">
							<vol-button size="small" type="primary"
								@click="rowBtnClick(scope.data.row,scope.data.rowIndex)">按钮[{{scope.data.rowIndex+1}}]
							</vol-button>
						</view>
					</view>
				</template>
			</vol-table>
		</view>
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

	const orderType = ref('0')
	const tabs = ref([{
			value: "0",
			name: "全部订单"
		},
		{
			value: "1",
			name: "销售订单"
		},
		{
			value: "5",
			name: "预约订单"
		},
		{
			value: "2",
			name: "采购订单"
		},
		{
			value: "4",
			name: "促销订单"
		},
		{
			value: "3",
			name: "退货订单"
		}
	])

	const orderNo = ref('')
	const tableRef = ref(null);

	//表格加载前设置条件
	const loadBefore = (params) => {
		//为全部时不过滤条件
		if (orderType.value !== '0') {
			//设置表格的查询条件
			params.wheres = [{
				name: "OrderType",
				value: orderType.value
			}]
		}
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
	//tabs点击事件
	const tabClick = (item) => {
		orderType.value = item.value;
		loadData();
	}
	const loadData = () => {
		//刷新表格
		tableRef.value.load()
	}


	const rowBtnClick = (row, index) => {
		proxy.$toast('点击了第[' + (index + 1) + ']行')
	}



	const columns = ref([{
			field: 'Order_Id',
			title: 'Order_Id',
			type: 'guid',
			hidden: true,
			readonly: true
		},
		{
			field: 'OrderNo',
			title: '订单编号',
			type: 'string',
			link: true,
			readonly: true,
			sort: true
		},
		[{
				field: 'OrderType',
				title: '订单类型',
				type: 'select',
				bind: {
					key: '订单类型',
					data: []
				},
				width: 90
			},
			{
				field: 'TotalPrice',
				title: '订单总价',
				type: 'decimal',
				width: 70
			}
		],
		[{
				field: 'TotalQty',
				title: '物料数量',
				type: 'int',
				width: 80
			},
			{
				field: 'OrderDate',
				title: '订单日期',
				type: 'date',
				width: 140
			}
		],
		[{
			field: 'Customer',
			title: '客户名称',
			type: 'string',
			width: 120
		}, {
			field: 'Creator',
			title: '创建人',
			type: 'string',
			width: 120
		}],
		{
			field: 'CreateDate',
			title: '创建时间',
			type: 'datetime',
			align: 'left', //居左边显示
			width: 190
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

	.scope-header {
		flex: 1;
		text-align: right;
		font-weight: 400;
		font-size: 26rpx;
		display: flex;
		align-items: center;
		justify-content: flex-end;

		.btns {
			margin-bottom: 10rpx;
		}
	}
</style>
