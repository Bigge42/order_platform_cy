/*
 *所有关于OCP_Customer类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_CustomerService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using HDPro.CY.Order.Services.K3Cloud;
using HDPro.CY.Order.Services.K3Cloud.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.IServices;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_CustomerService
    {
        private readonly IOCP_CustomerRepository _repository;//访问数据库
        private readonly IK3CloudService _k3CloudService;//K3Cloud服务
        private readonly ILogger<OCP_CustomerService> _logger;//日志服务

        [ActivatorUtilitiesConstructor]
        public OCP_CustomerService(
            IOCP_CustomerRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IK3CloudService k3CloudService,
            ILogger<OCP_CustomerService> logger
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _k3CloudService = k3CloudService;
            _logger = logger;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_Customer特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_Customer特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目特有的实体验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_Customer entity)
        {
            // 在此处添加OCP_Customer特有的验证逻辑
            if (string.IsNullOrEmpty(entity.Number))
            {
                return new WebResponseContent(false) { Message = "客户编码不能为空" };
            }

            if (string.IsNullOrEmpty(entity.Name))
            {
                return new WebResponseContent(false) { Message = "客户名称不能为空" };
            }

            return base.ValidateCYOrderEntity(entity);
        }

        /// <summary>
        /// 获取本地客户表最大修改时间
        /// </summary>
        /// <returns>最大修改时间</returns>
        private async Task<DateTime?> GetLocalCustomerMaxModifyDateAsync()
        {
            try
            {
                var maxDate = await _repository.FindAsIQueryable(x => x.ModifyDate.HasValue)
                    .MaxAsync(x => (DateTime?)x.ModifyDate);
                
                _logger.LogInformation($"本地客户表最大修改时间: {maxDate}");
                return maxDate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取本地客户表最大修改时间异常");
                return null;
            }
        }

        /// <summary>
        /// 构建K3Cloud查询过滤条件
        /// </summary>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <param name="includeIncrementalSync">是否包含增量同步过滤</param>
        /// <returns>完整的过滤条件</returns>
        private async Task<string> BuildK3CloudFilterStringAsync(string customFilter = null, bool includeIncrementalSync = true)
        {
            try
            {
                // 基础过滤条件：已审核且未禁用
                var baseFilter = "FDocumentStatus='C' and FForbidStatus='A'";
                var filters = new List<string> { baseFilter };

                // 增量同步过滤条件
                if (includeIncrementalSync)
                {
                    var maxModifyDate = await GetLocalCustomerMaxModifyDateAsync();
                    if (maxModifyDate.HasValue)
                    {
                        var incrementalFilter = $"FModifyDate>='{maxModifyDate.Value:yyyy-MM-dd HH:mm:ss}'";
                        filters.Add(incrementalFilter);
                        _logger.LogInformation($"使用增量同步，过滤条件: {incrementalFilter}");
                    }
                }

                // 自定义过滤条件
                if (!string.IsNullOrEmpty(customFilter))
                {
                    filters.Add($"({customFilter})");
                    _logger.LogInformation($"使用自定义过滤条件: {customFilter}");
                }

                var finalFilter = string.Join(" and ", filters);
                _logger.LogInformation($"最终过滤条件: {finalFilter}");
                return finalFilter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "构建K3Cloud查询过滤条件异常");
                return "FDocumentStatus='C' and FForbidStatus='A'";
            }
        }

        /// <summary>
        /// 从K3Cloud同步客户数据
        /// </summary>
        /// <param name="pageSize">每页处理数量</param>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncCustomersFromK3CloudAsync(int pageSize = 1000, string customFilter = null)
        {
            var startTime = DateTime.Now;
            var totalSyncedCount = 0;
            var totalErrorCount = 0;
            var allErrors = new List<string>();

            try
            {
                _logger.LogInformation("开始从K3Cloud同步客户数据");

                // 构建过滤条件
                var filterString = await BuildK3CloudFilterStringAsync(customFilter);

                // 获取K3Cloud客户总数
                var totalCount = await _k3CloudService.GetCustomerCountAsync(filterString);
                _logger.LogInformation($"K3Cloud客户总数: {totalCount}");

                if (totalCount == 0)
                {
                    return new WebResponseContent(true)
                    {
                        Message = "没有需要同步的客户数据",
                        Data = new { SyncedCount = 0, ErrorCount = 0, Errors = new List<string>() }
                    };
                }

                // 分页处理
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                _logger.LogInformation($"总页数: {totalPages}，每页: {pageSize} 条");

                for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
                {
                    try
                    {
                        _logger.LogInformation($"开始处理第 {pageIndex + 1}/{totalPages} 页数据");

                        // 获取K3Cloud客户数据
                        var k3Response = await _k3CloudService.GetCustomersAsync(pageIndex, pageSize, filterString);
                        
                        if (!k3Response.IsSuccess || k3Response.Data == null || !k3Response.Data.Any())
                        {
                            var error = $"获取第 {pageIndex + 1} 页K3Cloud客户数据失败: {k3Response.Message}";
                            _logger.LogWarning(error);
                            allErrors.Add(error);
                            totalErrorCount++;
                            continue;
                        }

                        var customers = k3Response.Data;
                        _logger.LogInformation($"获取到第 {pageIndex + 1} 页K3Cloud客户数据 {customers.Count} 条");

                        // 处理这一页的数据
                        var (syncedCount, errorCount, errors) = await ProcessCustomerPageAsync(customers);
                        
                        totalSyncedCount += syncedCount;
                        totalErrorCount += errorCount;
                        allErrors.AddRange(errors);

                        _logger.LogInformation($"第 {pageIndex + 1}/{totalPages} 页处理完成，同步: {syncedCount}，错误: {errorCount}");
                    }
                    catch (Exception ex)
                    {
                        var error = $"处理第 {pageIndex + 1} 页数据时发生异常: {ex.Message}";
                        _logger.LogError(ex, error);
                        allErrors.Add(error);
                        totalErrorCount++;
                    }
                }

                var duration = DateTime.Now - startTime;
                _logger.LogInformation($"客户数据同步完成，总计同步: {totalSyncedCount}，错误: {totalErrorCount}，耗时: {duration.TotalSeconds:F2}秒");

                return new WebResponseContent(true)
                {
                    Message = $"客户数据同步完成，成功同步 {totalSyncedCount} 条，失败 {totalErrorCount} 条，耗时 {duration.TotalSeconds:F2} 秒",
                    Data = new
                    {
                        SyncedCount = totalSyncedCount,
                        ErrorCount = totalErrorCount,
                        Errors = allErrors,
                        Duration = duration.TotalSeconds
                    }
                };
            }
            catch (Exception ex)
            {
                var error = $"同步客户数据时发生异常: {ex.Message}";
                _logger.LogError(ex, error);
                return new WebResponseContent(false)
                {
                    Message = error,
                    Data = new
                    {
                        SyncedCount = totalSyncedCount,
                        ErrorCount = totalErrorCount + 1,
                        Errors = allErrors.Concat(new[] { error }).ToList()
                    }
                };
            }
        }

        /// <summary>
        /// 处理单页客户数据
        /// </summary>
        /// <param name="customers">客户数据列表</param>
        /// <returns>处理结果</returns>
        private async Task<(int SyncedCount, int ErrorCount, List<string> Errors)> ProcessCustomerPageAsync(List<K3CloudCustomerData> customers)
        {
            var errors = new List<string>();
            var syncedCount = 0;
            var errorCount = 0;

            try
            {
                // 1. 批量查询现有客户
                var customerNumbers = customers.Select(x => x.Number).Where(x => !string.IsNullOrEmpty(x)).ToList();
                var existingCustomerDict = await _repository.FindAsIQueryable(x => customerNumbers.Contains(x.Number))
                    .ToDictionaryAsync(x => x.Number, x => x);

                _logger.LogInformation($"查询到现有客户 {existingCustomerDict.Count} 条");

                // 2. 分离需要更新和新增的数据
                var customersToUpdate = new List<OCP_Customer>();
                var customersToAdd = new List<OCP_Customer>();
                var currentTime = DateTime.Now;

                foreach (var customer in customers)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(customer.Number))
                        {
                            var error = "客户编码为空，跳过处理";
                            _logger.LogWarning(error);
                            errors.Add(error);
                            errorCount++;
                            continue;
                        }

                        if (existingCustomerDict.TryGetValue(customer.Number, out var existingCustomer))
                        {
                            // 更新现有客户
                            MapK3CloudDataToEntity(customer, existingCustomer, currentTime, false);
                            customersToUpdate.Add(existingCustomer);
                        }
                        else
                        {
                            // 创建新客户
                            var newCustomer = new OCP_Customer();
                            MapK3CloudDataToEntity(customer, newCustomer, currentTime, true);
                            customersToAdd.Add(newCustomer);
                        }
                    }
                    catch (Exception ex)
                    {
                        var error = $"处理客户 {customer.Number} 失败: {ex.Message}";
                        _logger.LogError(ex, error);
                        errors.Add(error);
                        errorCount++;
                    }
                }

                // 3. 批量执行数据库操作
                try
                {
                    // 批量更新
                    if (customersToUpdate.Any())
                    {
                        _repository.UpdateRange(customersToUpdate);
                        syncedCount += customersToUpdate.Count;
                        _logger.LogInformation($"批量更新客户 {customersToUpdate.Count} 条");
                    }

                    // 批量新增
                    if (customersToAdd.Any())
                    {
                        _repository.AddRange(customersToAdd, false);
                        syncedCount += customersToAdd.Count;
                        _logger.LogInformation($"批量新增客户 {customersToAdd.Count} 条");
                    }

                    // 一次性保存所有更改
                    if (customersToUpdate.Any() || customersToAdd.Any())
                    {
                        await _repository.SaveChangesAsync();
                        _logger.LogInformation($"成功保存客户数据，更新 {customersToUpdate.Count} 条，新增 {customersToAdd.Count} 条");
                    }
                }
                catch (Exception ex)
                {
                    var error = $"批量保存数据库更改失败: {ex.Message}";
                    _logger.LogError(ex, error);
                    errors.Add(error);
                    // 如果保存失败，将所有成功计数转为错误计数
                    errorCount += syncedCount;
                    syncedCount = 0;
                }
            }
            catch (Exception ex)
            {
                var error = $"处理客户页面数据异常: {ex.Message}";
                _logger.LogError(ex, error);
                errors.Add(error);
                errorCount = customers.Count;
                syncedCount = 0;
            }

            return (syncedCount, errorCount, errors);
        }

        /// <summary>
        /// 映射K3Cloud数据到实体
        /// </summary>
        /// <param name="k3Customer">K3Cloud客户数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="currentTime">当前时间</param>
        /// <param name="isNew">是否为新增</param>
        private void MapK3CloudDataToEntity(K3CloudCustomerData k3Customer, OCP_Customer entity, DateTime currentTime, bool isNew)
        {
            // 基本信息映射
            entity.Number = k3Customer.Number;                          // FNumber -> Number
            entity.Name = k3Customer.Name;                              // FName -> Name
            entity.Address = k3Customer.FAddress;                       // FBaseInfo.FAddress -> Address
            entity.TaxRegistrationNo = k3Customer.FSOCIALCRECODE;       // FBaseInfo.FSOCIALCRECODE -> TaxRegistrationNo
            entity.Salesperson = k3Customer.FSalesmanName;              // FSalesman.FName -> Salesperson
            
            // 联系电话优先级：联系人电话 > 联系人手机 > 地点电话 > 地点手机
            entity.ContactPhone = !string.IsNullOrEmpty(k3Customer.FContactTel) ? k3Customer.FContactTel :
                                 !string.IsNullOrEmpty(k3Customer.FContactMobile) ? k3Customer.FContactMobile :
                                 !string.IsNullOrEmpty(k3Customer.FLocTel) ? k3Customer.FLocTel :
                                 k3Customer.FLocMobile;
            
            // 如果是新增，设置CustomerID和创建信息
            if (isNew)
            {
                entity.CustomerID = k3Customer.FCustomerId ?? 0;
                
                // 解析K3Cloud的创建日期
                if (!string.IsNullOrEmpty(k3Customer.FCreateDate) && DateTime.TryParse(k3Customer.FCreateDate, out var k3CreateDate))
                {
                    entity.CreateDate = k3CreateDate;
                }
                else
                {
                    entity.CreateDate = currentTime;
                }
                
                entity.Creator = !string.IsNullOrEmpty(k3Customer.FCreatorName) ? k3Customer.FCreatorName : "K3CloudSync";
                entity.CreateID = 1; // 系统用户ID
            }
            
            // 解析K3Cloud的修改日期
            if (!string.IsNullOrEmpty(k3Customer.FModifyDate) && DateTime.TryParse(k3Customer.FModifyDate, out var k3ModifyDate))
            {
                entity.ModifyDate = k3ModifyDate;
            }
            else
            {
                entity.ModifyDate = currentTime;
            }
            
            entity.Modifier = !string.IsNullOrEmpty(k3Customer.FModifierName) ? k3Customer.FModifierName : "K3CloudSync";
            entity.ModifyID = 1; // 系统用户ID
        }

        /// <summary>
        /// 获取K3Cloud客户数据（不保存到数据库）
        /// </summary>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <returns>客户数据</returns>
        public async Task<WebResponseContent> GetK3CloudCustomersAsync(int pageIndex = 0, int pageSize = 1000, string filterString = null)
        {
            try
            {
                var response = await _k3CloudService.GetCustomersAsync(pageIndex, pageSize, filterString);
                
                if (response.IsSuccess)
                {
                    return new WebResponseContent(true)
                    {
                        Data = response.Data,
                        Message = $"成功获取 {response.Data?.Count ?? 0} 条客户数据"
                    };
                }
                else
                {
                    return new WebResponseContent(false) { Message = response.Message };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取K3Cloud客户数据异常");
                return new WebResponseContent(false) { Message = $"获取K3Cloud客户数据异常: {ex.Message}" };
            }
        }

        /// <summary>
        /// 获取K3Cloud客户总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>客户总数</returns>
        public async Task<WebResponseContent> GetK3CloudCustomerCountAsync(string filterString = null)
        {
            try
            {
                var count = await _k3CloudService.GetCustomerCountAsync(filterString);
                return new WebResponseContent(true)
                {
                    Data = count,
                    Message = $"K3Cloud中共有 {count} 条客户数据"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取K3Cloud客户总数异常");
                return new WebResponseContent(false) { Message = $"获取K3Cloud客户总数异常: {ex.Message}" };
            }
        }
    }
} 