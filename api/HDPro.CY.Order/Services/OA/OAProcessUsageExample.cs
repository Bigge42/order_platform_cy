/*
 * OA流程发起使用示例
 * 展示如何使用OA集成服务发起流程
 */
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IServices.OA;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.OA
{
    /// <summary>
    /// OA流程发起使用示例
    /// 展示如何在业务场景中发起OA流程
    /// </summary>
    public class OAProcessUsageExample : IDependency
    {
        private readonly IOAIntegrationService _oaIntegrationService;
        private readonly ILogger<OAProcessUsageExample> _logger;

        public OAProcessUsageExample(
            IOAIntegrationService oaIntegrationService,
            ILogger<OAProcessUsageExample> logger)
        {
            _oaIntegrationService = oaIntegrationService;
            _logger = logger;
        }

        /// <summary>
        /// 示例：发起订单跟踪报告流程
        /// </summary>
        /// <param name="loginName">发起人登录名</param>
        /// <param name="orderData">订单数据</param>
        /// <returns></returns>
        public async Task<WebResponseContent> StartOrderTrackingProcessAsync(
            string loginName,
            OrderTrackingData orderData)
        {
            try
            {
                // 构建表单数据
                var formData = new OAFormData
                {
                    DataSource = orderData.DataSource ?? "协同平台", // 数据来源：SRM/协同平台
                    SupplierCode = orderData.SupplierCode,
                    SupplierName = orderData.SupplierName,
                    OrderNo = orderData.OrderNo,
                    SourceOrderNo = orderData.SourceOrderNo,
                    DeliveryStatus = orderData.DeliveryStatus,
                    OrderDate = orderData.OrderDate?.ToString("yyyy-MM-dd"),
                    OrderRemark = orderData.OrderRemark,
                    MaterialName = orderData.MaterialName,
                    MaterialCode = orderData.MaterialCode,
                    Specification = orderData.Specification,
                    DrawingNo = orderData.DrawingNo,
                    Material = orderData.Material,
                    Unit = orderData.Unit,
                    PurchaseQuantity = orderData.PurchaseQuantity?.ToString(),
                    Shortage = orderData.Shortage?.ToString(),
                    PlanTrackingNo = orderData.PlanTrackingNo,
                    FirstRequiredDeliveryDate = orderData.DeliveryDate?.ToString("yyyy-MM-dd"), // 第一次要求交期（变更前交期）
                    ExecutiveOrganization = orderData.ExecutiveOrganization ?? "1", // 执行机构
                    ChangedDeliveryDate = orderData.ChangedDeliveryDate?.ToString("yyyy-MM-dd"), // 变更的交期
                    SupplierExceptionReply = orderData.SupplierExceptionReply
                };

                // 发起流程
                var result = await _oaIntegrationService.GetTokenAndStartProcessAsync(loginName, formData);

                if (result.Status)
                {
                    _logger.LogInformation($"订单跟踪流程发起成功，订单号: {orderData.OrderNo}");
                    
                    // 可以从result.Data中获取流程信息
                    if (result.Data is OAProcessData processData)
                    {
                        _logger.LogInformation($"流程ID: {processData.ProcessId}, 主题: {processData.Subject}");
                    }
                }
                else
                {
                    _logger.LogError($"订单跟踪流程发起失败，订单号: {orderData.OrderNo}, 错误: {result.Message}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发起订单跟踪流程时发生异常，订单号: {orderData.OrderNo}");
                return new WebResponseContent().Error($"发起流程异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 示例：直接使用Token发起流程
        /// </summary>
        /// <param name="token">已获取的Token</param>
        /// <param name="orderData">订单数据</param>
        /// <returns></returns>
        public async Task<WebResponseContent> StartProcessWithTokenAsync(
            string token,
            OrderTrackingData orderData)
        {
            try
            {
                // 构建表单数据
                var formData = new OAFormData
                {
                    DataSource = orderData.DataSource ?? "协同平台", // 数据来源：SRM/协同平台
                    SupplierCode = orderData.SupplierCode,
                    SupplierName = orderData.SupplierName,
                    OrderNo = orderData.OrderNo,
                    SourceOrderNo = orderData.SourceOrderNo,
                    DeliveryStatus = orderData.DeliveryStatus,
                    OrderDate = orderData.OrderDate?.ToString("yyyy-MM-dd"),
                    OrderRemark = orderData.OrderRemark,
                    MaterialName = orderData.MaterialName,
                    MaterialCode = orderData.MaterialCode,
                    Specification = orderData.Specification,
                    DrawingNo = orderData.DrawingNo,
                    Material = orderData.Material,
                    Unit = orderData.Unit,
                    PurchaseQuantity = orderData.PurchaseQuantity?.ToString(),
                    Shortage = orderData.Shortage?.ToString(),
                    PlanTrackingNo = orderData.PlanTrackingNo,
                    FirstRequiredDeliveryDate = orderData.DeliveryDate?.ToString("yyyy-MM-dd"), // 第一次要求交期（变更前交期）
                    ExecutiveOrganization = orderData.ExecutiveOrganization ?? "1", // 执行机构
                    ChangedDeliveryDate = orderData.ChangedDeliveryDate?.ToString("yyyy-MM-dd"), // 变更的交期
                    SupplierExceptionReply = orderData.SupplierExceptionReply
                };

                // 直接使用Token发起流程
                var result = await _oaIntegrationService.StartProcessAsync(token, formData);

                if (result.Status)
                {
                    _logger.LogInformation($"使用Token发起流程成功，订单号: {orderData.OrderNo}");
                }
                else
                {
                    _logger.LogError($"使用Token发起流程失败，订单号: {orderData.OrderNo}, 错误: {result.Message}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"使用Token发起流程时发生异常，订单号: {orderData.OrderNo}");
                return new WebResponseContent().Error($"发起流程异常: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 订单跟踪数据模型
    /// </summary>
    public class OrderTrackingData
    {
        /// <summary>
        /// 数据来源：SRM/协同平台
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// 供方编码
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 供方名称
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 源单号
        /// </summary>
        public string SourceOrderNo { get; set; }

        /// <summary>
        /// 送货状态
        /// </summary>
        public string DeliveryStatus { get; set; }

        /// <summary>
        /// 订单日期
        /// </summary>
        public DateTime? OrderDate { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        public string OrderRemark { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specification { get; set; }

        /// <summary>
        /// 图号
        /// </summary>
        public string DrawingNo { get; set; }

        /// <summary>
        /// 材质
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 采购量
        /// </summary>
        public decimal? PurchaseQuantity { get; set; }

        /// <summary>
        /// 差缺
        /// </summary>
        public decimal? Shortage { get; set; }

        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string PlanTrackingNo { get; set; }

        /// <summary>
        /// 第一次要求交期（变更前交期）
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// 执行机构
        /// </summary>
        public string ExecutiveOrganization { get; set; }

        /// <summary>
        /// 变更的交期
        /// </summary>
        public DateTime? ChangedDeliveryDate { get; set; }

        /// <summary>
        /// 协商日期（保留兼容性）
        /// </summary>
        public DateTime? NegotiationDate { get; set; }

        /// <summary>
        /// 供方异常回复
        /// </summary>
        public string SupplierExceptionReply { get; set; }
    }
} 