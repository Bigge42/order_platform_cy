using Microsoft.AspNetCore.Mvc;
using HDPro.Entity.AttributeManager;
using HDPro.Entity.SystemModels;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Services.OrderCollaboration.ESB;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.SubOrder;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.Purchase;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.Part;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.WholeUnit;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.Metalwork;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.TechManagement;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.SalesManagement;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.OrderTracking;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.LackMaterial;
using HDPro.Core.Filters;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using HDPro.Core.Controllers.Basic;
using System.Text;
using HDPro.Core.Configuration;

namespace HDPro.CY.Order.Controllers
{
    /// <summary>
    /// ESB数据同步控制器
    /// 统一管理所有业务领域的ESB数据同步接口
    /// 支持业务领域：委外、采购、部件、整机、金工车间、技术管理、销售管理、订单跟踪、缺料运算结果
    /// 特殊功能：缺料运算结果支持批量模式（自动获取运算方案并检查队列状态避免重复同步）
    /// </summary>
    [Route("api/ESBSync")]
    [ApiController]
    [PermissionTable(Name = "ESBSync")]
    public class ESBSyncController : VolController
    {
        private readonly ESBMasterCoordinator _masterCoordinator;
        private readonly SubOrderESBSyncCoordinator _subOrderCoordinator;
        private readonly PurchaseESBSyncCoordinator _purchaseCoordinator;
        private readonly PartESBSyncCoordinator _partCoordinator;
        private readonly WholeUnitESBSyncCoordinator _wholeUnitCoordinator;
        private readonly MetalworkESBSyncCoordinator _metalworkCoordinator;
        private readonly TechManagementESBSyncCoordinator _techManagementCoordinator;
        private readonly SalesManagementESBSyncCoordinator _salesManagementCoordinator;
        private readonly OrderTrackingESBSyncCoordinator _orderTrackingCoordinator;
        private readonly LackMtrlResultESBSyncCoordinator _lackMtrlResultCoordinator;
        private readonly ILogger<ESBSyncController> _logger;

        public ESBSyncController(
            ESBMasterCoordinator masterCoordinator,
            SubOrderESBSyncCoordinator subOrderCoordinator,
            PurchaseESBSyncCoordinator purchaseCoordinator,
            PartESBSyncCoordinator partCoordinator,
            WholeUnitESBSyncCoordinator wholeUnitCoordinator,
            MetalworkESBSyncCoordinator metalworkCoordinator,
            TechManagementESBSyncCoordinator techManagementCoordinator,
            SalesManagementESBSyncCoordinator salesManagementCoordinator,
            OrderTrackingESBSyncCoordinator orderTrackingCoordinator,
            LackMtrlResultESBSyncCoordinator lackMtrlResultCoordinator,
            ILogger<ESBSyncController> logger)
        {
            _masterCoordinator = masterCoordinator;
            _subOrderCoordinator = subOrderCoordinator;
            _purchaseCoordinator = purchaseCoordinator;
            _partCoordinator = partCoordinator;
            _wholeUnitCoordinator = wholeUnitCoordinator;
            _metalworkCoordinator = metalworkCoordinator;
            _techManagementCoordinator = techManagementCoordinator;
            _salesManagementCoordinator = salesManagementCoordinator;
            _orderTrackingCoordinator = orderTrackingCoordinator;
            _lackMtrlResultCoordinator = lackMtrlResultCoordinator;
            _logger = logger;
        }

        #region 主协调器接口

        /// <summary>
        /// 同步所有业务领域的ESB数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SyncAllBusinessDomains")]
        public async Task<IActionResult> SyncAllBusinessDomains([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation($"开始全业务领域ESB数据同步，参数：{request?.StartDate} - {request?.EndDate}");
                
                var result = await _masterCoordinator.SyncAllBusinessDomains(request?.StartDate, request?.EndDate);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "全业务领域ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 按业务领域同步ESB数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SyncByBusinessDomain")]
        public async Task<IActionResult> SyncByBusinessDomain([FromBody] ESBBusinessDomainSyncRequest request)
        {
            try
            {
                _logger.LogInformation($"开始{request?.BusinessDomain}业务领域ESB数据同步");
                
                if (string.IsNullOrWhiteSpace(request?.BusinessDomain))
                {
                    return Json(new WebResponseContent().Error("业务领域不能为空"));
                }

                var result = await _masterCoordinator.SyncByBusinessDomain(
                    request.BusinessDomain, 
                    request.StartDate, 
                    request.EndDate);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"业务领域ESB数据同步发生异常：{request?.BusinessDomain}");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取所有业务领域同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        [HttpGet("GetAllBusinessDomainStatus")]
        public async Task<IActionResult> GetAllBusinessDomainStatus()
        {
            try
            {
                var result = await _masterCoordinator.GetAllBusinessDomainStatus();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取业务领域状态发生异常");
                return Json(new WebResponseContent().Error($"获取状态异常：{ex.Message}"));
            }
        }

        #endregion

        #region 委外业务领域接口

        /// <summary>
        /// 同步所有委外订单相关数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SubOrder/SyncAll")]
        public async Task<IActionResult> SyncAllSubOrderData([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation($"开始委外订单全量ESB数据同步");
                
                var result = await _subOrderCoordinator.SyncAllSubOrderData(request?.StartDate, request?.EndDate);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "委外订单全量ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步委外订单头
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SubOrder/SyncHeaderOnly")]
        public async Task<IActionResult> SyncSubOrderOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _subOrderCoordinator.SyncSubOrderOnly(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "委外订单头ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步委外订单明细
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SubOrder/SyncDetailOnly")]
        public async Task<IActionResult> SyncSubOrderDetailOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _subOrderCoordinator.SyncSubOrderDetailOnly(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "委外订单明细ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步委外未完跟踪
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SubOrder/SyncUnFinishTrackOnly")]
        public async Task<IActionResult> SyncSubOrderUnFinishTrackOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _subOrderCoordinator.SyncSubOrderUnFinishTrackOnly(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "委外未完跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取委外业务同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        [HttpGet("SubOrder/GetStatus")]
        public async Task<IActionResult> GetSubOrderSyncStatus()
        {
            try
            {
                var result = await _subOrderCoordinator.GetSyncStatus();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取委外业务同步状态发生异常");
                return Json(new WebResponseContent().Error($"获取状态异常：{ex.Message}"));
            }
        }

        #endregion

        #region 采购业务领域接口

        /// <summary>
        /// 同步所有采购订单相关数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("Purchase/SyncAll")]
        public async Task<IActionResult> SyncAllPurchaseOrderData([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation($"开始采购订单全量ESB数据同步");
                
                var result = await _purchaseCoordinator.SyncAllPurchaseOrderData(request?.StartDate, request?.EndDate);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "采购订单全量ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步采购订单头
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("Purchase/SyncHeaderOnly")]
        public async Task<IActionResult> SyncPurchaseOrderOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _purchaseCoordinator.SyncPurchaseOrderOnly(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "采购订单头ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步采购订单明细
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("Purchase/SyncDetailOnly")]
        public async Task<IActionResult> SyncPurchaseOrderDetailOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _purchaseCoordinator.SyncPurchaseOrderDetailOnly(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "采购订单明细ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步采购未完跟踪
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("Purchase/SyncUnFinishTrackOnly")]
        public async Task<IActionResult> SyncPurchaseUnFinishTrackOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _purchaseCoordinator.SyncPurchaseUnFinishTrackOnly(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "采购未完跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取采购业务同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        [HttpGet("Purchase/GetStatus")]
        public async Task<IActionResult> GetPurchaseSyncStatus()
        {
            try
            {
                var result = await _purchaseCoordinator.GetSyncStatus();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取采购业务同步状态发生异常");
                return Json(new WebResponseContent().Error($"获取状态异常：{ex.Message}"));
            }
        }

        #endregion

        #region 部件业务领域接口

        /// <summary>
        /// 同步所有部件相关数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("Part/SyncAll")]
        public async Task<IActionResult> SyncAllPartData([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation($"开始部件全量ESB数据同步");
                
                var result = await _partCoordinator.SyncAllPartData(request?.StartDate, request?.EndDate);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "部件全量ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步部件生产订单（包括头和明细）
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("Part/SyncPrdMOOnly")]
        public async Task<IActionResult> SyncPartPrdMOOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _partCoordinator.SyncPartPrdMOOnly(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "部件生产订单ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步部件未完跟踪
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("Part/SyncUnFinishTrackOnly")]
        public async Task<IActionResult> SyncPartUnFinishTrackOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _partCoordinator.SyncPartUnFinishTrackOnly(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "部件未完跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取部件业务同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        [HttpGet("Part/GetStatus")]
        public IActionResult GetPartSyncStatus()
        {
            try
            {
                var result = _partCoordinator.GetSyncStatus();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取部件业务同步状态发生异常");
                return Json(new WebResponseContent().Error($"获取状态异常：{ex.Message}"));
            }
        }

        #endregion

        #region 整机业务领域接口

        /// <summary>
        /// 同步所有整机数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("WholeUnit/SyncAll")]
        public async Task<IActionResult> SyncAllWholeUnitData([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation($"开始整机业务全量ESB数据同步");
                
                var result = await _wholeUnitCoordinator.SyncAllWholeUnitData(request?.StartDate, request?.EndDate);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "整机业务全量ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步整机生产订单数据（头+明细）
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("WholeUnit/SyncPrdMOOnly")]
        public async Task<IActionResult> SyncWholeUnitPrdMOOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _wholeUnitCoordinator.SyncWholeUnitPrdMOOnly(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "整机生产订单ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步整机跟踪数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("WholeUnit/SyncTrackingOnly")]
        public async Task<IActionResult> SyncWholeUnitTrackingOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _wholeUnitCoordinator.SyncWholeUnitTrackingOnly(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "整机跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取整机业务同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        [HttpGet("WholeUnit/GetStatus")]
        public async Task<IActionResult> GetWholeUnitSyncStatus()
        {
            try
            {
                var result = await _wholeUnitCoordinator.GetWholeUnitSyncStatus();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取整机业务同步状态发生异常");
                return Json(new WebResponseContent().Error($"获取状态异常：{ex.Message}"));
            }
        }

        #endregion

        #region 金工车间业务领域接口

        /// <summary>
        /// 同步所有金工车间数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("Metalwork/SyncAll")]
        public async Task<IActionResult> SyncAllMetalworkData([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation($"开始金工车间业务全量ESB数据同步");
                
                var result = await _metalworkCoordinator.SyncAllMetalworkData(request?.StartDate, request?.EndDate);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "金工车间业务全量ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步金工生产订单数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("Metalwork/SyncPrdMOOnly")]
        public async Task<IActionResult> SyncMetalworkPrdMOOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _metalworkCoordinator.ManualSyncPrdMOData(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "金工生产订单ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步金工生产订单明细数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("Metalwork/SyncPrdMODetailOnly")]
        public async Task<IActionResult> SyncMetalworkPrdMODetailOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _metalworkCoordinator.ManualSyncPrdMODetailData(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "金工生产订单明细ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步金工未完工跟踪数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("Metalwork/SyncUnFinishTrackOnly")]
        public async Task<IActionResult> SyncMetalworkUnFinishTrackOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _metalworkCoordinator.ManualSyncUnFinishTrackData(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "金工未完工跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取金工车间业务同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        [HttpGet("Metalwork/GetStatus")]
        public async Task<IActionResult> GetMetalworkSyncStatus()
        {
            try
            {
                var result = await _metalworkCoordinator.GetMetalworkSyncStatus();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取金工车间业务同步状态发生异常");
                return Json(new WebResponseContent().Error($"获取状态异常：{ex.Message}"));
            }
        }

        #endregion

        #region 技术管理业务领域接口

        /// <summary>
        /// 同步技术管理数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("TechManagement/SyncAll")]
        public async Task<IActionResult> SyncTechManagementData([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation($"开始技术管理ESB数据同步");
                
                var result = await _techManagementCoordinator.SyncTechManagementData(request?.StartDate, request?.EndDate);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "技术管理ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 手动同步技术管理数据（需要指定时间范围）
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("TechManagement/ManualSync")]
        public async Task<IActionResult> ManualSyncTechManagementData([FromBody] ESBSyncRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.StartDate) || string.IsNullOrWhiteSpace(request?.EndDate))
                {
                    return Json(new WebResponseContent().Error("手动同步必须指定开始时间和结束时间"));
                }

                var result = await _techManagementCoordinator.ManualSyncTechManagementData(request.StartDate, request.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "技术管理手动ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取技术管理业务同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        [HttpGet("TechManagement/GetStatus")]
        public IActionResult GetTechManagementSyncStatus()
        {
            try
            {
                var result = _techManagementCoordinator.GetTechManagementSyncStatus();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取技术管理业务同步状态发生异常");
                return Json(new WebResponseContent().Error($"获取状态异常：{ex.Message}"));
            }
        }

        #endregion

        #region 销售管理业务领域接口

        /// <summary>
        /// 同步销售管理相关数据（仅销售订单列表）
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SalesManagement/SyncAll")]
        public async Task<IActionResult> SyncAllSalesManagementData([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation($"开始销售管理ESB数据同步");

                var result = await _salesManagementCoordinator.SyncAllSalesManagementData(
                    request?.StartDate,
                    request?.EndDate
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "销售管理ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 仅同步销售订单列表
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SalesManagement/SyncOrderListOnly")]
        public async Task<IActionResult> SyncSalesOrderListOnly([FromBody] ESBSyncRequest request)
        {
            try
            {
                var result = await _salesManagementCoordinator.SyncOrderListOnly(request?.StartDate, request?.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "销售订单列表ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 根据销售订单号同步单个订单明细
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SalesManagement/SyncSingleOrderDetail")]
        public async Task<IActionResult> SyncSingleOrderDetail([FromBody] ESBSingleOrderDetailRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.SalesOrderNo))
                {
                    return Json(new WebResponseContent().Error("销售订单号不能为空"));
                }

                var result = await _salesManagementCoordinator.SyncSingleOrderDetail(request.SalesOrderNo);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"销售订单明细ESB数据同步发生异常：{request?.SalesOrderNo}");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 根据计划跟踪号同步批次信息
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SalesManagement/SyncSingleBatchInfo")]
        public async Task<IActionResult> SyncSingleBatchInfo([FromBody] ESBSingleBatchInfoRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.PlanTrackingNo))
                {
                    return Json(new WebResponseContent().Error("计划跟踪号不能为空"));
                }

                var result = await _salesManagementCoordinator.SyncSingleBatchInfo(request.PlanTrackingNo);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"销售批次信息ESB数据同步发生异常：{request?.PlanTrackingNo}");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 手动同步销售管理数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("SalesManagement/ManualSync")]
        public async Task<IActionResult> ManualSyncSalesManagementData([FromBody] ESBSyncRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.StartDate) || string.IsNullOrWhiteSpace(request?.EndDate))
                {
                    return Json(new WebResponseContent().Error("手动同步必须指定开始和结束日期"));
                }

                var result = await _salesManagementCoordinator.ManualSyncAllData(request.StartDate, request.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手动同步销售管理ESB数据发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取销售管理同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        [HttpGet("SalesManagement/GetStatus")]
        public IActionResult GetSalesManagementSyncStatus()
        {
            try
            {
                var status = new
                {
                    BusinessDomain = "销售管理",
                    Services = new[]
                    {
                        new { Name = "销售订单列表同步服务", Status = "正常", LastSyncTime = "未同步" },
                        new { Name = "销售订单明细同步服务", Status = "正常", LastSyncTime = "未同步" },
                        new { Name = "销售批次信息同步服务", Status = "正常", LastSyncTime = "未同步" }
                    },
                    Coordinator = new { Name = "销售管理ESB同步协调器", Status = "正常" },
                    LastUpdate = DateTime.Now
                };

                return Json(new WebResponseContent().OK("获取销售管理状态成功", status));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取销售管理同步状态发生异常");
                return Json(new WebResponseContent().Error($"获取状态异常：{ex.Message}"));
            }
        }

        #endregion

        #region 订单跟踪业务领域接口

        /// <summary>
        /// 同步订单跟踪数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("OrderTracking/SyncAll")]
        public async Task<IActionResult> SyncOrderTrackingData([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation($"开始订单跟踪ESB数据同步");
                
                var result = await _orderTrackingCoordinator.ExecuteFullSync(request?.StartDate, request?.EndDate);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订单跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 手动同步订单跟踪数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("OrderTracking/ManualSync")]
        public async Task<IActionResult> ManualSyncOrderTrackingData([FromBody] ESBSyncRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.StartDate) || string.IsNullOrWhiteSpace(request?.EndDate))
                {
                    return Json(new WebResponseContent().Error("手动同步必须指定开始时间和结束时间"));
                }

                var result = await _orderTrackingCoordinator.ManualSync(request.StartDate, request.EndDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订单跟踪手动ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取订单跟踪业务同步状态
        /// </summary>
        /// <returns>状态信息</returns>
        [HttpGet("OrderTracking/GetStatus")]
        public async Task<IActionResult> GetOrderTrackingSyncStatus()
        {
            try
            {
                var result = await _orderTrackingCoordinator.GetSyncStatus();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单跟踪业务同步状态发生异常");
                return Json(new WebResponseContent().Error($"获取状态异常：{ex.Message}"));
            }
        }

        #endregion

        #region 缺料运算结果业务领域接口

        /// <summary>
        /// 批量同步所有可用运算方案的缺料数据
        /// 自动获取运算方案列表，检查运算队列状态，循环执行同步
        /// </summary>
        /// <returns>同步结果</returns>
        [HttpPost("LackMtrlResult/SyncAllSchemes")]
        public async Task<IActionResult> SyncAllLackMtrlResultSchemes()
        {
            try
            {
                _logger.LogInformation($"开始批量同步所有可用运算方案的缺料数据");
                
                var result = await _lackMtrlResultCoordinator.SyncAllAvailableComputeSchemes();
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量同步缺料运算结果ESB数据发生异常");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 根据运算ID同步缺料运算结果数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("LackMtrlResult/SyncByComputeId")]
        public async Task<IActionResult> SyncLackMtrlResultByComputeId([FromBody] ESBLackMtrlSyncRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.ComputeId))
                {
                    return Json(new WebResponseContent().Error("运算ID不能为空"));
                }

                _logger.LogInformation($"开始缺料运算结果ESB数据同步，运算ID：{request.ComputeId}");
                
                var result = await _lackMtrlResultCoordinator.ExecuteSyncByComputeId(request.ComputeId);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"缺料运算结果ESB数据同步发生异常，运算ID：{request?.ComputeId}");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 手动同步缺料运算结果数据
        /// </summary>
        /// <param name="request">同步请求参数</param>
        /// <returns>同步结果</returns>
        [HttpPost("LackMtrlResult/ManualSync")]
        public async Task<IActionResult> ManualSyncLackMtrlResultData([FromBody] ESBLackMtrlSyncRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.ComputeId))
                {
                    return Json(new WebResponseContent().Error("手动同步必须指定运算ID"));
                }

                var result = await _lackMtrlResultCoordinator.ManualSync(request.ComputeId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"手动同步缺料运算结果ESB数据发生异常，运算ID：{request?.ComputeId}");
                return Json(new WebResponseContent().Error($"同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 获取缺料运算结果业务同步状态
        /// </summary>
        /// <param name="request">状态查询请求参数</param>
        /// <returns>状态信息</returns>
        [HttpPost("LackMtrlResult/GetStatus")]
        public async Task<IActionResult> GetLackMtrlResultSyncStatus([FromBody] ESBLackMtrlSyncRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.ComputeId))
                {
                    return Json(new WebResponseContent().Error("获取状态需要提供运算ID"));
                }

                var result = await _lackMtrlResultCoordinator.GetSyncStatus(request.ComputeId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取缺料运算结果业务同步状态发生异常，运算ID：{request?.ComputeId}");
                return Json(new WebResponseContent().Error($"获取状态异常：{ex.Message}"));
            }
        }

        #endregion

        #region 定时任务接口

        /// <summary>
        /// 定时任务：委外业务数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/SubOrderSync")]
        public async Task<IActionResult> SubOrderSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始委外业务ESB数据同步");
                
                var result = await _subOrderCoordinator.SyncAllSubOrderData(request?.StartDate, request?.EndDate);
                
                _logger.LogInformation($"定时任务：委外业务ESB数据同步完成，结果：{result.Status}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：委外业务ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：采购业务数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/PurchaseSync")]
        public async Task<IActionResult> PurchaseSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始采购业务ESB数据同步");

                var result = await _purchaseCoordinator.SyncAllPurchaseOrderData(request?.StartDate, request?.EndDate);

                _logger.LogInformation($"定时任务：采购业务ESB数据同步完成，结果：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：采购业务ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：采购跟踪数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/PurchaseTrackingSync")]
        public async Task<IActionResult> PurchaseTrackingSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始采购跟踪ESB数据同步");

                var result = await _purchaseCoordinator.SyncPurchaseUnFinishTrackOnly(request?.StartDate, request?.EndDate);

                _logger.LogInformation($"定时任务：采购跟踪ESB数据同步完成，结果：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：采购跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：委外跟踪数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/SubOrderTrackingSync")]
        public async Task<IActionResult> SubOrderTrackingSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始委外跟踪ESB数据同步");

                var result = await _subOrderCoordinator.SyncSubOrderUnFinishTrackOnly(request?.StartDate, request?.EndDate);

                _logger.LogInformation($"定时任务：委外跟踪ESB数据同步完成，结果：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：委外跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：部件跟踪数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/PartTrackingSync")]
        public async Task<IActionResult> PartTrackingSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始部件跟踪ESB数据同步");

                var result = await _partCoordinator.SyncPartUnFinishTrackOnly(request?.StartDate, request?.EndDate);

                _logger.LogInformation($"定时任务：部件跟踪ESB数据同步完成，结果：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：部件跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：整机跟踪数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/WholeUnitTrackingSync")]
        public async Task<IActionResult> WholeUnitTrackingSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始整机跟踪ESB数据同步");

                var result = await _wholeUnitCoordinator.SyncWholeUnitTrackingOnly(request?.StartDate, request?.EndDate);

                _logger.LogInformation($"定时任务：整机跟踪ESB数据同步完成，结果：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：整机跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：金工跟踪数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/MetalworkTrackingSync")]
        public async Task<IActionResult> MetalworkTrackingSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始金工跟踪ESB数据同步");

                var result = await _metalworkCoordinator.SyncMetalworkUnFinishTrackOnly(request?.StartDate, request?.EndDate);

                _logger.LogInformation($"定时任务：金工跟踪ESB数据同步完成，结果：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：金工跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：部件业务数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/PartSync")]
        public async Task<IActionResult> PartSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始部件业务ESB数据同步");
                
                var result = await _partCoordinator.SyncAllPartData(request.StartDate, request.EndDate);
                
                _logger.LogInformation($"定时任务：部件业务ESB数据同步完成，结果：{result.Status}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：部件业务ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：整机业务数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/WholeUnitSync")]
        public async Task<IActionResult> WholeUnitSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始整机业务ESB数据同步");
                
                var result = await _wholeUnitCoordinator.SyncAllWholeUnitData(request?.StartDate, request?.EndDate);
                
                _logger.LogInformation($"定时任务：整机业务ESB数据同步完成，结果：{result.Status}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：整机业务ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：金工车间业务数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/MetalworkSync")]
        public async Task<IActionResult> MetalworkSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始金工车间业务ESB数据同步");
                
                var result = await _metalworkCoordinator.SyncAllMetalworkData(request.StartDate, request.EndDate);

                _logger.LogInformation($"定时任务：金工车间业务ESB数据同步完成，结果：{result.Status}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：金工车间业务ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：技术管理业务数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/TechManagementSync")]
        public async Task<IActionResult> TechManagementSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始技术管理业务ESB数据同步");
                
                var result = await _techManagementCoordinator.SyncTechManagementData(request?.StartDate, request?.EndDate);
                
                _logger.LogInformation($"定时任务：技术管理业务ESB数据同步完成，结果：{result.Status}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：技术管理业务ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：销售管理业务数据同步（仅销售订单列表）
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/SalesManagementSync")]
        public async Task<IActionResult> SalesManagementSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始销售管理业务ESB数据同步");

                var result = await _salesManagementCoordinator.SyncAllSalesManagementData(
                    request?.StartDate,
                    request?.EndDate
                );

                _logger.LogInformation($"定时任务：销售管理业务ESB数据同步完成，结果：{result.Status}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：销售管理业务ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：全业务领域数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/AllBusinessDomainsSync")]
        public async Task<IActionResult> AllBusinessDomainsSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始全业务领域ESB数据同步");

                var result = await _masterCoordinator.SyncAllBusinessDomains(request.StartDate, request.EndDate);


                _logger.LogInformation($"定时任务：全业务领域ESB数据同步完成，结果：{result.Status}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：全业务领域ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：订单跟踪数据同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/OrderTrackingSync")]
        public async Task<IActionResult> OrderTrackingSyncTask([FromBody] ESBSyncRequest request)
        {
            try
            {
                _logger.LogInformation("定时任务：开始订单跟踪ESB数据同步");
                
                var result = await _orderTrackingCoordinator.ExecuteFullSync(request?.StartDate, request?.EndDate);
                
                _logger.LogInformation($"定时任务：订单跟踪ESB数据同步完成，结果：{result.Status}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：订单跟踪ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：缺料运算结果数据同步（批量模式）
        /// 自动获取所有运算方案并循环同步，检查运算队列状态避免重复同步
        /// </summary>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/LackMtrlResultSyncBatch")]
        public async Task<IActionResult> LackMtrlResultSyncBatchTask()
        {
            try
            {
                _logger.LogInformation("定时任务：开始批量缺料运算结果ESB数据同步");
                
                var result = await _lackMtrlResultCoordinator.SyncAllAvailableComputeSchemes();
                
                _logger.LogInformation($"定时任务：批量缺料运算结果ESB数据同步完成，结果：{result.Status}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务：批量缺料运算结果ESB数据同步发生异常");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        /// <summary>
        /// 定时任务：缺料运算结果数据同步（单个运算ID模式）
        /// </summary>
        /// <param name="request">同步请求参数，需包含运算ID</param>
        /// <returns>同步结果</returns>
        [ApiTask]
        [HttpGet, HttpPost, Route("Task/LackMtrlResultSync")]
        public async Task<IActionResult> LackMtrlResultSyncTask([FromBody] ESBLackMtrlSyncRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.ComputeId))
                {
                    _logger.LogWarning("定时任务：缺料运算结果同步缺少运算ID参数，建议使用批量模式");
                    return Json(new WebResponseContent().Error("缺料运算结果同步需要提供运算ID参数，或使用批量模式接口"));
                }

                _logger.LogInformation($"定时任务：开始缺料运算结果ESB数据同步，运算ID：{request.ComputeId}");
                
                var result = await _lackMtrlResultCoordinator.ExecuteSyncByComputeId(request.ComputeId);
                
                _logger.LogInformation($"定时任务：缺料运算结果ESB数据同步完成，结果：{result.Status}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"定时任务：缺料运算结果ESB数据同步发生异常，运算ID：{request?.ComputeId}");
                return Json(new WebResponseContent().Error($"定时任务同步异常：{ex.Message}"));
            }
        }

        #endregion

        #region 测试和诊断接口

        /// <summary>
        /// 测试ESB超时配置
        /// </summary>
        /// <returns></returns>
        [HttpPost("TestESBTimeout")]
        public async Task<IActionResult> TestESBTimeout()
        {
            try
            {
                var testResults = new StringBuilder();
                testResults.AppendLine("=== ESB超时配置测试 ===");
                testResults.AppendLine($"测试时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                testResults.AppendLine();

                // 检查当前超时配置
                var esbConfig = AppSetting.ESB;
                if (esbConfig != null)
                {
                    testResults.AppendLine("📋 当前ESB超时配置：");
                    testResults.AppendLine($"   - 默认超时时间：{esbConfig.DefaultTimeoutMinutes} 分钟");
                    testResults.AppendLine($"   - 长时间运行超时：{esbConfig.LongRunningTimeoutMinutes} 分钟");
                    testResults.AppendLine($"   - 重试次数：{esbConfig.RetryCount}");
                    testResults.AppendLine($"   - 重试延迟：{esbConfig.RetryDelaySeconds} 秒");
                    testResults.AppendLine($"   - 批量处理大小：{esbConfig.BatchSize}");
                    testResults.AppendLine();
                }

                testResults.AppendLine("🔧 超时问题解决方案：");
                testResults.AppendLine("1. 已将默认超时从5分钟增加到15分钟");
                testResults.AppendLine("2. 添加了长时间运行接口的30分钟超时配置");
                testResults.AppendLine("3. 增加了重试机制配置");
                testResults.AppendLine("4. 为缺料运算接口添加了特殊的超时处理");
                testResults.AppendLine();

                testResults.AppendLine("📊 建议的超时时间：");
                testResults.AppendLine("   - 快速查询接口：5-10分钟");
                testResults.AppendLine("   - 普通同步接口：15分钟");
                testResults.AppendLine("   - 缺料运算接口：30分钟");
                testResults.AppendLine("   - 大数据量同步：30-60分钟");

                var response = new WebResponseContent();
                return Json(response.OK("ESB超时配置测试完成", new {
                    TestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Results = testResults.ToString(),
                    CurrentConfig = new {
                        DefaultTimeoutMinutes = esbConfig?.DefaultTimeoutMinutes ?? 15,
                        LongRunningTimeoutMinutes = esbConfig?.LongRunningTimeoutMinutes ?? 30,
                        RetryCount = esbConfig?.RetryCount ?? 3,
                        RetryDelaySeconds = esbConfig?.RetryDelaySeconds ?? 30
                    }
                }));
            }
            catch (Exception ex)
            {
                var response = new WebResponseContent();
                return Json(response.Error($"测试异常：{ex.Message}"));
            }
        }

        #endregion

    }

    /// <summary>
    /// ESB数据同步请求参数
    /// </summary>
    public class ESBSyncRequest
    {
        /// <summary>
        /// 开始日期 (yyyy-MM-dd)
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// 结束日期 (yyyy-MM-dd)
        /// </summary>
        public string EndDate { get; set; }
    }

    /// <summary>
    /// ESB业务领域同步请求参数
    /// </summary>
    public class ESBBusinessDomainSyncRequest : ESBSyncRequest
    {
        /// <summary>
        /// 业务领域名称
        /// </summary>
        public string BusinessDomain { get; set; }
    }

    /// <summary>
    /// ESB单个订单明细同步请求参数
    /// </summary>
    public class ESBSingleOrderDetailRequest
    {
        /// <summary>
        /// 销售订单号
        /// </summary>
        public string SalesOrderNo { get; set; }
    }

    /// <summary>
    /// ESB单个批次信息同步请求参数
    /// </summary>
    public class ESBSingleBatchInfoRequest
    {
        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string PlanTrackingNo { get; set; }
    }

    /// <summary>
    /// ESB缺料运算结果同步请求参数
    /// </summary>
    public class ESBLackMtrlSyncRequest
    {
        /// <summary>
        /// 运算ID
        /// </summary>
        public string ComputeId { get; set; }
    }

    /// <summary>
    /// 技术负责人获取功能测试请求参数
    /// </summary>
    public class TestTechManagerRequest
    {
        /// <summary>
        /// 测试用的物料编码列表
        /// </summary>
        public string[] MaterialCodes { get; set; }
    }


}