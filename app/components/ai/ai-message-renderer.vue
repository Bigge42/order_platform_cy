<template>
  <view class="assistant-renderer">
    <view v-for="block in blocks" :key="block.id" class="block-item">
      <view v-if="block.type === 'markdown'" class="markdown-block">
        <u-parse :content="block.html"></u-parse>
      </view>

      <view v-else-if="block.type === 'think'" class="think-card">
        <view class="think-head" @click="toggleThink(block.id)">
          <view class="think-title">{{ block.title || '思考过程' }}</view>
          <u-icon :name="isThinkCollapsed(block.id) ? 'arrow-down' : 'arrow-up'" size="16" color="#6b7a90"></u-icon>
        </view>
        <view v-if="!isThinkCollapsed(block.id)" class="think-body">
          <u-parse :content="block.html"></u-parse>
        </view>
      </view>

      <ai-message-chart
        v-else-if="block.type === 'chart'"
        :title="block.title"
        :description="block.description"
        :option="block.option"
      />
    </view>

    <view v-if="messageFiles.length" class="meta-section">
      <view class="section-title">附件</view>
      <view
        v-for="file in messageFiles"
        :key="file.id || file.name"
        class="file-card"
        @click="$emit('preview-file', file)"
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
          @click="$emit('preview-resource', resource)"
        >
          <text class="resource-index">[{{ resource.position }}]</text>
          <text class="resource-name">{{ resource.documentName }}</text>
          <text v-if="formatScore(resource.score)" class="resource-score">{{ formatScore(resource.score) }}</text>
        </view>
      </view>
    </view>

    <view
      v-if="message.usage"
      :class="['usage-chip', { active: !!message.showUsage }]"
      @click.stop="$emit('toggle-usage', message.localId)"
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
</template>

<script setup>
import { computed, reactive, watch } from 'vue'
import { formatDuration, formatScore } from '@/util/ai-message.js'
import AiMessageChart from './ai-message-chart.vue'

const props = defineProps({
  message: {
    type: Object,
    default: () => ({})
  }
})

defineEmits(['preview-resource', 'preview-file', 'toggle-usage'])

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

const showPrice = computed(() => {
  const totalPrice = props.message?.usage?.totalPrice
  return totalPrice && totalPrice !== '0' && totalPrice !== '0.0'
})

watch(
  blocks,
  (value) => {
    const nextIds = (value || [])
      .filter(item => item.type === 'think')
      .map(item => item.id)

    nextIds.forEach((id) => {
      if (typeof thinkState[id] === 'undefined') {
        thinkState[id] = true
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
.usage-chip {
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
  width: 100%;
  border-collapse: collapse;
  font-size: 24rpx;
}

.markdown-block :deep(th),
.markdown-block :deep(td) {
  border: 1px solid #dbe4f0;
  padding: 12rpx;
}

.markdown-block :deep(blockquote) {
  margin: 0;
  padding: 12rpx 18rpx;
  border-left: 6rpx solid #c7ddfb;
  background: #f6faff;
  color: #4d5d74;
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

.usage-chip {
  position: relative;
  display: inline-flex;
  align-items: center;
  gap: 8rpx;
  padding: 12rpx 16rpx;
  border-radius: 999px;
  background: #f6f7fb;
  color: #52627a;
  font-size: 22rpx;
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
