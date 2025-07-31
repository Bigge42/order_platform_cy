// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'QueryRecordID',
        footer: "Foots",
        cnName: '缺料运算查询',
        name: 'OCP_MtrlShortageQuery',
        newTabEdit: false,
        url: "/OCP_MtrlShortageQuery/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"OrderNumber":"","PlanTrackingNumber":"","DocumentCode":"","LineNumber":"","MaterialCode":"","MaterialName":"","MaterialProperty":"","QuantityUnit":"","PlannedDeliveryDate":"","EstimatedCompletionDate":"","UrgeDate":""};
    const editFormOptions = [[{"title":"订单号","required":true,"field":"OrderNumber"},
                               {"title":"计划跟踪号","required":true,"field":"PlanTrackingNumber"},
                               {"title":"单据编码","required":true,"field":"DocumentCode"}],
                              [{"title":"行号","required":true,"field":"LineNumber","type":"number"},
                               {"title":"物料编码","required":true,"field":"MaterialCode"},
                               {"title":"物料名称","required":true,"field":"MaterialName"}],
                              [{"title":"物料属性","field":"MaterialProperty"},
                               {"title":"数量单位","required":true,"field":"QuantityUnit"},
                               {"title":"计划交货日期","required":true,"field":"PlannedDeliveryDate","type":"datetime"}],
                              [{"title":"预计完工日期","field":"EstimatedCompletionDate","type":"datetime"},
                               {"title":"催货日期","field":"UrgeDate","type":"datetime"}]];
    const searchFormFields = {"PlanTrackingNumber":"","DocumentCode":"","LineNumber":"","MaterialCode":"","MaterialName":"","MaterialProperty":"","QuantityUnit":"","PlannedDeliveryDate":"","EstimatedCompletionDate":"","UrgeDate":""};
    const searchFormOptions = [[{"title":"查询记录ID","field":"QueryRecordID","type":"like"},{"title":"运算ID","field":"OperationID","type":"like"},{"title":"合同号","field":"ContractNumber","type":"like"},{"title":"订单号","field":"OrderNumber","type":"multipleInput"}],[{"title":"计划跟踪号","field":"PlanTrackingNumber","type":"multipleInput"},{"title":"单据编码","field":"DocumentCode","type":"multipleInput"},{"title":"行号","field":"LineNumber","type":"like"},{"title":"物料编码","field":"MaterialCode","type":"like"}],[{"title":"物料名称","field":"MaterialName","type":"like"},{"title":"物料属性","field":"MaterialProperty","type":"like"},{"title":"数量单位","field":"QuantityUnit","type":"like"},{"title":"计划交货日期","field":"PlannedDeliveryDate","type":"datetime"}],[{"title":"预计完工日期","field":"EstimatedCompletionDate","type":"datetime"},{"title":"催货日期","field":"UrgeDate","type":"datetime"}]];
    const columns = [{field:'QueryRecordID',title:'查询记录ID',type:'int',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'OperationID',title:'运算ID',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'ContractNumber',title:'合同号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'OrderNumber',title:'订单号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'PlanTrackingNumber',title:'计划跟踪号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'DocumentCode',title:'单据编码',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'LineNumber',title:'行号',type:'int',sort:true,width:80,require:true,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:110,require:true,align:'left'},
                       {field:'MaterialProperty',title:'物料属性',type:'string',sort:true,width:120,align:'left'},
                       {field:'QuantityUnit',title:'数量单位',type:'string',sort:true,width:110,require:true,align:'left'},
                       {field:'PlannedDeliveryDate',title:'计划交货日期',type:'datetime',sort:true,width:150,require:true,align:'left'},
                       {field:'EstimatedCompletionDate',title:'预计完工日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'UrgeDate',title:'催货日期',type:'datetime',sort:true,width:150,align:'left'},
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