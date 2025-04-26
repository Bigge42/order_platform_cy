<!--
*Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *业务请在@/extension/sys/db/Sys_DbService.js此处编写
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
    import extend from "@/extension/sys/db/Sys_DbService.js";
    import { ref, defineComponent } from "vue";
    export default defineComponent({
        setup() {
            const table = ref({
                key: 'DbServiceId',
                footer: "Foots",
                cnName: '租户管理',
                name: 'db/Sys_DbService',
                url: "/Sys_DbService/",
                sortName: "CreateDate"
            });
            const editFormFields = ref({"DbServiceName":"","GroupId":"","DbIpAddress":"","DatabaseName":"","UserId":"","Pwd":"","PhoneNo":"","Address":"","Remark":""});
            const editFormOptions = ref([[{"title":"租户名称","required":true,"field":"DbServiceName"},
                               {"dataKey":"集团","data":[],"title":"所属集团","field":"GroupId","type":"select"}],
                              [{"title":" 数据库IP","field":"DbIpAddress"},
                               {"title":"数据库名","field":"DatabaseName"}],
                              [{"title":"账号","field":"UserId"},
                               {"title":"密码","field":"Pwd"}],
                              [{"title":"手机号","field":"PhoneNo"},
                               {"title":"地址","field":"Address"}],
                              [{"title":"备注","field":"Remark","colSize":12,"type":"textarea"}]]);
            const searchFormFields = ref({"DbServiceName":"","DbIpAddress":"","PhoneNo":"","Address":""});
            const searchFormOptions = ref([[{"title":"租户名称","field":"DbServiceName","type":"like"},{"title":" 数据库IP","field":"DbIpAddress","type":"like"},{"title":"手机号","field":"PhoneNo"},{"title":"地址","field":"Address","type":"like"}]]);
            const columns = ref([{field:'DbServiceId',title:'DbServiceId',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'DbServiceName',title:'租户名称',type:'string',link:true,width:150,require:true,align:'left',sort:true},
                       {field:'GroupId',title:'所属集团',type:'guid',bind:{ key:'集团',data:[]},width:170,align:'left'},
                       {field:'DbIpAddress',title:' 数据库IP',type:'string',width:110,align:'left'},
                       {field:'DatabaseName',title:'数据库名',type:'string',width:100,align:'left'},
                       {field:'UserId',title:'账号',type:'string',width:100,hidden:true,align:'left'},
                       {field:'PhoneNo',title:'手机号',type:'string',width:130,align:'left'},
                       {field:'Address',title:'地址',type:'string',width:140,align:'left'},
                       {field:'Enable',title:'是否可用',type:'int',width:110,hidden:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:100,hidden:true,align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:150,align:'left',sort:true},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:130,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:150,hidden:true,align:'left',sort:true}]);
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
