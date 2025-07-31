// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'NegotiationID',
        footer: "Foots",
        cnName: '协商',
        name: 'OCP_Negotiation',
        newTabEdit: false,
        url: "/OCP_Negotiation/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"BillNo":"","Seq":"","NegotiationType":"","NegotiationContent":"","NegotiationReason":"","NegotiationDate":"","MatterDescription":"","Remarks":""};
    const editFormOptions = [[{"title":"单据编号","field":"BillNo"},
                               {"title":"单据行号","field":"Seq"},
                               {"title":"协商类型","field":"NegotiationType"}],
                              [{"title":"协商内容","field":"NegotiationContent"},
                               {"dataKey":"协商原因","data":[],"title":"协商原因","field":"NegotiationReason"},
                               {"title":"协商日期","field":"NegotiationDate","type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"title":"事项说明","field":"MatterDescription"},
                               {"title":"备注","field":"Remarks"}]];
    const searchFormFields = {"PlanTraceNo":"","UrgencyLevel":"","BillNo":"","MaterialNumber":"","MaterialName":"","Specification":""};
    const searchFormOptions = [[{"title":"计划跟踪号","field":"PlanTraceNo","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"紧急等级","field":"UrgencyLevel","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"单据编号","field":"BillNo","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}],[{"title":"物料编码","field":"MaterialNumber","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"物料名称","field":"MaterialName","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"规格型号","field":"Specification","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}]];
    const columns = [{field:'NegotiationID',title:'协商ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'BusinessType',title:'业务类型',type:'string',bind:{ key:'业务类型',data:[]},sort:true,width:120,align:'left'},
                       {field:'PlanTraceNo',title:'计划跟踪号',type:'string',width:220,align:'left'},
                       {field:'UrgencyLevel',title:'紧急等级',type:'string',width:110,align:'left'},
                       {field:'BusinessKey',title:'业务主键',type:'string',sort:true,width:220,hidden:true,align:'left'},
                       {field:'BillNo',title:'单据编号',type:'string',sort:true,width:120,align:'left'},
                       {field:'Seq',title:'单据行号',type:'string',sort:true,width:120,align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',width:220,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',width:220,align:'left'},
                       {field:'Specification',title:'规格型号',type:'string',width:220,align:'left'},
                       {field:'NegotiationType',title:'协商类型',type:'string',sort:true,width:120,hidden:true,align:'left'},
                       {field:'NegotiationStatus',title:'协商状态',type:'string',width:120,align:'left'},
                       {field:'NegotiationContent',title:'协商内容',type:'string',sort:true,width:220,hidden:true,align:'left'},
                       {field:'NegotiationReason',title:'协商原因',type:'string',bind:{ key:'协商原因',data:[]},sort:true,width:220,align:'left'},
                       {field:'NegotiationDate',title:'协商日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'DefaultResPerson',title:'默认负责人',type:'string',width:120,align:'left'},
                       {field:'DefaultResPersonName',title:'默认负责人姓名',type:'string',width:120,align:'left'},
                       {field:'AssignedResPerson',title:'指定负责人',type:'string',width:120,align:'left'},
                       {field:'AssignedResPersonName',title:'指定负责人姓名',type:'string',width:120,align:'left'},
                       {field:'MatterDescription',title:'事项说明',type:'string',sort:true,width:220,align:'left'},
                       {field:'Remarks',title:'备注',type:'string',sort:true,width:220,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'DeliveryDate',title:'交货日期',type:'datetime',width:150,align:'left'},
                       {field:'BusinessTypeChild',title:'业务类型子类',type:'string',width:120,align:'left'},
                       {field:'SupplierCode',title:'供应商代码',type:'string',width:220,align:'left'},
                       {field:'SupplierName',title:'供应商名称',type:'string',width:220,align:'left'},
                       {field:'ReplyDeliveryDate',title:'回复交期',type:'datetime',width:150,align:'left'}];
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