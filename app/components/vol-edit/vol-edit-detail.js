export const initDetail = (props) => {
	if (!props.details.length && props.detail && props.detail.columns.length) {
		props.details.push(props.detail)
	}
	props.details.forEach(x => {
		x.url = `api/${props.table.tableName}/getDetailPage`
		x.tableData = [];
	})
}


const setDelKey = (tableInfo, keys) => {
	if (!tableInfo.delKeys) {
		tableInfo.delKeys = []
	}
	if (!keys) {
		tableInfo.delKeys = []
		return;
	}
	tableInfo.delKeys.push(...keys)
}

const initFormOptions = (tableInfo) => {
	tableInfo.columns.filter(x => {
		return !x.hidden && x.edit
	}).forEach(col => {
		const ops = {
			field: col.field,
			title: col.title,
			require: col.require,
			readonly: !!col.readonly,
			extra:col.extra,
			type:col.edit.type,
			multiple: col.multiple,
			maxFile:col.maxFile
		};
		if (col.bind && col.bind.data) {
			ops.data = col.bind.data;
		} else if (col.data && col.data.length) {
			ops.data = col.data
		} else {
			ops.data = [];
		}
		switch (col.edit.type) {
			case 'selectList':
			case 'treeSelect':
				tableInfo.edit.formFields[col.field] = [];
				break;
			case 'img':
			case 'file':
			case 'excel':
				ops.url = `api/${tableInfo.table}/upload`;
				tableInfo.edit.formFields[col.field] = [];
				break;
			default:
				tableInfo.edit.formFields[col.field] = null;
				ops.type = col.edit.type;
				if (!col.bind&&col.type == 'int') {
					ops.type = 'number'
				}
				break;
		}
		tableInfo.edit.formOptions.push(ops);
	})
}
//添加行
export const addRow = async (proxy, props) => {
	await editRow(proxy, props, null, true)
}
export const editRow = async (proxy, props, editInfo, isAdd) => {
	const tableInfo = props.details[proxy.config.value.currentTableIndex]
	//弹出框编辑
	if (!tableInfo.edit) {
		tableInfo.edit = {
			row: null,
			formFields: {},
			formOptions: []
		}
		initFormOptions(tableInfo);
		props.detailFormInited&&props.detailFormInited(tableInfo)
	} else {
		proxy.base.resetForm(tableInfo.edit.formFields, tableInfo.edit.formOptions)
	}
	let row = {}

	if (isAdd) {
		tableInfo.edit.row = null;
		if (props.addRow) {
			row = await props.addRow(tableInfo.table, tableInfo) || {};
			if (Array.isArray(row)) {
				row = row[0]
			}
		}
		Object.assign(tableInfo.edit.formFields, row);
		return;
	}
	tableInfo.edit.row = editInfo.row;
	//编辑
	proxy.base.resetForm(tableInfo.edit.formFields, tableInfo.edit.formOptions, editInfo.row)
}
//明细编辑点击确认
export const confirmRow = (proxy, props) => {
	const tableInfo = props.details[proxy.config.value.currentTableIndex];
	//编辑
	if (tableInfo.row) {
		return;
	}
	let row = proxy.base.getFormValues(tableInfo.edit.formFields, tableInfo.edit.formOptions)
	if (tableInfo.edit.row) {
		Object.assign(tableInfo.edit.row, row)
	} else {
		tableInfo.tableData.unshift(row)
	}

}
export const delRow = async (proxy, props, tableRef) => {
	const rows = tableRef.getSelectedRows();
	if (rows.length) {
		const tableInfo = props.details[proxy.config.value.currentTableIndex];
		if (props.delRowBefore && !(await props.delRowBefore(rows, tableInfo.table, tableInfo))) {
			return;
		}
		uni.showModal({
			title: proxy.$ts('提示'),
			content: proxy.$ts('确定要删除选中的行吗'),
			success: async (res) => {
				if (!res.confirm) {
					return;
				}
				tableRef.delRows();
				props.delRowAfter && await props.delRowAfter(rows, tableInfo.table,
					tableInfo);
				const keyField = props.details.find(x => {
					return x.table == tableInfo.table
				}).key;
				const keys = rows.filter(x => {
						return x[keyField]
					})
					.map(x => {
						return x[keyField]
					})
				setDelKey(tableInfo, keys);
			}
		})
		return;
	}
	tableRef.delRows();
}