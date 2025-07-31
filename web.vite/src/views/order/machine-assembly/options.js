export default function(){
    // 表格基本配置
    const table = {
        key: 'id',
        cnName: '整机装配',
        name: 'MachineAssemblyReminder',
        
        // 禁用默认数据加载
        url: '',
        load: false,
        
        // 表格显示配置
        stripe: true,
        border: true,
        
        // 分页配置
        pagination: true,
        pageSize: 30,
        pageSizes: [20, 30, 50, 100],
    };

    const tableName = table.name;
    const tableCNName = table.cnName;
    const key = table.key;

    // 列配置
    const columns = [
        {
            field: 'productionNo',
            title: '生产单据号',
            align: 'left',
            width: 140,
            fixed: 'left',
            sort: true
        },
        {
            field: 'urgencyLevel', 
            title: '紧急等级',
            align: 'center',
            width: 100,
            sort: true
        },
        {
            field: 'orderCount',
            title: '订单笔数',
            width: 100,
            align: 'center',
            sort: true
        },
        {
            field: 'plannedQuantity',
            title: '计划数量',
            width: 100,
            align: 'center',
            sort: true
        },
        {
            field: 'completedQuantity',
            title: '完成数量',
            width: 100,
            align: 'center',
            sort: true
        },
        {
            field: 'overdueQuantity',
            title: '超期数量',
            width: 100,
            align: 'center',
            sort: true
        },
        {
            field: 'pendingQuantity',
            title: '待完成数量',
            width: 110,
            align: 'center',
            sort: true
        },
        {
            field: 'statusJudgment',
            title: '状态判断',
            width: 100,
            align: 'center'
        },
        {
            field: 'standardCompletionDate',
            title: '标准完工日期',
            width: 130,
            align: 'center',
            sort: true
        },
        {
            field: 'actualCompletionDate',
            title: '实际完工日期',
            width: 130,
            align: 'center',
            sort: true
        },
        {
            field: 'overdueDays',
            title: '超期天数',
            width: 100,
            align: 'center',
            sort: true
        },
        {
            field: 'action',
            title: '操作',
            width: 200,
            align: 'center',
            fixed: 'right'
        }
    ];

    // 编辑表单字段（空，因为不需要编辑功能）
    const editFormFields = {};

    // 编辑表单配置（空数组格式）
    const editFormOptions = [];

    // 搜索表单字段（空，因为不需要搜索功能）
    const searchFormFields = {};

    // 搜索表单配置（空数组格式）
    const searchFormOptions = [];

    // 详情配置
    const detail = {
        columns: []
    };

    // 明细表配置
    const details = [];

    return {
        table,
        key,
        tableName,
        tableCNName,
        editFormFields,
        editFormOptions,
        searchFormFields,
        searchFormOptions,
        columns,
        detail,
        details
    };
} 