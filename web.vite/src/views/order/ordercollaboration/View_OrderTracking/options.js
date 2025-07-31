// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Id',
        footer: "Foots",
        cnName: '缺料运算',
        name: 'View_OrderTracking',
        newTabEdit: false,
        url: "/View_OrderTracking/",
        sortName: "MtoNo"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {};
    const editFormOptions = [];
    const searchFormFields = {"MaterialNumber":"","MaterialName":"","Urgency":"","IsJoinTask":[]};
    const searchFormOptions = [[{"title":"销售合同号","field":"ContractNo","type":"multipleInput"},{"title":"销售订单号","field":"SOBillNo","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"计划跟踪号","field":"MtoNo","type":"multipleInput"},{"title":"整机规格型号","field":"TopSpecification","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}],[{"title":"物料编码","field":"MaterialNumber","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"物料名称","field":"MaterialName","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"紧急等级","field":"Urgency","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"dataKey":"enable","data":[],"title":"是否关联总任务","field":"IsJoinTask","type":"selectList","comparationList":[{"key":"selectList","value":"select多选"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}]];
    const columns = [{field:'ProjectName',title:'项目名称',type:'string',sort:true,width:110,hidden:true,align:'left'},
                       {field:'SalesPerson',title:'销售员',type:'string',sort:true,width:110,hidden:true,align:'left'},
                       {field:'ContractType',title:'合同类型',type:'string',sort:true,width:110,hidden:true,align:'left'},
                       {field:'ContractNo',title:'销售合同号',type:'string',sort:true,width:120,align:'left'},
                       {field:'UseUnit',title:'使用单位',type:'string',sort:true,width:110,hidden:true,align:'left'},
                       {field:'CustName',title:'客户名称',type:'string',sort:true,width:120,align:'left'},
                       {field:'SOBillNo',title:'销售订单号',type:'string',sort:true,width:120,align:'left'},
                       {field:'MtoNo',title:'计划跟踪号',type:'string',sort:true,width:120,align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:150,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',sort:true,width:150,align:'left'},
                       {field:'ProductionModel',title:'产品型号',type:'string',width:220,align:'left'},
                       {field:'TopSpecification',title:'整机规格型号',type:'string',sort:true,width:150,align:'left'},
                       {field:'ProScheduleYearMonth',title:'排产月份',type:'string',width:220,align:'left'},
                       {field:'PlanTaskMonth',title:'月度任务',type:'string',sort:true,width:120,align:'left'},
                       {field:'PlanTaskWeek',title:'周任务',type:'string',sort:true,width:120,align:'left'},
                       {field:'Urgency',title:'紧急等级',type:'string',sort:true,width:110,align:'left'},
                       {field:'DeliveryDate',title:'客户要货日期',type:'date',sort:true,width:110,align:'left'},
                       {field:'ReplyDeliveryDate',title:'回复交货日期',type:'date',sort:true,width:130,align:'left'},
                       {field:'ComputedDate',title:'运算日期',type:'datetime',width:150,align:'left'},
                       {field:'BidDate',title:'中标日期',type:'date',sort:true,width:110,align:'left'},
                       {field:'BomCreateDate',title:'BOM创建日期',type:'date',sort:true,width:140,align:'left'},
                       {field:'OrderCreateDate',title:'订单创建日期',type:'date',sort:true,width:120,hidden:true,align:'left'},
                       {field:'OrderAuditDate',title:'订单审核日期',type:'date',sort:true,width:130,align:'left'},
                       {field:'PrdScheduleDate',title:'排产日期',type:'datetime',sort:true,width:110,align:'left'},
                       {field:'PlanConfirmDate',title:'计划确认日期',type:'date',sort:true,width:110,align:'left'},
                       {field:'PlanStartDate',title:'计划开工日期',type:'date',sort:true,width:110,align:'left'},
                       {field:'StartDate',title:'实际开工日期',type:'date',sort:true,width:110,align:'left'},
                       {field:'LastInStockDate',title:'最近入库日期',type:'datetime',width:150,align:'left'},
                       {field:'OrderQty',title:'订单数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'InstockQty',title:'入库数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'LastOutStockDate',title:'最近出库日期',type:'datetime',width:150,align:'left'},
                       {field:'OutStockQty',title:'出库数量',type:'decimal',width:110,align:'left'},
                       {field:'UnInstockQty',title:'未完数量',type:'decimal',sort:true,width:110,align:'left'},
                       {field:'MtoNoStatus',title:'行状态',type:'string',sort:true,width:110,hidden:true,align:'left'},
                       {field:'Id',title:'主键ID',type:'long',sort:true,width:110,hidden:true,require:true,align:'left'},
                       {field:'IsJoinTask',title:'是否关联总任务',type:'int',bind:{ key:'enable',data:[]},sort:true,width:80,align:'left'},
                       {field:'JoinTaskBillNo',title:'关联总任务单据号',type:'string',sort:true,width:150,align:'left'},
                       {field:'FinishStatus',title:'订单完成状态',type:'string',sort:true,width:110,align:'left'},
                       {field:'SOBillID',title:'订单ID',type:'long',sort:true,width:110,hidden:true,align:'left'},
                       {field:'SOEntryID',title:'订单明细ID',type:'long',sort:true,width:110,hidden:true,align:'left'},
                       {field:'CancelStatus',title:'作废状态',type:'string',sort:true,width:110,hidden:true,align:'left'},
                       {field:'Amount',title:'订单明细金额',type:'decimal',sort:true,width:110,hidden:true,align:'left'},
                       {field:'MRPTERMINATESTATUS',title:'业务终止',type:'string',sort:true,width:110,hidden:true,align:'left'},
                       {field:'MRPFREEZESTATUS',title:'业务冻结',type:'string',sort:true,width:110,hidden:true,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',sort:true,width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',sort:true,width:110,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',sort:true,width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',sort:true,width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',sort:true,width:110,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',sort:true,width:80,hidden:true,align:'left'},
                       {field:'BillStatus',title:'订单状态',type:'string',width:120,align:'left'}];
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