/*
 *所有关于OCP_OrderTracking类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_OrderTrackingService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Core.ManageUser;
using HDPro.CY.Order.Services.Common;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_OrderTrackingService
    {
        private readonly IOCP_OrderTrackingRepository _repository;//访问数据库
        private readonly ILogger<OCP_OrderTrackingService> _logger;//日志记录器
        private readonly IOCP_LackMtrlResultRepository _lackMtrlResultRepository;//缺料运算结果Repository
        private readonly IOCP_LackMtrlPlanRepository _lackMtrlPlanRepository;//缺料运算方案Repository
        private readonly IOCP_AlertRulesRepository _alertRulesRepository;//预警规则Repository

        [ActivatorUtilitiesConstructor]
        public OCP_OrderTrackingService(
            IOCP_OrderTrackingRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OCP_OrderTrackingService> logger,
            IOCP_LackMtrlResultRepository lackMtrlResultRepository,
            IOCP_LackMtrlPlanRepository lackMtrlPlanRepository,
            IOCP_AlertRulesRepository alertRulesRepository
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _logger = logger;
            _lackMtrlResultRepository = lackMtrlResultRepository;
            _lackMtrlPlanRepository = lackMtrlPlanRepository;
            _alertRulesRepository = alertRulesRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_OrderTracking特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_OrderTracking特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_OrderTracking特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_OrderTracking entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_OrderTracking特有的数据验证逻辑
            
            return response;
        }

        //查询(主表合计)
        public override PageGridData<OCP_OrderTracking> GetPageData(PageDataOptions options)
        {
            //此处是从前台提交的原生的查询条件，这里可以自己过滤
            QueryRelativeList = (List<SearchParameters> parameters) =>
            {
                // 超级管理员不添加过滤条件
                if (UserContext.Current.IsSuperAdmin)
                {
                    return;
                }

                // 添加默认查询条件：根据订单状态排除【关闭】
                bool hasOrderStatusCondition = parameters.Any(p => p.Name == "BillStatus");
                if (!hasOrderStatusCondition)
                {
                    parameters.Add(new SearchParameters
                    {
                        Name = "BillStatus",
                        Value = "关闭",
                        DisplayType = "notIn" // 排除关闭状态
                    });
                }
            };
            //查询完成后，在返回页面前可对查询的数据进行操作
            GetPageDataOnExecuted = (PageGridData<OCP_OrderTracking> grid) =>
            {
                //可对查询的结果的数据操作
                List<OCP_OrderTracking> list = grid.rows;

                // 构建PrepareMtrl字段
                BuildPrepareMtrlField(list);

                // 应用预警标记
                ApplyAlertWarningToData(list);
            };

            //EF:查询table界面显示合计（需要与前端开发文档上的【table显示合计】一起使用）
            SummaryExpress = (IQueryable<OCP_OrderTracking> queryable) =>
            {
                return queryable.GroupBy(x => 1).Select(x => new
                {
                    //注意大小写和数据库字段大小写一样
                    InstockQty = x.Sum(o => o.InstockQty ?? 0),
                    UnInstockQty = x.Sum(o => o.UnInstockQty ?? 0),
                    OrderQty = x.Sum(o => o.OrderQty ?? 0),
                    Amount = x.Sum(o => o.Amount ?? 0)
                })
                .FirstOrDefault();
            };
            return base.GetPageData(options);
        }

        // 在此处添加OCP_OrderTracking特有的业务逻辑方法
        // 例如：订单状态更新、数据统计、业务规则验证等

        /// <summary>
        /// 获取近14天订单完成统计数据
        /// </summary>
        /// <param name="scheduleMonth">排产月份（格式：yyyy-MM）</param>
        /// <returns>近14天订单完成统计数据</returns>
        public async Task<List<object>> GetOrderCompletionStatsAsync(string scheduleMonth)
        {
            try
            {
                var result = new List<object>();
                var endDate = DateTime.Now.Date;
                var startDate = endDate.AddDays(-13); // 近14天

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // 查询当天的订单数据，根据排产月份过滤
                    var query = _repository.FindAsIQueryable(x => 
                        x.ProScheduleYearMonth == scheduleMonth &&
                        x.CreateDate.HasValue && 
                        x.CreateDate.Value.Date <= date);

                    // 对销售订单号去重计算总任务订单数
                    var totalOrders = await query
                        .Where(x => !string.IsNullOrEmpty(x.SOBillNo))
                        .Select(x => x.SOBillNo)
                        .Distinct()
                        .CountAsync();

                    // 计算已完成的订单数（订单完成状态=已完成）
                    var completedOrders = await query
                        .Where(x => !string.IsNullOrEmpty(x.SOBillNo) && x.FinishStatus == "已完成")
                        .Select(x => x.SOBillNo)
                        .Distinct()
                        .CountAsync();

                    // 计算完成率
                    var completionRate = totalOrders > 0 ? Math.Round((decimal)completedOrders * 100 / totalOrders, 0) : 0;

                    result.Add(new
                    {
                        date = date.ToString("yyyy.MM.dd"),
                        总任务订单数 = totalOrders,
                        总任务完成率 = completionRate.ToString()+"%",
                        任务完成数 = completedOrders
                    });
                }

                return result.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单完成统计数据时发生异常，排产月份：{ScheduleMonth}", scheduleMonth);
                throw;
            }
        }

        /// <summary>
        /// 构建PrepareMtrl字段
        /// 根据计划跟踪号查询缺料运算结果，构建备料情况字段
        /// </summary>
        /// <param name="orderTrackingList">订单跟踪列表</param>
        private void BuildPrepareMtrlField(List<OCP_OrderTracking> orderTrackingList)
        {
            try
            {
                if (orderTrackingList == null || !orderTrackingList.Any())
                {
                    return;
                }

                // 获取所有有效的计划跟踪号
                var mtoNos = orderTrackingList
                    .Where(x => !string.IsNullOrWhiteSpace(x.MtoNo))
                    .Select(x => x.MtoNo)
                    .Distinct()
                    .ToList();

                if (!mtoNos.Any())
                {
                    // 如果没有计划跟踪号，设置所有记录的PrepareMtrl为空
                    foreach (var item in orderTrackingList)
                    {
                        item.PrepareMtrl = "";
                    }
                    return;
                }

                // 首先获取默认的运算方案
                var defaultPlan = _lackMtrlPlanRepository
                    .FindAsIQueryable(x => x.IsDefault == 1)
                    .OrderByDescending(x => x.CreateDate)
                    .FirstOrDefault();

                // 查询缺料运算结果，分别获取有单据编号和无单据编号的记录
                List<dynamic> lackMtrlResultsWithBill = new List<dynamic>();
                List<dynamic> lackMtrlResultsWithoutBill = new List<dynamic>();

                if (defaultPlan != null)
                {
                    // 查询有单据编号的缺料运算结果，按计划跟踪号分组获取单据分类
                    lackMtrlResultsWithBill = _lackMtrlResultRepository
                        .FindAsIQueryable(x => x.ComputeID == defaultPlan.ComputeID &&
                                             mtoNos.Contains(x.MtoNo) &&
                                             !string.IsNullOrEmpty(x.BillNo) &&
                                             !string.IsNullOrEmpty(x.BillType))
                        .GroupBy(x => x.MtoNo)
                        .Select(g => new
                        {
                            MtoNo = g.Key,
                            BillTypes = g.Select(x => x.BillType).Distinct().ToList()
                        })
                        .ToList<dynamic>();

                    // 查询无单据编号的缺料运算结果
                    lackMtrlResultsWithoutBill = _lackMtrlResultRepository
                        .FindAsIQueryable(x => x.ComputeID == defaultPlan.ComputeID &&
                                             mtoNos.Contains(x.MtoNo) &&
                                             string.IsNullOrEmpty(x.BillNo))
                        .GroupBy(x => x.MtoNo)
                        .Select(g => new
                        {
                            MtoNo = g.Key,
                            HasRecords = true
                        })
                        .ToList<dynamic>();

                    _logger.LogInformation("使用默认运算方案查询缺料结果，方案ID：{ComputeID}，方案名称：{PlanName}",
                        defaultPlan.ComputeID, defaultPlan.PlanName);
                }
                else
                {
                    _logger.LogWarning("未找到默认的缺料运算方案，将使用默认备料类型");
                }

                // 为每个订单跟踪记录构建PrepareMtrl字段
                foreach (var orderTracking in orderTrackingList)
                {
                    if (string.IsNullOrWhiteSpace(orderTracking.MtoNo))
                    {
                        orderTracking.PrepareMtrl = "";
                        continue;
                    }

                    var prepareMtrlList = new List<string>();

                    // 1. 根据计划跟踪号查找缺料运算结果表，当存在单据编号+单据分类判断："外购"，"金工"，"委外"，"部件"
                    var lackResultWithBill = lackMtrlResultsWithBill.FirstOrDefault(x => x.MtoNo == orderTracking.MtoNo);
                    if (lackResultWithBill != null && lackResultWithBill.BillTypes != null && ((List<string>)lackResultWithBill.BillTypes).Count > 0)
                    {
                        foreach (var billType in (List<string>)lackResultWithBill.BillTypes)
                        {
                            switch (billType)
                            {
                                case "标准采购":
                                    if (!prepareMtrlList.Contains("外购"))
                                        prepareMtrlList.Add("外购");
                                    break;
                                case "标准委外":
                                    if (!prepareMtrlList.Contains("委外"))
                                        prepareMtrlList.Add("委外");
                                    break;
                                case "金工车间":
                                    if (!prepareMtrlList.Contains("金工"))
                                        prepareMtrlList.Add("金工");
                                    break;
                                case "部件车间":
                                    if (!prepareMtrlList.Contains("部件"))
                                        prepareMtrlList.Add("部件");
                                    break;
                            }
                        }
                    }

                    // 2. 根据计划跟踪号查找缺料运算结果表，当无单据编号判断为："计划"
                    var lackResultWithoutBill = lackMtrlResultsWithoutBill.FirstOrDefault(x => x.MtoNo == orderTracking.MtoNo);
                    if (lackResultWithoutBill != null && lackResultWithoutBill.HasRecords)
                    {
                        if (!prepareMtrlList.Contains("计划"))
                            prepareMtrlList.Add("计划");
                    }

                    // 3. 根据订单跟踪表是否有"BOM创建日期"，无日期的显示："技术"，有日期就不显示
                    if (!orderTracking.BomCreateDate.HasValue)
                    {
                        if (!prepareMtrlList.Contains("技术"))
                            prepareMtrlList.Add("技术");
                    }

                    // 4. 根据订单跟踪表是否有"计划确认日期"，无日期的显示："待计划确认"，有日期就不显示
                    if (!orderTracking.PlanConfirmDate.HasValue)
                    {
                        if (!prepareMtrlList.Contains("待计划确认"))
                            prepareMtrlList.Add("待计划确认");
                    }

                    // 5. 以上1-4都无值时，显示"备料完成"
                    if (prepareMtrlList.Count == 0)
                    {
                        prepareMtrlList.Add("备料完成");
                    }

                    // 构建最终的PrepareMtrl字段值，格式：["外购","委外","部件","金工","计划","技术","待计划确认","备料完成"] 或空字符串
                    orderTracking.PrepareMtrl = prepareMtrlList.Any()
                        ? $"[{string.Join(",", prepareMtrlList.Select(x => $"\"{x}\""))}]"
                        : "";
                }

                _logger.LogInformation("成功构建PrepareMtrl字段，处理了{OrderCount}条订单跟踪记录，涉及{MtoCount}个计划跟踪号",
                    orderTrackingList.Count, mtoNos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "构建PrepareMtrl字段时发生异常");

                // 异常情况下，设置所有记录的PrepareMtrl为空，避免影响其他功能
                foreach (var item in orderTrackingList)
                {
                    item.PrepareMtrl = "";
                }
            }
        }

        /// <summary>
        /// 获取总任务完成统计数据
        /// </summary>
        /// <returns>总任务完成统计数据</returns>
        public async Task<object[]> GetTaskCompletionSummary()
        {
            try
            {
                _logger.LogInformation("开始获取总任务完成统计数据");

                // 查询是否关联总任务=是的记录，按SOEntryID字段统计
                var taskQuery = _repository.FindAsIQueryable(x => x.IsJoinTask == 1 && x.SOEntryID.HasValue);

                // 统计总任务数（按SOEntryID去重）
                var totalTaskCount = await taskQuery
                    .Select(x => x.SOEntryID.Value)
                    .Distinct()
                    .CountAsync();

                // 统计任务完成数（条件：FinishStatus=已完成，按SOEntryID去重）
                var completedTaskCount = await taskQuery
                    .Where(x => x.FinishStatus == "已完成")
                    .Select(x => x.SOEntryID.Value)
                    .Distinct()
                    .CountAsync();

                // 计算总任务完成率（百分比，保留整数）
                var completionRate = totalTaskCount > 0 
                    ? Math.Round((decimal)completedTaskCount * 100 / totalTaskCount, 0) 
                    : 0;

                // 按照指定格式返回结果
                var result = new object[]
                {
                    new { name = "总任务数", value = totalTaskCount },
                    new { name = "任务完成数", value = completedTaskCount },
                    new { name = "总任务完成率", value = (int)completionRate }
                };

                _logger.LogInformation($"获取总任务完成统计数据完成，总任务数: {totalTaskCount}，任务完成数: {completedTaskCount}，完成率: {completionRate}%");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取总任务完成统计数据异常");
                throw; // 重新抛出异常，让控制器处理
            }
        }

        /// <summary>
        /// 应用预警标记到数据列表
        /// </summary>
        /// <param name="list">数据列表</param>
        private void ApplyAlertWarningToData(List<OCP_OrderTracking> list)
        {
            try
            {
                var rules = _alertRulesRepository.FindAsync(x =>
                    x.AlertPage == "OCP_OrderTracking" &&
                    x.TaskStatus == 1).GetAwaiter().GetResult();

                if (rules != null && rules.Any())
                {
                    AlertWarningHelper.ApplyAlertWarning(list, rules.ToList(), _logger);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "应用预警标记时发生异常");
            }
        }
    }
}