/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_Negotiation",Enums.ActionPermissionOptions.Search)]
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
using HDPro.Core.Middleware;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_NegotiationController
    {
        private readonly IOCP_NegotiationService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_NegotiationController(
            IOCP_NegotiationService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取协商消息统计数据
        /// </summary>
        /// <returns>协商统计数据</returns>
        [HttpGet("GetStatistics")]
        [ApiActionPermission("OCP_Negotiation", HDPro.Core.Enums.ActionPermissionOptions.Search)]
        public async Task<IActionResult> GetStatisticsAsync()
        {
            try
            {
                var statistics = await _service.GetNegotiationStatisticsAsync();
                return Ok(new HDPro.Core.Utilities.WebResponseContent().OK("获取协商统计数据成功", statistics));
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"获取协商统计数据失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 更新协商状态
        /// </summary>
        /// <param name="negotiationID">协商ID</param>
        /// <param name="newStatus">新状态</param>
        /// <returns>更新结果</returns>
        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatusAsync([FromBody] UpdateNegotiationStatusRequest request)
        {
            try
            {
                var result = await _service.UpdateNegotiationStatusAsync(request.NegotiationID, request.Status);
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
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"更新协商状态失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 批量更新协商状态
        /// </summary>
        /// <param name="request">批量更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPost("BatchUpdateStatus")]
        public async Task<IActionResult> BatchUpdateStatusAsync([FromBody] BatchUpdateNegotiationStatusRequest request)
        {
            try
            {
                var result = await _service.BatchUpdateNegotiationStatusAsync(request.NegotiationIDs, request.Status);
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
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"批量更新协商状态失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取协商状态选项
        /// </summary>
        /// <returns>状态选项列表</returns>
        [HttpGet("GetStatusOptions")]
        [ApiActionPermission("OCP_Negotiation", HDPro.Core.Enums.ActionPermissionOptions.Search)]
        public IActionResult GetStatusOptions()
        {
            try
            {
                var options = _service.GetNegotiationStatusOptions();
                return Ok(new HDPro.Core.Utilities.WebResponseContent().OK("获取协商状态选项成功", options));
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"获取协商状态选项失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取消息状态过滤选项
        /// </summary>
        /// <returns>消息状态过滤选项列表</returns>
        [HttpGet("GetMessageStatusOptions")]
        public IActionResult GetMessageStatusOptions()
        {
            try
            {
                var options = _service.GetMessageStatusFilterOptions();
                return Ok(new HDPro.Core.Utilities.WebResponseContent().OK("获取消息状态过滤选项成功", options));
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"获取消息状态过滤选项失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取按业务类型统计的协商消息数据
        /// </summary>
        /// <param name="messageStatus">消息状态过滤参数（可选）：sent-已发送消息, pending-待回复消息, overdue-已超期消息, replied-已回复消息</param>
        /// <returns>按业务类型统计的协商消息数据</returns>
        [HttpGet("GetStatisticsByBusinessType")]
        [ApiActionPermission("OCP_Negotiation", HDPro.Core.Enums.ActionPermissionOptions.Search)]
        public async Task<IActionResult> GetStatisticsByBusinessTypeAsync([FromQuery] string messageStatus = null)
        {
            try
            {
                var result = await _service.GetNegotiationStatisticsByBusinessTypeAsync(messageStatus);
                return Json(new { status = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 根据协商ID发起OA流程
        /// 从委外跟踪表或采购跟踪表读取相关信息并发起流程
        /// </summary>
        /// <param name="negotiationId">协商ID</param>
        /// <returns>OA流程发起结果</returns>
        [HttpPost("StartOAProcess/{negotiationId}")]
        [ApiActionPermission("OCP_Negotiation", HDPro.Core.Enums.ActionPermissionOptions.Update)]
        public async Task<IActionResult> StartOAProcessByNegotiationIdAsync(long negotiationId)
        {
            try
            {
                // 获取当前用户ID
                var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";

                var result = await _service.StartOAProcessByNegotiationIdAsync(negotiationId, userId);

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
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"发起OA流程失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取当前用户协商情况统计（管理员可以看到所有人的统计）
        /// </summary>
        /// <returns>当前用户协商情况统计数据</returns>
        [HttpPost("GetUserNegotiationStatistics")]
        [ApiActionPermission("OCP_Negotiation", HDPro.Core.Enums.ActionPermissionOptions.Search)]
        public async Task<IActionResult> GetUserNegotiationStatisticsAsync()
        {
            try
            {
                var statistics = await _service.GetUserNegotiationStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"获取用户协商统计数据失败: {ex.Message}"));
            }
        }
    
    }

    /// <summary>
    /// 更新协商状态请求模型
    /// </summary>
    public class UpdateNegotiationStatusRequest
    {
        /// <summary>
        /// 协商ID
        /// </summary>
        public long NegotiationID { get; set; }

        /// <summary>
        /// 新状态
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// 批量更新协商状态请求模型
    /// </summary>
    public class BatchUpdateNegotiationStatusRequest
    {
        /// <summary>
        /// 协商ID列表
        /// </summary>
        public List<long> NegotiationIDs { get; set; }

        /// <summary>
        /// 新状态
        /// </summary>
        public string Status { get; set; }
    }
}
