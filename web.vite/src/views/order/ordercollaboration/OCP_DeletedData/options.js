// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ID',
        footer: "Foots",
        cnName: '已删除数据表',
        name: 'OCP_DeletedData',
        newTabEdit: false,
        url: "/OCP_DeletedData/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"FID":"","FENTRYID":"","BusinessType":""};
    const editFormOptions = [[{"title":"业务主键ID","required":true,"field":"FID"}],
                              [{"title":"业务明细ID","field":"FENTRYID"}],
                              [{"title":"业务类型","field":"BusinessType"}]];
    const searchFormFields = {"FID":"","FENTRYID":"","BusinessType":""};
    const searchFormOptions = [[{"title":"业务主键ID","field":"FID"},{"title":"业务明细ID","field":"FENTRYID"},{"title":"业务类型","field":"BusinessType","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}]];
    const columns = [{field:'ID',title:'主键ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'FID',title:'业务主键ID',type:'long',sort:true,width:80,require:true,align:'left'},
                       {field:'FENTRYID',title:'业务明细ID',type:'long',sort:true,width:80,align:'left'},
                       {field:'BusinessType',title:'业务类型',type:'string',sort:true,width:120,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
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