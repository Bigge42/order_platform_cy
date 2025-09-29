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
    <CurrentProcessBatchInfoDialog v-model="batchInfoDialogVisible" :data="SaleOrderEntryData"
        ref="processBatchInfoDialogRef">
    </CurrentProcessBatchInfoDialog>

    <!-- 催单弹窗组件 -->
    <ReminderDialog v-model="reminderDialogVisible" :data="reminderDialogData" @send="handleReminderSend" />

    <!-- 发起协商弹窗组件 -->
    <NegotiationDialog v-model="negotiationDialogVisible" :data="negotiationDialogData" @confirm="handleNegotiationConfirm"
        @cancel="handleNegotiationCancel" />

    <!-- 留言板弹窗组件 -->
    <MessageBoard ref="messageBoardRef" />
</template>
<script setup lang="jsx">
import extend from "@/extension/order/ordercollaboration/OCP_SOProgressDetail.jsx";
import viewOptions from './OCP_SOProgressDetail/options.js'
import CurrentProcessBatchInfoDialog from './CurrentProcessBatchInfoDialog/index.vue';
import ReminderDialog from '@/comp/reminder-dialog/index.vue'
import NegotiationDialog from '@/comp/negotiation-dialog/index.vue'
import MessageBoard from '@/comp/message-board/index.vue'

import { ref, reactive, getCurrentInstance, watch, onMounted } from "vue";
import { ElMessage } from 'element-plus'
const grid = ref(null);
const { proxy } = getCurrentInstance()
//http请求，proxy.http.post/get
const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

const batchInfoDialogVisible = ref(false);
const SaleOrderEntryData = ref({});

// 催单弹窗相关
const reminderDialogVisible = ref(false)
const reminderDialogData = ref({})

// 发起协商弹窗相关
const negotiationDialogVisible = ref(false)
const negotiationDialogData = ref({})

// 留言板ref
const messageBoardRef = ref(null)

const saleOrder = ref(null);

let gridRef;//对应[表.jsx]文件中this.使用方式一样
//生成对象属性初始化
const onInit = async ($vm) => {
    gridRef = $vm;
    //设置分页条大小
    gridRef.pagination.sizes = [20, 50, 100, 200, 500, 1000];
    //设置默认分页数
    gridRef.pagination.size = 20;
    //与jsx中的this.xx使用一样，只需将this.xx改为gridRef.xx
    //gridRef.setFixedSearchForm(true);
    gridRef.load = false;
    columns.forEach(x => {
        // console.log(x.field);
        if (['InstockQty', 'UnInstockQty', 'OutStockQty'].includes(x.field)) {
            x.summary = true;
        }
    })

    const btns = [{
        name: "批量变更",
        type: 'primary',
        onClick: async function () {
            let selections = grid.value.selections;
            console.log(selections);

            // 检查是否选中
            if (!selections?.length > 0) {
                proxy.$message.warning('请至少选择一个销售订单明细！')
                return
            }
            let SelectedItems = [];
            selections.forEach(d => {
                SelectedItems.push({
                    PlanNo: d.MtoNo,
                    Specification: d.Specification,
                    CatalogPrice: d.CatalogPrice,//?
                    SalePrice: d.SalePrice//?
                })
            })
            parentRows
            let postData = {
                ContractNo: saleOrder.value.SalesContractNumber,
                CustomerType: saleOrder.value.ContractType,
                SelectedItems
            }
            // 防止重复点击
            if (syncLoading.value) {
                return
            }

            try {
                // 设置loading状态
                syncLoading.value = true

                // 显示加载状态
                proxy.$message.info('正在发送变更请求...')

                // 调用API接口，传递ComputeID
                const requestBody = {
                    ComputeID: selectedSchemeId.value
                }

                const result = await proxy.http.post(`/api/OCP_SOProgressDetail/StartContractChangeReview`, postData)

                // 成功提示
                proxy.$message.success('变更通知成功')
                console.log('变更通知成功:', result)

                // 可选：刷新当前页面数据
                // gridRef.search()

            } catch (error) {
                console.error('变更通知失败:', error)
                proxy.$message.error('变更通知失败' + (error.message || '未知错误'))
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
    gridRef.height = 400;
    gridRef.buttons.forEach(x => {
        if (!['批量变更', '查询'].includes(x.name)) {
            x.hidden = true;
        }
    })

    columns.push(...[
        {
            field: 'action',
            title: '操作',
            width: 250,
            align: 'center',
            fixed: 'right',
            render: (h, { row, column, index }) => {
                return (
                    <div style={{ display: 'flex', gap: '6px', alignItems: 'center', justifyContent: 'center' }}>
                        <el-button
                            type="primary"
                            size="small"
                            onClick={($e) => handleQueryBatchInfo(row)}
                        >
                            查看批次信息
                        </el-button>

                        <el-dropdown
                            trigger="click"
                            v-slots={{
                                dropdown: () => (
                                    <el-dropdown-menu>
                                        <el-dropdown-item>
                                            <div
                                                onClick={($e) => {
                                                    handleOpenMessageBoard(row);
                                                }}
                                            >
                                                消息
                                            </div>
                                        </el-dropdown-item>
                                        <el-dropdown-item>
                                            <div
                                                onClick={($e) => {
                                                    handleUrge(row);
                                                }}
                                            >
                                                催单
                                            </div>
                                        </el-dropdown-item>
                                        <el-dropdown-item>
                                            <div
                                                onClick={($e) => {
                                                    handleNegotiate(row);
                                                }}
                                            >
                                                协商
                                            </div>
                                        </el-dropdown-item>
                                    </el-dropdown-menu>
                                )
                            }}
                        >
                            <span style="font-size: 13px;color: #409eff;margin: 5px 0 0 10px;" class="el-dropdown-link">
                                更多<i class="el-icon-arrow-right"></i>
                            </span>
                        </el-dropdown>
                    </div>
                )
            }
        }
    ])
}

const searchBefore = async (param) => {
    //界面查询前,可以给param.wheres添加查询参数
    //返回false，则不会执行查询
    const saleOrderRows = getSaleOrderSelectRow();
    saleOrder.value = saleOrderRows[0];
    if (!saleOrderRows.length) {
        proxy.$message.error('请先选择【销售订单单】数据')
        return false;
    }
    //查询前获取采购单的id，查询明细表
    param.wheres.push({ name: "OrderID", value: saleOrderRows[0].OrderID })
    return true;
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

//获取OCP_SOProgress.vue中缓存的方法，读取采购单主表选中的行，保存数据时可以知道是哪条采购单数据的明细
const getSaleOrderSelectRow = () => {
    const fn = proxy.base.getItem('getSaleOrderSelectRow');
    return fn();
}

const processBatchInfoDialogRef = ref();
const handleQueryBatchInfo = (row) => {
    SaleOrderEntryData.value = {
        MtoNo: row.MtoNo
    }
    proxy.base.setItem('SaleOrderEntryData', () => {
        return SaleOrderEntryData;
    })
    batchInfoDialogVisible.value = true;
    processBatchInfoDialogRef.value?.search();
}

const handleSync2OA = (row) => {

}

// 催单操作
const handleUrge = (row) => {
    console.log('催单操作:', row)
    console.log('当前行完整数据:', row)

    // 获取主表选中的销售订单信息
    const saleOrderRows = getSaleOrderSelectRow();
    if (!saleOrderRows.length) {
        proxy.$message.error('请先选择【销售订单】数据')
        return;
    }

    const mainOrderInfo = saleOrderRows[0];
    console.log('主表数据:', mainOrderInfo)

    // 准备催单数据 - 销售订单进度明细标准格式
    const reminderData = {
        SupplierCode: mainOrderInfo.SupplierCode || '',
        BusinessType: "SO",
        BusinessKey: row.SOEntryID,
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
            ElMessage.error('获取主表数据失败，请重新选择销售订单')
            return;
        }

        console.log('当前行数据:', currentRow)
        console.log('主表数据:', mainOrder)

        // 构建提交数据，添加必要字段
        const submitData = {
            ...reminderData,
            // 行号 - 该表没有行号字段，使用空值
            Seq: '',
            // 计划跟踪号
            PlanTraceNo: currentRow.MtoNo || '',
            // 单据编号 - 取销售订单号
            BillNo: mainOrder.SOBillNo || mainOrder.BillNo || '',
            // 默认负责人姓名 - 取默认负责人的name
            DefaultResPersonName: reminderData.DefaultResPerson || '',
            // 默认负责人登录名 - 取默认负责人的loginName
            DefaultResPerson: reminderData.DefaultResPersonLoginName || '',
            // 销售负责人姓名（指定负责人）- 取选中人的userTrueName
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
        const response = await proxy.http.post('/api/OCP_UrgentOrder/add', { mainData: submitData })

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

    // 获取主表选中的销售订单信息
    const saleOrderRows = getSaleOrderSelectRow();
    if (!saleOrderRows.length) {
        proxy.$message.error('请先选择【销售订单】数据')
        return;
    }

    const mainOrderInfo = saleOrderRows[0];
    console.log('主表数据:', mainOrderInfo)

    // 准备协商数据 - 销售订单进度明细标准格式
    const negotiationData = {
        SupplierCode: mainOrderInfo.SupplierCode || '',
        BusinessType: "SO",
        BusinessKey: row.SOEntryID,
        BillNo: mainOrderInfo.SOBillNo || mainOrderInfo.BillNo || '',
        DefaultResPerson: mainOrderInfo.DefaultResPerson,
        id: row.Id || row.id,
        // 保存当前行信息和主表信息，用于提交时使用
        currentRowData: row,
        mainOrderData: mainOrderInfo
    }

    console.log('准备的协商数据:', negotiationData)

    // 设置数据并打开发起协商弹窗
    negotiationDialogData.value = negotiationData
    negotiationDialogVisible.value = true
}

// 发起协商弹窗确认事件
const handleNegotiationConfirm = async (data) => {
    console.log('发起协商确认:', data)

    try {
        // 获取当前行数据和主表数据
        const currentRow = data.currentRowData;
        const mainOrder = data.mainOrderData;

        // 添加防护性检查
        if (!currentRow) {
            console.error('当前行数据为空:', data)
            ElMessage.error('获取当前行数据失败，请重新操作')
            return;
        }

        if (!mainOrder) {
            console.error('主表数据为空:', data)
            ElMessage.error('获取主表数据失败，请重新选择销售订单')
            return;
        }

        if (!data.AssignedResPerson) {
            ElMessage.error('请选择负责人')
            return;
        }

        console.log('当前行数据:', currentRow)
        console.log('主表数据:', mainOrder)

        // 构建提交数据，添加必要字段
        const submitData = {
            ...data,
            // 行号 - 该表没有行号字段，使用空值
            Seq: '',
            // 计划跟踪号
            PlanTraceNo: currentRow.MtoNo || '',
            // 单据编号已在初始化时设置为SOBillNo
            // 默认负责人姓名 - 取默认负责人的name
            DefaultResPersonName: data.DefaultResPerson || '',
            // 默认负责人登录名 - 取默认负责人的loginName
            DefaultResPerson: data.DefaultResPersonLoginName || '',
            // 销售负责人姓名（指定负责人）- 取选中人的userTrueName
            AssignedResPersonName: data.AssignedResPerson ? data.AssignedResPerson.userTrueName : '',
            // 原有的AssignedResPerson字段保持兼容
            AssignedResPerson: data.AssignedResPerson.userName
        };

        // 移除临时保存的数据，避免提交到后端
        delete submitData.currentRowData;
        delete submitData.mainOrderData;
        delete submitData.DefaultResPersonLoginName;

        console.log('最终提交数据:', submitData)

        // 调用发起协商接口
        const response = await proxy.http.post('/api/OCP_Negotiation/add', { mainData: submitData })

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
    messageBoardRef.value?.open("SO", row.SOEntryID)
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
