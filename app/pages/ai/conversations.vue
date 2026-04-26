<template>
	<view class="conversation-page">
		<view class="top-bar">
			<view class="nav-btn" @click="goBack">
				<u-icon name="arrow-left" size="22" color="#17233d"></u-icon>
			</view>
			<view class="title">{{ appName || 'AI助手' }}</view>
			<view class="nav-btn primary" @click="openNewChat">
				<u-icon name="plus" size="20" color="#1677ff"></u-icon>
			</view>
		</view>

		<view class="app-panel">
			<view class="app-icon">{{ getInitial(appName) }}</view>
			<view class="app-info">
				<view class="app-name">{{ appName || 'AI助手' }}</view>
				<view class="app-desc">已授权应用，可继续历史会话或发起新对话</view>
			</view>
		</view>

		<view v-if="loading" class="state">加载中...</view>
		<view v-else-if="conversations.length === 0" class="empty">
			<view class="empty-title">暂无会话记录</view>
			<view class="empty-desc">新建一个对话，开始向该智能体提问。</view>
			<view class="empty-action" @click="openNewChat">新建对话</view>
		</view>
		<scroll-view v-else class="conversation-list" scroll-y lower-threshold="80" @scrolltolower="loadMore">
			<view
				class="conversation-card"
				v-for="item in conversations"
				:key="item.id"
				@click="openChat(item)"
			>
				<view class="conversation-main">
					<view class="conversation-title">{{ item.name || '未命名会话' }}</view>
					<view class="conversation-desc">{{ item.introduction || '点击继续对话' }}</view>
				</view>
				<view class="conversation-meta">
					<view class="time">{{ formatTime(item.updated_at || item.created_at) }}</view>
					<u-icon name="arrow-right" size="14" color="#a6afbd"></u-icon>
				</view>
			</view>
			<view v-if="loadingMore" class="load-more">加载更多...</view>
			<view v-else-if="!hasMore && conversations.length > 0" class="load-more muted">没有更多了</view>
		</scroll-view>
	</view>
</template>

<script setup>
	import { getCurrentInstance, ref } from 'vue'
	import { onLoad, onShow } from '@dcloudio/uni-app'

	const { proxy } = getCurrentInstance()
	const appId = ref(0)
	const appName = ref('')
	const conversations = ref([])
	const loading = ref(false)
	const loadingMore = ref(false)
	const hasMore = ref(false)
	let loaded = false
	let lastId = ''

	const goBack = () => {
		uni.navigateBack()
	}

	const getInitial = (name) => {
		return (name || 'AI').substring(0, 1)
	}

	const formatTime = (timestamp) => {
		if (!timestamp) {
			return ''
		}
		const date = new Date(Number(timestamp) * 1000)
		const month = `${date.getMonth() + 1}`.padStart(2, '0')
		const day = `${date.getDate()}`.padStart(2, '0')
		const hour = `${date.getHours()}`.padStart(2, '0')
		const minute = `${date.getMinutes()}`.padStart(2, '0')
		return `${month}-${day} ${hour}:${minute}`
	}

	const loadConversations = (reset = true) => {
		if (!appId.value) {
			return
		}
		if (reset) {
			loading.value = true
			lastId = ''
		} else {
			if (!hasMore.value || loadingMore.value) {
				return
			}
			loadingMore.value = true
		}

		proxy.http
			.get('api/AI/Conversations', {
				appId: appId.value,
				lastId,
				limit: 20
			}, false)
			.then((result) => {
				if (!result.status) {
					proxy.$toast(result.message || '加载会话失败')
					return
				}
				const data = result.data || {}
				const list = data.data || []
				conversations.value = reset ? list : conversations.value.concat(list)
				hasMore.value = !!data.has_more
				if (list.length) {
					lastId = list[list.length - 1].id
				}
				loaded = true
			})
			.finally(() => {
				loading.value = false
				loadingMore.value = false
			})
	}

	const loadMore = () => {
		loadConversations(false)
	}

	const openNewChat = () => {
		uni.navigateTo({
			url: `/pages/ai/chat?appId=${appId.value}&appName=${encodeURIComponent(appName.value || '')}`
		})
	}

	const openChat = (item) => {
		uni.navigateTo({
			url: `/pages/ai/chat?appId=${appId.value}&appName=${encodeURIComponent(appName.value || '')}&conversationId=${encodeURIComponent(item.id)}&conversationName=${encodeURIComponent(item.name || '')}`
		})
	}

	onLoad((options) => {
		appId.value = Number(options.appId || 0)
		appName.value = decodeURIComponent(options.appName || '')
		loadConversations()
	})

	onShow(() => {
		if (loaded) {
			loadConversations()
		}
	})
</script>

<style scoped lang="less">
	.conversation-page {
		min-height: 100%;
		background: #f5f7fb;
		padding: 24rpx;
		box-sizing: border-box;
	}

	.top-bar {
		display: flex;
		align-items: center;
		height: 72rpx;
		margin-bottom: 18rpx;
	}

	.nav-btn {
		width: 72rpx;
		height: 72rpx;
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: 8px;
	}

	.nav-btn.primary {
		background: #edf5ff;
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

	.app-panel {
		display: flex;
		align-items: center;
		gap: 22rpx;
		background: #fff;
		border-radius: 8px;
		padding: 26rpx;
		box-shadow: 0 4rpx 18rpx rgba(30, 48, 78, 0.06);
	}

	.app-icon {
		width: 88rpx;
		height: 88rpx;
		border-radius: 8px;
		background: #eaf3ff;
		color: #1677ff;
		font-size: 36rpx;
		font-weight: 700;
		display: flex;
		align-items: center;
		justify-content: center;
		flex-shrink: 0;
	}

	.app-info {
		min-width: 0;
		flex: 1;
	}

	.app-name {
		font-size: 32rpx;
		font-weight: 700;
		color: #17233d;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.app-desc {
		margin-top: 8rpx;
		font-size: 24rpx;
		line-height: 1.45;
		color: #7a8799;
	}

	.conversation-list {
		height: calc(100vh - 230rpx);
		margin-top: 20rpx;
	}

	.conversation-card {
		display: flex;
		align-items: center;
		gap: 18rpx;
		background: #fff;
		border-radius: 8px;
		padding: 24rpx;
		margin-bottom: 16rpx;
		box-shadow: 0 4rpx 18rpx rgba(30, 48, 78, 0.06);
	}

	.conversation-main {
		flex: 1;
		min-width: 0;
	}

	.conversation-title {
		color: #17233d;
		font-size: 30rpx;
		font-weight: 650;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.conversation-desc {
		margin-top: 10rpx;
		color: #7a8799;
		font-size: 24rpx;
		line-height: 1.45;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.conversation-meta {
		width: 130rpx;
		display: flex;
		align-items: center;
		justify-content: flex-end;
		gap: 8rpx;
		flex-shrink: 0;
	}

	.time {
		color: #a6afbd;
		font-size: 22rpx;
	}

	.state,
	.empty {
		margin-top: 180rpx;
		text-align: center;
	}

	.empty-title {
		font-size: 32rpx;
		font-weight: 650;
		color: #17233d;
	}

	.empty-desc {
		margin: 16rpx auto 0;
		max-width: 520rpx;
		color: #8c96a6;
		font-size: 26rpx;
		line-height: 1.5;
	}

	.empty-action {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		margin-top: 28rpx;
		height: 72rpx;
		padding: 0 34rpx;
		border-radius: 8px;
		color: #fff;
		background: #1677ff;
		font-size: 28rpx;
		font-weight: 650;
	}

	.state,
	.load-more {
		color: #8c96a6;
		font-size: 26rpx;
	}

	.load-more {
		padding: 20rpx 0 32rpx;
		text-align: center;
	}

	.load-more.muted {
		color: #b6bfcc;
	}
</style>
