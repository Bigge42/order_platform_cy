// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'PlanID',
        footer: "Foots",
        cnName: '订单计划',
        name: 'OCP_OrderPlan',
        newTabEdit: false,
        url: "/OCP_OrderPlan/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"BidDate":"","BOMCreationDate":"","OrderReviewDate":"","RequiredDeliveryDate":"","ReplyDeliveryDate":"","ScheduleDate":"","PlanConfirmationTime":"","PlannedStartDate":"","ActualStartDate":"","OrderStatus":"","OrderQuantity":""};
    const editFormOptions = [[{"title":"中标日期","field":"BidDate","type":"datetime"},
                               {"title":"BOM创建日期","field":"BOMCreationDate","type":"datetime"},
                               {"title":"订单审核日期","field":"OrderReviewDate","type":"datetime"}],
                              [{"title":"要货日期","field":"RequiredDeliveryDate","type":"datetime"},
                               {"title":"回复交货日期","field":"ReplyDeliveryDate","type":"datetime"},
                               {"title":"排产日期","field":"ScheduleDate","type":"datetime"}],
                              [{"title":"计划确认时间","field":"PlanConfirmationTime","type":"datetime"},
                               {"title":"计划开工日期","field":"PlannedStartDate","type":"datetime"},
                               {"title":"实际开工日期","field":"ActualStartDate","type":"datetime"}],
                              [{"title":"订单完成状态","field":"OrderStatus"},
                               {"title":"订单数量","required":true,"field":"OrderQuantity","type":"decimal"}]];
    const searchFormFields = {"BOMCreationDate":"","OrderReviewDate":"","RequiredDeliveryDate":"","ReplyDeliveryDate":"","ScheduleDate":"","PlanConfirmationTime":"","PlannedStartDate":"","ActualStartDate":"","OrderStatus":"","OrderQuantity":""};
    const searchFormOptions = [[{"title":"计划ID","field":"PlanID","type":"like"},{"title":"订单组ID","field":"OrderGroupID","type":"like"},{"title":"计划跟踪号","field":"PlanTrackingNumber","type":"like"},{"title":"中标日期","field":"BidDate","type":"datetime"}],[{"title":"BOM创建日期","field":"BOMCreationDate","type":"datetime"},{"title":"订单审核日期","field":"OrderReviewDate","type":"datetime"},{"title":"要货日期","field":"RequiredDeliveryDate","type":"datetime"},{"title":"回复交货日期","field":"ReplyDeliveryDate","type":"datetime"}],[{"title":"排产日期","field":"ScheduleDate","type":"datetime"},{"title":"计划确认时间","field":"PlanConfirmationTime","type":"datetime"},{"title":"计划开工日期","field":"PlannedStartDate","type":"datetime"},{"title":"实际开工日期","field":"ActualStartDate","type":"datetime"}],[{"title":"订单完成状态","field":"OrderStatus","type":"like"},{"title":"订单数量","field":"OrderQuantity","type":"like"}]];
    const columns = [{field:'PlanID',title:'计划ID',type:'int',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'OrderGroupID',title:'订单组ID',type:'int',sort:true,width:80,require:true,align:'left'},
                       {field:'PlanTrackingNumber',title:'计划跟踪号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'BidDate',title:'中标日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'BOMCreationDate',title:'BOM创建日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'OrderReviewDate',title:'订单审核日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'RequiredDeliveryDate',title:'要货日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'ReplyDeliveryDate',title:'回复交货日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'ScheduleDate',title:'排产日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'PlanConfirmationTime',title:'计划确认时间',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'PlannedStartDate',title:'计划开工日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'ActualStartDate',title:'实际开工日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'OrderStatus',title:'订单完成状态',type:'string',sort:true,width:110,align:'left'},
                       {field:'OrderQuantity',title:'订单数量',type:'decimal',sort:true,width:110,require:true,align:'left'},
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