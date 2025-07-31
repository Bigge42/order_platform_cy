<template>
  <view-grid
    ref="grid"
    :columns="columns"
    :detail="detail"
    :details="details"
    :editFormFields="editFormFields"
    :editFormOptions="editFormOptions"
    :searchFormFields="searchFormFields"
    :searchFormOptions="searchFormOptions"
    :table="table"
    :extend="extend"
    :onInit="onInit"
    :onInited="onInited"
    :searchBefore="searchBefore"
    :searchAfter="searchAfter"
    :addBefore="addBefore"
    :updateBefore="updateBefore"
    :rowClick="rowClick"
    :modelOpenBefore="modelOpenBefore"
    :modelOpenAfter="modelOpenAfter"
  >
    <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
    <template #gridHeader> </template>
  </view-grid>
</template>
<script setup lang="jsx">
import extend from "@/extension/sys/notification/Sys_Notification.jsx";
import viewOptions from "./Sys_Notification/options.js";
import { ref, reactive, getCurrentInstance, watch, onMounted } from "vue";
const grid = ref(null);
const { proxy } = getCurrentInstance();
//http请求，proxy.http.post/get
const {
  table,
  editFormFields,
  editFormOptions,
  searchFormFields,
  searchFormOptions,
  columns,
  detail,
  details,
} = reactive(viewOptions());

columns.forEach((x) => {
  if (x.field == "TargetObjectText") {
    //开启表格内容超出提示信息
    x.showOverflowTooltip = true;
  }
});

let target = {};
const initTargetOSelectTable = () => {
  //1用户，2角色，3，部门，4=岗位
  let option = gridRef.getFormOption("TargetObjectText");
  if (!editFormFields.TargetObjectType) {
    editFormFields.TargetObjectType = "1";
  }
  switch (editFormFields.TargetObjectType) {
    case "1": //用户
      target = { key: "User_Id", field: "UserTrueName" };
      option.url = "api/sys_user/getPageData";
      option.columns = [
        {
          field: "UserTrueName",
          title: "姓名",
          width: 100,
          search: true,
        },
        { field: "Email", title: "邮箱", type: "string", width: 140, align: "left" },
        { field: "Remark", title: "备注", type: "string", width: 120, align: "left" },
      ];
      break;
    case "2": //角色
      target = { key: "Role_Id", field: "RoleName" };
      option.url = "api/sys_role/getPageData";
      option.columns = [
        { field: "RoleName", title: "角色", width: 90, search: true },
        { field: "Role_Id", title: "角色ID", type: "int", width: 70 },
        { field: "Creator", title: "创建人", type: "string", width: 90 },
        { field: "CreateDate", title: "创建时间", type: "datetime", width: 140 },
      ];
      break;
    case "3": //部门
      target = { key: "DepartmentId", field: "DepartmentName" };
      option.url = "api/Sys_Department/getPageData";
      option.columns = [
        { field: "DepartmentName", title: "名称", width: 110, search: true },
        { field: "DepartmentCode", title: "编号", width: 90 },
        { field: "Creator", title: "创建人", type: "string", width: 90 },
        { field: "CreateDate", title: "创建时间", type: "datetime", width: 140 },
      ];
      break;
    case "4": //岗位
      target = { key: "PostId", field: "PostName" };
      option.url = "api/Sys_Post/getPageData";
      option.columns = [
        { field: "PostName", title: "岗位名称", search: true },
        { field: "PostCode", title: "岗位编号", width: 100 },
        { field: "Creator", title: "创建人", type: "string", width: 90 },
        { field: "CreateDate", title: "创建时间", type: "datetime", width: 140 },
      ];
      break;
  }
};

const clearTarget = () => {
  editFormFields.TargetObjectText = "";
  editFormFields.TargetObjectValue = "";
};

const initFormOption = () => {
  gridRef.getFormOption("NotificationContent").height = 230;
  const option = gridRef.getFormOption("TargetObjectText");
  // option.readonly = true;
  option.inputReadonly = true;
  option.extra = {
    render: (h, {}) => {
      return (
        <div>
          <el-button
            type="primary"
            style="margin-left:0"
            link
            onClick={() => {
              clearTarget();
            }}
          >
            <i class="el-icon-delete">清除</i>
          </el-button>
        </div>
      );
    },
  };
  option.placeholder = "请选择";
  option.type = "selectTable";
  option.single = false;
  initTargetOSelectTable();
  //隐藏分页
  option.paginationHide = false;
  option.pagination = { sizes: [20, 30, 40, 60, 90], size: 30 };

  //选中table数据后，回写到表单
  option.onSelect = (rows) => {
    //给表单字段设置值
    editFormFields.TargetObjectText = rows
      .map((x) => {
        return x[target.field];
      })
      .join(",");

    editFormFields.TargetObjectValue = rows
      .map((x) => {
        return x[target.key];
      })
      .join(",");
  };

  //设置过滤条件
  //(输入框搜索)表格数据加载前处理
  option.loadBefore = (param, callback) => {
    callback(true);
  };
  //表格数据加载后处理
  option.loadAfter = (rows, callback, result) => {
    callback(true);
  };
};

let gridRef; //对应[表.jsx]文件中this.使用方式一样
//生成对象属性初始化
const onInit = async ($vm) => {
  gridRef = $vm;
  initFormOption();
  const tableOption = gridRef.getFormOption("BusinessFunction");
  let permission = proxy.$store.getters.getPermission();
  const tables = permission
    .filter((x) => {
      return (!x.children || !x.children.length) && x.tableName && x.tableName.length > 2;
    })
    .map((x) => {
      return { key: x.name, value: x.name };
    });
  tableOption.type = "select";
  tableOption.data = tables;
  //通知对象类型选择
  const typeOption = gridRef.getFormOption("TargetObjectType");
  typeOption.filter = false;
  typeOption.onChange = (val) => {
    clearTarget();
    initTargetOSelectTable();
  };
};
//生成对象属性初始化后,操作明细表配置用到
const onInited = async () => {
  gridRef.boxOptions.width = 1100;
};

const send = (row) => {
  proxy.http.post("api/Sys_Notification/send", row, true).then((x) => {
    if (!row.PublishStatus) {
      gridRef.search();
    }
    proxy.$message.success(proxy.$ts("发送成功"));
  });
};

//表格上添加自定义按钮
columns.push({
  title: "操作",
  field: "操作",
  width: 80,
  align: "center", // 'left',
  fixed: "right", //固定到最右边，也可以设置为left固定到左边
  render: (h, { row, column, index }) => {
    return (
      <el-button
        onClick={($e) => {
          send(row, column, index, $e);
        }}
        type="primary"
        plain
        link
        style="height:26px; padding: 10px !important;"
      >
        推送消息
      </el-button>
    );
  },
});
const searchBefore = async (param) => {
  //界面查询前,可以给param.wheres添加查询参数
  //返回false，则不会执行查询
  return true;
};
const searchAfter = async (rows, result) => {
  return true;
};
const addBefore = async (formData) => {
  //新建保存前formData为对象，包括明细表，可以给给表单设置值，自己输出看formData的值
  return true;
};
const updateBefore = async (formData) => {
  //编辑保存前formData为对象，包括明细表、删除行的Id
  return true;
};
const rowClick = ({ row, column, event }) => {
  //查询界面点击行事件
  // grid.value.toggleRowSelection(row); //单击行时选中当前行;
};
const modelOpenBefore = async (row) => {
  //弹出框打开后方法
  return true; //返回false，不会打开弹出框
};
const modelOpenAfter = (row) => {
  initTargetOSelectTable();
  //弹出框打开后方法,设置表单默认值,按钮操作等
};
//监听表单输入，做实时计算
//watch(() => editFormFields.字段,(newValue, oldValue) => {	})
//对外暴露数据
defineExpose({});
</script>
