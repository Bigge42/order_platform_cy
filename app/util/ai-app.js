export const DEFAULT_AI_APP_ICON = '/static/logo.png'

const IMAGE_TAG_SRC_PATTERN = /<img[^>]+src=(['"]?)([^'">\s]+)\1/i
const RELATIVE_IMAGE_PATTERN =
  /^(?:\.\/)?[^?#]+\.(?:png|jpe?g|gif|webp|svg|bmp|ico)(?:[?#].*)?$/i

function pickTextValue(...values) {
  for (const value of values) {
    const text = String(value || '').trim()
    if (text) {
      return text
    }
  }

  return ''
}

function pickNumberValue(...values) {
  for (const value of values) {
    if (value === null || value === undefined || value === '') {
      continue
    }

    const number = Number(value)
    if (!Number.isNaN(number)) {
      return number
    }
  }

  return 0
}

function normalizeBaseUrl(baseUrl = '') {
  const value = String(baseUrl || '').trim()
  if (!value || value === '/') {
    return '/'
  }

  return value.endsWith('/') ? value : `${value}/`
}

export function normalizeAiApp(item = {}, index = 0) {
  return {
    id: pickTextValue(item.id, item.Id, index),
    appName: pickTextValue(item.appName, item.AppName),
    appType: pickTextValue(item.appType, item.AppType, 'AI'),
    description: pickTextValue(item.description, item.Description),
    icon: pickTextValue(item.icon, item.Icon),
    sortNo: pickNumberValue(item.sortNo, item.SortNo)
  }
}

export function normalizeAiAppList(payload) {
  const rows = Array.isArray(payload) ? payload : Array.isArray(payload?.rows) ? payload.rows : []

  return rows.map((item = {}, index) => normalizeAiApp(item, index))
}

export function getAiAppInitial(name = '') {
  const value = String(name || '').trim()
  return (value || 'AI').substring(0, 1).toUpperCase()
}

export function resolveAiAppIcon(icon = '', fallbackIcon = DEFAULT_AI_APP_ICON) {
  const value = String(icon || '').trim()
  if (!value) {
    return fallbackIcon
  }

  const imageTagMatch = value.match(IMAGE_TAG_SRC_PATTERN)
  if (imageTagMatch?.[2]) {
    return imageTagMatch[2]
  }

  return value
}

export function isAiAppImageSource(icon = '') {
  const value = String(icon || '').trim()
  if (!value) {
    return false
  }

  const lowerValue = value.toLowerCase()
  if (
    lowerValue.startsWith('http://') ||
    lowerValue.startsWith('https://') ||
    lowerValue.startsWith('//') ||
    lowerValue.startsWith('/static') ||
    lowerValue.startsWith('static/') ||
    lowerValue.startsWith('data:') ||
    lowerValue.startsWith('blob:')
  ) {
    return true
  }

  if (value.startsWith('/')) {
    return true
  }

  return RELATIVE_IMAGE_PATTERN.test(value)
}

export function resolveAiAppImageSrc(icon = '', baseUrl = '', fallbackIcon = DEFAULT_AI_APP_ICON) {
  const value = resolveAiAppIcon(icon, fallbackIcon)
  if (!isAiAppImageSource(value)) {
    return ''
  }

  if (
    value.startsWith('http://') ||
    value.startsWith('https://') ||
    value.startsWith('//') ||
    value.startsWith('/static') ||
    value.startsWith('data:') ||
    value.startsWith('blob:')
  ) {
    return value
  }

  if (value.startsWith('static/')) {
    return `/${value}`
  }

  if (value.startsWith('/')) {
    return `${normalizeBaseUrl(baseUrl)}${value.substring(1)}`
  }

  return `${normalizeBaseUrl(baseUrl)}${value.replace(/^\.?\//, '')}`
}

export function resolveAiAppGlyphIcon(icon = '') {
  const value = resolveAiAppIcon(icon, '')
  if (!value || isAiAppImageSource(value)) {
    return ''
  }

  return value
}
