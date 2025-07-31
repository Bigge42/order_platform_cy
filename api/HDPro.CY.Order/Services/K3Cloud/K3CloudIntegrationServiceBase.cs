/*
 * K3Cloud集成服务基类
 * 提供通用的K3Cloud数据同步功能
 */
using HDPro.CY.Order.Services.K3Cloud.Models;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using HDPro.Entity.SystemModels;
using HDPro.Core.Extensions.AutofacManager;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services.K3Cloud
{
    /// <summary>
    /// K3Cloud集成服务基类
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TK3CloudData">K3Cloud数据类型</typeparam>
    public abstract class K3CloudIntegrationServiceBase<TEntity, TK3CloudData>
        where TEntity : BaseEntity
    {
        protected readonly IK3CloudService _k3CloudService;
        protected readonly ILogger _logger;

        protected K3CloudIntegrationServiceBase(IK3CloudService k3CloudService, ILogger logger)
        {
            _k3CloudService = k3CloudService;
            _logger = logger;
        }

        /// <summary>
        /// 获取K3Cloud数据总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>数据总数</returns>
        protected abstract Task<int> GetK3CloudCountAsync(string filterString = null);

        /// <summary>
        /// 分页获取K3Cloud数据
        /// </summary>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="orderString">排序字段</param>
        /// <returns>K3Cloud数据</returns>
        protected abstract Task<K3CloudQueryResponse<TK3CloudData>> GetK3CloudDataAsync(
            int pageIndex, int pageSize, string filterString, string orderString);

        /// <summary>
        /// 处理单页数据
        /// </summary>
        /// <param name="k3CloudDataList">K3Cloud数据列表</param>
        /// <returns>处理结果</returns>
        protected abstract Task<(int SyncedCount, int ErrorCount, List<string> Errors)> ProcessDataPageAsync(
            List<TK3CloudData> k3CloudDataList);

        /// <summary>
        /// 获取实体类型名称
        /// </summary>
        /// <returns>实体类型名称</returns>
        protected virtual string GetEntityTypeName()
        {
            return typeof(TEntity).Name;
        }

        /// <summary>
        /// 从K3Cloud同步数据的通用方法
        /// </summary>
        /// <param name="pageSize">每页数量</param>
        /// <param name="filterString">过滤条件</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncFromK3CloudAsync(int pageSize = 1000, string filterString = null)
        {
            var entityTypeName = GetEntityTypeName();
            
            try
            {
                _logger.LogInformation($"开始从K3Cloud同步{entityTypeName}数据");

                // 1. 获取数据总数
                var totalCount = await GetK3CloudCountAsync(filterString);
                if (totalCount == 0)
                {
                    return new WebResponseContent(true) { Message = $"K3Cloud中没有找到{entityTypeName}数据" };
                }

                _logger.LogInformation($"K3Cloud中共有 {totalCount} 条{entityTypeName}数据");

                // 2. 计算总页数
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var syncedCount = 0;
                var errorCount = 0;
                var errors = new List<string>();

                // 3. 分页获取并同步数据
                for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
                {
                    try
                    {
                        _logger.LogInformation($"正在同步{entityTypeName}第 {pageIndex + 1}/{totalPages} 页数据");

                        var dataResponse = await GetK3CloudDataAsync(pageIndex, pageSize, filterString, "FNumber");
                        
                        if (!dataResponse.IsSuccess)
                        {
                            var error = $"获取{entityTypeName}第 {pageIndex + 1} 页数据失败: {dataResponse.Message}";
                            _logger.LogError(error);
                            errors.Add(error);
                            errorCount++;
                            continue;
                        }

                        if (dataResponse.Data?.Count > 0)
                        {
                            // 处理当前页的数据
                            var pageResult = await ProcessDataPageAsync(dataResponse.Data);
                            syncedCount += pageResult.SyncedCount;
                            errorCount += pageResult.ErrorCount;
                            errors.AddRange(pageResult.Errors);
                        }

                        // 添加延迟，避免对K3Cloud服务器造成过大压力
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        var error = $"处理{entityTypeName}第 {pageIndex + 1} 页数据异常: {ex.Message}";
                        _logger.LogError(ex, error);
                        errors.Add(error);
                        errorCount++;
                    }
                }

                var message = $"{entityTypeName}同步完成。总计: {totalCount} 条，成功: {syncedCount} 条，失败: {errorCount} 条";
                _logger.LogInformation(message);

                return new WebResponseContent(true)
                {
                    Message = message,
                    Data = new
                    {
                        EntityType = entityTypeName,
                        TotalCount = totalCount,
                        SyncedCount = syncedCount,
                        ErrorCount = errorCount,
                        Errors = errors.Take(10).ToList() // 只返回前10个错误信息
                    }
                };
            }
            catch (Exception ex)
            {
                var error = $"同步{entityTypeName}数据异常: {ex.Message}";
                _logger.LogError(ex, error);
                return new WebResponseContent(false) { Message = error };
            }
        }

        /// <summary>
        /// 获取K3Cloud数据（不保存到数据库）
        /// </summary>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <returns>数据</returns>
        public async Task<WebResponseContent> GetK3CloudDataAsync(int pageIndex = 0, int pageSize = 1000, string filterString = null)
        {
            var entityTypeName = GetEntityTypeName();
            
            try
            {
                var response = await GetK3CloudDataAsync(pageIndex, pageSize, filterString, "FNumber");
                
                if (response.IsSuccess)
                {
                    return new WebResponseContent(true)
                    {
                        Data = response.Data,
                        Message = $"成功获取 {response.Data?.Count ?? 0} 条{entityTypeName}数据"
                    };
                }
                else
                {
                    return new WebResponseContent(false) { Message = response.Message };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取K3Cloud{entityTypeName}数据异常");
                return new WebResponseContent(false) { Message = $"获取K3Cloud{entityTypeName}数据异常: {ex.Message}" };
            }
        }

        /// <summary>
        /// 获取K3Cloud数据总数（公共接口）
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>数据总数</returns>
        public async Task<WebResponseContent> GetK3CloudCountResultAsync(string filterString = null)
        {
            var entityTypeName = GetEntityTypeName();
            
            try
            {
                var count = await GetK3CloudCountAsync(filterString);
                return new WebResponseContent(true)
                {
                    Data = count,
                    Message = $"K3Cloud中共有 {count} 条{entityTypeName}数据"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取K3Cloud{entityTypeName}总数异常");
                return new WebResponseContent(false) { Message = $"获取K3Cloud{entityTypeName}总数异常: {ex.Message}" };
            }
        }
    }
} 