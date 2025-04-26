
<template>
    <div class="report-btns"><el-button @click="print" type="primary" plain><i class="el-icon-printer"></i>
            {{ $ts('打印') }}</el-button>
        <el-button @click="() => { download('xls', 'attachment') }" type="primary" plain> <i class="el-icon-document"></i>{{
            $ts('导出Excel') }}</el-button>
        <!-- <el-button @click="() => { download('pdf', 'attachment') }" type="primary" plain><i
                class="el-icon-document-checked"></i>导出PDF</el-button> -->
    </div>
    <div id="report_holder">
    </div>
</template>
<style>
@media print {
    body * {
        visibility: hidden;
    }

    #report_holder,
    #report_holder * {
        visibility: visible;
    }

    #report_holder {
        position: fixed;
        width: 100% !important;
        left: 0;
        right: 0;
        bottom: 0;
        top: 0;
        display: flex;
        flex-direction: column;
        align-items: center;
        /* left: 0;
        right: 0; */
    }

}
</style>
<script>
import ("./grhtml5-6.8-min");
// import print from 'print-js'

export default {
    methods: {
        download(type, open) {
            const params = {
                type: type,
                filename: "test123"
            }
            const url = "api/report/download?code=" + this.$route.query.code;
            this.http.download(url, params, params.filename + '.' + type, true, () => {

            })
        },
        print() {
            window.print();
        }
    },
    async mounted() {
        var reportViewer = window.rubylong.grhtml5.insertReportViewer("report_holder");
        reportViewer.reportPrepared = false; //指定报表生成需要加载报表模板
        reportViewer.dataPrepared = false;   //指定报表生成需要加载报表数据 
        await this.http.get("api/report/GetTemplateData?code=" + this.$route.query.code).then(x => {
            if (!x.status) {
                return this.$message.error(x.message);
            }
            reportViewer.loadReport(x.data.text);
            reportViewer.loadData(x.data.data || []);
        })
        reportViewer.start();
    }
}
</script>

<style scoped>
#report_holder {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.report-btns {
    padding: 20px 0 5px 0;
    display: flex;
    align-items: center;
    justify-content: center;
}
</style>