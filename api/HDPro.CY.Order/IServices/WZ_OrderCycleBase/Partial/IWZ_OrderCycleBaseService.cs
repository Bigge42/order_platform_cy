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
using HDPro.CY.Order.Models.WZProductionOutputDtos;
namespace HDPro.CY.Order.IServices
{
    public partial interface IWZ_OrderCycleBaseService
    {
        Task<int> SyncFromOrderTrackingAsync(CancellationToken cancellationToken = default);

        Task<ValveRuleBatchSummary> BatchCallValveRuleServiceAsync(CancellationToken cancellationToken = default);

        Task<int> FillValveCategoryByRuleAsync(int batchSize = 1000);

        Task<AssignedProductionLineBatchSummary> BatchAssignProductionLineByRuleAsync(int batchSize = 1000, CancellationToken cancellationToken = default);

        Task<int> SyncPreProductionOutputAsync(CancellationToken cancellationToken = default);

        string GetAssignedProductionLineSql();

        Task<List<CapacityScheduleResultDto>> CalculateCapacityScheduleAsync(
            CapacityScheduleRequestDto request,
            CancellationToken cancellationToken = default);
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

    public sealed class AssignedProductionLineBatchSummary
    {
        /// <summary>
        /// 本次处理的总行数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 实际更新的行数
        /// </summary>
        public int Updated { get; set; }

        /// <summary>
        /// 未命中规则或值未变化的行数
        /// </summary>
        public int Skipped { get; set; }

        /// <summary>
        /// 处理失败的行数
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// 失败的主键集合
        /// </summary>
        public List<int> FailedIds { get; set; } = new List<int>();

        /// <summary>
        /// 对照校验用 SQL
        /// </summary>
        public string SqlPreview { get; set; }
    }
 }
