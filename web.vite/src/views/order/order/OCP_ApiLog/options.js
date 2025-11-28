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
    const editFormFields = {"HttpMethod":"","RequestParams":"","ResponseLength":"","StatusCode":"","Status":"","ErrorMessage":"","StartTime":"","EndTime":"","ElapsedMs":"","DataCount":"","Remark":""};
    const editFormOptions = [[{"title":"请求方法","field":"HttpMethod"},
                               {"title":"请求参数","field":"RequestParams"},
                               {"title":"响应数据长度（字节数）","field":"ResponseLength"}],
                              [{"title":"HTTP状态码","field":"StatusCode","type":"number","comparationList":[{"key":"number","value":"number"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"调用状态","field":"Status","type":"number","comparationList":[{"key":"number","value":"number"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"错误信息","field":"ErrorMessage"}],
                              [{"title":"调用开始时间","field":"StartTime","type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"调用结束时间","field":"EndTime","type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"耗时（毫秒）","field":"ElapsedMs"}],
                              [{"title":"返回数据条数","field":"DataCount","type":"number","comparationList":[{"key":"number","value":"number"},
                               {"key":"EMPTY","value":"空"},
                               {"key":"NOT_EMPTY","value":"不空"}]},
                               {"title":"备注","field":"Remark"}]];
    const searchFormFields = {"RequestParams":"","ResponseLength":"","StatusCode":"","Status":"","ErrorMessage":"","StartTime":"","EndTime":"","ElapsedMs":"","DataCount":"","Remark":""};
    const searchFormOptions = [[{"title":"主键ID","field":"ID","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"接口名称","field":"ApiName","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"接口路径","field":"ApiPath","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"请求方法","field":"HttpMethod","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}],[{"title":"请求参数","field":"RequestParams","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"响应数据长度（字节数）","field":"ResponseLength","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"HTTP状态码","field":"StatusCode","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"调用状态","field":"Status","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}],[{"title":"错误信息","field":"ErrorMessage","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"调用开始时间","field":"StartTime","type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"调用结束时间","field":"EndTime","type":"datetime","comparationList":[{"key":"datetime","value":"datetime(年月日时分秒)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"耗时（毫秒）","field":"ElapsedMs","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}],[{"title":"返回数据条数","field":"DataCount","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"备注","field":"Remark","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}]];
    const columns = [{field:'ID',title:'主键ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ApiName',title:'接口名称',type:'string',sort:true,width:200,align:'left'},
                       {field:'StartTime',title:'调用时间',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'Status',title:'状态',type:'int',sort:true,width:80,align:'center',
                        formatter: (row, value, column) => {
                            return value === 1 ? '<span style="color: green;">成功</span>' : '<span style="color: red;">失败</span>';
                        }},
                       {field:'ElapsedMs',title:'耗时(ms)',type:'long',sort:true,width:100,align:'right',
                        formatter: (row, value, column) => {
                            if (value > 10000) return `<span style="color: red;">${value}</span>`;
                            if (value > 5000) return `<span style="color: orange;">${value}</span>`;
                            return value;
                        }},
                       {field:'DataCount',title:'数据条数',type:'int',sort:true,width:100,align:'right'},
                       {field:'ResponseLength',title:'响应大小(字节)',type:'long',sort:true,width:120,align:'right',
                        formatter: (row, value, column) => {
                            if (!value) return '0';
                            if (value > 1024 * 1024) return `${(value / 1024 / 1024).toFixed(2)} MB`;
                            if (value > 1024) return `${(value / 1024).toFixed(2)} KB`;
                            return `${value} B`;
                        }},
                       {field:'StatusCode',title:'HTTP状态码',type:'int',sort:true,width:100,align:'center'},
                       {field:'HttpMethod',title:'请求方法',type:'string',sort:true,width:100,align:'center'},
                       {field:'ApiPath',title:'接口路径',type:'string',sort:true,width:300,align:'left'},
                       {field:'ErrorMessage',title:'错误信息',type:'string',sort:true,width:200,align:'left'},
                       {field:'RequestParams',title:'请求参数',type:'string',sort:true,width:200,align:'left',hidden:true},
                       {field:'EndTime',title:'结束时间',type:'datetime',sort:true,width:150,align:'left',hidden:true},
                       {field:'Remark',title:'备注',type:'string',sort:true,width:150,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left',hidden:true},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left',hidden:true},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left',hidden:true},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left',hidden:true},
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