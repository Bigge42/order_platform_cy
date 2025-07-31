// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ComputeID',
        footer: "Foots",
        cnName: '缺料运算方案',
        name: 'OCP_LackMtrlPlan',
        newTabEdit: false,
        url: "/OCP_LackMtrlPlan/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"PlanName":"","Filter":"","IsDefault":""};
    const editFormOptions = [[{"title":"运算方案名","field":"PlanName"}],
                              [{"title":"运算条件","field":"Filter"}],
                              [{"dataKey":"enable","data":[],"title":"是否默认方案","field":"IsDefault","type":"select"}]];
    const searchFormFields = {"PlanName":"","Filter":"","CreateDate":"","Creator":"","IsDefault":[]};
    const searchFormOptions = [[{"title":"运算方案名","field":"PlanName","type":"like"},{"title":"运算条件","field":"Filter","type":"like"},{"title":"运算时间","field":"CreateDate","type":"datetime"}],[{"title":"运算人","field":"Creator","type":"like"},{"dataKey":"enable","data":[],"title":"是否默认方案","field":"IsDefault","type":"selectList"}]];
    const columns = [{field:'ComputeID',title:'运算ID',type:'long',sort:true,width:80,readonly:true,require:true,align:'left'},
                       {field:'PlanName',title:'运算方案名',type:'string',sort:true,width:120,align:'left'},
                       {field:'Filter',title:'运算条件',type:'string',sort:true,width:110,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',sort:true,width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'运算时间',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'Creator',title:'运算人',type:'string',sort:true,width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',sort:true,width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',sort:true,width:80,hidden:true,align:'left'},
                       {field:'IsDefault',title:'是否默认方案',type:'int',bind:{ key:'enable',data:[]},sort:true,width:80,align:'left'}];
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