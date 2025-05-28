<template>
	<view class="vol-title" :class="{'vol-title-bottom':border}">
		<view class="vol-title-boder">

		</view>
		<view class="vol-title-text" :style="{'font-size':size}">{{$ts(title)}}</view>
		<view class="vol-title-content">
			<slot></slot>
		</view>
		<view></view>
	</view>
</template>

<script setup>
	import {
		ref,
		reactive,
		defineExpose,
		defineEmits,
		getCurrentInstance,
		defineProps
	} from 'vue'

	const props = defineProps({
		title: {
			type: String,
			default: ""
		},
		size: {
			type: String || Number,
			default: "30rpx"
		},
		border: {
			type: Boolean,
			default: false
		}
	})
</script>

<style scope lang="less">
	.vol-title {
		display: flex;
		align-items: center;

		.vol-title-boder {
			width: 6rpx;
			background: #0091ff;
			height: 30rpx;
			border-radius: 4rpx;
			margin-right: 8rpx;
		}
		.vol-title-text {
			flex: 1;
			width: 0;
			font-weight: bolder;
		}
	}
	.vol-title-bottom{
		    border-bottom: 1px solid #f7f7f7;
		    padding-bottom: 6px;
	}
</style>
