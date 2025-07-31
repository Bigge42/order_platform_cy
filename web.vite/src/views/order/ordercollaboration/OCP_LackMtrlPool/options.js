// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ID',
        footer: "Foots",
        cnName: '缺料运算队列',
        name: 'OCP_LackMtrlPool',
        newTabEdit: false,
        url: "/OCP_LackMtrlPool/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"ComputeID":"","MtoNo":"","Status":""};
    const editFormOptions = [[{"title":"运算ID","required":true,"field":"ComputeID"}],
                              [{"title":"计划跟踪号","required":true,"field":"MtoNo"}],
                              [{"dataKey":"运算状态","data":[],"title":"运算状态","required":true,"field":"Status","type":"select"}]];
    const searchFormFields = {"ComputeID":"","MtoNo":"","Status":[],"CreateDate":"","Creator":""};
    const searchFormOptions = [[{"title":"运算ID","field":"ComputeID"},{"title":"计划跟踪号","field":"MtoNo","type":"like"},{"dataKey":"运算状态","data":[],"title":"运算状态","field":"Status","type":"selectList"}],[{"title":"运算时间","field":"CreateDate","type":"datetime"},{"title":"运算人","field":"Creator","type":"like"}]];
    const columns = [{field:'ID',title:'主键',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ComputeID',title:'运算ID',type:'long',sort:true,width:80,require:true,align:'left'},
                       {field:'MtoNo',title:'计划跟踪号',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'Status',title:'运算状态',type:'int',bind:{ key:'运算状态',data:[]},sort:true,width:80,require:true,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'运算时间',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'运算人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'}];
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