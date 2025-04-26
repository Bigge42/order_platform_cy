<!--
*Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *业务请在@/extension/sys/Sys_User.jsx此处编写
 -->
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
  >
  </view-grid>
</template>
<script>
import extend from "@/extension/sys/system/Sys_User.jsx";
import { ref, defineComponent } from "vue";
export default defineComponent({
  setup() {
    const table = ref({
      key: "User_Id",
      footer: "Foots",
      cnName: "用户管理",
      name: "Sys_User",
      newTabEdit: false,
      url: "/Sys_User/",
      sortName: "User_Id",
    });
    const editFormFields = ref({
      UserName: "",
      UserTrueName: "",
      HeadImageUrl: "",
      Gender: "",
      PhoneNo: "",
      Email: "",
      Remark: "",
      DeptIds: "",
      RoleIds: "",
      PostId: "",
    });
    const editFormOptions = ref([
      [
        { title: "帐号", required: true, field: "UserName", disabled: true },
        { title: "姓名", required: true, field: "UserTrueName", type: "text" },
        { title: "头像", field: "HeadImageUrl", type: "img" },
      ],
      [
        { dataKey: "gender", data: [], title: "性别", field: "Gender", type: "select" },
        { title: "手机号", field: "PhoneNo" },
      ],
      [
        { title: "邮箱", field: "Email", type: "mail" },
        { title: "备注", field: "Remark", type: "text" },
      ],
      [
        {
          dataKey: "部门级联",
          data: [],
          title: "部门",
          field: "DeptIds",
          colSize: 12,
          type: "treeSelect",
        },
      ],
      [
        {
          dataKey: "tree_roles",
          data: [],
          title: "角色",
          field: "RoleIds",
          colSize: 12,
          type: "treeSelect",
        },
      ],
      [
        {
          dataKey: "岗位",
          data: [],
          title: "岗位",
          field: "PostId",
          colSize: 12,
          type: "treeSelect",
        },
      ],
    ]);
    const searchFormFields = ref({
      UserName: "",
      UserTrueName: "",
      Gender: "",
      Token: "",
      CreateDate: "",
      PhoneNo: "",
    });
    const searchFormOptions = ref([
      [
        { title: "帐号", field: "UserName" },
        { title: "姓名", field: "UserTrueName" },
        { dataKey: "gender", data: [], title: "性别", field: "Gender", type: "select" },
        { title: "注册时间", field: "CreateDate", type: "datetime" },
      ],
      [
        { title: "Token", field: "Token" },
        { title: "手机号", field: "PhoneNo" },
      ],
    ]);
    const columns = ref([
      {
        field: "User_Id",
        title: "User_Id",
        type: "int",
        width: 90,
        hidden: true,
        readonly: true,
        require: true,
        align: "left",
      },
      {
        field: "UserName",
        title: "帐号",
        type: "string",
        link: true,
        width: 100,
        readonly: true,
        require: true,
        align: "left",
      },
      {
        field: "UserTrueName",
        title: "姓名",
        type: "string",
        width: 90,
        require: true,
        align: "left",
      },
      {
        field: "Gender",
        title: "性别",
        type: "int",
        bind: { key: "gender", data: [] },
        width: 80,
        align: "left",
      },
      { field: "HeadImageUrl", title: "头像", type: "img", width: 70, align: "left" },
      {
        field: "Role_Id",
        title: "(角色)不用字段",
        type: "int",
        width: 150,
        hidden: true,
        align: "left",
      },
      {
        field: "RoleIds",
        title: "角色",
        type: "string",
        bind: { key: "tree_roles", data: [] },
        width: 220,
        hidden: true,
        align: "left",
      },
      {
        field: "PostId",
        title: "岗位",
        type: "string",
        bind: { key: "岗位", data: [] },
        width: 120,
        align: "left",
      },
      { field: "Email", title: "邮箱", type: "string", width: 120, align: "left" },
      {
        field: "Token",
        title: "Token",
        type: "string",
        width: 180,
        hidden: true,
        align: "left",
      },
      {
        field: "CreateDate",
        title: "注册时间",
        type: "datetime",
        width: 120,
        readonly: true,
        align: "left",
      },
      {
        field: "PhoneNo",
        title: "手机号",
        type: "string",
        width: 150,
        hidden: true,
        align: "left",
      },
      {
        field: "CreateID",
        title: "CreateID",
        type: "int",
        width: 90,
        hidden: true,
        align: "left",
      },
      {
        field: "Creator",
        title: "创建人",
        type: "string",
        width: 90,
        readonly: true,
        align: "left",
      },
      {
        field: "Enable",
        title: "是否可用",
        type: "sbyte",
        bind: { key: "enable", data: [] },
        width: 90,
        hidden: true,
        align: "left",
      },
      {
        field: "ModifyID",
        title: "ModifyID",
        type: "int",
        width: 90,
        hidden: true,
        align: "left",
      },
      {
        field: "Modifier",
        title: "修改人",
        type: "string",
        width: 130,
        hidden: true,
        readonly: true,
        align: "left",
      },
      {
        field: "ModifyDate",
        title: "修改时间",
        type: "datetime",
        width: 90,
        hidden: true,
        readonly: true,
        align: "left",
      },
      {
        field: "LastLoginDate",
        title: "最后登陆时间",
        type: "datetime",
        width: 150,
        hidden: true,
        align: "left",
      },
      {
        field: "LastModifyPwdDate",
        title: "最后密码修改时间",
        type: "datetime",
        width: 150,
        hidden: true,
        align: "left",
      },
      { field: "Remark", title: "备注", type: "string", width: 120, align: "left" },
      {
        field: "OrderNo",
        title: "排序号",
        type: "int",
        width: 90,
        hidden: true,
        align: "left",
      },
      {
        field: "DeptIds",
        title: "部门",
        type: "string",
        bind: { key: "部门级联", data: [] },
        width: 220,
        hidden: true,
        align: "left",
      },
    ]);
    const detail = ref({ columns: [] });
    const details = ref([]);
    return {
      table,
      extend,
      editFormFields,
      editFormOptions,
      searchFormFields,
      searchFormOptions,
      columns,
      detail,
      details,
    };
  },
});
</script>
