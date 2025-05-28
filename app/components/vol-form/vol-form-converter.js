import common from '@/util/common.js'
import store from '@/store/index'
export const formatDate=(value,type)=>{
	 if (!value) {
	 	return ''
	 }
	 if (type=='date') {
	 	return value.substr(0,10)
	 }
	 if (type=='month') {
	 	return value.substr(0,7)
	 }
	 if (type=='year') {
	 	return value.substr(0,4)
	 }
	 if (type=='time') {
	 	return value.substr.substr(11,5)//,8)
	 }
	 return value;
}
export const getFormValues = (formFields, formOptions) => {
	if (formFields.value) {
		formFields = formFields.value;
	}
	if (formOptions.value && Array.isArray(formOptions.value)) {
		formOptions = formOptions.value;
	}
	//将数组转换成string
	const formValues = {};
	for (const key in formFields) {
		let _val = formFields[key];
		if (Array.isArray(_val)) {
			//上传的图片
			if (formOptions.some(x => {
					return x.field == key && (x.type == 'img' || x.type == 'file' || x.type == 'excel')
				})) {
				let imgs = _val.filter(c => {
					return c.orginUrl
				});
				imgs = imgs.map(m => {
					return m.orginUrl
				}).join(',');
				formValues[key] = imgs;
			} else {
				formValues[key] = _val.join(',');
			}
		} else {
			formValues[key] = _val
		}
	}
	return formValues;
}
//重置表单
const resetForm = (formFields, formOptions, data) => {
	//console.log('66')
	if (formFields.value) {
		formFields = formFields.value;
	}
	if (formOptions.value && Array.isArray(formOptions.value)) {
		formOptions = formOptions.value;
	}
	if (!data) {
		data = {}
	}
	for (const key in formFields) {
		if (data && data.hasOwnProperty(key)) {
			if (formOptions.some(x => {
					return x.field == key && (x.type == 'img' || x.type == 'file' || x.type == 'excel')
				})) {
				formFields[key] = common.getImg(data[key], common.http)
			} else {
				formFields[key] = data[key];
			}
		} else {
			if (Array.isArray(formFields[key])) {
				formFields[key].splice(0);
				if (formOptions.some(x => {
						return x.field == key && x.range
					})) {
					formFields[key].push(...['', '']);
				}
			} else {
				formFields[key] = null
			}
		}
	}
}
//获取表单配置
const getFormOption = (formOptions, field) => {
	if (formOptions.value && Array.isArray(formOptions.value)) {
		formOptions = formOptions.value;
	}
	// const option = formOptions.find(x => {
	// 	return x.field == field
	// });
	let option;
	for (var index = 0; index < formOptions.length; index++) {
		if (option) {
			return option;
		}
		var item = formOptions[index];
		if (Array.isArray(item)) {
			option = item.find(x => {
				return x.field == field
			})
		} else if (item.field == field) {
			option = item;
		}
	}
	if (!option) {
		console.log('未获取字段配置')
	}
	return option;
}

//获取表单字典
const getFormDicData = (formOptions, field) => {
	const option = getFormOption(formOptions, field);
	return option.data;
}

//获取表单字典选项(根据key获取value显示文本)
const getFormDicItem = (formOptions, field, key) => {
	const data = getFormDicItem(formOptions, field);
	if (Array.isArray(key)) {
		return data.filter(x => {
			return key.includes(x.key)
		});
	}
	return data.find(x => {
		return key == x.key
	});
}
//设置表单焦点
const setFormFocus = (formOptions, field, timeout = 400) => {
	const option = getFormOption(formOptions, field);
	if (!timeout || timeout < 50) {
		timeout = 200;
	}
	option.focus = false;
	setTimeout(() => {
		option.focus = true;
	}, timeout)
}

//获取表格配置
const getColumn = (columns, field) => {
	if (columns.value && Array.isArray(columns.value)) {
		columns = columns.value;
	}
	let column;
	for (var index = 0; index < columns.length; index++) {
		if (column) {
			return column;
		}
		var item = columns[index];
		if (Array.isArray(item)) {
			column = item.find(x => {
				return x.field == field
			})
		} else if (item.field == field) {
			column = item;
		}
	}
	if (!column) {
		console.log('未获取字段配置')
	}
	return column;
}

//获取表格字典配置
const getColumnDicData = (columns, field) => {
	const column = getColumn(columns, field);

	if (column.bind && column.bind.data) {
		return column.bind.data
	}
	if (!column.data) {
		console.log('未获取到字段字典配置')
	}
	return column.data;
}

//获取表格字典项(根据key获取value显示文本)
const getColumnDicItem = (columns, field, key) => {
	const data = getColumnDicData(columns, field);
	if (Array.isArray(key)) {
		return data.filter(x => {
			return key.includes(x.key)
		});
	}
	return data.find(x => {
		return key == x.key
	});
}

const setItem = (key, obj) => {
	store.getters.data()[key] = obj
}
const getItem = (key) => {
	const obj = store.getters.data()[key];
	return obj;
}

export default {
	formatDate,
	getFormValues,
	resetForm,
	getFormOption,
	getFormDicData,
	getFormDicItem,
	setFormFocus,
	getColumn,
	getColumnDicData,
	getColumnDicItem,
	setItem,
	getItem
}