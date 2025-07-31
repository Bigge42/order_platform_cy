export default function(){
    // 表格基本配置
    const table = {
        key: 'NegotiationID',
        footer: "Foots",
        cnName: '协商表',
        name: 'OCP_Negotiation',
        newTabEdit: false,
        url: "/OCP_Negotiation/",
        sortName: "CreateDate"
    };

    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;

    // 编辑表单字段
    const editFormFields = {"BillNo":"","Seq":"","NegotiationType":"","NegotiationContent":"","NegotiationReason":"","NegotiationDate":"","MatterDescription":"","Remarks":""};
    
    // 编辑表单配置
    const editFormOptions = [[{"title":"单据编号","field":"BillNo"},
                               {"title":"单据行号","field":"Seq"},
                               {"title":"协商类型","field":"NegotiationType"}],
                              [{"title":"协商内容","field":"NegotiationContent"},
                               {"title":"协商原因","field":"NegotiationReason"},
                               {"title":"协商日期","field":"NegotiationDate","type":"datetime"}],
                              [{"title":"事项说明","field":"MatterDescription"},
                               {"title":"备注","field":"Remarks"}]];

    // 搜索表单字段
    const searchFormFields = {"Seq":"","NegotiationType":"","NegotiationContent":"","NegotiationReason":"","NegotiationDate":"","MatterDescription":"","Remarks":""};
    
    // 搜索表单配置
    const searchFormOptions = [[{"title":"协商ID","field":"NegotiationID","type":"like"},{"title":"业务类型","field":"BusinessType","type":"like"},{"title":"业务主键","field":"BusinessKey","type":"like"},{"title":"单据编号","field":"BillNo","type":"like"}],[{"title":"单据行号","field":"Seq","type":"like"},{"title":"协商类型","field":"NegotiationType","type":"like"},{"title":"协商内容","field":"NegotiationContent","type":"like"},{"title":"协商原因","field":"NegotiationReason","type":"like"}],[{"title":"协商日期","field":"NegotiationDate","type":"datetime"},{"title":"事项说明","field":"MatterDescription","type":"like"},{"title":"备注","field":"Remarks","type":"like"}]];

    // 列配置
    const columns = [{field:'NegotiationID',title:'协商ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'BusinessType',title:'业务类型',type:'string',sort:true,width:120,align:'left'},
                       {field:'BillNo',title:'单据编号',type:'string',sort:true,width:120,align:'left'},
                       {field:'Seq',title:'单据行号',type:'string',sort:true,width:120,align:'left'},
                       {field:'NegotiationType',title:'协商类型',type:'string',sort:true,width:120,align:'left'},
                       {field:'NegotiationContent',title:'协商内容',type:'string',sort:true,width:220,align:'left'},
                       {field:'NegotiationReason',title:'协商原因',type:'string',sort:true,width:220,align:'left'},
                       {field:'NegotiationDate',title:'协商日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'MatterDescription',title:'事项说明',type:'string',sort:true,width:220,align:'left'},
                       {field: 'NegotiationStatus',title:'协商状态',type:'string',sort:true,width:120,align:'left'},
                       {field:'Remarks',title:'备注',type:'string',sort:true,width:220,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'}];

    // 详情配置
    const detail = {
        columns: []
    };

    // 明细表配置
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