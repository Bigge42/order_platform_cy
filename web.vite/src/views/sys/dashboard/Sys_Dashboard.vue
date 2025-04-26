<!--
*Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *业务请在@/extension/sys/dashboard/Sys_Dashboard.jsx此处编写
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
    import extend from "@/extension/sys/dashboard/Sys_Dashboard.jsx";
    import { ref, defineComponent } from "vue";
    export default defineComponent({
        setup() {
            const table = ref({
                key: 'DashboardId',
                footer: "Foots",
                cnName: '工作台',
                name: 'dashboard/Sys_Dashboard',
                newTabEdit: false,
                url: "/Sys_Dashboard/",
                sortName: "CreateDate"
            });
            const editFormFields = ref({"DashboardName":"","Remark":""});
            const editFormOptions = ref([[{"title":"名称","required":true,"field":"DashboardName"}],
                              [{"title":"备注","field":"Remark","type":"textarea"}]]);
            const searchFormFields = ref({"DashboardName":"","DashboardCode":"","Creator":"","CreateDate":""});
            const searchFormOptions = ref([[{"title":"名称","field":"DashboardName","type":"like"},{"title":"编号","field":"DashboardCode","type":"like"},{"title":"创建人","field":"Creator","type":"like"},{"title":"创建时间","field":"CreateDate","type":"date"}]]);
            const columns = ref([{field:'DashboardId',title:'DashboardId',type:'string',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'DashboardName',title:'名称',type:'string',link:true,width:120,require:true,align:'left'},
                       {field:'DashboardCode',title:'编号',type:'string',width:180,align:'left'},
                       {field:'ParentId',title:'父级id',type:'string',width:110,hidden:true,align:'left'},
                       {field:'Img',title:'图片',type:'img',width:70,align:'left'},
                       {field:'Options',title:'参数配置',type:'string',width:110,hidden:true,align:'left'},
                       {field:'Html',title:'Html',type:'string',width:110,hidden:true,align:'left'},
                       {field:'Enable',title:'Enable',type:'int',width:110,hidden:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:100,align:'left'},
                       {field:'UserId',title:'UserId',type:'int',width:110,hidden:true,align:'left'},
                       {field:'TenancyId',title:'TenancyId',type:'string',width:120,hidden:true,align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:90,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:130,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:130,hidden:true,align:'left'},
                       {field:'DbServiceId',title:'DbServiceId',type:'string',width:110,hidden:true,align:'left'}]);
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
