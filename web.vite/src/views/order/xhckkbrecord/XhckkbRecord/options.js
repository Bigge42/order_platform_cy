// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'FENTRYID',
        footer: "Foots",
        cnName: '循环仓库看板',
        name: 'XhckkbRecord',
        newTabEdit: false,
        url: "/XhckkbRecord/",
        sortName: "FENTRYID"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {};
    const editFormOptions = [];
    const searchFormFields = {"FBILLNO":"","FDOCUMENTSTATUSNAME":"","F_ORA_LB":"","FMATERIALID":"","F_ORA_TEXT1":"","F_ORA_TEXT4":""};
    const searchFormOptions = [[{"title":"单据编号","field":"FBILLNO","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"单据审核状态","field":"FDOCUMENTSTATUSNAME","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"物资大类","field":"F_ORA_LB","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"物料ID","field":"FMATERIALID","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}],[{"title":"负责人","field":"F_ORA_TEXT1","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"库存策略","field":"F_ORA_TEXT4","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}]];
    const columns = [{field:'FENTRYID',title:'订单明细主键',type:'int',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'FBILLNO',title:'单据编号',type:'string',width:110,readonly:true,align:'left'},
                       {field:'FDOCUMENTSTATUSNAME',title:'单据审核状态',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_LB',title:'物资大类',type:'string',width:110,readonly:true,align:'left'},
                       {field:'FMATERIALID',title:'物料ID',type:'int',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_INTEGER',title:'一次排的数量',type:'int',width:110,readonly:true,align:'left'},
                       {field:'FSeq',title:'行号',type:'int',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_INTEGER1',title:'最低库存',type:'int',sort:true,width:110,readonly:true,align:'left'},
                       {field:'F_ORA_INTEGER2',title:'最高库存',type:'int',sort:true,width:110,readonly:true,align:'left'},
                       {field:'F_ORA_WZXL',title:'物资小类',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT1',title:'负责人',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_JJSP',title:'建议SP',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT3',title:'管理编码',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT4',title:'库存策略',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT5',title:'供方库存',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT6',title:'供货周期',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT7',title:'年预计用量',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT8',title:'产品大类',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT9',title:'产品小类',type:'string',width:110,readonly:true,align:'left'}];
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