// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ID',
        footer: "Foots",
        cnName: '物料表',
        name: 'OCP_Material',
        newTabEdit: false,
        url: "/OCP_Material/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"MaterialName":"","SpecModel":"","ProductModel":"","NominalDiameter":"","NominalPressure":"","CV":"","Accessories":"","DrawingNo":"","Material":"","ErpClsid":"","Warehouse":"","Workshop":"","IsRelatedBOM":"","BasicUnit":"","PackingForm":"","FlowCharacteristic":"","FlangeConnection":"","ActuatorModel":"","ActuatorStroke":""};
    const editFormOptions = [[{"title":"名称","field":"MaterialName"},
                               {"title":"规格型号","field":"SpecModel"},
                               {"title":"产品型号","field":"ProductModel"}],
                              [{"title":"公称通径","field":"NominalDiameter"},
                               {"title":"公称压力","field":"NominalPressure"},
                               {"title":"CV","field":"CV"}],
                              [{"title":"附件","field":"Accessories"},
                               {"title":"图号","field":"DrawingNo"},
                               {"title":"材质","field":"Material"}],
                              [{"title":"物料属性","field":"ErpClsid"},
                               {"title":"库房","field":"Warehouse"},
                               {"title":"车间","field":"Workshop"}],
                              [{"title":"填料形式","field":"PackingForm"},
                               {"dataKey":"enable","data":[],"title":"是否关联BOM","field":"IsRelatedBOM","type":"select"},
                               {"title":"基本单位","field":"BasicUnit"}],
                              [{"title":"流量特性","field":"FlowCharacteristic"},
                               {"title":"法兰连接方式","field":"FlangeConnection"},
                               {"title":"执行机构型号","field":"ActuatorModel"}],
                              [{"title":"执行机构行程","field":"ActuatorStroke"}]];
    const searchFormFields = {"ProductModel":"","NominalDiameter":"","NominalPressure":"","CV":"","Accessories":"","DrawingNo":"","Material":"","ErpClsid":"","Warehouse":"","Workshop":"","IsRelatedBOM":[],"BasicUnit":"","PackingForm":"","FlowCharacteristic":"","FlangeConnection":"","ActuatorModel":"","ActuatorStroke":""};
    const searchFormOptions = [[{"title":"编码","field":"MaterialCode","type":"like"},{"title":"名称","field":"MaterialName","type":"like"},{"title":"规格型号","field":"SpecModel","type":"like"}],[{"title":"产品型号","field":"ProductModel","type":"like"},{"title":"公称通径","field":"NominalDiameter","type":"like"},{"title":"公称压力","field":"NominalPressure","type":"like"},{"title":"基本单位","field":"BasicUnit","type":"like"}],[{"title":"CV","field":"CV","type":"like"},{"title":"附件","field":"Accessories","type":"like"},{"title":"图号","field":"DrawingNo","type":"like"},{"title":"材质","field":"Material","type":"like"}],[{"title":"物料属性","field":"ErpClsid","type":"like"},{"title":"库房","field":"Warehouse","type":"like"},{"title":"车间","field":"Workshop","type":"like"},{"dataKey":"enable","data":[],"title":"是否关联BOM","field":"IsRelatedBOM","type":"selectList"}],[{"title":"填料形式","field":"PackingForm","type":"like"},{"title":"流量特性","field":"FlowCharacteristic","type":"like"},{"title":"法兰连接方式","field":"FlangeConnection","type":"like"},{"title":"执行机构型号","field":"ActuatorModel","type":"like"},{"title":"执行机构行程","field":"ActuatorStroke","type":"like"}]];
    const columns = [{field:'ID',title:'主键ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',sort:true,width:80,require:true,align:'left'},
                       {field:'MaterialCode',title:'编码',type:'string',sort:true,width:220,align:'left'},
                       {field:'MaterialName',title:'名称',type:'string',sort:true,width:220,align:'left'},
                       {field:'SpecModel',title:'规格型号',type:'string',sort:true,width:220,align:'left'},
                       {field:'ProductModel',title:'产品型号',type:'string',sort:true,width:220,align:'left'},
                       {field:'NominalDiameter',title:'公称通径',type:'string',sort:true,width:220,align:'left'},
                       {field:'NominalPressure',title:'公称压力',type:'string',sort:true,width:220,align:'left'},
                       {field:'CV',title:'CV',type:'string',sort:true,width:220,align:'left'},
                       {field:'Accessories',title:'附件',type:'string',sort:true,width:220,align:'left'},
                       {field:'DrawingNo',title:'图号',type:'string',sort:true,width:120,align:'left'},
                       {field:'Material',title:'材质',type:'string',sort:true,width:120,align:'left'},
                       {field:'ErpClsid',title:'物料属性',type:'string',sort:true,width:120,align:'left'},
                       {field:'Warehouse',title:'库房',type:'string',sort:true,width:120,align:'left'},
                       {field:'Workshop',title:'车间',type:'string',sort:true,width:120,align:'left'},
                       {field:'IsRelatedBOM',title:'是否关联BOM',type:'int',bind:{ key:'enable',data:[]},sort:true,width:80,align:'left'},
                       {field:'BasicUnit',title:'基本单位',type:'string',sort:true,width:110,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'PackingForm',title:'填料形式',type:'string',width:220,align:'left'},
                       {field:'FlowCharacteristic',title:'流量特性',type:'string',width:220,align:'left'},
                       {field:'FlangeConnection',title:'法兰连接方式',type:'string',width:220,align:'left'},
                       {field:'ActuatorModel',title:'执行机构型号',type:'string',width:220,align:'left'},
                       {field:'ActuatorStroke',title:'执行机构行程',type:'string',width:220,align:'left'},
                       {field:'ERPModifyDate',title:'ERP修改日期',type:'datetime',width:150,align:'left'}];
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