// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'WorkOrderNo',
        footer: "Foots",
        cnName: '附件库房发料看板',
        name: 'MaterialCallBoard',
        newTabEdit: false,
        url: "/MaterialCallBoard/",
        sortName: "PlanTrackNo"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {};
    const editFormOptions = [];
    const searchFormFields = {"PlanTrackNo":"","ProductCode":"","CallerName":"","CalledAt":""};
    const searchFormOptions = [[{"title":"计划跟踪号","field":"PlanTrackNo"},{"title":"产品编号","field":"ProductCode"},{"title":"叫料人","field":"CallerName"},{"title":"叫料时间","field":"CalledAt"}]];
    const columns = [{field:'WorkOrderNo',title:'WorkOrderNo',type:'string',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'PlanTrackNo',title:'计划跟踪号',type:'string',sort:true,width:110,readonly:true,require:true,align:'left'},
                       {field:'ProductCode',title:'产品编号',type:'string',sort:true,width:110,readonly:true,require:true,align:'left'},
                       {field:'CallerName',title:'叫料人',type:'string',sort:true,width:120,readonly:true,require:true,align:'left'},
                       {field:'CalledAt',title:'叫料时间',type:'datetime',sort:true,width:110,readonly:true,require:true,align:'left'}];
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