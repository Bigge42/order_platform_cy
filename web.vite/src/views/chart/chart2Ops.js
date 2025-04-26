import * as echarts from 'echarts';
export default {
  title: {
    text: '收支记录',
    textStyle: {
      fontSize: 16
    }
  },
  tooltip: {
    trigger: 'axis'
  },
  legend: {
    padding: 5,
    textStyle: {
      fontSize: 12
      // color: '#afe3ff'
    },
    itemHeight: 9,
    itemWidth: 12,
    icon: 'roundRect', // 'circle',
    data: ['收入', '支出']
  },
  xAxis: {
    show: true,
    axisTick: {
      show: false // 不显示坐标轴刻度线
    },
    axisLine: {
      show: false // 不显示坐标轴线
    },
    type: 'category',
    boundaryGap: false,
    data: [], // ['05-17', '05-18', '05-19', '05-20', '05-21', '05-22', '05-23'],
    axisLabel: {
      //y轴文字的配置
      textStyle: {
        color: '#a7a7a7',
        margin: 15
      }
    }
  },
  grid: {
    left: 50,
    bottom: 20,
    top: 40,
    right: 50
  },
  yAxis: {
    splitNumber: 3,
    splitLine: { show: false },
    type: 'value',
    axisLabel: {
      //y轴文字的配置
      textStyle: {
        color: '#a7a7a7',
        margin: 15
      }
    }
  },
  series: [
    {
      name: '收入',
      type: 'line',
      smooth: true,
      lineStyle: {
        // 阴影部分
        shadowOffsetX: 0, // 折线的X偏移
        shadowOffsetY: 6, // 折线的Y偏移
        shadowBlur: 8, // 折线模糊
        shadowColor: '#e3d6fd' //折线颜色
      },

      showSymbol: false,

      emphasis: {
        focus: 'series'
      },
      data: [
        30,
        765,
        456,
        697,
        23,
        564,
        400,
        345,
        478,
        123,
        45,
        789,
        231,
        654,
        98,
        34,
        56,
        78,
        192,
        321,
        645,
        700,
        213,
        546,
        600,
        312
      ]
    },
    {
      name: '支出',
      type: 'line',
      smooth: true,
      lineStyle: {
        // 阴影部分
        shadowOffsetX: 0, // 折线的X偏移
        shadowOffsetY: 7, // 折线的Y偏移
        shadowBlur: 8, // 折线模糊
        shadowColor: '#9fceff' //折线颜色
      },

      itemStyle: {
        color: '#2196F3'
      },
      showSymbol: false,

      emphasis: {
        focus: 'series'
      },
      data: [
        0,
        456,
        789,
        280,
        800,
        470,
        213,
        546,
        98,
        312,
        432,
        567,
        891,
        234,
        561,
        784,
        325,
        647,
        892,
        135,
        462,
        781,
        700,
        236,
        578,
        899
      ]
    }
  ]
};
