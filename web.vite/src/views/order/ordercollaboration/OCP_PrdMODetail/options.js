// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'DetailID',
        footer: "Foots",
        cnName: '整机生产订单明细表',
        name: 'OCP_PrdMODetail',
        newTabEdit: false,
        url: "/OCP_PrdMODetail/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"FENTRYID":"","Seq":"","PlanTraceNo":"","SalesContractNo":"","SalesDocumentNo":"","ProductionOrderStatus":"","MaterialID":"","MaterialNumber":"","MaterialName":"","ProductCategory":"","PlanTaskMonth":"","PlanTaskWeek":"","Urgency":"","PlanQty":"","InboundQty":"","UnInboundQty":"","OverdueQty":"","PickMtrlDate":"","PlanCompleteDate":"","ActualCompleteDate":""};
    const editFormOptions = [[{"title":"生产订单明细ID","field":"FENTRYID"},
                               {"title":"行号","field":"Seq","type":"number"},
                               {"title":"计划跟踪号","field":"PlanTraceNo"}],
                              [{"title":"销售合同号","field":"SalesContractNo"},
                               {"title":"销售单据号","field":"SalesDocumentNo"},
                               {"title":"生产订单状态","field":"ProductionOrderStatus"}],
                              [{"title":"物料ID","field":"MaterialID"},
                               {"title":"物料编码","field":"MaterialNumber"},
                               {"title":"物料名称","field":"MaterialName"}],
                              [{"title":"产品大类","field":"ProductCategory"},
                               {"title":"月度任务","field":"PlanTaskMonth"},
                               {"title":"周任务","field":"PlanTaskWeek"}],
                              [{"title":"紧急等级","field":"Urgency"},
                               {"title":"计划数量","field":"PlanQty","type":"decimal"},
                               {"title":"已入库数量","field":"InboundQty","type":"decimal"}],
                              [{"title":"未入库数量","field":"UnInboundQty","type":"decimal"},
                               {"title":"超期数量","field":"OverdueQty","type":"decimal"},
                               {"title":"生产订单领料日期","field":"PickMtrlDate","type":"datetime"}],
                              [{"title":"计划完工日期","field":"PlanCompleteDate","type":"datetime"},
                               {"title":"实际完工日期","field":"ActualCompleteDate","type":"datetime"}]];
    const searchFormFields = {"MaterialNumber":"","MaterialName":"","ProductCategory":"","Urgency":""};
    const searchFormOptions = [[{"title":"计划跟踪号","field":"PlanTraceNo","type":"like"},{"title":"销售合同号","field":"SalesContractNo","type":"like"},{"title":"销售单据号","field":"SalesDocumentNo","type":"like"},{"title":"生产订单状态","field":"ProductionOrderStatus","type":"like"}],[{"title":"物料编码","field":"MaterialNumber","type":"like"},{"title":"物料名称","field":"MaterialName","type":"like"},{"title":"产品大类","field":"ProductCategory","type":"like"},{"title":"紧急等级","field":"Urgency","type":"like"}]];
    const columns = [{field:'DetailID',title:'明细ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ID',title:'主表ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FID',title:'生产订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FENTRYID',title:'生产订单明细ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'Seq',title:'行号',type:'int',sort:true,width:80,align:'left'},
                       {field:'PlanTraceNo',title:'计划跟踪号',type:'string',sort:true,width:220,align:'left'},
                       {field:'SalesContractNo',title:'销售合同号',type:'string',sort:true,width:120,align:'left'},
                       {field:'SalesDocumentNo',title:'销售单据号',type:'string',sort:true,width:120,align:'left'},
                       {field:'ProductionOrderStatus',title:'生产订单状态',type:'string',sort:true,width:120,align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:220,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:220,align:'left'},
                       {field:'ProductCategory',title:'产品大类',type:'string',sort:true,width:120,align:'left'},
                       {field:'PlanTaskMonth',title:'月度任务',type:'string',sort:true,width:220,align:'left'},
                       {field:'PlanTaskWeek',title:'周任务',type:'string',sort:true,width:220,align:'left'},
                       {field:'Urgency',title:'紧急等级',type:'string',sort:true,width:220,align:'left'},
                       {field:'PlanQty',title:'计划数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'InboundQty',title:'已入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'UnInboundQty',title:'未入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueQty',title:'超期数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'PickMtrlDate',title:'生产订单领料日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'PlanCompleteDate',title:'计划完工日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'ActualCompleteDate',title:'实际完工日期',type:'date',sort:true,width:150,align:'left'},
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