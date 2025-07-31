/*
*所有关于Sys_FilterPlan类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.System.dto;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace HDPro.Sys.IServices
{
    public partial interface ISys_FilterPlanService
    {

        Task<WebResponseContent> AddOrUpdatePlan(System_FilterPlanInputDto plan);
        Task<WebResponseContent> GetPlan(string BillName);
        Task<WebResponseContent> DeletePlan(long id);       
    }
}
