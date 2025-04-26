// *Author：jxx
// *Contact：283591387@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Order_Id',
        footer: "Foots",
        cnName: '订单管理',
        name: 'Demo_Order',
        newTabEdit: false,
        url: "/Demo_Order/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"OrderNo":"","OrderType":"","TotalPrice":"","TotalQty":"","OrderDate":"","Customer":"","PhoneNo":"","OrderStatus":""};
    const editFormOptions = [[{"title":"订单编号","required":true,"field":"OrderNo","disabled":true},
                               {"dataKey":"订单类型","data":[],"title":"订单类型","required":true,"field":"OrderType","type":"select"},
                               {"title":"总价","field":"TotalPrice","type":"decimal"},
                               {"title":"总数量","field":"TotalQty","type":"number"}],
                              [{"title":"订单日期","required":true,"field":"OrderDate","type":"datetime"},
                               {"title":"客户","field":"Customer","disabled":true,"type":"selectTable"},
                               {"title":"手机","field":"PhoneNo","disabled":true,"type":"text"},
                               {"dataKey":"订单状态","data":[],"title":"订单状态","required":true,"field":"OrderStatus","type":"select"}]];
    const searchFormFields = {"OrderNo":"","OrderType":"","TotalPrice":[null,null],"OrderDate":"","PhoneNo":"","OrderStatus":"","CreateDate":""};
    const searchFormOptions = [[{"title":"订单编号","field":"OrderNo"},{"dataKey":"订单类型","data":[],"title":"订单类型","field":"OrderType","type":"select"},{"title":"总价","field":"TotalPrice","type":"range"},{"title":"手机","field":"PhoneNo"}],[{"title":"订单日期","field":"OrderDate","type":"datetime"},{"dataKey":"订单状态","data":[],"title":"订单状态","field":"OrderStatus","type":"select"},{"title":"创建时间","field":"CreateDate","type":"datetime"}]];
    const columns = [{field:'Order_Id',title:'Order_Id',type:'string',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'OrderNo',title:'订单编号',type:'string',link:true,width:130,readonly:true,require:true,align:'left'},
                       {field:'OrderType',title:'订单类型',type:'int',bind:{ key:'订单类型',data:[]},width:90,require:true,align:'left'},
                       {field:'TotalPrice',title:'总价',type:'decimal',width:70,align:'left'},
                       {field:'TotalQty',title:'总数量',type:'int',width:80,align:'left'},
                       {field:'OrderDate',title:'订单日期',type:'date',width:95,require:true,align:'left'},
                       {field:'CustomerId',title:'客户',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Customer',title:'客户',type:'string',width:80,readonly:true,align:'left'},
                       {field:'PhoneNo',title:'手机',type:'string',width:110,readonly:true,align:'left'},
                       {field:'OrderStatus',title:'订单状态',type:'int',bind:{ key:'订单状态',data:[]},width:90,require:true,align:'left'},
                       {field:'AuditStatus',title:'审核状态',type:'int',bind:{ key:'audit',data:[]},width:80,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:100,hidden:true,align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:80,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:145,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'Modifier',type:'string',width:130,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'ModifyDate',type:'datetime',width:110,hidden:true,align:'left'},
                       {field:'AuditId',title:'AuditId',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Auditor',title:'审核人',type:'string',width:110,hidden:true,align:'left'},
                       {field:'AuditDate',title:'审核时间',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'AuditReason',title:'AuditReason',type:'string',width:220,hidden:true,align:'left'}];
    const detail =  {
                    cnName: '订单明细',
                    table: 'Demo_OrderList',
                    columns: [{field:'OrderList_Id',title:'OrderList_Id',type:'string',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'Order_Id',title:'Order_Id',type:'string',width:110,hidden:true,align:'left'},
                       {field:'GoodsId',title:'商品id',type:'string',width:110,hidden:true,align:'left'},
                       {field:'GoodsName',title:'商品名称',type:'string',width:120,edit:{type:'selectTable'},require:true,align:'left'},
                       {field:'GoodsCode',title:'商品编号',type:'string',width:120,edit:{type:'text'},require:true,align:'left'},
                       {field:'Img',title:'商品图片',type:'img',width:100,align:'left'},
                       {field:'Specs',title:'商品规格',type:'string',bind:{ key:'商品规格',data:[]},width:120,readonly:true,edit:{type:'select'},align:'left'},
                       {field:'Price',title:'单价',type:'decimal',width:110,readonly:true,edit:{type:''},require:true,align:'left'},
                       {field:'Qty',title:'数量',type:'int',width:110,edit:{type:''},require:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:100,edit:{type:''},align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:145,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'Modifier',type:'string',width:130,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'ModifyDate',type:'datetime',width:110,hidden:true,align:'left'}],
                    sortName: 'CreateDate',
                    key: 'OrderList_Id'
                                            };
    const details = [];

    return {
        table,
        key,
        tableName,
        tableCNName,
        newTabEdit,
        editFormFields,
        editFormOptions,
        searchFormFields,
        searchFormOptions,
        columns,
        detail,
        details
    };
}