<template>
  <view class="chat-page" @click="closeUsagePanels">
    <view class="top-bar">
      <view class="nav-btn" @click="goBack">
        <u-icon name="arrow-left" size="22" color="#17233d"></u-icon>
      </view>
      <view class="title">{{ conversationName || appName || 'AI助手' }}</view>
      <view class="nav-btn" @click="newChat">
        <u-icon name="plus" size="20" color="#1677ff"></u-icon>
      </view>
    </view>

    <scroll-view class="message-list" scroll-y :scroll-into-view="scrollIntoView">
      <view v-if="loadingMessages" class="state">加载中...</view>
      <view v-else-if="messages.length === 0" class="welcome">
        <view class="welcome-icon">
          <image v-if="appIconSrc" :src="appIconSrc" mode="aspectFill"></image>
          <text v-else>{{ appGlyphIcon || getInitial(displayAppName) }}</text>
        </view>
        <view class="welcome-title">{{ appName || 'AI助手' }}</view>
        <view class="welcome-desc">{{ openingStatement || '输入问题开始对话。' }}</view>
        <view v-if="starterQuestions.length" class="starter-list">
          <view
            v-for="question in starterQuestions"
            :key="question"
            :class="['starter-chip', { disabled: sending }]"
            @click="sendMessage(question)"
          >
            {{ question }}
          </view>
        </view>
      </view>

      <view
        v-for="message in messages"
        :id="`msg-${message.localId}`"
        :key="message.localId"
        :class="['message-row', message.role]"
        @longpress="copyMessageItem(message)"
      >
        <view v-if="message.role === 'assistant'" class="avatar">
          <image v-if="appIconSrc" :src="appIconSrc" mode="aspectFill"></image>
          <text v-else>{{ appGlyphIcon || getInitial(displayAppName) }}</text>
        </view>
        <view class="bubble" @click.stop>
          <view
            v-if="message.role === 'assistant' && message.streaming && !message.content && !message.blocks.length"
            class="loading-bubble"
          >
            <view class="dot"></view>
            <view class="dot"></view>
            <view class="dot"></view>
          </view>

          <template v-else-if="message.role === 'assistant'">
            <ai-message-renderer
              :busy="sending"
              :message="message"
              @preview-resource="openResourcePopup"
              @preview-file="previewAssistantFile"
              @toggle-usage="toggleUsage"
              @feedback-message="submitMessageFeedback"
              @regenerate-message="regenerateAssistantMessage"
              @copy-message="copyAssistantMessage"
            />
          </template>

          <template v-else>
            <view v-if="message.files.length" class="user-file-list">
              <view
                v-for="file in message.files"
                :key="file.localId"
                class="user-file-card"
                @click.stop="previewUserFile(file, message.files)"
              >
                <image
                  v-if="file.type === 'image' && file.localPath"
                  :src="file.localPath"
                  class="user-image"
                  mode="aspectFill"
                ></image>
                <view v-else class="user-file-content">
                  <view class="user-file-badge">{{ formatFileType(file.type) }}</view>
                  <view class="user-file-name">{{ file.name }}</view>
                </view>
              </view>
            </view>
            <view v-if="message.content" class="message-text">{{ message.content }}</view>
          </template>

          <view v-if="message.role === 'assistant' && message.suggestions.length" class="suggestion-list">
            <view
              v-for="question in message.suggestions"
              :key="question"
              :class="['suggestion-chip', { disabled: sending }]"
              @click.stop="sendMessage(question)"
            >
              {{ question }}
            </view>
          </view>
        </view>
      </view>
    </scroll-view>

    <view v-if="pendingFiles.length" class="pending-panel" @click.stop>
      <view class="pending-title">待发送附件</view>
      <view class="pending-list">
        <view v-for="file in pendingFiles" :key="file.localId" class="pending-item">
          <image
            v-if="file.type === 'image' && file.localPath"
            :src="file.localPath"
            class="pending-thumb"
            mode="aspectFill"
            @click.stop="previewPendingFile(file)"
          ></image>
          <view v-else class="pending-type" @click.stop="previewPendingFile(file)">{{ formatFileType(file.type) }}</view>
          <view class="pending-main" @click.stop="previewPendingFile(file)">
            <view class="pending-name">{{ file.name }}</view>
            <view :class="['pending-status', { error: !!file.error }]">
              {{ getPendingStatus(file) }}
            </view>
          </view>
          <view class="pending-remove" @click.stop="removePendingFile(file.localId)">×</view>
        </view>
      </view>
    </view>

    <view class="input-bar" @click.stop>
      <view v-if="allowAttach" class="attach-btn" @click="openAttachActions">
        <u-icon name="plus" size="20" color="#1677ff"></u-icon>
      </view>
      <textarea
        class="prompt-input"
        v-model="inputText"
        :disabled="sending"
        :auto-height="true"
        :maxlength="-1"
        :show-confirm-bar="false"
        confirm-type="send"
        placeholder="输入问题"
        @confirm="handleConfirm"
      ></textarea>
      <view v-if="sending" class="stop-btn" @click="stopMessage">停止</view>
      <view v-else :class="['send-btn', { disabled: !canSend }]" @click="sendMessage()">
        <u-icon name="arrow-upward" size="20" color="#fff"></u-icon>
      </view>
    </view>

    <uni-popup ref="feedbackPopup" type="bottom">
      <view class="feedback-popup">
        <view class="feedback-popup-title">反馈</view>
        <view class="feedback-option-list">
          <view
            v-for="option in feedbackReasonOptions"
            :key="option"
            :class="['feedback-option', { active: feedbackSelectedReason === option }]"
            @click="toggleFeedbackReason(option)"
          >
            {{ option }}
          </view>
        </view>
        <textarea
          v-model="feedbackContent"
          class="feedback-input"
          :maxlength="-1"
          :auto-height="true"
          :show-confirm-bar="false"
          cursor-spacing="24"
          placeholder="我们想知道你对此回答不满意的原因，你认为更好的回答是什么？"
          placeholder-style="color: #9aa6b8;"
        ></textarea>
        <view class="feedback-popup-actions">
          <view class="feedback-btn secondary" @click="closeFeedbackPopup">取消</view>
          <view
            :class="['feedback-btn', 'primary', { disabled: !canSubmitDislikeFeedback || feedbackSubmitting }]"
            @click="submitDislikeFeedbackDetail"
          >
            {{ feedbackSubmitting ? '提交中' : '提交' }}
          </view>
        </view>
      </view>
    </uni-popup>

    <uni-popup ref="resourcePopup" type="bottom">
      <view class="resource-popup">
        <view class="resource-popup-head">
          <view class="resource-popup-title">{{ activeResource.documentName || '引用内容' }}</view>
          <view v-if="activeResource.scoreText" class="resource-popup-score">相关度 {{ activeResource.scoreText }}</view>
        </view>
        <scroll-view scroll-y class="resource-popup-body">
          <view v-if="activeResource.datasetName" class="resource-dataset">
            知识库：{{ activeResource.datasetName }}
          </view>
          <view class="resource-content">{{ activeResource.content || '暂无内容' }}</view>
        </scroll-view>
      </view>
    </uni-popup>
  </view>
</template>

<script setup>
import { computed, getCurrentInstance, nextTick, ref } from 'vue'
import { onLoad, onUnload } from '@dcloudio/uni-app'
import AiMessageRenderer from '@/components/ai/ai-message-renderer.vue'
import {
  buildAssistantBlocks,
  buildPreviewFileUrl,
  buildUsageSummary,
  extractCopyableAssistantText,
  findAssistantRegenerateSource,
  formatScore,
  isSyntheticMarkdownImageId,
  normalizeMessageFeedback,
  normalizeMessageFiles,
  normalizeRetrieverResources,
  shouldCollectFeedbackContent,
  resolveNextMessageFeedback,
  resolveAiMessageAssetUrl
} from '@/util/ai-message.js'
import {
  getAiAppInitial,
  normalizeAiApp,
  resolveAiAppGlyphIcon,
  resolveAiAppImageSrc
} from '@/util/ai-app.js'
import { isStreamSupported, readEventStream } from '@/util/http-stream.js'

const { proxy } = getCurrentInstance()
const appId = ref(0)
const appName = ref('')
const appInfo = ref(normalizeAiApp())
const conversationId = ref('')
const conversationName = ref('')
const inputText = ref('')
const messages = ref([])
const loadingMessages = ref(false)
const sending = ref(false)
const scrollIntoView = ref('')
const openingStatement = ref('')
const starterQuestions = ref([])
const suggestedQuestionsEnabled = ref(false)
const currentTaskId = ref('')
const pendingFiles = ref([])
const fileUploadConfig = ref(null)
const feedbackPopup = ref(null)
const feedbackTargetId = ref('')
const feedbackSelectedReason = ref('')
const feedbackContent = ref('')
const feedbackSubmitting = ref(false)
const resourcePopup = ref(null)
const activeResource = ref({})

let requestController = null
let stoppedManually = false
const feedbackReasonOptions = ['有害/不安全', '虚假信息', '没有帮助', '其他']

const readyPendingFiles = computed(() => {
  return pendingFiles.value.filter(item => item.uploadFileId && !item.uploading && !item.error)
})

const canSend = computed(() => {
  return (
    (!!inputText.value.trim() || readyPendingFiles.value.length > 0) &&
    !sending.value &&
    !pendingFiles.value.some(item => item.uploading) &&
    !!appId.value
  )
})

const allowAttach = computed(() => {
  return uploadCapabilities.value.imageEnabled || uploadCapabilities.value.documentEnabled
})

const displayAppName = computed(() => {
  return appInfo.value.appName || appName.value || 'AI助手'
})

const appIconSrc = computed(() => {
  return resolveAiAppImageSrc(appInfo.value.icon, proxy.http.ipAddress)
})

const appGlyphIcon = computed(() => {
  return resolveAiAppGlyphIcon(appInfo.value.icon)
})

const canSubmitDislikeFeedback = computed(() => {
  return !!feedbackSelectedReason.value || !!feedbackContent.value.trim()
})

const uploadCapabilities = computed(() => {
  const config = fileUploadConfig.value || {}
  const keys = Object.keys(config)
  const hasExplicitConfig = keys.length > 0
  const imageConfig = config.image || {}
  const documentConfig = config.document || config.file || {}
  const imageMethods = Array.isArray(imageConfig.transfer_methods) ? imageConfig.transfer_methods : []
  const documentMethods = Array.isArray(documentConfig.transfer_methods) ? documentConfig.transfer_methods : []

  return {
    imageEnabled: hasExplicitConfig
      ? imageConfig.enabled === true && imageMethods.includes('local_file')
      : true,
    documentEnabled: hasExplicitConfig
      ? documentConfig.enabled === true && documentMethods.includes('local_file')
      : true,
    imageLimit: Number(imageConfig.number_limits || 0),
    documentLimit: Number(documentConfig.number_limits || 0)
  }
})

const goBack = () => {
  uni.navigateBack()
}

const getInitial = (name) => {
  return getAiAppInitial(name)
}

const createLocalId = () => {
  return `${Date.now()}-${Math.random().toString(16).slice(2)}`
}

const scrollToBottom = () => {
  nextTick(() => {
    if (messages.value.length) {
      scrollIntoView.value = `msg-${messages.value[messages.value.length - 1].localId}`
    }
  })
}

const closeUsagePanels = () => {
  messages.value.forEach((item) => {
    item.showUsage = false
  })
}

function getAssistantBlockRenderOptions() {
  return {
    resolveAssetUrl(url) {
      return resolveAiMessageAssetUrl(url, proxy.http.resolveUrl, appId.value)
    }
  }
}

const createMessage = (role, content = '', extra = {}) => {
  return {
    localId: createLocalId(),
    role,
    content,
    messageId: '',
    streaming: false,
    suggestions: [],
    blocks: role === 'assistant' ? buildAssistantBlocks(content, [], getAssistantBlockRenderOptions()) : [],
    retrieverResources: [],
    usage: null,
    messageFiles: [],
    files: [],
    agentThoughts: [],
    feedback: null,
    feedbackLoading: false,
    thinkStartedAt: 0,
    thinkDurationMs: null,
    showUsage: false,
    ...extra
  }
}

const appendMessage = (message) => {
  messages.value.push(message)
  scrollToBottom()
  return messages.value[messages.value.length - 1]
}

const syncAssistantThinkingState = (message) => {
  if (!message || message.role !== 'assistant') {
    return
  }

  const thinkBlocks = Array.isArray(message.blocks)
    ? message.blocks.filter(block => block?.type === 'think')
    : []

  if (!thinkBlocks.length) {
    message.thinkStartedAt = 0
    message.thinkDurationMs = null
    return
  }

  const hasPendingThink = thinkBlocks.some(block => block?.pending)
  if (hasPendingThink) {
    if (!message.thinkStartedAt) {
      message.thinkStartedAt = Date.now()
    }

    if (!message.streaming && message.thinkDurationMs === null) {
      message.thinkDurationMs = Math.max(0, Date.now() - message.thinkStartedAt)
    }
    return
  }

  if (message.thinkDurationMs !== null) {
    return
  }

  if (message.thinkStartedAt) {
    message.thinkDurationMs = Math.max(0, Date.now() - message.thinkStartedAt)
    return
  }

  const fallbackSeconds = Number(message.usage?.timeToFirstToken || 0)
  message.thinkDurationMs = fallbackSeconds > 0 ? Math.round(fallbackSeconds * 1000) : null
}

const rebuildAssistantMessage = (message) => {
  if (!message) {
    return
  }
  message.blocks = buildAssistantBlocks(
    message.content || '',
    message.agentThoughts || [],
    getAssistantBlockRenderOptions()
  )
  syncAssistantThinkingState(message)
  void hydrateAssistantBlockImages(message.blocks)
}

const splitMessageFiles = (files, fallbackOwner = 'assistant') => {
  const normalized = normalizeMessageFiles(files)
  const userFiles = []
  const assistantFiles = []

  normalized.forEach((item) => {
    const owner = item.belongsTo || fallbackOwner
    if (owner === 'user') {
      userFiles.push(item)
      return
    }
    assistantFiles.push(item)
  })

  return {
    userFiles,
    assistantFiles
  }
}

const toChatFile = (file, extra = {}) => {
  return {
    id: extra.id || file.id || file.uploadFileId || '',
    localId: extra.localId || file.localId || file.id || createLocalId(),
    name: file.name || '附件',
    type: file.type || 'document',
    size: file.size || 0,
    localPath: file.localPath || '',
    previewUrl: file.previewUrl || file.url || '',
    url: file.url || '',
    previewRequestUrl: extra.previewRequestUrl || file.previewRequestUrl || getPreviewUrl(file),
    localPreviewPath: file.localPreviewPath || '',
    uploadFileId: file.uploadFileId || file.id || '',
    previewLoading: !!file.previewLoading,
    objectUrl: file.objectUrl || '',
    error: file.error || '',
    uploading: !!file.uploading
  }
}

const applyAssistantMeta = (message, options = {}) => {
  if (!message) {
    return
  }

  if (Array.isArray(options.agentThoughts)) {
    message.agentThoughts = options.agentThoughts
  }

  if (Array.isArray(options.retrieverResources)) {
    message.retrieverResources = normalizeRetrieverResources(options.retrieverResources)
  }

  if (Array.isArray(options.messageFiles)) {
    const { assistantFiles } = splitMessageFiles(options.messageFiles, 'assistant')
    releaseMessageFilePreviews(message.messageFiles || [])
    message.messageFiles = assistantFiles.map(file => toChatFile(file))
    void hydrateAssistantImageFiles(message.messageFiles)
  }

  const usage = buildUsageSummary(options.usage)
  if (usage) {
    message.usage = usage
  }

  rebuildAssistantMessage(message)
}

const normalizeHistory = (rows) => {
  const ordered = [...(rows || [])].sort((left, right) => Number(left.created_at || 0) - Number(right.created_at || 0))
  const result = []

  ordered.forEach((row) => {
    const { userFiles, assistantFiles } = splitMessageFiles(row.message_files, 'assistant')

    if (row.query || userFiles.length) {
      result.push(
        createMessage('user', row.query || '', {
          localId: `${row.id || createLocalId()}-query`,
          requestQuery: row.query || '',
          files: userFiles.map(file => toChatFile(file))
        })
      )
    }

    if (row.answer || assistantFiles.length || (row.retriever_resources || []).length || (row.agent_thoughts || []).length) {
      const assistantMessage = createMessage('assistant', row.answer || '', {
        localId: row.id || createLocalId(),
        messageId: row.id || '',
        feedback: normalizeMessageFeedback(row.feedback)
      })

      applyAssistantMeta(assistantMessage, {
        agentThoughts: row.agent_thoughts || [],
        retrieverResources: row.retriever_resources || [],
        messageFiles: assistantFiles,
        usage: row.metadata?.usage || null
      })

      result.push(assistantMessage)
    }
  })

  releaseAllMessagePreviews()
  messages.value = result
  scrollToBottom()
}

const loadAppInfo = () => {
  if (!appId.value) {
    return
  }

  proxy.http
    .get(
      'api/AI/AppInfo',
      {
        appId: appId.value
      },
      false
    )
    .then((result) => {
      const isSuccess = result?.status === true || result?.status === 0
      if (!isSuccess) {
        return
      }

      const normalized = normalizeAiApp(result.data || result.rows || {})
      appInfo.value = normalized
      if (normalized.appName) {
        appName.value = normalized.appName
      }
    })
}

const loadMessages = () => {
  if (!appId.value || !conversationId.value) {
    return
  }

  loadingMessages.value = true
  proxy.http
    .get(
      'api/AI/Messages',
      {
        appId: appId.value,
        conversationId: conversationId.value,
        limit: 50
      },
      false
    )
    .then((result) => {
      if (!result.status) {
        proxy.$toast(result.message || '加载消息失败')
        return
      }

      normalizeHistory((result.data || {}).data || [])
    })
    .finally(() => {
      loadingMessages.value = false
    })
}

const loadAppParameters = () => {
  if (!appId.value) {
    return
  }

  proxy.http
    .get(
      'api/AI/AppParameters',
      {
        appId: appId.value
      },
      false
    )
    .then((result) => {
      if (!result.status) {
        return
      }

      const data = result.data || {}
      const suggestedConfig = data.suggested_questions_after_answer || {}
      openingStatement.value = data.opening_statement || ''
      starterQuestions.value = Array.isArray(data.suggested_questions) ? data.suggested_questions : []
      suggestedQuestionsEnabled.value = !!suggestedConfig.enabled
      fileUploadConfig.value = data.file_upload || data.fileUpload || { enabled: true }
    })
}

const findMessage = (localId) => {
  return messages.value.find(item => item.localId === localId)
}

const setAssistantSuggestions = (localId, suggestions) => {
  const target = findMessage(localId)
  if (!target) {
    return
  }
  target.suggestions = suggestions
}

const loadSuggestedQuestions = (message) => {
  if (!message || !message.messageId || stoppedManually || !suggestedQuestionsEnabled.value) {
    return Promise.resolve()
  }

  return proxy.http
    .get(
      'api/AI/SuggestedQuestions',
      {
        appId: appId.value,
        messageId: message.messageId
      },
      false
    )
    .then((result) => {
      if (!result.status) {
        return
      }

      const data = result.data || {}
      const questions = Array.isArray(data.data) ? data.data : []
      setAssistantSuggestions(message.localId, questions)
    })
}

const appendAgentThought = (payload, assistantMessage) => {
  if (!assistantMessage || !payload || !String(payload.event || '').startsWith('agent_')) {
    return
  }

  const snapshot = {
    tool: payload.tool || payload.tool_name || payload.node_name || payload.label || '',
    thought: payload.thought || payload.message || '',
    observation: payload.observation || '',
    tool_input: payload.tool_input || '',
    tool_output: payload.tool_output || '',
    answer: payload.answer || ''
  }

  const hasContent = Object.values(snapshot).some(value => typeof value === 'string' && value.trim())
  if (!hasContent) {
    return
  }

  assistantMessage.agentThoughts = [...(assistantMessage.agentThoughts || []), snapshot]
  rebuildAssistantMessage(assistantMessage)
}

const applyStreamPayload = (payload, assistantMessage) => {
  if (!payload || !assistantMessage) {
    return
  }

  if (payload.task_id) {
    currentTaskId.value = payload.task_id
  }
  if (payload.conversation_id) {
    conversationId.value = payload.conversation_id
  }
  if (payload.message_id) {
    assistantMessage.messageId = payload.message_id
  }

  if (payload.event === 'error') {
    throw new Error(payload.message || payload.error || 'AI 响应异常')
  }

  appendAgentThought(payload, assistantMessage)

  if (typeof payload.answer === 'string' && payload.answer) {
    assistantMessage.content += payload.answer
    rebuildAssistantMessage(assistantMessage)
  }

  if (payload.event === 'message_end') {
    applyAssistantMeta(assistantMessage, {
      retrieverResources: payload.metadata?.retriever_resources || [],
      usage: payload.metadata?.usage || null,
      messageFiles: payload.files || [],
      agentThoughts: assistantMessage.agentThoughts || []
    })
  }

  scrollToBottom()
}

const parseJsonResult = async (response) => {
  try {
    return await response.json()
  } catch (error) {
    return null
  }
}

const buildSendFiles = (files = []) => {
  return files
    .filter(item => item.uploadFileId)
    .map(item => ({
      type: item.type || 'document',
      transfer_method: 'local_file',
      upload_file_id: item.uploadFileId
    }))
}

const sendMessageBlocking = (query, assistantMessage, files) => {
  return proxy.http
    .post(
      'api/AI/SendMessage',
      {
        appId: appId.value,
        query,
        conversationId: conversationId.value,
        inputs: {},
        files
      },
      false
    )
    .then((result) => {
      if (!result.status) {
        throw new Error(result.message || '发送失败，请稍后重试。')
      }

      const data = result.data || {}
      if (data.conversation_id) {
        conversationId.value = data.conversation_id
      }
      if (data.message_id) {
        assistantMessage.messageId = data.message_id
      }

      assistantMessage.content = data.answer || 'AI 未返回内容。'
      applyAssistantMeta(assistantMessage, {
        retrieverResources: data.metadata?.retriever_resources || [],
        usage: data.metadata?.usage || null,
        messageFiles: data.files || [],
        agentThoughts: data.agent_thoughts || []
      })
    })
}

const sendMessageStream = async (query, assistantMessage, files) => {
  const response = await fetch(proxy.http.resolveUrl('api/AI/StreamMessage'), {
    method: 'POST',
    headers: proxy.http.buildHeaders({
      'Content-Type': 'application/json',
      Accept: 'text/event-stream'
    }),
    body: JSON.stringify({
      appId: appId.value,
      query,
      conversationId: conversationId.value,
      inputs: {},
      files
    }),
    signal: requestController.signal
  })

  const contentType = response.headers.get('content-type') || ''
  if (contentType.includes('application/json')) {
    const result = await parseJsonResult(response)
    if (result && result.status === false) {
      throw new Error(result.message || '发送失败，请稍后重试。')
    }
    throw new Error('流式请求返回异常。')
  }

  if (!response.ok) {
    throw new Error(`流式请求失败(${response.status})`)
  }

  await readEventStream(response, async(payload) => {
    applyStreamPayload(payload, assistantMessage)
  })
}

const finalizeAssistantMessage = async (assistantMessage) => {
  assistantMessage.streaming = false

  if (!assistantMessage.content && !assistantMessage.messageFiles.length) {
    assistantMessage.content = stoppedManually ? '已停止生成。' : 'AI 未返回内容。'
    rebuildAssistantMessage(assistantMessage)
  }

  await loadSuggestedQuestions(assistantMessage)
  scrollToBottom()
}

const resetRequestState = () => {
  currentTaskId.value = ''
  requestController = null
  sending.value = false
}

const stopMessage = async () => {
  if (!sending.value) {
    return
  }

  stoppedManually = true
  const taskId = currentTaskId.value
  if (taskId) {
    try {
      await proxy.http.post(
        'api/AI/StopMessage',
        {
          appId: appId.value,
          taskId
        },
        false
      )
    } catch (error) {
    }
  }

  if (requestController) {
    requestController.abort()
  }
}

const formatFileType = (type) => {
  switch ((type || '').toLowerCase()) {
    case 'image':
      return '图片'
    case 'audio':
      return '音频'
    case 'video':
      return '视频'
    default:
      return '文档'
  }
}

const getPendingStatus = (file) => {
  if (file.error) {
    return file.error
  }
  if (file.uploading) {
    return '上传中...'
  }
  return '已就绪'
}

const getFileNameFromPath = (path = '') => {
  const value = (path || '').split('?')[0]
  const index = Math.max(value.lastIndexOf('/'), value.lastIndexOf('\\'))
  return index >= 0 ? value.slice(index + 1) : value
}

const inferUploadFileType = (value = '', fallback = 'document') => {
  const source = (value || '').toLowerCase()
  if (source.includes('image') || /\.(png|jpe?g|gif|bmp|webp|svg)$/.test(source)) {
    return 'image'
  }
  if (source.includes('audio') || /\.(mp3|wav|m4a|aac|ogg)$/.test(source)) {
    return 'audio'
  }
  if (source.includes('video') || /\.(mp4|mov|avi|mkv|webm)$/.test(source)) {
    return 'video'
  }
  return fallback
}

const getPreviewUrl = (file) => {
  if (!file) {
    return ''
  }

  if (file.uploadFileId) {
    return buildPreviewFileUrl(proxy.http.resolveUrl, appId.value, file.uploadFileId || file.id)
  }

  if (file.id && !isSyntheticMarkdownImageId(file.id)) {
    return buildPreviewFileUrl(proxy.http.resolveUrl, appId.value, file.id)
  }

  return file.previewUrl || file.localPath || file.url || ''
}

const canUseObjectUrlPreview = () => {
  return (
    typeof window !== 'undefined' &&
    typeof fetch === 'function' &&
    typeof URL !== 'undefined' &&
    typeof URL.createObjectURL === 'function'
  )
}

const revokeObjectUrl = (file) => {
  if (
    !file ||
    !file.objectUrl ||
    typeof URL === 'undefined' ||
    typeof URL.revokeObjectURL !== 'function'
  ) {
    return
  }

  URL.revokeObjectURL(file.objectUrl)
  if (file.localPreviewPath === file.objectUrl) {
    file.localPreviewPath = ''
  }
  file.objectUrl = ''
}

const releaseMessageFilePreviews = (files = []) => {
  files.forEach((file) => {
    revokeObjectUrl(file)
  })
}

const releaseAllMessagePreviews = () => {
  messages.value.forEach((message) => {
    releaseMessageFilePreviews(message.files || [])
    releaseMessageFilePreviews(message.messageFiles || [])
    ;(message.blocks || []).forEach((block) => {
      releaseMessageFilePreviews(block.images || [])
    })
  })
}

const fetchProtectedFileAsObjectUrl = async (url) => {
  const response = await fetch(url, {
    method: 'GET',
    headers: proxy.http.buildHeaders(),
    credentials: 'same-origin'
  })

  const contentType = (response.headers.get('content-type') || '').toLowerCase()
  if (contentType.includes('application/json')) {
    const result = await response.json().catch(() => null)
    throw new Error(result?.message || `Preview failed (${response.status})`)
  }

  if (!response.ok) {
    throw new Error(`Preview failed (${response.status})`)
  }

  const blob = await response.blob()
  if (!blob || !blob.size) {
    throw new Error('Preview file is empty')
  }

  return URL.createObjectURL(blob)
}

const previewImageList = (current, files = []) => {
  const urls = files
    .map(item => item.localPreviewPath || item.localPath || item.previewUrl || item.url || getPreviewUrl(item))
    .filter(Boolean)

  if (!urls.length) {
    return
  }

  uni.previewImage({
    current,
    urls
  })
}

const openFilePreview = (url) => {
  if (!url) {
    proxy.$toast('暂无可预览内容')
    return
  }

  uni.navigateTo({
    url: `/components/vol-preview/vol-preview?src=${encodeURIComponent(url)}`
  })
}

const downloadProtectedFile = (file) => {
  return new Promise((resolve, reject) => {
    const url = getPreviewUrl(file)
    if (!url) {
      reject(new Error('暂无可预览内容'))
      return
    }

    if (file.localPreviewPath) {
      resolve(file.localPreviewPath)
      return
    }

    if (canUseObjectUrlPreview()) {
      fetchProtectedFileAsObjectUrl(url)
        .then((objectUrl) => {
          revokeObjectUrl(file)
          file.objectUrl = objectUrl
          file.localPreviewPath = objectUrl
          resolve(objectUrl)
        })
        .catch((error) => {
          reject(error)
        })
      return
    }

    uni.downloadFile({
      url,
      header: proxy.http.buildHeaders(),
      success: (response) => {
        if (response.statusCode >= 400) {
          reject(new Error(`预览失败(${response.statusCode})`))
          return
        }

        const tempPath = response.tempFilePath || response.filePath || response.apFilePath
        if (!tempPath) {
          reject(new Error('预览文件下载失败'))
          return
        }

        file.localPreviewPath = tempPath
        resolve(tempPath)
      },
      fail: (error) => {
        reject(error)
      }
    })
  })
}

const hydrateAssistantImageFile = async (file) => {
  if (!file || String(file.type || '').toLowerCase() !== 'image') {
    return
  }

  if (file.localPreviewPath || file.localPath || file.previewLoading) {
    return
  }

  file.previewLoading = true
  try {
    await downloadProtectedFile(file)
  } catch (error) {
    console.warn('hydrate assistant image failed', error)
  } finally {
    file.previewLoading = false
  }
}

const hydrateAssistantImageFiles = async (files = []) => {
  await Promise.all((files || []).map(file => hydrateAssistantImageFile(file)))
}

const hydrateAssistantBlockImage = async (image) => {
  if (!image || !getPreviewUrl(image) || image.localPreviewPath || image.previewLoading) {
    return
  }

  image.previewLoading = true
  try {
    await downloadProtectedFile(image)
  } catch (error) {
    console.warn('hydrate assistant block image failed', error)
  } finally {
    image.previewLoading = false
  }
}

const hydrateAssistantBlockImages = async (blocks = []) => {
  const images = (blocks || []).flatMap(block => (Array.isArray(block?.images) ? block.images : []))
  await Promise.all(images.map(image => hydrateAssistantBlockImage(image)))
}

const previewPendingFile = async(file) => {
  if (!file) {
    return
  }

  if (file.type === 'image') {
    previewImageList(file.localPath || getPreviewUrl(file), pendingFiles.value.filter(item => item.type === 'image'))
    return
  }

  openFilePreview(file.localPath || getPreviewUrl(file))
}

const previewUserFile = async(file, files = []) => {
  if (!file) {
    return
  }

  if (file.type === 'image') {
    if (file.localPath) {
      previewImageList(file.localPath, files.filter(item => item.type === 'image'))
      return
    }

    try {
      const previewPath = await downloadProtectedFile(file)
      previewImageList(previewPath, [{ ...file, localPath: previewPath }])
    } catch (error) {
      proxy.$toast(error?.message || '图片预览失败')
    }
    return
  }

  try {
    const previewPath = file.localPath || (await downloadProtectedFile(file))
    openFilePreview(previewPath)
  } catch (error) {
    proxy.$toast(error?.message || '文件预览失败')
  }
}

const previewAssistantFile = async(file) => {
  if (!file) {
    return
  }

  try {
    const previewPath = file.localPreviewPath || file.localPath || (await downloadProtectedFile(file))
    if (file.type === 'image') {
      previewImageList(previewPath, [{ ...file, localPath: previewPath }])
      return
    }

    openFilePreview(previewPath)
  } catch (error) {
    proxy.$toast(error?.message || '文件预览失败')
  }
}

const toggleUsage = (localId) => {
  messages.value.forEach((item) => {
    if (item.role !== 'assistant') {
      return
    }
    item.showUsage = item.localId === localId ? !item.showUsage : false
  })
}

const promptDislikeFeedbackContent = () => {
  return new Promise((resolve, reject) => {
    uni.showModal({
      title: '不喜欢',
      editable: true,
      placeholderText: '请输入不喜欢的原因',
      confirmText: '提交',
      cancelText: '取消',
      confirmColor: '#cf1322',
      success: (res) => {
        if (!res.confirm) {
          resolve(null)
          return
        }

        const content = String(res.content || '').trim()
        if (!content) {
          proxy.$toast('请输入反馈内容')
          resolve(null)
          return
        }

        resolve(content)
      },
      fail: (error) => {
        reject(error)
      }
    })
  })
}

const resetFeedbackDraft = () => {
  feedbackTargetId.value = ''
  feedbackSelectedReason.value = ''
  feedbackContent.value = ''
  feedbackSubmitting.value = false
}

const openDislikeFeedbackPopup = (localId) => {
  resetFeedbackDraft()
  feedbackTargetId.value = localId
  feedbackPopup.value?.open()
}

const closeFeedbackPopup = () => {
  feedbackPopup.value?.close()
  resetFeedbackDraft()
}

const toggleFeedbackReason = (option) => {
  feedbackSelectedReason.value = feedbackSelectedReason.value === option ? '' : option
}

const buildDislikeFeedbackContent = () => {
  const parts = []
  if (feedbackSelectedReason.value) {
    parts.push('原因：' + feedbackSelectedReason.value)
  }

  const content = feedbackContent.value.trim()
  if (content) {
    parts.push(content)
  }

  return parts.join('\n')
}

const requestMessageFeedback = async(target, rating, content = '') => {
  target.feedbackLoading = true

  try {
    const payload = {
      appId: appId.value,
      messageId: target.messageId,
      rating
    }

    if (content) {
      payload.content = content
    }

    const result = await proxy.http.post(
      'api/AI/MessageFeedback',
      payload,
      false
    )

    if (!result.status) {
      throw new Error(result.message || '反馈提交失败')
    }

    target.feedback = normalizeMessageFeedback(rating)
  } finally {
    target.feedbackLoading = false
  }
}

const submitDislikeFeedbackDetail = async() => {
  if (!canSubmitDislikeFeedback.value || feedbackSubmitting.value) {
    return
  }

  const target = findMessage(feedbackTargetId.value)
  if (!target || target.role !== 'assistant' || !target.messageId) {
    closeFeedbackPopup()
    return
  }

  feedbackSubmitting.value = true
  try {
    await requestMessageFeedback(target, 'dislike', buildDislikeFeedbackContent())
    closeFeedbackPopup()
  } catch (error) {
    proxy.$toast(error?.message || '反馈提交失败')
    feedbackSubmitting.value = false
  }
}

const submitMessageFeedback = async({ localId, rating }) => {
  const target = findMessage(localId)
  if (!target || target.role !== 'assistant' || !target.messageId || target.feedbackLoading) {
    return
  }

  const nextRating = resolveNextMessageFeedback(target.feedback, rating)
  if (shouldCollectFeedbackContent(target.feedback, rating)) {
    openDislikeFeedbackPopup(localId)
    return
  }

  try {
    await requestMessageFeedback(target, nextRating)
    return
  } catch (error) {
    proxy.$toast(error?.message || '反馈提交失败')
    return
  }

  let content = ''

  if (shouldCollectFeedbackContent(target.feedback, rating)) {
    try {
      const inputContent = await promptDislikeFeedbackContent()
      if (!inputContent) {
        return
      }
      content = inputContent
    } catch (error) {
      proxy.$toast(error?.message || '打开反馈输入失败')
      return
    }
  }

  target.feedbackLoading = true

  try {
    const payload = {
      appId: appId.value,
      messageId: target.messageId,
      rating: nextRating
    }

    if (content) {
      payload.content = content
    }

    const result = await proxy.http.post(
      'api/AI/MessageFeedback',
      payload,
      false
    )

    if (!result.status) {
      throw new Error(result.message || '反馈提交失败')
    }

    target.feedback = normalizeMessageFeedback(nextRating)
  } catch (error) {
    proxy.$toast(error?.message || '反馈提交失败')
  } finally {
    target.feedbackLoading = false
  }
}

const copyAssistantMessage = (localId) => {
  const target = findMessage(localId)
  if (!target) {
    return
  }

  copyMessage(extractCopyableAssistantText(target.content))
}

const regenerateAssistantMessage = (localId) => {
  if (sending.value) {
    return
  }

  const source = findAssistantRegenerateSource(messages.value, localId)
  if (!source?.query) {
    proxy.$toast('\u672a\u627e\u5230\u53ef\u91cd\u65b0\u751f\u6210\u7684\u95ee\u9898')
    return
  }

  void sendMessage(source.query, {
    displayQuery: source.displayQuery,
    files: source.files,
    clearComposer: false,
    consumePendingFiles: false
  })
}

const copyMessageItem = (message) => {
  if (!message) {
    return
  }

  if (message.role === 'assistant') {
    copyMessage(extractCopyableAssistantText(message.content))
    return
  }

  copyMessage(message.content)
}

const openResourcePopup = (resource) => {
  activeResource.value = {
    ...resource,
    scoreText: formatScore(resource.score)
  }
  resourcePopup.value?.open()
}

const handleUploadResponse = (response) => {
  const payload = JSON.parse(response.data || '{}')
  if (response.statusCode >= 400 || payload.status === false) {
    throw new Error(payload.message || `上传失败(${response.statusCode})`)
  }
  return payload.data || {}
}

const uploadSingleFile = (item) => {
  return new Promise((resolve, reject) => {
    uni.uploadFile({
      url: `${proxy.http.resolveUrl('api/AI/UploadFile')}?appId=${encodeURIComponent(appId.value)}`,
      filePath: item.localPath,
      name: 'file',
      header: proxy.http.buildHeaders(),
      success: (response) => {
        try {
          resolve(handleUploadResponse(response))
        } catch (error) {
          reject(error)
        }
      },
      fail: (error) => {
        reject(error)
      }
    })
  })
}

const addPendingUploads = async (files = []) => {
  if (!files.length) {
    return
  }

  const list = files.map(file => ({
    ...file,
    localId: file.localId || createLocalId(),
    uploading: true,
    error: '',
    uploadFileId: '',
    previewUrl: file.previewUrl || ''
  }))

  pendingFiles.value.push(...list)

  for (const item of list) {
    try {
      const result = await uploadSingleFile(item)
      item.uploading = false
      item.uploadFileId = result.id || ''
      item.previewUrl = result.preview_url || result.source_url || result.original_url || item.previewUrl || ''
      item.name = result.name || item.name
      item.type = inferUploadFileType(result.mime_type || result.extension || item.type, item.type)
    } catch (error) {
      item.uploading = false
      item.error = error?.message || '上传失败'
    }
  }
}

const getRemainingUploadCount = () => {
  const configured = Math.max(
    uploadCapabilities.value.imageLimit || 0,
    uploadCapabilities.value.documentLimit || 0,
    Number(fileUploadConfig.value?.number_limits || fileUploadConfig.value?.number_limit || 0)
  )
  const limit = Number.isFinite(configured) && configured > 0 ? configured : 5
  return Math.max(0, limit - pendingFiles.value.length)
}

const chooseImages = () => {
  const count = getRemainingUploadCount()
  if (!count) {
    proxy.$toast('附件数量已达上限')
    return
  }

  uni.chooseImage({
    count,
    success: async(res) => {
      const files = (res.tempFiles || []).map((item, index) => ({
        localId: createLocalId(),
        name: getFileNameFromPath(item.path || res.tempFilePaths?.[index] || ''),
        localPath: item.path || res.tempFilePaths?.[index] || '',
        size: item.size || 0,
        type: 'image'
      }))
      await addPendingUploads(files)
    }
  })
}

const chooseDocuments = () => {
  const count = getRemainingUploadCount()
  if (!count) {
    proxy.$toast('附件数量已达上限')
    return
  }

  uni.chooseFile({
    count,
    success: async(res) => {
      const files = (res.tempFiles || []).map((item) => ({
        localId: createLocalId(),
        name: item.name || getFileNameFromPath(item.path || ''),
        localPath: item.path || '',
        size: item.size || 0,
        type: inferUploadFileType(item.type || item.name || item.path || '', 'document')
      }))
      await addPendingUploads(files)
    }
  })
}

const openAttachActions = () => {
  if (sending.value || !allowAttach.value) {
    return
  }

  const actions = []
  if (uploadCapabilities.value.imageEnabled) {
    actions.push({
      text: '选择图片',
      handler: chooseImages
    })
  }
  if (uploadCapabilities.value.documentEnabled) {
    actions.push({
      text: '选择文件',
      handler: chooseDocuments
    })
  }

  if (!actions.length) {
    proxy.$toast('当前应用未开启附件上传')
    return
  }

  if (actions.length === 1) {
    actions[0].handler()
    return
  }

  uni.showActionSheet({
    itemList: actions.map(item => item.text),
    success: ({ tapIndex }) => {
      actions[tapIndex]?.handler?.()
    }
  })
}

const removePendingFile = (localId) => {
  pendingFiles.value = pendingFiles.value.filter(item => item.localId !== localId)
}

const sendMessage = async (presetQuery = '', options = {}) => {
  const customFiles = Array.isArray(options.files) ? options.files : null
  const readyFiles = (customFiles || readyPendingFiles.value).map(file => ({ ...file }))
  const inputQuery = inputText.value.trim()
  const query = typeof presetQuery === 'string' && presetQuery.trim()
    ? presetQuery.trim()
    : inputQuery || (readyFiles.length ? '请结合附件进行分析' : '')

  const displayQuery = typeof presetQuery === 'string' && presetQuery.trim()
    ? presetQuery.trim()
    : inputQuery
  const resolvedDisplayQuery = typeof options.displayQuery === 'string' ? options.displayQuery : displayQuery
  const clearComposer = options.clearComposer !== false
  const consumePendingFiles = options.consumePendingFiles !== false

  if (!query || sending.value || !appId.value) {
    return
  }

  stoppedManually = false
  if (clearComposer) {
    inputText.value = ''
  }

  const userMessageFiles = readyFiles.map(file => toChatFile(file))
  appendMessage(
    createMessage('user', resolvedDisplayQuery, {
      requestQuery: query,
      files: userMessageFiles
    })
  )

  if (consumePendingFiles) {
    pendingFiles.value = pendingFiles.value.filter(item => item.error)
  }

  if (!conversationName.value) {
    conversationName.value = displayQuery || readyFiles[0]?.name || '新对话'
  }

  const assistantMessage = appendMessage(
    createMessage('assistant', '', {
      streaming: true
    })
  )

  sending.value = true
  requestController = typeof AbortController === 'function' ? new AbortController() : null

  const files = buildSendFiles(readyFiles)

  try {
    if (isStreamSupported() && requestController) {
      await sendMessageStream(query, assistantMessage, files)
    } else {
      await sendMessageBlocking(query, assistantMessage, files)
    }
  } catch (error) {
    if (error && error.name === 'AbortError') {
      if (!assistantMessage.content) {
        assistantMessage.content = '已停止生成。'
        rebuildAssistantMessage(assistantMessage)
      }
    } else {
      assistantMessage.content = error?.message || '发送失败，请稍后重试。'
      rebuildAssistantMessage(assistantMessage)
    }
  } finally {
    await finalizeAssistantMessage(assistantMessage)
    resetRequestState()
  }
}

const handleConfirm = () => {
  sendMessage()
}

const abortActiveStream = () => {
  if (requestController) {
    requestController.abort()
    requestController = null
  }
}

const newChat = () => {
  abortActiveStream()
  resetRequestState()
  stoppedManually = false
  conversationId.value = ''
  conversationName.value = ''
  releaseAllMessagePreviews()
  messages.value = []
  inputText.value = ''
  pendingFiles.value = []
  scrollIntoView.value = ''
  closeUsagePanels()
  resourcePopup.value?.close()
}

const copyMessage = (content) => {
  if (!content) {
    return
  }

  uni.setClipboardData({
    data: content,
    success: () => proxy.$toast('已复制')
  })
}

onLoad((options) => {
  appId.value = Number(options.appId || 0)
  appName.value = decodeURIComponent(options.appName || '')
  conversationId.value = decodeURIComponent(options.conversationId || '')
  conversationName.value = decodeURIComponent(options.conversationName || '')
  loadAppInfo()
  loadAppParameters()
  loadMessages()
})

onUnload(() => {
  abortActiveStream()
  releaseAllMessagePreviews()
})
</script>

<style scoped lang="less">
.chat-page {
  height: 100%;
  min-height: 0;
  background: #f5f7fb;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
  overflow: hidden;

  /* #ifdef H5 */
  height: calc(100vh - var(--window-top, 0px) - var(--window-bottom, 0px));
  /* #endif */
}

.top-bar {
  display: flex;
  align-items: center;
  height: 96rpx;
  padding: 12rpx 20rpx;
  box-sizing: border-box;
  background: #fff;
  border-bottom: 1px solid #edf0f5;
  flex-shrink: 0;
}

.nav-btn {
  width: 72rpx;
  height: 72rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
}

.title {
  flex: 1;
  text-align: center;
  font-size: 32rpx;
  font-weight: 650;
  color: #17233d;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.message-list {
  flex: 1;
  min-height: 0;
  padding: 24rpx 24rpx 12rpx;
  box-sizing: border-box;
}

.state,
.welcome {
  margin-top: 180rpx;
  text-align: center;
}

.state {
  color: #8c96a6;
  font-size: 26rpx;
}

.welcome-icon {
  width: 104rpx;
  height: 104rpx;
  margin: 0 auto;
  border-radius: 8px;
  background: #eaf3ff;
  color: #1677ff;
  font-size: 40rpx;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.welcome-icon image {
  width: 100%;
  height: 100%;
}

.welcome-title {
  margin-top: 22rpx;
  font-size: 34rpx;
  font-weight: 700;
  color: #17233d;
}

.welcome-desc {
  margin: 12rpx auto 0;
  max-width: 580rpx;
  color: #8c96a6;
  font-size: 26rpx;
  line-height: 1.6;
}

.starter-list {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 14rpx;
  margin-top: 28rpx;
  padding: 0 30rpx;
}

.starter-chip,
.suggestion-chip {
  max-width: 100%;
  padding: 12rpx 22rpx;
  border-radius: 999px;
  background: #edf5ff;
  color: #1677ff;
  font-size: 24rpx;
  line-height: 1.4;
  word-break: break-word;
}

.starter-chip.disabled,
.suggestion-chip.disabled {
  opacity: 0.45;
}

.message-row {
  display: flex;
  align-items: flex-start;
  gap: 16rpx;
  margin-bottom: 22rpx;
}

.message-row.user {
  justify-content: flex-end;
}

.avatar {
  width: 64rpx;
  height: 64rpx;
  border-radius: 8px;
  background: #eaf3ff;
  color: #1677ff;
  font-size: 28rpx;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  overflow: hidden;
}

.avatar image {
  width: 100%;
  height: 100%;
}

.bubble {
  max-width: 78%;
  border-radius: 20rpx;
  padding: 20rpx 22rpx;
  background: #fff;
  color: #17233d;
  box-shadow: 0 4rpx 18rpx rgba(30, 48, 78, 0.06);
}

.message-row.user .bubble {
  background: #1677ff;
  color: #fff;
}

.message-text {
  font-size: 28rpx;
  line-height: 1.55;
  white-space: pre-wrap;
  word-break: break-word;
}

.loading-bubble {
  display: flex;
  align-items: center;
  gap: 8rpx;
  min-width: 84rpx;
  height: 44rpx;
}

.user-file-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12rpx;
}

.user-file-list + .message-text {
  margin-top: 16rpx;
}

.user-file-card {
  overflow: hidden;
  border-radius: 16rpx;
  background: rgba(255, 255, 255, 0.16);
  border: 1px solid rgba(255, 255, 255, 0.26);
}

.user-image {
  width: 168rpx;
  height: 168rpx;
  display: block;
}

.user-file-content {
  min-width: 220rpx;
  max-width: 360rpx;
  padding: 16rpx;
}

.user-file-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 72rpx;
  height: 38rpx;
  padding: 0 12rpx;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.18);
  font-size: 20rpx;
}

.user-file-name {
  margin-top: 10rpx;
  font-size: 24rpx;
  line-height: 1.45;
  word-break: break-word;
}

.suggestion-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12rpx;
  margin-top: 18rpx;
}

.dot {
  width: 10rpx;
  height: 10rpx;
  border-radius: 50%;
  background: #8c96a6;
  animation: pulse 1.2s infinite ease-in-out;
}

.dot:nth-child(2) {
  animation-delay: 0.16s;
}

.dot:nth-child(3) {
  animation-delay: 0.32s;
}

.pending-panel {
  padding: 16rpx 20rpx 0;
  background: #fff;
  border-top: 1px solid #edf0f5;
}

.pending-title {
  margin-bottom: 12rpx;
  color: #6b7a90;
  font-size: 22rpx;
}

.pending-list {
  display: flex;
  flex-direction: column;
  gap: 12rpx;
  padding-bottom: 12rpx;
}

.pending-item {
  display: flex;
  align-items: center;
  gap: 14rpx;
  padding: 14rpx 16rpx;
  border-radius: 16rpx;
  background: #f7f9fc;
}

.pending-thumb,
.pending-type {
  width: 72rpx;
  height: 72rpx;
  border-radius: 12rpx;
  flex-shrink: 0;
}

.pending-type {
  display: flex;
  align-items: center;
  justify-content: center;
  background: #eaf3ff;
  color: #1677ff;
  font-size: 22rpx;
}

.pending-main {
  flex: 1;
  min-width: 0;
}

.pending-name {
  color: #17233d;
  font-size: 24rpx;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.pending-status {
  margin-top: 8rpx;
  color: #7a8799;
  font-size: 22rpx;
}

.pending-status.error {
  color: #d03050;
}

.pending-remove {
  width: 44rpx;
  height: 44rpx;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #7a8799;
  font-size: 34rpx;
  flex-shrink: 0;
}

.input-bar {
  display: flex;
  align-items: flex-end;
  gap: 16rpx;
  padding: 18rpx 20rpx calc(18rpx + env(safe-area-inset-bottom));
  background: #fff;
  border-top: 1px solid #edf0f5;
  box-sizing: border-box;
  flex-shrink: 0;
}

.attach-btn {
  width: 76rpx;
  height: 76rpx;
  border-radius: 18rpx;
  background: #edf5ff;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.prompt-input {
  flex: 1;
  min-height: 76rpx;
  max-height: 190rpx;
  padding: 18rpx 22rpx;
  border-radius: 18rpx;
  background: #f5f7fb;
  color: #17233d;
  font-size: 28rpx;
  line-height: 1.45;
  box-sizing: border-box;
}

.send-btn,
.stop-btn {
  width: 92rpx;
  height: 76rpx;
  border-radius: 18rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.send-btn {
  background: #1677ff;
}

.send-btn.disabled {
  background: #b8c7da;
}

.stop-btn {
  background: #fef0f0;
  color: #d03050;
  font-size: 26rpx;
  font-weight: 600;
}

.feedback-popup {
  padding: 34rpx 32rpx calc(34rpx + env(safe-area-inset-bottom));
  border-radius: 32rpx 32rpx 0 0;
  background: #f8fbff;
  border-top: 1px solid #e3edf9;
  box-shadow: 0 -16rpx 44rpx rgba(30, 48, 78, 0.12);
}

.feedback-popup-title {
  color: #17233d;
  font-size: 38rpx;
  font-weight: 700;
}

.feedback-option-list {
  display: flex;
  flex-wrap: wrap;
  gap: 16rpx;
  margin-top: 28rpx;
}

.feedback-option {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 66rpx;
  padding: 0 26rpx;
  border-radius: 999px;
  border: 1px solid #d7e6fb;
  background: #fff;
  color: #52627a;
  font-size: 26rpx;
  transition: all 0.18s ease;
}

.feedback-option.active {
  border-color: #8bb8ff;
  background: #edf5ff;
  color: #1677ff;
}

.feedback-input {
  width: 100%;
  min-height: 228rpx;
  margin-top: 26rpx;
  padding: 24rpx 22rpx;
  border-radius: 26rpx;
  border: 1px solid #dbe7f5;
  background: #fff;
  box-sizing: border-box;
  color: #17233d;
  font-size: 30rpx;
  line-height: 1.6;
  box-shadow: inset 0 1rpx 0 rgba(255, 255, 255, 0.65);
}

.feedback-popup-actions {
  display: flex;
  justify-content: flex-end;
  gap: 18rpx;
  margin-top: 32rpx;
}

.feedback-btn {
  min-width: 144rpx;
  height: 74rpx;
  padding: 0 34rpx;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 30rpx;
  font-weight: 600;
  box-sizing: border-box;
}

.feedback-btn.secondary {
  border: 1px solid #d7e3f3;
  background: #fff;
  color: #52627a;
}

.feedback-btn.primary {
  background: #1677ff;
  color: #fff;
  box-shadow: 0 10rpx 24rpx rgba(22, 119, 255, 0.22);
}

.feedback-btn.disabled {
  opacity: 0.42;
  pointer-events: none;
}

.resource-popup {
  padding: 26rpx 24rpx calc(26rpx + env(safe-area-inset-bottom));
  border-radius: 28rpx 28rpx 0 0;
  background: #fff;
}

.resource-popup-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16rpx;
}

.resource-popup-title {
  flex: 1;
  min-width: 0;
  color: #17233d;
  font-size: 30rpx;
  font-weight: 700;
}

.resource-popup-score {
  flex-shrink: 0;
  color: #1677ff;
  font-size: 22rpx;
}

.resource-popup-body {
  max-height: 56vh;
  margin-top: 20rpx;
}

.resource-dataset {
  margin-bottom: 16rpx;
  color: #6b7a90;
  font-size: 24rpx;
}

.resource-content {
  color: #17233d;
  font-size: 26rpx;
  line-height: 1.7;
  white-space: pre-wrap;
  word-break: break-word;
}

@keyframes pulse {
  0%,
  80%,
  100% {
    transform: scale(0.85);
    opacity: 0.45;
  }

  40% {
    transform: scale(1);
    opacity: 1;
  }
}
</style>
