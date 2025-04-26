<template>
  <div style="padding: 0 15px; margin-top: 10px" :closable="false">
    <el-alert
      :show-icon="true"
      type="info"
      class="alert-primary"
      title="一对多主从表，只需简单配置即可在列表页面显示(新建弹出框编辑不需要配置,代码生成器自动完成)"
      :closable="false"
    ></el-alert>
  </div>
  <div class="tabs">
    <div class="tabs-item"><product-color ref="color"></product-color></div>
    <div class="tabs-item"><product-size ref="size"></product-size></div>
  </div>
</template>
<script>
import ProductColor from "@/views/dbtest/product/Demo_ProductColor";
import ProductSize from "@/views/dbtest/product/Demo_ProductSize";
export default {
  components: {
    "product-color": ProductColor,
    "product-size": ProductSize,
  },
  data() {
    return {
      activeName: "color",
    };
  },
  emits: ["parentCall"],
  methods: {
    tabClick(params) {
      console.log("选项卡点击事件");
    },
    rowClick() {
      //主表(产品管理)点击行事件

      //操作步骤2：调用两张表明细表的查询方法

      //浮脉查询条件配置，见Demo_ProductColor.js、Demo_ProductSize.js文件
      this.$refs.color.$refs.grid.search();

      //加载尺寸明细表数据
      this.$refs.size.$refs.grid.search();
    },
  },
};
</script>
<style scoped lang="less">
.tabs {
  padding: 15px;
  padding-top: 10px;
  display: flex;
}

.tabs-item {
  width: 0;
  flex: 1;
  padding-right: 15px;
}
.tabs-item:last-child {
  padding-right: 0;
}
.tabs-item ::v-deep(.view-container .grid-search) {
  padding-top: 0;
}
.tabs-item ::v-deep(.grid-container),
.tabs-item ::v-deep(.view-header) {
  padding: 0;
}
.tabs-item ::v-deep(.view-header) {
  padding-bottom: 11px;
}
</style>
