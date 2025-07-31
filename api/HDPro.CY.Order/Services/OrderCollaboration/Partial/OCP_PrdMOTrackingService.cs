/*
 *所有关于OCP_PrdMOTracking类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_PrdMOTrackingService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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

namespace HDPro.CY.Order.Services
{
    public partial class OCP_PrdMOTrackingService
    {
        private readonly IOCP_PrdMOTrackingRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public OCP_PrdMOTrackingService(
            IOCP_PrdMOTrackingRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_PrdMOTracking特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_PrdMOTracking特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_PrdMOTracking特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_PrdMOTracking entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_PrdMOTracking特有的数据验证逻辑
            
            return response;
        }

        /// <summary>
        /// 重写GetPageData方法，实现Table表合计功能、超期天数计算和过滤字段逻辑
        /// </summary>
        public override PageGridData<OCP_PrdMOTracking> GetPageData(PageDataOptions options)
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

            //EF:查询table界面显示合计（需要与前端开发文档上的【table显示合计】一起使用）
            SummaryExpress = (IQueryable<OCP_PrdMOTracking> queryable) =>
            {
                return queryable.GroupBy(x => 1).Select(x => new
                {
                    //注意大小写和数据库字段大小写一样
                    ProductionQty = x.Sum(o => o.ProductionQty ?? 0),
                    InboundQty = x.Sum(o => o.InboundQty ?? 0)
                })
                .FirstOrDefault();
            };

            // 查询完成后，在返回页面前对查询的数据进行操作
            GetPageDataOnExecuted = (PageGridData<OCP_PrdMOTracking> grid) =>
            {
                // 可对查询的结果的数据操作
                List<OCP_PrdMOTracking> trackingRecords = grid.rows;

                // 计算每条记录的超期天数
                foreach (var record in trackingRecords)
                {
                    record.OverdueDays = CalculateOverdueDays(record);
                }
            };

            return base.GetPageData(options);
        }

        /// <summary>
        /// 计算超期天数
        /// </summary>
        /// <param name="record">整机跟踪记录</param>
        /// <returns>超期天数</returns>
        private int CalculateOverdueDays(OCP_PrdMOTracking record)
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
            catch (Exception)
            {
                // 记录异常但不影响其他数据
                return 0;
            }
        }

        /// <summary>
        /// 判断生产订单是否已完工
        /// 根据生产订单状态=完工、结案、结算 表示已完工，否则未完工
        /// </summary>
        /// <param name="billStatus">生产订单状态</param>
        /// <returns>是否已完工</returns>
        private static bool IsOrderCompleted(string billStatus)
        {
            if (string.IsNullOrWhiteSpace(billStatus))
            {
                return false;
            }

            // 根据业务规则：完工、结案、结算 表示已完工
            var completedStatuses = new[] { "完工", "结案", "结算" };
            return completedStatuses.Contains(billStatus.Trim());
        }
  }
} 