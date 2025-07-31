/*
 *所有关于OCP_SOProgressDetail类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_SOProgressDetailService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.Services.OrderCollaboration.ESB.SalesManagement;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using HDPro.CY.Order.IServices.OA;
using HDPro.CY.Order.Services.OA;
using System.Collections.Generic;
using HDPro.Core.UserManager;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_SOProgressDetailService
    {
        private readonly IOCP_SOProgressDetailRepository _repository;//访问数据库
        private readonly ILogger<OCP_SOProgressDetailService> _logger;
        private readonly IOAIntegrationService _oaIntegrationService;

        [ActivatorUtilitiesConstructor]
        public OCP_SOProgressDetailService(
            IOCP_SOProgressDetailRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OCP_SOProgressDetailService> logger
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _logger = logger;
            _oaIntegrationService = AutofacContainerModule.GetService<IOAIntegrationService>();
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_SOProgressDetail特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_SOProgressDetail特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_SOProgressDetail特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_SOProgressDetail entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_SOProgressDetail特有的数据验证逻辑
            
            return response;
        }

        //查询(主表合计)
        public override PageGridData<OCP_SOProgressDetail> GetPageData(PageDataOptions options)
        {
            try
            {
                // 在查询之前，先尝试同步ESB数据
                var orderIdFromOptions = ExtractOrderIdFromOptions(options);
                if (orderIdFromOptions.HasValue)
                {
                    SyncOrderDetailBeforeQuery(orderIdFromOptions.Value);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "查询前同步ESB数据时发生异常，继续执行查询操作");
            }

            //EF:查询table界面显示合计（需要与前端开发文档上的【table显示合计】一起使用）
            SummaryExpress = (IQueryable<OCP_SOProgressDetail> queryable) =>
            {
                return queryable.GroupBy(x => 1).Select(x => new
                {
                    //AvgPrice注意大小写和数据库字段大小写一样
                    InstockQty = x.Sum(o => o.InstockQty),
                    UnInstockQty = x.Sum(o => o.UnInstockQty),
                    OutStockQty = x.Sum(o => o.OutStockQty),
                })
                .FirstOrDefault();
            };
            ////sqlsugar：查询table界面显示合计（需要与前端开发文档上的【table显示合计】一起使用）
            // SummaryExpress = (ISugarQueryable<主表表名> queryable) =>
            // {
            //     return queryable.Select(x => new 
            //     {
            //         FundsSum = SqlFunc.AggregateSum(x.FundsSum),
            //         HasPayedSum = SqlFunc.AggregateSum(x.HasPayedSum),
            //         NeedPaySum = SqlFunc.AggregateSum(x.NeedPaySum),
            //         Items = SqlFunc.AggregateSum(x.Items)
            //     })
            //     .FirstOrDefault();
            // };
            return base.GetPageData(options);
        }

        /// <summary>
        /// 从PageDataOptions参数中提取OrderID
        /// </summary>
        /// <param name="options">查询选项</param>
        /// <returns>订单ID</returns>
        private long? ExtractOrderIdFromOptions(PageDataOptions options)
        {
            try
            {
                // 先检查Filter属性中的查询条件
                if (options?.Filter?.Any() == true)
                {
                    var orderIdCondition = options.Filter.FirstOrDefault(w => 
                        string.Equals(w.Name, "OrderID", StringComparison.OrdinalIgnoreCase));
                    
                    if (orderIdCondition?.Value != null && long.TryParse(orderIdCondition.Value, out long orderId))
                    {
                        _logger?.LogInformation($"从Filter查询条件中提取到OrderID: {orderId}");
                        return orderId;
                    }
                }

                // 如果Filter为空，尝试从JSON格式的Wheres字符串中解析
                if (!string.IsNullOrEmpty(options?.Wheres))
                {
                    try
                    {
                        var whereConditions = JsonConvert.DeserializeObject<dynamic[]>(options.Wheres);
                        foreach (var condition in whereConditions)
                        {
                            if (condition.name?.ToString().Equals("OrderID", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                if (long.TryParse(condition.value?.ToString(), out long orderId))
                                {
                                    _logger?.LogInformation($"从Wheres JSON查询条件中提取到OrderID: {orderId}");
                                    return orderId;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "解析Wheres JSON格式的查询条件时发生异常");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "提取OrderID时发生异常");
            }

            return null;
        }

        /// <summary>
        /// 根据OrderID获取SOBillNo并同步订单明细
        /// </summary>
        /// <param name="orderId">订单ID</param>
        private void SyncOrderDetailBeforeQuery(long orderId)
        {
            try
            {
                _logger?.LogInformation($"开始为OrderID={orderId}同步ESB订单明细数据");

                // 根据OrderID从OCP_SOProgress表中获取销售订单号(BillNo)
                var soProgressRepository = AutofacContainerModule.GetService<IOCP_SOProgressRepository>();
                var soProgress = soProgressRepository.Find(x => x.OrderID == orderId).FirstOrDefault();
                
                if (soProgress == null)
                {
                    _logger?.LogWarning($"未找到OrderID={orderId}对应的销售订单进度记录");
                    return;
                }

                if (string.IsNullOrEmpty(soProgress.BillNo))
                {
                    _logger?.LogWarning($"OrderID={orderId}对应的销售订单号(BillNo)为空");
                    return;
                }

                _logger?.LogInformation($"找到OrderID={orderId}对应的销售订单号: {soProgress.BillNo}");

                // 调用销售管理ESB同步协调器来同步单个订单的明细
                var syncCoordinator = SalesManagementESBSyncCoordinator.Instance;
                try
                {
                    var syncResult =  syncCoordinator.SyncSingleOrderDetail(soProgress.BillNo).GetAwaiter().GetResult();
                    if (syncResult.Status)
                    {
                        _logger?.LogInformation($"订单 {soProgress.BillNo} 明细同步成功: {syncResult.Message}");
                    }
                    else
                    {
                        _logger?.LogWarning($"订单 {soProgress.BillNo} 明细同步失败: {syncResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"异步同步订单 {soProgress.BillNo} 明细时发生异常");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"为OrderID={orderId}同步ESB订单明细数据时发生异常");
            }
        }

        /// <summary>
        /// 发起合同变更评审流程
        /// </summary>
        /// <param name="contractChangeRequest">合同变更请求数据</param>
        /// <returns>发起结果</returns>
        public async Task<WebResponseContent> StartContractChangeReviewAsync(ContractChangeRequest contractChangeRequest)
        {
            var response = new WebResponseContent();
            
            try
            {
                if (contractChangeRequest == null)
                {
                    response.Status = false;
                    response.Message = "合同变更请求数据不能为空";
                    _logger?.LogWarning("发起合同变更评审流程失败：请求数据为空");
                    return response;
                }

                // 参数验证
                if (string.IsNullOrEmpty(contractChangeRequest.ContractNo))
                {
                    response.Status = false;
                    response.Message = "合同号不能为空";
                    _logger?.LogWarning("发起合同变更评审流程失败：合同号为空");
                    return response;
                }

                if (contractChangeRequest.SelectedItems == null || !contractChangeRequest.SelectedItems.Any())
                {
                    response.Status = false;
                    response.Message = "请选择要变更的订单明细";
                    _logger?.LogWarning("发起合同变更评审流程失败：未选择订单明细");
                    return response;
                }

                _logger?.LogInformation("开始发起合同变更评审流程 - 合同号: {ContractNo}, 客户类型: {CustomerType}, 选中明细数量: {ItemCount}",
                    contractChangeRequest.ContractNo, contractChangeRequest.CustomerType, contractChangeRequest.SelectedItems.Count);

                // 处理选中的明细数据
                var planNos = new List<string>();
                var specifications = new List<string>();
                decimal totalCatalogPrice = 0; // 目录价累加
                decimal totalSalePrice = 0; // 售价累加

                foreach (var item in contractChangeRequest.SelectedItems)
                {
                    // 计划号拼接
                    if (!string.IsNullOrEmpty(item.PlanNo))
                    {
                        planNos.Add(item.PlanNo);
                    }

                    // 规格型号拼接
                    if (!string.IsNullOrEmpty(item.Specification))
                    {
                        specifications.Add(item.Specification);
                    }

                    // 价格累加
                    if (decimal.TryParse(item.CatalogPrice, out decimal catalogPrice))
                    {
                        totalCatalogPrice += catalogPrice;
                    }

                    if (decimal.TryParse(item.SalePrice, out decimal salePrice))
                    {
                        totalSalePrice += salePrice;
                    }
                }

                // 计算下浮率：（目录价累加-售价累加）/目录价累加 * 100
                decimal discountRate = 0;
                if (totalCatalogPrice > 0)
                {
                    discountRate = (totalCatalogPrice - totalSalePrice) / totalCatalogPrice * 100;
                }

                // 构建合同变更数据
                var contractChangeData = new ContractChangeReviewData
                {
                    CustomerType = GetCustomerTypeId(contractChangeRequest.CustomerType),
                    PlanNo = string.Join(";", planNos),
                    ContractNo = contractChangeRequest.ContractNo,
                    Specification = string.Join(";", specifications),
                    BeforeChangePrice = totalSalePrice.ToString("F2"),
                    BeforeChangeDiscount = discountRate.ToString("F2")
                };

                _logger?.LogInformation("合同变更数据准备完成 - 计划号: {PlanNos}, 规格型号: {Specifications}, 变更前价格: {Price}, 变更前下浮: {Discount}%",
                    contractChangeData.PlanNo, contractChangeData.Specification, contractChangeData.BeforeChangePrice, contractChangeData.BeforeChangeDiscount);

                // 获取当前用户登录名
                var currentUser = HDPro.Core.ManageUser.UserContext.Current;
                var loginName = currentUser?.UserName ?? "system";

                // 发起OA流程
                var oaResult = await _oaIntegrationService.StartContractChangeReviewProcessAsync(loginName, contractChangeData);
                
                if (oaResult.Status)
                {
                    response.Status = true;
                    response.Data = oaResult.Data;
                    response.Message = "合同变更评审流程发起成功";
                    
                    _logger?.LogInformation("合同变更评审流程发起成功 - 合同号: {ContractNo}, 流程ID: {ProcessId}",
                        contractChangeRequest.ContractNo, oaResult.Data);
                }
                else
                {
                    response.Status = false;
                    response.Message = $"合同变更评审流程发起失败: {oaResult.Message}";
                    _logger?.LogError("合同变更评审流程发起失败 - 合同号: {ContractNo}, 错误: {Error}",
                        contractChangeRequest.ContractNo, oaResult.Message);
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"发起合同变更评审流程异常: {ex.Message}";
                _logger?.LogError(ex, "发起合同变更评审流程时发生异常 - 合同号: {ContractNo}",
                    contractChangeRequest?.ContractNo);
            }

            return response;
        }

        /// <summary>
        /// 获取客户类型ID
        /// </summary>
        /// <param name="customerType">客户类型名称</param>
        /// <returns>客户类型ID</returns>
        private string GetCustomerTypeId(string customerType)
        {
            if (string.IsNullOrEmpty(customerType))
            {
                return "-661824289608153598"; // 默认为网点
            }

            switch (customerType.ToLower())
            {
                case "网点":
                    return "-661824289608153598";
                case "自签":
                    return "-1675890569785356551";
                default:
                    _logger?.LogWarning("未知的客户类型: {CustomerType}, 使用默认值（网点）", customerType);
                    return "-661824289608153598"; // 默认为网点
            }
        }
    }

    /// <summary>
    /// 合同变更请求数据模型
    /// </summary>
    public class ContractChangeRequest
    {
        /// <summary>
        /// 合同号
        /// </summary>
        public string ContractNo { get; set; }

        /// <summary>
        /// 客户类型（网点/自签）
        /// </summary>
        public string CustomerType { get; set; }

        /// <summary>
        /// 选中的订单明细项
        /// </summary>
        public List<ContractChangeItem> SelectedItems { get; set; }
    }

    /// <summary>
    /// 合同变更明细项
    /// </summary>
    public class ContractChangeItem
    {
        /// <summary>
        /// 计划号
        /// </summary>
        public string PlanNo { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specification { get; set; }

        /// <summary>
        /// 目录价格
        /// </summary>
        public string CatalogPrice { get; set; }

        /// <summary>
        /// 销售价格
        /// </summary>
        public string SalePrice { get; set; }
    }
} 