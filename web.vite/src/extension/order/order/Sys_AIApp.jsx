const appTypes = [
  { key: "chat", value: "对话型" },
  { key: "agent", value: "Agent" },
  { key: "workflow", value: "Workflow" },
];

const statuses = [
  { key: 1, value: "启用" },
  { key: 0, value: "停用" },
];

function setOptionData(options, field, data) {
  options.forEach((row) => {
    row.forEach((item) => {
      if (item.field === field) {
        item.data = data;
      }
    });
  });
}

let extension = {
  components: {
    gridHeader: "",
    gridBody: "",
    gridFooter: "",
    modelHeader: "",
    modelBody: "",
    modelRight: "",
    modelFooter: "",
  },
  tableAction: "Sys_AIApp",
  buttons: { view: [], box: [], detail: [] },
  methods: {
    onInit() {
      setOptionData(this.editFormOptions, "AppType", appTypes);
      setOptionData(this.editFormOptions, "Status", statuses);
      setOptionData(this.searchFormOptions, "AppType", appTypes);
      setOptionData(this.searchFormOptions, "Status", statuses);

      const appTypeColumn = this.columns.find((x) => x.field === "AppType");
      if (appTypeColumn) appTypeColumn.bind = { data: appTypes };

      const statusColumn = this.columns.find((x) => x.field === "Status");
      if (statusColumn) statusColumn.bind = { data: statuses };
    },
    modelOpenAfter() {
      if (this.currentAction === "Add") {
        this.editFormFields.AppType = "chat";
        this.editFormFields.SortNo = 0;
        this.editFormFields.Status = 1;
      }
    },
  },
};

export default extension;
