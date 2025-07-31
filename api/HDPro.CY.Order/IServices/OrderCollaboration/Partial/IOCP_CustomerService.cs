/*
*所有关于OCP_Customer类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.CY.Order.Services.K3Cloud.Models;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_CustomerService
    {
        /// <summary>
        /// 从K3Cloud同步客户数据
        /// </summary>
        /// <param name="pageSize">每页处理数量</param>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <returns>同步结果</returns>
        Task<WebResponseContent> SyncCustomersFromK3CloudAsync(int pageSize = 1000, string customFilter = null);

        /// <summary>
        /// 获取K3Cloud客户数据（不保存到数据库）
        /// </summary>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <returns>K3Cloud客户数据</returns>
        Task<WebResponseContent> GetK3CloudCustomersAsync(int pageIndex = 0, int pageSize = 1000, string filterString = null);

        /// <summary>
        /// 获取K3Cloud客户总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>客户总数</returns>
        Task<WebResponseContent> GetK3CloudCustomerCountAsync(string filterString = null);
    }
 }
