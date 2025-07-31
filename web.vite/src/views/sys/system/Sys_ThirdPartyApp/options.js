// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'Id',
        footer: "Foots",
        cnName: '第三方应用配置表',
        name: 'Sys_ThirdPartyApp',
        newTabEdit: false,
        url: "/Sys_ThirdPartyApp/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"AppId":"","AppSecret":"","AppName":"","Description":"","IsEnabled":"","AllowedIPs":"","ExpireTime":"","AccessCount":"","LastAccessTime":""};
    const editFormOptions = [[{"title":"应用ID","required":true,"field":"AppId"},
                               {"title":"应用密钥","required":true,"field":"AppSecret"},
                               {"title":"应用名称","required":true,"field":"AppName"}],
                              [{"title":"应用描述","field":"Description"},
                               {"dataKey":"enable","data":[],"title":"是否启用","required":true,"field":"IsEnabled","type":"select"},
                               {"title":"允许的IP地址","field":"AllowedIPs"}],
                              [{"title":"过期时间","field":"ExpireTime","type":"datetime"},
                               {"title":"访问次数","required":true,"field":"AccessCount"}],
                              [{"title":"最后访问时间","field":"LastAccessTime","type":"datetime"}]];
    const searchFormFields = {"IsEnabled":[],"AllowedIPs":"","ExpireTime":"","AccessCount":"","CreateTime":"","ModifyTime":"","LastAccessTime":""};
    const searchFormOptions = [[{"title":"应用ID","field":"AppId","type":"like"},{"title":"应用密钥","field":"AppSecret","type":"like"},{"title":"应用名称","field":"AppName","type":"like"},{"title":"应用描述","field":"Description","type":"like"}],[{"dataKey":"enable","data":[],"title":"是否启用","field":"IsEnabled","type":"selectList"},{"title":"允许的IP地址","field":"AllowedIPs","type":"like"},{"title":"过期时间","field":"ExpireTime","type":"datetime"},{"title":"访问次数","field":"AccessCount","type":"like"}],[{"title":"创建时间","field":"CreateTime","type":"datetime"},{"title":"修改时间","field":"ModifyTime","type":"datetime"}],[{"title":"最后访问时间","field":"LastAccessTime","type":"datetime"}]];
    const columns = [{field:'Id',title:'主键ID',type:'int',width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'AppId',title:'应用ID',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'AppSecret',title:'应用密钥',type:'string',sort:true,width:220,require:true,align:'left'},
                       {field:'AppName',title:'应用名称',type:'string',sort:true,width:110,require:true,align:'left'},
                       {field:'Description',title:'应用描述',type:'string',sort:true,width:220,align:'left'},
                       {field:'IsEnabled',title:'是否启用',type:'int',bind:{ key:'enable',data:[]},sort:true,width:80,require:true,align:'left'},
                       {field:'AllowedIPs',title:'允许的IP地址',type:'string',sort:true,width:220,align:'left'},
                       {field:'ExpireTime',title:'过期时间',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'AccessCount',title:'访问次数',type:'long',sort:true,width:80,require:true,align:'left'},
                       {field:'CreateTime',title:'创建时间',type:'datetime',sort:true,width:150,require:true,align:'left'},
                       {field:'CreateId',title:'创建人ID',type:'int',sort:true,width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'ModifyTime',title:'修改时间',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'ModifyId',title:'修改人ID',type:'int',sort:true,width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'LastAccessTime',title:'最后访问时间',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:150,align:'left'}];
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