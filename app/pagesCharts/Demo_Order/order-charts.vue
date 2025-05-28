<template>
	<view class="charts-item" style="padding: 0;">
		<vol-alert type="primary">
			<view>1.生成页面增加自定义图表统计信息</view>
			<view>2.受小程序包大小限制,代码需放在pagesCharts目录下并且pages.json配置页面地址,分包处理</view>
		</vol-alert>
	</view>
	<view class="charts">
		<view class="charts-item">
			<view style="width:100%; height:400rpx">
				<l-echart ref="chartRef"></l-echart>
			</view>
		</view>
	</view>
</template>
<!-- 注意：微信小程序开发者工具上会出现层级过高问题，请在手机上预览或者发布 -->
<script setup>
	import {ref,onMounted,getCurrentInstance,nextTick} from 'vue'
	const {proxy} = getCurrentInstance();

	import * as echarts from '../echarts.esm.min.js'
	import {option} from './order-options.js'
	const chartRef = ref(null)
	let chartOption = option;

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
	const beginDate = proxy.base.addDays(dateNow, -3600)
	
	//图表史称 
	const source = {1: '销售订单',2: '采购订单',3: '退货订单',4: '促销订单',5: '预约订单'}
	//获取接口统计信息
	const initData = async () => {
		const url =
			`api/demo_order/getTotal?
			  beginDate=${beginDate}
			  &endDate=${dateNow}`
		proxy.http.get(url, {}, true).then((res) => {
			//生成图表数据
			const data = res.filter(x => {
				return x.orderType > 0
			}).map((x) => {
				return {
					name: source[x.orderType],
					value: x.totalPrice
				}
			})
			let total = (data.reduce((sum, itme) => sum + itme.value, 0) || 1).toFixed(2) * 1.0
			chartOption.title.text = '合计金额'
			chartOption.title.subtext = total
			chartOption.series[0].data = data
			nextTick(async () => {
				const myChart = await chartRef.value.init(echarts)
				myChart.setOption(chartOption)
			})
		})
	}
	onMounted(() => {
		initData()
	})
	
	defineExpose({
		initData
	})
	
</script>

<style scope lang="less">
	.charts {
		background: #fcfcfc;
		margin: 16rpx 0;

		.charts-item {
			margin: 20rpx;
			margin-top: 0;
			padding: 10rpx 18rpx;
			background: #fff;
			border-radius: 8rpx;
		}
	}
</style>