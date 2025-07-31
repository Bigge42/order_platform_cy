<template>
  <div class="builder-container">
    <div class="builder-content" style="left: 0">
      <div style="height: 100%">
        <el-scrollbar style="height: 100%">
          <div class="coder-container">
            <div class="coder-item" style="padding-top: 7px">
              <VolHeader icon="ios-chatbubbles" text="快速代码生成器">
                <template #content>
                  <div style="color: red; font-size: 13px">
                    选择要生成的表，填写必要的参数后点击生成代码
                  </div>
                </template>
              </VolHeader>
              <div class="config">
                <vol-form :label-width="90" ref="form" :formRules="formOptions" :formFields="formFields"></vol-form>
              </div>
            </div>
            <el-alert type="warning" :closable="false">
              1、选择表名后，填写必要的参数，点击生成代码按钮。
              2、VuePath为前端项目路径，适用于前后端分离的项目。
            </el-alert>
            <div class="coder-item">
              <VolHeader icon="md-podium" style="border-bottom: 0" text="数据库表">
                <template #content>
                  <div style="color: red; font-size: 13px">
                    勾选需要生成代码的表
                  </div>
                </template>
              </VolHeader>

              <div class="search-container">
                <div class="search-input-container">
                  <div class="search-item">
                    <span class="search-label">表名：</span>
                    <el-input v-model="tableNameSearch" placeholder="请输入表名" size="small" clearable
                      @input="filterTables"></el-input>
                  </div>
                  <div class="search-action">
                    <el-button type="primary" size="small" @click="generateCode">
                      <i class="el-icon-check"></i>生成代码
                    </el-button>
                    <el-button type="success" size="small" @click="refreshTables">
                      <i class="el-icon-refresh"></i>刷新
                    </el-button>
                  </div>
                </div>
              </div>

              <div class="grid-container" style="padding-bottom: 20px">
                <vol-table ref="table" :tableData="tableData" :height="tableHeight" :columns="columns" :color="false"
                  :index="true" :ck="true" :paginationHide="true" :url="tableUrl"
                  @loadBefore="onLoadTableBefore"></vol-table>
              </div>
            </div>
          </div>
        </el-scrollbar>
      </div>
    </div>
  </div>
</template>
<script>
import VolForm from '@/components/basic/VolForm.vue';
import VolTable from '@/components/basic/VolTable.vue';
import VolHeader from '@/components/basic/VolHeader.vue';
import { ElMessage } from 'element-plus';

export default {
  components: {
    VolForm,
    VolTable,
    VolHeader
  },
  data() {
    return {
      tableHeight: 500,
      tableUrl: "",
      searchText: '',
      formFields: {
        DBServer: '',
        BaseClassName: '',
        NameSpace: '',
        FolderName: '',
        VuePath: '',
        SortName: 'Id',
        ParentMenuId: null,
        ParentId: null
      },
      formOptions: [
        [
          {
            title: '数据库',
            field: 'DBServer',
            type: 'select',
            dataKey: 'dbServer',
            required: true,
            data: [],
            colSize: 4
          },
       
          {
            title: '项目类库',
            field: 'NameSpace',
            type: 'select',
            required: true,
            data: [],
            colSize: 4
          },
          {
            title: '文件夹名',
            field: 'FolderName',
            required: true,
            placeholder: '生成文件所在类库中的文件夹名(文件夹可以不存在)',
            colSize: 4
          }
        ],
        [
         
          {
            title: '排序字段',
            field: 'SortName',
            placeholder: '默认Id',
            colSize: 4
          },
          {
            title: '父菜单ID',
            field: 'ParentMenuId',
            type: 'number',
            placeholder: '放在菜单下的菜单ID',
            colSize: 4
          },
          {
            title: '代码父级ID',
            field: 'ParentId',
            type: 'number',
            placeholder: '上级ID',
            colSize: 4
          }
        ],
        [
          {
            title: 'Vue路径',
            field: 'VuePath',
            type: 'text',
            placeholder: '路径：E:/app/src/views',
            colSize: 6
          },
          // {
          //   title: '强制覆盖',
          //   field: 'ForceOverWrite',
          //   type: 'select',
          //   dataKey: 'dbServer',
          //   required: true,
          //   data: [
          //   { key: 1, value: "覆盖" },
          //   { key: 0, value: "不覆盖" },
          // ],
          //   colSize: 6
          // },

        ]
      ],
      tableData: [],
      allTableData: [],
      columns: [
        {
          field: 'tableName',
          title: '表名',
          width: 150,
          align: 'left'
        },
        {
          field: 'tableCName',
          title: '中文名',
          width: 150,
          align: 'left'
        },
        {
          field: 'generationResult',
          title: '生成结果',
          width: 450,
          align: 'left'
        }
      ],
      selectedTables: [],
      tableNameSearch: ''
    };
  },
  methods: {
    // 通过接口查询表数据
    searchTables() {
      // 调用接口查询数据
      this.http.post('/api/Sys_TableFieldDefinition/GetAllTable', {}).then(res => {
        if (res && res.status && res.data) {
          // 获取表数据和存储完整数据
          this.allTableData = res.data.map(item => ({
            tableName: item.tableName,
            tableCName: item.tableCName || item.tableName,
            generationResult: ''
          }));

          // 初始显示所有数据
          this.tableData = [...this.allTableData];
        }
      });
    },

    // 前端过滤表数据
    filterTables() {
      if (!this.tableNameSearch) {
        // 如果搜索框为空，显示所有数据
        this.tableData = [...this.allTableData];
        return;
      }

      // 使用搜索文本在前端过滤数据
      const searchText = this.tableNameSearch.toLowerCase();
      this.tableData = this.allTableData.filter(item =>
        (item.tableName && item.tableName.toLowerCase().includes(searchText)) ||
        (item.tableCName && item.tableCName.toLowerCase().includes(searchText))
      );
    },
    generateCode() {
      this.$refs.form.validate(valid => {
        if (!valid) {
          ElMessage.error('请填写必要的参数');
          return;
        }

        if (!this.formFields.DBServer) {
          return ElMessage.error('请选择数据库')
        }

        // 获取选中的表
        const selectedRows = this.$refs.table.getSelected();
        if (!selectedRows || selectedRows.length === 0) {
          ElMessage.error('请选择需要生成代码的表');
          return;
        }

        // 构建请求参数
        const params = {
          TableNames: selectedRows.map(row => row.tableName),
          DBServer: this.formFields.DBServer,
          //BaseClassName: this.formFields.BaseClassName,
          NameSpace: this.formFields.NameSpace,
          FolderName: this.formFields.FolderName,
          VuePath: this.formFields.VuePath,
          SortName: this.formFields.SortName || 'Id',
          ParentMenuId: this.formFields.ParentMenuId ? parseInt(this.formFields.ParentMenuId) : null,
          ParentId: this.formFields.ParentId ? parseInt(this.formFields.ParentId) : null
        };

        console.log('生成代码参数:', params);

        // 保存所有表单字段到localStorage
        this.saveFormFields();

        // 调用后端接口
        this.http
          .post('/api/Builder/GenerateCodeByFieldDefinition?vite=1&v3=1&app=0', params, true)
          .then(result => {
            if (result.status) {
              ElMessage.success(result.message || '代码生成成功');

              // 处理返回的结果数据
              if (result.data && Array.isArray(result.data)) {
                // 遍历返回的结果数据
                result.data.forEach(item => {
                  if (item.item1 && item.item2) {
                    // 查找对应的表并更新生成结果
                    const index = this.tableData.findIndex(t => t.tableName === item.item1);
                    if (index !== -1) {
                      this.tableData[index].generationResult = item.item2;

                      // 同时更新缓存数据
                      const cacheIndex = this.allTableData.findIndex(t => t.tableName === item.item1);
                      if (cacheIndex !== -1) {
                        this.allTableData[cacheIndex].generationResult = item.item2;
                      }
                    }
                  }
                });
              } else {
                // 如果没有返回详细结果，则简单标记为"已生成"
                selectedRows.forEach(row => {
                  const index = this.tableData.findIndex(t => t.tableName === row.tableName);
                  if (index !== -1) {
                    this.tableData[index].generationResult = '已生成';

                    // 同时更新缓存数据
                    const cacheIndex = this.allTableData.findIndex(t => t.tableName === row.tableName);
                    if (cacheIndex !== -1) {
                      this.allTableData[cacheIndex].generationResult = '已生成';
                    }
                  }
                });
              }
            } else {
              ElMessage.error(result.message || '代码生成失败');
            }
          })
          .catch(error => {
            ElMessage.error('代码生成失败: ' + error);
          });
      });
    },

    // 保存表单字段到localStorage
    saveFormFields() {
      const formData = {
        DBServer: this.formFields.DBServer,
        //BaseClassName: this.formFields.BaseClassName,
        NameSpace: this.formFields.NameSpace,
        FolderName: this.formFields.FolderName,
        VuePath: this.formFields.VuePath,
        SortName: this.formFields.SortName,
        ParentMenuId: this.formFields.ParentMenuId,
        ParentId: this.formFields.ParentId,
        ForceOverwrite: this.formFields.ForceOverwrite
      };

      // 将表单数据保存到localStorage
      localStorage.setItem('fastCodeGeneratorForm', JSON.stringify(formData));
    },

    // 从localStorage恢复表单字段
    restoreFormFields() {
      const savedFormData = localStorage.getItem('fastCodeGeneratorForm');
      if (savedFormData) {
        try {
          const formData = JSON.parse(savedFormData);
          // 恢复表单数据
          this.formFields = { ...this.formFields, ...formData };
        } catch (e) {
          console.error('恢复表单数据出错:', e);
        }
      }
    },
    // 加载表数据
    loadTableData() {
      // 直接使用searchTables方法加载数据
      this.searchTables();
    },
    onLoadTableBefore(param, callBack) {
      // 阻止继续执行表格默认加载数据的操作
      callBack(false);
    },
    // 刷新表格数据
    refreshTables() {
      this.tableNameSearch = '';
      this.searchTables();
    }
  },
  mounted() {
    let clientHeight = document.documentElement.clientHeight - 300;
    this.tableHeight = clientHeight < 400 ? 400 : clientHeight;

    // 从localStorage恢复表单字段
    this.restoreFormFields();

    // 获取项目类库和业务基类
    this.http.post('/api/builder/GetTableTree', {}, false).then(x => {
      // 项目类库
      if (x.nameSpace) {
        let nameSpace = JSON.parse(x.nameSpace);
        let nameSpaceArr = [];
        for (let index = 0; index < nameSpace.length; index++) {
          nameSpaceArr.push({
            key: nameSpace[index],
            value: nameSpace[index]
          });
        }
        let namespaceField = this.formOptions[0].find(x => x.field === 'NameSpace');
        if (namespaceField) {
          namespaceField.data = nameSpaceArr;
        }
      }

      // 业务基类
      if (x.tableConfigOptions) {
        try {
          const options = JSON.parse(x.tableConfigOptions);
          if (options['代码生成基类']) {
            const baseClassData = options['代码生成基类'].map(item => ({
              key: item.key || item,
              value: item.value || item
            }));
            let baseClassField = this.formOptions[0].find(x => x.field === 'BaseClassName');
            if (baseClassField) {
              baseClassField.data = baseClassData;
            }
          }
        } catch (e) {
          console.error('解析业务基类数据出错:', e);
        }
      }
    });

    // 加载表数据
    this.loadTableData();
  }
};
</script>
<style scoped>
.builder-container {
  widows: 100%;
  position: absolute;
  top: 0px;
  left: 0;
  right: 0;
  display: inline-block;
  bottom: 0;
}

.grid-container>>>tr:hover {
  cursor: pointer;
}

.builder-content {
  position: absolute;
  top: 0px;
  display: inline-block;
  bottom: 0;
  right: 0px;
}

.builder-content .ivu-alert {
  position: relative;
  display: flex;
  padding: 12px 18px 12px 38px;
}

.builder-content .ivu-alert-icon {
  top: 10px;
}

.builder-content .action {
  text-align: right;
  line-height: 33px;
  padding-right: 26px;
  display: flex;
  justify-content: flex-end;
  align-items: center;
}

.builder-content .action i {
  top: 0px;
  position: relative;
}

.builder-content .action>span {
  padding: 0px 6px;
  font-size: 12px;
  letter-spacing: 1px;
  color: #5a5f5e;
  cursor: pointer;
}

.builder-content .action>span:hover {
  color: black;
}

.builder-content .config {
  padding: 15px 15px 0px 15px;
  border-radius: 3px;
  background: #ffffff;
  margin-bottom: 10px;
}

.coder-container {
  background: #eee;
}

.coder-container .coder-item {
  background: white;
  padding: 0px 15px;
}

.search-container {
  padding: 15px 20px;
  background-color: #f9f9f9;
  border-radius: 4px;
  margin-bottom: 10px;
}

.search-input {
  flex: 1;
  display: flex;
  align-items: center;
}

.search-btn {
  margin-left: 10px;
  margin-right: 20px;
}

.btn-container {
  margin-left: 10px;
}

.btn-container .el-button {
  padding: 8px 15px;
}

.search-input-container {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
}

.search-item {
  margin-right: 15px;
  margin-bottom: 5px;
  display: flex;
  align-items: center;
}

.search-item .el-input {
  width: 180px;
}

.search-label {
  margin-right: 5px;
  white-space: nowrap;
  color: #606266;
  font-size: 13px;
}

.search-action {
  display: flex;
  align-items: center;
  margin-left: 10px;
}

.search-action .el-button {
  min-width: 80px;
}
</style>