// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Id',
        footer: "Foots",
        cnName: '排产智能体优化看板',
        name: 'WZ_OrderCycleBase',
        newTabEdit: false,
        url: "/WZ_OrderCycleBase/",
        sortName: "Id"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {};
    const editFormOptions = [];
    const searchFormFields = {};
    const searchFormOptions = [];
    const columns = [{field:'Id',title:'Id',type:'int',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'SalesOrderNo',title:'销售订单号',type:'string',width:120,align:'left'},
                       {field:'PlanTrackingNo',title:'计划跟踪号',type:'string',width:120,align:'left'},
                       {field:'OrderApprovedDate',title:'订单审核日期',type:'datetime',width:110,align:'left'},
                       {field:'ReplyDeliveryDate',title:'回复交货日期',type:'datetime',width:110,align:'left'},
                       {field:'RequestedDeliveryDate',title:'要货日期',type:'datetime',width:110,align:'left'},
                       {field:'StandardDeliveryDate',title:'标准交货日期',type:'datetime',width:110,align:'left'},
                       {field:'ScheduleDate',title:'排产日期',type:'datetime',width:110,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',width:120,align:'left'},
                       {field:'BodyMaterial',title:'阀体材质',type:'string',width:150,align:'left'},
                       {field:'InnerMaterial',title:'内件材质',type:'string',width:150,align:'left'},
                       {field:'FlangeConnection',title:'法兰连接方式',type:'string',width:150,align:'left'},
                       {field:'BonnetForm',title:'上盖形式',type:'string',width:150,align:'left'},
                       {field:'FlowCharacteristic',title:'流量特性',type:'string',width:150,align:'left'},
                       {field:'Actuator',title:'执行机构',type:'string',width:150,align:'left'},
                       {field:'OutsourcedValveBody',title:'外购阀体',type:'string',width:150,align:'left'},
                       {field:'ValveCategory',title:'阀门类别',type:'string',width:150,align:'left'},
                       {field:'SealFaceForm',title:'密封面形式',type:'string',width:150,align:'left'},
                       {field:'SpecialProduct',title:'特品',type:'string',width:180,align:'left'},
                       {field:'PurchaseFlag',title:'外购标志',type:'string',width:110,align:'left'},
                       {field:'ProductName',title:'产品名称',type:'string',width:180,align:'left'},
                       {field:'NominalDiameter',title:'公称通径',type:'string',width:110,align:'left'},
                       {field:'NominalPressure',title:'公称压力',type:'string',width:110,align:'left'},
                       {field:'FixedCycleDays',title:'固定周期(天)',type:'int',width:110,align:'left'},
                       {field:'ProductionLine',title:'生产线',type:'string',width:110,align:'left'}];
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