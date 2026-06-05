using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Utilities;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices.OrderCollaboration
{
    /// <summary>
    /// 首页订单运营看板服务接口。
    /// </summary>
    public interface IOCP_HomeDashboardService : IDependency
    {
        /// <summary>
        /// 获取首页订单运营看板汇总数据。
        /// </summary>
        Task<WebResponseContent> GetHomeDashboardAsync(
            string dateRange,
            string businessType,
            string customer,
            string owner,
            string keyword);
    }
}
