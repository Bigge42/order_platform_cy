/*
*所有关于OCP_Supplier类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_SupplierService
    {
        /// <summary>
        /// 从K3Cloud增量同步供应商数据
        /// 基于本地供应商表最大修改时间进行增量同步
        /// </summary>
        /// <param name="pageSize">每页数量，默认1000</param>
        /// <param name="customFilter">自定义过滤条件</param>
        /// <returns>同步结果</returns>
        Task<WebResponseContent> SyncSuppliersFromK3CloudAsync(int pageSize = 1000, string customFilter = null);

        /// <summary>
        /// 获取K3Cloud供应商数据（不保存到数据库）
        /// </summary>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="filterString">过滤条件</param>
        /// <returns>供应商数据</returns>
        Task<WebResponseContent> GetK3CloudSuppliersAsync(int pageIndex = 0, int pageSize = 1000, string filterString = null);

        /// <summary>
        /// 获取K3Cloud供应商总数
        /// </summary>
        /// <param name="filterString">过滤条件</param>
        /// <returns>供应商总数</returns>
        Task<WebResponseContent> GetK3CloudSupplierCountAsync(string filterString = null);
    }
 }
