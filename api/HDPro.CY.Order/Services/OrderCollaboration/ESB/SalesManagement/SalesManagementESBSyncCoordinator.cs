/*
 * 销售管理ESB同步协调器
 * 统一管理和协调销售管理相关的所有ESB同步操作
 */
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Core.Utilities;
using HDPro.Core.Extensions.AutofacManager;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.SalesManagement
{
    /// <summary>
    /// 销售管理ESB同步协调器
    /// 统一管理销售订单列表的同步
    /// </summary>
    public class SalesManagementESBSyncCoordinator
    {
        private readonly SalesOrderListESBSyncService _salesOrderListService;
        private readonly SalesOrderDetailESBSyncService _salesOrderDetailService;
        private readonly SalesBatchInfoESBSyncService _batchInfoService;
        private readonly ILogger<SalesManagementESBSyncCoordinator> _logger;

        public SalesManagementESBSyncCoordinator(
            SalesOrderListESBSyncService salesOrderListService,
            SalesOrderDetailESBSyncService salesOrderDetailService,
            SalesBatchInfoESBSyncService batchInfoService,
            ILogger<SalesManagementESBSyncCoordinator> logger)
        {
            _salesOrderListService = salesOrderListService;
            _salesOrderDetailService = salesOrderDetailService;
            _batchInfoService = batchInfoService;
            _logger = logger;
        }

        #region 完整同步流程

        /// <summary>
        /// 执行销售管理数据同步
        /// 仅同步销售订单列表
        /// </summary>
        /// <param name="startDate">开始日期 (yyyy-MM-dd)</param>
        /// <param name="endDate">结束日期 (yyyy-MM-dd)</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncAllSalesManagementData(
            string startDate = null,
            string endDate = null)
        {
            var response = new WebResponseContent();
            var overallStartTime = DateTime.Now;

            try
            {
                _logger.LogInformation($"开始销售管理数据同步，时间范围：{startDate} 到 {endDate}");

                // 同步销售订单列表
                _logger.LogInformation("=== 同步销售订单列表 ===");
                var orderListResult = await _salesOrderListService.SyncSalesOrderListData(startDate, endDate);

                if (!orderListResult.Status)
                {
                    _logger.LogError($"销售订单列表同步失败：{orderListResult.Message}");
                    return response.Error($"销售订单列表同步失败。错误：{orderListResult.Message}");
                }

                var totalTime = DateTime.Now - overallStartTime;
                var successMessage = $"销售管理数据同步完成，总耗时：{totalTime.TotalMinutes:F2}分钟。\n结果：{orderListResult.Message}";

                _logger.LogInformation(successMessage);
                return response.OK(successMessage);
            }
            catch (Exception ex)
            {
                var errorMsg = $"销售管理数据同步时发生异常：{ex.Message}";
                _logger.LogError(ex, errorMsg);
                return response.Error(errorMsg);
            }
        }

        #endregion

        #region 分步同步方法

        /// <summary>
        /// 仅同步销售订单列表
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncOrderListOnly(string startDate = null, string endDate = null)
        {
            _logger.LogInformation("开始单独同步销售订单列表");
            return await _salesOrderListService.SyncSalesOrderListData(startDate, endDate);
        }

        /// <summary>
        /// 根据订单号同步单个订单的明细
        /// </summary>
        /// <param name="salesOrderNo">销售订单号</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncSingleOrderDetail(string salesOrderNo)
        {
            _logger.LogInformation($"开始同步单个订单明细，订单号：{salesOrderNo}");
            return await _salesOrderDetailService.SyncByOrderNumber(salesOrderNo);
        }

        /// <summary>
        /// 根据计划跟踪号同步批次信息
        /// </summary>
        /// <param name="planTrackingNo">计划跟踪号</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncSingleBatchInfo(string planTrackingNo)
        {
            _logger.LogInformation($"开始同步单个批次信息，计划跟踪号：{planTrackingNo}");
            return await _batchInfoService.SyncByPlanTrackingNo(planTrackingNo);
        }

        #endregion



        #region 手动同步方法

        /// <summary>
        /// 手动同步销售管理数据
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncAllData(string startDate, string endDate)
        {
            _logger.LogInformation($"开始手动同步销售管理数据，时间范围：{startDate} 到 {endDate}");
            return await SyncAllSalesManagementData(startDate, endDate);
        }

        #endregion



        #region 静态实例

        public static SalesManagementESBSyncCoordinator Instance
        {
            get { return AutofacContainerModule.GetService<SalesManagementESBSyncCoordinator>(); }
        }

        #endregion
    }
}