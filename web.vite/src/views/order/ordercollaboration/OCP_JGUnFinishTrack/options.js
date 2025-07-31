// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'TrackID',
        footer: "Foots",
        cnName: '金工跟踪',
        name: 'OCP_JGUnFinishTrack',
        newTabEdit: false,
        url: "/OCP_JGUnFinishTrack/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"ProductionOrderNo":"","MOBillNo":"","FENTRYID":"","MaterialNumber":"","MaterialName":"","MaterialID":"","PlanTaskMonth":"","PlanTaskWeek":"","Urgency":"","StartDate":"","MESBatchNo":"","CurrentProcess":"","MaterialRequestDate":"","ProductionQty":"","InboundQty":"","ReplyCompleteDate":""};
    const editFormOptions = [[{"title":"生产订单明细ID","field":"FENTRYID"},
                               {"title":"生产单据号","field":"ProductionOrderNo"},
                               {"title":"工单编号","field":"MOBillNo"}],
                              [{"title":"物料ID","field":"MaterialID"},
                               {"title":"物料编码","field":"MaterialNumber"},
                               {"title":"物料名称","field":"MaterialName"}],
                              [{"title":"月度任务","field":"PlanTaskMonth"},
                               {"title":"周任务","field":"PlanTaskWeek"},
                               {"title":"紧急等级","field":"Urgency"}],
                              [{"title":"开工日期","field":"StartDate","type":"datetime"},
                               {"title":"当前工序","field":"CurrentProcess"},
                               {"title":"MES批次号","field":"MESBatchNo"}],
                              [{"title":"叫料日期（生产工单）","field":"MaterialRequestDate","type":"datetime"},
                               {"title":"生产数量","field":"ProductionQty","type":"decimal"},
                               {"title":"入库数量","field":"InboundQty","type":"decimal"}],
                              [{"title":"回复完工日期","field":"ReplyCompleteDate","type":"datetime"}]];
    const searchFormFields = {"MaterialName":"","Urgency":"","MESBatchNo":"","CurrentProcess":""};
    const searchFormOptions = [[{"title":"计划跟踪号","field":"PlanTraceNo","type":"like"},{"title":"生产单据号","field":"ProductionOrderNo","type":"like"},{"title":"工单编号","field":"MOBillNo","type":"like"},{"title":"物料编码","field":"MaterialNumber","type":"like"}],[{"title":"物料名称","field":"MaterialName","type":"like"},{"title":"紧急等级","field":"Urgency","type":"like"},{"title":"当前工序","field":"CurrentProcess","type":"like"},{"title":"MES批次号","field":"MESBatchNo","type":"like"}]];
    const columns = [{field:'ProductionOrderNo',title:'生产单据号',type:'string',sort:true,width:120,align:'left'},
                       {field:'BillStatus',title:'生产订单状态',type:'string',width:120,align:'left'},
                       {field:'PlanTraceNo',title:'计划跟踪号',type:'string',sort:true,width:220,align:'left'},
                       {field:'MOBillNo',title:'工单编号',type:'string',sort:true,width:120,align:'left'},
                       {field:'ExecuteStatus',title:'执行状态',type:'string',width:120,align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:220,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:220,align:'left'},
                       {field:'Specification',title:'规格型号',type:'string',width:220,align:'left'},
                       {field:'MaterialCategory',title:'物料分类',type:'string',width:220,align:'left'},
                       {field:'ProScheduleYearMonth',title:'排产月份',type:'string',width:120,align:'left'},
                       {field:'PlanTaskMonth',title:'月度任务',type:'string',sort:true,width:120,align:'left'},
                       {field:'PlanTaskWeek',title:'周任务',type:'string',sort:true,width:120,align:'left'},
                       {field:'Urgency',title:'紧急等级',type:'string',sort:true,width:120,align:'left'},
                       {field:'StartDate',title:'开工日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'MaterialRequestDate',title:'叫料日期（生产工单）',type:'date',sort:true,width:180,align:'left'},
                       {field:'MESBatchNo',title:'MES批次号',type:'string',sort:true,width:120,align:'left'},
                       {field:'ProcessSubOutDate',title:'工序委外发出日期',type:'datetime',width:180,align:'left'},
                       {field:'ProcessSubInstockDate',title:'工序入库日期',type:'datetime',width:170,align:'left'},
                       {field:'CurrentProcess',title:'当前工序',type:'string',sort:true,width:120,align:'left'},
                       {field:'CurrentProcessStatus',title:'当前工序状态',type:'string',width:120,align:'left'},
                       {field:'ProductionQty',title:'生产数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'InboundQty',title:'入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'ReplyCompleteDate',title:'回复完工日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'OverdueDays',title:'超期天数',type:'int',width:80,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'TrackID',title:'跟踪ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'FID',title:'生产订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FENTRYID',title:'生产订单明细ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'ESBID',title:'ESB主键',type:'string',width:220,hidden:true,align:'left'}];
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