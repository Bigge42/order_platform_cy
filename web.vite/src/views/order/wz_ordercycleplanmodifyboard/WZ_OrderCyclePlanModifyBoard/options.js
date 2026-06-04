// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成风格派生，字段顺序参考 WZ_OrderCycleBase
export default function () {
  const table = {
    key: 'Id',
    footer: 'Foots',
    cnName: '计划修改看板',
    name: 'OCP_OrderTracking',
    newTabEdit: false,
    url: '/OCP_OrderTracking/',
    sortName: 'CreateDate'
  }
  const tableName = table.name
  const tableCNName = table.cnName
  const newTabEdit = false
  const key = table.key
  const editFormFields = {}
  const editFormOptions = []
  const searchFormFields = {
    SOBillNo: '',
    MtoNo: ''
  }
  const searchFormOptions = [
    [
      {
        title: '销售订单号',
        field: 'SOBillNo',
        type: 'likeStart',
        comparationList: [{ key: 'likeStart', value: '模糊查询(左包含)' }]
      },
      {
        title: '计划跟踪号',
        field: 'MtoNo',
        type: 'likeStart',
        comparationList: [{ key: 'likeStart', value: '模糊查询(左包含)' }]
      }
    ]
  ]
  const columns = [
    {
      field: 'Id',
      title: 'ID',
      type: 'int',
      width: 110,
      hidden: true,
      readonly: true,
      require: true,
      align: 'left'
    },
    { field: 'SOEntryID', title: '销售订单明细', type: 'long', sort: true, width: 130, align: 'left' },
    { field: 'SOBillNo', title: '销售订单号', type: 'string', width: 120, align: 'left' },
    { field: 'MtoNo', title: '计划跟踪号', type: 'string', width: 120, align: 'left' },
    { field: 'OrderAuditDate', title: '订单审核日期', type: 'date', width: 110, align: 'left' },
    { field: 'ReplyDeliveryDate', title: '回复交货日期', type: 'date', width: 110, align: 'left' },
    { field: 'DeliveryDate', title: '要货日期', type: 'date', width: 110, align: 'left' },
    { field: 'PrdScheduleDate', title: '排产日期', type: 'date', width: 110, align: 'left' },
    { field: 'PlanConfirmDate', title: '计划确认日期', type: 'date', width: 150, align: 'left' },
    { field: 'PlanStartDate', title: '计划开工日期', type: 'date', width: 150, align: 'left' },
    { field: 'MaterialNumber', title: '物料编码', type: 'string', width: 120, align: 'left' },
    { field: 'OrderQty', title: '订单数量', type: 'decimal', width: 110, require: true, align: 'left' },
    { field: 'TopSpecification', title: '规格型号', type: 'string', width: 180, align: 'left' },
    { field: 'MaterialName', title: '产品名称', type: 'string', width: 180, align: 'left' },
    { field: 'ProductionModel', title: '产品型号', type: 'string', width: 180, align: 'left' },
    { field: 'ProjectName', title: '项目名称', type: 'string', width: 150, align: 'left' },
    { field: 'CustName', title: '客户名称', type: 'string', width: 180, align: 'left' },
    { field: 'UseUnit', title: '使用单位', type: 'string', width: 180, align: 'left' },
    { field: 'ContractNo', title: '销售合同号', type: 'string', width: 130, align: 'left' },
    { field: 'ContractType', title: '合同类型', type: 'string', width: 110, align: 'left' },
    { field: 'SalesPerson', title: '销售员', type: 'string', width: 110, align: 'left' },
    { field: 'Urgency', title: '紧急等级', type: 'string', width: 110, align: 'left' },
    { field: 'BillStatus', title: '订单状态', type: 'string', width: 110, align: 'left' },
    { field: 'MtoNoStatus', title: '交货情况', type: 'string', width: 110, align: 'left' },
    { field: 'FinishStatus', title: '订单完成状态', type: 'string', width: 130, align: 'left' },
    { field: 'InstockQty', title: '入库数量', type: 'decimal', width: 110, align: 'left' },
    { field: 'UnInstockQty', title: '未完数量', type: 'decimal', width: 110, align: 'left' },
    { field: 'PrepareMtrl', title: '开工准备', type: 'string', width: 110, align: 'left' }
  ]
  const detail = { columns: [] }
  const details = []

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
  }
}
