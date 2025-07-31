// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'DetailID',
        footer: "Foots",
        cnName: '金工生产订单明细表',
        name: 'OCP_JGPrdMODetail',
        newTabEdit: false,
        url: "/OCP_JGPrdMODetail/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"FENTRYID":"","LineNumber":"","ProductionOrderStatus":"","PlanTraceNo":"","MaterialID":"","MaterialNumber":"","MaterialName":"","PlanTaskMonth":"","PlanTaskWeek":"","Urgency":"","ProductCategory":"","ProductionQty":"","InboundQty":"","UnInboundQty":"","OverdueQty":"","PlanCompleteDate":"","ActualCompleteDate":"","PickMtrlDate":""};
    const editFormOptions = [[{"title":"生产订单明细ID","field":"FENTRYID"},
                               {"title":"行号","field":"LineNumber","type":"number"},
                               {"title":"生产订单状态","field":"ProductionOrderStatus"}],
                              [{"title":"计划跟踪号","field":"PlanTraceNo"},
                               {"title":"物料ID","field":"MaterialID"},
                               {"title":"物料编码","field":"MaterialNumber"}],
                              [{"title":"物料名称","field":"MaterialName"},
                               {"title":"月度任务","field":"PlanTaskMonth"},
                               {"title":"周任务","field":"PlanTaskWeek"}],
                              [{"title":"紧急等级","field":"Urgency"},
                               {"title":"产品大类","field":"ProductCategory"},
                               {"title":"生产数量","field":"ProductionQty","type":"decimal"}],
                              [{"title":"已入库数量","field":"InboundQty","type":"decimal"},
                               {"title":"未入库数量","field":"UnInboundQty","type":"decimal"},
                               {"title":"超期数量","field":"OverdueQty","type":"decimal"}],
                              [{"title":"计划完工日期","field":"PlanCompleteDate","type":"datetime"},
                               {"title":"实际完工日期","field":"ActualCompleteDate","type":"datetime"},
                               {"title":"领料日期","field":"PickMtrlDate","type":"datetime"}]];
    const searchFormFields = {"LineNumber":"","ProductionOrderStatus":"","PlanTraceNo":"","MaterialID":""};
    const searchFormOptions = [[{"title":"明细ID","field":"DetailID"},{"title":"主表ID","field":"ID"},{"title":"生产订单ID","field":"FID"},{"title":"生产订单明细ID","field":"FENTRYID","type":"like"}],[{"title":"行号","field":"LineNumber","type":"like"},{"title":"生产订单状态","field":"ProductionOrderStatus","type":"like"},{"title":"计划跟踪号","field":"PlanTraceNo","type":"like"},{"title":"物料ID","field":"MaterialID"}]];
    const columns = [{field:'DetailID',title:'明细ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ID',title:'主表ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FID',title:'生产订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FENTRYID',title:'生产订单明细ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'LineNumber',title:'行号',type:'int',sort:true,width:80,align:'left'},
                       {field:'ProductionOrderStatus',title:'生产订单状态',type:'string',sort:true,width:120,align:'left'},
                       {field:'PlanTraceNo',title:'计划跟踪号',type:'string',sort:true,width:220,align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:220,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:220,align:'left'},
                       {field:'PlanTaskMonth',title:'月度任务',type:'string',sort:true,width:220,align:'left'},
                       {field:'PlanTaskWeek',title:'周任务',type:'string',sort:true,width:220,align:'left'},
                       {field:'Urgency',title:'紧急等级',type:'string',sort:true,width:220,align:'left'},
                       {field:'ProductCategory',title:'产品大类',type:'string',sort:true,width:120,align:'left'},
                       {field:'ProductionQty',title:'生产数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'InboundQty',title:'已入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'UnInboundQty',title:'未入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueQty',title:'超期数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'PlanCompleteDate',title:'计划完工日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'ActualCompleteDate',title:'实际完工日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'PickMtrlDate',title:'领料日期',type:'date',sort:true,width:150,align:'left'},
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