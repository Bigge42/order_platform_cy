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
    const editFormFields = {"UDF_Value1":"","UDF_Value2":"","UDF_Value3":"","UDF_Value4":"","UDF_Value5":"","UDF_Value6":"","UDF_Value7":"","UDF_Value8":"","UDF_Value9":"","UDF_Value10":"","UDF_Value11":"","UDF_Value12":"","UDF_Value13":"","UDF_Value14":"","UDF_Value15":""};
    const editFormOptions = [[{"title":"自定义值1","field":"UDF_Value1"}],
                              [{"title":"自定义值2","field":"UDF_Value2"}],
                              [{"title":"自定义值3","field":"UDF_Value3"}],
                              [{"title":"自定义值4","field":"UDF_Value4"}],
                              [{"title":"自定义值5","field":"UDF_Value5"}],
                              [{"title":"自定义值6","field":"UDF_Value6"}],
                              [{"title":"自定义值7","field":"UDF_Value7"}],
                              [{"title":"自定义值8","field":"UDF_Value8"}],
                              [{"title":"自定义值9","field":"UDF_Value9"}],
                              [{"title":"自定义值10","field":"UDF_Value10"}],
                              [{"title":"自定义值11","field":"UDF_Value11"}],
                              [{"title":"自定义值12","field":"UDF_Value12"}],
                              [{"title":"自定义值13","field":"UDF_Value13"}],
                              [{"title":"自定义值14","field":"UDF_Value14"}],
                              [{"title":"自定义值15","field":"UDF_Value15"}]];
    const searchFormFields = {"RetrospectCode":""};
    const searchFormOptions = [[{"title":"产品编码(批次号)","field":"RetrospectCode","type":"likeStart","comparationList":[{"key":"likeStart","value":"模糊查询(左包含)"},{"key":"EMPTY","value":"空"},{"key":"NOT_EMPTY","value":"不空"}]}]];
    const columns = [{field:'uid',title:'单据ID',type:'int',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'RetrospectCode',title:'产品编码(批次号)',type:'string',width:220,readonly:true,require:true,align:'left'},
                       {field:'PositionNumber',title:'设计位号',type:'string',width:220,align:'left'},
                       {field:'ProductModel',title:'产品型号',type:'string',width:220,align:'left'},
                       {field:'NominalDiameter',title:'公称通径',type:'string',width:220,align:'left'},
                       {field:'NominalPressure',title:'公称压力',type:'string',width:220,align:'left'},
                       {field:'ValveBodyMaterial',title:'阀体材质',type:'string',width:220,align:'left'},
                       {field:'ActuatorModel',title:'执行机构',type:'string',width:220,align:'left'},
                       {field:'FailPosition',title:'故障位',type:'string',width:220,align:'left'},
                       {field:'AirSupplyPressure',title:'气源压力',type:'string',width:220,align:'left'},
                       {field:'OperatingTemperature',title:'工作温度',type:'string',width:220,align:'left'},
                       {field:'RatedStroke',title:'额定行程',type:'string',width:220,align:'left'},
                       {field:'FlowCharacteristic',title:'流量特性',type:'string',width:220,align:'left'},
                       {field:'FlowCoefficient',title:'流量系数(CV值)',type:'string',width:220,align:'left'},
                       {field:'UDF_Key1',title:'自定义字段1',type:'string',width:220,align:'left'},
                       {field:'UDF_Value1',title:'自定义值1',type:'string',width:220,align:'left'},
                       {field:'UDF_Key2',title:'自定义字段2',type:'string',width:220,align:'left'},
                       {field:'UDF_Value2',title:'自定义值2',type:'string',width:220,align:'left'},
                       {field:'UDF_Key3',title:'自定义字段3',type:'string',width:220,align:'left'},
                       {field:'UDF_Value3',title:'自定义值3',type:'string',width:220,align:'left'},
                       {field:'UDF_Key4',title:'自定义字段4',type:'string',width:220,align:'left'},
                       {field:'UDF_Value4',title:'自定义值4',type:'string',width:220,align:'left'},
                       {field:'UDF_Key5',title:'自定义字段5',type:'string',width:220,align:'left'},
                       {field:'UDF_Value5',title:'自定义值5',type:'string',width:220,align:'left'},
                       {field:'UDF_Key6',title:'自定义字段6',type:'string',width:220,align:'left'},
                       {field:'UDF_Value6',title:'自定义值6',type:'string',width:220,align:'left'},
                       {field:'UDF_Key7',title:'自定义字段7',type:'string',width:220,align:'left'},
                       {field:'UDF_Value7',title:'自定义值7',type:'string',width:220,align:'left'},
                       {field:'UDF_Key8',title:'自定义字段8',type:'string',width:220,align:'left'},
                       {field:'UDF_Value8',title:'自定义值8',type:'string',width:220,align:'left'},
                       {field:'UDF_Key9',title:'自定义字段9',type:'string',width:220,align:'left'},
                       {field:'UDF_Value9',title:'自定义值9',type:'string',width:220,align:'left'},
                       {field:'UDF_Key10',title:'自定义字段10',type:'string',width:220,align:'left'},
                       {field:'UDF_Value10',title:'自定义值10',type:'string',width:220,align:'left'},
                       {field:'UDF_Key11',title:'自定义字段11',type:'string',width:220,align:'left'},
                       {field:'UDF_Value11',title:'自定义值11',type:'string',width:220,align:'left'},
                       {field:'UDF_Key12',title:'自定义字段12',type:'string',width:220,align:'left'},
                       {field:'UDF_Value12',title:'自定义值12',type:'string',width:220,align:'left'},
                       {field:'UDF_Key13',title:'自定义字段13',type:'string',width:220,align:'left'},
                       {field:'UDF_Value13',title:'自定义值13',type:'string',width:220,align:'left'},
                       {field:'UDF_Key14',title:'自定义字段14',type:'string',width:220,align:'left'},
                       {field:'UDF_Value14',title:'自定义值14',type:'string',width:220,align:'left'},
                       {field:'UDF_Key15',title:'自定义字段15',type:'string',width:220,align:'left'},
                       {field:'UDF_Value15',title:'自定义值15',type:'string',width:220,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:150,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:150,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
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