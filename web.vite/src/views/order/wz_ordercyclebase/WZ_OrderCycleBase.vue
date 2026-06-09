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
        <el-button type="info"
                   :loading="refreshLoading"
                   :disabled="syncLoading || ruleLoading || initLoading"
                   @click="handleRefreshOrders">刷新订单数据</el-button>
        <el-button type="primary"
                   :loading="syncLoading"
                   :disabled="ruleLoading || initLoading"
                   @click="handleSync">同步排产数据</el-button>
        <el-button type="success"
                   :loading="ruleLoading"
                   :disabled="initLoading"
                   @click="handleOptimize">智能体优化</el-button>
        <el-button type="warning"
                   :loading="initLoading"
                   :disabled="syncLoading || ruleLoading"
                   @click="handleInitialize">排产初始化</el-button>
        <el-button type="primary"
                   plain
                   :loading="dictSyncLoading"
                   :disabled="syncLoading || ruleLoading || initLoading || refreshLoading"
                   @click="handleSyncDictionary">同步字典</el-button>
        <el-button type="success"
                   plain
                   :loading="predictionExportLoading"
                   :disabled="syncLoading || ruleLoading || initLoading || refreshLoading || dictSyncLoading"
                   @click="handleExportPredictionReview">导出预测数据</el-button>
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
        <el-descriptions-item label="状态">{{ progressStatusText }}</el-descriptions-item>
        <el-descriptions-item label="阶段">{{ progressSummary.stage || '-' }}</el-descriptions-item>
        <el-descriptions-item label="总数">{{ progressSummary.total }}</el-descriptions-item>
        <el-descriptions-item label="已处理">{{ progressSummary.processed }}</el-descriptions-item>
        <el-descriptions-item label="成功">{{ progressSummary.succeeded }}</el-descriptions-item>
        <el-descriptions-item label="失败">{{ progressSummary.failed }}</el-descriptions-item>
        <el-descriptions-item label="已更新">{{ progressSummary.updated }}</el-descriptions-item>
        <el-descriptions-item label="分批进度">
          {{ progressSummary.batchCount }} / {{ progressSummary.totalBatchCount || '-' }}
        </el-descriptions-item>
        <el-descriptions-item v-if="progressSummary.message" label="结果">
          {{ progressSummary.message }}
        </el-descriptions-item>
        <el-descriptions-item v-if="progressSummary.error" label="异常">
          {{ progressSummary.error }}
        </el-descriptions-item>
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

  <el-dialog v-model="syncDialogVisible"
             class="wz-sync-dialog"
             title="选择订单审核日期范围"
             width="520px"
             :body-style="{ padding: '16px 20px' }"
             :close-on-click-modal="false">
    <div class="wz-sync-dialog__content">
      <p class="wz-sync-dialog__tip">
        同步排产数据会重置当前页面全部数据，需要重新排产，请确认审核日期范围。
      </p>
      <el-form :model="syncForm" label-width="120px">
        <el-form-item label="审核日期范围">
          <el-date-picker v-model="syncForm.approvedDateRange"
                          type="daterange"
                          range-separator="至"
                          start-placeholder="开始日期"
                          end-placeholder="结束日期"
                          format="YYYY-MM-DD"
                          value-format="YYYY-MM-DD" />
        </el-form-item>
      </el-form>
    </div>
    <template #footer>
      <el-button @click="syncDialogVisible = false" :disabled="syncLoading">取消</el-button>
      <el-button type="primary" :loading="syncLoading" @click="handleSyncConfirm">开始同步</el-button>
    </template>
  </el-dialog>
</template>
<script setup lang="jsx">
import extend from "@/extension/order//wz_ordercyclebase/WZ_OrderCycleBase.jsx";
import viewOptions from './WZ_OrderCycleBase/options.js'
import { ref, reactive, getCurrentInstance, computed, onBeforeUnmount } from "vue";
import { ElMessage } from 'element-plus'
const grid = ref(null);
const { proxy } = getCurrentInstance()
//http请求，proxy.http.post/get
const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

const syncLoading = ref(false);
const initLoading = ref(false);
const ruleLoading = ref(false);
const refreshLoading = ref(false);
const dictSyncLoading = ref(false);
const predictionExportLoading = ref(false);
const progressVisible = ref(false);
const syncDialogVisible = ref(false);
const syncForm = reactive({
  approvedDateRange: []
});
const progressSummary = reactive({
  taskId: '',
  status: '',
  stage: '',
  message: '',
  total: 0,
  processed: 0,
  succeeded: 0,
  failed: 0,
  updated: 0,
  batchCount: 0,
  totalBatchCount: 0,
  percent: 0,
  logFiles: [],
  error: '',
  updatedAt: ''
});
let optimizePollTimer = null;

const progressPercent = computed(() => {
  if (progressSummary.percent > 0) {
    return Math.min(100, progressSummary.percent);
  }
  if (progressSummary.total === 0) {
    return ruleLoading.value ? 20 : 0;
  }
  const processed = progressSummary.processed || progressSummary.succeeded + progressSummary.failed;
  if (processed <= 0) {
    return ruleLoading.value ? 20 : 0;
  }
  return Math.min(100, Math.round((processed / progressSummary.total) * 100));
});

const progressStatus = computed(() => {
  if (progressSummary.status === 'failed') {
    return 'exception';
  }
  if (ruleLoading.value && progressSummary.status !== 'success') {
    return 'warning';
  }
  if (progressSummary.failed > 0 && progressSummary.succeeded === 0) {
    return 'exception';
  }
  return 'success';
});

const progressStatusText = computed(() => {
  if (progressSummary.status === 'success') {
    return '已完成';
  }
  if (progressSummary.status === 'failed') {
    return '失败';
  }
  if (ruleLoading.value || progressSummary.status === 'running') {
    return '执行中';
  }
  return '未开始';
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

const formatDate = (date) => {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${year}-${month}-${day}`;
};

const getDefaultApprovedDateRange = () => {
  const start = new Date();
  const end = new Date(start);
  end.setDate(start.getDate() + 1);
  return [formatDate(start), formatDate(end)];
};

const resetProgressSummary = () => {
  progressSummary.taskId = '';
  progressSummary.status = '';
  progressSummary.stage = '';
  progressSummary.message = '';
  progressSummary.total = 0;
  progressSummary.processed = 0;
  progressSummary.succeeded = 0;
  progressSummary.failed = 0;
  progressSummary.updated = 0;
  progressSummary.batchCount = 0;
  progressSummary.totalBatchCount = 0;
  progressSummary.percent = 0;
  progressSummary.logFiles = [];
  progressSummary.error = '';
  progressSummary.updatedAt = '';
};

const pickValue = (source, ...keys) => {
  if (!source) {
    return undefined;
  }
  for (const key of keys) {
    if (source[key] !== undefined && source[key] !== null) {
      return source[key];
    }
  }
  return undefined;
};

const toNumber = (value) => {
  const number = Number(value);
  return Number.isFinite(number) ? number : 0;
};

const normalizeOptimizeData = (response) => {
  const data = response?.data ?? response?.Data ?? response;
  return {
    taskId: pickValue(data, 'taskId', 'TaskId') || '',
    status: pickValue(data, 'status', 'Status') || '',
    stage: pickValue(data, 'stage', 'Stage') || '',
    message: pickValue(data, 'message', 'Message') || response?.message || response?.Message || '',
    total: toNumber(pickValue(data, 'total', 'Total')),
    processed: toNumber(pickValue(data, 'processed', 'Processed')),
    succeeded: toNumber(pickValue(data, 'succeeded', 'Succeeded')),
    failed: toNumber(pickValue(data, 'failed', 'Failed')),
    updated: toNumber(pickValue(data, 'updated', 'Updated')),
    batchCount: toNumber(pickValue(data, 'batchCount', 'BatchCount')),
    totalBatchCount: toNumber(pickValue(data, 'totalBatchCount', 'TotalBatchCount')),
    percent: toNumber(pickValue(data, 'percent', 'Percent')),
    logFiles: pickValue(data, 'logFiles', 'LogFiles') || [],
    error: pickValue(data, 'error', 'Error') || '',
    updatedAt: pickValue(data, 'updatedAt', 'UpdatedAt') || ''
  };
};

const updateProgressSummary = (summary) => {
  progressSummary.taskId = summary.taskId || progressSummary.taskId;
  progressSummary.status = summary.status || progressSummary.status;
  progressSummary.stage = summary.stage || progressSummary.stage;
  progressSummary.message = summary.message || progressSummary.message;
  progressSummary.total = summary.total;
  progressSummary.processed = summary.processed;
  progressSummary.succeeded = summary.succeeded;
  progressSummary.failed = summary.failed;
  progressSummary.updated = summary.updated;
  progressSummary.batchCount = summary.batchCount;
  progressSummary.totalBatchCount = summary.totalBatchCount;
  progressSummary.percent = summary.percent;
  progressSummary.logFiles = Array.isArray(summary.logFiles)
    ? summary.logFiles
    : [summary.logFiles].filter(Boolean);
  progressSummary.error = summary.error || '';
  progressSummary.updatedAt = summary.updatedAt || '';
};

const clearOptimizePoll = () => {
  if (optimizePollTimer) {
    clearInterval(optimizePollTimer);
    optimizePollTimer = null;
  }
};

const finishOptimizeTask = (summary) => {
  clearOptimizePoll();
  ruleLoading.value = false;
  updateProgressSummary(summary);
  refreshGrid();

  if (summary.status === 'failed') {
    ElMessage.error(summary.error || summary.message || '智能体优化失败');
    return;
  }

  const successMsg = `优化完成，成功 ${summary.succeeded} 条，更新 ${summary.updated} 条`;
  ElMessage.success(summary.message || successMsg);
};

const pollOptimizeProgress = async (taskId) => {
  if (!taskId) {
    return;
  }

  const response = await proxy.http.post(
    `/api/WZ_OrderCycleBase/valve-rule-service-task-progress?taskId=${taskId}`,
    {},
    false
  );
  const status = response?.status ?? response?.Status;
  if (status === false) {
    return;
  }

  const summary = normalizeOptimizeData(response);
  updateProgressSummary(summary);
  if (summary.status === 'success' || summary.status === 'failed') {
    finishOptimizeTask(summary);
  }
};

onBeforeUnmount(() => {
  clearOptimizePoll();
});

const normalizeInitializeData = (response) => {
  const data = response?.data ?? response?.Data ?? {};
  const warnings = pickValue(data, 'warnings', 'Warnings') || [];
  const capacitySchedule = pickValue(data, 'capacitySchedule', 'CapacitySchedule') || {};
  const assignedProductionLine = pickValue(data, 'assignedProductionLine', 'AssignedProductionLine') || {};
  const valveRule = pickValue(data, 'valveRule', 'ValveRule') || {};

  return {
    warnings: Array.isArray(warnings) ? warnings : [warnings].filter(Boolean),
    remainingBlank: toNumber(pickValue(data, 'remainingNonBjBlankCapacityScheduleDate', 'RemainingNonBjBlankCapacityScheduleDate')),
    capacityUpdated: toNumber(pickValue(capacitySchedule, 'updated', 'Updated')),
    fallbackScheduleDateCount: toNumber(pickValue(capacitySchedule, 'fallbackScheduleDateCount', 'FallbackScheduleDateCount')),
    overThresholdCount: toNumber(pickValue(capacitySchedule, 'overThresholdCount', 'OverThresholdCount')),
    capacityFailed: toNumber(pickValue(capacitySchedule, 'failed', 'Failed')),
    missingThreshold: toNumber(pickValue(capacitySchedule, 'missingThreshold', 'MissingThreshold')),
    missingOutput: toNumber(pickValue(capacitySchedule, 'missingProductionOutput', 'MissingProductionOutput')),
    assignedFailed: toNumber(pickValue(assignedProductionLine, 'failed', 'Failed')),
    valveRuleFailed: toNumber(pickValue(valveRule, 'failed', 'Failed')),
    preProductionOutputSynced: toNumber(pickValue(data, 'preProductionOutputSynced', 'PreProductionOutputSynced'))
  };
};

const buildInitializeMessage = (summary) => {
  const parts = [
    `优化日期更新 ${summary.capacityUpdated} 条`,
    `排产日期兜底 ${summary.fallbackScheduleDateCount} 条`,
    `超阈值标红 ${summary.overThresholdCount} 条`,
    `预排产同步 ${summary.preProductionOutputSynced} 条`
  ];

  if (summary.remainingBlank > 0) {
    parts.push(`非BJ仍空 ${summary.remainingBlank} 条`);
  }

  if (summary.missingThreshold > 0) {
    parts.push(`阈值缺失 ${summary.missingThreshold} 条`);
  }

  if (summary.missingOutput > 0) {
    parts.push(`未命中产能 ${summary.missingOutput} 条`);
  }

  if (summary.assignedFailed > 0) {
    parts.push(`产线失败 ${summary.assignedFailed} 条`);
  }

  if (summary.valveRuleFailed > 0) {
    parts.push(`规则服务失败 ${summary.valveRuleFailed} 条`);
  }

  return parts.join('，');
};

const refreshGrid = () => {
  if (gridRef && gridRef.search) {
    gridRef.search();
  }
};

const handleSync = async () => {
  if (syncLoading.value || ruleLoading.value) {
    return;
  }

  syncForm.approvedDateRange = getDefaultApprovedDateRange();
  syncDialogVisible.value = true;
};

const handleSyncConfirm = async () => {
  if (syncLoading.value || ruleLoading.value) {
    return;
  }

  const range = syncForm.approvedDateRange || [];
  if (!Array.isArray(range) || range.length !== 2) {
    ElMessage.warning('请选择审核日期范围');
    return;
  }

  syncLoading.value = true;
  syncDialogVisible.value = false;

  const [startDate, endDate] = range;
  const query = new URLSearchParams({ startDate, endDate }).toString();
  try {
    const response = await proxy.http.post(`/api/WZ_OrderCycleBase/sync-from-order-tracking?${query}`, {}, true);
    if (response.status) {
      ElMessage.success(response.message || '同步排产数据成功');
      refreshGrid();
    } else {
      refreshGrid();
    }
  } catch (error) {
    refreshGrid();
  } finally {
    syncLoading.value = false;
  }
};

const handleRefreshOrders = async () => {
  if (refreshLoading.value || syncLoading.value || ruleLoading.value || initLoading.value) {
    return;
  }

  refreshLoading.value = true;
  try {
    const response = await proxy.http.post('/api/WZ_OrderCycleBase/refresh-order-tracking', {}, true);
    if (response?.status === false) {
      ElMessage.error(response.message || '订单数据刷新失败');
      return;
    }

    ElMessage.success(response?.message || '订单数据刷新完成');
  } catch (error) {
    ElMessage.error('订单数据刷新异常');
  } finally {
    refreshLoading.value = false;
  }
};

const handleOptimize = async () => {
  if (ruleLoading.value) {
    return;
  }

  clearOptimizePoll();
  resetProgressSummary();
  progressVisible.value = true;
  ruleLoading.value = true;

  try {
    const response = await proxy.http.post('/api/WZ_OrderCycleBase/start-valve-rule-service-task', {}, false);
    const status = response?.status ?? response?.Status;
    if (status === false) {
      ElMessage.error(response?.message || response?.Message || '智能体优化失败');
      ruleLoading.value = false;
      refreshGrid();
      return;
    }

    const summary = normalizeOptimizeData(response);
    updateProgressSummary(summary);
    const taskId = summary.taskId || progressSummary.taskId;
    if (!taskId) {
      ruleLoading.value = false;
      ElMessage.error('智能体优化任务启动失败：未返回任务编号');
      return;
    }

    await pollOptimizeProgress(taskId);
    if (!ruleLoading.value) {
      return;
    }

    optimizePollTimer = setInterval(() => {
      pollOptimizeProgress(taskId).catch(() => {});
    }, 1000);
  } catch (error) {
    clearOptimizePoll();
    ruleLoading.value = false;
    ElMessage.error('智能体优化异常');
    refreshGrid();
  }
};

const handleInitialize = async () => {
  if (initLoading.value || syncLoading.value || ruleLoading.value) {
    return;
  }

  initLoading.value = true;

  try {
    const response = await proxy.http.post('/api/WZ_OrderCycleBase/initialize-scheduling');
    refreshGrid();
    const status = response?.status ?? response?.Status;
    if (status === false) {
      ElMessage.error(response?.message || response?.Message || '排产初始化失败');
      return;
    }

    const summary = normalizeInitializeData(response);
    const message = response?.message || response?.Message || buildInitializeMessage(summary);
    if (summary.warnings.length || summary.remainingBlank > 0 || summary.missingThreshold > 0 || summary.missingOutput > 0) {
      ElMessage.warning(message);
    } else {
      ElMessage.success(message || '排产初始化完成');
    }
  } catch (error) {
    ElMessage.error('排产初始化异常');
    refreshGrid();
  } finally {
    initLoading.value = false;
  }
};

const handleSyncDictionary = async () => {
  if (dictSyncLoading.value || syncLoading.value || ruleLoading.value || initLoading.value || refreshLoading.value) {
    return;
  }

  dictSyncLoading.value = true;
  try {
    const response = await proxy.http.post('http://10.11.10.101:8000/sync_fmzd_today?dry_run=false', {}, true);
    if (response?.status === false) {
      ElMessage.error(response.message || '同步字典失败');
      return;
    }
    ElMessage.success(response?.message || '同步字典完成');
  } catch (error) {
    ElMessage.error('同步字典异常');
  } finally {
    dictSyncLoading.value = false;
  }
};

const handleExportPredictionReview = async () => {
  if (predictionExportLoading.value || syncLoading.value || ruleLoading.value || initLoading.value || refreshLoading.value || dictSyncLoading.value) {
    return;
  }

  predictionExportLoading.value = true;
  try {
    proxy.http.download(
      '/api/WZ_OrderCycleBase/export-schedule-prediction-review',
      {},
      '排产日期预测核对.xlsx',
      'loading....'
    );
  } finally {
    predictionExportLoading.value = false;
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

.wz-sync-dialog__content {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.wz-sync-dialog__tip {
  margin: 0;
  color: #606266;
  font-size: 13px;
}
</style>
