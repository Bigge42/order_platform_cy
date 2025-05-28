
<template>
	<view class="vol-view" v-if="isLoad">
		<slot name="header"></slot>
		<view class="fx vol-view-header">
			<view class="header-total" v-if="showTotal">
				<!-- <text><共</text><text class="fx-header-row-total">{{rowTotal}}</text><text>条数据</text> -->
				<text>{{$tst('共[{$ts}]条数据',rowTotal)}}</text>
			</view>
			<view class="btns fx-1">
				<slot name="button"></slot>
				<view @click="btn.onClick" class="btn fx" v-for="(btn,index) in searchButtons" :key="index">
					<image v-if="btn.img" :src="btn.img" style="width: 24rpx;height: 24rpx;;margin-right: 6rpx;">
					</image>
					<view>{{$ts(btn.name)}}</view>
				</view>
			</view>
		</view>
		<view v-if="searchInputItem" class="vol-search-input-item fx">
			<view class="fx-1">
				<u-search @search="inputSearch" :placeholder="$tsArr(['请输入',searchInputItem.title])"
					v-model="searchFormFields[searchInputItem.field]" @custom="inputSearch" @clear="inputSearch"
					:showAction="true" clearabled :actionText="$ts('搜索')"></u-search>
			</view>
			<view style="display: flex;align-items: center;">
				<slot name="search-input"></slot>
			</view>
		</view>
		<slot name="content"></slot>
		<slot></slot>
	</view>
	<!-- @touchmove.prevent -->
	<u-popup :safeAreaInsetBottom="false" :zIndex="900" :show="searchModel" @close="searchModel=false">
		<view class="vol-action-sheet-select-container" :style="{'max-height':(maxHeight-50+'px')}">
			<view style="background: #FFFFFF" class="vol-action-sheet-select-title">
				{{$ts('搜索')}} 
				<view class="vol-action-sheet-select-title-right f-icon" @click="searchModel=false"> {{$ts('取消')}}
				</view>
			</view>
			<slot name="search-header"></slot>
			<view class="form-content vol-action-sheet-select-content">
				<vol-form ref="searchFormRef" :form-options="searchFormOptions" :formFields="searchFormFields">
				</vol-form>
				<view class="btns">
					<slot name="search-button"></slot>
					<view class="btn">
						<vol-button @click="resetSearch" type="default">{{$ts('重置')}}</vol-button>
					</view>
					<view class="btn">
						<vol-button @click="searchBtnClick" type="primary">{{$ts('查询')}}</vol-button>
					</view>
				</view>
			</view>
		</view>
	</u-popup>
	<!-- @touchmove.prevent -->
	<u-popup :safeAreaInsetBottom="false" :zIndex="900" :show="boxOptions.model" @close="boxOptions.model=false">
		<view class="vol-action-sheet-select-container" :style="{'max-height':(maxHeight+'px')}">
			<view class="vol-action-sheet-select-title">
				{{$ts(boxOptions.buttons.length?(currentAction=='Add'?'新增':'编辑'):'基本信息')}}
			</view>
			<slot name="modelHeader"></slot>
			<view class="vol-action-sheet-select-content">
				<vol-form @input-confirm="inputConfirm" :labelWidth="labelWidth" :load-key="false" @onChange="onChange"
					ref="editFormRef" @extraClick="extraClick" :form-options="editFormOptions"
					:formFields="editFormFields">
				</vol-form>
			</view>
			<slot name="modelBody"></slot>
			<view class="btns">
				<slot name="edit-button"></slot>
				<view v-show="!btn.hidden" class="btn" v-for="(btn,index) in boxOptions.buttons" :key="index">
					<vol-button @click="btn.onClick" :customStyle="{'border-radius': '6px'}" :type="btn.type">
						{{$ts(btn.name)}}
					</vol-button>
				</view>
				<!-- 	<view style="padding: 0 20rpx;width: 100%;" v-if="!buttons.length">
					<u-button @click="model=false" :customStyle="{'border-radius': '6px'}" type="primary"
						:text="$ts('关闭')"></u-button>
				</view> -->
			</view>
			<slot name="modelFooter"></slot>
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
		nextTick
	} from 'vue'
	import propsOptions from './props.js'
	const emit = defineEmits(['searchClick', 'addClick', 'extraClick', 'onChange', 'inputConfirm']);
	const props = defineProps(propsOptions())
	const {
		proxy
	} = getCurrentInstance();

	import {
		initAsync,
		reset
	} from './vol-view-provider.js'
	import search from '../../uni_modules/uview-plus/components/u-search/search.js';

	props.columns.forEach(x => {
		x.require = false;
	})

	const info = uni.getSystemInfoSync()
	const maxHeight = ref(info.screenHeight * 0.80);

	const tableTitleField = ref();
	if (!props.titleField) {
		tableTitleField.value = (props.columns.find(x => {
			return x.link
		}) || {}).field
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
	const searchInputItem = ref();
	if (props.showSearchInput) {
		searchInputItem.value = props.searchFormOptions.find(x => {
			return (x.type == 'like' || x.type == 'text' || (!x.type && (x.field)));
		})
	}

	const searchFormRef = ref(null)

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
	const hasEditPermission = ref(false);
	const searchModel = ref(false);
	const tableRef = ref()

	const searchButtons = computed(() => {
		return buttons.value.filter(x => {
			return !x.hidden
		})
	})


	const selectable = (row, columns, rowindex) => {
		return true;
	};

	const tableRowClick = () => {

	}
	const tableLoadBefore = (param) => {
		return true;
	}


	const pagination = ref({
		page: 1,
		rows: 30,
		order: '',
		sort: 'desc'
	});
	const isLoad = ref(false)

	const authFields = ref([]);
	initAsync(proxy, props, (res) => {
		isLoad.value = true;
		hasEditPermission.value = res.hasEditPermission;
		boxOptions.value.buttons = res.boxButtons;
		buttons.value.push(...res.buttons);
		permission.value = res.permission;
	});

	const getBottomButton = (row, rowIndex) => {
		if (!props.getBottomButton) {
			return []
		}
		return props.getBottomButton(row, rowIndex)
	}

	const searchBtnClick = (b) => {
		searchModel.value = false;
		emit('searchClick', getSearchParameters(), b);
	}
	proxy.search = () => {
		searchBtnClick(true);
	}
	const inputSearch = () => {
		searchBtnClick(true);
	}

	const searchAfter = (res) => {
		rowTotal.value = res.total || 0;
		return true;
	}

	proxy.showSearch = () => {
		searchModel.value = true;
	}
	const getSearchItem = (field) => { //获取查询的参数
		let data = props.searchFormOptions.find(x => {
			return x.field == field
		});
		return (data || {}).type
	}
	const getSearchParameters = () => { //获取查询参数
		let query = {
			wheres: []
		};
		for (const key in props.searchFormFields) {
			let value = props.searchFormFields[key];
			if (proxy.base.isEmpty(value)) continue;
			if (typeof value == "number") {
				value = value + "";
			}
			let displayType = getSearchItem(key);
			//联级只保留选中节点的最后一个值
			if (displayType == "cascader" && Array.isArray(value)) {
				//查询下面所有的子节点，如：选中的是父节点，应该查询下面所有的节点数据--待完
				value = value.length ? (value[value.length - 1] + "") : "";
			}
			if (
				typeof value == "string" || ["date", "datetime", "range"].indexOf(displayType) == -1
			) {
				query.wheres.push({
					name: key,
					value: typeof value == "string" ? (value + '').trim() : value.join(","),
					displayType: displayType
				});
				continue;
			}
			for (let index = 0; index < value.length; index++) {
				if (!proxy.base.isEmpty(value[index])) {
					query.wheres.push({
						name: key,
						value: (value[index] + '').trim(),
						displayType: (() => {
							if (["date", "datetime", "range"].indexOf(displayType) != -1) {
								return index ? "lessorequal" : "thanorequal";
							}
							return displayType;
						})()
					});
				}
			}
		}
		return query;
	}

	const resetSearch = () => {
		proxy.base.resetForm(props.searchFormFields, props.searchFormOptions);
		proxy.$toast(proxy.$ts('查询已重置'));
	}

	const resetForm = (row) => {
		proxy.base.resetForm(props.editFormFields, props.editFormOptions, row);
	}

	const del = (rows) => {
		if (!rows.length) {
			proxy.$toast(proxy.$ts('请选择要删除的行'));
			return;
		}
		let url = `api/${props.table.tableName}/del`;

		const keys = rows.map(x => {
			return x[props.table.key]
		})

		if (props.delBefore && !props.delBefore(keys, rows)) {
			return;
		}
		uni.showModal({
			title: proxy.$ts('提示'),
			content: proxy.$ts('确定要删除数据吗?'),
			success: (res) => {
				if (res.confirm) {
					proxy.http.post(url, keys, true).then(result => {
						proxy.$toast(result.message);
						if (!result.status) {
							return;
						}
						boxOptions.value.model = false;
						searchBtnClick(false)
						props.delAfter && props.delAfter(keys, rows, result);

					})
				}
			}
		})
	}

	proxy.delData = () => {
		del([currentRow]);
	}


	proxy.add = () => {
		currentRow = {};
		resetForm(null)
		emit('addClick', null, null, null, (res) => {
			if (res) {
				showEdit()
			}
		})
	}

	const showEdit = (row, index) => {
		currentRow = row;
		currentAction.value = !!row ? 'Update' : 'Add'
		if (row) {
			resetForm(row)
		}
		boxOptions.value.buttons.forEach(x => {
			if (x.value == 'del') {
				x.hidden = !row;
			}
		})
		boxOptions.value.model = true;
	}

	const editFormRef = ref();

	const getFormData = () => {
		let mainData = proxy.base.getFormValues(props.editFormFields, props.editFormOptions)
		mainData[props.table.key] = (currentRow || {})[props.table.key]
		return mainData;
	}

	proxy.save = () => {
		const b = editFormRef.value.validate();
		if (!b) {
			return;
		}
		let formData = {
			mainData: proxy.base.getFormValues(props.editFormFields, props.editFormOptions),
			detailData: null,
			delKeys: null
		};

		const isAdd = currentAction.value == 'Add';
		if (!isAdd) {
			formData.mainData[props.table.key] = currentRow[props.table.key]
		}
		if (props.saveBefore) {
			props.saveBefore(formData, isAdd, (res) => {
				if (!res) {
					return;
				}
				submitData(formData, isAdd)
			})
			return;
		}
		submitData(formData, isAdd)
	}

	const submitData = (formData, isAdd) => {
		let url = `api/${props.table.tableName}/${isAdd?'add':'update'}`;
		proxy.http.post(url, formData, true).then(result => {
			proxy.$toast(result.message);
			if (!result.status) {
				return;
			}
			searchBtnClick(false);
			boxOptions.value.model = false;
			props.saveAfter && props.saveAfter(result)
		})
	}

	proxy.$store.getters.data().viewSearch = searchBtnClick

	defineExpose({
		buttons,
		boxOptions,
		showEdit,
		searchAfter,
		getSearchParameters,
		del
	})
</script>
<style lang="scss" scoped>
	@import './vol-view.scss';
</style>
