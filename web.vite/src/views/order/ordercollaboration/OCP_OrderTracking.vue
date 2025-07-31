<!-- 订单跟踪 -->
<template>
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
        </template>
    </view-grid>

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
    import { ref, reactive, getCurrentInstance, watch, onMounted, computed } from "vue";
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
    
    // 计算ESB按钮是否禁用
    const esbButtonDisabled = computed(() => syncLoading.value)
    
    // 计算跟踪按钮是否禁用（未选择数据时禁用）
    const trackingButtonDisabled = computed(() => selectedRows.value.length === 0)

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
    const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

    let gridRef;//对应[表.jsx]文件中this.使用方式一样
    //生成对象属性初始化
    const onInit = async ($vm) => {
        gridRef = $vm;

        gridRef.pagination.sizes = [20, 50, 100, 200, 500, 1000];
        //设置默认分页数
        gridRef.pagination.size = 20;

        gridRef.queryFields=['ContractNo', 'SOBillNo', 'MtoNo', 'IsJoinTask', 'Urgency']

        gridRef.single=true;
        
        // 设置主表合计字段
        columns.forEach(x => {
            if (x.field == 'InstockQty') {
                x.summary = true;
                x.numberLength = 2;
                x.summaryFormatter = (val, column, rows, summaryData) => {
                    if (!val) return '0.00';
                    summaryData[0] = '汇总';
                    return (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
                };
            }
            if (x.field == 'UnInstockQty') {
                x.summary = true;
                x.numberLength = 2;
            }
            if (x.field == 'OrderQty') {
                x.summary = true;
                x.numberLength = 2;
            }
            if (x.field == 'Amount') {
                x.summary = true;
                x.numberLength = 2;
                x.summaryFormatter = (val, column, rows, summaryData) => {
                    if (!val) return '0.00';
                    return '￥' + (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
                };
            }
        })
        
        const btns = [
            {       
                name: '整机跟踪',
                type: 'primary',
                plain: true,
                onClick: function () {
                    const currentSelection = updateSelectionManually()

                    // 获取选中行的MtoNo参数，如果没有选中数据则为空
                    const mtoNoList = currentSelection.map(row => row.MtoNo).filter(Boolean)
                    
                    // 构建查询参数，如果没有选中数据则不传参数
                    const query = mtoNoList.length > 0 ? { PlanTraceNo: mtoNoList.join(',') } : {}
                    
                    proxy.$tabs.open({
                        text: '整机跟踪表',
                        path: '/OCP_PrdMOTracking',
                        query: query
                    });

                    proxy.$tabs.clearCache('OCP_LackMtrlResult_MO_JG')
                }
            }
        ]

        const permissions = proxy.$store.getters.getPermission();
        const hasESBPermission = permissions.find(x => x.id === 313)?.permission?.includes('ESB_Sync');
        if (hasESBPermission) {
            btns.push({
                name: "ESB数据同步",
                type: 'primary',
                disabled: esbButtonDisabled,
                onClick: async function () {
                    // 防止重复点击
                    if (syncLoading.value) {
                        return
                    }
                    
                    try {
                        // 设置loading状态
                        syncLoading.value = true
                        
                        // 显示加载状态
                        proxy.$message.info('正在同步数据...')
                        
                        // 调用API接口
                        const result = await proxy.http.post('/api/OCP_OrderTracking/ManualSyncOrderData')
                        
                        // 成功提示
                        proxy.$message.success('ESB数据同步成功')
                        console.log('ESB数据同步成功:', result)
                        
                        // 可选：刷新当前页面数据
                        // gridRef.search()
                        
                    } catch (error) {
                        console.error('ESB数据同步失败:', error)
                        proxy.$message.error('ESB数据同步失败：' + (error.message || '未知错误'))
                    } finally {
                        // 无论成功失败都要重置loading状态
                        syncLoading.value = false
                    }
                }
            })
        }
        gridRef.buttons.push(...btns)
    }
    //生成对象属性初始化后,操作明细表配置用到
    const onInited = async () => {
        
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
                            icon={config.icon}
                            style={{ margin: '0' }}
                            onClick={() => {
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
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '2px' }}>
                        {buttons.length > 0 ? buttons : <span style={{ color: '#999' }}>-</span>}
                    </div>
                );
            };
            // 增加列宽以容纳更多按钮
            prepareMtrlColumn.width = 120;
        }
        
        // columns.push({
        //     field: 'action',
        //     title: '操作',
        //     width: 200,
        //     align: 'center',
        //     fixed: 'right',
        //     render: (h, { row, column, index }) => {
        //         return (
        //         <div style={{ display: 'flex', gap: '6px', alignItems: 'center', justifyContent: 'center' }}>
        //             <el-button 
        //             type="warning" 
        //             size="small" 
        //             onClick={($e) => handleUrge(row)}
        //             >
        //             催单
        //             </el-button>
        //             <el-button 
        //             type="primary" 
        //             size="small" 
        //             style={{ marginLeft: '0px' }}
        //             onClick={($e) => handleNegotiate(row)}
        //             >
        //             协商
        //             </el-button>
                    

        //         </div>
        //         )
        //     }
        // })
    }
    const searchBefore = async (param) => {
        //界面查询前,可以给param.wheres添加查询参数
        //返回false，则不会执行查询
        return true;
    }
    const searchAfter = async (rows, result) => {
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
        if (column && column.field === 'action') {
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
                // 行号
                LineNumber: currentRow.LineNumber || '',
                // 计划跟踪号
                PlanTraceNo: currentRow.PlanTraceNo || '',
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
                // 行号
                LineNumber: currentRow.LineNumber || '',
                // 计划跟踪号
                PlanTraceNo: currentRow.PlanTraceNo || '',
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
.tracking-content {
    padding: 20px;
    text-align: center;
    font-size: 16px;
}
</style>
