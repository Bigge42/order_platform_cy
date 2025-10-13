// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Id',
        footer: "Foots",
        cnName: '产能表',
        name: 'WZ_ProductionOutput',
        newTabEdit: false,
        url: "/WZ_ProductionOutput/",
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
                       {field:'ProductionDate',title:'ProductionDate',type:'datetime',width:110,require:true,align:'left'},
                       {field:'ValveCategory',title:'ValveCategory',type:'string',width:110,require:true,align:'left'},
                       {field:'ProductionLine',title:'ProductionLine',type:'string',width:110,require:true,align:'left'},
                       {field:'Quantity',title:'Quantity',type:'decimal',width:110,require:true,align:'left'}];
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