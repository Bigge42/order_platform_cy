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
                       {field:'SalesPerson',title:'销售负责人',type:'string',width:100},
                       {field:'ProjectName',title:'项目名称',type:'string',width:220}];

        const detail =   {
                    cnName: '销售订单进度详情',
                    table: 'OCP_SOProgressDetail',
                    columns: [{field:'DetailID',title:'详情ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'OrderID',title:'订单ID',type:'long',sort:true,width:80,hidden:true,align:'left'},
                       {field:'SOBillNo',title:'销售订单号',type:'string',sort:true,width:110,align:'left'},
                       {field:'MtoNo',title:'计划跟踪号',type:'string',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'MOBillNo',title:'生产单据号',type:'string',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'BusinessStatus',title:'业务状态',type:'string',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'Qty',title:'订货数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'PositionNo',title:'位号',type:'string',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'MaterialNumber',title:'物料编码',type:'string',sort:true,width:120,edit:{type:''},align:'left'},
                       {field:'ProductionModel',title:'产品型号',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'Specification',title:'规格型号',type:'string',sort:true,width:220,edit:{type:''},align:'left'},
                       {field:'InstockQty',title:'入库数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'UnInstockQty',title:'待入库数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'OutStockQty',title:'出库数量',type:'decimal',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'ChangeStatus',title:'变更状态',type:'string',sort:true,width:110,edit:{type:''},align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,hidden:true,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'SOEntryID',title:'订单明细ID',type:'long',width:80,hidden:true,align:'left'},
                       {field:'MaterialID',title:'物料ID',type:'long',width:80,hidden:true,align:'left'}],
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