/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("WZ_OrderCycleBase",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices;
using HDPro.Core.Utilities;
using Microsoft.AspNetCore.Authorization;
using HDPro.Core.Filters;
using HDPro.CY.Order.Services;
using HDPro.Core.EFDbContext;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Controllers
{
    public partial class WZ_OrderCycleBaseController
    {
        /// <summary>
        /// 刷新ERP订单跟踪数据（ApiTask）
        /// </summary>
        /// <returns>刷新结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("refresh-order-tracking")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshOrderTrackingData()
        {
            var startTime = DateTime.Now;
            var stopwatch = Stopwatch.StartNew();
            WebResponseContent result;

            try
            {
                result = await ERP_OrderTrackingService.Instance.SyncERPOrderTrackingAsync();
            }
            catch (Exception ex)
            {
                result = new WebResponseContent().Error($"刷新订单数据失败：{ex.Message}");
            }

            stopwatch.Stop();
            await WriteApiTaskQuartzLogAsync(result, startTime, DateTime.Now, stopwatch.Elapsed);
            return JsonNormal(result);
        }

        private static async Task WriteApiTaskQuartzLogAsync(WebResponseContent result, DateTime startTime, DateTime endTime, TimeSpan elapsed)
        {
            try
            {
                using var dbContext = new SysDbContext();
                var log = new Sys_QuartzLog
                {
                    LogId = Guid.NewGuid(),
                    TaskName = "WZ订单看板-刷新订单数据(ApiTask)",
                    Id = null,
                    StratDate = startTime,
                    EndDate = endTime,
                    CreateDate = startTime,
                    ElapsedTime = Math.Max(0, (int)Math.Ceiling(elapsed.TotalSeconds)),
                    Result = result?.Status == true ? 1 : 0,
                    ResponseContent = result?.Message,
                    ErrorMsg = result?.Status == true ? null : result?.Message
                };

                dbContext.Set<Sys_QuartzLog>().Add(log);
                await dbContext.SaveChangesAsync();
            }
            catch
            {
                // 记录日志失败不影响主流程返回
            }
        }

        /// <summary>
        /// 从订单跟踪表同步数据到订单周期基础表
        /// </summary>
        /// <param name="startDate">审核日期起</param>
        /// <param name="endDate">审核日期止</param>
        /// <returns>同步的记录数量</returns>
        [HttpPost("sync-from-order-tracking")]
        [AllowAnonymous]
        public async Task<IActionResult> SyncFromOrderTracking([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return JsonNormal(new WebResponseContent().Error("审核日期范围不合法，请重新选择"));
                }

                var result = await Service.SyncFromOrderTrackingAsync(startDate, endDate);
                return JsonNormal(new WebResponseContent().OK($"同步完成，共同步 {result} 条数据", result, false));
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"同步失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 批量调用阀门规则服务，回填订单周期信息
        /// </summary>
        /// <returns>回填批量结果摘要</returns>
        [HttpPost("batch-call-valve-rule-service")]
        [AllowAnonymous]
        public async Task<IActionResult> BatchCallValveRuleService()
        {
            try
            {
                var result = await Service.BatchCallValveRuleServiceAsync();
                return JsonNormal(result);
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"规则服务调用失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 规则直判回填阀门类别
        /// </summary>
        /// <param name="batchSize">单批处理数量</param>
        /// <returns>回填的记录数量</returns>
        [HttpPost("fill-valve-category-by-rule")]
        [AllowAnonymous]
        public async Task<IActionResult> FillValveCategoryByRule([FromQuery] int batchSize = 1000)
        {
            try
            {
                var result = await Service.FillValveCategoryByRuleAsync(batchSize);
                return JsonNormal(new WebResponseContent().OK($"回填完成，共更新 {result} 条记录", result, false));
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"回填失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 规则直判回填指派生产线
        /// </summary>
        /// <param name="batchSize">单批处理数量</param>
        /// <returns>回填批量结果摘要</returns>
        [HttpPost("batch-assign-production-line-by-rule")]
        [AllowAnonymous]
        public async Task<IActionResult> BatchAssignProductionLineByRule([FromQuery] int batchSize = 1000)
        {
            try
            {
                var result = await Service.BatchAssignProductionLineByRuleAsync(batchSize);
                return JsonNormal(new WebResponseContent().OK("规则直判回填完成", result, false));
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"规则直判回填失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 同步预排产输出数据
        /// </summary>
        /// <returns>同步的记录数量</returns>
        [HttpPost("sync-pre-production-output")]
        [AllowAnonymous]
        public async Task<IActionResult> SyncPreProductionOutput()
        {
            try
            {
                var result = await Service.SyncPreProductionOutputAsync();
                return JsonNormal(new WebResponseContent().OK($"同步完成，共同步 {result} 条数据", result, false));
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"同步失败：{ex.Message}"));
            }
        }

        /// <summary>
        /// 计算产能排产日期
        /// </summary>
        /// <returns>计算结果摘要</returns>
        [HttpPost("calculate-capacity-schedule-date")]
        [AllowAnonymous]
        public async Task<IActionResult> CalculateCapacityScheduleDate()
        {
            try
            {
                var result = await Service.CalculateCapacityScheduleDateAsync();
                return JsonNormal(new WebResponseContent().OK("计算完成", result, false));
            }
            catch (Exception ex)
            {
                return JsonNormal(new WebResponseContent().Error($"计算失败：{ex.Message}"));
            }
        }
    }
}
