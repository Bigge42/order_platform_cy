/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_TechManagement",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;
using HDPro.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_TechManagementController
    {
        private readonly IOCP_TechManagementService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OCP_TechManagementController> _logger;

        [ActivatorUtilitiesConstructor]
        public OCP_TechManagementController(
            IOCP_TechManagementService service,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OCP_TechManagementController> logger
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        
        /// <summary>
        /// 根据物料编码获取技术管理负责人
        /// 调用TC系统WebService接口获取物料编码对应的负责人信息
        /// </summary>
        /// <param name="request">获取负责人请求参数</param>
        /// <returns>负责人信息</returns>
        [HttpPost("GetTechManagerByMaterialCode")]
        public async Task<IActionResult> GetTechManagerByMaterialCode([FromBody] GetTechManagerRequest request)
        {
            try
            {
                _logger.LogInformation($"接收到获取技术管理负责人请求，物料编码: {request?.MaterialCode}");

                if (request == null || string.IsNullOrWhiteSpace(request.MaterialCode))
                {
                    return BadRequest(WebResponseContent.Instance.Error("物料编码不能为空"));
                }

                var result = await _service.GetTechManagerByMaterialCodeAsync(request.MaterialCode);

                if (result.Status)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取技术管理负责人异常，物料编码: {request?.MaterialCode}");
                return StatusCode(500, WebResponseContent.Instance.Error($"获取技术管理负责人异常: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取近14天的BOM未搭建数量统计
        /// </summary>
        /// <returns>近14天每日BOM未搭建数量统计数据</returns>
        [HttpPost("GetLast14DaysBomUnbuiltTrend")]
        public async Task<IActionResult> GetLast14DaysBomUnbuiltTrend()
        {
            try
            {
                _logger.LogInformation("接收到获取近14天BOM未搭建数量统计请求");

                var result = await _service.GetLast14DaysBomUnbuiltTrend();
                
                // 直接返回数组数据
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取近14天BOM未搭建数量统计异常");
                return StatusCode(500, new { error = $"获取BOM未搭建统计失败：{ex.Message}" });
            }
        }
    }

    /// <summary>
    /// 获取技术管理负责人请求参数
    /// </summary>
    public class GetTechManagerRequest
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
    }
}
