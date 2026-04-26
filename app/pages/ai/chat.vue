<template>
	<view class="chat-page">
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
				<view class="welcome-icon">{{ getInitial(appName) }}</view>
				<view class="welcome-title">{{ appName || 'AI助手' }}</view>
				<view class="welcome-desc">输入问题开始对话。</view>
			</view>

			<view
				v-for="(message, index) in messages"
				:id="`msg-${index}`"
				:key="message.localId"
				:class="['message-row', message.role]"
				@longpress="copyMessage(message.content)"
			>
				<view v-if="message.role === 'assistant'" class="avatar">{{ getInitial(appName) }}</view>
				<view class="bubble">
					<view class="message-text">{{ message.content }}</view>
				</view>
			</view>

			<view v-if="sending" id="sending" class="message-row assistant">
				<view class="avatar">{{ getInitial(appName) }}</view>
				<view class="bubble loading-bubble">
					<view class="dot"></view>
					<view class="dot"></view>
					<view class="dot"></view>
				</view>
			</view>
		</scroll-view>

		<view class="input-bar">
			<textarea
				class="prompt-input"
				v-model="inputText"
				:disabled="sending"
				:auto-height="true"
				:maxlength="-1"
				:show-confirm-bar="false"
				confirm-type="send"
				placeholder="输入问题"
				@confirm="sendMessage"
			></textarea>
			<view :class="['send-btn', { disabled: !canSend }]" @click="sendMessage">
				<u-icon name="arrow-upward" size="20" color="#fff"></u-icon>
			</view>
		</view>
	</view>
</template>

<script setup>
	import { computed, getCurrentInstance, nextTick, ref } from 'vue'
	import { onLoad } from '@dcloudio/uni-app'

	const { proxy } = getCurrentInstance()
	const appId = ref(0)
	const appName = ref('')
	const conversationId = ref('')
	const conversationName = ref('')
	const inputText = ref('')
	const messages = ref([])
	const loadingMessages = ref(false)
	const sending = ref(false)
	const scrollIntoView = ref('')

	const canSend = computed(() => {
		return !!inputText.value.trim() && !sending.value && !!appId.value
	})

	const goBack = () => {
		uni.navigateBack()
	}

	const getInitial = (name) => {
		return (name || 'AI').substring(0, 1)
	}

	const createLocalId = () => {
		return `${Date.now()}-${Math.random().toString(16).slice(2)}`
	}

	const scrollToBottom = () => {
		nextTick(() => {
			if (sending.value) {
				scrollIntoView.value = 'sending'
				return
			}
			if (messages.value.length) {
				scrollIntoView.value = `msg-${messages.value.length - 1}`
			}
		})
	}

	const normalizeHistory = (rows) => {
		const ordered = [...(rows || [])].reverse()
		const result = []
		ordered.forEach((row) => {
			if (row.query) {
				result.push({
					localId: `${row.id || createLocalId()}-query`,
					role: 'user',
					content: row.query
				})
			}
			if (row.answer) {
				result.push({
					localId: row.id || createLocalId(),
					role: 'assistant',
					content: row.answer
				})
			}
		})
		messages.value = result
		scrollToBottom()
	}

	const loadMessages = () => {
		if (!appId.value || !conversationId.value) {
			return
		}
		loadingMessages.value = true
		proxy.http
			.get('api/AI/Messages', {
				appId: appId.value,
				conversationId: conversationId.value,
				limit: 50
			}, false)
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

	const appendMessage = (role, content) => {
		messages.value.push({
			localId: createLocalId(),
			role,
			content
		})
		scrollToBottom()
	}

	const sendMessage = () => {
		if (!canSend.value) {
			return
		}
		const query = inputText.value.trim()
		inputText.value = ''
		appendMessage('user', query)
		sending.value = true
		scrollToBottom()

		proxy.http
			.post('api/AI/SendMessage', {
				appId: appId.value,
				query,
				conversationId: conversationId.value,
				inputs: {}
			}, false)
			.then((result) => {
				if (!result.status) {
					appendMessage('assistant', result.message || '发送失败，请稍后重试。')
					return
				}

				const data = result.data || {}
				if (data.conversation_id) {
					conversationId.value = data.conversation_id
				}
				if (!conversationName.value && data.conversation_name) {
					conversationName.value = data.conversation_name
				}
				appendMessage('assistant', data.answer || 'AI未返回内容。')
			})
			.finally(() => {
				sending.value = false
				scrollToBottom()
			})
	}

	const newChat = () => {
		conversationId.value = ''
		conversationName.value = ''
		messages.value = []
		inputText.value = ''
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
		loadMessages()
	})
</script>

<style scoped lang="less">
	.chat-page {
		height: 100vh;
		background: #f5f7fb;
		display: flex;
		flex-direction: column;
		box-sizing: border-box;
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
	}

	.welcome-title {
		margin-top: 22rpx;
		font-size: 34rpx;
		font-weight: 700;
		color: #17233d;
	}

	.welcome-desc {
		margin-top: 12rpx;
		color: #8c96a6;
		font-size: 26rpx;
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
	}

	.bubble {
		max-width: 78%;
		border-radius: 8px;
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

	.dot {
		width: 10rpx;
		height: 10rpx;
		border-radius: 50%;
		background: #8c96a6;
		animation: pulse 1.2s infinite ease-in-out;
	}

	.dot:nth-child(2) {
		animation-delay: .16s;
	}

	.dot:nth-child(3) {
		animation-delay: .32s;
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

	.prompt-input {
		flex: 1;
		min-height: 76rpx;
		max-height: 190rpx;
		padding: 18rpx 22rpx;
		border-radius: 8px;
		background: #f5f7fb;
		color: #17233d;
		font-size: 28rpx;
		line-height: 1.45;
		box-sizing: border-box;
	}

	.send-btn {
		width: 76rpx;
		height: 76rpx;
		border-radius: 8px;
		background: #1677ff;
		display: flex;
		align-items: center;
		justify-content: center;
		flex-shrink: 0;
	}

	.send-btn.disabled {
		background: #b8c7da;
	}

	@keyframes pulse {
		0%,
		80%,
		100% {
			opacity: .35;
			transform: scale(.85);
		}

		40% {
			opacity: 1;
			transform: scale(1);
		}
	}
</style>
