// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ID',
        footer: "Foots",
        cnName: '供应商表',
        name: 'OCP_Supplier',
        newTabEdit: false,
        url: "/OCP_Supplier/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"SupplierNumber":"","SupplierID":"","SupplierName":"","ContactPhone":"","PurchasePerson":"","SupplyScope":"","SupplyCategory":"","UnifiedCreditCode":"","Address":""};
    const editFormOptions = [[{"title":"编码","field":"SupplierNumber"}],
                              [{"title":"供应商Id","field":"SupplierID"}],
                              [{"title":"供应商名称","field":"SupplierName"}],
                              [{"title":"联系电话","field":"ContactPhone"}],
                              [{"title":"采购负责人","field":"PurchasePerson"}],
                              [{"title":"供货范围","field":"SupplyScope"}],
                              [{"title":"供应类别","field":"SupplyCategory"}],
                              [{"title":"统一社会信用代码","field":"UnifiedCreditCode"}],
                              [{"title":"通讯地址","field":"Address"}]];
    const searchFormFields = {"ContactPhone":"","PurchasePerson":"","SupplyScope":"","SupplyCategory":"","UnifiedCreditCode":"","Address":""};
    const searchFormOptions = [[{"title":"主键ID","field":"ID","type":"like"},{"title":"编码","field":"SupplierNumber","type":"like"},{"title":"供应商Id","field":"SupplierID","type":"like"},{"title":"供应商名称","field":"SupplierName","type":"like"}],[{"title":"联系电话","field":"ContactPhone","type":"like"},{"title":"采购负责人","field":"PurchasePerson","type":"like"},{"title":"供货范围","field":"SupplyScope","type":"like"},{"title":"供应类别","field":"SupplyCategory","type":"like"}],[{"title":"统一社会信用代码","field":"UnifiedCreditCode","type":"like"},{"title":"通讯地址","field":"Address","type":"like"}]];
    const columns = [{field:'ID',title:'主键ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'SupplierNumber',title:'编码',type:'string',sort:true,width:220,align:'left'},
                       {field:'SupplierID',title:'供应商Id',type:'long',sort:true,width:80,align:'left'},
                       {field:'SupplierName',title:'供应商名称',type:'string',sort:true,width:220,align:'left'},
                       {field:'ContactPhone',title:'联系电话',type:'string',sort:true,width:120,align:'left'},
                       {field:'PurchasePerson',title:'采购负责人',type:'string',sort:true,width:120,align:'left'},
                       {field:'SupplyScope',title:'供货范围',type:'string',sort:true,width:220,align:'left'},
                       {field:'SupplyCategory',title:'供应类别',type:'string',sort:true,width:120,align:'left'},
                       {field:'UnifiedCreditCode',title:'统一社会信用代码',type:'string',sort:true,width:120,align:'left'},
                       {field:'Address',title:'通讯地址',type:'string',sort:true,width:220,align:'left'},
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