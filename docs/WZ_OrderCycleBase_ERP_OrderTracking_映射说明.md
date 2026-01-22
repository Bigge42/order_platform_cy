# WZ_OrderCycleBase 取数来源切换映射说明

## 变更原因
- 业务需要将 WZ_OrderCycleBase 的同步来源从 `OCP_OrderTracking` 切换为 `ERP_OrderTracking`，以确保取数与 ERP 直连表一致。

## 影响范围
- WZ_OrderCycleBase 同步逻辑（`SyncFromOrderTrackingAsync`）的来源表与字段映射。
- WZ_OrderCycleBase 字段赋值（`MapFields`）来源切换为 `ERP_OrderTracking`。

## 字段新旧对照表
| 旧来源字段（OCP_OrderTracking） | 新来源字段（ERP_OrderTracking） | 说明 |
| --- | --- | --- |
| SOBillNo | FBILLNO | 销售订单号 |
| SOBillID | FENTRYID | 销售订单明细 ID |
| MtoNo | FMTONO | 计划跟踪号（空白/空字符串转 NULL） |
| OrderAuditDate | FAPPROVEDATE | 订单审核时间（已是 datetime，无需转换） |
| DeliveryDate | F_ORA_DATETIME | 要求交期 |
| ReplyDeliveryDate | F_BLN_HFJHRQ | 回复交期 |
| MaterialNumber | FNUMBER | 物料编码 |
| OrderQty | FQTY | 订单数量 |

## WZ_OrderCycleBase 目标字段取值变化
| 目标字段 | 新取值来源 |
| --- | --- |
| SalesOrderNo | FBILLNO |
| PlanTrackingNo | FMTONO（空白/空字符串转 NULL） |
| OrderApprovedDate | FAPPROVEDATE |
| ReplyDeliveryDate | F_BLN_HFJHRQ |
| RequestedDeliveryDate | F_ORA_DATETIME |
| MaterialCode | FNUMBER |
| OrderQty | FQTY |
| FENTRYID | FENTRYID |

## 关键处理规则
- **日期字段已是 datetime，不做毫秒时间戳转换。**
- **FMTONO 仅空格或空字符串时规范为 NULL。**
