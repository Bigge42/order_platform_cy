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

		<view v-if="loading" class="state">加载中...</view>
		<view v-else-if="filteredApps.length === 0" class="state">暂无可用AI应用</view>
		<view v-else class="app-list">
			<view class="app-card" v-for="item in filteredApps" :key="item.id" @click="openApp(item)">
				<view class="app-icon">
					<image v-if="item.icon" :src="getIcon(item.icon)" mode="aspectFill"></image>
					<text v-else>{{ getInitial(item.appName) }}</text>
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

	const { proxy } = getCurrentInstance()
	const apps = ref([])
	const keyword = ref('')
	const loading = ref(false)
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

	const loadApps = () => {
		loading.value = true
		proxy.http
			.get('api/AI/Apps', { keyword: keyword.value }, false)
			.then((result) => {
				if (!result.status) {
					proxy.$toast(result.message || '加载失败')
					return
				}
				apps.value = result.data || []
				loaded = true
			})
			.finally(() => {
				loading.value = false
			})
	}

	const getIcon = (icon) => {
		if (!icon) {
			return ''
		}
		if (icon.startsWith('http') || icon.startsWith('/static')) {
			return icon
		}
		if (icon.startsWith('/')) {
			return proxy.http.ipAddress + icon.substring(1)
		}
		return proxy.http.ipAddress + icon
	}

	const getInitial = (name) => {
		return (name || 'AI').substring(0, 1)
	}

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
	}

	.app-list {
		display: flex;
		flex-direction: column;
		gap: 18rpx;
		padding-bottom: 110rpx;
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
	}
</style>
