/*
 *所有关于OCP_Negotiation类的OA相关业务代码应在此处编写
 *包括OA流程发起、OA消息发送等功能
 */
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices.OA;
using System.Threading.Tasks;
using System.Collections.Generic;
using HDPro.Utilities;
using HDPro.CY.Order.Models;
using HDPro.CY.Order.Services.OA;
using HDPro.CY.Order.Services.OrderCollaboration.Common;
using HDPro.Utilities.Extensions;
using HDPro.CY.Order.IServices;

namespace HDPro.CY.Order.Services
{
    /// <summary>
    /// 协商服务 - OA相关功能分部类
    /// 包含OA流程发起、OA消息发送等功能
    /// </summary>
    public partial class OCP_NegotiationService
    {
        /// <summary>
        /// 发起协商OA流程（用于委外和采购业务）
        /// </summary>
        /// <param name="negotiation">协商实体</param>
        /// <param name="subOrderTrackingData">预加载的委外跟踪数据</param>
        /// <param name="purchaseTrackingData">预加载的采购跟踪数据</param>
        /// <param name="materialData">预加载的物料数据</param>
        /// <param name="senderLoginName">发送者登录名（可选，用于异步调用时传入）</param>
        /// <param name="senderName">发送者显示名（可选，用于异步调用时传入）</param>
        /// <returns></returns>
        private async Task StartOAProcessAsync(OCP_Negotiation negotiation,
            OCP_SubOrderUnFinishTrack subOrderTrackingData = null,
            OCP_POUnFinishTrack purchaseTrackingData = null,
            OCP_Material materialData = null,
            string senderLoginName = null,
            string senderName = null)
        {
            try
            {
                // 如果没有传入用户信息，尝试从当前上下文获取（同步调用时）
                if (string.IsNullOrEmpty(senderLoginName) || string.IsNullOrEmpty(senderName))
                {
                    var (currentLoginName, currentDisplayName) = MessageHelper.GetCurrentUserInfo();
                    senderLoginName = currentLoginName;
                    senderName = currentDisplayName;
                }
                
                var loginName = senderLoginName;
                var operatorName = senderName;

                MessageHelper.LogIntegrationStart("OA流程", "Negotiation", negotiation.NegotiationID, negotiation.BusinessType, operatorName);

                // 使用预加载的数据构建OA流程数据
                SmartOAProcessData oaProcessData = null;

                if (negotiation.BusinessType == BusinessConstants.BusinessType.OutSourcing && subOrderTrackingData != null)
                {
                    oaProcessData = BuildSubOrderProcessData(subOrderTrackingData, materialData, negotiation);
                }
                else if (negotiation.BusinessType == BusinessConstants.BusinessType.Purchase && purchaseTrackingData != null)
                {
                    oaProcessData = BuildPurchaseProcessData(purchaseTrackingData, materialData, negotiation);
                }

                // 如果无法从跟踪表获取信息，使用协商表的基本信息
                if (oaProcessData == null)
                {
                    oaProcessData = new SmartOAProcessData
                    {
                        DataSource = "协同平台",
                        SupplierCode = "", // OCP_Negotiation表中没有供应商编码字段
                        SupplierName = "", // OCP_Negotiation表中没有供应商名称字段
                        OrderNo = negotiation.BillNo ?? "",
                        SourceOrderNo = "", // OCP_Negotiation表中没有源订单号字段
                        DeliveryStatus = GetDeliveryStatusFromNegotiation(negotiation),
                        OrderDate = negotiation.CreateDate?.ToString("yyyy-MM-dd") ?? System.DateTime.Now.ToString("yyyy-MM-dd"),
                        OrderRemark = negotiation.NegotiationReason ?? "",
                        MaterialName = "", // OCP_Negotiation表中没有物料名称字段
                        MaterialCode = "", // OCP_Negotiation表中没有物料编码字段
                        Specification = "", // OCP_Negotiation表中没有规格字段
                        DrawingNo = "", // 从物料表获取
                        Material = "", // 从物料表获取
                        Unit = "", // 从物料表获取
                        PurchaseQuantity = "", // OCP_Negotiation表中没有数量字段
                        ProductionCycle = "", // 生产周期
                        Shortage = "", // 协商一般不涉及缺料
                        PlanTrackingNo = negotiation.PlanTraceNo ?? "",
                        FirstRequiredDeliveryDate = negotiation.DeliveryDate?.ToString("yyyy-MM-dd"),
                        ExecutiveOrganization = BusinessConstants.DefaultConfig.DefaultExecutiveOrganization,
                        ChangedDeliveryDate = negotiation.NegotiationDate?.ToString("yyyy-MM-dd"),
                        OrderEntryLineNo = null,
                        SupplierExceptionReply = BuildSupplierExceptionReply(negotiation),
                        AssignedResponsiblePerson = negotiation.AssignedResPerson ?? "" // 指定负责人工号
                    };
                }

                // 设置协商编号
                oaProcessData.NegotiationNo = negotiation.NegotiationID.ToString();

                var result = await _oaIntegrationService.SmartStartProcessAsync(loginName, oaProcessData);

                if (result.Status)
                {
                    MessageHelper.LogIntegrationSuccess("OA流程", "Negotiation", negotiation.NegotiationID, negotiation.BillNo, negotiation.BusinessType);
                    
                    // 记录流程信息
                    if (result.Data is OAProcessData processData)
                    {
                        HDLogHelper.Log("Negotiation_OA_Process_Info", $"协商OA流程详情 - 流程ID: {processData.ProcessId}, 主题: {processData.Subject}, 协商ID: {negotiation.NegotiationID}", BusinessConstants.LogCategory.OrderCollaboration);
                    }
                }
                else
                {
                    MessageHelper.LogIntegrationError("OA流程", "Negotiation", negotiation.NegotiationID, negotiation.BillNo, negotiation.BusinessType, result.Message);
                }
            }
            catch (System.Exception ex)
            {
                MessageHelper.LogIntegrationException("OA流程", "Negotiation", negotiation.NegotiationID, negotiation.BusinessType, ex);
            }
        }

        /// <summary>
        /// 根据协商ID发起OA流程
        /// 从委外跟踪表或采购跟踪表读取相关信息并发起流程
        /// </summary>
        /// <param name="negotiationId">协商ID</param>
        /// <param name="userId">发起用户ID</param>
        /// <returns>OA流程发起结果</returns>
        public async Task<WebResponseContent> StartOAProcessByNegotiationIdAsync(long negotiationId, string userId)
        {
            try
            {
                // 1. 获取协商信息
                var negotiation = await _repository.FindAsyncFirst(x => x.NegotiationID == negotiationId);
                if (negotiation == null)
                {
                    return WebResponseContent.Instance.Error($"未找到协商ID为{negotiationId}的协商记录");
                }

                if (string.IsNullOrWhiteSpace(negotiation.BusinessKey))
                {
                    return WebResponseContent.Instance.Error("协商记录的业务主键为空，无法获取相关业务信息");
                }

                // 2. 提前读取所有需要的数据（在Task.Run外面）
                SmartOAProcessData processData = null;

                if (negotiation.BusinessType == BusinessConstants.BusinessType.OutSourcing)
                {
                    // 提前读取委外跟踪数据
                    if (long.TryParse(negotiation.BusinessKey, out long entryId))
                    {
                        var trackingRecord = await _subOrderRepository.FindAsyncFirst(x => x.FENTRYID == entryId);
                        if (trackingRecord != null)
                        {
                            var materialInfo = await _materialRepository.FindAsyncFirst(x => x.MaterialCode == trackingRecord.MaterialNumber);
                            processData = BuildSubOrderProcessData(trackingRecord, materialInfo, negotiation);
                        }
                    }
                }
                else if (negotiation.BusinessType == BusinessConstants.BusinessType.Purchase)
                {
                    // 提前读取采购跟踪数据
                    if (long.TryParse(negotiation.BusinessKey, out long entryId))
                    {
                        var trackingRecord = await _purchaseRepository.FindAsyncFirst(x => x.FENTRYID == entryId);
                        if (trackingRecord != null)
                        {
                            var materialInfo = await _materialRepository.FindAsyncFirst(x => x.MaterialCode == trackingRecord.MaterialCode);
                            processData = BuildPurchaseProcessData(trackingRecord, materialInfo, negotiation);
                        }
                    }
                }
                else
                {
                    return WebResponseContent.Instance.Error($"不支持的业务类型：{MessageHelper.GetBusinessTypeText(negotiation.BusinessType)}({negotiation.BusinessType})");
                }

                if (processData == null)
                {
                    return WebResponseContent.Instance.Error($"未找到业务主键为{negotiation.BusinessKey}的{MessageHelper.GetBusinessTypeText(negotiation.BusinessType)}跟踪记录");
                }

                // 3. 设置协商编号
                processData.NegotiationNo = negotiationId.ToString();

                // 4. 发起OA流程
                var result = await _oaIntegrationService.SmartStartProcessAsync(userId, processData);

                return result;
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error($"发起OA流程失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 发送协商OA通知消息
        /// </summary>
        /// <param name="negotiation">协商实体</param>
        /// <param name="senderLoginName">发送者登录名（可选，用于异步调用时传入）</param>
        /// <param name="senderName">发送者显示名（可选，用于异步调用时传入）</param>
        /// <returns></returns>
        private async Task SendOANegotiationNotificationAsync(OCP_Negotiation negotiation, string senderLoginName = null, string senderName = null)
        {
            try
            {
                // 如果没有传入用户信息，尝试从当前上下文获取（同步调用时）
                if (string.IsNullOrEmpty(senderLoginName) || string.IsNullOrEmpty(senderName))
                {
                    var (currentLoginName, currentDisplayName) = MessageHelper.GetCurrentUserInfo();
                    senderLoginName = currentLoginName;
                    senderName = currentDisplayName;
                }
                
                // 获取接收者列表
                var receiverLoginNames = MessageHelper.DetermineReceivers(
                    negotiation.AssignedResPerson,
                    negotiation.DefaultResPerson,
                    negotiation.NegotiationID,
                    "Negotiation");

                // 构建消息内容
                var messageContent = MessageHelper.BuildNegotiationMessage(negotiation, senderName);

                // 同时发送原OA和股份OA消息
                MessageHelper.LogIntegrationStart("双OA系统通知", "Negotiation", negotiation.NegotiationID, negotiation.BusinessType, senderName, receiverLoginNames);

                var result = await _oaIntegrationService.SendToBothOASystemsAsync(
                    senderLoginName,
                    receiverLoginNames,
                    messageContent);

                if (result.Status)
                {
                    MessageHelper.LogIntegrationSuccess("双OA系统通知", "Negotiation", negotiation.NegotiationID, negotiation.BillNo, negotiation.BusinessType, receiverLoginNames);
                    
                    // 记录详细发送结果
                    if (result.Data is BothOASystemsResult bothResult)
                    {
                        HDLogHelper.Log("Negotiation_BothOA_Detail", $"协商双OA发送详情 - 协商ID: {negotiation.NegotiationID}, " +
                            $"原OA: {(bothResult.OriginalOA?.Success == true ? "成功" : "失败")} - {bothResult.OriginalOA?.Message}, " +
                            $"股份OA: {(bothResult.ShareholderOA?.Success == true ? "成功" : "失败")} - {bothResult.ShareholderOA?.Message}", 
                            BusinessConstants.LogCategory.OrderCollaboration);
                    }
                }
                else
                {
                    MessageHelper.LogIntegrationError("双OA系统通知", "Negotiation", negotiation.NegotiationID, negotiation.BillNo, negotiation.BusinessType, result.Message);
                }
            }
            catch (System.Exception ex)
            {
                MessageHelper.LogIntegrationException("OA通知", "Negotiation", negotiation.NegotiationID, negotiation.BusinessType, ex);
            }
        }

        /// <summary>
        /// 发送协商相关的OA消息（通用方法）
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="negotiationType">协商类型</param>
        /// <param name="negotiationContent">协商内容</param>
        /// <param name="customContent">自定义内容</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SendNegotiationOAMessageAsync(
            string senderLoginName,
            List<string> receiverLoginNames,
            string orderNo,
            string negotiationType,
            string negotiationContent,
            string customContent = null)
        {
            try
            {
                HDLogHelper.Log("Negotiation_OA_General_Start", $"开始发送协商双OA消息 - 发送者: {senderLoginName}, 订单号: {orderNo}, 协商类型: {negotiationType}", "OrderCollaboration");
                
                // 构建消息内容
                var messageContent = customContent ?? $"【协商通知】协同平台通知：订单 {orderNo} 有新的{negotiationType}协商，协商内容：{negotiationContent}，请及时查看处理。";
                
                var result = await _oaIntegrationService.SendToBothOASystemsAsync(
                    senderLoginName,
                    receiverLoginNames,
                    messageContent);

                if (result.Status)
                {
                    HDLogHelper.Log("Negotiation_BothOA_General_Success", $"协商双OA消息发送成功 - 订单号: {orderNo}, 协商类型: {negotiationType}, 接收者: {string.Join(",", receiverLoginNames)}", "OrderCollaboration");
                    
                    // 记录详细发送结果
                    if (result.Data is BothOASystemsResult bothResult)
                    {
                        HDLogHelper.Log("Negotiation_BothOA_General_Detail", $"协商双OA发送详情 - 订单号: {orderNo}, " +
                            $"原OA: {(bothResult.OriginalOA?.Success == true ? "成功" : "失败")} - {bothResult.OriginalOA?.Message}, " +
                            $"股份OA: {(bothResult.ShareholderOA?.Success == true ? "成功" : "失败")} - {bothResult.ShareholderOA?.Message}", 
                            "OrderCollaboration");
                    }
                }
                else
                {
                    HDLogHelper.Log("Negotiation_BothOA_General_Error", $"协商双OA消息发送失败 - 订单号: {orderNo}, 协商类型: {negotiationType}, 错误信息: {result.Message}", "OrderCollaboration");
                }

                return result;
            }
            catch (System.Exception ex)
            {
                HDLogHelper.Log("Negotiation_OA_General_Exception", $"发送协商OA消息异常 - 订单号: {orderNo}, 协商类型: {negotiationType}, 错误详情: {ex.Message}", "OrderCollaboration");
                return new WebResponseContent().Error($"发送协商OA消息异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送交期协商OA通知（便捷方法）
        /// </summary>
        /// <param name="senderLoginName">发送者登录名</param>
        /// <param name="receiverLoginNames">接收者登录名列表</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="originalDate">原交期</param>
        /// <param name="proposedDate">建议交期</param>
        /// <param name="negotiationReason">协商原因</param>
        /// <returns></returns>
        public async Task<WebResponseContent> SendDeliveryNegotiationOAMessageAsync(
            string senderLoginName,
            List<string> receiverLoginNames,
            string orderNo,
            System.DateTime originalDate,
            System.DateTime proposedDate,
            string negotiationReason)
        {
            try
            {
                var content = $"【交期协商通知】订单协同平台通知：\n\n" +
                             $"订单号：{orderNo}\n" +
                             $"原交期：{originalDate:yyyy-MM-dd}\n" +
                             $"建议交期：{proposedDate:yyyy-MM-dd}\n" +
                             $"协商原因：{negotiationReason}\n" +
                             $"请及时回复协商结果。\n\n" +
                             $"发起时间：{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                HDLogHelper.Log("Negotiation_Delivery_Start", $"发送交期协商双OA通知 - 订单号: {orderNo}, 原交期: {originalDate:yyyy-MM-dd}, 建议交期: {proposedDate:yyyy-MM-dd}", "OrderCollaboration");

                var result = await _oaIntegrationService.SendToBothOASystemsAsync(
                    senderLoginName,
                    receiverLoginNames,
                    content);

                if (result.Status)
                {
                    HDLogHelper.Log("Negotiation_Delivery_Success", $"交期协商双OA通知发送成功 - 订单号: {orderNo}, 接收者: {string.Join(",", receiverLoginNames)}", "OrderCollaboration");
                    
                    // 记录详细发送结果
                    if (result.Data is BothOASystemsResult bothResult)
                    {
                        HDLogHelper.Log("Negotiation_Delivery_Detail", $"交期协商双OA发送详情 - 订单号: {orderNo}, " +
                            $"原OA: {(bothResult.OriginalOA?.Success == true ? "成功" : "失败")} - {bothResult.OriginalOA?.Message}, " +
                            $"股份OA: {(bothResult.ShareholderOA?.Success == true ? "成功" : "失败")} - {bothResult.ShareholderOA?.Message}", 
                            "OrderCollaboration");
                    }
                }
                else
                {
                    HDLogHelper.Log("Negotiation_Delivery_Error", $"交期协商双OA通知发送失败 - 订单号: {orderNo}, 错误信息: {result.Message}", "OrderCollaboration");
                }

                return result;
            }
            catch (System.Exception ex)
            {
                HDLogHelper.Log("Negotiation_Delivery_Exception", $"发送交期协商OA通知异常 - 订单号: {orderNo}, 错误详情: {ex.Message}", "OrderCollaboration");
                return new WebResponseContent().Error($"发送交期协商OA通知异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据协商信息构建供方异常回复内容
        /// </summary>
        /// <param name="negotiation">协商实体</param>
        /// <returns>供方异常回复内容</returns>
        private string BuildSupplierExceptionReply(OCP_Negotiation negotiation)
        {
            // 取值协商内容NegotiationContent
            return negotiation.NegotiationContent ?? "";
        }

        /// <summary>
        /// 从协商信息中获取交货状态
        /// </summary>
        /// <param name="negotiation">协商实体</param>
        /// <returns>交货状态</returns>
        private string GetDeliveryStatusFromNegotiation(OCP_Negotiation negotiation)
        {
            // 根据协商类型映射交货状态
            switch (negotiation.NegotiationType)
            {
                case "交期协商":
                    return "交期变更";
                case "质量协商":
                    return "质量问题";
                case "技术协商":
                    return "技术变更";
                default:
                    return "协商中";
            }
        }

        /// <summary>
        /// 构建委外跟踪OA流程数据
        /// </summary>
        /// <param name="trackingRecord">委外跟踪记录</param>
        /// <param name="materialInfo">物料信息</param>
        /// <param name="negotiation">协商记录</param>
        /// <returns>OA流程数据</returns>
        private SmartOAProcessData BuildSubOrderProcessData(OCP_SubOrderUnFinishTrack trackingRecord, OCP_Material materialInfo, OCP_Negotiation negotiation)
        {
            return new SmartOAProcessData
            {
                DataSource = "协同平台",
                SupplierCode = trackingRecord.SupplierCode ?? "",
                SupplierName = trackingRecord.SupplierName ?? "",
                MaterialCode = trackingRecord.MaterialNumber ?? "",
                MaterialName = trackingRecord.MaterialName ?? "",
                Specification = trackingRecord.Specification ?? "",
                OrderNo = trackingRecord.POBillNo ?? "",
                SourceOrderNo = trackingRecord.SubOrderNo ?? "",
                PlanTrackingNo = trackingRecord.MtoNo ?? "",
                PurchaseQuantity = trackingRecord.POQty?.ToString() ?? "",
                DeliveryStatus = trackingRecord.DeliveryStatus ?? "",
                OrderDate = trackingRecord.POCreateDate?.ToString("yyyy-MM-dd") ?? "",
                FirstRequiredDeliveryDate = trackingRecord.DeliveryDate?.ToString("yyyy-MM-dd") ??
                                           trackingRecord.LastReplyDeliveryDate?.ToString("yyyy-MM-dd") ??
                                           trackingRecord.POCreateDate?.ToString("yyyy-MM-dd") ??
                                           DateTime.Now.ToString("yyyy-MM-dd"),
                ExecutiveOrganization = "委外车间",
                OrderRemark = trackingRecord.OrderRemark ?? "",
                Shortage = "", // 委外跟踪表中没有差缺字段
                ProductionCycle = "", // 委外跟踪表中没有生产周期字段
                DrawingNo = materialInfo?.DrawingNo ?? "", // 从物料表获取图号
                Material = materialInfo?.Material ?? "", // 从物料表获取材质
                Unit = materialInfo?.BasicUnit ?? "", // 从物料表获取单位
                ChangedDeliveryDate = negotiation.NegotiationDate?.ToString("yyyy-MM-dd") ?? "", // 取协商的日期
                OrderEntryLineNo = (int?)trackingRecord.Seq,
                SupplierExceptionReply = negotiation.NegotiationContent ?? "",
                AssignedResponsiblePerson = negotiation.AssignedResPerson ?? "" // 指定负责人工号
            };
        }

        /// <summary>
        /// 从委外跟踪表获取OA流程所需信息（保留原方法以兼容其他调用）
        /// </summary>
        /// <param name="negotiation">协商记录</param>
        /// <returns>OA流程数据</returns>
        private async Task<SmartOAProcessData> GetSubOrderTrackingInfoAsync(OCP_Negotiation negotiation)
        {
            try
            {
                // 根据业务主键（采购订单明细ID）查询委外跟踪记录
                if (long.TryParse(negotiation.BusinessKey, out long entryId))
                {
                    var trackingRecord = await _subOrderRepository.FindAsyncFirst(x => x.FENTRYID == entryId);
                    if (trackingRecord == null)
                    {
                        return null;
                    }

                    // 从物料表获取材质、图号、单位信息
                    var materialInfo = await _materialRepository.FindAsyncFirst(x => x.MaterialCode == trackingRecord.MaterialNumber);

                    return BuildSubOrderProcessData(trackingRecord, materialInfo, negotiation);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                HDLogHelper.Log("GetSubOrderTrackingInfo_Error",
                    $"获取委外跟踪信息失败: BusinessKey={negotiation.BusinessKey}, Error={ex.Message}",
                    BusinessConstants.LogCategory.OrderCollaboration);
                return null;
            }
        }

        /// <summary>
        /// 构建采购跟踪OA流程数据
        /// </summary>
        /// <param name="trackingRecord">采购跟踪记录</param>
        /// <param name="materialInfo">物料信息</param>
        /// <param name="negotiation">协商记录</param>
        /// <returns>OA流程数据</returns>
        private SmartOAProcessData BuildPurchaseProcessData(OCP_POUnFinishTrack trackingRecord, OCP_Material materialInfo, OCP_Negotiation negotiation)
        {
            return new SmartOAProcessData
            {
                DataSource = "协同平台",
                SupplierCode = trackingRecord.SupplierCode ?? "",
                SupplierName = trackingRecord.SupplierName ?? "",
                MaterialCode = trackingRecord.MaterialCode ?? "",
                MaterialName = trackingRecord.MaterialName ?? "",
                Specification = trackingRecord.Specification ?? "",
                OrderNo = trackingRecord.POBillNo ?? "",
                SourceOrderNo = trackingRecord.POBillNo ?? "", // 采购订单号作为源单号
                PlanTrackingNo = trackingRecord.PlanTraceNo ?? "",
                PurchaseQuantity = trackingRecord.POQty?.ToString() ?? "",
                DeliveryStatus = trackingRecord.DeliveryStatus ?? "",
                OrderDate = trackingRecord.POCreateDate?.ToString("yyyy-MM-dd") ?? "",
                FirstRequiredDeliveryDate = trackingRecord.RequiredDeliveryDate?.ToString("yyyy-MM-dd") ??
                                           trackingRecord.LatestReplyDeliveryDate?.ToString("yyyy-MM-dd") ??
                                           trackingRecord.POCreateDate?.ToString("yyyy-MM-dd") ??
                                           DateTime.Now.ToString("yyyy-MM-dd"),
                ExecutiveOrganization = "采购部",
                OrderRemark = trackingRecord.OrderRemark ?? "",
                Shortage = "", // 采购跟踪表中没有差缺字段
                ProductionCycle = "", // 采购跟踪表中没有生产周期字段
                DrawingNo = materialInfo?.DrawingNo ?? "", // 从物料表获取图号
                Material = materialInfo?.Material ?? "", // 从物料表获取材质
                Unit = materialInfo?.BasicUnit ?? "", // 从物料表获取单位
                ChangedDeliveryDate = negotiation.NegotiationDate?.ToString("yyyy-MM-dd") ?? "", // 取协商的日期
                OrderEntryLineNo = (int?)trackingRecord.Seq,
                SupplierExceptionReply = negotiation.NegotiationContent ?? "",
                AssignedResponsiblePerson = negotiation.AssignedResPerson ?? "" // 指定负责人工号
            };
        }

        /// <summary>
        /// 从采购跟踪表获取OA流程所需信息（保留原方法以兼容其他调用）
        /// </summary>
        /// <param name="negotiation">协商记录</param>
        /// <returns>OA流程数据</returns>
        private async Task<SmartOAProcessData> GetPurchaseTrackingInfoAsync(OCP_Negotiation negotiation)
        {
            try
            {
                // 根据业务主键（采购订单明细ID）查询采购跟踪记录
                if (long.TryParse(negotiation.BusinessKey, out long entryId))
                {
                    var trackingRecord = await _purchaseRepository.FindAsyncFirst(x => x.FENTRYID == entryId);
                    if (trackingRecord == null)
                    {
                        return null;
                    }

                    // 从物料表获取材质、图号、单位信息
                    var materialInfo = await _materialRepository.FindAsyncFirst(x => x.MaterialCode == trackingRecord.MaterialCode);

                    return BuildPurchaseProcessData(trackingRecord, materialInfo, negotiation);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                HDLogHelper.Log("GetPurchaseTrackingInfo_Error",
                    $"获取采购跟踪信息失败: BusinessKey={negotiation.BusinessKey}, Error={ex.Message}",
                    BusinessConstants.LogCategory.OrderCollaboration);
                return null;
            }
        }


    }
}
