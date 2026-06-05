<!-- 订单跟踪 -->
<template>
    <div class="ocp-order-tracking-page">
        <view-grid ref="grid"
            :columns="columns"
            :detail="detail"
            :details="details"
            :editFormFields="editFormFields"
            :editFormOptions="editFormOptions"
            :searchFormFields="searchFormFields"
            :searchFormOptions="searchFormOptions"
            :table="table"
            :extend="extend"
            :onInit="onInit"
            :onInited="onInited"
            :searchBefore="searchBefore"
            :searchAfter="searchAfter"
            :addBefore="addBefore"
            :updateBefore="updateBefore"
            :rowClick="rowClick"
            :rowChange="rowChange"
            :selectionChange="selectionChange"
            :modelOpenBefore="modelOpenBefore"
            :modelOpenAfter="modelOpenAfter">
            <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
            <template #gridHeader>
                <div class="tracking-workbench">
                    <div class="tracking-workbench__header">
                        <div>
                            <div class="tracking-workbench__title">订单跟踪工作台</div>
                            <div class="tracking-workbench__subtitle">
                                销售订单 / 排产计划 / 入库交付协同跟踪
                            </div>
                        </div>
                        <div class="tracking-workbench__actions">
                            <span class="tracking-workbench__selected">已选 {{ selectedRows.length }} 条</span>
                            <el-button type="primary" size="small" @click.stop="openMachineTracking">
                                <i class="el-icon-position"></i>
                                整机跟踪
                            </el-button>
                            <el-button
                                v-if="hasESBPermission"
                                type="primary"
                                plain
                                size="small"
                                :loading="syncLoading"
                                :disabled="syncLoading"
                                @click.stop="handleManualSync"
                            >
                                <i class="el-icon-refresh"></i>
                                ESB数据同步
                            </el-button>
                            <el-button plain size="small" @click.stop="refreshTrackingPage">
                                <i class="el-icon-refresh-right"></i>
                                刷新
                            </el-button>
                        </div>
                    </div>

                    <div class="tracking-metrics">
                        <div
                            v-for="card in metricCards"
                            :key="card.key"
                            class="tracking-metric"
                            :class="`tracking-metric--${card.tone}`"
                        >
                            <div class="tracking-metric__icon">
                                <i :class="card.icon"></i>
                            </div>
                            <div class="tracking-metric__content">
                                <div class="tracking-metric__label">{{ card.label }}</div>
                                <div class="tracking-metric__value">{{ card.value }}</div>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        </view-grid>
    </div>

    <!-- 留言板组件 -->
    <MessageBoard ref="messageBoardRef" />
    
    <!-- 催单弹窗组件 -->
    <ReminderDialog
      v-model="followUpReminderVisible"
      :data="followUpReminderData"
      @send="handleFollowUpSend"
    />
    
    <!-- 留言弹窗组件 -->
    <ReplyDialog
      v-model="replyDialogVisible"
      :title="replyDialogTitle"
      @confirm="handleReplyConfirm"
      @cancel="handleReplyCancel"
    />
    
    <!-- 发起协商弹窗组件 -->
    <NegotiationDialog
      v-model="negotiationDialogVisible"
      :data="negotiationDialogData"
      @confirm="handleNegotiationConfirm"
      @cancel="handleNegotiationCancel"
    />
    

</template>
<script setup lang="jsx">
    import extend from "@/extension/order/ordercollaboration/OCP_OrderTracking.jsx";
    import viewOptions from './OCP_OrderTracking/options.js'
    import MessageBoard from '@/comp/message-board/index.vue'
    import ReminderDialog from '@/comp/reminder-dialog/index.vue'
    import ReplyDialog from '@/comp/reply-dialog/index.vue'
    import NegotiationDialog from '@/comp/negotiation-dialog/index.vue'
    import { ref, reactive, getCurrentInstance, computed } from "vue";
    import { ElMessage } from 'element-plus'
    
    const grid = ref(null);
    const { proxy } = getCurrentInstance()
    const messageBoardRef = ref(null)
    
    // 跟踪弹窗相关状态
    const trackingDialogVisible = ref(false)
    const trackingDialogTitle = ref('')
    const currentPageKey = ref('')

    // 催单弹窗相关
    const followUpReminderVisible = ref(false)
    const followUpReminderData = ref({})
    
    // 留言弹窗相关
    const replyDialogVisible = ref(false)
    const replyDialogTitle = ref('留言')
    const currentReplyRow = ref(null)
    
    // 发起协商弹窗相关
    const negotiationDialogVisible = ref(false)
    const negotiationDialogData = ref({})
    
    // ESB数据同步loading状态
    const syncLoading = ref(false)
    
    // 选中的行数据
    const selectedRows = ref([])
    
    // 当前页表格数据，用于顶部概览统计
    const tableRows = ref([])
    const tableTotal = ref(0)

    const hasESBPermission = computed(() => {
        const permissions = proxy.$store.getters.getPermission() || []
        return permissions.find(x => x.id === 313)?.permission?.includes('ESB_Sync')
    })
    
    // 计算跟踪按钮是否禁用（未选择数据时禁用）
    const trackingButtonDisabled = computed(() => selectedRows.value.length === 0)

    const metricCards = computed(() => {
        const rows = tableRows.value || []
        const total = tableTotal.value || rows.length
        const alertCount = rows.filter(isAlertRow).length
        const urgentCount = rows.filter(isUrgentRow).length
        const unfinishedQty = sumRows(rows, 'UnInstockQty')
        const unInstockQty = sumRows(rows, 'UnInstockQty')
        const unJoinCount = rows.filter(row => !isTruthyValue(row.IsJoinTask)).length

        return [
            {
                key: 'total',
                label: '订单总数',
                value: formatNumber(total, 0),
                tone: 'primary',
                icon: 'bi bi-clipboard-data'
            },
            {
                key: 'alert',
                label: '预警订单',
                value: formatNumber(alertCount, 0),
                tone: 'warning',
                icon: 'bi bi-exclamation-triangle'
            },
            {
                key: 'urgent',
                label: '紧急订单',
                value: formatNumber(urgentCount, 0),
                tone: 'danger',
                icon: 'bi bi-bell'
            },
            {
                key: 'unfinished',
                label: '未完数量',
                value: formatNumber(unfinishedQty),
                tone: 'blue',
                icon: 'bi bi-layers'
            },
            {
                key: 'uninstock',
                label: '未入库',
                value: formatNumber(unInstockQty),
                tone: 'success',
                icon: 'bi bi-house-door'
            },
            {
                key: 'unjoin',
                label: '未关联任务',
                value: formatNumber(unJoinCount, 0),
                tone: 'purple',
                icon: 'bi bi-link-45deg'
            }
        ]
    })

    function toNumber(value) {
        const number = Number(value)
        return Number.isFinite(number) ? number : 0
    }

    function sumRows(rows, field) {
        return rows.reduce((total, row) => total + toNumber(row[field]), 0)
    }

    function formatNumber(value, precision = 2) {
        const number = toNumber(value)
        const fixedNumber = precision === 0 ? Math.round(number) : Number(number.toFixed(precision))
        return fixedNumber.toLocaleString('zh-CN', {
            minimumFractionDigits: precision === 0 ? 0 : 0,
            maximumFractionDigits: precision
        })
    }

    function formatDateText(value) {
        if (!value) return '-'
        if (typeof value === 'string') {
            return value.includes('T') ? value.split('T')[0] : value.slice(0, 10)
        }
        const date = new Date(value)
        if (Number.isNaN(date.getTime())) return '-'
        const year = date.getFullYear()
        const month = `${date.getMonth() + 1}`.padStart(2, '0')
        const day = `${date.getDate()}`.padStart(2, '0')
        return `${year}-${month}-${day}`
    }

    function valueText(value) {
        return value === undefined || value === null || value === '' ? '-' : `${value}`
    }

    function isTruthyValue(value) {
        return value === true || value === 1 || value === '1' || value === '是' || value === '已关联'
    }

    function includesAny(value, keywords) {
        const text = valueText(value)
        return keywords.some(keyword => text.includes(keyword))
    }

    function isUrgentRow(row) {
        return includesAny(row?.Urgency, ['紧急', '急', '高', 'A'])
    }

    function isAlertRow(row) {
        return row?.ShouldAlert === true ||
            isUrgentRow(row) ||
            includesAny(row?.MtoNoStatus, ['逾期', '延期', '超期', '延迟']) ||
            includesAny(row?.BillStatus, ['待确认', '异常', '冻结'])
    }

    function getStatusTone(value, fallback = 'info') {
        const text = valueText(value)
        if (includesAny(text, ['逾期', '延期', '超期', '异常', '作废', '终止', '紧急'])) return 'danger'
        if (includesAny(text, ['待确认', '待处理', '冻结'])) return 'warning'
        if (includesAny(text, ['完成', '正常', '已关联', '已审核'])) return 'success'
        if (includesAny(text, ['进行', '执行', '排产', '确认'])) return 'primary'
        return fallback
    }

    function getTagType(tone) {
        const tagMap = {
            primary: '',
            success: 'success',
            warning: 'warning',
            danger: 'danger',
            info: 'info'
        }
        return tagMap[tone] || 'info'
    }

    function renderTag(value, fallback = 'info') {
        const tone = getStatusTone(value, fallback)
        return (
            <el-tag
                size="small"
                effect="light"
                type={getTagType(tone)}
                class={['tracking-status-tag', `tracking-status-tag--${tone}`]}
            >
                {valueText(value)}
            </el-tag>
        )
    }

    function renderJoinTaskTag(value) {
        const joined = isTruthyValue(value)
        return (
            <el-tag
                size="small"
                effect="light"
                type={joined ? 'success' : 'warning'}
                class={['tracking-status-tag', joined ? 'tracking-status-tag--success' : 'tracking-status-tag--warning']}
            >
                {joined ? '已关联' : '未关联'}
            </el-tag>
        )
    }

    function renderDateCell(row, field) {
        const warning = isAlertRow(row) && ['DeliveryDate', 'ReplyDeliveryDate'].includes(field)
        return (
            <span class={['tracking-date-cell', warning ? 'tracking-date-cell--warning' : '']}>
                {formatDateText(row[field])}
            </span>
        )
    }

    function renderQuantityCell(row, field) {
        const value = toNumber(row[field])
        const orderQty = toNumber(row.OrderQty)
        const rate = field === 'InstockQty' && orderQty > 0
            ? Math.min(100, Math.round((value / orderQty) * 100))
            : null

        return (
            <div class="tracking-qty-cell">
                <span>{formatNumber(value)}</span>
                {rate !== null ? (
                    <span class="tracking-qty-cell__bar">
                        <span style={{ width: `${rate}%` }}></span>
                    </span>
                ) : null}
            </div>
        )
    }

    function mergeColumnCellStyle(column, cellStyle) {
        const originalCellStyle = column.cellStyle
        column.cellStyle = (row, rowIndex, columnIndex, tableData) => {
            const originalStyle = originalCellStyle
                ? originalCellStyle(row, rowIndex, columnIndex, tableData)
                : null
            const nextStyle = cellStyle(row, rowIndex, columnIndex, tableData)
            return { ...(originalStyle || {}), ...(nextStyle || {}) }
        }
    }

    function applyTrackingWarningStyle() {
        const keyWarningFields = ['DeliveryDate', 'ReplyDeliveryDate', 'Urgency', 'MtoNoStatus', 'BillStatus', 'UnInstockQty']
        columns.forEach(column => {
            mergeColumnCellStyle(column, row => {
                if (!isAlertRow(row)) return null
                const isKeyField = keyWarningFields.includes(column.field)
                return {
                    backgroundColor: isKeyField ? '#fff4de' : '#fffaf0',
                    color: isKeyField ? '#d42828' : '#595959'
                }
            })
        })
    }

    function renderMainText(value, tone = 'default') {
        return (
            <span class={['tracking-main-text', `tracking-main-text--${tone}`]}>
                {valueText(value)}
            </span>
        )
    }

    function configureTrackingColumns() {
        const preferredOrder = [
            'ContractNo',
            'SOBillNo',
            'MtoNo',
            'CustName',
            'MaterialNumber',
            'MaterialName',
            'TopSpecification',
            'Urgency',
            'DeliveryDate',
            'ReplyDeliveryDate',
            'OrderQty',
            'InstockQty',
            'UnInstockQty',
            'MtoNoStatus',
            'BillStatus',
            'PrepareMtrl'
        ]
        const compactHiddenFields = new Set([
            'ProjectName',
            'SalesPerson',
            'ContractType',
            'UseUnit',
            'ProductionModel',
            'IsJoinTask',
            'ProScheduleYearMonth',
            'PlanTaskMonth',
            'PlanTaskWeek',
            'ComputedDate',
            'BidDate',
            'BomCreateDate',
            'OrderCreateDate',
            'OrderAuditDate',
            'PrdScheduleDate',
            'PlanConfirmDate',
            'PlanStartDate',
            'StartDate',
            'LastInStockDate',
            'LastOutStockDate',
            'OutStockQty'
        ])
        const widthMap = {
            ContractNo: 132,
            SOBillNo: 136,
            MtoNo: 136,
            CustName: 150,
            MaterialNumber: 150,
            MaterialName: 180,
            TopSpecification: 180,
            Urgency: 96,
            DeliveryDate: 126,
            ReplyDeliveryDate: 136,
            OrderQty: 104,
            InstockQty: 126,
            UnInstockQty: 106,
            MtoNoStatus: 104,
            BillStatus: 104,
            PrepareMtrl: 210
        }
        const columnMap = new Map(columns.map(column => [column.field, column]))
        const orderedColumns = preferredOrder
            .map(field => columnMap.get(field))
            .filter(Boolean)
        const remainingColumns = columns.filter(column => !preferredOrder.includes(column.field))
        columns.splice(0, columns.length, ...orderedColumns, ...remainingColumns)

        columns.forEach(column => {
            column.showOverflowTooltip = true
            column.hidden = compactHiddenFields.has(column.field) ? true : column.hidden
            if (preferredOrder.includes(column.field)) {
                column.hidden = false
            }
            if (widthMap[column.field]) {
                column.width = widthMap[column.field]
            }
            if (['OrderQty', 'InstockQty', 'UnInstockQty', 'Amount'].includes(column.field)) {
                column.align = 'right'
                column.summary = true
                column.numberLength = 2
            }
            column.fixed = undefined
        })

        const contractColumn = columnMap.get('ContractNo')
        const soColumn = columnMap.get('SOBillNo')
        const mtoColumn = columnMap.get('MtoNo')
        const orderQtyColumn = columnMap.get('OrderQty')
        const instockQtyColumn = columnMap.get('InstockQty')
        const unInstockQtyColumn = columnMap.get('UnInstockQty')
        const urgencyColumn = columnMap.get('Urgency')
        const deliveryDateColumn = columnMap.get('DeliveryDate')
        const replyDeliveryDateColumn = columnMap.get('ReplyDeliveryDate')
        const mtoNoStatusColumn = columnMap.get('MtoNoStatus')
        const billStatusColumn = columnMap.get('BillStatus')
        const isJoinTaskColumn = columnMap.get('IsJoinTask')

        if (contractColumn) {
            contractColumn.fixed = 'left'
            contractColumn.render = (h, { row }) => renderMainText(row.ContractNo, 'contract')
        }
        if (soColumn) {
            soColumn.fixed = 'left'
            soColumn.render = (h, { row }) => renderMainText(row.SOBillNo, 'order')
        }
        if (mtoColumn) {
            mtoColumn.fixed = 'left'
            mtoColumn.render = (h, { row }) => renderMainText(row.MtoNo, 'trace')
        }
        if (urgencyColumn) {
            urgencyColumn.render = (h, { row }) => renderTag(row.Urgency, isUrgentRow(row) ? 'danger' : 'success')
        }
        if (deliveryDateColumn) {
            deliveryDateColumn.render = (h, { row }) => renderDateCell(row, 'DeliveryDate')
        }
        if (replyDeliveryDateColumn) {
            replyDeliveryDateColumn.render = (h, { row }) => renderDateCell(row, 'ReplyDeliveryDate')
        }
        if (orderQtyColumn) {
            orderQtyColumn.hidden = false
            orderQtyColumn.render = (h, { row }) => renderQuantityCell(row, 'OrderQty')
        }
        if (instockQtyColumn) {
            instockQtyColumn.render = (h, { row }) => renderQuantityCell(row, 'InstockQty')
        }
        if (unInstockQtyColumn) {
            unInstockQtyColumn.render = (h, { row }) => renderQuantityCell(row, 'UnInstockQty')
        }
        if (mtoNoStatusColumn) {
            mtoNoStatusColumn.render = (h, { row }) => renderTag(row.MtoNoStatus, 'primary')
        }
        if (billStatusColumn) {
            billStatusColumn.render = (h, { row }) => renderTag(row.BillStatus, 'primary')
        }
        if (isJoinTaskColumn) {
            isJoinTaskColumn.render = (h, { row }) => renderJoinTaskTag(row.IsJoinTask)
        }

        applyTrackingWarningStyle()
    }

    // 跟踪弹窗确认事件
    const handleTrackingConfirm = () => {
        console.log(`${trackingDialogTitle.value}确认操作`)
        // 这里可以添加具体的确认逻辑
        trackingDialogVisible.value = false
    }
    
    // 跟踪弹窗取消事件
    const handleTrackingCancel = () => {
        console.log(`${trackingDialogTitle.value}取消操作`)
        trackingDialogVisible.value = false
    }
    
    //http请求，proxy.http.post/get
    const viewOpts = reactive(viewOptions())
    const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = viewOpts

    let gridRef;//对应[表.jsx]文件中this.使用方式一样

    function openMachineTracking() {
        const currentSelection = updateSelectionManually()
        const mtoNoList = currentSelection.map(row => row.MtoNo).filter(Boolean)
        const query = mtoNoList.length > 0 ? { PlanTraceNo: mtoNoList.join(',') } : {}

        proxy.$tabs.open({
            text: '整机跟踪表',
            path: '/OCP_PrdMOTracking',
            query
        })

        proxy.$tabs.clearCache('OCP_LackMtrlResult_MO_JG')
    }

    async function handleManualSync() {
        if (syncLoading.value) {
            return
        }

        try {
            syncLoading.value = true
            proxy.$message.info('正在同步ESB数据...')
            const result = await proxy.http.post('/api/OCP_OrderTracking/ManualSyncOrderData')
            proxy.$message.success('ESB数据同步成功')
            console.log('ESB数据同步成功:', result)
            refreshTrackingPage()
        } catch (error) {
            console.error('ESB数据同步失败:', error)
            proxy.$message.error('ESB数据同步失败：' + (error.message || '未知错误'))
        } finally {
            syncLoading.value = false
        }
    }

    function refreshTrackingPage() {
        if (gridRef && gridRef.search) {
            gridRef.search(null, false)
        }
    }

    //生成对象属性初始化
    const onInit = async ($vm) => {
        gridRef = $vm;

        gridRef.pagination.sizes = [20, 50, 100, 200, 500, 1000];
        //设置默认分页数
        gridRef.pagination.size = 20;

        gridRef.queryFields=['PlanTaskMonth', 'ContractNo', 'SOBillNo', 'MtoNo', 'Urgency']

        gridRef.single=true;
    }
    //生成对象属性初始化后,操作明细表配置用到
    const onInited = async () => {
        configureTrackingColumns()

        // 找到PrepareMtrl列并添加自定义渲染
        const prepareMtrlColumn = columns.find(col => col.field === 'PrepareMtrl');
        if (prepareMtrlColumn) {
            prepareMtrlColumn.render = (h, { row, column, index }) => {
                // 解析PrepareMtrl字段值
                const parsePrepareMtrl = (value) => {
                    if (!value || value === '') {
                        return [];
                    }
                    try {
                        const parsed = JSON.parse(value);
                        return Array.isArray(parsed) ? parsed.filter(item => item && item.trim()) : [];
                    } catch (e) {
                        return [];
                    }
                };
                
                const prepareMtrlList = parsePrepareMtrl(row.PrepareMtrl);
                
                // 按钮配置映射
                const buttonConfig = {
                    '采购': {
                        type: 'primary',
                        path: '/OCP_LackMtrlResult_PO',
                        text: '采购缺料信息'
                    },
                    '委外': {
                        type: 'primary',
                        path: '/OCP_LackMtrlResult_WO',
                        text: '委外缺料信息'
                    },
                    '部件': {
                        type: 'primary',
                        path: '/OCP_LackMtrlResult_MO_BJ',
                        text: '部件缺料信息'
                    },
                    '金工': {
                        type: 'primary',
                        path: '/OCP_LackMtrlResult_MO_JG',
                        text: '金工缺料信息'
                    },
                    '技术': {
                        type: 'primary',
                        path: '/OCP_TechManagement',
                        text: 'BOM搭建进度表'
                    },
                    '计划': {
                        type: 'primary',
                        path: '/OCP_LackMtrlResult',
                        text: '缺料运算结果表'
                    }
                };
                
                // 生成按钮
                const buttons = prepareMtrlList.map(item => {
                    const config = buttonConfig[item];
                    if (!config) return null;
                    
                    return (
                        <el-button
                            key={item}
                            type={config.type}
                            link
                            class="tracking-ready-link"
                            onClick={($e) => {
                                $e.stopPropagation()
                                let query = row.MtoNo ? { MtoNo: row.MtoNo } : {};
                                if (item === '技术') {
                                    query = row.MtoNo ? { PlanTraceNo: row.MtoNo } : {};
                                }
                                proxy.$tabs.open({
                                    text: config.text,
                                    path: config.path,
                                    query: query
                                });
                                proxy.$tabs.clearCache(config.path.slice(1));
                            }}
                        >
                            {item}
                        </el-button>
                    );
                }).filter(Boolean);
                
                return (
                    <div class="tracking-ready-actions">
                        {buttons.length > 0 ? buttons : <span class="tracking-empty-text">-</span>}
                    </div>
                );
            };
            // 增加列宽以容纳更多按钮
            prepareMtrlColumn.width = 210;
        }

        if (!columns.some(column => column.field === 'action')) {
            columns.push({
                field: 'action',
                title: '操作',
                width: 168,
                align: 'center',
                fixed: 'right',
                render: (h, { row }) => {
                    return (
                        <div class="tracking-row-actions">
                            <el-button
                                type="success"
                                link
                                size="small"
                                onClick={($e) => {
                                    $e.stopPropagation()
                                    handleMessageBoard(row)
                                }}
                            >
                                <i class="el-icon-chat-dot-square"></i>
                                消息
                            </el-button>
                            <el-button
                                type="warning"
                                link
                                size="small"
                                onClick={($e) => {
                                    $e.stopPropagation()
                                    handleUrge(row)
                                }}
                            >
                                <i class="el-icon-bell"></i>
                                催单
                            </el-button>
                            <el-button
                                type="primary"
                                link
                                size="small"
                                onClick={($e) => {
                                    $e.stopPropagation()
                                    handleNegotiate(row)
                                }}
                            >
                                <i class="el-icon-chat-line-round"></i>
                                协商
                            </el-button>
                        </div>
                    )
                }
            })
        }
    }
    const searchBefore = async (param) => {
        //界面查询前,可以给param.wheres添加查询参数
        //返回false，则不会执行查询
        return true;
    }
    const searchAfter = async (rows, result) => {
        tableRows.value = rows || []
        tableTotal.value = result?.total || tableRows.value.length
        return true;
    }
    
    // 表格选择变化监听方法
    const selectionChange = (rows) => {
        selectedRows.value = rows || []
        console.log('selectionChange事件：选中行数量:', selectedRows.value.length)
    }
    
    // 行选择变化监听方法（备选方案）
    const rowChange = (rows) => {
        selectedRows.value = rows || []
        console.log('rowChange事件：选中行数量:', selectedRows.value.length)
    }
    
    // 手动获取选中行状态的调试函数
    const updateSelectionManually = () => {
        try {
            if (gridRef && gridRef.getSelected) {
                const selection = gridRef.getSelected()
                selectedRows.value = selection || []
                console.log('手动更新选中行数量:', selectedRows.value.length, selectedRows.value)
                console.log('按钮禁用状态:', trackingButtonDisabled.value)
                return selection
            }
        } catch (error) {
            console.warn('手动获取选中行失败:', error)
        }
        return []
    }
    
    const addBefore = async (formData) => {
        //新建保存前formData为对象，包括明细表，可以给给表单设置值，自己输出看formData的值
        return true;
    }
    const updateBefore = async (formData) => {
        //编辑保存前formData为对象，包括明细表、删除行的Id
        return true;
    }
    const rowClick = ({ row, column, event }) => {
        //查询界面点击行事件
        
        // 如果点击的是操作列，不执行选择逻辑
        if (column && (column.field === 'action' || column.property === 'action')) {
            return
        }
        
        // 单击行时选中/取消选中当前行
        if (grid.value && grid.value.toggleRowSelection) {
            grid.value.toggleRowSelection(row)
        }
        
        // 手动更新选中行状态（作为备选方案）
        try {
            if (gridRef && gridRef.getSelected) {
                const selection = gridRef.getSelected()
                selectedRows.value = selection || []
                console.log('行点击后选中行数量:', selectedRows.value.length)
            }
        } catch (error) {
            console.warn('获取选中行失败:', error)
        }
    }
    const modelOpenBefore = async (row) => {//弹出框打开后方法
        return true;//返回false，不会打开弹出框
    }
    const modelOpenAfter = (row) => {
        //弹出框打开后方法,设置表单默认值,按钮操作等
    }
    // 催单操作
    const handleUrge = (row) => {
        console.log('催单操作:', row)
        console.log('当前行完整数据:', row)
        
        // 准备催单数据 - 将当前行数据转换为催单组件需要的格式
        const reminderData = {
            BusinessType: row.BusinessType, // 业务类型
            SupplierCode: row.SupplierCode || '',
            id: row.Id || row.id,
            contractNo: row.ContractNo || row.contractNo || row.OrderNo || row.orderNo, // 使用合同号或订单号
            planTrackNo: `PT${(row.Id || row.id || '').toString().slice(-6)}`, // 生成计划跟踪号
            assignedPerson: null, // 指定负责人，初始为空
            urgentLevel: 'B', // 默认紧急等级
            replyTime: 24, // 默认回复时间
            replyTimeUnit: 'hour', // 默认单位为小时
            messageContent: '', // 消息内容初始为空
            // 保存当前行信息，用于提交时使用
            currentRowData: row
        }
        
        console.log('准备的催单数据:', reminderData)
        
        // 设置数据并打开弹窗
        followUpReminderData.value = reminderData
        followUpReminderVisible.value = true
    }

    // 处理催单发送
    const handleFollowUpSend = async (reminderData) => {
        console.log('发送催单数据:', reminderData)
        
        try {
            // 获取当前行数据
            const currentRow = reminderData.currentRowData;
            
            // 添加防护性检查
            if (!currentRow) {
                console.error('当前行数据为空:', reminderData)
                ElMessage.error('获取当前行数据失败，请重新操作')
                return;
            }
            
            console.log('当前行数据:', currentRow)
            
            // 构建提交数据，添加必要字段
            const submitData = {
                ...reminderData,
                // 行号 - 该表没有行号字段，使用空值
                Seq: '',
                // 计划跟踪号
                PlanTraceNo: currentRow.MtoNo || '',
                // 单据编号
                BillNo: currentRow.BillNo || currentRow.OrderNo || '',
                // 默认负责人姓名 - 取默认负责人的name
                DefaultResPersonName: reminderData.DefaultResPerson || '',
                // 默认负责人登录名 - 取默认负责人的loginName
                DefaultResPerson: reminderData.DefaultResPersonLoginName || '',
                // 采购负责人姓名（指定负责人）- 取选中人的userTrueName
                AssignedResPersonName: reminderData.AssignedResPerson ? reminderData.AssignedResPerson.userTrueName : '',
                // 原有的AssignedResPerson字段保持兼容
                AssignedResPerson: reminderData.AssignedResPerson ? reminderData.AssignedResPerson.userName : ''
            };
            
            // 移除临时保存的数据，避免提交到后端
            delete submitData.currentRowData;
            delete submitData.DefaultResPersonLoginName;
            
            console.log('最终提交数据:', submitData)
            
            // 调用催单接口
            const response = await proxy.http.post('/api/OCP_UrgentOrder/add', {mainData: submitData})
            
            if (response.status) {
                ElMessage.success(response.message || '催单发送成功')
                // 关闭弹窗
                followUpReminderVisible.value = false
                // 刷新表格数据
                gridRef.search()
            } else {
                ElMessage.error(response.message || '催单发送失败')
            }
        } catch (error) {
            console.error('催单发送失败:', error)
            ElMessage.error('催单发送失败，请稍后重试')
        }
    }

    // 协商操作
    const handleNegotiate = (row) => {
        console.log('协商操作:', row)
        console.log('当前行完整数据:', row)
        
        // 准备协商数据
        negotiationDialogData.value = {
            BusinessType: row.BusinessType,
            BusinessKey: row.Id || row.id,
            BillNo: row.BillNo || row.OrderNo || '',
            SupplierCode: row.SupplierCode || '',
            // 保存当前行信息，用于提交时使用
            currentRowData: row
        }
        
        // 打开发起协商弹窗
        negotiationDialogVisible.value = true
    }

    // 留言操作
    const handleMessage = (row) => {
        console.log('留言操作:', row)
        const orderNo = row.ContractNo || row.contractNo || row.OrderNo || row.orderNo || '订单'
        
        // 设置留言弹窗信息
        currentReplyRow.value = row
        replyDialogTitle.value = `对${orderNo}进行留言`
        replyDialogVisible.value = true
    }

    // 留言板操作
    const handleMessageBoard = (row) => {
        console.log('留言板操作:', row)
        // 打开留言板抽屉
        if (messageBoardRef.value) {
            messageBoardRef.value.open()
        }
        const orderNo = row.ContractNo || row.contractNo || row.OrderNo || row.orderNo || '订单'
        ElMessage.success(`查看${orderNo}的留言板`)
    }

    // 留言弹窗确认事件
    const handleReplyConfirm = (content) => {
        console.log('留言内容:', content)
        console.log('当前行数据:', currentReplyRow.value)
        
        // 这里可以调用API发送留言
        // 示例：await sendMessage({ orderId: currentReplyRow.value.Id, content })
        
        const orderNo = currentReplyRow.value?.ContractNo || currentReplyRow.value?.contractNo || 
                       currentReplyRow.value?.OrderNo || currentReplyRow.value?.orderNo || '订单'
        
        ElMessage.success(`成功发送留言到${orderNo}`)
        
        // 清空当前行数据
        currentReplyRow.value = null
    }
    
    // 留言弹窗取消事件
    const handleReplyCancel = () => {
        console.log('取消留言')
        currentReplyRow.value = null
    }


    
    // 发起协商弹窗确认事件
    const handleNegotiationConfirm = async (negotiationData) => {
        console.log('发起协商确认:', negotiationData)

        try {
            // 获取当前行数据
            const currentRow = negotiationData.currentRowData;
            
            // 添加防护性检查
            if (!currentRow) {
                console.error('当前行数据为空:', negotiationData)
                ElMessage.error('获取当前行数据失败，请重新操作')
                return;
            }
            
            console.log('当前行数据:', currentRow)
            
            // 构建提交数据，添加与催单相同的字段
            const submitData = {
                ...negotiationData,
                // 行号 - 该表没有行号字段，使用空值
                Seq: '',
                // 计划跟踪号
                PlanTraceNo: currentRow.MtoNo || '',
                // 单据编号
                BillNo: currentRow.BillNo || currentRow.OrderNo || '',
                // 默认负责人姓名 - 取默认负责人的name
                DefaultResPersonName: negotiationData.DefaultResPerson || '',
                // 默认负责人登录名 - 取默认负责人的loginName
                DefaultResPerson: negotiationData.DefaultResPersonLoginName || '',
                // 采购负责人姓名（指定负责人）- 取选中人的userTrueName
                AssignedResPersonName: negotiationData.AssignedResPerson ? negotiationData.AssignedResPerson.userTrueName : '',
                // 原有的AssignedResPerson字段保持兼容
                AssignedResPerson: negotiationData.AssignedResPerson ? negotiationData.AssignedResPerson.userName : ''
            };
            
            // 移除临时保存的数据，避免提交到后端
            delete submitData.currentRowData;
            delete submitData.DefaultResPersonLoginName;
            
            console.log('最终提交数据:', submitData)
            
            // 调用发起协商接口
            const response = await proxy.http.post('/api/OCP_Negotiation/add', {mainData: submitData})
            
            if (response.status) {
                ElMessage.success(response.message || '协商发起成功')
                // 关闭弹窗
                negotiationDialogVisible.value = false
                // 刷新表格数据
                gridRef.search()
            } else {
                ElMessage.error(response.message || '协商发起失败')
            }
        } catch (error) {
            console.error('发起协商失败:', error)
            ElMessage.error('协商发起失败，请稍后重试')
        }
    }
    
    // 发起协商弹窗取消事件
    const handleNegotiationCancel = () => {
        console.log('取消发起协商')
        negotiationDialogData.value = {}
    }

    //监听表单输入，做实时计算
    //watch(() => editFormFields.字段,(newValue, oldValue) => {	})
    //对外暴露数据
    defineExpose({
        openFollowUpReminder: (rowData) => {
            if (rowData) {
                handleUrge(rowData)
            } else {
                followUpReminderVisible.value = true
            }
        },
        closeFollowUpReminder: () => {
            followUpReminderVisible.value = false
        },
        openReplyDialog: (rowData, title = '留言') => {
            currentReplyRow.value = rowData
            replyDialogTitle.value = title
            replyDialogVisible.value = true
        },
        closeReplyDialog: () => {
            replyDialogVisible.value = false
            currentReplyRow.value = null
        },
        openNegotiationDialog: (rowData) => {
            if (rowData) {
                handleNegotiate(rowData)
            } else {
                negotiationDialogVisible.value = true
            }
        },
        closeNegotiationDialog: () => {
            negotiationDialogVisible.value = false
            negotiationDialogData.value = {}
        },
        // 调试用方法
        getSelectedRows: () => {
            return updateSelectionManually()
        },
        getButtonStatus: () => {
            return {
                selectedRowsCount: selectedRows.value.length,
                trackingButtonDisabled: trackingButtonDisabled.value,
                selectedRows: selectedRows.value
            }
        }
    })
</script>

<style lang="less" scoped>
.tracking-workbench {
  margin: 0 0 10px;
  padding: 16px 18px 14px;
  background: #fdfdfd;
  border: 1px solid #e5e9e9;
  border-radius: 6px;
}

.tracking-workbench__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 14px;
}

.tracking-workbench__title {
  color: #303384;
  font-size: 24px;
  font-weight: 700;
  line-height: 1.2;
}

.tracking-workbench__subtitle {
  margin-top: 6px;
  color: #595959;
  font-size: 13px;
  line-height: 1.4;
}

.tracking-workbench__actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 10px;
  flex-wrap: wrap;
  min-height: 32px;
}

.tracking-workbench__selected {
  color: #9c9c9f;
  font-size: 13px;
  white-space: nowrap;
}

.tracking-metrics {
  display: grid;
  grid-template-columns: repeat(6, minmax(130px, 1fr));
  gap: 10px;
}

.tracking-metric {
  display: flex;
  align-items: center;
  gap: 12px;
  min-height: 72px;
  padding: 12px 14px;
  background: #ffffff;
  border: 1px solid #e5e9e9;
  border-radius: 6px;
}

.tracking-metric__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 42px;
  height: 42px;
  flex: 0 0 42px;
  border-radius: 50%;
  font-size: 20px;
  background: #eef3fa;
  color: #0079c1;
}

.tracking-metric__label {
  color: #595959;
  font-size: 13px;
  line-height: 1.2;
  white-space: nowrap;
}

.tracking-metric__value {
  margin-top: 5px;
  color: #303384;
  font-size: 24px;
  font-weight: 700;
  line-height: 1;
}

.tracking-metric--warning {
  .tracking-metric__icon {
    background: #fff4de;
    color: #d98b00;
  }

  .tracking-metric__value {
    color: #d98b00;
  }
}

.tracking-metric--danger {
  .tracking-metric__icon {
    background: #ffe8e8;
    color: #d42828;
  }

  .tracking-metric__value {
    color: #d42828;
  }
}

.tracking-metric--blue {
  .tracking-metric__icon {
    background: #e8f3ff;
    color: #046bb6;
  }

  .tracking-metric__value {
    color: #046bb6;
  }
}

.tracking-metric--success {
  .tracking-metric__icon {
    background: #eaf8f1;
    color: #169b62;
  }

  .tracking-metric__value {
    color: #169b62;
  }
}

.tracking-metric--purple {
  .tracking-metric__icon {
    background: #f0efff;
    color: #5b45c8;
  }

  .tracking-metric__value {
    color: #5b45c8;
  }
}

.tracking-status-tag {
  min-width: 46px;
  justify-content: center;
  border-radius: 4px;
  font-weight: 600;
}

.tracking-status-tag--primary {
  color: #046bb6;
  border-color: #a8d3f2;
  background: #eef7ff;
}

.tracking-status-tag--success {
  color: #168a55;
  border-color: #b8dfcd;
  background: #eefaf4;
}

.tracking-status-tag--warning {
  color: #b76b00;
  border-color: #f2d19b;
  background: #fff7e8;
}

.tracking-status-tag--danger {
  color: #d42828;
  border-color: #f0b8b8;
  background: #fff0f0;
}

.tracking-date-cell {
  color: #595959;
  font-weight: 500;
}

.tracking-date-cell--warning {
  color: #e60012;
  font-weight: 700;
}

.tracking-main-text {
  color: #303384;
  font-weight: 600;
}

.tracking-main-text--order {
  color: #046bb6;
}

.tracking-main-text--trace {
  color: #23277d;
}

.tracking-qty-cell {
  display: inline-flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
  width: 100%;
  color: #333333;
  font-variant-numeric: tabular-nums;
  font-weight: 600;
}

.tracking-qty-cell__bar {
  position: relative;
  display: inline-block;
  width: 38px;
  height: 4px;
  overflow: hidden;
  border-radius: 10px;
  background: #e5e9e9;

  span {
    display: block;
    height: 100%;
    border-radius: inherit;
    background: #0079c1;
  }
}

.tracking-ready-actions {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 4px;
}

.tracking-ready-link {
  min-height: 22px;
  margin: 0 !important;
  padding: 1px 5px !important;
  border: 1px solid #a8d3f2;
  border-radius: 4px;
  background: #f5fbff;
  color: #046bb6;
  font-size: 12px;
  line-height: 1.2;
}

.tracking-row-actions {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;

  :deep(.el-button) {
    margin-left: 0;
    padding: 2px 3px;
  }
}

.tracking-empty-text {
  color: #9c9c9f;
}

.ocp-order-tracking-page {
  :deep(.view-container) {
    border: 1px solid #e5e9e9;
    border-radius: 6px;
    overflow: hidden;
    background: #ffffff;
  }

  :deep(.grid-search) {
    background: #fdfdfd;
  }

  :deep(.search-box),
  :deep(.fiexd-search-box) {
    padding: 12px 16px 8px;
    border-bottom: 1px solid #e5e9e9;
    background: #fdfdfd;
  }

  :deep(.view-header) {
    min-height: 44px;
    padding: 8px 14px;
    border-bottom: 1px solid #e5e9e9;
    background: #ffffff;
  }

  :deep(.desc-text) {
    color: #303384;
    font-weight: 700;
  }

  :deep(.btn-group .el-button--primary) {
    --el-button-bg-color: #0079c1;
    --el-button-border-color: #0079c1;
    --el-button-hover-bg-color: #046bb6;
    --el-button-hover-border-color: #046bb6;
  }

  :deep(.grid-container) {
    padding: 12px;
    background: #ffffff;
  }

  :deep(.el-table) {
    color: #595959;
    font-size: 13px;
  }

  :deep(.el-table th.el-table__cell) {
    background: #f5f8fc;
    color: #303384;
    font-weight: 700;
  }

  :deep(.el-table .el-table__cell) {
    padding: 7px 0;
  }

  :deep(.el-table__fixed-right),
  :deep(.el-table__fixed) {
    box-shadow: 0 0 0 transparent;
  }

  :deep(.el-table__footer-wrapper td.el-table__cell) {
    background: #f5f8fc;
    color: #303384;
    font-weight: 700;
  }

  :deep(.pagination) {
    padding: 10px 12px 14px;
    border-top: 1px solid #e5e9e9;
    background: #ffffff;
  }

  :deep(.tracking-status-tag) {
    min-width: 46px;
    justify-content: center;
    border-radius: 4px;
    font-weight: 600;
  }

  :deep(.tracking-status-tag--primary) {
    color: #046bb6;
    border-color: #a8d3f2;
    background: #eef7ff;
  }

  :deep(.tracking-status-tag--success) {
    color: #168a55;
    border-color: #b8dfcd;
    background: #eefaf4;
  }

  :deep(.tracking-status-tag--warning) {
    color: #b76b00;
    border-color: #f2d19b;
    background: #fff7e8;
  }

  :deep(.tracking-status-tag--danger) {
    color: #d42828;
    border-color: #f0b8b8;
    background: #fff0f0;
  }

  :deep(.tracking-date-cell) {
    color: #595959;
    font-weight: 500;
  }

  :deep(.tracking-date-cell--warning) {
    color: #e60012;
    font-weight: 700;
  }

  :deep(.tracking-main-text) {
    color: #303384;
    font-weight: 600;
  }

  :deep(.tracking-main-text--order) {
    color: #046bb6;
  }

  :deep(.tracking-main-text--trace) {
    color: #23277d;
  }

  :deep(.tracking-qty-cell) {
    display: inline-flex;
    align-items: center;
    justify-content: flex-end;
    gap: 8px;
    width: 100%;
    color: #333333;
    font-variant-numeric: tabular-nums;
    font-weight: 600;
  }

  :deep(.tracking-qty-cell__bar) {
    position: relative;
    display: inline-block;
    width: 38px;
    height: 4px;
    overflow: hidden;
    border-radius: 10px;
    background: #e5e9e9;
  }

  :deep(.tracking-qty-cell__bar span) {
    display: block;
    height: 100%;
    border-radius: inherit;
    background: #0079c1;
  }

  :deep(.tracking-ready-actions) {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 4px;
  }

  :deep(.tracking-ready-link) {
    min-height: 22px;
    margin: 0 !important;
    padding: 1px 5px !important;
    border: 1px solid #a8d3f2;
    border-radius: 4px;
    background: #f5fbff;
    color: #046bb6;
    font-size: 12px;
    line-height: 1.2;
  }

  :deep(.tracking-row-actions) {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 4px;
  }

  :deep(.tracking-row-actions .el-button) {
    margin-left: 0;
    padding: 2px 3px;
  }

  :deep(.tracking-empty-text) {
    color: #9c9c9f;
  }
}

@media (max-width: 1200px) {
  .tracking-metrics {
    grid-template-columns: repeat(3, minmax(160px, 1fr));
  }
}

@media (max-width: 768px) {
  .tracking-workbench__header {
    flex-direction: column;
  }

  .tracking-workbench__actions {
    justify-content: flex-start;
  }

  .tracking-metrics {
    grid-template-columns: repeat(2, minmax(140px, 1fr));
  }
}
</style>
