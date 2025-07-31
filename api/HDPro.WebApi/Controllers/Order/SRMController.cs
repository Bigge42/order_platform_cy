/*
 * SRM集成服务控制器
 * 提供SRM系统集成的API接口
 */
using HDPro.Core.Controllers.Basic;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IServices.SRM;
using HDPro.CY.Order.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HDPro.WebApi.Controllers.Order
{
    /// <summary>
    /// SRM集成服务控制器
    /// 提供与SRM系统集成的API接口
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SRMController : VolController
    {
        private readonly ISRMIntegrationService _srmIntegrationService;
        private readonly ILogger<SRMController> _logger;

        public SRMController(
            ISRMIntegrationService srmIntegrationService,
            ILogger<SRMController> logger)
        {
            _srmIntegrationService = srmIntegrationService;
            _logger = logger;
        }

        /// <summary>
        /// 推送催单数据到SRM系统
        /// </summary>
        /// <param name="request">催单推送请求</param>
        /// <returns>推送结果</returns>
        [HttpPost("push-order-ask")]
        public async Task<WebResponseContent> PushOrderAsk([FromBody] PushOrderAskRequest request)
        {
            _logger.LogInformation($"接收到催单推送请求，操作人: {request.Operator}，数据条数: {request.OrderAskList?.Count ?? 0}");

            if (request == null || request.OrderAskList == null || request.OrderAskList.Count == 0)
            {
                return WebResponseContent.Instance.Error("请求数据不能为空");
            }

            // 验证数据
            var validationResult = ValidateOrderAskData(request.OrderAskList);
            if (!validationResult.Status)
            {
                return validationResult;
            }

            return await _srmIntegrationService.PushOrderAskAsync(request.OrderAskList, request.Operator);
        }

        /// <summary>
        /// 推送单个催单数据到SRM系统
        /// </summary>
        /// <param name="request">单个催单推送请求</param>
        /// <returns>推送结果</returns>
        [HttpPost("push-single-order-ask")]
        public async Task<WebResponseContent> PushSingleOrderAsk([FromBody] PushSingleOrderAskRequest request)
        {
            _logger.LogInformation($"接收到单个催单推送请求，操作人: {request.Operator}，订单号: {request.OrderAskData?.OrderNo}");

            if (request?.OrderAskData == null)
            {
                return WebResponseContent.Instance.Error("催单数据不能为空");
            }

            // 验证数据
            var validationResult = ValidateSingleOrderAskData(request.OrderAskData);
            if (!validationResult.Status)
            {
                return validationResult;
            }

            return await _srmIntegrationService.PushSingleOrderAskAsync(request.OrderAskData, request.Operator);
        }

        /// <summary>
        /// 验证SRM连接状态
        /// </summary>
        /// <returns>连接状态</returns>
        [HttpGet("validate-connection")]
        public async Task<WebResponseContent> ValidateConnection()
        {
            _logger.LogInformation("接收到SRM连接验证请求");
            return await _srmIntegrationService.ValidateConnectionAsync();
        }

        /// <summary>
        /// 获取SRM配置信息
        /// </summary>
        /// <returns>配置信息</returns>
        [HttpGet("config")]
        public WebResponseContent GetConfig()
        {
            _logger.LogInformation("接收到获取SRM配置请求");
            
            try
            {
                var config = _srmIntegrationService.GetSRMConfig();
                
                // 隐藏敏感信息
                var safeConfig = new
                {
                    BaseUrl = config.BaseUrl,
                    OrderAskEndpoint = config.OrderAskEndpoint,
                    AppId = config.AppId,
                    TokenKey = MaskSensitiveData(config.TokenKey),
                    Timeout = config.Timeout
                };

                return WebResponseContent.Instance.OK("获取配置成功", safeConfig);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "获取SRM配置失败");
                return WebResponseContent.Instance.Error($"获取配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 批量推送催单数据（支持大批量数据分批处理）
        /// </summary>
        /// <param name="request">批量催单推送请求</param>
        /// <returns>推送结果</returns>
        [HttpPost("batch-push-order-ask")]
        public async Task<WebResponseContent> BatchPushOrderAsk([FromBody] BatchPushOrderAskRequest request)
        {
            _logger.LogInformation($"接收到批量催单推送请求，操作人: {request.Operator}，数据条数: {request.OrderAskList?.Count ?? 0}，批次大小: {request.BatchSize}");

            if (request == null || request.OrderAskList == null || request.OrderAskList.Count == 0)
            {
                return WebResponseContent.Instance.Error("请求数据不能为空");
            }

            // 验证数据
            var validationResult = ValidateOrderAskData(request.OrderAskList);
            if (!validationResult.Status)
            {
                return validationResult;
            }

            var batchSize = request.BatchSize > 0 ? request.BatchSize : 50; // 默认批次大小50
            var totalCount = request.OrderAskList.Count;
            var successCount = 0;
            var failureCount = 0;
            var errors = new List<string>();

            _logger.LogInformation($"开始批量推送，总数据量: {totalCount}，批次大小: {batchSize}");

            // 分批处理
            for (int i = 0; i < totalCount; i += batchSize)
            {
                var batch = request.OrderAskList.GetRange(i, System.Math.Min(batchSize, totalCount - i));
                var batchNumber = (i / batchSize) + 1;
                var totalBatches = (totalCount + batchSize - 1) / batchSize;

                _logger.LogInformation($"处理第 {batchNumber}/{totalBatches} 批，数据量: {batch.Count}");

                try
                {
                    var batchResult = await _srmIntegrationService.PushOrderAskAsync(batch, request.Operator);
                    
                    if (batchResult.Status)
                    {
                        successCount += batch.Count;
                        _logger.LogInformation($"第 {batchNumber} 批推送成功，数据量: {batch.Count}");
                    }
                    else
                    {
                        failureCount += batch.Count;
                        errors.Add($"第 {batchNumber} 批推送失败: {batchResult.Message}");
                        _logger.LogError($"第 {batchNumber} 批推送失败: {batchResult.Message}");
                    }
                }
                catch (System.Exception ex)
                {
                    failureCount += batch.Count;
                    errors.Add($"第 {batchNumber} 批推送异常: {ex.Message}");
                    _logger.LogError(ex, $"第 {batchNumber} 批推送异常");
                }

                // 批次间延迟，避免对SRM系统造成过大压力
                if (i + batchSize < totalCount)
                {
                    await Task.Delay(request.DelayBetweenBatches > 0 ? request.DelayBetweenBatches : 1000);
                }
            }

            var summaryResult = new
            {
                TotalCount = totalCount,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Errors = errors
            };

            if (failureCount == 0)
            {
                _logger.LogInformation($"批量推送完成，全部成功。总数: {totalCount}，成功: {successCount}");
                return WebResponseContent.Instance.OK("批量推送全部成功", summaryResult);
            }
            else if (successCount > 0)
            {
                _logger.LogWarning($"批量推送完成，部分成功。总数: {totalCount}，成功: {successCount}，失败: {failureCount}");
                return WebResponseContent.Instance.OK("批量推送部分成功", summaryResult);
            }
            else
            {
                _logger.LogError($"批量推送完成，全部失败。总数: {totalCount}，失败: {failureCount}");
                var errorResponse = WebResponseContent.Instance.Error("批量推送全部失败");
                errorResponse.Data = summaryResult;
                return errorResponse;
            }
        }

        /// <summary>
        /// 验证催单数据
        /// </summary>
        private WebResponseContent ValidateOrderAskData(List<OrderAskData> orderAskList)
        {
            for (int i = 0; i < orderAskList.Count; i++)
            {
                var item = orderAskList[i];
                var validationResult = ValidateSingleOrderAskData(item);
                if (!validationResult.Status)
                {
                    return WebResponseContent.Instance.Error($"第 {i + 1} 条数据验证失败: {validationResult.Message}");
                }
            }
            return WebResponseContent.Instance.OK();
        }

        /// <summary>
        /// 验证单个催单数据
        /// </summary>
        private WebResponseContent ValidateSingleOrderAskData(OrderAskData data)
        {
            if (string.IsNullOrWhiteSpace(data.AskId))
            {
                return WebResponseContent.Instance.Error("催单ID不能为空");
            }

            if (string.IsNullOrWhiteSpace(data.OrderNo))
            {
                return WebResponseContent.Instance.Error("订单号不能为空");
            }

      
            return WebResponseContent.Instance.OK();
        }

        /// <summary>
        /// 掩码敏感数据
        /// </summary>
        private string MaskSensitiveData(string data)
        {
            if (string.IsNullOrWhiteSpace(data) || data.Length <= 8)
            {
                return "****";
            }

            return data.Substring(0, 4) + "****" + data.Substring(data.Length - 4);
        }
    }

    /// <summary>
    /// 单个催单推送请求模型
    /// </summary>
    public class PushSingleOrderAskRequest
    {
        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 催单数据
        /// </summary>
        [Required]
        public OrderAskData OrderAskData { get; set; }
    }

    /// <summary>
    /// 批量催单推送请求模型
    /// </summary>
    public class BatchPushOrderAskRequest
    {
        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 催单数据列表
        /// </summary>
        [Required]
        public List<OrderAskData> OrderAskList { get; set; }

        /// <summary>
        /// 批次大小（默认50）
        /// </summary>
        public int BatchSize { get; set; } = 50;

        /// <summary>
        /// 批次间延迟时间（毫秒，默认1000）
        /// </summary>
        public int DelayBetweenBatches { get; set; } = 1000;
    }
} 