/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_Customer",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;
using HDPro.Core.Controllers.Basic;
using HDPro.Entity.AttributeManager;
using HDPro.CY.Order.Services.K3Cloud.Models;
using Microsoft.Extensions.Logging;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_CustomerController
    {
        private readonly IOCP_CustomerService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OCP_CustomerController> _logger;

        [ActivatorUtilitiesConstructor]
        public OCP_CustomerController(
            IOCP_CustomerService service,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OCP_CustomerController> logger
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// 从K3Cloud同步客户数据
        /// </summary>
        /// <param name="pageSize">每页处理数量</param>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <returns>同步结果</returns>
        [HttpPost("SyncFromK3Cloud")]
        [ActionName("SyncFromK3Cloud")]
        public async Task<IActionResult> SyncFromK3CloudAsync(int pageSize = 1000, string customFilter = null)
        {
            try
            {
                _logger.LogInformation("开始执行客户数据同步");
                
                var result = await Service.SyncCustomersFromK3CloudAsync(pageSize, customFilter);
                
                if (result.Status)
                {
                    _logger.LogInformation("客户数据同步完成");
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        data = result.Data
                    });
                }
                else
                {
                    _logger.LogWarning($"客户数据同步失败: {result.Message}");
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message,
                        data = result.Data
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "客户数据同步异常");
                return BadRequest(new
                {
                    success = false,
                    message = $"客户数据同步异常: {ex.Message}",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取K3Cloud客户数据（不保存到数据库）
        /// </summary>
        /// <param name="pageIndex">页索引（从0开始）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <returns>K3Cloud客户数据</returns>
        [HttpGet("GetK3CloudCustomers")]
        [ActionName("GetK3CloudCustomers")]
        public async Task<IActionResult> GetK3CloudCustomersAsync(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 1000,
            [FromQuery] string filterString = null)
        {
            try
            {
                var result = await Service.GetK3CloudCustomersAsync(pageIndex, pageSize, filterString);
                
                if (result.Status)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        data = result.Data
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取K3Cloud客户数据异常");
                return BadRequest(new
                {
                    success = false,
                    message = $"获取K3Cloud客户数据异常: {ex.Message}",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取K3Cloud客户总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>客户总数</returns>
        [HttpGet("GetK3CloudCustomerCount")]
        [ActionName("GetK3CloudCustomerCount")]
        public async Task<IActionResult> GetK3CloudCustomerCountAsync([FromQuery] string filterString = null)
        {
            try
            {
                var result = await Service.GetK3CloudCustomerCountAsync(filterString);
                
                if (result.Status)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        data = result.Data
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取K3Cloud客户总数异常");
                return BadRequest(new
                {
                    success = false,
                    message = $"获取K3Cloud客户总数异常: {ex.Message}",
                    error = ex.Message
                });
            }
        }
    }
}
