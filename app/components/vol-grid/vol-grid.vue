<template>
	<view class="vol-grid" :style="{'grid-template-columns': 'repeat('+col+', auto)'}">

		<view @click="gridClick(item)" v-for="(item,index) in data" class="vol-grid-item"
			:style="{background: background,'text-align':align}" :key="index">
			<view><text class="vol-grid-value">{{getValue(item)}}</text><text class="vol-grid-unit">
					{{item.unit||''}}</text></view>
			<view class="vol-grid-title">
				{{item.title}}
			</view>
		</view>

	</view>
</template>

<script setup>
	import {
		ref,
		reactive,
		defineExpose,
		defineEmits,
		getCurrentInstance,
		defineProps,
		computed
	} from 'vue'

	const emit = defineEmits(['click'])
	const props = defineProps({
		data: {
			type: Array,
			default: () => {
				return []
			}
		},
		col: {
			type: Number,
			default: 3
		},
		background: {
			type: String,
			default: '#fff'
		},
		align:{
			type: String,
			default: 'left'
		}

	})
	const gridClick = (item) => {
		emit('click', item)
	}

	const getValue = (item) => {
		if (!item.value) {
			return 0
		}
		return item.value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
	}
</script>

<style scope lang="less">
	.vol-grid {
		padding: 20rpx; // 0rpx 0rpx 0rpx;
		padding-bottom: 0;
		box-sizing: border-box;
		// align-items: center;
		// margin: 20rpx;
		display: grid;
		column-gap: 20rpx;

		.vol-grid-item {
			display: flex;
			flex-direction: column;
			padding: 12rpx;
			justify-content: center;
			background: #f0faffa3;
			border-radius: 6rpx;
			margin-bottom: 20rpx;
			white-space: nowrap;
			overflow: hidden;
			text-overflow: ellipsis;
		}

		.vol-grid-value {
			font-weight: bolder;
			font-size: 36rpx;
		}

		.vol-grid-title {
			font-size: 26rpx;
			color: #848181;
			white-space: nowrap;
			overflow: hidden;
			text-overflow: ellipsis;
			margin-top: 6rpx;
		}

		.vol-grid-unit {
			font-size: 26rpx;
			margin-left: 8rpx;
			color: #848181;
		}

	}
</style>
