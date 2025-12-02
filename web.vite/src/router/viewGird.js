//多个页面指向同一个菜单时请加上属性：
// meta: {
//   dynamic: true,
// }
let viewgird = [
  {
    path: '/Sys_Log',
    name: 'sys_Log',
    component: () => import('@/views/sys/system/Sys_Log.vue')
  },
  {
    path: '/Sys_User',
    name: 'Sys_User',
    component: () => import('@/views/sys/system/Sys_User.vue')
  },
  {
    path: '/permission',
    name: 'permission',
    component: () => import('@/views/sys/Permission.vue')
  },

  {
    path: '/Sys_Dictionary',
    name: 'Sys_Dictionary',
    component: () => import('@/views/sys/system/Sys_Dictionary.vue')
  },
  {
    path: '/Sys_Role',
    name: 'Sys_Role',
    component: () => import('@/views/sys/system/Sys_Role.vue')
  },
  {
    path: '/Sys_Language',
    name: 'Sys_Language',
    component: () => import('@/views/sys/lang/Sys_Language.vue')
  },
  {
    path: '/FormDesignOptions',
    name: 'FormDesignOptions',
    component: () => import('@/views/sys/form/FormDesignOptions.vue')
  },
  {
    path: '/FormCollectionObject',
    name: 'FormCollectionObject',
    component: () => import('@/views/sys/form/FormCollectionObject.vue')
  },
  {
    path: '/Sys_WorkFlow',
    name: 'Sys_WorkFlow',
    component: () => import('@/views/sys/flow/Sys_WorkFlow.vue')
  },
  {
    path: '/Sys_WorkFlowStep',
    name: 'Sys_WorkFlowStep',
    component: () => import('@/views/sys/flow/Sys_WorkFlowStep.vue')
  },
  {
    path: '/Sys_WorkFlowTable',
    name: 'Sys_WorkFlowTable',
    component: () => import('@/views/sys/flow/Sys_WorkFlowTable.vue')
  },
  {
    path: '/Sys_WorkFlowTableStep',
    name: 'Sys_WorkFlowTableStep',
    component: () => import('@/views/sys/flow/Sys_WorkFlowTableStep.vue')
  },
  {
    path: '/flowList',
    name: 'flowList',
    component: () => import('@/views/sys/flow/FlowList.vue')
  },
  {
    path: '/Sys_QuartzOptions',
    name: 'Sys_QuartzOptions',
    component: () => import('@/views/sys/quartz/Sys_QuartzOptions.vue')
  },
  {
    path: '/Sys_QuartzLog',
    name: 'Sys_QuartzLog',
    component: () => import('@/views/sys/quartz/Sys_QuartzLog.vue')
  },
  {
    path: '/Sys_Department',
    name: 'Sys_Department',
    component: () => import('@/views/sys/system/Sys_Department.vue')
  },
  {
    path: '/Sys_DbService',
    name: 'Sys_DbService',
    component: () => import('@/views/sys/db/Sys_DbService.vue')
  },
  {
    path: '/Sys_Group',
    name: 'Sys_Group',
    component: () => import('@/views/sys/group/Sys_Group.vue')
  },
  {
    path: '/Sys_Region',
    name: 'Sys_Region',
    component: () => import('@/views/sys/system/Sys_Region.vue')
  },
  {
    path: '/TestService',
    name: 'TestService',
    component: () => import('@/views/dbtest/test/TestService.vue')
  },
  {
    path: '/TestDb',
    name: 'TestDb',
    component: () => import('@/views/dbtest/test/TestDb.vue')
  },
  {
    path: '/Demo_Order',
    name: 'Demo_Order',
    component: () => import('@/views/dbtest/order/Demo_Order.vue')
  },
  {
    path: '/Demo_Order/edit',
    name: 'Demo_Order_edit',
    component: () => import('@/views/dbtest/order/Demo_OrderWindow/Edit.vue'),
    meta: {
      name: '订单管理窗口模式',
      edit: true,
      keepAlive: false
    }
  },
  {
    path: '/Demo_OrderTables',
    name: 'Demo_OrderTables',
    component: () => import('@/views/dbtest/order/Demo_OrderTabs.vue')
  },
  {
    path: '/Demo_OrderStats',
    name: 'Demo_OrderStats',
    component: () => import('@/views/dbtest/order/Demo_OrderStats.vue')
  },
  {
    path: '/tabs',
    name: 'tabs',
    component: () => import('@/views/example/tabs.vue')
  },
  {
    path: '/list',
    name: 'list',
    component: () => import('@/views/example/list.vue')
  },
  {
    path: '/charts1',
    name: 'charts1',
    component: () => import('@/views/example/charts1.vue')
  },
  {
    path: '/Demo_Customer',
    name: 'Demo_Customer',
    component: () => import('@/views/dbtest/customer/Demo_Customer.vue')
  },
  {
    path: '/Demo_CustomerMap',
    name: 'Demo_CustomerMap',
    component: () => import('@/views/dbtest/customer/Demo_CustomerMap.vue')
  },
  {
    path: '/Demo_Goods',
    name: 'Demo_Goods',
    component: () => import('@/views/dbtest/goods/Demo_Goods.vue')
  },
  {
    path: '/Demo_GoodsTree',
    name: 'Demo_GoodsTree',
    component: () => import('@/views/dbtest/goods/Demo_GoodsTree.vue')
  },
  {
    path: '/Demo_GoodsMerge',
    name: 'Demo_GoodsMerge',
    component: () => import('@/views/dbtest/goods/Demo_GoodsMerge.vue'),
    meta: {
      keepAlive: false
    }
  },
  {
    path: '/Demo_Product',
    name: 'Demo_Product',
    component: () => import('@/views/dbtest/product/Demo_Product.vue')
  },
  {
    path: '/Demo_Product2',
    name: 'Demo_Product2',
    component: () => import('@/views/dbtest/product/Demo_Product2.vue')
  },
  {
    path: '/Demo_ProductSize',
    name: 'Demo_ProductSize',
    component: () => import('@/views/dbtest/product/Demo_ProductSize.vue')
  },
  {
    path: '/Demo_ProductColor',
    name: 'Demo_ProductColor',
    component: () => import('@/views/dbtest/product/Demo_ProductColor.vue')
  },
  {
    path: '/Demo_Goods/edit',
    name: 'Demo_Goods_edit',
    component: () => import('@/views/dbtest/goods/Demo_Goods/Edit.vue'),
    meta: {
      name: '商品信息',
      edit: true,
      keepAlive: false
    }
  },
  {
    path: '/Sys_PrintOptions',
    name: 'Sys_PrintOptions',
    component: () => import('@/views/sys/system/Sys_PrintOptions.vue')
  },
  {
    path: '/Sys_ReportOptions',
    name: 'Sys_ReportOptions',
    component: () => import('@/views/sys/system/Sys_ReportOptions.vue')
  },
  {
    path: '/Sys_Dashboard',
    name: 'Sys_Dashboard',
    component: () => import('@/views/sys/dashboard/Sys_Dashboard.vue')
  },
  {
    path: '/DashboardEdit', //工作台设计
    name: 'DashboardEdit',
    component: () => import('@/views/sys/dashboard/DashboardEdit.vue'),
    meta: {
      name: '工作台',
      keepAlive: false
    }
  },
  {
    path: '/DashboardPreview', //工作台预览
    name: 'DashboardPreview',
    component: () => import('@/views/sys/dashboard/DashboardPreview.vue'),
    meta: {
      dynamic: true
      // keepAlive:false
    }
  },
  {
    path: '/Sys_CodeRule',
    name: 'Sys_CodeRule',
    component: () => import('@/views/sys/rule/Sys_CodeRule.vue')
  },
  {
    path: '/Sys_Post',
    name: 'Sys_Post',
    component: () => import('@/views/sys/system/Sys_Post.vue')
  },
  {
    path: '/Demo_Product/edit',
    name: 'Demo_Product_edit',
    component: () => import('@/views/dbtest/product/Demo_Product/Edit.vue'),
    meta: {
      name: '产品管理',
      edit: true,
      keepAlive: false
    }
  },
  {
    path: '/Demo_Catalog',
    name: 'Demo_Catalog',
    component: () => import('@/views/dbtest/catalog/Demo_Catalog.vue')
  },
  {
    path: '/MES_WarehouseManagement',
    name: 'MES_WarehouseManagement',
    component: () => import('@/views/mes/mes/MES_WarehouseManagement.vue')
  },
  {
    path: '/MES_ProductOutbound',
    name: 'MES_ProductOutbound',
    component: () => import('@/views/mes/mes/MES_ProductOutbound.vue')
  },
  {
    path: '/MES_LocationManagement',
    name: 'MES_LocationManagement',
    component: () => import('@/views/mes/mes/MES_LocationManagement.vue')
  },
  {
    path: '/MES_InventoryManagement',
    name: 'MES_InventoryManagement',
    component: () => import('@/views/mes/mes/MES_InventoryManagement.vue')
  },
  {
    path: '/MES_ProductInbound',
    name: 'MES_ProductInbound',
    component: () => import('@/views/mes/mes/MES_ProductInbound.vue')
  },
  {
    path: '/MES_Customer',
    name: 'MES_Customer',
    component: () => import('@/views/mes/mes/MES_Customer.vue')
  },
  {
    path: '/MES_Supplier',
    name: 'MES_Supplier',
    component: () => import('@/views/mes/mes/MES_Supplier.vue')
  },
  {
    path: '/MES_ProductionLine',
    name: 'MES_ProductionLine',
    component: () => import('@/views/mes/mes/MES_ProductionLine.vue')
  },
  {
    path: '/MES_Material',
    name: 'MES_Material',
    component: () => import('@/views/mes/mes/MES_Material.vue')
  },
  {
    path: '/MES_ProductionLineDevice',
    name: 'MES_ProductionLineDevice',
    component: () => import('@/views/mes/mes/MES_ProductionLineDevice.vue')
  },
  {
    path: '/MES_EquipmentManagement',
    name: 'MES_EquipmentManagement',
    component: () => import('@/views/mes/mes/MES_EquipmentManagement.vue')
  },
  {
    path: '/MES_EquipmentRepair',
    name: 'MES_EquipmentRepair',
    component: () => import('@/views/mes/mes/MES_EquipmentRepair.vue')
  },
  {
    path: '/MES_EquipmentFaultRecord',
    name: 'MES_EquipmentFaultRecord',
    component: () => import('@/views/mes/mes/MES_EquipmentFaultRecord.vue')
  },
  {
    path: '/MES_EquipmentMaintenance',
    name: 'MES_EquipmentMaintenance',
    component: () => import('@/views/mes/mes/MES_EquipmentMaintenance.vue')
  },
  {
    path: '/MES_Process',
    name: 'MES_Process',
    component: () => import('@/views/mes/mes/MES_Process.vue')
  },
  {
    path: '/MES_ProcessReport',
    name: 'MES_ProcessReport',
    component: () => import('@/views/mes/mes/MES_ProcessReport.vue')
  },
  {
    path: '/MES_ProcessRoute',
    name: 'MES_ProcessRoute',
    component: () => import('@/views/mes/mes/MES_ProcessRoute.vue')
  },
  {
    path: '/MES_MaterialCatalog',
    name: 'MES_MaterialCatalog',
    component: () => import('@/views/mes/mes/MES_MaterialCatalog.vue')
  },
  {
    path: '/MES_ProductionOrder',
    name: 'MES_ProductionOrder',
    component: () => import('@/views/mes/mes/MES_ProductionOrder.vue')
  },
  {
    path: '/MES_ProductionPlanDetail',
    name: 'MES_ProductionPlanDetail',
    component: () => import('@/views/mes/mes/MES_ProductionPlanDetail.vue')
  },
  {
    path: '/MES_ProductionPlanChangeRecord',
    name: 'MES_ProductionPlanChangeRecord',
    component: () => import('@/views/mes/mes/MES_ProductionPlanChangeRecord.vue')
  },
  {
    path: '/MES_ProductionReporting',
    name: 'MES_ProductionReporting',
    component: () => import('@/views/mes/mes/MES_ProductionReporting.vue')
  },
  {
    path: '/MES_DefectiveProductRecord',
    name: 'MES_DefectiveProductRecord',
    component: () => import('@/views/mes/mes/MES_DefectiveProductRecord.vue')
  },
  {
    path: '/MES_Bom_Main',
    name: 'MES_Bom_Main',
    component: () => import('@/views/mes/mes/MES_Bom_Main.vue')
  },
  {
    path: '/MES_QualityInspectionPlan',
    name: 'MES_QualityInspectionPlan',
    component: () => import('@/views/mes/mes/MES_QualityInspectionPlan.vue')
  },
  {
    path: '/MES_QualityInspectionRecord',
    name: 'MES_QualityInspectionRecord',
    component: () => import('@/views/mes/mes/MES_QualityInspectionRecord.vue')
  },
  {
    path: '/MES_SchedulingPlan',
    name: 'MES_SchedulingPlan',
    component: () => import('@/views/mes/mes/MES_SchedulingPlan.vue')
  },
  {
    path: '/MES_Calendar',
    name: 'MES_Calendar',
    component: () => import('@/views/mes/mes/MES_Calendar.vue')
  },
  {
    path: '/MES_Gantt',
    name: 'MES_Gantt',
    component: () => import('@/views/mes/mes/MES_Gantt.vue')
  },
  {
    path: '/MES_Bom_Detail',
    name: 'MES_Bom_Detail',
    component: () => import('@/views/mes/mes/MES_Bom_Detail.vue')
  },
  {
    path: '/Sys_TableFieldDefinition',
    name: 'Sys_TableFieldDefinition',
    component: () => import('@/views/sys/system/Sys_TableFieldDefinition.vue')
  },
  {
    path: '/OCP_BOMProgress',
    name: 'OCP_BOMProgress',
    component: () => import('@/views/order/order/OCP_BOMProgress.vue')
  },
  {
    path: '/OCP_MasterTask',
    name: 'OCP_MasterTask',
    component: () => import('@/views/order/order/OCP_MasterTask.vue')
  },
  {
    path: '/OCP_OrderPlan',
    name: 'OCP_OrderPlan',
    component: () => import('@/views/order/order/OCP_OrderPlan.vue')
  },
  {
    path: '/OCP_UrgeReply',
    name: 'OCP_UrgeReply',
    component: () => import('@/views/order/order/OCP_UrgeReply.vue')
  },
  {
    path: '/OCP_ProductionDetail',
    name: 'OCP_ProductionDetail',
    component: () => import('@/views/order/order/OCP_ProductionDetail.vue')
  },
  {
    path: '/OCP_PurchaseDetail',
    name: 'OCP_PurchaseDetail',
    component: () => import('@/views/order/order/OCP_PurchaseDetail.vue')
  },
  {
    path: '/OCP_SalesOrder',
    name: 'OCP_SalesOrder',
    component: () => import('@/views/order/order/OCP_SalesOrder.vue')
  },
  {
    path: '/OCP_SalesOrderDetail',
    name: 'OCP_SalesOrderDetail',
    component: () => import('@/views/order/order/OCP_SalesOrderDetail.vue')
  },
  {
    path: '/Sys_NotificationLog',
    name: 'Sys_NotificationLog',
    component: () => import('@/views/sys/notification/Sys_NotificationLog.vue')
  },
  {
    path: '/Sys_NotificationTemplate',
    name: 'Sys_NotificationTemplate',
    component: () => import('@/views/sys/notification/Sys_NotificationTemplate.vue')
  },
  {
    path: '/Sys_Notification',
    name: 'Sys_Notification',
    component: () => import('@/views/sys/notification/Sys_Notification.vue')
  },
  {
    path: '/message-center/negotiate',
    name: 'message-center-negotiate',
    component: () => import('@/views/order/message-center/negotiate/index.vue')
  },
  {
    path: '/message-center/reminder',
    name: 'message-center-reminder',
    component: () => import('@/views/order/message-center/reminder/index.vue')
  },
  {
    path: '/machine-assembly',
    name: 'machine-assembly',
    component: () => import('@/views/order/machine-assembly/index.vue')
  },
  {
    path: '/mtrl-shortage-dashboard',
    name: 'mtrl-shortage-dashboard',
    component: () => import('@/views/order/mtrl-shortage-dashboard/index.vue')
  }
    ,{
        path: '/OCP_SOProgress',
        name: 'OCP_SOProgress',
        component: () => import('@/views/order/ordercollaboration/OCP_SOProgress.vue')
    }    ,{
        path: '/OCP_SOProgressDetail',
        name: 'OCP_SOProgressDetail',
        component: () => import('@/views/order/ordercollaboration/OCP_SOProgressDetail.vue')
    }    ,{
        path: '/OCP_CurrentProcessBatchInfo',
        name: 'OCP_CurrentProcessBatchInfo',
        component: () => import('@/views/order/ordercollaboration/OCP_CurrentProcessBatchInfo.vue')
    }    ,{
        path: '/OCP_OrderTracking',
        name: 'OCP_OrderTracking',
        component: () => import('@/views/order/ordercollaboration/OCP_OrderTracking.vue')
    }    ,{
        path: '/OCP_LackMtrlPlan',
        name: 'OCP_LackMtrlPlan',
        component: () => import('@/views/order/ordercollaboration/OCP_LackMtrlPlan.vue')
    }    ,{
        path: '/OCP_LackMtrlPool',
        name: 'OCP_LackMtrlPool',
        component: () => import('@/views/order/ordercollaboration/OCP_LackMtrlPool.vue')
    }    ,{
        path: '/OCP_LackMtrlResult',
        name: 'OCP_LackMtrlResult',
        component: () => import('@/views/order/ordercollaboration/OCP_LackMtrlResult.vue')
    }    ,{
        path: '/View_OrderTracking',
        name: 'View_OrderTracking',
        component: () => import('@/views/order/ordercollaboration/View_OrderTracking.vue')
    }    ,{
        path: '/Sys_FilterPlan',
        name: 'Sys_FilterPlan',
        component: () => import('@/views/sys/system/Sys_FilterPlan.vue')
    }    ,{
        path: '/OCP_SubOrder',
        name: 'OCP_SubOrder',
        component: () => import('@/views/order/ordercollaboration/OCP_SubOrder.vue')
    }    ,{
        path: '/OCP_SubOrderDetail',
        name: 'OCP_SubOrderDetail',
        component: () => import('@/views/order/ordercollaboration/OCP_SubOrderDetail.vue')
    }    ,{
        path: '/OCP_SubOrderUnFinishTrack',
        name: 'OCP_SubOrderUnFinishTrack',
        component: () => import('@/views/order/ordercollaboration/OCP_SubOrderUnFinishTrack.vue')
    }    ,{
        path: '/OCP_PurchaseOrder',
        name: 'OCP_PurchaseOrder',
        component: () => import('@/views/order/ordercollaboration/OCP_PurchaseOrder.vue')
    }    ,{
        path: '/OCP_PurchaseOrderDetail',
        name: 'OCP_PurchaseOrderDetail',
        component: () => import('@/views/order/ordercollaboration/OCP_PurchaseOrderDetail.vue')
    }    ,{
        path: '/OCP_POUnFinishTrack',
        name: 'OCP_POUnFinishTrack',
        component: () => import('@/views/order/ordercollaboration/OCP_POUnFinishTrack.vue')
    }    ,{
        path: '/OCP_JGPrdMO',
        name: 'OCP_JGPrdMO',
        component: () => import('@/views/order/ordercollaboration/OCP_JGPrdMO.vue')
    }    ,{
        path: '/OCP_JGPrdMODetail',
        name: 'OCP_JGPrdMODetail',
        component: () => import('@/views/order/ordercollaboration/OCP_JGPrdMODetail.vue')
    }    ,{
        path: '/OCP_JGUnFinishTrack',
        name: 'OCP_JGUnFinishTrack',
        component: () => import('@/views/order/ordercollaboration/OCP_JGUnFinishTrack.vue')
    }    ,{
        path: '/OCP_PartPrdMO',
        name: 'OCP_PartPrdMO',
        component: () => import('@/views/order/ordercollaboration/OCP_PartPrdMO.vue')
    }    ,{
        path: '/OCP_PartPrdMODetail',
        name: 'OCP_PartPrdMODetail',
        component: () => import('@/views/order/ordercollaboration/OCP_PartPrdMODetail.vue')
    }    ,{
        path: '/OCP_PartUnFinishTracking',
        name: 'OCP_PartUnFinishTracking',
        component: () => import('@/views/order/ordercollaboration/OCP_PartUnFinishTracking.vue')
    }    ,{
        path: '/OCP_PrdMO',
        name: 'OCP_PrdMO',
        component: () => import('@/views/order/ordercollaboration/OCP_PrdMO.vue')
    }    ,{
        path: '/OCP_PrdMODetail',
        name: 'OCP_PrdMODetail',
        component: () => import('@/views/order/ordercollaboration/OCP_PrdMODetail.vue')
    }    ,{
        path: '/OCP_PrdMOTracking',
        name: 'OCP_PrdMOTracking',
        component: () => import('@/views/order/ordercollaboration/OCP_PrdMOTracking.vue')
    }    ,{
        path: '/OCP_TechManagement',
        name: 'OCP_TechManagement',
        component: () => import('@/views/order/ordercollaboration/OCP_TechManagement.vue')
    }    ,{
        path: '/OCP_Customer',
        name: 'OCP_Customer',
        component: () => import('@/views/order/ordercollaboration/OCP_Customer.vue')
    }    ,{
        path: '/OCP_Material',
        name: 'OCP_Material',
        component: () => import('@/views/order/ordercollaboration/OCP_Material.vue')
    }    ,{
        path: '/OCP_Supplier',
        name: 'OCP_Supplier',
        component: () => import('@/views/order/ordercollaboration/OCP_Supplier.vue')
    }, {
      path: '/OCP_LackMtrlResult_PO',
      name: 'OCP_LackMtrlResult_PO',
      component: () => import('@/views/order/ordercollaboration/OCP_LackMtrlResult.vue')
    }, {
      path: '/OCP_LackMtrlResult_WO',
      name: 'OCP_LackMtrlResult_WO',
      component: () => import('@/views/order/ordercollaboration/OCP_LackMtrlResult.vue')
    }, {
      path: '/OCP_LackMtrlResult_MO_JG',
      name: 'OCP_LackMtrlResult_MO_JG',
      component: () => import('@/views/order/ordercollaboration/OCP_LackMtrlResult.vue')
    }, {
      path: '/OCP_LackMtrlResult_MO_BJ',
      name: 'OCP_LackMtrlResult_MO_BJ',
      component: () => import('@/views/order/ordercollaboration/OCP_LackMtrlResult.vue')
    }, {
      path: '/OCP_LackMtrlResult_MO_ZJ',
      name: 'OCP_LackMtrlResult_MO_ZJ',
      component: () => import('@/views/order/ordercollaboration/OCP_LackMtrlResult.vue')
    }    ,{
        path: '/OCP_UrgentOrder',
        name: 'OCP_UrgentOrder',
        component: () => import('@/views/order/ordercollaboration/OCP_UrgentOrder.vue')
    }    ,{
        path: '/OCP_UrgentOrderReply',
        name: 'OCP_UrgentOrderReply',
        component: () => import('@/views/order/ordercollaboration/OCP_UrgentOrderReply.vue')
    }    ,{
        path: '/OCP_Negotiation',
        name: 'OCP_Negotiation',
        component: () => import('@/views/order/ordercollaboration/OCP_Negotiation.vue')
    }    ,{
        path: '/OCP_NegotiationReply',
        name: 'OCP_NegotiationReply',
        component: () => import('@/views/order/ordercollaboration/OCP_NegotiationReply.vue')
    }    ,{
        path: '/Sys_ThirdPartyApp',
        name: 'Sys_ThirdPartyApp',
        component: () => import('@/views/sys/system/Sys_ThirdPartyApp.vue')
    }    ,{
        path: '/OCP_BusinessTypeResponsible',
        name: 'OCP_BusinessTypeResponsible',
        component: () => import('@/views/order/ordercollaboration/OCP_BusinessTypeResponsible.vue')
    }    ,{
        path: '/OCP_PurchaseSupplierMapping',
        name: 'OCP_PurchaseSupplierMapping',
        component: () => import('@/views/order/ordercollaboration/OCP_PurchaseSupplierMapping.vue')
    }    ,{
        path: '/OCP_SOProgress/edit',
        name: 'OCP_SOProgress_edit',
        component: () => import('@/views/order/ordercollaboration/OCP_SOProgress/Edit.vue'),
        meta:{
              name:"销售订单进度查询",
              edit:true,
              keepAlive:false
            }
    }    ,{
        path: '/OCP_DeletedData',
        name: 'OCP_DeletedData',
        component: () => import('@/views/order/ordercollaboration/OCP_DeletedData.vue')
    }    ,{
        path: '/OCP_AlertRules',
        name: 'OCP_AlertRules',
        component: () => import('@/views/order/ordercollaboration/OCP_AlertRules.vue')
    }    ,{
        path: '/MES_SpecialPrintRequest',
        name: 'MES_SpecialPrintRequest',
        component: () => import('@/views/mes/mes/MES_SpecialPrintRequest.vue')
    }    ,{
        path: '/XhckkbRecord',
        name: 'XhckkbRecord',
        component: () => import('@/views/order/xhckkbrecord/XhckkbRecord.vue')
    }    ,{
        path: '/WZ_ProductionOutput',
        name: 'WZ_ProductionOutput',
        component: () => import('@/views/order/wz_productionoutput/WZ_ProductionOutput.vue')
    }    ,{
        path: '/MaterialCallBoard',
        name: 'MaterialCallBoard',
        component: () => import('@/views/order/materialcallboard/MaterialCallBoard.vue')
    }    ,{
        path: '/vw_OCP_Tech_BOM_Status_Monthly',
        name: 'vw_OCP_Tech_BOM_Status_Monthly',
        component: () => import('@/views/order/ocp_bom_completion/vw_OCP_Tech_BOM_Status_Monthly.vue')
    }    ,{
        path: '/ORDER_NOTE_FLAT',
        name: 'ORDER_NOTE_FLAT',
        component: () => import('@/views/order/order_note_flat/ORDER_NOTE_FLAT.vue')
    }    ,{
        path: '/WZ_OrderCycleBase',
        name: 'WZ_OrderCycleBase',
        component: () => import('@/views/order//wz_ordercyclebase/WZ_OrderCycleBase.vue')
    }    ,{
        path: '/V_XhckkbRecord_Material',
        name: 'V_XhckkbRecord_Material',
        component: () => import('@/views/order/v_xhckkbrecord_material/V_XhckkbRecord_Material.vue')
    }]

//上面的demo、MES开头的都是示例菜单，可以任意删除
export default viewgird
