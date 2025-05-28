
<template>
	<view :class="['vol-'+type]">
		<slot></slot>
	</view>
</template>

<script setup>
	import {
		defineProps
	} from 'vue';
	const props = defineProps({
		type: {
			type: String,
			default: "primary"
		}
	})
</script>

<style lang="less" scoped>
	.vol-primary {
		box-shadow: 1px 1px 9px #efefef;
		border: 1px solid #ddf0fb;
		background-color: #eef9ff;
		position: relative;
		border-radius: 4px;
		font-size: 26rpx;
		color: #212121;
		letter-spacing: 1px;
		line-height: 1.4;
		padding: 10rpx 16rpx;
		// margin: 16rpx 20rpx;
	}
</style>