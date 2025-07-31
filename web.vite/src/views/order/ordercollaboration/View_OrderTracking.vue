<!-- 缺料运算 -->
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
               :searchAfter="searchAfter"
               :addBefore="addBefore"
               :updateBefore="updateBefore"
               :rowClick="rowClick"
               :modelOpenBefore="modelOpenBefore"
               :modelOpenAfter="modelOpenAfter">
        <!-- 自定义组件数据槽扩展，更多数据槽slot见文档 -->
        <template #gridHeader>
        </template>
    </view-grid>

    <!-- 运算方案弹窗 -->
    <vol-box
        v-model="dialogVisible"
        title="生成运算结果"
        :width="500"
        :before-close="handleDialogClose">
        <el-form
            ref="formRef"
            :model="formData"
            :rules="formRules"
            label-width="120px">
            <el-form-item label="运算人：">
                <el-input
                    v-model="formData.operator"
                    disabled
                    placeholder="当前登录用户">
                </el-input>
            </el-form-item>
            <el-form-item label="运算日期：">
                <el-date-picker
                    v-model="formData.computeDate"
                    type="datetime"
                    placeholder="选择日期时间"
                    disabled
                    style="width: 100%"
                    format="YYYY-MM-DD HH:mm:ss"
                    value-format="YYYY-MM-DD HH:mm:ss">
                </el-date-picker>
            </el-form-item>
            <el-form-item label="自定义名称：" prop="customName">
                <el-input
                    v-model="formData.customName"
                    placeholder="请输入自定义名称"
                    maxlength="50"
                    show-word-limit>
                </el-input>
            </el-form-item>
            <el-form-item label="运算方案名：">
                <el-input
                    v-model="computedSchemeName"
                    disabled
                    placeholder="自动生成的方案名">
                </el-input>
            </el-form-item>
        </el-form>
        
        <template #footer>
            <span class="dialog-footer">
                <el-button @click="handleDialogClose">取 消</el-button>
                <el-button type="primary" @click="handleConfirm" :loading="submitLoading">
                    确 定
                </el-button>
            </span>
        </template>
    </vol-box>
</template>
<script setup lang="jsx">
    import extend from "@/extension/order/ordercollaboration/View_OrderTracking.jsx";
    import viewOptions from './View_OrderTracking/options.js'
    import { ref, reactive, getCurrentInstance, watch, onMounted, computed } from "vue";
    import { ElMessage, ElMessageBox } from 'element-plus';
    import { useStore } from 'vuex';
    import VolBox from '@/components/basic/VolBox.vue';
    import dayjs from 'dayjs';
    
    const grid = ref(null);
    const formRef = ref(null);
    const store = useStore();
    const { proxy } = getCurrentInstance()
    //http请求，proxy.http.post/get
    const { table, editFormFields, editFormOptions, searchFormFields, searchFormOptions, columns, detail, details } = reactive(viewOptions())

    // 获取当前日期时间字符串
    const getCurrentDateTime = () => {
        return dayjs().format('YYYY-MM-DD HH:mm:ss');
    };

    // 弹窗相关数据
    const dialogVisible = ref(false);
    const submitLoading = ref(false);
    const formData = reactive({
        operator: '', // 运算人
        computeDate: getCurrentDateTime(), // 运算日期
        customName: '', // 自定义名称
        queryConditions: '' // 当前页面查询条件
    });

    // 表单验证规则
    const formRules = {
        customName: [
            { required: true, message: '请输入自定义名称', trigger: 'blur' },
            { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' }
        ]
    };

    // 计算属性：运算方案名
    const computedSchemeName = computed(() => {
        if (!formData.operator || !formData.customName) return '';
        const formattedDateTime = dayjs(formData.computeDate).format('YYYYMMDDHHmmss');
        return `${formData.operator}-${formattedDateTime}-${formData.customName}`;
    });



    // 获取用户真实姓名
    const getUserTrueName = () => {
        const userInfo = store.getters.getUserInfo();
        return userInfo?.userTrueName || userInfo?.userName || '未知用户';
    };

    let gridRef;//对应[表.jsx]文件中this.使用方式一样
    //生成对象属性初始化
    const onInit = async ($vm) => {
        gridRef = $vm;
        gridRef.pagination.sizes = [20, 50, 100, 200, 500, 1000];
        //设置默认分页数
        gridRef.pagination.size = 20;
        // gridRef.setFixedSearchForm(true);
        
        // 设置快捷查询字段
        gridRef.queryFields = ['ContractNo', 'SOBillNo', 'MtoNo', 'MaterialNumber'];
        
        // 设置主表合计字段
        columns.forEach(x => {
            if (x.field == 'InstockQty') {
                x.summary = true;
                x.numberLength = 2;
                x.summaryFormatter = (val, column, rows, summaryData) => {
                    if (!val) return '0.00';
                    summaryData[0] = '汇总';
                    return (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
                };
            }
            if (x.field == 'UnInstockQty') {
                x.summary = true;
                x.numberLength = 2;
            }
            if (x.field == 'OrderQty') {
                x.summary = true;
                x.numberLength = 2;
            }
            if (x.field == 'Amount') {
                x.summary = true;
                x.numberLength = 2;
                x.summaryFormatter = (val, column, rows, summaryData) => {
                    if (!val) return '0.00';
                    return '￥' + (val + '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
                };
            }
        })
        
        gridRef.buttons.push({
            name: '生成运算结果',
            type: 'primary',
            onClick: function () {
                openComputeDialog();
            }
        })
    }
    //生成对象属性初始化后,操作明细表配置用到
    const onInited = async () => {
        
    }
    const searchBefore = async (param) => {
        //界面查询前,可以给param.wheres添加查询参数
        //返回false，则不会执行查询
        
        // 获取查询条件并赋值给弹窗使用的字段
        if (param && param.wheres) {
            formData.queryConditions = param.wheres;
            console.log('searchBefore获取查询条件：', formData.queryConditions);
        }
        
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
        //查询界面点击行事件
        // grid.value.toggleRowSelection(row); //单击行时选中当前行;
    }
    const modelOpenBefore = async (row) => {//弹出框打开后方法
        return true;//返回false，不会打开弹出框
    }
    const modelOpenAfter = (row) => {
        //弹出框打开后方法,设置表单默认值,按钮操作等
    }
    // 打开运算弹窗
    const openComputeDialog = () => {
        // 检查表格是否有数据
        const tableData = gridRef.getTable(true).rowData;
        if (!tableData || tableData.length === 0) {
            ElMessage.warning('当前页面无数据，无法生成运算结果');
            return;
        }
        
        // 初始化表单数据
        formData.operator = getUserTrueName();
        formData.computeDate = getCurrentDateTime();
        formData.customName = '';
        
        // 获取当前页面查询条件
        if (gridRef && gridRef.param && gridRef.param.wheres) {
            formData.queryConditions = gridRef.param.wheres;
            console.log('当前查询条件：', formData.queryConditions);
        }
        
        dialogVisible.value = true;
    };

    // 关闭弹窗
    const handleDialogClose = () => {
        dialogVisible.value = false;
        // 重置表单
        if (formRef.value) {
            formRef.value.resetFields();
        }
    };

    // 确认提交
    const handleConfirm = async () => {
        if (!formRef.value) return;
        
        try {
            // 表单验证
            await formRef.value.validate();
            
            submitLoading.value = true;
            
            // 准备提交数据
            const submitData = {
                "mainData": {
                    "PlanName": computedSchemeName.value, // 运算方案名
                    "Filter": JSON.stringify(formData.queryConditions), // 查询条件
                    "ComputeID": null
                },
                "detailData": null,
                "delKeys": null
            };
            
            console.log('提交数据：', submitData);
            
            // 调用后端API提交数据
            const result = await proxy.http.post('/api/OCP_LackMtrlPlan/add', submitData);
            
            if (result && result.status) {
                ElMessage.success('运算方案创建成功！');
                handleDialogClose();
            } else {
                ElMessage.error(result?.message || '运算方案创建失败');
            }
            
        } catch (error) {
            console.error('提交失败：', error);
            if (error !== 'cancel') {
                ElMessage.error('提交失败，请检查输入信息');
            }
        } finally {
            submitLoading.value = false;
        }
    };

    //监听表单输入，做实时计算
    //watch(() => editFormFields.字段,(newValue, oldValue) => {	})
    //对外暴露数据
    defineExpose({})
</script>
