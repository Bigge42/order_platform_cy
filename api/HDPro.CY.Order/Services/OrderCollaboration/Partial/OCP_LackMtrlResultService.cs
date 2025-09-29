/*
 *所有关于OCP_LackMtrlResult类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_LackMtrlResultService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using HDPro.Core.BaseProvider;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_LackMtrlResultService
    {
        private readonly IOCP_LackMtrlResultRepository _repository;//访问数据库
        private readonly IOCP_LackMtrlPlanRepository _planRepository;//访问缺料计划数据库
        private readonly ILogger<OCP_LackMtrlResultService> _logger;//日志记录器

        [ActivatorUtilitiesConstructor]
        public OCP_LackMtrlResultService(
            IOCP_LackMtrlResultRepository dbRepository,
            IOCP_LackMtrlPlanRepository planRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OCP_LackMtrlResultService> logger
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _planRepository = planRepository;
            _logger = logger;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_LackMtrlResult特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_LackMtrlResult特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_LackMtrlResult特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_LackMtrlResult entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_LackMtrlResult特有的数据验证逻辑
            
            return response;
        }

        //查询(主表合计)
        public override PageGridData<OCP_LackMtrlResult> GetPageData(PageDataOptions options)
        {
            //查询完成后，在返回页面前可对查询的数据进行操作
            GetPageDataOnExecuted = (PageGridData<OCP_LackMtrlResult> grid) =>
            {
                // 检查是否需要对供应商字段进行权限控制
                if (grid.rows != null && grid.rows.Count > 0 && ShouldApplySupplierFieldFilter(options))
                {
                    ApplySupplierFieldsFilter(grid.rows);
                }
            };

            //EF:查询table界面显示合计（需要与前端开发文档上的【table显示合计】一起使用）
            SummaryExpress = (IQueryable<OCP_LackMtrlResult> queryable) =>
            {
                return queryable.GroupBy(x => 1).Select(x => new
                {
                    //注意大小写和数据库字段大小写一样
                    NeedQty = x.Sum(o => o.NeedQty ?? 0),
                    InventoryQty = x.Sum(o => o.InventoryQty ?? 0),
                    PlanedQty = x.Sum(o => o.PlanedQty ?? 0),
                    UnPlanedQty = x.Sum(o => o.UnPlanedQty ?? 0)
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
            ExportOnExecuting = (List<OCP_LackMtrlResult> list, List<string> ignore) =>
            {
                // 检查是否需要对供应商字段进行权限控制
                if (ShouldApplySupplierFieldFilter(pageData))
                {
                    // 对导出数据进行供应商字段脱敏
                    ApplySupplierFieldsFilter(list);

                    // 将供应商相关字段添加到忽略列表中，避免在Excel中显示这些列
                    var supplierFields = new[] { "SupplierName" };
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
        /// 检查是否需要对供应商字段进行权限控制
        /// 根据CustomerParams中的BusinessType参数判断
        /// </summary>
        /// <param name="options">查询参数</param>
        /// <returns>是否需要进行供应商字段权限控制</returns>
        private static bool ShouldApplySupplierFieldFilter(PageDataOptions options)
        {
            // 检查CustomerParams中是否包含BusinessType="PO"
            if (options?.CustomerParams != null &&
                options.CustomerParams.TryGetValue("BusinessType", out var businessType))
            {
                return businessType?.ToString()?.Equals("PO", StringComparison.OrdinalIgnoreCase) == true;
            }

            return false;
        }

        /// <summary>
        /// 对查询结果应用供应商字段权限过滤
        /// </summary>
        /// <param name="dataList">查询结果数据列表</param>
        private static void ApplySupplierFieldsFilter(List<OCP_LackMtrlResult> dataList)
        {
            // 如果用户没有供应商字段权限，则对供应商相关字段进行脱敏处理
            if (!HasSupplierFieldPermission())
            {
                foreach (var item in dataList)
                {
                    // 供应商相关字段设为脱敏值
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
            var authorizedRoleIds = new int[] { 35 }; // 有权限查看供应商字段的角色ID列表

            return UserContext.Current.RoleIds.Any(roleId => authorizedRoleIds.Contains(roleId));
        }

        /// <summary>
        /// 获取默认运算方案的缺料情况统计
        /// </summary>
        /// <returns>按单据类型分组的缺料统计</returns>
        public async Task<object[]> GetDefaultLackMtrlSummary()
        {
            try
            {
                // 首先获取默认运算方案的ComputeID
                var defaultPlan = await _planRepository.FindAsIQueryable(x => x.IsDefault == 1)
                    .FirstOrDefaultAsync();

                if (defaultPlan == null)
                {
                    // 如果没有默认方案，返回空数据
                    return new object[]
                    {
                        new { value = 0, name = "采购缺料" },
                        new { value = 0, name = "委外缺料" },
                        new { value = 0, name = "金工缺料" },
                        new { value = 0, name = "部件缺料" }
                    };
                }

                // 获取默认运算方案的缺料数据，先对(计划跟踪号+物料编码)去重，然后计算总需求减去总库存
                var lackMtrlData = await _repository.FindAsIQueryable(x => x.ComputeID == defaultPlan.ComputeID)
                    .Select(x => new
                    {
                        x.MtoNo,              // 计划跟踪号
                        x.MaterialNumber,     // 物料编码
                        x.BillType,
                        x.NeedQty,            // 需求数量
                        x.InventoryQty,       // 库存数量
                        x.ErpClsid,           // 物料属性
                        x.MaterialCategory,   // 物料分类
                        x.SupplierName        // 供应商名称
                    })
                    .ToListAsync();

                // 先对(BillType+物料编码+计划跟踪号)去重，汇总需求数量和库存数量
                var groupedData = lackMtrlData
                    .Where(x => !string.IsNullOrEmpty(x.MtoNo) && !string.IsNullOrEmpty(x.MaterialNumber) && !string.IsNullOrEmpty(x.BillType))
                    .GroupBy(x => new { x.BillType, x.MaterialNumber, x.MtoNo })
                    .Select(g => new
                    {
                        BillType = g.Key.BillType,
                        MaterialNumber = g.Key.MaterialNumber,
                        MtoNo = g.Key.MtoNo,
                        TotalNeedQty = g.Sum(x => x.NeedQty ?? 0),
                        TotalInventoryQty = g.Sum(x => x.InventoryQty ?? 0),
                        ErpClsid = g.First().ErpClsid,
                        MaterialCategory = g.First().MaterialCategory,
                        SupplierName = g.First().SupplierName
                    })
                    .Where(x => x.TotalNeedQty > x.TotalInventoryQty) // 只统计缺料的记录（总需求 > 总库存）
                    .Select(x => new
                    {
                        x.BillType,
                        LackQty = x.TotalNeedQty - x.TotalInventoryQty, // 缺料数量 = 总需求 - 总库存
                        x.ErpClsid,
                        x.MaterialCategory,
                        x.SupplierName
                    })
                    .ToList();

                // 初始化各类型缺料数量
                var purchaseLack = 0m;  // 采购缺料
                var outsourceLack = 0m; // 委外缺料
                var metalworkLack = 0m; // 金工缺料
                var partLack = 0m;      // 部件缺料

                foreach (var item in groupedData)
                {
                    var billType = item.BillType?.Trim();
                    var lackQty = item.LackQty;

                    switch (billType)
                    {
                        case "标准采购":
                            purchaseLack += lackQty;
                            break;
                        case "标准委外":
                            outsourceLack += lackQty;
                            break;
                        case "金工车间":
                            metalworkLack += lackQty;
                            break;
                        case "部件车间":
                            partLack += lackQty;
                            break;
                        case "其他":
                        default:
                            // 其他类型归入部件缺料
                            partLack += lackQty;
                            break;
                    }
                }

                // 按照指定顺序返回结果
                return new object[]
                {
                    new { value = purchaseLack, name = "采购缺料" },
                    new { value = outsourceLack, name = "委外缺料" },
                    new { value = metalworkLack, name = "金工缺料" },
                    new { value = partLack, name = "部件缺料" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取默认运算方案缺料统计失败");
                throw; // 重新抛出异常，让控制器处理
            }
        }

        /// <summary>
        /// 获取近7天的缺料情况统计
        /// </summary>
        /// <returns>近7天每日缺料统计数据</returns>
        public async Task<object[]> GetLast7DaysLackMtrlTrend()
        {
            try
            {
                // 计算近7天的日期范围
                var endDate = DateTime.Now.Date;
                var startDate = endDate.AddDays(-6); // 包含今天共7天

                // 获取近7天的运算方案，排除方案名包含"全量"的方案
                var dailyPlans = await _planRepository.FindAsIQueryable(x => x.CreateDate >= startDate && x.CreateDate <= endDate.AddDays(1))
                    .Where(x => !x.PlanName.Contains("全量")) // 排除方案名包含"全量"的方案
                    .Select(x => new
                    {
                        x.ComputeID,
                        x.PlanName,
                        Date = x.CreateDate.Value.Date,
                        CreateTime = x.CreateDate.Value
                    })
                    .ToListAsync();

                // 按日期分组统计
                var dailyStats = new List<object>();

                for (int i = 0; i < 7; i++)
                {
                    var currentDate = startDate.AddDays(i);
                    
                    // 获取当天的运算方案，如果有多个非全量方案，只取最后一个
                    var dayPlans = dailyPlans.Where(x => x.Date == currentDate).ToList();
                    var selectedPlan = dayPlans.OrderByDescending(x => x.CreateTime).FirstOrDefault();
                    
                    var selectedPlanId = selectedPlan?.ComputeID;

                    // 初始化各类型缺料数量
                    var purchaseLack = 0m;  // 采购缺料
                    var outsourceLack = 0m; // 委外缺料
                    var metalworkLack = 0m; // 金工缺料
                    var partLack = 0m;      // 部件缺料

                    if (selectedPlanId.HasValue)
                    {
                        // 获取选中运算方案的缺料数据
                        var dayLackMtrlData = await _repository.FindAsIQueryable(x => x.ComputeID == selectedPlanId.Value)
                            .Select(x => new
                            {
                                x.MtoNo,              // 计划跟踪号
                                x.MaterialNumber,     // 物料编码
                                x.BillType,
                                x.NeedQty,            // 需求数量
                                x.InventoryQty,       // 库存数量
                                x.ErpClsid,
                                x.MaterialCategory,
                                x.SupplierName
                            })
                            .ToListAsync();

                        // 先对(BillType+物料编码+计划跟踪号)去重，汇总需求数量和库存数量
                        var groupedDayData = dayLackMtrlData
                            .Where(x => !string.IsNullOrEmpty(x.MtoNo) && !string.IsNullOrEmpty(x.MaterialNumber) && !string.IsNullOrEmpty(x.BillType))
                            .GroupBy(x => new { x.BillType, x.MaterialNumber, x.MtoNo })
                            .Select(g => new
                            {
                                BillType = g.Key.BillType,
                                MaterialNumber = g.Key.MaterialNumber,
                                MtoNo = g.Key.MtoNo,
                                TotalNeedQty = g.Sum(x => x.NeedQty ?? 0),
                                TotalInventoryQty = g.Sum(x => x.InventoryQty ?? 0),
                                ErpClsid = g.First().ErpClsid,
                                MaterialCategory = g.First().MaterialCategory,
                                SupplierName = g.First().SupplierName
                            })
                            .Where(x => x.TotalNeedQty > x.TotalInventoryQty) // 只统计缺料的记录
                            .Select(x => new
                            {
                                x.BillType,
                                LackQty = x.TotalNeedQty - x.TotalInventoryQty, // 缺料数量 = 总需求 - 总库存
                                x.ErpClsid,
                                x.MaterialCategory,
                                x.SupplierName
                            })
                            .ToList();

                        foreach (var item in groupedDayData)
                        {
                            var billType = item.BillType?.Trim();
                            var lackQty = item.LackQty;

                            switch (billType)
                            {
                                case "标准采购":
                                    purchaseLack += lackQty;
                                    break;
                                case "标准委外":
                                    outsourceLack += lackQty;
                                    break;
                                case "金工车间":
                                    metalworkLack += lackQty;
                                    break;
                                case "部件车间":
                                    partLack += lackQty;
                                    break;
                                case "其他":
                                default:
                                    // 其他类型归入部件缺料
                                    partLack += lackQty;
                                    break;
                            }
                        }
                    }

                    var totalLack = purchaseLack + outsourceLack + metalworkLack + partLack;

                    dailyStats.Add(new
                    {
                        date = currentDate.ToString("yyyy.MM.dd"),
                        总缺料数 = (int)totalLack,
                        采购缺料 = (int)purchaseLack,
                        委外缺料 = (int)outsourceLack,
                        金工缺料 = (int)metalworkLack,
                        部件缺料 = (int)partLack
                    });
                }

                return dailyStats.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取近7天缺料趋势失败");
                throw; // 重新抛出异常，让控制器处理
            }
        }

        // ESB相关功能已迁移到独立的ESB架构中
        // 如需ESB同步功能，请使用 LackMtrlResultESBSyncCoordinator
        // 调用方式：await lackMtrlResultCoordinator.ExecuteSyncByComputeId(computeId);
    }
} 