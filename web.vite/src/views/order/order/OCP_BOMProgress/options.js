// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'BOMRecordID',
        footer: "Foots",
        cnName: 'BOM搭建进度',
        name: 'OCP_BOMProgress',
        newTabEdit: false,
        url: "/OCP_BOMProgress/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"PlanID":"","MaterialCode":"","PriorityLevel":"","ActualBuildDate":"","OverdueDays":"","StatusFlag":""};
    const editFormOptions = [[{"title":"订单计划ID","required":true,"field":"PlanID","type":"number"}],
                              [{"title":"物料编码","required":true,"field":"MaterialCode"}],
                              [{"title":"紧急等级","field":"PriorityLevel"}],
                              [{"title":"实际搭建日期","field":"ActualBuildDate","type":"datetime"}],
                              [{"title":"超期天数","field":"OverdueDays","type":"number"}],
                              [{"title":"状态标识","field":"StatusFlag"}]];
    const searchFormFields = {"PlanID":"","MaterialCode":"","PriorityLevel":"","ActualBuildDate":"","OverdueDays":"","StatusFlag":""};
    const searchFormOptions = [[{"title":"BOM记录ID","field":"BOMRecordID","type":"like"}],[{"title":"订单计划ID","field":"PlanID","type":"like"}],[{"title":"物料编码","field":"MaterialCode","type":"like"}],[{"title":"紧急等级","field":"PriorityLevel","type":"like"}],[{"title":"实际搭建日期","field":"ActualBuildDate","type":"datetime"}],[{"title":"超期天数","field":"OverdueDays","type":"like"}],[{"title":"状态标识","field":"StatusFlag","type":"like"}]];
    const columns = [{field:'BOMRecordID',title:'BOM记录ID',type:'int',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'PlanID',title:'订单计划ID',type:'int',sort:true,width:80,require:true,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'PriorityLevel',title:'紧急等级',type:'string',sort:true,width:110,align:'left'},
                       {field:'ActualBuildDate',title:'实际搭建日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'OverdueDays',title:'超期天数',type:'int',sort:true,width:80,align:'left'},
                       {field:'StatusFlag',title:'状态标识',type:'string',sort:true,width:110,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',sort:true,width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',sort:true,width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',sort:true,width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',sort:true,width:80,hidden:true,align:'left'}];
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