export const option = {
	color: [
		'#7020ff',
		'#00d4f9',
		'#2EC7C9',
		'#c3a4ff',
		'#3281ff',
		'#a4dafe',
		'#B6A2DE',
		'#5AB1EF',
		'#FFB980',
		'#8D98B3'
	],
	tooltip: {
		trigger: 'item'
	},
	title: {
		show: true,
		text: '总金额',
		subtext: 0, // total,
		x: '49%',
		y: '42%',
		textStyle: {
			color: '#778CA2',
			fontSize: 10,
			fontWeight: '400'
		},
		subtextStyle: {
			color: 'rgba(0,0,0,0.85)',
			fontSize: 18,
			fontWeight: '700'
		},
		textAlign: 'center'
	},
	grid: {
		left: 40,
		bottom: 10,
		top: 10,
		right: 10
	},
	legend: {
		icon: 'circle',
		itemWidth: 8,
		itemHeight: 8,
		orient: 'horizontal',
		textStyle: {
			fontSize: 11
		},
		data: null,
		bottom:0
	},
	graphic: {
		type: 'text',
		left: 'center',
		top: 'center',
		style: {
			textAlign: 'center',
			fill: '#000',
			fontSize: 20
		}
	},
	series: [{
		type: 'pie',
		radius: ['41%', '60%'],
		center: ['50%', '50%'],
		avoidLabelOverlap: false,
		label: {
			show: true,
			position: 'outside'
		},
		emphasis: {
			label: {
				show: true,
				fontWeight: 'bold'
			}
		},
		labelLine: {
			show: true
		},
		data: [] // data,
	}]
}
