const defaultResult = {
  totalSelected: 0,
  matchedCount: 0,
  successCount: 0,
  failedCount: 0,
  inserted: 0,
  updated: 0,
  createdKeys: 0,
  details: [],
  failedItems: []
}

function toNumber(value, fallback = 0) {
  return Number.isFinite(value) ? value : fallback
}

function normalizeDetail(item) {
  return {
    platformAppId: item?.platformAppId || '',
    appName: item?.appName || '',
    success: item?.success === true,
    action: item?.action || '',
    message: item?.message || '',
    keyAction: item?.keyAction || ''
  }
}

export function buildSyncResultState(data) {
  const details = Array.isArray(data?.details) ? data.details.map(normalizeDetail) : []
  const failedItems = details.filter((item) => !item.success)
  const successCount = details.length
    ? details.filter((item) => item.success).length
    : toNumber(data?.successCount, 0)
  const failedCount = details.length
    ? details.filter((item) => !item.success).length
    : toNumber(data?.failedCount, 0)

  return {
    totalSelected: toNumber(data?.totalSelected, details.length),
    matchedCount: toNumber(data?.matchedCount, details.length),
    successCount,
    failedCount,
    inserted: toNumber(data?.inserted, 0),
    updated: toNumber(data?.updated, 0),
    createdKeys: toNumber(data?.createdKeys, 0),
    details,
    failedItems
  }
}

export function getSyncResultAlertType(result) {
  const state = result || defaultResult
  if (state.failedCount <= 0) {
    return 'success'
  }

  if (state.successCount > 0) {
    return 'warning'
  }

  return 'error'
}

export function getSyncResultSummaryText(result) {
  const state = result || defaultResult
  return `\u672c\u6b21\u540c\u6b65\uff1a\u6210\u529f ${state.successCount} \uff0c\u5931\u8d25 ${state.failedCount} \uff0c\u65b0\u589e ${state.inserted} \uff0c\u66f4\u65b0 ${state.updated}`
}

export function getRetrySelectionIds(result) {
  const state = result || defaultResult
  return state.failedItems
    .map((item) => item.platformAppId)
    .filter((id) => typeof id === 'string' && id.trim().length > 0)
}
