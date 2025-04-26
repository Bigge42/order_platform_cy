<template>
    <div style="padding: 0 4px;border-top: 10px solid #eee;">
        <h3>
            <i class="ivu-icon ivu-icon-ios-information-circle-outline"></i>订单明细
        </h3>
        <div style="padding:10px;background: white;padding-top: 0;">
            <vol-table ref="table" :loadKey="true" :columns="columns" :pagination-hide="true" :height="160"
                :defaultLoadPage="false" @loadBefore="loadBefore" :url="url" :row-index="true"
                :index="false"></vol-table>
        </div>
    </div>
</template>

<script>
import VolTable from "@/components/basic/VolTable.vue";
export default {
    components: {
        VolTable
    },
    methods: {
        loadBefore(params, callback) {//表格数据加载时触发
            //   这里也可以设置一些查询条件
            return callback(true);
        },
        gridRowClick(row) { //主表点击行加载明细表
            if (this.$refs.table) {
                //load方法可参照table组件api文档
                this.$refs.table.load({ value: row.Order_Id, sort: "CreateDate" })
            }
        },
        clearRows(){
            //主表没有数据时，清空明细数据
            this.$refs.table.rowData.splice(0)
        }
    },
    data() {
        return {
            url: 'api/Demo_Order/getDetailPage',//指定从某个接口获取table数据
            //明细表格配置，从生成的vue文件中可以复制过来
            columns: [{ field: 'OrderList_Id', title: 'OrderList_Id', type: 'guid', width: 110, hidden: true, readonly: true, align: 'left' },
            { field: 'Order_Id', title: 'Order_Id', type: 'guid', width: 110, hidden: true, align: 'left' },
            { field: 'GoodsId', title: '商品id', type: 'guid', width: 110, hidden: true, align: 'left' },
            { field: 'GoodsName', title: '商品名称', type: 'string', width: 120, align: 'left', sort: true },
            { field: 'GoodsCode', title: '商品编号', type: 'string', width: 120, align: 'left' },
            { field: 'Img', title: '商品图片', type: 'img', width: 100, align: 'left' },
            { field: 'Specs', title: '商品规格', type: 'string', bind: { key: '商品规格', data: [] }, width: 120, readonly: true, edit: { type: 'select' }, align: 'left' },
            { field: 'Price', title: '单价', type: 'decimal', width: 110, readonly: true, edit: { type: '' }, align: 'left' },
            { field: 'Qty', title: '数量', type: 'int', width: 110, edit: { type: '' }, align: 'left' },
            { field: 'Remark', title: '备注', type: 'string', width: 100, edit: { type: '' }, align: 'left' },
            { field: 'CreateID', title: 'CreateID', type: 'int', width: 80, hidden: true, align: 'left' },
            { field: 'Creator', title: '创建人', type: 'string', width: 100, align: 'left' },
            { field: 'CreateDate', title: '创建时间', type: 'datetime', width: 145, align: 'left', sort: true },
            { field: 'ModifyID', title: 'ModifyID', type: 'int', width: 80, hidden: true, align: 'left' },
            { field: 'Modifier', title: 'Modifier', type: 'string', width: 130, hidden: true, align: 'left' },
            { field: 'ModifyDate', title: 'ModifyDate', type: 'datetime', width: 110, hidden: true, align: 'left', sort: true }],
        }
    }
}
</script>
<style scoped>
h3 {
    font-weight: 500;
    padding-left: 10px;
    background: white;
    margin-top: 8px;
    padding-bottom: 5px;
}
</style>