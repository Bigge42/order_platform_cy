<template>
	<view class="demo-container">
	<view style="padding:26rpx 26rpx 0rpx 26rpx">
			<up-notice-bar bgColor="#3a9bff1a" :speed="60" color="#007aff" text="框架大量组件都属于自研,可控制性高、易定制、功能更强、提供低配置高效率开发、组件文档及示例齐全。。。"></up-notice-bar>
		</view>
		<view style="padding:26rpx 26rpx 0rpx 26rpx">
			<vol-alert type="primary">
				<view>框架已实现固定表头、左右滚动、分页、数据自动绑定、数据源自动绑定、格式化、行、单元格点击事件绑定、图片文件上传预览显示、及完全自定义等功能处理,只需要简单配置即可实现90%以上常见功能
				</view>
			</vol-alert>
		</view>
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
	const tabs = reactive([{
			name: '基础表格',
			path: "/pages/table/base-table",
			text: '自适应宽度,无滚动条,超出隐藏或换行显示'
		},
		{
			name: '可滚动表格',
			path: "/pages/table/scroll-table",
			text: '表格超出屏幕宽度时可滚动查看'
		},
		{
			name: '列表展示',
			path: "/pages/table/list-table",
			text: '表格以列表形式展示,适用于自定义事件处理'
		},
		{
			name: '自动分页、接口数据',
			path: "/pages/table/api-table",
			text: 'api自动分页获取接口数据加载及字典转换'
		},
		{
			name: '列表分组',
			path: "/pages/table/group-table",
			text: '每行可配置显示多个字段,减少页面显示幅度'
		},
		{
			name: '完全自定义列表',
			path: "/pages/table/custom-table",
			text: '只需处理数据展示;分页、查询等框架完成'
		},
		{
			name: '事件处理',
			path: "/pages/table/event-table",
			text: '行点击、单元格事件、分页、字典加载等'
		},
		{
			name: '高度设置',
			path: "/pages/table/height-table",
			text: '可设置固定高度、自适应高度及不固定高度'
		},
		{
			name: '格式化处理',
			path: "/pages/table/format-table",
			text: '数据格式化处理与实现方式,点开查看示例'
		},
		{
			name: '显示复选框、行号',
			path: "/pages/table/ck-table",
			text: '显示复选框、行号，获取选中的行、删除行'
		},
		{
			name: '自定义按钮',
			path: "/pages/table/btn-table",
			text: '自定义按钮及事件处理，点击查看'
		},
		{
			name: '行内编辑、下拉框事件',
			path: "/pages/table/edit-table",
			text: '行内编辑、下拉框、日期等选择事件'
		},
		// {
		// 	name: '弹出框编辑',
		// 	text: '行内编辑形式编辑,适用于数据量大的场景'
		// },
		// {
		// 	name: '表格搜索、排序',
		// 	text: '开启设置表格数据搜索与设置排序'
		// }
	])


	const itemClick = (item) => {
		uni.navigateTo({
			url: item.path
		})
	}

	defineExpose({
		tabs
	})
</script>

<style lang="less" scoped>
	.demo-container {
		height: 100%;
		//background: #f8f8f8;
		background: linear-gradient(to bottom, rgba(255, 255, 255, 0), rgb(255, 255, 255), rgb(255, 255, 255)), linear-gradient(to right, #FCE3E2, #F2E5E9, #DAE2EF, #DBE3F0);
	}

	.demo-list {
		padding: 26rpx;
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
		height: 150rpx;
		overflow: hidden;
		//box-shadow: 4px 5px 10px 1px #f7f7f7;
		box-shadow: 4px 5px 10px 1px #fafafa;
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
		margin-top: 10rpx;
		line-height: 1.4;
		letter-spacing: 1.2;
	}

	.item-name {
		display: flex;
		line-height: 1;
		font-weight: bolder;
		font-size: 26rpx;

		.border-name {
			font-weight: bold;
			//border: 4px solid #007bff;
			background: #007bff;
			padding-left: 6rpx;
			font-size: 16px;
			border-radius: 8rpx;
			margin-right: 8rpx;
		}
	}
</style>