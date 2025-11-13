<!-- 缺料运算结果 -->
<template>
    <div class="shortage-result-container">
        <!-- 左侧运算方案列表 -->
        <div v-if="!BillTypeMap[sourceKey]" class="scheme-panel">
            <div class="scheme-header">
                <h3 class="scheme-title">运算方案</h3>
                <div class="scheme-search">
                    <el-input
                        v-model="schemeKeyword"
                        placeholder="请输入方案名称关键字"
                        size="small"
                        clearable
                        @keyup.enter="searchSchemes"
                        @clear="clearSearch"
                    >
                        <template #append>
                            <el-button @click="searchSchemes" :loading="schemeLoading">
                                查询
                            </el-button>
                        </template>
                    </el-input>
                </div>
            </div>
            <div 
                v-infinite-scroll="loadMore" 
                class="scheme-list"
                :infinite-scroll-disabled="!initialized || schemeLoading || noMore"
                infinite-scroll-distance="50"
                infinite-scroll-immediate="false"
            >
                <div
                    v-for="item in schemeList"
                    :key="item.ComputeID"
                    class="scheme-item"
                    :class="{ 'active': selectedSchemeId === item.ComputeID }"
                    @click="selectScheme(item)"
                    @mouseenter="item._hover = true"
                    @mouseleave="item._hover = false"
                >
                    <i v-if="item.IsDefault === 1" class="el-icon-star-on scheme-default-icon"></i>
                    <div class="scheme-name">{{ item.PlanName }}</div>
                    <!-- 默认方案显示取消默认按钮 -->
                    <el-button
                        v-if="item.IsDefault === 1 && item._hover && hasSetDefaultSchemePermission"
                        class="set-default-btn"
                        type="warning"
                        size="small"
                        @click.stop="cancelDefaultScheme(item)"
                    >
                        取消默认
                    </el-button>
                    <!-- 非默认方案在hover时显示设为默认按钮 -->
                    <el-button
                        v-if="item._hover && item.IsDefault !== 1 && hasSetDefaultSchemePermission"
                        class="set-default-btn"
                        type="primary"
                        size="small"
                        @click.stop="setDefaultScheme(item)"
                    >
                        设为默认
                    </el-button>
                </div>
                <div v-if="schemeList.length === 0 && !schemeLoading" class="no-data">
                    暂无数据
                </div>
                <div v-if="schemeLoading" class="loading-text">
                    加载中...
                </div>
                <div v-if="noMore && schemeList.length > 0" class="no-more-text">
                    没有更多数据了
                </div>
            </div>
        </div>

        <!-- 右侧原有表格 -->
        <div class="grid-panel">
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
                       :modelOpenBefore="modelOpenBefore"
                       :modelOpenAfter="modelOpenAfter">
                <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
                <template #gridHeader>
                </template>
            </view-grid>
        </div>
    </div>

    <!-- 留言板组件 -->
    <MessageBoard ref="messageBoardRef" />
    
    <!-- 催单弹窗组件 -->
    <FollowUpReminder
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
    import extend from "@/extension/order/ordercollaboration/OCP_LackMtrlResult.jsx";
    import viewOptions from './OCP_LackMtrlResult/options.js'
    import MessageBoard from '@/comp/message-board/index.vue'
    import FollowUpReminder from '@/comp/follow-up-reminder/index.vue'
    import ReplyDialog from '@/comp/reply-dialog/index.vue'
    import NegotiationDialog from '@/comp/negotiation-dialog/index.vue'
    import { ref, reactive, getCurrentInstance, watch, onMounted, computed } from "vue";
    import { ElMessage } from 'element-plus'
    import { useRoute } from 'vue-router';

    const route = useRoute();
    const grid = ref(null);
    const { proxy } = getCurrentInstance()
    const messageBoardRef = ref(null)
    const sourceKey = window.location.hash.split('?')[0];

    const BillTypeMap = {
        '#/OCP_LackMtrlResult_PO': '标准采购',
        '#/OCP_LackMtrlResult_WO': '标准委外',
        '#/OCP_LackMtrlResult_MO_JG': '金工车间',
        '#/OCP_LackMtrlResult_MO_BJ': '部件车间',
        // '#/OCP_LackMtrlResult_MO_ZJ': '整机车间'
    }

    // 业务类型映射 - 用于供应商字段权限控制
    const BusinessTypeMap = {
        '#/OCP_LackMtrlResult_PO': 'PO',    // 采购缺料 - 需要权限控制
        '#/OCP_LackMtrlResult_WO': 'PO',    // 委外缺料 - 需要权限控制
        '#/OCP_LackMtrlResult_MO_JG': 'MO', // 金工车间 - 不需要权限控制
        '#/OCP_LackMtrlResult_MO_BJ': 'MO', // 部件车间 - 不需要权限控制
    }
    // 催单弹窗相关
    const followUpReminderVisible = ref(false)
    const followUpReminderData = ref([])
    
    // 留言弹窗相关
    const replyDialogVisible = ref(false)
    const replyDialogTitle = ref('留言')
    const currentReplyRow = ref(null)
    
    // 发起协商弹窗相关
    const negotiationDialogVisible = ref(false)
    const negotiationDialogData = ref({})
    
    //http请求，proxy.http.post/get
    const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

    // 运算方案相关数据
    const schemeList = ref([])
    const selectedSchemeId = ref(null)
    const schemeLoading = ref(false)
    const currentPage = ref(1)
    const pageSize = ref(50)
    const noMore = ref(false)
    const initialized = ref(false)
    const schemeKeyword = ref('')
    
    // ESB数据同步loading状态
    const syncLoading = ref(false)
    
    // 计算ESB按钮是否禁用
    const esbButtonDisabled = computed(() => !selectedSchemeId.value || syncLoading.value)

    const hasSetDefaultSchemePermission = computed(() => {
        const permissions = proxy.$store.getters.getPermission();
        const hasPermission = permissions.find(x => x.id === 314)?.permission?.includes('SetDefault');
        return hasPermission;
    })

    let gridRef;//对应[表.jsx]文件中this.使用方式一样
    //生成对象属性初始化
    const onInit = async ($vm) => {
        gridRef = $vm;
        gridRef.pagination.sizes = [20, 50, 100, 200, 500, 1000];
        //设置默认分页数
        gridRef.pagination.size = 20;
        // gridRef.setFixedSearchForm(true);
        
        // 设置快捷查询字段
        gridRef.queryFields = ['MtoNo', 'MaterialNumber', 'MaterialName', 'BillNo'];
        
        //与jsx中的this.xx使用一样，只需将this.xx改为gridRef.xx

        // 设置主表合计字段
        columns.forEach(x => {
            console.log(x.field, x.title)
            if (sourceKey === '#/OCP_LackMtrlResult_PO') {
                const hiddenFields = ['TopSpecification', 'FinishDate', 'POCreateDate', 'WWReleaseDate', 'PickMtrlDate', 'StartDate', 'MOCreateDate']
                if (hiddenFields.includes(x.field)) {
                    x.hidden = true;
                }

                if (x.field === 'PurchaserName') {
                    x.title = '采购负责人'
                }
            }

            if (sourceKey === '#/OCP_LackMtrlResult_WO') {
                const hiddenFields = ['MOCreateDate', 'FinishDate', 'POCreateDate', 'PlanDeliveryDate', 'CGSQAuditDate', 'StartDate', 'TopSpecification']
                if (hiddenFields.includes(x.field)) {
                    x.hidden = true;
                }

                if (x.field === 'PurchaserName') {
                    x.title = '采购负责人'
                }
            }

            if (sourceKey === '#/OCP_LackMtrlResult_MO_JG') {
                const hiddenFields = ['TopSpecification', 'CGSQAuditDate', 'WWReleaseDate', 'FinishDate', 'POCreateDate', 'POAuditDate']
                if (hiddenFields.includes(x.field)) {
                    x.hidden = true;
                }

                if (x.field === 'PurchaserName') {
                    x.title = '计划负责人'
                }
            }
            
            if (sourceKey === '#/OCP_LackMtrlResult_MO_BJ') {
                const hiddenFields = ['TopSpecification', 'CGSQAuditDate', 'WWReleaseDate', 'FinishDate', 'POCreateDate', 'POAuditDate']
                if (hiddenFields.includes(x.field)) {
                    x.hidden = true;
                }

                if (x.field === 'PurchaserName') {
                    x.title = '计划负责人'
                }

                if (x.field === 'SupplierName') {
                    x.title = '车间'
                }

                if (x.field === 'POQty') {
                    x.title = '生产数量'
                }

                if (x.field == 'PlanDeliveryDate') {
                    x.title = '交货日期'
                }
            }

            if (x.field === 'NeedQty') {
                x.summary = true;
                x.numberLength = 2;
                x.summaryFormatter = (val, column, rows, summaryData) => {
                    if (!val) return '0.00';
                    summaryData[0] = '汇总';
                    return (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
                };
            }
            if (x.field === 'InventoryQty') {
                x.summary = true;
                x.numberLength = 2;
            }
            if (x.field === 'PlanedQty') {
                x.summary = true;
                x.numberLength = 2;
            }
            if (x.field === 'UnPlanedQty') {
                x.summary = true;
                x.numberLength = 2;
            }
        })

        const query = route.query;
        for (const key in query) {
            if (query[key]) {
                // 对URL参数进行解码
                gridRef.searchFormFields[key] = decodeURIComponent(query[key]);
            }
        }

        if (sourceKey === '#/OCP_LackMtrlResult_PO') {
            columns.push({
                    field: '功能',
                    title: '功能',
                    width: 150,
                    align: 'center',
                    render: (h, { row, column, index }) => {
                        return (
                            <div>
                                <el-button 
                                    type="primary" 
                                    link 
                                    style={{padding: '2px 4px', fontSize: '12px'}} 
                                    onClick={($e) => handleJumpToTrackPO(row)}>跳转跟踪</el-button>
                                {/* <el-button 
                                    type="primary" 
                                    link 
                                    style={{padding: '2px 4px', fontSize: '12px'}} 
                                    onClick={($e) => handleJumpToShortage(row)}>跳转采购</el-button> */}
                            </div>
                        )
                    }
                })
        }

        if (sourceKey === '#/OCP_LackMtrlResult_WO') {
            columns.push({
                    field: '功能',
                    title: '功能',
                    width: 150,
                    align: 'center',
                    render: (h, { row, column, index }) => {
                        return (
                            <div>
                                <el-button 
                                    type="primary" 
                                    link 
                                    style={{padding: '2px 4px', fontSize: '12px'}} 
                                    onClick={($e) => handleJumpToTrackWO(row)}>跳转跟踪</el-button>
                                {/* <el-button 
                                    type="primary" 
                                    link 
                                    style={{padding: '2px 4px', fontSize: '12px'}} 
                                    onClick={($e) => handleJumpToSubOrder(row)}>跳转委外</el-button> */}
                            </div>
                        )
                    }
                })
        }

        if (sourceKey === '#/OCP_LackMtrlResult_MO_JG') {
            columns.push({
                    field: '功能',
                    title: '功能',
                    width: 150,
                    align: 'center',
                    render: (h, { row, column, index }) => {
                        return (
                            <div>
                                <el-button 
                                    type="primary" 
                                    link 
                                    style={{padding: '2px 4px', fontSize: '12px'}} 
                                    onClick={($e) => handleJumpToTrackJG(row)}>跳转跟踪</el-button>
                                {/* <el-button 
                                    type="primary" 
                                    link 
                                    style={{padding: '2px 4px', fontSize: '12px'}} 
                                    onClick={($e) => handleJumpToJGPrdMO(row)}>跳转金工</el-button> */}
                            </div>
                        )
                    }
                })
        }

        if (sourceKey === '#/OCP_LackMtrlResult_MO_BJ') {
            columns.push({
                    field: '功能',
                    title: '功能',
                    width: 150,
                    align: 'center',
                    render: (h, { row, column, index }) => {
                        return (
                            <div>
                                <el-button 
                                    type="primary" 
                                    link 
                                    style={{padding: '2px 4px', fontSize: '12px'}} 
                                    onClick={($e) => handleJumpToTrackBJ(row)}>跳转跟踪</el-button>
                                {/* <el-button 
                                    type="primary" 
                                    link 
                                    style={{padding: '2px 4px', fontSize: '12px'}} 
                                    onClick={($e) => handleJumpToPartPrdMO(row)}>跳转部件</el-button> */}
                            </div>
                        )
                    }
                })
        }

        const btns = [{
            name: "ESB数据同步",
            type: 'primary',
            disabled: esbButtonDisabled,
            onClick: async function () {
                // 检查是否选中了运算方案
                if (!selectedSchemeId.value) {
                    proxy.$message.warning('请先选择一个运算方案')
                    return
                }
                
                // 防止重复点击
                if (syncLoading.value) {
                    return
                }
                
                try {
                    // 设置loading状态
                    syncLoading.value = true
                    
                    // 显示加载状态
                    proxy.$message.info('正在同步数据...')
                    
                    // 调用API接口，传递ComputeID
                    const requestBody = {
                        ComputeID: selectedSchemeId.value
                    }
                    
                    const result = await proxy.http.post(`/api/OCP_LackMtrlResult/ManualSyncLackMtrlData?ComputeID=${selectedSchemeId.value}`)
                    
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
        }]
        gridRef.buttons.push(...btns)
    }
    //生成对象属性初始化后,操作明细表配置用到
    const onInited = async () => {
        // 初始化时加载运算方案列表
        await loadSchemes(true)
        // 设置初始化完成标记，防止无限滚动立即触发
        setTimeout(() => {
            initialized.value = true
        }, 100)
        
        if (!BillTypeMap[sourceKey]) {
            // 添加操作列
            // columns.push({
            //     field: 'action',
            //     title: '操作',
            //     width: 250,
            //     align: 'center',
            //     fixed: 'right',
            //     render: (h, { row, column, index }) => {
            //         return (
            //         <div style={{ display: 'flex', gap: '6px', alignItems: 'center', justifyContent: 'center' }}>
            //             <el-button 
            //             type="success" 
            //             size="small" 
            //             onClick={($e) => handleOpenMessageBoard(row)}
            //             >
            //             消息
            //             </el-button>
            //             <el-button 
            //             type="warning" 
            //             size="small" 
            //             style={{ marginLeft: '0px' }}
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
                        
            //             {/* <el-dropdown
            //             trigger="click"
            //             v-slots={{
            //                 dropdown: () => (
            //                 <el-dropdown-menu>
            //                     <el-dropdown-item>
            //                     <div
            //                         onClick={($e) => {
            //                         dropdownClick('留言', row, column, index, $e);
            //                         }}
            //                     >
            //                         留言
            //                     </div>
            //                     </el-dropdown-item>
            //                     <el-dropdown-item>
            //                     <div
            //                         onClick={($e) => {
            //                         dropdownClick('留言板', row, column, index, $e);
            //                         }}
            //                     >
            //                         留言板
            //                     </div>
            //                     </el-dropdown-item>
            //                 </el-dropdown-menu>
            //                 )
            //             }}
            //             >
            //             <span style={{ fontSize: '13px', color: '#409eff', cursor: 'pointer' }} class="el-dropdown-link">
            //                 更多<i class="el-icon-arrow-down" style={{ marginLeft: '4px' }}></i>
            //             </span>
            //             </el-dropdown> */}
            //         </div>
            //         )
            //     }
            // })
        }
    }
    
    // 加载运算方案数据
    const loadSchemes = async (reset = false) => {
        try {
            schemeLoading.value = true
            
            if (reset) {
                currentPage.value = 1
                schemeList.value = []
                noMore.value = false
            }
            
            // 构建查询条件
            let wheres = []
            if (schemeKeyword.value && schemeKeyword.value.trim()) {
                wheres.push({
                    name: 'PlanName',
                    value: schemeKeyword.value.trim(),
                    displayType: 'like'
                })
            }
            
            const requestBody = {
                page: currentPage.value,
                rows: pageSize.value,
                sort: "CreateDate",
                order: "desc",
                wheres: JSON.stringify(wheres),
                resetPage: true
            }
            
            const response = await proxy.http.post('/api/OCP_LackMtrlPlan/getPageData', requestBody)
            
            if (response.status === 0) {
                const newData = response.rows || []
                
                if (reset) {

                    schemeList.value = newData
                    let defaultScheme = newData.find(item => item.IsDefault === 1)
                    if (defaultScheme) {
                        selectedSchemeId.value = defaultScheme.ComputeID
                        // 将选中的运算方案ID保存到 sessionStorage,供导出功能使用
                        sessionStorage.setItem('OCP_LackMtrlResult_selectedSchemeId', defaultScheme.ComputeID)
                        console.log('自动选中默认运算方案:', defaultScheme)
                    } else if (newData.length > 0) {
                        selectedSchemeId.value = newData[0].ComputeID
                        // 将选中的运算方案ID保存到 sessionStorage,供导出功能使用
                        sessionStorage.setItem('OCP_LackMtrlResult_selectedSchemeId', newData[0].ComputeID)
                        console.log('自动选中第一个运算方案:', newData[0])
                    }
                } else {
                    // 根据ComputeID去重，新数据覆盖旧数据
                    const newDataIds = new Set(newData.map(item => item.ComputeID))
                    // 移除已存在的同ComputeID的旧数据
                    schemeList.value = schemeList.value.filter(item => !newDataIds.has(item.ComputeID))
                    // 添加所有新数据
                    schemeList.value.push(...newData)
                }
                
                // 判断是否还有更多数据
                if (newData.length < pageSize.value) {
                    noMore.value = true
                } else {
                    currentPage.value++
                }
            } else {
                proxy.$message.error(response.msg || '查询运算方案失败')
                if (reset) {
                    schemeList.value = []
                }
            }
        } catch (error) {
            console.error('加载运算方案失败:', error)
            proxy.$message.error('查询运算方案失败')
            if (reset) {
                schemeList.value = []
            }
        } finally {
            schemeLoading.value = false
        }
    }
    
    // 搜索运算方案
    const searchSchemes = async () => {
        console.log('搜索关键字:', schemeKeyword.value)
        await loadSchemes(true) // 重置并加载数据，会自动选中第一个方案
    }
    
    // 清空搜索
    const clearSearch = async () => {
        schemeKeyword.value = ''
        await loadSchemes(true) // 重新加载所有数据，会自动选中第一个方案
    }
    
    // 滚动加载更多
    const loadMore = async () => {
        console.log('触发滚动加载', {
            initialized: initialized.value,
            schemeLoading: schemeLoading.value,
            noMore: noMore.value,
            listLength: schemeList.value.length
        })
        
        if (!initialized.value || schemeLoading.value || noMore.value) {
            console.log('滚动加载被阻止')
            return
        }
        
        console.log('开始加载更多数据')
        await loadSchemes(false)
    }
    
    // 选择运算方案
    const selectScheme = (scheme) => {
        selectedSchemeId.value = scheme.ComputeID
        // 将选中的运算方案ID保存到 sessionStorage,供导出功能使用
        sessionStorage.setItem('OCP_LackMtrlResult_selectedSchemeId', scheme.ComputeID)
        console.log('选中运算方案:', scheme)
        // 这里可以根据选中的方案更新右侧表格数据
        // 可以触发右侧表格刷新
        if (gridRef && gridRef.search) {
            gridRef.search()
        }
    }
    const searchBefore = async (param) => {
        //界面查询前,可以给param.wheres添加查询参数
        param.wheres = param.wheres || []

        // 添加BillType条件
        if (BillTypeMap[sourceKey]) {
            param.wheres.push({ name: 'BillType', value: BillTypeMap[sourceKey] })
        }

        // 添加CustomerParams用于供应商字段权限控制
        param.customerParams = param.customerParams || {}
        if (BusinessTypeMap[sourceKey]) {
            param.customerParams.BusinessType = BusinessTypeMap[sourceKey]
            console.log('设置BusinessType:', BusinessTypeMap[sourceKey], '用于路由:', sourceKey)
        }

        //如果选中了运算方案，可以将方案ID作为查询条件
        if (selectedSchemeId.value) {
            param.wheres.push({ name: 'ComputeID', value: selectedSchemeId.value })
            return true;
        }
        //返回false，则不会执行查询
        return false;
    }
    const searchAfter = async (rows, result) => {
        return true;
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
        // grid.value.toggleRowSelection(row); //单击行时选中当前行;
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
        
        // 准备催单数据 - 将当前行数据转换为催单组件需要的格式
        const reminderData = [{
            id: row.Id || row.id,
            contractNo: row.MaterialCode || row.materialCode || row.ItemCode || row.itemCode, // 使用物料编码
            planTrackNo: `LM${(row.Id || row.id || '').toString().slice(-6)}`, // 生成缺料跟踪号
            business: 'material', // 业务类型为物料
            defaultResponsible: '采购部门', // 默认负责人
            assignedPersons: [], // 指定负责人，初始为空
            urgentLevel: 'B', // 默认紧急等级
            replyTime: 24, // 默认回复时间
            replyTimeUnit: 'hour', // 默认单位为小时
            messageContent: '' // 消息内容初始为空
        }]
        
        // 设置数据并打开弹窗
        followUpReminderData.value = reminderData
        followUpReminderVisible.value = true
    }

    // 处理催单发送
    const handleFollowUpSend = (selectedData) => {
        console.log('发送催单数据:', selectedData)
        
        // 这里可以调用API发送催单消息
        // 示例：await sendFollowUpReminder(selectedData)
        
        ElMessage.success(`成功发送 ${selectedData.length} 条催单消息`)
        
        // 关闭弹窗
        followUpReminderVisible.value = false
    }

    // 协商操作
    const handleNegotiate = (row) => {
        console.log('协商操作:', row)
        
        // 准备协商数据
        negotiationDialogData.value = {
            id: row.Id || row.id,
            materialCode: row.MaterialCode || row.materialCode,
            materialName: row.MaterialName || row.materialName,
            itemCode: row.ItemCode || row.itemCode,
            itemName: row.ItemName || row.itemName,
            computeId: selectedSchemeId.value, // 添加运算方案ID
            // 可以添加更多需要的字段
            ...row
        }
        
        // 打开发起协商弹窗
        negotiationDialogVisible.value = true
    }

    // 留言操作
    const handleMessage = (row) => {
        console.log('留言操作:', row)
        const materialName = row.MaterialName || row.materialName || row.ItemName || row.itemName || '物料'
        
        // 设置留言弹窗信息
        currentReplyRow.value = row
        replyDialogTitle.value = `对${materialName}进行留言`
        replyDialogVisible.value = true
    }

    // 留言板操作
    const handleMessageBoard = (row) => {
        console.log('留言板操作:', row)
        // 打开留言板抽屉
        if (messageBoardRef.value) {
            messageBoardRef.value.open()
        }
        const materialName = row.MaterialName || row.materialName || row.ItemName || row.itemName || '物料'
        ElMessage.success(`查看${materialName}的留言板`)
    }

    // 留言弹窗确认事件
    const handleReplyConfirm = (content) => {
        console.log('留言内容:', content)
        console.log('当前行数据:', currentReplyRow.value)
        
        // 这里可以调用API发送留言
        // 示例：await sendMessage({ materialId: currentReplyRow.value.Id, content })
        
        const materialName = currentReplyRow.value?.MaterialName || currentReplyRow.value?.materialName || 
                            currentReplyRow.value?.ItemName || currentReplyRow.value?.itemName || '物料'
        
        ElMessage.success(`成功发送留言到${materialName}`)
        
        // 清空当前行数据
        currentReplyRow.value = null
    }
    
    // 留言弹窗取消事件
    const handleReplyCancel = () => {
        console.log('取消留言')
        currentReplyRow.value = null
    }

    // 下拉菜单点击处理
    const dropdownClick = (value, row, column, index, $e) => {
        switch(value) {
            case '留言':
                handleMessage(row)
                break
            case '留言板':
                handleMessageBoard(row)
                break
            default:
                console.log('未知的下拉操作:', value)
        }
    }

    // 发起协商弹窗确认事件
    const handleNegotiationConfirm = (data) => {
        console.log('发起协商确认:', data)
        
        const materialName = data.materialName || data.itemName || '物料'
        
        // 这里可以调用API提交协商数据
        // 示例：await submitNegotiation(data)
        
        ElMessage.success(`成功发起对${materialName}的协商`)
    }
    
    // 发起协商弹窗取消事件
    const handleNegotiationCancel = () => {
        console.log('取消发起协商')
        negotiationDialogData.value = {}
    }

    // 跳转采购未完跟踪表
    const handleJumpToTrackPO = (row) => {
        console.log('跳转采购未完跟踪表:', row)
        const { MaterialNumber, BillNo } = row;

        proxy.$tabs.open({
            text: '采购未完跟踪表',
            path: '/OCP_POUnFinishTrack',
            query: {
                MaterialCode: encodeURIComponent(MaterialNumber || ''),
                POBillNo: encodeURIComponent(BillNo || '')
            }
        });

        proxy.$tabs.clearCache('OCP_POUnFinishTrack')
    }

    // 跳转采购
    const handleJumpToShortage = (row) => {
        console.log('跳转采购:', row)
        const { MaterialNumber, BillNo } = row;

        proxy.$tabs.open({
            text: '采购订单表',
            path: '/OCP_PurchaseOrder',
            query: {
                MaterialNumber: encodeURIComponent(MaterialNumber || ''),
                BillNo: encodeURIComponent(BillNo || '')
            }
        });

        proxy.$tabs.clearCache('OCP_PurchaseOrder')
    }

    // 跳转委外未完跟踪表
    const handleJumpToTrackWO = (row) => {
        console.log('跳转委外未完跟踪:', row)
        const { MaterialNumber, BillNo } = row;

        proxy.$tabs.open({
            text: '委外未完跟踪',
            path: '/OCP_SubOrderUnFinishTrack',
            query: {
                MaterialNumber: encodeURIComponent(MaterialNumber || ''),
                POBillNo: encodeURIComponent(BillNo || '')
            }
        });

        proxy.$tabs.clearCache('OCP_SubOrderUnFinishTrack')
    }

    // 跳转委外
    const handleJumpToSubOrder = (row) => {
        console.log('跳转委外:', row)
        const { MaterialNumber, BillNo } = row;

        proxy.$tabs.open({
            text: '委外订单表',
            path: '/OCP_SubOrder',
            query: {
                SubOrderNo: encodeURIComponent(BillNo || '')
            }
        });

        proxy.$tabs.clearCache('OCP_SubOrder')
    }

    // 跳转金工未完跟踪表
    const handleJumpToTrackJG = (row) => {
        console.log('跳转金工未完跟踪:', row)
        const { MaterialNumber, BillNo } = row;

        proxy.$tabs.open({
            text: '金工未完工跟踪',
            path: '/OCP_JGUnFinishTrack',
            query: {
                MaterialNumber: encodeURIComponent(MaterialNumber || ''),
                ProductionOrderNo: encodeURIComponent(BillNo || '')
            }
        });

        proxy.$tabs.clearCache('OCP_JGUnFinishTrack')
    }

    // 跳转金工生产工单
    const handleJumpToJGPrdMO = (row) => {
        console.log('跳转金工生产工单:', row)
        const { MaterialNumber, BillNo } = row;

        proxy.$tabs.open({
            text: '金工生产工单表',
            path: '/OCP_JGPrdMO',
            query: {
                MaterialNumber: encodeURIComponent(MaterialNumber || ''),
                ProductionOrderNo: encodeURIComponent(BillNo || '')
            }
        });

        proxy.$tabs.clearCache('OCP_JGPrdMO')
    }

    // 跳转部件未完跟踪表
    const handleJumpToTrackBJ = (row) => {
        console.log('跳转部件未完跟踪:', row)
        const { MaterialNumber, BillNo } = row;

        proxy.$tabs.open({
            text: '部件未完跟踪',
            path: '/OCP_PartUnFinishTracking',
            query: {
                MaterialCode: encodeURIComponent(MaterialNumber || ''),
                ProductionOrderNo: encodeURIComponent(BillNo || '')
            }
        });

        proxy.$tabs.clearCache('OCP_PartUnFinishTracking')
    }

    // 跳转部件生产工单
    const handleJumpToPartPrdMO = (row) => {
        console.log('跳转部件生产工单:', row)
        const { MaterialNumber, BillNo } = row;

        proxy.$tabs.open({
            text: '部件生产工单表',
            path: '/OCP_PartPrdMO',
            query: {
                ProductionOrderNo: encodeURIComponent(BillNo || '')
            }
        });

        proxy.$tabs.clearCache('OCP_PartPrdMO')
    }

    // 设为默认方案
    const setDefaultScheme = async (item) => {
        try {
            // 发起POST请求
            const res = await proxy.http.post('/api/OCP_LackMtrlPlan/SetDefaultPlan', {
                computeId: item.ComputeID
            })
            if (res && res.status) {
                ElMessage.success(`已设为默认方案：${item.PlanName}`)
                await loadSchemes()
            } else {
                ElMessage.error(res?.msg || '设为默认方案失败')
            }
        } catch (err) {
            ElMessage.error('设为默认方案失败')
            console.error(err)
        }
    }

    // 取消默认方案
    const cancelDefaultScheme = async (item) => {
        try {
            // 发起POST请求
            const res = await proxy.http.post('/api/OCP_LackMtrlPlan/CancelDefaultPlan', {
                computeId: item.ComputeID
            })
            if (res && res.status) {
                ElMessage.success(`已取消默认方案：${item.PlanName}`)
                await loadSchemes()
            } else {
                ElMessage.error(res?.msg || '取消默认方案失败')
            }
        } catch (err) {
            ElMessage.error('取消默认方案失败')
            console.error(err)
        }
    }

    // 打开留言板
    const handleOpenMessageBoard = (row) => {
        console.log('打开留言板:', row)
        if (messageBoardRef.value) {
            // 使用缺料业务类型和物料ID
            const businessKey = row.Id || row.id || row.MaterialID || row.MaterialCode
            messageBoardRef.value.open('LM', businessKey)
        }
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
        }
    })
</script>

<style scoped>
.shortage-result-container {
    display: flex;
    height: 100%;
    gap: 12px;
}

.scheme-panel {
    width: 300px;
    display: flex;
    flex-direction: column;
    border: 1px solid #e4e7ed;
    border-radius: 4px;
    background-color: #fff;
}

.scheme-header {
    flex-shrink: 0;
    padding: 16px;
    border-bottom: 1px solid #e4e7ed;
}

.scheme-title {
    margin: 0 0 12px 0;
    font-size: 16px;
    font-weight: 600;
    color: #303133;
}

.scheme-search {
    margin-top: 12px;
}

.search-tip {
    font-size: 12px;
    color: #909399;
    margin-top: 8px;
    text-align: center;
}

.scheme-list {
    flex: 1;
    overflow-y: auto;
    padding: 8px;
}

.scheme-item {
    padding: 12px;
    margin-bottom: 8px;
    border: 1px solid #e4e7ed;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.3s;
    background-color: #fff;
    font-size: 14px;
    position: relative;

    .scheme-default-icon {
        position: absolute;
        right: 0px;
        top: 0px;
        font-size: 16px;
        color: #409eff;
    }
}

.scheme-item:hover {
    border-color: #409eff;
    background-color: #f0f9ff;
}

.scheme-item.active {
    border-color: #409eff;
    background-color: #ecf5ff;
}

.scheme-name {
    font-weight: 500;
    color: #303133;
    margin-bottom: 4px;
}

.scheme-desc {
    font-size: 12px;
    color: #909399;
    line-height: 1.4;
}

.no-data {
    text-align: center;
    padding: 40px 20px;
    color: #909399;
    font-size: 14px;
}

.loading-text {
    text-align: center;
    padding: 20px;
    color: #909399;
    font-size: 14px;
}

.no-more-text {
    text-align: center;
    padding: 16px 20px;
    color: #c0c4cc;
    font-size: 12px;
}

.grid-panel {
    flex: 1;
    overflow: hidden;
}

/* 确保内容不超出容器高度 */
.scheme-list {
    max-height: calc(100vh - 200px);
}

.set-default-btn {
    position: absolute;
    right: 16px;
    top: 50%;
    transform: translateY(-50%);
    pointer-events: none;
    transition: opacity 0.2s;
    z-index: 2;
}

.scheme-item:hover .set-default-btn {
    opacity: 1;
    pointer-events: auto;
}
</style>
