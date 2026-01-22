/*
*所有关于ERP_OrderTracking类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace HDPro.CY.Order.IServices
{
    public partial interface IERP_OrderTrackingService
    {
        Task<WebResponseContent> SyncERPOrderTrackingAsync();
    }
 }
