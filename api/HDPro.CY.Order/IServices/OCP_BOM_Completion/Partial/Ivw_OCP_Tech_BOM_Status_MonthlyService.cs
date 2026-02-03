/*
*所有关于vw_OCP_Tech_BOM_Status_Monthly类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace HDPro.CY.Order.IServices
{
    public partial interface Ivw_OCP_Tech_BOM_Status_MonthlyService
    {
        Task<WebResponseContent> SyncOrderDatesAsync();
    }
 }
