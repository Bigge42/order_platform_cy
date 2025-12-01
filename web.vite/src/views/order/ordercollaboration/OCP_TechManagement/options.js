// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'TechID',
        footer: "Foots",
        cnName: '技术管理表',
        name: 'OCP_TechManagement',
        newTabEdit: false,
        url: "/OCP_TechManagement/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"PlanTraceNo":"","MaterialNumber":"","MaterialID":"","ProductModel":"","NominalDiameter":"","NominalPressure":"","PlanTaskMonth":"","SalesQty":"","OrderStatus":"","PlanTaskWeek":"","Urgency":"","BOMCreateDate":"","RequiredCompletionDate":"","OverdueDays":"","StandardDays":"","HasBOM":"","RequiredFinishTime":""};
    const editFormOptions = [[{"title":"计划跟踪号","field":"PlanTraceNo"},
                               {"title":"物料ID","field":"MaterialID"},
                               {"title":"物料编码","field":"MaterialNumber"}],
                              [{"title":"产品型号","field":"ProductModel"},
                               {"title":"公称通径","field":"NominalDiameter"},
                               {"title":"公称压力","field":"NominalPressure"}],
                              [{"title":"订单状态","field":"OrderStatus"},
                               {"title":"销售数量","field":"SalesQty","type":"decimal","comparationList":[{"key":"decimal","value":"decimal"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"月度任务","field":"PlanTaskMonth"}],
                              [{"title":"周任务","field":"PlanTaskWeek"},
                               {"title":"紧急等级","field":"Urgency"},
                               {"title":"BOM创建日期","field":"BOMCreateDate","type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"title":"标准天数","field":"StandardDays","type":"number","comparationList":[{"key":"number","value":"number"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"要求完成日期","field":"RequiredCompletionDate","type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"超期天数","field":"OverdueDays","type":"number","comparationList":[{"key":"number","value":"number"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"dataKey":"enable","data":[],"title":"是否有BOM","field":"HasBOM","type":"select","comparationList":[{"key":"select","value":"select"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"要求完工时间","field":"RequiredFinishTime","type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}]];
    const searchFormFields = {"Urgency":"","MaterialName":"","ProductModel":"","OrderStatus":"","HasBOM":[],"IsJoinTask":[]};
    const searchFormOptions = [[{"title":"销售合同号","field":"SalesContractNo","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"销售订单号","field":"SOBillNo","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"计划跟踪号","field":"PlanTraceNo","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"物料编码","field":"MaterialNumber","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}],[{"title":"物料名称","field":"MaterialName","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"产品型号","field":"ProductModel","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"紧急等级","field":"Urgency","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}],[{"title":"订单状态","field":"OrderStatus","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"dataKey":"enable","data":[],"title":"是否有BOM","field":"HasBOM","type":"selectList","comparationList":[{"key":"selectList","value":"select多选"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"dataKey":"enable","data":[],"title":"是否关联总任务","field":"IsJoinTask","type":"selectList","comparationList":[{"key":"selectList","value":"select多选"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}]];
    const columns = [{field:'SalesContractNo',title:'销售合同号',type:'string',sort:true,width:120,align:'left'},
                       {field:'SOBillNo',title:'销售订单号',type:'string',sort:true,width:120,align:'left'},
                       {field:'PlanTraceNo',title:'计划跟踪号',type:'string',sort:true,width:120,align:'left'},
                       {field:'ProScheduleYearMonth',title:'排产月份',type:'string',sort:true,width:120,align:'left'},
                       {field:'PlanTaskMonth',title:'月度任务',type:'string',sort:true,width:120,align:'left'},
                       {field:'PlanTaskWeek',title:'周任务',type:'string',sort:true,width:120,align:'left'},
                       {field:'Urgency',title:'紧急等级',type:'string',sort:true,width:120,align:'left'},
                       {field:'DeliveryDate',title:'客户要货日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'ReplyDeliveryDate',title:'回复交货日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'BOMCreateDate',title:'BOM创建日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'RequiredCompletionDate',title:'要求完成日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:120,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:120,align:'left'},
                       {field:'ProductModel',title:'产品型号',type:'string',sort:true,width:120,align:'left'},
                       {field:'NominalDiameter',title:'公称通径',type:'string',sort:true,width:120,align:'left'},
                       {field:'NominalPressure',title:'公称压力',type:'string',sort:true,width:120,align:'left'},
                       {field:'FlowCharacteristic',title:'流量特性',type:'string',width:120,align:'left'},
                       {field:'PackingForm',title:'填料形式',type:'string',width:120,align:'left'},
                       {field:'FlangeConnection',title:'法兰连接方式',type:'string',width:120,align:'left'},
                       {field:'ActuatorModel',title:'执行机构型号',type:'string',width:120,align:'left'},
                       {field:'ActuatorStroke',title:'执行机构行程',type:'string',width:220,align:'left'},
                       {field:'SalesQty',title:'销售数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueDays',title:'超期天数',type:'int',sort:true,width:120,align:'left'},
                       {field:'StandardDays',title:'标准天数',type:'int',sort:true,width:120,align:'left'},
                       {field:'IsSpecialContract',title:'是否特殊合同',type:'string',width:120,align:'left',formatter:(row)=>{
                           const standardDays = Number(row.StandardDays);
                           if (Number.isNaN(standardDays)) {
                               return '';
                           }
                           return standardDays > 3 ? '是' : '否';
                       }},
                       {field:'Designer',title:'设计人',type:'string',width:120,align:'left'},
                       {field:'Remarks',title:'备注',type:'string',width:220,align:'left'},
                       {field:'TechID',title:'技术ID',type:'long',width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',width:80,hidden:true,align:'left'},
                       {field:'OrderStatus',title:'订单状态',type:'string',sort:true,width:220,hidden:true,align:'left'},
                       {field:'HasBOM',title:'是否有BOM',type:'int',bind:{ key:'enable',data:[]},sort:true,width:110,align:'left'},
                       {field:'RequiredFinishTime',title:'要求完工时间',type:'date',sort:true,width:150,hidden:true,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'SOEntryID',title:'销售订单明细ID',type:'long',width:80,hidden:true,align:'left'},
                       {field:'JoinTaskBillNo',title:'关联总任务单据号',type:'string',sort:true,width:120,hidden:true,align:'left'},
                       {field:'IsJoinTask',title:'是否关联总任务',type:'int',bind:{ key:'enable',data:[]},sort:true,width:80,hidden:true,align:'left'},
                       {field:'ErpClsid',title:'物料属性',type:'string',sort:true,width:120,hidden:true,align:'left'},
                       {field:'ESBModifyDate',title:'ESB修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'FPSYJ',title:'评审意见',type:'string',width:220,align:'left'}];
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