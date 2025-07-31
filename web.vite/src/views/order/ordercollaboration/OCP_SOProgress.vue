<!-- 销售订单进度查询 -->
<template>
    <view-grid ref="grid" :columns="columns" :detail="detail" :details="details" :editFormFields="editFormFields"
        :editFormOptions="editFormOptions" :searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions"
        :table="table" :extend="extend" :onInit="onInit" :onInited="onInited" :searchBefore="searchBefore"
        :searchAfter="searchAfter" :addBefore="addBefore" :updateBefore="updateBefore" :rowClick="rowClick"
        :modelOpenBefore="modelOpenBefore" :modelOpenAfter="modelOpenAfter">
        <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
        <template #gridHeader>
        </template>
        <template #gridFooter>
            <div class="sale-order-detail">
                <sale-order-detail ref="saleOrderDetailRef"></sale-order-detail>
            </div>
        </template>
    </view-grid>
</template>
<script setup lang="jsx">
import saleOrderDetail from './OCP_SOProgressDetail.vue'
import extend from "@/extension/order/ordercollaboration/OCP_SOProgress.jsx";
import viewOptions from './OCP_SOProgress/options.js'
import { ref, reactive, getCurrentInstance, watch, onMounted } from "vue";


const grid = ref(null);
const { proxy } = getCurrentInstance()
//http请求，proxy.http.post/get
const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())
const saleOrderDetailRef = ref();


let gridRef;//对应[表.jsx]文件中this.使用方式一样
//生成对象属性初始化
const onInit = async ($vm) => {
    gridRef = $vm;
    //设置分页条大小
    gridRef.pagination.sizes = [20, 50, 100, 200, 500, 1000];
    //设置默认分页数
    gridRef.pagination.size = 20;
    //与jsx中的this.xx使用一样，只需将this.xx改为gridRef.xx
    // gridRef.setFixedSearchForm(true);
    
    // 设置快捷查询字段
    gridRef.queryFields = ['BillNo', 'SalesContractNumber', 'CustomerContractNumber'];

    //缓存主表方法，返回主表选中的行，在下面的明细表中会调用
    proxy.base.setItem('getSaleOrderSelectRow', () => {
        return gridRef.getTable(true).getSelected();
    })
}
//生成对象属性初始化后,操作明细表配置用到
const onInited = async () => {
    gridRef.height = gridRef.height - 510;
    if (gridRef.height < 500) {
        gridRef.height = 500;
    }
}
const searchBefore = async (param) => {
    //界面查询前,可以给param.wheres添加查询参数
    //返回false，则不会执行查询
    return true;
}
const searchAfter = async (rows, result) => {
    return true;
}
const addBefore = async (formData) => {
    //新建保存前formData为对象，包括明细表，可以给给表单设置值，自己输出看formData的值
    return true;
}
const updateBefore = async (formData) => {
    //编辑保存前formData为对象，包括明细表、删除行的Id
    return true;
}
const rowClick = ({ row, column, event }) => {
    //点击行清除选中的行(用于下面明细表判断)
    grid.value.clearSelection();
    //点击行默认选中
    grid.value.toggleRowSelection(row); //单击行时选中当前行;
    //加载明细表
    saleOrderDetailRef.value.$refs.grid.search();
}
const modelOpenBefore = async (row) => {//弹出框打开后方法
    return true;//返回false，不会打开弹出框
}
const modelOpenAfter = (row) => {
    //弹出框打开后方法,设置表单默认值,按钮操作等
}
//监听表单输入，做实时计算
//watch(() => editFormFields.字段,(newValue, oldValue) => {	})
//对外暴露数据
defineExpose({})
</script>
<style lang="less" scoped>
.sale-order-detail {
    margin-top: 13px;
    border-top: 7px solid #eee;
}
</style>