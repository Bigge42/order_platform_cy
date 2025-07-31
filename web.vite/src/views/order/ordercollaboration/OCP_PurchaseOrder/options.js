// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'OrderID',
        footer: "Foots",
        cnName: '采购订单表',
        name: 'OCP_PurchaseOrder',
        newTabEdit: false,
        url: "/OCP_PurchaseOrder/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"PlanTaskMonth":"","PlanTaskWeek":"","Urgency":"","BusinessType":"","SupplierID":"","SupplierCode":"","SupplierName":"","PurchaseQty":"","InstockQty":"","UnfinishedQty":"","OverdueQty":"","PurchasePerson":""};
    const editFormOptions = [[{"title":"月度任务","field":"PlanTaskMonth"},
                               {"title":"周任务","field":"PlanTaskWeek"},
                               {"title":"紧急等级","field":"Urgency"}],
                              [{"title":"业务类型","field":"BusinessType"},
                               {"title":"供应商ID","field":"SupplierID","type":"number"},
                               {"title":"供应商编码","field":"SupplierCode"}],
                              [{"title":"供应商名称","field":"SupplierName"},
                               {"title":"采购数量","field":"PurchaseQty","type":"decimal"},
                               {"title":"入库数量","field":"InstockQty","type":"decimal"}],
                              [{"title":"未完数量","field":"UnfinishedQty","type":"decimal"},
                               {"title":"超期数量","field":"OverdueQty","type":"decimal"},
                               {"title":"采购负责人","field":"PurchasePerson"}]];
    const searchFormFields = {};
    const searchFormOptions = [[{"title":"单据编号","field":"BillNo","type":"like"},{"title":"紧急等级","field":"Urgency","type":"like"},{"title":"业务类型","field":"BusinessType","type":"like"},{"title":"供应商编码","field":"SupplierCode","type":"like"},{"title":"供应商名称","field":"SupplierName","type":"like"}]];
    const columns = [{field:'OrderID',title:'订单ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'BillNo',title:'单据编号',type:'string',sort:true,width:120,align:'left'},
                       {field:'FID',title:'采购订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'PlanTaskMonth',title:'月度任务',type:'string',sort:true,width:220,align:'left'},
                       {field:'PlanTaskWeek',title:'周任务',type:'string',sort:true,width:220,align:'left'},
                       {field:'Urgency',title:'紧急等级',type:'string',sort:true,width:220,align:'left'},
                       {field:'BusinessType',title:'业务类型',type:'string',sort:true,width:120,align:'left'},
                       {field:'SupplierID',title:'供应商ID',type:'int',sort:true,width:80,hidden:true,align:'left'},
                       {field:'SupplierCode',title:'供应商编码',type:'string',sort:true,width:220,align:'left'},
                       {field:'SupplierName',title:'供应商名称',type:'string',sort:true,width:220,align:'left'},
                       {field:'PurchaseQty',title:'采购数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'InstockQty',title:'入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'UnfinishedQty',title:'未完数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueQty',title:'超期数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'PurchasePerson',title:'采购负责人',type:'string',sort:true,width:120,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'}];
    const detail =  {
                    cnName: '采购订单明细表',
                    table: 'OCP_PurchaseOrderDetail',
                    columns: [{field:'DetailID',title:'明细ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'OrderID',title:'主表ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FID',title:'采购订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FENTRYID',title:'采购订单明细ID',type:'long',sort:true,width:80,hidden:true,edit:{type:''},align:'left'},
                       {field:'LineNumber',title:'行号',type:'int',sort:true,width:80,edit:{type:''},align:'left'},
                       {field:'PlanTraceNo',title:'计划跟踪号',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',sort:true,width:80,hidden:true,edit:{type:''},align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'MaterialType',title:'物料类型',type:'string',sort:true,width:120,edit:{type:''},align:'left'},
                       {field:'PlanTaskMonth',title:'总任务月',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'PlanTaskWeek',title:'总任务周',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'Urgency',title:'紧急等级',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'SupplierID',title:'供应商ID',type:'int',sort:true,width:80,hidden:true,edit:{type:''},align:'left'},
                       {field:'SupplierCode',title:'供应商编码',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'DeliveryNo',title:'送货单号',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'PurchaseQty',title:'采购数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'RequiredDeliveryDate',title:'要求交货日期',type:'date',sort:true,width:150,edit:{type:'datetime'},align:'left'},
                       {field:'ReplyDeliveryDate',title:'回复交货日期',type:'date',sort:true,width:150,edit:{type:'datetime'},align:'left'},
                       {field:'InstockQty',title:'入库数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'UnfinishedQty',title:'未完数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'OverdueQty',title:'超期数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'OverdueDays',title:'超期时长',type:'int',sort:true,width:80,edit:{type:''},align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'}],
                    sortName: 'CreateDate',
                    key: 'DetailID'
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