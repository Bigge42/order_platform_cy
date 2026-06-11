// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const redWarningCellStyle = {color:'#d03050',fontWeight:'700',backgroundColor:'#fff1f0'};
    const holidayWarningCellStyle = {color:'#8c5a00',fontWeight:'700',backgroundColor:'#fff4c7'};
    const sundayReserveCellStyle = {color:'#8c5a00',fontWeight:'700',backgroundColor:'#fff7d6'};
    const capacityStatutoryHolidayDates2026 = new Set([
        '2026-01-01','2026-01-02','2026-01-03',
        '2026-02-15','2026-02-16','2026-02-17','2026-02-18','2026-02-19','2026-02-20','2026-02-21','2026-02-22','2026-02-23',
        '2026-04-04','2026-04-05','2026-04-06',
        '2026-05-01','2026-05-02','2026-05-03','2026-05-04','2026-05-05',
        '2026-06-19','2026-06-20','2026-06-21',
        '2026-09-25','2026-09-26','2026-09-27',
        '2026-10-01','2026-10-02','2026-10-03','2026-10-04','2026-10-05','2026-10-06','2026-10-07'
    ]);
    const capacityMakeupWorkdayDates2026 = new Set([
        '2026-01-04',
        '2026-02-14',
        '2026-02-28',
        '2026-05-09',
        '2026-09-20',
        '2026-10-10'
    ]);
    const toDateOnlyText = (value) => {
        if (!value) {
            return '';
        }

        const text = String(value).slice(0, 10);
        const match = /^(\d{4})-(\d{1,2})-(\d{1,2})/.exec(text);
        if (match) {
            return `${match[1]}-${String(match[2]).padStart(2, '0')}-${String(match[3]).padStart(2, '0')}`;
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return '';
        }

        return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
    };
    const toDateOnlyTime = (value) => {
        if (!value) {
            return null;
        }

        const text = String(value).slice(0, 10);
        const match = /^(\d{4})-(\d{1,2})-(\d{1,2})/.exec(text);
        if (match) {
            return new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3])).getTime();
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return null;
        }
        return new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime();
    };
    const isReplyDeliveryDateLaterThanStandard = ({ ReplyDeliveryDate, StandardDeliveryDate } = {}) => {
        const replyTime = toDateOnlyTime(ReplyDeliveryDate);
        const standardTime = toDateOnlyTime(StandardDeliveryDate);
        return replyTime !== null && standardTime !== null && replyTime < standardTime;
    };
    const isSundayCapacityDate = (value) => {
        const dateOnlyTime = toDateOnlyTime(value);
        if (dateOnlyTime === null) {
            return false;
        }

        const dateOnlyText = toDateOnlyText(value);
        return new Date(dateOnlyTime).getDay() === 0
            && !capacityMakeupWorkdayDates2026.has(dateOnlyText)
            && !capacityStatutoryHolidayDates2026.has(dateOnlyText);
    };
    const isCapacityStatutoryHolidayDate = (value) => {
        return capacityStatutoryHolidayDates2026.has(toDateOnlyText(value));
    };
    const getCapacityScheduleDateCellStyle = ({ CapacityScheduleDateOverThreshold, CapacityScheduleDate }) => {
        if (CapacityScheduleDateOverThreshold) {
            return redWarningCellStyle;
        }

        if (isCapacityStatutoryHolidayDate(CapacityScheduleDate)) {
            return holidayWarningCellStyle;
        }

        if (isSundayCapacityDate(CapacityScheduleDate)) {
            return sundayReserveCellStyle;
        }

        return {};
    };
    const applyCapacityHolidayRowStyle = (column) => {
        const originalCellStyle = column.cellStyle;
        column.cellStyle = (row, rowIndex, columnIndex, tableData) => {
            const originalStyle = originalCellStyle
                ? originalCellStyle(row, rowIndex, columnIndex, tableData) || {}
                : {};
            if (originalStyle.backgroundColor === redWarningCellStyle.backgroundColor) {
                return originalStyle;
            }

            if (isCapacityStatutoryHolidayDate(row && row.CapacityScheduleDate)) {
                return Object.keys(originalStyle).length
                    ? { ...holidayWarningCellStyle, ...originalStyle }
                    : holidayWarningCellStyle;
            }

            return originalStyle;
        };
    };
    const applyReplyDeliveryWarningRowStyle = (column) => {
        const originalCellStyle = column.cellStyle;
        column.cellStyle = (row, rowIndex, columnIndex, tableData) => {
            if (isReplyDeliveryDateLaterThanStandard(row)) {
                return redWarningCellStyle;
            }

            return originalCellStyle
                ? originalCellStyle(row, rowIndex, columnIndex, tableData) || {}
                : {};
        };
    };
    const table = {
        key: 'Id',
        footer: "Foots",
        cnName: '排产智能体优化看板',
        name: 'WZ_OrderCycleBase',
        newTabEdit: false,
        url: "/WZ_OrderCycleBase/",
        sortName: "Id"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {};
    const editFormOptions = [];
    const searchFormFields = {};
    const searchFormOptions = [];
    const columns = [{field:'Id',title:'ID',type:'int',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'FENTRYID',title:'销售订单明细',type:'long',sort:true,width:130,align:'left'},
                       {field:'SalesOrderNo',title:'销售订单号',type:'string',width:120,align:'left'},
                       {field:'PlanTrackingNo',title:'计划跟踪号',type:'string',width:120,align:'left'},
                       {field:'OrderApprovedDate',title:'订单审核日期',type:'date',width:110,align:'left'},
                       {field:'ReplyDeliveryDate',title:'回复交货日期',type:'date',width:110,align:'left'},
                       {field:'RequestedDeliveryDate',title:'要货日期',type:'date',width:110,align:'left'},
                       {field:'StandardDeliveryDate',title:'标准交货日期',type:'date',width:110,align:'left'},
                       {field:'ScheduleDate',title:'排产日期',type:'date',width:110,align:'left'},
                       {field:'CapacityScheduleDate',title:'排产优化日期',type:'date',width:150,align:'left',cellStyle:({CapacityScheduleDateOverThreshold})=>CapacityScheduleDateOverThreshold?{color:'#d03050',fontWeight:'700',backgroundColor:'#fff1f0'}:{}},
                       {field:'CapacityScheduleDateOverThreshold',title:'排产优化日期超阈值',type:'bool',hidden:true,width:110,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',width:120,align:'left'},
                       {field:'OrderQty',title:'订单数量',type:'decimal',width:110,require:true,align:'left'},
                       {field:'GUI_GE_XING_HAO',title:'规格型号',type:'string',width:180,align:'left'},
                       {field:'ProductName',title:'产品名称',type:'string',width:180,align:'left'},
                       {field:'NominalDiameter',title:'公称通径',type:'string',width:110,align:'left'},
                       {field:'NominalPressure',title:'公称压力',type:'string',width:110,align:'left'},
                       {field:'BodyMaterial',title:'阀体材质',type:'string',width:220,align:'left'},
                       {field:'InnerMaterial',title:'内件材质',type:'string',width:150,align:'left'},
                       {field:'FlangeConnection',title:'法兰连接方式',type:'string',width:150,align:'left'},
                       {field:'BonnetForm',title:'上盖形式',type:'string',width:150,align:'left'},
                       {field:'SealFaceForm',title:'密封面形式',type:'string',sort:true,width:150,align:'left'},
                       {field:'FlowCharacteristic',title:'流量特性',type:'string',width:150,align:'left'},
                       {field:'Actuator',title:'执行机构',type:'string',width:150,align:'left'},
                       {field:'OutsourcedValveBody',title:'外购阀体',type:'string',width:150,align:'left'},
                       {field:'ValveCategory1',title:'阀门大类',type:'string',width:150,align:'left'},
                       {field:'ValveCategory',title:'阀门类别',type:'string',width:150,align:'left'},
                       {field:'ProductionLine',title:'生产线',type:'string',width:110,align:'left'},
                       {field:'FixedCycleDays',title:'固定周期(天)',type:'int',width:110,align:'left'},
                       {field:'SpecialProduct',title:'特品',type:'string',width:180,align:'left'},
                       {field:'PurchaseFlag',title:'外购标志',type:'string',width:110,align:'left'},
                       {field:'AssignedProductionLine',title:'产线',type:'string',width:110,align:'left'}];
    const capacityScheduleDateColumn = columns.find(column => column.field === 'CapacityScheduleDate');
    if (capacityScheduleDateColumn) {
        capacityScheduleDateColumn.cellStyle = getCapacityScheduleDateCellStyle;
    }
    columns.forEach(applyCapacityHolidayRowStyle);
    columns.forEach(applyReplyDeliveryWarningRowStyle);
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
