/*
 *所有关于OCP_Material类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OCP_MaterialService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_MaterialService
    {
        private readonly IOCP_MaterialRepository _repository;//访问数据库
        private readonly IK3CloudService _k3CloudService;//K3Cloud服务
        private readonly ILogger<OCP_MaterialService> _logger;//日志服务
        private readonly ILogger _materialSyncLogger;//物料同步专用日志记录器

        [ActivatorUtilitiesConstructor]
        public OCP_MaterialService(
            IOCP_MaterialRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IK3CloudService k3CloudService,
            ILogger<OCP_MaterialService> logger,
            ILoggerFactory loggerFactory
            )
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
            _k3CloudService = k3CloudService;
            _logger = logger;
            // 创建物料同步专用日志记录器
            _materialSyncLogger = loggerFactory.CreateLogger("MaterialSync");
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        /// <summary>
        /// 重写CY.Order项目特有的初始化逻辑
        /// 可在此处添加OCP_Material特有的初始化代码
        /// </summary>
        protected override void InitCYOrderSpecific()
        {
            base.InitCYOrderSpecific();
            // 在此处添加OCP_Material特有的初始化逻辑
        }

        /// <summary>
        /// 重写CY.Order项目通用数据验证方法
        /// 可在此处添加OCP_Material特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected override WebResponseContent ValidateCYOrderEntity(OCP_Material entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            
            // 在此处添加OCP_Material特有的数据验证逻辑
            
            return response;
        }

        /// <summary>
        /// 获取本地物料表的最大创建时间
        /// </summary>
        /// <returns>最大创建时间，如果没有数据则返回null</returns>
        private async Task<DateTime?> GetLocalMaterialMaxCreateDateAsync()
        {
            try
            {
                 var maxCreateDate = await _repository.DbContext.Set<OCP_Material>().Where(p => p.ModifyDate.HasValue).MaxAsync(p => p.ModifyDate);
                LogMaterialSync(LogLevel.Information, $"本地物料表最大修改时间: {maxCreateDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "无数据"}");
                return maxCreateDate;
            }
            catch (Exception ex)
            {
                LogMaterialSync(LogLevel.Error, "获取本地物料表最大修改时间失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 构建K3Cloud查询过滤条件
        /// </summary>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <param name="includeIncrementalSync">是否包含增量同步条件</param>
        /// <returns>完整的过滤条件字符串</returns>
        private async Task<string> BuildK3CloudFilterStringAsync(string customFilter = null, bool includeIncrementalSync = true)
        {
            var filterParts = new List<string>();
            
            // 基础过滤条件
            filterParts.Add("FDocumentStatus='C' and FForbidStatus='A'");
            
            // 增量同步条件
            if (includeIncrementalSync)
            {
                var maxCreateDate = await GetLocalMaterialMaxCreateDateAsync();
                if (maxCreateDate.HasValue)
                {
                    // 格式化时间为K3Cloud接受的格式
                    var formattedDate = maxCreateDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    filterParts.Add($"FModifyDate>='{formattedDate}'");
                    LogMaterialSync(LogLevel.Information, $"启用增量同步，过滤条件: FModifyDate>='{formattedDate}'");
                }
                else
                {
                    LogMaterialSync(LogLevel.Information, "本地无物料数据，执行全量同步");
                }
            }
            
            // 自定义过滤条件
            if (!string.IsNullOrEmpty(customFilter))
            {
                filterParts.Add($"({customFilter})");
            }
            
            var finalFilter = string.Join(" and ", filterParts);
            LogMaterialSync(LogLevel.Information, $"最终过滤条件: {finalFilter}");
            
            return finalFilter;
        }

        /// <summary>
        /// 构建K3Cloud时间范围查询过滤条件
        /// </summary>
        /// <param name="startDate">开始日期 (yyyy-MM-dd)</param>
        /// <param name="endDate">结束日期 (yyyy-MM-dd)</param>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <returns>完整的过滤条件字符串</returns>
        private async Task<string> BuildK3CloudDateRangeFilterStringAsync(string? startDate, string? endDate, string? customFilter)
        {
            var filterParts = new List<string>();

            // 基础过滤条件
            filterParts.Add("FDocumentStatus='C' and FForbidStatus='A'");

            // 时间范围过滤条件
            if (!string.IsNullOrEmpty(startDate))
            {
                if (DateTime.TryParse(startDate, out var start))
                {
                    var formattedStartDate = start.ToString("yyyy-MM-dd 00:00:00");
                    filterParts.Add($"FModifyDate>='{formattedStartDate}'");
                    LogMaterialSync(LogLevel.Information, $"设置开始时间过滤条件: FModifyDate>='{formattedStartDate}'");
                }
                else
                {
                    LogMaterialSync(LogLevel.Warning, $"开始日期格式无效: {startDate}，将忽略此条件");
                }
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                if (DateTime.TryParse(endDate, out var end))
                {
                    var formattedEndDate = end.ToString("yyyy-MM-dd 23:59:59");
                    filterParts.Add($"FModifyDate<='{formattedEndDate}'");
                    LogMaterialSync(LogLevel.Information, $"设置结束时间过滤条件: FModifyDate<='{formattedEndDate}'");
                }
                else
                {
                    LogMaterialSync(LogLevel.Warning, $"结束日期格式无效: {endDate}，将忽略此条件");
                }
            }

            // 如果没有指定时间范围，记录警告
            if (string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                LogMaterialSync(LogLevel.Warning, "未指定时间范围，将同步所有符合基础条件的物料数据");
            }

            // 自定义过滤条件
            if (!string.IsNullOrEmpty(customFilter))
            {
                filterParts.Add($"({customFilter})");
            }

            var finalFilter = string.Join(" and ", filterParts);
            LogMaterialSync(LogLevel.Information, $"最终过滤条件: {finalFilter}");

            return finalFilter;
        }

        /// <summary>
        /// 从K3Cloud增量同步物料数据
        /// 基于本地物料表最大创建时间进行增量同步
        /// </summary>
        /// <param name="pageSize">每页数量，默认1000</param>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncMaterialsFromK3CloudAsync(int pageSize = 2000, string customFilter = null)
        {
            return await SyncMaterialsFromK3CloudByDateRangeAsync(null, null, pageSize, customFilter, true);
        }

        /// <summary>
        /// 从K3Cloud按时间范围同步物料数据
        /// 支持指定时间范围的物料数据同步
        /// </summary>
        /// <param name="startDate">开始日期 (yyyy-MM-dd)</param>
        /// <param name="endDate">结束日期 (yyyy-MM-dd)</param>
        /// <param name="pageSize">每页数量，默认1000</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncMaterialsFromK3CloudByDateRangeAsync(string? startDate = null, string? endDate = null, int pageSize = 1000)
        {
            return await SyncMaterialsFromK3CloudByDateRangeAsync(startDate, endDate, pageSize, null, false);
        }

        /// <summary>
        /// 从K3Cloud同步物料数据的核心实现方法
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <param name="useIncrementalSync">是否使用增量同步</param>
        /// <returns>同步结果</returns>
        private async Task<WebResponseContent> SyncMaterialsFromK3CloudByDateRangeAsync(string? startDate, string? endDate, int pageSize, string? customFilter, bool useIncrementalSync)
        {
            try
            {
                var syncType = useIncrementalSync ? "增量同步" : "时间范围同步";
                LogMaterialSync(LogLevel.Information, $"========== 开始从K3Cloud{syncType}物料数据 ==========");
                LogMaterialSync(LogLevel.Information, $"同步参数 - 页大小: {pageSize}, 开始日期: {startDate ?? "无"}, 结束日期: {endDate ?? "无"}, 自定义过滤条件: {customFilter ?? "无"}");

                // 1. 构建过滤字符串
                string filterString;
                if (useIncrementalSync)
                {
                    // 使用增量同步逻辑
                    filterString = await BuildK3CloudFilterStringAsync(customFilter, false);
                }
                else
                {
                    // 使用时间范围过滤逻辑
                    filterString = await BuildK3CloudDateRangeFilterStringAsync(startDate, endDate, customFilter);
                }

                // 2. 获取物料总数
                var totalCount = await _k3CloudService.GetMaterialCountAsync(filterString);
                if (totalCount == 0)
                {
                    LogMaterialSync(LogLevel.Warning, "K3Cloud中没有找到物料数据");
                    return new WebResponseContent(true) { Message = "K3Cloud中没有找到物料数据" };
                }

                LogMaterialSync(LogLevel.Information, $"K3Cloud中共有 {totalCount} 条物料数据");

                // 3. 计算总页数
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var syncedCount = 0;
                var errorCount = 0;
                var errors = new List<string>();

                LogMaterialSync(LogLevel.Information, $"开始分页同步，总页数: {totalPages}");

                // 4. 分页获取并同步数据（每页一个小事务，避免长事务）
                for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
                {
                    try
                    {
                        LogMaterialSync(LogLevel.Information, $"正在同步第 {pageIndex + 1}/{totalPages} 页数据");

                        var materialsResponse = await _k3CloudService.GetMaterialsAsync(pageIndex, pageSize, filterString);
                        
                        if (!materialsResponse.IsSuccess)
                        {
                            var error = $"获取第 {pageIndex + 1} 页数据失败: {materialsResponse.Message}";
                            LogMaterialSync(LogLevel.Error, error);
                            errors.Add(error);
                            errorCount++;
                            continue;
                        }

                        if (materialsResponse.Data?.Count > 0)
                        {
                            // 处理当前页的数据（每页数据在独立事务中处理）
                            var pageResult = await ProcessMaterialPageAsync(materialsResponse.Data);
                            syncedCount += pageResult.SyncedCount;
                            errorCount += pageResult.ErrorCount;
                            errors.AddRange(pageResult.Errors);
                        }

                        // 添加延迟，避免对K3Cloud服务器造成过大压力
                        if (pageIndex < totalPages - 1) // 最后一页不需要延迟
                        {
                            await Task.Delay(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        var error = $"处理第 {pageIndex + 1} 页数据异常: {ex.Message}";
                        LogMaterialSync(LogLevel.Error, error, ex);
                        errors.Add(error);
                        errorCount++;
                    }
                }

                var message = $"同步完成。总计: {totalCount} 条，成功: {syncedCount} 条，失败: {errorCount} 条";
                LogMaterialSync(LogLevel.Information, $"========== {message} ==========");
                if (errorCount > 0)
                {
                    LogMaterialSync(LogLevel.Warning, $"同步过程中出现 {errorCount} 个错误，请查看详细日志");
                }

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
                var error = $"增量同步物料数据异常: {ex.Message}";
                LogMaterialSync(LogLevel.Error, error, ex);
                return new WebResponseContent(false) { Message = error };
            }
        }

        /// <summary>
        /// 处理单页物料数据（性能优化版本）
        /// </summary>
        /// <param name="materials">物料数据列表</param>
        /// <returns>处理结果</returns>
        private async Task<(int SyncedCount, int ErrorCount, List<string> Errors)> ProcessMaterialPageAsync(List<K3CloudMaterialData> materials)
        {
            var syncedCount = 0;
            var errorCount = 0;
            var errors = new List<string>();

            if (materials == null || !materials.Any())
            {
                return (0, 0, new List<string>());
            }

            try
            {
                // 1. 批量查询现有物料，减少数据库查询次数
                var materialCodes = materials.Select(m => m.Number).Where(code => !string.IsNullOrEmpty(code)).ToList();
                
                var existingMaterials = await _repository.FindAsync(m => materialCodes.Contains(m.MaterialCode));
                var existingMaterialDict = existingMaterials.ToDictionary(m => m.MaterialCode, m => m);

                // 2. 分离需要更新和新增的数据
                var materialsToUpdate = new List<OCP_Material>();
                var materialsToAdd = new List<OCP_Material>();
                var currentTime = DateTime.Now;

                foreach (var material in materials)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(material.Number))
                        {
                            var error = "物料编码为空，跳过处理";
                            LogMaterialSync(LogLevel.Warning, error);
                            errors.Add(error);
                            errorCount++;
                            continue;
                        }

                        if (existingMaterialDict.TryGetValue(material.Number, out var existingMaterial))
                        {
                            // 更新现有物料
                            MapK3CloudDataToEntity(material, existingMaterial, currentTime, false);
                            materialsToUpdate.Add(existingMaterial);
                        }
                        else
                        {
                            // 创建新物料
                            var newMaterial = new OCP_Material();
                            MapK3CloudDataToEntity(material, newMaterial, currentTime, true);
                            materialsToAdd.Add(newMaterial);
                        }
                    }
                    catch (Exception ex)
                    {
                        var error = $"处理物料 {material.Number} 失败: {ex.Message}";
                        LogMaterialSync(LogLevel.Error, error, ex);
                        errors.Add(error);
                        errorCount++;
                    }
                }

                // 3. 批量执行数据库操作
                try
                {
                    // 批量更新
                    if (materialsToUpdate.Any())
                    {
                        _repository.UpdateRange(materialsToUpdate);
                        syncedCount += materialsToUpdate.Count;
                        LogMaterialSync(LogLevel.Information, $"批量更新物料 {materialsToUpdate.Count} 条");
                    }

                    // 批量新增
                    if (materialsToAdd.Any())
                    {
                        _repository.AddRange(materialsToAdd, false);
                        syncedCount += materialsToAdd.Count;
                        LogMaterialSync(LogLevel.Information, $"批量新增物料 {materialsToAdd.Count} 条");
                    }

                    // 一次性保存所有更改
                    if (materialsToUpdate.Any() || materialsToAdd.Any())
                    {
                        await _repository.SaveChangesAsync();
                        LogMaterialSync(LogLevel.Information, $"成功保存物料数据，更新 {materialsToUpdate.Count} 条，新增 {materialsToAdd.Count} 条");
                    }
                }
                catch (Exception ex)
                {
                    var error = $"批量保存数据库更改失败: {ex.Message}";
                    LogMaterialSync(LogLevel.Error, error, ex);
                    errors.Add(error);
                    // 如果保存失败，将所有成功计数转为错误计数
                    errorCount += syncedCount;
                    syncedCount = 0;
                }
            }
            catch (Exception ex)
            {
                var error = $"处理物料页面数据异常: {ex.Message}";
                LogMaterialSync(LogLevel.Error, error, ex);
                errors.Add(error);
                errorCount = materials.Count;
                syncedCount = 0;
            }

            return (syncedCount, errorCount, errors);
        }

        /// <summary>
        /// 映射K3Cloud数据到实体
        /// </summary>
        /// <param name="k3Material">K3Cloud物料数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="currentTime">当前时间</param>
        /// <param name="isNew">是否为新增</param>
        private void MapK3CloudDataToEntity(K3CloudMaterialData k3Material, OCP_Material entity, DateTime currentTime, bool isNew)
        {
            // 基本信息映射
            entity.MaterialCode = k3Material.Number;                    // FNumber -> MaterialCode
            entity.MaterialName = k3Material.Name;                     // FName -> MaterialName
            entity.SpecModel = k3Material.FSpecification;              // FSpecification -> SpecModel
            entity.ProductModel = k3Material.F_BLN_CPXH;               // F_BLN_CPXH -> ProductModel
            entity.NominalDiameter = k3Material.F_BLN_Gctj;            // F_BLN_Gctj -> NominalDiameter
            entity.NominalPressure = k3Material.F_BLN_Gcyl;            // F_BLN_Gcyl -> NominalPressure
            entity.CV = k3Material.F_BLN_CV;                           // F_BLN_CV -> CV
            entity.Accessories = k3Material.F_BLN_Fj;                  // F_BLN_Fj -> Accessories
            entity.DrawingNo = k3Material.F_BLN_DwgNum;                // F_BLN_DwgNum -> DrawingNo
            entity.Material = k3Material.F_BLN_Material;               // F_BLN_Material -> Material
            entity.ErpClsid = k3Material.FErpClsID;                    // FErpClsID -> ErpClsid
            entity.Warehouse = k3Material.FStockNumber;                // FStockId.FNumber -> Warehouse
            entity.Workshop = k3Material.FWorkShopId;                  // FWorkShopId -> Workshop
            entity.BasicUnit = k3Material.FBaseUnitNumber;             // FBaseUnitId.FNumber -> BasicUnit

            // 新增字段映射
            entity.FlowCharacteristic = k3Material.F_BLN_LLTX;         // F_BLN_LLTX -> FlowCharacteristic
            entity.PackingForm = k3Material.F_BLN_TLXS;                // F_BLN_TLXS -> PackingForm
            entity.FlangeConnection = k3Material.F_BLN_FLLJFS;         // F_BLN_FLLJFS -> FlangeConnection
            entity.ActuatorModel = k3Material.F_BLN_ZXJGXH;            // F_BLN_ZXJGXH -> ActuatorModel
            entity.ActuatorStroke = k3Material.F_BLN_ZXJGXC;           // F_BLN_ZXJGXC -> ActuatorStroke

            // 2024-11-16 新增字段映射
            entity.FlangeStandard = k3Material.F_BLN_Flbz;             // F_BLN_Flbz -> FlangeStandard (法兰标准)
            entity.BodyMaterial = k3Material.F_BLN_Ftcz;               // F_BLN_Ftcz -> BodyMaterial (阀体材质)
            entity.TrimMaterial = k3Material.F_BLN_Fljcz;              // F_BLN_Fljcz -> TrimMaterial (阀内件材质)
            entity.FlangeSealType = k3Material.F_BLN_Flmfmxs;          // F_BLN_Flmfmxs -> FlangeSealType (法兰密封面型F式)
            entity.TCReleaser = k3Material.F_TC_RELEASER;              // F_TC_RELEASER -> TCReleaser (TC发布人)
            entity.BonnetForm= k3Material.F_BLN_SFGXS;

            // 是否关联BOM映射
            if (!string.IsNullOrEmpty(k3Material.FIsBOM))
            {
                entity.IsRelatedBOM = k3Material.FIsBOM.ToLower() == "true" ? 1 : 0;
            }
            
            // 如果是新增，设置MaterialID和创建信息
            if (isNew)
            {
                entity.MaterialID = k3Material.FMaterialID ?? 0;
                
                // 解析K3Cloud的创建日期
                if (!string.IsNullOrEmpty(k3Material.FModifyDate) && DateTime.TryParse(k3Material.FModifyDate, out var k3CreateDate))
                {
                    entity.CreateDate = k3CreateDate;
                }
                else
                {
                    entity.CreateDate = currentTime;
                }
            }
            
            // 解析K3Cloud的修改日期
            if (!string.IsNullOrEmpty(k3Material.FModifyDate) && DateTime.TryParse(k3Material.FModifyDate, out var k3ModifyDate))
            {
                entity.ModifyDate = k3ModifyDate;
                entity.ERPModifyDate = k3ModifyDate;  // FModifyDate -> ERPModifyDate
            }
            else
            {
                entity.ModifyDate = currentTime;
                entity.ERPModifyDate = currentTime;
            }

            entity.Modifier = !string.IsNullOrEmpty(k3Material.FModifierName) ? k3Material.FModifierName : "K3CloudSync";
            entity.ModifyID = 1; // 系统用户ID
        }

        /// <summary>
        /// 获取K3Cloud物料数据（不保存到数据库）
        /// </summary>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <returns>物料数据</returns>
        public async Task<WebResponseContent> GetK3CloudMaterialsAsync(int pageIndex = 0, int pageSize = 1000, string filterString = null)
        {
            try
            {
                var response = await _k3CloudService.GetMaterialsAsync(pageIndex, pageSize, filterString);
                
                if (response.IsSuccess)
                {
                    return new WebResponseContent(true)
                    {
                        Data = response.Data,
                        Message = $"成功获取 {response.Data?.Count ?? 0} 条物料数据"
                    };
                }
                else
                {
                    return new WebResponseContent(false) { Message = response.Message };
                }
            }
            catch (Exception ex)
            {
                LogMaterialSync(LogLevel.Error, "获取K3Cloud物料数据异常", ex);
                return new WebResponseContent(false) { Message = $"获取K3Cloud物料数据异常: {ex.Message}" };
            }
        }

        /// <summary>
        /// 获取K3Cloud物料总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>物料总数</returns>
        public async Task<WebResponseContent> GetK3CloudMaterialCountAsync(string filterString = null)
        {
            try
            {
                var count = await _k3CloudService.GetMaterialCountAsync(filterString);
                return new WebResponseContent(true)
                {
                    Data = count,
                    Message = $"K3Cloud中共有 {count} 条物料数据"
                };
            }
            catch (Exception ex)
            {
                LogMaterialSync(LogLevel.Error, "获取K3Cloud物料总数异常", ex);
                return new WebResponseContent(false) { Message = $"获取K3Cloud物料总数异常: {ex.Message}" };
            }
        }

        /// <summary>
        /// 物料同步专用日志记录方法
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        private void LogMaterialSync(LogLevel level, string message, Exception exception = null)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logMessage = $"[{timestamp}] {message}";
            
            switch (level)
            {
                case LogLevel.Information:
                    _materialSyncLogger.LogInformation(logMessage);
                    break;
                case LogLevel.Warning:
                    _materialSyncLogger.LogWarning(logMessage);
                    break;
                case LogLevel.Error:
                    if (exception != null)
                        _materialSyncLogger.LogError(exception, logMessage);
                    else
                        _materialSyncLogger.LogError(logMessage);
                    break;
                case LogLevel.Debug:
                    _materialSyncLogger.LogDebug(logMessage);
                    break;
                case LogLevel.Trace:
                    _materialSyncLogger.LogTrace(logMessage);
                    break;
                default:
                    _materialSyncLogger.LogInformation(logMessage);
                    break;
            }
        }

    }
} 