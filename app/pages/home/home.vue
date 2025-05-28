<template>
	<view class="home">
		<view class="home-bg fx">
			<view class="login-title">
				<view class="login-title-txt">
					移动端
					<view class="login-title-txt-small">mobile devices</view>
				</view>
			</view>

			<image class="home-bg-img" src="/static/home-bg.png"></image>
			<!-- 	<image class="home-bg-img" src="/static/home-bg.png"></image> -->
		</view>

		<view class="home-item home-item-grid home-item-first">
			<u-grid :border="false" @click="gridClick" col="4">
				<u-grid-item v-for="(item,index) in fn" :key="index">
					<view :class="['grid-item-bg','grid-item-bg-'+(index+1)]">
						<image style="width:100rpx;height: 100rpx;" :src="item.icon"></image>
					</view>
					<view class="grid-text">{{item.name}}</view>
				</u-grid-item>
			</u-grid>
		</view>
		<view class="home-item" style="border: 1px solid #d8edff;border-radius: 5px;">
			<up-notice-bar bgColor="#3a9bff1a" color="#007aff"
				:text="noticeText"></up-notice-bar>
		</view>

		<!-- 功能列表 -->
		<view class="home-item home-item-grid" style="padding: 0;background: none;">
			<func></func>
		</view>
		<!-- 我的审批 -->
		<work></work>

	</view>
</template>

<script setup>
	import func from './func.vue'
	import work from './work.vue'
	import {
		ref
	} from 'vue';

	const noticeText = ref('');
	noticeText.value = 'xxx'

	const fn = ref([{
			name: "菜单功能",
			icon: '/static/icon/11.png',
			path: "/pages/menu/menu",
			subPage: false //一级页面
		}, {
			name: "审批流程",
			icon: '/static/icon/26.png',
			path: "/pages/flow/flow",
			subPage: false
		},
		{
			name: "报表分析",
			icon: '/static/icon/31.png',
			path:"/pages/report/index",
			//path: "/pages/message/message",
			subPage: false
		},
		{
			name: "个人中心",
			icon: '/static/icon/59.png',
			path: "/pages/user/user",
			subPage: false
		},
	]);

	const gridClick = (index) => {
		const item = fn.value[index];
		if (!item.path) {
			return;
		}
		//注意下面的跳转方式，一级页面指pages.json中tabBar配置path
		//具体见uni页面跳转文档
		if (item.subPage) {
			//二级页面用navigateTo跳转
			uni.navigateTo({
				url: fn.value[index].path
			})
			return;
		}
		//一级页面
		uni.switchTab({
			url: fn.value[index].path
		})
	}
</script>

<style scoped lang="less">
	.home {
		overflow: auto;
		height: 100%;
		background: #f7f7f7;
	}

	.home-bg {
		position: relative;
		width: 100%;
		height: 420rpx;
		display: flex;
		align-items: center;
		width: 100%;
		/*#ifdef H5*/
		  height: 340rpx;
		/*#endif*/
	
		//justify-content: center;

		.login-title {
				
			z-index: 99;
			color: #fff;
			//font-style: italic;
			padding: 0 36rpx;
			padding-top: 10rpx;
			letter-spacing: 1px;

			.login-title-txt {
				font-weight: bolder;
				font-size: 46rpx;
			}

			.login-title-txt-small {
				font-size: 24rpx;
				font-weight: 400;
			}
		}

		.home-bg-img {
			width: 100%;
			height: 100%;
			position: absolute;
			z-index: 0;
		}
	}

	.home-item {
		margin: 0px 30rpx 26rpx 30rpx;
		// height: 200px;
		background: #fff;
		z-index: 9999;
		position: relative;
		border-radius: 8px;
		box-shadow: 3px 5px 11px #f0f1f2a6;

	}

	.home-item-grid {
		padding: 30rpx 10rpx 10rpx 10rpx;
	}

	.home-item-first {
		margin-top: -140rpx;
		/*#ifdef H5*/
		margin-top: -90rpx;
		/*#endif*/
	}
</style>