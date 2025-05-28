<template>
	<view class="menu-container">
		<view class="demo-pd-30 menu-item">
			<vol-alert type="primary">
				<view class="text-item">1.页面自动生成，支持菜单、按钮、数据、字段权限</view>
				<view class="text-item">2.页面支持零代码自动生成增删改查功能；支持主从表、一对多自动生成；支持组件扩展实现复杂操作; 支持扫码、定位、拍照等常用基础功能</view>
			</vol-alert>
		</view>
		<view class="menu-item menu-item-data" v-for="(item,index) in menuList" :key="index">
			<vol-title :border="true" :title="item.name"></vol-title>
			<view class="menu-item-grid">
				<u-grid :border="false" col="4">
					<u-grid-item v-for="(item2,index2) in getChildren(item.id)" :key="index2">
						<view @click="menuClick(item2)" class="menu-item-grid-content">
							<view>
								<view class="item-icon-text" v-if="checkIcon(item2)">
									<view class="item-icon-text-bg">
										{{item2.name.substr(0,1)}}
									</view>
								</view>
								<image @error="(event)=>{error(item2,event)}" v-else style="width:80rpx;height: 80rpx;"
									:src="getSrc(item2)"></image>
							</view>
							<view class="grid-text">{{$ts(item2.name)}}</view>
						</view>
					</u-grid-item>
				</u-grid>
			</view>
		</view>
	</view>
</template>
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


	const menu = ref([]);

	const {
		proxy
	} = getCurrentInstance();
	proxy.http.get("api/menu/getTreeMenu", {}, false).then(result => {
		menu.value.splice(0)
		menu.value.push(...result.menu ? result.menu : result);
		proxy.$store.commit("setPermission", menu.value);
		proxy.$store.commit("setAuthFields", result.authFields || []);
	})

	const menuList = computed(() => {
		return menu.value.filter(x => {
			return !x.parentId && (x.enable == 1 || x.enable === undefined)
		})
	})

	const getChildren = ((id) => {
		return menu.value.filter(x => {
			return x.parentId === id && (x.enable == 1 || x.enable === undefined)
		})
	})

	const getSrc = (item) => {
		if (item.isError) {
			return item.icon;
		}
		//console.log(item.icon)
		return proxy.http.ipAddress + item.icon
	}
	const checkIcon = (item) => {
		return (!item.icon || item.icon.indexOf('.') == -1);
	}
	const error = (item, event) => {
		item.isError = true;
		item.icon = "/static/error.png"
	}
	const menuClick = (item) => {
		if (!item.url) {
			// proxy.$toast('');
			return
		}
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
		width: 100%;
		//position: absolute;
		background-color: #f3f3f3ad;
		// padding-top: 26rpx;
		overflow: auto;
		/*#ifdef H5*/
		// padding-bottom: 100rpx;
		/*#endif*/
	}

	.menu-item {
		margin: 20rpx;
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
		text-indent: -28rpx;
		padding: 4rpx;
		padding-left: 34rpx;
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
			font-size: 26rpx;
		}
	}
	.demo-pd-30{
		margin-top: 20rpx;
	}
</style>
