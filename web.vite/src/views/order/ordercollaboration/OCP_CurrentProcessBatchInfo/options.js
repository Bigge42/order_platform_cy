// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ProcessID',
        footer: "Foots",
        cnName: '当前工序批次信息',
        name: 'OCP_CurrentProcessBatchInfo',
        newTabEdit: false,
        url: "/OCP_CurrentProcessBatchInfo/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"DetailID":"","SOBillNo":"","MOBillNo":"","MtoNo":"","ProductNo":"","PositionNo":"","CurrentProcess":"","WorkOrder_ID":"","CurrentProcessStatus":""};
    const editFormOptions = [[{"title":"详情ID","field":"DetailID"}],
                              [{"title":"销售单据号","field":"SOBillNo"}],
                              [{"title":"生产单据号","field":"MOBillNo"}],
                              [{"title":"计划跟踪号","field":"MtoNo"}],
                              [{"title":"产品编号","field":"ProductNo"}],
                              [{"title":"位号","field":"PositionNo"}],
                              [{"title":"当前工序","field":"CurrentProcess"}],
                              [{"title":"工单编号","field":"WorkOrder_ID","type":"number"}],
                              [{"title":"当前工序执行状态","field":"CurrentProcessStatus"}]];
    const searchFormFields = {"MtoNo":"","ProductNo":"","PositionNo":"","CurrentProcess":"","CurrentProcessStatus":"","WorkOrder_ID":""};
    const searchFormOptions = [[{"title":"工序ID","field":"ProcessID"},{"title":"详情ID","field":"DetailID"},{"title":"销售单据号","field":"SOBillNo","type":"like"},{"title":"生产单据号","field":"MOBillNo","type":"like"}],[{"title":"计划跟踪号","field":"MtoNo","type":"like"},{"title":"产品编号","field":"ProductNo","type":"like"},{"title":"位号","field":"PositionNo","type":"like"},{"title":"当前工序","field":"CurrentProcess","type":"like"}],[{"title":"当前工序执行状态","field":"CurrentProcessStatus","type":"like"},{"title":"工单编号","field":"WorkOrder_ID","type":"like"}]];
    const columns = [{field:'ProcessID',title:'工序ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'DetailID',title:'详情ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'SOBillNo',title:'销售单据号',type:'string',sort:true,width:110,align:'left'},
                       {field:'MOBillNo',title:'生产单据号',type:'string',sort:true,width:110,align:'left'},
                       {field:'MtoNo',title:'计划跟踪号',type:'string',sort:true,width:110,align:'left'},
                       {field:'ProductNo',title:'产品编号',type:'string',sort:true,width:120,align:'left'},
                       {field:'PositionNo',title:'位号',type:'string',sort:true,width:120,align:'left'},
                       {field:'CurrentProcess',title:'当前工序',type:'string',sort:true,width:120,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CurrentProcessStatus',title:'当前工序执行状态',type:'string',width:120,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'WorkOrder_ID',title:'工单编号',type:'int',width:80,align:'left'}];
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