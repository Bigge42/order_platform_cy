<template>
  <el-dropdown trigger="hover" v-if="tree && tree.length">
    <el-button
      link
      class="el-dropdown-link"
      style="outline: none"
      :style="{ color: color }"
    >
      {{ $ts(orgName) }}<i style="margin-left: 3px" class="el-icon-sort"></i
    ></el-button>
    <template #dropdown>
      <el-dropdown-menu>
        <el-dropdown-item
          :disabled="orgName == item.name"
          v-for="(item, index) in tree"
          :key="index"
        >
          <div style="width: 100%" @click="nodeClick(item)">
            {{ $ts(item.name) }}
          </div>
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>
<script>
export default {
  props: {
    load: {
      type: Boolean,
      default: false,
    },
    color: {
      type: String,
      default: "white",
    },
  },
  data() {
    return {
      selectCode: "",
      orgName: "请选择",
      serviceList: [],
      tree: [],
      model: false,
    };
  },
  created() {
    let list = this.$store.getters.getServiceList();
    list = JSON.parse(JSON.stringify(list));
    this.serviceList.push(...list);
    this.selectCode = localStorage.getItem("serviceId") || "";
    this.setCurrentServiceName();
    this.serviceList.forEach((x) => {
      if (!x.parentId) {
        x.lv = 1;
        x.children = [];
        this.tree.push(x);
        this.getTree(x.id, x);
      }
    });
  },
  methods: {
    setCurrentServiceName() {
      if (
        this.selectCode &&
        this.serviceList.length &&
        !this.serviceList.some((x) => {
          return x.id == this.selectCode;
        })
      ) {
        this.selectCode = null;
      }

      if (!this.multiple && this.serviceList.length && !this.selectCode) {
        this.selectCode = this.serviceList[0].id;
        this.orgName = this.serviceList[0].name;
      } else {
        this.orgName = (
          this.serviceList.find((x) => {
            return x.id == this.selectCode;
          }) || {}
        ).name;
      }
      localStorage.setItem("serviceId", this.selectCode || "");
    },
    getTree(id, data) {
      this.serviceList.forEach((x) => {
        if (x.parentId == id) {
          x.lv = data.lv + 1;
          if (!data.children) data.children = [];
          data.children.push(x);
          this.getTree(x.id, x);
        }
      });
    },
    showModel() {
      this.model = true;
    },
    // change() {
    //   localStorage.setItem('orgCode', this.selectCode);
    //   this.$emit('onChange', this.selectCode);
    // },
    nodeClick(node, selected) {
      this.model = false;
      this.selectCode = node.id;
      this.setCurrentServiceName();
      window.location.reload();
    },
  },
};
</script>
<style scoped lang="less">
// .service-selector {
//   height: 100%;
// }

button {
  color: #5a5a5a;
  // height: 32px;
  // padding: 2px 14px 0 14px;
  font-size: 14px !important;
}

.service-model ::v-deep(.el-drawer__header) {
  height: 52px;
  line-height: 52px;
  margin-bottom: 0;
  padding: 10px;
  border-bottom: 1px solid #f0f0f0;
}

.action-group {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 15px;
}
</style>
