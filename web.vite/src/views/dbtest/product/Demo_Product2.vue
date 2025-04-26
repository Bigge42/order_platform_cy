<!--
*Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *业务请在@/extension/dbtest/product/Demo_Product.js此处编写
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
               :extend="extend">
    </view-grid>
</template>
<script>
    import extend from "@/extension/dbtest/product/Demo_Product2.jsx";
    import { ref, defineComponent } from "vue";
    export default defineComponent({
        setup() {
            const table = ref({
                key: 'ProductId',
                footer: "Foots",
                cnName: '产品管理',
                name: 'product/Demo_Product',
                newTabEdit: false,
                url: "/Demo_Product/",
                sortName: "CreateDate"
            });
            const editFormFields = ref({"ProductName":"","ProductCode":"","Price":"","Remark":"","Creator":"","CreateDate":"","Modifier":"","ModifyDate":""});
            const editFormOptions = ref([[{"title":"商品名称","required":true,"field":"ProductName"},
                               {"title":"商品编号","required":true,"field":"ProductCode"},
                               {"title":"价格","required":true,"field":"Price","type":"number"},
                               {"title":"备注","field":"Remark"}],
                              [{"title":"创建人","field":"Creator","disabled":true},
                               {"title":"创建时间","field":"CreateDate","disabled":true},
                               {"title":"修改人","field":"Modifier","disabled":true},
                               {"title":"修改时间","field":"ModifyDate","disabled":true}]]);
            const searchFormFields = ref({"ProductName":"","ProductCode":"","AuditStatus":"","CreateDate":""});
            const searchFormOptions = ref([[{"title":"商品名称","field":"ProductName","type":"like"},{"title":"商品编号","field":"ProductCode","type":"like"},{"dataKey":"audit","data":[],"title":"审核状态","field":"AuditStatus","type":"select"},{"title":"创建时间","field":"CreateDate","type":"datetime"}]]);
            const columns = ref([{field:'ProductId',title:'ProductId',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ProductName',title:'商品名称',type:'string',link:true,width:200,require:true,align:'left'},
                       {field:'ProductCode',title:'商品编号',type:'string',width:120,require:true,align:'left'},
                       {field:'Price',title:'价格',type:'decimal',width:90,require:true,align:'left'},
                       {field:'AuditStatus',title:'审核状态',type:'int',bind:{ key:'audit',data:[]},width:80,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:120,align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:110,readonly:true,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:150,readonly:true,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:110,readonly:true,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:150,readonly:true,align:'left'}]);
            const detail = ref({columns:[]});
            const details = ref([  { 
                    cnName: '产品颜色',
                    table: 'Demo_ProductColor',
                    columns: [{field:'ProductColorId',title:'ProductColorId',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ProductId',title:'商品id',type:'guid',width:110,hidden:true,align:'left'},
                       {field:'Color',title:'颜色',type:'string',bind:{ key:'颜色',data:[]},width:100,edit:{type:'radio'},require:true,align:'left'},
                       {field:'Qty',title:'数量',type:'string',link:true,width:90,edit:{type:''},require:true,align:'left'},
                       {field:'Unit',title:'单位',type:'string',bind:{ key:'单位',data:[]},width:90,edit:{type:'select'},require:true,align:'left'},
                       {field:'Img',title:'图片',type:'img',width:90,edit:{type:'img'},require:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:130,edit:{type:''},align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:110,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:110,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:200,align:'left'}],
                    sortName: 'CreateDate',
                    key: 'ProductColorId',
                    buttons:[],
                    delKeys:[],
                    detail:{
                                    cnName: '产品颜色明细',
                                    table: 'Demo_ProductColorSub',
                                    firstTable: 'Demo_Product',
                                    secondTable: 'Demo_ProductColor',
                                    secondKey: 'ProductColorId',
                                    sortName: 'CreateDate',
                                    key: 'ProductColorSubId',
                                    buttons:[],
                                    delKeys:[],
                                    columns: [{field:'ProductColorSubId',title:'ProductColorSubId',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                                              {field:'ProductColorId',title:'产品颜色Id',type:'guid',width:110,hidden:true,align:'left'},
                                              {field:'FactoryLocation',title:'生产地',type:'string',width:100,edit:{type:''},require:true,align:'left'},
                                              {field:'Manufacturer',title:'生产商',type:'string',width:100,edit:{type:''},require:true,align:'left'},
                                              {field:'ProductionDate',title:'生产日期',type:'date',width:110,edit:{type:'date'},align:'left'},
                                              {field:'Quantity',title:'数量',type:'int',width:110,edit:{type:'number'},require:true,align:'left'},
                                              {field:'Price',title:'价格',type:'decimal',width:110,edit:{type:'number'},require:true,align:'left'},
                                              {field:'Remark',title:'备注',type:'string',width:100,edit:{type:''},align:'left'},
                                              {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                                              {field:'Creator',title:'创建人',type:'string',width:80,align:'left'},
                                              {field:'CreateDate',title:'创建时间',type:'datetime',width:100,align:'left'},
                                              {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                                              {field:'Modifier',title:'Modifier',type:'string',width:130,hidden:true,align:'left'},
                                              {field:'ModifyDate',title:'ModifyDate',type:'datetime',width:110,hidden:true,align:'left'},]
                             }
                                            },
                    { 
                    cnName: '产品尺寸',
                    table: 'Demo_ProductSize',
                    columns: [{field:'ProductSizeId',title:'ProductSizeId',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ProductId',title:'商品id',type:'guid',width:110,hidden:true,align:'left'},
                       {field:'Size',title:'尺寸',type:'string',bind:{ key:'尺寸',data:[]},width:120,edit:{type:'select'},require:true,align:'left'},
                       {field:'Unit',title:'单位',type:'string',bind:{ key:'单位',data:[]},width:90,edit:{type:'select'},require:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',link:true,width:120,edit:{type:''},align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:120,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:120,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:200,align:'left'}],
                    sortName: 'CreateDate',
                    key: 'ProductSizeId',
                    buttons:[],
                    delKeys:[],
                    detail:{
                                    cnName: '产品尺寸明细',
                                    table: 'Demo_ProductSizeSub',
                                    firstTable: 'Demo_Product',
                                    secondTable: 'Demo_ProductSize',
                                    secondKey: 'ProductSizeId',
                                    sortName: 'CreateDate',
                                    key: 'ProductSizeSubId',
                                    buttons:[],
                                    delKeys:[],
                                    columns: [{field:'ProductSizeSubId',title:'ProductSizeSubId',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                                              {field:'ProductSizeId',title:'产品尺寸ID',type:'guid',width:110,hidden:true,align:'left'},
                                              {field:'ChestCircumference',title:'胸围尺寸',type:'string',width:90,edit:{type:''},require:true,align:'left'},
                                              {field:'WaistCircumference',title:'腰围尺寸',type:'string',width:90,edit:{type:''},require:true,align:'left'},
                                              {field:'HipCircumference',title:'臀围尺寸',type:'string',width:90,edit:{type:''},require:true,align:'left'},
                                              {field:'ShoulderWidth',title:'肩宽尺寸',type:'string',width:90,edit:{type:''},require:true,align:'left'},
                                              {field:'Remark',title:'备注',type:'string',width:100,edit:{type:''},align:'left'},
                                              {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                                              {field:'Creator',title:'创建人',type:'string',width:80,align:'left'},
                                              {field:'CreateDate',title:'创建时间',type:'datetime',width:140,align:'left'},
                                              {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                                              {field:'Modifier',title:'Modifier',type:'string',width:130,hidden:true,align:'left'},
                                              {field:'ModifyDate',title:'ModifyDate',type:'datetime',width:110,hidden:true,align:'left'},]
                             }
                                            }]);
            return {
                table,
                extend,
                editFormFields,
                editFormOptions,
                searchFormFields,
                searchFormOptions,
                columns,
                detail,
                details
            };
        },
    });
</script>
