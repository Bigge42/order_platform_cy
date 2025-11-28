<template>
  <div v-if="watermarkUrl" class="watermark-layer" :style="layerStyle"></div>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useStore } from 'vuex'

const store = useStore()
const watermarkUrl = ref('')

const userName = computed(() => (store.getters.getUserName ? store.getters.getUserName() : ''))
const loginName = computed(() => (store.getters.getLoginName ? store.getters.getLoginName() : ''))
const isLogin = computed(() => (store.getters.isLogin ? store.getters.isLogin() : false))

const layerStyle = computed(() => ({
  backgroundImage: watermarkUrl.value ? `url(${watermarkUrl.value})` : '',
  backgroundRepeat: 'repeat',
  backgroundSize: '260px 200px'
}))

const renderWatermark = () => {
  if (typeof window === 'undefined') return
  if (!isLogin.value) {
    watermarkUrl.value = ''
    return
  }
  const canvas = document.createElement('canvas')
  const ratio = window.devicePixelRatio || 1
  const width = 260
  const height = 200
  canvas.width = width * ratio
  canvas.height = height * ratio
  canvas.style.width = width + 'px'
  canvas.style.height = height + 'px'
  const ctx = canvas.getContext('2d')
  ctx.scale(ratio, ratio)
  ctx.clearRect(0, 0, width, height)
  ctx.globalAlpha = 0.08
  ctx.translate(width / 2, height / 2)
  ctx.rotate((-20 * Math.PI) / 180)
  ctx.textAlign = 'center'
  ctx.fillStyle = '#000'
  ctx.font = '16px Microsoft YaHei, sans-serif'
  const lines = [
    userName.value || '',
    loginName.value || '',
    new Date().toLocaleString()
  ]
  lines.forEach((text, idx) => {
    ctx.fillText(text, 0, idx * 22)
  })
  watermarkUrl.value = canvas.toDataURL('image/png')
}

onMounted(() => {
  renderWatermark()
  window.addEventListener('resize', renderWatermark)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', renderWatermark)
})

watch([userName, loginName, isLogin], renderWatermark)
</script>

<style scoped>
.watermark-layer {
  position: fixed;
  inset: 0;
  z-index: 9999;
  pointer-events: none;
}
</style>
