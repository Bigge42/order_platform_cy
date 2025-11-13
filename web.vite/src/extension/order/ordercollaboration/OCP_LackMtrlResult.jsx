let extension = {
  components: {
    //查询界面扩展组件
    gridHeader: '',
    gridBody: '',
    gridFooter: '',
    //新建、编辑弹出框扩展组件
    modelHeader: '',
    modelBody: '',
    modelRight: '',
    modelFooter: ''
  },
  tableAction: '', //指定某张表的权限(这里填写表名,默认不用填写)
  buttons: { view: [], box: [], detail: [] }, //扩展的按钮
  methods: {
     //下面这些方法可以保留也可以删除
    onInit() {  //框架初始化配置前，
        //示例：在按钮的最前面添加一个按钮
        //   this.buttons.unshift({  //也可以用push或者splice方法来修改buttons数组
        //     name: '按钮', //按钮名称
        //     icon: 'el-icon-document', //按钮图标：https://element.eleme.cn/#/zh-CN/component/icon
        //     type: 'primary', //按钮样式:https://element-plus.gitee.io/zh-CN/component/button.html
        //     //color:"#eee",//自定义按钮颜色
        //     onClick: function () {
        //       this.$Message.success('点击了按钮');
        //     }
        //   });

        //示例：设置修改新建、编辑弹出框字段标签的长度
        // this.boxOptions.labelWidth = 150;
    },
    onInited() {
      //框架初始化配置后
      //如果要配置明细表,在此方法操作
      //this.detailOptions.columns.forEach(column=>{ });
    },
    searchBefore(param) {
      //界面查询前,可以给param.wheres添加查询参数
      //返回false，则不会执行查询
      return true;
    },
    exportBefore(param) {
      //导出前处理，添加供应商字段权限控制参数和运算方案条件
      const sourceKey = window.location.hash.split('?')[0];

      // 单据类型映射 - 用于过滤不同类型的缺料数据
      const BillTypeMap = {
        '#/OCP_LackMtrlResult_PO': '标准采购',
        '#/OCP_LackMtrlResult_WO': '标准委外',
        '#/OCP_LackMtrlResult_MO_JG': '金工车间',
        '#/OCP_LackMtrlResult_MO_BJ': '部件车间',
      };

      // 业务类型映射 - 用于供应商字段权限控制
      const BusinessTypeMap = {
        '#/OCP_LackMtrlResult_PO': 'PO',    // 采购缺料 - 需要权限控制
        '#/OCP_LackMtrlResult_WO': 'PO',    // 委外缺料 - 需要权限控制
        '#/OCP_LackMtrlResult_MO_JG': 'MO', // 金工车间 - 不需要权限控制
        '#/OCP_LackMtrlResult_MO_BJ': 'MO', // 部件车间 - 不需要权限控制
      };

      // 初始化 wheres 数组
      param.wheres = param.wheres || [];

      // 添加BillType条件
      if (BillTypeMap[sourceKey]) {
        param.wheres.push({ name: 'BillType', value: BillTypeMap[sourceKey] });
      }

      // 添加CustomerParams用于供应商字段权限控制
      param.customerParams = param.customerParams || {};
      if (BusinessTypeMap[sourceKey]) {
        param.customerParams.BusinessType = BusinessTypeMap[sourceKey];
        console.log('导出设置BusinessType:', BusinessTypeMap[sourceKey], '用于路由:', sourceKey);
      }

      // 如果选中了运算方案,将方案ID作为导出条件
      // 从 sessionStorage 中获取当前选中的运算方案ID
      const selectedSchemeId = sessionStorage.getItem('OCP_LackMtrlResult_selectedSchemeId');
      if (selectedSchemeId) {
        // 检查是否已经存在 ComputeID 条件,避免重复添加
        const hasComputeId = param.wheres.some(w => w.name === 'ComputeID');
        if (!hasComputeId) {
          param.wheres.push({ name: 'ComputeID', value: selectedSchemeId });
          console.log('导出添加运算方案条件:', selectedSchemeId);
        }
      } else {
        // 如果没有选中运算方案,提示用户并阻止导出
        console.warn('未选中运算方案,无法导出数据');
        this.$message.warning('请先选择一个运算方案');
        return false;
      }

      return true;
    },
    searchAfter(result) {
      //查询后，result返回的查询数据,可以在显示到表格前处理表格的值
      return true;
    },
    addBefore(formData) {
      //新建保存前formData为对象，包括明细表，可以给给表单设置值，自己输出看formData的值
      return true;
    },
    updateBefore(formData) {
      //编辑保存前formData为对象，包括明细表、删除行的Id
      return true;
    },
    rowClick({ row, column, event }) {
      //查询界面点击行事件
      // this.$refs.table.$refs.table.toggleRowSelection(row); //单击行时选中当前行;
    },
    modelOpenAfter(row) {
      //点击编辑、新建按钮弹出框后，可以在此处写逻辑，如，从后台获取数据
      //(1)判断是编辑还是新建操作： this.currentAction=='Add';
      //(2)给弹出框设置默认值
      //(3)this.editFormFields.字段='xxx';
      //如果需要给下拉框设置默认值，请遍历this.editFormOptions找到字段配置对应data属性的key值
      //看不懂就把输出看：console.log(this.editFormOptions)
    }
  }
};
export default extension;
