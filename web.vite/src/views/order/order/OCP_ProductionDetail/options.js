// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'DetailID',
        footer: "Foots",
        cnName: '生产明细',
        name: 'OCP_ProductionDetail',
        newTabEdit: false,
        url: "/OCP_ProductionDetail/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"ContractNumber":"","OrderNumber":"","PlanTrackingNumber":"","ProductionDocNumber":"","ProductionQuantity":"","StockInQuantity":"","UnstockedQuantity":"","OverdueQuantity":"","MaterialCode":"","MaterialName":"","MaterialCategory":""};
    const editFormOptions = [[{"title":"合同号","required":true,"field":"ContractNumber"},
                               {"title":"订单号","required":true,"field":"OrderNumber"},
                               {"title":"计划跟踪号","required":true,"field":"PlanTrackingNumber"}],
                              [{"title":"生产单据号","required":true,"field":"ProductionDocNumber"},
                               {"title":"生产数量","required":true,"field":"ProductionQuantity","type":"decimal"},
                               {"title":"已入库数量","field":"StockInQuantity","type":"decimal"}],
                              [{"title":"未入库数量","field":"UnstockedQuantity","type":"decimal"},
                               {"title":"超期数量","field":"OverdueQuantity","type":"decimal"},
                               {"title":"物料编码","required":true,"field":"MaterialCode"}],
                              [{"title":"物料名称","required":true,"field":"MaterialName"},
                               {"title":"物料分类","required":true,"field":"MaterialCategory"}]];
    const searchFormFields = {"OrderNumber":"","PlanTrackingNumber":"","ProductionDocNumber":"","ProductionQuantity":"","StockInQuantity":"","UnstockedQuantity":"","OverdueQuantity":"","MaterialCode":"","MaterialName":"","MaterialCategory":""};
    const searchFormOptions = [[{"title":"明细ID","field":"DetailID","type":"like"},{"title":"订单计划ID","field":"PlanID","type":"like"},{"title":"生产车间","field":"ProductionWorkshop","type":"like"},{"title":"合同号","field":"ContractNumber","type":"like"}],[{"title":"订单号","field":"OrderNumber","type":"like"},{"title":"计划跟踪号","field":"PlanTrackingNumber","type":"like"},{"title":"生产单据号","field":"ProductionDocNumber","type":"like"},{"title":"生产数量","field":"ProductionQuantity","type":"like"}],[{"title":"已入库数量","field":"StockInQuantity","type":"like"},{"title":"未入库数量","field":"UnstockedQuantity","type":"like"},{"title":"超期数量","field":"OverdueQuantity","type":"like"},{"title":"物料编码","field":"MaterialCode","type":"like"}],[{"title":"物料名称","field":"MaterialName","type":"like"},{"title":"物料分类","field":"MaterialCategory","type":"like"}]];
    const columns = [{field:'DetailID',title:'明细ID',type:'int',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'PlanID',title:'订单计划ID',type:'int',sort:true,width:80,require:true,align:'left'},
                       {field:'ProductionWorkshop',title:'生产车间',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'ContractNumber',title:'合同号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'OrderNumber',title:'订单号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'PlanTrackingNumber',title:'计划跟踪号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'ProductionDocNumber',title:'生产单据号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'ProductionQuantity',title:'生产数量',type:'decimal',sort:true,width:110,require:true,align:'left'},
                       {field:'StockInQuantity',title:'已入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'UnstockedQuantity',title:'未入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueQuantity',title:'超期数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:110,require:true,align:'left'},
                       {field:'MaterialCategory',title:'物料分类',type:'string',sort:true,width:120,require:true,align:'left'},
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