<template>
	<view class="table-demo">
		<view class="title">
			<view class="title-text">我的审批</view>
			<view class="more" @click="moreClick">更多 ></view>
		</view>
		<vol-table custom :height='-1' :table-data="tableData" :columns="columns">
			<template #data>
				<view class="fx-item" @click="rowClick(row)" :class="{'fx-item-activd':!!row.checked}"
					v-for="(row,index) in tableData" :key="index">
					<!-- 	表头 -->
					<view class="item-title">
						<view class="border-name"></view>
						<view style="flex: 1;font-size: 30rpx;">{{row.WorkName}}</view>
						<view>
							<u-icon color="rgb(217 217 217)" name="arrow-right"></u-icon>
						</view>
					</view>
					<!-- 	内容部分 -->
					<view class="fx fx-item-row">
						<view class="fx-1 fx-row">
							<text class="fx-text">业务名称:</text>
							<view class="fx-value">{{row.WorkTableName}}</view>
						</view>
						<view class="fx-1 fx-row">
							<text class="fx-text">当前审批节点:</text>
							<view class="fx-value">{{row.CurrentStepId}}</view>
						</view>
					</view>
					<view class="fx fx-item-row">
						<view class="fx-1 fx-row">
							<text class="fx-text">审批状态:</text>
							<view class="fx-value">{{getAuditText(row.AuditStatus)}}</view>
						</view>
						<view class="fx-1 fx-row">
							<text class="fx-text">提交人员:</text>
							<view class="fx-value">{{row.Creator}}</view>
						</view>
					</view>
					<view class="fx fx-item-row">
						<view class="fx-2 fx-row">
							<text class="fx-text">提交时间:</text>
							<view class="fx-value">{{row.CreateDate}}</view>
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
	const columns = ref([{
		title: "审批状态", //用于绑定数据源
		field: "AuditStatus",
		bind: {
			data: [],
			key: "audit"
		}
	}]);
	const tableData = ref([]);

	const {
		proxy
	} = getCurrentInstance();
	const params = {
		page: 1,
		rows: 30,
		value: '0',
		order: "desc",
		sort: "CreateDate"
	};
	proxy.http.post('api/Sys_WorkFlowTable/getPageData', params, false).then(res => {
		tableData.value.splice(0);
		tableData.value.push(...res.rows);
	})

	//手动转换数据源
	const getAuditText = (auditStatus) => {
		const item = columns.value.find(x => {
			return x.field == 'AuditStatus'
		}).bind.data.find(c => {
			return c.key == auditStatus
		});
		if (item) {
			return item.value
		}
		return auditStatus;
	}

	const rowClick = (row) => {
		console.log('aaa')
		row.checked = !row.checked;
		uni.navigateTo({
			url: "/pages/flow/audit/audit?workTable=" + row.WorkTable + '&tableKey=' + row.WorkTableKey
		})
	}
	const moreClick = () => {

	}
</script>

<style scoped lang="less">
	.table-demo {
		padding: 0 6rpx;
		.fx-item {
			padding: 10rpx 20rpx;
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
			font-size: 26rpx !important;
		}
	}
	.fx-item-row{
		padding: 20rpx 0;
	}

	.item-title {
		font-weight: bold;
		padding: 16rpx 0 12rpx 0;
		border-bottom: 1px solid #f7f7f7;
		display: flex;
		line-height: 1;
		margin-bottom: 8rpx;

		.border-name {
			font-weight: bold;
			background: #007bff;
			padding-left: 8rpx;
			font-size: 28rpx;
			border-radius: 8rpx;
			margin-right: 8rpx;
		}
	}

	.title {
		padding: 0 26rpx;
		font-weight: bolder;
		display: flex;
		padding-bottom: 0rpx;

		.title-text {
			flex: 1;
			border-left: 8rpx solid #c35862;
			height: 30rpx;
			line-height: 30rpx;
			padding-left: 10rpx;
			font-size: 32rpx;
		}

		.more {
			color: #848484;
			font-weight: 500;
			font-size: 26rpx;
		}
	}
</style>
