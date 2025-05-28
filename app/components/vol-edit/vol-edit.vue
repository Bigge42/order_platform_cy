
<template>
	<view class="vol-edit" v-if="isLoad">
		<slot name="header"></slot>
		<view class="vol-edit-form">
			<vol-form @input-confirm="inputConfirm" :getStyle="getStyle" :labelWidth="labelWidth"
				:label-position="labelPosition" :load-key="false" @onChange="onChange" ref="editFormRef"
				@extraClick="extraClick" :form-options="editFormOptions" :formFields="editFormFields">
			</vol-form>
		</view>
		<view class="btns">
			<slot name="button"></slot>
			<view class="btn" v-if="state.del&&!isAdd">
				<vol-button type="default" @click="delClick">{{$ts('删除')}}</vol-button>
			</view>
			<view class="btn" v-if="state.save">
				<!-- 保存 -->
				<vol-button @click="saveClick" type="primary">{{$ts(saveName)}}</vol-button>
			</view>
		</view>
		<slot name="content"></slot>
		<slot></slot>
		<view class="vol-edit-tabs">
			<view class="fx vol-edit-table-title" :class="{'vol-edit-table-title-tabs':tabs.length>1}"
				v-if="tabs.length>0">
				<view class="fx-1">
					<view v-if="tabs.length>1" class="tabs">
						<u-tabs :itemStyle="{height:'66rpx'}" styl="hegith:70rpx" :current="currentTableIndex"
							@click="tabClick" :list="tabs"></u-tabs>
					</view>
					<view v-else>
						<vol-title :title="tabs[0].name"></vol-title>
					</view>
				</view>
				<view class="detail-btns">
					<view>
						<slot name="detail-button"></slot>
					</view>
					<view class="btn" @click="delRowClick" v-if="state.save">
						<image style="width: 30rpx;height:30rpx;margin-right: 6rpx;" src='./del.png'></image>
						{{$ts('删除')}}
					</view>
					<view class="btn" @click="addRowClick" v-if="state.save">
						<image style="width: 30rpx;height:30rpx;margin-right: 6rpx;" src='./add.png'></image>
						{{$ts('添加')}}
					</view>
				</view>
			</view>
			<view class="vol-edit-table">
				<!-- 	{{JSON.stringify(details)}} -->
				<view v-show="currentTableIndex===itemIndex" v-for="(item,itemIndex) in details" :key="itemIndex">
					<vol-table :minHeight="280" :content-padding='10' :ck="ck" :index="index"
						:ref="(el)=>{setTableRef(item,el)}" :url="item.url"
						@rowClick="(row, rowIndex,rows)=>{rowClick(row, rowIndex,rows,item)}"
						:loadBefore="(params)=>{return loadBefore(params,item,itemIndex)}"
						:loadAfter="(result)=>{return loadAfter(result,item,itemIndex)}" index direction="horizontal"
						:columns="item.columns" :table-data="item.tableData">
					</vol-table>
				</view>
			</view>
		</view>
	</view>
	<!-- @touchmove.prevent -->
	<u-popup :safeAreaInsetBottom="false" :zIndex="900" :show="boxOptions.model" @close="boxOptions.model=false">
		<view v-if="detailFormInited">
			<view class="vol-action-sheet-select-container" :style="{'max-height':(maxHeight+'px')}">
				<view class="vol-action-sheet-select-title">
					{{$ts(state.save?(currentAction=='Add'?'新增':'编辑'):'基本信息')}}
				</view>
				<slot name="modelHeader"></slot>
				<view class="vol-action-sheet-select-content">
					<vol-form :load-key="false" ref="detailFormRef" @extraClick="detailExtraClick"
						@input-confirm="detailInputConfirm" @onChange="detailOnChange"
						:form-options="details[currentTableIndex].edit.formOptions"
						:formFields="details[currentTableIndex].edit.formFields">
					</vol-form>
				</view>
				<view class="btns">
					<slot name="box-button"></slot>
					<view class="btn" v-if="state.save">
						<vol-button @click="detailConfirmClick" :customStyle="{'border-radius': '6px'}" type="primary">
							{{$ts('确定')}}
						</vol-button>
					</view>
				</view>
				<slot name="modelFooter"></slot>
			</view>
		</view>
	</u-popup>
</template>
<script setup>
	import {
		ref,
		defineProps,
		defineEmits,
		defineExpose,
		computed,
		getCurrentInstance,
		nextTick,
		reactive
	} from 'vue'
	import propsOptions from './props.js'
	const emit = defineEmits(['addClick', 'extraClick', 'detailExtraClick', 'onChange', 'detailOnChange',
		'detailInputConfirm', 'inputConfirm'
	]);
	const props = defineProps(propsOptions())
	const {
		proxy
	} = getCurrentInstance();

	import {
		initAsync,
		getFormData
	} from './vol-edit-provider.js'

	import {
		initDetail,
		addRow,
		confirmRow,
		editRow,
		delRow
	} from './vol-edit-detail.js'

	initDetail(props);
	// console.log(props.id)

	const id = ref(props.id);
	const isAdd = ref(!props.id);

	const info = uni.getSystemInfoSync()
	const maxHeight = ref(info.screenHeight * 0.80);

	const detailFormInited = ref(false);
	const currentTableIndex = ref(0);

	const tabClick = (item) => {
		currentTableIndex.value = item.index;
	}
	const tabs = ref(props.details.map((c, index) => {
		return {
			name: c.cnName,
			index: index
		}
	}))
	const tableRefs = ref([])

	const setTableRef = (item, el) => {
		tableRefs.value.push({
			table: item.table,
			ref: el
		});
	}

	const config = ref({
		tableRefs: tableRefs,
		currentTableIndex: currentTableIndex
	})

	proxy.config = config;

	const getTableRef = (table) => {
		if (!table) {
			table = props.details[currentTableIndex.value].table;
		}
		return tableRefs.value.find(x => {
			return x.table == table
		}).ref;
	}
	//明细表加载前方法
	const loadBefore = (params, item, index) => {
		if (!id.value) {
			return false;
		}
		if (props.details.length > 1) {
			params.tableName = item.table;
		}
		params.value = id.value;
		props.loadDetailBefore && props.loadDetailBefore(params, item, index);
		return true;
	}
	//明细表加载后方法
	const loadAfter = (result, item, index) => {
		props.loadDetailAfter && props.loadDetailAfter(result.rows, result, item, index);
		return true;
	}

	// const editTableInfo = ref({
	// 	table: "",
	// 	row: {},
	// 	formFields: {},
	// 	formOptions: []
	// })
	const delRowClick = () => {
		delRow(proxy, props, getTableRef())
	}
	//明细表点击行事件
	const rowClick = async (row, rowIndex, tableData, item) => {
		await editRow(proxy, props, {
			row,
			rowIndex,
			tableData,
			item
		});
		currentAction.value = 'Update';
		detailFormInited.value = true;
		boxOptions.value.model = true;
	}
	//添加行
	const addRowClick = async () => {
		await addRow(proxy, props);
		currentAction.value = 'Add'
		// editTableInfo.value = props.details[config.value.currentTableIndex]
		detailFormInited.value = true;
		boxOptions.value.model = true;
	}

	const detailFormRef = ref()
	const detailConfirmClick = () => {
		const b = detailFormRef.value.validate();
		if (!b) {
			return;
		}
		confirmRow(proxy, props);
		boxOptions.value.model = false;
	}

	const extraClick = (item) => {
		emit('extraClick', item)
	}
	const onChange = (field, value, item) => {
		emit('onChange', field, value, item)
	}
	const inputConfirm = (field, e) => {
		emit('inputConfirm', field, e)
	}
	//明细表
	const detailExtraClick = (item) => {
		emit('detailExtraClick',
			item,
			props.details[currentTableIndex.value].edit.formFields,
			props.details[currentTableIndex.value].edit.formOptions)
	}

	const detailOnChange = (field, value, item) => {
		emit('detailOnChange', field, value, item,
			props.details[currentTableIndex.value].edit.formFields,
			props.details[currentTableIndex.value].edit.formOptions)
	}
	const detailInputConfirm = (field, e) => {
		emit('detailInputConfirm', field, e,
			props.details[currentTableIndex.value].edit.formFields,
			props.details[currentTableIndex.value].edit.formOptions)
	}
	
	const tableUrl = ref('api/' + props.table.tableName + '/getPageData');
	const rowTotal = ref(0);
	const currentAction = ref('Add');
	let currentRow = {};
	const buttons = ref([]);
	const boxOptions = ref({
		model: false,
		buttons: []
	});
	const permission = ref([]);
	const tableRef = ref()

	const pagination = ref({
		page: 1,
		rows: 30,
		order: '',
		sort: 'desc'
	});
	const isLoad = ref(false)

	const state = ref({
		del: false,
		save: false,
		permission: {}
	});
	const navTitle = proxy.$ts(props.table.tableCNName)

	getFormData(props, proxy);
	// const authFields = ref([]);
	initAsync(proxy, props, (res) => {
		Object.assign(state.value, res);
		isLoad.value = true;
		if (res.save) {
			uni.setNavigationBarTitle({
				title: `${navTitle}(${proxy.$ts(props.id?'编辑':'新建')})`
			})
		}
		permission.value = res.permission;
	});


	const getBottomButton = (row, rowIndex) => {
		if (!props.getBottomButton) {
			return []
		}
		return props.getBottomButton(row, rowIndex)
	}
	const del = (rows) => {

	}

	const editFormRef = ref();

	const delExec = () => {
		let url = `api/${props.table.tableName}/del`;
		proxy.http.post(url, [id.value], true).then(result => {
			proxy.$toast(result.message);
			if (result.status) {
				proxy.$store.getters.data().viewSearch && proxy.$store.getters
					.data().viewSearch()
				setTimeout(() => {
					uni.navigateBack()
				}, 50)
			}
		})
	}
	const delClick = async () => {
		if (isAdd.value) {
			console.log('新建不能删除数据')
			return;
		}
		if (props.delBefore && !(await props.delBefore(id.value, props.editFormFields))) {
			return;
		}
		uni.showModal({
			title: proxy.$ts('提示'),
			content: proxy.$ts('确定要删除数据吗?'),
			success: (res) => {
				if (res.confirm) {
					delExec();
				}
			}
		})
	}
	const getSaveData = () => {
		if (!props.details.length) {
			return {};
		}
		if (props.details.length === 1) {
			return {
				detailData: props.details[0].tableData,
				delKeys: props.details[0].delKeys
			};
		}
		return {
			details: props.details.map((c) => {
				let itemDetail = {
					table: c.table,
					delKeys: c.delKeys,
					data: c.tableData
				}
				return itemDetail
			})
		}
	}

	const saveClick = async () => {
		const b = editFormRef.value.validate();
		if (!b) {
			return;
		}
		let formData = {
			mainData: proxy.base.getFormValues(props.editFormFields, props.editFormOptions),
		};

		Object.assign(formData, getSaveData());


		if (props.saveBefore) {
			await props.saveBefore(formData, isAdd.value, (res) => {
				if (!res) {
					return;
				}
				submitData(formData)
			})
			return;
		}
		submitData(formData)
	}

	const submitData = async (formData) => {
		let url = `api/${props.table.tableName}/${isAdd.value?'add':'update'}`;
		proxy.http.post(url, formData, true).then(result => {

			if (!result.status) {
				proxy.$toast(result.message);
				return;
			}
			props.saveAfter && props.saveAfter(result)
			proxy.$store.getters.data().viewSearch && proxy.$store.getters.data().viewSearch();
			//新建后变更为编辑状态
			if (props.editChange) {
				uni.setNavigationBarTitle({
					title: `${navTitle}(${proxy.$ts('编辑')})`
				})
				id.value = JSON.parse(result.data).data[props.table.key]
				props.editFormFields[props.table.key] = id.value;
				isAdd.value = false;
				getFormData(props, proxy)
			}
			//刷新明细表
			tableRefs.value.forEach(x => {
				x.ref.load(null, true)
			});
			proxy.$toast(result.message);
		})
	}

	const getTable = (table) => {
		return getTableRef(table);
	}

	defineExpose({
		buttons,
		boxOptions,
		del,
		getTable
	})
</script>
<style lang="scss" scoped>
	@import './vol-edit.scss';
</style>