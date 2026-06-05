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
using HDPro.CY.Order.IServices.OrderCollaboration;
using HDPro.Core.Filters;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.OrderTracking;
using HDPro.Core.Enums;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_OrderTrackingController
    {
        private readonly IOCP_OrderTrackingService _service;//访问业务代码
        private readonly IOCP_HomeDashboardService _homeDashboardService;
        private readonly OrderTrackingESBSyncCoordinator _coordinator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_OrderTrackingController(
            IOCP_OrderTrackingService service,
            IOCP_HomeDashboardService homeDashboardService,
            IHttpContextAccessor httpContextAccessor,
             OrderTrackingESBSyncCoordinator coordinator
        )
        : base(service)
        {
            _service = service;
            _homeDashboardService = homeDashboardService;
            _httpContextAccessor = httpContextAccessor;
            _coordinator = coordinator;
        }

        /// <summary>
        /// 计划修改看板导出，权限按新增看板校验，数据来自OCP_OrderTracking。
        /// </summary>
        /// <param name="loadData">导出参数</param>
        /// <returns>Excel文件</returns>
        [HttpPost("PlanModifyBoardExport")]
        [ApiActionPermission("WZ_OrderCyclePlanModifyBoard", ActionPermissionOptions.Export)]
        public IActionResult PlanModifyBoardExport([FromBody] PageDataOptions loadData)
        {
            var result = _service.ExportPlanModifyBoard(loadData);
            if (!result.Status)
            {
                return JsonNormal(result);
            }

            var fullPath = result.Data?.ToString();
            return File(
                System.IO.File.ReadAllBytes(fullPath),
                System.Net.Mime.MediaTypeNames.Application.Octet,
                System.IO.Path.GetFileName(fullPath)
            );
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
        /// 获取首页订单运营看板汇总数据。
        /// </summary>
        [HttpGet("GetHomeDashboard")]
        [ApiActionPermission("OCP_OrderTracking", ActionPermissionOptions.Search)]
        public async Task<IActionResult> GetHomeDashboard(
            string dateRange = "week",
            string businessType = "all",
            string customer = "all",
            string owner = "all",
            string keyword = null)
        {
            var result = await _homeDashboardService.GetHomeDashboardAsync(dateRange, businessType, customer, owner, keyword);
            return JsonNormal(result);
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

        /// <summary>
        /// 获取总任务完成统计数据
        /// </summary>
        /// <returns>总任务完成统计数据</returns>
        [HttpPost("GetTaskCompletionSummary")]
        public async Task<IActionResult> GetTaskCompletionSummary()
        {
            try
            {
                var result = await _service.GetTaskCompletionSummary();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"获取总任务完成统计失败：{ex.Message}" });
            }
        }
    }
}
