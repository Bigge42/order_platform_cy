<template>
	<view class="demo-container ">
		<view class="demo-pd-30">
			<up-notice-bar bgColor="#3a9bff1a" :speed="60" color="#007aff" text="框架大量组件都属于自研,可控制性高、易定制、功能更强、提供低配置高效率开发、组件文档及示例齐全。。。"></up-notice-bar>
		</view>
		<view class="demo-pd-30">
			<vol-alert type="primary">
				<view>表单支持编辑、数据自动绑定、数据字典自动绑定、点击事件、图片文件上传预览显示、表单分组显示,只需要简单配置即可实现90%以上常见功能</view>
			</vol-alert>
		</view>
		<view class="body">
			<view>
				<view v-for="item in list" :key="item.name" class="back-fff" @click="skipPage(item)">
					<view class="box-center">
						<image class="mar-rig-20" style="width: 70rpx; height: 70rpx;margin-right: 20rpx;"
							:src="item.icon"></image>
						<view class="fx-1">
							<view class="item-name">
								{{item.name}}
							</view>
							<view class="item-text">
								{{item.text}}
							</view>
						</view>
						<view>
							<u-icon color="rgb(217 217 217)" name="arrow-right"></u-icon>
						</view>
					</view>
				</view>
			</view>
		</view>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				list: [{
						name: '基础表单、只读、数据字典绑定',
						icon: '/static/icon/42.png',
						text: "输入框、下拉框单多选、日期、范围输入、级联、radio、图片文件上传预览等",
						url: '/pages/form/form1',
					},
					{
						name: '标签显示位置、标题宽度',
						text: "指定标签显示位置靠左或者右、指定标题宽度值",
						icon: '/static/icon/42.png',
						url: '/pages/form/position',
					},
					{
						name: '焦点、事件处理',
						text: "焦点、输入框、下拉框、日期、范围输入、级联、图片文件上传等选择及输入事件处理",
						icon: '/static/icon/42.png',
						url: '/pages/form/event',
					},
					{
						name: '表单字段实时计算',
						text: "监听表单输入或值变化时给其他字段设置值或计算值",
						icon: '/static/icon/42.png',
						url: '/pages/form/calc',
					},
					{
						name: '表单分组',
						text: "多个字段按分组显示在一行，减少页面幅度，并支持编辑功能",
						icon: '/static/icon/42.png',
						url: '/pages/form/group',
					},
					{
						name: '格式化处理、自定义按钮、扫码',
						text: "字段显示格式化、自定义颜色、自定义按钮、扫码等功能",
						icon: '/static/icon/42.png',
						url: '/pages/form/format',
					}
				],
			}
		},
		methods: {
			skipPage(item) {
				uni.navigateTo({
					url: item.url
				})
			},

		},
		onLoad(options) {}
	}
</script>

<style lang="less">
	.demo-container {
		height: 100%;
		//background: #f8f8f8;
		background: linear-gradient(to bottom, rgba(255, 255, 255, 0), rgb(255, 255, 255), rgb(255, 255, 255)), linear-gradient(to right, #FCE3E2, #F2E5E9, #DAE2EF, #DBE3F0);
		width: 100%;
		height: 100%;
		position: relative;

		.body {
			padding: 30rpx;
		}
	}

	.back-fff {
		border-radius: 16rpx;
		background-color: #fff;
		padding: 30rpx;
		box-shadow: 4px 5px 7px 4px #fafafa;
		margin-bottom: 40rpx;
	}

	.box-center {
		display: flex;
		align-items: center;
		justify-content: center;
	}

	.item-text {
		font-size: 24rpx;
		color: #6c6c6c;
		margin-top: 4rpx;
	}

	.demo-pd-30 {
		padding: 30rpx 30rpx 0rpx 30rpx
	}
</style>
