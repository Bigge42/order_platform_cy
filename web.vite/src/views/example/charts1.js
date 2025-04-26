export default function(){
  return {
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
      subtext:0,// total,
      x: '49%',
      y: '45%',
      textStyle: {
        color: '#778CA2',
        fontSize: 12,
        fontWeight: '400'
      },
      subtextStyle: {
        color: 'rgba(0,0,0,0.85)',
        fontSize: 25,
        fontWeight: '700'
      },
      textAlign: 'center'
    },
    grid: {
      left: 40,
      bottom: 40,
      top: 40,
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
      bottom: 30
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
    series: [
      {
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
        data:[],// data,
        itemStyle: {
          normal: {
            shadowBlur: 20,
            shadowColor: '#85c1ffc4',
            shadowOffsetX: 5,
            shadowOffsetY: 5
          }
        }
      }
    ]
  }
}