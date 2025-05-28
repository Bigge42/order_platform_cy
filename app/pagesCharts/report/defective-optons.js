export const option = {
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
		"padding": 10,
		"textStyle": {
			"fontSize": 12
		},
		"itemHeight": 12,
		"itemWidth": 12,
		"icon": "circle",
		"data": [
			"不良品数量",
			"生产工序不良率%"
		],
		"orient": "horizontal"
	},
	"grid": {
		"left": 40,
		"bottom": 30,
		"top": 40,
		"right": 40
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
			"show": false
		},
		"type": "category",
		"data": [
			"2024.01",
			"2024.02",
			"2024.03",
			"2024.04",
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
	"yAxis": [{
			"splitNumber": 3,
			"splitLine": {
				"show": false
			},
			"type": "value",
			"axisLabel": {
				"color": "#a7a7a7"
			},
			"data": null
		},
		{
			min: 0,
			max: 100,
			"splitLine": {
				"show": false
			},
			"type": "value",
			"axisLabel": {
				"color": "#a7a7a7"
			},
			"data": null
		}
	],
	"series": [{
			"name": "不良品数量",
			"barMaxWidth": 10,
			"yAxisIndex": 0,
			"type": "bar",
			"lineStyle": {
				"shadowOffsetX": 0,
				"shadowOffsetY": 6,
				"shadowBlur": 8,
				"shadowColor": "#e3d6fd"
			},
			"showSymbol": false,
			"label": {
				"show": false,
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
				360,
				260,
				450,
				700,
				230,
				550,
				400,
				340,
				480,
				120,
				220,
				190
			]
		},
		{
			"name": "生产工序不良率%",
			"yAxisIndex": 1,
			"type": "line",
			"label": {
				"show": false,
				"position": "top"
			},
			"emphasis": {
				"focus": "series"
			},
			"color": "#FF9800",
			"showBackground": false,
			"itemStyle": {
				"borderRadius": [
					0
				]
			},
			"data": [
				23, 56, 42, 37, 51, 18, 29, 49, 14, 32, 53, 47
			]
		}
	]
}
