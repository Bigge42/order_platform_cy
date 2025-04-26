// *Author：jxx
// *Contact：283591387@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Customer_Id',
        footer: "Foots",
        cnName: '客户管理',
        name: 'Demo_Customer',
        newTabEdit: false,
        url: "/Demo_Customer/",
        sortName: "Customer_Id"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"Customer":"","PhoneNo":"","Remark":"","Province":"","City":"","County":"","DetailAddress":""};
    const editFormOptions = [[{"title":"客户","required":true,"field":"Customer"},
                               {"title":"手机","required":true,"field":"PhoneNo","type":"phone"},
                               {"title":"备注","field":"Remark"}],
                              [{"dataKey":"省","data":[],"title":"省","required":true,"field":"Province","type":"select"},
                               {"dataKey":"市","data":[],"title":"市","required":true,"field":"City","type":"select"},
                               {"dataKey":"县","data":[],"title":"县","field":"County","type":"select"}],
                              [{"title":"详细地址","field":"DetailAddress","colSize":12}]];
    const searchFormFields = {"Customer":"","PhoneNo":"","Province":"","City":"","Creator":"","CreateDate":"","ModifyDate":""};
    const searchFormOptions = [[{"title":"客户","field":"Customer","type":"like"},{"title":"手机","field":"PhoneNo"},{"dataKey":"省","data":[],"title":"省","field":"Province"},{"dataKey":"市","data":[],"title":"市","field":"City"}],[{"title":"创建人","field":"Creator"},{"title":"创建时间","field":"CreateDate","type":"datetime"},{"title":"修改时间","field":"ModifyDate","type":"datetime"}]];
    const columns = [{field:'Customer_Id',title:'Customer_Id',type:'int',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'Customer',title:'客户',type:'string',link:true,width:100,require:true,align:'left'},
                       {field:'PhoneNo',title:'手机',type:'string',width:130,require:true,align:'left'},
                       {field:'Province',title:'省',type:'string',bind:{ key:'省',data:[]},width:80,require:true,align:'left'},
                       {field:'City',title:'市',type:'string',bind:{ key:'市',data:[]},width:80,require:true,align:'left'},
                       {field:'County',title:'县',type:'string',bind:{ key:'县',data:[]},width:120,align:'left'},
                       {field:'DetailAddress',title:'详细地址',type:'string',width:120,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:100,align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:145,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:145,hidden:true,align:'left'}];
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