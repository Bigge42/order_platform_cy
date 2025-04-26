<template>
  <vol-box
    :lazy="true"
    v-model="model"
    title="选择客户"
    :height="550"
    :width="1200"
    :padding="0"
  >
    <div>
      <customer ref="customer"></customer>
    </div>
    <template #footer>
      <div>
        <el-button type="primary" size="samll" @click="onSelect">确认</el-button>
      </div>
    </template>
  </vol-box>
  <!-- 
   批量选择明细表数据 -->
  <vol-box
    :lazy="true"
    v-model="detailModel"
    title="批量选择明细表数据"
    :height="560"
    :width="1200"
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
        ref="detailTable"
        :loadKey="true"
        :columns="columns"
        :pagination-hide="false"
        :height="420"
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

<script>
import VolBox from "@/components/basic/VolBox.vue";
import customer from "@/views/dbtest/customer/Demo_Customer";
import VolTable from "@/components/basic/VolTable.vue";

//这里使用的vue2语法，也可以写成vue3语法
export default {
  emits: ["parentCall"],
  components: {
    "vol-box": VolBox,
    customer,
    VolTable,
  },
  methods: {},
  data() {
    return {
      model: false, //弹出框
      detailModel: false, //批量选择明细表数据model
      url: "api/Demo_Order/getGoods",
      GoodsName: "", //商品名称
      columns: [
        {
          field: "GoodsId",
          title: "商品ID",
          type: "guid",
          width: 110,
          hidden: true,
          readonly: true,
        },
        {
          field: "GoodsName",
          title: "商品名称",
          type: "string",
          link: true,
          width: 120,
          sort: true,
        },
        { field: "GoodsCode", title: "商品编号", type: "string", width: 100 },
        { field: "Img", title: "商品图片", type: "img", width: 80 },
        { field: "Specs", title: "商品规格", type: "string", width: 90 },
        { field: "Price", title: "单价", type: "decimal", width: 80 },
        {
          field: "Enable",
          title: "是否可用",
          type: "int",
          bind: { key: "enable", data: [] },
          width: 90,
        },
        { field: "Remark", title: "备注", type: "string", width: 100 },
        {
          field: "CreateID",
          title: "CreateID",
          type: "int",
          width: 80,
          hidden: true,
        },
        { field: "Creator", title: "创建人", type: "string", width: 80 },
        {
          field: "CreateDate",
          title: "创建时间",
          type: "datetime",
          width: 140,
          sort: true,
        },
      ],
    };
  },
  methods: {
    open() {
      //打开主表选择数据
      this.model = true;

      //打开时刷新界面数据(2022.12.06)
      this.$nextTick(() => {
        this.$refs.customer && this.$refs.customer.$refs.grid.search();
      });
    },
    openDetail() {
      //打明细表批量选择
      this.detailModel = true;

      //刷新表格的数据
      this.$nextTick(() => {
        this.$refs.detailTable?.load(null, true);
      });
    },
    onSelect() {
      //主表的选择数据回写到主表的表单上
      let rows = this.$refs.customer.$refs.grid.getSelectRows();
      if (!rows.length) {
        return this.$message.error("请选择数据");
      }
      this.$emit("parentCall", ($parent) => {
        //如：调用页面查询
        $parent.editFormFields.Customer = rows[0].Customer;
        $parent.editFormFields.CustomerId = rows[0].CustomerId;
        $parent.editFormFields.PhoneNo = rows[0].PhoneNo;
        this.model = false;
      });
    },
    detailSelectClick() {
      //回写数据到明细表
      let rows = this.$refs.detailTable.getSelected();
      if (!rows.length) {
        return this.$message.error("请选择数据");
      }
      //获取回写到明细表的字段
      let _rows = rows.map((row) => {
        return {
          GoodsId: row.GoodsId,
          GoodsCode: row.GoodsCode,
          GoodsName: row.GoodsName,
          Img: row.Img,
          Specs: row.Specs,
          Price: row.Price,
        };
      });
      this.$emit("parentCall", ($parent) => {
        $parent.$refs.detail.rowData.unshift(..._rows);
        this.detailModel = false;
      });
    },
    search() {
      //搜索
      this.$refs.detailTable.load(null, true);
    },
    loadBefore(param, callBack) {
      //批量选择设置查询条件
      param.wheres = [{ name: "GoodsName", value: this.GoodsName, displayType: "like" }];
      callBack(true);
    },
  },
};
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
