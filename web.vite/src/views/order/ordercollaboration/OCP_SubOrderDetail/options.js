// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'DetailID',
        footer: "Foots",
        cnName: '委外订单明细表',
        name: 'OCP_SubOrderDetail',
        newTabEdit: false,
        url: "/OCP_SubOrderDetail/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"FENTRYID":"","Seq":"","MtoNo":"","MaterialID":"","MaterialNumber":"","MaterialName":"","MaterialType":"","SupplierID":"","SupplierCode":"","DeliveryNo":"","PurchaseQty":"","InstockQty":"","UnfinishedQty":"","OverdueQty":"","OverdueDays":"","PickDate":""};
    const editFormOptions = [[{"title":"委外订单明细ID","field":"FENTRYID"},
                               {"title":"行号","field":"Seq","type":"number"},
                               {"title":"计划跟踪号","field":"MtoNo"}],
                              [{"title":"物料ID","field":"MaterialID"},
                               {"title":"物料编码","field":"MaterialNumber"},
                               {"title":"物料名称","field":"MaterialName"}],
                              [{"title":"物料类型","field":"MaterialType"},
                               {"title":"供应商ID","field":"SupplierID","type":"number"},
                               {"title":"供应商编码","field":"SupplierCode"}],
                              [{"title":"送货单号","field":"DeliveryNo"},
                               {"title":"采购数量","field":"PurchaseQty","type":"decimal"},
                               {"title":"入库数量","field":"InstockQty","type":"decimal"}],
                              [{"title":"未完数量","field":"UnfinishedQty","type":"decimal"},
                               {"title":"超期数量","field":"OverdueQty","type":"decimal"},
                               {"title":"超期时长","field":"OverdueDays","type":"number"}],
                              [{"title":"领料日期","field":"PickDate","type":"datetime"}]];
    const searchFormFields = {"Seq":"","MtoNo":"","MaterialID":"","MaterialNumber":""};
    const searchFormOptions = [[{"title":"明细ID","field":"DetailID","type":"like"},{"title":"主表ID","field":"ID","type":"like"},{"title":"委外订单ID","field":"FID","type":"like"},{"title":"委外订单明细ID","field":"FENTRYID","type":"like"}],[{"title":"行号","field":"Seq","type":"like"},{"title":"计划跟踪号","field":"MtoNo","type":"like"},{"title":"物料ID","field":"MaterialID","type":"like"},{"title":"物料编码","field":"MaterialNumber","type":"like"}]];
    const columns = [{field:'DetailID',title:'明细ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ID',title:'主表ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FID',title:'委外订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FENTRYID',title:'委外订单明细ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'Seq',title:'行号',type:'int',sort:true,width:80,align:'left'},
                       {field:'MtoNo',title:'计划跟踪号',type:'string',sort:true,width:220,align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:220,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:220,align:'left'},
                       {field:'MaterialType',title:'物料类型',type:'string',sort:true,width:120,align:'left'},
                       {field:'SupplierID',title:'供应商ID',type:'int',sort:true,width:80,hidden:true,align:'left'},
                       {field:'SupplierCode',title:'供应商编码',type:'string',sort:true,width:220,align:'left'},
                       {field:'DeliveryNo',title:'送货单号',type:'string',sort:true,width:220,align:'left'},
                       {field:'PurchaseQty',title:'采购数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'InstockQty',title:'入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'UnfinishedQty',title:'未完数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueQty',title:'超期数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueDays',title:'超期时长',type:'int',sort:true,width:80,align:'left'},
                       {field:'PickDate',title:'领料日期',type:'date',sort:true,width:150,align:'left'},
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