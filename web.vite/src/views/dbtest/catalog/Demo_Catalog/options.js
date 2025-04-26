// *Author：jxx
// *Contact：283591387@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'CatalogId',
        footer: "Foots",
        cnName: '商品分类',
        name: 'Demo_Catalog',
        newTabEdit: false,
        url: "/Demo_Catalog/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"CatalogCode":"","CatalogName":"","ParentId":[],"Enable":"","Remark":"","Img":""};
    const editFormOptions = [[{"title":"分类编号","required":true,"field":"CatalogCode"},
                               {"title":"分类名称","required":true,"field":"CatalogName"}],
                              [{"dataKey":"分类级联","data":[],"title":"上级分类","field":"ParentId","type":"cascader"},
                               {"dataKey":"商品分类可用","data":[],"title":"是否可用","field":"Enable","type":"radio"}],
                              [{"title":"备注","field":"Remark","colSize":12,"type":"textarea"}],
                              [{"title":"分类图片","field":"Img","type":"img"}]];
    const searchFormFields = {"CatalogCode":"","CatalogName":"","Enable":"","CreateDate":""};
    const searchFormOptions = [[{"title":"分类编号","field":"CatalogCode"},{"title":"分类名称","field":"CatalogName","type":"like"},{"dataKey":"商品分类可用","data":[],"title":"是否可用","field":"Enable","type":"select"},{"title":"创建时间","field":"CreateDate","type":"datetime"}]];
    const columns = [{field:'CatalogId',title:'商品分类',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'CatalogCode',title:'分类编号',type:'string',sort:true,width:90,require:true,align:'left'},
                       {field:'CatalogName',title:'分类名称',type:'string',link:true,width:100,require:true,align:'left'},
                       {field:'ParentId',title:'上级分类',type:'guid',bind:{ key:'分类级联',data:[]},sort:true,width:110,align:'left'},
                       {field:'Img',title:'分类图片',type:'img',width:90,align:'left'},
                       {field:'Enable',title:'是否可用',type:'int',bind:{ key:'商品分类可用',data:[]},sort:true,width:110,align:'left'},
                       {field:'Remark',title:'备注',type:'string',sort:true,width:120,align:'left'},
                       {field:'CreateID',title:'创建人id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:130,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:110,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:130,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:110,hidden:true,align:'left'}];
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