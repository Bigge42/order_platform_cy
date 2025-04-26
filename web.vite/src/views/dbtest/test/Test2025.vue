<!--
*Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *业务请在@/extension/dbtest/test/Test2025.jsx此处编写
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
    import extend from "@/extension/dbtest/test/Test2025.jsx";
    import { ref, defineComponent } from "vue";
    export default defineComponent({
        setup() {
            const table = ref({
                key: 'Id',
                footer: "Foots",
                cnName: '测试表2025',
                name: 'test/Test2025',
                newTabEdit: true,
                url: "/Test2025/",
                sortName: "Id"
            });
            const editFormFields = ref({"Name":"","Address":"","OrderCode":"","Age":""});
            const editFormOptions = ref([[{"title":"名字","field":"Name"}],
                              [{"title":"地址","field":"Address"}],
                              [{"dataKey":"订单状态","data":[],"title":"编号","field":"OrderCode","type":"select"}],
                              [{"title":"Age","field":"Age","type":"number"}]]);
            const searchFormFields = ref({"Name":"","Address":"","OrderCode":"","CreateDate":""});
            const searchFormOptions = ref([[{"title":"名字","field":"Name","type":"like"},{"title":"地址","field":"Address","type":"like"},{"dataKey":"订单状态","data":[],"title":"编号","field":"OrderCode"},{"title":"创建时间","field":"CreateDate","type":"datetime"}]]);
            const columns = ref([{field:'Id',title:'Id',type:'int',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'Name',title:'名字',type:'string',link:true,width:120,align:'left'},
                       {field:'Address',title:'地址',type:'string',width:120,align:'left'},
                       {field:'OrderCode',title:'编号',type:'string',bind:{ key:'订单状态',data:[]},width:120,align:'left'},
                       {field:'Age',title:'Age',type:'int',width:80,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:110,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'}]);
            const detail = ref({columns:[]});
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
