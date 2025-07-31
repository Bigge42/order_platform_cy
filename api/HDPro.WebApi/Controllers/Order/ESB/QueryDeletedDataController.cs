using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Core.Controllers.Basic;
using HDPro.Core.Utilities;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.IServices.OrderCollaboration.ESB;
using HDPro.Entity.DomainModels.ESB;
using System.Collections.Generic;

namespace HDPro.WebApi.Controllers.Order.ESB
{
    /// <summary>
    /// ESB删除数据查询控制器
    /// 提供订单协同平台删除数据查询的Web API接口
    /// </summary>
    [Route("api/ESB/QueryDeletedData")]
    [ApiController]
    [PermissionTable(Name = "ESBQueryDeletedData")]
    public class QueryDeletedDataController : VolController
    {
        private readonly IQueryDeletedDataService _queryDeletedDataService;
        private readonly ILogger<QueryDeletedDataController> _logger;

        public QueryDeletedDataController(
            IQueryDeletedDataService queryDeletedDataService,
            ILogger<QueryDeletedDataController> logger)
        {
            _queryDeletedDataService = queryDeletedDataService;
            _logger = logger;
        }

        /// <summary>
        /// 查询单个业务类型的删除数据
        /// </summary>
        /// <param name="businessType">业务类型（DDGZ、BOMDJJD、DDJDCX、CGGZ、WWGZ、ZJGZ、BJGZ、JGGZ）</param>
        /// <returns>删除数据查询结果</returns>
        [HttpGet("QueryByBusinessType")]
        public async Task<IActionResult> QueryByBusinessType([FromQuery] string businessType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(businessType))
                {
                    return Json(new WebResponseContent().Error("业务类型参数不能为空"));
                }

                _logger.LogInformation($"开始查询业务类型 {businessType} 的删除数据");

                var result = await _queryDeletedDataService.QueryDeletedDataAsync(businessType);

                _logger.LogInformation($"查询业务类型 {businessType} 的删除数据完成，状态：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询业务类型 {businessType} 的删除数据发生异常");
                return Json(new WebResponseContent().Error($"查询异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 批量查询所有业务类型的删除数据
        /// </summary>
        /// <returns>所有业务类型的删除数据查询结果</returns>
        [HttpGet("QueryAll")]
        public async Task<IActionResult> QueryAll()
        {
            try
            {
                _logger.LogInformation("开始批量查询所有业务类型的删除数据");

                var result = await _queryDeletedDataService.QueryAllDeletedDataAsync();

                _logger.LogInformation($"批量查询所有业务类型的删除数据完成，状态：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量查询所有业务类型的删除数据发生异常");
                return Json(new WebResponseContent().Error($"批量查询异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取业务类型统计信息
        /// </summary>
        /// <returns>业务类型统计信息</returns>
        [HttpGet("GetBusinessTypeStatistics")]
        public IActionResult GetBusinessTypeStatistics()
        {
            try
            {
                _logger.LogInformation("开始获取业务类型统计信息");

                var result = _queryDeletedDataService.GetBusinessTypeStatistics();

                _logger.LogInformation($"获取业务类型统计信息完成，状态：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取业务类型统计信息发生异常");
                return Json(new WebResponseContent().Error($"获取统计信息异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 验证业务类型参数
        /// </summary>
        /// <param name="businessType">业务类型</param>
        /// <returns>验证结果</returns>
        [HttpGet("ValidateBusinessType")]
        public IActionResult ValidateBusinessType([FromQuery] string businessType)
        {
            try
            {
                _logger.LogInformation($"开始验证业务类型参数：{businessType}");

                var result = _queryDeletedDataService.ValidateBusinessType(businessType);

                _logger.LogInformation($"验证业务类型参数完成，状态：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"验证业务类型参数 {businessType} 发生异常");
                return Json(new WebResponseContent().Error($"验证异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 测试ESB删除数据查询接口连接
        /// </summary>
        /// <param name="businessType">业务类型（可选，默认使用DDGZ）</param>
        /// <returns>测试结果</returns>
        [HttpGet("TestConnection")]
        public async Task<IActionResult> TestConnection([FromQuery] string businessType = "DDGZ")
        {
            try
            {
                _logger.LogInformation($"开始测试ESB删除数据查询接口连接，业务类型：{businessType}");

                // 先验证业务类型
                var validationResult = _queryDeletedDataService.ValidateBusinessType(businessType);
                if (!validationResult.Status)
                {
                    return Json(validationResult);
                }

                // 执行查询测试
                var queryResult = await _queryDeletedDataService.QueryDeletedDataAsync(businessType);

                var testResult = new WebResponseContent().OK("ESB删除数据查询接口连接测试完成", new
                {
                    TestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    BusinessType = businessType,
                    ConnectionStatus = queryResult.Status ? "连接成功" : "连接失败",
                    QueryResult = queryResult.Status,
                    Message = queryResult.Message,
                    DeletedDataCount = queryResult.Status && queryResult.Data != null ? (( List<ESBDeletedDataResponse>)queryResult.Data)?.Count : 0,
                    ValidationResult = validationResult.Data
                });

                _logger.LogInformation($"ESB删除数据查询接口连接测试完成，连接状态：{(queryResult.Status ? "成功" : "失败")}");

                return Json(testResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"测试ESB删除数据查询接口连接发生异常，业务类型：{businessType}");
                return Json(new WebResponseContent().Error($"连接测试异常：{ex.Message}"));
            }
        }
    }
}