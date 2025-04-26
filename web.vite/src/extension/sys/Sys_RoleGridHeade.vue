<template>
  <div>
    <vol-box v-model="model" :padding="0" :title="row.RoleName" :width="1200">
      <el-tabs v-model="activeName" class="role-tabs" @tab-click="handleClick">
        <el-tab-pane name="menu">
          <template #label>
            <div class="custom-tabs-label">
              <el-icon>
                <user :size="20" />
              </el-icon>
              <span>{{ $ts("菜单权限") }}</span>
            </div>
          </template>
        </el-tab-pane>
        <el-tab-pane name="data">
          <template #label>
            <div class="custom-tabs-label">
              <el-icon>
                <document :size="20" />
              </el-icon>
              <span>{{ $ts("数据权限") }}</span>
            </div>
          </template>
        </el-tab-pane>
      </el-tabs>
      <el-tree
        @check-change="leftCheckChange"
        @check="nodeCheck"
        :data="roleTree"
        :show-checkbox="false"
        class="role-tree"
        node-key="id"
        default-expand-all
        :expand-on-click-node="false"
      >
        <template #default="{ data }">
          <div class="action-group">
            <div class="action-text" :style="{ width: (4 - data.lv) * 18 + 150 + 'px' }">
              <el-checkbox v-model="data.leftCk" @change="allChange(data)">{{
                $ts(data.text) + (data.isApp ? "(app)" : "")
              }}</el-checkbox>
            </div>
            <div class="action-item" v-show="activeName == 'menu'">
              <div class="auth">
                <el-tooltip
                  class="box-item"
                  v-if="!data.children"
                  effect="dark"
                  :content="$ts('字段权限')"
                  placement="left"
                >
                  <i
                    style="font-size: 16px"
                    @click="openAuthFields(data)"
                    class="el-icon-user"
                  ></i>
                </el-tooltip>
              </div>

              <el-checkbox
                v-for="(item, index) in data.actions"
                :key="index"
                v-model="item.checked"
                @change="actionChange(data, item.checked, item)"
                >{{ $ts(item.text) }}</el-checkbox
              >
            </div>
            <div v-show="activeName == 'data' && !data.children">
              <el-radio-group v-model="data.authMenuData">
                <el-radio v-for="kv in authData" :key="kv.key" :label="kv.key">{{
                  $ts(kv.value)
                }}</el-radio>
              </el-radio-group>
            </div>
          </div>
        </template>
      </el-tree>
      <template #footer>
        <div style="text-align: center">
          <el-button size="small" @click="model = false">{{ $ts("关闭") }}</el-button>
          <el-button type="primary" size="small" @click="save">{{
            $ts("保存")
          }}</el-button>
        </div>
      </template>
    </vol-box>

    <vol-box v-model="fieldModel" :padding="0" :title="text" :width="650">
      <el-alert
        type="success"
        :closable="false"
        :title="$ts('不勾选默认就有全部字段权限')"
        show-icon
      ></el-alert>
      <div style="height: 300px; padding: 10px 20px">
        <div>
          <el-checkbox v-model="checkAllModel" @change="checkAll">
            {{ $ts("全选") }}
          </el-checkbox>
        </div>
        <div v-for="(item, index) in auth.fields" :key="index" class="check-item">
          <el-checkbox v-model="item.checked">{{ $ts(item.name) }}</el-checkbox>
        </div>
      </div>
      <template #footer>
        <div style="text-align: center">
          <el-button size="small" @click="fieldModel = false">{{
            $ts("关闭")
          }}</el-button>
          <el-button type="primary" size="small" @click="fieldSave">{{
            $ts("保存")
          }}</el-button>
        </div>
      </template>
    </vol-box>
  </div>
</template>
<script>
import { defineComponent, defineAsyncComponent } from "vue";
import authData from "./Sys_RoleAuthDataOptions.js";
export default defineComponent({
  components: {
    VolBox: defineAsyncComponent(() => import("@/components/basic/VolBox.vue")),
  },
  data() {
    return {
      activeName: "menu",
      authData: authData.map((x) => {
        return { key: x.key + "", value: this.$ts(x.value) };
      }),
      text: "",
      isInit: false,
      height: 200,
      row: {},
      model: false,
      fieldModel: false,
      auth: { fields: [], roleFields: [] },
      checkAllModel: false,
      roleList: [],
      roleTree: [],
      roles: [],
      menuId: 0,
    };
  },
  methods: {
    handleClick() {},
    async open(row) {
      this.row = row;
      if (!this.isInit) {
        await this.getCurrentTreePermission();
        this.isInit = true;
      }
      this.roleList.forEach((x) => {
        x.leftCk = false;
      });

      this.getUserRole();
      this.model = true;
    },
    save() {
      let userPermissions = [];
      this.roleList.forEach((x) => {
        let checkedPermission = x.actions.filter((f) => {
          return f.checked;
        });
        if (checkedPermission.length > 0) {
          let actions = checkedPermission.map((m) => {
            return { text: m.text, value: m.value };
          });
          userPermissions.push({
            id: x.id,
            actions: actions,
            authMenuData: x.authMenuData,
          });
        }
      });
      let url = `api/role/SavePermission?roleId=${this.row.Role_Id}`;
      this.http.post(url, userPermissions, true).then((result) => {
        this.$message[result.status ? "success" : "error"](result.message);
      });
    },
    leftCheckChange(node, selected) {
      node.actions.forEach((x, index) => {
        x.checked = selected;
      });
    },
    nodeCheck(node, data) {
      let rootData = roleList.find((x) => {
        return x.id === node.pid;
      });
      if (rootData && rootData.actions.length) {
        rootData.actions[0].checked =
          node.actions.some((x) => {
            return x.checked;
          }) ||
          data.halfCheckedNodes.some((x) => {
            return x.id === node.pid;
          });
      }
    },
    allChange(data) {
      data.actions.forEach((item) => {
        item.checked = data.leftCk;
      });
      if (!data.children) {
        return;
      }
      this.setChildrenChecked(data, data.leftCk);
    },
    setChildrenChecked(data, ck) {
      data.children.forEach((item) => {
        item.leftCk = ck;
        item.actions.forEach((c) => {
          c.checked = ck;
        });
        if (item.children) {
          this.setChildrenChecked(item, ck);
        }
      });
    },
    setParentCheck(data) {
      if (!data.pid) {
        return;
      }
      let b = true;
      let pid = data.pid;
      let perentData;
      while (b) {
        if (!pid) {
          return false;
        }
        perentData = this.roleList.find((x) => {
          return x.id == pid;
        });
        if (!perentData || !perentData.actions) {
          b = false;
          return false;
        }
        pid = perentData.pid;
        let obj = perentData.actions.find((c) => {
          return c.value == "Search";
        });
        if (obj) {
          obj.checked = true;
        }
      }
    },
    actionChange(data, ck, item) {
      ck =
        data.actions.filter((x) => {
          return x.checked;
        }).length == data.actions.length;
      data.leftCk = ck;
      if (item.value == "Search" && item.checked) {
        this.setParentCheck(data);
      }
    },
    async getCurrentTreePermission() {
      let url = "/api/role/getCurrentTreePermission";
      await this.http.post(url, {}, true).then((result) => {
        if (!result.status) return;
        this.roleList.splice(0);
        this.roles.splice(0);
        this.roleList.push(...result.data.tree);
        this.roles.push(...result.data.roles);
        this.roleList.forEach((x) => {
          if (x.pid == 0) {
            x.lv = 1;
            x.children = [];
            this.roleTree.push(x);
            this.getRoleTree(x.id, x);
          }
        });
      });
    },
    getRoleTree(id, data, isRootId) {
      this.roleList.forEach((x) => {
        if (x.pid == id) {
          x.lv = data.lv + 1;
          if (isRootId) {
            x.rootId = id;
          }
          if (!data.children) data.children = [];
          data.children.push(x);
          this.getRoleTree(x.id, x, isRootId);
        }
      });
    },
    getUserRole() {
      this.roleList.forEach((x) => {
        x.authMenuData = null;
        x.actions.forEach((a) => {
          a.checked = false;
        });
      });
      let url = `/api/role/getUserTreePermission?roleId=${this.row.Role_Id}`;
      this.http.post(url, {}, true).then((result) => {
        if (!result.status) return;
        result.data.forEach((item) => {
          if (item.actions.length == 0) return;
          let sourceItem = this.roleList.find((f) => f.id == item.id);
          if (!sourceItem) return;
          sourceItem.authMenuData = item.authMenuData || null;
          item.actions.forEach((actions) => {
            sourceItem.actions.forEach((soure) => {
              soure.leftCk = false;
              if (soure.value == actions.value) {
                soure.checked = true;
              }
            });
          });
        });
      });
    },
    openAuthFields(item) {
      this.fieldModel = true;
      this.menuId = item.id;
      this.text = this.$ts("字段权限") + "(" + this.$ts(item.text) + ")";
      const url = `api/role/getAuthFields?menuId=${item.id}&roleId=${this.row.Role_Id}`;
      this.http.get(url, {}).then((result) => {
        result.fields.forEach((x) => {
          x.checked = result.roleFields.indexOf(x.field) != -1;
        });
        this.auth = result;
      });
    },
    fieldSave() {
      const fields = this.auth.fields
        .filter((c) => {
          return c.checked;
        })
        .map((c) => {
          return c.field;
        });
      const url = `api/role/saveAuthFields?menuId=${this.menuId}&roleId=${this.row.Role_Id}`;
      this.http.post(url, fields).then((result) => {
        this.$message.success(result);
      });
    },
    checkAll() {
      this.auth.fields.forEach((x) => {
        x.checked = this.checkAllModel;
      });
    },
  },
  created() {
    this.authData.unshift({ key: "", value: "不分配" });
  },
});
</script>

<style lang="less" scoped>
.action-group {
  display: flex;
  // line-height: 32px;
  justify-content: center;
  align-items: center;

  label {
    float: left;
  }

  .action-text {
    line-height: 33px;

    label {
      margin-right: 5px;
    }
  }
}

.title {
  padding: 10px;
  background: rgb(246 250 255);
  font-weight: bold;
  font-size: 14px;
  letter-spacing: 2px;
}

.el-role-list {
  flex: 1;
  height: 0;
  overflow-x: hidden;
}

::v-deep(.el-tree-node__content) {
  // margin-bottom: 5px;
  height: auto;
}

::v-deep(.el-checkbox__label) {
  position: relative;
  top: 2px;
}

.auth {
  margin-top: 8px;
  float: left;
  width: 31px;
  height: 10px;
}

.el-icon-user {
  font-size: 16px;
}

.action-item {
  flex: 1;
}

.check-item {
  margin-left: 20px;
  float: left;
  min-width: 150px;
}

.custom-tabs-label {
  display: flex;
  align-items: center;

  i {
    margin-right: 3px;
  }
}

::v-deep(.el-tabs__nav-wrap:after) {
  height: 1px !important;
}

.role-tabs {
  position: absolute;
  width: 100%;
  background: #ffff;
  top: 0;
  z-index: 99;
  padding: 0 10px;

  ::v-deep(.el-tabs__header) {
    margin: 0 !important;
  }
}

.role-tree {
  padding: 50px 10px 10px 10px;
}
</style>
