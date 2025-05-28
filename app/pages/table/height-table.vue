<template>
	<view class="table-demo">
		<view style="padding:0">
			<vol-alert type="primary">
				<view>高度=0时默认为页面剩余可用高度(100%高度)</view>
				<view>高度=-1时，不固定高度，内容有多长就显示多高</view>
				<view>其他自定义高度，指定固定高度值。</view>
			</vol-alert>
		</view>
		<view class="btns">
			<view class="btn"><vol-button type="error" @click="btn1Click">固定高度</vol-button></view>
			<view class="btn"><vol-button type="primary" @click="btn2Click">100%高度</vol-button></view>
			<view class="btn"><vol-button type="default" @click="btn3Click">不固定高度</vol-button></view>
		</view>
		<vol-table ref="tableRef" :height="height" :textInline="false" direction="horizontal" :table-data="tableData"
			:columns="columns">
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

	const {
		proxy
	} = getCurrentInstance()
	const height = ref(0)
	const tableRef = ref();


	const btn1Click = () => {
		height.value = 200;
		proxy.$toast('固定高度为200')
	}

	const btn2Click = () => {
		height.value = 0;
		proxy.$toast('高度已设置为自适应，滚动屏幕看效果')
	}

	const btn3Click = () => {
		height.value = -1;
		proxy.$toast('高度设置成功，滚动屏幕看效果')
	}

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
		}
	]);


	const rowClick = (row) => {
		console.log('aaa')
		row.checked = !row.checked;
	}
</script>

<style scoped lang="less">
	.table-demo {
		// padding: 20rpx;
		height: 100%;
		background: #fff;
		overflow-y: auto;
	}

	.btns {
		padding: 20rpx;
	}
</style>