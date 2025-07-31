/*这是生成的配置信息,任何修改都会被生成覆盖,如果需要修改,请在生成OCP_SOProgress.vue中修改,searchFormOptions、editFormOptions、columns属性
Author:vol
 QQ:461857658
 Date:2024
*/
export default function(){
		const table = {
			tableName: "OCP_SOProgress",
			tableCNName: "销售订单进度查询",
			titleField:'',
			key: 'OrderID',
			sortName: "CreateDate"
		}

	    const searchFormFields = {"BillNo":"","SalesContractNumber":"","CustomerContractNumber":"","Customer":""};
	    const searchFormOptions = [{"title":"销售订单号","field":"BillNo","type":"like"},{"title":"销售合同号","field":"SalesContractNumber","type":"like"},{"title":"用户合同号","field":"CustomerContractNumber","type":"like"},{"type":"group"},{"title":"客户","field":"Customer","type":"like"}]
        const editFormFields = {};
        const editFormOptions = [];

		const columns = [{field:'BillNo',title:'销售订单号',type:'string',width:110},
                       {field:'SalesContractNumber',title:'销售合同号',type:'string',width:110},
                       {field:'CustomerContractNumber',title:'用户合同号',type:'string',width:110},
                       {field:'Customer',title:'客户',type:'string',width:110},
                       {field:'SaleQty',title:'数量',type:'decimal',width:110},
                       {field:'InstockQty',title:'入库数量',type:'decimal',width:110},
                       {field:'UnInstockQty',title:'待入库数量',type:'decimal',width:110},
                       {field:'OutStockQty',title:'出库数量',type:'decimal',width:110},
                       {field:'ProjectName',title:'项目名称',type:'string',width:220}];

        const detail =   {
                    cnName: '销售订单进度详情',
                    table: 'OCP_SOProgressDetail',
                    columns: [{field:'FBILLNO',title:'销售订单号',type:'string',sort:true,width:120,align:'left'},
                       {field:'F_BLN_CONTACTNONAME',title:'销售合同号',type:'string',sort:true,width:120,align:'left'},
                       {field:'F_ORA_XMMC',title:'项目名称',type:'string',sort:true,width:200,align:'left'},
                       {field:'FCUSTName',title:'客户名称',type:'string',sort:true,width:150,align:'left'},
                       {field:'FQTY',title:'订货台数',type:'decimal',sort:true,width:100,align:'right'},
                       {field:'FZJQTY',title:'整机台数',type:'decimal',sort:true,width:100,align:'right'},
                       {field:'FBJQTY',title:'备件台数',type:'decimal',sort:true,width:100,align:'right'},
                       {field:'FDGQTY',title:'单供台数',type:'decimal',sort:true,width:100,align:'right'},
                       {field:'F_ORA_DATE4',title:'合同签订',type:'datetime',sort:true,width:150,align:'center'},
                       {field:'F_ORA_DATE1',title:'排产日期',type:'datetime',sort:true,width:150,align:'center'},
                       {field:'FRKREALQTY',title:'入库数量',type:'decimal',sort:true,width:100,align:'right'},
                       {field:'FSQTY',title:'待入库数量',type:'decimal',sort:true,width:110,align:'right'},
                       {field:'F_BLN_YHHTH',title:'用户合同号',type:'string',sort:true,width:120,align:'left'},
                       {field:'XSYNAME',title:'销售负责人',type:'string',sort:true,width:100,align:'left'},
                       {field:'F_BLN_HFJHRQ',title:'交货日期',type:'datetime',sort:true,width:150,align:'center'},
                       {field:'SuperiorQty',title:'超期数量',type:'decimal',sort:true,width:100,align:'right'},
                       {field:'FCKREALQTY',title:'出库数量',type:'decimal',sort:true,width:100,align:'right'}],
                    sortName: 'CreateDate',
                    key: 'DetailID'
                                            };
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