<!--
*Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *业务请在@/extension/dbtest/order/Demo_Order.js此处编写
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
    import extend from "@/extension/dbtest/order/Demo_OrderStats.jsx";
    import { ref, defineComponent } from "vue";
    export default defineComponent({
        setup() {
            const table = ref({
                key: 'Order_Id',
                footer: "Foots",
                cnName: '订单管理',
                name: 'order/Demo_Order',
                newTabEdit: false,
                url: "/Demo_Order/",
                sortName: "CreateDate"
            });
            const editFormFields = ref({"OrderNo":"","OrderType":"","TotalPrice":"","TotalQty":"","OrderDate":"","Customer":"","PhoneNo":"","OrderStatus":""});
            const editFormOptions = ref([[{"title":"订单编号","required":true,"field":"OrderNo","disabled":true},
                               {"dataKey":"订单类型","data":[],"title":"订单类型","required":true,"field":"OrderType","type":"select"},
                               {"title":"总价","field":"TotalPrice","type":"decimal"},
                               {"title":"总数量","field":"TotalQty","type":"number"}],
                              [{"title":"订单日期","required":true,"field":"OrderDate","type":"datetime"},
                               {"title":"客户","field":"Customer","disabled":true,"type":"selectTable"},
                               {"title":"手机","field":"PhoneNo","disabled":true,"type":"text"},
                               {"dataKey":"订单状态","data":[],"title":"订单状态","required":true,"field":"OrderStatus","type":"select"}]]);
            const searchFormFields = ref({"OrderNo":"","OrderType":"","TotalPrice":[null,null],"OrderDate":"","OrderStatus":"","CreateDate":""});
            const searchFormOptions = ref([[{"title":"订单编号","field":"OrderNo"},{"dataKey":"订单状态","data":[],"title":"订单类型","field":"OrderType","type":"select"},{"title":"总价","field":"TotalPrice","type":"range"}],[{"title":"订单日期","field":"OrderDate","type":"datetime"},{"dataKey":"订单状态","data":[],"title":"订单状态","field":"OrderStatus","type":"select"},{"title":"创建时间","field":"CreateDate","type":"datetime"}]]);
            const columns = ref([{field:'Order_Id',title:'Order_Id',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'OrderNo',title:'订单编号',type:'string',link:true,width:130,readonly:true,require:true,align:'left',sort:true},
                       {field:'OrderType',title:'订单类型',type:'int',bind:{ key:'订单类型',data:[]},width:90,require:true,align:'left'},
                       {field:'TotalPrice',title:'总价',type:'decimal',width:70,align:'left'},
                       {field:'TotalQty',title:'总数量',type:'int',width:70,align:'left'},
                       {field:'OrderDate',title:'订单日期',type:'date',width:95,require:true,align:'left',sort:true},
                       {field:'CustomerId',title:'客户',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Customer',title:'客户',type:'string',width:80,readonly:true,align:'left'},
                       {field:'PhoneNo',title:'手机',type:'string',width:110,hidden:true,readonly:true,align:'left'},
                       {field:'OrderStatus',title:'订单状态',type:'int',bind:{ key:'订单状态',data:[]},hidden:false,width:90,require:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:120,hidden:true,align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:80,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:145,align:'left',sort:true},
                        ]);
            const detail = ref(  {
                    cnName: '订单明细',
                    table: 'Demo_OrderList',
                    columns: [{field:'OrderList_Id',title:'OrderList_Id',type:'guid',width:110,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'Order_Id',title:'Order_Id',type:'guid',width:110,hidden:true,align:'left'},
                       {field:'GoodsId',title:'商品id',type:'guid',width:110,hidden:true,align:'left'},
                       {field:'GoodsName',title:'商品名称',type:'string',width:120,edit:{type:'selectTable'},require:true,align:'left',sort:true},
                       {field:'GoodsCode',title:'商品编号',type:'string',width:120,edit:{type:'text'},require:true,align:'left'},
                       {field:'Img',title:'商品图片',type:'img',width:100,align:'left'},
                       {field:'Specs',title:'商品规格',type:'string',bind:{ key:'商品规格',data:[]},width:120,readonly:true,edit:{type:'select'},align:'left'},
                       {field:'Price',title:'单价',type:'decimal',width:110,readonly:true,edit:{type:''},require:true,align:'left'},
                       {field:'Qty',title:'数量',type:'int',width:110,edit:{type:''},require:true,align:'left'},
                       {field:'Remark',title:'备注',type:'string',width:100,edit:{type:''},align:'left'},
                       {field:'CreateID',title:'CreateID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'CreateDate',title:'创建时间',type:'datetime',width:145,align:'left',sort:true},
                       {field:'ModifyID',title:'ModifyID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'Modifier',title:'Modifier',type:'string',width:130,hidden:true,align:'left'},
                       {field:'ModifyDate',title:'ModifyDate',type:'datetime',width:110,hidden:true,align:'left',sort:true}],
                    sortName: 'CreateDate',
                    key: 'OrderList_Id',
                    buttons:[],
                    delKeys:[]
                                            });
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
