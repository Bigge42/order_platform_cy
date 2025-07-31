// import { hiprint } from "vue-plugin-hiprint";
import hiprintPackage from "./vue-plugin-hiprint.js" // "vue-plugin-hiprint";
var hiprint=hiprintPackage.hiprint;
import common from "@/uitils/common.js";
import store from '@/store/index';
export const provider2 = function (options) {
  console.log(options);
  let date=common.getDate();
  let datetime=common.getDate(true);
  let user=store.getters.getUserInfo();
  if (!user) {
    user='--'
  }else{
    user=user.userName;
  }
  var addElementTypes = function (context) {
    context.removePrintElementTypes("providerModule2");
    context.addPrintElementTypes("providerModule2", [
      new hiprint.PrintElementTypeGroup("其他", [
        // {
        //   tid: "providerModule2.table",
        //   title: "订单数据",
        //   type: "table",
        //   options: {
        //     field: "table",
        //     fields: [
        //       { text: "名称", field: "NAME" },
        //       { text: "数量", field: "SL" },
        //       { text: "规格", field: "GG" },
        //       { text: "条码", field: "TM" },
        //       { text: "单价", field: "DJ" },
        //       { text: "金额", field: "JE" },
        //       { text: "备注", field: "DETAIL" },
        //     ],
        //   },
        //   columns: [
        //     [
        //       { title: "名称", align: "center", field: "NAME", width: 100 },
        //       { title: "数量", align: "center", field: "SL", width: 100 },
        //       { title: "条码", align: "center", field: "TM", width: 100 },
        //       { title: "规格", align: "center", field: "GG", width: 100 },
        //       { title: "单价", align: "center", field: "DJ", width: 100 },
        //       { title: "金额", align: "center", field: "JE", width: 100 },
        //       { title: "备注", align: "center", field: "DETAIL", width: 100 },
        //     ],
        //   ],
        //   footerFormatter: function (options, rows, data, currentPageGridRowsData) {
        //     console.log(currentPageGridRowsData);
        //     if (data && data["totalCap"]) {
        //       return `<td style="padding:0 10px" colspan="100">${"应收金额大写: " + data["totalCap"]}</td>`;
        //     }
        //     return '<td style="padding:0 10px" colspan="100">应收金额大写: </td>';
        //   },
        // },
        { tid: "providerModule2.customText", title: "文本", customText: "自定义文本", custom: true, type: "text" },
        {
          tid: "providerModule2.longText",
          title: "长文本",
          type: "longText",
          options: {
            field: "test.longText",
            width: 200,
            testData: "长文本分页/不分页测试",
          },
        },
      // ]),
      // new hiprint.PrintElementTypeGroup("辅助", [
        {
          tid: "providerModule2.hline",
          title: "横线",
          type: "hline",
        },
        {
          tid: "providerModule2.vline",
          title: "竖线",
          type: "vline",
        },
        {
          tid: "providerModule2.rect",
          title: "矩形",
          type: "rect",
        },
        {
          tid: "providerModule2.oval",
          title: "椭圆",
          type: "oval",
        },
        {
          tid: 'providerModule2.date',
          title: '当前日期',
          data: '',
          type: 'text',
          options: {
            field: 'sys:date',
            testData: date,
            height: 16,
            fontSize: 6.75,
            fontWeight: '700',
            textAlign: 'left',
            textContentVerticalAlign: 'middle'
          }
        },
        {
          tid: 'providerModule2.datetime',
          title: '当前时间',
          data: '',
          type: 'text',
          options: {
            field: 'sys:datetime',
            testData: datetime,
            height: 16,
            fontSize: 6.75,
            fontWeight: '700',
            width: 160,
            textAlign: 'left',
            textContentVerticalAlign: 'middle'
          }
        },
        {
          tid: 'providerModule2.user',
          title: '当用用户',
          data: '',
          type: 'text',
          options: {
            field: 'sys:user',
            testData:user,
            height: 16,
            fontSize: 6.75,
            fontWeight: '700',
            textAlign: 'left',
            textContentVerticalAlign: 'middle'
          }
        },
      ]),
    ]);
  };
  return {
    addElementTypes: addElementTypes,
  };
};
