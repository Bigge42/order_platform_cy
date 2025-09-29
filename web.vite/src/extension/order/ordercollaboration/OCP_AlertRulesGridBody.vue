<template>
  <vol-box :lazy="true" v-model="model" title="预警规则执行日志" :width="1200" :padding="0">
    <div style="padding-bottom: 15px"><AlertRulesLog ref="log"></AlertRulesLog></div>
    <template #footer>
      <div>
        <el-button type="default" size="small" @click="model = false">关闭</el-button>
      </div>
    </template>
  </vol-box>
</template>
<script>
import VolBox from '@/components/basic/VolBox.vue'
import AlertRulesLog from '@/views/sys/quartz/Sys_QuartzLog'

export default {
  components: { 'vol-box': VolBox, AlertRulesLog },
  data() {
    return {
      model: false
    }
  },
  methods: {
    open() {
      this.model = true
      this.$nextTick(() => {
        // Sys_QuartzLog组件会自动从store中获取quartzId并过滤数据
        this.$refs.log.$refs.grid.search()
      })
    }
  }
}
</script>
