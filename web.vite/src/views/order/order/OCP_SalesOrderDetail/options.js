// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'DetailID',
        footer: "Foots",
        cnName: '销售订单明细',
        name: 'OCP_SalesOrderDetail',
        newTabEdit: false,
        url: "/OCP_SalesOrderDetail/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"ValveModel":"","MaterialCode":"","Quantity":"","UnitPrice":"","TotalPrice":"","PlanTrackingNumber":"","CurrentNode":"","ERPPreviousNode":"","ERPCompletionTime":"","MOMPreviousNode":"","MOMCompletionTime":"","Status":""};
    const editFormOptions = [[{"title":"阀门型号","required":true,"field":"ValveModel"},
                               {"title":"物料编码","required":true,"field":"MaterialCode"},
                               {"title":"数量","required":true,"field":"Quantity","type":"decimal"}],
                              [{"title":"单价","required":true,"field":"UnitPrice","type":"decimal"},
                               {"title":"总价","required":true,"field":"TotalPrice","type":"decimal"},
                               {"title":"计划跟踪号","required":true,"field":"PlanTrackingNumber"}],
                              [{"title":"当前节点","required":true,"field":"CurrentNode"},
                               {"title":"ERP上一节点","field":"ERPPreviousNode"},
                               {"title":"ERP完成时间","field":"ERPCompletionTime","type":"datetime"}],
                              [{"title":"MOM上一节点","field":"MOMPreviousNode"},
                               {"title":"MOM完成时间","field":"MOMCompletionTime","type":"datetime"},
                               {"title":"状态","field":"Status"}]];
    const searchFormFields = {"MaterialCode":"","Quantity":"","UnitPrice":"","TotalPrice":""};
    const searchFormOptions = [[{"title":"订单ID","field":"OrderID","type":"like"},{"title":"明细ID","field":"DetailID","type":"like"},{"title":"位号","field":"PositionNumber","type":"like"},{"title":"阀门型号","field":"ValveModel","type":"like"}],[{"title":"物料编码","field":"MaterialCode","type":"like"},{"title":"数量","field":"Quantity","type":"like"},{"title":"单价","field":"UnitPrice","type":"like"},{"title":"总价","field":"TotalPrice","type":"like"}]];
    const columns = [{field:'OrderID',title:'订单ID',type:'int',sort:true,width:80,require:true,align:'left'},
                       {field:'DetailID',title:'明细ID',type:'int',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'PositionNumber',title:'位号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'ValveModel',title:'阀门型号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'Quantity',title:'数量',type:'decimal',sort:true,width:110,require:true,align:'left'},
                       {field:'UnitPrice',title:'单价',type:'decimal',sort:true,width:110,require:true,align:'left'},
                       {field:'TotalPrice',title:'总价',type:'decimal',sort:true,width:110,require:true,align:'left'},
                       {field:'PlanTrackingNumber',title:'计划跟踪号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'CurrentNode',title:'当前节点',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'ERPPreviousNode',title:'ERP上一节点',type:'string',sort:true,width:120,align:'left'},
                       {field:'ERPCompletionTime',title:'ERP完成时间',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'MOMPreviousNode',title:'MOM上一节点',type:'string',sort:true,width:120,align:'left'},
                       {field:'MOMCompletionTime',title:'MOM完成时间',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'Status',title:'状态',type:'string',sort:true,width:110,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'}];
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