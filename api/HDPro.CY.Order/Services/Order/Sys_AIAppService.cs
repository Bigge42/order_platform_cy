using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Services
{
    public partial class Sys_AIAppService : CYOrderServiceBase<Sys_AIApp, ISys_AIAppRepository>,
        ISys_AIAppService, IDependency
    {
        public static ISys_AIAppService Instance
        {
            get { return AutofacContainerModule.GetService<ISys_AIAppService>(); }
        }
    }
}
