<template>
  <vol-box v-model="model" :padding="30" title="修改密码" :width="500" :height="250">
    <el-alert type="success">
      <h3>
        <span>{{ $ts('帐号') }}：{{ row.UserName }}</span>
      </h3>
    </el-alert>
    <div>
      <el-input :placeholder="$ts('密码')" v-model="password" size="large" style="width: 100%; margin-top: 15px" />
    </div>
    <template #footer>
      <el-button type="primary" size="mini" @click="savePwd()">{{
        $ts('修改密码')
      }}</el-button>
      <el-button type="priarmy" size="mini" @click="model = false">{{
        $ts('关闭')
      }}</el-button>
    </template>
  </vol-box>


  <vol-box v-model="modelAuth" :padding="5" :title="row.UserTrueName" :width="1200">
    <el-table :data="roleTree" style="width: 100%;height:calc(100vh - 150px)" row-key="id" border
      :default-expand-all="false" :tree-props="{ children: 'children', hasChildren: 'hasChildren' }">
      <el-table-column prop="text" :label="$ts('菜单')" width="250">
      </el-table-column>
      <el-table-column prop="name" :label="$ts('用户')">
        <template #default="scope">
          <div v-if="scope.row.sub" class="user-list">
            <div class="user-item" v-for="(item, index) in scope.row.data" :key="item.id">
              {{ item.name }}
              <i @click.stop="delUser(scope.row.data, index)" class="el-icon-delete" style="display: none;"></i>
            </div>
          </div>
        </template>
      </el-table-column>
      <el-table-column align="center" prop="address" width="110" :label="$ts('操作')">
        <template #default="scope">
          <el-button @click="showUser(scope.row)" type="primary" link v-if="scope.row.sub"> <i class="el-icon-plus"></i>{{
            $ts('选择用户') }}</el-button>
        </template>
      </el-table-column>
    </el-table>
    <template #footer>
      <div style="text-align: center;">
        <el-button size="small" @click="modelAuth = false">{{ $ts('关闭') }}</el-button>
        <el-button type="primary" plain @click="save">{{ $ts('保存') }}</el-button>
      </div>
    </template>
  </vol-box>

  <vol-box v-model="modelUser" :padding="5" title="选择数据" :width="1000">
    <div class="search-form">
      <label>帐号：</label> <el-input style="width: 150px;" v-model="userName" :placeholder="$ts('请输入')"></el-input>
      <label class="label">姓名：</label> <el-input style="width: 150px;" v-model="userTrueName"
        :placeholder="$ts('请输入')"></el-input>
      <el-button type="primary" @click="searchClick" plain><i class="el-icon-search"></i> {{ $ts('搜索') }}</el-button>
    </div>
    <VolTable @loadBefore="loadBefore" :paginationHide="false" :height="400" ref="table" url="api/sys_user/getPageData"
      :columns="columns">
    </VolTable>
    <template #footer>
      <div style="text-align: center;">
        <el-button size="small" @click="modelUser = false">{{ $ts('关闭') }}</el-button>
        <el-button type="primary" size="small" @click="selectClick">{{ $ts('确定') }}</el-button>
      </div>
    </template>
  </vol-box>
</template>

<script setup>
import VolBox from '@/components/basic/VolBox.vue';
import VolTable from '@/components/basic/VolTable.vue';
import {
  defineComponent,
  ref,
  reactive,
  getCurrentInstance
} from 'vue';

const row = ref({});
const password = ref('')
const model = ref(false)
const modelAuth = ref(false)

const modelUser = ref(false)

const userName = ref('');

const userTrueName = ref('')

const open = (_row) => {
  password.value = '';
  row.value = _row;
  model.value = true;
}

const columns = reactive([{ field: 'User_Id', title: 'User_Id', type: 'int', width: 90, hidden: true, readonly: true, require: true, align: 'left' },
{ field: 'UserName', title: '帐号', type: 'string', width: 120, readonly: true, require: true, align: 'left', sort: true },
{ field: 'UserTrueName', title: '姓名', type: 'string', width: 100, require: true, align: 'left' },
{ field: 'Gender', title: '性别', type: 'int', bind: { key: 'gender', data: [] }, width: 80, align: 'left' },
{ field: 'HeadImageUrl', title: '头像', type: 'img', width: 80, align: 'left' },
{ field: 'Email', title: '邮箱', type: 'string', width: 140, align: 'left' },
{ field: 'Token', title: 'Token', type: 'string', width: 180, hidden: true, align: 'left' },
{ field: 'CreateDate', title: '注册时间', type: 'datetime', width: 150, readonly: true, align: 'left', sort: true },
{ field: 'PhoneNo', title: '手机号', type: 'string', width: 150, hidden: true, align: 'left' },
{ field: 'Remark', title: '备注', type: 'string', width: 100, align: 'left' },
])


const { proxy } = getCurrentInstance();
const savePwd = () => {
  if (!password.value) return proxy.$Message.error(proxy.$ts('请输入密码'));
  if (password.value.length < 6)
    return proxy.$Message.error(proxy.$ts('"密码长度至少6位"'));
  let url =
    '/api/user/modifyUserPwd?password=' +
    password.value +
    '&userName=' +
    row.value.UserName;
  proxy.http.post(url, {}, true).then((x) => {
    if (!x.status) {
      return proxy.$message.error(proxy.$ts(x.message));
    }
    model.value = false;
    proxy.$Message.success(proxy.$ts(x.message));
  });
}
const openAuth = async (_row) => {
  row.value = _row;
  modelAuth.value = true;
  if (!roleTree.length) {
    await getCurrentTreePermission();
  }
  list.forEach(c => {
    c.data = []
  })
  proxy.http.post("api/sys_user/getUserAuth?userId=" + row.value.User_Id).then(result => {

    let data = result.data;
    data.forEach(x => {
      x.data= x.userIds.split(',').map(c => {
        let obj = result.users.find(m => { return m.userId + '' === c });
        if (!obj) {
          return {
            id: c.id,
            name:c.id,// obj.userName
          }
        }
        return {
          id:  obj.userId,
          name: obj.userName
        }
      })
    })


    for (let index = 0; index < list.length; index++) {
      const item = list[index];
      const obj = data.find(c => { return c.id === item.id });
      if (obj) {
        item.data.splice(0);
        item.data.push(...(obj.data||[]))
       // item.data =obj.data||[]// (obj.userIds || '').split(",").map(x => { return x * 1 }).filter(x => { return x > 0 })
      }
    }
  })
}
const list = reactive([]);
const roleTree = reactive([]);
const getCurrentTreePermission = async () => {
  let url = 'api/role/getCurrentTreePermission';
  await proxy.http.post(url, {}, true).then((result) => {
    if (!result.status) return;
    list.push(... result.data.tree);
    list.forEach(x => {
      x.parentId = x.pid;
      x.data = [];
      // if (!list.some(c => { return c.pid+'' === x.id+'' })) {
      //   x.parentId = 0;
      // }
      x.sub = !list.some(c => { return c.pid + '' === x.id + '' })
    })
    let tree = proxy.base.convertTree(list, (node, data, isRoot) => {
      if (isRoot) {
        node.lv = 1;
      }
    })
    roleTree.push(...tree)

  });
}
const delUser = (item, index) => {
  item.splice(index, 1)
}

const table = ref(null)
const save = () => {
  const authData = list.filter(x => { return x.sub }).map(x => {
    return {
      MenuId: x.id,
      AuthUserIds: x.data.map(c => { return c.id }).join(',')
    }
  })
  proxy.http.post("api/sys_user/saveUserAuth?userId=" + row.value.User_Id, authData).then(result => {
    proxy.$Message.success(proxy.$ts("保存成功"));
  })
}

let menuRow = {}
const showUser = (_row) => {
  menuRow = _row;
  modelUser.value = true;
  if (table.value && table.value.load) {
    table.value.load(null, true)
  }
}

const searchClick = () => {
  table.value.load(null, true)
}
const selectClick = () => {
  let rows = table.value.getSelected();
  if (rows.length == 0) {
    return proxy.$Message.error(proxy.$ts("请选择行!"));
  }
  //{ id: 1, name: "123" }, { id: 1, name: "1234" }, { id: 1, name: "1235" }
  let currentUserIds = menuRow.data.map(x => { return x.id });
  let userInfo = rows.filter(x => { return currentUserIds.indexOf(x.User_Id) == -1 })
    .map(x => { return { id: x.User_Id, name: x.UserTrueName } });
  userInfo.unshift(...menuRow.data);
  menuRow.data = userInfo;
  // menuRow.data.push(...userInfo);

  modelUser.value = false;

}

const loadBefore = (params) => {
  params.wheres = [{ name: "UserName", value: userName.value, displayType: "like" },
  { name: "UserTrueName", value: userTrueName.value, displayType: "like" }
  ]
  return true;
}

defineExpose({ open, openAuth, getCurrentTreePermission })
</script>

<style lang="less" scoped>
h3 {
  font-weight: 500;

  >span:last-child {
    margin-left: 10px;
  }
}
</style>

<style lang="less" scoped>
::v-deep(.el-table_2_column_12) {

  // align-items: center;
  // display: flex;
  .cell {
    align-items: center;
    display: flex;
  }
}

.user-list {
  display: inline-block;
  width: 100%;
  margin: 0;
  padding: 0;

  // padding: 5px;
  .user-item {
    float: left;
    list-style: none;
    padding: 2px 10px;
    position: relative;
    margin-right: 10px;

    i {
      position: absolute;
      right: 0;
      top: 0;
      right: -5px;
    }
  }

  .user-item:hover {
    cursor: pointer;
    color: #898585;

    i {
      display: inline-block !important;
    }
  }
}

.user-search {
  cursor: pointer;
  width: 90px;
  text-align: center;
  // color: #898585;
}

::v-deep(.el-table__header th) {
  background: #f6f6f6;
  color: #595959;
  font-size: 13px;
}

.search-form {
  padding: 10px 3px;
  display: flex;
  align-items: center;

  .label {
    margin-left: 20px;
  }

  button {
    margin-left: 15px;
  }
}
</style>
