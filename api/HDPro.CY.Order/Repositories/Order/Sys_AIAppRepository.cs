using HDPro.CY.Order.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Repositories
{
    public partial class Sys_AIAppRepository : RepositoryBase<Sys_AIApp>, ISys_AIAppRepository
    {
        public Sys_AIAppRepository(ServiceDbContext dbContext)
        : base(dbContext)
        {
        }

        public static ISys_AIAppRepository Instance
        {
            get { return AutofacContainerModule.GetService<ISys_AIAppRepository>(); }
        }
    }
}
