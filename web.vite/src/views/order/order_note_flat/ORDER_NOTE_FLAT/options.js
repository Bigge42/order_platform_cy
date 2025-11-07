// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'source_entry_id',
        footer: "Foots",
        cnName: '备注拆分工具',
        name: 'ORDER_NOTE_FLAT',
        newTabEdit: false,
        url: "/ORDER_NOTE_FLAT/",
        sortName: "contract_no"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"note_body_actuator":"","note_accessory_debug":"","note_pressure_leak":"","note_packing":""};
    const editFormOptions = [[{"title":"阀体及执行机构装配备注","field":"note_body_actuator","type":"textarea","comparationList":[{"key":"textarea","value":"textarea"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"附件安装及调试备注","field":"note_accessory_debug","type":"textarea","comparationList":[{"key":"textarea","value":"textarea"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"强压泄漏试验备注","field":"note_pressure_leak","type":"textarea","comparationList":[{"key":"textarea","value":"textarea"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"装箱备注","field":"note_packing","type":"textarea","comparationList":[{"key":"textarea","value":"textarea"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}]];
    const searchFormFields = {};
    const searchFormOptions = [];
    const columns = [{field:'source_type',title:'source_type',type:'string',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'source_entry_id',title:'source_entry_id',type:'long',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'contract_no',title:'销售合同号',type:'string',sort:true,width:120,readonly:true,align:'left'},
                       {field:'product_model',title:'产品型号',type:'string',sort:true,width:180,readonly:true,align:'left'},
                       {field:'specification',title:'规格型号',type:'string',width:150,readonly:true,align:'left'},
                       {field:'mto_no',title:'计划跟踪号',type:'string',sort:true,width:120,readonly:true,align:'left'},
                       {field:'internal_note',title:'内部信息传递',type:'string',sort:true,width:110,readonly:true,align:'left'},
                       {field:'selection_guid',title:'选型ID',type:'string',sort:true,width:120,readonly:true,align:'left'},
                       {field:'remark_raw',title:'原始备注',type:'string',sort:true,width:110,readonly:true,align:'left'},
                       {field:'note_body_actuator',title:'阀体及执行机构装配备注',type:'string',width:110,align:'left'},
                       {field:'note_accessory_debug',title:'附件安装及调试备注',type:'string',width:110,align:'left'},
                       {field:'note_pressure_leak',title:'强压泄漏试验备注',type:'string',width:110,align:'left'},
                       {field:'note_packing',title:'装箱备注',type:'string',width:110,align:'left'},
                       {field:'created_at',title:'created_at',type:'datetime',width:110,hidden:true,require:true,align:'left'},
                       {field:'updated_at',title:'updated_at',type:'datetime',width:110,hidden:true,require:true,align:'left'},
                       {field:'bz_changed',title:'发生变更',type:'bool',width:110,readonly:true,require:true,align:'left'}];
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