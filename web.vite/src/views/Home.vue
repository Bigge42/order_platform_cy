<template>
  <!-- <div class="message-center-page"> -->
  <!-- Tab切换按钮 -->
  <!-- <div class="tab-switch-container">
      <el-button
        :type="activeTab === 'reminder' ? 'primary' : ''"
        @click="switchTab('reminder')"
        class="tab-button"
      >
        催单消息 -->
  <!-- </el-button>
      <el-button
        :type="activeTab === 'negotiate' ? 'primary' : ''"
        @click="switchTab('negotiate')"
        class="tab-button"
      > -->
  协商消息
  <!-- </el-button>
    </div> -->

  <!-- 消息内容 -->
  <!-- <div class="message-content"> -->
  <!-- 催单消息内容 -->
  <!-- <div v-if="activeTab === 'reminder'" class="tab-content">
        <UrgentOrderMessage ref="reminderRef" :key="reminderKey" />
      </div> -->

  <!-- 协商消息内容 -->
  <!-- <div v-if="activeTab === 'negotiate'" class="tab-content">
        <NegotiationMessage ref="negotiateRef" :key="negotiateKey" />
      </div>
    </div>
  </div> -->
</template>

<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'

// 路由实例
const router = useRouter()

// 页面挂载时重定向到 DashboardPreview
onMounted(() => {
  router.push('/DashboardPreview?id=8cd64d47-cb9c-463c-8b62-ea69e7416569')
})

// import { ref, nextTick } from 'vue'
// import UrgentOrderMessage from '@/views/order/ordercollaboration/OCP_UrgentOrder.vue'
// import NegotiationMessage from '@/views/order/ordercollaboration/OCP_Negotiation.vue'

// // 当前激活的tab
// const activeTab = ref('reminder')

// // 组件引用
// const reminderRef = ref(null)
// const negotiateRef = ref(null)

// // 强制刷新组件的key
// const reminderKey = ref(0)
// const negotiateKey = ref(0)

// // 处理tab切换
// const switchTab = async (tabName) => {
//   // 更新activeTab值
//   activeTab.value = tabName

//   // 等待下一个tick确保组件已经渲染
//   await nextTick()

//   // 根据当前tab刷新对应组件的数据
//   if (tabName === 'reminder' && reminderRef.value) {
//     // 如果催单组件有刷新方法，调用它
//     if (typeof reminderRef.value.refreshData === 'function') {
//       reminderRef.value.refreshData()
//     }
//   } else if (tabName === 'negotiate' && negotiateRef.value) {
//     // 如果协商组件有刷新方法，调用它
//     if (typeof negotiateRef.value.refreshData === 'function') {
//       negotiateRef.value.refreshData()
//     }
//   }
// }

// // 刷新当前激活的tab内容
// const refreshCurrentTab = () => {
//   if (activeTab.value === 'reminder') {
//     reminderKey.value += 1
//   } else if (activeTab.value === 'negotiate') {
//     negotiateKey.value += 1
//   }
// }

// // 暴露方法给父组件
// defineExpose({
//   refreshCurrentTab,
//   switchToTab: switchTab
// })
</script>

<style lang="less">
.message-center-page {
  width: 100%;
  height: calc(100% - 230px);
  display: flex;
  flex-direction: column;
}

.tab-switch-container {
  padding: 12px 24px;
  border-bottom: 1px solid #e8e8e8;
  background: #fff;
}

.tab-button {
  margin-right: 8px;
}

.message-content {
  flex: 1;
  overflow: hidden;
  height: calc(100% - 65px);
}

.tab-content {
  height: 100%;
  overflow: hidden;
  flex-direction: column;
}
</style>
