// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: '计划号',
        footer: "Foots",
        cnName: '当月完成BOM情况',
        name: 'vw_OCP_Tech_BOM_Status',
        newTabEdit: false,
        url: "/vw_OCP_Tech_BOM_Status/",
        sortName: "订单日期"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {};
    const editFormOptions = [];
    const searchFormFields = {};
    const searchFormOptions = [];
    const columns = [{field:'计划号',title:'计划号',type:'string',width:150,readonly:true,require:true,align:'left'},
                       {field:'物料代码',title:'物料代码',type:'string',width:150,readonly:true,align:'left'},
                       {field:'产品型号',title:'产品型号',type:'string',width:150,readonly:true,align:'left'},
                       {field:'订单编号',title:'订单编号',type:'string',width:110,readonly:true,align:'left'},
                       {field:'订单日期',title:'订单创建日期',type:'datetime',width:110,readonly:true,align:'left'},
                       {field:'订单审核日期',title:'订单审核日期',type:'datetime',width:110,readonly:true,align:'left'},
                       {field:'TC_BOM创建人',title:'BOM创建人',type:'datetime',width:110,readonly:true,align:'left'},
                       {field:'TC_BOM是否发布',title:'TC_BOM是否发布',type:'int',width:110,hidden:true,readonly:true,align:'left'},
                       {field:'物料名称',title:'物料名称',type:'string',width:150,readonly:true,align:'left'},
                       {field:'公称通径',title:'公称通径',type:'string',width:150,readonly:true,align:'left'},
                       {field:'公称压力',title:'公称压力',type:'string',width:150,readonly:true,align:'left'},
                       {field:'流量特性',title:'流量特性',type:'string',width:150,readonly:true,align:'left'},
                       {field:'填料形式',title:'填料形式',type:'string',width:150,readonly:true,align:'left'},
                       {field:'法兰连接方式',title:'法兰连接方式',type:'string',width:150,readonly:true,align:'left'},
                       {field:'执行机构型号',title:'执行机构型号',type:'string',width:150,readonly:true,align:'left'},
                       {field:'执行机构行程',title:'执行机构行程',type:'string',width:150,readonly:true,align:'left'},
                       {field:'备注',title:'备注',type:'string',width:150,readonly:true,align:'left'},
                       {field:'数量',title:'数量',type:'decimal',width:110,readonly:true,align:'left'},
                       {field:'回复交货日期',title:'回复交货日期',type:'datetime',width:110,readonly:true,align:'left'},
                       {field:'ERP_BOM创建延期天数（bom创建延期天数=bom创建日期-订单审核日期）',title:'ERP_BOM创建延期天数（bom创建延期天数=bom创建日期-订单审核日期）',type:'int',width:80,hidden:true,align:'left'},
                       {field:'ERP_BOM已缺失天数（bom缺失天数=当天-审核日期）',title:'ERP_BOM已缺失天数（bom缺失天数=当天-审核日期）',type:'int',width:80,hidden:true,align:'left'},
                       {field:'TC_BOM创建人',title:'TC_BOM创建人',type:'string',width:110,hidden:true,align:'left'},
                       {field:'TC_BOM是否发布',title:'TC_BOM是否发布',type:'string',width:110,hidden:true,align:'left'}];
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