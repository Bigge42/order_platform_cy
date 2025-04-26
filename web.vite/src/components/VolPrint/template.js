export default {
  panels: [
    {
      index: 0,
      name: 1,
      height: 297,
      width: 210,
      paperHeader: 49.5,
      paperFooter: 841.8897637795277,
      printElements: [
        {
          options: {
            left: 430.5,
            top: 9,
            height: 25,
            width: 155,
            title: 'OT2023080140000001',
            fontFamily: '微软雅黑',
            textAlign: 'center',
            textType: 'barcode',
            right: 591.5000228881836,
            bottom: 34,
            vCenter: 514.0000228881836,
            hCenter: 21.5,
            coordinateSync: false,
            widthHeightSync: false,
            qrCodeLevel: 0
          },
          printElementType: { title: '自定义文本', type: 'text' }
        },
        {
          options: {
            left: 175.5,
            top: 10.5,
            height: 27,
            width: 259,
            title: '销售订单',
            fontSize: 19,
            fontWeight: '600',
            textAlign: 'center',
            lineHeight: 26,
            coordinateSync: true,
            widthHeightSync: true,
            draggable: false,
            qrCodeLevel: 0
          },
          printElementType: { title: '自定义文本', type: 'text' }
        },
        {
          options: {
            left: 166.5,
            top: 63,
            height: 16,
            width: 120,
            field: '订单类型',
            testData: '2020-01-01',
            fontSize: 6.75,
            fontWeight: '700',
            textContentVerticalAlign: 'middle',
            title: '订单类型',
            coordinateSync: false,
            widthHeightSync: false,
            qrCodeLevel: 0
          },
          printElementType: { title: '业务日期', type: 'text' }
        },
        {
          options: {
            left: 328.5,
            top: 63,
            height: 16,
            width: 120,
            field: '订单状态',
            testData: '2020-01-01',
            fontSize: 6.75,
            fontWeight: '700',
            textContentVerticalAlign: 'middle',
            title: '订单状态',
            right: 443.2500228881836,
            bottom: 80.5,
            vCenter: 383.2500228881836,
            hCenter: 72.5,
            coordinateSync: false,
            widthHeightSync: false,
            qrCodeLevel: 0
          },
          printElementType: { title: '业务日期', type: 'text' }
        },
        {
          options: {
            left: 468,
            top: 63,
            height: 16,
            width: 120,
            field: 'date',
            testData: '2020-01-01',
            fontSize: 6.75,
            fontWeight: '700',
            textContentVerticalAlign: 'middle',
            title: '订单日期',
            coordinateSync: false,
            widthHeightSync: false,
            qrCodeLevel: 0
          },
          printElementType: { title: '业务日期', type: 'text' }
        },
        {
          options: {
            left: 13.5,
            top: 63,
            height: 16,
            width: 120,
            field: 'OrderNo',
            testData: 'T2023080140001',
            fontSize: 6.75,
            fontWeight: '700',
            textContentVerticalAlign: 'middle',
            title: '订单编号',
            right: 139.5,
            bottom: 79,
            vCenter: 79.5,
            hCenter: 71,
            coordinateSync: false,
            widthHeightSync: false,
            qrCodeLevel: 0
          },
          printElementType: { title: '业务日期', type: 'text' }
        },
        {
          options: {
            left: 13.5,
            top: 90,
            height: 39,
            width: 573,
            field: 'table',
            fields: [
              { text: '名称', field: 'name' },
              { text: '数量', field: 'SL' },
              { text: '规格', field: 'GG' },
              { text: '条码', field: 'TM' },
              { text: '单价', field: 'DJ' },
              { text: '金额', field: 'JE' },
              { text: '备注', field: 'DETAIL' }
            ],
            right: 592.5,
            bottom: 147,
            vCenter: 306,
            hCenter: 129,
            columns: [
              [
                {
                  width: 81.85714285714288,
                  title: '名称',
                  field: 'name',
                  checked: true,
                  columnId: 'NAME',
                  fixed: false,
                  rowspan: 1,
                  colspan: 1,
                  align: 'center'
                },
                {
                  width: 81.85714285714288,
                  title: '数量',
                  field: 'count',
                  checked: true,
                  columnId: 'count',
                  fixed: false,
                  rowspan: 1,
                  colspan: 1,
                  align: 'center'
                },
                {
                  width: 81.85714285714288,
                  title: '条码',
                  field: 'TM',
                  checked: true,
                  columnId: 'TM',
                  fixed: false,
                  rowspan: 1,
                  colspan: 1,
                  align: 'center'
                },
                {
                  width: 81.85714285714288,
                  title: '规格',
                  field: 'GG',
                  checked: true,
                  columnId: 'GG',
                  fixed: false,
                  rowspan: 1,
                  colspan: 1,
                  align: 'center'
                },
                {
                  width: 81.85714285714288,
                  title: '单价',
                  field: 'amount',
                  checked: true,
                  columnId: 'amount',
                  fixed: false,
                  rowspan: 1,
                  colspan: 1,
                  align: 'center'
                },
                {
                  width: 81.85714285714288,
                  title: '金额',
                  field: 'JE',
                  checked: true,
                  columnId: 'JE',
                  fixed: false,
                  rowspan: 1,
                  colspan: 1,
                  align: 'center'
                },
                {
                  width: 81.85714285714288,
                  title: '备注',
                  field: 'DETAIL',
                  checked: true,
                  columnId: 'DETAIL',
                  fixed: false,
                  rowspan: 1,
                  colspan: 1,
                  align: 'center'
                }
              ]
            ]
          },
          printElementType: {
            title: '批量打印',
            type: 'table',
            editable: true,
            columnDisplayEditable: true,
            columnDisplayIndexEditable: true,
            columnTitleEditable: true,
            columnResizable: true,
            columnAlignEditable: true,
            isEnableEditField: true,
            isEnableContextMenu: true,
            isEnableInsertRow: true,
            isEnableDeleteRow: true,
            isEnableInsertColumn: true,
            isEnableDeleteColumn: true,
            isEnableMergeCell: true
          }
        }
      ],
      watermarkOptions: {}
    }
  ]
};
