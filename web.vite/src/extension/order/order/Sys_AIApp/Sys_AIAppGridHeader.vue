<template>
  <vol-box
    v-model="model"
    :title="labels.title"
    :width="1100"
    :height="660"
    :padding="0"
  >
    <div class="sync-wrap">
      <div class="sync-toolbar">
        <el-input
          v-model="keyword"
          clearable
          :placeholder="labels.keywordPlaceholder"
          @keyup.enter="loadApps"
        />
        <el-button type="primary" :loading="loading" @click="loadApps">
          {{ labels.search }}
        </el-button>
        <el-button :loading="loading" @click="resetAndReload">
          {{ labels.refresh }}
        </el-button>
      </div>

      <el-alert :title="summaryText" type="info" :closable="false" show-icon />

      <div v-if="lastSyncResult" class="result-wrap">
        <el-alert
          :title="resultSummaryText"
          :type="resultAlertType"
          :closable="false"
          show-icon
        />

        <div class="result-stats">
          <div class="result-stat">
            <span class="result-stat__label">{{ labels.resultTotal }}</span>
            <strong>{{ lastSyncResult.totalSelected }}</strong>
          </div>
          <div class="result-stat">
            <span class="result-stat__label">{{ labels.resultMatched }}</span>
            <strong>{{ lastSyncResult.matchedCount }}</strong>
          </div>
          <div class="result-stat">
            <span class="result-stat__label">{{ labels.resultSuccess }}</span>
            <strong>{{ lastSyncResult.successCount }}</strong>
          </div>
          <div class="result-stat">
            <span class="result-stat__label">{{ labels.resultFailed }}</span>
            <strong>{{ lastSyncResult.failedCount }}</strong>
          </div>
          <div class="result-stat">
            <span class="result-stat__label">{{ labels.resultInserted }}</span>
            <strong>{{ lastSyncResult.inserted }}</strong>
          </div>
          <div class="result-stat">
            <span class="result-stat__label">{{ labels.resultUpdated }}</span>
            <strong>{{ lastSyncResult.updated }}</strong>
          </div>
        </div>

        <div v-if="lastSyncResult.details.length" class="result-details">
          <div class="result-title">{{ labels.resultDetails }}</div>
          <el-table :data="lastSyncResult.details" border size="small" max-height="220">
            <el-table-column prop="appName" :label="labels.appName" min-width="160" />
            <el-table-column prop="platformAppId" :label="labels.platformAppId" min-width="210" />
            <el-table-column :label="labels.resultAction" width="110" align="center">
              <template #default="{ row }">
                <el-tag :type="getResultActionTagType(row.action)" size="small">
                  {{ resultActionMap[row.action] || labels.failed }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column :label="labels.resultKeyAction" width="130" align="center">
              <template #default="{ row }">
                <el-tag :type="getKeyActionTagType(row.keyAction)" size="small">
                  {{ keyActionMap[row.keyAction] || labels.noChange }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="message" :label="labels.resultMessage" min-width="320" show-overflow-tooltip />
          </el-table>
        </div>
      </div>

      <el-table
        ref="tableRef"
        v-loading="loading"
        :data="apps"
        row-key="platformAppId"
        border
        :height="tableHeight"
        @selection-change="handleSelectionChange"
      >
        <el-table-column type="selection" width="55" reserve-selection />
        <el-table-column prop="appName" :label="labels.appName" min-width="180" />
        <el-table-column prop="appType" :label="labels.appType" width="110">
          <template #default="{ row }">
            <el-tag size="small">{{ appTypeMap[row.appType] || row.appType || '-' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="platformAppId" :label="labels.platformAppId" min-width="220" />
        <el-table-column prop="description" :label="labels.description" min-width="260" show-overflow-tooltip />
        <el-table-column :label="labels.platformKey" width="130" align="center">
          <template #default="{ row }">
            <el-tag :type="getPlatformKeyTagType(row)" size="small">
              {{ getPlatformKeyText(row) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="labels.syncStatus" width="120" align="center">
          <template #default="{ row }">
            <el-tag :type="row.exists ? 'warning' : 'success'" size="small">
              {{ row.exists ? labels.existing : labels.newItem }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="labels.localKey" width="120" align="center">
          <template #default="{ row }">
            <el-tag :type="row.hasLocalKey ? 'success' : 'info'" size="small">
              {{ row.hasLocalKey ? labels.hasKey : labels.noKey }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="labels.actions" width="140" align="center" fixed="right">
          <template #default="{ row }">
            <el-button
              v-if="shouldShowCreateKeyButton(row)"
              type="primary"
              size="small"
              :loading="creatingKeyId === row.platformAppId"
              @click="createPlatformKey(row)"
            >
              {{ getCreateKeyButtonText(row) }}
            </el-button>
            <span v-else>{{ labels.noAction }}</span>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <template #footer>
      <div class="sync-footer">
        <span>{{ selectedSummary }}</span>
        <div>
          <el-button @click="model = false">{{ labels.close }}</el-button>
          <el-button type="primary" :loading="syncing" @click="syncSelected">
            {{ labels.confirmSync }}
          </el-button>
        </div>
      </div>
    </template>
  </vol-box>
</template>

<script setup>
import VolBox from '@/components/basic/VolBox.vue'
import { computed, getCurrentInstance, nextTick, ref } from 'vue'
import {
  buildSyncResultState,
  getRetrySelectionIds,
  getSyncResultAlertType,
  getSyncResultSummaryText
} from './syncResult'

const emit = defineEmits(['parentCall'])
const { proxy } = getCurrentInstance()

const labels = {
  title: '\u4eceAI\u5c0f\u52a9\u624b\u5e73\u53f0\u540c\u6b65\u5e94\u7528',
  keywordPlaceholder: '\u6309\u5e94\u7528\u540d\u79f0\u641c\u7d22',
  search: '\u641c\u7d22',
  refresh: '\u5237\u65b0',
  appName: '\u5e94\u7528\u540d\u79f0',
  appType: '\u5e94\u7528\u7c7b\u578b',
  platformAppId: '\u5e73\u53f0AppID',
  description: '\u5e94\u7528\u63cf\u8ff0',
  platformKey: '\u5e73\u53f0AppKey',
  syncStatus: '\u540c\u6b65\u72b6\u6001',
  localKey: '\u672c\u5730AppKey',
  actions: '\u64cd\u4f5c',
  existing: '\u5df2\u5b58\u5728',
  newItem: '\u65b0\u589e',
  hasKey: '\u5df2\u914d\u7f6e',
  noKey: '\u5f85\u586b\u5145',
  platformKeyReady: '\u5df2\u53ef\u53d6',
  platformKeyMissing: '\u672a\u521b\u5efa',
  platformKeyUnavailable: '\u65e0\u53ef\u7528Key',
  platformKeyReadFailed: '\u8bfb\u53d6\u5931\u8d25',
  createPlatformKey: '\u521b\u5efaAppKey',
  recreatePlatformKey: '\u91cd\u5efaAppKey',
  createKeyConfirmTitle: '\u521b\u5efa\u786e\u8ba4',
  createKeyConfirmText: '\u5f53\u524d\u5e94\u7528\u6ca1\u6709\u53ef\u7528\u7684\u5e73\u53f0 AppKey\uff0c\u786e\u8ba4\u73b0\u5728\u521b\u5efa\u5417\uff1f',
  createKeyConfirmButton: '\u786e\u8ba4\u521b\u5efa',
  cancel: '\u53d6\u6d88',
  noAction: '-',
  close: '\u5173\u95ed',
  confirmSync: '\u786e\u8ba4\u540c\u6b65',
  selectedPrefix: '\u5df2\u9009',
  selectedSuffix: '\u4e2a\u5e94\u7528',
  selectWarning: '\u8bf7\u5148\u9009\u62e9\u9700\u8981\u540c\u6b65\u7684\u5e94\u7528',
  defaultSummary:
    '\u7cfb\u7edf\u4f1a\u4ee5 PlatformAppId \u4e3a\u552f\u4e00\u952e\u6267\u884c\u65b0\u589e/\u66f4\u65b0\uff0c\u540c\u6b65\u65f6\u53ea\u8bfb\u53d6\u5e73\u53f0\u73b0\u6709\u53ef\u7528 AppKey\uff0c\u4e0d\u4f1a\u81ea\u52a8\u521b\u5efa\uff1b\u5df2\u5b58\u5728\u8bb0\u5f55\u9ed8\u8ba4\u4e0d\u8986\u76d6\u672c\u5730 AppKey\u3002',
  resultTotal: '\u9009\u4e2d',
  resultMatched: '\u547d\u4e2d',
  resultSuccess: '\u6210\u529f',
  resultFailed: '\u5931\u8d25',
  resultInserted: '\u65b0\u589e',
  resultUpdated: '\u66f4\u65b0',
  resultDetails: '\u540c\u6b65\u660e\u7ec6',
  resultAction: '\u7ed3\u679c',
  resultKeyAction: 'Key\u5904\u7406',
  resultMessage: '\u539f\u56e0',
  failed: '\u5931\u8d25',
  noChange: '\u65e0'
}

const appTypeMap = {
  chat: '\u5bf9\u8bdd\u578b',
  agent: 'Agent',
  workflow: 'Workflow'
}

const resultActionMap = {
  inserted: '\u65b0\u589e',
  updated: '\u66f4\u65b0',
  failed: '\u5931\u8d25',
  missing: '\u672a\u627e\u5230'
}

const keyActionMap = {
  reused: '\u590d\u7528\u5e73\u53f0Key',
  'preserved-local': '\u4fdd\u7559\u672c\u5730Key',
  required: '\u9700\u5148\u521b\u5efa\u5e73\u53f0Key'
}

const model = ref(false)
const loading = ref(false)
const syncing = ref(false)
const creatingKeyId = ref('')
const keyword = ref('')
const apps = ref([])
const selectedIds = ref([])
const tableRef = ref(null)
const lastSyncResult = ref(null)

const summaryText = computed(() => {
  if (!apps.value.length) {
    return labels.defaultSummary
  }

  const existsCount = apps.value.filter((x) => x.exists).length
  const newCount = apps.value.length - existsCount
  const platformKeyReadyCount = apps.value.filter((x) => x.hasPlatformKey).length
  const platformKeyUnavailableCount = apps.value.filter(
    (x) => !x.hasPlatformKey && !x.platformKeyReadFailed && (x.platformKeyTotalCount || 0) > 0
  ).length
  const platformKeyReadFailedCount = apps.value.filter((x) => x.platformKeyReadFailed).length

  let suffix = `${labels.newItem} ${newCount} / ${labels.existing} ${existsCount} / ${labels.platformKeyReady} ${platformKeyReadyCount}`
  if (platformKeyUnavailableCount > 0) {
    suffix += ` / ${labels.platformKeyUnavailable} ${platformKeyUnavailableCount}`
  }
  if (platformKeyReadFailedCount > 0) {
    suffix += ` / ${labels.platformKeyReadFailed} ${platformKeyReadFailedCount}`
  }

  return `${labels.defaultSummary} ${suffix}`
})

const selectedSummary = computed(
  () => `${labels.selectedPrefix} ${selectedIds.value.length} ${labels.selectedSuffix}`
)

const resultAlertType = computed(() => getSyncResultAlertType(lastSyncResult.value))
const resultSummaryText = computed(() => getSyncResultSummaryText(lastSyncResult.value))
const tableHeight = computed(() => (lastSyncResult.value ? 300 : 470))

const getPlatformKeyText = (row) => {
  if (row?.platformKeyReadFailed) {
    return labels.platformKeyReadFailed
  }

  if (row?.hasPlatformKey) {
    return row?.platformKeyCount > 1 ? `${labels.platformKeyReady}(${row.platformKeyCount})` : labels.platformKeyReady
  }

  if ((row?.platformKeyTotalCount || 0) > 0) {
    return row?.platformKeyTotalCount > 1
      ? `${labels.platformKeyUnavailable}(${row.platformKeyTotalCount})`
      : labels.platformKeyUnavailable
  }

  return labels.platformKeyMissing
}

const getPlatformKeyTagType = (row) => {
  if (row?.platformKeyReadFailed) {
    return 'danger'
  }

  return row?.hasPlatformKey ? 'success' : 'warning'
}

const getResultActionTagType = (action) => {
  if (action === 'inserted') {
    return 'success'
  }

  if (action === 'updated') {
    return 'warning'
  }

  if (action === 'missing') {
    return 'warning'
  }

  return 'danger'
}

const getKeyActionTagType = (keyAction) => {
  if (keyAction === 'preserved-local') {
    return 'warning'
  }

  if (keyAction === 'reused') {
    return 'info'
  }

  if (keyAction === 'required') {
    return 'danger'
  }

  return ''
}

const shouldShowCreateKeyButton = (row) =>
  !row?.platformKeyReadFailed && !row?.hasPlatformKey

const getCreateKeyButtonText = (row) =>
  (row?.platformKeyTotalCount || 0) > 0 ? labels.recreatePlatformKey : labels.createPlatformKey

const handleSelectionChange = (rows) => {
  selectedIds.value = rows.map((x) => x.platformAppId)
}

const applySelectionByIds = async (ids) => {
  await nextTick()
  if (!tableRef.value) {
    return
  }

  const selectionSet = new Set(ids)
  const matchedIds = []
  tableRef.value.clearSelection()
  apps.value.forEach((row) => {
    if (selectionSet.has(row.platformAppId)) {
      tableRef.value.toggleRowSelection(row, true)
      matchedIds.push(row.platformAppId)
    }
  })
  selectedIds.value = matchedIds
}

const selectAllCurrentRows = async () => {
  await nextTick()
  if (!tableRef.value) {
    return
  }

  tableRef.value.clearSelection()
  apps.value.forEach((row) => {
    tableRef.value.toggleRowSelection(row, true)
  })
}

const loadApps = async (options = {}) => {
  const { autoSelectAll = true, selectionIds = [] } = options
  loading.value = true
  try {
    const url = `api/Sys_AIApp/GetPlatformApps?keyword=${encodeURIComponent(keyword.value || '')}`
    const result = await proxy.http.get(url, {}, true)
    if (!result.status) {
      proxy.$Message.error(result.message)
      return
    }

    apps.value = result.data?.items || []
    if (selectionIds.length) {
      await applySelectionByIds(selectionIds)
      return
    }

    selectedIds.value = []
    tableRef.value?.clearSelection()
    if (autoSelectAll) {
      await selectAllCurrentRows()
    }
  } finally {
    loading.value = false
  }
}

const resetAndReload = async () => {
  keyword.value = ''
  lastSyncResult.value = null
  await loadApps()
}

const createPlatformKey = async (row) => {
  if (!row?.platformAppId) {
    return
  }

  try {
    await proxy.$confirm(
      `${row.appName || row.platformAppId}${labels.createKeyConfirmText}`,
      labels.createKeyConfirmTitle,
      {
        confirmButtonText: labels.createKeyConfirmButton,
        cancelButtonText: labels.cancel,
        type: 'warning'
      }
    )
  } catch {
    return
  }

  const currentSelectionIds = selectedIds.value.slice()
  creatingKeyId.value = row.platformAppId
  try {
    const result = await proxy.http.post(
      'api/Sys_AIApp/CreatePlatformAppKey',
      { platformAppId: row.platformAppId },
      true
    )
    if (!result.status) {
      proxy.$Message.error(result.message)
      return
    }

    proxy.$Message.success(result.message)
    await loadApps({
      autoSelectAll: false,
      selectionIds: currentSelectionIds
    })
  } finally {
    creatingKeyId.value = ''
  }
}

const syncSelected = async () => {
  if (!selectedIds.value.length) {
    proxy.$Message.error(labels.selectWarning)
    return
  }

  syncing.value = true
  try {
    lastSyncResult.value = null
    const result = await proxy.http.post('api/Sys_AIApp/SyncFromPlatform', selectedIds.value, true)
    if (!result.status) {
      proxy.$Message.error(result.message)
      return
    }

    const syncResult = buildSyncResultState(result.data)
    lastSyncResult.value = syncResult

    if (syncResult.failedCount <= 0) {
      proxy.$Message.success(result.message)
      model.value = false
      emit('parentCall', ($parent) => {
        $parent.refresh()
      })
      return
    }

    if (syncResult.successCount > 0) {
      proxy.$Message.info(result.message)
    } else {
      proxy.$Message.error(result.message)
    }

    await loadApps({
      autoSelectAll: false,
      selectionIds: getRetrySelectionIds(syncResult)
    })

    emit('parentCall', ($parent) => {
      $parent.refresh()
    })
  } finally {
    syncing.value = false
  }
}

const open = async () => {
  model.value = true
  keyword.value = ''
  lastSyncResult.value = null
  await loadApps()
}

defineExpose({ open })
</script>

<style lang="less" scoped>
.sync-wrap {
  padding: 16px;
}

.sync-toolbar {
  display: flex;
  gap: 12px;
  margin-bottom: 12px;
}

.result-wrap {
  margin-top: 12px;
}

.result-stats {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: 10px;
  margin-top: 12px;
}

.result-stat {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 10px 12px;
  background: #f5f7fa;
  border: 1px solid #ebeef5;
  border-radius: 4px;
}

.result-stat__label {
  font-size: 12px;
  color: #909399;
}

.result-details {
  margin-top: 12px;
}

.result-title {
  margin-bottom: 8px;
  font-weight: 600;
  color: #303133;
}

.sync-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}

@media (max-width: 1400px) {
  .result-stats {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
}
</style>
