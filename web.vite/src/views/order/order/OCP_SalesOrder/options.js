// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'OrderID',
        footer: "Foots",
        cnName: '销售订单',
        name: 'OCP_SalesOrder',
        newTabEdit: false,
        url: "/OCP_SalesOrder/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"CustomerContractNumber":"","ContractType":"","ContractSignDate":"","CustomerRequiredDate":"","TotalOrderAmount":"","ProjectName":"","Customer":"","Consignee":"","Salesperson":"","SalesRegion":""};
    const editFormOptions = [[{"title":"用户合同号","field":"CustomerContractNumber"},
                               {"title":"合同类型","required":true,"field":"ContractType"},
                               {"title":"合同签订日期","required":true,"field":"ContractSignDate","type":"datetime"}],
                              [{"title":"客户要货日期","required":true,"field":"CustomerRequiredDate","type":"datetime"},
                               {"title":"订单累计金额","required":true,"field":"TotalOrderAmount","type":"decimal"},
                               {"title":"项目名称","required":true,"field":"ProjectName"}],
                              [{"title":"客户","required":true,"field":"Customer"},
                               {"title":"使用单位","required":true,"field":"Consignee"},
                               {"title":"销售员","required":true,"field":"Salesperson"}],
                              [{"title":"销售员区域","required":true,"field":"SalesRegion"}]];
    const searchFormFields = {"ContractType":"","ContractSignDate":"","CustomerRequiredDate":"","TotalOrderAmount":""};
    const searchFormOptions = [[{"title":"订单ID","field":"OrderID","type":"like"},{"title":"订单编号","field":"OrderNumber","type":"like"},{"title":"销售合同号","field":"SalesContractNumber","type":"like"},{"title":"用户合同号","field":"CustomerContractNumber","type":"like"}],[{"title":"合同类型","field":"ContractType","type":"like"},{"title":"合同签订日期","field":"ContractSignDate","type":"datetime"},{"title":"客户要货日期","field":"CustomerRequiredDate","type":"datetime"},{"title":"订单累计金额","field":"TotalOrderAmount","type":"like"}]];
    const columns = [{field:'OrderID',title:'订单ID',type:'int',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'OrderNumber',title:'订单编号',type:'string',link:true,sort:true,width:120,require:true,align:'left'},
                       {field:'SalesContractNumber',title:'销售合同号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'CustomerContractNumber',title:'用户合同号',type:'string',sort:true,width:120,align:'left'},
                       {field:'ContractType',title:'合同类型',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'ContractSignDate',title:'合同签订日期',type:'datetime',sort:true,width:150,require:true,align:'left'},
                       {field:'CustomerRequiredDate',title:'客户要货日期',type:'datetime',sort:true,width:150,require:true,align:'left'},
                       {field:'TotalOrderAmount',title:'订单累计金额',type:'decimal',sort:true,width:110,require:true,align:'left'},
                       {field:'ProjectName',title:'项目名称',type:'string',sort:true,width:110,require:true,align:'left'},
                       {field:'Customer',title:'客户',type:'string',sort:true,width:110,require:true,align:'left'},
                       {field:'Consignee',title:'使用单位',type:'string',sort:true,width:110,require:true,align:'left'},
                       {field:'Salesperson',title:'销售员',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'SalesRegion',title:'销售员区域',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'}];
    const detail =  {
                    cnName: '销售订单明细',
                    table: 'OCP_SalesOrderDetail',
                    columns: [{field:'OrderID',title:'订单ID',type:'int',sort:true,width:80,require:true,align:'left'},
                       {field:'DetailID',title:'明细ID',type:'int',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'PositionNumber',title:'位号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'ValveModel',title:'阀门型号',type:'string',sort:true,width:120,edit:{type:''},require:true,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',sort:true,width:120,edit:{type:''},require:true,align:'left'},
                       {field:'Quantity',title:'数量',type:'decimal',sort:true,width:110,edit:{type:''},require:true,align:'left'},
                       {field:'UnitPrice',title:'单价',type:'decimal',sort:true,width:110,edit:{type:''},require:true,align:'left'},
                       {field:'TotalPrice',title:'总价',type:'decimal',sort:true,width:110,edit:{type:''},require:true,align:'left'},
                       {field:'PlanTrackingNumber',title:'计划跟踪号',type:'string',sort:true,width:120,edit:{type:''},require:true,align:'left'},
                       {field:'CurrentNode',title:'当前节点',type:'string',sort:true,width:120,edit:{type:''},require:true,align:'left'},
                       {field:'ERPPreviousNode',title:'ERP上一节点',type:'string',sort:true,width:120,edit:{type:''},align:'left'},
                       {field:'ERPCompletionTime',title:'ERP完成时间',type:'datetime',sort:true,width:150,edit:{type:'datetime'},align:'left'},
                       {field:'MOMPreviousNode',title:'MOM上一节点',type:'string',sort:true,width:120,edit:{type:''},align:'left'},
                       {field:'MOMCompletionTime',title:'MOM完成时间',type:'datetime',sort:true,width:150,edit:{type:'datetime'},align:'left'},
                       {field:'Status',title:'状态',type:'string',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'}],
                    sortName: 'CreateDate',
                    key: 'DetailID'
                                            };
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