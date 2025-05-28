
<template>
	<vol-view ref="viewRef" :table="table" :columns="columns" :table-data="tableData"
		:searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions" :editFormFields="editFormFields"
		:editFormOptions="editFormOptions" @searchClick="loadData" @addClick="modelOpenBefore">
		<!--表格配置 -->
		<vol-alert>生成页面配置字段分组显示,更多配置参照表格示例</vol-alert>
		<vol-table ref="tableRef" :ck="false" :index="true" :url="tableUrl" @rowClick="modelOpenBefore"
			:loadBefore="searchBefore" :label-width="60" :loadAfter="searchAfter" :direction="direction" :titleField="table.titleField"
			:columns="columns" :table-data="tableData" labelPosition="left">
				<template #bottom="scope">
					<view class="btns" style="margin-top: 20rpx;">
						<view class="fx-3"></view>
						<view class="btn">
							<vol-button size="small" type="default"
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
	columns.push(...[{field:'OrderNo',title:'订单编号',type:'string',link:true,width:130,readonly:true},
                       [{field:'OrderType',title:'订单类型',type:'int',bind:{ key:'订单类型',data:[]},width:90},
                       {field:'TotalPrice',title:'订单价格',type:'decimal',width:70}],
                       [{field:'TotalQty',title:'订单数量',type:'int',width:80},
                       {field:'OrderDate',title:'订单日期',type:'date',width:95}],
                       [{field:'Customer',title:'客户姓名',type:'string',width:80,readonly:true},
                       {field:'PhoneNo',title:'联系方式',type:'string',width:110,readonly:true}],
                       [{field:'OrderStatus',title:'订单状态',type:'int',bind:{ key:'订单状态',data:[]},width:90},
                       {field:'Creator',title:'创建人',type:'string',width:80}],
                       {field:'CreateDate',title:'创建时间',type:'datetime',align:'left'},
					   {field:'Remark',title:'备注',type:'textarea',align:'left'}])

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
	const viewBtnClick=(row,rowIndex)=>{
		modelOpenBefore(row);
	}
	//删除按钮
	const delBtnClick=(row,rowIndex)=>{
		viewRef.value.del([row]);
	}

	//调用表格查询
	const loadData = (params) => {
		//生成查询条件
		params = params || viewRef.value.getSearchParameters();
		//params可以设置查询条件
		tableRef.value.load(params);
	}

	//如果是其他页面跳转过来的，获取页面跳转参数
	onLoad((ops) => {})

	defineExpose({
		//对外暴露数据
	})
</script>
<style lang="less" scoped>
	.summary{
		padding: 20rpx 0;
		.txt{
			margin-left: 20rpx;
			font-size: 26rpx;
		}
	}
</style>