<!--
*Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *业务请在@/extension/dbtest/product/Demo_ProductSize.js此处编写
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
    import extend from "@/extension/dbtest/product/Demo_ProductSize.jsx";
    import { ref, defineComponent } from "vue";
    export default defineComponent({
        setup() {
            const table = ref({
                key: 'ProductSizeId',
                footer: "Foots",
                cnName: '产品尺寸',
                name: 'product/Demo_ProductSize',
                newTabEdit: false,
                url: "/Demo_ProductSize/",
                sortName: "CreateDate"
            });
            const editFormFields = ref({"Size":"","Unit":"","Remark":""});
            const editFormOptions = ref([[{"dataKey":"尺寸","data":[],"title":"尺寸","required":true,"field":"Size","type":"select"}],
                              [{"dataKey":"单位","data":[],"title":"单位","required":true,"field":"Unit","type":"select"}],
                              [{"title":"备注","field":"Remark"}]]);
            const searchFormFields = ref({"Size":"","Unit":"","CreateDate":"","ModifyDate":""});
            const searchFormOptions = ref([[{"dataKey":"尺寸","data":[],"title":"尺寸","field":"Size","type":"select"},{"dataKey":"单位","data":[],"title":"单位","field":"Unit","type":"select"},{"title":"创建时间","field":"CreateDate","type":"datetime"},{"title":"修改时间","field":"ModifyDate","type":"datetime"}]]);
            const columns = ref([{field:'ProductSizeId',title:'ProductSizeId',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'ProductId',title:'商品id',type:'guid',width:110,hidden:true,align:'left'},
                       {field:'Size',title:'尺寸',type:'string',bind:{ key:'尺寸',data:[]},width:120,require:true,align:'left',sort:true},
                       {field:'Unit',title:'单位',type:'string',bind:{ key:'单位',data:[]},width:90,require:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',link:true,width:120,align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:120,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:150,align:'left',sort:true},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:120,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:200,align:'left',sort:true}]);
            const detail = ref([]);
            const details = ref([]);
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
