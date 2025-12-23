using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Entity.SystemModels;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using HDPro.Core.BaseProvider;
using HDPro.Core.Configuration;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.CY.Order.IRepositories;
using HDPro.Entity.DomainModels.ESB;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB
{
    /// <summary>
    /// ESB同步服务泛型基类，提供统一的同步流程模板
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TESBData">ESB数据类型</typeparam>
    /// <typeparam name="TRepository">仓储接口类型</typeparam>
    public abstract class ESBSyncServiceBase<TEntity, TESBData, TRepository>
        where TEntity : class, new()
        where TESBData : class
        where TRepository : class
    {
        protected readonly TRepository _repository;
        protected readonly ESBBaseService _esbService;
        protected readonly ILogger _logger;

        protected ESBSyncServiceBase(TRepository repository, ESBBaseService esbService, ILogger logger)
        {
            _repository = repository;
            _esbService = esbService;
            _logger = logger;
        }

        /// <summary>
        /// 获取ESB专用日志记录器
        /// 派生类可以重写此属性来提供自己的ESB日志记录器
        /// 如果派生类未重写，则使用普通日志记录器
        /// </summary>
        protected virtual ESBLogger ESBLogger => null;

        /// <summary>
        /// 初始化ESB服务的日志记录器
        /// 在派生类构造函数完成后调用，确保ESBLogger属性已经被重写
        /// 注意：ESBBaseService现在使用操作类型参数来创建日志记录器，不再需要设置共享日志记录器
        /// </summary>
        protected void InitializeESBServiceLogger()
        {
            // ESBBaseService现在通过操作类型参数动态创建日志记录器，不再需要设置共享日志记录器
            // 保留此方法以保持向后兼容性，但实际上不执行任何操作
        }

        /// <summary>
        /// 记录信息日志（优先使用ESB日志记录器）
        /// </summary>
        protected void LogInfo(string message)
        {
            if (ESBLogger != null)
                ESBLogger.LogInfo(message);
            else
                _logger.LogInformation(message);
        }

        /// <summary>
        /// 记录信息日志（优先使用ESB日志记录器，带参数）
        /// </summary>
        protected void LogInfo(string message, params object[] args)
        {
            if (ESBLogger != null)
                ESBLogger.LogInfo(message, args);
            else
                _logger.LogInformation(message, args);
        }

        /// <summary>
        /// 记录警告日志（优先使用ESB日志记录器）
        /// </summary>
        protected void LogWarning(string message)
        {
            if (ESBLogger != null)
                ESBLogger.LogWarning(message);
            else
                _logger.LogWarning(message);
        }

        /// <summary>
        /// 记录警告日志（优先使用ESB日志记录器，带参数）
        /// </summary>
        protected void LogWarning(string message, params object[] args)
        {
            if (ESBLogger != null)
                ESBLogger.LogWarning(message, args);
            else
                _logger.LogWarning(message, args);
        }

        /// <summary>
        /// 记录错误日志（优先使用ESB日志记录器）
        /// </summary>
        protected void LogError(Exception ex, string message)
        {
            if (ESBLogger != null)
                ESBLogger.LogError(ex, message);
            else
                _logger.LogError(ex, message);
        }

        /// <summary>
        /// 记录错误日志（优先使用ESB日志记录器，带参数）
        /// </summary>
        protected void LogError(Exception ex, string message, params object[] args)
        {
            if (ESBLogger != null)
                ESBLogger.LogError(ex, message, args);
            else
                _logger.LogError(ex, message, args);
        }

        /// <summary>
        /// 记录调试日志（优先使用ESB日志记录器）
        /// </summary>
        protected void LogDebug(string message)
        {
            if (ESBLogger != null)
                ESBLogger.LogDebug(message);
            else
                _logger.LogDebug(message);
        }

        #region 抽象方法 - 由子类实现具体业务逻辑

        /// <summary>
        /// 获取ESB接口配置名称（子类重写）
        /// </summary>
        protected abstract string GetESBApiConfigName();

        /// <summary>
        /// 获取操作类型名称（用于日志）
        /// </summary>
        protected abstract string GetOperationType();

        /// <summary>
        /// 验证单条ESB数据的有效性
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <returns>验证结果</returns>
        protected abstract bool ValidateESBData(TESBData esbData);

        /// <summary>
        /// 获取用于查询现有记录的键值
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <returns>查询键值</returns>
        protected abstract object GetEntityKey(TESBData esbData);

        /// <summary>
        /// 查询现有记录
        /// </summary>
        /// <param name="keys">查询键值列表</param>
        /// <returns>现有记录列表</returns>
        protected abstract Task<List<TEntity>> QueryExistingRecords(List<object> keys);

        /// <summary>
        /// 判断现有记录是否匹配ESB数据
        /// </summary>
        /// <param name="entity">现有实体</param>
        /// <param name="esbData">ESB数据</param>
        /// <returns>是否匹配</returns>
        protected abstract bool IsEntityMatch(TEntity entity, TESBData esbData);

        /// <summary>
        /// 将ESB数据映射到实体
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        protected abstract void MapESBDataToEntity(TESBData esbData, TEntity entity);

        /// <summary>
        /// 执行批量数据库操作
        /// </summary>
        /// <param name="toUpdate">待更新实体列表</param>
        /// <param name="toInsert">待插入实体列表</param>
        /// <returns>操作结果</returns>
        protected abstract Task<WebResponseContent> ExecuteBatchOperations(List<TEntity> toUpdate, List<TEntity> toInsert);

        #endregion

        #region 统一工具方法

        /// <summary>
        /// 统一的日期解析方法，支持多种日期格式，并验证SQL Server DateTime范围
        /// </summary>
        /// <param name="dateString">日期字符串</param>
        /// <returns>解析后的日期时间，失败或超出范围时返回null</returns>
        protected DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            // SQL Server DateTime有效范围
            var sqlMinDate = new DateTime(1753, 1, 1);
            var sqlMaxDate = new DateTime(9999, 12, 31, 23, 59, 59);

            // 尝试多种日期格式解析
            string[] formats = {
                "yyyy-MM-dd HH:mm:ss.fff",    // 包含毫秒
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "yyyy/MM/dd HH:mm:ss",
                "yyyy/MM/dd",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy",
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy",
                "yyyy年MM月dd日 HH:mm:ss",
                "yyyy年MM月dd日",
                "yyyyMMdd HH:mm:ss",
                "yyyyMMdd",
                "yyyy-M-d H:m:s",            // 单数字月日时分秒
                "yyyy-M-d"
            };

            var trimmedDateString = dateString.Trim();

            DateTime parsedDate = DateTime.MinValue;
            bool parseSuccess = false;

            // 先尝试精确格式匹配
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(trimmedDateString, format, null, System.Globalization.DateTimeStyles.None, out parsedDate))
                {
                    parseSuccess = true;
                    break;
                }
            }

            // 尝试通用解析
            if (!parseSuccess && DateTime.TryParse(trimmedDateString, out parsedDate))
            {
                parseSuccess = true;
            }

            // 特殊处理：如果包含T分隔符的ISO 8601格式
            if (!parseSuccess && trimmedDateString.Contains('T'))
            {
                if (DateTime.TryParse(trimmedDateString.Replace('T', ' '), out parsedDate))
                {
                    parseSuccess = true;
                }
            }

            if (!parseSuccess)
            {
                // 记录解析失败的日志
                _logger.LogWarning($"无法解析日期格式：'{dateString}'，请检查数据源格式");
                return null;
            }

            // 验证日期是否在SQL Server DateTime有效范围内
            if (parsedDate < sqlMinDate || parsedDate > sqlMaxDate)
            {
                _logger.LogWarning($"日期超出SQL Server DateTime有效范围：'{dateString}' -> {parsedDate:yyyy-MM-dd HH:mm:ss}，有效范围：{sqlMinDate:yyyy-MM-dd} 到 {sqlMaxDate:yyyy-MM-dd}");
                return null;
            }

            return parsedDate;
        }

        /// <summary>
        /// 根据物料编码和名称确定物料分类
        /// 分类规则：
        /// - 阀体部件：02开头
        /// - 阀体零件：08开头或04开头或6.开头
        /// - 毛坯：05开头或3.开头
        /// - 棒料：15开头
        /// - 管材：19开头
        /// - 密封圈、垫片等：06开头
        /// - 执行器：10开头
        /// - 附件：12或13开头
        /// - 手轮机构：20开头
        /// - 管道附件：09开头
        /// - 球芯/阀座组件：03开头且名称包含"球芯"或"阀座"
        /// </summary>
        /// <param name="materialCode">物料编码</param>
        /// <param name="materialName">物料名称</param>
        /// <returns>物料分类</returns>
        protected static string GetMaterialCategoryByCode(string materialCode, string materialName)
        {
            if (string.IsNullOrWhiteSpace(materialCode))
                return string.Empty;

            // 根据物料编码前缀确定分类
            if (materialCode.StartsWith("02"))
                return "阀体部件";
            else if (materialCode.StartsWith("08") || materialCode.StartsWith("04") || materialCode.StartsWith("6."))
                return "阀体零件";
            else if (materialCode.StartsWith("05") || materialCode.StartsWith("3."))
                return "毛坯";
            else if (materialCode.StartsWith("15"))
                return "棒料";
            else if (materialCode.StartsWith("19"))
                return "管材";
            else if (materialCode.StartsWith("06"))
                return "密封圈、垫片等";
            else if (materialCode.StartsWith("10"))
                return "执行器";
            else if (materialCode.StartsWith("12") || materialCode.StartsWith("13"))
                return "附件";
            else if (materialCode.StartsWith("20"))
                return "手轮机构";
            else if (materialCode.StartsWith("09"))
                return "管道附件";
            else if (materialCode.StartsWith("03"))
            {
                // 03开头且名称包含球芯或阀座的为球芯/阀座组件
                if (!string.IsNullOrWhiteSpace(materialName) &&
                    (materialName.Contains("球芯") || materialName.Contains("阀座")))
                {
                    return "球芯/阀座组件";
                }
                return "其他";
            }
            else
                return "其他";
        }

        #endregion

        #region 公共同步方法

        /// <summary>
        /// 通用的ESB数据同步方法
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncDataFromESB(string startDate = null, string endDate = null)
        {
            var response = new WebResponseContent();
            var (syncStartTime, executorInfo, currentUserId, currentUserName) = _esbService.GetSyncOperationInfo();
            
            try
            {
                // 设置默认时间范围
                var (actualStartDate, actualEndDate) = _esbService.GetDateRange(startDate, endDate);
                var operationType = GetOperationType();
                
                _esbService.LogSyncStart(operationType, executorInfo, actualStartDate, actualEndDate);

                // 第一阶段：调用ESB接口获取数据
                LogInfo("第一阶段：开始从ESB接口获取{OperationType}数据...", operationType);
                var esbData = await _esbService.CallESBApi<TESBData>(GetFinalESBApiPath(), actualStartDate, actualEndDate, operationType);

                if (esbData == null || !esbData.Any())
                {
                    var message = $"ESB接口返回空数据，时间范围：{actualStartDate} 到 {actualEndDate}";
                    LogWarning(message);
                    return response.OK(message);
                }

                LogInfo("第一阶段完成：从ESB获取到 {Count} 条{OperationType}数据", esbData.Count, operationType);

                // 第二阶段：数据验证和预处理
                LogInfo("第二阶段：开始数据验证和预处理...");
                var validData = ValidateESBDataList(esbData);
                LogInfo("第二阶段完成：有效数据 {ValidCount} 条，无效数据 {InvalidCount} 条", validData.Count, esbData.Count - validData.Count);

                if (!validData.Any())
                {
                    var message = "所有ESB数据验证失败，无有效数据可处理";
                    LogWarning(message);
                    return response.Error(message);
                }

                // 第三阶段：批量更新或插入数据
                LogInfo("第三阶段：开始批量数据库操作...");
                var updateCount = await ProcessESBDataBatch(validData, currentUserId, currentUserName);
                
                var successMessage = _esbService.LogSyncComplete(operationType, executorInfo, esbData.Count, validData.Count, updateCount, syncStartTime);
                
                return response.OK(successMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = _esbService.LogSyncError(GetOperationType(), ex, syncStartTime);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 手动触发同步（用于测试或手动执行）
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncData(string startDate, string endDate)
        {
            var userName = HDPro.Core.ManageUser.UserContext.Current?.UserName ?? "未知用户";
            LogInfo("手动触发{OperationType}ESB数据同步，操作用户：{UserName}，时间范围：{StartDate} 到 {EndDate}",
                GetOperationType(), userName, startDate, endDate);
            return await SyncDataFromESB(startDate, endDate);
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 验证ESB数据列表的有效性
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>有效数据列表</returns>
        private List<TESBData> ValidateESBDataList(List<TESBData> esbDataList)
        {
            var validData = new List<TESBData>();
            var invalidCount = 0;

            foreach (var data in esbDataList)
            {
                try
                {
                    if (ValidateESBData(data))
                    {
                        validData.Add(data);
                    }
                    else
                    {
                        invalidCount++;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "验证{OperationType}ESB数据时发生异常", GetOperationType());
                    invalidCount++;
                }
            }

            if (invalidCount > 0)
            {
                LogWarning("{OperationType}数据验证完成，无效数据 {InvalidCount} 条已跳过", GetOperationType(), invalidCount);
            }

            return validData;
        }

        /// <summary>
        /// 批量处理ESB数据
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <param name="currentUserId">当前用户ID</param>
        /// <param name="currentUserName">当前用户名</param>
        /// <returns>处理的记录数</returns>
        private async Task<int> ProcessESBDataBatch(List<TESBData> esbDataList, int currentUserId, string currentUserName)
        {
            if (esbDataList == null || !esbDataList.Any())
                return 0;

            var totalProcessed = 0;
            var batchSize = AppSetting.ESB?.BatchSize ?? 1000;
            
            LogInfo("开始批量处理 {Count} 条{OperationType}ESB数据，批量大小：{BatchSize}",
                esbDataList.Count, GetOperationType(), batchSize);

            try
            {
                // 分批处理数据
                for (int i = 0; i < esbDataList.Count; i += batchSize)
                {
                    var batch = esbDataList.Skip(i).Take(batchSize).ToList();
                    var batchProcessed = await ProcessSingleBatch(batch, currentUserId, currentUserName);
                    totalProcessed += batchProcessed;

                    LogInfo("已处理批次 {BatchNumber}，本批次处理 {BatchProcessed} 条，累计处理 {TotalProcessed} 条",
                        i / batchSize + 1, batchProcessed, totalProcessed);
                }

                LogInfo("{OperationType}批量更新完成，总共处理 {TotalProcessed} 条记录", GetOperationType(), totalProcessed);
                return totalProcessed;
            }
            catch (Exception ex)
            {
                LogError(ex, "批量更新{OperationType}数据失败", GetOperationType());
                throw;
            }
        }

        /// <summary>
        /// 处理单个批次的数据
        /// </summary>
        /// <param name="batchData">批次数据</param>
        /// <param name="currentUserId">当前用户ID</param>
        /// <param name="currentUserName">当前用户名</param>
        /// <returns>处理的记录数</returns>
        private async Task<int> ProcessSingleBatch(List<TESBData> batchData, int currentUserId, string currentUserName)
        {
            var processedCount = 0;
            
            try
            {
                // 🚀 性能优化：批量预查询主表记录
                var masterRecordsCache = await PreQueryMasterRecords(batchData);
                _logger.LogDebug($"批量预查询主表记录完成，缓存 {masterRecordsCache.Count} 条主表记录");
                
                // 🚀 性能优化：批量预查询物料记录
                var materialRecordsCache = await PreQueryMaterialRecords(batchData);
                _logger.LogDebug($"批量预查询物料记录完成，缓存 {materialRecordsCache.Count} 条物料记录");
                
                // 🚀 性能优化：批量预查询供应商记录  
                var supplierRecordsCache = await PreQuerySupplierRecords(batchData);
                _logger.LogDebug($"批量预查询供应商记录完成，缓存 {supplierRecordsCache.Count} 条供应商记录");
                
                // 🚀 性能优化：批量预查询客户记录
                var customerRecordsCache = await PreQueryCustomerRecords(batchData);
                _logger.LogDebug($"批量预查询客户记录完成，缓存 {customerRecordsCache.Count} 条客户记录");
                
                // 批量查询现有记录
                var keys = batchData.Select(GetEntityKey).Distinct().ToList();
                var existingRecords = await QueryExistingRecords(keys);

                var toUpdate = new List<TEntity>();
                var toInsert = new List<TEntity>();
                var currentTime = DateTime.Now;

                // 🔧 关键修复：使用 HashSet 跟踪已处理的现有记录，避免重复添加到 toUpdate
                var processedExistingRecords = new HashSet<TEntity>();

                foreach (var esbData in batchData)
                {
                    try
                    {                  
                        var existingRecord = existingRecords.FirstOrDefault(x => IsEntityMatch(x, esbData));

                        if (existingRecord != null)
                        {
                            // 🔧 检查该记录是否已经被处理过
                            if (!processedExistingRecords.Contains(existingRecord))
                            {
                                // 更新现有记录
                                MapESBDataToEntityWithCache(esbData, existingRecord, masterRecordsCache, materialRecordsCache, supplierRecordsCache, customerRecordsCache);
                                SetMasterTableId(esbData, existingRecord, masterRecordsCache); // 使用缓存设置主表ID
                                SetAuditFields(existingRecord, currentTime, currentUserId, currentUserName, false);
                                toUpdate.Add(existingRecord);
                                processedExistingRecords.Add(existingRecord);
                            }
                            else
                            {
                                // 记录警告：同一个现有记录被多个ESB数据匹配
                                _logger.LogWarning($"{GetOperationType()}：检测到重复匹配，ESB数据键={GetEntityKey(esbData)}，已跳过重复更新");
                            }
                        }
                        else
                        {
                            // 创建新记录
                            var newRecord = new TEntity();
                            MapESBDataToEntityWithCache(esbData, newRecord, masterRecordsCache, materialRecordsCache, supplierRecordsCache, customerRecordsCache);
                            SetMasterTableId(esbData, newRecord, masterRecordsCache); // 使用缓存设置主表ID
                            SetAuditFields(newRecord, currentTime, currentUserId, currentUserName, true);
                            toInsert.Add(newRecord);
                        }
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"处理单条{GetOperationType()}ESB数据失败：{ex.Message}");
                    }
                }

                // 批量执行数据库操作
                var result = await ExecuteBatchOperations(toUpdate, toInsert);
                if (!result.Status)
                {
                    throw new Exception($"{GetOperationType()}批量数据库操作失败：{result.Message}");
                }
                
                return processedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理{GetOperationType()}批次数据失败：{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 设置实体的审计字段
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="currentTime">当前时间</param>
        /// <param name="currentUserId">当前用户ID</param>
        /// <param name="currentUserName">当前用户名</param>
        /// <param name="isNew">是否为新记录</param>
        protected void SetAuditFields(TEntity entity, DateTime currentTime, int currentUserId, string currentUserName, bool isNew)
        {
            // 使用反射设置审计字段
            var entityType = typeof(TEntity);
            
            if (isNew)
            {
                // 设置创建字段
                SetPropertyValue(entity, "CreateDate", currentTime);
                SetPropertyValue(entity, "CreateID", currentUserId);
                SetPropertyValue(entity, "Creator", currentUserName);
            }
            
            // 设置修改字段
            SetPropertyValue(entity, "ModifyDate", currentTime);
            SetPropertyValue(entity, "ModifyID", currentUserId);
            SetPropertyValue(entity, "Modifier", currentUserName);
        }

        /// <summary>
        /// 通过反射设置属性值
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        private void SetPropertyValue(TEntity entity, string propertyName, object value)
        {
            var property = typeof(TEntity).GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                try
                {
                    property.SetValue(entity, value);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug($"设置属性 {propertyName} 失败：{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 批量预查询主表记录（虚方法，明细表服务可重写以优化性能）
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>主表记录字典，Key为FID，Value为主表记录</returns>
        protected virtual async Task<Dictionary<long, object>> PreQueryMasterRecords(List<TESBData> esbDataList)
        {
            // 默认实现：返回空字典，适用于主表服务
            return new Dictionary<long, object>();
        }

        /// <summary>
        /// 批量预查询物料记录（虚方法，子类可重写以优化性能）
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料记录字典，Key为MaterialID，Value为物料记录</returns>
        protected virtual async Task<Dictionary<long, OCP_Material>> PreQueryMaterialRecords(List<TESBData> esbDataList)
        {
            // 默认实现：提取所有物料ID并批量查询
            var materialIds = ExtractMaterialIds(esbDataList);
            
            if (!materialIds.Any())
                return new Dictionary<long, OCP_Material>();
            
            return await Task.Run(() => GetMaterialDictionary(materialIds));
        }

        /// <summary>
        /// 批量预查询供应商记录（虚方法，子类可重写以优化性能）
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>供应商记录字典，Key为SupplierID，Value为供应商记录</returns>
        protected virtual async Task<Dictionary<long, OCP_Supplier>> PreQuerySupplierRecords(List<TESBData> esbDataList)
        {
            // 默认实现：提取所有供应商ID并批量查询
            var supplierIds = ExtractSupplierIds(esbDataList);
            
            if (!supplierIds.Any())
                return new Dictionary<long, OCP_Supplier>();
            
            return await Task.Run(() => GetSupplierDictionary(supplierIds));
        }

        /// <summary>
        /// 批量预查询客户记录（虚方法，子类可重写以优化性能）
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>客户记录字典，Key为CustomerID，Value为客户记录</returns>
        protected virtual async Task<Dictionary<long, OCP_Customer>> PreQueryCustomerRecords(List<TESBData> esbDataList)
        {
            // 默认实现：提取所有客户ID并批量查询
            var customerIds = ExtractCustomerIds(esbDataList);
            
            if (!customerIds.Any())
                return new Dictionary<long, OCP_Customer>();
            
            return await Task.Run(() => GetCustomerDictionary(customerIds));
        }

        /// <summary>
        /// 设置明细表的主表ID关联（虚方法，明细表服务可重写）
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">实体对象</param>
        /// <param name="masterRecordsCache">主表记录缓存</param>
        /// <returns>是否成功设置主表ID</returns>
        protected virtual bool SetMasterTableId(TESBData esbData, TEntity entity, Dictionary<long, object> masterRecordsCache = null)
        {
            // 默认实现：不做任何处理，适用于主表服务
            // 明细表服务需要重写此方法来设置主表ID
            return true;
        }

        /// <summary>
        /// 设置明细表的主表ID关联（重载方法，保持向后兼容）
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">实体对象</param>
        /// <returns>是否成功设置主表ID</returns>
        protected virtual bool SetMasterTableId(TESBData esbData, TEntity entity)
        {
            return SetMasterTableId(esbData, entity, null);
        }

        /// <summary>
        /// 使用缓存进行ESB数据到实体的映射（虚方法，子类可重写以使用缓存优化）
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="masterRecordsCache">主表记录缓存</param>
        /// <param name="materialRecordsCache">物料记录缓存</param>
        /// <param name="supplierRecordsCache">供应商记录缓存</param>
        /// <param name="customerRecordsCache">客户记录缓存</param>
        protected virtual void MapESBDataToEntityWithCache(TESBData esbData, TEntity entity, 
            Dictionary<long, object> masterRecordsCache = null,
            Dictionary<long, OCP_Material> materialRecordsCache = null,
            Dictionary<long, OCP_Supplier> supplierRecordsCache = null,
            Dictionary<long, OCP_Customer> customerRecordsCache = null)
        {
            // 默认实现：调用原始的映射方法，子类可重写以使用缓存优化
            MapESBDataToEntity(esbData, entity);
        }

        /// <summary>
        /// 从ESB数据列表中提取物料ID（虚方法，子类可重写以提取特定的物料ID字段）
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>物料ID列表</returns>
        protected virtual List<long?> ExtractMaterialIds(List<TESBData> esbDataList)
        {
            // 默认实现：返回空列表，子类需要重写此方法来提取实际的物料ID字段
            return new List<long?>();
        }

        /// <summary>
        /// 从ESB数据列表中提取供应商ID（虚方法，子类可重写以提取特定的供应商ID字段）
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>供应商ID列表</returns>
        protected virtual List<int?> ExtractSupplierIds(List<TESBData> esbDataList)
        {
            // 默认实现：返回空列表，子类需要重写此方法来提取实际的供应商ID字段
            return new List<int?>();
        }

        /// <summary>
        /// 从ESB数据列表中提取客户ID（虚方法，子类可重写以提取特定的客户ID字段）
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>客户ID列表</returns>
        protected virtual List<int?> ExtractCustomerIds(List<TESBData> esbDataList)
        {
            // 默认实现：返回空列表，子类需要重写此方法来提取实际的客户ID字段
            return new List<int?>();
        }

        /// <summary>
        /// 获取最终的ESB接口路径（从配置文件读取）
        /// </summary>
        protected string GetFinalESBApiPath()
        {
            var configName = GetESBApiConfigName();
            if (string.IsNullOrEmpty(configName))
            {
                throw new InvalidOperationException($"[{GetType().Name}] 必须重写GetESBApiConfigName方法并返回有效的配置名称");
            }

            var configPath = AppSetting.ESB?.Apis?.GetApiPath(configName);
            if (string.IsNullOrEmpty(configPath))
            {
                throw new InvalidOperationException($"[{GetType().Name}] 配置文件中未找到API路径配置: {configName}");
            }

            _logger.LogDebug($"从配置文件获取API路径：{configName} -> {configPath}");
            return configPath;
        }

        #endregion

        #region 物料信息查询通用方法

        /// <summary>
        /// 批量获取物料信息字典（用于性能优化）
        /// </summary>
        /// <param name="materialIds">物料ID列表</param>
        /// <returns>物料ID与物料实体的字典</returns>
        protected Dictionary<long, OCP_Material> GetMaterialDictionary(List<long?> materialIds)
        {
            var result = new Dictionary<long, OCP_Material>();
            
            if (materialIds == null || !materialIds.Any())
                return result;
            
            try
            {
                // 过滤有效的物料ID
                var validMaterialIds = materialIds
                    .Where(id => id.HasValue && id.Value > 0)
                    .Select(id => id.Value)
                    .Distinct()
                    .ToList();
                
                if (!validMaterialIds.Any())
                    return result;
                
                // 批量查询完整的物料信息
                var materialRepository = AutofacContainerModule.GetService<IOCP_MaterialRepository>();
                var materials = materialRepository.FindAsIQueryable(x => validMaterialIds.Contains(x.MaterialID))
                    .ToList();
                
                foreach (var material in materials)
                {
                    result[material.MaterialID] = material;
                }
                
                _logger.LogInformation($"批量获取物料信息完成，查询物料ID数量：{validMaterialIds.Count}，获取到物料信息数量：{result.Count}");
                
                // 记录未找到物料信息的ID
                var notFoundIds = validMaterialIds.Where(id => !result.ContainsKey(id)).ToList();
                if (notFoundIds.Any())
                {
                    _logger.LogWarning($"以下物料ID未找到对应的物料信息：{string.Join(", ", notFoundIds)}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批量查询物料信息异常：{ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// 根据物料ID获取物料信息（支持字典优化）
        /// </summary>
        /// <param name="materialId">物料ID</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <returns>物料实体</returns>
        protected OCP_Material GetMaterialByIdWithDict(long? materialId, Dictionary<long, OCP_Material> materialDict = null)
        {
            if (!materialId.HasValue || materialId <= 0)
                return null;
            
            // 如果提供了字典，优先从字典中获取
            if (materialDict != null && materialDict.ContainsKey(materialId.Value))
            {
                return materialDict[materialId.Value];
            }
            
            // 字典中没有找到，回退到单个查询
            try
            {
                var materialRepository = AutofacContainerModule.GetService<IOCP_MaterialRepository>();
                return materialRepository.FindAsIQueryable(x => x.MaterialID == materialId.Value)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询物料信息异常，物料ID：{materialId}");
                return null;
            }
        }

        /// <summary>
        /// 根据物料ID获取物料编码
        /// </summary>
        /// <param name="materialId">物料ID</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <returns>物料编码</returns>
        protected string GetMaterialCodeById(long? materialId, Dictionary<long, OCP_Material> materialDict = null)
        {
            var material = GetMaterialByIdWithDict(materialId, materialDict);
            return material?.MaterialCode ?? string.Empty;
        }

        /// <summary>
        /// 根据物料ID获取物料名称
        /// </summary>
        /// <param name="materialId">物料ID</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <returns>物料名称</returns>
        protected string GetMaterialNameById(long? materialId, Dictionary<long, OCP_Material> materialDict = null)
        {
            var material = GetMaterialByIdWithDict(materialId, materialDict);
            return material?.MaterialName ?? string.Empty;
        }

        /// <summary>
        /// 根据物料ID获取规格型号
        /// </summary>
        /// <param name="materialId">物料ID</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <returns>规格型号</returns>
        protected string GetMaterialSpecById(long? materialId, Dictionary<long, OCP_Material> materialDict = null)
        {
            var material = GetMaterialByIdWithDict(materialId, materialDict);
            return material?.SpecModel ?? string.Empty;
        }

        /// <summary>
        /// 根据物料ID获取物料属性
        /// </summary>
        /// <param name="materialId">物料ID</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <returns>物料属性</returns>
        protected string GetMaterialErpClsidById(long? materialId, Dictionary<long, OCP_Material> materialDict = null)
        {
            var material = GetMaterialByIdWithDict(materialId, materialDict);
            return material?.ErpClsid ?? string.Empty;
        }

        /// <summary>
        /// 根据物料ID获取产品型号
        /// </summary>
        /// <param name="materialId">物料ID</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <returns>产品型号</returns>
        protected string GetMaterialProductModelById(long? materialId, Dictionary<long, OCP_Material> materialDict = null)
        {
            var material = GetMaterialByIdWithDict(materialId, materialDict);
            return material?.ProductModel ?? string.Empty;
        }

        #endregion

        #region 供应商信息查询通用方法

        /// <summary>
        /// 批量获取供应商信息字典（用于性能优化）
        /// </summary>
        /// <param name="supplierIds">供应商ID列表</param>
        /// <returns>供应商ID与供应商实体的字典</returns>
        protected Dictionary<long, OCP_Supplier> GetSupplierDictionary(List<int?> supplierIds)
        {
            var result = new Dictionary<long, OCP_Supplier>();
            
            if (supplierIds == null || !supplierIds.Any())
                return result;
            
            try
            {
                // 过滤有效的供应商ID
                var validSupplierIds = supplierIds
                    .Where(id => id.HasValue && id.Value > 0)
                    .Select(id => (long)id.Value)
                    .Distinct()
                    .ToList();
                
                if (!validSupplierIds.Any())
                    return result;
                
                // 批量查询完整的供应商信息
                var supplierRepository = AutofacContainerModule.GetService<IOCP_SupplierRepository>();
                var suppliers = supplierRepository.FindAsIQueryable(x => x.SupplierID.HasValue && validSupplierIds.Contains(x.SupplierID.Value))
                    .ToList();
                
                foreach (var supplier in suppliers)
                {
                    if (supplier.SupplierID.HasValue)
                    {
                        result[supplier.SupplierID.Value] = supplier;
                    }
                }
                
                _logger.LogInformation($"批量获取供应商信息完成，查询供应商ID数量：{validSupplierIds.Count}，获取到供应商信息数量：{result.Count}");
                
                // 记录未找到供应商信息的ID
                var notFoundIds = validSupplierIds.Where(id => !result.ContainsKey(id)).ToList();
                if (notFoundIds.Any())
                {
                    _logger.LogWarning($"以下供应商ID未找到对应的供应商信息：{string.Join(", ", notFoundIds)}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批量查询供应商信息异常：{ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// 根据供应商ID获取供应商信息（支持字典优化）
        /// </summary>
        /// <param name="supplierId">供应商ID</param>
        /// <param name="supplierDict">供应商信息字典（可选，用于性能优化）</param>
        /// <returns>供应商实体</returns>
        protected OCP_Supplier GetSupplierByIdWithDict(int? supplierId, Dictionary<long, OCP_Supplier> supplierDict = null)
        {
            if (!supplierId.HasValue || supplierId <= 0)
                return null;
            
            // 如果提供了字典，优先从字典中获取
            if (supplierDict != null && supplierDict.ContainsKey(supplierId.Value))
            {
                return supplierDict[supplierId.Value];
            }
            
            // 字典中没有找到，回退到单个查询
            try
            {
                var supplierRepository = AutofacContainerModule.GetService<IOCP_SupplierRepository>();
                return supplierRepository.FindAsIQueryable(x => x.SupplierID == supplierId.Value)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询供应商信息异常，供应商ID：{supplierId}");
                return null;
            }
        }

        /// <summary>
        /// 根据供应商ID获取供应商编码
        /// </summary>
        /// <param name="supplierId">供应商ID</param>
        /// <param name="supplierDict">供应商信息字典（可选，用于性能优化）</param>
        /// <returns>供应商编码</returns>
        protected string GetSupplierCodeById(int? supplierId, Dictionary<long, OCP_Supplier> supplierDict = null)
        {
            var supplier = GetSupplierByIdWithDict(supplierId, supplierDict);
            return supplier?.SupplierNumber ?? string.Empty;
        }

        /// <summary>
        /// 根据供应商ID获取供应商名称
        /// </summary>
        /// <param name="supplierId">供应商ID</param>
        /// <param name="supplierDict">供应商信息字典（可选，用于性能优化）</param>
        /// <returns>供应商名称</returns>
        protected string GetSupplierNameById(int? supplierId, Dictionary<long, OCP_Supplier> supplierDict = null)
        {
            var supplier = GetSupplierByIdWithDict(supplierId, supplierDict);
            return supplier?.SupplierName ?? string.Empty;
        }

        /// <summary>
        /// 根据供应商ID获取供应商联系电话
        /// </summary>
        /// <param name="supplierId">供应商ID</param>
        /// <param name="supplierDict">供应商信息字典（可选，用于性能优化）</param>
        /// <returns>供应商联系电话</returns>
        protected string GetSupplierPhoneById(int? supplierId, Dictionary<long, OCP_Supplier> supplierDict = null)
        {
            var supplier = GetSupplierByIdWithDict(supplierId, supplierDict);
            return supplier?.ContactPhone ?? string.Empty;
        }

        /// <summary>
        /// 根据供应商ID获取供应商地址
        /// </summary>
        /// <param name="supplierId">供应商ID</param>
        /// <param name="supplierDict">供应商信息字典（可选，用于性能优化）</param>
        /// <returns>供应商地址</returns>
        protected string GetSupplierAddressById(int? supplierId, Dictionary<long, OCP_Supplier> supplierDict = null)
        {
            var supplier = GetSupplierByIdWithDict(supplierId, supplierDict);
            return supplier?.Address ?? string.Empty;
        }

        #endregion

        #region 客户信息查询通用方法

        /// <summary>
        /// 批量获取客户信息字典（用于性能优化）
        /// </summary>
        /// <param name="customerIds">客户ID列表</param>
        /// <returns>客户ID与客户实体的字典</returns>
        protected Dictionary<long, OCP_Customer> GetCustomerDictionary(List<int?> customerIds)
        {
            var result = new Dictionary<long, OCP_Customer>();
            
            if (customerIds == null || !customerIds.Any())
                return result;
            
            try
            {
                // 过滤有效的客户ID
                var validCustomerIds = customerIds
                    .Where(id => id.HasValue && id.Value > 0)
                    .Select(id => (long)id.Value)
                    .Distinct()
                    .ToList();
                
                if (!validCustomerIds.Any())
                    return result;
                
                // 批量查询完整的客户信息
                var customerRepository = AutofacContainerModule.GetService<IOCP_CustomerRepository>();
                var customers = customerRepository.FindAsIQueryable(x => validCustomerIds.Contains(x.CustomerID))
                    .ToList();
                
                foreach (var customer in customers)
                {
                    result[customer.CustomerID] = customer;
                }
                
                _logger.LogInformation($"批量获取客户信息完成，查询客户ID数量：{validCustomerIds.Count}，获取到客户信息数量：{result.Count}");
                
                // 记录未找到客户信息的ID
                var notFoundIds = validCustomerIds.Where(id => !result.ContainsKey(id)).ToList();
                if (notFoundIds.Any())
                {
                    _logger.LogWarning($"以下客户ID未找到对应的客户信息：{string.Join(", ", notFoundIds)}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批量查询客户信息异常：{ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// 根据客户ID获取客户信息（支持字典优化）
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="customerDict">客户信息字典（可选，用于性能优化）</param>
        /// <returns>客户实体</returns>
        protected OCP_Customer GetCustomerByIdWithDict(int? customerId, Dictionary<long, OCP_Customer> customerDict = null)
        {
            if (!customerId.HasValue || customerId <= 0)
                return null;
            
            // 如果提供了字典，优先从字典中获取
            if (customerDict != null && customerDict.ContainsKey(customerId.Value))
            {
                return customerDict[customerId.Value];
            }
            
            // 字典中没有找到，回退到单个查询
            try
            {
                var customerRepository = AutofacContainerModule.GetService<IOCP_CustomerRepository>();
                return customerRepository.FindAsIQueryable(x => x.CustomerID == customerId.Value)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询客户信息异常，客户ID：{customerId}");
                return null;
            }
        }

        /// <summary>
        /// 根据客户ID获取客户编码
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="customerDict">客户信息字典（可选，用于性能优化）</param>
        /// <returns>客户编码</returns>
        protected string GetCustomerCodeById(int? customerId, Dictionary<long, OCP_Customer> customerDict = null)
        {
            var customer = GetCustomerByIdWithDict(customerId, customerDict);
            return customer?.Number ?? string.Empty;
        }

        /// <summary>
        /// 根据客户ID获取客户名称
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="customerDict">客户信息字典（可选，用于性能优化）</param>
        /// <returns>客户名称</returns>
        protected string GetCustomerNameById(int? customerId, Dictionary<long, OCP_Customer> customerDict = null)
        {
            var customer = GetCustomerByIdWithDict(customerId, customerDict);
            return customer?.Name ?? string.Empty;
        }

        /// <summary>
        /// 根据客户ID获取客户联系电话
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="customerDict">客户信息字典（可选，用于性能优化）</param>
        /// <returns>客户联系电话</returns>
        protected string GetCustomerPhoneById(int? customerId, Dictionary<long, OCP_Customer> customerDict = null)
        {
            var customer = GetCustomerByIdWithDict(customerId, customerDict);
            return customer?.ContactPhone ?? string.Empty;
        }

        /// <summary>
        /// 根据客户ID获取客户地址
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="customerDict">客户信息字典（可选，用于性能优化）</param>
        /// <returns>客户地址</returns>
        protected string GetCustomerAddressById(int? customerId, Dictionary<long, OCP_Customer> customerDict = null)
        {
            var customer = GetCustomerByIdWithDict(customerId, customerDict);
            return customer?.Address ?? string.Empty;
        }

        #endregion
    }
} 