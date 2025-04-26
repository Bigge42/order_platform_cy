<!--
*Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *业务请在@/extension/dbtest/goods/Demo_Goods.js此处编写
 -->
<template>
    <view-grid ref="grid" :columns="columns" :detail="detail" :editFormFields="editFormFields"
        :editFormOptions="editFormOptions" :searchFormFields="searchFormFields" :searchFormOptions="searchFormOptions"
        :table="table" :extend="extend">
    </view-grid>
</template>
<script>
import extend from "@/extension/dbtest/goods/Demo_GoodsMerge.jsx";
import { ref, defineComponent } from "vue";
export default defineComponent({
    setup() {
        const table = ref({
            key: 'GoodsId',
            footer: "Foots",
            cnName: '字段合并',
            name: 'goods/Demo_Goods',
            url: "/Demo_Goods/",
            sortName: "CreateDate"
        });
        const editFormFields = ref({ "GoodsName": "", "CatalogId": [], "GoodsCode": "","Specs":"", "Price": "", "Remark": "", "Img": "" });
        const editFormOptions = ref([[{ "title": "商品名称", "required": true, "field": "GoodsName", "type": "text" }],
        [{ "dataKey": "分类级联", "data": [], "title": "所属分类", "field": "CatalogId", "type": "cascader" }],
        [{ "title": "商品编号", "required": true, "field": "GoodsCode", "type": "text" }],
        [{ "title": "单价", "required": true, "field": "Price", "type": "decimal" }],
        [{ "title": "规格", "required": true, "field": "Specs", "type": "text" }],
        [{ "title": "备注", "field": "Remark", "colSize": 12, "type": "textarea" }],
        [{ "title": "商品图片", "field": "Img", "type": "img" }]]);
        const searchFormFields = ref({ "GoodsName": "", "CatalogId": [], "GoodsCode": "" });
        const searchFormOptions = ref([[{ "title": "商品名称", "field": "GoodsName", "type": "like" }, { "dataKey": "分类级联", "data": [], "title": "所属分类", "field": "CatalogId", "type": "cascader" }, { "title": "商品编号", "field": "GoodsCode" }]]);
        const columns = ref([
            { field: 'GoodsId', title: '商品ID', type: 'guid', width: 110, hidden: true, readonly: true, require: true, align: 'left' },
            { field: 'GoodsName', title: '商品名称', type: 'string', link: true, width: 120, require: true, align: 'left' },
            { field: 'CatalogId', title: '所属分类', type: 'guid', bind: { key: '分类级联', data: [] }, sort: true, width: 110, align: 'left' },
            { field: 'GoodsCode', title: '商品编号', type: 'string', sort: true, width: 100, require: true, align: 'left' },
            { field: 'Img', title: '商品图片', type: 'img', width: 80, align: 'left' },
            { field: 'Specs', title: '规格', type: 'string', width: 60, align: 'left' },
            { field: 'Price', title: '单价', type: 'decimal', sort: true, width: 70, require: true, align: 'left' },
            { field: 'Enable', title: '启用', type: 'int', bind: { key: 'enable', data: [] }, sort: true, width: 90, align: 'left' },
            { field: 'Remark', title: '备注', type: 'string', width: 100, hidden: true, align: 'left' },
            { field: 'CreateID', title: 'CreateID', type: 'int', width: 80, hidden: true, align: 'left' },
            { field: 'Creator', title: '创建人', type: 'string', width: 80, align: 'left' },
            { field: 'CreateDate', title: '创建时间', type: 'datetime', width: 140, align: 'left'},
            { field: 'ModifyID', title: 'ModifyID', type: 'int', width: 80, hidden: true, align: 'left' },
            { field: 'Modifier', title: '修改人', type: 'string', width: 90, align: 'left' },
            { field: 'ModifyDate', title: '修改时间', type: 'datetime', width: 140, align: 'left'}]);
        const detail = ref({
            cnName: "#detailCnName",
            table: "#detailTable",
            columns: [],
            sortName: "",
            key: ""
        });
        return {
            table,
            extend,
            editFormFields,
            editFormOptions,
            searchFormFields,
            searchFormOptions,
            columns,
            detail,
        };
    },
});
</script>
