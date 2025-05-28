<template>
	<view class="charts">
		<view style="padding: 0;">
			<vol-alert type="primary">
				<view>框架提供查询、table、表单组件,简单配置即可实现功能</view>
			</vol-alert>
			<view class=" search">
				<u-search @search="loadData" placeholder="请输订单编号" v-model="orderNo" @custom="loadData" @clear="loadData"
					:showAction="true" clearabled actionText="搜索"></u-search>
			</view>
		</view>
		<view>
			<view class="title">
				<vol-title title="table表格"></vol-title>
			</view>
			<view class="charts-item" style="padding: 0;">
				<vol-table ref="tableRef" :rowStyle="rowStyle" index :height='-1' direction="horizontal"
					:table-data="tableData" :columns="columns">
				</vol-table>
			</view>
		</view>
		<view>
			<view class="title">
				<vol-title title="tag"></vol-title>
			</view>
			<view class="charts-item" style="padding:20rpx;">
				<text style="margin-right: 20rpx;" class="vol-tag vol-tag-primary">tag-primary</text>
				<text style="margin-right: 20rpx;" class="vol-tag vol-tag-success">tag-success</text>
				<text style="margin-right: 20rpx;" class="vol-tag vol-tag-error">tag-error</text>
			</view>
		</view>
		<view>
			<view class="title">
				<vol-title title="button"></vol-title>
			</view>
			<view class="charts-item btns" style="padding:20rpx;">
				<view class="btn"><vol-button type="primary">primary</vol-button></view>
				<view class="btn"><vol-button type="success">success</vol-button></view>
				<view class="btn"><vol-button type="error">error</vol-button></view>
				<view class="btn"><vol-button type="default">default</vol-button></view>
			</view>
			<view class="charts-item btns" style="padding:20rpx;">
				<view class="btn"><vol-button size="small" type="primary">primary</vol-button></view>
				<view class="btn"><vol-button size="small"  type="success">success</vol-button></view>
				<view class="btn"><vol-button size="small"  type="error">error</vol-button></view>
				<view class="btn"><vol-button size="small"  type="default">default</vol-button></view>
			</view>
		</view>
		<view>
			<view class="title">
				<vol-title title="form表单"></vol-title>
			</view>
			<view class="charts-item" style="padding: 0;">
				<vol-form ref="formRef" :form-options="editFormOptions" :formFields="editFormFields">
				</vol-form>
			</view>
		</view>

		<view>
			<view class="title">
				<vol-title title="grid宫格"></vol-title>
			</view>
			<view class="charts-item" style="padding: 0;">
				<vol-grid :data="gridData" align="center" :col="4"></vol-grid>
			</view>
		</view>

		<view>
			<view class="title">
				<vol-title title="line进度条"></vol-title>
			</view>
			<view class="charts-item" style="padding: 0;">
				<vol-line :data="lineData"></vol-line>
			</view>
		</view>
	</view>
</template>
<!-- 注意：微信小程序开发者工具上会出现层级过高问题，请在手机上预览或者发布 -->
<script setup>
	import {
		ref,
		onMounted,
		getCurrentInstance,
		nextTick
	} from 'vue'

	const {
		proxy
	} = getCurrentInstance();

	const orderNo = ref();
	const search = () => {}

	const loadData = () => {

	}
	//设置背景颜色
	const rowStyle = (row, index, rows) => {
		if (row.OrderType == -1) {
			return {
				'background': '#daeef7'
			}
		}
	}

	//表格配置
	const tableData = ref([{
			name: '销售订单',
			count: 100,
			totalPrice: 3000,
			qty: 1000
		},
		{
			name: '采购订单',
			count: 200,
			totalPrice: 3500,
			qty: 1200
		},
		{
			name: '退货订单',
			count: 150,
			totalPrice: 4000,
			qty: 3000
		},
		{
			name: '汇总合计',
			orderType: -1,
			count: 3600,
			totalPrice: 9200,
			qty: 4500
		}
	])

	const columns = ref([{
		title: "订单类型",
		field: "name",
	}, {
		title: "订单数量",
		field: "count",
	}, {
		title: "订单单价",
		field: "qty",
	}, {
		title: "订单金额",
		field: "totalPrice",
		format: true, //设置启用格式化
		width: 110,
	}])

	const editFormFields = ref({
		processCode: "G202412080000001",
		processLine: "测试第二路线第一工序",
		materialAmount: 3000,
		jobAmount: 1200,
		cj: "(ICR)成品部",
		major: 1
	});
	const editFormOptions = ref([{
			type: "input",
			"title": "工序编号",
			"required": true,
			"readonly": true,
			"field": "processCode",
			align: 'left'
		},
		{
			type: "input",
			"title": "工艺路线",
			"readonly": true,
			"field": "processLine",
			align: 'left'
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
			data: [],
			dataKey: "enable"
		}]
	])


	const gridData = ref([{
			"title": "原料检验",
			"value": 2000
		},
		{
			"title": "外观检查",
			"value": 1500
		},
		{
			"title": "功能测试",
			"value": 2600
		},
		{
			"title": "耐久性测试",
			"value": 1800
		},
		{
			"title": "热处理工序",
			"value": 1900
		},
		{
			"title": "车间加工",
			"value": 2400
		},
		{
			"title": "车间装配",
			"value": 2300
		},
		{
			"title": "成品检验",
			"value": 3100
		}
	]);

	const lineData = [{
			title: "在制工单数",
			value: 1800,
			rate: 98,
			unit: '件'
		},
		{
			title: "在制工单计划生产数",
			value: 1500,
			rate: 80,
			unit: '件'
		},
		{
			title: "工制工单超期生产数",
			value: 300,
			color: '#c0dfff',
			unit: '件'
		},
		{
			title: "在制工序任务数",
			value: 1200,
			rate: 75,
			unit: '件'
		},
		{
			title: "在制工序计划生产数",
			value: 1000,
			unit: '件'
		},
		{
			title: "工制工序超期生产数",
			value: 500,
			color: '#c0dfff',
			unit: '件'
		}
	]
</script>

<style scope lang="less">
	.charts {
		background-color: #f3f3f3ad;
		height: 100%;
		overflow: auto;

		.search-item {
			display: flex;
			margin: 20rpx;
			padding-right: 20rpx;
			margin-top: 0;
			background: #fff;
			align-items: center;
			border-radius: 8rpx;

			.search-item-filter {
				flex: 1;
			}
		}

		.title {
			margin: 0 0 10rpx 26rpx;
		}

		.charts-item {
			margin: 20rpx;
			margin-top: 0;
			padding: 10rpx 18rpx;
			background: #fff;
			border-radius: 8rpx;
		}
	}

	.search {
		background: #ffff;
		padding: 20rpx;
	}
</style>