/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_OrderTracking",Enums.ActionPermissionOptions.Search)]
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
using HDPro.CY.Order.Services.OrderCollaboration.ESB.OrderTracking;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_OrderTrackingController
    {
        private readonly IOCP_OrderTrackingService _service;//访问业务代码
        private readonly OrderTrackingESBSyncCoordinator _coordinator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_OrderTrackingController(
            IOCP_OrderTrackingService service,
            IHttpContextAccessor httpContextAccessor,
             OrderTrackingESBSyncCoordinator coordinator
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
            _coordinator = coordinator;
        }
        /// <summary>
        /// 手动触发ESB订单数据同步
        /// </summary>
        /// <param name="startDate">开始时间（格式：yyyy-MM-dd）</param>
        /// <param name="endDate">结束时间（格式：yyyy-MM-dd）</param>
        /// <returns>同步结果</returns>
        [HttpPost("ManualSyncOrderData")]
        public async Task<IActionResult> ManualSyncOrderData(string startDate = null, string endDate = null)
        {
            try
            {

                var result = await _coordinator.ManualSync(startDate, endDate);
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"同步失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时同步
        /// </summary>
        /// <param name="startDate">开始时间（格式：yyyy-MM-dd）</param>
        /// <param name="endDate">结束时间（格式：yyyy-MM-dd）</param>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpPost("SyncOrderData")]
        public async Task<IActionResult> SyncOrderDataFromESB(string startDate = null, string endDate = null)
        {
            try
            {

                var result = await _coordinator.ManualSync(startDate, endDate);
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"同步失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取近14天订单完成统计数据
        /// </summary>
        /// <param name="scheduleMonth">排产月份（格式：yyyy-MM，如：2025-07）</param>
        /// <returns>近14天订单完成统计折线图数据</returns>
        [HttpPost("GetOrderCompletionStats")]
        public async Task<IActionResult> GetOrderCompletionStats(string scheduleMonth)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(scheduleMonth))
                {
                    scheduleMonth = DateTime.Now.ToString("yyyyMM");
                }

                var result = await _service.GetOrderCompletionStatsAsync(scheduleMonth);
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"获取订单完成统计数据失败：{ex.Message}"));
            }
        }
    }
}
