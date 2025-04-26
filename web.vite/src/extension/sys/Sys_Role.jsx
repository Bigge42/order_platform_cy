import gridHeader from './Sys_RoleGridHeade.vue';
import auth from './Sys_RoleAuthDataOptions.js'
let extension = {
  components: {
    //动态扩充组件或组件路径
    //表单header、content、footer对应位置扩充的组件
    gridHeader: gridHeader,
    gridBody: '',
    gridFooter: '',
    //弹出框(修改、编辑、查看)header、content、footer对应位置扩充的组件
    modelHeader: '',
    modelBody: '',
    modelFooter: ''
  },
  buttons: [], //扩展的按钮
  tableAction: 'Sys_Role',
  methods: {
    //事件扩展
    onInited() {
      let authData =auth;
      //  [
      //   { key: 1, value: '全部' },
      //   //  { key: 10, value: "本组织+本角色以及下数据" },
      //   { key: 20, value: '本组织(部门)及下数据' },
      //   { key: 30, value: '本组织(部门)数据' },
      //   { key: 40, value: '本角色以及下数据' },
      //   { key: 50, value: '本角色数据' },
      //   { key: 60, value: '仅自己数据' }
      // ];

      // this.height = this.height - 80;
      this.editFormOptions.forEach((x) => {
        x.forEach((item) => {
          if (item.field == 'ParentId') {
            item.title = '上级角色';
            //设置任意节点都能选中(默认只能选中最后一个节点)
            item.changeOnSelect = true;
          } else if (item.field == 'DatAuth') {
            //设置任意节点都能选中(默认只能选中最后一个节点)
            item.type = 'select';
            item.data = authData;
          }
        });
      });
      this.columns.forEach((x) => {
        if (x.field == 'DatAuth') {
          x.bind = { data: authData };
        }
      });
    },
    initService() {
      if (!this.$global.db) {
        return;
      }
      try {
        let list = this.$store.getters.getServiceList().map((x) => {
          return {
            key: x.id,
            value: x.name
          };
        });
        if (!list.length) {
          return;
        }
        this.editFormFields.DbServiceId=null;
        this.editFormOptions.push([{field:"DbServiceId",title:"租户",data:list,type:"select"}])
      } catch (error) {}
    },
    onInit() {
      this.initService();
      //设置treetable的唯一值字段(这个字段的值在表里面必须是唯一的)
      this.rowKey = 'Role_Id';
      this.columns.find((x) => {
        return x.field == 'ParentId';
      }).hidden = true;

      if (
        this.buttons.some((x) => {
          return x.value == 'Add' || x.value == 'Update';
        })
      ) {
        this.columns.push({
          title: '权限',
          field: '权限',
          width: 50,
          fixed: 'right',
          align: 'center',
          render: (h, { row, column, index }) => {
            return (
              <div>
                <el-tooltip
                  class="box-item"
                  effect="dark"
                  content={this.$ts('权限设置')}
                  placement="top"
                >
                  <el-button
                    link
                    onClick={($e) => {
                      this.openModel(row);
                    }}
                    type="success"
                    plain
                    size="small"
                    style="padding: 5px !important;margin: 0;"
                  >
                    <i style="font-size:16px" class="el-icon-user"></i>
                  </el-button>
                </el-tooltip>
              </div>
            );
          }
        });
      }
    },
    openModel(row) {
      this.$refs.gridHeader.open(row);
    },
    /***加载后台数据见Sys_RoleController.cs文件***/
    loadTreeChildren(tree, treeNode, resolve) {
      //加载子节点
      let url = `api/role/getTreeTableChildrenData?roleId=${tree.Role_Id}`;
      this.http.post(url, {}).then((result) => {
        resolve(result.rows);
      });
    },
    /***加载后台数据见Sys_RoleController.cs文件***/
    searchBefore(params) {
      //判断加载根节点或子节点
      //没有查询条件，默认查询返回所有根节点数据
      if (!params.wheres.length) {
        params.value = 1;
      }
      return true;
    },
    addAfter() {
      this.initDicKeys();
      return true;
    },
    updateAfter() {
      this.initDicKeys();
      return true;
    },
    delAfter() {
      this.initDicKeys();
      return true;
    }
  }
};
export default extension;
