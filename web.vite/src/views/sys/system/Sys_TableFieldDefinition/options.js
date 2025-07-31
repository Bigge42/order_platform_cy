// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Id',
        footer: "Foots",
        cnName: '数据库表字段定义',
        name: 'Sys_TableFieldDefinition',
        newTabEdit: false,
        url: "/Sys_TableFieldDefinition/",
        sortName: "Id"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"TableName":"","TableCName":"","FieldName":"","FieldCName":"","DataType":"","IsNullable":"","IsKey":"","DefaultValue":"","Bak":""};
    const editFormOptions = [[{"title":"表名","required":true,"field":"TableName"},
                               {"title":"表中文名","required":true,"field":"TableCName"},
                               {"title":"字段名","required":true,"field":"FieldName"}],
                              [{"dataKey":"enable","data":[],"title":"允许为空","field":"IsNullable","type":"select"},
                               {"title":"字段中文名","required":true,"field":"FieldCName"},
                               {"title":"数据类型","required":true,"field":"DataType"}],
                              [{"dataKey":"enable","data":[],"title":"主键","field":"IsKey","type":"select"},
                               {"title":"默认值","field":"DefaultValue"},
                               {"title":"备注","field":"Bak"}]];
    const searchFormFields = {"TableName":"","TableCName":"","FieldName":"","FieldCName":""};
    const searchFormOptions = [[{"title":"表名","field":"TableName","type":"like"},{"title":"表中文名","field":"TableCName","type":"like"},{"title":"字段名","field":"FieldName","type":"like"},{"title":"字段中文名","field":"FieldCName","type":"like"}]];
    const columns = [{field:'Id',title:'ID',type:'int',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'TableName',title:'表名',type:'string',link:true,sort:true,width:120,require:true,align:'left'},
                       {field:'TableCName',title:'表中文名',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'FieldName',title:'字段名',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'FieldCName',title:'字段中文名',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'DataType',title:'数据类型',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'IsNullable',title:'允许为空',type:'int',bind:{ key:'enable',data:[]},sort:true,width:110,align:'left'},
                       {field:'IsKey',title:'主键',type:'int',bind:{ key:'enable',data:[]},sort:true,width:110,align:'left'},
                       {field:'DefaultValue',title:'默认值',type:'string',sort:true,width:180,align:'left'},
                       {field:'Bak',title:'备注',type:'string',sort:true,width:180,align:'left'},
                       {field:'CreateID',title:'创建人编号',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',sort:true,width:110,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',sort:true,width:110,align:'left'},
                       {field:'ModifyID',title:'修改人编号',type:'int',width:80,hidden:true,align:'left'}];
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