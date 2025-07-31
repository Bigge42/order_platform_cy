/*
*所有关于OCP_LackMtrlResult类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_LackMtrlResultService
    {
        // ESB相关接口已迁移到独立的ESB架构中
        // 如需ESB同步功能，请使用 LackMtrlResultESBSyncCoordinator

        /// <summary>
        /// 获取默认运算方案的缺料情况统计
        /// </summary>
        /// <returns>按单据类型分组的缺料统计</returns>
        Task<object[]> GetDefaultLackMtrlSummary();

        /// <summary>
        /// 获取近14天的缺料情况统计
        /// </summary>
        /// <returns>近14天每日缺料统计数据</returns>
        Task<object[]> GetLast14DaysLackMtrlTrend();
    }
} 