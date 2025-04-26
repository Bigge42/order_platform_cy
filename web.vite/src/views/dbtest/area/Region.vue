<!--
*Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *业务请在@/extension/system/area/Region.js此处编写
 -->
<template>
    <view-grid ref="grid"
               :columns="columns"
               :detail="detail"
               :editFormFields="editFormFields"
               :editFormOptions="editFormOptions"
               :searchFormFields="searchFormFields"
               :searchFormOptions="searchFormOptions"
               :table="table"
               :extend="extend">
    </view-grid>
</template>
<script>
    import extend from "@/extension/demo/area/Region.js";
    import { ref, defineComponent } from "vue";
    export default defineComponent({
        setup() {
            const table = ref({
                key: 'id',
                footer: "Foots",
                cnName: '省市县',
                name: 'area/Region',
                url: "/Region/",
                sortName: "id"
            });
            const editFormFields = ref({});
            const editFormOptions = ref([]);
            const searchFormFields = ref({"Name":"","Code":"","ParentId":"","Level":"","Mername":""});
            const searchFormOptions = ref([[{"title":"省市县","field":"Name","type":"like"},{"title":"编号","field":"Code"},{"title":"父级编号","field":"ParentId","type":"number"}],[{"title":"级别","field":"Level","type":"number"}],[{"title":"完整级别","field":"Mername"}]]);
            const columns = ref([{field:'id',title:'id',type:'int',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'Name',title:'省市县',type:'string',sort:true,width:120,align:'left',sort:true},
                       {field:'Code',title:'编号',type:'string',sort:true,width:110,align:'left'},
                       {field:'ParentId',title:'父级编号',type:'int',width:110,align:'left'},
                       {field:'Level',title:'级别',type:'int',sort:true,width:90,align:'left'},
                       {field:'Mername',title:'完整级别',type:'string',sort:true,width:180,align:'left'},
                       {field:'Lng',title:'精度',type:'float',width:110,align:'left'},
                       {field:'Lat',title:'纬度',type:'float',width:110,align:'left'},
                       {field:'pinyin',title:'拼音',type:'string',width:120,align:'left'}]);
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
