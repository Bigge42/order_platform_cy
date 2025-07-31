using Microsoft.AspNetCore.Mvc;
using HDPro.Entity.AttributeManager;
using HDPro.Entity.SystemModels;
using HDPro.Core.Utilities;
using HDPro.Core.Filters;
using HDPro.CY.Order.Services.K3Cloud;
using HDPro.CY.Order.IServices;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Core.Controllers.Basic;

namespace HDPro.CY.Order.Controllers
{
    /// <summary>
    /// K3Cloud数据同步控制器
    /// 管理K3Cloud基础数据的同步任务
    /// </summary>
    [Route("api/K3CloudSync")]
    [ApiController]
    [PermissionTable(Name = "K3CloudSync")]
    public class K3CloudSyncController : VolController
    {
        private readonly IK3CloudService _k3CloudService;
        private readonly IOCP_MaterialService _materialService;
        private readonly IOCP_SupplierService _supplierService;
        private readonly IOCP_CustomerService _customerService;
        private readonly ILogger<K3CloudSyncController> _logger;

        public K3CloudSyncController(
            IOCP_MaterialService materialService,
            IOCP_SupplierService supplierService,
            IOCP_CustomerService customerService,
            IK3CloudService k3CloudService,
            ILogger<K3CloudSyncController> logger)
        {
            _materialService = materialService;
            _supplierService = supplierService;
            _customerService = customerService;
            _k3CloudService = k3CloudService;
            _logger = logger;
        }

        #region 定时任务接口

        /// <summary>
        /// 定时任务：物料数据同步
        /// </summary>
        /// <param name="request">同步请求参数，支持时间范围</param>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/MaterialSync")]
        public async Task<IActionResult> MaterialSyncTask([FromBody] MaterialSyncRequest request = null)
        {
            try
            {
                _logger.LogInformation($"定时任务：开始K3Cloud物料数据同步，参数：{request?.StartDate} - {request?.EndDate}");

                WebResponseContent result;
                if (request != null && (!string.IsNullOrEmpty(request.StartDate) || !string.IsNullOrEmpty(request.EndDate)))
                {
                    // 使用时间范围同步
                    result = await _materialService.SyncMaterialsFromK3CloudByDateRangeAsync(request.StartDate, request.EndDate, request.PageSize);
                }
                else
                {
                    // 使用增量同步
                    result = await _materialService.SyncMaterialsFromK3CloudAsync(request?.PageSize ?? 1000);
                }

                _logger.LogInformation($"定时任务：K3Cloud物料数据同步完成，结果：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：K3Cloud物料数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：供应商数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/SupplierSync")]
        public async Task<IActionResult> SupplierSyncTask()
        {
            try
            {
                _logger.LogInformation("定时任务：开始K3Cloud供应商数据同步");
                
                var result = await _supplierService.SyncSuppliersFromK3CloudAsync();
                
                _logger.LogInformation($"定时任务：K3Cloud供应商数据同步完成，结果：{result.Status}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：K3Cloud供应商数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：客户数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/CustomerSync")]
        public async Task<IActionResult> CustomerSyncTask()
        {
            try
            {
                _logger.LogInformation("定时任务：开始K3Cloud客户数据同步");
                
                var result = await _customerService.SyncCustomersFromK3CloudAsync();
                
                _logger.LogInformation($"定时任务：K3Cloud客户数据同步完成，结果：{result.Status}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：K3Cloud客户数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：全部基础数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/AllBasicDataSync")]
        public async Task<IActionResult> AllBasicDataSyncTask()
        {
            try
            {
                _logger.LogInformation("定时任务：开始K3Cloud全部基础数据同步");
                
                var results = new
                {
                    MaterialSync = await _materialService.SyncMaterialsFromK3CloudAsync(),
                    SupplierSync = await _supplierService.SyncSuppliersFromK3CloudAsync(),
                    CustomerSync = await _customerService.SyncCustomersFromK3CloudAsync()
                };

                var allSuccess = results.MaterialSync.Status && 
                                results.SupplierSync.Status && 
                                results.CustomerSync.Status;

                var message = $"物料同步：{(results.MaterialSync.Status ? "成功" : "失败")}；" +
                             $"供应商同步：{(results.SupplierSync.Status ? "成功" : "失败")}；" +
                             $"客户同步：{(results.CustomerSync.Status ? "成功" : "失败")}";

                _logger.LogInformation($"定时任务：K3Cloud全部基础数据同步完成，结果：{message}");
                
                return Json(new WebResponseContent(allSuccess)
                {
                    Data = results,
                    Message = message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：K3Cloud全部基础数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        #endregion

        #region 手动同步接口

        /// <summary>
        /// 手动同步物料数据
        /// </summary>
        /// <returns>同步结果</returns>
        [HttpPost("ManualSyncMaterial")]
        public async Task<IActionResult> ManualSyncMaterial()
        {
            try
            {
                _logger.LogInformation("手动同步：开始K3Cloud物料数据同步");
                
                var result = await _materialService.SyncMaterialsFromK3CloudAsync();
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手动同步K3Cloud物料数据发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 手动同步供应商数据
        /// </summary>
        /// <returns>同步结果</returns>
        [HttpPost("ManualSyncSupplier")]
        public async Task<IActionResult> ManualSyncSupplier()
        {
            try
            {
                _logger.LogInformation("手动同步：开始K3Cloud供应商数据同步");
                
                var result = await _supplierService.SyncSuppliersFromK3CloudAsync();
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手动同步K3Cloud供应商数据发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 手动同步客户数据
        /// </summary>
        /// <returns>同步结果</returns>
        [HttpPost("ManualSyncCustomer")]
        public async Task<IActionResult> ManualSyncCustomer()
        {
            try
            {
                _logger.LogInformation("手动同步：开始K3Cloud客户数据同步");
                
                var result = await _customerService.SyncCustomersFromK3CloudAsync();
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手动同步K3Cloud客户数据发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 手动同步全部基础数据
        /// </summary>
        /// <returns>同步结果</returns>
        [HttpPost("ManualSyncAllBasicData")]
        public async Task<IActionResult> ManualSyncAllBasicData()
        {
            try
            {
                _logger.LogInformation("手动同步：开始K3Cloud全部基础数据同步");
                
                var results = new
                {
                    MaterialSync = await _materialService.SyncMaterialsFromK3CloudAsync(),
                    SupplierSync = await _supplierService.SyncSuppliersFromK3CloudAsync(),
                    CustomerSync = await _customerService.SyncCustomersFromK3CloudAsync()
                };

                var allSuccess = results.MaterialSync.Status && 
                                results.SupplierSync.Status && 
                                results.CustomerSync.Status;

                var message = $"物料同步：{(results.MaterialSync.Status ? "成功" : "失败")}；" +
                             $"供应商同步：{(results.SupplierSync.Status ? "成功" : "失败")}；" +
                             $"客户同步：{(results.CustomerSync.Status ? "成功" : "失败")}";

                return Json(new WebResponseContent(allSuccess)
                {
                    Data = results,
                    Message = message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手动同步K3Cloud全部基础数据发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        #endregion

        #region 状态查询接口

        /// <summary>
        /// 获取K3Cloud连接状态
        /// </summary>
        /// <returns>连接状态</returns>
        [HttpGet("GetConnectionStatus")]
        public async Task<IActionResult> GetConnectionStatus()
        {
            try
            {
                var loginResult = await _k3CloudService.LoginAsync();
                
                var status = new
                {
                    IsConnected = loginResult.IsSuccess,
                    Message = loginResult.Message,
                    SessionId = loginResult.KDSVCSessionId,
                    CheckTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return Json(new WebResponseContent(loginResult.IsSuccess)
                {
                    Data = status,
                    Message = loginResult.IsSuccess ? "K3Cloud连接正常" : $"K3Cloud连接异常：{loginResult.Message}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查K3Cloud连接状态发生异常");
                return Json(new WebResponseContent().Error($"检查连接状态异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取K3Cloud基础数据统计
        /// </summary>
        /// <returns>数据统计</returns>
        [HttpGet("GetBasicDataStatistics")]
        public async Task<IActionResult> GetBasicDataStatistics()
        {
            try
            {
                var statistics = new
                {
                    MaterialCount = await _k3CloudService.GetMaterialCountAsync(),
                    SupplierCount = await _k3CloudService.GetSupplierCountAsync(),
                    CustomerCount = await _k3CloudService.GetCustomerCountAsync(),
                    StatisticsTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return Json(new WebResponseContent(true)
                {
                    Data = statistics,
                    Message = "获取K3Cloud基础数据统计成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取K3Cloud基础数据统计发生异常");
                return Json(new WebResponseContent().Error($"获取统计数据异常：{ex.Message}"));
            }
        }

        #endregion

        /// <summary>
        /// 测试依赖注入配置
        /// </summary>
        /// <returns>测试结果</returns>
        [HttpGet("TestDependencyInjection")]
        public IActionResult TestDependencyInjection()
        {
            try
            {
                var testResults = new System.Text.StringBuilder();
                
                testResults.AppendLine($"K3Cloud服务状态：{(_k3CloudService != null ? "✓ 正常" : "✗ 未注入")}");
                testResults.AppendLine($"物料服务状态：{(_materialService != null ? "✓ 正常" : "✗ 未注入")}");
                testResults.AppendLine($"供应商服务状态：{(_supplierService != null ? "✓ 正常" : "✗ 未注入")}");
                testResults.AppendLine($"客户服务状态：{(_customerService != null ? "✓ 正常" : "✗ 未注入")}");
                testResults.AppendLine($"日志器状态：{(_logger != null ? "✓ 正常" : "✗ 未注入")}");
                
                testResults.AppendLine("\n支持的定时任务：");
                testResults.AppendLine("1. MaterialSync - 物料数据同步");
                testResults.AppendLine("2. SupplierSync - 供应商数据同步");
                testResults.AppendLine("3. CustomerSync - 客户数据同步");
                testResults.AppendLine("4. AllBasicDataSync - 全部基础数据同步");
                
                var response = new WebResponseContent();
                return Json(response.OK("K3Cloud同步服务依赖注入测试完成", new { 
                    TestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Results = testResults.ToString()
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "依赖注入测试发生异常");
                return Json(new WebResponseContent().Error($"测试异常：{ex.Message}"));
            }
        }
    }

    /// <summary>
    /// 物料同步请求参数
    /// </summary>
    public class MaterialSyncRequest
    {
        /// <summary>
        /// 开始日期 (yyyy-MM-dd)
        /// </summary>
        public string? StartDate { get; set; }

        /// <summary>
        /// 结束日期 (yyyy-MM-dd)
        /// </summary>
        public string? EndDate { get; set; }

        /// <summary>
        /// 每页数量，默认1000
        /// </summary>
        public int PageSize { get; set; } = 1000;
    }
}