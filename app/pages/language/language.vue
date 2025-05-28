<template>
	<view class="lang">
		<view @click="itemClick(item)" class="item" v-for="(item,index) in lang" :key="index">
			{{$ts(item.text)}}
		</view>
	</view>
</template>

<script>
	import {$changeSource} from '@/translator/index.js'
	import tabBarChangeLanguage from '@/translator/tabBarChangeLanguage.js'
	export default {
		data() {
			return {
				lang: [{
						key: "zh-CN",
						text: "Chinese(简体中文)" //"Chinese" //"Chinese(中文)"
					}, {
						key: "en",
						text: "English(英语)", //"English" //"English(英语)"
					}
				]
			}
		},
		methods: {
			itemClick(item) {
				$changeSource(item.key);
				this.setTitle();

				//uni.navigateBack();
				let pages = getCurrentPages(); //获取所有页面栈实例列表
				let prevPage = pages[pages.length - 2]; //上一页页面实例

				if (prevPage && prevPage.route) {
					uni.reLaunch({
						url:'/'+ prevPage.route
					})
					return;
				}
				uni.navigateBack();
			},
			setTitle() {
				let val = this.$ts("语言设置");
				uni.setNavigationBarTitle({
					title: val
				})
			}
		},
		onShow() {
			this.setTitle()
		}
	}
</script>

<style scoped lang="less">
	.lang {
		padding: 10rpx 30rpx;

		.item {
			padding: 26rpx 0;
			border-bottom: 1px solid #eee;
		}
	}
</style>
