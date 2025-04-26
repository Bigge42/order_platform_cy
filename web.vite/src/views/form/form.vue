<template>
  <div class="container">
    <div class="content">
      <div class="header">
        <el-alert type="success" title="关于表单">
          表单基于element
          plus与框架独立开发的组件,只需要简单配置即可完成各种类型表单;(配置源码见:企业版本文档:组件api->表单)
        </el-alert>
      </div>
      <VolForm
        :labelWidth="90"
        ref="form"
        :loadKey="true"
        :formFields="formFields"
        :formRules="formRules"
      >
        <div style="text-align: center; width: 100%">
          <el-button type="primary" plain @click="getForm">表单校验</el-button>
          <el-button type="success" plain @click="reset">重置表单</el-button>
          <el-button type="info" plain @click="setReadonlyStaus(true)"
            >全部只读</el-button
          >
          <el-button type="primary" plain @click="setReadonlyStaus(false)"
            >取消只读</el-button
          >
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

        inputVal: "abcd",
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

        label1: null,
        label2: null,
        rate: 4,
        color: "#eee",

        selectVal: null,
        selectAutoVal: null,
        selectListVal: [], //多选值

        selectTableVal: "",
        selectNameVal: "",
        selectCodeVal: "",
        selectTextVal: "纯文本显示",

        radioVal: "1", //设置单选框默认值
        checkboxVal: ["1", "2"], //多选框是数组

        editorValue: "",
      },
      formRules: [
        //表单配置formRules数据格式是二维数组，表示每个标签所在位置:[[{},{}]]
        [
          {
            colSize: 12,
            render: (h) => {
              return (
                <div
                  style={{
                    display: "flex",
                    "margin-bottom": "-4px",
                    "line-height": "20px",
                    "padding-bottom": "5px",
                    "border-bottom": "1px solid  rgb(227 227 227)",
                  }}
                >
                  <div style="height: 15px;background: #2196F3;width: 5px;border-radius: 10px;top: 2px;position: relative;"></div>
                  <div style="padding-left: 6px; font-weight: bold; font-size: 15px;">
                    输入类型
                  </div>
                </div>
              );
            },
          },
        ],
        [
          {
            type: "text",
            title: "输入框",
            required: true,
            placeholder: "输入框。。。",
            field: "inputVal",
            inputStyle: { color: "#f50000" }, //设置输入框样式
            //
          },
          {
            type: "decimal",
            title: "小数",
            //required: true,
            placeholder: "只能输入小数",
            field: "decimalVal",
          },
          {
            type: "number",
            title: "整数",
            placeholder: "只能输入整数",
            field: "numberVal",
            min: 0, //限制数字大小
            max: 10,
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
          },
          {
            title: "手机",
            // required: true,
            field: "phoneValue",
            type: "phone",
          },
          {
            title: "邮箱",
            //required: true,
            field: "mailVal",
            type: "mail",
          },
        ],
        [
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
          },
          {
            title: "(年)日期区间",
            field: "dateRangeVal",
            type: "year", //date\datetime
            range: true,
            onChange: (val) => {
              console.log("日期:" + val);
            },
          },
          {
            title: "年月",
            field: "monthVal",
            placeholder: "年月",
            type: "month", //如果选择年，这里改为year
            onChange: (value) => {
              console.log(value);
            },
          },

          {
            dataKey: "top",
            title: "是否选择",
            required: true,
            field: "switchVal",
            activeText: "已选择",
            inactiveText: "未选择",
            type: "switch",
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
          },
        ],
        [
          {
            title: "级联",
            field: "cascaderVal", //注意上面formFields属性cascader是数组
            placeholder: "配置数据源后自动绑定级联",
            checkStrictly: true, //是否可以选择任意一级，false只能选择最后一级
            type: "cascader", //注意这个是值是数组
            dataKey: "tree_roles", //配置数据源(见菜单下拉框绑定设置中的级联角色自定义sql)
            data: [], //绑定的数据源dataKey，
          },
          {
            title: "树形级联",
            dataKey: "部门级联",
            // 如果这里绑定了data数据，后台不会加载此数据源
            data: [],
            field: "treeVal",
            multiple: true, //设置为多选
            // readonly: true,
            type: "treeSelect",
          },
          {
            dataKey: "city", //后台下拉框对应的数据字典编号
            data: [], //loadKey设置为true,会根据dataKey从后台的下拉框数据源中自动加载数据
            title: "下拉框",
            filter: true,
            //  required: true, //设置为必选项
            field: "selectVal",
            type: "select",
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
                      reference: () => {
                        return h(
                          <i
                            style="color:#9E9E9E;font-size:13px;"
                            onClick={() => {
                              this.popover();
                            }}
                            class="el-icon-warning-outline"
                          ></i>
                        );
                      },
                    }}
                  </el-popover>
                );
              },
            },
          },
          {
            title: "多选(配置colSize属性指定标签宽度)",
            data: [],
            dataKey: "city",
            field: "selectListVal",
            type: "selectList", //上面selectListVal值为数组
            colSize: 4, //指定宽度可选值2\3\4\6\8\10\12
            // itemStyle:{width:'400px'}//或者使用固定样式宽度
          },
        ],
        [
          {
            title: "备注",
            required: true,
            field: "Remark",
            placeholder: "你可以设置colSize属性决定标签的长度，可选值12/8/6/4",
            minRows: 3, //设置最小高度
            maxRows: 10,
            type: "textarea",
            colSize: 12, //设置宽度100%
          },
        ],
        [
          {
            colSize: 12,
            render: (h) => {
              return (
                <div
                  style={{
                    display: "flex",
                    "line-height": "20px",
                    "margin-top": "5px",
                    "padding-bottom": "5px",
                    "border-bottom": "1px solid  rgb(227 227 227)",
                  }}
                >
                  <div style="height: 15px;background: #2196F3;width: 5px;border-radius: 10px;top: 2px;position: relative;"></div>
                  <div style="padding-left: 6px; font-weight: bold; font-size: 15px;">
                    自定义标签
                  </div>
                </div>
              );
            },
          },
        ],
        [
          {
            title: "标题样式1",
            field: "label1",
            labelStyle: "color:red;font-weight:700",
            placeholder: "自定义标题样式1",
          },
          {
            title: "标题样式2",
            field: "label2",
            placeholder: "自定义标题样式2",
            labelRender: (h, {}) => {
              return (
                <div>
                  <el-popover
                    placement="top-start"
                    title="提示"
                    width="200"
                    trigger="hover"
                  >
                    {{
                      reference: () => {
                        return (
                          <i
                            style="color:#2196F3;font-size:14px;"
                            onClick={($e) => {
                              this.$message.success("提示信息");
                            }}
                            class="el-icon-search"
                          >
                            标题事件+提示
                          </i>
                        );
                      },
                      default: () => {
                        return (
                          <div style="width:300px">
                            <div style="font-size:12px">
                              触发 Popover 显示的 HTML 元素
                            </div>
                            <div style="font-size:12px">
                              触发 Popover 显示的 HTML 元素
                            </div>
                          </div>
                        );
                      },
                    }}
                  </el-popover>
                </div>
              );
            },
          },
          {
            title: "评分",
            field: "rate",
            type: "rate",
          },
          {
            title: "拾色器",
            field: "color",
            type: "color",
          },
        ],
        [
          {
            colSize: 12,
            render: (h) => {
              return (
                <div
                  style={{
                    display: "flex",
                    "margin-bottom": "-4px",
                    "line-height": "20px",
                    "margin-top": "5px",
                    "padding-bottom": "5px",
                    "border-bottom": "1px solid  rgb(227 227 227)",
                  }}
                >
                  <div style="height: 15px;background: #2196F3;width: 5px;border-radius: 10px;top: 2px;position: relative;"></div>
                  <div style="padding-left: 6px; font-weight: bold; font-size: 15px;">
                    下拉框table搜索
                  </div>
                </div>
              );
            },
          },
        ],
        [
          {
            title: "下拉框table搜索",
            field: "selectTableVal",
            type: "selectTable",
            required: true,
            // inputReadonly:true,
            url: "api/Sys_Region/getPageData",
            columns: [
              { field: "code", title: "编码", type: "string", width: 50, search: true }, //设置不可输入，这里需要配置字段的查询
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
            title: "不可输入(开启搜索)",
            field: "selectTableVal",
            type: "selectTable",
            // required:true,
            inputReadonly: true, //设置不可输入
            url: "api/Sys_Region/getPageData",
            columns: [
              { field: "code", title: "编码", type: "string", width: 50, search: true },
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
              //设置不可输入后，这里不能手动设置输入框的值，否则查不到数据
              // param.wheres.push({
              //   name: "mername",
              //   value: this.formFields.selectTableVal,
              //   displayType: "like",
              // });
              callback(true);
            },
            paginationHide: false, //显示分页
            //colSize: 3, //设置宽度100%
          },
          {
            title: "搜索结果(只读)",
            field: "selectCodeVal",
            readonly: true,
          },
          {
            title: "搜索结果2(只读)",
            field: "selectNameVal",
            // colSize: 6,
            readonly: true,
          },
          {
            title: "搜索结果3(纯文本)",
            field: "selectTextVal",
            label: true,
          },
        ],
        [
          {
            colSize: 12,
            render: (h) => {
              return (
                <div
                  style={{
                    display: "flex",
                    "margin-bottom": "-4px",
                    "line-height": "20px",
                    "margin-top": "5px",
                    "padding-bottom": "5px",
                    "border-bottom": "1px solid  rgb(227 227 227)",
                  }}
                >
                  <div style="height: 15px;background: #2196F3;width: 5px;border-radius: 10px;top: 2px;position: relative;"></div>
                  <div style="padding-left: 6px; font-weight: bold; font-size: 15px;">
                    其他类型
                  </div>
                </div>
              );
            },
          },
        ],

        [
          {
            dataKey: "订单状态",
            title: "单选框radio",
            data: [],
            field: "radioVal",
            type: "radio",
            colSize: 3,
          },
          {
            dataKey: "订单状态",
            title: "多选框checkbox",
            data: [],
            min: 2,
            max: 4,
            field: "checkboxVal",
            type: "checkbox",
            colSize: 3,
          },
          {
            title: "纯文本",
            field: "selectTextVal",
            type: "label",
            inputStyle: { color: "#409eff" }, //自定义样式,其他的input也生效
          },
          {
            title: "只读",
            field: "selectNameVal",
            readonly: true,
          },
        ],
        [
          {
            title: "图片",
            required: true,
            field: "img",
            type: "img",
            multiple: true,
            maxFile: 2,
            maxSize: 5,
            url: "api/Demo_Order/Upload",
            colSize: 3, //设置宽度占比
          },
          {
            title: "上传",
            required: true,
            field: "uploadFile",
            type: "excel", //指定上传类型excel/img/file
            multiple: true,
            readonly: true,
            maxFile: 5,
            maxSize: 3,
            url: "api/Demo_Order/Upload",
            // colSize: 6, //设置宽度50%
          },
        ],
        [
          {
            title: "编辑器",
            required: true,
            field: "editorValue",
            type: "editor",
            height: 120,
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
  padding: 10px;

  background: #f3f7fc;

  .content {
    border-radius: 5px;
    padding: 10px 0;
    background: #fff;
  }

  .header {
    margin: 0 13px 13px 13px;
  }
}
</style>
