
export default function(){
		const table = {
			tableName: "Demo_Goods",
			tableCNName: "商品信息",
			titleField:'GoodsName',
			key: 'GoodsId',
			sortName: "CreateDate"
		}

	    const searchFormFields = {"GoodsName":"","Enable":"","CreateDate":""};
	    const searchFormOptions = [{"title":"商品名称","field":"GoodsName","type":"like"},{"key":"enable","data":[],"title":"是否可用","field":"Enable","type":"select"},{"title":"创建时间","field":"CreateDate"},{"type":"group"}]
        const editFormFields = {"Specs":"","Enable":"","GoodsName":"","CatalogId":[],"GoodsCode":"","Price":"","Img":""};
        const editFormOptions = [{"title":"商品名称","required":true,"field":"GoodsName","type":"text"},
                               {"type":"group"},
                               {"key":"分类级联","data":[],"title":"所属分类","field":"CatalogId","type":"cascader"},
                               {"type":"group"},
                               {"title":"商品编号","required":true,"field":"GoodsCode","type":"text"},
                               {"type":"group"},
                               {"title":"单价","required":true,"field":"Price","type":"decimal"},
                               {"type":"group"},
                               {"title":"商品图片","field":"Img","type":"img"},
                               {"type":"group"},
                               {"title":"规格","field":"Specs"},
                               {"key":"enable","data":[],"title":"是否可用","field":"Enable","type":"radio"}];

		const columns = [{field:'GoodsName',title:'商品名称',type:'string',link:true},
                       {field:'GoodsCode',title:'商品编号',type:'string',align:"left"}, 
                       [{field:'Price',title:'商品单价',type:'decimal'},{field:'Specs',title:'规格',type:'string'}],
					   [{field:'Creator',title:'创建人',type:'string'},
                       {field:'CreateDate',title:'创建时间',type:'date'}],
					   {field:'Img',title:'商品图片',type:'img'}];

        const detail = {columns:[]};
        const details = [];

    return {
        table,
		searchFormFields,
		searchFormOptions,
        editFormFields,
        editFormOptions,
		columns,
		detail,
		details
    }
}