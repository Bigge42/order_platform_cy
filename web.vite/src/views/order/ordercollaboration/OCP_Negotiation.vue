<!--
 *Author：jxx
 *Date：{Date}
 *Contact：461857658@qq.com
 *业务请在@/extension/order/ordercollaboration/OCP_Negotiation.jsx或OCP_Negotiation.vue文件编写
 *新版本支持vue或【表.jsx]文件编写业务,
 -->
<template>
    <div class="negotiation-page">
        <!-- 消息统计组件 -->
        <div class="stats-section">
            <div class="section-header">
                <h3 class="section-title">协商统计概览</h3>
            </div>
            <MessageCount 
                :count-list="messageStats"
                :selected-type="selectedMessageType"
                @item-click="handleStatClick"
            />
        </div>

        <!-- 消息列表区域 -->
        <div class="message-section">
            <!-- Tabs切换器 -->
            <div class="tabs-header">
                <el-tabs 
                    v-model="activeTab" 
                    @tab-click="handleTabClick"
                >
                    <el-tab-pane 
                        v-for="tab in messageTabs" 
                        :key="tab.name"
                        :name="tab.name"
                    >
                        <template #label>
                            <div class="custom-tabs-label">
                                <el-icon><calendar /></el-icon>
                                <span>{{ tab.label }}</span>
                                <span v-if="tab.count" class="item-qty">{{ tab.count }}</span>
                            </div>
                        </template>
                    </el-tab-pane>
                </el-tabs>
            </div>

            <!-- 表格容器 -->
            <div class="table-section">
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

        <!-- 回复弹窗 -->
        <ReplyDialog
            v-model="replyDialogVisible"
            :data="currentMessage"
            @confirm="handleReplySubmit"
        />
        
        <!-- 留言板弹窗 -->
        <MessageBoard ref="messageBoardRef" />
    </div>
</template>
<script setup lang="jsx">
import { ref, reactive, getCurrentInstance, watch, onMounted } from "vue";
import { ElMessage } from 'element-plus'
import { Calendar } from '@element-plus/icons-vue'
import { useStore } from 'vuex'
import extend from "@/extension/order/ordercollaboration/OCP_Negotiation.jsx";
import viewOptions from './OCP_Negotiation/options.js'
import MessageCount from '@/comp/message-count/index.vue'
import ReplyDialog from '@/comp/negotiation-dialog/reply.vue'
import MessageBoard from '@/comp/message-board/index.vue'
import http from '@/api/http'
    
    const grid = ref(null);
    const { proxy } = getCurrentInstance()
    const store = useStore()
    //http请求，proxy.http.post/get
    const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

    // 页面状态
    const activeTab = ref('ALL')
    const selectedMessageType = ref(null)
    
    // 当前登录用户
    const currentLoginUser = ref('')

    // 回复弹窗相关状态
    const replyDialogVisible = ref(false)
    const currentMessage = ref({
        id: '',
        contractNo: '',
        planTrackNo: ''
    })
    
    // 留言板ref
    const messageBoardRef = ref(null)

    // Tab配置
    const messageTabs = ref([
        { name: 'ALL', label: '全部消息', count: 0 },
        { name: 'PO', label: '采购', count: 0 },
        { name: 'SO', label: '销售', count: 0 },
        { name: 'JS', label: '技术', count: 0 },
        { name: 'JH', label: '计划', count: 0 },
        { name: 'ZP', label: '装配', count: 0 },
        { name: 'JG', label: '金工', count: 0 },
        { name: 'WW', label: '委外', count: 0 },
        { name: 'BJ', label: '部件', count: 0 }
    ])

    // 消息统计数据
    const messageStats = ref([
        {
            label: '发送消息数',
            count: 0,
            clickable: true,
            type: 'sent',
            status: 'success',
            needRefresh: false
        },
        {
            label: '待回复消息',
            count: 0,
            clickable: true,
            type: 'pending',
            status: 'warning'
        },
        {
            label: '已超期消息',
            count: 0,
            clickable: true,
            type: 'overdue',
            status: 'danger'
        },
        {
            label: '已回复消息',
            count: 0,
            clickable: true,
            type: 'replied',
            status: 'success'
        }
    ])

    let gridRef;//对应[表.jsx]文件中this.使用方式一样
    //生成对象属性初始化
    const onInit = async ($vm) => {
        gridRef = $vm;
        gridRef.pagination.sizes = [20, 50, 100, 200, 500, 1000];
        //设置默认分页数
        gridRef.pagination.size = 20;
        gridRef.setFixedSearchForm(true);
        //与jsx中的this.xx使用一样，只需将this.xx改为gridRef.xx

        // 获取当前登录用户
        currentLoginUser.value = store.getters.getLoginName();

        // 添加操作列
        columns.push({
            field: 'action',
            title: '操作',
            width: 150,
            align: 'left',
            fixed: 'right',
            render: (h, { row, column, index }) => {
                return (
                    <div>
                        <el-button type="success" onClick={() => {
                            openMessageBoard(row)
                        }}>消息</el-button>
                        {(row.NegotiationStatus === '协商中' && row.AssignedResPerson === currentLoginUser.value) && (
                            <el-button type="primary" onClick={() => {
                                replyMessage(row)
                            }}>回复</el-button>
                        )}
                    </div>
                )
            }
        })

        // 并发获取统计数据
        await Promise.all([
            fetchCardStatistics(),
            fetchTabStatistics()
        ])
    }
    //生成对象属性初始化后,操作明细表配置用到
    const onInited = async () => {
        // 组件初始化完成后，触发数据查询
        setTimeout(() => {
            if (gridRef && gridRef.search) {
                gridRef.search()
            }
        }, 200)
    }
    const searchBefore = async (param) => {
        //界面查询前,可以给param.wheres添加查询参数
        //返回false，则不会执行查询
        
        // 构造查询条件数组
        let whereConditions = []
        
        // 添加业务类型查询条件（根据当前tab）
        if (activeTab.value && activeTab.value !== 'ALL') {
            whereConditions.push({
                name: "BusinessType",
                value: activeTab.value,
                displayType: "="
            })
        }
        
        // 添加消息状态查询条件（根据统计卡片选择）
        if (selectedMessageType.value) {
            whereConditions.push({
                name: "MessageStatus",
                value: selectedMessageType.value,
                displayType: "="
            })
        }
        
        // 如果有查询条件，设置到参数中
        if (whereConditions.length > 0) {
            param.wheres = JSON.stringify(whereConditions)
        }
        
        return true;
    }
    const searchAfter = async (rows, result) => {
        // 处理API返回的数据，更新统计信息
        if (result && result.total !== undefined) {
            // 根据返回的数据更新统计数据
            updateStatsFromApiResult(result)
        }
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

    // 获取顶部卡片统计数据
    const fetchCardStatistics = async () => {
        try {
            const response = await http.get('/api/OCP_Negotiation/GetStatistics')
            if (response && response.status) {
                const data = response.data || {}
                
                // 更新顶部统计数据
                messageStats.value = [
                    {
                        label: '发送消息数',
                        count: data.sentCount || 0,
                        clickable: true,
                        type: 'sent',
                        status: 'success',
                        needRefresh: false
                    },
                    {
                        label: '待回复消息',
                        count: data.pendingCount || 0,
                        clickable: true,
                        type: 'pending',
                        status: 'warning'
                    },
                    {
                        label: '已超期消息',
                        count: data.overdueCount || 0,
                        clickable: true,
                        type: 'overdue',
                        status: 'danger'
                    },
                    {
                        label: '已回复消息',
                        count: data.repliedCount || 0,
                        clickable: true,
                        type: 'replied',
                        status: 'success'
                    }
                ]
            }
        } catch (error) {
            console.error('获取卡片统计数据失败:', error)
            ElMessage.error('获取卡片统计数据失败')
        }
    }

    // 获取Tab统计数据
    const fetchTabStatistics = async (messageStatus = null) => {
        try {
            let url = '/api/OCP_Negotiation/GetStatisticsByBusinessType'
            if (messageStatus) {
                url += `?messageStatus=${messageStatus}`
            }
            
            const response = await http.get(url)
            if (response && response.status) {
                const data = response.data || {}
                const allMessages = data.allMessages || {}
                const businessTypeList = data.businessTypeList || []
                
                // 更新Tab的count数据
                messageTabs.value.forEach(tab => {
                    if (tab.name === 'ALL') {
                        // 全部消息的总数
                        tab.count = allMessages.pendingCount || 0
                    } else {
                        // 查找对应业务类型的数据
                        const businessData = businessTypeList.find(item => item.businessTypeCode === tab.name)
                        tab.count = businessData ? (businessData.pendingCount || 0) : 0
                    }
                })
            }
        } catch (error) {
            console.error('获取Tab统计数据失败:', error)
            ElMessage.error('获取Tab统计数据失败')
        }
    }

    // 根据API结果更新统计数据
    const updateStatsFromApiResult = (result) => {
        // 处理API返回的数据，不需要重复调用统计接口
        console.log('表格数据更新:', result)
    }

    // 处理统计项点击
    const handleStatClick = ({ item, index, type }) => {
        console.log('点击了统计项：', { item, index, type })
        
        // 更新选中状态
        selectedMessageType.value = type
        
        // 根据选中的消息状态重新获取Tab统计数据
        fetchTabStatistics(type)
        
        // 触发表格重新查询，会自动应用MessageStatus查询条件
        if (gridRef && gridRef.search) {
            gridRef.search()
        }
    }



    // Tab点击事件
    const handleTabClick = async (tab) => {
        console.log('切换到tab：', tab.props?.name)
        activeTab.value = tab.props?.name || 'ALL'
        
        // 切换tab时触发搜索，会自动应用BusinessType查询条件
        if (gridRef && gridRef.search) {
            gridRef.search()
        }
    }

    // 回复消息
    const replyMessage = (row) => {
        const { NegotiationID, NegotiationDate } = row;
        currentMessage.value = {
            NegotiationID,
            NegotiationDate
        }
        replyDialogVisible.value = true
    }

    // 打开留言板
    const openMessageBoard = (row) => {
        console.log('打开留言板:', row)
        
        // 传入当前行的业务类型和业务主键
        const businessType = row.BusinessType || ''
        const businessKey = row.BusinessKey || ''
        
        console.log('传入留言板的业务数据:', { businessType, businessKey })
        
        // 默认选中协商类型
        messageBoardRef.value?.open(businessType, businessKey, 'negotiate')
    }

    // 处理回复提交
    const handleReplySubmit = async (formData) => {
        try {
            // 处理回复提交逻辑
            console.log('回复提交:', formData)
            
            // 构造API请求数据 - 按照接口文档格式
            const requestData = {
                negotiationID: formData.negotiationID,
                replyContent: formData.replyContent || null,
                replyDeliveryDate: formData.replyDeliveryDate || null,
                negotiationStatus: formData.negotiationStatus || null
            }
            
            // 调用协商回复API - 使用新的AddReply接口
            const response = await http.post('/api/OCP_NegotiationReply/AddReply', requestData)
            
            // 检查API响应
            if (response && response.status === true) {
                ElMessage.success(response.message || '回复提交成功')
                
                // 刷新表格数据
                if (gridRef && gridRef.search) {
                    gridRef.search()
                }
                
                // 更新统计数据
                await fetchCardStatistics()
                await fetchTabStatistics()
                
            } else {
                throw new Error(response?.message || '回复提交失败')
            }
            
        } catch (error) {
            console.error('回复提交失败:', error)
            const errorMessage = error.message || '回复提交失败，请重试'
            ElMessage.error(errorMessage)
        }
    }

    //监听表单输入，做实时计算
    //watch(() => editFormFields.字段,(newValue, oldValue) => {	})
    //对外暴露数据
    defineExpose({})
</script>

<style scoped>
.negotiation-page {
    padding: 20px;
    margin: 0 auto;
    height: calc(100vh - 94px);
    display: flex;
    flex-direction: column;
    overflow: hidden;
}



.stats-section {
    margin-bottom: 10px;
    flex-shrink: 0;
}

.section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
}

.section-title {
    color: var(--el-text-color-primary);
    font-size: 18px;
    font-weight: 500;
    margin: 0;
    padding-left: 8px;
    border-left: 4px solid var(--el-color-primary);
}

.message-section {
    overflow: hidden;
    height: calc(100% - 200px);
}

.tabs-header {
    flex-shrink: 0;
    padding: 10px 15px 0 15px;
}

.custom-tabs-label {
    display: flex;
    position: relative;
    align-items: center;
}

.custom-tabs-label i {
    margin-right: 3px;
}

.custom-tabs-label .item-qty {
    position: absolute;
    background: #f21f0f;
    color: #fff;
    height: 18px;
    width: 18px;
    border-radius: 50%;
    font-size: 10px;
    line-height: 20px;
    text-align: center;
    right: -19px;
    top: -6px;
}

.table-section {
    height: calc(100% - 65px);
    overflow: hidden;
}
</style>

<style>
:deep(.el-tabs__content) {
    display: none;
}

.tabs-header :deep(.el-tabs__nav-wrap:after),
.tabs-header :deep(.el-tabs__active-bar) {
    height: 1px;
}

.tabs-header :deep(.el-tabs__header) {
    margin: 0px;
}

/* 当作为组件使用时的高度覆盖 */
.message-center-page .negotiation-page {
    height: calc(100vh - 150px) !important;
}
</style>
