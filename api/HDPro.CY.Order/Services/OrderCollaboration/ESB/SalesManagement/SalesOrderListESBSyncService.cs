/*
 * 销售订单列表ESB同步服务
 * 对接ESB接口：SearchERPSalOrderList
 * 负责同步销售订单列表数据到OCP_SOProgress表
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

namespace HDPro.CY.Order.Services.OrderCollaboration.ESB.SalesManagement
{
    /// <summary>
    /// 销售订单列表ESB同步服务
    /// </summary>
    public class SalesOrderListESBSyncService : ESBSyncServiceBase<OCP_SOProgress, ESBSalesOrderListData, IOCP_SOProgressRepository>
    {
        private readonly ESBLogger _esbLogger;

        public SalesOrderListESBSyncService(
            IOCP_SOProgressRepository repository,
            ESBBaseService esbService,
            ILogger<SalesOrderListESBSyncService> logger,
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
            return nameof(SalesOrderListESBSyncService);
        }

        protected override string GetOperationType()
        {
            return "销售订单列表";
        }

        protected override bool ValidateESBData(ESBSalesOrderListData esbData)
        {
            if (esbData == null)
            {
                ESBLogger.LogValidationError("销售订单列表", "ESB数据为空", "");
                return false;
            }

            // 仅验证必填字段FID
            if (esbData.FID <= 0)
            {
                ESBLogger.LogValidationError("销售订单列表", "FID无效", $"FID={esbData.FID}");
                return false;
            }

            return true;
        }

        protected override object GetEntityKey(ESBSalesOrderListData esbData)
        {
            return esbData.FID;
        }

        protected override async Task<List<OCP_SOProgress>> QueryExistingRecords(List<object> keys)
        {
            var repository = _repository as IRepository<OCP_SOProgress>;
            var longKeys = keys.Cast<int>().Select(k => (long)k).ToList();

            return await Task.FromResult(
                repository.FindAsIQueryable(x => x.FID.HasValue && longKeys.Contains(x.FID.Value))
                    .ToList()
            );
        }

        protected override bool IsEntityMatch(OCP_SOProgress entity, ESBSalesOrderListData esbData)
        {
            return entity.FID == esbData.FID;
        }

        protected override void MapESBDataToEntity(ESBSalesOrderListData esbData, OCP_SOProgress entity)
        {
            // 设置主键
            entity.FID = esbData.FID;

            // 基本信息
            entity.BillNo = esbData.FSALBILLNO?.Trim();
            entity.Customer = esbData.FCUSTName?.Trim();
            entity.BillStatus = esbData.FSTATE?.Trim();
            entity.SalesContractNumber = esbData.F_BLN_CONTACTNONAME?.Trim();
            entity.CustomerContractNumber = esbData.F_BLN_YHHTH?.Trim();
            entity.ContractType = esbData.F_ORA_ASSISTANTNAME;
            entity.ProjectName = esbData.F_ORA_XMMC;
            entity.SalesPerson = esbData.XSYNAME?.Trim(); // 销售负责人

            // 日期信息
            entity.BillDate = ParseDate(esbData.FDATE) ?? DateTime.Now;
            ESBLogger.LogDebug($"映射销售订单数据完成，订单号：{entity.BillNo}，客户：{entity.Customer}，销售负责人：{entity.SalesPerson}");
        }

        protected override async Task<WebResponseContent> ExecuteBatchOperations(List<OCP_SOProgress> toUpdate, List<OCP_SOProgress> toInsert)
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
                        ESBLogger.LogInfo($"准备批量更新 {toUpdate.Count} 条销售订单记录");
                    }
                    if (toInsert.Any())
                    {
                        _repository.AddRange(toInsert, false);
                        ESBLogger.LogInfo($"准备批量插入 {toInsert.Count} 条销售订单记录");
                    }
                    _repository.SaveChanges();
                    var totalProcessed = toUpdate.Count + toInsert.Count;
                    ESBLogger.LogInfo($"销售订单列表批量操作成功完成，总计处理 {totalProcessed} 条记录");
                    return webResponse.OK($"批量操作成功，更新 {toUpdate.Count} 条，新增 {toInsert.Count} 条");
                }
                catch (Exception ex)
                {
                    ESBLogger.LogError(ex, "执行销售订单列表批量操作失败");
                    return webResponse.Error($"批量操作失败：{ex.Message}");
                }
            }));
        }

        #endregion

        #region 公共接口方法

        /// <summary>
        /// 根据时间范围同步销售订单列表数据
        /// </summary>
        /// <param name="startDate">开始日期 (yyyy-MM-dd)</param>
        /// <param name="endDate">结束日期 (yyyy-MM-dd)</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> SyncSalesOrderListData(string startDate = null, string endDate = null)
        {
            return await SyncDataFromESB(startDate, endDate);
        }

        /// <summary>
        /// 手动同步销售订单列表数据
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>同步结果</returns>
        public async Task<WebResponseContent> ManualSyncSalesOrderListData(string startDate, string endDate)
        {
            return await ManualSyncData(startDate, endDate);
        }

        #endregion

        #region 静态实例

        public static SalesOrderListESBSyncService Instance
        {
            get { return AutofacContainerModule.GetService<SalesOrderListESBSyncService>(); }
        }

        #endregion
    }
} 