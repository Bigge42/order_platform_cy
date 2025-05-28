
<template>
	<view class="vol-table"
		:class="{'vol-table-border':$global.table.border,'vol-table-inline':textInline,'vol-table-full-column':fullColumn}">
		<scroll-view v-if="dataIsLoad" style="touch-action: pan-y" scroll-view scroll="1" :lower-threshold='40'
			@scrolltolower="scrolltolower" class="vol-table-scroll" :style="{height:tableHeight}" scroll-y="true"
			:scroll-x="!fullColumn" :show-scrollbar="false" :bounces="false" :enhanced="true">
			<view v-if="hasDataSlot">
				<slot name="data"></slot>
			</view>
			<!--          表格形式展示 -->
			<view v-else-if="direction=='horizontal'" class="vol-table-content" :style="{'width':tableWidth+'px'}">
				<view class="vol-table-header">
					<view v-if="index" class="vol-table-cell vol-table-cell-index cell" :style="getHeaderIndexStyle()">#
					</view>
					<view class="vol-table-cell vol-table-cell-ck cell" :style="getHeaderIndexStyle(true)" v-if="ck">
						<u-checkbox-group @change="checkAll">
							<u-checkbox :checked="checked" :size="12"></u-checkbox>
						</u-checkbox-group>
						<!-- <u-checkbox v-model="checked" :size="16"></u-checkbox> -->
					</view>
					<view v-for="(column, index) in tableColumns" :style="getHeaderStyle(column)" :key="index"
						class="vol-table-cell cell">{{column.title }}</view>
				</view>
				<view @click="rowClick(row,rowIndex)" :style="getRowStyle(row,rowIndex)"
					v-for="(row, rowIndex) in tableData" :key="rowIndex" class="vol-table-row row">
					<view :style="getHeaderIndexStyle()" v-if="index" class="vol-table-cell vol-table-cell-index cell">
						{{rowIndex+1}}
					</view>

					<view :style="getHeaderIndexStyle(true)" v-if="ck" class="vol-table-cell vol-table-cell-ck cell">
						<view v-if="getSelectable(row,columns,rowIndex)">
							<u-checkbox-group @change="()=>{ rowItemCheckClick(row,rowIndex)}">
								<u-checkbox :checked="row.ck" :size="12"></u-checkbox>
							</u-checkbox-group>
						</view>
						<view v-else>
							<u-checkbox-group>
								<u-checkbox :disabled="true" :size="12"></u-checkbox>
							</u-checkbox-group>
						</view>
					</view>
					<view :key="index" class="vol-table-cell" v-for="(column, index) in tableColumns"
						:style="getColumnStyle(row,column,rowIndex)">
						<view class="cell-1" v-if="column.type=='img'" @click.stop>
							<u--image @click="previewImage(column,index,row)" style="float:left;margin-left:5px;"
								width="80rpx" height="80rpx" radius="4px" :src="src"
								v-for="(src,index) in getImgSrcs(column,row)" :key="index">
							</u--image>
						</view>
						<view class="cell-1" v-else-if="column.click" :style="getCellPadding()"
							@click.stop="cellClick(row,column,rowIndex)">
							{{getValue(row,column,rowIndex)}}
						</view>
						<view class="cell-1" v-else-if="column.btn" :style="getCellPadding()"
							@click.stop="cellClick(row,column,rowIndex)">
							<vol-button v-for="(btn,btnIndex) in getButtons(row,rowIndex)" size="small" :type="btn.type"
								@click="()=>{btn.click(row,btn)}">{{btn.name}}</vol-button>
						</view>
						<view :style="getCellPadding()" v-else class="cell-1">
							{{getValue(row,column,rowIndex)}}
						</view>
					</view>
					<!-- 	<view v-for="(cell, cellIndex) in row" :key="cellIndex" :style="getColumnStyle(row,)" class="cell">{{ cell }}</view> -->
				</view>
			</view>
			<!-- 	表单列表形式显示 -->
			<view v-else :class="{'vol-form-left':labelPosition=='left'|| $global.form.textAlign=='left'}">
				<view @click="rowClick(row,rowIndex)" :key="rowIndex" v-for="(row,rowIndex) in tableData"
					class="vol-table-list">
					<view v-if="titleField" class="vol-table-list-item">
						<view class="vol-table-list-item-title fx">
							<view class="item-name fx-1">
								<text class="border-name"></text><text
									class="item-title-name">{{row[titleField]||''}}</text>
							</view>
							<view>
								<slot name="header" :data="{row,rowIndex}"></slot>
							</view>
						</view>
					</view>
					<view v-for="(column, index) in tableColumns" :key="index" class="vol-table-list-item">
						<!-- 	<view v-if="column.field==titleField" class="vol-table-list-item-title fx">
							<view class="item-name fx-1">
								<text class="border-name"></text><text
									class="item-title-name">{{row[column.field]||''}}</text>
							</view>
							<view>
								<slot name="header" :data="{row,rowIndex}"></slot>
							</view>
						</view> -->
						<view v-if="column.field==titleField">

						</view>
						<view class="f-form-item-group" v-else-if="Array.isArray(column)">
							<view style="flex: 1;" v-for="(column2,row2Index) in column" :key="row2Index">
								<vol-form-item :readonly="readonly" :item="column2" :form-fields="row"
									@showPicker="(dateIndex)=>{showPicker(row2,column,dateIndex,rowIndex)}"
									@extraClick="()=>{extraClick(row,column2,rowIndex)}"
									@showSelect="()=>{showSelect(row,column2,rowIndex)}" :labelWidth="labelWidth"
									:labelPosition="labelPosition" :getStyle="getStyle"
									@afterRead="(option,event)=>{afterRead(row,column,rowIndex,event)}"
									>
								</vol-form-item>
							</view>
						</view>
						<vol-form-item :getStyle="getStyle" :readonly="readonly" v-else :item="column"
							:form-fields="row" :labelWidth="labelWidth"
							@showPicker="(dateIndex)=>{showPicker(row,column,dateIndex,rowIndex)}"
							@extraClick="()=>{extraClick(row,column,rowIndex)}"
							@showSelect="()=>{showSelect(row,column,rowIndex)}" :labelPosition="labelPosition"
							@afterRead="(option,event)=>{afterRead(row,column,rowIndex,event)}"
							>
						</vol-form-item>
					</view>
					<view class="vol-table-list-bottom">
						<slot name="bottom" :data="{row,rowIndex}"></slot>
					</view>
				</view>
			</view>
			<view class="empty-data" v-if="tableData.length==0&&tableInited">
				<vol-empty></vol-empty>
			</view>
			<!-- 	<view style="padding-bottom: 10rpx;"></view>
 -->
			<slot></slot>
		</scroll-view>

		<!-- 日期组件 -->
		<template v-if="dateIsInit">
			<up-datetime-picker :hasInput="false" :minDate="pickerCurrentItem.min" :maxDate="pickerCurrentItem.max"
				:show="pickerModel" v-model="pickerValue"
				:mode="pickerCurrentItem.type=='month'?'year-month':pickerCurrentItem.type" closeOnClickOverlay
				@confirm="pickerConfirm" @cancel="pickerModel=false" @close="pickerModel=false"></up-datetime-picker>

		</template>
		<!-- 	下拉框组件@touchmove.prevent -->
		<view v-if="selectIsInit">
			<u-popup class="form-popup" :zIndex="999999" :show="actionSheetModel" @close="actionSheetModel=false;">
				<view class="vol-action-sheet-select-container" :style="{'max-height':(maxHeight+'px')}">
					<view class="vol-action-sheet-select-title vol-action-sheet-fx-header">
						{{$tst('请选择',$ts(actionSheetCurrentItem.title))}}
						<view class="vol-action-sheet-select-btn">
							<text class="vol-action-sheet-select-reset" @click="actionResetClick">{{$ts('重置')}}</text>
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
					<view class="vol-action-sheet-select-content">
						<view :class="{'vol-action-sheet-select-actived':actionSheetModel&&isActionSelected(item)}"
							@click="actionClick(item)"
							v-show="!item.hidden&&(!searchText||item.value.indexOf(searchText)!=-1)" :key="index"
							v-for="(item,index) in actionSheetCurrentItem.bind.data"
							class="vol-action-sheet-select-item">
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

		<u-overlay :opacity="0" :show="showOverlay" @click="showOverlay = false">
			<view class="loading-warp">
				<u-loading-icon text="正在加载..." textSize="16"></u-loading-icon>
			</view>
		</u-overlay>
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
		nextTick,
		onMounted,
		useSlots
	} from 'vue'
	import propsOptions from './props.js'
	const props = defineProps(propsOptions())
	const emit = defineEmits(['onChange', 'inputConfirm', 'extraClick', 'dicInited', 'cellClick', 'rowClick', 'ckChange',
		'ckChangeAll'
	])

	const extraClick = (item, row, index) => {
		emit('extraClick', item, row, index)
	}

	const {
		proxy
	} = getCurrentInstance()

	import {
		initDataSource,
		uploadBeofore
	} from './vol-table-provider.js'


	const slots = useSlots();
	const hasDataSlot = Object.keys(slots).includes('data');

	const tableInited = ref(true)
	if (props.url) {
		tableInited.value = false;
	}

	const itemHeight = 80;
	const scrollLeft = ref(0);
	const navBarHeight = ref();
	const info = uni.getSystemInfoSync()
	navBarHeight.value = info.system.toLowerCase().indexOf('ios') > -1 ? 44 : 46

	const maxHeight = ref(info.screenHeight * 0.85);


	const afterRead = (row,column,rowIndex,event) => {
		
		uploadBeofore(row, proxy.http, proxy.$store, column, event)
	}

	const showFilter = ref(false)
	const popupHeight = ref(0)
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

	const currentData = ref({
		column: {}, //当前点击的字段
		row: {}, //当前点击的行数据
		index: -1 //当前点击的行号
	})




	const isActionSelected = (item) => {
		let isSelect = actionSheetSelectValues.value.some(x => {
			return x + '' === item.key + ''
		});
		//formFields[item.field]
		return isSelect;
	}

	const cascaderConfirm = (value, item) => {
		currentData.value.row[actionSascaderCurrentItem.value.field] = value;
		emit("onChange", actionSascaderCurrentItem.value.field, value, item);
	}


	const headerTop = () => {
		// #ifdef H5
		return navBarHeight.value + 'px'
		// #endif
		// #ifdef MP-WEIXIN
		return '0px'
		// #endif
	}

	const fullColumn = ref(false);
	const tableWidth = ref(0);
	let lastColumnWidth = 0;

	//表格列
	const tableColumns = computed(() => {
		// console.log('computed')
		const _columns = props.columns.filter(x => {
			return !x.hidden; // && !x.fixed
		});
		if (props.direction == 'list') {
			return _columns;
		}
		let wd = 0;
		_columns.forEach(col => {
			wd += col.width || defaultWidth;
		})
		if (props.index) {
			wd += getPadding()
		}
		if (props.ck) {
			wd += 30;
		}
		if (props.index) {
			wd += 40;
		}
		tableWidth.value = wd;
		//不缩放x轴
		if (info.windowWidth >= wd) {
			fullColumn.value = true;
			tableWidth.value = info.windowWidth;
			//表格没有固定宽度了，宽度会失真，这里计算还不对
			lastColumnWidth = ~~((info.windowWidth - wd) / (_columns.length - (props.ck ? 0 : 0) - (props.index ?
				0 : 0)));

			// _columns.forEach(x => {
			// 	x.width = (x.width || defaultWidth) + lastColumnWidth + 1.2
			// })
			_columns.forEach(x => {
				x.width = (x.width || defaultWidth) + 1.2
			})
		} else {
			lastColumnWidth = 0
		}
		return _columns;
	})

	const topHeight = ref(0);
	const tableHeight = computed(() => {
		if (props.height == -1) {
			return 'auto';
		}
		//固定屏幕高度
		if (props.height) {
			return props.height + 'px'
		}
		// if (info.windowHeight - tableHeight.value <= 200) {
		// 	return (info.windowHeight) + 'px'
		// }
		// if (topHeight.value - topHeight.value <= 200) {
		// 	return '200px'
		// }
		if (props.minHeight && info.windowHeight - topHeight.value < props.minHeight) {
			return props.minHeight + 'px'
		}
		return (info.windowHeight - topHeight.value) + 'px'
	})

	const instance = getCurrentInstance();
	onMounted(() => {
		//console.log(instance)
		var view = uni.createSelectorQuery().in(instance).select(".vol-table");
		view.boundingClientRect().exec(res => {
			topHeight.value = res[0].top + 3 + props.contentPadding;
		})
	})

	const defaultWidth = 80;

	const getPadding = () => {
		return (props.padding || proxy.$global.table.padding);
	}
	const getHeaderIndexStyle = (isCk) => {
		let pd = getPadding();
		return Object.assign({
			width: '40px',
			textAlign: (props.textAlign || proxy.$global.table.textAlign),
			padding: `${pd+(4+(isCk?3:0))}px ${pd}px`
		})
	}
	const getHeaderStyle = (column, row) => {
		let pd = getPadding();
		return Object.assign({
			width: (column.width || defaultWidth) + lastColumnWidth + 'px',
			textAlign: (props.textAlign || proxy.$global.table.textAlign),
			padding: `${pd+4}px ${pd}px`
		})
	}

	const getRowStyle = (row, index) => {
		//row.voRowIndex = index;
		let oddStyle = {};
		if (index % 2 == 1) {
			oddStyle = {
				'background': proxy.$global.table.oddRowStyle
			}
		}
		if (!props.rowStyle) {
			return oddStyle;
		}
		return Object.assign(oddStyle, props.rowStyle(row, index, props.tableData))
	}
	const getCellPadding = () => {
		let pd = (props.padding || proxy.$global.table.padding);
		return {
			padding: `${pd+4}px ${pd}px`
		}
	}
	const getColumnStyle = (row, column, index) => {
		let pd = (props.padding || proxy.$global.table.padding);
		return Object.assign({
				'font-size': proxy.$global.table.fontSize
			},
			props.columnStyle && props.columnStyle(row, column, index,props.tableData), {
				width: (column.width || defaultWidth) + lastColumnWidth + 'px',
				textAlign: (props.textAlign || proxy.$global.table.textAlign)
			}, {
				padding: getCellPadding()
			})
	}

	// const getStyle = (item) => {
	// 	// return Object.assign({
	// 	// 	width: (item.width || defaultWidth) + 'px'
	// 	// }, item.style, props.getStyle(item))
	// }
	const getWidth = (value) => {
		return (value || defaultWidth) + 'px'
	}
	const scrollXFunc = async (e) => {
		scrollLeft.value = e.detail.scrollLeft
		console.log(scrollLeft.value)
	}
	const scrollToLeft = (e) => {
		if (e.detail.direction === 'left') {
			scrollLeft.value = 0
		}
	}
	const getSelectable = (row, column, rowindex) => {
		//复选框是否可以选中
		if (props.selectable) {
			return proxy.selectable(row, column, rowindex)
		}
		return true;
	}

	const checked = ref(false);
	const checkAll = () => {
		checked.value = !checked.value;
		props.tableData.forEach((x, rowindex) => {
			if (props.selectable) {
				if (proxy.selectable(x, null, rowindex)) {
					x.ck = checked.value;
				}
			} else {
				x.ck = checked.value;
			}
		})
		emit('ckChangeAll', props.tableData.filter(x => {
			return x.ck
		}), checked.value);
	}
	const getSelectedRows = () => {
		return props.tableData.filter(x => {
			return x.ck
		});
	}

	const clearSelectedRows = () => {
		props.tableData.forEach(x => {
			x.ck = false;
		});
		checked.value = false;
	}

	const rowItemCheckClick = (row, index) => {
		row.ck = !row.ck
		emit('ckChange', row, row.ck, index);
	}
	const getPageData = () => {

	}
	const refresh = () => {

	}

	const initDicKeys = () => {
		initDataSource(props.columns, proxy.http, (dicData) => {
			emit('dicInited', dicData)
		});
	}
	initDicKeys();


	const cascaderRef = ref(null)

	const actionResetClick = () => {
		if (Array.isArray(actionSheetSelectValues.value)) {
			actionSheetSelectValues.value = []
		} else {
			actionSheetSelectValues.value = null;
		}
	}

	//下拉框、级联弹出选择框
	const showSelect = (row, item, rowIndex) => {
		setCurrentData(row, item, rowIndex);
		selectIsInit.value = true;
		if (item.type == 'cascader') {
			cascaderIsInit.value = true;
			actionSascaderCurrentItem.value.field = item.field;

			//item.data这里没值时会显示上一个级联组件的data?
			actionSascaderCurrentItem.value.data = item.data
			actionSascaderCurrentItem.value.checkStrictly = item.checkStrictly || false; //是否只能选择最后一个节点

			nextTick(() => {
				cascaderRef.value.show(currentData.value.row[item.field], item.field);
			})
			//actionSascaderCurrentItem.cancel = item.cancel;
			//actionSascaderCurrentItem.confirm = item.confirm;
			return;
		}
		searchText.value = '';
		actionSheetSelectValues.value = [];
		actionSheetCurrentItem.value = item;
		var value = currentData.value.row[item.field];
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
		showFilter.value = item.bind.data.length > 15;
		let height = (item.bind.data.length + 1 + (showFilter.value ? 1 : 0)) * 50;
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
		return ['checkbox', 'selectList'].indexOf(type) != -1;
	}
	const actionConfirmClick = () => {
		////多选
		if (isMultiSelect()) {
			actionSheetModel.value = false;
			//深复制原来的数据
			if (!Array.isArray(actionSheetSelectValues.value)) {
				actionSheetSelectValues.value = []
			}
			currentData.value.row[actionSheetCurrentItem.value.field] = actionSheetSelectValues.value.map(x => {
				return x
			});
			emit("onChange", actionSheetCurrentItem.value.field, currentData.value.row[actionSheetCurrentItem
				.value.field], actionSheetCurrentItem.value);
			return;
			//	return this.actionClick(item)
		}
		//单选
		currentData.value.row[actionSheetCurrentItem.value.field] = actionSheetSelectValues.value.join(',');
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
			currentData.value.row[actionSheetCurrentItem.value.field] = [...actionSheetSelectValues.value];
			return;
		} else {
			currentData.value.row[actionSheetCurrentItem.value.field] = item.key;
		}
		actionSheetModel.value = false;
		emit("onChange", actionSheetCurrentItem.value.field, currentData.value.row[actionSheetCurrentItem
			.value.field], currentData.value.row);
	}

	const previewImage = (column, imgIndex, row) => {
		const imgs = getImgSrcs(column, row)
		// .map(x => {
		// 	return x;//.url
		// })
		uni.previewImage({
			urls: imgs,
			current: imgIndex
		})
	}
	const getImgSrcs = (column, row) => {
		let imgs = (row || currentData.value.row)[column.field];
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
			return (m.startsWith('http') ? '' : proxy.http.ipAddress) + m;
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
	const getValue = (row, column, index) => {
		if (column.format && props.format) {
			return props.format(row, column, index)
		}
		if (column.bind && column.bind.data) {
			return formatDicValue(column, column.field, row)
		}
		if (proxy.base.isEmpty(row[column.field])) {
			return '';
		}
		if (column.type == 'date') {
			return row[column.field].substr(0, 10)
		}
		return row[column.field];
	}

	const formatDicValue = (item, field, row) => {
		let value = row ? row[field] : currentData.value.row[item.field];
		if (proxy.base.isEmpty(value)) {
			return '';
		}
		if (item.type == 'cascader') {
			return getCascaderNames(value, item, row);
		}
		if (isMultiSelect(item)) {
			return formatDicValueList(item, row);
		}
		let _kv = item.bind.data.find(x => {
			return x.key + '' == value + ''
		});
		if (!_kv) {
			return proxy.$ts(value);
		}
		return proxy.$ts(_kv.value);
	}
	const formatDicValueList = (item, row) => { //多选
		let value = row ? row[field] : currentData.value.row[item.field];
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
			return (currentData.value.row[item.field] || '').substr(0, 10);
		}
		return currentData.value.row[item.field] || '';
	}

	const setPickerValue = (value) => {
		if (pickerCurrentItem.value.range) {
			currentData.value.row[pickerCurrentItem.value.field][pickerCurrentRangeIndex.value] = value
		} else {
			currentData.value.row[pickerCurrentItem.value.field] = value
		}
		emit("onChange", pickerCurrentItem.value.field, value, currentData.value.row);
	}
	//日期选择
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

	const setCurrentData = (row, item, rowIndex) => {
		currentData.value.column = item;
		currentData.value.row = row;
		currentData.value.index = rowIndex;
	}

	//日期弹出框
	const showPicker = (row, item, dateIndex, rowIndex) => {
		setCurrentData(row, item, rowIndex);
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
		let val = currentData.value.row[item.field];
		if (item.range) {
			pickerCurrentRangeIndex.value = index;
			if (!Array.isArray(val)) {
				currentData.value.row[item.field] = ['', ''];
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

	const pageOption = {
		page: 1,
		size: 30,
		loaded: false,
		sort: '',
		order: "",
		hasSummary: false,
		summary: []
	}

	const showOverlay = ref(false);

	const dataIsLoad = ref(true)
	//解决小程序发布后拉伸问题
	// #ifdef MP-WEIXIN
	if (props.url && props.defaultLoad && props.direction == 'horizontal') {
		dataIsLoad.value = false;
	}
	// #endif

	const load = (params, reset) => {
		if (!proxy.url) {
			return
		}
		let status = true;
		if (reset || reset === undefined) {
			reset = true;
			//this.rowsData.splice(0);
			pageOption.page = 1;
			pageOption.loaded = false;
		}
		if (!params) {
			params = {}
		}
		if (Array.isArray(params)) {
			params = {
				wheres: params
			}
		}
		params = Object.assign({
			page: pageOption.page,
			rows: pageOption.size || pageOption.rows,
			sort: pageOption.sort,
			order: pageOption.order || "desc",
			wheres: [], // 查询条件，格式为[{ name: "字段", value: "xx" }]
		}, params);

		if (props.loadBefore && !props.loadBefore(params)) {
			dataIsLoad.value = true;
			return;
		}

		checked.value = false;
		params.wheres = JSON.stringify(params.wheres);
		showOverlay.value = true;
		proxy.http.post(props.url, params, false).then(data => {
			tableInited.value = true;
			showOverlay.value = false;
			if (props.loadAfter && !props.loadAfter(data, params)) {
				return;
			}
			if (!data.rows) {
				data = {
					rows: data
				};
			}
			if (!data.rows.length || data.rows.length < params.rows) {
				pageOption.loaded = true;
			}
			if (reset) {
				proxy.tableData.splice(0);
			}
			proxy.tableData.push(...data.rows);
			if (!dataIsLoad.value) {
				dataIsLoad.value = true;
			}
		})
	}

	const getData = () => {
		if (!props.url) {
			return
		}
		if (!props.defaultLoad) {
			return
		};
		load();
	}
	getData();

	const scrolltolower = () => {
		// if (!proxy.url) {
		// 	this.$emit('scrolltolower');
		// }
		if (pageOption.loaded) {
			return;
		}
		pageOption.page = pageOption.page + 1;
		load(null, false)
	}

	const cellClick = (row, column, rowIndex) => {
		emit('cellClick', row, column, rowIndex, props.tableData)
	}
	const rowClick = (row, rowIndex) => {
		emit('rowClick', row, rowIndex, props.tableData)
	}
	const getButtons = (row, rowIndex) => {
		if (props.getButtons) {
			return props.getButtons(row, rowIndex)
		}
		return [];
	}
	const getSelectRowIndex = () => {

		let indexArr = props.tableData.filter(x => {
			return x.ck
		}).map((x) => {
			return x.volRowIndex
		})
		return indexArr || []
	}
	const delRows = () => {
		//let rows = this.getSelected()
		if (!props.tableData.some(x => {
				return x.ck
			})) {
			proxy.$toast('请选择要删除的行');
			return [];
		}
		const rows = [];
		for (let i = props.tableData.length - 1; i >= 0; i--) {
			if (props.tableData[i].ck) {
				let _row = props.tableData.splice(i, 1)
				rows.push(_row);
			}
		}
		return rows;
	}
	defineExpose({
		load,
		delRows,
		getSelectedRows,
		clearSelectedRows
	})
</script>

<style lang="scss" scoped>
	@import './table.scss';
	@import './table-list.scss';
	@import '../vol-form/form.scss';
</style>