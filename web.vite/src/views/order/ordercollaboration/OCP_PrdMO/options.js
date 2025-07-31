// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ID',
        footer: "Foots",
        cnName: '整机生产订单表',
        name: 'OCP_PrdMO',
        newTabEdit: false,
        url: "/OCP_PrdMO/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"PlanTaskMonth":"","PlanTaskWeek":"","Urgency":"","MOAuditDate":"","ProductionType":"","ProductionQty":"","InboundQty":"","UnInboundQty":"","OverdueQty":"","OverdueDays":""};
    const editFormOptions = [[{"title":"月度任务","field":"PlanTaskMonth"},
                               {"title":"周任务","field":"PlanTaskWeek"},
                               {"title":"紧急等级","field":"Urgency"}],
                              [{"title":"生产订单审核时间","field":"MOAuditDate","type":"datetime"},
                               {"title":"生产类型","field":"ProductionType"},
                               {"title":"生产数量","field":"ProductionQty","type":"decimal"}],
                              [{"title":"已入库数量","field":"InboundQty","type":"decimal"},
                               {"title":"未入库数量","field":"UnInboundQty","type":"decimal"},
                               {"title":"超期数量","field":"OverdueQty","type":"decimal"}],
                              [{"title":"超期天数","field":"OverdueDays","type":"number"}]];
    const searchFormFields = {};
    const searchFormOptions = [[{"title":"生产单据号","field":"ProductionOrderNo","type":"like"},{"title":"紧急等级","field":"Urgency","type":"like"},{"title":"生产订单审核时间","field":"MOAuditDate","type":"datetime"},{"title":"生产类型","field":"ProductionType","type":"like"}]];
    const columns = [{field:'ID',title:'主键ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ProductionOrderNo',title:'生产单据号',type:'string',sort:true,width:120,align:'left'},
                       {field:'FID',title:'生产订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'PlanTaskMonth',title:'月度任务',type:'string',sort:true,width:220,align:'left'},
                       {field:'PlanTaskWeek',title:'周任务',type:'string',sort:true,width:220,align:'left'},
                       {field:'Urgency',title:'紧急等级',type:'string',sort:true,width:220,align:'left'},
                       {field:'MOAuditDate',title:'生产订单审核时间',type:'date',sort:true,width:150,align:'left'},
                       {field:'ProductionType',title:'生产类型',type:'string',sort:true,width:120,align:'left'},
                       {field:'ProductionQty',title:'生产数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'InboundQty',title:'已入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'UnInboundQty',title:'未入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueQty',title:'超期数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'OverdueDays',title:'超期天数',type:'int',sort:true,width:80,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'}];
    const detail =  {
                    cnName: '整机生产订单明细表',
                    table: 'OCP_PrdMODetail',
                    columns: [{field:'DetailID',title:'明细ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ID',title:'主表ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FID',title:'生产订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'FENTRYID',title:'生产订单明细ID',type:'long',sort:true,width:80,hidden:true,edit:{type:''},align:'left'},
                       {field:'Seq',title:'行号',type:'int',sort:true,width:80,edit:{type:''},align:'left'},
                       {field:'PlanTraceNo',title:'计划跟踪号',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'SalesContractNo',title:'销售合同号',type:'string',sort:true,width:120,edit:{type:''},align:'left'},
                       {field:'SalesDocumentNo',title:'销售单据号',type:'string',sort:true,width:120,edit:{type:''},align:'left'},
                       {field:'ProductionOrderStatus',title:'生产订单状态',type:'string',sort:true,width:120,edit:{type:''},align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',sort:true,width:80,hidden:true,edit:{type:''},align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'ProductCategory',title:'产品大类',type:'string',sort:true,width:120,edit:{type:''},align:'left'},
                       {field:'PlanTaskMonth',title:'总任务月',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'PlanTaskWeek',title:'总任务周',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'Urgency',title:'紧急等级',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'PlanQty',title:'计划数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'InboundQty',title:'已入库数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'UnInboundQty',title:'未入库数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'OverdueQty',title:'超期数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'PickMtrlDate',title:'生产订单领料日期',type:'date',sort:true,width:150,edit:{type:'datetime'},align:'left'},
                       {field:'PlanCompleteDate',title:'计划完工日期',type:'date',sort:true,width:150,edit:{type:'datetime'},align:'left'},
                       {field:'ActualCompleteDate',title:'实际完工日期',type:'date',sort:true,width:150,edit:{type:'datetime'},align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'}],
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