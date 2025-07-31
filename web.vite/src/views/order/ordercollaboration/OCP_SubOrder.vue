<!-- 委外订单表 -->
<template>
    <view-grid ref="grid" :columns="columns" :detail="detail" :details="details" :editFormFields="editFormFields"
        :editFormOptions="editFormOptions" :searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions"
        :table="table" :extend="extend" :onInit="onInit" :onInited="onInited" :searchBefore="searchBefore"
        :searchAfter="searchAfter" :addBefore="addBefore" :updateBefore="updateBefore" :rowClick="rowClick"
        :modelOpenBefore="modelOpenBefore" :modelOpenAfter="modelOpenAfter">
        <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
        <template #gridHeader>
        </template>
        <template #gridFooter>
            <div class="sub-order-detail">
                <sub-order-detail ref="subOrderDetailRef"></sub-order-detail>
            </div>
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
</template>
<script setup lang="jsx">
//引用生成的vue文件(明细表)
import subOrderDetail from './OCP_SubOrderDetail.vue'
import extend from "@/extension/order/ordercollaboration/OCP_SubOrder.jsx";
import viewOptions from './OCP_SubOrder/options.js'
import ReminderDialog from '@/comp/reminder-dialog/index.vue'
import NegotiationDialog from '@/comp/negotiation-dialog/index.vue'
import { ref, reactive, getCurrentInstance, watch, onMounted } from "vue";
import { ElMessage } from 'element-plus'
import { useRoute } from 'vue-router';

const route = useRoute();
const grid = ref(null);
const { proxy } = getCurrentInstance()
//http请求，proxy.http.post/get
const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

const subOrderDetailRef = ref();

// 催单弹窗相关
const followUpReminderVisible = ref(false)
const followUpReminderData = ref({})

// 发起协商弹窗相关
const negotiationDialogVisible = ref(false)
const negotiationDialogData = ref({})

let gridRef;//对应[表.jsx]文件中this.使用方式一样
//生成对象属性初始化
const onInit = async ($vm) => {
    gridRef = $vm;
    //设置分页条大小
    gridRef.pagination.sizes = [20, 50, 100, 200, 500, 1000];
    //设置默认分页数
    gridRef.pagination.size = 20;
    // gridRef.setFixedSearchForm(true);
    
    // 设置快捷查询字段
    gridRef.queryFields = ['SubOrderNo', 'PurchaseBillNo', 'Urgency'];
    
    //与jsx中的this.xx使用一样，只需将this.xx改为gridRef.xx

    //缓存主表方法，返回主表选中的行，在下面的明细表中会调用
    proxy.base.setItem('getSubOrderSelectRow', () => {
        return gridRef.getTable(true).getSelected();
    })

    const query = route.query;
    for (const key in query) {
        if (query[key]) {
            // 对URL参数进行解码
            gridRef.searchFormFields[key] = decodeURIComponent(query[key]);
        }
    }
}
//生成对象属性初始化后,操作明细表配置用到
const onInited = async () => {
    gridRef.height = gridRef.height - 510;
    if (gridRef.height < 500) {
        gridRef.height = 500;
    }
    
    // 添加操作列
    // columns.push({
    //     field: 'action',
    //     title: '操作',
    //     width: 150,
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

    // 配置Table表合计功能
    columns.forEach(col => {
        if (col.field === 'InstockQty') {
            col.summary = {
                sum: true,
                summaryFormatter: (value) => {
                    return value ? value.toLocaleString() : '0';
                },
                summaryLabel: '汇总'
            };
        } else if (col.field === 'UnfinishedQty') {
            col.summary = {
                sum: true,
                summaryFormatter: (value) => {
                    return value ? value.toLocaleString() : '0';
                }
            };
        } else if (col.field === 'OverdueQty') {
            col.summary = {
                sum: true,
                summaryFormatter: (value) => {
                    return value ? value.toLocaleString() : '0';
                }
            };
        }
    });
}
const searchBefore = async (param) => {
    //界面查询前,可以给param.wheres添加查询参数
    //返回false，则不会执行查询
    return true;
}
const searchAfter = async (rows, result) => {
    // 主表查询完成后，清空明细表数据
    if (subOrderDetailRef.value && subOrderDetailRef.value.$refs.grid) {
        const detailTable = subOrderDetailRef.value.$refs.grid.getTable(true)
        if (detailTable && detailTable.rowData) {
            detailTable.rowData.splice(0) // 清空明细表数据
        }
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
    //点击行清除选中的行(用于下面明细表判断)
    grid.value.clearSelection();
    //点击行默认选中
    grid.value.toggleRowSelection(row); //单击行时选中当前行;
    //加载明细表
    subOrderDetailRef.value.$refs.grid.search();
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
    
    // 准备催单数据 - 适配新的reminder-dialog组件格式
    const reminderData = {
        BusinessType: 'WW', // 委外订单业务类型
        SupplierCode: row.SupplierCode || row.supplierCode, // 供应商编码
        UrgencyLevel: 'B', // 默认紧急等级
        AssignedReplyTime: 24, // 默认回复时间
        TimeUnit: '2', // 默认单位为小时
        UrgentContent: '', // 催单内容初始为空
    }
    
    // 设置数据并打开弹窗
    followUpReminderData.value = reminderData
    followUpReminderVisible.value = true
}

// 处理催单发送
const handleFollowUpSend = async (reminderData) => {
    console.log('发送催单数据:', reminderData)
    
    try {
        // 调用催单接口
        const response = await proxy.http.post('/api/OCP_UrgentOrder/add', {mainData: {
            ...reminderData,
            AssignedResPerson: reminderData.AssignedResPerson ? reminderData.AssignedResPerson.userName : ''
        }})
        
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
    
    // 准备协商数据
    negotiationDialogData.value = {
        BusinessType: "WW",
        BusinessKey: row.FID, // 主表取FID
        DefaultResPerson: row.DefaultResPerson,
        SupplierCode: row.SupplierCode,
    }
    
    // 打开发起协商弹窗
    negotiationDialogVisible.value = true
}

// 发起协商弹窗确认事件
const handleNegotiationConfirm = async (data) => {
    console.log('发起协商确认:', data)
    
    try {
        // 调用发起协商接口
        const response = await proxy.http.post('/api/OCP_Negotiation/add', {mainData: {
            ...data,
            AssignedResPerson: data.AssignedResPerson ? data.AssignedResPerson.userName : ''
        }})
        
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
<style lang="less" scoped>
.sub-order-detail {
  margin-top: 13px;
  border-top: 7px solid #eee;
}
</style>
