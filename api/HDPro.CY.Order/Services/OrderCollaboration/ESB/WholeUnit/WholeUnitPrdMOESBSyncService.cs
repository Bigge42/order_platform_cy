using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HDPro.Entity.SystemModels;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.ESB;
using HDPro.CY.Order.IRepositories;
using HDPro.Core.Utilities;

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.WholeUnit
{
    /// <summary>
    /// 整机生产订单头ESB同步服务实现
    /// 对接ESB接口：/SearchPrdMO (整机版本)
    /// 将ESB数据同步到OCP_PrdMO表
    /// </summary>
    public class WholeUnitPrdMOESBSyncService : ESBSyncServiceBase<OCP_PrdMO, ESBWholeUnitPrdMOData, IOCP_PrdMORepository>
    {
        private readonly ESBLogger _esbLogger;

        public WholeUnitPrdMOESBSyncService(
            IOCP_PrdMORepository repository,
            ESBBaseService esbService,
            ILogger<WholeUnitPrdMOESBSyncService> logger,
            ILoggerFactory loggerFactory)
            : base(repository, esbService, logger)
        {
            _esbLogger = ESBLoggerFactory.CreateWholeUnitTrackingLogger(loggerFactory);
            InitializeESBServiceLogger();
        }

        /// <summary>
        /// 重写基类的ESBLogger属性，提供整机跟踪专用的ESB日志记录器
        /// </summary>
        protected override ESBLogger ESBLogger => _esbLogger;

        #region 实现抽象方法

        /// <summary>
        /// 获取操作类型描述
        /// </summary>
        protected override string GetOperationType()
        {
            return "整机生产订单头";
        }

        /// <summary>
        /// 验证ESB数据有效性
        /// </summary>
        protected override bool ValidateESBData(ESBWholeUnitPrdMOData esbData)
        {
            // 基本字段验证
            if (esbData.FID <= 0)
            {
                ESBLogger.LogValidationError("整机生产订单头", "FID无效", $"FID={esbData.FID}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取实体主键
        /// </summary>
        protected override object GetEntityKey(ESBWholeUnitPrdMOData esbData)
        {
            return (long)esbData.FID;
        }

        /// <summary>
        /// 查询现有记录
        /// </summary>
        protected override async Task<List<OCP_PrdMO>> QueryExistingRecords(List<object> keys)
        {
            var fidList = keys.Cast<long>().ToList();
            return await Task.Run(() =>
                _repository.FindAsIQueryable(x => x.FID.HasValue && fidList.Contains(x.FID.Value))
                .ToList());
        }

        /// <summary>
        /// 判断现有记录是否匹配ESB数据
        /// </summary>
        protected override bool IsEntityMatch(OCP_PrdMO entity, ESBWholeUnitPrdMOData esbData)
        {
            return entity.FID == esbData.FID;
        }

        /// <summary>
        /// 将ESB数据映射到实体
        /// </summary>
        protected override void MapESBDataToEntity(ESBWholeUnitPrdMOData esbData, OCP_PrdMO entity)
        {
            // 基本信息映射
            entity.FID = esbData.FID;
            entity.ProductionOrderNo = esbData.FBILLNO;
            entity.ProductionType = esbData.FBILLTYPENAME;

            // 计划信息映射
            entity.PlanTaskMonth = esbData.FCUSTUNMONTH;
            entity.PlanTaskWeek = esbData.FCUSTUNWEEK;
            entity.Urgency = esbData.FCUSTUNEMER;

            // 日期字段映射，使用基类的统一日期解析方法
            entity.MOAuditDate = ParseDate(esbData.FAPPROVEDATE);

            // 系统字段
            var now = DateTime.Now;
            if (entity.ID <= 0) // 新增
            {
                entity.CreateDate = now;
                entity.CreateID = 1; // 系统用户
                entity.Creator = "ESB";
            }
            entity.ModifyDate = now;
            entity.ModifyID = 1;
            entity.Modifier = "ESB";
        }

        /// <summary>
        /// 执行批量操作
        /// </summary>
        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_PrdMO> toUpdate, List<OCP_PrdMO> toInsert)
        {
            if (!toUpdate.Any() && !toInsert.Any())
                return new WebResponseContent().OK("无数据需要处理");

            return await Task.Run(() => _repository.DbContextBeginTransaction(() =>
            {
                var webResponse = new WebResponseContent();
                
                try
                {
                    // 使用UpdateRange批量更新
                    if (toUpdate.Any())
                    {
                        _repository.UpdateRange(toUpdate, false);
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条整机生产订单头记录");
                    }

                    // 使用AddRange批量插入
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条整机生产订单头记录");
                    }

                    _repository.SaveChanges();
                    
                    var totalProcessed = toUpdate.Count + toInsert.Count;
                    ESBLogger.LogInfo($"整机生产订单头批量操作成功完成，总计处理 {totalProcessed} 条记录");
                    
                    return webResponse.OK($"批量操作成功，更新 {toUpdate.Count} 条，新增 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, "执行整机生产订单头批量操作失败");
                    return webResponse.Error($"批量操作失败：{ex.Message}");
                }
            }));
        }

        protected override string GetESBApiConfigName()
        {
            return nameof(WholeUnitPrdMOESBSyncService);
        }

        #endregion
    }
} 