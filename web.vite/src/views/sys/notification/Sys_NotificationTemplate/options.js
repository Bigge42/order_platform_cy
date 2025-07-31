// *Author：jxx
// *Contact：283591387@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'NotificationTemplateId',
        footer: "Foots",
        cnName: '消息模板',
        name: 'Sys_NotificationTemplate',
        newTabEdit: false,
        url: "/Sys_NotificationTemplate/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"NotificationTitle":"","NotificationCode":"","BusinessFunction":"","NotificationType":"","NotificationLevel":"","TargetObjectType":"","TargetObjectText":"","LinkUrl":"","IsFieldTemplate":"","NotificationContent":""};
    const editFormOptions = [[{"title":"通知标题","required":true,"field":"NotificationTitle","colSize":12,"type":"textarea"}],
                              [{"title":"通知编码","required":true,"field":"NotificationCode","disabled":true},
                               {"title":"业务功能","field":"BusinessFunction"},
                               {"dataKey":"通知类型","data":[],"title":"通知类型","field":"NotificationType","type":"select"},
                               {"title":"通知级别","field":"NotificationLevel"}],
                              [{"dataKey":"通知对象类型","data":[],"title":"通知对象类型","required":true,"field":"TargetObjectType","type":"select"},
                               {"title":"通知对象","field":"TargetObjectText"},
                               {"title":"跳转地址","field":"LinkUrl"},
                               {"dataKey":"enable","data":[],"title":"字段模板","field":"IsFieldTemplate","type":"radio"}],
                              [{"title":"通知内容","required":true,"field":"NotificationContent","colSize":12,"type":"editor"}]];
    const searchFormFields = {"NotificationCode":"","BusinessFunction":"","NotificationType":"","TargetObjectType":"","CreateDate":""};
    const searchFormOptions = [[{"title":"通知编码","field":"NotificationCode"},{"title":"业务功能","field":"BusinessFunction"},{"dataKey":"通知类型","data":[],"title":"通知类型","field":"NotificationType","type":"select"},{"dataKey":"通知对象类型","data":[],"title":"通知对象类型","field":"TargetObjectType","type":"select"},{"title":"创建时间","field":"CreateDate","type":"datetime"}]];
    const columns = [{field:'NotificationTemplateId',title:'主键ID',type:'string',width:120,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'TableName',title:'业务表',type:'string',width:120,hidden:true,align:'left'},
                       {field:'TableKey',title:'业务表主键id',type:'string',width:120,hidden:true,align:'left'},
                       {field:'NotificationTitle',title:'通知标题',type:'string',width:120,require:true,align:'left'},
                       {field:'NotificationCode',title:'通知编码',type:'string',width:120,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'BusinessFunction',title:'业务功能',type:'string',link:true,width:120,align:'left'},
                       {field:'NotificationType',title:'通知类型',type:'string',bind:{ key:'通知类型',data:[]},width:120,align:'left'},
                       {field:'NotificationLevel',title:'通知级别',type:'string',width:120,align:'left'},
                       {field:'TargetObjectType',title:'通知对象类型',type:'string',bind:{ key:'通知对象类型',data:[]},width:120,hidden:true,require:true,align:'left'},
                       {field:'TargetObjectValue',title:'通知对象',type:'string',width:120,hidden:true,require:true,align:'left'},
                       {field:'TargetObjectText',title:'通知对象',type:'string',width:120,hidden:true,align:'left'},
                       {field:'PublishStatus',title:'发布状态',type:'int',width:120,hidden:true,align:'left'},
                       {field:'NotificationContent',title:'通知内容',type:'string',width:120,require:true,align:'left'},
                       {field:'LinkUrl',title:'跳转地址',type:'string',width:120,hidden:true,align:'left'},
                       {field:'LinkType',title:'跳转类型',type:'string',width:120,hidden:true,align:'left'},
                       {field:'IsFieldTemplate',title:'字段模板',type:'int',bind:{ key:'enable',data:[]},width:120,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:120,hidden:true,align:'left'},
                       {field:'Enable',title:'Enable',type:'int',width:120,hidden:true,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:120,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:120,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:120,require:true,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:120,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:120,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:120,hidden:true,align:'left'}];
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