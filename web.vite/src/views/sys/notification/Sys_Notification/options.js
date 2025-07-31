// *Author：jxx
// *Contact：283591387@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'NotificationId',
        footer: "Foots",
        cnName: '消息通知',
        name: 'Sys_Notification',
        newTabEdit: false,
        url: "/Sys_Notification/",
        sortName: "v"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"NotificationTitle":"","TargetObjectType":"","TargetObjectText":"","LinkUrl":"","IsFieldTemplate":"","NotificationCode":"","BusinessFunction":"","NotificationType":"","NotificationLevel":"","NotificationContent":""};
    const editFormOptions = [[{"title":"通知标题","required":true,"field":"NotificationTitle","colSize":12,"type":"textarea"}],
                              [{"dataKey":"通知对象类型","data":[],"title":"通知对象类型","field":"TargetObjectType","type":"select"},
                               {"title":"通知对象","field":"TargetObjectText"},
                               {"title":"跳转地址","field":"LinkUrl"},
                               {"dataKey":"enable","data":[],"title":"字段模板","field":"IsFieldTemplate","type":"radio"}],
                              [{"title":"通知编码","field":"NotificationCode"},
                               {"title":"业务功能","field":"BusinessFunction"},
                               {"dataKey":"通知类型","data":[],"title":"通知类型","field":"NotificationType","type":"select"},
                               {"title":"通知级别","field":"NotificationLevel"}],
                              [{"title":"通知内容","required":true,"field":"NotificationContent","colSize":12,"type":"editor"}]];
    const searchFormFields = {"NotificationTitle":"","BusinessFunction":"","NotificationType":"","TargetObjectType":"","PublishStatus":"","CreateDate":""};
    const searchFormOptions = [[{"title":"通知标题","field":"NotificationTitle","type":"like"},{"title":"业务功能","field":"BusinessFunction"},{"dataKey":"通知类型","data":[],"title":"通知类型","field":"NotificationType","type":"select"},{"dataKey":"通知对象类型","data":[],"title":"通知对象类型","field":"TargetObjectType","type":"select"},{"dataKey":"enable","data":[],"title":"发布状态","field":"PublishStatus","type":"select"},{"title":"创建时间","field":"CreateDate","type":"datetime"}]];
    const columns = [{field:'NotificationId',title:'主键ID',type:'string',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'NotificationTemplateId',title:'模板ID',type:'string',width:110,hidden:true,align:'left'},
                       {field:'NotificationTitle',title:'通知标题',type:'string',link:true,width:150,require:true,align:'left'},
                       {field:'NotificationCode',title:'通知编码',type:'string',width:80,align:'left'},
                       {field:'BusinessFunction',title:'业务功能',type:'string',width:90,hidden:true,align:'left'},
                       {field:'TableName',title:'业务表',type:'string',width:100,hidden:true,align:'left'},
                       {field:'TableKey',title:'业务表主键',type:'string',width:120,hidden:true,align:'left'},
                       {field:'NotificationType',title:'通知类型',type:'string',bind:{ key:'通知类型',data:[]},width:70,align:'left'},
                       {field:'NotificationLevel',title:'通知级别',type:'string',width:80,align:'left'},
                       {field:'TargetObjectType',title:'通知对象类型',type:'string',bind:{ key:'通知对象类型',data:[]},width:100,align:'left'},
                       {field:'TargetObjectValue',title:'通知对象',type:'string',width:150,hidden:true,align:'left'},
                       {field:'TargetObjectText',title:'通知对象',type:'string',width:120,align:'left'},
                       {field:'PublishStatus',title:'发布状态',type:'int',bind:{ key:'enable',data:[]},width:80,align:'left'},
                       {field:'LinkUrl',title:'跳转地址',type:'string',width:150,hidden:true,align:'left'},
                       {field:'LinkType',title:'跳转类型',type:'string',width:120,hidden:true,align:'left'},
                       {field:'IsFieldTemplate',title:'字段模板',type:'int',bind:{ key:'enable',data:[]},width:110,hidden:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:150,hidden:true,align:'left'},
                       {field:'NotificationContent',title:'通知内容',type:'string',width:110,hidden:true,require:true,align:'left'},
                       {field:'Enable',title:'Enable',type:'int',width:110,hidden:true,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:90,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:130,require:true,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:110,hidden:true,align:'left'}];
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