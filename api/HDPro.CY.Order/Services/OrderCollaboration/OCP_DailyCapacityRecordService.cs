using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Services.OrderCollaboration.ESB;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_DailyCapacityRecordService
        : CYOrderServiceBase<OCP_DailyCapacityRecord, IOCP_DailyCapacityRecordRepository>,
          IOCP_DailyCapacityRecordService, IDependency
    {
        // 通过Autofac获取ESB同步服务实例
        private readonly DailyCapacityRecordESBSyncService _esbSyncService
            = AutofacContainerModule.GetService<DailyCapacityRecordESBSyncService>();

        /// <summary>
        /// 手动同步指定日期范围的产能数据
        /// </summary>
        public async Task<WebResponseContent> SyncCapacityData(string startDate, string endDate)
        {
            // 调用ESB同步服务执行同步
            return await _esbSyncService.ManualSyncData(startDate, endDate);
        }

        /// <summary>
        /// 按年份、产线、阀门大类筛选获取产能记录列表
        /// </summary>
        public async Task<List<OCP_DailyCapacityRecord>> GetRecords(int? year, string productionLine, string valveCategory)
        {
            // 构建查询条件
            var query = repository.FindAsIQueryable(x => true);
            if (year.HasValue)
            {
                // 过滤年份：ProductionDate在该年范围内
                DateTime start = new DateTime(year.Value, 1, 1);
                DateTime end = start.AddYears(1);
                query = query.Where(x => x.ProductionDate >= start && x.ProductionDate < end);
            }
            if (!string.IsNullOrEmpty(productionLine))
            {
                query = query.Where(x => x.ProductionLine == productionLine);
            }
            if (!string.IsNullOrEmpty(valveCategory))
            {
                query = query.Where(x => x.ValveCategory == valveCategory);
            }
            // 按日期排序输出
            return await Task.Run(() => query.OrderBy(x => x.ProductionDate).ToList());
        }
    }
}
