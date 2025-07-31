/*
 *所有关于OCP_Supplier类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_SupplierService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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

namespace HDPro.CY.Order.Services
{
    public partial class OCP_SupplierService
    {
        private readonly IOCP_SupplierRepository _repository;//访问数据库
        private readonly IK3CloudService _k3CloudService;//K3Cloud服务
        private readonly ILogger<OCP_SupplierService> _logger;//日志服务

        [ActivatorUtilitiesConstructor]
        public OCP_SupplierService(
            IOCP_SupplierRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IK3CloudService k3CloudService,
            ILogger<OCP_SupplierService> logger
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
        /// 可在此处添加OCP_Supplier特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_Supplier特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目特有的实体验证逻辑
        /// </summary>
        /// <param name="entity">供应商实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_Supplier entity)
        {
            // 供应商编码不能为空
            if (string.IsNullOrEmpty(entity.SupplierNumber))
            {
                return new WebResponseContent(false) { Message = "供应商编码不能为空" };
            }

            // 供应商名称不能为空
            if (string.IsNullOrEmpty(entity.SupplierName))
            {
                return new WebResponseContent(false) { Message = "供应商名称不能为空" };
            }

            return new WebResponseContent(true);
        }

        /// <summary>
        /// 获取本地供应商表最大修改时间
        /// </summary>
        /// <returns>最大修改时间</returns>
        private async Task<DateTime?> GetLocalSupplierMaxModifyDateAsync()
        {
            try
            {
                var maxModifyDate = await _repository.DbContext.Set<OCP_Supplier>().Where(p => p.ModifyDate.HasValue).MaxAsync(p => p.ModifyDate);
                _logger.LogInformation($"本地供应商表最大修改时间: {maxModifyDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "无数据"}");
                return maxModifyDate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取本地供应商表最大修改时间失败");
                return null;
            }
        }

        /// <summary>
        /// 构建K3Cloud查询过滤条件
        /// </summary>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <param name="includeIncrementalSync">是否包含增量同步条件</param>
        /// <returns>过滤条件字符串</returns>
        private async Task<string> BuildK3CloudFilterStringAsync(string customFilter = null, bool includeIncrementalSync = true)
        {
            try
            {
                // 基础过滤条件：只获取已审核且未禁用的供应商
                var baseFilter = "FDocumentStatus='C' and FForbidStatus='A'";

                // 增量同步条件
                string incrementalFilter = null;
                if (includeIncrementalSync)
                {
                    var maxModifyDate = await GetLocalSupplierMaxModifyDateAsync();
                    if (maxModifyDate.HasValue)
                    {
                        // 使用修改时间进行增量过滤
                        incrementalFilter = $"FModifyDate>='{maxModifyDate.Value:yyyy-MM-dd HH:mm:ss}'";
                        _logger.LogInformation($"增量同步过滤条件: {incrementalFilter}");
                    }
                }

                // 组合过滤条件
                var filters = new List<string> { baseFilter };
                if (!string.IsNullOrEmpty(incrementalFilter))
                {
                    filters.Add(incrementalFilter);
                }
                if (!string.IsNullOrEmpty(customFilter))
                {
                    filters.Add(customFilter);
                }

                var finalFilter = string.Join(" and ", filters);
                _logger.LogInformation($"最终过滤条件: {finalFilter}");
                return finalFilter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "构建K3Cloud查询过滤条件失败");
                return "FDocumentStatus='C' and FForbidStatus='A'"; // 返回基础过滤条件
            }
        }

        /// <summary>
        /// 从K3Cloud增量同步供应商数据
        /// 基于本地供应商表最大修改时间进行增量同步
        /// </summary>
        /// <param name="pageSize">每页数量，默认1000</param>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncSuppliersFromK3CloudAsync(int pageSize = 1000, string customFilter = null)
        {
            try
            {
                _logger.LogInformation("开始从K3Cloud增量同步供应商数据");

                // 1. 构建包含增量同步条件的过滤字符串
                var filterString = await BuildK3CloudFilterStringAsync(customFilter, true);

                // 2. 获取供应商总数
                var totalCount = await _k3CloudService.GetSupplierCountAsync(filterString);
                if (totalCount == 0)
                {
                    return new WebResponseContent(true) { Message = "K3Cloud中没有找到需要同步的供应商数据" };
                }

                _logger.LogInformation($"K3Cloud中共有 {totalCount} 条需要同步的供应商数据");

                // 3. 计算总页数
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var syncedCount = 0;
                var errorCount = 0;
                var errors = new List<string>();

                // 4. 分页获取并同步数据（每页一个小事务，避免长事务）
                for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
                {
                    try
                    {
                        _logger.LogInformation($"开始处理第 {pageIndex + 1}/{totalPages} 页数据");

                        // 获取当前页数据
                        var response = await _k3CloudService.GetSuppliersAsync(pageIndex, pageSize, filterString);
                        if (!response.IsSuccess || response.Data == null || !response.Data.Any())
                        {
                            _logger.LogWarning($"第 {pageIndex + 1} 页数据获取失败或为空: {response.Message}");
                            continue;
                        }

                        // 处理当前页数据
                        var pageResult = await ProcessSupplierPageAsync(response.Data);
                        syncedCount += pageResult.SyncedCount;
                        errorCount += pageResult.ErrorCount;
                        errors.AddRange(pageResult.Errors);

                        _logger.LogInformation($"第 {pageIndex + 1} 页处理完成，成功: {pageResult.SyncedCount}，失败: {pageResult.ErrorCount}");
                    }
                    catch (Exception ex)
                    {
                        var error = $"处理第 {pageIndex + 1} 页数据异常: {ex.Message}";
                        _logger.LogError(ex, error);
                        errors.Add(error);
                        errorCount += pageSize; // 假设整页都失败
                    }
                }

                var message = $"同步完成。总计: {totalCount} 条，成功: {syncedCount} 条，失败: {errorCount} 条";
                _logger.LogInformation(message);

                return new WebResponseContent(true)
                {
                    Message = message,
                    Data = new
                    {
                        TotalCount = totalCount,
                        SyncedCount = syncedCount,
                        ErrorCount = errorCount,
                        Errors = errors.Take(10).ToList() // 只返回前10个错误信息
                    }
                };
            }
            catch (Exception ex)
            {
                var error = $"增量同步供应商数据异常: {ex.Message}";
                _logger.LogError(ex, error);
                return new WebResponseContent(false) { Message = error };
            }
        }

        /// <summary>
        /// 处理单页供应商数据（性能优化版本）
        /// </summary>
        /// <param name="suppliers">供应商数据列表</param>
        /// <returns>处理结果</returns>
        private async Task<(int SyncedCount, int ErrorCount, List<string> Errors)> ProcessSupplierPageAsync(List<K3CloudSupplierData> suppliers)
        {
            var syncedCount = 0;
            var errorCount = 0;
            var errors = new List<string>();

            if (suppliers == null || !suppliers.Any())
            {
                return (0, 0, new List<string>());
            }

            try
            {
                // 1. 批量查询现有供应商，减少数据库查询次数
                var supplierCodes = suppliers.Select(s => s.Number).Where(code => !string.IsNullOrEmpty(code)).ToList();
                
                var existingSuppliers = await _repository.FindAsync(s => supplierCodes.Contains(s.SupplierNumber));
                var existingSupplierDict = existingSuppliers.ToDictionary(s => s.SupplierNumber, s => s);

                // 2. 分离需要更新和新增的数据
                var suppliersToUpdate = new List<OCP_Supplier>();
                var suppliersToAdd = new List<OCP_Supplier>();
                var currentTime = DateTime.Now;

                foreach (var supplier in suppliers)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(supplier.Number))
                        {
                            var error = "供应商编码为空，跳过处理";
                            _logger.LogWarning(error);
                            errors.Add(error);
                            errorCount++;
                            continue;
                        }

                        if (existingSupplierDict.TryGetValue(supplier.Number, out var existingSupplier))
                        {
                            // 更新现有供应商
                            MapK3CloudDataToEntity(supplier, existingSupplier, currentTime, false);
                            suppliersToUpdate.Add(existingSupplier);
                        }
                        else
                        {
                            // 创建新供应商
                            var newSupplier = new OCP_Supplier();
                            MapK3CloudDataToEntity(supplier, newSupplier, currentTime, true);
                            suppliersToAdd.Add(newSupplier);
                        }
                    }
                    catch (Exception ex)
                    {
                        var error = $"处理供应商 {supplier.Number} 失败: {ex.Message}";
                        _logger.LogError(ex, error);
                        errors.Add(error);
                        errorCount++;
                    }
                }

                // 3. 批量执行数据库操作
                try
                {
                    // 批量更新
                    if (suppliersToUpdate.Any())
                    {
                        _repository.UpdateRange(suppliersToUpdate);
                        syncedCount += suppliersToUpdate.Count;
                        _logger.LogInformation($"批量更新供应商 {suppliersToUpdate.Count} 条");
                    }

                    // 批量新增
                    if (suppliersToAdd.Any())
                    {
                        _repository.AddRange(suppliersToAdd, false);
                        syncedCount += suppliersToAdd.Count;
                        _logger.LogInformation($"批量新增供应商 {suppliersToAdd.Count} 条");
                    }

                    // 一次性保存所有更改
                    if (suppliersToUpdate.Any() || suppliersToAdd.Any())
                    {
                        await _repository.SaveChangesAsync();
                        _logger.LogInformation($"成功保存供应商数据，更新 {suppliersToUpdate.Count} 条，新增 {suppliersToAdd.Count} 条");
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
                var error = $"处理供应商页面数据异常: {ex.Message}";
                _logger.LogError(ex, error);
                errors.Add(error);
                errorCount = suppliers.Count;
                syncedCount = 0;
            }

            return (syncedCount, errorCount, errors);
        }

        /// <summary>
        /// 映射K3Cloud数据到实体
        /// </summary>
        /// <param name="k3Supplier">K3Cloud供应商数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="currentTime">当前时间</param>
        /// <param name="isNew">是否为新增</param>
        private void MapK3CloudDataToEntity(K3CloudSupplierData k3Supplier, OCP_Supplier entity, DateTime currentTime, bool isNew)
        {
            // 基本信息映射
            entity.SupplierNumber = k3Supplier.Number;                  // FNumber -> SupplierNumber
            entity.SupplierName = k3Supplier.Name;                     // FName -> SupplierName
            entity.SupplyScope = k3Supplier.F_ORA_GHFW;                // F_ORA_GHFW -> SupplyScope
            entity.PurchasePerson = k3Supplier.FCenterPurchaserName;   // FCenterPurchaser.FName -> PurchasePerson
            entity.Address = k3Supplier.FAddress;                      // FBaseInfo.FAddress -> Address
            entity.UnifiedCreditCode = k3Supplier.FSOCIALCRECODE;      // FBaseInfo.FSOCIALCRECODE -> UnifiedCreditCode
            entity.SupplyCategory = k3Supplier.FSupplyClassifyName;    // FBaseInfo.FSupplyClassify.FName -> SupplyCategory
            
            // 联系电话优先级：联系人电话 > 联系人手机 > 地点电话 > 地点手机
            entity.ContactPhone = !string.IsNullOrEmpty(k3Supplier.FContactTel) ? k3Supplier.FContactTel :
                                 !string.IsNullOrEmpty(k3Supplier.FContactMobile) ? k3Supplier.FContactMobile :
                                 !string.IsNullOrEmpty(k3Supplier.FLocTel) ? k3Supplier.FLocTel :
                                 k3Supplier.FLocMobile;
            
            // 如果是新增，设置SupplierID和创建信息
            if (isNew)
            {
                entity.SupplierID = k3Supplier.FSupplierId ?? 0;
                
                // 解析K3Cloud的创建日期
                if (!string.IsNullOrEmpty(k3Supplier.FModifyDate) && DateTime.TryParse(k3Supplier.FModifyDate, out var k3CreateDate))
                {
                    entity.CreateDate = k3CreateDate;
                }
                else
                {
                    entity.CreateDate = currentTime;
                }
            }
            
            // 解析K3Cloud的修改日期
            if (!string.IsNullOrEmpty(k3Supplier.FModifyDate) && DateTime.TryParse(k3Supplier.FModifyDate, out var k3ModifyDate))
            {
                entity.ModifyDate = k3ModifyDate;
            }
            else
            {
                entity.ModifyDate = currentTime;
            }
            
            entity.Modifier = !string.IsNullOrEmpty(k3Supplier.FModifierName) ? k3Supplier.FModifierName : "K3CloudSync";
            entity.ModifyID = 1; // 系统用户ID
        }

        /// <summary>
        /// 获取K3Cloud供应商数据（不保存到数据库）
        /// </summary>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <returns>供应商数据</returns>
        public async Task<WebResponseContent> GetK3CloudSuppliersAsync(int pageIndex = 0, int pageSize = 1000, string filterString = null)
        {
            try
            {
                var response = await _k3CloudService.GetSuppliersAsync(pageIndex, pageSize, filterString);
                
                if (response.IsSuccess)
                {
                    return new WebResponseContent(true)
                    {
                        Data = response.Data,
                        Message = $"成功获取 {response.Data?.Count ?? 0} 条供应商数据"
                    };
                }
                else
                {
                    return new WebResponseContent(false) { Message = response.Message };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取K3Cloud供应商数据异常");
                return new WebResponseContent(false) { Message = $"获取K3Cloud供应商数据异常: {ex.Message}" };
            }
        }

        /// <summary>
        /// 获取K3Cloud供应商总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>供应商总数</returns>
        public async Task<WebResponseContent> GetK3CloudSupplierCountAsync(string filterString = null)
        {
            try
            {
                var count = await _k3CloudService.GetSupplierCountAsync(filterString);
                return new WebResponseContent(true)
                {
                    Data = count,
                    Message = $"K3Cloud中共有 {count} 条供应商数据"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取K3Cloud供应商总数异常");
                return new WebResponseContent(false) { Message = $"获取K3Cloud供应商总数异常: {ex.Message}" };
            }
        }
  }
} 