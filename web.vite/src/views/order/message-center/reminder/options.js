export default function(){
    // 表格基本配置
    const table = {
        key: 'UrgentOrderID',
        footer: "Foots",
        cnName: '催单表',
        name: 'OCP_UrgentOrder',
        newTabEdit: false,
        url: "/OCP_UrgentOrder/",
        sortName: "CreateDate"
    };

    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;

    // 编辑表单字段
    const editFormFields = {
        "BillNo":"",
        "Seq":"",
        "UrgencyLevel":"",
        "DefaultResPerson":"",
        "AssignedResPerson":"",
        "AssignedReplyTime":"",
        "TimeUnit":"",
        "UrgentContent":"",
        "Remarks":""
    };

    // 编辑表单配置
    const editFormOptions = [
        [
            {"title":"单据编号","field":"BillNo"},
            {"title":"单据行号","field":"Seq"},
            {"title":"紧急等级","field":"UrgencyLevel"}
        ],
        [
            {"title":"默认负责人","field":"DefaultResPerson"},
            {"title":"指定负责人","field":"AssignedResPerson"},
            {"title":"指定回复时间","field":"AssignedReplyTime","type":"decimal"}
        ],
        [
            {"title":"时间单位","field":"TimeUnit","type":"number"},
            {"title":"催单内容","field":"UrgentContent"},
            {"title":"备注","field":"Remarks"}
        ]
    ];

    // 搜索表单字段
    const searchFormFields = {
        "Seq":"",
        "UrgencyLevel":"",
        "DefaultResPerson":"",
        "AssignedResPerson":"",
        "AssignedReplyTime":"",
        "TimeUnit":"",
        "UrgentContent":"",
        "Remarks":""
    };

    // 搜索表单配置
    const searchFormOptions = [
        [
            {"title":"催单ID","field":"UrgentOrderID","type":"like"},
            {"title":"业务类型","field":"BusinessType","type":"like"},
            {"title":"业务主键","field":"BusinessKey","type":"like"},
            {"title":"单据编号","field":"BillNo","type":"like"}
        ],
        [
            {"title":"单据行号","field":"Seq","type":"like"},
            {"title":"紧急等级","field":"UrgencyLevel","type":"like"},
            {"title":"默认负责人","field":"DefaultResPerson","type":"like"},
            {"title":"指定负责人","field":"AssignedResPerson","type":"like"}
        ],
        [
            {"title":"指定回复时间","field":"AssignedReplyTime","type":"like"},
            {"title":"时间单位","field":"TimeUnit","type":"like"},
            {"title":"催单内容","field":"UrgentContent","type":"like"},
            {"title":"备注","field":"Remarks","type":"like"}
        ]
    ];

    // 列配置
    const columns = [
        {field:'UrgentOrderID',title:'催单ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
        {field:'BusinessType',title:'业务类型',type:'string',sort:true,width:120,align:'left'},
        {field:'BillNo',title:'单据编号',type:'string',sort:true,width:120,align:'left'},
        {field:'Seq',title:'单据行号',type:'string',sort:true,width:120,align:'left'},
        {field:'UrgencyLevel',title:'紧急等级',type:'string',sort:true,width:110,align:'left'},
        {field:'DefaultResPerson',title:'默认负责人',type:'string',sort:true,width:120,align:'left'},
        {field:'AssignedResPerson',title:'指定负责人',type:'string',sort:true,width:120,align:'left'},
        {field:'AssignedReplyTime',title:'指定回复时间',type:'decimal',sort:true,width:110,align:'left'},
        {field:'TimeUnit',title:'时间单位',type:'int',sort:true,width:80,align:'left'},
        {field:'UrgentContent',title:'催单内容',type:'string',sort:true,width:220,align:'left'},
        {field:'Remarks',title:'备注',type:'string',sort:true,width:220,align:'left'},
        {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
        {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
        {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
        {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
        {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
        {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'},
        {field:'PlanTraceNo',title:'计划跟踪号',type:'string',width:220,align:'left'},
        {field:'UrgentStatus',title:'催单状态',type:'string',width:120,align:'left'},
    ];

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