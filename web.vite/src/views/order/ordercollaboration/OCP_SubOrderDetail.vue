<!--
 *Author：jxx
 *Date：{Date}
 *Contact：461857658@qq.com
 *业务请在@/extension/order/ordercollaboration/OCP_SubOrderDetail.jsx或OCP_SubOrderDetail.vue文件编写
 *新版本支持vue或【表.jsx]文件编写业务,
 -->
<template>
    <view-grid ref="grid" :columns="columns" :detail="detail" :details="details" :editFormFields="editFormFields"
        :editFormOptions="editFormOptions" :searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions"
        :table="table" :extend="extend" :onInit="onInit" :onInited="onInited" :searchBefore="searchBefore"
        :searchAfter="searchAfter" :addBefore="addBefore" :updateBefore="updateBefore" :rowClick="rowClick"
        :modelOpenBefore="modelOpenBefore" :modelOpenAfter="modelOpenAfter">
        <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
        <template #gridHeader>
        </template>
    </view-grid>
    
    <!-- 催单弹窗组件 -->
    <ReminderDialog
      v-model="followUpReminderVisible"
      :data="followUpReminderData"
      @send="handleFollowUpSend"
    />
    
    <!-- 发起协商弹窗组件 -->
    <NegotiationDialog
      v-model="negotiationDialogVisible"
      :data="negotiationDialogData"
      @confirm="handleNegotiationConfirm"
      @cancel="handleNegotiationCancel"
    />
    
    <!-- 留言板弹窗组件 -->
    <MessageBoard ref="messageBoardRef" />
</template>
<script setup lang="jsx">
import extend from "@/extension/order/ordercollaboration/OCP_SubOrderDetail.jsx";
import viewOptions from './OCP_SubOrderDetail/options.js'
import ReminderDialog from '@/comp/reminder-dialog/index.vue'
import NegotiationDialog from '@/comp/negotiation-dialog/index.vue'
import MessageBoard from '@/comp/message-board/index.vue'
import { ref, reactive, getCurrentInstance, watch, onMounted } from "vue";
import { ElMessage } from 'element-plus'

const grid = ref(null);
const { proxy } = getCurrentInstance()
//http请求，proxy.http.post/get
const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

// 催单弹窗相关
const followUpReminderVisible = ref(false)
const followUpReminderData = ref({})

// 发起协商弹窗相关
const negotiationDialogVisible = ref(false)
const negotiationDialogData = ref({})

// 留言板ref
const messageBoardRef = ref(null)

let gridRef;//对应[表.jsx]文件中this.使用方式一样
//生成对象属性初始化
const onInit = async ($vm) => {
    gridRef = $vm;
    //设置分页条大小
    gridRef.pagination.sizes = [20, 50, 100, 200, 500, 1000];
    //设置默认分页数
    gridRef.pagination.size = 20;
    //默认不加载数据
    gridRef.load = false
}
//隐藏高级查询按钮
const onInited = async () => {
    gridRef.height = 400;
    gridRef.buttons.forEach(x => {
        if (x.name !== '查询') {
            x.hidden = true;
        }
    })
    
    // 添加功能列
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
                    <el-button 
                        type="primary" 
                        link 
                        style={{padding: '2px 4px', fontSize: '12px'}} 
                        onClick={($e) => handleJumpToShortageWO(row)}>跳转缺料</el-button>
                </div>
            )
        }
    })
    
    // 添加操作列
    columns.push({
        field: 'action',
        title: '操作',
        width: 200,
        align: 'center',
        fixed: 'right',
        render: (h, { row, column, index }) => {
            return (
            <div style={{ display: 'flex', gap: '6px', alignItems: 'center', justifyContent: 'center' }}>
                <el-button 
                type="success" 
                size="small" 
                onClick={($e) => handleOpenMessageBoard(row)}
                >
                消息
                </el-button>
                <el-button 
                type="warning" 
                size="small" 
                style={{ marginLeft: '0px' }}
                onClick={($e) => handleUrge(row)}
                >
                催单
                </el-button>
                <el-button 
                type="primary" 
                size="small" 
                style={{ marginLeft: '0px' }}
                onClick={($e) => handleNegotiate(row)}
                >
                协商
                </el-button>
            </div>
            )
        }
    })
}
const searchBefore = async (param) => {
    //界面查询前,可以给param.wheres添加查询参数
    //返回false，则不会执行查询
    const subOrderRows = getSubOrderSelectRow();
    if (!subOrderRows.length) {
        proxy.$message.error('请先选择【子订单】数据')
        return false;
    }
    //查询前获取子订单的id，查询明细表
    param.wheres.push({ name: "ID", value: subOrderRows[0].ID })
    return true;
}
const searchAfter = async (rows, result) => {
    return true;
}
const addBefore = async (formData) => {
    //明细表新建前获取【子订单】选中的主键ID值(若不获取，写入的明细表数据无法知道属于哪个子订单)
    const ID = getSubOrderSelectRow()[0].ID;
    formData.mainData['ID'] = ID
    return true;
}
const updateBefore = async (formData) => {
    //编辑保存前formData为对象，包括明细表、删除行的Id
    return true;
}
const rowClick = ({ row, column, event }) => {
    //查询界面点击行事件
}
//获取OCP_SubOrder.vue中缓存的方法，读取子订单主表选中的行，保存数据时可以知道是哪条子订单数据的明细
const getSubOrderSelectRow = () => {
    const fn = proxy.base.getItem('getSubOrderSelectRow');
    return fn();
}
const modelOpenBefore = async (row) => {//弹出框打开后方法
    //获取OCP_SubOrder.vue中缓存的方法，读取子订单主表选中的行，保存数据时可以知道是哪条子订单数据的明细
    const subOrderRows = getSubOrderSelectRow();
    if (!subOrderRows.length) {
        proxy.$message.error('请先选择【子订单】数据')
        return false;
    }
    return true;//返回false，不会打开弹出框
}
const modelOpenAfter = (row) => {
    //弹出框打开后方法,设置表单默认值,按钮操作等
}

// 催单操作
const handleUrge = (row) => {
    console.log('催单操作:', row)
    console.log('当前行完整数据:', row)
    
    // 获取主表选中的订单信息
    const subOrderRows = getSubOrderSelectRow();
    if (!subOrderRows.length) {
        proxy.$message.error('请先选择【子订单】数据')
        return;
    }
    
    const mainOrderInfo = subOrderRows[0];
    console.log('主表数据:', mainOrderInfo)
    
    // 准备催单数据 - 适配新的reminder-dialog组件格式
    const reminderData = {
        BusinessType: 'WW', // 委外订单业务类型
        SupplierCode: mainOrderInfo.SupplierCode || mainOrderInfo.supplierCode, // 供应商编码
        BusinessKey: row.FENTRYID,
        // 保存当前行信息和主表信息，用于提交时使用
        currentRowData: row,
        mainOrderData: mainOrderInfo
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
        // 获取当前行数据和主表数据
        const currentRow = reminderData.currentRowData;
        const mainOrder = reminderData.mainOrderData;
        
        // 添加防护性检查
        if (!currentRow) {
            console.error('当前行数据为空:', reminderData)
            ElMessage.error('获取当前行数据失败，请重新操作')
            return;
        }
        
        if (!mainOrder) {
            console.error('主表数据为空:', reminderData)
            ElMessage.error('获取主表数据失败，请重新选择子订单')
            return;
        }
        
        console.log('当前行数据:', currentRow)
        console.log('主表数据:', mainOrder)
        
        // 构建提交数据，添加新的字段
        const submitData = {
            ...reminderData,
            // 行号 - 委外取Seq字段
            Seq: currentRow.Seq || '',
            // 计划跟踪号
            PlanTraceNo: currentRow.MtoNo || '',
            // 单据编号 - 委外取主表PurchaseBillNo字段
            BillNo: mainOrder.PurchaseBillNo || '',
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
        delete submitData.mainOrderData;
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
    
    // 获取主表选中的订单信息
    const subOrderRows = getSubOrderSelectRow();
    if (!subOrderRows.length) {
        proxy.$message.error('请先选择【子订单】数据')
        return;
    }
    
    const mainOrderInfo = subOrderRows[0];
    console.log('主表数据:', mainOrderInfo)
    
    // 准备协商数据
    negotiationDialogData.value = {
        BusinessType: "WW",
        BusinessKey: row.FENTRYID, // 子表取FENTRYID
        DefaultResPerson: mainOrderInfo.DefaultResPerson,
        SupplierCode: mainOrderInfo.SupplierCode,
        // 保存当前行信息和主表信息，用于提交时使用
        currentRowData: row,
        mainOrderData: mainOrderInfo
    }
    
    // 打开发起协商弹窗
    negotiationDialogVisible.value = true
}

// 发起协商弹窗确认事件
const handleNegotiationConfirm = async (negotiationData) => {
    console.log('发起协商确认:', negotiationData)
    
    try {
        // 获取当前行数据和主表数据
        const currentRow = negotiationData.currentRowData;
        const mainOrder = negotiationData.mainOrderData;
        
        // 添加防护性检查
        if (!currentRow) {
            console.error('当前行数据为空:', negotiationData)
            ElMessage.error('获取当前行数据失败，请重新操作')
            return;
        }
        
        if (!mainOrder) {
            console.error('主表数据为空:', negotiationData)
            ElMessage.error('获取主表数据失败，请重新选择子订单')
            return;
        }
        
        console.log('当前行数据:', currentRow)
        console.log('主表数据:', mainOrder)
        
        // 构建提交数据，添加与催单相同的字段
        const submitData = {
            ...negotiationData,
            // 行号 - 委外取Seq字段
            Seq: currentRow.Seq || '',
            // 计划跟踪号 - 委外取MtoNo字段
            PlanTraceNo: currentRow.MtoNo || '',
            // 单据编号 - 委外取主表PurchaseBillNo字段
            BillNo: mainOrder.PurchaseBillNo || '',
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
        delete submitData.mainOrderData;
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

// 打开留言板
const handleOpenMessageBoard = (row) => {
    console.log('打开留言板:', row)
    // 传入业务类型和业务键值
    messageBoardRef.value?.open("WW", row.FENTRYID)
}

// 跳转委外未完跟踪
const handleJumpToTrackWO = (row) => {
    console.log('跳转委外未完跟踪:', row)
    
    // 获取主表选中的订单信息
    const subOrderRows = getSubOrderSelectRow();
    if (!subOrderRows.length) {
        proxy.$message.error('请先选择【子订单】数据')
        return;
    }
    
    const mainOrderInfo = subOrderRows[0];
    const { MaterialNumber } = row;
    const { SubOrderNo } = mainOrderInfo;

    proxy.$tabs.open({
        text: '委外未完跟踪',
        path: '/OCP_SubOrderUnFinishTrack',
        query: {
            MaterialNumber: encodeURIComponent(MaterialNumber || ''),
            BillNo: encodeURIComponent(SubOrderNo || '')
        }
    });

    proxy.$tabs.clearCache('OCP_SubOrderUnFinishTrack')
}

// 跳转委外缺料
const handleJumpToShortageWO = (row) => {
    console.log('跳转委外缺料:', row)
    
    // 获取主表选中的订单信息
    const subOrderRows = getSubOrderSelectRow();
    if (!subOrderRows.length) {
        proxy.$message.error('请先选择【子订单】数据')
        return;
    }
    
    const mainOrderInfo = subOrderRows[0];
    const { MaterialNumber } = row;
    const { PurchaseBillNo } = mainOrderInfo;

    proxy.$tabs.open({
        text: '委外缺料结果',
        path: '/OCP_LackMtrlResult_WO',
        query: {
            MaterialNumber: encodeURIComponent(MaterialNumber || ''),
            BillNo: encodeURIComponent(PurchaseBillNo || '')
        }
    });

    proxy.$tabs.clearCache('OCP_LackMtrlResult_WO')
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
