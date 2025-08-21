// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'uid',
        footer: "Foots",
        cnName: '自定义打印资料',
        name: 'MES_SpecialPrintRequest',
        newTabEdit: false,
        url: "/MES_SpecialPrintRequest/",
        sortName: "uid"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"udf1_key":"","udf1_value":"","udf2_key":"","udf2_value":""};
    const editFormOptions = [[{"title":"自定义列1","field":"udf1_key"}],
                              [{"title":"自定义值1","field":"udf1_value"}],
                              [{"title":"自定义列2","field":"udf2_key"}],
                              [{"title":"自定义值2","field":"udf2_value"}]];
    const searchFormFields = {"udf1_key":"","udf1_value":"","udf2_key":"","udf2_value":""};
    const searchFormOptions = [[{"title":"自定义列1","field":"udf1_key","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"自定义值1","field":"udf1_value","type":"like","comparationList":[{"key":"like","value":"模糊查询(包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]},{"title":"自定义列2","field":"udf2_key"},{"title":"自定义值2","field":"udf2_value"}]];
    const columns = [{field:'uid',title:'uid',type:'int',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'udf1_key',title:'自定义列1',type:'string',link:true,width:110,align:'left'},
                       {field:'udf1_value',title:'自定义值1',type:'string',width:110,align:'left'},
                       {field:'udf2_key',title:'自定义列2',type:'string',width:110,align:'left'},
                       {field:'udf2_value',title:'自定义值2',type:'string',width:110,align:'left'}];
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