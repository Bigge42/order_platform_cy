/*
*所有关于OCP_PurchaseSupplierMapping类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using HDPro.CY.Order.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_PurchaseSupplierMappingService
    {
        /// <summary>
        /// 根据业务类型和业务主键获取供应商负责人信息
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>供应商负责人信息</returns>
        Task<WebResponseContent> GetSupplierResponsibleAsync(GetSupplierResponsibleRequest request);

        /// <summary>
        /// 批量获取供应商负责人信息
        /// </summary>
        /// <param name="requests">批量查询请求列表</param>
        /// <returns>批量供应商负责人信息</returns>
        Task<WebResponseContent> BatchGetSupplierResponsibleAsync(List<GetSupplierResponsibleRequest> requests);
    }
 }
