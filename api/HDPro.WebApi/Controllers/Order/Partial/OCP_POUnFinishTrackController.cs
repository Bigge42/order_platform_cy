/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("OCP_POUnFinishTrack",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;
using HDPro.Core.Enums;
using HDPro.Core.Filters;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.Purchase;

namespace HDPro.CY.Order.Controllers
{
    public partial class OCP_POUnFinishTrackController
    {
        private readonly IOCP_POUnFinishTrackService _service;//访问业务代码
        private readonly PurchaseESBSyncCoordinator _purchaseCoordinator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OCP_POUnFinishTrackController(
            IOCP_POUnFinishTrackService service,
            IHttpContextAccessor httpContextAccessor,
            PurchaseESBSyncCoordinator purchaseCoordinator
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
            _purchaseCoordinator = purchaseCoordinator;
        }

        /// <summary>
        /// 手动同步采购跟踪数据，权限跟随采购跟踪页面查询权限。
        /// </summary>
        /// <returns>同步结果</returns>
        [HttpPost("SyncPurchaseTracking")]
        [ApiActionPermission("OCP_POUnFinishTrack", ActionPermissionOptions.Search)]
        public async Task<IActionResult> SyncPurchaseTracking()
        {
            try
            {
                var today = DateTime.Today;
                var startDate = today.AddDays(-1).ToString("yyyy-MM-dd");
                var endDate = today.AddDays(1).ToString("yyyy-MM-dd");
                var result = await _purchaseCoordinator.SyncPurchaseUnFinishTrackOnly(startDate, endDate);

                if (result.Status)
                {
                    result.Message = string.IsNullOrWhiteSpace(result.Message)
                        ? $"采购跟踪同步完成，日期范围：{startDate} 至 {endDate}"
                        : $"{result.Message}，日期范围：{startDate} 至 {endDate}";
                }

                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"采购跟踪同步失败：{ex.Message}"));
            }
        }
    }
}
