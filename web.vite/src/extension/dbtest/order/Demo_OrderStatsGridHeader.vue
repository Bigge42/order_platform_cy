<template>
  <div class="demo-list">
    <div
      class="list-item"
      @click="itemClick(item, index)"
      v-for="(item, index) in tabs"
      :key="index"
    >
      <div class="content">
        <div class="content-right">
          <div class="name">
            {{ item.name }}
          </div>
          <div class="data">
            <span class="rmb">¥</span>
            {{ ((item.totalPrice || 0) + "").replace(/\B(?=(\d{3})+(?!\d))/g, ",") }}
          </div>
        </div>
        <div class="f-icon">
          <el-icon>
            <MessageBox />
          </el-icon>
        </div>
      </div>
      <div class="content-bottom">
        <div class="num-01">数量：{{ item.qty }}</div>
        <div class="num-01">金额：{{ item.totalPrice }}</div>
      </div>
    </div>
  </div>
</template>
<script setup>
import { ref, reactive, getCurrentInstance } from "vue";

const emit = defineEmits(["parentCall"]);

const { proxy } = getCurrentInstance();

//默认选中项
const activeName = ref(-1);
const tabs = reactive([
  // { name: '全部订单', icon: '', orderType: -1, count: 0, totalPrice: 0, qty: 0 },
  { name: "销售订单", icon: "", orderType: 1, count: 0, totalPrice: 0, qty: 0 },
  { name: "采购订单", icon: "", orderType: 2, count: 0, totalPrice: 0, qty: 0 },
  { name: "退货订单", icon: "", orderType: 3, count: 0, totalPrice: 0, qty: 0 },
  { name: "促销订单", icon: "", orderType: 4, count: 0, totalPrice: 0, qty: 0 },
  { name: "预约订单", icon: "", orderType: 5, count: 0, totalPrice: 0, qty: 0 },
]);

//获取接口统计信息
const initData = () => {
  proxy.http.get("api/demo_order/getTotal", {}, false).then((res) => {
    tabs.forEach((item) => {
      res.forEach((c) => {
        if (c.orderType == item.orderType) {
          item.count = c.count;
          item.totalPrice = c.totalPrice;
          item.qty = c.qty;
        }
      });
    });
  });
};

initData();

const itemClick = (item) => {
  emit("parentCall", ($parent) => {
    const orderType = item.orderType;
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
.demo-list {
  padding: 15px 15px 0 15px;
  display: grid;
  -moz-column-gap: 12px;
  column-gap: 12px;
  //上面汇总显示的数量
  grid-template-columns: repeat(5, auto);
}

.list-item {
  position: relative;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  justify-content: center;
  background: #ffffff;
  border-radius: 5px;
  height: 110px;
  overflow: hidden;
  border: 1px solid #eee;
  box-shadow: 4px 5px 10px 1px #f7f7f7;
  .content {
    position: relative;

    display: flex;
    align-items: center;
    .content-right {
      color: #1d252f;
      padding: 0 20px;

      .el-icon-warning-outline {
        margin-right: 5px;
      }
    }
    .name {
      color: #7d7b7b;
      font-size: 14px;
      font-weight: 400;
      padding-bottom: 5px;
    }

    .data {
      font-size: 19px;
      font-family: Source Han Sans CN, sans-serif;
      color: #505050;
      font-weight: bold;
      letter-spacing: 1px;
    }
  }
  .f-icon {
    position: absolute;
    right: 10px;
    bottom: 2px;

    i {
      font-size: 44px;
      color: #f5f5f5;
    }
  }
}
.content-bottom {
  display: flex;
  margin-top: 6px;
  padding: 10px 20px 0;
  border-top: 1px solid #eee;
  .num-01 {
    display: flex;
    font-size: 13px;
    color: #837e7e;
    flex: 1;
  }
}
.rmb {
  color: #686161;
  font-size: 14px;
}
</style>
