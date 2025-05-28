
<template>
	<vol-view ref="viewRef" :table="table" :columns="columns" :table-data="tableData"
		:searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions" :editFormFields="editFormFields"
		:editFormOptions="editFormOptions" @searchClick="loadData" @addClick="modelOpenBefore">

		<!--增加删除按钮 -->
		<template #button>
			<view class="fx-center" @click="()=>{}">
				<view style="position: relative;top:2rpx">
					<u-icon size="16" name="play-right"></u-icon>
				</view>
				按钮
			</view>
		</template>

		<template #search-input>
			<view class="fx-center" @click="scanClick">
				<image style="height: 60rpx;width: 60rpx;" src="/static/icon/27.png"></image>
			</view>
		</template>

		<!-- 筛选弹出框按钮 -->
		<template #search-header>
			<vol-alert>
				<view>筛选弹出框内容</view>
			</vol-alert>
		</template>
		<template #search-button>
			<view class="btn">
				<vol-button type="error">按钮
				</vol-button>
			</view>
		</template>

		<!--表格配置 -->
		<vol-alert>
			<view>生成页面增加自定义按钮并绑定事件</view>
			<view>这个位置可以自定义任意内容</view>
		</vol-alert>
		<vol-table ref="tableRef" :ck="false" :index="true" :url="tableUrl" @rowClick="modelOpenBefore"
			:loadBefore="searchBefore" :label-width="60" :loadAfter="searchAfter" :direction="direction"
			:titleField="table.titleField" :columns="columns" :table-data="tableData" labelPosition="left"
			@extraClick="extraClick">

			<!--行表头位置 -->
			<template #header="scope">
				<view class="scope-header">
					<view style="font-size: 26rpx;">
						<text>第[{{scope.data.rowIndex+1}}]行</text>
						<!-- <vol-button size="mini" type="primary"
							@click="rowBtnClick(scope.data.row,scope.data.rowIndex)">按钮第[{{scope.data.rowIndex+1}}]行
						</vol-button> -->
					</view>
				</view>
			</template>
			<!-- 行表尾位置 -->
			<template #bottom="scope">
				<view class="btns">
					<!--可以从scope.data.row取数据 -->
					<view class="fx-3 txt">这里自定义一段文本</view>
					<view class="btn">
						<vol-button size="small" type="default"
							@click="delBtnClick(scope.data.row,scope.data.rowIndex)">按钮
						</vol-button>
					</view>
					<view class="btn">
						<vol-button size="small" type="error"
							@click="delBtnClick(scope.data.row,scope.data.rowIndex)">删除
						</vol-button>
					</view>
					<view class="btn">
						<vol-button size="small" type="primary"
							@click="viewBtnClick(scope.data.row,scope.data.rowIndex)">查看
						</vol-button>
					</view>
				</view>
			</template>
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

	//注意上面的direction属性设置为list,上面vol-table标签加上label-width指定文字宽度

	//清空原有数据，重新手动配置格式
	columns.splice(0)
	columns.push(...[{
			field: 'OrderNo',
			title: '订单编号',
			type: 'string',
			link: true,
			width: 130,
			readonly: true
		},
		[{
				field: 'OrderType',
				title: '订单类型',
				type: 'int',
				bind: {
					key: '订单类型',
					data: []
				},
				width: 90
			},
			{
				field: 'TotalPrice',
				title: '订单价格',
				type: 'decimal',
				width: 70
			}
		],
		[{
				field: 'TotalQty',
				title: '订单数量',
				type: 'int',
				width: 80,
				extra: {
					text: "件",
					style: "font-size:26rpx;color:#0055ff;margin-left:10rpx"
				}
			},
			{
				field: 'OrderDate',
				title: '订单日期',
				type: 'date',
				width: 95
			}
		],
		{
			field: 'CreateDate',
			title: '创建时间',
			type: 'datetime',
			extra: {
				text: "按钮",
				button: true,
				type: "default",
				icon: "search",
				color: "#4e4d4d",
				size: 13
			}
		}
	])

	//查询前方法，可以设置查询条件(与生成页面文档上的searchBefore配置一致)
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
		//跳转到新页面编辑
		uni.navigateTo({
			url: "/pages/dbtest/Demo_Order/Demo_OrderEdit?id=" + ((row || {})[table.key] || ''),
			fail(e) {
				console.log(e)
			}
		})
	}
	//查看按钮
	const viewBtnClick = (row, rowIndex) => {
		modelOpenBefore(row);
	}
	//删除按钮
	const delBtnClick = (row, rowIndex) => {
		viewRef.value.del([row]);
	}

	//调用表格查询
	const loadData = (params) => {
		//生成查询条件
		params = params || viewRef.value.getSearchParameters();
		//params可以设置查询条件
		tableRef.value.load(params);
	}

	//自定义按钮点击
	const rowBtnClick = (row, index) => {
		proxy.$toast('点击了第[' + (index + 1) + ']行')
	}
	const extraClick = (row, column, index) => {
		proxy.$toast('点击了第[' + (index + 1) + ']行,字段[' + (column.title) + ']')
	}
	const scanClick = () => {
		uni.scanCode({
			success: (res) => {
				proxy.$toast(res.result)
			},
			fail: (e) => {
				console.log(e)
				proxy.$toast('扫码失败')
			}
		})
	}
	//如果是其他页面跳转过来的，获取页面跳转参数
	onLoad((ops) => {})

	defineExpose({
		//对外暴露数据
	})
</script>
<style lang="less" scoped>
	.btns {
		background: #fff;
		padding: 16rpx;
		align-items: center;

		.txt {
			font-size: 26rpx;
			color: #7e7e7e;
		}
	}

	.fx-center {
		font-size: 26rpx;
	}
</style>