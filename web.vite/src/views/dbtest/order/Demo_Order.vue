
<template>
  <view-grid
    ref="grid"
    :columns="columns"
    :detail="detail"
    :details="details"
    :editFormFields="editFormFields"
    :editFormOptions="editFormOptions"
    :searchFormFields="searchFormFields"
    :searchFormOptions="searchFormOptions"
    :table="table"
    :extend="extend"
    :onInit="onInit"
    :onInited="onInited"
    :searchBefore="searchBefore"
    :searchAfter="searchAfter"
    :addBefore="addBefore"
    :updateBefore="updateBefore"
    :rowClick="rowClick"
    :modelOpenBefore="modelOpenBefore"
    :modelOpenAfter="modelOpenAfter"
    :detailSelectable="detailSelectable"
    :saveConfirm="saveConfirm"
  >
    <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
    <template #gridFooter>
      <gridFooter ref="footerRef"></gridFooter>
    </template>
    <template #gridBody>
      <el-alert
        show-icon
        style="margin-bottom: 10px"
        type="success"
        title="当前页面示例：打印、多表头；弹出框：编辑表单下拉框table搜索、明细表table搜索；"
      ></el-alert>
    </template>
    <template #modelBody>
      <el-alert
        show-icon
        :closable="false"
        type="success"
        title="点击[客户]或明细表[商品名称]可进行下拉框table搜索(代码生成器生成后即可使用)"
      ></el-alert>
    </template>
    <template #detailContent>
      <!-- 自定义明细表内容 -->
      <DetailContent @detailContentOnChange="detailContentOnChange"></DetailContent>
    </template>
  </view-grid>

  <!-- 弹出框选择数据 -->
  <SelectData
    ref="selectRef"
    @detailOnSelect="detailOnSelect"
    @customerOnSelect="customerOnSelect"
  ></SelectData>
</template>
<script setup lang="jsx">
import extend from "@/extension/dbtest/order/Demo_Order.jsx";
import viewOptions from "./Demo_Order/options.js";
import { ref, reactive, getCurrentInstance,  watch, onMounted } from "vue";
//同一界面明细表格显示
import gridFooter from "./Demo_Order/Demo_OrderGridFooter.vue";
//弹出框自定义输入框
import DetailContent from "./Demo_Order/Demo_OrderDetailContent.vue";
//主表与明细表弹出框选择数据
import SelectData from "./Demo_Order/SelectData.vue";
//下拉table配置
import { initFormSelectTable, initDetailTable } from "./Demo_Order/OrderSelectTable.jsx";
const grid = ref(null);
const { proxy } = getCurrentInstance();
//http请求，proxy.http.post/get
const {
  table,
  editFormFields,
  editFormOptions,
  searchFormFields,
  searchFormOptions,
  columns,
  detail,
  details,
} = reactive(viewOptions());
const footerRef = ref();
let gridRef;
//生成对象属性初始化
const onInit = async ($vm) => {
  gridRef = $vm;
  //设置按钮显示数量，超出的显示在更多里面
  gridRef.maxBtnLength = 6;
  initBtn();
};

const saveConfirm =async (callback, formData, isAdd) => {
    proxy.$confirm('提示文字信息', '警告', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
        center: true
      }).then(() => {
        //逻辑处理完成后调用保存操作
         callback(true)
      })
};

const detailSelectable=(row, index, item)=>{
  console.log(index)
   return index==1;
}

const selectRef = ref();
//生成对象属性初始化后,操作明细表配置用到
const onInited = async () => {
  initFormSelectTable(gridRef);
  //明细初始化配置必须写在onInited中
  initDetailTable(gridRef);

  //增加明细表选择数据按钮
  gridRef.detailOptions.buttons.unshift({
    name: "选择数据", //按钮名称
    icon: "el-icon-plus", //按钮图标，参照iview图标
    hidden: false, //是否隐藏按钮(如果想要隐藏按钮，在onInited方法中遍历buttons，设置hidden=true)
    onClick: () => {
      //触发事件
      selectRef.value.openDetail();
    },
  });

  //这里可以重新设置主表表格高度
  if (gridRef.height > 300) {
    gridRef.height = gridRef.height - 160 - 50;
  }
  gridRef.height = gridRef.height - 40;
  //主表设置表格合计功能
  gridRef.summary = true;
  gridRef.columns.forEach((x) => {
    if (x.field == "TotalQty" || x.field == "TotalPrice") {
      x.summary = true;
    }
  });
  //明细表求和
  gridRef.detailOptions.summary = true;
  gridRef.detailOptions.columns.forEach((x) => {
    x.require = false;
    if (x.field == "Price" || x.field == "Qty") {
      x.summary = true;
      //按回车自动跳转到下一行焦点
      // x.onKeyPress = (row, column, $e) => {
      //   if ($e && $e.keyCode == 13) {
      //     gridRef.$refs.detail.toNextCell(null, row, "Qty", true);
      //   }
      // };
    }
  });

  //设置明细表高度
  gridRef.detailOptions.height = 240;
  //固定弹出框宽度
  gridRef.boxOptions.width = 1200;
};
//明细表选择数据回写
const detailOnSelect = (rows) => {
  //获取明细表rowData追加数据
  gridRef.getTable().rowData.push(...rows);
};
//表单选择数据回调
const customerOnSelect = (row) => {
  editFormFields.Customer = row.Customer;
  editFormFields.CustomerId = row.CustomerId;
  editFormFields.PhoneNo = row.PhoneNo;
};

const searchBefore = async (param) => {
  //界面查询前,可以给param.wheres添加查询参数
  //返回false，则不会执行查询
  return true;
};
const searchAfter = async (rows, result) => {
  return true;
};
const addBefore = async (formData) => {
  //新建保存前formData为对象，包括明细表，可以给给表单设置值，自己输出看formData的值
  return true;
};
const updateBefore = async (formData) => {
  //编辑保存前formData为对象，包括明细表、删除行的Id
  return true;
};
const rowClick = ({ row, column, event }) => {
  //取消其他行选中
  gridRef.getTable(true).clearSelection();
  //设置选中当前行
  gridRef.getTable(true).toggleRowSelection(row);
  //调用Demo_OrderGridFooter.vue中明细表table的查询方法
  footerRef.value?.gridRowClick(row);
};
const modelOpenBefore = async (row) => {
  //弹出框打开后方法
  return true; //返回false，不会打开弹出框
};
const modelOpenAfter = (row) => {
  //弹出框打开后方法,设置表单默认值,按钮操作等
};
//监听表单输入，做实时计算
//watch(() => editFormFields.字段,(newValue, oldValue) => {	})

const detailContentOnChange = (val) => {
  proxy.$message.success("下选择选择了:" + val);
};

//配置表头过滤
columns.forEach((x) => {
  if (x.field == "OrderNo" || x.field == "OrderType") {
    x.filterData = true;
  }
});

//自定义合计显示格式
columns.forEach((x) => {
  if (x.field == "TotalPrice") {
    x.summary = true;
    x.align = "center";
    x.width = 80;
    x.summaryFormatter = (val, column, result, summaryData) => {
      if (!val) {
        return "0.00";
      }
      summaryData[0] = "汇总";
      return (
        "￥" + (val + "").replace(/\B(?=(\d{3})+(?!\d))/g, ",") //+ '元'
      );
    };
    //计算平均值
    //x.summary = 'avg';
    //设置小数显示位数(默认2位)
    // x.numberLength = 4;
  }
});

const initBtn = () => {
  //增加提交审批按钮
  let index = gridRef.buttons.findIndex((x) => {
    return x.value == "Audit";
  });
  gridRef.buttons.splice(index + 1, 0, {
    name: "提交",
    icon: "el-icon-check",
    class: "",
    plain: true,
    type: "primary",
    onClick: () => {
      let rows = gridRef.getSelectRows();
      if (!rows.length) {
        return gridRef.$message.error("请选择行数据");
      }
      let ids = rows.map((x) => {
        return x.Order_Id;
      });

      gridRef.http.post("api/Demo_Order/submitAudit", ids).then((result) => {
        if (!result.status) {
          gridRef.$message.error(result.message);
          return;
        }
        gridRef.$message.success(result.message);
        gridRef.search();
      });
    },
  });

  //自定义表单按钮
  let countdown = 10;
  const btnValue = ref("发送短信");
  const phoneOption = gridRef.getFormOption("PhoneNo");
  phoneOption.extra = {
    render: (h, {}) => {
      return (
        <div>
          <el-button
            type="primary"
            link
            onClick={() => {
              selectRef.value.open();
            }}
          >
            <i class="el-icon-search">选择数据</i>
          </el-button>
          <el-button
            type="primary"
            style="margin-left:0"
            link
            onClick={() => {
              //设置倒计时
              var timer = setInterval(function () {
                if (countdown > 0) {
                  btnValue.value = countdown + "(秒)";
                  countdown--;
                } else {
                  btnValue.value = "发送短信";
                  countdown = 10;
                  clearInterval(timer);
                }
              }, 1000);
            }}
          >
            <i class="el-icon-message">{btnValue.value}</i>
          </el-button>
          <el-popover placement="top-start" title="提示" width="200" trigger="hover">
            {{
              reference: () => {
                return (
                  <i
                    style="color:rgb(6 118 169);font-size:12px;margin-left:5px"
                    onClick={() => {
                      gridRef.$message.success("提示信息");
                    }}
                    class="el-icon-warning-outline"
                  ></i>
                );
              },
              default: () => {
                return (
                  <div style="width:300px">
                    <div style="font-size:12px">触发 Popover 显示的 HTML 元素</div>
                  </div>
                );
              },
            }}
          </el-popover>
        </div>
      );
    },
  };
};
//对外暴露数据
defineExpose({});
</script>
