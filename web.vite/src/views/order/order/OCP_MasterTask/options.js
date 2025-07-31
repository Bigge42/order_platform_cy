// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'OrderGroupID',
        footer: "Foots",
        cnName: '总任务',
        name: 'OCP_MasterTask',
        newTabEdit: false,
        url: "/OCP_MasterTask/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"SalesOrderNumber":"","ContractNumber":"","CustomerName":"","PriorityLevel":""};
    const editFormOptions = [[{"title":"销售订单号","required":true,"field":"SalesOrderNumber"}],
                              [{"title":"合同号22","required":true,"field":"ContractNumber"}],
                              [{"title":"客户名称","required":true,"field":"CustomerName"}],
                              [{"title":"紧急等级","field":"PriorityLevel"}]];
    const searchFormFields = {"PriorityLevel":""};
    const searchFormOptions = [[{"title":"订单组ID","field":"OrderGroupID","type":"like"},{"title":"销售订单号","field":"SalesOrderNumber","type":"like"},{"title":"合同号22","field":"ContractNumber","type":"like"},{"title":"客户名称","field":"CustomerName","type":"like"}],[{"title":"紧急等级","field":"PriorityLevel","type":"like"}]];
    const columns = [{field:'OrderGroupID',title:'订单组ID',type:'int',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'SalesOrderNumber',title:'销售订单号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'ContractNumber',title:'合同号22',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'CustomerName',title:'客户名称',type:'string',sort:true,width:110,require:true,align:'left'},
                       {field:'PriorityLevel',title:'紧急等级',type:'string',sort:true,width:110,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',sort:true,width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',sort:true,width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',sort:true,width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',sort:true,width:80,hidden:true,align:'left'}];
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