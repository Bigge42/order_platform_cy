import modelRight from './Demo_CustomerMapModelRight.vue';
import { h, resolveComponent } from 'vue';
let extension = {
  components: {
    //查询界面扩展组件
    gridHeader: '',
    gridBody: {
      render() {
        return h(
          <el-alert
            show-icon={true}
            closable={false}
            style="margin-bottom:10px"
            type="success"
            title="点击新建或编辑进行地图选点(只需简单配置即可实现编辑表单左右结构)"
          ></el-alert>
        );
      }
    },
    gridFooter: '',
    //新建、编辑弹出框扩展组件
    modelHeader: '',
    modelBody: '',
    modelRight: modelRight,
    modelFooter: ''
  },
  text: '',
  tableAction: '', //指定某张表的权限(这里填写表名,默认不用填写)
  buttons: { view: [], box: [], detail: [] }, //扩展的按钮
  methods: {
    onInit() {

      //设置弹出框高度
      this.boxOptions.width = 900;
      //设置表单标签显示位置，显示到左边(也可以在main.js统一设置labelPosition属性)
      this.labelPosition = 'left';
      //设置表单标签宽度
      this.boxOptions.labelWidth = 70;
    },
    onInited() {
      //调整表单高度
      this.height = this.height - 40;
    },
    modelOpenAfter(row) {}
  }
};
export default extension;
