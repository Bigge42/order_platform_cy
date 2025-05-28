<template>
	<view class="table-demo">
		<view style="padding:0">
			<vol-alert type="primary">
				<view>1、列表显示，表头、表尾自定义按钮或内容</view>
				<view>2、每行同时可以自定义按钮、背景颜色、字体颜色</view>
			</vol-alert>
		</view>
		<view class="search">
			<u-search @search="loadData" placeholder="请输商品名称" v-model="goodsName" @custom="loadData" @clear="loadData"
				:showAction="true" clearabled actionText="搜索"></u-search>
		</view>
		<view class="table-content">
			<vol-table ref="tableRef" index url="api/Demo_Goods/getPageData" :height='0' direction="list"
			 @rowClick="rowClick"	titleField="GoodsName" :table-data="tableData" :loadBefore="loadBefore" :loadAfter="loadAfter"
				:columns="columns" @extraClick="extraClick">
				<!-- 	头部自定义按钮部分 -->
				<template #header="scope">
					<view class="scope-header">
						<view style="font-size: 26rpx;">
							第[{{scope.data.rowIndex+1}}]行
							<!-- 	这里可显示按钮在顶部 -->
							<!-- 	<vol-button size="small" type="primary"
								@click="rowBtnClick(scope.data.row,scope.data.rowIndex)">按钮[{{scope.data.rowIndex+1}}]
							</vol-button> -->
						</view>
					</view>
				</template>
				<!--底部按钮位置 -->
				<template #bottom="scope">
					<view class="scope-header">
						<view class="fx-1" style="text-align: left;">￥<text
								class="scope-price">{{scope.data.row.Price}}</text></view>
						<view class="btns">
							<view class="btn">
								<vol-button size="small" type="default"
									@click="rowBtnClick(scope.data.row,scope.data.rowIndex)">取消
								</vol-button>
							</view>
							<view class="btn">
								<vol-button size="small" type="primary"
									@click="rowBtnClick(scope.data.row,scope.data.rowIndex)">收藏
								</vol-button>
							</view>
						</view>
					</view>
				</template>
		
			</vol-table>
		</view>
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
	} = getCurrentInstance();

	const goodsName = ref('')
	const tableRef = ref(null);

	//表格加载前设置条件
	const loadBefore = (params) => {
		//订单编号搜索
		params.wheres.push({
			name: "GoodsName",
			value: goodsName.value,
			displayType: 'like'
		})
		return true;
	}
	//表格加载后方法
	const loadAfter = (res) => {
		return true;
	}
	//tabs点击事件
	const tabClick = (item) => {
		orderType.value = item.value;
		loadData();
	}
	const loadData = () => {
		//刷新表格
		tableRef.value.load()
	}

	//自定义按钮点击
	const rowBtnClick = (row, index) => {
		proxy.$toast('点击了第[' + (index + 1) + ']行')
	}
	
	const rowClick=()=>{
		proxy.$toast('rowClick')
	}

	//extra按钮、文本点击事件
	const extraClick = (row, column, index) => {
		if (column.field == 'GoodsCode') {
			proxy.$toast('点击了第[' + (index + 1) + ']行,字段[' + (column.title) + '],[' + row.GoodsCode + ']')
		} else {
			proxy.$toast('点击了第[' + (index + 1) + ']行,字段[' + (column.title) + '],[' + row.Price + ']')
		}
	}



	const columns = ref([{
			field: 'GoodsId',
			title: '商品ID',
			hidden: true
		},
		{
			field: 'GoodsName',
			title: '商品名称'
		},
		{
			field: 'GoodsCode',
			title: '商品编号'
		},
		{
			field: 'CatalogId',
			title: '商品分类',
			valueStyle: "color:#007aff", //右边文本样式
			bind: {
				key: '分类级联',
				data: []
			},
			extra: {
				text: "按钮",
				button: true,
				type: "error",
				icon: "search",
				color: "#ffff",
				size: 13
			}
		},
		{
			field: 'Price',
			title: '商品单价',
			type: 'decimal',
			titleStyle: "color:#007aff", //左边文本样式
			valueStyle: "color:#f70202;font-weight:bolder", //右边文本样式
			extra: {
				text: "元",
				style: "font-size:28rpx;color:#5b5c5c;margin-left:10rpx"
			}
		},
		{
			field: 'CreateDate',
			title: '创建时间',
			type: 'datetime',
			itemStyle: "background:#fff9f0", //行样式
		},
		{
			field: 'Remark',
			title: '商品描述',
		},
		{
			field: 'Img',
			title: '商品图片',
			type: 'img'
		}
	]);
	const tableData = ref([]);
</script>

<style scoped lang="less">
	.table-demo {
		height: 100%;
		background: #fbfbfb;
	}

	.search {
		padding: 10px;
	}

	.scope-header {
		flex: 1;
		text-align: right;
		font-weight: 400;
		font-size: 26rpx;
		display: flex;
		align-items: center;
		justify-content: flex-end;

		.btns {
			margin-top: 12rpx;
			margin-bottom: 26rpx;
		}
	}

	.scope-price {
		font-weight: bolder;
		color: #df0000;
		font-size: 30rpx;
	}
</style>
