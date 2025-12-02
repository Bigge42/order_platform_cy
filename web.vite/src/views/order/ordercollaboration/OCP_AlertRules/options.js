// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ID',
        footer: "Foots",
        cnName: '预警规则表',
        name: 'OCP_AlertRules',
        newTabEdit: false,
        url: "/OCP_AlertRules/",
        sortName: "Id"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"RuleName":"","AlertPage":"","FieldName":"","DayCount":"","advanceWarningDays":"","FinishStatusField":"","ConditionType":"","ConditionValue":"","ResponsiblePersonName":"","PushInterval":"","TriggerOA":""};
    const editFormOptions = [[{"title":"规则名称","required":true,"field":"RuleName"},
                               {"dataKey":"预警页面","data":[],"title":"预警页面","required":true,"field":"AlertPage","type":"select","comparationList":[{"key":"select","value":"select"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"dataKey":"预警字段名","data":[],"title":"字段名","required":true,"field":"FieldName","type":"select","comparationList":[{"key":"select","value":"select"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"title":"阈值天数","required":true,"field":"DayCount","type":"number","comparationList":[{"key":"number","value":"number"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"提前预警天数","field":"advanceWarningDays","type":"number","comparationList":[{"key":"number","value":"number"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"dataKey":"预警字段名","data":[],"title":"完成状态字段","required":true,"field":"FinishStatusField","type":"select","comparationList":[{"key":"select","value":"select"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"dataKey":"完成判定方式","data":[],"title":"完成判定方式","required":true,"field":"ConditionType","type":"select","comparationList":[{"key":"select","value":"select"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"完成判定值","field":"ConditionValue","type":"text","comparationList":[{"key":"text","value":""},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"title":"责任人（OA接受人）","field":"ResponsiblePersonName","disabled":true},
                               {"title":"推送周期","required":true,"field":"PushInterval","type":"text","comparationList":[{"key":"text","value":""},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"dataKey":"是否触发OA流程","data":[],"title":"是否触发OA流程","required":true,"field":"TriggerOA","type":"select","comparationList":[{"key":"select","value":"select"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}]];
    const searchFormFields = {"RuleName":"","AlertPage":[],"FieldName":"","TriggerOA":[],"TaskStatus":[]};
    const searchFormOptions = [[{"title":"规则名称","field":"RuleName"},{"dataKey":"预警页面","data":[],"title":"预警页面","field":"AlertPage","type":"selectList","comparationList":[{"key":"selectList","value":"select多选"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"dataKey":"预警字段名","data":[],"title":"字段名","field":"FieldName","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}],[{"dataKey":"任务状态","data":[],"title":"任务状态","field":"TaskStatus","type":"selectList","comparationList":[{"key":"selectList","value":"select多选"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"dataKey":"是否触发OA流程","data":[],"title":"是否触发OA流程","field":"TriggerOA","type":"selectList","comparationList":[{"key":"selectList","value":"select多选"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}]];
    const columns = [{field:'ID',title:'主键ID',type:'long',width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'RuleName',title:'规则名称',type:'string',width:120,require:true,align:'left'},
                       {field:'AlertPage',title:'预警页面',type:'string',bind:{ key:'预警页面',data:[]},width:120,require:true,align:'left'},
                       {field:'FieldName',title:'字段名',type:'string',bind:{ key:'预警字段名',data:[]},width:110,require:true,align:'left'},
                       {field:'DayCount',title:'阈值天数',type:'int',width:80,require:true,align:'left'},
                       {field:'advanceWarningDays',title:'提前预警天数',type:'int',sort:true,width:120,align:'left'},
                       {field:'FinishStatusField',title:'完成状态字段',type:'string',bind:{ key:'预警字段名',data:[]},sort:true,width:140,require:true,align:'left'},
                       {field:'ConditionType',title:'完成判定方式',type:'string',bind:{ key:'完成判定方式',data:[]},sort:true,width:140,require:true,align:'left'},
                       {field:'ConditionValue',title:'完成判定值',type:'string',sort:true,width:120,align:'left'},
                       {field:'ResponsiblePersonName',title:'责任人（OA接受人）',type:'string',width:160,readonly:true,align:'left'},
                       {field:'ResponsiblePersonLoginName',title:'责任人登录名',type:'string',width:110,hidden:true,align:'left'},
                       {field:'PushInterval',title:'推送周期',type:'string',sort:true,width:110,require:true,align:'left'},
                       {field:'TriggerOA',title:'是否触发OA流程',type:'int',bind:{ key:'是否触发OA流程',data:[]},sort:true,width:160,require:true,align:'left'},
                       {field:'TaskStatus',title:'任务状态',type:'int',bind:{ key:'任务状态',data:[]},width:100,require:true,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'}];
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