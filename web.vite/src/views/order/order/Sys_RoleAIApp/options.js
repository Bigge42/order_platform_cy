// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Id',
        footer: "Foots",
        cnName: '角色AI应用授权',
        name: 'Sys_RoleAIApp',
        newTabEdit: false,
        url: "/Sys_RoleAIApp/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"Role_Id":"","AIAppId":"","Enable":""};
    const editFormOptions = [[{"dataKey":"roles","data":[],"title":"角色ID","required":true,"field":"Role_Id","type":"select","comparationList":[{"key":"select","value":"select"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"dataKey":"AI应用","data":[],"title":"AI应用ID","required":true,"field":"AIAppId","type":"select","comparationList":[{"key":"select","value":"select"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"dataKey":"enable","data":[],"title":"是否启用","required":true,"field":"Enable","type":"select","comparationList":[{"key":"select","value":"select"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}]];
    const searchFormFields = {};
    const searchFormOptions = [[{"dataKey":"roles","data":[],"title":"角色ID","field":"Role_Id","type":"selectList","comparationList":[{"key":"selectList","value":"select多选"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"dataKey":"AI应用","data":[],"title":"AI应用ID","field":"AIAppId","type":"selectList","comparationList":[{"key":"selectList","value":"select多选"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"dataKey":"enable","data":[],"title":"是否启用","field":"Enable","type":"selectList","comparationList":[{"key":"selectList","value":"select多选"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}]];
    const columns = [{field:'Id',title:'主键ID',type:'long',width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'Role_Id',title:'角色ID',type:'int',bind:{ key:'roles',data:[]},sort:true,width:80,require:true,align:'left'},
                       {field:'AIAppId',title:'AI应用ID',type:'long',bind:{ key:'AI应用',data:[]},sort:true,width:80,require:true,align:'left'},
                       {field:'Enable',title:'是否启用',type:'int',bind:{ key:'enable',data:[]},sort:true,width:80,require:true,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:150,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'}];
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