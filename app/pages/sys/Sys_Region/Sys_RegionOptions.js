
export default function(){
		const table = {
			tableName: "Sys_Region",
			tableCNName: "省市区县",
			titleField:'',
			key: 'id',
			sortName: "id"
		}

	    const searchFormFields = {"code":"","name":"","parentId":"","Lng":"","Lat":"","pinyin":""};
	    const searchFormOptions = [{"title":"编码","field":"code","type":"like"},{"title":"上级编码","field":"parentId","type":"number"},{"title":"经度","field":"Lng","type":"decimal"},{"title":"纬度","field":"Lat","type":"decimal"},{"type":"group"},{"title":"名称","field":"name"},{"title":"拼音","field":"pinyin","type":"like"}]
        const editFormFields = {"code":"","name":"","parentId":"","level":"","mername":"","pinyin":"","Lng":"","Lat":""};
        const editFormOptions = [{"title":"编码","field":"code"},
                               {"title":"名称","field":"name"},
                               {"title":"上级编码","field":"parentId","type":"number"},
                               {"type":"group"},
                               {"title":"级别","field":"level","type":"number"},
                               {"title":"完整地址","field":"mername"},
                               {"title":"拼音","field":"pinyin"},
                               {"type":"group"},
                               {"title":"经度","field":"Lng","type":"decimal"},
                               {"title":"纬度","field":"Lat","type":"decimal"}];

		const columns = [{field:'code',title:'编码',type:'string',width:70},
                       {field:'name',title:'名称',type:'string',width:100},
                       {field:'parentId',title:'上级编码',type:'int',width:70},
                       {field:'level',title:'级别',type:'int',width:60},
                       {field:'mername',title:'完整地址',type:'string',width:170},
                       {field:'Lng',title:'经度',type:'float',width:70},
                       {field:'Lat',title:'纬度',type:'float',width:70},
                       {field:'pinyin',title:'拼音',type:'string',width:120}];

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