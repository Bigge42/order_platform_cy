<template>
    <!-- 弹出框一 -->
    <vol-box :lazy="true" v-model="model1" title="弹出框1" :width="700" :padding="5" :onModelClose="onModelClose">
        <div style="height:400px;">弹出1框内容, 选中传入的行数据
            <p>
                {{ JSON.stringify(model1Rows) }}
            </p>
        </div>
        <template #footer>
            <div>
                <el-button type="primary" plain size="small" @click="callParent">调用生成(父)页面对象</el-button>
                <el-button type="primary" size="small" @click="closeModel1">确认</el-button>
                <el-button type="default" size="small" @click="model1 = false">关闭</el-button>
            </div>
        </template>
    </vol-box>

    <!-- 弹出框二 -->
    <vol-box :lazy="true" v-model="model2" title="弹出框1" :width="700" :padding="5" :onModelClose="onModelClose">
        <div style="height:400px;">弹出2框内容, 当前点击的行数据
            <p>
                {{ JSON.stringify(model2Row) }}
            </p>
        </div>
        <template #footer>
            <div>
                <el-button type="primary" plain size="small" @click="callParent">调用生成(父)页面对象</el-button>
                <el-button type="primary" size="small" @click="closeModel2">确认</el-button>
                <el-button type="default" size="small" @click="model2 = false">关闭</el-button>
            </div>
        </template>
    </vol-box>
</template>
<script>
import VolBox from '@/components/basic/VolBox.vue';
//这里使用的vue2语法，也可以写成vue3语法
export default {
    emits:['parentCall'],
    components: { 'vol-box': VolBox },
    methods: {},
    data() {
        return {
            model1: false,
            model1Rows: [], //弹出框1传入的选中行
            model2: false,
            model2Row: {},//弹出框2点击的当前行数据
        };
    },
    methods: {
        openModel1(rows) { //弹出框1
            this.model1Rows = rows;
            this.model1 = true;
        },
        openModel2(row, column, index) {//弹出框2
            this.model2Row = row;
            this.model2 = true;
        },
        closeModel1() {
            this.model1 = false;
        },
        closeModel2() {
            this.model2 = false;
        },
        callParent() {
            this.$emit('parentCall', $parent => {
                //$parent就是生成页面的对象，比如可以调用页面刷新，可获取的属性与方法见【生成页面文档】,$parent.xxx直接调用
                //调用刷新
                $parent.search();
                //获取所有选中的行数据
                // $parent.getSelectRows();
                this.$message.success('调用了刷新')
            })
        },
        onModelClose() {
            this.$message.success('弹出框右上角点击x关闭事件')
        }
    }
};
</script>
  