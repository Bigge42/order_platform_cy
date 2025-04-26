/*
 * @Description:
 * @Author: CcSimple
 * @Github: https://github.com/CcSimple
 * @Date: 2023-02-09 10:40:26
 * @LastEditors: CcSimple
 * @LastEditTime: 2023-02-09 11:37:21
 */
// import { hiprint } from 'vue-plugin-hiprint';
import hiprintPackage from "./vue-plugin-hiprint.js" // "vue-plugin-hiprint";
var hiprint=hiprintPackage.hiprint;
export const provider1 = function(options) {
  console.log(options);
  var addElementTypes = function(context) {
    context.removePrintElementTypes('providerModule1');
    context.addPrintElementTypes('providerModule1', [
      new hiprint.PrintElementTypeGroup('常规', [
        // options.config,
        {
          tid: 'providerModule1.header',
          title: '单据表头',
          data: '单据表头',
          type: 'text',
          options: {
            testData: '单据表头',
            height: 17,
            fontSize: 16.5,
            fontWeight: '700',
            textAlign: 'center',
            hideTitle: true
          }
        },
        // {
        //   tid: 'providerModule1.type',
        //   title: '单据类型',
        //   data: '单据类型',
        //   type: 'text',
        //   options: {
        //     testData: '单据类型',
        //     height: 16,
        //     fontSize: 15,
        //     fontWeight: '700',
        //     textAlign: 'center',
        //     hideTitle: true
        //   }
        // },
        // {
        //   tid: 'providerModule1.order',
        //   title: '订单编号',
        //   data: 'XS888888888',
        //   type: 'text',
        //   options: {
        //     field: 'order',
        //     testData: 'XS888888888',
        //     height: 16,
        //     fontSize: 6.75,
        //     fontWeight: '700',
        //     textAlign: 'left',
        //     textContentVerticalAlign: 'middle'
        //   }
        // },
        // {
        //   tid: 'providerModule1.date',
        //   title: '业务日期',
        //   data: '2020-01-01',
        //   type: 'text',
        //   options: {
        //     field: 'date',
        //     testData: '2020-01-01',
        //     height: 16,
        //     fontSize: 6.75,
        //     fontWeight: '700',
        //     textAlign: 'left',
        //     textContentVerticalAlign: 'middle'
        //   }
        // },
        {
          tid: 'providerModule1.barcode',
          title: '自定义条形码',
          data: '10000000010',
          type: 'text',
          options: {
            field: 'custom:barcode',
            testData: '10000000010',
            height: 32,
            fontSize: 10,
            lineHeight: 18,
            textAlign: 'left',
            textType: 'barcode'
          }
        },
        {
          tid: 'providerModule1.qrcode',
          title: '自定义二维码',
          data: '10000000010',
          type: 'text',
          options: {
            field: 'custom:qrcode',
            testData: '10000000010',
            height: 32,
            fontSize: 10,
            lineHeight: 18,
            textType: 'qrcode'
          }
        },
        // {
        //   tid: 'providerModule1.platform',
        //   title: '平台名称',
        //   data: '平台名称',
        //   type: 'text',
        //   options: {
        //     field: 'platform',
        //     testData: '平台名称',
        //     height: 17,
        //     fontSize: 16.5,
        //     fontWeight: '700',
        //     textAlign: 'center',
        //     hideTitle: true
        //   }
        // },
        { tid: 'providerModule1.image', title: 'Logo', data: '', type: 'image' }
      ]),
      new hiprint.PrintElementTypeGroup('表格', [
        {
          tid: 'providerModule1.table',
          title: '自定义表格',
          type: 'table',
          options: {
            field: 'table',
            fields: [
              { text: '名称', field: 'NAME' },
              { text: '数量', field: 'SL' },
              { text: '规格', field: 'GG' },
              { text: '条码', field: 'TM' },
              { text: '单价', field: 'DJ' }
            ]
          },
          columns: [
            [
              { title: '名称', align: 'center', field: 'NAME', width: 100 },
              { title: '数量', align: 'center', field: 'SL', width: 100 },
              { title: '条码', align: 'center', field: 'TM', width: 100 },
              { title: '规格', align: 'center', field: 'GG', width: 100 },
              { title: '单价', align: 'center', field: 'DJ', width: 100 }
            ]
          ]
        },
        {
          tid: 'providerModule1.tableDetail',
          title: '自定义表格2',
          type: 'table',
          options: {
            field: 'table',
            fields: [
              { text: '名称', field: 'NAME' },
              { text: '数量', field: 'SL' },
              { text: '规格', field: 'GG' },
              { text: '条码', field: 'TM' },
              { text: '备注', field: 'DETAIL' }
            ]
          },
          columns: [
            [
              { title: '名称', align: 'center', field: 'NAME', width: 100 },
              { title: '数量', align: 'center', field: 'SL', width: 100 },
              { title: '条码', align: 'center', field: 'TM', width: 100 },
              { title: '规格', align: 'center', field: 'GG', width: 100 },
              { title: '备注', align: 'center', field: 'DETAIL', width: 100 }
            ]
          ]
        }
      ])
    ]);
  };
  return {
    addElementTypes: addElementTypes
  };
};
