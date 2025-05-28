
<template>
	<view class="vol-form-item" :style="item.itemStyle"
		:class="{'vol-form-item-label-top':item.labelPosition=='top','vol-form-item-label-bottom':item.labelPosition=='bottom', 'vol-form-item-group':item.type=='group','vol-form-item-textarea':item.type=='textarea','vol-form-item-border':border}">
		<view class="f-form-label" :style="getTitleStyle(item,false)" v-if="item.type!='group'&&item.title">
			<!-- 	<text class="f-form-label-required" v-if="item.require||item.required">*</text> -->
			<text :class="{'f-form-label-required':(item.require||item.required)&&!item.readonly}"
				:style="{'font-size':fontSize}">{{$ts(item.title)}}</text>
		</view>
		<view class="vol-form-item-right" :style="{'justify-content':item.align=='left'?'flex-start':''}"
			:class="{'f-form-group':item.type=='group'}">
			<view v-if="(item.type=='file'|| readonly||item.readonly||item.disabled)&&item.type!='group'"
				class="vol-form-item-readonly">
				<view v-if="item.type=='img'" class="readonly-imgs" @click.stop>
					<image style="width: 70px;height: 70px;margin-left: 20rpx;border-radius: 10rpx;"
						@click="previewImage(src,imgIndex)" v-for="(src,imgIndex) in getImgSrcs(item)" :key="imgIndex"
						:src="src.url"></image>
				</view>
				<view v-else-if="item.type=='file'" class="vol-files" @click.stop>
					<view @click="chooseFile" v-if="!(readonly||item.readonly||item.disabled)"
						class="vol-file-item fx vol-file-select">
						<view class="fx-1"></view>
						<view style="position: relative;top:2px;">
							<u-icon size="34rpx" color="#036adb" name="file-text"></u-icon>
						</view>
						<text>{{$ts('选择文件')}}</text>
					</view>
					<view class="vol-file-item fx" v-for="(src,imgIndex) in getImgSrcs(item)" :key="imgIndex">
						<view class="fx-1" :style="{'font-size':fontSize}" @click="previewFile(src,imgIndex)">{{getFileName(src.url)}}
						</view>
						<view v-if="!(readonly||item.readonly||item.disabled)" @click="deletePic(item,{index:imgIndex})"
							class="vol-file-item-icon">
							<u-icon name="trash"></u-icon>
						</view>
					</view>
				</view>
				<text :style="getItemStyle(item)" v-else class="vol-form-item-right-value">
					{{formatReadonlyValue(item)}}</text>
			</view>
			<view v-else-if="item.type=='editor'">
				<u-parse :content="formFields[item.field]"></u-parse>
			</view>

			<view class="f-form-content-select" @click="showCitySheet(item)" v-else-if="item.type=='city'">
				<view style="flex:1;">
					<view class="f-form-content-select-placeholder" :style="getPlaceholderStyle(item)"
						v-if="base.isEmpty(formFields[item.field],true)">
						{{$ts('请选择')}}
					</view>
					<view :style="getItemStyle(item)" :placeholder-style="getPlaceholderStyle(item)"
						class="vol-form-item-right-value" v-show="!base.isEmpty(formFields[item.field],true)">
						{{(formFields[item.field]||'').replace(/,/g,'/')}}
					</view>
				</view>
				<u-icon color="rgb(186 186 186)" size="15" name="arrow-right"></u-icon>
			</view>

			<template v-else-if="item.type=='date'||item.type=='datetime'||item.type=='time'||item.type=='month'">
				<template v-if="item.range">
					<view v-if="item.align!='left'" style="flex: 1;"
						:style="{'max-width': item.type=='date'?'120rpx':'30rpx'}"></view>
					<view class="f-form-content-select f-form-content-date" @click="showPicker(item,0)">
						<view class="f-form-content-select-placeholder" :style="getPlaceholderStyle(item)"
							v-if="base.isEmpty(formFields[item.field][0])">
							{{$ts('开始时间')}}
						</view>
						<view class="f-form-daterange-item">
							<view :style="getItemStyle(item)" :placeholder-style="getPlaceholderStyle(item)"
								class="vol-form-item-right-value">
								{{base.formatDate(formFields[item.field][0],item.type)}}
							</view>
						</view>
						<!-- <u-icon name="arrow-right"></u-icon> -->
					</view>
					<text style="margin:0 12rpx;">-</text>
					<view class="f-form-content-select f-form-content-date" @click="showPicker(item,1)">
						<view class="f-form-content-select-placeholder" :style="getPlaceholderStyle(item)"
							v-if="base.isEmpty(formFields[item.field][1])">
							{{$ts('结束时间')}}
						</view>
						<view v-if="formFields[item.field][1]" class="f-form-daterange-item">
							<view :style="getItemStyle(item)" :placeholder-style="getPlaceholderStyle(item)"
								class="vol-form-item-right-value">
								{{base.formatDate(formFields[item.field][0],item.type)}}
							</view>
						</view>
						<!-- <u-icon name="arrow-right"></u-icon> -->
					</view>
				</template>
				<view v-else class="f-form-content-select" @click="showPicker(item)">
					<view class="f-form-content-select-placeholder" :style="getPlaceholderStyle(item)"
						v-if="base.isEmpty(formFields[item.field])">
						{{$ts(item.placeholder|| '请选择')}}
					</view>
					<view :style="getItemStyle(item)" :placeholder-style="getPlaceholderStyle(item)"
						class="vol-form-item-right-value">
					{{base.formatDate(formFields[item.field],item.type)}}
					</view>
					<u-icon color="rgb(186 186 186)" size="15" name="arrow-right"></u-icon>
				</view>
			</template>
			<template v-else-if="item.range">
				<view style="flex: 1;max-width: 120rpx;"></view>
				<view class=" f-form-content-select f-form-range-value" style="text-align: left;">
					<view style="flex:1;">
						<view class="vol-form-item-right-value">
							<input type="number" :style="getItemStyle(item)"
								:placeholder-style="getPlaceholderStyle(item)" v-model="formFields[item.field][0]"
								border="none" :ref="item.field" :placeholder="$ts(item.placeholder||'请输入')" />
						</view>
					</view>
				</view>
				<text style="margin:0 0rpx;">-</text>
				<view class="f-form-content-select f-form-range-value">
					<input type="number" :style="getItemStyle(item)" :placeholder-style="getPlaceholderStyle(item)"
						v-model="formFields[item.field][1]" border="none" :ref="item.field" :focus="item.focus"
						:placeholder="$ts(item.placeholder||'请输入')" />
				</view>
			</template>
			<view class="f-form-content-select" @click="showSelect(item)"
				v-else-if="['select','selectList','checkbox','cascader','treeSelect'].indexOf(item.type)!=-1">
				<view style="flex:1;">
					<view class="f-form-content-select-placeholder" :style="getPlaceholderStyle(item)"
						v-if="base.isEmpty(formFields[item.field],true)">
						{{$ts(item.placeholder|| '请选择')}}
					</view>
					<view :style="getItemStyle(item)" :placeholder-style="getPlaceholderStyle(item)"
						class="vol-form-item-right-value" v-show="!base.isEmpty(formFields[item.field],true)">
						{{formatDicValue(item)}}
					</view>
				</view>
				<u-icon color="rgb(186 186 186)" size="15" name="arrow-right"></u-icon>
			</view>
			<view class="f-form-group-content" :style="item.style" v-else-if="item.type=='group'">
				{{$ts(item.title)||''}}
			</view>

			<view v-else-if="item.type=='number'">
				<input :style="getItemStyle(item)" :placeholder-style="getPlaceholderStyle(item)" :focus="item.focus"
					:ref="item.field" type="number" v-model="formFields[item.field]" border="none"
					:placeholder="$ts(item.placeholder||'请输入')" />
			</view>
			<view v-else-if="item.type=='decimal'">
				<input :style="getItemStyle(item)" :placeholder-style="getPlaceholderStyle(item)" :ref="item.field"
					type="digit" v-model="formFields[item.field]" border="none" :focus="item.focus"
					:placeholder="$ts(item.placeholder||'请输入')" />
			</view>
			<view class="f-form-content-radio" v-else-if="item.type=='radio'">
				<!--  <view> -->
				<u-radio-group @change="(val)=>{radioOnChange(val,item)}" :placement="item.placement"
					v-model="formFields[item.field]">
					<u-radio v-for="(option,opIndex) in ( item.data||item.bind.data)"
						:customStyle="{'margin-bottom':item.placement=='column'?'30rpx':0,'margin-right':item.placement=='column'?'0':'30rpx'}"
						:label="option.value" :name="option.key">
					</u-radio>
				</u-radio-group>
				<!-- 	  </view> -->
			</view>
			<view class="f-form-content-radio" v-else-if="item.type=='switch'">
				<u-radio-group @change="(val)=>{radioOnChange(val,item)}" :placement="item.placement"
					v-model="formFields[item.field]" placement="row">
					<u-radio :customStyle="{'margin-right': '40rpx'}" :label="$ts('是')" :name="1">
					</u-radio>
					<u-radio :label="$ts('否')" :name="0">
					</u-radio>
				</u-radio-group>
			</view>
			<view class="f-form-content-textarea" v-else-if="item.type=='textarea'">
				<view style="width: 30rpx;"></view>
				<textarea :style="getItemStyle(item)" :placeholder-style="getPlaceholderStyle(item)" :ref="item.field"
					auto-height :maxlength="item.maxlength||100000" v-model="formFields[item.field]" :focus="item.focus"
					border="none" :placeholder="$ts(item.placeholder||'请输入')"></textarea>
			</view>
			<!-- 	 -->
			<u-upload :ref="item.field" :sizeType="['compressed']" v-else-if="item.type=='img'"
				:fileList="formFields[item.field]" @afterRead="(event)=>{afterRead(item,event)}"
				@delete="(event)=>{deletePic(item,event)}" name="3" :multiple="item.multiple" :accept="item.accept"
				:maxCount="item.maxCount||item.maxFile||1" :previewFullImage="true">
				<image src="/static/zp.png" mode="widthFix" style="width: 82px;height: 82px;"></image>
			</u-upload>
			<view v-else-if="item.type=='password'" class="vol-form-item-input">
				<input :style="getItemStyle(item)" :placeholder-style="getPlaceholderStyle(item)" type="password"
					v-model="formFields[item.field]" border="none" :ref="item.field" :focus="item.focus"
					:placeholder="$ts(item.placeholder||'请输入')" />
			</view>
			<view v-else style="flex: 1;" class="vol-form-item-input">
				<input :style="getItemStyle(item)" :placeholder-style="getPlaceholderStyle(item)" type="text"
					@confirm="(e)=>{inputConfirm(item.field,e)}" v-model="formFields[item.field]" border="none"
					:ref="item.field" :focus="item.focus" :placeholder="$ts(item.placeholder||'请输入')" />
			</view>
			<view v-if="item.extra" :style="item.extra.style" style="display: flex;" class="vol-form-item-extra">
				<view v-if="item.extra.button" style="margin-left: 10rpx;">
					<vol-button @click="extraClick(item,formFields)" :type="item.extra.type" size="mini" shape="circle">
						<u-icon v-if="item.extra.icon" :name="item.extra.icon" size="12" :color="item.extra.color">
						</u-icon>
						{{item.extra.text}}
					</vol-button>
				</view>

				<view v-else @click="extraClick(item,formFields)" style="margin-left: 10rpx;display: flex;">
					<u-icon v-if="item.extra.icon" :name="item.extra.icon" :color="item.extra.color"
						:size="item.extra.size">
					</u-icon>
					<text>{{item.extra.text}}</text>
				</view>
			</view>
		</view>
	</view>
</template>

<script setup>
	import propsOptions from './props.js'
	import {
		ref,
		defineProps,
		defineEmits,
		defineExpose,
		getCurrentInstance
	} from 'vue'
	const props = defineProps(propsOptions())

	const emit = defineEmits(['onChange', 'inputConfirm', 'extraClick', 'showPicker', 'showSelect', 'afterRead',
		'chooseFile'
	])
	const {
		proxy
	} = getCurrentInstance()


	const imgFields = ref([])


	const deletePic = (item, event) => {
		props.formFields[item.field].splice(event.index, 1)
	}
	const afterRead = (option, event) => {
		emit('afterRead', option, event)
	}
	const chooseFile = () => {
		emit('chooseFile')
	}


	const previewImage = (item, imgIndex) => {
		// const imgs = getImgSrcs(item).map(x => {
		// 	return x.url
		// })
		const imgs =[item.url]
		uni.previewImage({
			urls: imgs,
			current: imgIndex
		})
	}
	const previewFile = (item, imgIndex) => {
		//item.url='http://localhost:9081/123.pdf'
		let fileType = item.url.split('/').pop().split('.').pop().toLowerCase();
		if (['jpg', 'jpeg', 'png'].includes(fileType)) {
			uni.previewImage({
				urls: [item.url],
				current: 0
			})
			return;
		}
		if (['mp4', 'mp3'].includes(fileType)) {
			proxy.$toast('暂未支持视频播放')
			return;
		}
		// #ifdef H5
		if (fileType=='pdf') {
			window.open(item.url,'self')
			// uni.navigateTo({
			// 	url:'/components/vol-preview/vol-preview?src='+encodeURIComponent(item.url)
			// })
			return;
		}
		proxy.$toast('h5暂未支持文件预览')
		return;
		// #endif
		uni.showToast({
			icon: "none",
			title: proxy.$ts('正在打开文件,请稍等')
		})
		uni.downloadFile({
			url: item.url,
			success: function(res) {
				var filePath = res.tempFilePath;
				uni.openDocument({
					filePath: filePath,
					showMenu: true,
					success: function(res) {
						uni.hideToast()
						console.log('打开文档成功');
						uni.hideToast()
					}
				});
			},
			fail(error) {
				uni.hideToast()
			}
		});
	}

	const getFileName = (path) => {
		//console.log(path)
		if (!path) {
			return '未获取到文件名'
		}
		return path.split('/').pop();
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
			return {
				path: m,
				url: m.startsWith('http') ? m : (proxy.http.ipAddress + m)
			}
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
		let ids = getAllParentId(value, (item.data || item.bind.data));
		let names = [];
		for (let i = 0; i < ids.length; i++) {
			let obj = (item.data || item.bind.data).find(x => {
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
	const formatDicValue = (item) => {
		let value = props.formFields[item.field];
		if (proxy.base.isEmpty(value)) {
			return '';
		}
		if (item.type == 'cascader') {//||item.type=='treeSelect'
			return getCascaderNames(value, item);
		}
		if (isMultiSelect(item) || (typeof value == 'string' && value.indexOf(',') != -1)) {
			return formatDicValueList(item);
		}
		let _kv = (item.data || item.bind.data).find(x => {
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
			var obj = (item.data || item.bind.data).find(c => {
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
		if (proxy.base.isEmpty(props.formFields[item.field])) {
			return ''
		}
		if (item.range) {
			return props.formFields[item.field].filter(c => {
				return !proxy.base.isEmpty(c)
			}).map(c => {
				if (item.type == 'date') {
					return c.substr(0, 10)
				}
				return c;
			}).join(' - ')
		}

		if (item.data || (item.bind && item.bind.data)) {
			return formatDicValue(item);
		} else if (item.type == 'switch') {
			return props.formFields[item.field] ? '是' : '否'
		}
		if (item.type == 'date') {
			return props.formFields[item.field].substr(0, 10)
		}

		return props.formFields[item.field] || '';
	}
	//动态自定义标签样式
	const getTitleStyle = (item, isValue) => {
		if (props.getStyle) {
			let sty = props.getStyle(props.formFields, item, isValue)
			if (sty) {
				return sty;
			}
		}
		if (item.titleStyle) {
			//if (typeof(item.titleStyle) == 'string') {
			if (props.labelWidth) {
				return `width:${props.labelWidth}px;${item.titleStyle}`
			}
			return item.titleStyle;
			//}
			// if (props.labelWidth) {
			// 	return Object.assign(({
			// 		width: props.labelWidth + 'px'
			// 	}, item.titleStyle))
			// }
			// return item.titleStyle;
		}
		if (props.labelWidth) {
			return `width:${props.labelWidth}px`
		}
		return;
	}
	const getItemStyle = (item) => {
		//动态自定义值样式
		if (props.getStyle) {
			let sty = props.getStyle(props.formFields, item, true);
			if (sty) {
				return sty;
			}
		}
		if (item.itemStyle || item.valueStyle) {
			return 'font-size:' + props.fontSize + ';' + (item.itemStyle || '') + ';' + (item.valueStyle || '')
		}
		return 'font-size:' + props.fontSize
	}

	const extraClick = (item, inFormFields, index) => {
		emit('extraClick', item, props.formFields, index)
	}
	const getPlaceholderStyle = (item) => {
		return 'font-size:' + props.fontSize
	}

	const showPicker = (item, index) => {
		emit('showPicker', index);
	}

	const cascaderConfirm = (value, item) => {
		props[actionSascaderCurrentItem.value.field] = value;
		emit("onChange", actionSascaderCurrentItem.value.field, value, item);
	}

	const radioOnChange = (value, item) => {
		emit("onChange", item.field, value, item, (item.data || item.bind?.data));
	}

	const inputConfirm = (field, e) => {
		console.log('回车')
		emit('inputConfirm', field, e);
	}

	const showSelect = (item) => {
		emit('showSelect');
	}
	defineExpose({})
</script>
<style lang="scss" scoped>
	@import './form.scss';
</style>

<style scoped>
	.vol-form-item-right-value {
		font-size: 28rpx;
	}
</style>