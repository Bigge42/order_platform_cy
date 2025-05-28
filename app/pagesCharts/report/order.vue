<template>
	<view class="charts">
		<view class="charts-item" style="padding: 0;">
			<vol-alert type="primary">
				<view>框架提供了查询、table组件,简单配置即可实现功能</view>
			</vol-alert>
		</view>
		<view class="search-item">
			<view class="search-item-filter">
				<vol-form ref="formRef" :form-options="searchFormOptions" :formFields="searchFormFields">
				</vol-form>
			</view>
			<view style="margin-left: 40rpx;">
				<vol-button type="primary" size="small" @click="initData">查询</vol-button>
			</view>
		</view>
		<view class="title">
			<vol-title title="销售情况"></vol-title>
		</view>
		<view class="charts-item" style="padding: 0;">
			<vol-table ref="tableRef" :rowStyle="rowStyle" :format="formatData" index :height='-1'
				direction="horizontal" :table-data="tableData" :columns="columns">
			</vol-table>
		</view>
		<view class="title">
			<vol-title title="销售汇总"></vol-title>
		</view>
		<view class="charts-item">
			<view style="width:100%; height:400rpx">
				<l-echart ref="chartRef"></l-echart>
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

	import * as echarts from '../echarts.esm.min.js'
	import {
		option
	} from './order-options.js'
	const chartRef = ref(null)
	let chartOption = option;

	//设置背景颜色
	const rowStyle = (row, index, rows) => {
		if (row.orderType == -1) {
			return {
				'background': 'rgb(253 244 230)'
			}
		}
	}
	//格式化单元格内容,这里目前不支持返回自定义标签，待开发
	const formatData = (row, column, index) => {
		if (column.field == 'totalPrice') {
			return '￥' + ((row.totalPrice + '').replace(/\B(?=(\d{3})+(?!\d))/g, ","))
		}
		return row[column.field]
	}

	//表格配置
	const tableData = ref([{
			name: '销售订单',
			orderType: 1,
			count: 0,
			totalPrice: 0,
			qty: 0
		},
		{
			name: '采购订单',
			orderType: 2,
			count: 0,
			totalPrice: 0,
			qty: 0
		},
		{
			name: '退货订单',
			orderType: 3,
			count: 0,
			totalPrice: 0,
			qty: 0
		},
		{
			name: '促销订单',
			orderType: 4,
			count: 0,
			totalPrice: 0,
			qty: 0
		},
		{
			name: '预约订单',
			orderType: 5,
			count: 0,
			totalPrice: 0,
			qty: 0
		},
		{
			name: '汇总合计',
			orderType: -1,
			count: 0,
			totalPrice: 0,
			qty: 0
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



	//配置查询条件
	const searchFormFields = ref({
		orderDate: [null, null]
	})
	const searchFormOptions = ref([{
		title: "订单日期：",
		align: "left",
		field: 'orderDate',
		range: true,
		type: 'date'
	}])

	//设置查询默认值
	const dateNow = proxy.base.getDate()
	searchFormFields.value.orderDate[0] = proxy.base.addDays(dateNow, -3600)
	searchFormFields.value.orderDate[1] = dateNow

	//获取接口统计信息
	const initData = async (showMsg) => {
		const url =
			`api/demo_order/getTotal?
			  beginDate=${searchFormFields.value.orderDate[0]}
			  &endDate=${searchFormFields.value.orderDate[1]}`
		proxy.http.get(url, {}, true).then((res) => {

			//生成表格数据
			tableData.value.forEach((item) => {
				res.forEach((c) => {
					if (c.orderType == item.orderType) {
						Object.assign(item, c)
					}
				})
			})
			//生成图表数据
			const data = tableData.value.filter(x => {
				return x.orderType != -1
			}).map((x) => {
				return {
					name: x.name,
					value: x.totalPrice
				}
			})
			let total = data.reduce((sum, itme) => sum + itme.value, 0) || 1
			total = total.toFixed(2) * 1.0
			chartOption.title.text = '合计金额'
			chartOption.title.subtext = total
			chartOption.series[0].data = data
			console.log(chartOption)
			// 组件能被调用必须是组件的节点已经被渲染到页面上
			nextTick(async () => {
				if (!chartRef.value) return
				const myChart = await chartRef.value.init(echarts)
				myChart.setOption(chartOption)
			})
		})
	}

	onMounted(() => {
		initData()
	})
</script>

<style scope lang="less">
	.charts {
		background: #fcfcfc;
		padding-top: 20rpx;
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
</style>
