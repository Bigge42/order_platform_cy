// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'PlanTraceNo',
        footer: "Foots",
        cnName: '当月完成BOM情况',
        name: 'vw_OCP_Tech_BOM_Status_Monthly',
        newTabEdit: false,
        url: "/vw_OCP_Tech_BOM_Status_Monthly/",
        sortName: "订单日期"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {};
    const editFormOptions = [];
    const searchFormFields = {"PlanTraceNo":"","MaterialCode":"","ProductModel":"","TCBomCreator":""};
    const searchFormOptions = [[{"title":"计划号","field":"PlanTraceNo"},{"title":"物料编码","field":"MaterialCode"},{"title":"产品型号","field":"ProductModel"},{"title":"BOM创建人","field":"TCBomCreator"}]];
    const columns = [{field:'PlanTraceNo',title:'计划号',type:'string',sort:true,width:220,readonly:true,require:true,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',sort:true,width:220,readonly:true,align:'left'},
                       {field:'ProductModel',title:'产品型号',type:'string',sort:true,width:220,readonly:true,align:'left'},
                       {field:'OrderNo',title:'订单编号',type:'string',sort:true,width:120,readonly:true,align:'left'},
                       {field:'OrderDate',title:'订单日期',type:'datetime',sort:true,width:150,readonly:true,align:'left'},
                       {field:'OrderAuditDate',title:'订单审核日期',type:'datetime',sort:true,width:150,readonly:true,align:'left'},
                       {field:'BomCreateDate',title:'BOM创建日期',type:'datetime',sort:true,width:150,readonly:true,align:'left'},
                       {field:'BomAgeDays',title:'BOM存在时间(天)',type:'int',sort:true,width:160,hidden:true,readonly:true,align:'left'},
                       {field:'BomDelayDays',title:'BOM创建延迟天数',type:'int',sort:true,width:160,readonly:true,align:'left'},
                       {field:'BomMissingDays',title:'BOM已缺失天数',type:'int',sort:true,width:160,readonly:true,align:'left'},
                       {field:'TCBomCreator',title:'BOM创建人',type:'string',width:110,readonly:true,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',width:220,readonly:true,align:'left'},
                       {field:'CV',title:'CV',type:'string',width:220,readonly:true,align:'left'},
                       {field:'NominalDiameter',title:'公称通径',type:'string',width:220,readonly:true,align:'left'},
                       {field:'NominalPressure',title:'公称压力',type:'string',width:220,readonly:true,align:'left'},
                       {field:'FlangeStandard',title:'法兰标准',type:'string',width:110,hidden:true,readonly:true,align:'left'},
                       {field:'SealFaceForm',title:'密封面形式',type:'string',width:110,hidden:true,readonly:true,align:'left'},
                       {field:'BodyMaterial',title:'阀体材质',type:'string',width:110,hidden:true,readonly:true,align:'left'},
                       {field:'TrimMaterial',title:'阀内件材质',type:'string',width:110,hidden:true,readonly:true,align:'left'},
                       {field:'FlowCharacteristic',title:'流量特性',type:'string',width:220,readonly:true,align:'left'},
                       {field:'PackingForm',title:'填料形式',type:'string',width:220,readonly:true,align:'left'},
                       {field:'FlangeConnection',title:'法兰连接方式',type:'string',width:220,readonly:true,align:'left'},
                       {field:'ActuatorModel',title:'执行机构型号',type:'string',width:220,readonly:true,align:'left'},
                       {field:'ActuatorStroke',title:'执行机构行程',type:'string',width:220,readonly:true,align:'left'},
                       {field:'Remarks',title:'备注',type:'string',width:220,readonly:true,align:'left'},
                       {field:'Qty',title:'数量',type:'decimal',sort:true,width:110,readonly:true,align:'left'},
                       {field:'ReplyDeliveryDate',title:'回复交货日期',type:'datetime',sort:true,width:150,readonly:true,align:'left'}];
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