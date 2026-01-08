/*
*所有关于WZ_OrderCycleBase类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace HDPro.CY.Order.IServices
{
    public partial interface IWZ_OrderCycleBaseService
    {
        Task<int> SyncFromOrderTrackingAsync(CancellationToken cancellationToken = default);

        Task<ValveRuleBatchSummary> BatchCallValveRuleServiceAsync(CancellationToken cancellationToken = default);

        Task<List<PreScheduleOutputDto>> GetPreScheduleOutputAsync(
            string valveCategory,
            string productionLine,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);

        Task<int> RefreshPreScheduleOutputAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);
    }

    public sealed class PreScheduleOutputDto
    {
        public DateTime ProductionDate { get; set; }

        public string ValveCategory { get; set; }

        public string ProductionLine { get; set; }

        public decimal Quantity { get; set; }
    }

    public sealed class ValveRuleBatchSummary
    {
        /// <summary>
        /// 本次提交到规则服务的总行数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 规则服务返回 success=true 的条数
        /// </summary>
        public int Succeeded { get; set; }

        /// <summary>
        /// 规则服务返回失败或未能匹配到实体的条数
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// 实际更新到数据库的条数
        /// </summary>
        public int Updated { get; set; }

        /// <summary>
        /// 本次分批数量，便于前端展示进度
        /// </summary>
        public int BatchCount { get; set; }

        /// <summary>
        /// 规则服务返回的日志文件路径集合
        /// </summary>
        public List<string> LogFiles { get; set; } = new List<string>();
    }
 }
