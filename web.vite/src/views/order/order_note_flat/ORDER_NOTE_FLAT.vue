<!--
 *Author：jxx
 *Date：{Date}
 *Contact：461857658@qq.com
 *业务请在@/extension/order/order_note_flat/ORDER_NOTE_FLAT.jsx或ORDER_NOTE_FLAT.vue文件编写
 *新版本支持vue或【表.jsx]文件编写业务,
 -->
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
               :modelOpenBefore="modelOpenBefore"
               :modelOpenAfter="modelOpenAfter">
        <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
        <template #gridHeader>
        </template>
    </view-grid>
    <el-dialog
        v-model="noteDialogVisible"
        :title="noteDialogTitle"
        width="640px"
        destroy-on-close
        :close-on-click-modal="false"
    >
        <div v-loading="noteDialogLoading">
            <el-form label-width="160px" label-position="left" class="note-split-form">
                <el-form-item label="备注1（remark_raw）">
                    <el-input
                        v-model="noteForm.remark_raw"
                        type="textarea"
                        :rows="3"
                        readonly
                        resize="none"
                    />
                </el-form-item>
                <el-form-item label="备注2（internal_note）">
                    <el-input
                        v-model="noteForm.internal_note"
                        type="textarea"
                        :rows="3"
                        readonly
                        resize="none"
                    />
                </el-form-item>
                <el-divider></el-divider>
                <el-form-item label="阀体及执行机构装配备注">
                    <el-input
                        v-model="noteForm.note_body_actuator"
                        type="textarea"
                        :rows="3"
                        resize="none"
                        placeholder="请输入阀体及执行机构装配备注"
                    />
                </el-form-item>
                <el-form-item label="附件安装及调试备注">
                    <el-input
                        v-model="noteForm.note_accessory_debug"
                        type="textarea"
                        :rows="3"
                        resize="none"
                        placeholder="请输入附件安装及调试备注"
                    />
                </el-form-item>
                <el-form-item label="强压泄漏试验备注">
                    <el-input
                        v-model="noteForm.note_pressure_leak"
                        type="textarea"
                        :rows="3"
                        resize="none"
                        placeholder="请输入强压泄漏试验备注"
                    />
                </el-form-item>
                <el-form-item label="装箱备注">
                    <el-input
                        v-model="noteForm.note_packing"
                        type="textarea"
                        :rows="3"
                        resize="none"
                        placeholder="请输入装箱备注"
                    />
                </el-form-item>
            </el-form>
        </div>
        <template #footer>
            <span class="dialog-footer">
                <el-button @click="closeNoteDialog" :disabled="noteDialogSaving">取 消</el-button>
                <el-button type="primary" @click="submitNoteDetails" :loading="noteDialogSaving">确 认</el-button>
            </span>
        </template>
    </el-dialog>
</template>
<script setup lang="jsx">
    import extend from "@/extension/order/order_note_flat/ORDER_NOTE_FLAT.jsx";
    import viewOptions from './ORDER_NOTE_FLAT/options.js'
    import { ref, reactive, getCurrentInstance } from "vue";
    const grid = ref(null);
    const { proxy } = getCurrentInstance()
    //http请求，proxy.http.post/get
    const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

    let gridRef;//对应[表.jsx]文件中this.使用方式一样
    const noteDialogVisible = ref(false)
    const noteDialogLoading = ref(false)
    const noteDialogSaving = ref(false)
    const noteDialogTitle = ref('备注拆分')
    const currentRow = ref(null)
    const noteForm = reactive({
        source_entry_id: 0,
        remark_raw: '',
        internal_note: '',
        note_body_actuator: '',
        note_accessory_debug: '',
        note_pressure_leak: '',
        note_packing: ''
    })

    const resetNoteForm = () => {
        noteForm.source_entry_id = 0
        noteForm.remark_raw = ''
        noteForm.internal_note = ''
        noteForm.note_body_actuator = ''
        noteForm.note_accessory_debug = ''
        noteForm.note_pressure_leak = ''
        noteForm.note_packing = ''
    }

    const closeNoteDialog = () => {
        if (noteDialogSaving.value) {
            return
        }
        noteDialogVisible.value = false
    }

    const openNoteDialog = async (row) => {
        if (!row || !row.source_entry_id) {
            proxy.$message.error('未找到有效的source_entry_id')
            return
        }
        currentRow.value = row
        noteDialogTitle.value = `备注拆分 - ${row.contract_no || row.source_entry_id}`
        resetNoteForm()
        noteForm.source_entry_id = row.source_entry_id
        noteForm.note_body_actuator = row.note_body_actuator || ''
        noteForm.note_accessory_debug = row.note_accessory_debug || ''
        noteForm.note_pressure_leak = row.note_pressure_leak || ''
        noteForm.note_packing = row.note_packing || ''
        noteDialogVisible.value = true
        noteDialogLoading.value = true
        try {
            const result = await proxy.http.get(`api/ORDER_NOTE_FLAT/${row.source_entry_id}/note`, {}, false)
            if (result && result.status) {
                noteForm.remark_raw = result.data?.remark_raw || ''
                noteForm.internal_note = result.data?.internal_note || ''
            } else {
                proxy.$message.error(result?.message || '获取备注信息失败')
            }
        } catch (error) {
            proxy.$message.error('获取备注信息异常')
        } finally {
            noteDialogLoading.value = false
        }
    }

    const submitNoteDetails = async () => {
        if (!noteForm.source_entry_id) {
            proxy.$message.error('source_entry_id 无效，无法保存')
            return
        }
        noteDialogSaving.value = true
        try {
            const payload = {
                source_entry_id: noteForm.source_entry_id,
                note_body_actuator: noteForm.note_body_actuator,
                note_accessory_debug: noteForm.note_accessory_debug,
                note_pressure_leak: noteForm.note_pressure_leak,
                note_packing: noteForm.note_packing
            }
            const result = await proxy.http.post('api/ORDER_NOTE_FLAT/updateNoteDetails', payload, true)
            if (result && result.status) {
                proxy.$message.success(result.message || '保存成功')
                if (currentRow.value) {
                    currentRow.value.note_body_actuator = payload.note_body_actuator
                    currentRow.value.note_accessory_debug = payload.note_accessory_debug
                    currentRow.value.note_pressure_leak = payload.note_pressure_leak
                    currentRow.value.note_packing = payload.note_packing
                }
                noteDialogVisible.value = false
            } else {
                proxy.$message.error(result?.message || '保存失败')
            }
        } catch (error) {
            proxy.$message.error('保存备注拆分信息异常')
        } finally {
            noteDialogSaving.value = false
        }
    }

    //生成对象属性初始化
    const onInit = async ($vm) => {
        gridRef = $vm;
        //gridRef.setFixedSearchForm(true);
        //与jsx中的this.xx使用一样，只需将this.xx改为gridRef.xx

    }
    //生成对象属性初始化后,操作明细表配置用到
    const onInited = async () => {
        if (!gridRef.columns.some(c => c.field === '__note_split__')) {
            gridRef.columns.push({
                field: '__note_split__',
                title: '操作',
                width: 120,
                align: 'center',
                fixed: 'right',
                render: (h, { row }) => {
                    return (
                        <el-button
                            type="primary"
                            size="small"
                            plain
                            onClick={(event) => {
                                event.stopPropagation()
                                openNoteDialog(row)
                            }}
                        >
                            备注拆分
                        </el-button>
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
    //监听表单输入，做实时计算
    //watch(() => editFormFields.字段,(newValue, oldValue) => { })
    //对外暴露数据
    defineExpose({})
</script>
