using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.TechManagement
{
    /// <summary>
    /// 技术管理ESB同步协调器
    /// 负责统一协调技术管理业务的ESB数据同步操作
    /// </summary>
    public class TechManagementESBSyncCoordinator
    {
        private readonly TechManagementESBSyncService _techManagementSyncService;
        private readonly ILogger<TechManagementESBSyncCoordinator> _logger;

        public TechManagementESBSyncCoordinator(
            TechManagementESBSyncService techManagementSyncService,
            ILogger<TechManagementESBSyncCoordinator> logger)
        {
            _techManagementSyncService = techManagementSyncService;
            _logger = logger;
        }

        /// <summary>
        /// 同步技术管理数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncTechManagementData(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var startTime = DateTime.Now;

            try
            {
                _logger.LogInformation("========== 开始技术管理ESB数据同步 ==========");
                _logger.LogInformation($"同步时间范围：{startDate ?? "默认"} 到 {endDate ?? "默认"}");

                // 执行技术管理数据同步
                var syncResult = await _techManagementSyncService.SyncDataFromESB(startDate, endDate);

                var duration = DateTime.Now - startTime;
                _logger.LogInformation($"========== 技术管理ESB数据同步完成 ==========");
                _logger.LogInformation($"总耗时：{duration.TotalSeconds:F2} 秒");

                if (syncResult.Status)
                {
                    return response.OK($"技术管理数据同步成功。{syncResult.Message}。耗时：{duration.TotalSeconds:F2} 秒", syncResult.Data);
                }
                else
                {
                    return response.Error($"技术管理数据同步失败：{syncResult.Message}");
                }
            }
            catch (Exception ex)
            {
                var duration = DateTime.Now - startTime;
                _logger.LogError(ex, $"技术管理ESB数据同步过程中发生异常，耗时：{duration.TotalSeconds:F2} 秒");
                return response.Error($"技术管理数据同步异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 手动同步技术管理数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncTechManagementData(string startDate, string endDate)
        {
            if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
            {
                return new WebResponseContent().Error("手动同步必须指定开始时间和结束时间");
            }

            return await SyncTechManagementData(startDate, endDate);
        }

        /// <summary>
        /// 获取技术管理同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        public WebResponseContent GetTechManagementSyncStatus()
        {
            try
            {
                var status = new
                {
                    ServiceStatus = _techManagementSyncService != null ? "运行正常" : "服务未启动",
                    LastCheckTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    SupportedOperations = new[]
                    {
                        "技术管理数据同步 - 基于销售订单明细的BOM创建日期查询"
                    },
                    ESBInterface = new
                    {
                        Path = "/SearchBOMCreateDate",
                        Description = "查询技术管理BOM创建日期接口",
                        Parameters = new
                        {
                            FSTARTDATE = "时间范围开始时间",
                            FENDDATE = "时间范围结束时间"
                        }
                    }
                };

                return new WebResponseContent().OK("技术管理ESB同步状态正常", status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取技术管理同步状态时发生异常");
                return new WebResponseContent().Error($"获取状态失败：{ex.Message}");
            }
        }
    }
} 