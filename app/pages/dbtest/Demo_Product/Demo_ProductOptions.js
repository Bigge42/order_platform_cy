
export default function(){
		const table = {
			tableName: "Demo_Product",
			tableCNName: "产品管理",
			titleField:'ProductName',
			key: 'ProductId',
			sortName: "CreateDate"
		}

	    const searchFormFields = {"ProductName":"","ProductCode":"","AuditStatus":"","Creator":"","CreateDate":""};
	    const searchFormOptions = [{"title":"创建人","field":"Creator"},{"type":"group"},{"title":"商品名称","field":"ProductName","type":"like"},{"title":"商品编号","field":"ProductCode","type":"like"},{"key":"audit","data":[],"title":"审核状态","field":"AuditStatus","type":"select"},{"title":"创建时间","field":"CreateDate","type":"datetime"}]
        const editFormFields = {"ProductName":"","ProductCode":"","Price":""};
        const editFormOptions = [
                               {"title":"商品名称","required":true,"field":"ProductName"},
                               {"title":"商品编号","required":true,"field":"ProductCode"},
							   {"type":"group"},
                               {"title":"价格","required":true,"field":"Price","type":"number"}];

		const columns = [{field:'ProductName',title:'商品名称',type:'string',link:true,width:200},
                       {field:'ProductCode',title:'商品编号',type:'string',width:120},
                       {field:'Price',title:'价格',type:'decimal',width:60},
                       {field:'AuditStatus',title:'审核状态',type:'int',bind:{ key:'audit',data:[]},width:80},
                       {field:'Remark',title:'备注',type:'string',width:80},
                       {field:'Creator',title:'创建人',type:'string',width:80,readonly:true},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:140,readonly:true}];

        const detail = {columns:[]};
        const details = [  { 
                    cnName: '产品颜色',
                    table: 'Demo_ProductColor',
                    columns: [{field:'ProductColorId',title:'ProductColorId',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ProductId',title:'商品id',type:'guid',width:110,hidden:true,align:'left'},
                       {field:'Color',title:'颜色',type:'string',bind:{ key:'颜色',data:[]},width:70,edit:{type:'radio'},require:true,align:'left'},
                       {field:'Qty',title:'数量',type:'string',link:true,width:70,edit:{type:''},require:true,align:'left'},
                       {field:'Unit',title:'单位',type:'string',bind:{ key:'单位',data:[]},width:70,edit:{type:'select'},require:true,align:'left'},
                       {field:'Img',title:'图片',type:'img',width:70,edit:{type:'img'},require:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:100,edit:{type:''},align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:110,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:110,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:200,align:'left'}],
                    sortName: 'CreateDate',
                    key: 'ProductColorId',
                    buttons:[],
                    delKeys:[],
                    detail:null
                                            },
                    { 
                    cnName: '尺寸列表',
                    table: 'Demo_ProductSize',
                    columns: [{field:'ProductSizeId',title:'ProductSizeId',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ProductId',title:'商品id',type:'guid',width:110,hidden:true,align:'left'},
                       {field:'Size',title:'尺寸',type:'string',bind:{ key:'尺寸',data:[]},width:70,edit:{type:'select'},require:true,align:'left'},
                       {field:'Unit',title:'单位',type:'string',bind:{ key:'单位',data:[]},width:70,edit:{type:'select'},require:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',link:true,width:100,edit:{type:''},align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:120,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:120,align:'left'}
                     ],
                    sortName: 'CreateDate',
                    key: 'ProductSizeId',
                    buttons:[],
                    delKeys:[],
                    detail:null
                                            }];

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