import {
	ref,
	toRefs
} from 'vue'
export const initAsync = (proxy, props, callback) => {
	props.editFormOptions.forEach(x => {
		if (x.type == 'img' || x.type == 'file' || x.type == 'excel') {
			x.url = x.url || `api/${props.table.tableName}/upload`;
			if (!Array.isArray(props.editFormFields[x.field])) {
				props.editFormFields[x.field] = []
			}
		}
	})

	props.searchFormOptions.forEach(x => {
		if (x.type == 'range') {
			x.range = true;
		} else if ((x.type == 'datetime' || x.type === 'date') && x.range !== false && !Array.isArray(props
				.searchFormFields[x.field])) {
			x.range = true;
			if (!Array.isArray(props.searchFormFields[x.field])) {
				props.searchFormFields[x.field] = [null, null]
			}
		}
	})

	let authFields = proxy.$store.getters.getAuthFields()
	let permission = proxy.$store.getters.getMenu();
	// console.log(callback)
	// return {}
	if ((!permission || !permission.length)) {
		proxy.http.get("api/menu/getTreeMenu", {}, false).then(result => {
			permission = (result.menu ? result.menu : result) || [];
			authFields = result.authFields || []
			proxy.$store.commit("setPermission", JSON.parse(JSON.stringify(permission)));
			proxy.$store.commit("setAuthFields", authFields);
			callback(initPermissionButtons(proxy, props, permission, authFields));
			return;
		})
	}
	callback(initPermissionButtons(proxy, props, permission, authFields));
}
const initPermissionButtons = (proxy, props, permission, authFields) => {
	const state = {
		buttons: [],
		boxButtons: [],
		permission: {},
		hasEditpermission: false
	}
	const data = (permission.find(x => {
		return (props.tableAction || props.table.tableName).toUpperCase() == x.tableName
			.toUpperCase()
	}) || {});
	let _permission = data.permission;
	if (!_permission) {
		return state;
	}
	state.permission = data;
	if (_permission.indexOf("Delete") != -1) {
		state.boxButtons.push({
			name: "删除",
			icon: 'close',
			value: 'del',
			hidden: true,
			type: 'error',
			onClick: () => {
				proxy.delData();
			}
		})
	}
	//新建、编辑弹出框按钮
	if (_permission.indexOf("Update") != -1 || _permission.indexOf("Add") != -1) {
		state.boxButtons.push(...[{
			name: "提交",
			icon: 'checkbox-mark',
			value: 'add',
			hidden: false,
			type: 'primary',
			onClick: () => {
				proxy.save();
			}
		}])
		state.hasEditpermission = true;
	} else {
		//没有编辑权限，将编辑表单的全部设置为只读
		props.editFormOptions.forEach(x => {
			x.readonly = true;
			x.required = false;
		})
	}
	//table界面浮动按钮
	state.buttons.push(...[{
		icon: "reload", //刷新
		img: "/static/btn/refresh.png",
		value: "Search",
		name: "刷新",
		hidden: false,
		onClick: () => {
			proxy.search();
		}
	}, {
		icon: "search",
		img: "/static/btn/filter.png",
		value: "Search",
		name: "筛选",
		hidden: false,
		onClick: () => {
			proxy.showSearch();
		}
	}])
	if (_permission.indexOf("Add") != -1) {
		state.buttons.unshift({
			img: "/static/btn/add.png",
			hidden: false,
			name: "添加",
			size: '17',
			color: 'rgb(2, 171, 255)',
			onClick: () => {
				proxy.add();
			}
		})
	}
	return state;
	// return {
	// 	...toRefs(ref(state))
	// }
}


export const reset = (formFields, source, formOptions, base, http) => {

	proxy.base.resetForm(props.editFormFields, props.editFormOptions, result.rows[0]);

	for (const key in formFields) {
		if (source && source.hasOwnProperty(key)) {
			if (formOptions.some(x => {
					return x.field == key && (x.type == 'img'||x.type=='file'||x.type=='excel')
				})) {
				formFields[key] = base.getImg(source[key], http)
			} else {
				formFields[key] = source[key];
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