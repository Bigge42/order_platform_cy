using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.IRepositories
{
    public partial interface ISys_AIAppRepository : IDependency, IRepository<Sys_AIApp>
    {
    }
}
