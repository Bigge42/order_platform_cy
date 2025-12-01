<!--
 *Author：jxx
 *Date：{Date}
 *Contact：461857658@qq.com
 *业务请在@/extension/order/ocp_bom_completion/vw_OCP_Tech_BOM_Status_Monthly.jsx或vw_OCP_Tech_BOM_Status_Monthly.vue文件编写
 *新版本支持vue或【表.jsx]文件编写业务,
 -->
<template>
    <view-grid ref="grid"
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
               :exportBefore="exportBefore"
               :searchAfter="searchAfter"
               :addBefore="addBefore"
               :updateBefore="updateBefore"
               :rowClick="rowClick"
               :modelOpenBefore="modelOpenBefore"
               :modelOpenAfter="modelOpenAfter">
        <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
        <template #btnLeft>
            <div class="month-filter">
                <el-date-picker v-model="selectedMonth"
                                type="month"
                                placeholder="审核月份"
                                format="YYYY-MM"
                                value-format="YYYY-MM"
                                style="width: 150px"
                                @change="triggerMonthFilter" />
                <el-button type="primary" @click="triggerMonthFilter">按审核月份筛选</el-button>
            </div>
        </template>
    </view-grid>
</template>
<script setup lang="jsx">
    import extend from "@/extension/order/ocp_bom_completion/vw_OCP_Tech_BOM_Status_Monthly.jsx";
    import viewOptions from './vw_OCP_Tech_BOM_Status_Monthly/options.js'
    import { ref, reactive, getCurrentInstance, watch, onMounted } from "vue";
    const grid = ref(null);
    const { proxy } = getCurrentInstance()
    //http请求，proxy.http.post/get
    const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

    const padZero = (val) => val.toString().padStart(2, '0')
    const getCurrentMonth = () => {
        const now = new Date()
        return `${now.getFullYear()}-${padZero(now.getMonth() + 1)}`
    }

    const selectedMonth = ref(getCurrentMonth())

    const appendMonthFilter = (param) => {
        const ym = selectedMonth.value
        if (!ym) return
        param.wheres = param.wheres || []
        const [year, month] = ym.split('-').map(Number)
        const start = `${year}-${padZero(month)}-01`
        const endDate = new Date(year, month, 0).getDate()
        const end = `${year}-${padZero(month)}-${padZero(endDate)} 23:59:59`
        param.wheres.push({ name: 'OrderAuditDate', value: start, displayType: 'thanorequal' })
        param.wheres.push({ name: 'OrderAuditDate', value: end, displayType: 'lessorequal' })
    }

    let gridRef;//对应[表.jsx]文件中this.使用方式一样
    //生成对象属性初始化
    const onInit = async ($vm) => {
        gridRef = $vm;
        //gridRef.setFixedSearchForm(true);
        //与jsx中的this.xx使用一样，只需将this.xx改为gridRef.xx

    }
    //生成对象属性初始化后,操作明细表配置用到
    const onInited = async () => {
    }
    const searchBefore = async (param) => {
        //界面查询前,可以给param.wheres添加查询参数
        //返回false，则不会执行查询
        appendMonthFilter(param)
        return true;
    }
    const exportBefore = async (param) => {
        appendMonthFilter(param)
        return true
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
        //查询界面点击行事件
        // grid.value.toggleRowSelection(row); //单击行时选中当前行;
    }
    const modelOpenBefore = async (row) => {//弹出框打开后方法
        return true;//返回false，不会打开弹出框
    }
    const modelOpenAfter = (row) => {
        //弹出框打开后方法,设置表单默认值,按钮操作等
    }

    const triggerMonthFilter = () => {
        if (gridRef && gridRef.search) {
            gridRef.search()
        }
    }

    watch(() => selectedMonth.value, () => {
        triggerMonthFilter()
    })
    //监听表单输入，做实时计算
    //watch(() => editFormFields.字段,(newValue, oldValue) => {	})
    //对外暴露数据
    defineExpose({})
</script>

<style scoped>
.month-filter {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    margin-left: 12px;
}
</style>
