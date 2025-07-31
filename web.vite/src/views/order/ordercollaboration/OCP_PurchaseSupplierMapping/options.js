// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Id',
        footer: "Foots",
        cnName: '采购负责人与供应商映射关系',
        name: 'OCP_PurchaseSupplierMapping',
        newTabEdit: false,
        url: "/OCP_PurchaseSupplierMapping/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"Code":"","Name":"","SupplierGroup":"","BusinessPersonName":"","BusinessPersonLoginName":""};
    const editFormOptions = [[{"title":"编码","field":"Code"}],
                              [{"title":"名称","field":"Name"}],
                              [{"title":"供应商分组","field":"SupplierGroup"}],
                              [{"title":"对应业务员姓名","field":"BusinessPersonName"}],
                              [{"title":"对应业务员登录名","field":"BusinessPersonLoginName"}]];
    const searchFormFields = {"Code":"","Name":"","SupplierGroup":"","BusinessPersonName":"","BusinessPersonLoginName":""};
    const searchFormOptions = [[{"title":"编码","field":"Code","type":"like"},{"title":"名称","field":"Name","type":"like"},{"title":"供应商分组","field":"SupplierGroup","type":"like"}],[{"title":"对应业务员姓名","field":"BusinessPersonName","type":"like"},{"title":"对应业务员登录名","field":"BusinessPersonLoginName","type":"like"}]];
    const columns = [{field:'Id',title:'主键ID',type:'long',width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'Code',title:'编码',type:'string',sort:true,width:110,align:'left'},
                       {field:'Name',title:'名称',type:'string',sort:true,width:110,align:'left'},
                       {field:'SupplierGroup',title:'供应商分组',type:'string',sort:true,width:120,align:'left'},
                       {field:'BusinessPersonName',title:'对应业务员姓名',type:'string',sort:true,width:120,align:'left'},
                       {field:'BusinessPersonLoginName',title:'对应业务员登录名',type:'string',sort:true,width:120,align:'left'},
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