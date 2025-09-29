/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_LackMtrlResult",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;
using HDPro.Core.Filters;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.LackMaterial;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_LackMtrlResultController
    {
        private readonly IOCP_LackMtrlResultService _service;//访问业务代码
        private readonly LackMtrlResultESBSyncCoordinator _corrdinator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_LackMtrlResultController(
            IOCP_LackMtrlResultService service,
            IHttpContextAccessor httpContextAccessor,
            LackMtrlResultESBSyncCoordinator syncCoordinator
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
            _corrdinator = syncCoordinator;
        }

        /// <summary>
        /// 手动触发ESB缺料数据同步
        /// </summary>
        /// <param name="computeId">运算ID</param>
        /// <returns>同步结果</returns>
        [HttpPost("ManualSyncLackMtrlData")]
        public async Task<IActionResult> ManualSyncLackMtrlData(string computeId)
        {
            try
            {
                var result = await _corrdinator.ManualSync(computeId);
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"同步失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取默认运算方案的缺料情况统计
        /// </summary>
        /// <returns>按单据类型分组的缺料统计</returns>
        [HttpPost("GetDefaultLackMtrlSummary")]
        public async Task<IActionResult> GetDefaultLackMtrlSummary()
        {
            try
            {
                var result = await _service.GetDefaultLackMtrlSummary();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"获取缺料统计失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 获取近7天的缺料情况统计
        /// </summary>
        /// <returns>近7天每日缺料统计数据</returns>
        [HttpPost("GetLast7DaysLackMtrlTrend")]
        public async Task<IActionResult> GetLast7DaysLackMtrlTrend()
        {
            try
            {
                var result = await _service.GetLast7DaysLackMtrlTrend();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"获取近7天缺料趋势失败：{ex.Message}" });
            }
        }

    }
}
