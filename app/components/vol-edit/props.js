export default function() {
	return {
		id: {
			type: String,
			default: ''
		},
		editChange: {
			//新建数据后，变更为编辑状态
			type: Boolean,
			default: false
		},
		table: {
			type: Object,
			default: () => {
				return {
					key: '',
					sortName: "",
					tableName: "",
					tableCNName: "",
				}
			}
		},
		detail: { //主从明细表
			type: Object,
			default: () => {
				return {
					cnName: "",
					table: "",
					columns: []
				}
			}
		},
		details: { //一对多明细表
			type: Array,
			default: () => {
				return []
			}
		},
		keyField: {
			//主键字段
			type: String,
			default: ''
		},
		tableName: {
			type: String,
			default: ''
		},
		tableCNName: {
			type: String,
			default: ''
		},
		defaultLoad: { //默认加载表格数据
			type: Boolean,
			default: true
		},
		direction: { //页面显示表格或列表：horizontal、list
			type: String,
			default: "list"
		},
		titleField: { //list表列标题字段
			type: String,
			default: ""
		},
		tableAction: { //指定表的权限
			type: String,
			default: ""
		},
		labelPosition: {
			type: String,
			default: '' //left\right
		},
		labelWidth: {
			type: Number, //表单标签宽度
			default: 0
		},
		editFormFields: {
			type: Object,
			default: {}
		},
		editFormOptions: {
			type: Object,
			default: () => {
				return []
			}
		},
		index: { //显示行号
			type: Boolean,
			default: true
		},
		ck: {
			//明细表复选框
			type: Boolean,
			default: true
		},
		textInline: {
			type: Boolean,
			default: true
		},
		saveName:{//保存按钮名字
			type: String,
			default: "保存"
		},
		getStyle: null, //标题与属性样式
		loadFormBefore: null, //表单加载前
		loadFormAfter: null, //表单加载后
		delBefore: null,
		delAfter: null,
		loadDetailBefore: null,
		loadDetailAfter: null, //明细表加载
		delRowBefore: null, //明细表删除方法
		delRowAfter: null,
		addRow: null, //明细表添加行方法
		modelOpenBefore: null,
		modelOpenBeforeAsync: null,
		modelOpenAfter: null,
		rowClick: null,
		getBottomButton: null,
		saveBefore: null,
		saveAfter: null
	}
}