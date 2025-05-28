<template>
	<view class="empty-data">
		<image class="f-icon-img" src="./bg.svg" />
		<view>{{$ts('无数据')}}</view>
	</view>
</template>

<script>
</script>

<style scoped>
	.empty-data {
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: center;
		padding: 60rpx;
		color: #868686;
		font-size: 24rpx;
	}

	.f-icon-img {
		height: 40px;
		width: 40px;
	}
</style>
