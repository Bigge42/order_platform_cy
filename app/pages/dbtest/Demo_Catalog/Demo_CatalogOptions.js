
export default function(){
		const table = {
			tableName: "Demo_Catalog",
			tableCNName: "商品分类",
			titleField:'CatalogName',
			key: 'CatalogId',
			sortName: "CreateDate"
		}

	    const searchFormFields = {"CatalogCode":"","CatalogName":"","Enable":"","Creator":"","CreateDate":""};
	    const searchFormOptions = [{"title":"创建人","field":"Creator"},{"type":"group"},{"title":"分类编号","field":"CatalogCode"},{"title":"分类名称","field":"CatalogName","type":"like"},{"key":"商品分类可用","data":[],"title":"是否可用","field":"Enable","type":"select"},{"title":"创建时间","field":"CreateDate","type":"datetime"}]
        const editFormFields = {"CatalogCode":"","CatalogName":"","ParentId":[],"Enable":"","Remark":"","Img":""};
        const editFormOptions = [{"title":"分类编号","required":true,"field":"CatalogCode"},
                               {"title":"分类名称","required":true,"field":"CatalogName"},
                               {"type":"group"},
                               {"key":"分类级联","data":[],"title":"上级分类","field":"ParentId","type":"cascader"},
                               {"key":"商品分类可用","data":[],"title":"是否可用","field":"Enable","type":"radio"},
                               {"type":"group"},
                               {"title":"备注","field":"Remark","type":"textarea"},
                               {"type":"group"},
                               {"title":"分类图片","field":"Img","type":"img"}];

		const columns = [{field:'CatalogCode',title:'分类编号',type:'string'},
                       {field:'CatalogName',title:'分类名称',type:'string',link:true},
                       {field:'ParentId',title:'上级分类',type:'guid',bind:{ key:'分类级联',data:[]}},
                       {field:'Enable',title:'是否可用',type:'int',bind:{ key:'商品分类可用',data:[]}},
                       {field:'Remark',title:'备注',type:'string'},
					   {field:'Img',title:'分类图片',type:'img'},
                       {field:'Creator',title:'创建人',type:'string'},
                       {field:'CreateDate',title:'创建时间',type:'datetime'}];

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