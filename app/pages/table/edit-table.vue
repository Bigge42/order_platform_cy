<template>
	<view class="table-demo">
		<view style="padding:0">
			<vol-alert type="primary">
				<view>1、支持表格行内直接编辑，并支持分组编辑</view>
				<view>2、下拉框、日期等选择事件联动给其他字段设置值</view>
				<view>3、注意：大批量返回的数据不要使用行内编辑</view>
			</vol-alert>
		</view>
		<view class="search">
			<u-search @search="loadData" placeholder="请输商品名称" v-model="goodsName" @custom="loadData" @clear="loadData"
				:showAction="true" clearabled actionText="搜索"></u-search>
		</view>
		<view class="table-content">
			<vol-table :readonly="false" ref="tableRef" index url="api/Demo_Goods/getPageData" :height='0'
				direction="list" :loadBefore="loadBefore" :loadAfter="loadAfter" :columns="columns"
				:table-data="tableData" @onChange="onChange">
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
								<vol-button size="small" type="error"
									@click="delClick(scope.data.row,scope.data.rowIndex)">删除
								</vol-button>
							</view>
							<view class="btn">
								<vol-button size="small" type="default"
									@click="saveClick(scope.data.row,scope.data.rowIndex)">保存
								</vol-button>
							</view>
						</view>
					</view>
				</template>
				<view class="add-btn">
					<view class="btn">
						<vol-button type="primary" @click="addClick">添加数据</vol-button>
					</view>
				</view>

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

	//删除行
	const delClick = (row, index) => {
		uni.showModal({
			title: '提示',
			content: '确定要删除数据吗?',
			success: (res) => {
				if (res.confirm) {
					tableData.value.splice(index, 1)
					proxy.$toast('删除成功')
				}
			}
		});
	}
	//保存
	const saveClick = (row, index) => {
		console.log(row)
		proxy.$toast('保存成功')
	}

	//表格加载前设置条件,需要在vol-tables标签配置url才会生效
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
		//注意：如果需要编辑，请按下面配置转换数据格式
		res.rows.forEach(row => {
			//1、如果字段是图片，请将图片字符串调用getImg方法转换为框架要求的格式
			row.Img = proxy.base.getImg(row.Img, proxy.http)

			//如果是多选或者下拉框多选、级联，需要将值转换为数组格式
			//row.字段 = (row.字段||'').split(',')
		})
		return true;
	}

	//加载数据，只有vol-table配置了url此方法才生效
	const loadData = () => {
		//刷新表格
		tableRef.value.load()
	}

	//添加行数据
	const addClick = () => {
		//注意，如果有图上传或者下拉杠多选，这里需要给默认值数组
		tableData.value.push({
			Img: []
		})
		//也可以将数据添加到第一行
		//tableData.value.unshift({Img:[]})
	}

	const onChange = (field, value, row) => {
		proxy.$toast('选择了字段[' + (field) + '],值[' + value + ']')
		//这里可以做其他操作。给其他字段设置值：row.字段=
	}

	//表格数据
	const tableData = ref([]);
	//示例配置的url自动调用接口获取的数据，也可以手动调用接口给tableData设置值，
	// proxy.http.post(url,{参数},true).then(res=>{
	// 	  //注意返回的数据格式，如果有编辑图片、下拉框多选、级联，参照上面loadAfter方法说明转换数据格式
	// 	   tableData.value.splice(0);
	// 	   tableData.value.push(...res);
	// })


	//<vol-table :readonly="false">上面vol-table标签设置取消只读才可以编辑
	//如果某个字段不需要编辑，请将设置下readonly:true
	const columns = ref([{
			field: 'GoodsCode',
			required: true,
			title: '商品编号',
			readonly: false,
			type: "input"
		}, {
			field: 'GoodsName',
			title: '商品名称',
			//required: true,
			readonly: false,
			type: "input", // "textarea"
		},
		{
			field: 'CatalogId',
			title: '商品分类',
			type: "select",
			bind: {
				key: '分类级联',
				data: []
			}
		},
		{
			field: 'Price',
			//required: true,
			title: '商品单价',
			type: 'decimal',
			extra: {
				text: "元",
				style: "font-size:28rpx;color:#5b5c5c;margin-left:10rpx"
			}
		},
		{
			field: 'CreateDate',
			title: '出厂时间',
			type: 'date'
		},
		{
			field: 'Img',
			title: '商品图片',
			type: 'img',
			url: "api/Demo_Goods/upload",
			"multiple": true, //从图上传
			"maxCount": 3, //最多只能上传3张图片
		}
	]);
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

	.add-btn {
		height: 50px;
		position: relative;

		.btn {
			padding: 20rpx;
			width: 100%;
			background: #fff;
			position: fixed;
			box-sizing: border-box;
			bottom: 4rpx;
			z-index: 99;
		}
	}
</style>
