<template>
  <div class="container">
    <div class="header">
      <el-alert type="error" title="表单分组" :closable="false">
        表单分组支持自定义Tabs选项卡分组配置，解决字段过多时显示的问题。配置与前面的【表单组件】配置相同，只需要增加group属性
      </el-alert>
    </div>
    <div class="content">
      <VolForm ref="form" :loadKey="true" :formFields="formFields" :formRules="formRules">
        <div style="text-align: center; margin-top: 20px; width: 100%">
          <el-button type="primary" plain @click="getForm">获取表单</el-button>
          <el-button type="success" plain @click="reset">重置表单</el-button>
          <el-button type="success" plain @click="resetData">刷新数据源</el-button>
        </div>
      </VolForm>
    </div>
  </div>
</template>
<script lang="jsx">
import VolForm from "@/components/basic/VolForm.vue";
import VolHeader from "@/components/basic/VolHeader.vue";
export default {
  props: {
    showBtn: { type: Boolean, default: true },
  },
  components: {
    VolForm,
    VolHeader,
  },
  methods: {
    setReadonlyStaus(status) {
      this.formRules.forEach((rules) => {
        rules.forEach((option) => {
          option.readonly = status;
        });
      });
    },
    getForm() {
      this.$refs.form.validate(() => {
        this.$message.error(JSON.stringify(this.formFields));
      });
    },
    reset() {
      //重置表单，重置时可指定重置的值，如果没有指定重置的值，默认全部清空
      let data = { decimalVal: 100, numberVal: 100 };
      this.$refs.form.reset(data);
      this.$message.error("表单已重置");
    },
    popover() {
      this.$message.success("点击了提示");
    },
    resetData(){
      this.$refs.form.initSource(true);
      this.$message.success("刷新成功");
    }
  },
  data() {
    return {
      formFields: {
        uploadFile: [
          {
            name: "exceltest.xlsx",
            path:
              "https://imgs-1256993465.cos.ap-chengdu.myqcloud.com/github/exceltest.xlsx",
          },
        ],
        img: [
          {
            name: "060222.jpg",
            path:
              "http://www.baidu.com/Upload/Tables/App_Expert/202103061753415708/060222.jpg",
          },
        ],

        inputVal: null,
        decimalVal: 100,
        numberVal: 100,
        selectVal: null,
        phoneValue: null,
        mailVal: null,
        extraVal: null,

        customVal: null,
        dateTimeVal: null,
        dateVal: null,
        dateRangeVal: [null, null], //日期范围是数组
        monthVal: null,

        cascaderVal: [], //级联的默认值是数组
        treeVal: null,

        selectVal: null,
        selectAutoVal: null,
        selectListVal: [], //多选值

        selectTableVal: "",
        selectNameVal: "",
        selectCodeVal: "",
        selectTextVal: "纯文本显示",

        radioVal: "1", //设置单选框默认值
        checkBoxVal: [], //多选框是数组
        editorValue: "",
      },
      formRules: [
        //表单配置formRules数据格式是二维数组，表示每个标签所在位置:[[{},{}]]
        [
          {
            type: "text",
            title: "输入框",
            required: true,
            placeholder: "输入框。。。",
            field: "inputVal",
            group: "输入类型",
          },
          {
            type: "decimal",
            title: "小数",
            //required: true,
            placeholder: "只能输入小数",
            field: "decimalVal",
            group: "输入类型",
          },
          {
            type: "number",
            title: "整数",
            placeholder: "只能输入整数",
            field: "numberVal",
            min: 0, //限制数字大小
            max: 10,
            group: "输入类型",
          },
          {
            title: "额外标签",
            field: "extraVal",
            type: "text",
            extra: {
              style: "color:#2196F3;cursor: pointer;font-size:12px",
              icon: "el-icon-search", //显示图标
              //text: "点击", //显示文本
              //触发事件
              click: (item) => {
                this.$message.error("点击标签触发的事件");
              },
            },
            group: "输入类型",
          },
          {
            title: "手机",
            // required: true,
            field: "phoneValue",
            type: "phone",
            group: "输入类型",
          },
        ],
        [
          {
            title: "邮箱",
            //required: true,
            field: "mailVal",
            type: "mail",
            group: "输入类型",
          },
          {
            title: "自定义验证",
            // required: true,
            field: "customVal",
            placeholder: "只能输入值：123",
            type: "text",
            validator: (rule, val) => {
              if (!val && val != "0") {
                return "";
              }
              if (val != "123") {
                return "自定设置必须输入123";
              }
              return "";
            },
            group: "输入类型",
          },
          {
            title: "日期(datetime)",
            // required: true,
            placeholder: "限制时间范围",
            field: "dateTimeVal",
            type: "datetime", //
            min: "2021-07-01", //设置日期选择范围
            max: Date.now(), // //设置日期选择范围
            onChange: (val) => {
              console.log("选择日期:" + val);
            },
            group: "输入类型",
          },
          {
            title: "日期(date)",
            // required: true,
            placeholder: "限制时间范围",
            field: "dateVal",
            type: "datetime",
            min: "2021-07-01", //设置日期选择范围
            max: Date.now(), // //设置日期选择范围
            onChange: (val) => {
              console.log("选择日期:" + val);
            },
            group: "输入类型",
          },
          {
            title: "日期区间",
            field: "dateRangeVal",
            type: "date",
            range: true,
            onChange: (val) => {
              console.log("日期:" + val);
            },
            group: "输入类型",
          },
        ],
        [
          {
            colSize: 12,
            render: (h) => {
              return (
                <el-alert
                  type="success"
                  title=""
                  style="line-height: 21px; padding: 0 0 5px 5px;"
                >
                  在任意位置可以使用render
                  jsx语法添加一些自定义组件或者代码，比如现在的这段代码
                </el-alert>
              );
            },
            group: "输入类型",
          },
        ],
        [
          {
            dataKey: "top",
            title: "是否选择",
            required: true,
            field: "switchVal",
            activeText: "已选择",
            inactiveText: "未选择",
            type: "switch",
            group: "输入类型",
          },
          {
            title: "级联",
            field: "cascaderVal", //注意上面formFields属性cascader是数组
            placeholder: "配置数据源后自动绑定级联",
            checkStrictly: true, //是否可以选择任意一级，false只能选择最后一级
            type: "cascader", //注意这个是值是数组
            dataKey: "tree_roles", //配置数据源(见菜单下拉框绑定设置中的级联角色自定义sql)
            data: [], //绑定的数据源dataKey，
            group: "输入类型",
          },
          {
            title: "树形级联",
            dataKey: "部门级联",
            // 如果这里绑定了data数据，后台不会加载此数据源
            data: [],
            field: "treeVal",
            multiple: true, //设置为多选
            readonly: false,
            type: "treeSelect",
            group: "输入类型",
          },
          {
            dataKey: "city", //后台下拉框对应的数据字典编号
            data: [], //loadKey设置为true,会根据dataKey从后台的下拉框数据源中自动加载数据
            title: "下拉框",
            filter: true,
            //  required: true, //设置为必选项
            field: "selectVal",
            type: "select",
            group: "输入类型",
          },
          {
            title: "自动完成",
            autocomplete: true, //设置为自动完成
            dataKey: "city",
            placeholder: "自动填充",
            data: [],
            //如果这里绑定了data数据，后台不会加载此数据源
            // data: [
            //     { key: "北京市", value: "北京市" },
            //     { key: "上海市", value: "上海市" },
            // ],
            field: "selectAutoVal",
            type: "select",
            extra: {
              render: (h) => {
                return (
                  <el-popover
                    placement="top-start"
                    title="提示"
                    width="200"
                    trigger="hover"
                    content="下拉框选择可以输入，不存在的数据自动填入"
                  >
                    {{
                      reference:()=>{
                        return h(
                        <i
                          style="color:#9E9E9E;font-size:13px;"
                          onClick={() => {
                            this.popover();
                          }}
                          class="el-icon-warning-outline"
                        ></i>
                      )
                      }
                    }}
                  </el-popover>
                );
              },
            },
            group: "输入类型",
          },
        ],
        [
          {
            //详细配置见企业版文档[表单select下拉框table搜索]:
            title: "下拉框table搜索",
            field: "selectTableVal",
            type: "selectTable",
            url: "api/Sys_Region/getPageData",
            group: "其他类型",
            columns: [
              { field: "code", title: "编码", type: "string", width: 50 },
              { field: "name", title: "名称", type: "string", width: 70 },
              { field: "level", title: "级别", type: "int", width: 40 },
              { field: "mername", title: "完整地址", type: "string", width: 140 },
            ],
            //选择数据时
            onSelect: (rows) => {
              //给表单字段设置值
              this.formFields.selectTableVal = rows[0].name;
              this.formFields.selectNameVal = rows[0].mername;
              this.formFields.selectCodeVal = rows[0].code;

              this.formFields.selectTextVal = rows[0].name;
            },
            //输入搜索时
            loadBefore: (param, callback) => {
              param.wheres.push({
                name: "mername",
                value: this.formFields.selectTableVal,
                displayType: "like",
              });
              callback(true);
            },
            paginationHide: false, //显示分页
            //colSize: 3, //设置宽度100%
          },
          {
            title: "搜索结果(只读)",
            field: "selectCodeVal",
            readonly: true,
            group: "其他类型",
          },
          {
            title: "搜索结果2(只读)",
            field: "selectNameVal",
            readonly: true,
            group: "其他类型",
          },
          {
            title: "搜索结果3(纯文本)",
            field: "selectTextVal",
            type: "label",
            colSize: 4,
            group: "其他类型",
          },
        ],
        [
          {
            title: "多选(配置colSize属性指定标签宽度)",
            data: [],
            dataKey: "city",
            field: "selectListVal",
            type: "selectList", //上面selectListVal值为数组
            group: "其他类型",
            colSize: 12,
          },
        ],
        [
          {
            title: "备注",
            required: true,
            field: "Remark",
            placeholder: "你可以设置colSize属性决定标签的长度，可选值12/8/6/4",
            minRows: 5,
            maxRows: 10,
            type: "textarea",
            colSize: 12, //设置宽度100%
            group: "其他类型",
          },
        ],
        [
          {
            group: "编辑器类型",
            title: "编辑器",
            required: true,
            field: "editorValue",
            type: "editor",
            height: 200,
            url: "api/Demo_Order/Upload",
            colSize: 12, //设置宽度占比
          },
        ],
      ],
    };
  },
};
</script>
<style lang="less" scoped>
.container {
  position: absolute;
  height: 100%;
  width: 100%;
  padding-top: 15px;

  background: #f3f7fc;

  .content {
    border-radius: 5px;
    padding: 10px 0;
    background: #fff;
    // height: 100%;
    margin: 10px;
  }

  .header {
    margin: 0 13px 13px 13px;
  }
}
</style>
