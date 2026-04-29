<template>
  <view class="assistant-renderer">
    <view v-for="block in blocks" :key="block.id" class="block-item">
      <template v-if="block.type === 'markdown'">
        <view v-if="block.html" class="markdown-block">
          <u-parse :content="block.html"></u-parse>
        </view>
        <view v-if="getBlockImages(block).length" class="markdown-image-list">
          <view
            v-for="image in getBlockImages(block)"
            :key="image.id || image.previewUrl || image.url"
            class="markdown-image-card"
          >
            <image
              v-if="getImageSrc(image)"
              :src="getImageSrc(image)"
              class="markdown-image"
              mode="widthFix"
              @click="emit('preview-file', image)"
            ></image>
            <view v-else class="markdown-image-placeholder">
              <u-icon name="photo" size="28" color="#9ab4d5"></u-icon>
            </view>
          </view>
        </view>
      </template>

      <view v-else-if="block.type === 'think'" class="think-card">
        <view class="think-head" @click="toggleThink(block.id)">
          <view class="think-title">{{ getThinkTitle(block) }}</view>
          <u-icon :name="isThinkCollapsed(block.id) ? 'arrow-down' : 'arrow-up'" size="16" color="#6b7a90"></u-icon>
        </view>
        <view v-if="!isThinkCollapsed(block.id)" class="think-body">
          <u-parse v-if="block.html" :content="block.html"></u-parse>
          <view v-if="getBlockImages(block).length" class="markdown-image-list">
            <view
              v-for="image in getBlockImages(block)"
              :key="image.id || image.previewUrl || image.url"
              class="markdown-image-card"
            >
              <image
                v-if="getImageSrc(image)"
                :src="getImageSrc(image)"
                class="markdown-image"
                mode="widthFix"
                @click="emit('preview-file', image)"
              ></image>
              <view v-else class="markdown-image-placeholder">
                <u-icon name="photo" size="28" color="#9ab4d5"></u-icon>
              </view>
            </view>
          </view>
        </view>
      </view>

      <ai-message-chart
        v-else-if="block.type === 'chart'"
        :title="block.title"
        :description="block.description"
        :option="block.option"
      />
    </view>

    <view v-if="imageFiles.length" class="meta-section">
      <view class="section-title">图片</view>
      <view class="image-grid">
        <view
          v-for="file in imageFiles"
          :key="file.uploadFileId || file.id || file.localId || file.name"
          class="image-card"
          @click="emit('preview-file', file)"
        >
          <image
            v-if="getImageSrc(file)"
            :src="getImageSrc(file)"
            class="image-thumb"
            mode="aspectFill"
          ></image>
          <view v-else class="image-placeholder">
            <u-icon name="photo" size="28" color="#9ab4d5"></u-icon>
          </view>
          <view class="image-name">{{ file.name }}</view>
        </view>
      </view>
    </view>

    <view v-if="otherFiles.length" class="meta-section">
      <view class="section-title">附件</view>
      <view
        v-for="file in otherFiles"
        :key="file.uploadFileId || file.id || file.localId || file.name"
        class="file-card"
        @click="emit('preview-file', file)"
      >
        <view class="file-badge">{{ formatFileType(file.type) }}</view>
        <view class="file-name">{{ file.name }}</view>
        <view class="file-action">预览</view>
      </view>
    </view>

    <view v-if="resources.length" class="meta-section">
      <view class="section-title">引用文档</view>
      <view class="resource-list">
        <view
          v-for="resource in resources"
          :key="resource.id"
          class="resource-chip"
          @click="emit('preview-resource', resource)"
        >
          <text class="resource-index">[{{ resource.position }}]</text>
          <text class="resource-name">{{ resource.documentName }}</text>
          <text v-if="formatScore(resource.score)" class="resource-score">{{ formatScore(resource.score) }}</text>
        </view>
      </view>
    </view>

    <view v-if="showActionRow" class="message-action-row">
      <view v-if="showActionIcons" class="message-action-tools">
        <view
          v-if="message.messageId"
          :class="[
            'icon-action',
            'feedback-action',
            { active: isFeedbackActive('like'), disabled: !!message.feedbackLoading || busy }
          ]"
          title="喜欢"
          @click.stop="emitFeedback('like')"
        >
          <u-icon
            :name="isFeedbackActive('like') ? 'thumb-up-fill' : 'thumb-up'"
            size="18"
            :color="isFeedbackActive('like') ? '#1677ff' : '#6b7a90'"
          ></u-icon>
          <view class="action-tooltip">喜欢</view>
        </view>
        <view
          v-if="message.messageId"
          :class="[
            'icon-action',
            'feedback-action',
            'negative',
            { active: isFeedbackActive('dislike'), disabled: !!message.feedbackLoading || busy }
          ]"
          title="不喜欢"
          @click.stop="emitFeedback('dislike')"
        >
          <u-icon
            :name="isFeedbackActive('dislike') ? 'thumb-down-fill' : 'thumb-down'"
            size="18"
            :color="isFeedbackActive('dislike') ? '#cf1322' : '#6b7a90'"
          ></u-icon>
          <view class="action-tooltip">不喜欢</view>
        </view>
        <view
          v-if="message.content"
          class="icon-action"
          title="复制"
          @click.stop="emit('copy-message', message.localId)"
        >
          <u-icon name="file-text" size="18" color="#6b7a90"></u-icon>
          <view class="action-tooltip">复制</view>
        </view>
        <view
          v-if="canRegenerate"
          :class="['icon-action', 'regenerate-action', { disabled: busy }]"
          title="重新生成"
          @click.stop="emitRegenerate"
        >
          <u-icon name="reload" size="18" color="#6b7a90"></u-icon>
          <view class="action-tooltip">重新生成</view>
        </view>
      </view>
      <view v-if="showUsageChip" class="message-action-meta">
        <view
          :class="['usage-chip', { active: !!message.showUsage }]"
          @click.stop="emit('toggle-usage', message.localId)"
        >
          <text>耗时 {{ formatDuration(message.usage.latency) }}</text>
          <text>·</text>
          <text>{{ message.usage.totalTokens }} tokens</text>
          <view class="usage-card">
            <view class="usage-row">
              <text>总 Token</text>
              <text>{{ message.usage.totalTokens }}</text>
            </view>
            <view class="usage-row">
              <text>输入 Token</text>
              <text>{{ message.usage.promptTokens }}</text>
            </view>
            <view class="usage-row">
              <text>输出 Token</text>
              <text>{{ message.usage.completionTokens }}</text>
            </view>
            <view class="usage-row">
              <text>总耗时</text>
              <text>{{ formatDuration(message.usage.latency) }}</text>
            </view>
            <view v-if="message.usage.timeToFirstToken" class="usage-row">
              <text>首字耗时</text>
              <text>{{ formatDuration(message.usage.timeToFirstToken) }}</text>
            </view>
            <view v-if="message.usage.timeToGenerate" class="usage-row">
              <text>生成耗时</text>
              <text>{{ formatDuration(message.usage.timeToGenerate) }}</text>
            </view>
            <view v-if="showPrice" class="usage-row">
              <text>价格</text>
              <text>{{ message.usage.totalPrice }} {{ message.usage.currency || '' }}</text>
            </view>
          </view>
        </view>
      </view>
    </view>
  </view>
</template>

<script setup>
import { computed, reactive, watch } from 'vue'
import {
  formatDuration,
  formatScore,
  getMessageFileImageDisplaySrc
} from '@/util/ai-message.js'
import AiMessageChart from './ai-message-chart.vue'

const props = defineProps({
  message: {
    type: Object,
    default: () => ({})
  },
  busy: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits([
  'preview-resource',
  'preview-file',
  'toggle-usage',
  'feedback-message',
  'copy-message',
  'regenerate-message'
])

const thinkState = reactive({})

const blocks = computed(() => {
  return Array.isArray(props.message?.blocks) ? props.message.blocks : []
})

const resources = computed(() => {
  return Array.isArray(props.message?.retrieverResources) ? props.message.retrieverResources : []
})

const messageFiles = computed(() => {
  return Array.isArray(props.message?.messageFiles) ? props.message.messageFiles : []
})

const imageFiles = computed(() => {
  return messageFiles.value.filter(file => (file?.type || '').toLowerCase() === 'image')
})

const otherFiles = computed(() => {
  return messageFiles.value.filter(file => (file?.type || '').toLowerCase() !== 'image')
})

const showPrice = computed(() => {
  const totalPrice = props.message?.usage?.totalPrice
  return totalPrice !== undefined && totalPrice !== null && totalPrice !== '' && totalPrice !== '0' && totalPrice !== '0.0'
})

const showActionRow = computed(() => {
  return !!(props.message?.messageId || props.message?.content || props.message?.usage)
})

const showActionIcons = computed(() => {
  return !!(props.message?.messageId || props.message?.content)
})

const showUsageChip = computed(() => {
  return !!props.message?.usage
})

const canRegenerate = computed(() => {
  return !!(props.message?.localId && (props.message?.content || props.message?.messageId))
})

watch(
  blocks,
  (value) => {
    const nextIds = (value || [])
      .filter(item => item.type === 'think')
      .map(item => item.id)

    nextIds.forEach((id) => {
      if (typeof thinkState[id] === 'undefined') {
        thinkState[id] = false
      }
    })

    Object.keys(thinkState).forEach((id) => {
      if (!nextIds.includes(id)) {
        delete thinkState[id]
      }
    })
  },
  {
    immediate: true,
    deep: true
  }
)

const isThinkCollapsed = (id) => {
  return thinkState[id] !== false
}

const toggleThink = (id) => {
  thinkState[id] = !isThinkCollapsed(id)
}

const getImageSrc = (file) => {
  return getMessageFileImageDisplaySrc(file)
}

const getBlockImages = (block) => {
  return Array.isArray(block?.images) ? block.images : []
}

const isFeedbackActive = (rating) => {
  return props.message?.feedback === rating
}

const formatThinkingDuration = () => {
  const durationMs = Number(props.message?.thinkDurationMs || 0)
  const fallbackSeconds = Number(props.message?.usage?.timeToFirstToken || 0)
  const totalMs = durationMs > 0 ? durationMs : fallbackSeconds > 0 ? fallbackSeconds * 1000 : 0

  if (!totalMs) {
    return ''
  }

  const seconds = totalMs / 1000
  if (seconds < 100) {
    return `${seconds.toFixed(1)}秒`
  }
  return `${Math.round(seconds)}秒`
}

const getThinkTitle = (block) => {
  const title = String(block?.title || '').trim()
  const isGenericTitle = !title || title === '思考过程' || /^思考\s*\d+$/i.test(title)

  if (!isGenericTitle) {
    return title
  }

  if (block?.pending && props.message?.streaming) {
    return '正在思考'
  }

  const durationText = formatThinkingDuration()
  return durationText ? `已思考（用时${durationText}）` : '已思考'
}

const emitFeedback = (rating) => {
  if (!props.message?.messageId || props.message?.feedbackLoading || props.busy) {
    return
  }

  emit('feedback-message', {
    localId: props.message.localId,
    rating
  })
}

const emitRegenerate = () => {
  if (!canRegenerate.value || props.busy) {
    return
  }

  emit('regenerate-message', props.message.localId)
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
</script>

<style scoped lang="less">
.assistant-renderer {
  width: 100%;
}

.block-item + .block-item,
.meta-section,
.message-action-row {
  margin-top: 18rpx;
}

.markdown-block {
  color: #17233d;
  font-size: 28rpx;
  line-height: 1.65;
  word-break: break-word;
}

.markdown-block :deep(p) {
  margin: 0 0 16rpx;
}

.markdown-block :deep(p:last-child) {
  margin-bottom: 0;
}

.markdown-block :deep(pre) {
  overflow-x: auto;
  border-radius: 16rpx;
  background: #0f172a;
  color: #e2e8f0;
  padding: 18rpx;
  font-size: 22rpx;
}

.markdown-block :deep(code) {
  word-break: break-word;
}

.markdown-block :deep(table) {
  display: block;
  width: max-content;
  min-width: 100%;
  border-collapse: collapse;
  font-size: 24rpx;
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}

.markdown-block :deep(th),
.markdown-block :deep(td) {
  border: 1px solid #dbe4f0;
  padding: 12rpx;
  min-width: 220rpx;
  box-sizing: border-box;
  word-break: break-word;
}

.markdown-block :deep(blockquote) {
  margin: 0;
  padding: 12rpx 18rpx;
  border-left: 6rpx solid #c7ddfb;
  background: #f6faff;
  color: #4d5d74;
}

.markdown-image-list {
  display: flex;
  flex-direction: column;
  gap: 16rpx;
}

.markdown-image-card {
  overflow: hidden;
  border-radius: 16rpx;
  border: 1px solid #e2eefc;
  background: #f8fbff;
}

.markdown-image,
.markdown-image-placeholder {
  width: 100%;
  min-height: 180rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #eef5ff;
}

.think-card {
  border: 1px solid #dbe7f5;
  border-radius: 18rpx;
  background: #f9fbfe;
  overflow: hidden;
}

.think-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16rpx;
  padding: 16rpx 18rpx;
}

.think-title {
  font-size: 24rpx;
  font-weight: 600;
  color: #52627a;
}

.think-body {
  padding: 0 18rpx 18rpx;
  color: #52627a;
}

.think-body :deep(p) {
  margin: 0 0 14rpx;
}

.section-title {
  margin-bottom: 12rpx;
  font-size: 22rpx;
  color: #6b7a90;
}

.image-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 12rpx;
}

.image-card {
  width: 188rpx;
  overflow: hidden;
  border-radius: 16rpx;
  background: #f8fbff;
  border: 1px solid #e2eefc;
}

.image-thumb,
.image-placeholder {
  width: 188rpx;
  height: 188rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #eef5ff;
}

.image-name {
  padding: 12rpx 14rpx;
  color: #17233d;
  font-size: 22rpx;
  line-height: 1.45;
  word-break: break-word;
}

.file-card {
  display: flex;
  align-items: center;
  gap: 14rpx;
  padding: 16rpx 18rpx;
  border-radius: 16rpx;
  background: #f8fbff;
  border: 1px solid #e2eefc;
}

.file-card + .file-card {
  margin-top: 10rpx;
}

.file-badge {
  min-width: 72rpx;
  height: 40rpx;
  padding: 0 12rpx;
  border-radius: 999px;
  background: #eaf3ff;
  color: #1677ff;
  font-size: 20rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.file-name {
  flex: 1;
  min-width: 0;
  color: #17233d;
  font-size: 24rpx;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.file-action {
  flex-shrink: 0;
  font-size: 22rpx;
  color: #1677ff;
}

.resource-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12rpx;
}

.resource-chip {
  display: inline-flex;
  align-items: center;
  gap: 8rpx;
  max-width: 100%;
  padding: 10rpx 16rpx;
  border-radius: 999px;
  background: #f6faff;
  border: 1px solid #d7e6fb;
  color: #3868a8;
  font-size: 22rpx;
}

.resource-index,
.resource-score {
  flex-shrink: 0;
}

.resource-name {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.message-action-row {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 12rpx;
}

.message-action-tools {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12rpx;
  width: 100%;
}

.message-action-meta {
  width: 100%;
}

.icon-action,
.usage-chip {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 22rpx;
}

.icon-action {
  width: 60rpx;
  height: 60rpx;
  border-radius: 18rpx;
  border: 1px solid #e2e8f0;
  background: #fff;
  color: #52627a;
}

.icon-action.disabled {
  opacity: 0.55;
}

.regenerate-action {
  margin-left: auto;
}

.action-tooltip {
  position: absolute;
  left: 50%;
  bottom: calc(100% + 10rpx);
  transform: translateX(-50%) translateY(6rpx);
  padding: 8rpx 14rpx;
  border-radius: 12rpx;
  background: rgba(21, 31, 47, 0.94);
  color: #fff;
  font-size: 20rpx;
  line-height: 1.2;
  white-space: nowrap;
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.18s ease, transform 0.18s ease;
  z-index: 5;
}

.icon-action:hover .action-tooltip {
  opacity: 1;
  transform: translateX(-50%) translateY(0);
}

.feedback-action.active {
  border-color: #bfd8ff;
  background: #eef5ff;
  color: #1677ff;
}

.feedback-action.negative.active {
  border-color: #ffc9c2;
  background: #fff2f0;
  color: #cf1322;
}

.usage-chip {
  gap: 8rpx;
  padding: 12rpx 16rpx;
  border-radius: 999px;
  background: #f6f7fb;
  color: #52627a;
}

.usage-card {
  position: absolute;
  left: 0;
  bottom: calc(100% + 12rpx);
  min-width: 300rpx;
  padding: 18rpx;
  border-radius: 18rpx;
  background: rgba(21, 31, 47, 0.96);
  color: #fff;
  box-shadow: 0 18rpx 40rpx rgba(15, 23, 42, 0.22);
  opacity: 0;
  pointer-events: none;
  transform: translateY(8rpx);
  transition: opacity 0.2s ease, transform 0.2s ease;
  z-index: 4;
}

.usage-chip.active .usage-card,
.usage-chip:hover .usage-card {
  opacity: 1;
  pointer-events: auto;
  transform: translateY(0);
}

.usage-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20rpx;
  font-size: 22rpx;
  line-height: 1.5;
}

.usage-row + .usage-row {
  margin-top: 8rpx;
}
</style>
