// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'TrackID',
        footer: "Foots",
        cnName: '部件跟踪表',
        name: 'OCP_PartUnFinishTracking',
        newTabEdit: false,
        url: "/OCP_PartUnFinishTracking/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"ProductionOrderNo":"","MOBillNo":"","FENTRYID":"","MaterialCode":"","MaterialName":"","MaterialID":"","PlanTaskMonth":"","PlanTaskWeek":"","Urgency":"","MaterialPickDate":"","PreCompleteDate":"","ValvePartCompleteDate":"","PressureCompleteDate":"","InspectionDate":"","ProductionQty":"","InboundQty":"","ReplyDate":""};
    const editFormOptions = [[{"title":"生产订单明细ID","field":"FENTRYID"},
                               {"title":"生产单据号","field":"ProductionOrderNo"},
                               {"title":"工单编号","field":"MOBillNo"}],
                              [{"title":"物料ID","field":"MaterialID"},
                               {"title":"物料编码","field":"MaterialCode"},
                               {"title":"物料名称","field":"MaterialName"}],
                              [{"title":"月度任务","field":"PlanTaskMonth"},
                               {"title":"周任务","field":"PlanTaskWeek"},
                               {"title":"紧急等级","field":"Urgency"}],
                              [{"title":"领料日期","field":"MaterialPickDate","type":"datetime"},
                               {"title":"预装完工日期","field":"PreCompleteDate","type":"datetime"},
                               {"title":"部件装配日期","field":"ValvePartCompleteDate","type":"datetime"}],
                              [{"title":"强压泄漏日期","field":"PressureCompleteDate","type":"datetime"},
                               {"title":"检验日期","field":"InspectionDate","type":"datetime"},
                               {"title":"生产数量","field":"ProductionQty","type":"decimal"}],
                              [{"title":"入库数量","field":"InboundQty","type":"decimal"},
                               {"title":"回复日期","field":"ReplyDate","type":"datetime"}]];
    const searchFormFields = {"Urgency":"","MaterialName":"","MESBatchNo":"","MaterialPickDate":""};
    const searchFormOptions = [[{"title":"计划跟踪号","field":"PlanTraceNo","type":"like"},{"title":"生产单据号","field":"ProductionOrderNo","type":"like"},{"title":"工单编号","field":"MOBillNo","type":"like"},{"title":"物料编码","field":"MaterialCode","type":"like"}],[{"title":"物料名称","field":"MaterialName","type":"like"},{"title":"紧急等级","field":"Urgency","type":"like"},{"title":"MES批次号","field":"MESBatchNo"},{"title":"领料日期","field":"MaterialPickDate","type":"datetime"}]];
    const columns = [{field:'PlanTraceNo',title:'计划跟踪号',type:'string',sort:true,width:120,align:'left'},
                       {field:'JoinTaskBillNo',title:'总任务编号',type:'string',width:120,align:'left'},
                       {field:'ProScheduleYearMonth',title:'排产月份',type:'string',width:120,align:'left'},
                       {field:'PlanTaskMonth',title:'月度任务',type:'string',sort:true,width:120,align:'left'},
                       {field:'PlanTaskWeek',title:'周任务',type:'string',sort:true,width:120,align:'left'},
                       {field:'Urgency',title:'紧急等级',type:'string',sort:true,width:120,align:'left'},
                       {field:'ProductionOrderNo',title:'生产单据号',type:'string',sort:true,width:120,align:'left'},
                       {field:'BillStatus',title:'生产订单状态',type:'string',width:120,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',sort:true,width:120,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:220,align:'left'},
                       {field:'Specification',title:'规格型号',type:'string',width:220,align:'left'},
                       {field:'MOBillNo',title:'工单编号',type:'string',sort:true,width:120,align:'left'},
                       {field:'MESBatchNo',title:'MES批次号',type:'string',width:120,align:'left'},
                       {field:'ProductModel',title:'产品型号',type:'string',width:160,align:'left'},
                       {field:'NominalDiameter',title:'公称通径',type:'string',width:120,align:'left'},
                       {field:'NominalPressure',title:'公称压力',type:'string',width:120,align:'left'},
                       {field:'FlowCharacteristic',title:'流量特性',type:'string',width:120,align:'left'},
                       {field:'PackingForm',title:'填料形式',type:'string',width:120,align:'left'},
                       {field:'FlangeConnection',title:'法兰连接方式',type:'string',width:120,align:'left'},
                       {field:'ActuatorModel',title:'执行机构型号',type:'string',width:120,align:'left'},
                       {field:'ActuatorStroke',title:'执行机构行程',type:'string',width:120,align:'left'},
                       {field:'ErpClsid',title:'物料属性',type:'string',width:120,align:'left'},
                       {field:'ExecuteStatus',title:'执行状态',type:'string',width:120,align:'left'},
                       {field:'MaterialPickDate',title:'领料日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'PreCompleteDate',title:'预装完工日期',type:'date',sort:true,width:180,align:'left'},
                       {field:'ValvePartCompleteDate',title:'部件装配日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'PressureCompleteDate',title:'强压泄漏日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'InspectionDate',title:'检验日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'ProductionQty',title:'生产数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'InboundQty',title:'入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'ReplyDate',title:'回复日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'OverdueDays',title:'超期天数',type:'int',width:80,align:'left'},
                       {field:'TrackID',title:'跟踪ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'FID',title:'生产订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FENTRYID',title:'生产订单明细ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'ESBID',title:'ESB主键',type:'string',width:220,hidden:true,align:'left'},
                       {field:'PlanCompleteDate',title:'计划完工日期',type:'datetime',width:150,align:'left'},
                       {field:'CompleteDate',title:'实际完工日期',type:'datetime',width:150,align:'left'}];
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