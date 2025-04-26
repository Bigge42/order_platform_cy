export default function () {
  const editFormFields = {
    ProjectCode: 'Q202408310001',
    ProjectName: '2024年采购招标',
    FacilityId: 'A组',
    FacilityNum: 'A00001',
    SetPurchaseAmount: 1000,
    PurchaseAmount: 2000,
    SetMaterialAmount: 1200,
    MaterialAmount: 450,
    SetWorkTimes: '0',
    WorkTimes: 8,
    SetDeliveryAmount: '1',
    DeliveryAmount: '厢式货车',
    SetConstructAmount: 2000,
    ConstructAmount: 1400,
    SetConstructAmount1: '',
    ConstructAmount1: '',
    Company1: '',
    Phone1: '',
    Creator: '',
    CreateDate: '',
    Company: '',
    Phone: '',
    Modifier: '',
    ModifyDate: '',
    remark: [],
    img1: [],
    img2: [],
    img3: [],
    img4: []
  }
  const editFormOptions = [
    [
      { title: '销售合同', field: 'ProjectCode', required: true },
      { title: '项目名称', field: 'ProjectName' },
      { title: '机组名称', field: 'FacilityId', required: true },
      { title: '机组编号', field: 'FacilityNum', required: true }
    ],
    [
      { title: '物料成本', field: 'SetPurchaseAmount', required: true, type: 'decimal' },
      { title: '外购金额', field: 'PurchaseAmount', required: true, type: 'decimal' },
      { title: '材料金额', field: 'SetMaterialAmount', required: true, type: 'decimal' },
      { title: '材料运费', field: 'MaterialAmount', required: true, type: 'decimal' }
    ],
    [
      { title: '运输方式', field: 'DeliveryAmount' },
      { title: '累计工时', field: 'WorkTimes' },
      {
        title: '是否正常',
        field: 'SetDeliveryAmount',
        type: 'select',
        data: [],
        dataKey: 'enable'
      },
      { title: '是否立项', field: 'SetWorkTimes', type: 'radio', data: [], dataKey: 'enable' }
    ],
    [
      { title: '供应商名称', field: 'SetConstructAmount1', type: '' },
      { title: '供应商编号', field: 'ConstructAmount1', type: '' },
      { title: '科目名称', field: 'Company1' },
      { title: '电话号码', field: 'Phone1' }
    ],
    [
      { title: '安装金额', field: 'SetConstructAmount', type: 'decimal' },
      { title: '安装金额', field: 'ConstructAmount', type: 'decimal' },
      { title: '服务公司', field: 'Company' },
      { title: '联系方式', field: 'Phone' }
    ],
    [
      { title: '提交人员', field: 'Creator' },
      { title: '提交日期', field: 'CreateDate', type: 'date' },
      { title: '签收人员', field: 'Modifier', type: '' },
      { title: '签收日期', field: 'ModifyDate', type: 'date' }
    ],

    [
      { title: '厂家资质', field: 'img1', type: 'file' },
      { title: '营业执照', field: 'img2', type: 'file' },
      { title: '经营许可', field: 'img3', type: 'file' },
      { title: '合同附件', field: 'img4', type: 'file' }
    ],
    [{ title: '补充说明', field: 'remark', type: 'img', colSize: 12 }],
  ]
  const tableName = 'Demo_Order'
  const tableCNName = '项目成本控制'
  const newTabEdit = true
  const key = 'GoodsId'
  const detail = {
    cnName: '',
    table: '',
    url: '',
    columns: [],
    sortName: '',
    key: ''
  }

  const details = [
    {
      cnName: '成本控制分项',
      table: 'Pm_ControlDetail',
      columns: [
        { field: 'ItemName', title: '分项名称', type: 'string', width: 110, edit: { type: '' }, align: 'left' },
        {
          field: 'PreCostAmount',
          title: '预计成本',
          type: 'decimal',
          width: 80,
          edit: { type: '' },
          align: 'left'
        },
        {
          field: 'CurrCostAmount',
          title: '当前累计成本',
          type: 'decimal',
          width: 110,
          readonly: true,
          edit: { type: '' },
          align: 'left'
        },
        {
          field: 'Memo',
          title: '备注',
          type: 'string',
          width: 120,
          edit: { type: '' },
          align: 'left'
        },
        {
          field: 'CreateID',
          title: 'CreateID',
          type: 'int',
          width: 80,
          hidden: true,
          align: 'left'
        },
        { field: 'Creator', title: '创建人', type: 'string', width: 100, align: 'left' },
        { field: 'CreateDate', title: '创建日期', type: 'datetime', width: 110, align: 'left' },
        {
          field: 'ModifyID',
          title: 'ModifyID',
          type: 'int',
          width: 80,
          hidden: true,
          align: 'left'
        },
        { field: 'Modifier', title: '修改人', type: 'string', width: 100, align: 'left' },
        { field: 'ModifyDate', title: '修改日期', type: 'datetime', width: 140, align: 'left' }
      ],
      sortName: 'ProjectCostDetailId',
      key: 'ProjectCostDetailId',
      buttons: [],
      delKeys: [],
      detail: null
    },
    {
      cnName: '外购价格表',
      table: 'Pm_PrePurchaseDetail',
      columns: [
        {
          field: 'ProductName',
          title: '物料名称',
          type: 'string',
          width: 110,
          edit: { type: '' },
          align: 'left'
        },
        {
          field: 'ProductUnit',
          title: '单位',
          type: 'string',
          width: 110,
          edit: { type: '' },
          align: 'left'
        },
        {
          field: 'Amount',
          title: '金额',
          type: 'decimal',
          width: 110,
          edit: { type: '' },
          align: 'left'
        },
        { field: 'Company', title: '公司', type: 'string', width: 100, align: 'left' },
        {
          field: 'CreateID',
          title: 'CreateID',
          type: 'int',
          width: 80,
          hidden: true,
          align: 'left'
        },
        { field: 'Creator', title: '创建人', type: 'string', width: 100, align: 'left' },
        { field: 'CreateDate', title: '创建日期', type: 'datetime', width: 110, align: 'left' },
        {
          field: 'ModifyID',
          title: 'ModifyID',
          type: 'int',
          width: 80,
          hidden: true,
          align: 'left'
        },
        { field: 'Modifier', title: '修改人', type: 'string', width: 100, align: 'left' },
        { field: 'ModifyDate', title: '修改日期', type: 'datetime', width: 110, align: 'left' }
      ],
      sortName: 'PrePurchaseDetailId',
      key: 'PrePurchaseDetailId',
      buttons: [],
      delKeys: [],
      detail: null
    },
    {
      cnName: '外购件明细',
      table: 'Pm_PurchaseDetail',
      columns: [
        {
          field: 'PurchaseDetail',
          title: 'PurchaseDetail',
          type: 'int',
          width: 110,
          hidden: true,
          require: true,
          align: 'left'
        },
        {
          field: 'FacilityId',
          title: 'FacilityId',
          type: 'int',
          width: 110,
          hidden: true,
          align: 'left'
        },
        { field: 'PurchaseOrderNum', title: '单号', type: 'string', width: 110, align: 'left' },
        { field: 'ProductCode', title: '物料编码', type: 'string', width: 110, align: 'left' },
        { field: 'ProductName', title: '物料名称', type: 'string', width: 110, align: 'left' },
        { field: 'ProductUnit', title: '单位', type: 'string', width: 110, align: 'left' },
        { field: 'Quantity', title: '数量', type: 'decimal', width: 110, align: 'left' },
        { field: 'Price', title: '单价', type: 'decimal', width: 110, align: 'left' },
        { field: 'TaxAmount', title: '金额', type: 'decimal', width: 110, align: 'left' },
        { field: 'subProjectCode', title: '施工单号', type: 'string', width: 120, align: 'left' }
      ],
      sortName: 'PurchaseDetail',
      key: 'PurchaseDetail',
      buttons: [],
      delKeys: [],
      detail: null
    },
    {
      cnName: '原材料明细',
      table: 'Pm_MaterialDetail',
      columns: [
        {
          field: 'MaterialDetail',
          title: 'MaterialDetail',
          type: 'int',
          width: 110,
          hidden: true,
          require: true,
          align: 'left'
        },
        {
          field: 'FacilityId',
          title: 'FacilityId',
          type: 'int',
          width: 110,
          hidden: true,
          align: 'left'
        },
        { field: 'PurchaseOrderNum', title: '单号', type: 'string', width: 110, align: 'left' },
        { field: 'ProductCode', title: '物料编码', type: 'string', width: 110, align: 'left' },
        { field: 'ProductName', title: '物料名称', type: 'string', width: 110, align: 'left' },
        { field: 'ProductUnit', title: '单位', type: 'string', width: 110, align: 'left' },
        { field: 'Quantity', title: '数量', type: 'decimal', width: 110, align: 'left' },
        { field: 'Price', title: '单价', type: 'decimal', width: 110, align: 'left' },
        { field: 'TaxAmount', title: '金额', type: 'decimal', width: 110, align: 'left' },
        { field: 'subProjectCode', title: '施工单号', type: 'string', width: 120, align: 'left' }
      ],
      sortName: 'MaterialDetail',
      key: 'MaterialDetail',
      buttons: [],
      delKeys: [],
      detail: null
    },
    {
      cnName: '工时明细',
      table: 'Pm_WorkTimeDetail',
      columns: [
        {
          field: 'WorkTimeDetail',
          title: 'WorkTimeDetail',
          type: 'int',
          width: 110,
          hidden: true,
          require: true,
          align: 'left'
        },
        {
          field: 'FacilityId',
          title: 'FacilityId',
          type: 'int',
          width: 110,
          hidden: true,
          align: 'left'
        },
        { field: 'ProductCode', title: '产品编号', type: 'string', width: 110, align: 'left' },
        { field: 'ProductName', title: '产品名称', type: 'string', width: 110, align: 'left' },
        { field: 'ProductUnit', title: '单位', type: 'string', width: 110, align: 'left' },
        { field: 'WorkTimeQuantity', title: '工时', type: 'decimal', width: 110, align: 'left' },
        { field: 'subProjectId', title: '施工单', type: 'int', width: 110, align: 'left' },
        { field: 'subProjectCode', title: '施工单号', type: 'string', width: 120, align: 'left' },
        { field: 'Memo', title: '备注', type: 'string', width: 120, align: 'left' },
        { field: 'Company', title: '公司', type: 'string', width: 120, align: 'left' }
      ],
      sortName: 'WorkTimeDetail',
      key: 'WorkTimeDetail',
      buttons: [],
      delKeys: [],
      detail: null
    },
    {
      cnName: '安装明细',
      table: 'Pm_ConstructionDetail',
      columns: [
        {
          field: 'ConstructionDetail',
          title: 'ConstructionDetail',
          type: 'int',
          width: 110,
          hidden: true,
          require: true,
          align: 'left'
        },
        {
          field: 'FacilityId',
          title: 'FacilityId',
          type: 'int',
          width: 110,
          hidden: true,
          align: 'left'
        },
        { field: 'ConstructionOrderNum', title: '单号', type: 'string', width: 110, align: 'left' },
        { field: 'ProductName', title: '产品名称', type: 'string', width: 110, align: 'left' },
        { field: 'ProductUnit', title: '单位', type: 'string', width: 110, align: 'left' },
        { field: 'Quantity', title: '数量', type: 'decimal', width: 110, align: 'left' },
        { field: 'Price', title: '单价', type: 'decimal', width: 110, align: 'left' },
        { field: 'TaxAmount', title: '金额', type: 'decimal', width: 110, align: 'left' },
        { field: 'subProjectCode', title: '施工单号', type: 'string', width: 120, align: 'left' },
        { field: 'Memo', title: '备注', type: 'string', width: 120, align: 'left' }
      ],
      sortName: 'ConstructionDetail',
      key: 'ConstructionDetail',
      buttons: [],
      delKeys: [],
      detail: null
    }
  ]

  return {
    key,
    tableName,
    tableCNName,
    editFormFields,
    editFormOptions,
    detail,
    details,
    newTabEdit
  }
}
