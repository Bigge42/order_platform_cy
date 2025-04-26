<template>
  <vol-box :lazy="true" v-model="model" title="选择客户" :width="1200" :padding="0">
    <div>
      <customer ref="customerRef"></customer>
    </div>
    <template #footer>
      <div>
        <el-button type="primary" size="small" @click="onSelect">确认</el-button>
      </div>
    </template>
  </vol-box>
  <!-- 
     批量选择明细表数据 -->
  <vol-box
    :lazy="true"
    v-model="detailModel"
    title="批量选择明细表数据"
    :height="440"
    :width="1000"
    :padding="0"
  >
    <div>
      <!-- 搜索配置 -->
      <div class="search-form">
        <label>商品名称：</label>
        <el-input style="width: 200px" v-model="GoodsName"></el-input>
        <el-button size="small" type="primary" plain @click="search"
          ><i class="el-icon-search"></i>搜索</el-button
        >
      </div>
      <!-- 表格数据 -->
      <vol-table
        ref="detailTableRef"
        :loadKey="true"
        :columns="columns"
        :pagination-hide="false"
        :height="340"
        :url="url"
        @loadBefore="loadBefore"
      ></vol-table>
    </div>

    <template #footer>
      <div>
        <el-button type="primary" @click="detailSelectClick" size="default"
          >选择数据</el-button
        >
      </div>
    </template>
  </vol-box>
</template>

<script setup lang="jsx">
import customer from "@/views/dbtest/customer/Demo_Customer";
import {
  ref,
  reactive,
  getCurrentInstance,
  
  
  
  nextTick,
} from "vue";

//创建emit回调方法
const emit = defineEmits(["customerOnSelect", "detailOnSelect"]);

const { proxy } = getCurrentInstance();

const model = ref(false); //弹出框
const detailModel = ref(false); //批量选择明细表数据model
const GoodsName = ref(""); //商品名称搜索
//明细的查询url
const url = "api/Demo_Order/getGoods";
//明细表字段配置
const columns = reactive([
  { field: "GoodsName", title: "商品名称", width: 120 },
  { field: "GoodsCode", title: "商品编号", type: "string", width: 100 },
  { field: "Img", title: "商品图片", type: "img", width: 80 },
  { field: "Specs", title: "商品规格", type: "string", width: 90 },
  { field: "Price", title: "单价", type: "decimal", width: 80 },
  { field: "Remark", title: "备注", width: 100 },
  { field: "CreateDate", title: "创建时间", width: 140 },
]);

const customerRef = ref();
//打开主表选择数据
const open = () => {
  model.value = true;
  //打开时刷新界面数据
  customerRef.value && customerRef.value.$refs.grid.search();
};
//打明细表批量选择
const openDetail = () => {
  GoodsName.value = "";
  detailModel.value = true;
  //刷新表格的数据
  nextTick(() => {
    detailTableRef.value?.load(null, true);
  });
};
const onSelect = () => {
  //主表的选择数据回写到主表的表单上
  let rows = customerRef.value.$refs.grid.getSelectRows();
  if (!rows.length) {
    return proxy.$message.error("请选择数据");
  }
  emit("customerOnSelect", rows[0]);
  model.value = false;
  // this.$emit("customerOnSelect", ($parent) => {
  //   //如：调用页面查询
  //   $parent.editFormFields.Customer = rows[0].Customer;
  //   $parent.editFormFields.CustomerId = rows[0].CustomerId;
  //   $parent.editFormFields.PhoneNo = rows[0].PhoneNo;
  //   this.model = false;
  // });
};
//明细表选择数据, 回写数据到明细表
const detailSelectClick = () => {
  //获取选中的行数据
  let rows = detailTableRef.value.getSelected();
  if (!rows.length) {
    return proxy.$message.error("请选择数据");
  }
  //获取回写到明细表的字段
  rows = rows.map((row) => {
    return {
      GoodsId: row.GoodsId,
      GoodsCode: row.GoodsCode,
      GoodsName: row.GoodsName,
      Img: row.Img,
      Specs: row.Specs,
      Price: row.Price,
    };
  });
  //调用demo_order.vue的detailOnSelect方法回写数据
  emit("detailOnSelect", rows);
  detailModel.value = false;
};
const detailTableRef = ref();
//明细表点击搜索
const search = () => {
  //搜索
  detailTableRef.value.load(null, true);
};
//明细表查询前方法设置查询条件
const loadBefore = (param, callBack) => {
  //批量选择设置查询条件
  param.wheres = [{ name: "GoodsName", value: GoodsName.value, displayType: "like" }];
  callBack(true);
};
//对外暴露打开弹出框方法，否则demo_oder.vue中点击按钮访问不到方法
defineExpose({
  open,
  openDetail,
});
</script>

<style lang="less" scoped>
.search-form {
  display: flex;
  padding: 10px;
  line-height: 34px;

  button {
    margin-left: 10px;
  }
}
</style>
