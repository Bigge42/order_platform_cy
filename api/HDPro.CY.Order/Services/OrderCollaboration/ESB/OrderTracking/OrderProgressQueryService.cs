using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.OrderTracking
{
    /// <summary>
    /// 订单进度监造查询请求模型
    /// </summary>
    public class OrderProgressQueryRequest
    {
        /// <summary>
        /// 销售订单号
        /// </summary>
        [Required(ErrorMessage = "销售订单号不能为空")]
        [StringLength(30, ErrorMessage = "销售订单号长度不能超过30个字符")]
        public string FBILLNO { get; set; }
    }

    /// <summary>
    /// 订单进度监造查询响应模型
    /// </summary>
    public class OrderProgressQueryResponse
    {
        /// <summary>
        /// 销售订单号
        /// </summary>
        public string FBILLNO { get; set; }

        /// <summary>
        /// 用户合同号
        /// </summary>
        public string F_BLN_YHHTH { get; set; }

        /// <summary>
        /// 销售合同号
        /// </summary>
        public string F_BLN_CONTACTNONAME { get; set; }

        /// <summary>
        /// 销售负责人
        /// </summary>
        public string XSYNAME { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string F_ORA_XMMC { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string FCUSTName { get; set; }

        /// <summary>
        /// 订货台数
        /// </summary>
        public decimal? FQTY { get; set; }

        /// <summary>
        /// 整机台数
        /// </summary>
        public decimal? FZJQTY { get; set; }

        /// <summary>
        /// 备件台数
        /// </summary>
        public decimal? FBJQTY { get; set; }

        /// <summary>
        /// 单供台数
        /// </summary>
        public decimal? FDGQTY { get; set; }

        /// <summary>
        /// 合同签订时间
        /// </summary>
        public string F_ORA_DATE4 { get; set; }

        /// <summary>
        /// 交货日期
        /// </summary>
        public string F_BLN_HFJHRQ { get; set; }

        /// <summary>
        /// 排产日期
        /// </summary>
        public string F_ORA_DATE1 { get; set; }

        /// <summary>
        /// 入库数量
        /// </summary>
        public decimal? FRKREALQTY { get; set; }

        /// <summary>
        /// 待入库数量
        /// </summary>
        public decimal? FSQTY { get; set; }

        /// <summary>
        /// 出库数量
        /// </summary>
        public decimal? FCKREALQTY { get; set; }

        /// <summary>
        /// 物料表是否存在BOM（是）数量
        /// </summary>
        public decimal? FISBOMQTY { get; set; }

        /// <summary>
        /// 销售订单明细行数
        /// </summary>
        public decimal? FENTRYQTY { get; set; }

        /// <summary>
        /// 销售订单数量(整机+备件）
        /// </summary>
        public decimal? FZJBJQTY { get; set; }

        /// <summary>
        /// 计划确认数量
        /// </summary>
        public decimal? FJHQRSL { get; set; }

        /// <summary>
        /// 计划确认未开工数量
        /// </summary>
        public decimal? FJHQRWKGSL { get; set; }

        /// <summary>
        /// 开工数量
        /// </summary>
        public decimal? FKGSL { get; set; }

        /// <summary>
        /// 已领料工单数量
        /// </summary>
        public decimal? LLQTY { get; set; }

        /// <summary>
        /// 已预装工单数量
        /// </summary>
        public decimal? YZQTY { get; set; }

        /// <summary>
        /// 已阀体部件及执行机构装配工单数量
        /// </summary>
        public decimal? FTBJJZJJGZPQTY { get; set; }

        /// <summary>
        /// 已强压泄漏试验工单数量
        /// </summary>
        public decimal? QYXLSYQTY { get; set; }

        /// <summary>
        /// 已附件安装及调试工单数量
        /// </summary>
        public decimal? FJAZJTSQTY { get; set; }

        /// <summary>
        /// 已终检工单数量
        /// </summary>
        public decimal? ZJQTY { get; set; }

        /// <summary>
        /// 已油漆工单数量
        /// </summary>
        public decimal? YQQTY { get; set; }

        /// <summary>
        /// 已装箱工单数量
        /// </summary>
        public decimal? ZXQTY { get; set; }

        /// <summary>
        /// 已装箱检验工单数量
        /// </summary>
        public decimal? ZXJYQTY { get; set; }

        /// <summary>
        /// 已装配完工工单数量
        /// </summary>
        public decimal? ZPWGQTY { get; set; }

        /// <summary>
        /// 主键ID
        /// </summary>
        public int FID { get; set; }
    }

    /// <summary>
    /// 订单进度监造查询服务
    /// 提供移动端查询订单进度信息的接口
    /// </summary>
    public class OrderProgressQueryService : ESBBaseService
    {
        private const string API_PATH = "/DataCenter/SearchSalAllProgress";

        public OrderProgressQueryService(IHttpClientFactory httpClientFactory, ILogger<OrderProgressQueryService> logger)
            : base(httpClientFactory, logger)
        {
        }

        /// <summary>
        /// 查询订单进度监造信息
        /// </summary>
        /// <param name="request">查询请求参数</param>
        /// <returns>订单进度信息列表</returns>
        public async Task<List<OrderProgressQueryResponse>> QueryOrderProgressAsync(OrderProgressQueryRequest request)
        {
            if (request == null)
            {
                _logger.LogWarning("订单进度查询请求参数为空");
                return new List<OrderProgressQueryResponse>();
            }

            if (string.IsNullOrWhiteSpace(request.FBILLNO))
            {
                _logger.LogWarning("销售订单号不能为空");
                return new List<OrderProgressQueryResponse>();
            }

            try
            {
                _logger.LogInformation($"开始查询订单进度监造信息，销售订单号：{request.FBILLNO}");

                // 调用ESB接口获取数据
                var requestData = new { FBILLNO = request.FBILLNO.Trim() };
                var result = await CallESBApiWithRequestData<OrderProgressQueryResponse>(
                    API_PATH, 
                    requestData, 
                    "订单进度监造查询"
                );

                _logger.LogInformation($"订单进度查询完成，销售订单号：{request.FBILLNO}，返回记录数：{result?.Count ?? 0}");

                return result ?? new List<OrderProgressQueryResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询订单进度监造信息异常，销售订单号：{request.FBILLNO}，错误：{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 查询订单进度监造信息（通过销售订单号）
        /// </summary>
        /// <param name="billNo">销售订单号</param>
        /// <returns>订单进度信息列表</returns>
        public async Task<List<OrderProgressQueryResponse>> QueryOrderProgressAsync(string billNo)
        {
            var request = new OrderProgressQueryRequest { FBILLNO = billNo };
            return await QueryOrderProgressAsync(request);
        }

        /// <summary>
        /// 获取移动端订单进度信息（含业务逻辑处理）
        /// </summary>
        /// <param name="billNo">销售订单号</param>
        /// <returns>移动端订单进度信息</returns>
        public async Task<MobileOrderProgressResponse> GetMobileOrderProgressAsync(string billNo)
        {
            try
            {
                _logger.LogInformation("开始获取移动端订单进度信息，销售订单号：{BillNo}", billNo);

                var progressList = await QueryOrderProgressAsync(billNo);
                if (progressList == null || progressList.Count == 0)
                {
                    _logger.LogWarning("未找到订单进度数据，销售订单号：{BillNo}", billNo);
                    return new MobileOrderProgressResponse { FBILLNO = billNo };
                }

                var data = progressList[0]; // ESB返回的是汇总数据，取第一条
                var totalQty = (data.FQTY ?? 0) + (data.FBJQTY ?? 0); // 整机+备件总数量

                var mobileResponse = new MobileOrderProgressResponse
                {
                    // 基础信息
                    FBILLNO = data.FBILLNO,
                    F_BLN_CONTACTNONAME = data.F_BLN_CONTACTNONAME,
                    F_BLN_YHHTH = data.F_BLN_YHHTH,
                    XSYNAME = data.XSYNAME,
                    F_ORA_XMMC = data.F_ORA_XMMC,
                    FCUSTName = data.FCUSTName,
                    FQTY = data.FQTY ?? 0,
                    FBJQTY = data.FBJQTY ?? 0,
                    F_ORA_DATE4 = data.F_ORA_DATE4,
                    F_ORA_DATE1 = data.F_ORA_DATE1,
                    F_BLN_HFJHRQ = data.F_BLN_HFJHRQ,
                    FRKREALQTY = data.FRKREALQTY ?? 0,
                    FSQTY = data.FSQTY ?? 0,
                    FCKREALQTY = data.FCKREALQTY ?? 0,
                };

                // 计算超期数量：待入库数量>0 并且 当前时间>交货日期
                mobileResponse.OverdueQty = CalculateOverdueQty(data.FSQTY ?? 0, data.F_BLN_HFJHRQ);

                // 发货状态计算
                if (totalQty > 0)
                {
                    mobileResponse.ShippingProgress = Math.Round((mobileResponse.FCKREALQTY / totalQty) * 100, 0);
                    mobileResponse.ShippingStatus = mobileResponse.ShippingProgress >= 100 ? "已完成" : 
                        mobileResponse.ShippingProgress >= 98 ? "即将完成" : $"未完成({mobileResponse.ShippingProgress}%)";
                }

                // 逾期状态
                mobileResponse.OverdueStatus = mobileResponse.OverdueQty > 0 ? "逾期" : "未逾期";

                // BOM创建进度
                if (totalQty > 0)
                {
                    var bomQty = data.FISBOMQTY ?? 0; // BOM数量
                    mobileResponse.BOMProgress = Math.Round((bomQty / totalQty) * 100, 0);
                    mobileResponse.BOMStatus = GetProgressStatus(mobileResponse.BOMProgress, "BOM创建");
                }

                // 计划确认进度
                if (totalQty > 0)
                {
                    var planConfirmQty = data.FJHQRSL ?? 0; // 计划确认数量
                    mobileResponse.PlanConfirmProgress = Math.Round((planConfirmQty / totalQty) * 100, 0);
                    mobileResponse.PlanConfirmStatus = GetProgressStatus(mobileResponse.PlanConfirmProgress, "计划确认");
                }

                // 物料准备进度
                if (totalQty > 0)
                {
                    var materialPrepQty = data.FJHQRWKGSL ?? 0; // 计划确认未开工数量
                    mobileResponse.MaterialPrepProgress = Math.Round((materialPrepQty / totalQty) * 100, 0);
                    mobileResponse.MaterialPrepStatus = GetProgressStatus(mobileResponse.MaterialPrepProgress, "物料准备");
                }

                // 推送生产进度
                if (totalQty > 0)
                {
                    var productionQty = data.FKGSL ?? 0; // 开工数量
                    mobileResponse.ProductionPushProgress = Math.Round((productionQty / totalQty) * 100, 0);
                    mobileResponse.ProductionPushStatus = GetProgressStatus(mobileResponse.ProductionPushProgress, "推送生产");
                }

                // 领料进度
                if (totalQty > 0)
                {
                    var materialCollectionQty = data.LLQTY ?? 0; // 已领料工单数量
                    mobileResponse.MaterialCollectionProgress = Math.Round((materialCollectionQty / totalQty) * 100, 0);
                    mobileResponse.MaterialCollectionStatus = GetProgressStatus(mobileResponse.MaterialCollectionProgress, "领料");
                }

                // 预装进度
                if (totalQty > 0)
                {
                    var preAssemblyQty = data.YZQTY ?? 0; // 已预装工单数量
                    mobileResponse.PreAssemblyProgress = Math.Round((preAssemblyQty / totalQty) * 100, 0);
                    mobileResponse.PreAssemblyStatus = GetProgressStatus(mobileResponse.PreAssemblyProgress, "预装");
                }

                // 部件装配进度
                if (totalQty > 0)
                {
                    var partAssemblyQty = data.FTBJJZJJGZPQTY ?? 0; // 阀体部件及执行机构装配工单数量
                    mobileResponse.PartAssemblyProgress = Math.Round((partAssemblyQty / totalQty) * 100, 0);
                    mobileResponse.PartAssemblyStatus = GetProgressStatus(mobileResponse.PartAssemblyProgress, "部件装配");
                }

                // 强压泄漏进度
                if (totalQty > 0)
                {
                    var pressureTestQty = data.QYXLSYQTY ?? 0; // 强压泄漏试验工单数量
                    mobileResponse.PressureTestProgress = Math.Round((pressureTestQty / totalQty) * 100, 0);
                    mobileResponse.PressureTestStatus = GetProgressStatus(mobileResponse.PressureTestProgress, "强压泄漏");
                }

                // 附件安装进度
                if (totalQty > 0)
                {
                    var accessoryInstallQty = data.FJAZJTSQTY ?? 0; // 附件安装及调试工单数量
                    mobileResponse.AccessoryInstallProgress = Math.Round((accessoryInstallQty / totalQty) * 100, 0);
                    mobileResponse.AccessoryInstallStatus = GetProgressStatus(mobileResponse.AccessoryInstallProgress, "附件安装");
                }

                // 终检进度
                if (totalQty > 0)
                {
                    var finalInspectionQty = data.ZJQTY ?? 0; // 终检工单数量
                    mobileResponse.FinalInspectionProgress = Math.Round((finalInspectionQty / totalQty) * 100, 0);
                    mobileResponse.FinalInspectionStatus = GetProgressStatus(mobileResponse.FinalInspectionProgress, "终检");
                }

                // 油漆进度
                if (totalQty > 0)
                {
                    var paintingQty = data.YQQTY ?? 0; // 油漆工单数量
                    mobileResponse.PaintingProgress = Math.Round((paintingQty / totalQty) * 100, 0);
                    mobileResponse.PaintingStatus = GetProgressStatus(mobileResponse.PaintingProgress, "油漆");
                }

                // 装箱进度
                if (totalQty > 0)
                {
                    var packingQty = data.ZXQTY ?? 0; // 装箱工单数量
                    mobileResponse.PackingProgress = Math.Round((packingQty / totalQty) * 100, 0);
                    mobileResponse.PackingStatus = GetProgressStatus(mobileResponse.PackingProgress, "装箱");
                }

                // 装箱检验进度
                if (totalQty > 0)
                {
                    var packingInspectionQty = data.ZXJYQTY ?? 0; // 装箱检验工单数量
                    mobileResponse.PackingInspectionProgress = Math.Round((packingInspectionQty / totalQty) * 100, 0);
                    mobileResponse.PackingInspectionStatus = GetProgressStatus(mobileResponse.PackingInspectionProgress, "装箱检验");
                }

                // 装配完工进度
                if (totalQty > 0)
                {
                    var assemblyCompleteQty = data.ZPWGQTY ?? 0; // 装配完工工单数量
                    mobileResponse.AssemblyCompleteProgress = Math.Round((assemblyCompleteQty / totalQty) * 100, 0);
                    mobileResponse.AssemblyCompleteStatus = GetProgressStatus(mobileResponse.AssemblyCompleteProgress, "装配完工");
                }

                // 出库进度
                if (totalQty > 0)
                {
                    mobileResponse.OutboundProgress = Math.Round((mobileResponse.FCKREALQTY / totalQty) * 100, 0);
                    mobileResponse.OutboundStatus = GetProgressStatus(mobileResponse.OutboundProgress, "出库");
                }

                _logger.LogInformation("获取移动端订单进度信息成功，订单号：{BillNo}", billNo);
                return mobileResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取移动端订单进度信息失败，销售订单号：{BillNo}", billNo);
                throw;
            }
        }

        /// <summary>
        /// 计算超期数量
        /// </summary>
        /// <param name="waitInStockQty">待入库数量</param>
        /// <param name="deliveryDateStr">交货日期字符串</param>
        /// <returns>超期数量</returns>
        private decimal CalculateOverdueQty(decimal waitInStockQty, string deliveryDateStr)
        {
            try
            {
                // 如果待入库数量为0，则不存在超期
                if (waitInStockQty <= 0)
                {
                    return 0;
                }

                // 如果交货日期为空，则无法判断是否超期
                if (string.IsNullOrWhiteSpace(deliveryDateStr))
                {
                    return 0;
                }

                // 解析交货日期
                if (DateTime.TryParse(deliveryDateStr, out DateTime deliveryDate))
                {
                    // 当前时间大于交货日期，且待入库数量>0，则该数量为超期数量
                    if (DateTime.Now > deliveryDate)
                    {
                        _logger.LogInformation("计算超期数量：待入库数量={WaitQty}，交货日期={DeliveryDate}，当前时间={CurrentTime}，超期数量={OverdueQty}", 
                            waitInStockQty, deliveryDate.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"), waitInStockQty);
                        return waitInStockQty;
                    }
                    else
                    {
                        // 未超期
                        return 0;
                    }
                }
                else
                {
                    _logger.LogWarning("无法解析交货日期：{DeliveryDateStr}", deliveryDateStr);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "计算超期数量时发生异常，待入库数量：{WaitQty}，交货日期：{DeliveryDate}", waitInStockQty, deliveryDateStr);
                return 0;
            }
        }

        /// <summary>
        /// 根据进度百分比获取状态描述
        /// </summary>
        /// <param name="progress">进度百分比</param>
        /// <param name="processName">工序名称</param>
        /// <returns>状态描述</returns>
        private string GetProgressStatus(decimal progress, string processName)
        {
            // 统一格式：工序名称（百分比%），百分比四舍五入为整数
            var roundedProgress = Math.Round(progress, 0);
            return $"{processName}({roundedProgress}%)";
        }

        /// <summary>
        /// 获取订单进度统计信息
        /// </summary>
        /// <param name="billNo">销售订单号</param>
        /// <returns>进度统计信息</returns>
        public async Task<OrderProgressSummary> GetOrderProgressSummaryAsync(string billNo)
        {
            var progressList = await QueryOrderProgressAsync(billNo);
            
            if (progressList == null || progressList.Count == 0)
            {
                return new OrderProgressSummary();
            }

            // ESB接口返回的数据已经是按销售订单号汇总的结果
            // 通常每个销售订单号只返回一条汇总记录
            var firstRecord = progressList[0];
            
            var summary = new OrderProgressSummary
            {
                TotalOrderQty = firstRecord.FQTY ?? 0,
                TotalZJQty = firstRecord.FZJQTY ?? 0,
                TotalBJQty = firstRecord.FBJQTY ?? 0,
                TotalDGQty = firstRecord.FDGQTY ?? 0,
                TotalInStockQty = firstRecord.FRKREALQTY ?? 0,
                TotalWaitInStockQty = firstRecord.FSQTY ?? 0,
                TotalOutStockQty = firstRecord.FCKREALQTY ?? 0,
                TotalStartWorkQty = firstRecord.FKGSL ?? 0,
                TotalMaterialCollectedQty = firstRecord.LLQTY ?? 0,
                TotalPreAssemblyQty = firstRecord.YZQTY ?? 0,
                TotalValveAssemblyQty = firstRecord.FTBJJZJJGZPQTY ?? 0,
                TotalPressureTestQty = firstRecord.QYXLSYQTY ?? 0,
                TotalAccessoryInstallQty = firstRecord.FJAZJTSQTY ?? 0,
                TotalFinalInspectionQty = firstRecord.ZJQTY ?? 0,
                TotalPaintingQty = firstRecord.YQQTY ?? 0,
                TotalPackingQty = firstRecord.ZXQTY ?? 0,
                TotalPackingInspectionQty = firstRecord.ZXJYQTY ?? 0,
                TotalAssemblyCompleteQty = firstRecord.ZPWGQTY ?? 0
            };

            // 计算完成率，确保分母不为零
            if (summary.TotalOrderQty > 0)
            {
                summary.CompletionRate = Math.Round((summary.TotalAssemblyCompleteQty / summary.TotalOrderQty) * 100, 2);
            }
            else
            {
                summary.CompletionRate = 0;
            }

            return summary;
        }
    }

    /// <summary>
    /// 移动端订单进度查询响应模型
    /// 包含业务逻辑处理后的字段和进度百分比
    /// </summary>
    public class MobileOrderProgressResponse
    {
        /// <summary>
        /// 销售订单号
        /// </summary>
        public string FBILLNO { get; set; }

        /// <summary>
        /// 销售合同号
        /// </summary>
        public string F_BLN_CONTACTNONAME { get; set; }

        /// <summary>
        /// 用户合同号
        /// </summary>
        public string F_BLN_YHHTH { get; set; }

        /// <summary>
        /// 销售负责人
        /// </summary>
        public string XSYNAME { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string F_ORA_XMMC { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string FCUSTName { get; set; }

        /// <summary>
        /// 订货台数
        /// </summary>
        public decimal FQTY { get; set; }

        /// <summary>
        /// 备件台数
        /// </summary>
        public decimal FBJQTY { get; set; }

        /// <summary>
        /// 合同签订时间
        /// </summary>
        public string F_ORA_DATE4 { get; set; }

        /// <summary>
        /// 排产日期
        /// </summary>
        public string F_ORA_DATE1 { get; set; }

        /// <summary>
        /// 交货日期
        /// </summary>
        public string F_BLN_HFJHRQ { get; set; }

        /// <summary>
        /// 入库数量
        /// </summary>
        public decimal FRKREALQTY { get; set; }

        /// <summary>
        /// 待入库数量
        /// </summary>
        public decimal FSQTY { get; set; }

        /// <summary>
        /// 超期数量
        /// </summary>
        public decimal OverdueQty { get; set; }

        /// <summary>
        /// 出库数量
        /// </summary>
        public decimal FCKREALQTY { get; set; }

        /// <summary>
        /// 发货状态（百分比）
        /// </summary>
        public decimal ShippingProgress { get; set; }

        /// <summary>
        /// 发货状态描述
        /// </summary>
        public string ShippingStatus { get; set; }

        /// <summary>
        /// 逾期状态
        /// </summary>
        public string OverdueStatus { get; set; }

        /// <summary>
        /// BOM创建进度（百分比）
        /// </summary>
        public decimal BOMProgress { get; set; }

        /// <summary>
        /// BOM创建状态描述
        /// </summary>
        public string BOMStatus { get; set; }

        /// <summary>
        /// 计划确认进度（百分比）
        /// </summary>
        public decimal PlanConfirmProgress { get; set; }

        /// <summary>
        /// 计划确认状态描述
        /// </summary>
        public string PlanConfirmStatus { get; set; }

        /// <summary>
        /// 物料准备进度（百分比）
        /// </summary>
        public decimal MaterialPrepProgress { get; set; }

        /// <summary>
        /// 物料准备状态描述
        /// </summary>
        public string MaterialPrepStatus { get; set; }

        /// <summary>
        /// 推送生产进度（百分比）
        /// </summary>
        public decimal ProductionPushProgress { get; set; }

        /// <summary>
        /// 推送生产状态描述
        /// </summary>
        public string ProductionPushStatus { get; set; }

        /// <summary>
        /// 领料进度（百分比）
        /// </summary>
        public decimal MaterialCollectionProgress { get; set; }

        /// <summary>
        /// 领料状态描述
        /// </summary>
        public string MaterialCollectionStatus { get; set; }

        /// <summary>
        /// 预装进度（百分比）
        /// </summary>
        public decimal PreAssemblyProgress { get; set; }

        /// <summary>
        /// 预装状态描述
        /// </summary>
        public string PreAssemblyStatus { get; set; }

        /// <summary>
        /// 部件装配进度（百分比）
        /// </summary>
        public decimal PartAssemblyProgress { get; set; }

        /// <summary>
        /// 部件装配状态描述
        /// </summary>
        public string PartAssemblyStatus { get; set; }

        /// <summary>
        /// 强压泄漏进度（百分比）
        /// </summary>
        public decimal PressureTestProgress { get; set; }

        /// <summary>
        /// 强压泄漏状态描述
        /// </summary>
        public string PressureTestStatus { get; set; }

        /// <summary>
        /// 附件安装进度（百分比）
        /// </summary>
        public decimal AccessoryInstallProgress { get; set; }

        /// <summary>
        /// 附件安装状态描述
        /// </summary>
        public string AccessoryInstallStatus { get; set; }

        /// <summary>
        /// 终检进度（百分比）
        /// </summary>
        public decimal FinalInspectionProgress { get; set; }

        /// <summary>
        /// 终检状态描述
        /// </summary>
        public string FinalInspectionStatus { get; set; }

        /// <summary>
        /// 油漆进度（百分比）
        /// </summary>
        public decimal PaintingProgress { get; set; }

        /// <summary>
        /// 油漆状态描述
        /// </summary>
        public string PaintingStatus { get; set; }

        /// <summary>
        /// 装箱进度（百分比）
        /// </summary>
        public decimal PackingProgress { get; set; }

        /// <summary>
        /// 装箱状态描述
        /// </summary>
        public string PackingStatus { get; set; }

        /// <summary>
        /// 装箱检验进度（百分比）
        /// </summary>
        public decimal PackingInspectionProgress { get; set; }

        /// <summary>
        /// 装箱检验状态描述
        /// </summary>
        public string PackingInspectionStatus { get; set; }

        /// <summary>
        /// 装配完工进度（百分比）
        /// </summary>
        public decimal AssemblyCompleteProgress { get; set; }

        /// <summary>
        /// 装配完工状态描述
        /// </summary>
        public string AssemblyCompleteStatus { get; set; }

        /// <summary>
        /// 出库进度（百分比）
        /// </summary>
        public decimal OutboundProgress { get; set; }

        /// <summary>
        /// 出库状态描述
        /// </summary>
        public string OutboundStatus { get; set; }
    }

    /// <summary>
    /// 订单进度统计信息
    /// </summary>
    public class OrderProgressSummary
    {
        /// <summary>
        /// 总订货台数
        /// </summary>
        public decimal TotalOrderQty { get; set; }

        /// <summary>
        /// 总整机台数
        /// </summary>
        public decimal TotalZJQty { get; set; }

        /// <summary>
        /// 总备件台数
        /// </summary>
        public decimal TotalBJQty { get; set; }

        /// <summary>
        /// 总单供台数
        /// </summary>
        public decimal TotalDGQty { get; set; }

        /// <summary>
        /// 总入库数量
        /// </summary>
        public decimal TotalInStockQty { get; set; }

        /// <summary>
        /// 总待入库数量
        /// </summary>
        public decimal TotalWaitInStockQty { get; set; }

        /// <summary>
        /// 总出库数量
        /// </summary>
        public decimal TotalOutStockQty { get; set; }

        /// <summary>
        /// 总开工数量
        /// </summary>
        public decimal TotalStartWorkQty { get; set; }

        /// <summary>
        /// 总已领料工单数量
        /// </summary>
        public decimal TotalMaterialCollectedQty { get; set; }

        /// <summary>
        /// 总已预装工单数量
        /// </summary>
        public decimal TotalPreAssemblyQty { get; set; }

        /// <summary>
        /// 总已阀体部件及执行机构装配工单数量
        /// </summary>
        public decimal TotalValveAssemblyQty { get; set; }

        /// <summary>
        /// 总已强压泄漏试验工单数量
        /// </summary>
        public decimal TotalPressureTestQty { get; set; }

        /// <summary>
        /// 总已附件安装及调试工单数量
        /// </summary>
        public decimal TotalAccessoryInstallQty { get; set; }

        /// <summary>
        /// 总已终检工单数量
        /// </summary>
        public decimal TotalFinalInspectionQty { get; set; }

        /// <summary>
        /// 总已油漆工单数量
        /// </summary>
        public decimal TotalPaintingQty { get; set; }

        /// <summary>
        /// 总已装箱工单数量
        /// </summary>
        public decimal TotalPackingQty { get; set; }

        /// <summary>
        /// 总已装箱检验工单数量
        /// </summary>
        public decimal TotalPackingInspectionQty { get; set; }

        /// <summary>
        /// 总已装配完工工单数量
        /// </summary>
        public decimal TotalAssemblyCompleteQty { get; set; }

        /// <summary>
        /// 完成率（百分比）
        /// </summary>
        public decimal CompletionRate { get; set; }
    }
} 