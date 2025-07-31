/*
*所有关于OCP_PurchaseOrder类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
namespace HDPro.CY.Order.IServices
{
    public partial interface IOCP_PurchaseOrderService
    {
        Task<WebResponseContent> SummaryDetails2Head();
    }
 }
