<template>
  <div
    class="flex-col print-container"
    :class="{ 'print-box': isBox, 'print-view': view }"
  >
    <div class="flex-row justify-center header p-buttons" style="margin-bottom: 10px">
      <!-- 纸张大小 A3、A4 等 -->
      <div v-if="!view" class="paper">
        <label>纸张：</label>
        <el-select
          @change="
            (type) => {
              setPaper(type);
            }
          "
          style="width: 80px; margin-right: 10px"
          v-model="paperValue"
        >
          <el-option
            v-for="(item, index) in paperSelect"
            :key="index"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>

        <!-- <template v-for="(value, type) in paperTypes" :key="type">
         
            <el-button circle :type="curPaperType === type ? 'primary' : ''" @click="setPaper(type, value)">
              {{ type }}
            </el-button>
          </template> -->

        <!-- 自定义纸张 -->
        <el-popover placement="bottom" :width="400" trigger="click">
          <template #reference>
            <el-button type="primary" plain @click="showPaperPop">自定义纸张</el-button>
          </template>
          <div class="custom-popover">
            <div class="custom-popover-text">设置纸张宽高(mm)</div>
            <div class="flex-row mt-10">
              <el-input v-model="paperWidth" :min="1" placeholder="宽(mm)" />
              <span class="ml-10 mr-10">x</span>
              <el-input v-model="paperHeight" :min="1" placeholder="高(mm)" />
            </div>
            <div class="custom-popover-btn">
              <el-button
                type="primary"
                @click.stop="
                  () => {
                    setPaperOther({ width: paperWidth, height: paperHeight });
                  }
                "
                >确定</el-button
              >
            </div>
          </div>
        </el-popover>
      </div>
      <!-- 缩放 -->
      <!-- <div class="flex-row align-center ml-10 btn-scale">
                <el-button circle><i class="el-icon-circle-plus-outline" @click="changeScale(true)"></i></el-button>
                <div style="margin: 0 4px; width: 40px">{{ (scaleValue * 100).toFixed(0) }}%</div>
                <el-button circle><i class="el-icon-remove-outline" @click="changeScale(false)"></i></el-button>
            </div> -->

      <el-button v-if="!view" type="primary" plain @click.stop="rotatePaper">
        <i class="el-icon-refresh" />
        旋转
      </el-button>

      <el-button v-if="!view" type="primary" plain @click.stop="clearPaper">
        <i class="el-icon-close" />
        清空纸张
      </el-button>

      <el-button v-if="!view" type="primary" plain class="" @click.stop="exportJson">
        <i class="el-icon-document" />
        导出模板
      </el-button>
      <el-button
        v-if="!view"
        type="primary"
        plain
        class=""
        @click.stop="
          () => {
            getHtml();
          }
        "
      >
        <i class="el-icon-document" />
        预览
      </el-button>

      <el-button
        type="primary"
        plain
        class="secondary circle-10 ml-10"
        @click.stop="print"
      >
        <i class="el-icon-printer" />
        打印
      </el-button>
      <!-- <span class="tm-text">模板管理：</span>
            <el-tree-select style="width: 200px;margin-right:10px;" v-model="templateName" :data="templateData"
                :multiple="false" @change="templateChange" :render-after-expand="false" :show-checkbox="false"
                :check-strictly="false" check-on-click-node node-key="value">
                <template #default="{ data, node }">
                    {{ data.label }}</template>
            </el-tree-select> -->
      <el-button
        v-if="!view"
        type="danger"
        plain
        class="secondary circle-10 ml-10"
        @click.stop="delTemplate"
      >
        <i class="el-icon-delete" />
        删除模板
      </el-button>

      <el-button
        v-if="!isBox && !view"
        type="success"
        plain
        class="secondary circle-10 ml-10"
        @click.stop="
          () => {
            addTemplate();
          }
        "
      >
        <i class="el-icon-plus" />
        新建模板
      </el-button>
      <el-button
        v-if="!view"
        type="success"
        plain
        class="secondary circle-10 ml-10"
        @click.stop="printPagination = !printPagination"
      >
        <i class="el-icon-plus" />
        多面板
      </el-button>
      <div v-show="printPagination" class="hiprint-printPagination"></div>
      <!-- 
        <el-button   class="warning circle-10 ml-10" @click.stop="print2">
          <i class="el-icon-print" />
          直接打印(需要连接客户端)
        </el-button> -->
    </div>
    <div class="view-content" v-if="view">
      <div v-html="previewHtml"></div>
    </div>

    <div class="flex-row desc-content" v-show="!view">
      <div class="flex-2 left flex-col print-ops" v-if="!view">
        <el-scrollbar style="height: 100%">
          <div id="provider-container" class="container rect-printElement-types"></div>

          <!-- <div id="provider-container-table" class="container rect-printElement-types"></div> -->

          <!-- <div class="title">provider1 默认样式</div> -->
          <!-- provider1 的容器; 加上 class "rect-printElement-types" 使用默认样式 -->
          <!-- 当然可以 重写 或者 自定义样式 -->
          <div id="provider-container1" class="container rect-printElement-types"></div>
          <!-- <div class="title">provider2 自定义样式</div> -->
          <!-- provider2 的容器; -->
          <!-- 这里自定义显示样式 custom-style-types -->
          <div id="provider-container2" class="container custom-style-types"></div>
          <div class=""></div>
        </el-scrollbar>
      </div>
      <div class="flex-5 center" style="padding: 0">
        <el-scrollbar style="height: 100%">
          <!-- 设计器的 容器 -->
          <div id="hiprint-printTemplate" style="padding: 20px"></div>
        </el-scrollbar>
      </div>
      <vol-box title="预览" v-model="previewModel" :width="840" :padding="0">
        <div class="preview-model" v-html="previewHtml"></div>
      </vol-box>
      <div class="flex-2 right" v-if="!view">
        <el-scrollbar style="height: 100%; padding: 5px">
          <div id="PrintElementOptionSetting"></div>
        </el-scrollbar>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: "start-02",
  methods: {},
};
</script>

<script setup>
import store from "@/store/index";
import { onMounted, ref, getCurrentInstance, reactive, nextTick } from "vue";
import VolBox from "@/components/basic/VolBox.vue";
import hiprintPackage from "./vue-plugin-hiprint.js"; // "vue-plugin-hiprint";
//import {hiprint} from  "./vue-plugin-hiprint.js";
var hiprint = hiprintPackage.hiprint;
import { provider1 } from "./provider1";
import { provider2 } from "./provider2";
import template from "./template";
import printData from "./printData";
// 组合式函数 hooks
import { usePaper } from "./use-paper";
import { useZoom } from "./use-zoom";
// 工具
import { newHiprintPrintTemplate } from "./template-helper";
import helper from "./provider-helper";
import providerHelper from "./provider-helper";

import { useRouter, useRoute } from "vue-router";

const { proxy } = getCurrentInstance();
// proxy.menu && proxy.menu.hide();

const router = useRoute();

const view = ref(false);

if (router.query.view === "1") {
  view.value = true;
}

const loading = ref(false);
const previewModel = ref(false);

const props = defineProps({
  isBox: {
    type: Boolean,
    default: false,
  },
});

const TEMPLATE_KEY = getCurrentInstance().type.name; // 存储模板对象的 key
const {
  paperTypes,
  curPaperType,
  paperPopVisible,
  paperWidth,
  paperHeight,
  showPaperPop,
  setPaper,
  paperSelect,
  setPaperOther,
} = usePaper(TEMPLATE_KEY);
const { scaleValue, changeScale } = useZoom(TEMPLATE_KEY);

const drawer = ref(false);
// 自定义传入 provider 的参数
let options = {
  config: {
    tid: "providerModule1.config",
    title: "参数provider示例",
    type: "text",
    options: {
      testData: "单据表头",
      height: 30,
      fontSize: 16,
    },
  },
};
// 初始化 provider
hiprint.init({
  providers: [provider1(options), provider2(options)],
});
/**
 * 这里必须要在 onMounted 中去构建 左侧可拖拽元素 或者 设计器
 * 因为都是把元素挂载到对应容器中, 必须要先找到该容器
 */
let callbackApi;
onMounted(() => {
  buildLeftElement();
  if (view.value) {
    //buildDesigner
    //获取打印参数及打印配置
    proxy.http
      .get("api/Sys_PrintOptions/getPrintDetail?id=" + router.query.templateId)
      .then((result) => {
        if (result) {
          callbackApi = result.html;
        }
        buildDesigner(result.options);
      });
  }
  // buildDesigner();
});
/**
 * 构建左侧可拖拽元素
 * 注意: 可拖拽元素必须在 hiprint.init() 之后调用
 * 调用之前 可以先 console.log($("#hiprint-printTemplate")) 看看是否有该 dom
 */
const buildLeftElement = () => {
  // ----- providerModule1 -----
  // eslint-disable-next-line no-undef
  $("#provider-container1").empty(); // 先清空, 避免重复构建
  // eslint-disable-next-line no-undef
  hiprint.PrintElementTypeManager.build($("#provider-container1"), "providerModule1");
  // ----- providerModule2 -----
  // eslint-disable-next-line no-undef
  $("#provider-container2").empty(); // 先清空, 避免重复构建
  // eslint-disable-next-line no-undef
  hiprint.PrintElementTypeManager.build($("#provider-container2"), "providerModule2");
};
/**
 * 构建设计器
 * 注意: 必须要在 onMounted 中去构建
 * 因为都是把元素挂载到对应容器中, 必须要先找到该容器
 */
let hiprintTemplate;
// ref 创建的模板json
const templateRef = ref(); // ref(template);
console.log("templateRef 数据格式:");
console.log("templateRef", templateRef);

let currentData = {};
let batchMain, detail;
// console.log("templateRef.value", templateRef.value);
const buildDesigner = (ops) => {
  // alert(1)
  // eslint-disable-next-line no-undef
  $("#hiprint-printTemplate").empty(); // 先清空, 避免重复构建
  // if (!ops) {
  //     return;
  // }
  if (typeof ops == "string") {
    ops = JSON.parse(ops);
  }
  if (Array.isArray(ops)) {
    ops = ops[0];
  }
  templateRef.value = ops;
  // 注意事项: 模板json(object)
  // 如果使用 vue ref创建的模板json, 需要使用 .value 获取 (确保内部能够使用 object.key 拿到对应数据就行)
  hiprintTemplate = newHiprintPrintTemplate(TEMPLATE_KEY, {
    template: templateRef.value, // 模板json(object)
    settingContainer: "#PrintElementOptionSetting", // 元素参数容器
    paginationContainer: ".hiprint-printPagination",
    onDataChanged: (type, json) => {
      console.log(type); // 新增、移动、删除、修改(参数调整)、大小、旋转
      console.log(json); // 返回 template
    },
  });
  // 构建 并填充到 容器中
  hiprintTemplate.design("#hiprint-printTemplate");
  // console.log(hiprintTemplate);

  let ids = (router.query.ids || "").split(",");
  if (!ids.length) {
    proxy.$message.error("参数缺失");
    return;
  }

  if (view.value) {
    batchMain = ops.panels[0].printElements.some((x) => {
      return x.options.field == "main:table";
    });
    detail = ops.panels[0].printElements.some((x) => {
      return x.options.field == "detail:table";
    });
    let parms = {
      ids: ids,
      templateId: router.query.templateId,
      table: router.query.table,
      batchMain,
      detail,
    };
    parms.options = router.query.options;
    proxy.http.post("api/Sys_PrintOptions/getPrintData", parms, true).then((result) => {
      if (batchMain) {
        currentData = { "main:table": result };
      } else if (Array.isArray(result)) {
        currentData = result; // result[0];
      }
      // currentData = result;
      getHtml(currentData, batchMain);

      //数量太多时这里考虑减少预览
      // getHtml(currentData.slice(0,10), batchMain)
    });
  }
};

/**
 * 浏览器打印
 */
const print = () => {
  // 参数: 打印时设置 左偏移量，上偏移量
  let options = { leftOffset: -1, topOffset: -1 };
  // 扩展
  let ext = {
    callback: () => {
      // console.log("浏览器打印窗口已打开");
      printCallback();
    },
    styleHandler: () => {
      // 重写 文本 打印样式
      return "<style>.hiprint-printElement-text{}</style>";
    },
  };
  // 调用浏览器打印
  hiprintTemplate.print(currentData, options, ext);

  //https://mp.weixin.qq.com/s/E-ZfuKLH3-GvKn4AosRvaw
  hiprintTemplate.on("printSuccess", (data) => {
    // alert('打印成功')
    //  console.log('打印完成')
    printCallback(true);
  });
  hiprintTemplate.on("printError", (data) => {
    alert("打印失败");
    console.log("打印失败");
    printCallback(false);
  });
};

const printCallback = (status) => {
  if (!callbackApi) {
    return;
  }
  let ids = (router.query.ids || "").split(",");

  if (callbackApi.indexOf('?')==-1) {
      callbackApi=callbackApi+"?table="+(router.query.table||'')
  }else{
      callbackApi=callbackApi+"&table="+(router.query.table||'')
  }
  if (router.query.options) {
    ids = {
      ids: ids,
      options: router.query.options,
    };
  }
  proxy.http.post(callbackApi, ids, true).then((result) => {});
};
/**
 * 直接打印: 借助客户端,静默打印(无弹窗直接打印)
 * 注意: 需要先连接客户端
 */
const print2 = () => {
  if (hiprint.hiwebSocket.opened) {
    hiprintTemplate.print2(printData);
  } else {
    alert("请先连接客户端(刷新网页), 然后再点击「直接打印」");
  }
};
// ----------------- 模板对象 api 部分 -----------------
/**
 * 旋转纸张
 */
const rotatePaper = () => {
  hiprintTemplate.rotatePaper();
};
/**
 * 清空所有元素
 */
const clearPaper = () => {
  hiprintTemplate.clear();
};

const buildProvider = (data, clear) => {
  if (typeof data == "string") {
    data = JSON.parse(data);
  }
  // 组装 provider
  if (Array.isArray(data)) {
    let providerList = helper.createProviderList(data);
    hiprint.init({ providers: providerList });
  } else {
    let provider = helper.createProvider(data.key, data.options);
    hiprint.init({ providers: [provider] });
  }
  if (clear) {
    // eslint-disable-next-line no-undef
    $("#provider-container").empty(); // 先清空, 避免重复构建

    //  $("#provider-container-table").empty();
  }
  if (Array.isArray(data)) {
    // eslint-disable-next-line no-undef
    data.forEach((item) =>
      hiprint.PrintElementTypeManager.build($("#provider-container"), item.key)
    );
  } else {
    // eslint-disable-next-line no-undef
    hiprint.PrintElementTypeManager.build($("#provider-container"), data.key);
  }
  loading.value = false;
};

/**
 * 导出模板 json
 * 必须确保 hiprintTemplate 已成功创建
 */
const exportJson = () => {
  let json = hiprintTemplate.getJson();
  // console.log(json);
  console.log(JSON.stringify(json));
  // alert("导出成功! 请查看控制台输出");
  return json;
};
/**
 * 导出模板 json tid
 * 仅导出 options, 不导出 printElementType
 * 必须确保 hiprintTemplate 已成功创建
 */
const exportJsonTid = () => {
  let json = hiprintTemplate.getJsonTid();
  console.log(JSON.stringify(json));
  alert("导出成功! 请查看控制台输出");
};

const previewHtml = ref("");
const getHtml = (data, batchMain) => {
  // if (Array.isArray(data)) {
  //     data = data[0];
  // }
  //批量打印主表
  // if (batchMain) {
  //     data = { "main:table": data }
  // } else if (Array.isArray(data)) {
  //     data = data[0];
  // }

  let json = hiprintTemplate.getJson();
  let user = store.getters.getUserInfo();
  if (!user) {
    user = "--";
  } else {
    user = user.userName;
  }

  // if (!data) {
  // console.log(JSON.stringify(json))
  let obj = {};
  json.panels[0].printElements.forEach((x) => {
    if (x.options.field == "sys:datetime") {
      obj[x.options.field] = proxy.base.getDate(true);
    } else if (x.options.field == "sys:date") {
      obj[x.options.field] = proxy.base.getDate();
    } else if (x.options.field == "sys:user") {
      obj[x.options.field] = user;
    } else if (
      x.options.field == "custom:qrcode" ||
      x.options.field == "custom:barcode"
    ) {
      obj[x.options.field] = x.options.testData;
    }
    if (!data && x.options.field) {
      //  if (x.options.field == 'detail:table') {
      if (x.options.fields && x.options.fields.length) {
        obj[x.options.field] = [{}]; //[{}, {}];
      } else if (x.options.textType == "barcode" || x.options.textType == "qrcode") {
        obj[x.options.field] = "123456";
      } else if (!obj[x.options.field]) {
        obj[x.options.field] = x.options.title;
      }
    }
  });

  if (!data) {
    if (!obj["main:table"]) {
      obj["main:table"] = [{}, {}];
    }
    if (!obj["main:table"] || !Array.isArray(obj["main:table"])) {
      obj["main:table"] = [{}, {}];
    }
    data = obj;
  } else {
    if (!Array.isArray(data)) {
      data = [data];
    }
    data.forEach((x) => {
      Object.assign(x, obj);
    });
    // Object.assign(data, obj);
  }

  // }

  let html = hiprintTemplate.getHtml(data); //printData
  previewHtml.value = html[0].innerHTML;
  previewModel.value = true;
  // preview.value.showModal(html);
};
const paperValue = ref("A4");

const templateName = ref("批量打印");
const templateChange = (val) => {
  templateName.value = "";
};

let tableOptions = [];

const printOptions = reactive([]);
let isInit = false;
let v = false;
const addTemplate = async (ops, table) => {
  // if (v) {
  //     //   templateRef.value.panels[0].printElements[1].printElementType.title=Math.random()+''
  //     templateRef.value = { "panels": [{ "index": 0, "name": 1, "height": 297, "width": 210, "paperHeader": 49.5, "paperFooter": 841.8897637795277, "printElements": [{ "options": { "left": 165, "top": 24, "height": 16, "width": 160, "field": "sys:datetime", "testData": "2023-08-22 00:06:06", "fontSize": 6.75, "fontWeight": "700", "textAlign": "left", "textContentVerticalAlign": "middle", "title": "当前时间", "right": 325.0000228881836, "bottom": 40, "vCenter": 245.0000228881836, "hCenter": 32 }, "printElementType": { "title": "当前时间", "type": "text" } }], "watermarkOptions": {} }] }
  // } else {

  //     templateRef.value = template
  // }

  // v=!v;
  buildDesigner(ops);
  // if (row) {
  //     if (!isInit) {
  //         await initPrintFields();
  //     }
  // }

  // if (!printOptions.length) {
  //     return proxy.$message.error('未配置打印,请在后台startup.cs中方法ConfigureContainer配置打印参数')
  // }
  // if (!row) {
  //     return;
  // }
  // let ops = printOptions.find(c => { return c.key == row.TableName });

  if (table) {
    await initPrintFields(table);
    ops = printOptions.find((x) => {
      return x.key === table;
    });
    if (!ops) {
      proxy.$message.error(`未找到表[${table}]在打印配置`);
      return;
    }
  }
  nextTick(() => {
    buildProvider(ops, true);
  });
};
//初始化左侧面板,页面弹出框打开触发
const initPanel = async (row, table) => {
  let ops = [];
  try {
    ops = JSON.parse(row.Options);
  } catch (error) {
    console.log("初始化provider异常:" + error.message);
  }
  await addTemplate(ops, table);
};

defineExpose({
  initPanel,
  exportJson,
  buildDesigner,
});

const initPrintFields = async (tableName) => {
  if (
    printOptions.some((x) => {
      return x.key === tableName;
    })
  ) {
    return;
  }
  await proxy.http
    .get("api/Sys_PrintOptions/getPrintFields?table=" + tableName, {}, true)
    .then((result) => {
      tableOptions = result;
      isInit = true;
      printOptions.push(...convertOptions(result));
      //  console.log(jso)
      //   printOptions.push(...[])
    });
};

const tk = (proxy.$store.getters.getUserInfo() || { accessToken: "" }).accessToken || "";
window.access_token = tk ? "?access_token=" + tk : "";

const getFormatter = (c, isTable) => {
  //图片
  const fn = isTable
    ? `function(value,row,colIndex,options,rowIndex){`
    : `function(title,value,options,templateData,target){`;
  if (c.tableType == 1) {
    return `${fn}
        if(!value){return ''};  
        let src=(value+'').split(',').filter(x=>{return x})
        .map(c=>{return (c.startsWith('http')?c:(ipAddress+c)+access_token)});
         let style=src.length>=1?'style="width:80px;height:80px"':''
         src=src.map(c=>{return '<img '+style+' src="'+c+'"/>'})
         return src.join('')
       }`;
  } else if (c.tableType == 2 || c.tableType == 3) {
    //图片或文件
    //colType = "excel";
  }
  //日期
  else if (c.IsImage == 4) {
    return `${fn}
        if(!value){return ''}; 
         return (value+'').substr(0,10) }`;
  } else if (c.IsImage == 5) {
    //年月
    return `${fn}
        if(!value){return ''}; 
         return (value+'').substr(0,7) }`;
  }
  return "";
};
//生成打印模板
const convertOptions = (result) => {
  return result.map((x) => {
    let fields = x.fields.map((c) => {
      return {
        text: c.name,
        field: c.field,
      };
    });

    let ops = {
      key: x.tableName,
      options: {
        groupName: x.name,
        printElements: x.fields.map((c) => {
          let val = "";
          let width = 100;
          if (c.tableType === 4 || c.editType === "date") {
            val = proxy.base.getDate();
            width = 110;
          } else if (c.editType === "datetime") {
            val = proxy.base.getDate(true);
            width = 150;
          }
          return {
            id: c.field,
            type: "txt",
            width: width,
            height: 20,
            title: c.name,
            field: c.field,
            testData: val,

            // 协商的可选参数, 没有可选参数就返回 {}
            options: {
              textContentVerticalAlign: "middle",
              fontSize: 10,
              formatter: getFormatter(c),
              //   formatter: `function (data, options) {
              //     return '<img src="http://localhost:9170/Upload/Tables/App_User/202410150032232764/wechart.png"/>';
              //   }`,
              //   type: "image",
              // fields: fields
              // color: "#f00808",
            },
          };
        }),
      },
    };

    let _code = new Date().valueOf();

    ops.options.printElements.push(
      ...[
        {
          tid: "providerModule1.barcode",
          title: "条形码",
          data: "1234567890",
          type: "text",
          options: {
            field: fields[0].field, //'barcode',
            testData: "1234567890",
            height: 32,
            fontSize: 10,
            lineHeight: 18,
            textAlign: "center",
            textType: "barcode",
            fields: fields,
          },
        },
        // {
        //   //  tid: 'providerModule1.qrcode',
        //     title: '二维码',
        //     data: "Q1234567890",
        //     type: 'text',
        //     options: {
        //         field:"qrcode",// fields[0].field,// 'qrcode',
        //         testData: "Q1234567890",
        //         height: 32,
        //         fontSize: 10,
        //         lineHeight: 18,
        //         textType: 'qrcode',
        //       //  fields: fields
        //     }
        // }
      ]
    );

    let columns = {
      id: "table:" + x.tableName,
      type: "tableCustom",
      title: "批量打印",
      field: "main:table",
      options: {
        field: "main:table",
        fields: x.fields.map((c) => {
          return { text: c.name, field: c.field };
        }),
      },
      columns: [
        x.fields.map((c) => {
          return {
            title: c.name,
            align: "center",
            field: c.field,
            width: c.width || 90,
            formatter2: getFormatter(c, true), //主表批量打印
          };
        }),
      ],
    };

    ops.options.printElements.push(columns);

    if (x.templateDetails && x.templateDetails.length) {
      x.templateDetails.forEach((item) => {
        columns = {
          isTable: true,
          id: item.detailTableName, // "detail:table",
          type: "tableCustom",
          title: item.detailName, // "明细批量打印",
          //  field: "detail:table:" + x.tableName,
          field: item.detailTableName, //"detail:table",

          options: {
            field: item.detailTableName, // "detail:table",
            fields: (item.fields || item.detailFields).map((c) => {
              return {
                text: c.name,
                field: c.field,
              };
            }),
          },
          columns: [
            item.detailFields.map((c) => {
              return {
                title: c.name,
                align: "center",
                field: c.field,
                width: c.width || 90,
                formatter2: getFormatter(c, true),
                // options: {
                //   textContentVerticalAlign: "middle",
                //   fontSize: 10,
                //   formatter: "return 66666",
                //   //   formatter: `function (data, options) {
                //   //     return '<img src="http://localhost:9170/Upload/Tables/App_User/202410150032232764/wechart.png"/>';
                //   //   }`,
                //   //   type: "image",
                //   // fields: fields
                //   // color: "#f00808",
                // },
                // options: { formatter: getFormatter(c) },
              };
            }),
          ],
        };
        ops.options.printElements.push(columns);
      });
    }

    // if (x.detailFields.length) {
    //     columns = {
    //         id: "detail:table",
    //         type: "tableCustom",
    //         title: "明细批量打印",
    //         //  field: "detail:table:" + x.tableName,
    //         field: "detail:table",
    //         options: {
    //             field: "detail:table",
    //             fields:
    //                 x.fields.map(c => {
    //                     return { text: c.name, field: c.field }
    //                 })
    //         },
    //         columns: [x.detailFields.map(c => {
    //             return { title: c.name, align: "center", field: c.field, width: c.width || 90 }
    //         })]
    //     }
    //     ops.options.printElements.push(columns);
    // }
    return ops;
  });
};

if (props.isBox) {
  initPrintFields();
}

const delTemplate = () => {
  proxy.$message.success("开发中");
};

const printPagination = ref(false);
</script>

<style lang="less" scoped>
/* 重写默认的一个样式 */
.print-ops
  ::v-deep(.rect-printElement-types .hiprint-printElement-type > li > ul > li > a) {
  cursor: move;
  color: #272626ee;
  float: left;
  width: 115px;
  text-align: center;
  border: 1px solid #eee;
  padding: 1px 8px !important;
  text-align: left;
  line-height: 31px;
  margin: 4px;
  border-radius: 3px;
  background: #f0f9eb;
  font-size: 13px;
  height: 33px;
  padding: 4px 6px;
  font-size: 13px;
  box-shadow: none;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
  -o-text-overflow: ellipsis;
}

.print-ops ::v-deep(.rect-printElement-types .hiprint-printElement-type > li > ul > li) {
  max-width: none;
}

.print-ops ::v-deep(

    /* 自定义 provider 构建样式 */
    .custom-style-types .hiprint-printElement-type) {
  display: block;
}

.print-ops ::v-deep(.custom-style-types .hiprint-printElement-type) {
  padding: 0 0 0 0;
  list-style: none;
}

.print-ops ::v-deep(.custom-style-types .hiprint-printElement-type > li > .title) {
  display: block;
  padding: 4px 0px;
  // color: rgb(0, 58, 230);
  clear: both;
  font-weight: bold;
  padding-left: 5px;
  font-size: 14px;
}

.print-ops ::v-deep(.rect-printElement-types .hiprint-printElement-type > li > .title) {
  font-weight: bold;
  padding-left: 5px;
  font-size: 14px;
}

.print-ops ::v-deep(.custom-style-types .hiprint-printElement-type > li > ul) {
  /* padding: 0 0 0 0;
    display: block;
    list-style: none; */
  padding: 0;
}

.print-ops ::v-deep(.custom-style-types .hiprint-printElement-type > li > ul > li) {
  list-style: none;
  display: block;
  width: 50%;
  float: left;
}

.print-ops ::v-deep(.custom-style-types .hiprint-printElement-type > li > ul > li > a) {
  cursor: move;
  color: #272626ee;
  float: left;
  width: 100px;
  text-align: center;
  border: 1px solid #eee;
  padding: 1px 8px !important;
  text-align: left;
  line-height: 31px;
  margin: 4px;
  border-radius: 3px;
  background: #f0f9eb;
  height: 33px;
  padding: 4px 6px;
  font-size: 13px;
  box-shadow: none;
}

.print-ops ::v-deep(ul) {
  margin: 0;
}
</style>

<style scoped>
/* api按钮 */
.api {
  background: #00acc1;
}

.auto {
  width: auto !important;
}

/* 纸张 */
.paper {
  margin-right: 10px;
}

/* 两边的 btn 圆角 */

.popover {
  position: absolute;
  margin-top: 10px;
  z-index: 10;
}

.popover .popover-content {
  background: white;
  border-radius: 4px;
  padding: 10px 14px;
  /* box-shadow: 2px 2px 2px 4px rgb(128 0 128 / 20%); */
}

.popover .input {
  height: 24px;
  padding: 2px 4px;
}

.popover .input:hover {
  /* border-color: rgb(245, 155, 241); */
  border-radius: 4px;
}

/* 区域 */
.left {
  width: 223px;
  padding: 0 4px;
  flex: none;
  background: white;
  border-radius: 4px;
  border: 1px solid #d9d9d9;
  /* box-shadow: 2px 2px 2px 0px rgb(128 0 128 / 20%);
    overflow: auto; */
}

.left .container {
  /* height: 50%; */
  /* overflow: auto;
    padding: 0 10%; */
  /* background: rgb(245, 155, 241); */
}

.left .container[id*="provider-container2"] {
  margin-bottom: 10px;
  /* background: rgb(209, 120, 239); */
}

.center {
  margin: 0 10px;
  background: white;
  border-radius: 4px;
  border: 1px solid #d9d9d9;
  padding: 20px;
  /* box-shadow: 2px 2px 2px 0px rgb(128 0 128 / 20%); */
  /* overflow: auto; */
}

.right {
  background: white;
  border-radius: 4px;
  border: 1px solid #d9d9d9;
  padding: 10px 0;
  /* box-shadow: 2px 2px 2px 0px rgb(128 0 128 / 20%); */
  overflow: auto;
}

/* 左侧拖拽元素样式 */
.title {
  font-size: 16px;
  font-weight: 500;
  margin: 4px 0 4px 10px;
}

.item {
  display: flex;
  flex-direction: column;
  align-items: center;
  background: white;
  padding: 4px 10px;
  margin: 10px 8px 4px 8px;
  width: 38%;
  min-height: 60px;
  border-radius: 4px;
  box-shadow: 2px 2px 2px 2px rgba(171, 171, 171, 0.2);
}

.item .iconfont {
  font-size: 1.5rem;
}

.item span {
  font-size: 14px;
}
</style>
<style lang="less" scoped>
.desc-content {
  height: calc(100vh - 160px);
}

.flex-row {
  display: flex;

  .right {
    // width: 280px;
    flex: 1;
  }
}

.center {
  // max-width: 840px;
  width: 840px;
  // flex: 1;
  // flex: 5;
  // width: 0;
}

.btn-scale {
  display: flex;
  align-items: center;
  margin-right: 10px;

  button {
    width: 29px;
    height: 29px;
  }
}

.custom-popover {
  .custom-popover-text {
    margin-bottom: 10px;
    font-size: 14px;
    font-weight: bolder;
  }

  .flex-row {
    align-items: center;

    .el-input-number {
      flex: 1;
    }

    .mr-10 {
      margin: 0 10px;
    }
  }

  .custom-popover-btn {
    margin-top: 10px;
    text-align: center;
  }
}

ul {
  margin: 0 !important;
}

.preview-model ::v-deep(.hiprint-printPanel) {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.paper {
  label {
    font-size: 13px;
    margin-right: 5px;
  }
}

.print-container {
  padding: 10px;
}

.tm-text {
  font-size: 13px;
  margin-left: 15px;
  padding-top: 7px;
}

.print-box {
  position: absolute;
  height: 100%;
  width: 100%;
  display: flex;
  flex-direction: column;

  .desc-content {
    flex: 1;
    height: 0;
  }
}

.print-view {
  overflow: auto;
  position: absolute;
  height: 100%;
  width: 100%;
  background: #f6f6f6;

  .header {
    text-align: center;
    display: block;
  }

  .desc-content {
    justify-content: center;

    height: calc(100vh - 70px) !important;
  }

  .center {
    border: 1px solid #fbfafa;
  }
}

.view-content {
  ::v-deep(.hiprint-printPanel) {
    display: flex;
    justify-content: center;
    align-items: center;
    flex-direction: column;

    .hiprint-printPaper {
      background: #ffff;
      border: 1px dotted #eee;
    }
  }
}

.p-buttons ::v-deep(.hiprint-pagination) {
  margin: 0;
  margin-left: 10px !important;
  bottom: -3px;
  position: relative;
}
</style>
