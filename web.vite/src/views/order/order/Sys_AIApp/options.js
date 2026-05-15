export default function () {
  const table = {
    key: "Id",
    footer: "Foots",
    cnName: "AI小助手应用管理",
    name: "Sys_AIApp",
    newTabEdit: false,
    url: "/Sys_AIApp/",
    sortName: "SortNo",
  };
  const editFormFields = {
    AppName: "",
    AppType: "",
    PlatformAppId: "",
    PlatformAppKey: "",
    Description: "",
    Icon: "",
    SortNo: 0,
    Status: 1,
    Remark: "",
  };
  const editFormOptions = [
    [
      { title: "应用名称", required: true, field: "AppName" },
      { title: "应用类型", required: true, field: "AppType", type: "select", data: [] },
    ],
    [
      { title: "平台AppID", required: true, field: "PlatformAppId" },
      { title: "平台AppKey", required: true, field: "PlatformAppKey", type: "password" },
    ],
    [
      { title: "应用图标", field: "Icon" },
      { title: "排序号", required: true, field: "SortNo", type: "number" },
      { title: "状态", required: true, field: "Status", type: "select", data: [] },
    ],
    [{ title: "应用描述", field: "Description", type: "textarea", colSize: 12 }],
    [{ title: "备注", field: "Remark", type: "textarea", colSize: 12 }],
  ];
  const searchFormFields = { AppName: "", AppType: "", Status: "" };
  const searchFormOptions = [
    [
      { title: "应用名称", field: "AppName", type: "like" },
      { title: "应用类型", field: "AppType", type: "select", data: [] },
      { title: "状态", field: "Status", type: "select", data: [] },
    ],
  ];
  const columns = [
    { field: "Id", title: "主键ID", type: "long", width: 80, hidden: true, readonly: true, require: true, align: "left" },
    { field: "AppName", title: "应用名称", type: "string", sort: true, link: true, width: 140, require: true, align: "left" },
    { field: "AppType", title: "应用类型", type: "string", sort: true, width: 100, require: true, align: "left" },
    { field: "PlatformAppId", title: "平台AppID", type: "string", width: 160, require: true, align: "left" },
    { field: "PlatformAppKey", title: "平台AppKey", type: "string", width: 120, hidden: true, require: true, align: "left" },
    { field: "Description", title: "应用描述", type: "string", width: 220, align: "left" },
    { field: "Icon", title: "应用图标", type: "string", width: 100, align: "left" },
    { field: "SortNo", title: "排序号", type: "int", sort: true, width: 80, require: true, align: "left" },
    { field: "Status", title: "状态", type: "int", sort: true, width: 80, require: true, align: "left" },
    { field: "Remark", title: "备注", type: "string", width: 160, align: "left" },
    { field: "CreateDate", title: "创建时间", type: "datetime", width: 150, align: "left" },
    { field: "CreateID", title: "创建人ID", type: "int", width: 80, hidden: true, align: "left" },
    { field: "Creator", title: "创建人", type: "string", width: 100, align: "left" },
    { field: "Modifier", title: "修改人", type: "string", width: 100, align: "left" },
    { field: "ModifyDate", title: "修改时间", type: "datetime", width: 150, align: "left" },
    { field: "ModifyID", title: "修改人ID", type: "int", width: 80, hidden: true, align: "left" },
  ];
  return {
    table,
    key: table.key,
    tableName: table.name,
    tableCNName: table.cnName,
    newTabEdit: false,
    editFormFields,
    editFormOptions,
    searchFormFields,
    searchFormOptions,
    columns,
    detail: { columns: [] },
    details: [],
  };
}
