export default function() {
	return {
		url: { //接口地址
			type: String,
			default: ""
		},
		index: { //是否显示行号
			type: Boolean,
			default: false
		},
		height: {
			type: Number, //表格高度,为0时默认为页面剩余高度(100%高度),为-1时，不固定高度，内容有多长就显示多高
			default: 0 //可选值0、-1、自定义高度值
		},
		minHeight: {  //表格为自动高度时最小高度
			type: Number,
			default: 0
		},
		ck: { //设置显示checkbox，只有水平(table)显示类型时才生效
			type: Boolean,
			default: false
		},
		direction: { //table显示方向
			type: String,
			default: "list" //horizontal:水平table形式显示，list：列表表单形式展示 
		},
		titleField: { //如果direction是以list显示，可以指定第一个标题
			type: String,
			default: ""
		},
		readonly: { //表格是否只读状态(true不可以编辑)
			type: Boolean,
			default: true
		},
		columns: {
			type: Array,
			default: () => {
				return []
			}
		},
		tableData: {
			type: Array,
			default: () => {
				return []
			}
		},
		padding: { //表格padding，main.js也可以全局设置
			type: Number,
			default: 0
		},
		contentPadding:{
			type: Number,
			default: 0
		},
		textAlign: "", //表格标签显示位置(左边、右边)：left、center、right
		textInline: { //文本是否显示在一行（超出不换行）
			type: Boolean,
			default: true
		},
		labelWidth: {
			type: Number,
			default: 0
		},
		labelPosition: {
			type: String,
			default: '' //left\right
		},
		loadKey: {
			type: Boolean,
			default: true
		},
		defaultLoad: {
			// 配置了url，是否默认加载表格数据
			type: Boolean,
			default: true
		},
		labelWidth: {
			type: Number,
			default: 0
		},
		rowStyle: null,
		// { //每行自定义样式
		// 	type: Function,
		// 	default: (row, index, rows) => {
		// 		return;
		// 	}
		// },
		columnStyle: null,
		// { //每个单元格自定义样式
		// 	type: Function,
		// 	default: (row, column, field, rows) => {
		// 		return;
		// 	}
		// },
		getStyle: null, //表单形式，标题与属性样式
		selectable: null,
		// {
		// 	type: Function,
		// 	default: (row, column, rowIndex) => { // //复选框是否可以选中
		// 		return true;
		// 	}
		// },
		rowClick: null,
		formatter: null,
		loadBefore: null,
		loadAfter: null,
		getButtons: null,
		format: null
	}
}
