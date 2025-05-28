export default function() {
	return {
		formOptions: {
			type: Array,
			default: () => {
				return []
			}
		},
		height:{ //表格高度可选值，0自动高度
			type:Number,
			default:0
		},
		item: {
			type: Object,
			default: () => {
				return {}
			} 
		},
		formFields: {
			type: Object,
			default: () => {
				return {}
			}
		},
		padding: {
			type: Number,
			default: 30
		},
		labelWidth: {
			type: Number,
			default: 0
		},
		labelPosition: {
			type: String,
			default: ''//left/right/top/bottom
		},
		loadKey: {
			type: Boolean,
			default: true
		},
		getStyle:null,//标题与属性样式
	}
}
