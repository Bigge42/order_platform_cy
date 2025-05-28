   
export default function() {
	return {
		formOptions: {
			type: Array,
			default: () => {
				return []
			}
		},
		item: {
			type: Object,
			default: () => {
				return {}
			} 
		},
		readonly:{ //默认是否只读
			type:Boolean,
			default:false
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
		labelWidth: {//标题标签宽度
			type: Number,
			default: 0
		},
		labelPosition: {
			type: String,
			default: ''//left\right
		},
		loadKey: {
			type: Boolean,
			default: true
		},
		border:{
			type: Boolean,
			default: false
		},
		fontSize:{
			type: String,
			default: '28rpx'
		},
		getStyle:null,//标题与属性样式
	}
}
