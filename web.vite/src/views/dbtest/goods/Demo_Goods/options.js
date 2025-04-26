// *Author：jxx
// *Contact：283591387@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'GoodsId',
        footer: "Foots",
        cnName: '商品信息',
        name: 'Demo_Goods',
        newTabEdit: true,
        url: "/Demo_Goods/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = true;
    const key = table.key;
    const editFormFields = {"GoodsName":"","CatalogId":[],"GoodsCode":"","Price":"","Remark":"","Img":""};
    const editFormOptions = [[{"title":"商品名称","required":true,"field":"GoodsName","type":"text"}],
                              [{"dataKey":"分类级联","data":[],"title":"所属分类","field":"CatalogId","type":"cascader"}],
                              [{"title":"商品编号","required":true,"field":"GoodsCode","type":"text"}],
                              [{"title":"单价","required":true,"field":"Price","type":"decimal"}],
                              [{"title":"备注","field":"Remark","colSize":12,"type":"textarea"}],
                              [{"title":"商品图片","field":"Img","type":"img"}]];
    const searchFormFields = {"GoodsName":"","CatalogId":[],"GoodsCode":""};
    const searchFormOptions = [[{"title":"商品名称","field":"GoodsName","type":"like"},{"dataKey":"分类级联","data":[],"title":"所属分类","field":"CatalogId","type":"cascader"},{"title":"商品编号","field":"GoodsCode"}]];
    const columns = [{field:'GoodsId',title:'商品ID',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'GoodsName',title:'商品名称',type:'string',link:true,sort:true,width:120,require:true,align:'left'},
                       {field:'CatalogId',title:'所属分类',type:'guid',bind:{ key:'分类级联',data:[]},width:100,align:'left'},
                       {field:'GoodsCode',title:'商品编号',type:'string',sort:true,width:100,require:true,align:'left'},
                       {field:'Img',title:'商品图片',type:'img',width:60,align:'left'},
                       {field:'Specs',title:'规格',type:'string',width:60,align:'left'},
                       {field:'Price',title:'单价',type:'decimal',sort:true,width:70,require:true,align:'left'},
                       {field:'Enable',title:'是否可用',type:'int',bind:{ key:'enable',data:[]},width:80,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:100,hidden:true,align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:70,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:130,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:130,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'ModifyDate',type:'datetime',width:110,hidden:true,align:'left'}];
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