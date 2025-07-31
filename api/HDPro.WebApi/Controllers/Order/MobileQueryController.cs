using Microsoft.AspNetCore.Mvc;
using HDPro.Entity.AttributeManager;
using HDPro.Entity.SystemModels;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.OrderTracking;
using HDPro.Core.Filters;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using HDPro.Core.Controllers.Basic;

namespace HDPro.CY.Order.Controllers
{
    /// <summary>
    /// 移动端查询控制器
    /// 提供移动端专用的数据查询接口
    /// </summary>
    [Route("api/MobileQuery")]
    [ApiController]
    [PermissionTable(Name = "MobileQuery")]
    public class MobileQueryController : VolController
    {
        private readonly OrderProgressQueryService _orderProgressQueryService;
        private readonly ILogger<MobileQueryController> _logger;

        public MobileQueryController(
            OrderProgressQueryService orderProgressQueryService,
            ILogger<MobileQueryController> logger)
        {
            _orderProgressQueryService = orderProgressQueryService;
            _logger = logger;
        }

        #region 移动端订单进度查询接口

        /// <summary>
        /// 查询移动端订单进度信息（含业务逻辑处理）
        /// </summary>
        /// <param name="request">查询请求参数</param>
        /// <returns>移动端订单进度信息</returns>
        [HttpPost("MobileOrderProgress/Query")]
        public async Task<IActionResult> QueryMobileOrderProgress([FromBody] OrderProgressQueryRequest request)
        {
            try
            {
                // 参数验证
                if (request == null)
                {
                    return JsonNormal(new WebResponseContent().Error("请求参数不能为空"));
                }

                if (string.IsNullOrWhiteSpace(request.FBILLNO))
                {
                    return JsonNormal(new WebResponseContent().Error("销售订单号不能为空"));
                }

                _logger.LogInformation($"移动端查询订单进度信息，销售订单号：{request.FBILLNO}");

                var result = await _orderProgressQueryService.GetMobileOrderProgressAsync(request.FBILLNO);

                if (result == null)
                {
                    return JsonNormal(new WebResponseContent().OK(data: new MobileOrderProgressResponse { FBILLNO = request.FBILLNO }, message: "未找到相关订单进度信息"));
                }

                _logger.LogInformation($"移动端订单进度查询完成，销售订单号：{request.FBILLNO}");

                return JsonNormal(new WebResponseContent().OK(data: result, message: "查询成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"移动端查询订单进度信息异常，销售订单号：{request?.FBILLNO}，错误：{ex.Message}");
                return JsonNormal(new WebResponseContent().Error($"查询异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 通过销售订单号查询移动端订单进度信息（GET请求）
        /// </summary>
        /// <param name="billNo">销售订单号</param>
        /// <returns>移动端订单进度信息</returns>
        [HttpGet("MobileOrderProgress/QueryByBillNo")]
        public async Task<IActionResult> QueryMobileOrderProgressByBillNo([Required] string billNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(billNo))
                {
                    return JsonNormal(new WebResponseContent().Error("销售订单号不能为空"));
                }

                _logger.LogInformation($"移动端GET查询订单进度信息，销售订单号：{billNo}");

                var result = await _orderProgressQueryService.GetMobileOrderProgressAsync(billNo);

                if (result == null)
                {
                    return JsonNormal(new WebResponseContent().OK(data: new MobileOrderProgressResponse { FBILLNO = billNo }, message: "未找到相关订单进度信息"));
                }

                _logger.LogInformation($"移动端订单进度查询完成，销售订单号：{billNo}");

                return JsonNormal(new WebResponseContent().OK(data: result, message: "查询成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"移动端GET查询订单进度信息异常，销售订单号：{billNo}，错误：{ex.Message}");
                return JsonNormal(new WebResponseContent().Error($"查询异常：{ex.Message}"));
            }
        }

        #endregion

        #region 原始订单进度监造查询接口

        /// <summary>
        /// 查询订单进度监造信息（原始ESB数据）
        /// </summary>
        /// <param name="request">查询请求参数</param>
        /// <returns>订单进度信息</returns>
        [HttpPost("OrderProgress/Query")]
        public async Task<IActionResult> QueryOrderProgress([FromBody] OrderProgressQueryRequest request)
        {
            try
            {
                // 参数验证
                if (request == null)
                {
                    return JsonNormal(new WebResponseContent().Error("请求参数不能为空"));
                }

                if (string.IsNullOrWhiteSpace(request.FBILLNO))
                {
                    return JsonNormal(new WebResponseContent().Error("销售订单号不能为空"));
                }

                _logger.LogInformation($"移动端查询订单进度监造信息，销售订单号：{request.FBILLNO}");

                var result = await _orderProgressQueryService.QueryOrderProgressAsync(request);

                if (result == null || result.Count == 0)
                {
                    return JsonNormal(new WebResponseContent().OK(data: new object[] { }, message: "未找到相关订单进度信息"));
                }

                _logger.LogInformation($"订单进度查询完成，销售订单号：{request.FBILLNO}，返回记录数：{result.Count}");

                return JsonNormal(new WebResponseContent().OK(data: result, message: "查询成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询订单进度监造信息异常，销售订单号：{request?.FBILLNO}，错误：{ex.Message}");
                return Json(new WebResponseContent().Error($"查询异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 通过销售订单号查询订单进度监造信息（GET请求）
        /// </summary>
        /// <param name="billNo">销售订单号</param>
        /// <returns>订单进度信息</returns>
        [HttpGet("OrderProgress/QueryByBillNo")]
        public async Task<IActionResult> QueryOrderProgressByBillNo([Required] string billNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(billNo))
                {
                    return JsonNormal(new WebResponseContent().Error("销售订单号不能为空"));
                }

                _logger.LogInformation($"移动端GET查询订单进度监造信息，销售订单号：{billNo}");

                var result = await _orderProgressQueryService.QueryOrderProgressAsync(billNo);

                if (result == null || result.Count == 0)
                {
                    return JsonNormal(new WebResponseContent().OK(data: new object[] { }, message: "未找到相关订单进度信息"));
                }

                _logger.LogInformation($"订单进度查询完成，销售订单号：{billNo}，返回记录数：{result.Count}");

                return JsonNormal(new WebResponseContent().OK(data: result, message: "查询成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GET查询订单进度监造信息异常，销售订单号：{billNo}，错误：{ex.Message}");
                return JsonNormal(new WebResponseContent().Error($"查询异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取订单进度统计信息
        /// </summary>
        /// <param name="billNo">销售订单号</param>
        /// <returns>进度统计信息</returns>
        [HttpGet("OrderProgress/GetSummary")]
        public async Task<IActionResult> GetOrderProgressSummary([Required] string billNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(billNo))
                {
                    return JsonNormal(new WebResponseContent().Error("销售订单号不能为空"));
                }

                _logger.LogInformation($"获取订单进度统计信息，销售订单号：{billNo}");

                var summary = await _orderProgressQueryService.GetOrderProgressSummaryAsync(billNo);

                if (summary == null)
                {
                    return JsonNormal(new WebResponseContent().OK(data: new OrderProgressSummary(), message: "未找到相关订单进度统计信息"));
                }

                _logger.LogInformation($"订单进度统计查询完成，销售订单号：{billNo}，完成率：{summary.CompletionRate}%");

                return JsonNormal(new WebResponseContent().OK(data: summary, message: "查询成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取订单进度统计信息异常，销售订单号：{billNo}，错误：{ex.Message}");
                return JsonNormal(new WebResponseContent().Error($"查询异常：{ex.Message}"));
            }
        }

        #endregion

        #region 移动端通用查询接口

        /// <summary>
        /// 移动端健康检查
        /// </summary>
        /// <returns>健康状态</returns>
        [HttpGet("HealthCheck")]
        public IActionResult HealthCheck()
        {
            return Json(new WebResponseContent().OK(data: new
            {
                status = "healthy",
                timestamp = DateTime.Now,
                version = "1.0.0",
                serviceName = "移动端查询服务"
            }, message: "服务正常"));
        }

        /// <summary>
        /// 获取移动端支持的查询接口列表
        /// </summary>
        /// <returns>接口列表</returns>
        [HttpGet("GetSupportedApis")]
        public IActionResult GetSupportedApis()
        {
            var apis = new object[]
            {
                new
                {
                    name = "移动端订单进度查询",
                    path = "/api/MobileQuery/MobileOrderProgress/Query",
                    method = "POST",
                    description = "通过销售订单号查询移动端订单进度信息（含业务逻辑处理和进度百分比）",
                    parameters = new { FBILLNO = "销售订单号（必填）" }
                },
                new
                {
                    name = "移动端订单进度查询（GET）",
                    path = "/api/MobileQuery/MobileOrderProgress/QueryByBillNo",
                    method = "GET",
                    description = "通过销售订单号查询移动端订单进度信息（含业务逻辑处理和进度百分比）",
                    parameters = new { billNo = "销售订单号（必填）" }
                },
                new
                {
                    name = "订单进度监造查询（原始数据）",
                    path = "/api/MobileQuery/OrderProgress/Query",
                    method = "POST",
                    description = "通过销售订单号查询订单进度监造信息（原始ESB数据）",
                    parameters = new { FBILLNO = "销售订单号（必填）" }
                },
                new
                {
                    name = "订单进度监造查询（GET）",
                    path = "/api/MobileQuery/OrderProgress/QueryByBillNo",
                    method = "GET",
                    description = "通过销售订单号查询订单进度监造信息（原始ESB数据）",
                    parameters = new { billNo = "销售订单号（必填）" }
                },
                new
                {
                    name = "订单进度统计",
                    path = "/api/MobileQuery/OrderProgress/GetSummary",
                    method = "GET",
                    description = "获取订单进度统计信息",
                    parameters = new { billNo = "销售订单号（必填）" }
                },
                new
                {
                    name = "健康检查",
                    path = "/api/MobileQuery/HealthCheck",
                    method = "GET",
                    description = "检查服务健康状态",
                    parameters = new { }
                }
            };

            return Json(new WebResponseContent().OK(data: apis, message: "获取支持的API列表成功"));
        }

        #endregion
    }
} 