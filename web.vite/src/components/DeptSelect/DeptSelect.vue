<template>
  <div class="service-select" v-if="tree&&tree.length">

    <el-dropdown trigger="hover">
      <el-button link class="el-dropdown-link" style="outline: none;" :style="{ color: color }">
        {{ $ts(deptName)  }}<i style="margin-left: 5px;" class="el-icon-arrow-down"></i></el-button>
      <template #dropdown>
        <el-dropdown-menu>
          <el-dropdown-item :disabled="deptName==item.name"  v-for="(item, index) in tree" :key="index">
            <div @click="nodeClick(item)">
              {{ $ts(item.name) }}
            </div>
          </el-dropdown-item>
        </el-dropdown-menu>
      </template>
    </el-dropdown>
  </div>
</template>
<script>
export default {
  props: {
    load: {
      type: Boolean,
      default: false
    },
    color: {
      type: String,
      default: "white",
    },
  },
  data() {
    return {
      selectCode: '',
      deptName: '请选择',
      deptList: [],
      tree: [],
      model: false
    };
  },
  created() {
    let list = this.$store.getters.data().deptList
    list = JSON.parse(JSON.stringify(list))
    this.deptList.push(...list);
    this.selectCode = localStorage.getItem('deptId') || '';
    this.setCurrentServiceName();
    this.tree=list;
    // this.deptList.forEach((x) => {
    //   if (!x.parentId) {
    //     x.lv = 1;
    //     x.children = [];
    //     this.tree.push(x);
    //     this.getTree(x.id, x);
    //   }
    // });
  },
  methods: {
    setCurrentServiceName() {
      if (
        this.selectCode &&
        this.deptList.length &&
        !this.deptList.some((x) => {
          return x.id == this.selectCode;
        })
      ) {
        this.selectCode = null;
      }

      if (!this.multiple && this.deptList.length && !this.selectCode) {
        this.selectCode = this.deptList[0].id;
        this.deptName = this.deptList[0].name;
      } else {
        this.deptName = (
          this.deptList.find((x) => {
            return x.id == this.selectCode;
          }) || {}
        ).name;
      }
      localStorage.setItem('deptId', this.selectCode || '');
    },
    getTree(id, data) {
      this.deptList.forEach((x) => {
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
    }
  }
};
</script>
<style scoped lang="less">
.service-selector {
  height: 100%;
}

.service-select {
  button {
    color: #5a5a5a;
    height: 32px;
    padding: 2px 14px 0 14px;
    font-size: 14px !important;
  }
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
