/*
 *所有关于OCP_JGUnFinishTrack类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_JGUnFinishTrackService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using HDPro.Core.ManageUser;
using HDPro.CY.Order.Services.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_JGUnFinishTrackService
    {
        private readonly IOCP_JGUnFinishTrackRepository _repository;//访问数据库
        private readonly IOCP_AlertRulesRepository _alertRulesRepository;//预警规则Repository
        private readonly ILogger<OCP_JGUnFinishTrackService> _logger;//日志记录器

        [ActivatorUtilitiesConstructor]
        public OCP_JGUnFinishTrackService(
            IOCP_JGUnFinishTrackRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IOCP_AlertRulesRepository alertRulesRepository,
            ILogger<OCP_JGUnFinishTrackService> logger
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _alertRulesRepository = alertRulesRepository;
            _logger = logger;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_JGUnFinishTrack特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_JGUnFinishTrack特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_JGUnFinishTrack特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_JGUnFinishTrack entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_JGUnFinishTrack特有的数据验证逻辑
            
            return response;
        }

        /// <summary>
        /// 重写GetPageData方法以实现表格汇总功能、超期天数计算和过滤字段逻辑
        /// </summary>
        /// <param name="pageData">页面数据</param>
        /// <returns>包含汇总信息的页面数据</returns>
        public override PageGridData<OCP_JGUnFinishTrack> GetPageData(PageDataOptions pageData)
        {
            //此处是从前台提交的原生的查询条件，这里可以自己过滤
            QueryRelativeList = (List<SearchParameters> parameters) =>
            {
                // 超级管理员不添加过滤条件
                if (UserContext.Current.IsSuperAdmin)
                {
                    return;
                }

                // 添加默认查询条件：根据生产订单状态排除【完工、结案、结算】
                bool hasProductionOrderStatusCondition = parameters.Any(p => p.Name == "BillStatus");
                if (!hasProductionOrderStatusCondition)
                {
                    parameters.Add(new SearchParameters
                    {
                        Name = "BillStatus",
                        Value = "完工,结案,结算",
                        DisplayType = "notIn" // 排除完工、结案、结算状态
                    });
                }
            };

            // 设置SummaryExpress委托用于页面表格的合计功能
            SummaryExpress = (IQueryable<OCP_JGUnFinishTrack> queryable) =>
            {
                return queryable.GroupBy(x => 1).Select(x => new
                {
                    ProductionQty = x.Sum(o => o.ProductionQty ?? 0),
                    InboundQty = x.Sum(o => o.InboundQty ?? 0)
                }).FirstOrDefault();
            };

            // 查询完成后，在返回页面前对查询的数据进行操作
            GetPageDataOnExecuted = (PageGridData<OCP_JGUnFinishTrack> grid) =>
            {
                // 可对查询的结果的数据操作
                List<OCP_JGUnFinishTrack> trackingRecords = grid.rows;

                // 计算每条记录的超期天数
                foreach (var record in trackingRecords)
                {
                    record.OverdueDays = CalculateOverdueDays(record);
                }

                // 应用预警标记
                ApplyAlertWarningToData(trackingRecords);
            };

            return base.GetPageData(pageData);
        }

        /// <summary>
        /// 计算超期天数
        /// </summary>
        /// <param name="record">金工跟踪记录</param>
        /// <returns>超期天数</returns>
        private int CalculateOverdueDays(OCP_JGUnFinishTrack record)
        {
            try
            {
                // 如果没有计划完工日期，无法计算超期天数
                if (!record.PlanCompleteDate.HasValue)
                {
                    return 0;
                }

                DateTime planCompleteDate = record.PlanCompleteDate.Value;
                DateTime compareDate;

                // 判断是否已完工：根据生产订单状态判断
                bool isCompleted = IsOrderCompleted(record.BillStatus);

                if (isCompleted && record.CompleteDate.HasValue)
                {
                    // 已完工：实际完工日期 - 计划完工日期
                    compareDate = record.CompleteDate.Value;
                }
                else
                {
                    // 未完工：当前日期 - 计划完工日期
                    compareDate = DateTime.Now;
                }

                // 计算天数差
                int daysDiff = (int)(compareDate.Date - planCompleteDate.Date).TotalDays;

                // 超期天数大于等于0
                return Math.Max(0, daysDiff);
            }
            catch (Exception ex)
            {
                // 记录异常但不影响其他数据
                // 可以考虑添加日志记录
                return 0;
            }
        }

        /// <summary>
        /// 判断生产订单是否已完工
        /// 根据生产订单状态=完工、结案、结算 表示已完工，否则未完工
        /// </summary>
        /// <param name="billStatus">生产订单状态</param>
        /// <returns>是否已完工</returns>
        private bool IsOrderCompleted(string billStatus)
        {
            if (string.IsNullOrWhiteSpace(billStatus))
            {
                return false;
            }

            // 根据业务规则：完工、结案、结算 表示已完工
            var completedStatuses = new[] { "完工", "结案", "结算" };
            return completedStatuses.Contains(billStatus.Trim());
        }

        /// <summary>
        /// 应用预警标记到数据列表
        /// </summary>
        /// <param name="list">数据列表</param>
        private void ApplyAlertWarningToData(List<OCP_JGUnFinishTrack> list)
        {
            try
            {
                var rules = _alertRulesRepository.FindAsync(x =>
                    x.AlertPage == "OCP_JGUnFinishTrack" &&
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