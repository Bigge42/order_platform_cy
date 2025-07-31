using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Entity.SystemModels;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.SubOrder;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.Purchase;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.OrderTracking;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.LackMaterial;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB
{
    /// <summary>
    /// ESB主协调器 - 统一管理所有业务领域的ESB同步
    /// 支持业务领域：采购、委外、缺料、订单跟踪、部件、整机、技术
    /// </summary>
    public class ESBMasterCoordinator
    {
        private readonly PurchaseESBSyncCoordinator _purchaseCoordinator;
        private readonly SubOrderESBSyncCoordinator _subOrderCoordinator;
        private readonly OrderTrackingESBSyncCoordinator _orderTrackingCoordinator;
        private readonly LackMtrlResultESBSyncCoordinator _lackMtrlResultCoordinator;
        private readonly ILogger<ESBMasterCoordinator> _logger;

        // TODO: 后续添加其他业务领域协调器
        // private readonly PartESBSyncCoordinator _partCoordinator;
        // private readonly WholeUnitESBSyncCoordinator _wholeUnitCoordinator;
        // private readonly TechnologyESBSyncCoordinator _technologyCoordinator;

        public ESBMasterCoordinator(
            PurchaseESBSyncCoordinator purchaseCoordinator,
            SubOrderESBSyncCoordinator subOrderCoordinator,
            OrderTrackingESBSyncCoordinator orderTrackingCoordinator,
            LackMtrlResultESBSyncCoordinator lackMtrlResultCoordinator,
            ILogger<ESBMasterCoordinator> logger)
        {
            _purchaseCoordinator = purchaseCoordinator;
            _subOrderCoordinator = subOrderCoordinator;
            _orderTrackingCoordinator = orderTrackingCoordinator;
            _lackMtrlResultCoordinator = lackMtrlResultCoordinator;
            _logger = logger;
        }

        #region 统一同步接口

        /// <summary>
        /// 同步所有业务领域的数据
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncAllBusinessDomains(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var results = new List<string>();
            var errors = new List<string>();

            try
            {
                _logger.LogInformation("开始所有业务领域的完整ESB数据同步");

                // 1. 采购订单数据同步
                try
                {
                    var purchaseResult = await _purchaseCoordinator.SyncAllPurchaseOrderData(startDate, endDate);
                    if (purchaseResult.Status)
                    {
                        results.Add($"采购领域：{purchaseResult.Message}");
                    }
                    else
                    {
                        errors.Add($"采购领域同步失败：{purchaseResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"采购领域同步异常：{ex.Message}");
                    _logger.LogError(ex, "采购领域同步异常");
                }

                // 2. 委外订单数据同步
                try
                {
                    var subOrderResult = await _subOrderCoordinator.SyncAllSubOrderData(startDate, endDate);
                    if (subOrderResult.Status)
                    {
                        results.Add($"委外领域：{subOrderResult.Message}");
                    }
                    else
                    {
                        errors.Add($"委外领域同步失败：{subOrderResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"委外领域同步异常：{ex.Message}");
                    _logger.LogError(ex, "委外领域同步异常");
                }

                // 3. 订单跟踪数据同步
                try
                {
                    var orderTrackingResult = await _orderTrackingCoordinator.ExecuteFullSync(startDate, endDate);
                    if (orderTrackingResult.Status)
                    {
                        results.Add($"订单跟踪领域：{orderTrackingResult.Message}");
                    }
                    else
                    {
                        errors.Add($"订单跟踪领域同步失败：{orderTrackingResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"订单跟踪领域同步异常：{ex.Message}");
                    _logger.LogError(ex, "订单跟踪领域同步异常");
                }

                // 注意：缺料数据同步需要运算ID参数，不适合在全量同步中调用
                // 缺料数据同步应该通过特定的运算ID触发

                // TODO: 4. 部件数据同步
                // TODO: 5. 整机数据同步
                // TODO: 6. 技术数据同步

                // 汇总结果
                return GenerateSyncResult(response, results, errors, "所有业务领域");
            }
            catch (Exception ex)
            {
                var errorMessage = $"ESB数据同步过程中发生系统异常：{ex.Message}";
                _logger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 按业务领域同步数据
        /// </summary>
        /// <param name="businessDomain">业务领域名称</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <param name="computeId">运算ID（缺料数据同步专用）</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncByBusinessDomain(string businessDomain, string startDate = null, string endDate = null, string computeId = null)
        {
            _logger.LogInformation($"开始同步指定业务领域：{businessDomain}，时间范围：{startDate} 到 {endDate}，运算ID：{computeId}");

            return businessDomain?.ToLower() switch
            {
                "purchase" or "采购" => await _purchaseCoordinator.SyncAllPurchaseOrderData(startDate, endDate),
                "suborder" or "委外" => await _subOrderCoordinator.SyncAllSubOrderData(startDate, endDate),
                "ordertracking" or "订单跟踪" => await _orderTrackingCoordinator.ExecuteFullSync(startDate, endDate),
                "lackmtrlresult" or "缺料运算结果" or "缺料" => 
                    string.IsNullOrEmpty(computeId) 
                        ? new WebResponseContent().Error("缺料数据同步需要提供运算ID参数") 
                        : await _lackMtrlResultCoordinator.ExecuteSyncByComputeId(computeId),
                // "part" or "部件" => await _partCoordinator.SyncAllPartData(startDate, endDate),
                // "wholeunit" or "整机" => await _wholeUnitCoordinator.SyncAllWholeUnitData(startDate, endDate),
                // "technology" or "技术" => await _technologyCoordinator.SyncAllTechnologyData(startDate, endDate),
                _ => new WebResponseContent().Error($"不支持的业务领域：{businessDomain}")
            };
        }

        #endregion

        #region 缺料数据专用同步接口

        /// <summary>
        /// 根据运算ID同步缺料数据
        /// </summary>
        /// <param name="computeId">运算ID</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncLackMtrlResultByComputeId(string computeId)
        {
            _logger.LogInformation($"开始根据运算ID同步缺料数据，运算ID：{computeId}");
            return await _lackMtrlResultCoordinator.ExecuteSyncByComputeId(computeId);
        }

        /// <summary>
        /// 手动触发缺料数据同步
        /// </summary>
        /// <param name="computeId">运算ID</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncLackMtrlResult(string computeId)
        {
            var currentUser = HDPro.Core.ManageUser.UserContext.Current?.UserName ?? "未知用户";
            _logger.LogInformation($"手动触发缺料数据ESB同步，操作用户：{currentUser}，运算ID：{computeId}");
            return await _lackMtrlResultCoordinator.ManualSync(computeId);
        }

        #endregion

        #region 手动同步接口

        /// <summary>
        /// 手动触发所有业务领域数据同步
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncAllBusinessDomains(string startDate, string endDate)
        {
            var currentUser = HDPro.Core.ManageUser.UserContext.Current?.UserName ?? "未知用户";
            _logger.LogInformation($"手动触发所有业务领域ESB数据同步，操作用户：{currentUser}，时间范围：{startDate} 到 {endDate}");
            return await SyncAllBusinessDomains(startDate, endDate);
        }

        /// <summary>
        /// 手动触发指定业务领域数据同步
        /// </summary>
        /// <param name="businessDomain">业务领域</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncByBusinessDomain(string businessDomain, string startDate, string endDate)
        {
            var currentUser = HDPro.Core.ManageUser.UserContext.Current?.UserName ?? "未知用户";
            _logger.LogInformation($"手动触发{businessDomain}业务领域ESB数据同步，操作用户：{currentUser}，时间范围：{startDate} 到 {endDate}");
            return await SyncByBusinessDomain(businessDomain, startDate, endDate);
        }

        #endregion

        #region 状态查询接口

        /// <summary>
        /// 获取所有业务领域的同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        public async Task<WebResponseContent> GetAllBusinessDomainStatus()
        {
            var response = new WebResponseContent();
            
            try
            {
                var statusInfo = new
                {
                    LastSyncTime = DateTime.Now, // 应该从数据库或缓存获取实际的最后同步时间
                    SupportedBusinessDomains = new[]
                    {
                        new { Domain = "Purchase", Name = "采购", Status = "已实现", Description = "采购订单头、明细同步" },
                        new { Domain = "SubOrder", Name = "委外", Status = "已实现", Description = "委外订单头、明细同步" },
                        new { Domain = "Material", Name = "缺料", Status = "待实现", Description = "缺料数据同步" },
                        new { Domain = "OrderTracking", Name = "订单跟踪", Status = "已实现", Description = "订单跟踪数据同步" },
                        new { Domain = "Part", Name = "部件", Status = "待实现", Description = "部件数据同步" },
                        new { Domain = "WholeUnit", Name = "整机", Status = "待实现", Description = "整机数据同步" },
                        new { Domain = "Technology", Name = "技术", Status = "待实现", Description = "技术数据同步" }
                    },
                    AvailableOperations = new[]
                    {
                        "SyncAllBusinessDomains - 同步所有业务领域数据",
                        "SyncByBusinessDomain - 按业务领域同步数据",
                        "ManualSyncAllBusinessDomains - 手动触发所有业务领域同步",
                        "ManualSyncByBusinessDomain - 手动触发指定业务领域同步"
                    }
                };

                return response.OK("ESB主协调器状态获取成功", statusInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取ESB主协调器状态失败");
                return response.Error($"获取状态失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定业务领域的同步状态
        /// </summary>
        /// <param name="businessDomain">业务领域</param>
        /// <param name="computeId">运算ID（缺料数据状态查询专用）</param>
        /// <returns>状态信息</returns>
        public async Task<WebResponseContent> GetBusinessDomainStatus(string businessDomain, string computeId = null)
        {
            return businessDomain?.ToLower() switch
            {
                "purchase" or "采购" => await _purchaseCoordinator.GetSyncStatus(),
                "suborder" or "委外" => await _subOrderCoordinator.GetSyncStatus(),
                "ordertracking" or "订单跟踪" => await _orderTrackingCoordinator.GetSyncStatus(),
                "lackmtrlresult" or "缺料运算结果" or "缺料" => 
                    string.IsNullOrEmpty(computeId) 
                        ? new WebResponseContent().Error("缺料数据状态查询需要提供运算ID参数") 
                        : await _lackMtrlResultCoordinator.GetSyncStatus(computeId),
                // "material" or "缺料" => await _materialCoordinator.GetSyncStatus(),
                // 其他业务领域的状态查询...
                _ => new WebResponseContent().Error($"不支持的业务领域：{businessDomain}")
            };
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 生成同步结果
        /// </summary>
        private WebResponseContent GenerateSyncResult(WebResponseContent response, List<string> results, List<string> errors, string operationType)
        {
            if (errors.Count == 0)
            {
                var successMessage = $"{operationType}数据同步全部成功！详情：{string.Join("；", results)}";
                _logger.LogInformation(successMessage);
                return response.OK(successMessage);
            }
            else if (results.Count > 0)
            {
                var partialMessage = $"{operationType}数据部分同步成功。成功：{string.Join("；", results)}。失败：{string.Join("；", errors)}";
                _logger.LogWarning(partialMessage);
                return response.Error(partialMessage);
            }
            else
            {
                var failMessage = $"{operationType}数据同步全部失败。失败：{string.Join("；", errors)}";
                _logger.LogError(failMessage);
                return response.Error(failMessage);
            }
        }

        #endregion
    }
} 