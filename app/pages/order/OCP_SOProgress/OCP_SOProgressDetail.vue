<template>
	<uni-card>
		<view class='sale-detail-head-inner'>
			<view class="fx fx-item-row">
				<view class="fx-1 fx-row">
					<text class="fx-text">销售订单号:</text>
					<view class="fx-value">{{tableData.FBILLNO}}</view>
				</view>
				<view class="fx-1 fx-row">
					<text class="fx-text">用户合同号:</text>
					<view class="fx-value">{{tableData.F_BLN_YHHTH}}</view>
				</view>
			</view>
			<view class="fx fx-item-row">
				<view class="fx-1 fx-row">
					<text class="fx-text">销售合同号:</text>
					<view class="fx-value">{{tableData.F_BLN_CONTACTNONAME}}</view>
				</view>
				<view class="fx-1 fx-row">
					<text class="fx-text">销售负责人:</text>
					<view class="fx-value">{{tableData.XSYNAME}}</view>
				</view>
			</view>
			<view class="fx fx-item-row">
				<view class="fx-2 fx-row">
					<text class="fx-text">项目名称:</text>
					<view class="fx-value">{{tableData.F_ORA_XMMC}}</view>
				</view>
			</view>
			<view class="fx fx-item-row">
				<view class="fx-2 fx-row">
					<text class="fx-text">客户名称:</text>
					<view class="fx-value">{{tableData.FCUSTName}}</view>
				</view>
			</view>
			<view class="fx fx-item-row">
				<view class="fx-1 fx-row">
					<text class="fx-text">订货台数:</text>
					<view class="fx-value">{{tableData.FQTY}}</view>
				</view>
				<view class="fx-1 fx-row">
					<text class="fx-text">备货台数:</text>
					<view class="fx-value">{{tableData.FBJQTY}}</view>
				</view>
			</view>
			<view class="fx fx-item-row">
				<view class="fx-1 fx-row">
					<text class="fx-text">合同签订:</text>
					<view class="fx-value">{{formatDate(tableData.F_ORA_DATE4)}}</view>
				</view>
				<view class="fx-1 fx-row">
					<text class="fx-text">交货日期:</text>
					<view class="fx-value">{{formatDate(tableData.F_BLN_HFJHRQ)}}</view>
				</view>
			</view>
			<view class="fx fx-item-row">
				<view class="fx-2 fx-row">
					<text class="fx-text">排产日期:</text>
					<view class="fx-value">{{formatDate(tableData.F_ORA_DATE1)}}</view>
				</view>
			</view>
			<view class="fx fx-item-row">
				<view class="fx-1 fx-row">
					<text class="fx-text">入库数量:</text>
					<view class="fx-value">{{tableData.FRKREALQTY}}</view>
				</view>
				<view class="fx-1 fx-row">
					<text class="fx-text">超期数量:</text>
					<view class="fx-value">{{tableData.OverdueQty}}</view>
				</view>
			</view>
			<view class="fx fx-item-row">
				<view class="fx-1 fx-row">
					<text class="fx-text">待入库数量:</text>
					<view class="fx-value">{{tableData.FSQTY}}</view>
				</view>
				<view class="fx-1 fx-row">
					<text class="fx-text">出库数量:</text>
					<view class="fx-value">{{tableData.FCKREALQTY}}</view>
				</view>
			</view>
		</view>
	</uni-card>

	<!-- 状态信息显示 -->
	<view class="status-info-container">
		<view class="status-item">
			<text class="status-label">发货状态：</text>
			<text class="status-value">{{ shippingStatus }}</text>
		</view>
		<view class="status-item">
			<text class="status-label">逾期状态：</text>
			<text class="status-value">{{ overdueStatus }}</text>
		</view>
	</view>

	<!-- 流程图演示 -->
	<FlowProgress :steps="steps" :currentKey="currentKey" />
</template>
<script setup>
	import options from "./OCP_SOProgressOptions.js";
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
	import FlowProgress from '@/comp/flow/FlowProgress.vue'

	//发起请求proxy.http.get/post
	//消息提示proxy.$toast()

	const props = defineProps({
		BillNo: ''
	})
	const BillNo = ref(props.BillNo); //编辑的主键值
	const isAdd = !BillNo.value; //当前是新建还是编辑

	const labelWidth = ref(70);
	const labelPosition = ref('left')

	const {
		proxy
	} = getCurrentInstance();
	//发起请求proxy.http.get/post
	//消息提示proxy.$toast()

	//vol-table组件对象
	const tableRef = ref(null);

	//编辑、查询、表格配置
	const {
		table,
		editFormFields,
		editFormOptions,
		detail,
		details
	} = reactive(options());

	//表格数据
	const tableData = ref([]);

	//表格列配置（使用detail中的columns配置）
	const tableColumns = ref(detail.columns);

	//下拉框、日期、radio选择事件
	const onChange = (field, value, item, data) => {}

	//表单数据加载后方法
	const loadFormAfter = async (result) => {
		//isAdd通过判断是新还是编辑状态，可以页面加载后设置一些其他默认值(新建/编辑都可使用)
		//editFormFields.字段=值;

		// 页面加载后查询订单进度数据
		await loadOrderProgressData();
		// 单独加载流程图数据
		await loadProgressFlowData();
	}

	//新建、编辑保存前,isAdd判断当前是编辑还是新建操作
	const saveBefore = (formData, isAdd, callback) => {
		callback(true); //返回false，不会保存
	}

	//新建、编辑保存后
	const saveAfter = (res, isAdd) => {}

	//主表删除前方法
	const delBefore = async (id, fields) => {
		return true; //返回false不会执行删除
	}
	//明细表删除前
	const delRowBefore = async (rows, table, ops) => {
		// await proxy.http.post(url,{}).then(x => {
		// })
		return true
	}

	//明细表删除后前
	const delRowAfter = (rows, table, ops) => {}

	//如果是其他页面跳转过来的，获取页面跳转参数
	onLoad(async (ops) => {
		// 如果通过页面跳转传入了BillNo参数，也需要查询数据
		if (ops && ops.BillNo) {
			BillNo.value = ops.BillNo;
		}

		console.log(editFormFields);
		console.log(editFormOptions)

		// 页面加载时查询订单进度数据
		loadOrderProgressData();
		// 单独加载流程图数据
		loadProgressFlowData();
	})

	//监听表单输入，做实时计算
	// watch(
	// 	() => editFormFields.字段,
	// 	(newValue, oldValue) => {
	// 	})

	// 初始化流程步骤（key直接使用接口字段名）
	const steps = ref([{
			name: '',
			percent: 0,
			key: 'BOMStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'PlanConfirmStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'MaterialPrepStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'ProductionPushStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'MaterialCollectionStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'PreAssemblyStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'PartAssemblyStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'PressureTestStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'AccessoryInstallStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'FinalInspectionStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'PaintingStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'PackingStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'PackingInspectionStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'AssemblyCompleteStatus'
		},
		{
			name: '',
			percent: 0,
			key: 'OutboundStatus'
		},
	])
	const currentKey = ref('')

	// 状态信息
	const shippingStatus = ref('') // 发货状态
	const overdueStatus = ref('') // 逾期状态

	// 加载订单进度数据（表格数据，使用原始接口）
	const loadOrderProgressData = async () => {
		if (!BillNo.value) return;
		uni.showLoading({
			title: '加载中'
		})
		try {
			const response = await proxy.http.get(
				`api/MobileQuery/OrderProgress/QueryByBillNo?billNo=${BillNo.value}`);
			if (response.Status && response.Data) {
				// 将接口数据填充到表格
				fillTableData(response.Data[0]);
			}
			uni.hideLoading()
		} catch (error) {
			uni.hideLoading()
			console.error('查询订单进度失败:', error);
			proxy.$toast('查询订单进度失败');
		}
	}

	// 加载流程图数据（使用移动端接口）
	const loadProgressFlowData = async () => {
		if (!BillNo.value) return;
		uni.showLoading({
			title: '加载中'
		})
		try {
			const response = await proxy.http.post('api/MobileQuery/MobileOrderProgress/Query', {
				FBILLNO: BillNo.value
			});

			if (response.Status && response.Data) {
				// 更新流程进度
				updateProgressSteps(response.Data);

				console.log(steps.value);
			}
			uni.hideLoading();
		} catch (error) {
			uni.hideLoading();
			console.error('查询移动端订单进度失败:', error);
			proxy.$toast('查询移动端订单进度失败');
		}
	}

	// 将接口数据填充到表格中
	const fillTableData = (data) => {
		if (!data) return;

		// 直接设置表格数据
		tableData.value = data;
	}

	const formatDate = (value) => {
		if (value == null) return '';
		let date = new Date(value);
		var year = date.getFullYear();
		var month = (date.getMonth() + 1).toString().padStart(2, '0'); // 月份是从0开始的
		var day = date.getDate().toString().padStart(2, '0');
		return  year + '-' + month + '-' + day;
	}

	// 更新流程进度的方法（直接使用接口字段名）
	const updateProgressSteps = (data) => {
		if (!data) return;

		// 更新步骤数据
		steps.value.forEach(step => {
			// 直接使用key作为状态字段名，name已包含百分比
			step.name = data[step.key] || '';
			step.percent = data[step.key.replace('Status', 'Progress')] || 0;
		});

		// 更新状态信息
		shippingStatus.value = data.ShippingStatus || '';
		overdueStatus.value = data.OverdueStatus || '';
	}
</script>
<style lang="less">
	.sop-process-detail-table {
		height: 70vh !important;

		.vol-table-scroll {
			height: 100% !important;
		}
	}

	.status-info-container {
		display: flex;
		flex-direction: row;
		gap: 20px;
		padding: 15px 20px;
		background-color: #f5f5f5;
		border-radius: 8px;
		margin: 15px 0;

		.status-item {
			display: flex;
			align-items: center;

			.status-label {
				font-weight: 500;
				color: #666;
				margin-right: 8px;
			}

			.status-value {
				color: #333;
				font-weight: 400;
			}
		}
	}

	.field {
		width: 50%;
		display: inline;
	}

	.sale-detail-head {
		background-color: #f5f5f5;
		margin-bottom: 30rpm;
	}

	.sale-detail-head-inner {
		margin: 20rpx;
		background-color: #fff;
	}
</style>