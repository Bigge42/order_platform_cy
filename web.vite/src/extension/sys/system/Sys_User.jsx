import { defineAsyncComponent } from 'vue'
let extension = {
  components: {
    //动态扩充组件或组件路径
    //表单header、content、footer对应位置扩充的组件
    gridHeader: defineAsyncComponent(() => import('./Sys_User/Sys_UserGridHeader.vue')),
    gridBody: '',
    gridFooter: '',
    //弹出框(修改、编辑、查看)header、content、footer对应位置扩充的组件
    modelHeader: '',
    modelBody: '',
    modelFooter: ''
  },
  text: '',
  buttons: [], //扩展的按钮
  methods: {
    initImg() {
      //自定义图片显示位置
      //代码生成器配置：
      //1、将图片字段编辑行放在第一行，
      //2、第二、三行的编辑字段数量应该比第一行字段少一个,如：第一行设置了5个编辑字段，第二、三行的字段编辑字段只应配置4个
      const option = this.getFormOption('HeadImageUrl')
      if (option) {
        //通过css控制图片显示位置
        option.itemStyle = 'position: absolute;right: 20px;'
        //控制上传图片大小
        option.imgOption = {
          icon: 'el-icon-camera-solid',
          iconSize: 60,
          height: 160, //图片高度
          width: 160 //图片宽度
        }
        this.boxOptions.width = 650
        if (this.$global.labelPosition == 'left') {
          option.imgOption.height = 125
          option.imgOption.width = 110
          this.boxOptions.width = 800
        }
        //指定弹出框宽度
      }
      // this.height = this.height - 80;
      this.editFormOptions.forEach((x) => {
        x.forEach((item) => {
          if (item.field == 'RoleIds') {
            item.required = true
          }
        })
      })
    },
    //事件扩展
    onInit() {
      //初始化头像配置
      this.initImg()

      let hasPwd = this.buttons.some((x) => {
        return x.value == 'Add' || x.value == 'Update'
      })

      let hasDel = this.buttons.some((x) => {
        return x.value == 'Delete'
      })

      this.columns.push({
        title: '在线状态',
        align: 'center',
        //fixed: 'right',
        width: 90,
        render: (h, { row, column, index }) => {
          return h(
            <div style={{ 'font-size': '13px' }}>
              <span
                style={{
                  'border-radius': '50%',
                  height: '8px',
                  width: '8px',
                  display: 'inline-block',
                  'margin-right': '5px',
                  background: row.IsOnline ? '#01dc0a' : '#bdbdbd'
                }}
              ></span>
              {this.$ts(row.IsOnline ? '在线' : '离线')}
            </div>
          )
        }
      })

      this.useAuth = false
      this.columns.push({
        title: '操作',
        hidden: false,
        align: 'center',
        fixed: 'right',
        width: 120 + (this.useAuth ? 40 : 0),
        render: (h, { row, column, index }) => {
          return h(
            <div>
              {hasPwd ? (
                <el-tooltip
                  class="box-item"
                  effect="dark"
                  content={this.$ts('强制下线')}
                  placement="top"
                >
                  <el-button
                    type="danger"
                    link
                    disabled={row.IsOnline == 0}
                    onClick={($e) => {
                      this.offLine(row)
                    }}
                    icon="SwitchButton"
                  ></el-button>
                </el-tooltip>
              ) : (
                ''
              )}
              {this.useAuth ? (
                <el-tooltip
                  class="box-item"
                  effect="dark"
                  content={this.$ts('用户权限')}
                  placement="top"
                >
                  <el-button
                    type="success"
                    link
                    onClick={($e) => {
                      this.$refs.gridHeader.openAuth(row)
                    }}
                    icon="User"
                  ></el-button>
                </el-tooltip>
              ) : (
                ''
              )}
              {hasPwd ? (
                <el-button
                  onClick={($e) => {
                    this.$refs.gridHeader.open(row)
                  }}
                  type="primary"
                  link
                  icon="Unlock"
                ></el-button>
              ) : (
                ''
              )}
              <el-button
                onClick={($e) => {
                  this.edit(row)
                }}
                type="success"
                link
                icon="Edit"
              ></el-button>
              {hasDel ? (
                <el-button
                  link
                  onClick={($e) => {
                    this.del(row)
                  }}
                  type="danger"
                  icon="Delete"
                ></el-button>
              ) : (
                ''
              )}
            </div>
          )
        }
      })
    },
    offLine(row) {
      this.$confirm(
        this.$tst('确定要强制用户【{$ts}】下线吗?', row.UserTrueName),
        this.$ts('提示'),
        {
          confirmButtonText: this.$ts('确定'),
          cancelButtonText: this.$ts('取消'),
          'show-cancel-button': true,
          center: true,
          callback: (action) => {
            if (action == 'confirm') {
              this.loginout(row)
            }
          }
        }
      )
    },
    loginout(row) {
      const url = 'api/signalRUser/loginout?userName=' + row.UserName
      this.http.get(url, {}, true).then((result) => {
        this.$message.success(result)
        row.IsOnline = 0
      })
    },
    onInited() {},
    searchAfter(rows, result) {
      this.useAuth = result.extra
      return true
    },
    addAfter(result) {
      //用户新建后，显示随机生成的密码
      if (!result.status) {
        return true
      }
      //显示新建用户的密码
      //2020.08.28优化新建成后提示方式
      this.$confirm(result.message, this.$ts('新建用户成功'), {
        confirmButtonText: this.$ts('确定'),
        cancelButtonText: this.$ts('取消'),
        type: 'success',
        center: true
      }).then(() => {})

      this.boxModel = false
      this.refresh()
      return false
    },
    modelOpenAfter() {
      //点击弹出框后，如果是编辑状态，禁止编辑用户名，如果新建状态，将用户名字段设置为可编辑
      let isEDIT = this.currentAction !='Add'
      this.editFormOptions.forEach((item) => {
        item.forEach((x) => {
          if (x.field == 'UserName') {
            x.disabled = isEDIT
          }
        })
        //不是新建，性别默认值设置为男
        if (!isEDIT) {
          this.editFormFields.Gender = '0'
        }
      })
    }
  }
}
export default extension
