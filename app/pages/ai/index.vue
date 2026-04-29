<template>
	<view class="ai-page">
		<view class="ai-header">
			<view>
				<view class="ai-title">AI助手</view>
				<view class="ai-subtitle">我的智能体</view>
			</view>
			<view class="ai-count">{{ filteredApps.length }}</view>
		</view>

		<view class="search-wrap">
			<u-search
				v-model="keyword"
				placeholder="搜索应用"
				:showAction="false"
				clearabled
				@clear="loadApps"
				@search="loadApps"
			></u-search>
		</view>

		<view v-if="showRecentSection" class="recent-panel">
			<view class="section-head">
				<view class="section-title">最近对话</view>
				<view class="section-subtitle">快速回到最近使用的智能体</view>
			</view>

			<view v-if="recentLoading" class="recent-state">正在获取最近会话...</view>
			<view v-else-if="recentConversations.length === 0" class="recent-state muted">暂无最近对话</view>
			<view v-else class="recent-list">
				<view
					class="recent-card"
					v-for="item in recentConversations"
					:key="`${item.appId}-${item.id}`"
					@click="openRecentConversation(item)"
				>
					<view class="recent-main">
						<view class="recent-title">{{ item.name || '未命名会话' }}</view>
						<view class="recent-desc">{{ item.appName }}</view>
					</view>
					<view class="recent-time">{{ formatTime(item.updated_at || item.created_at) }}</view>
				</view>
			</view>
		</view>

		<view class="section-head app-section">
			<view class="section-title">应用列表</view>
			<view class="section-subtitle">按角色授权展示可用智能体</view>
		</view>

		<view v-if="loading" class="state">加载中...</view>
		<view v-else-if="filteredApps.length === 0" class="state">暂无可用AI应用</view>
		<view v-else class="app-list">
			<view class="app-card" v-for="item in filteredApps" :key="item.id" @click="openApp(item)">
				<view class="app-icon">
					<image v-if="getDisplayIcon(item)" :src="getDisplayIcon(item)" mode="aspectFill"></image>
					<text v-else>{{ getDisplayGlyph(item) || getInitial(item.appName) }}</text>
				</view>
				<view class="app-content">
					<view class="app-row">
						<view class="app-name">{{ item.appName }}</view>
						<view class="app-type">{{ getTypeText(item.appType) }}</view>
					</view>
					<view class="app-desc">{{ item.description || '暂无描述' }}</view>
				</view>
			</view>
		</view>
	</view>
</template>

<script setup>
	import { computed, getCurrentInstance, onMounted, ref } from 'vue'
	import { onShow } from '@dcloudio/uni-app'
	import {
		resolveAiAppGlyphIcon,
		resolveAiAppImageSrc,
		getAiAppInitial,
		normalizeAiAppList
	} from '@/util/ai-app.js'

	const { proxy } = getCurrentInstance()
	const apps = ref([])
	const recentConversations = ref([])
	const keyword = ref('')
	const loading = ref(false)
	const recentLoading = ref(false)
	let loaded = false

	const filteredApps = computed(() => {
		const text = keyword.value.trim().toLowerCase()
		if (!text) {
			return apps.value
		}
		return apps.value.filter((item) => {
			return (
				(item.appName || '').toLowerCase().includes(text) ||
				(item.description || '').toLowerCase().includes(text)
			)
		})
	})

	const showRecentSection = computed(() => {
		return !keyword.value.trim()
	})

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

	const loadRecentConversations = async (appList) => {
		const targetApps = (appList || []).slice(0, 8)
		if (!targetApps.length) {
			recentConversations.value = []
			return
		}

		recentLoading.value = true
		try {
			const groups = await Promise.all(targetApps.map((item) => {
				return proxy.http
					.get('api/AI/Conversations', {
						appId: item.id,
						limit: 2
					}, false)
					.then((result) => {
						if (!result.status) {
							return []
						}

						return ((result.data || {}).data || []).map((conversation) => ({
							...conversation,
							appId: item.id,
							appName: item.appName
						}))
					})
					.catch(() => [])
			}))

			recentConversations.value = groups
				.flat()
				.sort((a, b) => (b.updated_at || b.created_at || 0) - (a.updated_at || a.created_at || 0))
				.slice(0, 6)
		} finally {
			recentLoading.value = false
		}
	}

	const loadApps = () => {
		loading.value = true
		proxy.http
			.get('api/AI/Apps', { keyword: keyword.value }, false)
			.then((result) => {
				const isSuccess = result?.status === true || result?.status === 0
				if (!isSuccess) {
					proxy.$toast(result.message || '加载失败')
					return
				}
				const appList = normalizeAiAppList(result.data || result.rows || [])
				apps.value = appList
				loaded = true
				if (!keyword.value.trim()) {
					loadRecentConversations(appList)
				}
			})
			.finally(() => {
				loading.value = false
			})
	}

	const getDisplayIcon = (item) => {
		return resolveAiAppImageSrc(item?.icon, proxy.http.ipAddress)
	}

	const getDisplayGlyph = (item) => {
		return resolveAiAppGlyphIcon(item?.icon)
	}

	const getInitial = getAiAppInitial

	const getTypeText = (type) => {
		const map = {
			chat: '对话',
			agent: 'Agent',
			workflow: 'Workflow'
		}
		return map[type] || type || 'AI'
	}

	const openApp = (item) => {
		uni.navigateTo({
			url: `/pages/ai/conversations?appId=${item.id}&appName=${encodeURIComponent(item.appName || '')}`
		})
	}

	const openRecentConversation = (item) => {
		uni.navigateTo({
			url: `/pages/ai/chat?appId=${item.appId}&appName=${encodeURIComponent(item.appName || '')}&conversationId=${encodeURIComponent(item.id || '')}&conversationName=${encodeURIComponent(item.name || '')}`
		})
	}

	onMounted(loadApps)
	onShow(() => {
		if (loaded) {
			loadApps()
		}
	})
</script>

<style scoped lang="less">
	.ai-page {
		min-height: 100%;
		background: #f5f7fb;
		padding: 28rpx;
		box-sizing: border-box;
		display: flex;
		flex-direction: column;
	}

	.ai-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 10rpx 4rpx 24rpx;
	}

	.ai-title {
		font-size: 42rpx;
		font-weight: 700;
		color: #17233d;
	}

	.ai-subtitle {
		margin-top: 8rpx;
		font-size: 24rpx;
		color: #7a8799;
	}

	.ai-count {
		min-width: 64rpx;
		height: 64rpx;
		border-radius: 50%;
		background: #1677ff;
		color: #fff;
		font-size: 28rpx;
		font-weight: 700;
		display: flex;
		align-items: center;
		justify-content: center;
	}

	.search-wrap {
		margin-bottom: 22rpx;
		order: 1;
	}

	.section-head {
		padding: 4rpx 2rpx 18rpx;
	}

	.section-title {
		font-size: 30rpx;
		font-weight: 700;
		color: #17233d;
	}

	.section-subtitle {
		margin-top: 8rpx;
		font-size: 22rpx;
		color: #7a8799;
	}

	.recent-panel {
		margin-bottom: 16rpx;
		order: 4;
	}

	.recent-list {
		display: flex;
		flex-direction: column;
		gap: 14rpx;
	}

	.recent-card {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 16rpx;
		background: #fff;
		border-radius: 8px;
		padding: 22rpx 24rpx;
		box-shadow: 0 4rpx 18rpx rgba(30, 48, 78, 0.06);
	}

	.recent-main {
		flex: 1;
		min-width: 0;
	}

	.recent-title {
		font-size: 28rpx;
		font-weight: 650;
		color: #17233d;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.recent-desc {
		margin-top: 8rpx;
		color: #7a8799;
		font-size: 24rpx;
	}

	.recent-time {
		color: #a6afbd;
		font-size: 22rpx;
		flex-shrink: 0;
	}

	.recent-state {
		padding: 18rpx 6rpx 24rpx;
		color: #7a8799;
		font-size: 24rpx;
	}

	.recent-state.muted {
		color: #b8c7da;
	}

	.app-section {
		margin-top: 10rpx;
		order: 2;
	}

	.app-list {
		display: flex;
		flex-direction: column;
		gap: 18rpx;
		padding-bottom: 110rpx;
		order: 3;
	}

	.app-card {
		display: flex;
		align-items: center;
		gap: 22rpx;
		background: #fff;
		border-radius: 8px;
		padding: 24rpx;
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
		overflow: hidden;
		flex-shrink: 0;

		image {
			width: 100%;
			height: 100%;
		}
	}

	.app-content {
		flex: 1;
		min-width: 0;
	}

	.app-row {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 16rpx;
	}

	.app-name {
		color: #17233d;
		font-size: 30rpx;
		font-weight: 650;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.app-type {
		color: #1677ff;
		background: #edf5ff;
		border-radius: 4px;
		padding: 4rpx 12rpx;
		font-size: 22rpx;
		flex-shrink: 0;
	}

	.app-desc {
		margin-top: 10rpx;
		color: #7a8799;
		font-size: 24rpx;
		line-height: 1.45;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.state {
		margin-top: 140rpx;
		text-align: center;
		color: #8c96a6;
		font-size: 28rpx;
		order: 3;
	}
</style>
