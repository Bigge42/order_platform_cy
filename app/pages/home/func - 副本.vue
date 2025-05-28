<template>
	<view class="demo-list">
		<view class="list-item" :class="['list-item-'+(index+1)]" @click="itemClick(item, index)"
			v-for="(item, index) in tabs" :key="index">
			<view class="content">
				<view class="content-right">
					<view class="item-name">
						<text class="border-name"></text>{{ item.name }}
					</view>
					<view class="text">{{item.text}}</view>
				</view>
				<view class="f-icon">
					<image class="f-icon-img" src="/static/grid-bg.svg" />
				</view>
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
		getCurrentInstance
	} from 'vue'

	const emit = defineEmits(['parentCall'])

	const {
		proxy
	} = getCurrentInstance()

	//默认选中项
	const activeName = ref(-1)
	const tabs = reactive([
		// { name: '全部订单', icon: '', orderType: -1, count: 0, totalPrice: 0, qty: 0 },
		{
			name: '基础表单',
			path: "/pages/form/index",
			text: '简单配置即可快速构建移动端表单'
		},
		{
			name: '表格组件',
			path: "/pages/table/index",
			text: '编辑、表格固定、自动数据源等'
		},
		{
			name: '统计图表',
			text: '数据统计、图表查询显示'
		},
		{
			name: '其他组件',
			text: '其他一些常用功能及组件'
		}
	])


	const itemClick = (item) => {
		if (item.path) {
			uni.navigateTo({
				url: item.path
			})
			return;
		}
		uni.navigateTo({
			url: "/pages/table/index"
		})
	}

	defineExpose({
		tabs
	})
</script>

<style lang="less" scoped>
	.background {
		width: 100vw;
		height: 100vh;
		background: linear-gradient(135deg, rgba(0, 123, 255, 0.1), rgba(0, 123, 255, 0));
		position: relative;
		overflow: hidden;
	}

	.demo-list {
		padding: 26rpx;
		padding-top: 0;
		display: grid;
		-moz-column-gap: 20rpx;
		column-gap: 20rpx;
		grid-template-columns: repeat(2, auto);
	}

	.list-item {
		position: relative;
		cursor: pointer;
		display: flex;
		flex-direction: column;
		justify-content: center;
		background: #ffffff;
		border-radius: 5px;
		height: 170rpx;
		overflow: hidden;
		//box-shadow: 4px 5px 10px 1px #f7f7f7;
		box-shadow: 4px 5px 10px 1px #e5e4ff;
		margin-bottom: 20rpx;
		padding-top: 6rpx;

		.content {
			position: relative;
			display: flex;
			align-items: center;
			height: 100%;
			padding: 36rpx 26rpx;

			.content-right {
				color: #1d252f;

				.el-icon-warning-outline {
					margin-right: 5px;
				}
			}

			.name {
				font-size: 30rpx;
				color: #000;
				font-weight: bold;
				letter-spacing: 1px;
			}
		}

		.f-icon {
			position: absolute;
			right: 5px;
			top: 8px;
			z-index: 0;
			color: #f5f5f5;

			.f-icon-img {
				width: 68rpx;
				height: 68rpx;
			}
		}
	}

	.text {
		font-size: 26rpx;
		color: #837e7e;
		margin-top: 16rpx;
		line-height: 1.4;
		letter-spacing: 1.2;
	}

	.item-name {
		display: flex;
		line-height: 1;
		font-weight: bolder;
		font-size: 30rpx;

		.border-name {
			font-weight: bold;
			//border: 4px solid #007bff;
			background: #007bff;
			padding-left: 6rpx;
			font-size:  30rpx;
			border-radius: 8rpx;
			margin-right: 8rpx;
		}
	}

	.list-item-1 {
		background: linear-gradient(to top left, #81bbf7, #0073b6);

		//background: rgb(0, 115, 182);
		svg {
			fill: red;
		}

		.item-name,
			{
			color: #ffff;
		}

		.border-name {
			background: #fff;
		}

		.text {
			color: #e7e7e7;
		}

		//background-image: repeating-linear-gradient(45deg, #d8e5ff08, #d6f4fa8f);
	}

	.list-item-2 {
		// background: rgb(105, 102, 172);
		background: linear-gradient(to top left, #47a1ff, #6966ac);

		.item-name,
			{
			color: #ffff;
		}

		.border-name {
			background: #fff;
		}

		.text {
			color: #e7e7e7;
		}

	}

	.list-item-3 {
		background: linear-gradient(to top left, #3cdfa0, #00b695);

		// background: rgb(25, 190, 160);
		.item-name,
			{
			color: #ffff;
		}

		.border-name {
			background: #fff;
		}

		.text {
			color: #e7e7e7;
		}
	}

	.list-item-4 {
		background: rgb(48, 148, 209);

		.item-name,
			{
			color: #ffff;
		}

		.border-name {
			background: #fff;
		}

		.text {
			color: #e7e7e7;
		}

		//  background-image: repeating-linear-gradient(35deg, #dde8ff08, #fffaf3);
	}
</style>
