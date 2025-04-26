<!--
*Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *业务请在@/extension/sys/rule/Sys_CodeRule.jsx此处编写
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
    import extend from "@/extension/sys/rule/Sys_CodeRule.jsx";
    import { ref, defineComponent } from "vue";
    export default defineComponent({
        setup() {
            const table = ref({
                key: 'CodeRuleId',
                footer: "Foots",
                cnName: '单据编码',
                name: 'rule/Sys_CodeRule',
                newTabEdit: false,
                url: "/Sys_CodeRule/",
                sortName: "CreateDate"
            });
            const editFormFields = ref({"TableName":"","Name":"","Field":"","OrderFiled":"","PrefixCode":"","RuleType":"","ValueLen":"","RuleIncremental":"","Code":"","ConcatenationSymbol":""});
            const editFormOptions = ref([[{"title":"业务名称","field":"TableName"},
                               {"title":"规则名称","required":true,"field":"Name"}],
                              [{"title":"单据字段","field":"Field"},
                               {"title":"日期排序字段","field":"OrderFiled"}],
                              [{"title":"前缀","field":"PrefixCode"},
                               {"title":"规则类型","required":true,"field":"RuleType"}],
                              [{"title":"编号位数","required":true,"field":"ValueLen","type":"number"},
                               {"title":"增长类型","field":"RuleIncremental"}],
                              [{"title":"连接符号","field":"ConcatenationSymbol"},
                               {"title":"编码预览","field":"Code","disabled":true}]]);
            const searchFormFields = ref({"TableCNName":"","Name":"","Code":""});
            const searchFormOptions = ref([[{"title":"业务名称","field":"TableCNName","type":"like"},{"title":"规则名称","field":"Name","type":"like"},{"title":"编码预览","field":"Code"}]]);
            const columns = ref([{field:'CodeRuleId',title:'CodeRuleId',type:'string',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'TableName',title:'业务名称',type:'string',width:100,hidden:true,align:'left'},
                       {field:'TableCNName',title:'业务名称',type:'string',link:true,width:100,align:'left'},
                       {field:'Field',title:'单据字段',type:'string',width:120,align:'left'},
                       {field:'CodeText2',title:'日期排序字段名称',type:'string',width:180,hidden:true,align:'left'},
                       {field:'Name',title:'规则名称',type:'string',width:100,hidden:true,require:true,align:'left'},
                       {field:'Code',title:'编码预览',type:'string',width:120,hidden:true,readonly:true,align:'left'},
                       {field:'PrefixCode',title:'前缀',type:'string',width:70,align:'left'},
                       {field:'RuleType',title:'规则类型',type:'string',width:80,require:true,align:'left'},
                       {field:'ValueLen',title:'编号位数',type:'int',width:70,require:true,align:'left'},
                       {field:'RuleIncremental',title:'增长类型',type:'string',width:90,hidden:true,align:'left'},
                       {field:'OrderFiled',title:'日期排序字段',type:'string',width:120,align:'left'},
                       {field:'ConcatenationSymbol',title:'连接符号',type:'string',width:90,hidden:true,align:'left'},
                       {field:'CodeText1',title:'单据字段名称',type:'string',width:180,hidden:true,align:'left'},
                       {field:'Enable',title:'Enable',type:'int',width:110,hidden:true,align:'left'},
                       {field:'UserId',title:'UserId',type:'int',width:110,hidden:true,align:'left'},
                       {field:'TenancyId',title:'TenancyId',type:'string',width:120,hidden:true,align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:90,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:140,align:'left'},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:90,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'修改时间',type:'datetime',width:140,hidden:true,align:'left'},
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
