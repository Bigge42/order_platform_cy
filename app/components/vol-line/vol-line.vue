<template>
	<view class="vol-line-title" :class="{'vol-title-bottom':border}">

		<view v-for="(item,index) in lineData" class="vol-line-title-item">
			<view class="fx fx-title">
				<view class="fx-1">
					{{item.title}}
				</view>
				<view>{{item.value}} {{item.unit||''}}</view>
			</view>
			<u-line-progress height="12rpx" :showText="false" :percentage="item.rate" inactiveColor="#f9f9f9"
				:activeColor="item.color"></u-line-progress>
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

	const props = defineProps({
		data: {
			type: Array,
			default: () => {
				return []
			}
		},
		border:{
			type:Boolean,
			default:false
		}
	})

	const lineData = computed(() => {
		if (!props.data.some(x => {
				return !x.rate&& x.value > 100
			})) {
			return props.data;
		}
		
		let total = props.data.filter(x=>{return !x.rate}).reduce((v, current) => v + current.value, 0);

		return props.data.map(item => {
			return {
				title: item.title,
				value: item.value,
				unit:item.unit,
				color:item.color||'rgb(60, 156, 255)',
				rate:item.rate|| (((item.value / (total||1)) * 100).toFixed(2) * 1.0)
			}
		});
	})
</script>

<style scope lang="less">
	.vol-line-title {
        padding:10rpx 20rpx 20rpx 20rpx;
		align-items: center;

		.vol-line-title-item {
			display: flex;
			flex-direction: column;
			padding: 10rpx;
		}

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

	.vol-title-bottom {
		border-bottom: 1px solid #f7f7f7;
		padding-bottom: 6px;
	}

	.fx-title {
		    color: #4a4848;
		font-size: 28rpx;
		padding-bottom: 6rpx;
	}
</style>
