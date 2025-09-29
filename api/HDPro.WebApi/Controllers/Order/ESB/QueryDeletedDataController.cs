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
using System.Linq;
using HDPro.Core.Filters;

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
        /// 处理单个业务类型的删除数据（查询、存储、删除完整流程）
        /// </summary>
        /// <param name="businessType">业务类型（DDGZ、BOMDJJD、DDJDCX、CGGZ、WWGZ、ZJGZ、BJGZ、JGGZ）</param>
        /// <returns>完整流程处理结果</returns>
        [HttpPost("ProcessByBusinessType")]
        public async Task<IActionResult> ProcessByBusinessType([FromQuery] string businessType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(businessType))
                {
                    return Json(new WebResponseContent().Error("业务类型参数不能为空"));
                }

                _logger.LogInformation($"开始处理业务类型 {businessType} 的删除数据");

                var result = await _queryDeletedDataService.ProcessDeletedDataAsync(businessType);

                _logger.LogInformation($"处理业务类型 {businessType} 的删除数据完成，状态：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理业务类型 {businessType} 的删除数据发生异常");
                return Json(new WebResponseContent().Error($"处理异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 处理所有业务类型的删除数据（查询、存储、删除完整流程）
        /// </summary>
        /// <returns>完整流程处理结果</returns>
        [HttpPost("ProcessAll"), ApiTask]
        public async Task<IActionResult> ProcessAll()
        {
            try
            {
                _logger.LogInformation("开始批量处理所有业务类型的删除数据");

                var result = await _queryDeletedDataService.ProcessAllDeletedDataAsync();

                _logger.LogInformation($"批量处理所有业务类型的删除数据完成，状态：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量处理所有业务类型的删除数据发生异常");
                return Json(new WebResponseContent().Error($"批量处理异常：{ex.Message}"));
            }
        }


    }


}