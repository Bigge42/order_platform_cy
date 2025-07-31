/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_Material",Enums.ActionPermissionOptions.Search)]
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

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_MaterialController
    {
        private readonly IOCP_MaterialService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_MaterialController(
            IOCP_MaterialService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 从K3Cloud增量同步物料数据
        /// 基于本地物料表最大创建时间进行增量同步
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SyncFromK3Cloud")]
        public async Task<IActionResult> SyncMaterialsFromK3Cloud([FromBody] SyncMaterialRequest request = null)
        {
            try
            {
                var pageSize = request?.PageSize ?? 1000;
                var customFilter = request?.CustomFilter;

                var result = await _service.SyncMaterialsFromK3CloudAsync(pageSize, customFilter);
                
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
                return StatusCode(500, new WebResponseContent(false) 
                { 
                    Message = $"增量同步物料数据异常: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// 获取K3Cloud物料数据（不保存到数据库）
        /// </summary>
        /// <param name="request">查询请求参数</param>
        /// <returns>物料数据</returns>
        [HttpPost("GetK3CloudMaterials")]
        public async Task<IActionResult> GetK3CloudMaterials([FromBody] GetK3CloudMaterialRequest request = null)
        {
            try
            {
                var pageIndex = request?.PageIndex ?? 0;
                var pageSize = request?.PageSize ?? 1000;
                var filterString = request?.FilterString;

                var result = await _service.GetK3CloudMaterialsAsync(pageIndex, pageSize, filterString);
                
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
                return StatusCode(500, new WebResponseContent(false) 
                { 
                    Message = $"获取K3Cloud物料数据异常: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// 获取K3Cloud物料总数
        /// </summary>
        /// <param name="request">查询请求参数</param>
        /// <returns>物料总数</returns>
        [HttpPost("GetK3CloudMaterialCount")]
        public async Task<IActionResult> GetK3CloudMaterialCount([FromBody] GetK3CloudMaterialCountRequest request = null)
        {
            try
            {
                var filterString = request?.FilterString;

                var result = await _service.GetK3CloudMaterialCountAsync(filterString);
                
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
                return StatusCode(500, new WebResponseContent(false) 
                { 
                    Message = $"获取K3Cloud物料总数异常: {ex.Message}" 
                });
            }
        }
    }

    /// <summary>
    /// 增量同步物料请求参数
    /// </summary>
    public class SyncMaterialRequest
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

    /// <summary>
    /// 获取K3Cloud物料请求参数
    /// </summary>
    public class GetK3CloudMaterialRequest
    {
        /// <summary>
        /// 页索引，从0开始
        /// </summary>
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// 每页数量，默认1000
        /// </summary>
        public int PageSize { get; set; } = 1000;

        /// <summary>
        /// 过滤条件
        /// </summary>
        public string FilterString { get; set; }
    }

    /// <summary>
    /// 获取K3Cloud物料总数请求参数
    /// </summary>
    public class GetK3CloudMaterialCountRequest
    {
        /// <summary>
        /// 过滤条件
        /// </summary>
        public string FilterString { get; set; }
    }


}
