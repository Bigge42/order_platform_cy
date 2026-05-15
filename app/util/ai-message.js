import MarkdownIt from 'markdown-it'

const markdown = new MarkdownIt({
  html: false,
  breaks: true,
  linkify: true,
  typographer: false
})

const defaultLinkOpen =
  markdown.renderer.rules.link_open ||
  function linkOpen(tokens, idx, options, env, self) {
    return self.renderToken(tokens, idx, options)
  }

function resolveTokenUrlAttr(tokens, idx, attrName, env) {
  const attrIndex = tokens[idx].attrIndex(attrName)
  if (attrIndex < 0) {
    return ''
  }

  const originalUrl = tokens[idx].attrs[attrIndex][1] || ''
  const resolvedUrl =
    env && typeof env.resolveAssetUrl === 'function' ? env.resolveAssetUrl(originalUrl) || originalUrl : originalUrl

  tokens[idx].attrs[attrIndex][1] = resolvedUrl
  return resolvedUrl
}

markdown.renderer.rules.link_open = function linkOpenWithTarget(tokens, idx, options, env, self) {
  resolveTokenUrlAttr(tokens, idx, 'href', env)

  const targetIndex = tokens[idx].attrIndex('target')
  if (targetIndex < 0) {
    tokens[idx].attrPush(['target', '_blank'])
  } else {
    tokens[idx].attrs[targetIndex][1] = '_blank'
  }

  const relIndex = tokens[idx].attrIndex('rel')
  if (relIndex < 0) {
    tokens[idx].attrPush(['rel', 'noopener noreferrer'])
  } else {
    tokens[idx].attrs[relIndex][1] = 'noopener noreferrer'
  }

  return defaultLinkOpen(tokens, idx, options, env, self)
}

markdown.renderer.rules.image = function imageWithResolvedUrl(tokens, idx, options, env, self) {
  const resolvedSrc = resolveTokenUrlAttr(tokens, idx, 'src', env)
  const token = tokens[idx]
  const altText = markdown.utils.escapeHtml(self.renderInlineAsText(token.children, options, env))
  const attrs = self.renderAttrs(token)
  const imageHtml = `<img${attrs} alt="${altText}${options.xhtmlOut ? '" />' : '">'}` 

  if (!env || !env.includeImageDebugUrl || !resolvedSrc) {
    return imageHtml
  }

  const escapedUrl = markdown.utils.escapeHtml(resolvedSrc)
  return `${imageHtml}<br><span class="ai-image-url">${escapedUrl}</span>`
}

const chartFenceLanguages = new Set(['echarts', 'chart'])

function isObject(value) {
  return Object.prototype.toString.call(value) === '[object Object]'
}

function tryParseJson(text) {
  if (!text || typeof text !== 'string') {
    return null
  }

  try {
    return JSON.parse(text)
  } catch (error) {
    return null
  }
}

function isEchartsOption(value) {
  if (!isObject(value)) {
    return false
  }

  return !!(
    value.series ||
    value.xAxis ||
    value.yAxis ||
    value.radar ||
    value.angleAxis ||
    value.geo ||
    value.calendar ||
    value.parallel
  )
}

function renderMarkdown(text, env = {}) {
  const content = (text || '').trim()
  if (!content) {
    return ''
  }

  return markdown.render(content, env)
}

function extractMarkdownImages(text = '', env = {}) {
  const images = []
  const source = text || ''
  const imagePattern = /!\[([^\]]*)\]\(([^)\s]+)(?:\s+"[^"]*")?\)/g

  const contentWithoutImages = source.replace(imagePattern, (fullMatch, altText, rawUrl) => {
    const originalUrl = rawUrl || ''
    const previewUrl =
      env && typeof env.resolveAssetUrl === 'function' ? env.resolveAssetUrl(originalUrl) || originalUrl : originalUrl

    images.push({
      id: `md-image-${images.length}-${hashText(`${altText || ''}|${originalUrl}`)}`,
      alt: altText || '',
      originalUrl,
      previewUrl,
      url: previewUrl,
      localPreviewPath: '',
      objectUrl: '',
      type: 'image'
    })

    return ''
  })

  return {
    contentWithoutImages,
    images
  }
}

function createMarkdownPayload(text, env = {}) {
  const { contentWithoutImages, images } = extractMarkdownImages(text, env)
  const html = renderMarkdown(contentWithoutImages, env)

  return {
    raw: text,
    html,
    images
  }
}

function createBlock(type, payload = {}) {
  return {
    type,
    ...payload
  }
}

function hashText(text = '') {
  let hash = 0
  for (let index = 0; index < text.length; index += 1) {
    hash = (hash << 5) - hash + text.charCodeAt(index)
    hash |= 0
  }
  return Math.abs(hash).toString(16)
}

function withStableBlockIds(blocks = []) {
  return blocks.map((block, index) => {
    let seed = `${block.type || 'block'}|${index}`

    if (block.type === 'think') {
      seed = [block.type, index, block.title || 'think'].join('|')
    } else if (block.type === 'chart') {
      seed = [
        block.type,
        index,
        block.title || '',
        block.description || '',
        JSON.stringify(block.option || {})
      ]
        .filter(Boolean)
        .join('|')
    }

    return {
      ...block,
      id: `${block.type || 'block'}-${index}-${hashText(seed)}`
    }
  })
}

function parseChartBlock(language, code) {
  const lang = (language || '').trim().toLowerCase()
  const payload = tryParseJson((code || '').trim())
  if (!payload) {
    return null
  }

  const option = isObject(payload.option)
    ? payload.option
    : isEchartsOption(payload)
      ? payload
      : null

  if (!option) {
    return null
  }

  if (!chartFenceLanguages.has(lang) && lang !== 'json') {
    return null
  }

  const title = isObject(payload.title)
    ? payload.title.text || ''
    : payload.title || option.title?.text || ''

  return createBlock('chart', {
    title: title || '',
    description: payload.description || payload.summary || '',
    option
  })
}

function pushMarkdownBlock(blocks, text, env = {}) {
  const payload = createMarkdownPayload(text, env)
  if (!payload.html && !payload.images.length) {
    return
  }

  blocks.push(
    createBlock('markdown', {
      ...payload
    })
  )
}

function splitMarkdownAndCharts(text, env = {}) {
  const blocks = []
  const source = text || ''
  const fencePattern = /```([a-zA-Z0-9_-]+)?\s*\r?\n([\s\S]*?)```/g
  let cursor = 0
  let match = null

  while ((match = fencePattern.exec(source)) !== null) {
    const [fullMatch, language, code] = match
    const before = source.slice(cursor, match.index)
    pushMarkdownBlock(blocks, before, env)

    const chartBlock = parseChartBlock(language, code)
    if (chartBlock) {
      blocks.push(chartBlock)
    } else {
      pushMarkdownBlock(blocks, fullMatch, env)
    }

    cursor = match.index + fullMatch.length
  }

  pushMarkdownBlock(blocks, source.slice(cursor), env)
  return blocks
}

function normalizeAgentThoughts(agentThoughts = [], env = {}) {
  if (!Array.isArray(agentThoughts)) {
    return []
  }

  return agentThoughts
    .map((item, index) => {
      const lines = [
        item?.thought,
        item?.message,
        item?.observation,
        item?.tool_input,
        item?.tool_output,
        item?.answer
      ].filter(value => typeof value === 'string' && value.trim())

      if (!lines.length) {
        return null
      }

      const title =
        item?.tool ||
        item?.tool_name ||
        item?.node_name ||
        item?.label ||
        `思考 ${index + 1}`

      return createBlock('think', {
        title,
        pending: false,
        ...createMarkdownPayload(lines.join('\n\n'), env)
      })
    })
    .filter(Boolean)
}

function splitThinkBlocks(content, env = {}) {
  const blocks = []
  const source = content || ''
  const thinkPattern = /<think>([\s\S]*?)<\/think>/gi
  let cursor = 0
  let match = null

  while ((match = thinkPattern.exec(source)) !== null) {
    const before = source.slice(cursor, match.index)
    blocks.push(...splitMarkdownAndCharts(before, env))

    const thinkText = (match[1] || '').trim()
    if (thinkText) {
      blocks.push(
        createBlock('think', {
          title: '思考过程',
          pending: false,
          ...createMarkdownPayload(thinkText, env)
        })
      )
    }

    cursor = match.index + match[0].length
  }

  blocks.push(...splitMarkdownAndCharts(source.slice(cursor), env))
  return blocks
}

function splitThinkBlocksStreaming(content, env = {}) {
  const blocks = []
  const source = String(content || '')
  const lowerSource = source.toLowerCase()
  const openTag = '<think>'
  const closeTag = '</think>'
  let cursor = 0
  let mode = 'markdown'
  let buffer = ''

  const flushBuffer = (pendingThink = false) => {
    if (!buffer) {
      return
    }

    if (mode === 'think') {
      const thinkText = buffer.trim()
      if (thinkText) {
        blocks.push(
          createBlock('think', {
            title: '思考过程',
            pending: pendingThink,
            ...createMarkdownPayload(thinkText, env)
          })
        )
      }
    } else {
      blocks.push(...splitMarkdownAndCharts(buffer, env))
    }

    buffer = ''
  }

  while (cursor < source.length) {
    const remainingLower = lowerSource.slice(cursor)

    if (remainingLower.startsWith(openTag)) {
      flushBuffer(false)
      mode = 'think'
      cursor += openTag.length
      continue
    }

    if (remainingLower.startsWith(closeTag)) {
      flushBuffer(false)
      mode = 'markdown'
      cursor += closeTag.length
      continue
    }

    if (openTag.startsWith(remainingLower) || closeTag.startsWith(remainingLower)) {
      break
    }

    buffer += source[cursor]
    cursor += 1
  }

  flushBuffer(mode === 'think')
  return blocks
}

export function buildAssistantBlocks(content = '', agentThoughts = [], renderEnv = {}) {
  const blocks = normalizeAgentThoughts(agentThoughts, renderEnv)
  const contentBlocks = splitThinkBlocksStreaming(content, renderEnv)

  if (!blocks.length && !contentBlocks.length && content) {
    pushMarkdownBlock(contentBlocks, content, renderEnv)
  }

  return withStableBlockIds([...blocks, ...contentBlocks])
}

export function extractCopyableAssistantText(content = '') {
  return String(content || '')
    .replace(/<think>[\s\S]*?<\/think>/gi, '')
    .replace(/\r\n/g, '\n')
    .replace(/\n{3,}/g, '\n\n')
    .trim()
}

export function normalizeRetrieverResources(resources = []) {
  if (!Array.isArray(resources)) {
    return []
  }

  return resources
    .map((item, index) => ({
      id: item?.segment_id || item?.document_id || item?.dataset_id || `resource-${index}`,
      position: item?.position || index + 1,
      datasetId: item?.dataset_id || '',
      datasetName: item?.dataset_name || '',
      documentId: item?.document_id || '',
      documentName: item?.document_name || '引用文档',
      segmentId: item?.segment_id || '',
      score: typeof item?.score === 'number' ? item.score : Number(item?.score || 0),
      content: item?.content || ''
    }))
    .filter(item => item.documentId || item.content || item.documentName)
}

function extractFileName(url = '') {
  const value = (url || '').split('?')[0]
  const index = value.lastIndexOf('/')
  return index >= 0 ? value.slice(index + 1) : value
}

function inferFileType(type = '', name = '', url = '') {
  const explicitType = (type || '').toLowerCase()
  if (explicitType) {
    return explicitType
  }

  const candidate = `${name || ''} ${url || ''}`.toLowerCase()
  if (/\.(png|jpe?g|gif|bmp|webp|svg)/.test(candidate)) {
    return 'image'
  }
  if (/\.(mp3|wav|m4a|aac|ogg)/.test(candidate)) {
    return 'audio'
  }
  if (/\.(mp4|mov|avi|mkv|webm)/.test(candidate)) {
    return 'video'
  }
  return 'document'
}

export function normalizeMessageFiles(files = []) {
  if (!Array.isArray(files)) {
    return []
  }

  return files
    .map((item, index) => {
      const url = item?.url || item?.preview_url || item?.source_url || item?.original_url || ''
      const name = item?.name || item?.file_name || extractFileName(url) || `附件${index + 1}`
      return {
        id: item?.id || item?.file_id || '',
        type: inferFileType(item?.type, name, url),
        name,
        url,
        belongsTo: item?.belongs_to || ''
      }
    })
    .filter(item => item.id || item.url || item.name)
}

export function getMessageFileRenderSrc(file = {}) {
  if (!file || typeof file !== 'object') {
    return ''
  }

  return file.localPreviewPath || file.localPath || file.previewUrl || file.url || ''
}

export function getMessageFileImageDisplaySrc(file = {}) {
  if (!file || typeof file !== 'object') {
    return ''
  }

  if (file.localPreviewPath || file.localPath) {
    return file.localPreviewPath || file.localPath || ''
  }

  const remoteUrl = file.previewUrl || file.url || ''
  if (/\/api\/AI\/PreviewFile\?/i.test(remoteUrl)) {
    return ''
  }

  return remoteUrl
}

export function getMessageFileDebugUrl(file = {}, fallbackUrl = '') {
  if (!file || typeof file !== 'object') {
    return fallbackUrl || ''
  }

  return file.previewRequestUrl || file.previewUrl || file.url || fallbackUrl || ''
}

function isDifyPreviewAssetPath(url = '') {
  return /\/files\/[^/?#]+\/file-preview(?:[/?#]|$)/i.test(String(url || '').trim())
}

export function buildPreviewAssetUrl(resolveUrl, appId, assetUrl) {
  if (!appId || !assetUrl || typeof resolveUrl !== 'function') {
    return ''
  }

  return resolveUrl(
    `api/AI/PreviewAsset?appId=${encodeURIComponent(appId)}&assetUrl=${encodeURIComponent(assetUrl)}`
  )
}

export function resolveAiMessageAssetUrl(url = '', resolveUrl, appId) {
  const value = String(url || '').trim()
  if (!value) {
    return ''
  }

  if (isDifyPreviewAssetPath(value) && typeof resolveUrl === 'function' && appId) {
    return buildPreviewAssetUrl(resolveUrl, appId, value)
  }

  if (value.startsWith('/') && typeof resolveUrl === 'function') {
    return resolveUrl(value)
  }

  return value
}

export function isSyntheticMarkdownImageId(value = '') {
  return /^md-image-\d+-/i.test(String(value || '').trim())
}

export function normalizeMessageFeedback(feedback) {
  const rating =
    typeof feedback === 'string'
      ? feedback
      : feedback && typeof feedback === 'object'
        ? feedback.rating
        : null

  const normalized = String(rating || '').trim().toLowerCase()
  if (normalized === 'like' || normalized === 'dislike') {
    return normalized
  }

  return null
}

export function resolveNextMessageFeedback(currentFeedback, nextFeedback) {
  const current = normalizeMessageFeedback(currentFeedback)
  const next = normalizeMessageFeedback(nextFeedback)
  if (!next) {
    return null
  }

  return current === next ? null : next
}

export function shouldCollectFeedbackContent(currentFeedback, nextFeedback) {
  return resolveNextMessageFeedback(currentFeedback, nextFeedback) === 'dislike'
}

export function findAssistantRegenerateSource(messages = [], assistantLocalId = '') {
  if (!Array.isArray(messages) || !assistantLocalId) {
    return null
  }

  const assistantIndex = messages.findIndex(
    item => item?.role === 'assistant' && item?.localId === assistantLocalId
  )

  if (assistantIndex <= 0) {
    return null
  }

  for (let index = assistantIndex - 1; index >= 0; index -= 1) {
    const message = messages[index]
    if (message?.role !== 'user') {
      continue
    }

    const files = Array.isArray(message.files) ? message.files : []
    const query = String(message.requestQuery || message.query || message.content || '').trim()

    if (query || files.length) {
      return {
        localId: message.localId || '',
        query,
        displayQuery: typeof message.content === 'string' ? message.content : '',
        files
      }
    }
  }

  return null
}

export function buildUsageSummary(usage) {
  if (!usage || typeof usage !== 'object') {
    return null
  }

  return {
    promptTokens: Number(usage.prompt_tokens || 0),
    completionTokens: Number(usage.completion_tokens || 0),
    totalTokens: Number(usage.total_tokens || 0),
    totalPrice: usage.total_price || '0',
    currency: usage.currency || '',
    latency: Number(usage.latency || 0),
    timeToFirstToken: Number(usage.time_to_first_token || 0),
    timeToGenerate: Number(usage.time_to_generate || 0)
  }
}

export function formatDuration(seconds) {
  const value = Number(seconds || 0)
  if (!value) {
    return '0s'
  }
  if (value < 1) {
    return `${Math.round(value * 1000)}ms`
  }
  return `${value.toFixed(value >= 10 ? 1 : 2)}s`
}

export function formatScore(score) {
  const value = Number(score || 0)
  if (!value) {
    return ''
  }
  return value > 1 ? value.toFixed(2) : `${(value * 100).toFixed(1)}%`
}

export function buildPreviewFileUrl(resolveUrl, appId, fileId, asAttachment = false) {
  if (!appId || !fileId || typeof resolveUrl !== 'function') {
    return ''
  }

  const query = [
    `appId=${encodeURIComponent(appId)}`,
    `fileId=${encodeURIComponent(fileId)}`
  ]
  if (asAttachment) {
    query.push('asAttachment=true')
  }

  return resolveUrl(`api/AI/PreviewFile?${query.join('&')}`)
}
