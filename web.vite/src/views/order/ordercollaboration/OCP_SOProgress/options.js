// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'OrderID',
        footer: "Foots",
        cnName: '销售订单进度查询',
        name: 'OCP_SOProgress',
        newTabEdit: true,
        url: "/OCP_SOProgress/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = true;
    const key = table.key;
    const editFormFields = {"SalesContractNumber":"","CustomerContractNumber":"","Customer":"","BillStatus":"","SaleQty":"","InstockQty":"","UnInstockQty":"","OutStockQty":"","ContractType":"","ProjectName":""};
    const editFormOptions = [[{"title":"销售合同号","field":"SalesContractNumber"},
                               {"title":"用户合同号","field":"CustomerContractNumber"},
                               {"title":"客户","field":"Customer"}],
                              [{"title":"订单状态","field":"BillStatus"},
                               {"title":"数量","field":"SaleQty","type":"decimal"},
                               {"title":"入库数量","field":"InstockQty","type":"decimal"}],
                              [{"title":"待入库数量","field":"UnInstockQty","type":"decimal"},
                               {"title":"出库数量","field":"OutStockQty","type":"decimal"}],
                              [{"title":"合同类型","field":"ContractType"},
                               {"title":"项目名称","field":"ProjectName"}]];
    const searchFormFields = {"ContractType":"","Customer":"","BillStatus":"","ProjectName":""};
    const searchFormOptions = [[{"title":"销售订单号","field":"BillNo","type":"like"},{"title":"订单日期","field":"BillDate","type":"date"},{"title":"销售合同号","field":"SalesContractNumber","type":"like"},{"title":"用户合同号","field":"CustomerContractNumber","type":"like"}],[{"title":"客户","field":"Customer","type":"like"},{"title":"订单状态","field":"BillStatus","type":"like"}],[{"title":"合同类型","field":"ContractType","type":"like"},{"title":"项目名称","field":"ProjectName","type":"like"}]];
    const columns = [{field:'OrderID',title:'订单ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'BillNo',title:'销售订单号',type:'string',sort:true,width:110,align:'left'},
                       {field:'BillDate',title:'订单日期',type:'date',sort:true,width:150,align:'left'},
                       {field:'SalesContractNumber',title:'销售合同号',type:'string',sort:true,width:110,align:'left'},
                       {field:'CustomerContractNumber',title:'用户合同号',type:'string',sort:true,width:110,align:'left'},
                       {field:'ContractType',title:'合同类型',type:'string',width:120,align:'left'},
                       {field:'Customer',title:'客户',type:'string',sort:true,width:110,align:'left'},
                       {field:'BillStatus',title:'订单状态',type:'string',sort:true,width:110,align:'left'},
                       {field:'ProjectName',title:'项目名称',type:'string',width:220,align:'left'},
                       {field:'SalesPerson',title:'销售负责人',type:'string',width:220,align:'left'},
                       {field:'SaleQty',title:'数量',type:'decimal',sort:true,width:110,hidden:true,align:'left'},
                       {field:'InstockQty',title:'入库数量',type:'decimal',sort:true,width:110,hidden:true,align:'left'},
                       {field:'UnInstockQty',title:'待入库数量',type:'decimal',sort:true,width:110,hidden:true,align:'left'},
                       {field:'OutStockQty',title:'出库数量',type:'decimal',sort:true,width:110,hidden:true,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'FID',title:'销售订单主键',type:'long',width:80,hidden:true,align:'left'},
                       {field:'ESBModifyDate',title:'ESB修改日期',type:'datetime',width:150,align:'left'}];
    const detail =  {
                    cnName: '销售订单进度详情',
                    table: 'OCP_SOProgressDetail',
                    columns: [{field:'DetailID',title:'详情ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'OrderID',title:'订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'SOBillNo',title:'销售订单号',type:'string',sort:true,width:110,align:'left'},
                       {field:'MtoNo',title:'计划跟踪号',type:'string',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'MOBillNo',title:'生产单据号',type:'string',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'BusinessStatus',title:'业务状态',type:'string',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'Qty',title:'订货数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'PositionNo',title:'位号',type:'string',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:120,edit:{type:''},align:'left'},
                       {field:'ProductionModel',title:'产品型号',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'Specification',title:'规格型号',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'InstockQty',title:'入库数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'UnInstockQty',title:'待入库数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'OutStockQty',title:'出库数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'ChangeStatus',title:'变更状态',type:'string',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'SOEntryID',title:'订单明细ID',type:'long',width:80,hidden:true,align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',width:80,hidden:true,align:'left'}],
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