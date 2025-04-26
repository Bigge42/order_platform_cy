<template>
   <div class="vol-dashboard vol-dashboard-container">
      <vol-dashboard :readonly="false" @preview="preview" :id="route.query.id"></vol-dashboard>
   </div>
</template>
<script setup>
import {
   defineComponent,
   ref,
   reactive,
   toRefs,
   getCurrentInstance,
   onMounted
} from 'vue';
import { useRouter, useRoute } from 'vue-router';
// import VolDashboard from '@/components/VolDashboard/vol-dashboard.js'
const route = useRoute();
const { proxy } = getCurrentInstance();
// proxy.menu.hide();
onMounted(() => {
   if (document.body.clientWidth < 1600) {
      // document.getElementsByClassName('vol-dashboard-container')[0].style = 'zoom:0.80'
   }
})
const preview = (resultData, layout) => {
   proxy.$tabs.open({
      path: "/DashboardPreview",
      text: "工作台模板预览",
      query: {
         id: route.query.id
      }
   })
}
</script>



<style scoped lang="less">
.vol-dashboard {
   padding: 10px;
   position: absolute;
   width: 100%;
   height: 100%;
   background: #f9f9f9;
}

.vol-dashboard ::v-deep(.grid-layout-content) {
   margin: 0 !important;
}

.vol-dashboard ::v-deep(.grid-box) {
   padding: 0 !important;
}
</style>
