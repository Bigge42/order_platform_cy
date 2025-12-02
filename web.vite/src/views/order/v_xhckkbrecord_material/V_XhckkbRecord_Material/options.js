// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'FENTRYID',
        footer: "Foots",
        cnName: '循环仓库物料版',
        name: 'V_XhckkbRecord_Material',
        newTabEdit: false,
        url: "/V_XhckkbRecord_Material/",
        sortName: "FENTRYID"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {};
    const editFormOptions = [];
    const searchFormFields = {};
    const searchFormOptions = [];
    const columns = [{field:'FENTRYID',title:'FENTRYID',type:'int',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'FBILLNO',title:'FBILLNO',type:'string',width:110,hidden:true,readonly:true,align:'left'},
                       {field:'FDOCUMENTSTATUSNAME',title:'FDOCUMENTSTATUSNAME',type:'string',width:110,hidden:true,readonly:true,align:'left'},
                       {field:'F_ORA_LB',title:'物资大类',type:'string',width:110,readonly:true,align:'left'},
                       {field:'FMATERIALID',title:'FMATERIALID',type:'int',width:110,hidden:true,readonly:true,align:'left'},
                       {field:'F_ORA_INTEGER',title:'一次排数量',type:'int',sort:true,width:110,readonly:true,align:'left'},
                       {field:'FSeq',title:'行号',type:'int',sort:true,width:110,readonly:true,align:'left'},
                       {field:'F_ORA_INTEGER1',title:'最低库存',type:'int',sort:true,width:110,readonly:true,align:'left'},
                       {field:'F_ORA_INTEGER2',title:'最高库存',type:'int',sort:true,width:110,readonly:true,align:'left'},
                       {field:'F_ORA_WZXL',title:'物资小类',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT1',title:'负责人',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_JJSP',title:'建议SP',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT3',title:'管理编码',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT4',title:'库存策略',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT5',title:'供方库存',type:'string',sort:true,width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT6',title:'供货周期',type:'string',sort:true,width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT7',title:'年预计用量',type:'string',sort:true,width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT8',title:'产品大类',type:'string',width:110,readonly:true,align:'left'},
                       {field:'F_ORA_TEXT9',title:'产品小类',type:'string',width:110,readonly:true,align:'left'},
                       {field:'CreateDate',title:'CreateDate',type:'string',width:110,hidden:true,align:'left'},
                       {field:'CreateID',title:'CreateID',type:'string',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'Creator',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'Modifier',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'ModifyDate',type:'string',width:110,hidden:true,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'string',width:80,hidden:true,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',sort:true,width:150,readonly:true,align:'left'},
                       {field:'MaterialName',title:'物料名称',type:'string',width:150,readonly:true,align:'left'},
                       {field:'SpecModel',title:'规格型号',type:'string',width:150,readonly:true,align:'left'},
                       {field:'ProductModel',title:'产品型号',type:'string',width:150,readonly:true,align:'left'},
                       {field:'NominalDiameter',title:'公称通径',type:'string',width:150,readonly:true,align:'left'},
                       {field:'NominalPressure',title:'公称压力',type:'string',width:150,readonly:true,align:'left'},
                       {field:'CV',title:'CV',type:'string',width:150,readonly:true,align:'left'},
                       {field:'Accessories',title:'附件',type:'string',width:150,readonly:true,align:'left'},
                       {field:'DrawingNo',title:'图号',type:'string',width:150,readonly:true,align:'left'},
                       {field:'Material',title:'材质',type:'string',width:150,readonly:true,align:'left'},
                       {field:'ErpClsid',title:'物料属性',type:'string',width:110,readonly:true,align:'left'}];
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