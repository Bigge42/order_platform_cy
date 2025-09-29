/*
 * 缺料运算结果ESB同步服务
 * 对接ESB接口：SearchQLCal
 * 负责同步缺料计算结果数据到OCP_LackMtrlResult表
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.ESB;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.Utilities;
using HDPro.Entity.SystemModels;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.BaseProvider;
using HDPro.Core.ManageUser;
using HDPro.Core.Configuration;
using Microsoft.EntityFrameworkCore;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.LackMaterial
{
    /// <summary>
    /// 缺料运算结果ESB同步服务
    /// </summary>
    public class LackMtrlResultESBSyncService : ESBSyncServiceBase<OCP_LackMtrlResult, ESBLackMtrlData, IOCP_LackMtrlResultRepository>
    {
        private readonly IOCP_LackMtrlPoolRepository _poolRepository;
        private readonly ESBLogger _esbLogger;

        public LackMtrlResultESBSyncService(
            IOCP_LackMtrlResultRepository repository,
            ESBBaseService esbService,
            ILogger<LackMtrlResultESBSyncService> logger,
            IOCP_LackMtrlPoolRepository poolRepository,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _poolRepository = poolRepository;
            _esbLogger = ESBLoggerFactory.CreateLackMaterialLogger(loggerFactory);
            // 初始化ESB服务的日志记录器，确保ESBBaseService也使用缺料运算的专用日志
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供缺料运算专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        protected override string GetESBApiConfigName()
        {
            return nameof(LackMtrlResultESBSyncService);
        }

        protected override string GetOperationType()
        {
            return "缺料运算结果";
        }

        protected override bool ValidateESBData(ESBLackMtrlData esbData)
        {
            if (esbData == null)
            {
                ESBLogger.LogValidationError("缺料运算结果", "ESB数据为空");
                return false;
            }

            // 基本字段验证 - ID是唯一标识
            if (string.IsNullOrWhiteSpace(esbData.ID))
            {
                ESBLogger.LogValidationError("缺料运算结果", "ID为空");
                return false;
            }

            if (!esbData.ComputeID.HasValue || esbData.ComputeID <= 0)
            {
                ESBLogger.LogValidationError("缺料运算结果", "运算ID无效", $"ComputeID={esbData.ComputeID}");
                return false;
            }

            return true;
        }

        protected override object GetEntityKey(ESBLackMtrlData esbData)
        {
            return esbData.ID;
        }

        protected override async Task<List<OCP_LackMtrlResult>> QueryExistingRecords(List<object> keys)
        {
            var esbIds = keys.Cast<string>().ToList();
            
            return await Task.FromResult(
                _repository.FindAsIQueryable(x => esbIds.Contains(x.ESBID))
                    .ToList()
            );
        }

        protected override bool IsEntityMatch(OCP_LackMtrlResult entity, ESBLackMtrlData esbData)
        {
            return entity.ESBID == esbData.ID;
        }

        protected override void MapESBDataToEntity(ESBLackMtrlData esbData, OCP_LackMtrlResult entity)
        {
            MapESBDataToEntity(esbData, entity, []);
        }

        /// <summary>
        /// 映射ESB数据到实体（支持物料信息字典优化）
        /// </summary>
        /// <param name="esbData">ESB数据</param>
        /// <param name="entity">目标实体</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        private void MapESBDataToEntity(ESBLackMtrlData esbData, OCP_LackMtrlResult entity, Dictionary<long, OCP_Material> materialDict)
        {
            // 基本信息映射
            entity.ComputeID = esbData.ComputeID ?? 0;
            entity.ContractNo = esbData.F_BLN_CONTACTNO;
            entity.SOBillNo = esbData.FSALBILLNO;
            entity.MtoNo = esbData.FMOMTONO;

            // 映射整机物料ID到实体
            entity.TopMaterialID = esbData.FZJMATERIALID;

            // 获取整机物料信息并映射相关字段
            var topMaterial = GetMaterialByIdWithDict(esbData.FZJMATERIALID, materialDict);
            entity.TopMaterialNumber = topMaterial?.MaterialCode ?? string.Empty;
            entity.TopSpecification = topMaterial?.SpecModel ?? string.Empty; // 整机规格型号

            // 记录整机物料信息获取情况
            if (esbData.FZJMATERIALID.HasValue && esbData.FZJMATERIALID.Value > 0)
            {
                if (topMaterial == null)
                {
                    ESBLogger.LogWarning("整机物料ID存在但未找到对应物料信息，整机物料ID：{TopMaterialID}", esbData.FZJMATERIALID.Value);
                }
                else if (string.IsNullOrEmpty(topMaterial.MaterialCode))
                {
                    ESBLogger.LogWarning("整机物料信息存在但物料编码为空，整机物料ID：{TopMaterialID}，物料名称：{MaterialName}",
                        esbData.FZJMATERIALID.Value, topMaterial.MaterialName);
                }
            }

            // 催货单位字段映射
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = !string.IsNullOrEmpty(esbData.FCUSTUNEMER) ? esbData.FCUSTUNEMER : esbData.FCUSTUN;

            // 映射缺料物料ID到实体
            entity.MaterialID = esbData.FMATERIALID;

            // 获取物料信息并映射相关字段
            var material = GetMaterialByIdWithDict(esbData.FMATERIALID, materialDict);
            entity.MaterialNumber = material?.MaterialCode ?? string.Empty;
            entity.MaterialName = material?.MaterialName ?? string.Empty;
            entity.ProductCategory = material?.ProductModel ?? string.Empty; // 产品大类映射到产品型号
            entity.ErpClsid = material?.ErpClsid ?? string.Empty; // 物料属性
            entity.Specification = material?.SpecModel ?? string.Empty; // 规格型号
            entity.MaterialCategory = GetMaterialCategoryByCode(material?.MaterialCode, material?.MaterialName); // 根据编码规则确定物料分类
            
            // 数量信息映射
            entity.NeedQty = esbData.FNEEDQTY;
            entity.InventoryQty = esbData.Finvqty;
            entity.PlanedQty = esbData.ZTZZqty;
            entity.UnPlanedQty = esbData.FWXJHQTY;
            entity.BillNo = esbData.FBILLNO;
            entity.BillType = esbData.FBILLNOCLASS;
            entity.POQty = esbData.FQTY;
            entity.SupplierName = esbData.FWORKSHOPNAME;
            entity.PurchaserName = esbData.FPURCHASERNAME;
            entity.ProScheduleYearMonth = esbData.FZRWBILLNO; // 排产年月（关联总任务单据号）
            
            // 日期字段映射
            entity.PlanStartDate = ParseDate(esbData.FJHSTRATDATE);
            entity.PlanEndDate = ParseDate(esbData.FJHENDDATE);
            entity.CGSQAuditDate = ParseDate(esbData.FCGSQDATE?.ToString());
            entity.WWReleaseDate = ParseDate(esbData.FWWXDDATE);
            entity.PickMtrlDate = ParseDate(esbData.F_BLN_LLRQ);
            entity.StartDate = ParseDate(esbData.FSCSTARTDATE);
            entity.MOCreateDate = ParseDate(esbData.FSCCREATEDATE);
            entity.FinishDate = ParseDate(esbData.FSCPLANFINISHDATE);
            entity.POCreateDate = ParseDate(esbData.FCGCREATEDATE);
            entity.POAuditDate = ParseDate(esbData.FCGAPPROVEDATE);
            entity.PlanDeliveryDate = ParseDate(esbData.FJHJHRQ);
            entity.F_ORA_DATETIME = ParseDate(esbData.F_ORA_DATETIME);
            entity.F_RLRP_CDRQ = ParseDate(esbData.F_RLRP_CDRQ);
            entity.F_BLN_HFJHRQ = ParseDate(esbData.F_BLN_HFJHRQ);

            // 设置ESB主键
            entity.ESBID = esbData.ID;
        }

        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_LackMtrlResult> toUpdate, List<OCP_LackMtrlResult> toInsert)
        {
            if (!toUpdate.Any() && !toInsert.Any())
                return new WebResponseContent().OK("无数据需要处理");

            return await Task.Run(() => _repository.DbContextBeginTransaction(() =>
            {
                var response = new WebResponseContent();
                
                try
                {
                    // 使用UpdateRange批量更新
                    if (toUpdate.Any())
                    {
                        _repository.UpdateRange(toUpdate, false);
                        ESBLogger.LogInfo("准备批量更新缺料记录 {UpdateCount} 条", toUpdate.Count);
                    }

                    // 使用AddRange批量插入
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo("准备批量插入缺料记录 {InsertCount} 条", toInsert.Count);
                    }

                    // 一次性保存所有更改
                    _repository.SaveChanges();
                    
                    return response.OK($"缺料数据批量操作完成，更新 {toUpdate.Count} 条，插入 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogProcessingError("缺料运算结果", "批量缺料数据库操作失败", string.Empty, ex);
                    return response.Error($"批量操作失败：{ex.Message}");
                }
            }));
        }

        #endregion

        #region 业务特定方法

        /// <summary>
        /// 根据运算ID同步缺料数据（专用方法）
        /// </summary>
        /// <param name="computeId">运算ID</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncDataByComputeId(string computeId)
        {
            if (string.IsNullOrEmpty(computeId))
            {
                return new WebResponseContent().Error("运算ID不能为空");
            }

            // ESBBaseService现在通过操作类型参数动态创建日志记录器，不再需要设置共享日志记录器

            var response = new WebResponseContent();
            var syncStartTime = DateTime.Now;
            var operationType = GetOperationType();
            var executorInfo = UserContext.Current?.UserName ?? "系统同步";

            try
            {
                ESBLogger.LogInfo("开始{OperationType}ESB数据同步，执行者：{ExecutorInfo}，运算ID：{ComputeId}",
                    operationType, executorInfo, computeId);

                // 1. 调用ESB接口获取数据
                var requestData = new
                {
                    ComputeID = computeId
                };

                ESBLogger.LogInfo("开始调用缺料运算ESB接口，运算ID：{ComputeId}，使用长超时配置", computeId);

                // 缺料运算可能需要更长时间，使用自定义超时处理
                List<ESBLackMtrlData> esbDataList;
                try
                {
                    esbDataList = await _esbService.CallESBApiWithRequestData<ESBLackMtrlData>(GetFinalESBApiPath(), requestData, operationType);
                }
                catch (TimeoutException ex)
                {
                    ESBLogger.LogError(ex, "缺料运算ESB接口调用超时，运算ID：{ComputeId}，建议检查ESB服务器状态或增加超时时间", computeId);
                    return response.Error($"缺料运算ESB接口调用超时，运算ID：{computeId}，请稍后重试或联系管理员");
                }
                
                if (esbDataList == null || !esbDataList.Any())
                {
                    ESBLogger.LogWarning("{OperationType}ESB接口返回空数据，运算ID：{ComputeId}", operationType, computeId);
                    return response.OK($"{operationType}ESB接口返回空数据");
                }

                ESBLogger.LogInfo("从ESB获取到 {Count} 条{OperationType}数据", esbDataList.Count, operationType);

                // 2. 数据验证和过滤
                var validDataList = esbDataList.Where(ValidateESBData).ToList();
                if (validDataList.Count != esbDataList.Count)
                {
                    ESBLogger.LogWarning("{OperationType}数据验证过滤：原始 {OriginalCount} 条，有效 {ValidCount} 条",
                        operationType, esbDataList.Count, validDataList.Count);
                }

                if (!validDataList.Any())
                {
                    return response.OK($"{operationType}没有有效数据需要处理");
                }

                // 3. 批量处理数据
                var processedCount = await ProcessLackMtrlDataBatch(validDataList);
                
                // 4. 同步成功后更新运算队列状态
                try
                {
                    var statusUpdated = await UpdateLackMtrlPoolStatus(computeId, 2);
                    if (!statusUpdated)
                    {
                        ESBLogger.LogWarning("更新运算队列状态失败，但不影响数据同步结果，运算ID：{ComputeId}", computeId);
                    }
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, "更新运算队列状态异常，但不影响数据同步结果，运算ID：{ComputeId}", computeId);
                }

                var syncEndTime = DateTime.Now;
                var totalTime = (syncEndTime - syncStartTime).TotalSeconds;

                var successMessage = $"{operationType}ESB数据同步完成！共处理 {esbDataList.Count} 条数据，更新 {processedCount} 条记录，总耗时：{totalTime:F2} 秒";
                ESBLogger.LogInfo(successMessage);
                return response.OK(successMessage);
            }
            catch (Exception ex)
            {
                var syncEndTime = DateTime.Now;
                var totalTime = (syncEndTime - syncStartTime).TotalSeconds;
                var errorMessage = $"{operationType}ESB数据同步异常，总耗时：{totalTime:F2} 秒，错误：{ex.Message}";

                ESBLogger.LogError(ex, errorMessage);
                return response.Error(errorMessage);
            }
        }

        /// <summary>
        /// 处理缺料数据批次
        /// </summary>
        /// <param name="esbDataList">ESB数据列表</param>
        /// <returns>处理的记录数</returns>
        private async Task<int> ProcessLackMtrlDataBatch(List<ESBLackMtrlData> esbDataList)
        {
            var totalProcessed = 0;
            var batchSize = AppSetting.ESB?.BatchSize ?? 1000;
            
            ESBLogger.LogInfo("开始批量处理 {Count} 条ESB缺料数据，批量大小：{BatchSize}", esbDataList.Count, batchSize);

            try
            {
                // 分批处理数据，避免内存占用过大和事务超时
                for (int i = 0; i < esbDataList.Count; i += batchSize)
                {
                    var batch = esbDataList.Skip(i).Take(batchSize).ToList();
                    var batchProcessed = await ProcessSingleLackMtrlBatch(batch);
                    totalProcessed += batchProcessed;
                    
                    ESBLogger.LogInfo("已处理缺料数据批次 {BatchNumber}，本批次处理 {BatchProcessed} 条，累计处理 {TotalProcessed} 条",
                        i / batchSize + 1, batchProcessed, totalProcessed);
                }

                ESBLogger.LogInfo("缺料数据批量更新完成，总共处理 {TotalProcessed} 条记录", totalProcessed);
                return totalProcessed;
            }
            catch (Exception ex)
            {
                ESBLogger.LogError(ex, "批量更新缺料数据失败：{Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 处理单个缺料数据批次
        /// </summary>
        /// <param name="batchData">批次数据</param>
        /// <returns>处理的记录数</returns>
        private async Task<int> ProcessSingleLackMtrlBatch(List<ESBLackMtrlData> batchData)
        {
            var processedCount = 0;
            
            try
            {
                // 1. 获取现有记录的键值
                var keys = batchData.Select(GetEntityKey).ToList();
                
                // 2. 批量查询现有记录
                var existingRecords = await QueryExistingRecords(keys);
                
                // 3. 批量获取物料信息字典（性能优化）
                var allMaterialIds = new List<int?>();
                foreach (var esbData in batchData)
                {
                    allMaterialIds.Add(esbData.FMATERIALID);
                    allMaterialIds.Add(esbData.FZJMATERIALID);
                }
                var materialDict = GetMaterialDictionaryFromIntList(allMaterialIds);
                
                // 4. 分离新增和更新的数据
                var toUpdate = new List<OCP_LackMtrlResult>();
                var toInsert = new List<OCP_LackMtrlResult>();
                var currentTime = DateTime.Now;
                var currentUserId = UserContext.Current?.UserId ?? 1;
                var currentUserName = UserContext.Current?.UserName ?? "系统同步";

                foreach (var esbData in batchData)
                {
                    try
                    {
                        var existingRecord = existingRecords.FirstOrDefault(x => IsEntityMatch(x, esbData));

                        if (existingRecord != null)
                        {
                            // 更新现有记录
                            MapESBDataToEntity(esbData, existingRecord, materialDict);
                            SetAuditFields(existingRecord, currentTime, currentUserId, currentUserName, false);
                            toUpdate.Add(existingRecord);
                        }
                        else
                        {
                            // 创建新记录
                            var newRecord = new OCP_LackMtrlResult();
                            MapESBDataToEntity(esbData, newRecord, materialDict);
                            SetAuditFields(newRecord, currentTime, currentUserId, currentUserName, true);
                            toInsert.Add(newRecord);
                        }
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        ESBLogger.LogError(ex, "处理单条ESB缺料数据失败，运算ID：{ComputeID}，物料ID：{MaterialID}，错误：{Message}",
                            esbData.ComputeID, esbData.FMATERIALID, ex.Message);
                        // 继续处理下一条记录
                    }
                }

                // 5. 批量执行数据库操作
                await ExecuteBatchOperations(toUpdate, toInsert);
                
                return processedCount;
            }
            catch (Exception ex)
            {
                ESBLogger.LogError(ex, "处理缺料数据批次失败：{Message}", ex.Message);
                throw;
            }
        }



        /// <summary>
        /// 手动触发缺料数据同步
        /// </summary>
        /// <param name="computeId">运算ID</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncLackMtrlData(string computeId)
        {
            // ESBBaseService现在通过操作类型参数动态创建日志记录器，不再需要设置共享日志记录器

            var userName = UserContext.Current?.UserName ?? "未知用户";
            ESBLogger.LogInfo("手动触发ESB缺料数据同步，操作用户：{UserName}，运算ID：{ComputeId}", userName, computeId);
            return await SyncDataByComputeId(computeId);
        }



        /// <summary>
        /// 批量获取物料信息字典（用于性能优化）
        /// </summary>
        /// <param name="materialIds">物料ID列表</param>
        /// <returns>物料ID与物料实体的字典</returns>
        private Dictionary<long, OCP_Material> GetMaterialDictionaryFromIntList(List<int?> materialIds)
        {
            if (materialIds == null || !materialIds.Any())
                return [];

            try
            {
                // 过滤有效的物料ID并转换为long类型
                var validMaterialIds = materialIds
                    .Where(id => id.HasValue && id.Value > 0)
                    .Select(id => (long)id!.Value)  // 使用null-forgiving operator
                    .Distinct()
                    .ToList();

                // 使用基类的方法获取物料字典
                var longNullableIds = validMaterialIds.Select(id => (long?)id).ToList();
                return GetMaterialDictionary(longNullableIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量查询物料信息异常：{Message}", ex.Message);
                return [];
            }
        }

        /// <summary>
        /// 根据物料ID获取物料信息（支持字典优化）
        /// </summary>
        /// <param name="materialId">物料ID</param>
        /// <param name="materialDict">物料信息字典（可选，用于性能优化）</param>
        /// <returns>物料实体</returns>
        private OCP_Material? GetMaterialByIdWithDict(int? materialId, Dictionary<long, OCP_Material>? materialDict)
        {
            if (!materialId.HasValue || materialId <= 0)
                return null;

            // 转换为long类型并使用基类的方法
            return GetMaterialByIdWithDict((long)materialId.Value, materialDict);
        }

        /// <summary>
        /// 更新缺料运算队列状态
        /// </summary>
        /// <param name="computeId">运算ID</param>
        /// <param name="status">状态值（2=运算完成）</param>
        /// <returns>更新结果</returns>
        private async Task<bool> UpdateLackMtrlPoolStatus(string computeId, int status)
        {
            try
            {
                if (!long.TryParse(computeId, out long computeIdLong))
                {
                    _logger.LogWarning("运算ID格式无效：{ComputeId}", computeId);
                    return false;
                }

                var currentTime = DateTime.Now;
                var currentUserId = UserContext.Current?.UserId ?? 1;
                var currentUserName = UserContext.Current?.UserName ?? "系统同步";

                // 使用EF Core的ExecuteUpdateAsync进行批量更新
                var affectedRows = await Task.Run(() =>
                    _poolRepository.FindAsIQueryable(x => x.ComputeID == computeIdLong)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.Status, status)
                        .SetProperty(p => p.ModifyDate, currentTime)
                        .SetProperty(p => p.ModifyID, currentUserId)
                        .SetProperty(p => p.Modifier, currentUserName)));

                if (affectedRows > 0)
                {
                    _logger.LogInformation("成功批量更新运算队列状态，运算ID：{ComputeId}，状态：{Status}，影响行数：{AffectedRows}",
                        computeId, status, affectedRows);
                    return true;
                }
                else
                {
                    _logger.LogWarning("未找到运算ID为 {ComputeId} 的队列记录，影响行数：0", computeId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新运算队列状态异常，运算ID：{ComputeId}，状态：{Status}，错误：{Message}",
                    computeId, status, ex.Message);
                return false;
            }
        }

        #endregion
    }
} 