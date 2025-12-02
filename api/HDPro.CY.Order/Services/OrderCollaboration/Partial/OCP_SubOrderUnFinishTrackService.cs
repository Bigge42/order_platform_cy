/*
 *所有关于OCP_SubOrderUnFinishTrack类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_SubOrderUnFinishTrackService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using HDPro.Core.BaseProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.Services.Common;
using Microsoft.Extensions.Logging;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_SubOrderUnFinishTrackService
    {
        private readonly IOCP_SubOrderUnFinishTrackRepository _repository;//访问数据库
        private readonly IOCP_AlertRulesRepository _alertRulesRepository;//预警规则Repository
        private readonly ILogger<OCP_SubOrderUnFinishTrackService> _logger;//日志记录器

        [ActivatorUtilitiesConstructor]
        public OCP_SubOrderUnFinishTrackService(
            IOCP_SubOrderUnFinishTrackRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IOCP_AlertRulesRepository alertRulesRepository,
            ILogger<OCP_SubOrderUnFinishTrackService> logger
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
        /// 可在此处添加OCP_SubOrderUnFinishTrack特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_SubOrderUnFinishTrack特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_SubOrderUnFinishTrack特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_SubOrderUnFinishTrack entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_SubOrderUnFinishTrack特有的数据验证逻辑
            
            return response;
        }

        /// <summary>
        /// 重写GetPageData方法，实现Table表合计功能和过滤字段逻辑
        /// </summary>
        public override PageGridData<OCP_SubOrderUnFinishTrack> GetPageData(PageDataOptions options)
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
            GetPageDataOnExecuted = (PageGridData<OCP_SubOrderUnFinishTrack> grid) =>
            {
                //对查询结果进行供应商字段权限过滤
                if (grid.rows != null && grid.rows.Any())
                {
                    ApplySupplierFieldsFilter(grid.rows);

                    // 应用预警标记
                    ApplyAlertWarningToData(grid.rows);
                }
            };

            //EF:查询table界面显示合计（需要与前端开发文档上的【table显示合计】一起使用）
            SummaryExpress = (IQueryable<OCP_SubOrderUnFinishTrack> queryable) =>
            {
                return queryable.GroupBy(x => 1).Select(x => new
                {
                    //注意大小写和数据库字段大小写一样
                    InstockQty = x.Sum(o => o.InstockQty ?? 0)
                })
                .FirstOrDefault();
            };
            return base.GetPageData(options);
        }

        /// <summary>
        /// 重写导出方法，应用供应商字段权限过滤
        /// </summary>
        /// <param name="pageData">导出参数</param>
        /// <returns>导出结果</returns>
        public override WebResponseContent Export(PageDataOptions pageData)
        {
            //设置最大导出的数量
            Limit = 10000;

            //查询要导出的数据后，在生成excel文件前处理
            //list导出的实体，ignore过滤不导出的字段
            ExportOnExecuting = (List<OCP_SubOrderUnFinishTrack> list, List<string> ignore) =>
            {
                // 如果用户没有供应商字段权限，则对导出数据进行字段脱敏处理
                if (!HasSupplierFieldPermission())
                {
                    // 对导出数据进行供应商字段脱敏
                    ApplySupplierFieldsFilter(list);

                    // 将供应商相关字段添加到忽略列表中，避免在Excel中显示这些列
                    var supplierFields = new[] { "SupplierCode", "SupplierName" };
                    foreach (var field in supplierFields)
                    {
                        if (!ignore.Contains(field))
                        {
                            ignore.Add(field);
                        }
                    }
                }

                return new WebResponseContent(true);
            };

            return base.Export(pageData);
        }

        /// <summary>
        /// 对查询结果应用供应商字段权限过滤
        /// 在GetPageDataOnExecuted中调用，对已查询的数据进行字段脱敏处理
        /// </summary>
        /// <param name="dataList">查询结果数据列表</param>
        private static void ApplySupplierFieldsFilter(List<OCP_SubOrderUnFinishTrack> dataList)
        {
            // 如果用户没有供应商字段权限，则对供应商相关字段进行脱敏处理
            if (!HasSupplierFieldPermission())
            {
                foreach (var item in dataList)
                {
                    // 供应商相关字段设为脱敏值
                    item.SupplierCode = "***";
                    item.SupplierName = "***"; // 显示为星号表示无权限查看
                }
            }
        }

        /// <summary>
        /// 检查当前用户是否有权限查看供应商相关字段
        /// </summary>
        /// <returns>是否有权限</returns>
        private static bool HasSupplierFieldPermission()
        {
            // 超级管理员有所有权限
            if (UserContext.Current.IsSuperAdmin)
            {
                return true;
            }

            // 检查用户是否具有指定角色（例如：物资供应中心相关角色）
            // 这里可以根据实际需求配置具体的角色ID
            var authorizedRoleIds = new int[] { 35 }; // 示例：有权限查看供应商字段的角色ID列表

            return UserContext.Current.RoleIds.Any(roleId => authorizedRoleIds.Contains(roleId));
        }

        /// <summary>
        /// 应用预警标记到数据列表
        /// </summary>
        /// <param name="list">数据列表</param>
        private void ApplyAlertWarningToData(List<OCP_SubOrderUnFinishTrack> list)
        {
            try
            {
                var rules = _alertRulesRepository.FindAsync(x =>
                    x.AlertPage == "OCP_SubOrderUnFinishTrack" &&
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