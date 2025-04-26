<template>
   <div class="vol-dashboard">
      <vol-dashboard :readonly="true" :id="route.query.id"></vol-dashboard>
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
// import VolDashboard from '@/components/VolDashboard/vol-dashboard.js'
import { useRouter, useRoute } from 'vue-router';
const route = useRoute();
const { proxy } = getCurrentInstance();


</script>




<style scoped lang="less">
.vol-dashboard {
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
