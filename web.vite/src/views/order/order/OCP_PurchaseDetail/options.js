// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'PurchaseRecordID',
        footer: "Foots",
        cnName: '采购明细',
        name: 'OCP_PurchaseDetail',
        newTabEdit: false,
        url: "/OCP_PurchaseDetail/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"ContractNumber":"","OrderNumber":"","PlanTrackingNumber":"","PurchaseDocNumber":"","SupplierCode":"","SupplierName":"","MaterialCategory":"","MaterialCode":"","MaterialName":"","PriorityLevel":"","PurchaseQuantity":"","StockInQuantity":"","OverdueQuantity":"","PurchaseOrderDate":""};
    const editFormOptions = [[{"title":"合同号","required":true,"field":"ContractNumber"},
                               {"title":"订单号","required":true,"field":"OrderNumber"},
                               {"title":"计划跟踪号","required":true,"field":"PlanTrackingNumber"}],
                              [{"title":"采购单据号","required":true,"field":"PurchaseDocNumber"},
                               {"title":"供应商编码","required":true,"field":"SupplierCode"},
                               {"title":"供应商名称","required":true,"field":"SupplierName"}],
                              [{"title":"物料分类","required":true,"field":"MaterialCategory"},
                               {"title":"物料编码","required":true,"field":"MaterialCode"},
                               {"title":"物料名称","required":true,"field":"MaterialName"}],
                              [{"title":"紧急等级","field":"PriorityLevel"},
                               {"title":"采购数量","required":true,"field":"PurchaseQuantity","type":"decimal"},
                               {"title":"入库数量","field":"StockInQuantity","type":"decimal"}],
                              [{"title":"超期数量","field":"OverdueQuantity","type":"decimal"},
                               {"title":"采购订单日期","required":true,"field":"PurchaseOrderDate","type":"datetime"}]];
    const searchFormFields = {"OrderNumber":"","PlanTrackingNumber":"","PurchaseDocNumber":"","SupplierCode":""};
    const searchFormOptions = [[{"title":"采购记录ID","field":"PurchaseRecordID","type":"like"},{"title":"订单计划ID","field":"PlanID","type":"like"},{"title":"采购类型","field":"PurchaseType","type":"like"},{"title":"合同号","field":"ContractNumber","type":"like"}],[{"title":"订单号","field":"OrderNumber","type":"like"},{"title":"计划跟踪号","field":"PlanTrackingNumber","type":"like"},{"title":"采购单据号","field":"PurchaseDocNumber","type":"like"},{"title":"供应商编码","field":"SupplierCode","type":"like"}]];
    const columns = [{field:'PurchaseRecordID',title:'采购记录ID',type:'int',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'PlanID',title:'订单计划ID',type:'int',sort:true,width:80,require:true,align:'left'},
                       {field:'PurchaseType',title:'采购类型',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'ContractNumber',title:'合同号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'OrderNumber',title:'订单号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'PlanTrackingNumber',title:'计划跟踪号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'PurchaseDocNumber',title:'采购单据号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'SupplierCode',title:'供应商编码',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'SupplierName',title:'供应商名称',type:'string',sort:true,width:110,require:true,align:'left'},
                       {field:'MaterialCategory',title:'物料分类',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:110,require:true,align:'left'},
                       {field:'PriorityLevel',title:'紧急等级',type:'string',sort:true,width:110,align:'left'},
                       {field:'PurchaseQuantity',title:'采购数量',type:'decimal',sort:true,width:110,require:true,align:'left'},
                       {field:'StockInQuantity',title:'入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueQuantity',title:'超期数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'PurchaseOrderDate',title:'采购订单日期',type:'datetime',sort:true,width:150,require:true,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'}];
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