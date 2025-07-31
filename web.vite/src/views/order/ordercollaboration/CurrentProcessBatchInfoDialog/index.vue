<template>
    <div class="reminder-dialog">
        <CompDialog v-model="visible" title="查看批次信息" width="80%" :height="height" :padding="0" :footer="true"
            :onModelClose="handleDialogClose">
            <template #content>
                <div class="dialog-content">
                    <ProcessBatchInfo ref="processBatchInfoRef"></ProcessBatchInfo>
                </div>
            </template>

            <template #footer>
                <div class="dialog-footer">
                    <el-button @click="handleCancel">
                        取消
                    </el-button>
                </div>
            </template>
        </CompDialog>

        <!-- 人员选择器 -->
        <PersonSelector v-model="personSelectorVisible" :selectedPersonId="selectedPersonId" @confirm="handlePersonConfirm"
            @cancel="handlePersonCancel" />
    </div>
</template>
  
<script setup>
import { ref, computed, watch, nextTick, getCurrentInstance } from 'vue'
import { User } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import PersonSelector from '@/comp/person-selector/index.vue'
import CompDialog from '@/comp/dialog/index.vue'
import ProcessBatchInfo from '../OCP_CurrentProcessBatchInfo.vue'
// Props
const props = defineProps({
    modelValue: {
        type: Boolean,
        default: false
    },
    data: {
        type: Object,
        default: () => ({})
    },
    height: {
        type: Number,
        default: 600
    }
})

// Emits
const emit = defineEmits(['update:modelValue', 'send'])

// 响应式数据
const visible = ref(false)
const formRef = ref()
const personSelectorVisible = ref(false)
const selectedPersonId = ref('')
const { proxy } = getCurrentInstance()
const processBatchInfoRef = ref();




// 表单数据
const formData = ref({

})

// 业务类型选项
const businessTypeOptions = ref([])
// 业务类型显示标签
const businessTypeLabel = ref('')

// 表单验证规则
const formRules = ref({

})

// 监听props变化
watch(
    () => props.modelValue,
    async (newVal) => {
        visible.value = newVal
        if (newVal) {

        } else {

        }
    },
    { immediate: true }
)

// 监听内部visible变化，同步到外部
watch(
    visible,
    (newVal) => {
        if (newVal !== props.modelValue) {
            emit('update:modelValue', newVal)
        }
    }
)


// 方法
const handleCancel = () => {
    handleClose()
}

const handleClose = () => {
    visible.value = false
    emit('update:modelValue', false)
}


// 重置表单数据
const resetFormData = () => {
    formData.value = {

    }

    // 清除表单验证状态
    nextTick(() => {
        if (formRef.value) {
            formRef.value.clearValidate()
        }
    })
}

const handleDialogClose = () => {
    resetFormData()
    handleClose()
    return true
}

defineExpose({
    search: () => {
        processBatchInfoRef.value.$refs.grid.search();
    }
})
</script>
  
<style scoped>
.dialog-content {
    padding: 24px;
    max-height: 600px;
    overflow-y: auto;
}

.assigned-persons-container {
    display: flex;
    flex-direction: row;
    gap: 12px;
    align-items: center;
}

.person-tag {
    max-width: 120px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.dialog-footer {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
}

:deep(.el-form-item__label) {
    font-weight: 500;
    color: #606266;
}

:deep(.el-select) {
    width: 100%;
}

:deep(.el-input-group__append .el-select) {
    width: auto;
}

:deep(.el-textarea__inner) {
    resize: vertical;
}
</style>