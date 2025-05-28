<template>
	<view class="charts">
		  <view class="charts-item">
			  <view style="width:100%; height:400rpx">
			  	<l-echart ref="chartRef"></l-echart>
			  </view>
		  </view>
	</view>
</template>
<script setup>
	import {
		ref,
		onMounted
	} from 'vue'
	import * as echarts from '../echarts.esm.min.js'
	const chartRef = ref(null)
	const option = {
		"title": {
			"text": "",
			"textStyle": {
				"fontSize": 16
			}
		},
		"tooltip": {
			"trigger": "axis"
		},
		"legend": {
			"padding": 5,
			"textStyle": {
				"fontSize": 12
			},
			"itemHeight": 12,
			"itemWidth": 12,
			"icon": "circle",
			"data": [
				"入库数量",
				"出库数量",
				"合计"
			],
			"orient": "horizontal"
		},
		"grid": {
			"left": 40,
			"bottom": 20,
			"top": 40,
			"right": 10
		},
		"xAxis": {
			"show": true,
			"axisTick": {
				"show": false
			},
			"axisLine": {
				"show": false
			},
			"splitLine": {
				"show": true
			},
			"type": "category",
			"data": [
				"2024.05",
				"2024.06",
				"2024.07",
				"2024.08",
				"2024.09",
				"2024.10",
				"2024.11",
				"2024.12"
			],
			"color": "#a7a7a7"
		},
		"yAxis": {
			"splitNumber": 3,
			"splitLine": {
				"show": true
			},
			"type": "value",
			"axisLabel": {
				"color": "#a7a7a7"
			},
			"data": null
		},
		"series": [{
				"name": "入库数量",
				"type": "bar",
				"smooth": true,
				"lineStyle": {
					"shadowOffsetX": 0,
					"shadowOffsetY": 6,
					"shadowBlur": 8,
					"shadowColor": "#e3d6fd"
				},
				"stack": "total",
				"showSymbol": true,
				"label": {
					"show": true,
					"position": "top"
				},
				"emphasis": {
					"focus": "series"
				},
				"showBackground": false,
				"itemStyle": {
					"borderRadius": [
						0
					]
				},
				"color": "#2196F3",
				"data": [
					30,
					760,
					450,
					700,
					23,
					550,
					400,
					340,
					480,
					120,
					20,
					0
				]
			},
			{
				"name": "出库数量",
				"type": "bar",
				"smooth": true,
				"lineStyle": {
					"shadowOffsetX": 0,
					"shadowOffsetY": 7,
					"shadowBlur": 8,
					"shadowColor": "#9fceff"
				},
				"stack": "total",
				"itemStyle": {
					"borderRadius": [
						0
					]
				},
				"color": "#5470c6",
				"showSymbol": true,
				"label": {
					"show": true,
					"position": "top"
				},
				"emphasis": {
					"focus": "series"
				},
				"showBackground": false,
				"data": [
					0,
					560,
					789,
					280,
					800,
					470,
					210,
					545,
					100,
					310,
					230,
					50
				]
			},
			{
				"name": "合计",
				"type": "bar",
				"smooth": true,
				"lineStyle": {
					"shadowOffsetX": 0,
					"shadowOffsetY": 6,
					"shadowBlur": 8,
					"shadowColor": "#e3d6fd"
				},
				"stack": "total",
				"showSymbol": true,
				"label": {
					"show": true,
					"position": "top"
				},
				"emphasis": {
					"focus": "series"
				},
				"color": "#00d4f9",
				"showBackground": false,
				"itemStyle": {
					"borderRadius": [
						0
					]
				},
				"data": [
					30,
					1320,
					1239,
					980,
					823,
					1020,
					610,
					885,
					580,
					430,
					250,
					50
				]
			}
		]
	}


	onMounted(() => {
		// 组件能被调用必须是组件的节点已经被渲染到页面上
		setTimeout(async () => {
			if (!chartRef.value) return
			const myChart = await chartRef.value.init(echarts)
			console.log(myChart)
			myChart.setOption(option)
		}, 300)
	})
</script>

<style scope lang="less">
	.charts{
		background: #f5f5f5;
		padding-top: 20rpx;
		height: 100%;
		.charts-item{
			margin: 20rpx;
			margin-top: 0;
			padding: 26rpx;
			background: #fff;
			border-radius: 8rpx;
		}
	}
</style>