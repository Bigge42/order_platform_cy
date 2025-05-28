
<template>
	<vol-view ref="viewRef" :table="table" :columns="columns" :table-data="tableData"
		:searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions" :editFormFields="editFormFields"
		:editFormOptions="editFormOptions" @searchClick="loadData" @addClick="modelOpenBefore" :saveBefore="saveBefore"
		:saveAfter="saveAfter">
		<!--表格配置 -->
		<vol-alert>
			<view>页面生成后全自定义显示内容与事件处理(框架自动完成分页、查询、添加删除等功能)</view>
		</vol-alert>
		<vol-table ref="tableRef" :url="tableUrl" @rowClick="modelOpenBefore" :loadBefore="searchBefore"
			:loadAfter="searchAfter" :direction="direction" :titleField="table.titleField" :columns="columns"
			:table-data="tableData">
			<template #data>
				<view class="demo-item" :class="{active:row.ck}" v-for="(row,index) in tableData" :key="index">
					<view class="demo-item-left">
						<view class="title fx">
							<view class="fx-1 fx"><u-icon size="20" name="calendar"></u-icon>{{row.OrderNo}}</view>
							<view class="vol-tag vol-tag-primary" style="font-weight: 400;">{{getOrderType(row)}}</view>
						</view>
						<view class="demo-item-row">
							<text class="cell">价格：{{row.TotalPrice}}</text>
							<text class="cell">数量：{{row.TotalQty}}</text>
							<text class="cell">日期：{{(row.OrderDate||'').substr(0,7)}}</text>
						</view>
						<view class="demo-item-row">
							<text class="cell">客户：{{row.Customer}}</text>
							<text class="cell">提交人：{{row.Creator}}</text>
						</view>
						<view class="demo-item-row">
							<text class="cell">提交时间：{{row.CreateDate}}</text>
						</view>

						<!-- 	<view style="margin-top: 16rpx;">
							<vol-alert>
								<view>这里显示一些描述信息。。。</view>
							</vol-alert>
						</view> -->
						<view class="item-tag fx">
							<view class="fx-1">
								<text class="vol-tag vol-tag-primary">采购订单</text>
								<text class="vol-tag vol-tag-error">销售订单</text>
								<text v-if="index%2" class="vol-tag vol-tag-success">销售订单</text>
							</view>
							<view><u-icon name="share" size="18" color="#3c9cff"></u-icon></view>
						</view>
					</view>
					<view class="demo-item-icon" @click="row.ck=!row.ck">
						<u-icon :size="18" color="#007aff" name="checkmark-circle" v-if="row.ck"></u-icon>
						<u-icon :size="18" color="#c2c2c2" v-else name="more-circle"></u-icon>
					</view>
				</view>
			</template>
			<view class="bottom-height"></view>
			<view class="btns bottom-height">
				<view class="btn">
					<vol-button type="error" @click="delRow">删除选中行({{selectCount}})</vol-button>
				</view>
				<view class="btn">
					<vol-button type="primary" @click="getSelectRows">获取选中行({{selectCount}})</vol-button>
				</view>
			</view>
		</vol-table>
	</vol-view>
</template>
<script setup>
	import options from "./Demo_OrderOptions.js";
	import {
		onLoad
	} from '@dcloudio/uni-app'
	import {
		ref,
		reactive,
		getCurrentInstance,
		defineEmits,
		defineExpose,
		defineProps,
		watch,
		computed,
		nextTick
	} from "vue";
	const {
		proxy
	} = getCurrentInstance();
	//发起请求proxy.http.get/post
	//消息提示proxy.$toast()

	//表格显示方式:list=列表显示，horizontal=表格显示
	const direction = ref('horizontal')

	//vol-view组件
	const viewRef = ref(null);
	//table组件
	const tableRef = ref(null);

	//表格数据，可以直接获取使用
	const tableData = ref([]);

	//编辑、查询、表格配置
	//要对table注册事件、格式化、按钮等，看移动端文档上的table示例配置
	//表单配置看移动端文档上的表单示例配置，searchFormOptions查询配置，editFormOptions编辑配置
	const {
		table,
		searchFormFields,
		searchFormOptions,
		editFormFields,
		editFormOptions,
		columns
	} = reactive(options());
	const tableUrl = ref('api/' + table.tableName + '/getPageData');

	//tabs点击事件
	const tabClick = (item) => {
		orderType.value = item.value;
		loadData();
	}

	//查询前方法，可以设置查询条件(与生成页面文档上的searchBefore配置一致)
	const searchBefore = (params) => {
		return true;
	}

	//查询后方法，res返回的查询结果
	const searchAfter = (res) => {
		//设置默认不选中
		res.rows.forEach(x => {
			x.ck = false;
		})
		nextTick(() => {

			viewRef.value.searchAfter(res);
		})
		return true;
	}

	//打开新建、编辑弹出框
	const modelOpenBefore = (row, index, obj, callback) => {
		//跳转到新页面编辑
		uni.navigateTo({
			url: "/pages/dbtest/Demo_Order/Demo_OrderEdit?id=" + ((row || {})[table.key] || ''),
			fail(e) {
				console.log(e)
			}
		})
	}

	//新建、编辑保存前
	const saveBefore = (formData, isAdd, callback) => {
		callback(true); //返回false，不会保存
	}

	//新建、编辑保存后
	const saveAfter = (res, isAdd) => {}

	//调用表格查询
	const loadData = (params) => {
		//生成查询条件
		params = params || viewRef.value.getSearchParameters();
		//params可以设置查询条件
		tableRef.value.load(params);
	}
	
	//下面部分是自定义代码
	
	//手动转换数据源
	const getOrderType = (row) => {
		//row.OrderType
		let txt = row.OrderType;
		columns.forEach(col => {
			if (col.field == 'OrderType') {
				const item = col.bind.data.find(x => {
					return x.key == row.OrderType
				});
				if (item) {
					txt = item.value;
				}
			}
		})
		return txt;
	}

	//选中行的数量
	const selectCount = computed(() => {
		return tableData.value.filter(x => {
			return x.ck
		}).length;
	})

	const getSelectRows = () => {
		const rows = tableData.value.filter(x => {
			return x.ck
		});
		proxy.$toast(`选中的${rows.length}行数据`)
	}
	const delRow = () => {
		const rows = tableData.value.filter(x => {
			return x.ck
		});
		viewRef.value.del(rows);
	}
</script>
<style lang="less" scoped>
	.demo-item {
		padding: 20rpx;
		background: #fff;
		margin: 20rpx;
		border-radius: 10rpx;
		display: flex;

		.demo-item-left {
			flex: 1;
		}

		.demo-item-icon {
			display: flex;
			align-items: center;
		}

		.title {
			font-weight: bolder;
			font-size: 30rpx;
			color: #000;
			margin-bottom: 6rpx;
		}

		.demo-item-row {
			margin-top: 8rpx;

			.cell {
				font-size: 26rpx;
				color: #5a5a5a;
				margin-right: 30rpx;
			}
		}
	}

	.active {
		background: #edf5ff;
	}

	.bottom-height {
		height: 130rpx;
	}

	.btns {
		padding: 14rpx;
		padding-bottom: 40rpx;
		height: 130rpx;
		position: fixed;
		bottom: 0;
		box-sizing: border-box;
		z-index: 999;
		background: #fff;
		width: 100%;
	}

	.item-tag {
		align-items: center;
		padding: 14rpx 0;

		.vol-tag {
			margin-right: 14rpx;
		}
	}
</style>