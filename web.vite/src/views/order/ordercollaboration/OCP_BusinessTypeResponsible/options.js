// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Id',
        footer: "Foots",
        cnName: '业务类型与默认负责人关系',
        name: 'OCP_BusinessTypeResponsible',
        newTabEdit: false,
        url: "/OCP_BusinessTypeResponsible/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"BusinessType":"","DefaultResponsibleName":"","DefaultResponsibleLoginName":""};
    const editFormOptions = [[{"dataKey":"业务类型","data":[],"title":"业务类型","field":"BusinessType","type":"select"}],
                              [{"title":"默认负责人姓名","field":"DefaultResponsibleName"}],
                              [{"title":"默认负责人登录名","field":"DefaultResponsibleLoginName"}]];
    const searchFormFields = {"BusinessType":[],"DefaultResponsibleName":"","DefaultResponsibleLoginName":""};
    const searchFormOptions = [[{"dataKey":"业务类型","data":[],"title":"业务类型","field":"BusinessType","type":"selectList"},{"title":"默认负责人姓名","field":"DefaultResponsibleName","type":"like"},{"title":"默认负责人登录名","field":"DefaultResponsibleLoginName","type":"like"}]];
    const columns = [{field:'Id',title:'主键ID',type:'long',width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'BusinessType',title:'业务类型',type:'string',bind:{ key:'业务类型',data:[]},sort:true,width:120,align:'left'},
                       {field:'DefaultResponsibleName',title:'默认负责人姓名',type:'string',sort:true,width:120,align:'left'},
                       {field:'DefaultResponsibleLoginName',title:'默认负责人登录名',type:'string',sort:true,width:120,align:'left'},
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