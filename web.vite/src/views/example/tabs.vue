<template>
  <div class="demo-tabs">

    <div class="demo-content">
      
      <el-tabs v-model="activeName" @tab-click="handleClick">
        <el-tab-pane v-for="(item, index) in tabs" :name="item.component" :key="index">
          <template #label>
            <div class="custom-tabs-label">
              <el-icon><calendar /></el-icon>
              <span>{{ item.name }}</span>
            </div>
          </template>
        </el-tab-pane>
      </el-tabs>
      <el-alert style="margin-top: 10px;" title="使用高性能动态组件异步加载多个页面数据" type="success"  :closable="false" show-icon />
      <keep-alive>
        <component ref="com" :is="activeName"></component>
      </keep-alive>
    </div>
  </div>
</template>
<script>

/*
   实现方式说明
     1、引用生成的vue页面
     2、打开每个生成的页面表.jsx文件，填写taleAction属性(对应实际表，用于权限判断)，onInited同时减去上面tabs的高度
     3、可以打开下面三张表jsx文件找到taleAction及onInited重新设置表格高度属性示例
*/
import {
  ref,
  reactive,
  
  markRaw,
  nextTick,
  
  getCurrentInstance,
  defineAsyncComponent
} from 'vue'

import customer from '@/views/dbtest/customer/Demo_Customer.vue'

export default {
  components: {
    customer,
    //异步引用组件
    order: defineAsyncComponent(() => import('@/views/dbtest/order/Demo_OrderStats.vue')),
   catalog: defineAsyncComponent(() => import('@/views/dbtest/catalog/Demo_Catalog.vue'))
  },
  setup() {
    const activeName = ref('customer')
    const tabs = reactive([
      { name: '客户管理', component: 'customer' },
      { name: '订单管理', component: 'order' },
      { name: '基础商品', component: 'catalog' }
    ])

    const handleClick = (item, event) => {
      activeName.value = tabs[item.index].component
    }

    const com=ref()
    return {
      tabs,
      activeName,
      handleClick,
      com
    }
  }
}
</script>

<style lang="less" scoped>
.demo-tabs {
   background: #f3f7fb;
  padding: 15px;
  position: absolute;
  width: 100%;
  height: 100%;
  .demo-content{
    height: 100%;
    padding: 0 10px;
    background: #fff;
  }
  ::v-deep(.grid-container) {
    padding: 0 !important;
  }
  ::v-deep(.view-header),
  ::v-deep(.demo-list) {
    padding-left: 0 !important;
    padding-right: 0 !important;
  }
}
.custom-tabs-label {
  display: flex;
  position: relative;
  align-items: center;
  i {
    margin-right: 3px;
  }
}
.demo-tabs ::v-deep(.el-tabs__nav-wrap:after),
.demo-tabs ::v-deep(.el-tabs__active-bar) {
  height: 1px;
}
.demo-tabs ::v-deep(.el-tabs__header) {
  margin: 0px;
}
</style>
