
<template>
	
	<vol-view ref="viewRef" :table="table" :columns="columns" :table-data="tableData"
		:searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions" :editFormFields="editFormFields"
		:editFormOptions="editFormOptions" @searchClick="loadData" @addClick="modelOpenBefore" :saveBefore="saveBefore"
		:saveAfter="saveAfter" :delBefore="delBefore">
		<template #content>
			<vol-alert>页面默认自动生成内部实现自动无限滚动分页、查询，并可全定自定义配置页面样式</vol-alert>
		</template>
		<!--表格配置 -->
		<vol-table custom ref="tableRef" :url="tableUrl" @rowClick="modelOpenBefore" :loadBefore="searchBefore"
			:loadAfter="searchAfter" :direction="direction" :titleField="table.titleField" :columns="columns"
			:table-data="tableData">
			<!-- <template #data>
				<view class="fx-item fx" v-for="(row,index) in tableData" :key="index">
					<view>
						<image style="width: 140rpx;height: 140rpx;" :src='http.ipAddress+row.Img'></image>
					</view>
					<view class="fx-right fx-col">
						<view class="title"><text class="vol-tag vol-tag-error">自营</text> {{row.GoodsName}}</view>
						<view class="small-text">销售10W+ 95%好评</view>
						<view class="price fx">
							<text class="rmb">¥</text> {{row.Price}}
							<view class="cart" @click="addCart">
								<u-icon name="shopping-cart" color="#007aff" size="25"></u-icon>
							</view>
						</view>
					</view>
				</view>
			</template> -->
<!-- 			<view style="height: 90rpx;"></view> -->
		</vol-table>
	</vol-view>
<!-- 	<view class="goods-carts">
		<uni-goods-nav :options="optionLeft" :fill="true" :button-group="buttonGroup" @click="onClick"
			@buttonClick="buttonClick" />
	</view> -->
</template>
<script setup>
	import options from "./Demo_GoodsOptions.js";
	import {
		onLoad
	} from '@dcloudio/uni-app'
	import {
		defineComponent,
		ref,
		reactive,
		getCurrentInstance,
		defineEmits,
		defineExpose,
		defineProps,
		watch,
		nextTick
	} from "vue";

	const {
		proxy
	} = getCurrentInstance();

	//发起请求proxy.http.get/post
	//消息提示proxy.$toast()

	//表格显示方式:list=列表显示，horizontal=表格显示
	const direction = ref('list')

	//vol-view组件
	const viewRef = ref(null);
	//table组件
	const tableRef = ref(null);

	//表格数据，可以直接获取
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

	//查询前方法，可以设置默认值
	const searchBefore = (params) => {
		return true;
	}

	//查询后方法，res返回的查询结果
	const searchAfter = (res) => {
		nextTick(() => {
			viewRef.value.searchAfter(res);
		})
		return true;
	}

	//打开新建、编辑弹出框
	const modelOpenBefore = (row, index, obj, callback) => {
		//新建操作
		if (!row) {
			//这里可以设置默认值：editFormFields.字段=
			callback(true); //返回false，不会弹出框
			return;
		}
		//编辑
		viewRef.value.showEdit(row, index);
		//这里可以给弹出框字段设置或修改值：editFormFields.字段=
	}

	//新建、编辑保存前
	const saveBefore = (formData, isAdd, callback) => {
		callback(true); //返回false，不会保存
	}

	//新建、编辑保存后
	const saveAfter = (res, isAdd) => {

	}

	//删除前方法
	const delBefore = (ids, rows, result) => {
		return true; //返回false不会执行删除
	}

	//调用查询
	const loadData = (params) => {
		//生成查询条件
		params = params || viewRef.value.getSearchParameters();
		//params可以设置查询条件
		tableRef.value.load(params);
	}

	//如果是其他页面跳转过来的，获取页面跳转参数
	onLoad((ops) => {

	})

	const optionLeft = ref([{
		icon: 'chat',
		text: '客服'
	}, {
		icon: 'shop',
		text: '店铺',
		info: 1,
		infoBackgroundColor: '#007aff',
		infoColor: "#f5f5f5"
	}, {
		icon: 'cart',
		text: '购物车',
		info: 2
	}])
	const buttonGroup = ref([{
			text: '加入购物车',
			backgroundColor: 'linear-gradient(90deg, #FFCD1E, #FF8A18)',
			color: '#fff'
		},
		{
			text: '立即购买',
			backgroundColor: 'linear-gradient(90deg, #FE6035, #EF1224)',
			color: '#fff'
		}
	])
	
	const addCart=()=>{
		optionLeft.value[2].info=optionLeft.value[2].info+1;
	}

	const buttonClick = () => {

	}
	const onClick = () => {}

	//监听表单输入，做实时计算
	// watch(
	// 	() => editFormFields.字段,
	// 	(newValue, oldValue) => {

	// 	})
</script>
<style lang="less" scoped>
	.fx-item {
		margin: 20rpx;
		border-radius: 8rpx;

		.vol-tag-error {
			font-size: 22rpx !important;
			background: #ff0000;
			padding: 2rpx 6rpx;
			color: #fff;
			border-radius: 4rpx;
			margin-right: 6rpx;
			border: none;
		}

		.fx-right {
			margin-left: 20rpx;
		}

		.title {
			font-size: 28rpx;
			flex: 1;
		}

		.small-text {
			font-size: 22rpx;
			color: #828282;
		}

		.price {
			font-size: 32rpx;
			color: #ea0000;
			align-items: flex-end;

			.rmb {
				font-size: 26rpx;
				margin-right: 4rpx;
			}

			.cart {
				flex: 1;
				display: flex;
				justify-content: flex-end;
			}
		}
	}

	.goods-carts {
		/* #ifndef APP-NVUE */
		display: flex;
		/* #endif */
		flex-direction: column;
		position: fixed;
		left: 0;
		right: 0;
		/* #ifdef H5 */
		left: var(--window-left);
		right: var(--window-right);
		/* #endif */
		bottom: 0;
	}
</style>
