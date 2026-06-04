/*
*所有关于OCP_OrderTracking类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_OrderTrackingService
    {
        // ESB相关接口已迁移到独立的ESB架构中
        // 如需ESB同步功能，请使用 OrderTrackingESBSyncCoordinator

        /// <summary>
        /// 获取近14天订单完成统计数据
        /// </summary>
        /// <param name="scheduleMonth">排产月份（格式：yyyy-MM）</param>
        /// <returns>近14天订单完成统计数据</returns>
        Task<List<object>> GetOrderCompletionStatsAsync(string scheduleMonth);

        /// <summary>
        /// 获取总任务完成统计数据
        /// </summary>
        /// <returns>总任务完成统计数据</returns>
        Task<object[]> GetTaskCompletionSummary();

        /// <summary>
        /// 导出计划修改看板，按前端当前显示列生成文件。
        /// </summary>
        /// <param name="pageData">导出参数</param>
        /// <returns>导出文件路径</returns>
        WebResponseContent ExportPlanModifyBoard(PageDataOptions pageData);
    }
}
