// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'DetailID',
        footer: "Foots",
        cnName: '销售订单进度详情',
        name: 'OCP_SOProgressDetail',
        newTabEdit: false,
        url: "/OCP_SOProgressDetail/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"MtoNo":"","MOBillNo":"","BusinessStatus":"","Qty":"","PositionNo":"","MaterialNumber":"","ProductionModel":"","Specification":"","InstockQty":"","UnInstockQty":"","OutStockQty":"","ChangeStatus":""};
    const editFormOptions = [[{"title":"计划跟踪号","field":"MtoNo"},
                               {"title":"生产单据号","field":"MOBillNo"},
                               {"title":"业务状态","field":"BusinessStatus"}],
                              [{"title":"订货数量","field":"Qty","type":"decimal"},
                               {"title":"位号","field":"PositionNo"},
                               {"title":"物料编码","field":"MaterialNumber"}],
                              [{"title":"产品型号","field":"ProductionModel"},
                               {"title":"规格型号","field":"Specification"},
                               {"title":"入库数量","field":"InstockQty","type":"decimal"}],
                              [{"title":"待入库数量","field":"UnInstockQty","type":"decimal"},
                               {"title":"出库数量","field":"OutStockQty","type":"decimal"},
                               {"title":"变更状态","field":"ChangeStatus"}]];
    const searchFormFields = {"MaterialNumber":"","ProductionModel":"","Specification":"","ChangeStatus":""};
    const searchFormOptions = [[{"title":"销售订单号","field":"SOBillNo","type":"like"},{"title":"计划跟踪号","field":"MtoNo","type":"like"},{"title":"生产单据号","field":"MOBillNo","type":"like"},{"title":"业务状态","field":"BusinessStatus","type":"like"}],[{"title":"物料编码","field":"MaterialNumber","type":"like"},{"title":"产品型号","field":"ProductionModel","type":"like"},{"title":"规格型号","field":"Specification","type":"like"},{"title":"变更状态","field":"ChangeStatus","type":"like"}]];
    const columns = [{field:'DetailID',title:'详情ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'OrderID',title:'订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'SOBillNo',title:'销售订单号',type:'string',sort:true,width:110,align:'left'},
                       {field:'MtoNo',title:'计划跟踪号',type:'string',sort:true,width:110,align:'left'},
                       {field:'MOBillNo',title:'生产单据号',type:'string',sort:true,width:110,align:'left'},
                       {field:'BusinessStatus',title:'业务状态',type:'string',sort:true,width:110,align:'left'},
                       {field:'Qty',title:'订货数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'PositionNo',title:'位号',type:'string',sort:true,width:110,align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:120,align:'left'},
                       {field:'ProductionModel',title:'产品型号',type:'string',sort:true,width:220,align:'left'},
                       {field:'Specification',title:'规格型号',type:'string',sort:true,width:220,align:'left'},
                       {field:'InstockQty',title:'入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'UnInstockQty',title:'待入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OutStockQty',title:'出库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'ChangeStatus',title:'变更状态',type:'string',sort:true,width:110,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'SOEntryID',title:'订单明细ID',type:'long',width:80,hidden:true,align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',width:80,hidden:true,align:'left'}];
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