<template>
  <div class="tabs">
    <el-tabs v-model="activeName" @tab-click="tabClick" class="vol-tabs">
      <el-tab-pane name="color">
        <template #label>
          <span class="custom-tabs-label">
            <el-icon><calendar /></el-icon>
            <span>产品颜色</span>
          </span>
        </template>

        <product-color style="margin-top: 4px" ref="color"></product-color>
      </el-tab-pane>
      <el-tab-pane name="size">
        <template #label>
          <span class="custom-tabs-label">
            <el-icon><calendar /></el-icon>
            <span>产品尺寸</span>
          </span>
        </template>
        <product-size style="margin-top: 4px" ref="size"></product-size>
      </el-tab-pane>
    </el-tabs>
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
  padding-top: 0;
  margin-top: 15px;
  border-top: 8px solid #eee;
}
.custom-tabs-label {
  display: flex;
  justify-content: center;
  align-content: center;
  .el-icon {
    margin-right: 2px;
    padding-top: 6px;
  }
}
.tabs ::v-deep(.el-tabs) {
  box-shadow: none;
}
.tabs ::v-deep(.el-tabs__content) {
  padding: 0 !important;
  padding-bottom: 15px;
}
.tabs {
  ::v-deep(.grid-search) {
    padding-top: 0 !important;
  }
  ::v-deep(.view-header) {
    padding-left: 0 !important;
    padding-right: 0 !important;
    margin-top: 5px;
  }
  ::v-deep(.grid-container) {
    padding: 0 !important;
  }
}
.tabs ::v-deep(.el-tabs__nav-wrap:after) {
  background-color: #f5f5f5 !important;
}
</style>
