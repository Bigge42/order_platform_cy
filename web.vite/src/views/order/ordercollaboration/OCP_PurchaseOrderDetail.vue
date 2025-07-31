<!--
 *Author：jxx
 *Date：{Date}
 *Contact：461857658@qq.com
 *业务请在@/extension/order/ordercollaboration/OCP_PurchaseOrderDetail.jsx或OCP_PurchaseOrderDetail.vue文件编写
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
      v-model="reminderDialogVisible"
      :data="reminderDialogData"
      @send="handleReminderSend"
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
import extend from "@/extension/order/ordercollaboration/OCP_PurchaseOrderDetail.jsx";
import viewOptions from './OCP_PurchaseOrderDetail/options.js'
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
const reminderDialogVisible = ref(false)
const reminderDialogData = ref({})

// 发起协商弹窗相关
const negotiationDialogVisible = ref(false)
const negotiationDialogData = ref({})

// 留言板ref
const messageBoardRef = ref(null)

// 跳转跟踪
const handleJumpToTrack = (row) => {
    console.log('跳转跟踪:', row)
    const { MaterialNumber } = row;
    
    // 获取主表选中行的BillNo
    const purchaseOrderRows = getPurchaseOrderSelectRow();
    const BillNo = purchaseOrderRows.length > 0 ? purchaseOrderRows[0].BillNo : '';

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

// 跳转缺料
const handleJumpToShortage = (row) => {
    console.log('跳转缺料:', row)
    const { MaterialNumber } = row;
    
    // 获取主表选中行的BillNo
    const purchaseOrderRows = getPurchaseOrderSelectRow();
    const BillNo = purchaseOrderRows.length > 0 ? purchaseOrderRows[0].BillNo : '';

    proxy.$tabs.open({
        text: '采购缺料信息',
        path: '/OCP_LackMtrlResult_PO',
        query: {
            MaterialNumber: encodeURIComponent(MaterialNumber || ''),
            BillNo: encodeURIComponent(BillNo || '')
        }
    });

    proxy.$tabs.clearCache('OCP_LackMtrlResult_PO')
}

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
    
    // 设置主表合计字段
    columns.forEach(x => {
        if (x.field == 'PurchaseQty') {
            x.summary = true;
            // 设置小数显示位数(默认2位)
            x.numberLength = 2;
            // 自定义返回合计的格式、文本显示
            x.summaryFormatter = (val, column, rows, summaryData) => {
                if (!val) return '0.00';
                summaryData[0] = '汇总';
                return (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
            };
        }
        if (x.field == 'InstockQty') {
            x.summary = true;
            x.numberLength = 2;
        }
        if (x.field == 'UnfinishedQty') {
            x.summary = true;
            x.numberLength = 2;
        }
    })
}
//隐藏高级查询按钮
const onInited = async () => {
    console.log(gridRef)
    gridRef.height = 400;
    gridRef.buttons.forEach(x => {
        if (x.name !== '查询') {
            x.hidden = true;
        }
    })
    
    // 添加操作列
    columns.push(...[
        {
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
                            onClick={($e) => handleJumpToTrack(row)}>跳转跟踪</el-button>
                        <el-button 
                            type="primary" 
                            link 
                            style={{padding: '2px 4px', fontSize: '12px'}} 
                            onClick={($e) => handleJumpToShortage(row)}>跳转缺料</el-button>
                    </div>
                )
            }
        },
        {
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
        }
])
}
const searchBefore = async (param) => {
    //界面查询前,可以给param.wheres添加查询参数
    //返回false，则不会执行查询
    const purchaseOrderRows = getPurchaseOrderSelectRow();
    if (!purchaseOrderRows.length) {
        proxy.$message.error('请先选择【采购单】数据')
        return false;
    }
    //查询前获取采购单的id，查询明细表
    param.wheres.push({ name: "OrderID", value: purchaseOrderRows[0].OrderID })
    return true;
}
const searchAfter = async (rows, result) => {
    return true;
}
const addBefore = async (formData) => {
    //明细表新建前获取【采购单】选中的主键OrderID值(若不获取，写入的明细表数据无法知道属于哪个采购单)
    const OrderID = getPurchaseOrderSelectRow()[0].OrderID;
    formData.mainData['OrderID'] = OrderID
    return true;
}
const updateBefore = async (formData) => {
    //编辑保存前formData为对象，包括明细表、删除行的Id
    return true;
}
const rowClick = ({ row, column, event }) => {
    //查询界面点击行事件
}
//获取OCP_PurchaseOrder.vue中缓存的方法，读取采购单主表选中的行，保存数据时可以知道是哪条采购单数据的明细
const getPurchaseOrderSelectRow = () => {
    const fn = proxy.base.getItem('getPurchaseOrderSelectRow');
    return fn();
}
const modelOpenBefore = async (row) => {//弹出框打开后方法
    //获取OCP_PurchaseOrder.vue中缓存的方法，读取采购单主表选中的行，保存数据时可以知道是哪条采购单数据的明细
    const purchaseOrderRows = getPurchaseOrderSelectRow();
    if (!purchaseOrderRows.length) {
        proxy.$message.error('请先选择【采购单】数据')
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
    
    // 获取主表选中的采购订单信息
    const purchaseOrderRows = getPurchaseOrderSelectRow();
    if (!purchaseOrderRows.length) {
        proxy.$message.error('请先选择【采购单】数据')
        return;
    }
    
    const mainOrderInfo = purchaseOrderRows[0];
    console.log('主表数据:', mainOrderInfo)
    
    // 准备催单数据 - 将当前行数据转换为催单组件需要的格式
    const reminderData = {
        SupplierCode: mainOrderInfo.SupplierCode,
        BusinessType: "PO",
        BusinessKey: row.FENTRYID,
        contractNo: mainOrderInfo.PurchaseOrderCode || mainOrderInfo.OrderCode || row.MaterialCode, // 使用主采购订单编码或物料编码
        planTrackNo: `PD${(row.Id || row.id || '').toString().slice(-6)}`, // 生成采购订单明细跟踪号
        // 保存当前行信息和主表信息，用于提交时使用
        currentRowData: row,
        mainOrderData: mainOrderInfo
    }
    
    console.log('准备的催单数据:', reminderData)
    
    // 设置数据并打开弹窗
    reminderDialogData.value = reminderData
    reminderDialogVisible.value = true
}

// 处理催单发送
const handleReminderSend = async (reminderData) => {
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
            ElMessage.error('获取主表数据失败，请重新选择采购单')
            return;
        }
        
        console.log('当前行数据:', currentRow)
        console.log('主表数据:', mainOrder)
        
        // 构建提交数据，添加新的字段
        const submitData = {
            ...reminderData,
            // 行号
            Seq: currentRow.LineNumber || '',
            // 计划跟踪号
            PlanTraceNo: currentRow.PlanTraceNo || '',
            // 单据编号 - 取主表选中行的BillNo
            BillNo: mainOrder.BillNo || '',
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
        delete submitData.DefaultResPersonLoginName
        
        console.log('最终提交数据:', submitData)
        
        // 调用催单接口
        const response = await proxy.http.post('/api/OCP_UrgentOrder/add', {mainData: submitData})
        
        if (response.status) {
            ElMessage.success(response.message || '催单发送成功')
            // 关闭弹窗
            reminderDialogVisible.value = false
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
    
    // 获取主表选中的采购订单信息
    const purchaseOrderRows = getPurchaseOrderSelectRow();
    if (!purchaseOrderRows.length) {
        proxy.$message.error('请先选择【采购单】数据')
        return;
    }
    
    const mainOrderInfo = purchaseOrderRows[0];
    console.log('主表数据:', mainOrderInfo)
    
    // 准备协商数据
    negotiationDialogData.value = {
        BusinessType: "PO",
        BusinessKey: row.DetailID,
        BillNo: mainOrderInfo.BillNo,
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
            ElMessage.error('获取主表数据失败，请重新选择采购单')
            return;
        }
        
        console.log('当前行数据:', currentRow)
        console.log('主表数据:', mainOrder)
        
        // 构建提交数据，添加与催单相同的字段
        const submitData = {
            ...negotiationData,
            // 行号
            Seq: currentRow.LineNumber || '',
            // 计划跟踪号
            PlanTraceNo: currentRow.PlanTraceNo || '',
            // 单据编号 - 取主表选中行的BillNo
            BillNo: mainOrder.BillNo || '',
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
    messageBoardRef.value?.open("PO", row.FENTRYID)
}

//监听表单输入，做实时计算
//watch(() => editFormFields.字段,(newValue, oldValue) => {	})
//对外暴露数据
defineExpose({
    openReminderDialog: (rowData) => {
        if (rowData) {
            handleUrge(rowData)
        } else {
            reminderDialogVisible.value = true
        }
    },
    closeReminderDialog: () => {
        reminderDialogVisible.value = false
        reminderDialogData.value = {}
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
