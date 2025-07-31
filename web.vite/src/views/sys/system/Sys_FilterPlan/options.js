// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ID',
        footer: "Foots",
        cnName: '单据过滤方案',
        name: 'Sys_FilterPlan',
        newTabEdit: false,
        url: "/Sys_FilterPlan/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"BillName":"","Name":"","Content":"","UserIds":"","RoleIds":"","IsDefault":"","IsSystem":""};
    const editFormOptions = [[{"title":"单据名称","field":"BillName"},
                               {"title":"方案名称","field":"Name"}],
                              [{"title":"方案内容","field":"Content","type":"textarea"}],
                              [{"title":"用户","field":"UserIds"},
                               {"title":"角色","field":"RoleIds"},
                               {"dataKey":"enable","data":[],"title":"是否默认","field":"IsDefault","type":"select"}],
                              [{"dataKey":"enable","data":[],"title":"系统内置","field":"IsSystem","type":"select"}]];
    const searchFormFields = {"BillName":"","Name":"","Content":"","CreateDate":""};
    const searchFormOptions = [[{"title":"单据名称","field":"BillName","type":"like"},{"title":"方案名称","field":"Name","type":"like"}],[{"title":"方案内容","field":"Content"},{"title":"创建时间","field":"CreateDate","type":"datetime"}]];
    const columns = [{field:'ID',title:'主建',type:'long',width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'BillName',title:'单据名称',type:'string',width:120,align:'left'},
                       {field:'Name',title:'方案名称',type:'string',width:120,align:'left'},
                       {field:'Content',title:'方案内容',type:'string',width:110,align:'left'},
                       {field:'UserIds',title:'用户',type:'string',width:220,align:'left'},
                       {field:'RoleIds',title:'角色',type:'string',width:220,align:'left'},
                       {field:'IsDefault',title:'是否默认',type:'int',bind:{ key:'enable',data:[]},width:80,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'IsSystem',title:'系统内置',type:'int',bind:{ key:'enable',data:[]},width:80,align:'left'}];
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