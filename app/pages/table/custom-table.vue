<template>
	<view class="table-demo">
		<view style="padding:20rpx 20rpx 0rpx 20rpx">
			<vol-alert type="primary">
				<view>框架内部已实现分页自动加载数据、table高度自适应，只需自定义表格内容</view>
			</vol-alert>
		</view>
		<view class="search">
			<u-search @search="loadData" placeholder="请输工序名称" v-model="processName" @custom="loadData" @clear="loadData"
				:showAction="true" clearabled actionText="搜索"></u-search>
		</view>
		<vol-table :height='0' 
			:table-data="tableData" :columns="columns">
			<template #data>
				<view class="fx-item" @click="rowClick(row)" :class="{'fx-item-activd':!!row.checked}"
					v-for="(row,index) in tableData" :key="index">
					<!-- 	表头 -->
					<view class="item-title">
						<view class="border-name"></view>
						<view style="flex: 1;">{{row.processCode}}</view>
						<view>{{row.submitTime}}</view>
						<view>
							<u-icon color="rgb(217 217 217)" name="arrow-right"></u-icon>
						</view>
					</view>
					<!-- 	内容部分 -->
					<view class="fx fx-item-row">
						<view class="fx-1 fx-row">
							<text class="fx-text">工序编号:</text>
							<view class="fx-value">{{row.processCode}}</view>
						</view>
						<view class="fx-1 fx-row">
							<text class="fx-text">工序:</text>
							<view class="fx-value">{{row.processName}}</view>
						</view>

					</view>
					<view class="fx fx-item-row">
						<view class="fx-1 fx-row">
							<text class="fx-text">物料名称:</text>
							<view class="fx-value">{{row.materialName}}</view>
						</view>
						<view class="fx-1 fx-row">
							<text class="fx-text">用料数量:</text>
							<view class="fx-value">{{row.materialAmount}}</view>
						</view>

					</view>
					<view class="fx fx-item-row">
						<view class="fx-1 fx-row">
							<text class="fx-text">提交人员:</text>
							<view class="fx-value">{{row.submitter}}</view>
						</view>
						<view class="fx-1 fx-row">
							<text class="fx-text">关键工序:</text>
							<view class="fx-value">
								<u-tag v-if="index%2" text="是" size="mini" type="error" plainFill plain></u-tag>
								<u-tag v-else text="否" size="mini" type="success" plainFill plain></u-tag>
							</view>
						</view>

					</view>
					<view class="fx fx-item-row">
						<view class="fx-1 fx-row">
							<text class="fx-text">提交时间:</text>
							<view class="fx-value">{{row.submitTime}}</view>
						</view>
					</view>
					<view style="margin-bottom: 20rpx;">
						<vol-alert type="primary">
							<view style="color: #3c9cff;">检查已派工未完成工作</view>
						</vol-alert>
					</view>
					<view class="fx">
						<view class="fx-1"></view>
						<view class="fx btns">
							<view class="btn">
								<vol-button size="small" type="default">查看</vol-button>
							</view>
							<view class="btn">
								<vol-button size="small" type="primary">完成</vol-button>
							</view>
						</view>
					</view>
				</view>
			</template>
		</vol-table>
	</view>
</template>

<script setup>
	import {
		ref,
		defineProps,
		defineEmits,
		defineExpose,
		computed,
		getCurrentInstance,
		nextTick
	} from 'vue'
	
	
	const processName=ref()
	
	const columns = ref([{
			"title": "工序编号",
			"field": "processCode",
			"width": 80
		},
		{
			"title": "工序名称",
			"field": "processName",
			"width": 85
		},
		{
			"title": "物料名称",
			"field": "materialName",
			"width": 80
		},
		{
			"title": "用料数量",
			"field": "materialAmount",
			"width": 80
		},
		{
			"title": "提交人员",
			"field": "submitter",
			"width": 80,
			hidden: false
		},
		{
			"title": "提交时间",
			"field": "submitTime",
			"width": 170,
			type: "date",
			hidden: false
		}
	]);
	const tableData = ref([{
			"processCode": "PR001",
			"processName": "插轴",
			"materialName": "螺丝",
			"materialAmount": 100,
			"submitter": "张三",
			"submitTime": "2022-01-15 08:20"
		},
		{
			"processCode": "PR002",
			"processName": "整平",
			"materialName": "电路板",
			"materialAmount": 50,
			"submitter": "李四",
			"submitTime": "2022-01-16 10:30"
		},
		{
			"processCode": "PR003",
			"processName": "下盖打码",
			"materialName": "包装盒",
			"materialAmount": 200,
			"submitter": "王五",
			"submitTime": "2022-01-17 14:45"
		},
		{
			"processCode": "PR004",
			"processName": "半成品组装",
			"materialName": "螺丝",
			"materialAmount": 150,
			"submitter": "赵六",
			"submitTime": "2022-01-18 09:20"
		},
		{
			"processCode": "PR005",
			"processName": "半成品清洗",
			"materialName": "外壳",
			"materialAmount": 80,
			"submitter": "钱七",
			"submitTime": "2022-01-19 11:30"
		},
		{
			"processCode": "PR006",
			"processName": "成品组装",
			"materialName": "主板",
			"materialAmount": 120,
			"submitter": "孙八",
			"submitTime": "2022-01-20 13:20"
		},
		{
			"processCode": "PR007",
			"processName": "动检测试",
			"materialName": "显示屏",
			"materialAmount": 90,
			"submitter": "周九",
			"submitTime": "2022-01-21 15:30"
		},
		{
			"processCode": "PR008",
			"processName": "外观检查",
			"materialName": "外壳",
			"materialAmount": 100,
			"submitter": "吴十",
			"submitTime": "2022-01-22 17:20"
		},
		{
			"processCode": "PR009",
			"processName": "包装",
			"materialName": "包装盒",
			"materialAmount": 200,
			"submitter": "李丽",
			"submitTime": "2022-01-23 08:20"
		},
		{
			"processCode": "PR010",
			"processName": "标签贴附",
			"materialName": "标签",
			"materialAmount": 300,
			"submitter": "陈丽",
			"submitTime": "2022-01-24 10:30"
		},
		{
			"processCode": "PR011",
			"processName": "包装箱装配",
			"materialName": "包装箱",
			"materialAmount": 150,
			"submitter": "王明",
			"submitTime": "2022-01-25 14:45"
		},
		{
			"processCode": "PR012",
			"processName": "打包",
			"materialName": "产品",
			"materialAmount": 250,
			"submitter": "李军",
			"submitTime": "2022-01-26 16:20"
		},
		{
			"processCode": "PR013",
			"processName": "运输",
			"materialName": "成品",
			"materialAmount": 400,
			"submitter": "赵敏",
			"submitTime": "2022-01-27 18:20"
		},
		{
			"processCode": "PR014",
			"processName": "入库",
			"materialName": "成品",
			"materialAmount": 400,
			"submitter": "孙涛",
			"submitTime": "2022-01-28 20:20"
		},
		{
			"processCode": "PR015",
			"processName": "出库",
			"materialName": "成品",
			"materialAmount": 400,
			"submitter": "刘芳",
			"submitTime": "2022-01-29 22:20"
		}
	]);

	const rowClick = (row) => {
		console.log('aaa')
		row.checked = !row.checked;
	}
	const loadData = () => {
		console.log('查询');	
	}
</script>

<style scoped lang="less">
	.table-demo {
		height: 100%;
		background: #fbfbfb;
		// padding: 20rpx;

		.fx-item {
			padding: 16rpx 20rpx;
			margin: 20rpx;
			background: #fff;
			border-radius: 10rpx;
			box-shadow: 3px 5px 11px #f0f1f2a6;

			.fx-row {
				font-size: 28rpx;
			}
		}

		.fx-item-activd {
			background: #eef6ffc7;
		}

		.fx-row {
			font-size: 28rpx !important;
		}

		.fx-item-row {
			padding: 16rpx;
		}
	}

	.item-title {
		font-weight: bold;
		padding: 3px 0 6px 0;
		border-bottom: 1px solid #fafafa;
		display: flex;
		line-height: 1;
		margin-bottom: 8rpx;

		.border-name {
			font-weight: bold;
			background: #007bff;
			padding-left: 8rpx;
			font-size: 16px;
			border-radius: 8rpx;
			margin-right: 8rpx;
		}
	}
	.search {
		padding: 10px;
		background: #fff;
	}
</style>
