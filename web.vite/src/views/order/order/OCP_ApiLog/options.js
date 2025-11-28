// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ID',
        footer: "Foots",
        cnName: '接口日志表',
        name: 'OCP_ApiLog',
        newTabEdit: false,
        url: "/OCP_ApiLog/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"ApiName":"","ApiPath":"","HttpMethod":"","RequestParams":"","ResponseLength":"","StatusCode":"","Status":"","DataCount":"","ErrorMessage":"","StartTime":"","EndTime":"","ElapsedMs":"","ResponseResult":"","Remark":""};
    const editFormOptions = [[{"title":"请求方法","field":"HttpMethod","disabled":true},
                               {"title":"接口名称","field":"ApiName","disabled":true},
                               {"title":"接口路径","field":"ApiPath","disabled":true,"colSize":6}],
                              [{"title":"请求参数","field":"RequestParams","disabled":true,"colSize":12,"type":"textarea","comparationList":[{"key":"textarea","value":"textarea"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"title":"HTTP状态码","field":"StatusCode","disabled":true,"type":"number","comparationList":[{"key":"number","value":"number"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"dataKey":"成功与否","data":[],"title":"调用状态","field":"Status","disabled":true,"type":"select","comparationList":[{"key":"select","value":"select"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"响应数据长度（字节数）","field":"ResponseLength","disabled":true},
                               {"title":"返回数据条数","field":"DataCount","disabled":true,"type":"number","comparationList":[{"key":"number","value":"number"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"title":"错误信息","field":"ErrorMessage","disabled":true,"colSize":12,"type":"textarea","comparationList":[{"key":"textarea","value":"textarea"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"title":"调用开始时间","field":"StartTime","disabled":true,"type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"调用结束时间","field":"EndTime","disabled":true,"type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"耗时（毫秒）","field":"ElapsedMs","disabled":true}],
                              [{"title":"返回结果","field":"ResponseResult","disabled":true,"colSize":12,"type":"textarea","comparationList":[{"key":"textarea","value":"textarea"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}],
                              [{"title":"备注","field":"Remark","disabled":true,"colSize":12,"type":"textarea","comparationList":[{"key":"textarea","value":"textarea"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]}]];
    const searchFormFields = {"RequestParams":"","ResponseLength":"","StatusCode":"","Status":[],"ErrorMessage":"","StartTime":"","EndTime":"","ElapsedMs":"","DataCount":"","ResponseResult":"","Remark":""};
    const searchFormOptions = [[{"title":"主键ID","field":"ID"},{"title":"接口名称","field":"ApiName","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"接口路径","field":"ApiPath","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"请求方法","field":"HttpMethod","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}],[{"title":"请求参数","field":"RequestParams","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"响应数据长度（字节数）","field":"ResponseLength"},{"title":"HTTP状态码","field":"StatusCode","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"dataKey":"成功与否","data":[],"title":"调用状态","field":"Status","type":"selectList","comparationList":[{"key":"selectList","value":"select多选"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}],[{"title":"错误信息","field":"ErrorMessage","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"调用开始时间","field":"StartTime","type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"调用结束时间","field":"EndTime","type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"耗时（毫秒）","field":"ElapsedMs"}],[{"title":"返回结果","field":"ResponseResult","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"返回数据条数","field":"DataCount","type":"number","comparationList":[{"key":"number","value":"number"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"备注","field":"Remark","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}]];
    const columns = [{field:'ID',title:'主键ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ApiName',title:'接口名称',type:'string',sort:true,width:220,readonly:true,align:'left'},
                       {field:'ApiPath',title:'接口路径',type:'string',sort:true,width:220,readonly:true,align:'left'},
                       {field:'HttpMethod',title:'请求方法',type:'string',sort:true,width:110,readonly:true,align:'left'},
                       {field:'RequestParams',title:'请求参数',type:'string',sort:true,width:110,readonly:true,align:'left'},
                       {field:'ResponseLength',title:'响应数据长度（字节数）',type:'long',sort:true,width:80,readonly:true,align:'left'},
                       {field:'StatusCode',title:'HTTP状态码',type:'int',sort:true,width:80,readonly:true,align:'left'},
                       {field:'Status',title:'调用状态',type:'int',bind:{ key:'成功与否',data:[]},sort:true,width:80,readonly:true,align:'left'},
                       {field:'ErrorMessage',title:'错误信息',type:'string',sort:true,width:110,readonly:true,align:'left'},
                       {field:'StartTime',title:'调用开始时间',type:'datetime',sort:true,width:150,readonly:true,align:'left'},
                       {field:'EndTime',title:'调用结束时间',type:'datetime',sort:true,width:150,readonly:true,align:'left'},
                       {field:'ElapsedMs',title:'耗时（毫秒）',type:'long',sort:true,width:80,readonly:true,align:'left'},
                       {field:'DataCount',title:'返回数据条数',type:'int',sort:true,width:80,readonly:true,align:'left'},
                       {field:'ResponseResult',title:'返回结果',type:'string',width:110,readonly:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',sort:true,width:220,readonly:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
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