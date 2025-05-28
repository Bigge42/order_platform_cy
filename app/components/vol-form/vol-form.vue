
<template>
	<view>
		<u-toast ref="uToast"></u-toast>
		<view class="vol-form" :class="{'vol-form-left':labelPosition=='left'|| $global.form.textAlign=='left'}">
			<template class="f-form-item" v-for="(item,index) in getFormOptions" :key="index">
				<view class="f-form-item-group" v-if="Array.isArray(item)">
					<view style="flex: 1;" v-for="(item2,item2Index) in item" :key="item2Index">
						<vol-form-item v-show="!item2.hidden" :border="$global.form.border" :item="item2"
							:form-fields="formFields" :labelWidth="labelWidth"
							@showPicker="(dateIndex)=>{showPicker(item2,dateIndex)}"
							@extraClick="()=>{extraClick(item2,e)}" @showSelect="()=>{showSelect(item2)}"
							:labelPosition="labelPosition" @inputConfirm="(field)=>{inputConfirm(field,item2,e)}" :getStyle="getStyle"
							:fontSize="$global.form.fontSize" @chooseFile="()=>{chooseFileClick(item2)}">
						</vol-form-item>
					</view>
				</view>
				<vol-form-item :border="$global.form.border" v-else v-show="!item.hidden" :item="item"
					:form-fields="formFields" :labelWidth="labelWidth" :getStyle="getStyle"
					@showPicker="(dateIndex)=>{showPicker(item,dateIndex)}" @extraClick="()=>{extraClick(item)}"
					@afterRead="afterRead" @showSelect="()=>{showSelect(item)}" :labelPosition="labelPosition"
					@inputConfirm="(field,e)=>{inputConfirm(field,item,e)}" :fontSize="$global.form.fontSize"
					@chooseFile="()=>{chooseFileClick(item)}"></vol-form-item>
			</template>
		</view>
		<slot></slot>
		<!-- 日期组件 -->
		<template v-if="dateIsInit">
			<up-datetime-picker :hasInput="false" :minDate="pickerCurrentItem.min" :maxDate="pickerCurrentItem.max"
				:show="pickerModel" v-model="pickerValue"
				:mode="pickerCurrentItem.type=='month'?'year-month':pickerCurrentItem.type" closeOnClickOverlay
				@confirm="pickerConfirm" @cancel="pickerModel=false" @close="pickerModel=false"></up-datetime-picker>

		</template>
		<!-- 	下拉框组件 -->
		<view v-if="selectIsInit">
			<!-- 		@touchmove.prevent -->
			<u-popup class="form-popup" :zIndex="999999" :show="actionSheetModel" @close="actionSheetModel=false;">
				<view class="vol-action-sheet-select-container" :style="{'max-height':((popupHeight)+'px')}">
					<view class="vol-action-sheet-fx-header">
						<view class="vol-action-sheet-select-title">{{$tst('请选择',$ts(actionSheetCurrentItem.title))}}
							<view class="vol-action-sheet-select-btn">
								<text class="vol-action-sheet-select-reset"
									@click="actionResetClick">{{$ts('重置')}}</text>
								<text class="vol-action-sheet-select-confirm"
									@click="actionConfirmClick">{{$ts('确定')}}</text>
							</view>
						</view>
						<!-- 	超过10个下拉框选项默认开启搜索 -->
						<!-- 	 -->
						<view v-if="showFilter" class="vol-action-sheet-select-filter">
							<view class="fx-1" style="padding:10rpx">
								<u-search @custom="searchText=''" :placeholder="$ts('请输入关键字搜索')" :showAction="true"
									:actionText="$ts('清除')" :animation="false" v-model="searchText">
								</u-search>
							</view>
						</view>
					</view>
					<view class="vol-action-sheet-select-content">
						<view :class="{'vol-action-sheet-select-actived':actionSheetModel&&isActionSelected(item)}"
							@click="actionClick(item)"
							v-show="!item.hidden&&(!searchText||item.value.indexOf(searchText)!=-1)" :key="index"
							v-for="(item,index) in actionSheetCurrentItem.data" class="vol-action-sheet-select-item">
							{{$ts(item.value)}}
						</view>
					</view>
				</view>
			</u-popup>
		</view>
		<!-- 		树形级联 -->
		<view v-if="cascaderIsInit">
			<vol-tree ref="cascaderRef" :data="actionSascaderCurrentItem.data"
				:title="'请选择'+actionSascaderCurrentItem.title" :checkStrictly="actionSascaderCurrentItem.checkStrictly"
				@cancel="actionSascaderCurrentItem.cancel" @confirm="cascaderConfirm">
			</vol-tree>
		</view>
	</view>
</template>

<script setup>
	import {
		ref,
		defineProps,
		defineEmits,
		defineExpose,
		getCurrentInstance,
		nextTick,
		computed
	} from 'vue'
	import propsOptions from './props.js'
	const props = defineProps(propsOptions())

	import {
		initDataSource,
		uploadBefore,
		chooseFile
	} from './vol-form-provider.js'


	const afterRead = (option, event) => {
		uploadBefore(props.formFields, proxy.http, proxy.$store, option, event)
	}
	const chooseFileClick = (option) => {
		chooseFile(props.formFields, option, proxy)
	}

	const emit = defineEmits(['onChange', 'inputConfirm', 'extraClick', 'dicInited'])
	const {
		proxy
	} = getCurrentInstance()

	const showFilter = ref(false)
	const popupHeight = ref(0)

	const info = uni.getSystemInfoSync()

	popupHeight.value = info.screenHeight * 0.80;

	const pickerValue = ref(Date.now())
	const dateIsInit = ref(false)
	const pickerModel = ref(false) //日期组件
	const pickerCurrentItem = ref({}) //当前选项
	const pickerCurrentRangeIndex = ref(0)

	const searchText = ref('');
	const selectIsInit = ref(false);
	const actionSheetModel = ref(false);
	const cascaderIsInit = ref(false);
	const actionSascaderCurrentItem = ref({
		title: "",
		field: '',
		checkStrictly: false, //是否只能选择最后一个节点
		// cancel: () => {},//这里报错？
		// confirm: () => {} 
		data: []
	})
	const actionSheetCurrentItem = ref({
		min: 633715200000,
		max: 0
	}) //当前选项
	const actionSheetSelectValues = ref([]) //当前选中的项
	const numberModel = ref(false)
	const numberType = ref('number')
	const numberCurrentItem = ref({})
	const imgFields = ref([])

	const getFormOptions = computed(() => {
		return props.formOptions.filter(x => {
			return !x.hidden
		});
	})

	const isActionSelected = (item) => {
		let isSelect = actionSheetSelectValues.value.some(x => {
			return x + '' === item.key + ''
		});
		//formFields[item.field]
		return isSelect;
	}

	const cascaderConfirm = (value, item) => {
		props.formFields[actionSascaderCurrentItem.value.field] = value;
		emit("onChange", actionSascaderCurrentItem.value.field, value, item);
	}

	const radioOnChange = (value, item) => {
		emit("onChange", item.field, value, item, item.data);
	}

	const inputConfirm = (field,item, e) => {
		emit('inputConfirm', field,props.formFields[field],item, e);
	}
	const maxHeight = ref(0);
	uni.getSystemInfo({
		success: (res) => {
			maxHeight.value = res.screenHeight * 0.85;
		}
	});
	const cascaderRef = ref(null)

	const actionResetClick = () => {
		if (Array.isArray(actionSheetSelectValues.value)) {
			actionSheetSelectValues.value = []
		} else {
			actionSheetSelectValues.value = null;
		}
	}

	//下拉框、级联弹出选择框
	const showSelect = (item) => {
		selectIsInit.value = true;
		if (item.type == 'cascader') {
			cascaderIsInit.value = true;
			actionSascaderCurrentItem.value.field = item.field;

			//item.data这里没值时会显示上一个级联组件的data?
			actionSascaderCurrentItem.value.data = item.data
			actionSascaderCurrentItem.value.checkStrictly = item.checkStrictly || false; //是否只能选择最后一个节点

			nextTick(() => {
				cascaderRef.value.show(props.formFields[item.field], item.field);
			})
			//actionSascaderCurrentItem.cancel = item.cancel;
			//actionSascaderCurrentItem.confirm = item.confirm;
			return;
		}
		searchText.value = '';
		actionSheetSelectValues.value = [];
		actionSheetCurrentItem.value = item;
		var value = props.formFields[item.field];
		if (!proxy.base.isEmpty(value, true)) {
			if (Array.isArray(value)) {
				actionSheetSelectValues.value.push(...value.map(x => {
					return x;
				}));
			} else if (isMultiSelect()) {
				actionSheetSelectValues.value = value.split(',');
			} else {
				actionSheetSelectValues.value.push(value);
			}
		}
		showFilter.value = item.data.length > 15;
		let height = (item.data.length + 1 + (showFilter.value ? 1 : 0)) * 50;
		// popupHeight.value = height > maxHeight ? maxHeight : height;
		actionSheetModel.value = true;
	}

	const isMultiSelect = (item) => {
		var type;
		if (item) {
			type = item.type;
		} else {
			type = actionSheetCurrentItem.value.type
		}
		if (!type) {
			return false;
		}
		return ['checkbox', 'selectList','treeSelect'].indexOf(type) != -1;
	}
	const actionConfirmClick = () => {
		////多选
		if (isMultiSelect()) {
			actionSheetModel.value = false;
			//深复制原来的数据
			if (!Array.isArray(actionSheetSelectValues.value)) {
				actionSheetSelectValues.value = []
			}
			props.formFields[actionSheetCurrentItem.value.field] = actionSheetSelectValues.value.map(x => {
				return x
			});
			emit("onChange", actionSheetCurrentItem.value.field, props.formFields[actionSheetCurrentItem
				.value.field], actionSheetCurrentItem.value, actionSheetCurrentItem.data);
			return;
			//	return this.actionClick(item)
		}
		//单选
		props.formFields[actionSheetCurrentItem.value.field] = actionSheetSelectValues.value.join(',');
		actionSheetModel.value = false;
	}
	//下拉框选择事件
	const actionClick = (item) => {
		//多选
		if (isMultiSelect()) {
			//已经选中过的再次点取消选选中
			if (isActionSelected(item)) {
				actionSheetSelectValues.value = actionSheetSelectValues.value.filter(x => {
					return x + '' !== item.key + ''
				});
			} else {
				actionSheetSelectValues.value.push(item.key)
			}
			props.formFields[actionSheetCurrentItem.value.field] = [...actionSheetSelectValues.value];
			return;
		} else {
			props.formFields[actionSheetCurrentItem.value.field] = item.key;
		}
		actionSheetModel.value = false;
		emit("onChange", actionSheetCurrentItem.value.field, props.formFields[actionSheetCurrentItem
			.value.field], item, actionSheetCurrentItem.data);
	}

	const previewImage = (item, imgIndex) => {
		const imgs = getImgSrcs(item).map(x => {
			return x.url
		})
		uni.previewImage({
			urls: imgs,
			current: imgIndex
		})
	}
	const getImgSrcs = (item) => {
		let imgs = props.formFields[item.field];
		if (!imgs) {
			return []
		}
		if (Array.isArray(imgs)) {
			return imgs;
		}
		let imgArr = imgs.split(',');
		return imgArr.filter(x => {
			return x
		}).map(m => {
			return m;
		})
	}

	const getAllParentId = (id, data) => {
		if (id === null || id === '' || id === undefined) {
			return []
		}
		if (data.some((x) => {
				return typeof(x.id) == 'string'
			})) {
			id = id + '';
		} else {
			id = id * 1;
		}
		let ids = [id];

		for (let index = 0; index < ids.length; index++) {
			var node = data.find((x) => {
				return x.id === ids[index]
			});
			if (!node || (node.parentId === null && node.parentId === undefined)) {
				return ids;
			}
			if (data.some(x => {
					return x.id === node.parentId
				})) {
				ids.push(node.parentId);
			}
		}

		return ids.reverse();
	};
	const getCascaderNames = (value, item) => {
		let ids = getAllParentId(value, item.data);
		let names = [];
		for (let i = 0; i < ids.length; i++) {
			let obj = item.data.find(x => {
				return x.id === ids[i]
			});
			if (obj) {
				names.push(obj.value || obj.name)
			} else {
				names.push(ids[i])
			}
		}
		return names.join('/');
	}

	const formatDicValue = (item) => {
		let value = props.formFields[item.field];
		if (proxy.base.isEmpty(value)) {
			return '';
		}
		if (item.type == 'cascader') {
			return getCascaderNames(value, item);
		}
		if (isMultiSelect(item)) {
			return formatDicValueList(item);
		}
		let _kv = item.data.find(x => {
			return x.key + '' == value + ''
		});
		if (!_kv) {
			return proxy.$ts(value);
		}
		return proxy.$ts(_kv.value);
	}
	const formatDicValueList = (item) => { //多选
		var value = props.formFields[item.field];
		if (proxy.base.isEmpty(value)) {
			return '';
		}
		var _textArr = [];

		if (!(Array.isArray(value))) {
			value = (value + '').split(',')
		}
		value.forEach(x => {
			var obj = item.data.find(c => {
				return x + '' === c.key + '';
			});
			if (obj) {
				_textArr.push(obj.value);
			} else {
				_textArr.push(proxy.$ts(x));
			}
		})
		return _textArr.join(",");
	}
	const formatReadonlyValue = (item) => {
		if (item.data) {
			return formatDicValue(item);
		}
		if (item.type == 'date') {
			return (props.formFields[item.field] || '').substr(0, 10);
		}
		return props.formFields[item.field] || '';
	}

	//日期弹出框
	const showPicker = (item, index) => {
		dateIsInit.value = true;
		//console.log(item)
		if (item.min && typeof(item.min) == 'string') {
			item.min = new Date(item.min).valueOf();
		}
		if (item.max && typeof(item.max) == 'string') {
			// console.log(new Date(item.max).valueOf())
			item.max = new Date(item.max).valueOf();
		}
		pickerCurrentItem.value = item;
		let val = props.formFields[item.field];
		if (item.range) {
			pickerCurrentRangeIndex.value = index;
			if (!Array.isArray(val)) {
				props.formFields[item.field] = ['', ''];
				val = ['', ''];
			}
			val = val[index];
		}
		if (!val) {
			if (item.type == 'date') {
				val = proxy.base.getDate();
			} else if (item.type == 'month') {
				val = proxy.base.getDate().substr(0, 7) + '-01';
			} else {
				val = proxy.base.getDateTime().substring(0, 16)
			}
		}
		pickerValue.value = Number(new Date(val.replace(/-/g, "/")))
		pickerModel.value = true;
		//this.hideKeyboard();
	}

	const setPickerValue = (value) => {
		if (pickerCurrentItem.value.range) {
			props.formFields[pickerCurrentItem.value.field][pickerCurrentRangeIndex.value] = value
		} else {
			props.formFields[pickerCurrentItem.value.field] = value
		}
		emit("onChange", pickerCurrentItem.value.field, value);
	}
	const pickerConfirm = (e) => {
		pickerModel.value = false;
		// if (this.pickerCurrentItem.range && this.pickerCurrentRangeIndex == 1) {
		// 	//判断结束时间大于开始时间
		// }
		if (pickerCurrentItem.value.type == 'time') {
			setPickerValue(e.value)
			return;
		} else if (typeof e.value == 'number') {
			let timeFormat =
				pickerCurrentItem.value.type == 'time' ? 'hh:mm' :
				(pickerCurrentItem.value.type == 'date' ? 'yyyy-mm-dd' : 'yyyy-mm-dd hh:MM')
			setPickerValue(uni.$u.timeFormat(e.value, timeFormat))
		} else {
			setPickerValue(uni.$u.timeFormat(e.value))
		}
	};


	const extraClick = (item) => {
		emit('extraClick', item, props.formFields)
	}


	const initDicKeys = () => {
		initDataSource(props.formOptions, proxy.http, (dicData) => {
			emit('dicInited', dicData)
		});
	}

	const validate = () => {
		let _option = props.formOptions.filter(c => {
			return (c.require || c.required) && (!c.readonly && !c.disabled)
		}).find(x => {
			let val = props.formFields[x.field];
			if (Array.isArray(val)) {
				return !val.length
			} else {
				return (proxy.base.isEmpty(val))
			}
			return;
		});
		if (_option) {
			if (['date', 'datetime', 'month', 'checkbox', 'select', 'selectList', 'radio', 'switch'].indexOf(
					_option
					.type) != -
				1) {
				proxy.$toast(proxy.$tsArr(['请选择', _option.title]))
			} else {
				proxy.$toast(proxy.$tsArr([_option.title, '不能为空']))
			}
			return false;
		}
		return true;
	}

	const reset = (source) => {
		proxy.base.resetForm(props.formFields, props.formOptions, source)
	}
	const setReadonly = (readonly = true) => {
		props.formOptions.forEach(item => {
			if (Array.isArray(item)) {
				item.forEach(x => {
					x.readonly = readonly;
				})
			} else {
				item.readonly = readonly;
			}
		})
	}
	initDicKeys();



	defineExpose({
		initDicKeys,
		validate,
		reset,
		setReadonly
	})
</script>


<style lang="scss" scoped>
	@import './form.scss'
</style>