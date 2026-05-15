<template>
  <view class="chart-card">
    <view v-if="title" class="chart-title">{{ title }}</view>
    <view v-if="description" class="chart-desc">{{ description }}</view>
    <view class="chart-box">
      <l-echart ref="chartRef"></l-echart>
    </view>
  </view>
</template>

<script setup>
import { nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import * as echarts from '@/pagesCharts/echarts.esm.min.js'

const props = defineProps({
  title: {
    type: String,
    default: ''
  },
  description: {
    type: String,
    default: ''
  },
  option: {
    type: Object,
    default: () => ({})
  }
})

const chartRef = ref(null)
let chart = null
let timer = null

const clearRenderTimer = () => {
  if (timer) {
    clearTimeout(timer)
    timer = null
  }
}

const renderChart = async () => {
  clearRenderTimer()
  timer = setTimeout(async() => {
    if (!chartRef.value || !props.option || !Object.keys(props.option).length) {
      return
    }

    await nextTick()
    if (!chart) {
      chart = await chartRef.value.init(echarts)
    }

    chart.setOption(props.option, true)
  }, 80)
}

watch(
  () => props.option,
  () => {
    renderChart()
  },
  {
    deep: true
  }
)

onMounted(() => {
  renderChart()
})

onBeforeUnmount(() => {
  clearRenderTimer()
  if (chart && typeof chart.dispose === 'function') {
    chart.dispose()
  }
  chart = null
})
</script>

<style scoped lang="less">
.chart-card {
  border-radius: 20rpx;
  background: #f8fbff;
  border: 1px solid #e2eefc;
  padding: 20rpx;
}

.chart-title {
  font-size: 28rpx;
  font-weight: 600;
  color: #17233d;
}

.chart-desc {
  margin-top: 10rpx;
  font-size: 24rpx;
  line-height: 1.5;
  color: #6b7a90;
}

.chart-box {
  width: 100%;
  height: 420rpx;
  margin-top: 16rpx;
}
</style>
