/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_Supplier",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_SupplierController
    {
        private readonly IOCP_SupplierService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_SupplierController(
            IOCP_SupplierService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 从K3Cloud同步供应商数据
        /// </summary>
        /// <param name="pageSize">每页数量，默认1000</param>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <returns>同步结果</returns>
        [HttpPost("SyncFromK3Cloud")]
        public async Task<IActionResult> SyncSuppliersFromK3Cloud([FromBody] SyncSupplierRequest request = null)
        {
            try
            {
                var pageSize = request?.PageSize ?? 1000;
                var customFilter = request?.CustomFilter;

                var result = await _service.SyncSuppliersFromK3CloudAsync(pageSize, customFilter);
                
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
                return StatusCode(500, new { message = $"同步供应商数据异常: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取K3Cloud供应商数据（不保存到数据库）
        /// </summary>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <returns>供应商数据</returns>
        [HttpGet("GetK3CloudSuppliers")]
        public async Task<IActionResult> GetK3CloudSuppliers(int pageIndex = 0, int pageSize = 1000, string filterString = null)
        {
            try
            {
                var result = await _service.GetK3CloudSuppliersAsync(pageIndex, pageSize, filterString);
                
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
                return StatusCode(500, new { message = $"获取K3Cloud供应商数据异常: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取K3Cloud供应商总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>供应商总数</returns>
        [HttpGet("GetK3CloudSupplierCount")]
        public async Task<IActionResult> GetK3CloudSupplierCount(string filterString = null)
        {
            try
            {
                var result = await _service.GetK3CloudSupplierCountAsync(filterString);
                
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
                return StatusCode(500, new { message = $"获取K3Cloud供应商总数异常: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// 同步供应商请求参数
    /// </summary>
    public class SyncSupplierRequest
    {
        /// <summary>
        /// 每页数量，默认1000
        /// </summary>
        public int PageSize { get; set; } = 1000;

        /// <summary>
        /// 自定义过滤条件
        /// </summary>
        public string CustomFilter { get; set; }
    }
}
