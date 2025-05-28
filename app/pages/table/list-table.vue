<template>
	<view class="table-demo">
		<view style="padding:0">
			<vol-alert type="primary">
				<view>以列表形式显示,可在slot数据槽自定义每行头与底部信息<view>设置titleField固定表头字段</view>
				</view>
			</vol-alert>
		</view>
		<vol-table :readonly="false" direction="list" titleField="OrderNo" :table-data="tableData" :columns="columns">
			<template #header="scope">
				<view style="flex: 1;text-align: right;font-weight: 400;font-size: 26rpx;">
					slot header，第[{{scope.data.rowIndex+1}}]行</view>
			</template>

			<template #bottom="scope">
				<!--获取当前行数据 	{{scope.data.row}} -->
				<!-- 获取行号	{{scope.data.rowIndex}} -->
				<view class="fx buttons" style="margin-bottom: 20rpx;">
		<!-- 			<view>slotbottom位置</view> -->
					<view class="btn">
						<vol-button size="small" shape="circle" @click="rowBtnClick(scope.data.row,scope.data.rowIndex)"
							type="default" :plain="true">查看</vol-button>
					</view>
					<view class="btn">
						<vol-button size="small" shape="circle" @click="rowBtnClick(scope.data.row,scope.data.rowIndex)"
							type="primary" :plain="true" >签收</vol-button>
					</view>
				</view>
			</template>
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

	//调用接口
	//proxy.http

	const rowBtnClick = (row, index) => {
		proxy.$toast('点击了第' + (index + 1) + '行按钮')
		console.log(row)
	}
	const columns = ref([{
			field: 'Order_Id',
			title: 'Order_Id',
			type: 'guid',
			width: 110,
			hidden: true,
			readonly: true
		},
		{
			field: 'OrderNo',
			title: '订单编号',
			type: 'string',
			link: true,
			width: 130,
			readonly: true,
			sort: true
		},
		{
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
			title: '总价',
			type: 'decimal',
			width: 70
		},
		{
			field: 'TotalQty',
			title: '总数量',
			type: 'int',
			width: 80
		},
		{
			field: 'OrderDate',
			title: '订单日期',
			type: 'date',
			width: 95,
			sort: true
		},
		{
			field: 'Remark',
			title: '备注',
			type: 'string',
			width: 100,
			hidden: true
		},
		{
			field: 'CreateID',
			title: 'CreateID',
			type: 'int',
			width: 80,
			hidden: true
		},
		{
			field: 'Creator',
			title: '创建人',
			type: 'string',
			width: 80
		},
		{
			field: 'CreateDate',
			title: '创建时间',
			type: 'datetime',
			width: 145,
			sort: true
		}
	]);
	const tableData = ref([{
			"Order_Id": "42733f34-9943-4670-a234-0d3cf65ca90a",
			"OrderNo": "P-20241114-000005",
			"OrderType": 5,
			"TotalPrice": 50089.00,
			"TotalQty": 150,
			"OrderDate": "2024-07-25 00:00:00",
			"CustomerId": null,
			"Customer": "王语嫣",
			"PhoneNo": "18612349000",
			"OrderStatus": 3,
			"Remark": null,
			"Creator": "超级管理员",
			"CreateDate": "2024-11-14 03:12:04"
		},
		{
			"Order_Id": "7f2f6405-671e-4c05-9ddf-79175e272283",
			"OrderNo": "P-20241114-000004",
			"OrderType": 3,
			"TotalPrice": 0.00,
			"TotalQty": 0,
			"OrderDate": "2024-07-25 00:00:00",
			"CustomerId": null,
			"Customer": "王语嫣",
			"PhoneNo": "18612349000",
			"OrderStatus": 1,
			"Remark": null,
			"Creator": "超级管理员",
			"CreateDate": "2024-11-14 03:11:40"
		},
		{
			"Order_Id": "794c4fe8-1c06-4216-9871-9b86873c663a",
			"OrderNo": "P-20241114-000003",
			"OrderType": 3,
			"TotalPrice": 3500.00,
			"TotalQty": 200,
			"OrderDate": "2024-07-25 00:00:00",
			"CustomerId": null,
			"Customer": "王语嫣",
			"PhoneNo": "18612349000",
			"OrderStatus": 1,
			"Remark": null,
			"Creator": "超级管理员",
			"CreateDate": "2024-11-14 03:11:24"
		},
		{
			"Order_Id": "157dff42-2401-4540-9dc5-09c20a428e9e",
			"OrderNo": "P-20241114-000002",
			"OrderType": 4,
			"TotalPrice": 2700.00,
			"TotalQty": 100,
			"OrderDate": "2024-07-25 00:00:00",
			"CustomerId": null,
			"Customer": "王语嫣",
			"PhoneNo": "18612349000",
			"OrderStatus": 1,
			"Remark": null,
			"Creator": "超级管理员",
			"CreateDate": "2024-11-14 03:11:04"
		},
		{
			"Order_Id": "42924cfd-4fac-4fa3-a2bc-68ae82ed8bb9",
			"OrderNo": "P-20241114-000001",
			"OrderType": 4,
			"TotalPrice": 200.00,
			"TotalQty": 300,
			"OrderDate": "2024-07-25 00:00:00",
			"CustomerId": null,
			"Customer": "王语嫣",
			"PhoneNo": "18612349000",
			"OrderStatus": 1,
			"Remark": null,
			"Creator": "超级管理员",
			"CreateDate": "2024-11-14 03:10:50"
		},
		{
			"Order_Id": "31d99491-9c0d-45ac-ad38-9b08f84fef29",
			"OrderNo": "P-20241113-000001",
			"OrderType": 2,
			"TotalPrice": 500.00,
			"TotalQty": 100,
			"OrderDate": "2024-07-25 00:00:00",
			"CustomerId": null,
			"Customer": "王语嫣",
			"PhoneNo": "18612349000",
			"OrderStatus": 1,
			"Remark": null,
			"Creator": "超级管理员",
			"CreateDate": "2024-11-13 09:47:33"
		}
	]);
</script>

<style scoped lang="less">
	.table-demo {
		height: 100%;
		background: #fbfbfb;
	}

	.item-title {
		font-weight: bold;
		padding: 3px 0 6px 0;
		border-bottom: 1px solid #eee;
		display: flex;
		line-height: 1;
		margin-bottom: 8rpx;

		.border-name {
			font-weight: bold;
			background: #007bff;
			padding-left: 8rpx;
			font-size: 16px;
			border-radius: 8rpx;
			margin-right: 8rpx;
		}
	}

	.buttons {
		align-items: baseline;
		justify-content: flex-end;
		padding-top: 12rpx;
		font-size: 26rpx;

		.btn {
			margin-left: 20rpx;
		}
	}
</style>
