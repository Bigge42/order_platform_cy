<template>
  <div class="demo-tabs">
    <el-tabs v-model="activeName" @tab-click="handleClick">
      <el-tab-pane v-for="(item, index) in tabs" :name="item.orderType" :key="index">
        <template #label>
          <div class="custom-tabs-label">
            <el-icon><calendar /></el-icon>
            <span>{{ item.name }}</span>
            <span v-if="item.count" class="item-qty">{{ item.count }}</span>
          </div>
        </template>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>
<script setup>
import { ref, reactive,   getCurrentInstance } from "vue";

const emit = defineEmits(["parentCall"]);

const { proxy } = getCurrentInstance();

//默认选中项
const activeName = ref(-1);
const tabs = reactive([
  { name: "全部订单", icon: "", orderType: -1, count: 0 },
  { name: "销售订单", icon: "", orderType: 1, count: 0 },
  { name: "采购订单", icon: "", orderType: 2, count: 0 },
  { name: "退货订单", icon: "", orderType: 3, count: 0 },
  { name: "促销订单", icon: "", orderType: 4, count: 0 },
  { name: "预约订单", icon: "", orderType: 5, count: 0 },
]);

//获取接口统计信息
const initData = () => {
  proxy.http.get("api/demo_order/getTotal", {}, false).then((res) => {
    tabs.forEach((item) => {
      res.forEach((c) => {
        if (c.orderType == item.orderType) {
          item.count = c.count;
        }
      });
    });
  });
};

initData();

const handleClick = (item, event) => {
  emit("parentCall", ($parent) => {
    const orderType = tabs[item.index].orderType;
    //点击tabs刷新表格，并传入查询参数
    let params = {};
    //全部不传条件
    if (orderType !== -1) {
      params.wheres = [{ name: "OrderType", value: orderType }];
    }
    $parent.search(params, true);
  });
};

defineExpose({
  tabs,
  initData,
});
</script>

<style lang="less" scoped>
.demo-tabs {
  padding: 10px 15px 0 15px;
}
.custom-tabs-label {
  display: flex;
  position: relative;
  align-items: center;
  i {
    margin-right: 3px;
  }
  .item-qty {
    position: absolute;
    background: #f21f0f;
    color: #fff;
    height: 18px;
    width: 18px;
    border-radius: 50%;
    font-size: 10px;
    line-height: 20px;
    text-align: center;
    right: -19px;
    top: -6px;
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
