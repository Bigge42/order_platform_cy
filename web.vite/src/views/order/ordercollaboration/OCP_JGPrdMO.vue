<!-- 金工生产订单表 -->
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
            <div class="jg-prd-mo-detail">
                <JGPrdMODetail ref="jgPrdMODetailRef"></JGPrdMODetail>
            </div>
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
</template>
<script setup lang="jsx">
//引用生成的vue文件(明细表)
import JGPrdMODetail from './OCP_JGPrdMODetail.vue'
import extend from "@/extension/order/ordercollaboration/OCP_JGPrdMO.jsx";
import viewOptions from './OCP_JGPrdMO/options.js'
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

const jgPrdMODetailRef = ref();

// 催单弹窗相关
const reminderDialogVisible = ref(false)
const reminderDialogData = ref({})

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
    gridRef.setFixedSearchForm(true);
    //与jsx中的this.xx使用一样，只需将this.xx改为gridRef.xx

    //缓存主表方法，返回主表选中的行，在下面的明细表中会调用
    proxy.base.setItem('getJGPrdMOSelectRow', () => {
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
    
    // 配置汇总功能
    columns.forEach(x => {
        if (x.field == 'ProductionQty') {
            x.summary = true;
            x.numberLength = 2;
            x.summaryFormatter = (val, column, rows, summaryData) => {
                if (!val) return '0.00';
                summaryData[0] = '汇总';
                return (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
            };
        }
        if (x.field == 'InboundQty') {
            x.summary = true;
            x.numberLength = 2;
            x.summaryFormatter = (val, column, rows, summaryData) => {
                if (!val) return '0.00';
                summaryData[0] = '汇总';
                return (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
            };
        }
        if (x.field == 'UnInboundQty') {
            x.summary = true;
            x.numberLength = 2;
            x.summaryFormatter = (val, column, rows, summaryData) => {
                if (!val) return '0.00';
                summaryData[0] = '汇总';
                return (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
            };
        }
        if (x.field == 'OverdueQty') {
            x.summary = true;
            x.numberLength = 2;
            x.summaryFormatter = (val, column, rows, summaryData) => {
                if (!val) return '0.00';
                summaryData[0] = '汇总';
                return (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
            };
        }
        if (x.field == 'OverdueDays') {
            x.summary = true;
            x.numberLength = 0;
            x.summaryFormatter = (val, column, rows, summaryData) => {
                if (!val) return '0';
                summaryData[0] = '汇总';
                return (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
            };
        }
    })
    
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
}
const searchBefore = async (param) => {
    //界面查询前,可以给param.wheres添加查询参数
    //返回false，则不会执行查询
    return true;
}
const searchAfter = async (rows, result) => {
    // 主表查询完成后，清空明细表数据
    if (jgPrdMODetailRef.value && jgPrdMODetailRef.value.$refs.grid) {
        const detailTable = jgPrdMODetailRef.value.$refs.grid.getTable(true)
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
    jgPrdMODetailRef.value.$refs.grid.search();
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
    const reminderData = {
        BusinessType: "JG",
        BusinessKey: row.FID,
        id: row.Id || row.id,
        contractNo: row.MOCode || row.moCode || row.OrderCode || row.orderCode, // 使用生产订单编码
        planTrackNo: `JM${(row.Id || row.id || '').toString().slice(-6)}`, // 生成金工生产订单跟踪号
        business: 'jg_manufacturing_order', // 业务类型为金工生产订单
        defaultResponsible: '金工车间', // 默认负责人
        assignedPersons: [], // 指定负责人，初始为空
        urgentLevel: 'B', // 默认紧急等级
        replyTime: 24, // 默认回复时间
        replyTimeUnit: 'hour', // 默认单位为小时
        messageContent: '' // 消息内容初始为空
    }
    
    // 设置数据并打开弹窗
    reminderDialogData.value = reminderData
    reminderDialogVisible.value = true
}

// 处理催单发送
const handleReminderSend = async (reminderData) => {
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
    
    // 准备协商数据
    negotiationDialogData.value = {
        BusinessType: "JG",
        BusinessKey: row.FID,
        BillNo: row.BillNo,
        DefaultResPerson: row.DefaultResPerson,
        id: row.Id || row.id,
        moCode: row.MOCode || row.moCode,
        orderCode: row.OrderCode || row.orderCode,
        workshopName: row.WorkshopName || row.workshopName,
        productCode: row.ProductCode || row.productCode,
        productName: row.ProductName || row.productName,
        planStartDate: row.PlanStartDate || row.planStartDate,
        planEndDate: row.PlanEndDate || row.planEndDate,
        // 可以添加更多需要的字段
        ...row
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
            AssignedResPerson: data.AssignedResPerson.userName
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
<style lang="less" scoped>
.jg-prd-mo-detail {
  margin-top: 13px;
  border-top: 7px solid #eee;
}
</style>
