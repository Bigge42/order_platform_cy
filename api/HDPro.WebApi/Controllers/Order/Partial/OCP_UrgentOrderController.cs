/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_UrgentOrder",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Models;
using HDPro.Core.Filters;
using System.Linq;
using EnumsNET;
using HDPro.Core.Middleware;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_UrgentOrderController
    {
        private readonly IOCP_UrgentOrderService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_UrgentOrderController(
            IOCP_UrgentOrderService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取催单消息统计数据
        /// </summary>
        /// <returns>催单统计数据</returns>
        [HttpGet("GetStatistics")]
        [ApiActionPermission("OCP_UrgentOrder", HDPro.Core.Enums.ActionPermissionOptions.Search)]
        public async Task<IActionResult> GetStatisticsAsync()
        {
            try
            {
                var statistics = await _service.GetUrgentOrderStatisticsAsync();
                return Ok(new HDPro.Core.Utilities.WebResponseContent().OK("获取催单统计数据成功", statistics));
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"获取催单统计数据失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 更新催单状态
        /// </summary>
        /// <param name="urgentOrderId">催单ID</param>
        /// <param name="status">新状态</param>
        /// <returns>操作结果</returns>
        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatusAsync(long urgentOrderId, string status)
        {
            try
            {
                var result = await _service.UpdateUrgentOrderStatusAsync(urgentOrderId, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"更新催单状态失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 批量更新催单状态
        /// </summary>
        /// <param name="request">批量更新请求</param>
        /// <returns>操作结果</returns>
        [HttpPost("BatchUpdateStatus")]
        public async Task<IActionResult> BatchUpdateStatusAsync([FromBody] BatchUpdateStatusRequest request)
        {
            try
            {
                if (request == null || request.UrgentOrderIds == null || !request.UrgentOrderIds.Any())
                {
                    return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error("请求参数无效"));
                }

                var result = await _service.BatchUpdateUrgentOrderStatusAsync(request.UrgentOrderIds, request.Status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"批量更新催单状态失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取催单状态选项
        /// </summary>
        /// <returns>状态选项列表</returns>
        [HttpGet("GetStatusOptions")]
        public IActionResult GetStatusOptions()
        {
            try
            {
                var options = _service.GetUrgentOrderStatusOptions();
                return Ok(new HDPro.Core.Utilities.WebResponseContent().OK("获取催单状态选项成功", options));
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"获取催单状态选项失败: {ex.Message}"));
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
        /// 获取按业务类型统计的催单消息数据
        /// </summary>
        /// <param name="messageStatus">消息状态过滤参数（可选）：sent-已发送消息, pending-待回复消息, overdue-已超期消息, replied-已回复消息</param>
        /// <returns>按业务类型统计的催单消息数据</returns>
        [HttpGet("GetStatisticsByBusinessType")]
        [ApiActionPermission("OCP_UrgentOrder", HDPro.Core.Enums.ActionPermissionOptions.Search)]
        public async Task<IActionResult> GetStatisticsByBusinessTypeAsync([FromQuery] string messageStatus = null)
        {
            try
            {
                var result = await _service.GetUrgentOrderStatisticsByBusinessTypeAsync(messageStatus);
                return Ok(new HDPro.Core.Utilities.WebResponseContent().OK("获取催单按业务类型统计数据成功", result));
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"获取催单按业务类型统计数据失败: {ex.Message}"));
            }
        }


        /// <summary>
        /// 根据业务类型和业务主键查询催单和协商信息
        /// </summary>
        /// <param name="businessType">业务类型</param>
        /// <param name="businessKey">业务主键</param>
        /// <returns>业务关联的催单和协商信息</returns>
        [HttpGet("GetBusinessOrderCollaboration")]
        [ApiActionPermission("OCP_UrgentOrder", HDPro.Core.Enums.ActionPermissionOptions.Search)]
        public async Task<IActionResult> GetBusinessOrderCollaborationAsync(string businessType, string businessKey)
        {
            try
            {
                var result = await _service.GetBusinessOrderCollaborationAsync(businessType, businessKey);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"查询业务协同信息失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 批量添加催单
        /// </summary>
        /// <param name="urgentOrders">催单列表</param>
        /// <returns>操作结果</returns>
        [HttpPost("BatchAdd")]
        [ApiActionPermission("OCP_UrgentOrder", HDPro.Core.Enums.ActionPermissionOptions.Add)]
        public async Task<IActionResult> BatchAddAsync([FromBody] List<OCP_UrgentOrder> urgentOrders)
        {
            try
            {
                if (urgentOrders == null || !urgentOrders.Any())
                {
                    return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error("催单列表不能为空"));
                }

                var result = await _service.BatchAddAsync(urgentOrders);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"批量添加催单失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 批量获取默认负责人
        /// </summary>
        /// <param name="request">批量获取请求</param>
        /// <returns>默认负责人信息列表</returns>
        [HttpPost("BatchGetDefaultResponsible")]
        [ApiActionPermission("OCP_UrgentOrder", HDPro.Core.Enums.ActionPermissionOptions.Search)]
        public async Task<IActionResult> BatchGetDefaultResponsibleAsync([FromBody] BatchGetDefaultResponsibleRequest request)
        {
            try
            {
                if (request == null || request.BusinessItems == null || !request.BusinessItems.Any())
                {
                    return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error("请求参数不能为空"));
                }

                var result = await _service.BatchGetDefaultResponsibleAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"批量获取默认负责人失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取当前用户催单情况统计（管理员可以看到所有人的统计）
        /// </summary>
        /// <returns>当前用户催单情况统计数据</returns>
        [HttpPost("GetUserUrgentOrderStatistics")]
        public async Task<IActionResult> GetUserUrgentOrderStatisticsAsync()
        {
            try
            {
                var statistics = await _service.GetUserUrgentOrderStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"获取用户催单统计数据失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 物资供应中心用户对催单向SRM发起催货
        /// </summary>
        /// <param name="urgentOrderId">催单ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("SendToSRM")]
        [ApiActionPermission("OCP_UrgentOrder", HDPro.Core.Enums.ActionPermissionOptions.Update)]
        public async Task<IActionResult> SendUrgentOrderToSRMAsync(long urgentOrderId)
        {
            try
            {
                var result = await _service.SendUrgentOrderToSRMAsync(urgentOrderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"向SRM发起催货失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 批量向SRM发起催货
        /// </summary>
        /// <param name="request">批量发送请求</param>
        /// <returns>操作结果</returns>
        [HttpPost("BatchSendToSRM")]
        [ApiActionPermission("OCP_UrgentOrder", HDPro.Core.Enums.ActionPermissionOptions.Update)]
        public async Task<IActionResult> BatchSendUrgentOrderToSRMAsync([FromBody] BatchSendToSRMRequest request)
        {
            try
            {
                if (request == null || request.UrgentOrderIds == null || !request.UrgentOrderIds.Any())
                {
                    return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error("请求参数无效"));
                }

                var result = await _service.BatchSendUrgentOrderToSRMAsync(request.UrgentOrderIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new HDPro.Core.Utilities.WebResponseContent().Error($"批量向SRM发起催货失败: {ex.Message}"));
            }
        }

    }

    /// <summary>
    /// 批量更新状态请求模型
    /// </summary>
    public class BatchUpdateStatusRequest
    {
        /// <summary>
        /// 催单ID列表
        /// </summary>
        public List<long> UrgentOrderIds { get; set; }

        /// <summary>
        /// 新状态
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// 批量向SRM发送请求模型
    /// </summary>
    public class BatchSendToSRMRequest
    {
        /// <summary>
        /// 催单ID列表
        /// </summary>
        public List<long> UrgentOrderIds { get; set; }
    }
}
