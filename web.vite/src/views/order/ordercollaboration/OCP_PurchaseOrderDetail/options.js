// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'DetailID',
        footer: "Foots",
        cnName: '采购订单明细表',
        name: 'OCP_PurchaseOrderDetail',
        newTabEdit: false,
        url: "/OCP_PurchaseOrderDetail/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"FENTRYID":"","LineNumber":"","PlanTraceNo":"","MaterialID":"","MaterialNumber":"","MaterialName":"","MaterialType":"","PlanTaskMonth":"","PlanTaskWeek":"","Urgency":"","SupplierID":"","SupplierCode":"","DeliveryNo":"","PurchaseQty":"","RequiredDeliveryDate":"","ReplyDeliveryDate":"","InstockQty":"","UnfinishedQty":"","OverdueQty":"","OverdueDays":""};
    const editFormOptions = [[{"title":"采购订单明细ID","field":"FENTRYID"},
                               {"title":"行号","field":"LineNumber","type":"number"},
                               {"title":"计划跟踪号","field":"PlanTraceNo"}],
                              [{"title":"物料ID","field":"MaterialID"},
                               {"title":"物料编码","field":"MaterialNumber"},
                               {"title":"物料名称","field":"MaterialName"}],
                              [{"title":"物料类型","field":"MaterialType"},
                               {"title":"月度任务","field":"PlanTaskMonth"},
                               {"title":"周任务","field":"PlanTaskWeek"}],
                              [{"title":"紧急等级","field":"Urgency"},
                               {"title":"供应商ID","field":"SupplierID","type":"number"},
                               {"title":"供应商编码","field":"SupplierCode"}],
                              [{"title":"送货单号","field":"DeliveryNo"},
                               {"title":"采购数量","field":"PurchaseQty","type":"decimal"},
                               {"title":"要求交货日期","field":"RequiredDeliveryDate","type":"datetime"}],
                              [{"title":"回复交货日期","field":"ReplyDeliveryDate","type":"datetime"},
                               {"title":"入库数量","field":"InstockQty","type":"decimal"},
                               {"title":"未完数量","field":"UnfinishedQty","type":"decimal"}],
                              [{"title":"超期数量","field":"OverdueQty","type":"decimal"},
                               {"title":"超期时长","field":"OverdueDays","type":"number"}]];
    const searchFormFields = {"Urgency":"","SupplierCode":"","DeliveryNo":""};
    const searchFormOptions = [[{"title":"计划跟踪号","field":"PlanTraceNo","type":"like"},{"title":"物料编码","field":"MaterialNumber","type":"like"},{"title":"物料名称","field":"MaterialName","type":"like"},{"title":"物料类型","field":"MaterialType","type":"like"}],[{"title":"紧急等级","field":"Urgency","type":"like"},{"title":"供应商编码","field":"SupplierCode","type":"like"},{"title":"送货单号","field":"DeliveryNo","type":"like"}]];
    const columns = [{field:'DetailID',title:'明细ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'OrderID',title:'主表ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FID',title:'采购订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FENTRYID',title:'采购订单明细ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'LineNumber',title:'行号',type:'int',sort:true,width:80,align:'left'},
                       {field:'PlanTraceNo',title:'计划跟踪号',type:'string',sort:true,width:220,align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:220,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:220,align:'left'},
                       {field:'MaterialType',title:'物料类型',type:'string',sort:true,width:120,align:'left'},
                       {field:'PlanTaskMonth',title:'月度任务',type:'string',sort:true,width:220,align:'left'},
                       {field:'PlanTaskWeek',title:'周任务',type:'string',sort:true,width:220,align:'left'},
                       {field:'Urgency',title:'紧急等级',type:'string',sort:true,width:220,align:'left'},
                       {field:'SupplierID',title:'供应商ID',type:'int',sort:true,width:80,hidden:true,align:'left'},
                       {field:'SupplierCode',title:'供应商编码',type:'string',sort:true,width:220,align:'left'},
                       {field:'DeliveryNo',title:'送货单号',type:'string',sort:true,width:220,align:'left'},
                       {field:'PurchaseQty',title:'采购数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'RequiredDeliveryDate',title:'要求交货日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'ReplyDeliveryDate',title:'回复交货日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'InstockQty',title:'入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'UnfinishedQty',title:'未完数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueQty',title:'超期数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueDays',title:'超期时长',type:'int',sort:true,width:80,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'}];
    const detail ={columns:[]};
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