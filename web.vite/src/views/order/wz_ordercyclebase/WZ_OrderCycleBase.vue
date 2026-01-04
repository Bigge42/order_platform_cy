<!--
 *Author：jxx
 *Date：{Date}
 *Contact：461857658@qq.com
 *业务请在@/extension/order//wz_ordercyclebase/WZ_OrderCycleBase.jsx或WZ_OrderCycleBase.vue文件编写
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
        <template #btnLeft>
            <div class="wz-ordercyclebase-action">
                <el-button type="success" :loading="ruleLoading" @click="handleOptimize">智能体优化</el-button>
                <el-button type="primary"
                           :loading="exportLoading"
                           :disabled="ruleLoading || exportLoading"
                           @click="handleExportMissingCycle">一键导出</el-button>
            </div>
        </template>
    </view-grid>

    <el-dialog v-model="progressVisible"
               class="wz-progress-dialog"
               title="智能体优化进度"
               width="520px"
               :body-style="{ padding: '16px 20px' }"
               :close-on-click-modal="false">
        <div class="wz-progress-dialog__content">
            <el-progress :percentage="progressPercent"
                         :indeterminate="ruleLoading && progressSummary.total === 0"
                         :status="progressStatus"
                         stroke-width="14"></el-progress>
            <el-descriptions :column="1" border>
                <el-descriptions-item label="总数">{{ progressSummary.total }}</el-descriptions-item>
                <el-descriptions-item label="成功">{{ progressSummary.succeeded }}</el-descriptions-item>
                <el-descriptions-item label="失败">{{ progressSummary.failed }}</el-descriptions-item>
                <el-descriptions-item label="已更新">{{ progressSummary.updated }}</el-descriptions-item>
                <el-descriptions-item label="分批次数">{{ progressSummary.batchCount }}</el-descriptions-item>
                <el-descriptions-item v-if="progressSummary.logFiles.length" label="日志">
                    <div class="log-list">
                        <div v-for="(log, index) in progressSummary.logFiles" :key="index">{{ log }}</div>
                    </div>
                </el-descriptions-item>
            </el-descriptions>
        </div>
        <template #footer>
            <el-button @click="progressVisible = false" :disabled="ruleLoading">关闭</el-button>
        </template>
    </el-dialog>
</template>
<script setup lang="jsx">
    import extend from "@/extension/order//wz_ordercyclebase/WZ_OrderCycleBase.jsx";
    import viewOptions from './WZ_OrderCycleBase/options.js'
    import { ref, reactive, getCurrentInstance, computed } from "vue";
    import { ElMessage } from 'element-plus'
    const grid = ref(null);
    const { proxy } = getCurrentInstance()
    //http请求，proxy.http.post/get
    const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

    const ruleLoading = ref(false);
    const exportLoading = ref(false);
    const progressVisible = ref(false);
    const progressSummary = reactive({
        total: 0,
        succeeded: 0,
        failed: 0,
        updated: 0,
        batchCount: 0,
        logFiles: []
    });

    const progressPercent = computed(() => {
        if (progressSummary.total === 0) {
            return ruleLoading.value ? 20 : 0;
        }
        const processed = progressSummary.succeeded + progressSummary.failed;
        if (processed <= 0) {
            return ruleLoading.value ? 20 : 0;
        }
        return Math.min(100, Math.round((processed / progressSummary.total) * 100));
    });

    const progressStatus = computed(() => {
        if (ruleLoading.value) {
            return 'warning';
        }
        if (progressSummary.failed > 0 && progressSummary.succeeded === 0) {
            return 'exception';
        }
        return 'success';
    });

    let gridRef;//对应[表.jsx]文件中this.使用方式一样
    //生成对象属性初始化
    const onInit = async ($vm) => {
        gridRef = $vm;
        //gridRef.setFixedSearchForm(true);
        //与jsx中的this.xx使用一样，只需将this.xx改为gridRef.xx
 
    }
    //生成对象属性初始化后,操作明细表配置用到
    const onInited = async () => {
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

    const resetProgressSummary = () => {
        progressSummary.total = 0;
        progressSummary.succeeded = 0;
        progressSummary.failed = 0;
        progressSummary.updated = 0;
        progressSummary.batchCount = 0;
        progressSummary.logFiles = [];
    };

    const handleOptimize = async () => {
        if (ruleLoading.value) {
            return;
        }

        resetProgressSummary();
        progressVisible.value = true;
        ruleLoading.value = true;

        try {
            const response = await proxy.http.post('/api/WZ_OrderCycleBase/batch-call-valve-rule-service');
            if (response.status && response.data) {
                progressSummary.total = response.data.total || 0;
                progressSummary.succeeded = response.data.succeeded || 0;
                progressSummary.failed = response.data.failed || 0;
                progressSummary.updated = response.data.updated || 0;
                progressSummary.batchCount = response.data.batchCount || 0;
                progressSummary.logFiles = response.data.logFiles || [];

                if (gridRef && gridRef.search) {
                    gridRef.search();
                }

                const successMsg = `优化完成，成功 ${progressSummary.succeeded} 条，更新 ${progressSummary.updated} 条`;
                ElMessage.success(response.message || successMsg);
            } else {
                ElMessage.error(response.message || '智能体优化失败');
            }
        } catch (error) {
            ElMessage.error('智能体优化失败');
        } finally {
            ruleLoading.value = false;
        }
    };

    const getExportColumns = () => {
        if (!Array.isArray(columns)) {
            return [];
        }

        const exportColumns = [];

        columns.forEach((item) => {
            if (item.hidden || item.render) {
                return;
            }

            if (item.children && Array.isArray(item.children)) {
                exportColumns.push(
                    ...item.children
                        .filter((child) => !child.hidden && !child.render)
                        .map((child) => child.field)
                        .filter(Boolean)
                );
                return;
            }

            if (item.field) {
                exportColumns.push(item.field);
            }
        });

        return exportColumns;
    };

    const getMissingCycleExportParams = () => {
        const pagination = gridRef?.$refs?.table?.paginations || {};
        const wheres = gridRef?.base?.getSearchParameters?.(gridRef, searchFormFields, searchFormOptions) || [];

        wheres.push({
            name: 'FixedCycleDays',
            value: null,
            displayType: 'EMPTY'
        });

        if (!wheres.some((x) => x.name === table.key)) {
            const ids = gridRef?.getSelectRows?.()
                ?.map((row) => row[table.key])
                .filter(Boolean)
                .join(',');

            if (ids) {
                wheres.push({
                    name: table.key,
                    value: ids,
                    displayType: 'selectList'
                });
            }
        }

        const params = {
            order: pagination.order,
            sort: pagination.sort,
            wheres
        };

        const exportColumns = getExportColumns();
        if (exportColumns.length) {
            params.columns = exportColumns;
        }

        return params;
    };

    const handleExportMissingCycle = async () => {
        if (ruleLoading.value || exportLoading.value) {
            return;
        }

        exportLoading.value = true;
        const payload = getMissingCycleExportParams();
        const fileName = `固定周期缺失_${new Date().toISOString().slice(0, 10)}.xlsx`;

        if (payload.wheres && typeof payload.wheres === 'object') {
            payload.wheres = JSON.stringify(payload.wheres);
        }

        try {
            await new Promise((resolve, reject) => {
                const timeoutId = setTimeout(() => reject(new Error('导出超时')), 600000);
                proxy.http.download('/api/WZ_OrderCycleBase/Export', payload, fileName, '正在导出...', () => {
                    clearTimeout(timeoutId);
                    resolve(true);
                });
            });
            ElMessage.success('导出成功');
        } catch (error) {
            ElMessage.error(error?.message || '导出失败，请稍后重试');
        } finally {
            exportLoading.value = false;
        }
    };
    //监听表单输入，做实时计算
    //watch(() => editFormFields.字段,(newValue, oldValue) => {	})
    //对外暴露数据
    defineExpose({})
</script>

<style scoped>
.wz-ordercyclebase-action {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 6px 12px;
    background-color: #fff;
    border-radius: 4px;
}

.wz-progress-dialog__content {
    background-color: #fff;
    border-radius: 4px;
    padding: 8px 8px 0;
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.log-list {
    display: flex;
    flex-direction: column;
    gap: 4px;
    word-break: break-all;
}
</style>
