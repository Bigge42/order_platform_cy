// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ID',
        footer: "Foots",
        cnName: '客户表',
        name: 'OCP_Customer',
        newTabEdit: false,
        url: "/OCP_Customer/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"CustomerID":"","Number":"","Name":"","ContactPhone":"","Address":"","TaxRegistrationNo":"","Salesperson":""};
    const editFormOptions = [[{"title":"客户ID","required":true,"field":"CustomerID"}],
                              [{"title":"客户编码","field":"Number"}],
                              [{"title":"客户名称","field":"Name"}],
                              [{"title":"联系电话","field":"ContactPhone"}],
                              [{"title":"通讯地址","field":"Address"}],
                              [{"title":"纳税登记号","field":"TaxRegistrationNo"}],
                              [{"title":"销售员","field":"Salesperson"}]];
    const searchFormFields = {"ContactPhone":"","Address":"","TaxRegistrationNo":"","Salesperson":""};
    const searchFormOptions = [[{"title":"主键ID","field":"ID","type":"like"},{"title":"客户ID","field":"CustomerID","type":"like"},{"title":"客户编码","field":"Number","type":"like"},{"title":"客户名称","field":"Name","type":"like"}],[{"title":"联系电话","field":"ContactPhone","type":"like"},{"title":"通讯地址","field":"Address","type":"like"},{"title":"纳税登记号","field":"TaxRegistrationNo","type":"like"},{"title":"销售员","field":"Salesperson","type":"like"}]];
    const columns = [{field:'ID',title:'主键ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'CustomerID',title:'客户ID',type:'long',sort:true,width:80,require:true,align:'left'},
                       {field:'Number',title:'客户编码',type:'string',sort:true,width:220,align:'left'},
                       {field:'Name',title:'客户名称',type:'string',sort:true,width:220,align:'left'},
                       {field:'ContactPhone',title:'联系电话',type:'string',sort:true,width:120,align:'left'},
                       {field:'Address',title:'通讯地址',type:'string',sort:true,width:220,align:'left'},
                       {field:'TaxRegistrationNo',title:'纳税登记号',type:'string',sort:true,width:120,align:'left'},
                       {field:'Salesperson',title:'销售员',type:'string',sort:true,width:120,align:'left'},
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