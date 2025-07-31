/*
 * 消息统计控制器
 * 提供催单和协商的汇总统计数据
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IServices;
using HDPro.Core.Filters;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace HDPro.CY.Order.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageStatisticsController : ControllerBase
    {
        private readonly IOCP_UrgentOrderService _urgentOrderService;
        private readonly IOCP_NegotiationService _negotiationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public MessageStatisticsController(
            IOCP_UrgentOrderService urgentOrderService,
            IOCP_NegotiationService negotiationService,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _urgentOrderService = urgentOrderService;
            _negotiationService = negotiationService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取汇总消息统计数据
        /// 返回催单和协商的汇总统计：发送消息数、待回复消息数、已超期消息数、已回复消息数
        /// </summary>
        /// <returns>汇总统计数据</returns>
        [HttpGet("GetSummaryStatistics")]
        [ApiActionPermission()]
        public async Task<IActionResult> GetSummaryStatisticsAsync()
        {
            try
            {
                // 串行获取催单和协商的统计数据，避免DbContext并发问题
                var urgentOrderStats = await _urgentOrderService.GetUrgentOrderStatisticsAsync();
                var negotiationStats = await _negotiationService.GetNegotiationStatisticsAsync();

                // 汇总统计数据
                var summaryStatistics = new SummaryStatisticsDto
                {
                    // 总的发送消息数
                    TotalSentCount = urgentOrderStats.SentCount + negotiationStats.SentCount,
                    // 总的待回复消息数
                    TotalPendingCount = urgentOrderStats.PendingCount + negotiationStats.PendingCount,
                    // 总的已超期消息数
                    TotalOverdueCount = urgentOrderStats.OverdueCount + negotiationStats.OverdueCount,
                    // 总的已回复消息数
                    TotalRepliedCount = urgentOrderStats.RepliedCount + negotiationStats.RepliedCount,
                    
                    // 详细分类统计
                    Details = new StatisticsDetailsDto
                    {
                        UrgentOrder = urgentOrderStats,
                        Negotiation = negotiationStats
                    },
                    
                    // 统计时间
                    StatisticsTime = DateTime.Now
                };

                var response = new WebResponseContent().OK("获取汇总统计数据成功", summaryStatistics);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new WebResponseContent().Error($"获取汇总统计数据失败: {ex.Message}");
                return BadRequest(response);
            }
        }

        /// <summary>
        /// 获取催单统计数据
        /// </summary>
        /// <returns>催单统计数据</returns>
        [HttpGet("GetUrgentOrderStatistics")]
        [ApiActionPermission()]
        public async Task<IActionResult> GetUrgentOrderStatisticsAsync()
        {
            try
            {
                var statistics = await _urgentOrderService.GetUrgentOrderStatisticsAsync();
                var response = new WebResponseContent().OK("获取催单统计数据成功", statistics);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new WebResponseContent().Error($"获取催单统计数据失败: {ex.Message}");
                return BadRequest(response);
            }
        }

        /// <summary>
        /// 获取协商统计数据
        /// </summary>
        /// <returns>协商统计数据</returns>
        [HttpGet("GetNegotiationStatistics")]
        [ApiActionPermission()]
        public async Task<IActionResult> GetNegotiationStatisticsAsync()
        {
            try
            {
                var statistics = await _negotiationService.GetNegotiationStatisticsAsync();
                var response = new WebResponseContent().OK("获取协商统计数据成功", statistics);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new WebResponseContent().Error($"获取协商统计数据失败: {ex.Message}");
                return BadRequest(response);
            }
        }

        /// <summary>
        /// 获取按业务类型统计的催单消息数据
        /// </summary>
        /// <returns>按业务类型统计的催单消息数据</returns>
        [HttpGet("GetUrgentOrderStatisticsByBusinessType")]
        [ApiActionPermission()]
        public async Task<IActionResult> GetUrgentOrderStatisticsByBusinessTypeAsync()
        {
            try
            {
                var statistics = await _urgentOrderService.GetUrgentOrderStatisticsByBusinessTypeAsync();
                var response = new WebResponseContent().OK("获取催单按业务类型统计数据成功", statistics);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new WebResponseContent().Error($"获取催单按业务类型统计数据失败: {ex.Message}");
                return BadRequest(response);
            }
        }

        /// <summary>
        /// 获取按业务类型统计的协商消息数据
        /// </summary>
        /// <returns>按业务类型统计的协商消息数据</returns>
        [HttpGet("GetNegotiationStatisticsByBusinessType")]
        [ApiActionPermission()]
        public async Task<IActionResult> GetNegotiationStatisticsByBusinessTypeAsync()
        {
            try
            {
                var statistics = await _negotiationService.GetNegotiationStatisticsByBusinessTypeAsync();
                var response = new WebResponseContent().OK("获取协商按业务类型统计数据成功", statistics);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new WebResponseContent().Error($"获取协商按业务类型统计数据失败: {ex.Message}");
                return BadRequest(response);
            }
        }

        /// <summary>
        /// 获取汇总的按业务类型统计消息数据（催单+协商）
        /// </summary>
        /// <returns>汇总的按业务类型统计消息数据</returns>
        [HttpGet("GetSummaryStatisticsByBusinessType")]
        [ApiActionPermission()]
        public async Task<IActionResult> GetSummaryStatisticsByBusinessTypeAsync()
        {
            try
            {
                // 串行获取催单和协商的按业务类型统计数据
                var urgentOrderStats = await _urgentOrderService.GetUrgentOrderStatisticsByBusinessTypeAsync();
                var negotiationStats = await _negotiationService.GetNegotiationStatisticsByBusinessTypeAsync();

                // 合并统计数据
                var allBusinessTypes = urgentOrderStats.BusinessTypeList
                    .Select(x => x.BusinessTypeCode)
                    .Union(negotiationStats.BusinessTypeList.Select(x => x.BusinessTypeCode))
                    .Distinct()
                    .ToList();

                var mergedBusinessTypeList = new List<BusinessTypeStatisticsDto>();

                foreach (var businessType in allBusinessTypes)
                {
                    var urgentStat = urgentOrderStats.BusinessTypeList.FirstOrDefault(x => x.BusinessTypeCode == businessType);
                    var negotiationStat = negotiationStats.BusinessTypeList.FirstOrDefault(x => x.BusinessTypeCode == businessType);

                    var merged = new BusinessTypeStatisticsDto
                    {
                        BusinessTypeCode = businessType,
                        BusinessTypeName = urgentStat?.BusinessTypeName ?? negotiationStat?.BusinessTypeName ?? "未知",
                        SentCount = (urgentStat?.SentCount ?? 0) + (negotiationStat?.SentCount ?? 0),
                        PendingCount = (urgentStat?.PendingCount ?? 0) + (negotiationStat?.PendingCount ?? 0),
                        OverdueCount = (urgentStat?.OverdueCount ?? 0) + (negotiationStat?.OverdueCount ?? 0),
                        RepliedCount = (urgentStat?.RepliedCount ?? 0) + (negotiationStat?.RepliedCount ?? 0)
                    };

                    mergedBusinessTypeList.Add(merged);
                }

                // 创建汇总的全部消息统计
                var totalAllMessages = new BusinessTypeStatisticsDto
                {
                    BusinessTypeCode = "ALL",
                    BusinessTypeName = "全部消息",
                    SentCount = urgentOrderStats.AllMessages.SentCount + negotiationStats.AllMessages.SentCount,
                    PendingCount = urgentOrderStats.AllMessages.PendingCount + negotiationStats.AllMessages.PendingCount,
                    OverdueCount = urgentOrderStats.AllMessages.OverdueCount + negotiationStats.AllMessages.OverdueCount,
                    RepliedCount = urgentOrderStats.AllMessages.RepliedCount + negotiationStats.AllMessages.RepliedCount
                };

                var summaryResult = new BusinessTypeMessageStatisticsDto
                {
                    AllMessages = totalAllMessages,
                    BusinessTypeList = mergedBusinessTypeList,
                    StatisticsTime = DateTime.Now
                };

                var response = new WebResponseContent().OK("获取汇总按业务类型统计数据成功", summaryResult);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new WebResponseContent().Error($"获取汇总按业务类型统计数据失败: {ex.Message}");
                return BadRequest(response);
            }
        }

        /// <summary>
        /// 获取业务类型选项
        /// </summary>
        /// <returns>业务类型选项列表</returns>
        [HttpGet("GetBusinessTypeOptions")]
        [ApiActionPermission()]
        public IActionResult GetBusinessTypeOptions()
        {
            try
            {
                var options = HDPro.CY.Order.Services.OrderCollaboration.Common.BusinessConstants.GetAllBusinessTypes()
                    .Select(x => new { value = x.Code, text = x.Name })
                    .ToList();

                var response = new WebResponseContent().OK("获取业务类型选项成功", options);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new WebResponseContent().Error($"获取业务类型选项失败: {ex.Message}");
                return BadRequest(response);
            }
        }
    }
} 