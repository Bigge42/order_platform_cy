/*
 * 销售批次信息ESB同步服务
 * 对接ESB接口：SearchMOMProcedure
 * 负责同步当前工序批次信息到OCP_CurrentProcessBatchInfo表
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.ESB;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.Utilities;
using HDPro.Entity.SystemModels;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.BaseProvider;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.SalesManagement
{
    /// <summary>
    /// 销售批次信息ESB同步服务
    /// </summary>
    public class SalesBatchInfoESBSyncService : ESBSyncServiceBase<OCP_CurrentProcessBatchInfo, ESBSalesBatchInfoData, IOCP_CurrentProcessBatchInfoRepository>
    {
        // 批量同步时缓存计划跟踪号到DetailID和SOBillNo的映射关系
        private Dictionary<string, (long? DetailID, string SOBillNo)> _mtoNoToDetailCache = new Dictionary<string, (long?, string)>();
        private readonly ESBLogger _esbLogger;

        public SalesBatchInfoESBSyncService(
            IOCP_CurrentProcessBatchInfoRepository repository,
            ESBBaseService esbService,
            ILogger<SalesBatchInfoESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _esbLogger = ESBLoggerFactory.CreateSalesManagementLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供销售管理专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        protected override string GetESBApiConfigName()
        {
            return nameof(SalesBatchInfoESBSyncService);
        }

        protected override string GetOperationType()
        {
            return "销售批次信息";
        }

        protected override bool ValidateESBData(ESBSalesBatchInfoData esbData)
        {
            if (esbData == null)
            {
                ESBLogger.LogValidationError("销售批次信息", "ESB数据为空", "");
                return false;
            }

            // 仅验证计划跟踪号，移除对生产订单号的必填验证
            if (string.IsNullOrWhiteSpace(esbData.ERPMTONO))
            {
                ESBLogger.LogValidationError("销售批次信息", "计划跟踪号为空", "");
                return false;
            }

            return true;
        }

        protected override object GetEntityKey(ESBSalesBatchInfoData esbData)
        {
            // 使用计划跟踪号作为键值
            return esbData.ERPMTONO;
        }

        protected override async Task<List<OCP_CurrentProcessBatchInfo>> QueryExistingRecords(List<object> keys)
        {
            var repository = _repository as IRepository<OCP_CurrentProcessBatchInfo>;
            var batchNos = keys.Cast<string>().ToList();
            
            return await Task.FromResult(
                repository.FindAsIQueryable(x => batchNos.Contains(x.ProductNo))
                    .ToList()
            );
        }

        protected override bool IsEntityMatch(OCP_CurrentProcessBatchInfo entity, ESBSalesBatchInfoData esbData)
        {
            return entity.ProductNo == esbData.Retrospect_Code;
        }

        protected override void MapESBDataToEntity(ESBSalesBatchInfoData esbData, OCP_CurrentProcessBatchInfo entity)
        {
            // 基本信息
            entity.MtoNo = esbData.ERPMTONO?.Trim();
            entity.MOBillNo = esbData.FMOBILLNO?.Trim();
            
            // 批次号字段映射修正 - 从partial类定义可以看到BatchNo是NotMapped字段
            if (!string.IsNullOrWhiteSpace(esbData.Retrospect_Code))
            {
                entity.ProductNo = esbData.Retrospect_Code?.Trim();
            }
            
            entity.CurrentProcess = esbData.OperationName?.Trim();
            entity.PositionNo = esbData.Position_Num?.Trim();
            
            // 工序状态字段映射修正 - 从partial类定义可以看到ProcessState是NotMapped字段
            if (!string.IsNullOrWhiteSpace(esbData.ProcessStateName))
            {
                entity.ProcessState = esbData.ProcessStateName?.Trim();
            }

            // 重要：通过计划跟踪号关联第二步的DetailID和SOBillNo
            if (!string.IsNullOrWhiteSpace(esbData.ERPMTONO))
            {
                // 优先使用缓存查询DetailID和SOBillNo
                if (_mtoNoToDetailCache.ContainsKey(esbData.ERPMTONO))
                {
                    var cacheData = _mtoNoToDetailCache[esbData.ERPMTONO];
                    entity.DetailID = cacheData.DetailID;
                    entity.SOBillNo = cacheData.SOBillNo; // 从缓存获取SOBillNo
                    _logger.LogDebug($"从缓存获取DetailID：{entity.DetailID}，SOBillNo：{entity.SOBillNo}，计划跟踪号：{esbData.ERPMTONO}");
                }
                else
                {
                    // 缓存中没有时，实时查询（单个同步时使用）
                    try
                    {
                        var detailRepository = AutofacContainerModule.GetService<IOCP_SOProgressDetailRepository>();
                        var detailRecord = detailRepository.FindAsIQueryable(x => x.MtoNo == esbData.ERPMTONO)
                                                           .FirstOrDefault();
                        
                        if (detailRecord != null)
                        {
                            entity.DetailID = detailRecord.DetailID;
                            entity.SOBillNo = detailRecord.SOBillNo; // 从明细表获取SOBillNo
                            _logger.LogDebug($"实时查询获取DetailID：{entity.DetailID}，SOBillNo：{entity.SOBillNo}，计划跟踪号：{esbData.ERPMTONO}");
                        }
                        else
                        {
                            _logger.LogWarning($"未找到计划跟踪号 {esbData.ERPMTONO} 对应的销售订单明细记录，DetailID和SOBillNo将为空");
                            entity.DetailID = null;
                            entity.SOBillNo = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"查询计划跟踪号 {esbData.ERPMTONO} 对应的DetailID和SOBillNo时发生异常");
                        entity.DetailID = null;
                        entity.SOBillNo = null;
                    }
                }
            }

            // 如果从明细表中没有获取到SOBillNo，才从计划跟踪号中提取作为兜底方案
            if (string.IsNullOrWhiteSpace(entity.SOBillNo) && !string.IsNullOrWhiteSpace(esbData.ERPMTONO))
            {
                var parts = esbData.ERPMTONO.Split('-');
                if (parts.Length > 1)
                {
                    // 取前面几段作为销售订单号
                    entity.SOBillNo = string.Join("-", parts.Take(parts.Length - 1));
                    _logger.LogDebug($"从计划跟踪号提取SOBillNo：{entity.SOBillNo}");
                }
                else
                {
                    entity.SOBillNo = esbData.ERPMTONO;
                }
            }

            _logger.LogDebug($"映射销售批次信息数据完成，计划跟踪号：{entity.MtoNo}，当前工序：{entity.CurrentProcess}，关联DetailID：{entity.DetailID}，SOBillNo：{entity.SOBillNo}");
        }

        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_CurrentProcessBatchInfo> toUpdate, List<OCP_CurrentProcessBatchInfo> toInsert)
        {
            if (!toUpdate.Any() && !toInsert.Any())
                return new WebResponseContent().OK("无数据需要处理");

            return await Task.Run(() => _repository.DbContextBeginTransaction(() =>
            {
                var webResponse = new WebResponseContent();
                try
                {
                    if (toUpdate.Any())
                    {
                        _repository.UpdateRange(toUpdate, false);
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条批次信息记录");
                    }
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条批次信息记录");
                    }
                    _repository.SaveChanges();
                    var totalProcessed = toUpdate.Count + toInsert.Count;
                    ESBLogger.LogInfo($"销售批次信息批量操作成功完成，总计处理 {totalProcessed} 条记录");
                    return webResponse.OK($"批量操作成功，更新 {toUpdate.Count} 条，新增 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, "执行销售批次信息批量操作失败");
                    return webResponse.Error($"批量操作失败：{ex.Message}");
                }
            }));
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 批量预查询计划跟踪号到DetailID和SOBillNo的映射关系
        /// </summary>
        /// <param name="mtoNos">计划跟踪号列表</param>
        private void PreloadDetailIdMappings(List<string> mtoNos)
        {
            try
            {
                _mtoNoToDetailCache.Clear();
                
                if (mtoNos == null || !mtoNos.Any())
                {
                    return;
                }

                var detailRepository = AutofacContainerModule.GetService<IOCP_SOProgressDetailRepository>();
                var mappings = detailRepository.FindAsIQueryable(x => mtoNos.Contains(x.MtoNo) && !string.IsNullOrEmpty(x.MtoNo))
                                               .Select(x => new { x.MtoNo, x.DetailID, x.SOBillNo })
                                               .ToList();

                foreach (var mapping in mappings)
                {
                    if (!_mtoNoToDetailCache.ContainsKey(mapping.MtoNo))
                    {
                        _mtoNoToDetailCache[mapping.MtoNo] = (mapping.DetailID, mapping.SOBillNo);
                    }
                }

                _logger.LogInformation($"预加载了 {_mtoNoToDetailCache.Count} 个计划跟踪号到DetailID和SOBillNo的映射关系");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "预加载DetailID和SOBillNo映射关系时发生异常");
                _mtoNoToDetailCache.Clear();
            }
        }

        /// <summary>
        /// 根据计划跟踪号删除现有记录（使用EF Core 7.0 ExecuteDelete）
        /// </summary>
        /// <param name="planTrackingNo">计划跟踪号</param>
        /// <returns>删除操作结果</returns>
        private async Task DeleteExistingRecordsByPlanTrackingNo(string planTrackingNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(planTrackingNo))
                {
                    _logger.LogWarning("计划跟踪号为空，跳过删除操作");
                    return;
                }

                _logger.LogInformation($"开始删除计划跟踪号 {planTrackingNo} 的现有记录");

                // 使用EF Core 7.0的ExecuteDelete方法直接删除，无需先查询
                var deletedCount = await _repository.FindAsIQueryable(x => x.MtoNo == planTrackingNo)
                                                   .ExecuteDeleteAsync();

                if (deletedCount > 0)
                {
                    _logger.LogInformation($"成功删除计划跟踪号 {planTrackingNo} 的 {deletedCount} 条现有记录");
                }
                else
                {
                    _logger.LogDebug($"计划跟踪号 {planTrackingNo} 没有现有记录需要删除");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除计划跟踪号 {planTrackingNo} 的现有记录时发生异常");
                throw;
            }
        }

        /// <summary>
        /// 执行批量插入操作
        /// </summary>
        /// <param name="toInsert">要插入的记录列表</param>
        /// <returns>操作结果</returns>
        private async Task<WebResponseContent> ExecuteInsertOperations(List<OCP_CurrentProcessBatchInfo> toInsert)
        {
            if (!toInsert.Any())
                return new WebResponseContent().OK("无数据需要插入");

            return await Task.Run(() => _repository.DbContextBeginTransaction(() =>
            {
                var webResponse = new WebResponseContent();
                try
                {
                    _repository.AddRange(toInsert, false);
                    _logger.LogInformation($"准备批量插入 {toInsert.Count} 条批次信息记录");
                    
                    _repository.SaveChanges();
                    
                    _logger.LogInformation($"销售批次信息批量插入成功完成，总计处理 {toInsert.Count} 条记录");
                    return webResponse.OK($"批量插入成功，新增 {toInsert.Count} 条记录");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "执行销售批次信息批量插入失败");
                    return webResponse.Error($"批量插入失败：{ex.Message}");
                }
            }));
        }

        #endregion

        #region 特殊方法 - 按计划跟踪号查询

        /// <summary>
        /// 根据计划跟踪号同步批次信息
        /// 注意：此接口需要计划跟踪号作为参数
        /// </summary>
        /// <param name="planTrackingNo">计划跟踪号</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncByPlanTrackingNo(string planTrackingNo)
        {
            var response = new WebResponseContent();
            var (syncStartTime, executorInfo, currentUserId, currentUserName) = _esbService.GetSyncOperationInfo();

            try
            {
                if (string.IsNullOrWhiteSpace(planTrackingNo))
                {
                    return response.Error("计划跟踪号不能为空");
                }

                // 单个同步时清空缓存，使用实时查询确保数据准确性
                _mtoNoToDetailCache.Clear();

                _logger.LogInformation($"开始根据计划跟踪号同步批次信息，跟踪号：{planTrackingNo}，执行人：{executorInfo}");

                // 调用ESB接口
                var requestData = new ESBSalesBatchInfoRequest { ERPMTONO = planTrackingNo };
                var esbData = await _esbService.CallESBApiWithRequestData<ESBSalesBatchInfoData>(
                    GetFinalESBApiPath(), 
                    requestData, 
                    $"销售批次信息-{planTrackingNo}"
                );

                if (esbData == null || !esbData.Any())
                {
                    var message = $"未获取到计划跟踪号 {planTrackingNo} 的批次信息";
                    _logger.LogWarning(message);
                    return response.OK(message);
                }

                _logger.LogInformation($"获取到 {esbData.Count} 条批次信息");

                // 验证和处理数据
                var validData = esbData.Where(ValidateESBData).ToList();
                if (!validData.Any())
                {
                    return response.Error("所有数据验证失败，无有效数据可处理");
                }

                // 先删除该计划跟踪号对应的所有现有记录
                await DeleteExistingRecordsByPlanTrackingNo(planTrackingNo);

                // 创建新记录进行插入（不进行更新操作）
                var toInsert = new List<OCP_CurrentProcessBatchInfo>();
                var currentTime = DateTime.Now;

                foreach (var data in validData)
                {
                    // 创建新记录
                    var newEntity = new OCP_CurrentProcessBatchInfo();
                    MapESBDataToEntity(data, newEntity);
                    SetAuditFields(newEntity, currentTime, currentUserId, currentUserName, true);
                    toInsert.Add(newEntity);
                }

                // 执行数据库插入操作
                return await ExecuteInsertOperations(toInsert);
            }
            catch (Exception ex)
            {
                var errorMsg = $"根据计划跟踪号同步批次信息时发生异常：{ex.Message}";
                _logger.LogError(ex, errorMsg);
                return response.Error(errorMsg);
            }
        }

        #endregion

        #region 公共接口方法

        /// <summary>
        /// 预加载DetailID和SOBillNo映射关系（供协调器批量同步时调用）
        /// </summary>
        /// <param name="mtoNos">计划跟踪号列表</param>
        public void PreloadDetailIdMappingsForBatch(List<string> mtoNos)
        {
            PreloadDetailIdMappings(mtoNos);
        }

        /// <summary>
        /// 清空DetailID和SOBillNo映射缓存（批量同步完成后调用）
        /// </summary>
        public void ClearDetailIdMappingsCache()
        {
            _mtoNoToDetailCache.Clear();
            _logger.LogDebug("已清空DetailID和SOBillNo映射缓存");
        }

        /// <summary>
        /// 根据时间范围同步销售批次信息
        /// 注意：此方法调用的是按计划跟踪号查询的接口，需要先获取计划跟踪号列表
        /// </summary>
        /// <param name="startDate">开始日期 (yyyy-MM-dd)</param>
        /// <param name="endDate">结束日期 (yyyy-MM-dd)</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncSalesBatchInfoData(string startDate = null, string endDate = null)
        {
            // 此接口需要特殊处理，因为ESB接口需要计划跟踪号作为参数
            // 通常需要先从其他表中获取计划跟踪号，然后逐个查询批次信息
            return await Task.FromResult(new WebResponseContent().Error("此接口需要计划跟踪号，请使用SyncByPlanTrackingNo方法"));
        }

        /// <summary>
        /// 手动同步销售批次信息
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncSalesBatchInfoData(string startDate, string endDate)
        {
            return await ManualSyncData(startDate, endDate);
        }

        #endregion

        #region 静态实例

        public static SalesBatchInfoESBSyncService Instance
        {
            get { return AutofacContainerModule.GetService<SalesBatchInfoESBSyncService>(); }
        }

        #endregion
    }
} 