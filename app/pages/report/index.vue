<template>
	<view class="menu-container">
		<view class="demo-pd-30 menu-item">
			<vol-alert type="primary">
				<view class="text-item">当前为演示页面，如果需要权限控制，请在后台管理页面配置菜单并给用户角色分配权限</view>
			</vol-alert>
		</view>
		<view class="menu-item menu-item-data" v-for="(item,index) in menu">
			<vol-title :border="true" :title="item.name" :key="index"></vol-title>
			<view class="menu-item-grid">
				<u-grid :border="false" col="4">
					<u-grid-item v-for="(item2,index) in item.children" :key="index">
						<view @click="menuClick(item2)" class="menu-item-grid-content">
							<image style="width:80rpx;height: 80rpx;" :src="item2.icon"></image>
							<view class="grid-text">{{$ts(item2.name)}}</view>
						</view>
					</u-grid-item>
				</u-grid>
			</view>
		</view>
	</view>
</template>
<!-- 注意：因小程序包大小限制，需要分包处理，请将报表、图表统计页面全部创建在pagesCharts文件夹下,并在pages.json中subPackages配置页面地址， -->
<script setup>
	import {
		ref,
		reactive,
		computed,
		defineExpose,
		defineEmits,
		getCurrentInstance,
		defineProps
	} from 'vue'

   const {proxy}=getCurrentInstance()

	const menu = ref([{
		name: "报表统计",
		children: [{
			name: "销售统计",
			icon:"/static/icon/36.png",
			url: "/pagesCharts/report/order"
		},{
			name: "不良品数量",
			icon:"/static/icon/57.png",
			url: "/pagesCharts/report/defective"
		},{
			name: "生产看板",
			icon:"/static/icon/30.png",
			url: "/pagesCharts/report/production"
		}]
	}]);

	const menuClick = (item) => {
		if (!item.url) {
			proxy.$toast('未配置url');
			return
		}
		if (item.url[0] != '/') {
			item.url = '/' + item.url;
		}
		uni.navigateTo({
			url: item.url,
			fail: (err) => {
				proxy.$toast(`跳转异常:${JSON.stringify(err.errMsg)}`);
			}
		})
	}
</script>

<style lang="less" scoped>
	.menu-container {
		height: 100%;
		background-color: #f3f3f3ad;
		padding-top: 26rpx;
		padding-bottom: 100rpx;
	}

	.menu-item {
		margin: 26rpx;
		margin-top: 0;

	}

	.menu-item-data {
		box-shadow: 1px 1px 9px #f9f9f97d;
		background-color: #fff;
		padding: 20rpx;
	}

	.menu-item-grid {
		margin-top: 20rpx;
	}

	.menu-item-grid-content {
		display: flex;
		flex-direction: column;
		align-items: center;
	}

	.text-item {
		padding: 4rpx;
	}

	.item-icon-text {
		height: 60rpx;
		width: 60rpx;
		padding: 12rpx;

		.item-icon-text-bg {
			height: 100%;
			background: #eee;
			border-radius: 50%;
			text-align: center;
			justify-content: center;
			display: flex;
			align-items: center;
			background: #c3b3ff;
			font-size: 30rpx;
			color: #fff;
		}
	}
</style>
